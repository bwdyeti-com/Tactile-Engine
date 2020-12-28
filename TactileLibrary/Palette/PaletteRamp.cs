using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ListExtension;
using TactileContentExtension;

namespace TactileLibrary.Palette
{
    public class PaletteRamp : TactileDataContent
    {
        [ContentSerializer]
        internal string Name;
        [ContentSerializer(ElementName = "BaseColorIndex")]
        private float _BaseColorIndex;
        [ContentSerializer(ElementName = "Colors")]
        private List<Color> _Colors;
        [ContentSerializer(ElementName = "Adjustments")]
        private List<ColorVector> _Adjustments;
        [ContentSerializer]
        internal PaletteGenerator Generator;
        [ContentSerializer]
        internal bool BlueYellowAdjustments;

        [ContentSerializerIgnore]
        internal float BaseColorIndex
        {
            get { return _BaseColorIndex; }
            set { _BaseColorIndex = MathHelper.Clamp(value, -1, _Colors.Count); }
        }
        [ContentSerializerIgnore]
        internal List<Color> Colors
        {
            get
            {
                return new List<Color>(_Colors);
            }
        }
        [ContentSerializerIgnore]
        internal List<ColorVector> Adjustments
        {
            get
            {
                return new List<ColorVector>(_Adjustments);
            }
        }

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static PaletteRamp GetEmptyInstance()
        {
            return new PaletteRamp();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var source = (PaletteRamp)other;

            Name = source.Name;
            _BaseColorIndex = source._BaseColorIndex;
            _Colors = new List<Color>(source._Colors);
            _Adjustments = new List<ColorVector>(source._Adjustments);
            Generator = (PaletteGenerator)source.Generator.Clone();
            BlueYellowAdjustments = source.BlueYellowAdjustments;
        }

        public static PaletteRamp ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            Name = input.ReadString();
            _BaseColorIndex = input.ReadSingle();
            _Colors.read(input);
            input.ReadTactileContentStruct(_Adjustments);
            Generator.Read(input);
            BlueYellowAdjustments = input.ReadBoolean();
        }

        public override void Write(BinaryWriter output)
        {
            output.Write(Name);
            output.Write(_BaseColorIndex);
            _Colors.write(output);
            output.WriteStruct(_Adjustments);
            Generator.Write(output);
            output.Write(BlueYellowAdjustments);
        }
        #endregion

        internal PaletteRamp() : this("", Color.Black) { }
        internal PaletteRamp(string name, Color darkestColor)
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
            CopyFrom(source);
        }

        public override string ToString()
        {
            return string.Format("PaletteRamp: {0}; {1} colors", Name, this.Count);
        }

        [ContentSerializerIgnore]
        internal int Count { get { return _Colors.Count; } }

        internal void AddColor(Color color)
        {
            _Colors.Add(color);
            _Adjustments.Add(new ColorVector());
        }

        internal void RemoveColor(int index)
        {
            RemoveColor(_Colors[index]);
        }
        internal void RemoveColor(Color color)
        {
            int index = _Colors.IndexOf(color);

            if (index >= 0)
            {
                _Colors.RemoveAt(index);
                _Adjustments.RemoveAt(index);

                if (_BaseColorIndex >= index)
                    _BaseColorIndex--;
            }
        }

        internal Color GetColor(int index)
        {
            return _Colors[index];
        }

        internal void SetAdjustment(int index, ColorVector adjustment)
        {
            _Adjustments[index] = adjustment;
        }

        internal void SetColors(List<Color> colors, List<ColorVector> adjustments)
        {
            _Colors = colors;
            _Adjustments = adjustments;

            _BaseColorIndex = 0;
        }

        internal static IEnumerable<int> ColorLumaOrder(IEnumerable<Color> colors)
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

        internal void SetBlackLevel(Color darkestColor)
        {
            Generator.BlackLevel = GeneratedColorPalette.ValueFormula(darkestColor);
        }

        #region ICloneable
        public override object Clone()
        {
            return new PaletteRamp(this);
        }
        #endregion
    }
}
