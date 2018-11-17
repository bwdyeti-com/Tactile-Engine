using System;
using System.IO;
using System.Reflection;
using ArrayExtension;

namespace FEXNA
{
    class Rng
    {
        Random Rand;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            Type type = typeof(Random);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            //Int32 inext = (Int32)type.GetField("inext", flags).GetValue(Rand);
            //Int32 inextp = (Int32)type.GetField("inextp", flags).GetValue(Rand);
            //int[] arr = (int[])type.GetField("SeedArray", flags).GetValue(Rand);
            writer.Write((int)type.GetField("inext", flags).GetValue(Rand));
            writer.Write((int)type.GetField("inextp", flags).GetValue(Rand));
            ((int[])type.GetField("SeedArray", flags).GetValue(Rand)).write(writer);
        }

        public static Rng read(BinaryReader reader)
        {
            Rng result = new Rng();
            Type type = typeof(Random);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;

            int[] ary = new int[0];
            type.GetField("inext", flags).SetValue(result.Rand, reader.ReadInt32());
            type.GetField("inextp", flags).SetValue(result.Rand, reader.ReadInt32());
            type.GetField("SeedArray", flags).SetValue(result.Rand, ary.read(reader));
            return result;
        }
        #endregion

        public Rng()
        {
            Rand = new Random();
        }

        public int get_rng(int max)
        {
            return Rand.Next(max);
        }

#if DEBUG
        internal Rng copy_generator()
        {
            Rng result = new Rng();
            Type type = typeof(Random);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;

            int[] ary = new int[0];
            type.GetField("inext", flags).SetValue(result.Rand,
                (int)type.GetField("inext", flags).GetValue(Rand));
            type.GetField("inextp", flags).SetValue(result.Rand,
                (int)type.GetField("inextp", flags).GetValue(Rand));
            int[] base_seed_array =
                ((int[])type.GetField("SeedArray", flags).GetValue(Rand));
            int[] new_seed_array = new int[base_seed_array.Length];
            Array.Copy(base_seed_array, new_seed_array, base_seed_array.Length);
            type.GetField("SeedArray", flags).SetValue(result.Rand, new_seed_array);

            return result;
        }
#endif
    }
}
