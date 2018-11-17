using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Objective_Window : Sprite
    {
        protected int Lines = 1;

        #region Accessors
        public int lines
        {
            get { return Lines; }
            set { Lines = Math.Max(value, 1); }
        }
        #endregion

        public Objective_Window()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Objective_Info");
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    Vector2 offset = this.offset;
                    if (mirrored)
                        offset.X = src_rect.Width - offset.X;
                    if (Lines == 1)
                    {
                        // Top
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(0, 0)) - draw_offset,
                            new Rectangle(0 + 0, 41 * Global.game_options.window_color + 0, 86, 4), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        // Bottom
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(0, 20)) - draw_offset,
                            new Rectangle(0 + 0, 41 * Global.game_options.window_color + 36, 86, 5), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        // Center
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(8, 4)) - draw_offset,
                            new Rectangle(0 + 8, 41 * Global.game_options.window_color + 4, 70, 16), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        // Sides
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(0, 4)) - draw_offset,
                            new Rectangle(0 + 0, 41 * Global.game_options.window_color + 12, 8, 16), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(78, 4)) - draw_offset,
                            new Rectangle(0 + 78, 41 * Global.game_options.window_color + 12, 8, 16), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                    else
                    {
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(0, 0)) - draw_offset,
                            new Rectangle(0, 41 * Global.game_options.window_color, 86, 12), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        int y = 0;
                        for (; y < Lines - 1; y++)
                            sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(0, 12 + y * 16)) - draw_offset,
                                new Rectangle(0, 12 + 41 * Global.game_options.window_color, 86, 16), tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(0, 12 + y * 16)) - draw_offset,
                            new Rectangle(0, 28 + 41 * Global.game_options.window_color, 86, 13), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                }
        }
    }
}
