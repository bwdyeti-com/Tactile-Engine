using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Graphics.Windows
{
    class SystemWindowHeadered : System_Color_Window
    {
        new public readonly static string FILENAME = @"Graphics/Windowskins/Headered_Window";

        public SystemWindowHeadered()
            : base(Global.Content.Load<Texture2D>(SystemWindowHeadered.FILENAME))
        {
            TopHeight = 24;
        }

        public override Rectangle src_rect(int frame, int width, int height)
        {
            int x = 0;
            int y = 40 * window_color;
            switch (frame)
            {
                // Bottom
                case 1:
                    return new Rectangle(x + 0, y + 32, width, height);
                case 2:
                    return new Rectangle(x + 8, y + 32, width, height);
                case 3:
                    return new Rectangle(x + 16, y + 32, width, height);
                // Middle
                case 4:
                    return new Rectangle(x + 0, y + 24, width, height);
                case 5:
                    return new Rectangle(x + 8, y + 24, width, height);
                case 6:
                    return new Rectangle(x + 16, y + 24, width, height);
                // Top row
                case 7:
                    return new Rectangle(x + 0, y + 0, width, height);
                case 8:
                    return new Rectangle(x + 8, y + 0, width, height);
                case 9:
                    return new Rectangle(x + 16, y + 0, width, height);
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
                    int y = 0;
                    int temp_height;
                    while (y < height)
                    {
                        int base_height = y == 0 ? 24 : 8;
                        temp_height = base_height;
                        if (height - (y + base_height) < base_height && height - (y + base_height) != 0)
                            temp_height = height - (y + base_height);
                        int x = 0;
                        int temp_width;
                        while (x < width)
                        {
                            temp_width = 8;
                            if (width - (x + 8) < 8 && width - (x + 8) != 0)
                                temp_width = width - (x + 8);
                            if (x == 0)
                            {
                                src_rect = this.src_rect((y == 0 ? 7 : (height - y <= 8 ? 1 : 4)),
                                    temp_width, temp_height);
                                if (mirrored) offset.X = src_rect.Width - offset.X;
                                sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset, src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                            }
                            else if (width - x <= 8)
                            {
                                src_rect = this.src_rect((y == 0 ? 9 : (height - y <= 8 ? 3 : 6)),
                                    temp_width, temp_height);
                                if (mirrored) offset.X = src_rect.Width - offset.X;
                                sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset, src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                            }
                            else
                            {
                                src_rect = this.src_rect((y == 0 ? 8 : (height - y <= 8 ? 2 : 5)),
                                    temp_width, temp_height);
                                if (mirrored) offset.X = src_rect.Width - offset.X;
                                sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset, src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                            }
                            x += temp_width;
                        }
                        y += temp_height;
                    }
                }
        }
    }
}
