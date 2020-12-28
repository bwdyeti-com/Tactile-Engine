using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TactileLibrary.Palette
{
    public struct GeneratedColorPalette
    {
        public const float BASE_VALUE = 0.6f;

        public XnaHSL BaseColor { get; private set; }
        public XnaHSL Darkest { get; private set; }
        private XnaHSL Shadow;
        private XnaHSL Highlight;
        public XnaHSL Specular { get; private set; }

        public bool ReducedDepth;

        public GeneratedColorPalette(
            PaletteGenerator generator,
            PaletteParameters parameters)
        {
            // Adjust base color up if black level is too close to it
            XnaHSL baseColor = AdjustBaseColor(parameters.BaseColor, generator.BlackLevel);

            // Tint just a little toward yellow based on yellow light
            XnaHSL untintedBaseColor = baseColor;
            if (baseColor.Saturation > 0)
            {
                baseColor = baseColor.LerpHue(60, parameters.YellowLight * 0.075f);
            }

            XnaHSL highlight, specular;
            GetHighlight(baseColor, generator, parameters,
                out highlight, out specular);

            XnaHSL shadow, darkest;
            GetShadow(untintedBaseColor, generator, parameters,
                out shadow, out darkest);

            // Set the calculated values
            BaseColor = baseColor;
            Darkest = darkest;
            Shadow = shadow;
            Highlight = highlight;
            Specular = specular;
            ReducedDepth = parameters.ReducedDepth;
        }

        private static float BlackLevelAdjustment(float inputLightness, float blackLevel)
        {
            float min = MathHelper.Lerp(blackLevel, 1f, 0.05f);
            float scale = MathHelper.Lerp(16f, 64f, blackLevel);
            float softPlus = SoftPlus((inputLightness - min) * scale) / scale + min;
            return Math.Min(softPlus, 1f);
        }

        /// <summary>
        /// A smoothed rectifier function.
        /// Returns 0 as x approaches -infinity, and returns x as x approaches +infinity.
        /// </summary>
        private static float SoftPlus(float x)
        {
            return (float)Math.Log(1 + Math.Exp(x));
        }

        /// <summary>
        /// Adjusts base color lightness up if it's too close to the BlackLevel
        /// </summary>
        private static XnaHSL AdjustBaseColor(Color color, float blackLevel)
        {
            // Use HSV instead of HSL to have finer control over the saturation
            float h, s, v;
            Color_Util.ColorToHSV(color, out h, out s, out v);
            float adjustedV = BlackLevelAdjustment(v, blackLevel);
            // Adjust the saturation down if the color is brightened, so
            // extremely dark colors don't have strangely high saturation
            float adjustedS = adjustedV == 0 ? 0 : s * (v / adjustedV);
            Color adjusted = Color_Util.HSVToColor(h, adjustedS, adjustedV);
            adjusted.A = color.A;

            return new XnaHSL(adjusted);
        }

        private static void GetHighlight(
            XnaHSL baseHSL,
            PaletteGenerator generator,
            PaletteParameters parameters,
            out XnaHSL highlight,
            out XnaHSL specular)
        {
            Color baseSpecular =  PaletteGenerator.BaseSpecularColor(baseHSL.GetColor());

            // Tint yellow
            XnaHSL specularHSL = new XnaHSL(baseSpecular);
            // If greyscale, simply treat the hue as yellow
            if (float.IsNaN(specularHSL.Hue))
                specularHSL = specularHSL.SetHue(60);
            specularHSL = specularHSL.LerpHue(60, parameters.YellowLight);
            specularHSL = specularHSL.SetSaturation(MathHelper.Lerp(
                specularHSL.Saturation, 1f, parameters.YellowLight));
            specularHSL = specularHSL.SetLightness(MathHelper.Lerp(
                specularHSL.Lightness, Math.Max(0.5f, specularHSL.Lightness), parameters.YellowLight));

            // Add specular
            specularHSL = specularHSL.SetLightness(MathHelper.Lerp(
                specularHSL.Lightness, 1f, generator.Specularity));
            // Reduce the lightness if the base color is darker than the generator color
            if (generator.BaseLightness > 0 && baseHSL.Lightness < generator.BaseLightness)
            {
                float lightnessDiff = generator.BaseLightness - baseHSL.Lightness;
                specularHSL = specularHSL.SetLightness(specularHSL.Lightness - (lightnessDiff));
            }
            float highlightAmount = 0.5f;
            // If the base color is sufficiently bright, lighten the specular
            // to keep the contrast from getting too low
            float lightnessRatio = (baseHSL.Lightness - 0.5f) * 2f;
            if (lightnessRatio > 0)
            {
                specularHSL = specularHSL.SetLightness(
                    MathHelper.Lerp(specularHSL.Lightness, 1f, lightnessRatio));
                highlightAmount = MathHelper.Lerp(0.5f, 1f, lightnessRatio);
            }

            // Get highlight
            XnaHSL highlightHSL = XnaHSL.Lerp(baseHSL, specularHSL, highlightAmount);

            highlight = highlightHSL;
            specular = specularHSL;
        }

        private static void GetShadow(
            XnaHSL baseHSL,
            PaletteGenerator generator,
            PaletteParameters parameters,
            out XnaHSL shadow,
            out XnaHSL darkest)
        {
            XnaHSL blackHSL = new XnaHSL(
                baseHSL.Hue,
                baseHSL.Saturation * generator.ShadowAmount,
                generator.BlackLevel,
                baseHSL.Alpha);

            // If the base color is sufficiently bright, lighten the darker shades
            float lightnessRatio = (baseHSL.Lightness - 0.5f) * 2f;
            if (lightnessRatio > 0)
            {
                blackHSL = blackHSL.SetSaturation(blackHSL.Saturation * (1f - lightnessRatio * 0.95f));

                float blackLuma = Color_Util.GetLuma(blackHSL.GetColor());
                // Increase the lightness by more if it's a color with a low
                // luma to lightness ratio
                float lumaRatio = 1f;
                if (blackLuma > 0)
                    lumaRatio = blackHSL.Lightness / blackLuma;

                blackHSL = blackHSL.SetLightness(
                    MathHelper.Lerp(blackHSL.Lightness,
                        MathHelper.Lerp(blackHSL.Lightness, 1f, 0.25f),
                        lightnessRatio * lumaRatio));
            }

            // Tint black and shadow blue
            blackHSL = blackHSL.LerpHue(240, parameters.BlueShadow);
            blackHSL = blackHSL.SetSaturation(MathHelper.Lerp(blackHSL.Saturation, 1f, parameters.BlueShadow / 4f));

            XnaHSL shadowHSL = XnaHSL.Lerp(blackHSL, baseHSL, generator.ShadowAmount);
            shadowHSL = shadowHSL.SetSaturation(MathHelper.Lerp(blackHSL.Saturation, baseHSL.Saturation, 0.5f));
            shadowHSL = shadowHSL.SetHue(blackHSL.Hue);
            shadowHSL = shadowHSL.LerpHue(baseHSL.Hue, 0.25f);

            shadow = shadowHSL;
            darkest = blackHSL;
        }

        public Color GetColor(float value)
        {
            var ranges = GetRanges();
            var start = ranges.First();
            foreach (var end in ranges.Skip(1))
            {
                if (value < end.Item2)
                {
                    float range = end.Item2 - start.Item2;
                    XnaHSL hsl = XnaHSL.Lerp(start.Item1, end.Item1,
                        (value - start.Item2) / range);
                    return DepthColor(hsl.GetColor());
                }
                start = end;
            }
            return DepthColor(Specular.GetColor());
        }
        private Color DepthColor(Color c)
        {
            if (ReducedDepth)
            {
                int r = (int)MathHelper.Clamp((float)Math.Round(c.R / 8f), 0, 31) * 8;
                int g = (int)MathHelper.Clamp((float)Math.Round(c.G / 8f), 0, 31) * 8;
                int b = (int)MathHelper.Clamp((float)Math.Round(c.B / 8f), 0, 31) * 8;
                return new Color(r, g, b, c.A);
            }
            else
                return c;
        }

        public float GetValue(Color color)
        {
            float targetLight = ValueFormula(color);

            var ranges = GetRanges();
            var start = ranges.First();

            // If lighter than the whole gradient
            float startLight = ValueFormula(start.Item1);
            if (targetLight < startLight)
                return 0f;

            foreach (var end in ranges.Skip(1))
            {
                // Skip if same color
                if (start.Item1 != end.Item1)
                {
                    startLight = ValueFormula(start.Item1);
                    float endLight = ValueFormula(end.Item1);

                    if (targetLight < endLight)
                    {
                        float percent = (targetLight - startLight) / (endLight - startLight);
                        float value = (end.Item2 - start.Item2) * percent + start.Item2;
                        return value;
                    }
                }
                start = end;
            }
            return 1f;
        }

        public static float ValueFormula(Color color)
        {
            return Color_Util.GetLuma(color);
        }
        public static float ValueFormula(XnaHSL hsl)
        {
            return ValueFormula(hsl.GetColor());
        }

        public bool DarkerThanBase(Color color)
        {
            return ValueFormula(color) < ValueFormula(BaseColor);
        }

        public float GetError(List<PaletteEntry> ramp)
        {
            var palette = this;
            var values = ramp
                .Select(x => palette.GetValue(x.Color))
                .ToList();

            // Show final error
            float error = 0;
            for (int i = 0; i < ramp.Count; i++)
            {
                int weight = ramp[i].Weight;
                var source = ramp[i].Color;
                var target = GetColor(values[i]);
                
                var vector = XnaHSL.BiconeDifference(source, target);

                float length = vector.LengthSquared();
                error += length * weight;
            }
            int totalWeight = ramp.Select(x => x.Weight).Sum();
            error /= totalWeight;

            return error;
        }

        private IEnumerable<Tuple<XnaHSL, float>> GetRanges()
        {
            var anchors = AnchorValues().ToArray();
            var colors = AnchorColors().ToArray();

            for (int i = 0; i < anchors.Length; i++)
            {
                yield return new Tuple<XnaHSL, float>(colors[i], anchors[i]);
            }
        }

        private IEnumerable<XnaHSL> AnchorColors()
        {
            yield return Darkest;
            yield return Shadow;
            yield return BaseColor;
            yield return Highlight;
            yield return Specular;
        }
        public static IEnumerable<float> AnchorValues()
        {
            yield return 0f;
            yield return BASE_VALUE / 2f;
            yield return BASE_VALUE;
            yield return MathHelper.Lerp(BASE_VALUE, 1f, 5f / 8f);
            yield return 1f;
        }
    }
}
