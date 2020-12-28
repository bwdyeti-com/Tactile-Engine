namespace TactileLibrary
{
    public class Noise
    {
        internal int Seed { get; private set; }
        public Noise(int seed)
        {
            Seed = seed;
        }

        public double noise(int x)
        {
            return IntNoise(x, Seed);
        }
        public double noise(int x, int y)
        {
            return IntNoise(x, y, Seed);
        }
        public double noise(int x, int y, int z)
        {
            return IntNoise(x, y, z, Seed);
        }

        protected static double IntNoise(params int[] parameters)
        {
            double result = IntNoise(parameters[0]);
            if (parameters.Length > 0)
            {
                for (int i = 1; i < parameters.Length; i++)
                {
                    int value = (int)((result + 1) / 2 * int.MaxValue) ^ parameters[i];
                    result = IntNoise(value);
                }
            }
            return result;
        }
        protected static double IntNoise(int x)
        {
            return SquirrelNoise.DoubleNoise(x);
        }
    }
}
