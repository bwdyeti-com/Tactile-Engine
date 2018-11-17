using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Battle_Crit_Spark : Battle_Hit_Spark
    {
        public override void initialize()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Dest_Rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            loc.Y = -40;
            loc.X = Config.WINDOW_WIDTH / 2;
            offset.X = Config.WINDOW_WIDTH / 2;
            tint = new Color(255, 255, 255, 0);
            update();
        }

        public override void update()
        {
            if (Timer > 16)
                finished = true;
            switch (Timer)
            {
                case 0:
                case 1:
                    break;
                case 2:
                    texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Crit-Spark");
                    Dest_Rect = null;
                    refresh(Timer - 2);
                    break;
                default:
                    refresh(Timer - 2);
                    break;
            }
            Timer++;
        }

        protected override void refresh(int timer)
        {
            Src_Rect = new Rectangle((timer % 3) * 320, (timer / 3) * 224, 320, 224);
        }
    }
}
