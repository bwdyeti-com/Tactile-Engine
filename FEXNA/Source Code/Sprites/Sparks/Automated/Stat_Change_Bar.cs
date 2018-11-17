using Microsoft.Xna.Framework;

namespace FEXNA
{
    class Stat_Change_Bar : Spark
    {
        public readonly static string FILENAME = "Arrows";

        public Stat_Change_Bar()
        {
            offset = new Vector2(0, -1);
        }

        public override void update()
        {
            update(-1);
        }
        public void update(int glow)
        {
            if (glow > -1)
            {

            }
            Frame = Timer;
            if (Timer < 3)
                Timer++;
        }

        public override Rectangle src_rect
        {
            get
            {
                return new Rectangle(0, 68 + 3 * Frame, 44, 3);
            }
        }
    }
}