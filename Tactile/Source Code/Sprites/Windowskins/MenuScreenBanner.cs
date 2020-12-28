using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class MenuScreenBanner : Sprite
    {
        public readonly static string FILENAME = @"Graphics/Pictures/Menu_Banner";

        public int width = 0, height = 32;

        public MenuScreenBanner()
        {
            texture = Global.Content.Load<Texture2D>(MenuScreenBanner.FILENAME);
        }

        new public Rectangle src_rect(int frame, int width)
        {
            int x = 0;
            int y = 32 * Global.game_options.window_color;
            switch (frame)
            {
                // Left
                case 0:
                    return new Rectangle(x + 0, y, width, height);
                // Middle
                case 1:
                    return new Rectangle(x + 24, y, width, height);
                // Right
                case 2:
                    return new Rectangle(x + 32, y, width, height);
            }
            if (frames.Count == 0)
                return new Rectangle(0, 0, texture.Width, texture.Height);
            else if (current_frame < 0)
                return new Rectangle(0, 0, 0, 0);
            else
                return frames[current_frame];
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (!(texture == null))
                if (visible)
                {
                    Vector2 offset = this.offset;
                    Rectangle src_rect;
                    int x = 0;
                    int temp_width;
                        while (x < width)
                        {
                            temp_width = 8;
                            if (x == 0)
                            {
                                temp_width = 24;
                                if (width - (x + 24) < 24 && width - (x + 24) != 0)
                                    temp_width = width - (x + 24);
                                src_rect = this.src_rect(0, temp_width);
                                sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset, src_rect, tint, angle, offset - new Vector2(x, 0), scale,
                                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                            }
                            else if (width - x <= 8)
                            {
                                if (width - (x + 8) < 8 && width - (x + 8) != 0)
                                    temp_width = width - (x + 8);
                                src_rect = this.src_rect(2, temp_width);
                                sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset, src_rect, tint, angle, offset - new Vector2(x, 0), scale,
                                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                            }
                            else
                            {
                                if (width - (x + 8) < 8 && width - (x + 8) != 0)
                                    temp_width = width - (x + 8);
                                src_rect = this.src_rect(1, temp_width);
                                sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset, src_rect, tint, angle, offset - new Vector2(x, 0), scale,
                                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                            }
                            x += temp_width;
                        }
                }
        }
    }
}
