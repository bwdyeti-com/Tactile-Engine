using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FEXNA_Library.Palette
{
    public class PaletteRamp : ICloneable
    {
        public string Name;
        public int BaseColorIndex;
        public List<Color> Colors;
        public List<ColorVector> Adjustments;
        public PaletteGenerator Generator;
        public bool BlueYellowAdjustments;

        public PaletteRamp()
        {
            Name = "";
            BaseColorIndex = -1;
            Colors = new List<Color>();
            Adjustments = new List<ColorVector>();
            Generator = new PaletteGenerator();
            BlueYellowAdjustments = true;
        }
        private PaletteRamp(PaletteRamp source)
        {
            Name = source.Name;
            BaseColorIndex = source.BaseColorIndex;
            Colors = new List<Color>(source.Colors);
            Adjustments = new List<ColorVector>(source.Adjustments);
            Generator = (PaletteGenerator)source.Generator.Clone();
            BlueYellowAdjustments = source.BlueYellowAdjustments;
        }

        public Color BaseColor
        {
            get
            {
                if (BaseColorIndex < 0)
                    return Color.Black;
                return Colors[BaseColorIndex];
            }
        }

        public void AddColor(Color color)
        {
            // If the color already exists
            if (Colors.Contains(color))
                RemoveColor(color);

            Colors.Add(color);
            Adjustments.Add(new ColorVector());

            if (BaseColorIndex < 0)
                SetBaseColor(0, null);

            // Order the colors by lightness
            OrderColors();
        }

        private void OrderColors()
        {
            if (!Colors.Any())
                return;

            Color baseColor = this.BaseColor;

            var order = PaletteRamp.ColorLightnessOrder(Colors).ToList();
            Adjustments = order
                .Select(x => Adjustments[x])
                .ToList();
            Colors = order
                .Select(x => Colors[x])
                .ToList();

            BaseColorIndex = Colors.IndexOf(baseColor);
        }

        public void RemoveColor(int index)
        {
            RemoveColor(Colors[index]);
        }
        public void RemoveColor(Color color)
        {
            int index = Colors.IndexOf(color);

            if (index >= 0)
            {
                Colors.RemoveAt(index);
                Adjustments.RemoveAt(index);

                if (BaseColorIndex >= Colors.Count)
                    BaseColorIndex--;
            }
        }

        public static IEnumerable<int> ColorLightnessOrder(IEnumerable<Color> colors)
        {
            List<int> order = Enumerable
                .Range(0, colors.Count())
                .Select(x =>
                {
                    Color color = colors.ElementAt(x);
                    XnaHSL hsl = new XnaHSL(color);
                    return Tuple.Create(x, color, hsl);
                })
                .OrderBy(x => x.Item3.Lightness)
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

        public void SetBaseColor(int index, List<PaletteEntry> entries)
        {
            if (index < 0 || index >= Colors.Count)
                return;

            BaseColorIndex = index;

            // Set initial palette values
            // If there are colors darker than the base color
            if (BaseColorIndex > 0)
            {
                // Sets the darkest color a fraction of the distance to the base color
                float step = GeneratedColorPalette.BASE_VALUE / (BaseColorIndex + 1);
                SetValue(0, step);
            }
            else
                Generator.ShadowAmount = 0.5f;
            // If there are colors lighter than the base color
            if (BaseColorIndex < Colors.Count - 1)
            {
                // Sets the lightest color a fraction of the distance to the base color
                float step = (1f - GeneratedColorPalette.BASE_VALUE) /
                    (Colors.Count - BaseColorIndex);
                SetValue(Colors.Count - 1, 1f - step);
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

        public void SetValue(int index, float value)
        {
            // Can't adjust the base color value
            if (index == BaseColorIndex)
                return;

            var color = Colors[index];

            // Update the gradient so that this index natrually has this value
            Generator.AdjustValue(
                color,
                this.BaseColor,
                value);
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

        public List<float> ColorValues
        {
            get
            {
                var parameters = new PaletteParameters(this.BaseColor);
                var palette = GetColorPalette(parameters);

                var result = Colors
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
            var values = Colors
                .Select(x => basePalette.GetValue(x))
                .ToArray();

            for (int i = 0; i < Colors.Count; i++)
            {
                // Get the adjustment that turns the palette color back into
                // the ramp color
                Color source = basePalette.GetColor(values[i]);
                Color target = Colors[i];

                var adjustment = ColorVector.ColorDifference(source, target, BlueYellowAdjustments);
                Adjustments[i] = adjustment;
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
