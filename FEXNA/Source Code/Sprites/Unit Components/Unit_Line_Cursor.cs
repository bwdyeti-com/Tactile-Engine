using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Unit_Line_Cursor : Sprite
    {
        const int TIME_MAX = 64;
        int Width;
        int Frame = 0;

        #region Accessors
        protected int row { get { return (Frame < TIME_MAX / 2 ? Frame / 4 : (TIME_MAX - (Frame + 1)) / 4); } }
        #endregion

        public Unit_Line_Cursor(int width)
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Unit_Window_Components");
            Width = Math.Max(24, width / 8 * 8);
        }

        public override void update()
        {
            Frame = (Frame + 1) % TIME_MAX;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    int x = 0;
                    // Left
                    sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(x, 0)) - draw_offset,
                        new Rectangle(Global.game_options.window_color * 48 + 0, 16 + row * 8, 8, 8), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    x += 8;
                    // Center
                    while (x + 16 <= Width)
                    {
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(x, 0)) - draw_offset,
                            new Rectangle(Global.game_options.window_color * 48 + 16, 16 + row * 8, 8, 8), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        x += 8;
                    }
                    // Right
                    sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(x, 0)) - draw_offset,
                        new Rectangle(Global.game_options.window_color * 48 + 40, 16 + row * 8, 8, 8), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
