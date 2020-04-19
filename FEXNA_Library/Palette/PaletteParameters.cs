using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ColorExtension;

namespace FEXNA_Library.Palette
{
    public class PaletteParameters : IFEXNADataContent
    {
        private float _YellowLight = 0f;
        private float _BlueShadow = 0f;
        private float _OriginalLightness = 0.5f;

        public Color BaseColor;
        public float YellowLight
        {
            get { return _YellowLight; }
            set
            {
                var range = YellowRange();
                _YellowLight = MathHelper.Clamp(value, range.Minimum, range.Maximum);
            }
        }
        public float BlueShadow
        {
            get { return _BlueShadow; }
            set
            {
                var range = BlueRange();
                _BlueShadow = MathHelper.Clamp(value, range.Minimum, range.Maximum);
            }
        }
        public float OriginalLightness
        {
            get { return _OriginalLightness; }
            set
            {
                var range = new Range<float>(0f, 1f);
                _OriginalLightness = MathHelper.Clamp(value, range.Minimum, range.Maximum);
            }
        }
        public bool ReducedDepth = false;

        #region Serialization
        public IFEXNADataContent Read_Content(ContentReader input)
        {
            PaletteParameters result = new PaletteParameters();

            result.BaseColor = result.BaseColor.read(input);
            result._YellowLight = input.ReadSingle();
            result._BlueShadow = input.ReadSingle();
            result._OriginalLightness = input.ReadSingle();
            result.ReducedDepth = input.ReadBoolean();

            return result;
        }

        public void Write(BinaryWriter output)
        {
            BaseColor.write(output);
            output.Write(_YellowLight);
            output.Write(_BlueShadow);
            output.Write(_OriginalLightness);
            output.Write(ReducedDepth);
        }
        #endregion

        private PaletteParameters() { }
        public PaletteParameters(Color baseColor)
        {
            BaseColor = baseColor;
        }
        private PaletteParameters(PaletteParameters source)
        {
            BaseColor = source.BaseColor;
            YellowLight = source.YellowLight;
            BlueShadow = source.BlueShadow;
            OriginalLightness = source.OriginalLightness;
            ReducedDepth = source.ReducedDepth;
        }

        private static Range<float> YellowRange()
        {
            return new Range<float>(0f, 1f);
        }
        private static Range<float> BlueRange()
        {
            return new Range<float>(0f, 01f);
        }

        internal void MinimizeYellowError(PaletteGenerator generator, List<PaletteEntry> ramp)
        {
            var range = YellowRange();
            Func<PaletteParameters, float, float> function =
                (PaletteParameters parameters, float value) => parameters.YellowLight = value;

            float minimum = MinimizeError(generator, ramp, range, function);
            YellowLight = minimum;
        }
        internal void MinimizeBlueError(PaletteGenerator generator, List<PaletteEntry> ramp)
        {
            var range = BlueRange();
            Func<PaletteParameters, float, float> function =
                (PaletteParameters parameters, float value) => parameters.BlueShadow = value;

            float minimum = MinimizeError(generator, ramp, range, function);
            BlueShadow = minimum;
        }

        private float MinimizeError(
            PaletteGenerator generator,
            List<PaletteEntry> ramp,
            Range<float> range,
            Func<PaletteParameters, float, float> function)
        {
            const int count = 20;
            const int iterations = 10;

            List<Tuple<float, float>> errors = null;
            int index = -1;
            for (int i = 0; i < iterations; i++)
            {
                errors = Errors(generator, ramp, range, count, function);
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
            PaletteGenerator generator,
            List<PaletteEntry> ramp,
            Range<float> range,
            int count,
            Func<PaletteParameters, float, float> function)
        {
            PaletteParameters copyParameters = (PaletteParameters)Clone();
            float interval = (range.Maximum - range.Minimum) / count;

            var errors = new List<Tuple<float, float>>();

            for (int i = 0; i <= count; i++)
            {
                float value = range.Minimum + interval * i;
                function(copyParameters, value);

                var palette = generator.GetPalette(copyParameters);
                float error = palette.GetError(ramp);
                errors.Add(Tuple.Create(value, error));
            }

            return errors;
        }

        #region ICloneable
        public object Clone()
        {
            return new PaletteParameters(this);
        }
        #endregion
    }
}
