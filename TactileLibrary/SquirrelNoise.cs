//-----------------------------------------------------------------------------------------------
// Returns an unsigned integer containing 32 reasonably-well-scrambled bits, based on a given
//  (signed) integer input parameter (position/index) and [optional] seed.  Kind of like looking
//  up a value in an infinitely large [non-existant] table of previously generated random numbers.
//
// The base bit-noise constants were crafted to have distinctive and interesting bits,
//  and have so far produced excellent experimental test results.
//
// I call this particular approach SquirrelNoise, specifically SquirrelNoise3 (version 3).
//
//-----------------------------------------------------------------------------------------------
// Original C++ code from a GDC talk by Squirrel Eiserloh

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TactileLibrary
{
    class SquirrelNoise
    {
        const uint BIT_NOISE1 = 0x68E31DA4; // 0b0110_1000_1110_0011_0001_1101_1010_0100;
        const uint BIT_NOISE2 = 0xB5297A4D; // 0b1011_0101_0010_1001_0111_1010_0100_1101;
        const uint BIT_NOISE3 = 0x1B56C4E9; // 0b0001_1011_0101_0110_1100_0100_1110_1001;

        /// <summary>
        /// Returns a value in [0,uint.MaxValue]
        /// </summary>
        public static uint Noise(int position, uint seed = 0)
        {
            return _Noise(position, seed);
        }
        private static uint _Noise(int position, uint seed)
        {
            uint mangledBits = (uint)position;

            mangledBits *= BIT_NOISE1;
            mangledBits += seed;
            mangledBits ^= (mangledBits >> 8);

            mangledBits += BIT_NOISE2;
            mangledBits ^= (mangledBits << 8);

            mangledBits *= BIT_NOISE3;
            mangledBits ^= (mangledBits >> 8);

            return mangledBits;
        }

        public static uint Noise2D(int posX, int posY, uint seed = 0)
        {
            return _Noise2D(posX, posY, seed);
        }
        private static uint _Noise2D(int posX, int posY, uint seed)
        {
            const int PRIME_NUMBER = 198491317; // Large prime number with non-boring bits
            return Noise(posX + (PRIME_NUMBER * posY), seed);
        }

        public static uint Noise3D(int posX, int posY, int posZ, uint seed = 0)
        {
            return _Noise3D(posX, posY, posZ, seed);
        }
        private static uint _Noise3D(int posX, int posY, int posZ, uint seed)
        {
            // The large prime numbers should be orders of magnitude different
            const int PRIME1 = 198491317; // Large prime number with non-boring bits
            const int PRIME2 = 6542989; // Large prime number with distinct and non-boring bits
            return Noise(posX + (PRIME1 * posY) + (PRIME2 * posZ), seed);
        }

        public static uint Noise4D(int posX, int posY, int posZ, int posT, uint seed = 0)
        {
            return _Noise4D(posX, posY, posZ, posT, seed);
        }
        private static uint _Noise4D(int posX, int posY, int posZ, int posT, uint seed)
        {
            // The large prime numbers should be orders of magnitude different
            const int PRIME1 = 198491317; // Large prime number with non-boring bits
            const int PRIME2 = 6542989; // Large prime number with distinct and non-boring bits
            const int PRIME3 = 357239; // Large prime number with distinct and non-boring bits
            return Noise(posX + (PRIME1 * posY) + (PRIME2 * posZ) + (PRIME3 * posT), seed);
        }

        /// <summary>
        /// Returns a value in [-1.0,1.0]
        /// </summary>
        public static double DoubleNoise(int position, uint seed = 0)
        {
            return MapUIntToDouble(Noise(position, seed));
        }

        /// <summary>
        /// Maps a uint from [0,uint.MaxValue] to a double in [-1.0,1.0]
        /// </summary>
        public static double MapUIntToDouble(uint value)
        {
            // Maps from uint to [-1.0,1.0]
            double percent = (value / (double)uint.MaxValue);
            return (percent * 2) - 1;
        }
    }
}
