using System.IO;
using Microsoft.Xna.Framework;

namespace Tactile
{
    struct Tone
    {
        // These should be bytes (or shorts, since bytes don't have enough data for [-255 - 255] //Yeti
        private int R;
        private int G;
        private int B;
        private int A;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(R);
            writer.Write(G);
            writer.Write(B);
            writer.Write(A);
        }

        public static Tone read(BinaryReader reader)
        {
            return new Tone(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }
        #endregion

        #region Accessors
        public int r
        {
            get { return R; }
            set { R = (int)MathHelper.Clamp(value, -255, 255); }
        }

        public int g
        {
            get { return G; }
            set { G = (int)MathHelper.Clamp(value, -255, 255); }
        }

        public int b
        {
            get { return B; }
            set { B = (int)MathHelper.Clamp(value, -255, 255); }
        }

        public int a
        {
            get { return A; }
            set { A = (int)MathHelper.Clamp(value, 0, 255); }
        }
        #endregion

        public Tone(int r, int g, int b, int a)
        {
            R = 0; G = 0; B = 0; A = 0;
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }


        public Vector4 to_vector_4()
        {
            return to_vector_4(1f);
        }
        public Vector4 to_vector_4(float mult)
        {
            return new Vector4(R / 255f, G / 255f, B / 255f, A / 255f) * mult;
        }
    }
}
