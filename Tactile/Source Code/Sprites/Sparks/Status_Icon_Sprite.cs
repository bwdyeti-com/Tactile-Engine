using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Status_Icon_Sprite : Icon_Sprite
    {
        protected TextSprite Counter;

        #region Accessors
        public int counter
        {
            set { Counter.text = value < 0 ? "-" : value.ToString(); }
        }

        public override Color tint
        {
            get { return base.tint; }
            set
            {
                base.tint = value;
                Counter.tint = value;
            }
        }
        #endregion

        public Status_Icon_Sprite()
        {
            this.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Statuses");
            this.size = new Vector2(16, 16);

            Counter = new TextSprite();
            Counter.offset = new Vector2(-11, 0);
            Counter.SetFont(Config.LEVEL_STAT_FONT, Global.Content);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.draw(sprite_batch, draw_offset);
            if (this.visible)
                Counter.draw(sprite_batch, -(this.loc + draw_vector()) + offset + draw_offset);
        }
    }
}
