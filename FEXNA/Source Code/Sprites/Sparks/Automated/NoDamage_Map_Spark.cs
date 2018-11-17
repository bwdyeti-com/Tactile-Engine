using Microsoft.Xna.Framework;

namespace FEXNA
{
    class NoDamage_Map_Spark : Spark
    {
        public readonly static string FILENAME = "NoDamage_Map";

        public NoDamage_Map_Spark()
        {
            Timer_Maxes = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 10, 3, 3, 3 };
            Frames = new Vector2(1, 12);
        }
    }
}
