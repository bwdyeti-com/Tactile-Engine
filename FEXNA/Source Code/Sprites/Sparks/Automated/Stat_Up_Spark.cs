using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Stat_Up_Spark : Spark
    {
        public readonly static string FILENAME = "Stat_Up_Spark";

        public Stat_Up_Spark()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + FILENAME);
            Timer_Maxes = new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3 };
            Frames = new Vector2(11, 1);
        }
    }
}
