using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Press_Start : Sprite
    {
        const int FRAMES = 8;
        protected int Timer = 0;
        protected int Frame = 0;

        public Press_Start(Texture2D texture)
        {
            this.texture = texture;
            offset = new Vector2(texture.Width / 2, 0);
        }

        public void reset()
        {
            Timer = 0;
            Frame = 0;
            tint = new Color(0, 0, 0, 0);
        }

        public override void update()
        {
            switch (Timer)
            {
                case 0:
                    Frame = 0;
                    break;
                case 8:
                case 12:
                case 16:
                case 20:
                case 24:
                case 28:
                case 32:
                    Frame = (Timer / 4) - 1;
                    break;
                case 40:
                case 44:
                case 48:
                case 52:
                case 56:
                case 60:
                    Frame = 16 - (Timer / 4);
                    break;
            }
            Timer = (Timer + 1) % 64;
        }

        public override Rectangle src_rect
        {
            get
            {
                return new Rectangle(0, Frame * (texture.Height / FRAMES),
                    texture.Width, texture.Height / FRAMES);
            }
        }
    }
}
