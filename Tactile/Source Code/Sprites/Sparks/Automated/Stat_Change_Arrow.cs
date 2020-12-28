using Microsoft.Xna.Framework;

namespace Tactile
{
    class Stat_Change_Arrow : Spark
    {
        public readonly static string FILENAME = "Arrows";
        public int value = 0;

        public override void update()
        {
            update(-1);
        }
        public void update(int glow)
        {
            if (glow > -1)
            {

            }
            if (value == 0)
            {
                if (Timer < 16)
                {
                    switch (Timer)
                    {
                        case 0:
                        case 1:
                        case 2:
                            Frame = Timer;
                            break;
                        case 3:
                        case 4:
                            Frame = 3;
                            break;
                        case 5:
                        case 6:
                            Frame = 4;
                            break;
                        case 7:
                        case 8:
                        case 9:
                            Frame = 5;
                            break;
                        case 10:
                        case 11:
                            Frame = 6;
                            break;
                        case 12:
                        case 13:
                            Frame = 7;
                            break;
                        case 14:
                        case 15:
                            Frame = Timer - 6;
                            break;
                    }
                    Timer++;
                }
                else
                {
                    if (Timer < 2)
                    {
                        Frame = Timer;
                        Timer++;
                    }
                }
            }
        }

        public override Rectangle src_rect
        {
            get
            {
                return new Rectangle(16 * Frame, value % 2, 16, 24);
            }
        }
    }
}
