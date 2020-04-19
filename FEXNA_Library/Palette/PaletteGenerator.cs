using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using FEXNA_Library;

namespace FEXNA_Library.Palette
{
    public class PaletteGenerator : ICloneable
    {
        private float _Specularity = 0.5f;
        private float _ShadowAmount = 0.5f;
        private float _BlackLevel = 0f;

        public float Specularity
        {
            get { return _Specularity; }
            set
            {
                var range = SpecularityRange();
                _Specularity = MathHelper.Clamp(value, range.Minimum, range.Maximum);
            }
        }
        public float ShadowAmount
        {
            get { return _ShadowAmount; }
            set
            {
                var range = ShadowRange();
                _ShadowAmount = MathHelper.Clamp(value, range.Minimum, range.Maximum);
            }
        }
        public float BlackLevel
        {
            get { return _BlackLevel; }
            set
            {
                var range = BlackRange();
                _BlackLevel = MathHelper.Clamp(value, range.Minimum, range.Maximum);
            }
        }

        public PaletteGenerator() { }
        private PaletteGenerator(PaletteGenerator source)
        {
            _Specularity = source._Specularity;
            _ShadowAmount = source._ShadowAmount;
            _BlackLevel = source._BlackLevel;
        }

        private static Range<float> SpecularityRange()
        {
            return new Range<float>(0.25f, 1f);
            //return new Range<float>(0f, 1f);
        }
        private static Range<float> ShadowRange()
        {
            return new Range<float>(0.25f, 0.75f);
            //return new Range<float>(0f, 1f);
        }
        private static Range<float> BlackRange()
        {
            return new Range<float>(0f, 0.5f);
            //return new Range<float>(0f, 1f);
        }

        public GeneratedColorPalette GetPalette(PaletteParameters parameters)
        {
            return new GeneratedColorPalette(this, parameters);
        }

        public void AdjustValue(Color color, Color baseColor, float value)
        {
            var parameters = new PaletteParameters(baseColor);
            var palette = GetPalette(parameters);

            if (palette.DarkerThanBase(color))
            {
                // Adjust shadow
                AdjustShadowAmount(color, palette, value);
            }
            else
            {
                // Adjust specular
                AdjustSpecularity(color, palette, value);
            }
        }

        internal static Color BaseSpecularColor(Color baseColor)
        {
            float h, s, v;
            Color_Util.ColorToHSV(baseColor, out h, out s, out v);
            Color fullValue = Color_Util.HSVToColor(h, s, 1f);
            fullValue.A = baseColor.A;
            return Color.Lerp(baseColor, fullValue, 0.5f);
        }

        private void AdjustShadowAmount(Color color, GeneratedColorPalette palette, float rawAmount)
        {
            float amount = rawAmount / GeneratedColorPalette.BASE_VALUE;
            // Avoid divide by 0
            if (amount <= 0)
            {
                this.ShadowAmount = 1f;
                return;
            }
            
            float darkestValue = GeneratedColorPalette.ValueFormula(palette.Darkest);
            float baseValue = GeneratedColorPalette.ValueFormula(palette.BaseColor);
            // Avoid divide by 0
            if (baseValue - darkestValue == 0)
            {
                this.ShadowAmount = 0.5f;
                return;
            }
            float colorValue = GeneratedColorPalette.ValueFormula(color);
            float percent = (colorValue - darkestValue) / (baseValue - darkestValue);
            
            if (amount < 0.5f)
            {
                float midPointValue = (percent / amount) / 2f;
                this.ShadowAmount = midPointValue;
            }
            else
            {
                // Avoid divide by 0
                if (amount >= 1f)
                    this.ShadowAmount = 0;
                else
                {
                    float midPointValue = MathHelper.Lerp(
                        1f, percent,
                        1 / (1 - (2f * amount - 1f)));
                    this.ShadowAmount = midPointValue;
                }
            }
        }
        private void AdjustSpecularity(Color color, GeneratedColorPalette palette, float rawAmount)
        {
            float amount =
                (rawAmount - GeneratedColorPalette.BASE_VALUE) /
                (1f - GeneratedColorPalette.BASE_VALUE);
            // Avoid divide by 0
            if (amount <= 0)
            {
                this.Specularity = 1f;
                return;
            }
            
            Color baseSpecular = BaseSpecularColor(palette.BaseColor);
            float baseSpecularValue = GeneratedColorPalette.ValueFormula(baseSpecular);
            // Avoid divide by 0
            if (baseSpecularValue >= 1f)
            {
                this.Specularity = 0.5f;
                return;
            }

            float baseValue = GeneratedColorPalette.ValueFormula(palette.BaseColor);
            float colorValue = GeneratedColorPalette.ValueFormula(color);
            float targetSpecularValue = MathHelper.Lerp(baseValue, colorValue, 1f / amount);

            this.Specularity = (targetSpecularValue - baseSpecularValue) / (1f - baseSpecularValue);
        }

        internal void MinimizeSpecularityError(PaletteParameters parameters, List<PaletteEntry> ramp)
        {
            var range = SpecularityRange();
            Func<PaletteGenerator, float, float> function =
                (PaletteGenerator generator, float value) => generator.Specularity = value;

            float minimum = MinimizeError(parameters, ramp, range, function);
            Specularity = minimum;
        }
        internal void MinimizeShadowError(PaletteParameters parameters, List<PaletteEntry> ramp)
        {
            var range = ShadowRange();
            Func<PaletteGenerator, float, float> function =
                (PaletteGenerator generator, float value) => generator.ShadowAmount = value;

            float minimum = MinimizeError(parameters, ramp, range, function);
            ShadowAmount = minimum;
        }

        private float MinimizeError(
            PaletteParameters parameters,
            List<PaletteEntry> ramp,
            Range<float> range,
            Func<PaletteGenerator, float, float> function)
        {
            const int count = 20;
            const int iterations = 10;

            List<Tuple<float, float>> errors = null;
            int index = -1;
            for (int i = 0; i < iterations; i++)
            {
                errors = Errors(parameters, ramp, range, count, function);
                float min = errors.Min(x => x.Item2);
                index = errors.FindIndex(x => x.Item2 == min);

                // Get the indices above and below the minimum value
                int step = Math.Max(1, (int)(count * 0.75f) / 2);
                int startIndex = Math.Max(0, index - step);
                startIndex = Math.Min(count - step * 2, startIndex);
                int endIndex = startIndex + step * 2;
                range = new Range<float>(errors[startIndex].Item1, errors[endIndex].Item1);
            }
            return errors[index].Item1;
        }

        private List<Tuple<float, float>> Errors(
            PaletteParameters parameters,
            List<PaletteEntry> ramp,
            Range<float> range,
            int count,
            Func<PaletteGenerator, float, float> function)
        {
            PaletteGenerator copyGenerator = (PaletteGenerator)Clone();
            float interval = (range.Maximum - range.Minimum) / count;

            var errors = new List<Tuple<float, float>>();

            for (int i = 0; i <= count; i++)
            {
                float value = range.Minimum + interval * i;
                function(copyGenerator, value);

                var palette = copyGenerator.GetPalette(parameters);
                float error = palette.GetError(ramp);
                errors.Add(Tuple.Create(value, error));
            }

            return errors;
        }

        #region ICloneable
        public object Clone()
        {
            return new PaletteGenerator(this);
        }
        #endregion
    }
}
