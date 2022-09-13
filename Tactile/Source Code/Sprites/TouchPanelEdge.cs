using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Graphics.Help
{
    class TouchPanelEdge : Sprite
    {
        const int WAIT_TIME = 300;

        protected int Timer;

        public TouchPanelEdge()
        {
            Texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/TouchEdge");
            Reset();
            this.offset = new Vector2(0, Texture.Height);
        }

        public void Reset()
        {
            Timer = WAIT_TIME;
            this.visible = false;
        }

        public void Rotate(bool isLeftEdge)
        {
            if (isLeftEdge)
                // Rotate 90 degrees for the left edge
                this.angle = MathHelper.PiOver2;
            else
                // Don't rotate for the bottom edge
                this.angle = 0;
        }

        public override void update()
        {
            if (Timer > 0)
            {
                Timer--;
                if (Timer == 0)
                    this.visible = true;
                else
                    this.visible = false;
            }
            else
                this.visible = true;
        }
    }
}
