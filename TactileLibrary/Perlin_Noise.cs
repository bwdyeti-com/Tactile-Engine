using System;

namespace TactileLibrary
{
    public enum Perlin_Smooth_Mode { Normal, Smooth }
    public class Perlin_Noise
    {
        const int OCTAVES = 6;

        protected Noise Noise_Gen;
        float Scale;
        protected Perlin_Smooth_Mode Mode = Perlin_Smooth_Mode.Normal;
        protected float Persistence = 0.5f;
        public Perlin_Noise(int seed, float scale)
        {
            Noise_Gen = new Noise(seed);
            Scale = scale;
        }

        public override string ToString()
        {
            return string.Format("Perlin Noise: seed({0})", Noise_Gen.Seed);
        }

        public double noise(int x)
        {
            return Noise_Gen.noise(x);
        }
        public double noise(int x, int y)
        {
            return Noise_Gen.noise(x, y);
        }
        public double noise(int x, int y, int z)
        {
            return Noise_Gen.noise(x, y, z);
        }

        public static double lerp(double a, double b, double x)
        {
            return a * (1 - x) + b * x;
        }

        protected double smooth_noise(int x)
        {
            return noise(x - 1) / 4 + noise(x) / 2 + noise(x + 1) / 4;
        }
        protected double smooth_noise(int x, int y)
        {
            return noise(x, y) / 4 +
                (noise(x, y - 1) + noise(x - 1, y) + noise(x + 1, y) + noise(x, y + 1)) / 8 +
                (noise(x - 1, y - 1) + noise(x + 1, y - 1) + noise(x - 1, y + 1) + noise(x + 1, y + 1)) / 16;
        }

        protected double noise(float x)
        {
            int xint = (int)x;
            x = x - xint;

            double v1 = smooth_noise(xint);
            double v2 = smooth_noise(xint + 1);

            return lerp(v1, v2, x);
        }
        protected double noise(float x, float y)
        {
            int xint = (int)x;
            x = x - xint;
            int yint = (int)y;
            y = y - yint;

            double v1 = 0, v2 = 0, v3 = 0, v4 = 0;
            switch(Mode)
            {
                case Perlin_Smooth_Mode.Normal:
                    v1 = noise(xint, yint);
                    v2 = noise(xint + 1, yint);

                    v3 = noise(xint, yint + 1);
                    v4 = noise(xint + 1, yint + 1);
                    break;
                case Perlin_Smooth_Mode.Smooth:
                    v1 = smooth_noise(xint, yint);
                    v2 = smooth_noise(xint + 1, yint);

                    v3 = smooth_noise(xint, yint + 1);
                    v4 = smooth_noise(xint + 1, yint + 1);
                    break;
            }

            return lerp(lerp(v1, v2, x), lerp(v3, v4, x), y);
        }

        public double perlin_noise(float x)
        {
            double result = 0;
            for (int i = 0; i < OCTAVES; i++)
            {
                int frequency = (int)Math.Pow(2, i);
                //float amplitude = (float)Math.Pow(Persistence, i); //Debug
                float amplitude = (float)Math.Pow(Persistence, i + 1);

                result += noise(x * frequency) * amplitude;
            }
            return result;
        }
        public double perlin_noise(float x, float y)
        {
            double result = 0;
            for (int i = 0; i < OCTAVES; i++)
            {
                int frequency = (int)Math.Pow(2, i);
                float amplitude = (float)Math.Pow(Persistence, OCTAVES - (i + 1));

                result += noise((Scale * x) / frequency, (Scale * y) / frequency) * amplitude;
            }
            return result;
        }
    }
}
