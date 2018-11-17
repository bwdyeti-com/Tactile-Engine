using Microsoft.Xna.Framework;

namespace FEXNA
{
    class LevelUp_Map_Spark : Spark
    {
        public readonly static string FILENAME = "LevelUp_Map";

        public LevelUp_Map_Spark()
        {
            Timer_Maxes = new int[] { 1, 1, 1, 1, 1,
                                    1, 1, 1, 1, 1,
                                    1, 1, 1, 1, 1,
                                    1, 1, 1, 1, 1,
                                    1, 1, 1, 1, 44 };
            Frames = new Vector2(5, 5);
        }
    }
}
