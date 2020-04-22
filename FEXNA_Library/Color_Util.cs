using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FEXNA_Library
{
    public static class Color_Util
    {
        /// <summary>
        /// Converts HSV coordinates to a Color object.
        /// </summary>
        public static Color HSVToColor(float h, float s, float v)
        {
            if (float.IsNaN(h) || s == 0)
                return new Color(v, v, v);
            h /= 60f;

            float c = s * v;
            float x = c * (1 - Math.Abs(h % 2 - 1));
            float m = v - c;

            if (h < 1) return new Color(c + m, x + m, m);
            else if (h < 2) return new Color(x + m, c + m, m);
            else if (h < 3) return new Color(m, c + m, x + m);
            else if (h < 4) return new Color(m, x + m, c + m);
            else if (h < 5) return new Color(x + m, m, c + m);
            else return new Color(c + m, m, x + m);
        }
        public static void ColorToHSV(Color color, out float h, out float s, out float v)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float min, max, delta;
            min = System.Math.Min(System.Math.Min(r, g), b);
            max = System.Math.Max(System.Math.Max(r, g), b);
            v = max;               // v
            delta = max - min;
            if (max != 0)
            {
                s = delta / max;       // s

                if (r == max)
                    h = (g - b) / delta;       // between yellow & magenta
                else if (g == max)
                    h = 2 + (b - r) / delta;   // between cyan & yellow
                else
                    h = 4 + (r - g) / delta;   // between magenta & cyan
                h *= 60;               // degrees
                if (h < 0)
                    h += 360;

                // Minimize s to the lowest value that still gives the same
                // result when converted back into a Color
                if (s > 0)
                {
                    // Experimentally confirmed with all 24 bit colors, so
                    // that's good enough for me
                    // Gets the saturation as if the max channel were one lower
                    float steppedDownMax = max - 1 / 255f;
                    float steppedDownS = (steppedDownMax - min) / max;
                    // Set the final saturation to slightly above the average
                    s = MathHelper.Lerp(steppedDownS, s, 0.501f);

#if DEBUG
                    Color testColor = HSVToColor(h, s, v);
                    testColor.A = color.A;
                    if (color != testColor)
                        throw new ArgumentException();
#endif
                }
            }
            else
            {
                // r = g = b = 0       // s = 0, h is undefined
                s = 0;
                h = -1;
            }
        }

        public static Color HSLToColor(float hue, float saturation, float lightness)
        {
            float r, g, b;
            float h = hue % 360;
            if (h < 0)
                h += 360;

            if (lightness <= 0)
                r = g = b = 0;
            else if (saturation <= 0)
                r = g = b = lightness;
            else
            {
                float chroma = (1 - Math.Abs(lightness * 2 - 1)) * saturation;
                float min = lightness - (chroma / 2);
                float max = min + chroma;

                float hf = hue / 60f;
                float x = chroma * (1 - Math.Abs(hf % 2 - 1));

                int i = (int)Math.Floor(hf);
                switch (i)
                {

                    // Red dominant: Red to yellow
                    case 0:
                    case 6:
                        r = max;
                        g = x + min;
                        b = min;
                        break;
                    // Green dominant: Yellow to green
                    case 1:
                        r = x + min;
                        g = max;
                        b = min;
                        break;
                    // Green dominant: Green to cyan
                    case 2:
                        r = min;
                        g = max;
                        b = x + min;
                        break;
                    // Blue dominant: Cyan to blue
                    case 3:
                        r = min;
                        g = x + min;
                        b = max;
                        break;
                    // Blue dominant: Blue to Magenta
                    case 4:
                        r = x + min;
                        g = min;
                        b = max;
                        break;
                    // Red dominant: Magenta to red
                    case 5:
                    case -1:
                        r = max;
                        g = min;
                        b = x + min;
                        break;
                    // Something has gone wrong
                    default:
                        r = g = b = lightness;
                        break;
                }
            }

            return new Color(r, g, b);
        }
        public static void ColorToHSL(Color color, out float hue, out float sat, out float lit)
        {
            hue = GetHue(color);
            sat = GetHSLSaturation(color);
            lit = GetLightness(color);
        }

        public static float GetHue(Color color)
        {
            float chroma = GetChroma(color);
            if (chroma == 0)
                return float.NaN;

            float value = GetValue(color);
            
            float hue;
            if (color.R == Math.Round(value * 255))
                hue = ((color.G - color.B) / 255f) / chroma;       // between yellow & magenta
            else if (color.G == Math.Round(value * 255))
                hue = 2 + ((color.B - color.R) / 255f) / chroma;   // between cyan & yellow
            else
                hue = 4 + ((color.R - color.G) / 255f) / chroma;   // between magenta & cyan

            hue *= 60;               // degrees
            if (hue < 0)
                hue += 360;
            return hue;
        }
        public static float GetHSLSaturation(Color color)
        {
            float L = GetLightness(color);
            if (L < 0.00001 || L > 0.99999)
                return 0;
            float C = GetChroma(color);
            return C / (1 - Math.Abs(2 * L - 1));
        }
        public static float GetChroma(Color color)
        {
            int M = Math.Max(Math.Max(color.R, color.G), color.B);
            int m = Math.Min(Math.Min(color.R, color.G), color.B);
            return (M - m) / 255f;
        }
        public static float GetValue(Color color)
        {
            return Math.Max(Math.Max(color.R, color.G), color.B) / 255f;
        }
        public static float GetLightness(Color color)
        {
            float max = GetValue(color);
            float min = Math.Min(Math.Min(color.R, color.G), color.B) / 255f;
            return (max + min) / 2f;
        }

        public static float GetLuma(Color color)
        {
            return ((color.R * 0.3f) + (color.G * 0.59f) + (color.B * 0.11f)) / 765f;
        }
    }
}
