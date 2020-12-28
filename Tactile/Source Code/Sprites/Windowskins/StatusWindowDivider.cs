using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class StatusWindowDivider : Sprite
    {
        private int Width;

        public StatusWindowDivider()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Shop_Portrait_bg");
        }

        public void SetWidth(int value)
        {
            Width = (value / 8) * 8;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible && Width >= 16)
                {
                    int srcY = 64 + Global.game_options.window_color * 8;
                    int x = 0;
                    // Left
                    sprite_batch.Draw(texture, (loc + new Vector2(x, 0) + draw_vector()) - draw_offset,
                        new Rectangle(0, srcY, 8, 8), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Middle
                    for (x += 8; x < Width - 8; x += 8)
                        sprite_batch.Draw(texture, (loc + new Vector2(x, 0) + draw_vector()) - draw_offset,
                            new Rectangle(8, srcY, 8, 8), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Right
                    sprite_batch.Draw(texture, (loc + new Vector2(x, 0) + draw_vector()) - draw_offset,
                        new Rectangle(16, srcY, 8, 8), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
