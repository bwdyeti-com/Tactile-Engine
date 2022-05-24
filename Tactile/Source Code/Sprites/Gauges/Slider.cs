using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Graphics.Gauges
{
    class Slider : Stat_Bar
    {
        protected Texture2D ThumbTexture;

        public Slider()
        {
            ThumbTexture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/UIButtons");
        }

        public override void draw_fill(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            base.draw_fill(sprite_batch, draw_offset);

            if (visible)
                if (ThumbTexture != null)
                {
                    sprite_batch.Draw(ThumbTexture, (loc + draw_vector()) - draw_offset +
                        new Vector2(fill_width - 6, -6),
                        new Rectangle(0, 32, 16, 16), tint, 0f,
                    this.offset, 1f, SpriteEffects.None, 0f);
                }
        }
    }
}
