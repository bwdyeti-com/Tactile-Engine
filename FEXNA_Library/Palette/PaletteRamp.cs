using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ListExtension;
using FEXNAContentExtension;

namespace FEXNA_Library.Palette
{
    public class PaletteRamp : IFEXNADataContent
    {
        [ContentSerializer]
        public string Name;
        [ContentSerializer(ElementName = "BaseColorIndex")]
        private float _BaseColorIndex;
        [ContentSerializer(ElementName = "Colors")]
        private List<Color> _Colors;
        [ContentSerializer(ElementName = "Adjustments")]
        private List<ColorVector> _Adjustments;
        [ContentSerializer]
        public PaletteGenerator Generator;
        [ContentSerializer]
        public bool BlueYellowAdjustments;

        [ContentSerializerIgnore]
        public float BaseColorIndex { get { return _BaseColorIndex; } }
        [ContentSerializerIgnore]
        public List<Color> Colors
        {
            get
            {
                return new List<Color>(_Colors);
            }
        }
        [ContentSerializerIgnore]
        public List<ColorVector> Adjustments
        {
            get
            {
                return new List<ColorVector>(_Adjustments);
            }
        }
        
        #region IFEXNADataContent
        public IFEXNADataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static PaletteRamp GetEmptyInstance()
        {
            return new PaletteRamp();
        }

        public static PaletteRamp ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public void Read(BinaryReader input)
        {
            Name = input.ReadString();
            _BaseColorIndex = input.ReadSingle();
            _Colors.read(input);
            input.ReadFEXNAContentStruct(_Adjustments);
            Generator.Read(input);
            BlueYellowAdjustments = input.ReadBoolean();
        }

        public void Write(BinaryWriter output)
        {
            output.Write(Name);
            output.Write(_BaseColorIndex);
            _Colors.write(output);
            output.WriteStruct(_Adjustments);
            Generator.Write(output);
            output.Write(BlueYellowAdjustments);
        }
        #endregion

        public PaletteRamp() : this("", Color.Black) { }
        public PaletteRamp(string name, Color darkestColor)
        {
            Name = name;
            _BaseColorIndex = 0;
            _Colors = new List<Color>();
            _Adjustments = new List<ColorVector>();
            Generator = new PaletteGenerator();
            BlueYellowAdjustments = true;

            SetBlackLevel(darkestColor);
        }
        private PaletteRamp(PaletteRamp source)
        {
            Name = source.Name;
            _BaseColorIndex = source._BaseColorIndex;
            _Colors = new List<Color>(source._Colors);
            _Adjustments = new List<ColorVector>(source._Adjustments);
            Generator = (PaletteGenerator)source.Generator.Clone();
            BlueYellowAdjustments = source.BlueYellowAdjustments;
        }

        [ContentSerializerIgnore]
        public int Count { get { return _Colors.Count; } }
        [ContentSerializerIgnore]
        public Color BaseColor
        {
            get
            {
                return GetColor(_BaseColorIndex);
            }
        }

        public XnaHSL GetHsl(float index)
        {
            XnaHSL black = new XnaHSL(_Colors.FirstOrDefault());
            black = black.SetSaturation(0f);
            black = black.SetLightness(0f);
            XnaHSL white = new XnaHSL(_Colors.LastOrDefault());
            white = white.SetSaturation(0f);
            white = white.SetLightness(1f);

            XnaHSL left = black;
            XnaHSL right = white;

            for (int i = 0; i <= _Colors.Count; i++, index -= 1f, left = right)
            {
                if (i == _Colors.Count)
                    right = white;
                else
                    right = new XnaHSL(_Colors[i]);

                if (index <= 0f)
                {
                    XnaHSL hslResult = XnaHSL.Lerp(left, right, (index + 1));
                    return hslResult;
                }
            }
            return right;
        }
        public Color GetColor(float index)
        {
            return GetHsl(index).GetColor();
        }

        public void AddColor(Color color)
        {
            // If the color already exists
            if (_Colors.Contains(color))
                RemoveColor(color);

            _Colors.Add(color);
            _Adjustments.Add(new ColorVector());

            if (_BaseColorIndex < 0)
                SetBaseColor(0, null);

            // Order the colors by lightness
            OrderColors();
        }

        private void OrderColors()
        {
            if (!_Colors.Any())
                return;

            Color baseColor = this.BaseColor;

            var order = PaletteRamp.ColorLumaOrder(_Colors).ToList();
            _Adjustments = order
                .Select(x => _Adjustments[x])
                .ToList();
            _Colors = order
                .Select(x => _Colors[x])
                .ToList();

            _BaseColorIndex = _Colors.IndexOf(baseColor);
        }

        public void RemoveColor(int index)
        {
            RemoveColor(_Colors[index]);
        }
        public void RemoveColor(Color color)
        {
            int index = _Colors.IndexOf(color);

            if (index >= 0)
            {
                _Colors.RemoveAt(index);
                _Adjustments.RemoveAt(index);

                if (_BaseColorIndex >= _Colors.Count)
                    _BaseColorIndex--;
            }
        }

        public static IEnumerable<int> ColorLumaOrder(IEnumerable<Color> colors)
        {
            List<int> order = Enumerable
                .Range(0, colors.Count())
                .Select(x =>
                {
                    Color color = colors.ElementAt(x);
                    XnaHSL hsl = new XnaHSL(color);
                    return Tuple.Create(x, color, hsl);
                })
                // This actually needs to use luma instead of lightness,
                // because the perceptual order is important
                .OrderBy(x => GeneratedColorPalette.ValueFormula(x.Item2))
                .ThenBy(x => x.Item3.Lightness)
                // If colors have the same lightness,
                // use the one with the least saturation
                .ThenBy(x => x.Item3.Saturation)
                // If colors have the same lightness,
                // use the one with the most alpha
                .ThenByDescending(x => x.Item2.A)
                .Select(x => x.Item1)
                .ToList();
            return order;
        }

        public void SetBaseColor(float index, List<PaletteEntry> entries)
        {
            _BaseColorIndex = MathHelper.Clamp(index, -1, _Colors.Count);

            Generator.BaseLightness = new XnaHSL(this.BaseColor).Lightness;
            // Set initial palette values
            // If there are colors darker than the base color
            if (_BaseColorIndex > 0)
            {
                // Sets the darkest color a fraction of the distance to the base color
                float step = GeneratedColorPalette.BASE_VALUE / (_BaseColorIndex + 1);
                SetValue(0, step);
            }
            else
                Generator.ShadowAmount = 0.5f;
            // If there are colors lighter than the base color
            if (_BaseColorIndex < _Colors.Count - 1)
            {
                // Sets the lightest color a fraction of the distance to the base color
                float step = (1f - GeneratedColorPalette.BASE_VALUE) /
                    (_Colors.Count - _BaseColorIndex);
                SetValue(_Colors.Count - 1, 1f - step);
            }
            else
                Generator.Specularity = 0.5f;

            if (entries != null)
            {
                var parameters = new PaletteParameters(this.BaseColor);
                // Minimize error on shadow amount
                MinimizeShadowError(parameters, entries);
                // Minimize error on specularity
                MinimizeSpecularityError(parameters, entries);
            }
        }

        private void SetValue(int index, float value)
        {
            // Can't adjust the base color value
            if (index == _BaseColorIndex)
                return;

            var color = _Colors[index];

            // Update the gradient so that this index natrually has this value
            Generator.AdjustValue(
                color,
                this.BaseColor,
                value);
        }

        public void SetBlackLevel(Color darkestColor)
        {
            Generator.BlackLevel = GeneratedColorPalette.ValueFormula(darkestColor);
        }

        public PaletteParameters DefaultParameters(List<PaletteEntry> entries)
        {
            var parameters = new PaletteParameters(this.BaseColor);
            parameters.ReducedDepth = true;
            // Minimize error on blue shadow
            MinimizeBlueError(parameters, entries);
            // Minimize error on yellow light
            MinimizeYellowError(parameters, entries);

            return parameters;
        }

        [ContentSerializerIgnore]
        public List<float> ColorValues
        {
            get
            {
                var parameters = new PaletteParameters(this.BaseColor);
                var palette = GetColorPalette(parameters);

                var result = _Colors
                    .Select(x => palette.GetValue(x))
                    .ToList();
                return result;
            }
        }

        private void MinimizeShadowError(PaletteParameters parameters, List<PaletteEntry> entries)
        {
            Generator.MinimizeShadowError(parameters, entries);
            RefreshAdjustments(entries);
        }
        private void MinimizeSpecularityError(PaletteParameters parameters, List<PaletteEntry> entries)
        {
            Generator.MinimizeSpecularityError(parameters, entries);
            RefreshAdjustments(entries);
        }
        public void MinimizeBlueError(PaletteParameters parameters, List<PaletteEntry> entries)
        {
            parameters.MinimizeBlueError(Generator, entries);
        }
        public void MinimizeYellowError(PaletteParameters parameters, List<PaletteEntry> entries)
        {
            parameters.MinimizeYellowError(Generator, entries);
        }

        public void RefreshAdjustments(List<PaletteEntry> entries)
        {
            var basePalette = GetDefaultColorPalette(entries);
            var values = _Colors
                .Select(x => basePalette.GetValue(x))
                .ToArray();

            for (int i = 0; i < _Colors.Count; i++)
            {
                // Get the adjustment that turns the palette color back into
                // the ramp color
                Color source = basePalette.GetColor(values[i]);
                Color target = _Colors[i];

                var adjustment = ColorVector.ColorDifference(source, target, BlueYellowAdjustments);
                _Adjustments[i] = adjustment;
            }
        }

        public GeneratedColorPalette GetDefaultColorPalette(List<PaletteEntry> entries)
        {
            return GetColorPalette(DefaultParameters(entries));
        }
        public GeneratedColorPalette GetColorPalette(PaletteParameters parameters)
        {
            parameters = (PaletteParameters)parameters.Clone();

            // Adjust the lightness of the base color
            XnaHSL baseHsl = new XnaHSL(this.BaseColor);
            XnaHSL parametersHsl = new XnaHSL(parameters.BaseColor);
            parametersHsl = parametersHsl.SetLightness(
                MathHelper.Lerp(
                    parametersHsl.Lightness,
                    baseHsl.Lightness,
                    parameters.OriginalLightness));
            parameters.BaseColor = parametersHsl.GetColor();

            return Generator.GetPalette(parameters);
        }

        public IndexedGeneratedPalette GetDefaultIndexedPalette(List<PaletteEntry> entries)
        {
            return GetIndexedPalette(DefaultParameters(entries));
        }
        public IndexedGeneratedPalette GetIndexedPalette(PaletteParameters parameters)
        {
            return new IndexedGeneratedPalette(this, parameters);
        }

        #region ICloneable
        public object Clone()
        {
            return new PaletteRamp(this);
        }
        #endregion
    }
}
