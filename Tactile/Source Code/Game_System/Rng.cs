using System;
using System.IO;
using System.Reflection;
using ArrayExtension;
using TactileLibrary;
using TactileVersionExtension;

namespace Tactile
{
    class Rng
    {
        NoiseRand Rand;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            Rand.Write(writer);
        }

        public static Rng read(BinaryReader reader, Version loadedVersion)
        {
            Rng result = new Rng();

            if (loadedVersion.older_than(0, 6, 11, 0))
            {
                int inext = reader.ReadInt32();
                int inextp = reader.ReadInt32();
                int[] seedArray = new int[0].read(reader);

                // Just seed the rng with whatever
                result.Rand = new NoiseRand();
            }
            else
            {
                result.Rand.Read(reader);
            }

            return result;
        }
        #endregion

        public Rng()
        {
            Rand = new NoiseRand();
        }

        public int get_rng(int max)
        {
            return Rand.Next(max);
        }

#if DEBUG
        internal Rng copy_generator()
        {
            Rng result = new Rng();
            result.Rand = (NoiseRand)Rand.Clone();
            return result;
        }
#endif
    }
}
