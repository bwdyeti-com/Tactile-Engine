using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Shop_Window_Divider : Sprite
    {
        private int Width;

        #region Accessors
        public int width { set { Width = (value / 8) * 8; } }
        #endregion

        public Shop_Window_Divider()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Shop_Portrait_bg");
            src_rect = new Rectangle(0, 64 + Global.game_options.window_color * 8, 24, 8);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible && Width >= 16)
                {
                    int x = 0;
                    // Left
                    sprite_batch.Draw(texture, (loc + new Vector2(x, 0) + draw_vector()) - draw_offset,
                        new Rectangle(0, 64 + Global.game_options.window_color * 8, 8, 8), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Middle
                    for (x += 8; x < Width - 8; x+= 8 )
                        sprite_batch.Draw(texture, (loc + new Vector2(x, 0) + draw_vector()) - draw_offset,
                            new Rectangle(8, 64 + Global.game_options.window_color * 8, 8, 8), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Right
                    sprite_batch.Draw(texture, (loc + new Vector2(x, 0) + draw_vector()) - draw_offset,
                        new Rectangle(16, 64 + Global.game_options.window_color * 8, 8, 8), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
