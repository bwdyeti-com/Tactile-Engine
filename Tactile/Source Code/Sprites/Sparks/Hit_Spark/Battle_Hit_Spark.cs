using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Battle_Hit_Spark : Sprite
    {
        protected int Timer = 0;
        public bool finished = false;

        public Battle_Hit_Spark()
        {
            initialize();
        }

        public virtual void initialize()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Hit-Spark");
            loc.Y = -40;
            loc.X = Config.WINDOW_WIDTH / 2;
            offset.X = Config.WINDOW_WIDTH / 2;
            tint = new Color(255, 255, 255, 0);
            update();
        }

        public override void update()
        {
            if (Timer > 19)
                finished = true;
            switch (Timer)
            {
                case 0:
                case 2:
                case 4:
                case 6:
                case 8:
                case 10:
                    refresh(Timer / 2);
                    break;
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                    refresh(Timer - 6);
                    break;
            }
            Timer++;
        }

        protected virtual void refresh(int timer)
        {
            Src_Rect = new Rectangle((timer % 3) * 320, (timer / 3) * 240, 320, 240);
        }
    }
}
