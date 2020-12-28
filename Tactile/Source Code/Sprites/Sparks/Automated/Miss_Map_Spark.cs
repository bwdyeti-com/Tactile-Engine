using Microsoft.Xna.Framework;

namespace Tactile
{
    class Miss_Map_Spark : Spark
    {
        public readonly static string FILENAME = "Miss_Map";

        public Miss_Map_Spark()
        {
            Timer_Maxes = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 10, 3, 3, 3 };
            Frames = new Vector2(1, 13);
        }
    }
}
