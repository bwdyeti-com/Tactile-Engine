using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FEXNA_Library.Palette
{
    public struct GeneratedColorPalette
    {
        public const float BASE_VALUE = 0.6f;

        public Color BaseColor { get; private set; }
        public Color Darkest { get; private set; }
        private Color Shadow;
        private Color Highlight;
        public Color Specular { get; private set; }

        public bool ReducedDepth;

        public GeneratedColorPalette(
            PaletteGenerator generator,
            PaletteParameters parameters)
        {
            // Adjust base color up if black level is too close to it
            Color baseColor = AdjustBaseColor(parameters.BaseColor, generator.BlackLevel);

            Color highlight, specular;
            GetHighlight(baseColor, generator, parameters,
                out highlight, out specular);

            Color shadow, darkest;
            GetShadow(baseColor, generator, parameters,
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
        private static Color AdjustBaseColor(Color color, float blackLevel)
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
            return adjusted;
        }

        private static void GetHighlight(
            Color baseColor,
            PaletteGenerator generator,
            PaletteParameters parameters,
            out Color highlight,
            out Color specular)
        {
            XnaHSL baseHSL = new XnaHSL(baseColor);
            Color baseSpecular =  PaletteGenerator.BaseSpecularColor(baseColor);

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
            // Reduce the lightness if the base color is darker than the source color
            if (generator.BaseLightness > 0 && baseHSL.Lightness < generator.BaseLightness)
            {
                float lightnessDiff = generator.BaseLightness - baseHSL.Lightness;
                specularHSL = specularHSL.SetLightness(specularHSL.Lightness - (lightnessDiff));
            }
            specular = specularHSL.GetColor();

            // Get highlight
            XnaHSL highlightHSL = XnaHSL.Lerp(baseHSL, specularHSL, 0.5f);

            highlight = highlightHSL.GetColor();
        }

        private static void GetShadow(
            Color baseColor,
            PaletteGenerator generator,
            PaletteParameters parameters,
            out Color shadow,
            out Color darkest)
        {
            XnaHSL baseHSL = new XnaHSL(baseColor);
            XnaHSL blackHSL = new XnaHSL(baseHSL.Hue, baseHSL.Saturation, generator.BlackLevel, baseColor.A);

            // Tint shadow blue
            XnaHSL shadowHSL = XnaHSL.Lerp(blackHSL, baseHSL, generator.ShadowAmount);
            shadowHSL = shadowHSL.LerpHue(240, parameters.BlueShadow);
            blackHSL = blackHSL.SetHue(shadowHSL.Hue);

            shadow = shadowHSL.GetColor();
            darkest = new XnaHSL(0, 0, generator.BlackLevel, baseColor.A).GetColor();
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
                    Color c = Color.Lerp(start.Item1, end.Item1,
                        (value - start.Item2) / range);
                    return DepthColor(c);
                }
                start = end;
            }
            return DepthColor(Specular);
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
            return new XnaHSL(color).Lightness;
        }

        public bool DarkerThanBase(Color color)
        {
            return ValueFormula(color) < ValueFormula(BaseColor);
        }

        public float GetError(List<PaletteEntry> ramp)
        {
            var palette = this;
            var values = ramp
                .Select(x => palette.GetValue(x.Value))
                .ToList();

            // Show final error
            float error = 0;
            for (int i = 0; i < ramp.Count; i++)
            {
                int weight = ramp[i].Weight;
                var source = ramp[i].Value;
                var target = GetColor(values[i]);
                
                var vector = XnaHSL.BiconeDifference(source, target);

                float length = vector.LengthSquared();
                error += length * weight;
            }
            int totalWeight = ramp.Select(x => x.Weight).Sum();
            error /= totalWeight;

            return error;
        }

        private IEnumerable<Tuple<Color, float>> GetRanges()
        {
            var anchors = AnchorValues().ToArray();
            var colors = AnchorColors().ToArray();

            for (int i = 0; i < anchors.Length; i++)
            {
                yield return new Tuple<Color, float>(colors[i], anchors[i]);
            }
        }

        private IEnumerable<Color> AnchorColors()
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
