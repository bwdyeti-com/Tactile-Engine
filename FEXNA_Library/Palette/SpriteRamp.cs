using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FEXNA_Library.Palette
{
    public class SpriteRamp : ICloneable
    {
        private SpritePalette Palette;
        private PaletteRamp Ramp;

        internal SpriteRamp(SpritePalette palette, PaletteRamp ramp)
        {
            Palette = palette;
            Ramp = ramp;
        }
        private SpriteRamp(SpriteRamp source)
        {
            Palette = source.Palette;
            Ramp = (PaletteRamp)source.Ramp.Clone();
        }

        public override string ToString()
        {
            return string.Format("SpriteRamp: {0}", this.Name);
        }

        public string Name { get { return Ramp.Name; } }
        public int Count { get { return Ramp.Count; } }

        public Color BaseColor
        {
            get
            {
                return GetColor(Ramp.BaseColorIndex);
            }
        }
        public float BaseColorIndex { get { return Ramp.BaseColorIndex; } }

        public List<Color> Colors { get { return Ramp.Colors; } }
        public List<ColorVector> Adjustments { get { return Ramp.Adjustments; } }

        public bool BlueYellowAdjustments
        {
            get { return Ramp.BlueYellowAdjustments; }
            set
            {
                Ramp.BlueYellowAdjustments = value;
                RefreshAdjustments();
            }
        }

        public XnaHSL GetHsl(float index)
        {
            PaletteEntry entry = Palette.GetEntry(Ramp.Colors.FirstOrDefault());
            XnaHSL black = new XnaHSL(entry.Color);
            black = black.SetSaturation(0f);
            black = black.SetLightness(0f);
            entry = Palette.GetEntry(Ramp.Colors.LastOrDefault());
            XnaHSL white = new XnaHSL(entry.Color);
            white = white.SetSaturation(0f);
            white = white.SetLightness(1f);

            XnaHSL left = black;
            XnaHSL right = white;

            for (int i = 0; i <= Ramp.Count; i++, index -= 1f, left = right)
            {
                if (i == Ramp.Count)
                    right = white;
                else
                {
                    entry = Palette.GetEntry(Ramp.GetColor(i));
                    right = new XnaHSL(entry.Color);
                }

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

        public List<PaletteEntry> RampEntries()
        {
            var result = Ramp.Colors
                .Select(color =>
                {
                    int index = Palette.ColorIndex(color);
                    if (index >= 0)
                        return Palette.GetEntry(index);
                    else
                        return new PaletteEntry(color, 0);
                })
                .ToList();
            return result;
        }
        public PaletteEntry GetEntry(int index)
        {
            return Palette.GetEntry(Ramp.GetColor(index));
        }

        public List<PaletteEntry> SpriteEntries()
        {
            return Palette.GetEntries();
        }

        public void AddRampColor(Color color)
        {
            // If the color already exists
            if (Ramp.Colors.Contains(color))
                Ramp.RemoveColor(color);

            Ramp.AddColor(color);

            if (Ramp.BaseColorIndex < 0)
                SetBaseColor(0);

            // Order the colors by lightness
            OrderColors();
        }
        public void RemoveRampColor(int index)
        {
            Ramp.RemoveColor(index);
        }

        internal void OrderColors()
        {
            if (!Ramp.Colors.Any())
                return;

            int oldBaseIndex = (int)Math.Floor(Ramp.BaseColorIndex);
            float adjustment = Ramp.BaseColorIndex - oldBaseIndex;

            var entries = Palette.RampEntries(Ramp);
            var order = PaletteRamp.ColorLumaOrder(entries.Select(x => x.Color)).ToList();
            var colors = order
                .Select(x => Ramp.Colors[x])
                .ToList();
            var adjustments = order
                .Select(x => Ramp.Adjustments[x])
                .ToList();

            Ramp.SetColors(colors, adjustments);

            float newBaseIndex = oldBaseIndex;
            if (oldBaseIndex >= 0 && oldBaseIndex < this.Count)
                newBaseIndex = order.IndexOf(oldBaseIndex);
            newBaseIndex += adjustment;

            Ramp.BaseColorIndex = newBaseIndex;
        }

        public void SetBaseColor(float index)
        {
            Ramp.BaseColorIndex = index;

            RecalibrateError();
        }

        internal void RecalibrateError()
        {
            Ramp.Generator.BaseLightness = new XnaHSL(this.BaseColor).Lightness;

            // Set initial palette values
            // If there are colors darker than the base color
            if (Ramp.BaseColorIndex > 0)
            {
                // Sets the darkest color a fraction of the distance to the base color
                float step = GeneratedColorPalette.BASE_VALUE / (Ramp.BaseColorIndex + 1);
                SetValue(0, step);
            }
            else
                Ramp.Generator.ShadowAmount = 0.5f;

            // If there are colors lighter than the base color
            if (Ramp.BaseColorIndex < Ramp.Count - 1)
            {
                // Sets the lightest color a fraction of the distance to the base color
                float step = (1f - GeneratedColorPalette.BASE_VALUE) /
                    (Ramp.Count - Ramp.BaseColorIndex);
                SetValue(Ramp.Count - 1, 1f - step);
            }
            else
                Ramp.Generator.Specularity = 0.5f;

            var parameters = new PaletteParameters(this.BaseColor);
            // Minimize error on shadow amount
            MinimizeShadowError(parameters);
            // Minimize error on specularity
            MinimizeSpecularityError(parameters);
        }

        private void SetValue(int index, float value)
        {
            // Can't adjust the base color value
            if (index == Ramp.BaseColorIndex)
                return;

            PaletteEntry entry = GetEntry(index);
            var color = entry.Color;

            // Update the gradient so that this index natrually has this value
            Ramp.Generator.AdjustValue(
                color,
                this.BaseColor,
                value);
        }

        public PaletteParameters DefaultParameters()
        {
            var parameters = new PaletteParameters(this.BaseColor);
            parameters.ReducedDepth = true;
            // Minimize error on blue shadow
            MinimizeBlueError(parameters);
            // Minimize error on yellow light
            MinimizeYellowError(parameters);

            return parameters;
        }

        private void MinimizeShadowError(PaletteParameters parameters)
        {
            var entries = Palette.RampEntries(Ramp);
            Ramp.Generator.MinimizeShadowError(parameters, entries);
            RefreshAdjustments();
        }
        private void MinimizeSpecularityError(PaletteParameters parameters)
        {
            var entries = Palette.RampEntries(Ramp);
            Ramp.Generator.MinimizeSpecularityError(parameters, entries);
            RefreshAdjustments();
        }
        public void MinimizeBlueError(PaletteParameters parameters)
        {
            var entries = Palette.RampEntries(Ramp);
            parameters.MinimizeBlueError(Ramp.Generator, entries);
        }
        public void MinimizeYellowError(PaletteParameters parameters)
        {
            var entries = Palette.RampEntries(Ramp);
            parameters.MinimizeYellowError(Ramp.Generator, entries);
        }

        public void RefreshAdjustments()
        {
            var basePalette = GetDefaultColorPalette();
            var entries = Palette.RampEntries(Ramp);
            var values = entries
                .Select(x => x.Color)
                .Select(x => basePalette.GetValue(x))
                .ToArray();

            for (int i = 0; i < Ramp.Count; i++)
            {
                // Get the adjustment that turns the palette color back into
                // the ramp color
                Color source = basePalette.GetColor(values[i]);
                PaletteEntry entry = entries[i];
                Color target = entry.Color;

                var adjustment = ColorVector.ColorDifference(source, target, Ramp.BlueYellowAdjustments);
                Ramp.SetAdjustment(i, adjustment);
            }
        }

        public float GetError(GeneratedColorPalette colorPalette)
        {
            return colorPalette.GetError(RampEntries());
        }

        public List<float> ColorValues
        {
            get
            {
                var parameters = new PaletteParameters(this.BaseColor);
                var palette = GetColorPalette(parameters);

                var entries = Palette.RampEntries(Ramp);
                var result = entries
                    .Select(x => x.Color)
                    .Select(x => palette.GetValue(x))
                    .ToList();
                return result;
            }
        }

        public void AdjustGeneratorValue(Color color, float value)
        {
            Ramp.Generator.AdjustValue(color, this.BaseColor, value);
        }

        public void SetGeneratorSpecularity(float value)
        {
            Ramp.Generator.Specularity = value;
        }
        public void SetGeneratorShadow(float value)
        {
            Ramp.Generator.ShadowAmount = value;
        }
        public void SetGeneratorBlack(float value)
        {
            Ramp.Generator.BlackLevel = value;
        }

        public PaletteGenerator GetGenerator()
        {
            return (PaletteGenerator)Ramp.Generator.Clone();
        }

        public GeneratedColorPalette GetDefaultColorPalette()
        {
            return GetColorPalette(DefaultParameters());
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

            return Ramp.Generator.GetPalette(parameters);
        }

        public IndexedGeneratedPalette GetDefaultIndexedPalette()
        {
            return GetIndexedPalette(DefaultParameters());
        }
        public IndexedGeneratedPalette GetIndexedPalette(PaletteParameters parameters)
        {
            return new IndexedGeneratedPalette(this, parameters);
        }

        public void PaletteReplaceRamp(int index)
        {
            Palette.ReplaceRamp(index, Ramp);
        }

        #region ICloneable
        public object Clone()
        {
            return new SpriteRamp(this);
        }
        #endregion
    }
}
