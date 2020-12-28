using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Rain_Handler : Weather_Handler
    {
        public Rain_Handler(Vector2 draw_offset)
        {
            Angle = 3f;
            for (int i = 0; i < sprite_count() * 2; i++)
            {
                Sprite_Locs[i] = new_sprite_loc(draw_offset, true);
                Sprite_Rects[i] = new Rectangle(0, 0, 8, RAND.Next(4) == 0 ? 8 : 16);
            }
        }

        protected override Vector2 new_sprite_loc(Vector2 draw_offset, bool initial)
        {
            if (initial)
                return new Vector2(RAND.Next(Config.WINDOW_WIDTH), RAND.Next(Config.WINDOW_HEIGHT)) + draw_offset;
            else
            {
                return new Vector2(RAND.Next(Config.WINDOW_WIDTH + height_slope) - height_slope,
                    RAND.Next(16) - 8) + draw_offset;
            }
        }

        protected override int sprite_count()
        {
            return Config.RAIN_SPRITE_COUNT;
        }

        public override void update(Vector2 draw_offset)
        {
            for (int i = 0; i < sprite_count() * 2; i++)
            {
                Sprite_Locs[i] += new Vector2(4) * new Vector2(1, Angle);
                // If fell off the screen
                if (Sprite_Locs[i].Y > (Config.WINDOW_HEIGHT + draw_offset.Y) ||
                // If went off the right edge
                    Sprite_Locs[i].X > (Config.WINDOW_WIDTH + 16 + draw_offset.X))
                {
                    Sprite_Locs[i] = new_sprite_loc(draw_offset, false);
                    Sprite_Rects[i] = new Rectangle(0, 0, 8, RAND.Next(4) == 0 ? 8 : 16);
                }
                // If off the top of the screen due to scrolling
                else if (Sprite_Locs[i].Y + Sprite_Rects[i].Y < draw_offset.Y)
                    Sprite_Locs[i] += new Vector2(0, Config.WINDOW_HEIGHT);
                // If off the left of the screen due to scrolling
                //else if (Sprite_Locs[i].X + Sprite_Rects[i].X < -16 + draw_offset.X) //Debug
                //    Sprite_Locs[i] += new Vector2(Config.WINDOW_WIDTH + 32, 0);
            }
            base.update(draw_offset);
        }

        public override void draw_upper(SpriteBatch sprite_batch, Vector2 draw_offset, Matrix matrix, float opacity)
        {
            if (!Off_Frame && texture != null)
                if (visible)
                {
                    Color tint = this.tint * opacity;

                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    for (int i = Off_Frame ? sprite_count() : 0; i < (Off_Frame ? sprite_count() * 2 : sprite_count()); i++)
                    {
                        sprite_batch.Draw(texture, Vector2.Transform((Sprite_Locs[i] + draw_vector()) - draw_offset, matrix),
                            Sprite_Rects[i], tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                }
        }
        public override void draw_lower(SpriteBatch sprite_batch, Vector2 draw_offset, Matrix matrix, float opacity)
        {
            if (Off_Frame && texture != null)
                if (visible)
                {
                    Color tint = this.tint * opacity;

                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    for (int i = Off_Frame ? sprite_count() : 0; i < (Off_Frame ? sprite_count() * 2 : sprite_count()); i++)
                    {
                        sprite_batch.Draw(texture, Vector2.Transform((Sprite_Locs[i] + draw_vector()) - draw_offset, matrix),
                            Sprite_Rects[i], tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                }
        }
    }
}
