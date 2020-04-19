using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace FEXNA_Library.Palette
{
    public struct ColorVector : IFEXNADataContent
    {
        [ContentSerializer(ElementName = "H")]
        public float Hue { get; private set; }
        [ContentSerializer(ElementName = "S")]
        public float Saturation { get; private set; }
        [ContentSerializer(ElementName = "L")]
        public float Lightness { get; private set; }
        [ContentSerializer(ElementName = "ToYellow")]
        public bool BlueYellowRamp { get; private set; }

        #region Serialization
        public IFEXNADataContent Read_Content(ContentReader input)
        {
            float hue = input.ReadSingle();
            float saturation = input.ReadSingle();
            float lightness = input.ReadSingle();
            bool blueYellowRamp = input.ReadBoolean();

            ColorVector result = new ColorVector(hue, saturation, lightness, blueYellowRamp);

            return result;
        }

        public void Write(BinaryWriter output)
        {
            output.Write(Hue);
            output.Write(Saturation);
            output.Write(Lightness);
            output.Write(BlueYellowRamp);
        }
        #endregion

        public ColorVector(float hue, float sat, float lit, bool blueYellowRamp = false)
        {
            Hue = MathHelper.Clamp(hue, -360f, 360f);
            Saturation = MathHelper.Clamp(sat, -1f, 1f);
            Lightness = MathHelper.Clamp(lit, -1f, 1f);
            BlueYellowRamp = blueYellowRamp;
        }
        private ColorVector(ColorVector source)
        {
            Hue = source.Hue;
            Saturation = source.Saturation;
            Lightness = source.Lightness;
            BlueYellowRamp = source.BlueYellowRamp;
        }

        public override string ToString()
        {
            return string.Format("{{H:{0:+#;-#;0}, S:{1:+#;-#;0}, L:{2:+#;-#;0}, {3}}}",
                (int)Hue, (int)(Saturation * 100), (int)(Lightness * 100),
                BlueYellowRamp ? "Light Ramp" : "Literal Hue");
        }

        public static ColorVector ColorDifference(Color source, Color target, bool blueYellowRamp = false)
        {
            XnaHSL sourceHsl = new XnaHSL(source);
            XnaHSL targetHsl = new XnaHSL(target);

            float hue;
            if (float.IsNaN(sourceHsl.Hue) || float.IsNaN(targetHsl.Hue))
                hue = 0;
            else
            {
                if (blueYellowRamp)
                {
                    hue = GetBlueYellowHue(targetHsl.Hue) -
                        GetBlueYellowHue(sourceHsl.Hue);
                }
                else
                {
                    hue = targetHsl.Hue - sourceHsl.Hue;
                    hue %= 360;
                    if (hue <= -180)
                        hue += 360;
                    else if (hue > 180)
                        hue -= 360;
                }
            }
            float sat = targetHsl.Saturation - sourceHsl.Saturation;
            float lit = targetHsl.Lightness - sourceHsl.Lightness;

            return new ColorVector(hue, sat, lit, blueYellowRamp);
        }

        private static float GetBlueYellowHue(float hue)
        {
            // Gets the position along the blue-yellow ramp,
            // regardless of whether the color is on the red-magenta half of
            // the color wheel or the green-cyan half
            const float yellow = 60;
            const float blue = 240;

            hue %= 360;
            if (hue < 0)
                hue += 360;

            // If on the red-magenta spectrum
            // If red-yellow
            if (hue < yellow)
                hue = yellow + yellow - hue;
            // If blue-red
            if (hue > blue)
                hue = blue + blue - hue;

            return blue - hue;
        }
        private static float ApplyBlueYellowHue(float hue, float adjustment)
        {
            const float yellow = 60;
            const float blue = 240;

            if (hue < yellow)
            {
                hue += adjustment;
                hue = MathHelper.Clamp(hue, yellow - 180, yellow);
            }
            else if (hue > blue)
            {
                hue += adjustment;
                hue = MathHelper.Clamp(hue, blue, blue + 180);
            }
            else
            {
                hue -= adjustment;
                hue = MathHelper.Clamp(hue, yellow, blue);
            }

            hue %= 360;
            if (hue < 0)
                hue += 360;

            return hue;
        }

        public ColorVector SetHue(float value)
        {
            ColorVector result = (ColorVector)Clone();
            result.Hue = MathHelper.Clamp(value, -360f, 360f);
            return result;
        }
        public ColorVector SetSaturation(float value)
        {
            ColorVector result = (ColorVector)Clone();
            result.Saturation = MathHelper.Clamp(value, -1f, 1f);
            return result;
        }
        public ColorVector SetLightness(float value)
        {
            ColorVector result = (ColorVector)Clone();
            result.Lightness = MathHelper.Clamp(value, -1f, 1f);
            return result;
        }
        
        public static Color operator +(Color color, ColorVector vector)
        {
            XnaHSL hsl = new XnaHSL(color) + vector;
            return hsl.GetColor();
        }
        public static XnaHSL operator +(XnaHSL hsl, ColorVector vector)
        {
            if (vector.BlueYellowRamp)
            {
                hsl = hsl.SetHue(ApplyBlueYellowHue(hsl.Hue, vector.Hue));
            }
            else
                hsl = hsl.SetHue(hsl.Hue + vector.Hue);
            hsl = hsl.SetSaturation(hsl.Saturation + vector.Saturation);
            hsl = hsl.SetLightness(hsl.Lightness + vector.Lightness);
            return hsl;
        }

        #region ICloneable
        public object Clone()
        {
            return new ColorVector(this);
        }
        #endregion
    }
}
