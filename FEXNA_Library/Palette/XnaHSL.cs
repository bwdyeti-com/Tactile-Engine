using System;
using Microsoft.Xna.Framework;
using FEXNA_Library;

namespace FEXNA_Library.Palette
{
    public struct XnaHSL : IEquatable<XnaHSL>, ICloneable
    {
        public float Hue { get; private set; }
        public float Saturation { get; private set; }
        public float Lightness { get; private set; }
        public byte Alpha { get; private set; }

        public XnaHSL(float hue, float saturation, float lightness, byte alpha = 255)
        {
            hue = hue %= 360;
            if (hue < 0)
                hue += 360;
            Hue = MathHelper.Clamp(hue, 0f, 360f);
            Saturation = MathHelper.Clamp(saturation, 0f, 1f);
            Lightness = MathHelper.Clamp(lightness, 0f, 1f);
            Alpha = alpha;
        }
        public XnaHSL(Color color)
        {
            float hue, sat, lit;
            Color_Util.ColorToHSL(color, out hue, out sat, out lit);
            Hue = hue;
            Saturation = sat;
            Lightness = lit;
            Alpha = color.A;
        }
        private XnaHSL(XnaHSL source)
        {
            Hue = source.Hue;
            Saturation = source.Saturation;
            Lightness = source.Lightness;
            Alpha = source.Alpha;
        }

        public override string ToString()
        {
            return string.Format("{{H:{0}, S:{1}, L:{2}, A:{3}}}",
                (int)Hue, (int)(Saturation * 100), (int)(Lightness * 100), Alpha);
        }

        public XnaHSL SetHue(float value)
        {
            value = value %= 360;
            if (value < 0)
                value += 360;
            XnaHSL result = (XnaHSL)Clone();
            result.Hue = MathHelper.Clamp(value, 0f, 360f);
            return result;
        }
        public XnaHSL SetSaturation(float value)
        {
            XnaHSL result = (XnaHSL)Clone();
            result.Saturation = MathHelper.Clamp(value, 0f, 1f);
            return result;
        }
        public XnaHSL SetLightness(float value)
        {
            XnaHSL result = (XnaHSL)Clone();
            result.Lightness = MathHelper.Clamp(value, 0f, 1f);
            return result;
        }

        public XnaHSL LerpHue(float target, float amount)
        {
            return SetHue(LerpHue(Hue, target, amount));
        }
        private static float LerpHue(float value1, float value2, float amount)
        {
            // If the hues are closer across the blue-red boundary
            float diff = Math.Abs(value1 - value2);
            if (diff >= 180)
            {
                float result;
                if (value1 > value2)
                    result = MathHelper.Lerp(value1, value2 + 360, amount);
                else
                    result = MathHelper.Lerp(value1 + 360, value2, amount);

                if (result >= 360)
                    result -= 360;
                return result;
            }
            else
            {
                return MathHelper.Lerp(value1, value2, amount);
            }
        }

        public Color GetColor()
        {
            Color color = Color_Util.HSLToColor(Hue, Saturation, Lightness);
            color.A = Alpha;
            return color;
        }

        public static XnaHSL Lerp(XnaHSL value1, XnaHSL value2, float amount)
        {
            var result = (XnaHSL)value1.Clone();
            result.Hue = LerpHue(value1.Hue, value2.Hue, amount);
            result.Saturation = MathHelper.Lerp(value1.Saturation, value2.Saturation, amount);
            result.Lightness = MathHelper.Lerp(value1.Lightness, value2.Lightness, amount);
            result.Alpha = (byte)Math.Round(MathHelper.Lerp(value1.Alpha, value2.Alpha, amount));
            return result;
        }

        public Vector3 BiconePosition()
        {
            // Right is red, backward is yellow-green, up is white, down is black

            float z = Lightness * 2 - 1;
            // Set radius
            float radius = MathHelper.Lerp(Saturation, 0f, z);
            // Rotate using hue
            Vector3 chroma = Vector3.Zero;
            if (radius > 0)
            {
                Matrix rotation = Matrix.CreateFromAxisAngle(Vector3.Up, -MathHelper.TwoPi * Hue / 360f);
                chroma = Vector3.Transform(Vector3.Right * radius, rotation);
            }
            // Set height from lightness
            Vector3 loc = chroma + Vector3.Up * z;

            return loc;
        }

        public Vector3 BiconeDifference(XnaHSL other)
        {
            return BiconePosition() - other.BiconePosition();
        }
        public static Vector3 BiconeDifference(Color source, Color target)
        {
            return new XnaHSL(target).BiconePosition() - new XnaHSL(source).BiconePosition();
        }

        #region IEquatable<XnaHSL>
        public bool Equals(XnaHSL other)
        {
            return Hue == other.Hue &&
                Saturation == other.Saturation &&
                Lightness == other.Lightness &&
                Alpha == other.Alpha;
        }

        public static bool operator ==(XnaHSL a, XnaHSL b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(XnaHSL a, XnaHSL b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is XnaHSL))
                return false;
            return Equals((XnaHSL)obj);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 37;

                hash = hash * 53 + Hue.GetHashCode();
                hash = hash * 53 + Saturation.GetHashCode();
                hash = hash * 53 + Lightness.GetHashCode();
                hash = hash * 53 + Alpha;

                return hash;
            }
        }
        #endregion

        #region ICloneable
        public object Clone()
        {
            return new XnaHSL(this);
        }
        #endregion
    }
}
