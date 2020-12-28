using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    enum Snow_Size { Large, Medium, Small }
    class Snow_Handler : Weather_Handler
    {
        protected Vector2[] Sprite_Vels;

        public Snow_Handler(Vector2 draw_offset)
        {
            Angle = 3 / 2f;
            Sprite_Vels = new Vector2[sprite_count() * 2];
            for (int i = 0; i < sprite_count() * 2; i++)
            {
                Sprite_Locs[i] = new_sprite_loc(draw_offset, true);
                int size = (int)particle_size(i);
                Sprite_Rects[i] = new Rectangle(8, size * 8, 8, 8);
                reset_sprite_vel(i);
            }
        }

        private Snow_Size particle_size(int i)
        {
            return (Snow_Size)(2 - ((i % 5) / 2));
        }

        protected override Vector2 new_sprite_loc(Vector2 draw_offset, bool initial)
        {
            if (initial)
                return new Vector2(RAND.Next(Config.WINDOW_WIDTH), RAND.Next(Config.WINDOW_HEIGHT)) + draw_offset;
            else
            {
                return new Vector2(RAND.Next(Config.WINDOW_WIDTH + height_slope) - height_slope,
                    RAND.Next(8) - 4) + draw_offset;
            }
        }

        protected override int sprite_count()
        {
            return Config.SNOW_SPRITE_COUNT;
        }

        protected void reset_sprite_vel(int i)
        {
            Snow_Size size = particle_size(i);
                Sprite_Vels[i] = size == Snow_Size.Small ?
                    // size == 2
                    new Vector2((float)(1 + (RAND.NextDouble() / 2)) / 2f, (float)(1 + (RAND.NextDouble() / 2)) / 2f) :
                    //new Vector2((float)(1 + (RAND.NextDouble() / 2)), (float)(1 + (RAND.NextDouble() / 2))) / 2f :
                    (size == Snow_Size.Medium ?
                    // size == 1
                    new Vector2((float)(1 + RAND.NextDouble()), (float)(1 + RAND.NextDouble()) * 1.5f) :
                    // size == 0
                    new Vector2((float)(1 + RAND.NextDouble()), (float)(1 + RAND.NextDouble()) * 2f));
        }

        public override void update(Vector2 draw_offset)
        {
            for (int i = 0; i < sprite_count() * 2; i++)
            {
                Sprite_Locs[i] += Sprite_Vels[i];
                // If fell off the screen
                if (Sprite_Locs[i].Y > (Config.WINDOW_HEIGHT + draw_offset.Y) ||
                // If went off the right edge
                    Sprite_Locs[i].X > (Config.WINDOW_WIDTH + 16 + draw_offset.X))
                {
                    Sprite_Locs[i] = new_sprite_loc(draw_offset, false);
                    reset_sprite_vel(i);
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
            if (texture != null)
                if (visible)
                {
                    Color tint = this.tint * opacity;

                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    for (int i = Off_Frame ? sprite_count() : 0; i < (Off_Frame ? sprite_count() * 2 : sprite_count()); i++)
                    {
                        if (particle_size(i) != Snow_Size.Small)
                            sprite_batch.Draw(texture, Vector2.Transform((Sprite_Locs[i] + draw_vector()) - draw_offset, matrix),
                                Sprite_Rects[i], tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                }
        }
        public override void draw_lower(SpriteBatch sprite_batch, Vector2 draw_offset, Matrix matrix, float opacity)
        {
            if (texture != null)
                if (visible)
                {
                    Color tint = this.tint * opacity;

                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    for (int i = Off_Frame ? sprite_count() : 0; i < (Off_Frame ? sprite_count() * 2 : sprite_count()); i++)
                    {
                        if (particle_size(i) == Snow_Size.Small)
                            sprite_batch.Draw(texture, Vector2.Transform((Sprite_Locs[i] + draw_vector()) - draw_offset, matrix),
                                Sprite_Rects[i], tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                }
        }
    }
}
