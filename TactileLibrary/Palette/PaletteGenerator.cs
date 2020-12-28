using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TactileLibrary.Palette
{
    public class PaletteGenerator : TactileDataContent
    {
        private float _Specularity = 0.5f;
        private float _BaseLightness = 0.5f;
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
        public float BaseLightness
        {
            get { return _BaseLightness; }
            set
            {
                var range = new Range<float>(0f, 1f);
                _BaseLightness = MathHelper.Clamp(value, range.Minimum, range.Maximum);
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

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static PaletteGenerator GetEmptyInstance()
        {
            return new PaletteGenerator();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var source = (PaletteGenerator)other;

            _Specularity = source._Specularity;
            _BaseLightness = source._BaseLightness;
            _ShadowAmount = source._ShadowAmount;
            _BlackLevel = source._BlackLevel;
        }

        public static PaletteGenerator ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            _Specularity = input.ReadSingle();
            _BaseLightness = input.ReadSingle();
            _ShadowAmount = input.ReadSingle();
            _BlackLevel = input.ReadSingle();
        }

        public override void Write(BinaryWriter output)
        {
            output.Write(_Specularity);
            output.Write(_BaseLightness);
            output.Write(_ShadowAmount);
            output.Write(_BlackLevel);
        }
        #endregion

        public PaletteGenerator() { }
        private PaletteGenerator(PaletteGenerator source)
        {
            CopyFrom(source);
        }

        public static Range<float> SpecularityRange()
        {
            return new Range<float>(0.25f, 1f);
            //return new Range<float>(0f, 1f);
        }
        public static Range<float> ShadowRange()
        {
            return new Range<float>(0.25f, 0.75f);
            //return new Range<float>(0f, 1f);
        }
        public static Range<float> BlackRange()
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
            AdjustValue(color, new PaletteParameters(baseColor), value);
        }
        public void AdjustValue(Color color, PaletteParameters parameters, float value)
        {
            var palette = GetPalette(parameters);

            if (palette.DarkerThanBase(color))
            {
                // Adjust shadow
                AdjustShadowAmount(color, palette, value);
            }
            else
            {
                // Adjust specular
                AdjustSpecularity(color, parameters, value);
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
            // The percent of the way between 0 and the BASE_VALUE
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
        private void AdjustSpecularity(Color color, PaletteParameters parameters, float rawAmount)
        {
            // Find what this color's value looks like with minimum and maximum
            // specularity, and using where it should be inverse lerp into that
            // range
            var range = SpecularityRange();

            var darkGenerator = (PaletteGenerator)Clone();
            darkGenerator.Specularity = range.Minimum;
            var darkPalette = darkGenerator.GetPalette(parameters);

            var lightGenerator = (PaletteGenerator)Clone();
            lightGenerator.Specularity = range.Maximum;
            var lightPalette = lightGenerator.GetPalette(parameters);

            // Dark Base Value will be a larger number, because a dark color
            // will be closer to the specular
            float darkBaseValue = darkPalette.GetValue(color);
            float lightBaseValue = lightPalette.GetValue(color);

            // I don't know how, but
            // Avoid divide by 0
            if (lightBaseValue == darkBaseValue)
            {
                this.Specularity = 1f;
                return;
            }

            float value = (rawAmount - lightBaseValue) / (darkBaseValue - lightBaseValue);
            this.Specularity = MathHelper.Lerp(range.Maximum, range.Minimum, value);
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
            const int iterations = 3;

            List<Tuple<float, float>> errors = null;
            int index = -1;
            for (int i = 0; i < iterations; i++)
            {
                errors = Errors(parameters, ramp, range, count, function);
                float min = errors.Min(x => x.Item2);
                index = errors.FindIndex(x => x.Item2 == min);

                // Get the indices above and below the minimum value
                int step = Math.Max(1, (int)(count * 0.3f) / 2);
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
            PaletteParameters copyParameters = (PaletteParameters)parameters.Clone();
            float interval = (range.Maximum - range.Minimum) / count;

            var errors = new List<Tuple<float, float>>();

            for (int i = 0; i <= count; i++)
            {
                float value = range.Minimum + interval * i;
                function(copyGenerator, value);

                // Kind of necessary for user facing values
                // Minimize error on blue shadow
                copyParameters.MinimizeBlueError(copyGenerator, ramp);
                // Minimize error on yellow light
                copyParameters.MinimizeYellowError(copyGenerator, ramp);

                var palette = copyGenerator.GetPalette(copyParameters);
                float error = palette.GetError(ramp);
                errors.Add(Tuple.Create(value, error));
            }

            return errors;
        }

        #region ICloneable
        public override object Clone()
        {
            return new PaletteGenerator(this);
        }
        #endregion
    }
}
