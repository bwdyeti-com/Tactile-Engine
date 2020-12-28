using System;
using System.IO;

namespace TactileLibrary
{
    /// <summary>
    /// Represents a pseudo-random number generator, a device that produces a sequence
    /// of numbers that meet certain statistical requirements for randomness.
    /// Uses a noise function to generate random numbers.
    /// </summary>
    public class NoiseRand : ICloneable
    {
        private uint Seed;
        private uint Index = 0;

        #region IO
        /// <summary>
        /// Writes this object to stream a using a <see cref="BinaryWriter"/>.
        /// </summary>
        public void Write(BinaryWriter writer)
        {
            writer.Write(Seed);
            writer.Write(Index);
        }

        /// <summary>
        /// Reads into this object from a stream using a
        /// <see cref="BinaryReader"/>.
        /// </summary>
        public void Read(BinaryReader reader)
        {
            Seed = reader.ReadUInt32();
            Index = reader.ReadUInt32();
        }

        /// <summary>
        /// Creates a new object using data from a stream using a
        /// <see cref="BinaryReader"/>.
        /// </summary>
        public static NoiseRand ReadObject(BinaryReader reader)
        {
            NoiseRand result = new NoiseRand();
            result.Read(reader);
            return result;
        }
        #endregion

        /// <summary>
        /// Creates a new noise based random number generator.
        /// </summary>
        public NoiseRand(uint seed)
        {
            Seed = seed;
        }
        /// <summary>
        /// Creates a new noise based random number generator.
        /// Uses current DateTime for the seed.
        /// </summary>
        public NoiseRand()
        {
            DateTime now = DateTime.UtcNow;
            Seed = (uint)now.Ticks;
        }
        /// <summary>
        /// Creates a new noise based random number generator.
        /// </summary>
        private NoiseRand(NoiseRand source) : this()
        {
            Seed = source.Seed;
            Index = source.Index;
        }

        public override string ToString()
        {
            return string.Format("NoiseRand: Seed {0}, Index {1}", Seed, Index);
        }

        private uint _NextRand()
        {
            uint result = SquirrelNoise.Noise((int)Index, Seed);
            Index++;
            return result;
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>A 32-bit signed integer greater than or equal to zero and less than <see cref="System.Int32.MaxValue"/>.</returns>
        public int Next()
        {
            return _Next();
        }
        private int _Next()
        {
            return (int)(_NextRand() >> 1);
        }
        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must
        /// be greater than or equal to zero.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero, and less than maxValue;
        /// that is, the range of return values ordinarily includes zero but not maxValue.
        /// However, if maxValue equals zero, maxValue is returned.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">maxValue is less than zero.</exception>
        public int Next(int maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException("maxValue is less than zero.");

            return _Next((uint)maxValue);
        }
        private int _Next(uint maxValue)
        {
            if (maxValue == 0)
                return 0;

            // If the maxValue is sufficiently large, use long instead of int
            if (maxValue > 1 << 24)
            {
                long result = (((long)_NextRand()) << 32) +_NextRand();
                return (int)(result % maxValue);
            }
            else
            {
                return (int)(_NextRand() % maxValue);
            }
        }
        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater
        /// than or equal to minValue.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to minValue and less than maxValue;
        /// that is, the range of return values includes minValue but not maxValue. If minValue
        /// equals maxValue, minValue is returned.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">minValue is greater than maxValue.</exception>
        public int Next(int minValue, int maxValue)
        {
            if (maxValue < minValue)
                throw new ArgumentOutOfRangeException("maxValue is less than zero.");

            return _Next(minValue, maxValue);
        }
        private int _Next(int minValue, int maxValue)
        {
            if (minValue == maxValue)
                return maxValue;

            // Combine min and max value to get the range
            int result = _Next((uint)((long)maxValue - (long)minValue));
            // Adjust result space using min value
            return result + minValue;
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        /// <exception cref="System.ArgumentNullException">buffer is null.</exception>
        public void Next(int[] buffer)
        {
            _Next(buffer);
        }
        private void _Next(int[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer is null");

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(_NextRand() % (byte.MaxValue + 1));
            }
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number greater than or equal to 0.0, and less
        /// than 1.0.</returns>
        public double NextDouble()
        {
            return _NextDouble();
        }
        private double _NextDouble()
        {
            long result = _NextRand();
            return result / (double)(uint.MaxValue + 1L);
        }

        #region ICloneable
        public object Clone()
        {
            return new NoiseRand(this);
        }
        #endregion
    }
}
