using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FEXNA_Library.Palette
{
    public class IndexedGeneratedPalette
    {
        private GeneratedColorPalette BasePalette;
        private float[] ColorValues;
        private ColorVector[] PaletteAdjustments;
        float AdjustmentsSaturationMultiplier;

        public IndexedGeneratedPalette(
            PaletteRamp ramp,
            PaletteParameters parameters)
        {
            BasePalette = ramp.GetColorPalette(parameters);

            ColorValues = ramp.ColorValues.ToArray();
            PaletteAdjustments = ramp.Adjustments.ToArray();

#if DEBUG
            System.Diagnostics.Debug.Assert(ramp.Colors.Count == ColorValues.Length, "Palette and ramp size must match");
            System.Diagnostics.Debug.Assert(ramp.Colors.Count == PaletteAdjustments.Length, "Palette and ramp size must match");
#endif

            // Saturation of the adjustment is reduced if the generated palette
            // is itself low saturation
            var rawParameters = (PaletteParameters)parameters.Clone();
            rawParameters.YellowLight = 0;
            rawParameters.BlueShadow = 0;
            var rawPalette = ramp.GetColorPalette(rawParameters);

            AdjustmentsSaturationMultiplier = SaturationMultiplier(
                rawPalette, ramp, ramp.BaseColorIndex);

            //@Debug: per color saturation adjustments
            /*AdjustmentsSaturationMultipliers = PaletteAdjustments
                .Select(x => 1f)
                .ToArray();
            for (int i = 0; i < AdjustmentsSaturationMultipliers.Length; i++)
            {
                AdjustmentsSaturationMultipliers[i] = SaturationMultiplier(
                    rawPalette, ramp, i);
            }*/
        }

        private float SaturationMultiplier(GeneratedColorPalette rawPalette, PaletteRamp ramp, int index)
        {
            Color target = rawPalette.GetColor(ColorValues[index]);
            Color source = ramp.Colors[index];

            XnaHSL targetHsl = new XnaHSL(target);
            XnaHSL sourceHsl = new XnaHSL(source);

            if (targetHsl.Saturation < sourceHsl.Saturation && sourceHsl.Saturation > 0)
                return targetHsl.Saturation / sourceHsl.Saturation;
            else
                return 1f;
        }

        public Color GetColor(int index)
        {
            if (index < 0 || index >= ColorValues.Length)
                throw new ArgumentException();

            return BasePalette.GetColor(ColorValues[index]) + GetAdjustment(index);
        }

        public ColorVector GetAdjustment(int index)
        {
            if (index < 0 || index >= PaletteAdjustments.Length)
                throw new ArgumentException();

            var adjustment = PaletteAdjustments[index];
            adjustment = adjustment.SetSaturation(
                adjustment.Saturation * AdjustmentsSaturationMultiplier);
            return PaletteAdjustments[index];
        }
    }
}
