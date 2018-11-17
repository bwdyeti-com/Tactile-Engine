using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Data_Team_Window : Sprite
    {
        public readonly static string FILENAME = @"Graphics/Windowskins/Data_Screen";
        const int WIDTH = 120;

        public int Height = 0;
        public bool small = false;
        protected int Team;

        public Data_Team_Window(int team, int height)
        {
            texture = Global.Content.Load<Texture2D>(Data_Team_Window.FILENAME);
            Team = (int)MathHelper.Clamp(team, 1, Constants.Team.NUM_TEAMS) - 1;
            Height = (height + 1) * 16 + 8;
        }

        new public Rectangle src_rect(int frame, int width, int height)
        {
            int x = 80 + 32 * Team;
            int y = 208;
            switch (frame)
            {
                // Bottom
                case 1:
                    return new Rectangle(x + 0, y + 24, width, height);
                case 2:
                    return new Rectangle(x + 8, y + 24, width, height);
                case 3:
                    return new Rectangle(x + 16, y + 24, width, height);
                // Middle
                case 4:
                    return new Rectangle(x + 0, y + 16, width, height);
                case 5:
                    return new Rectangle(x + 8, y + 16, width, height);
                case 6:
                    return new Rectangle(x + 16, y + 16, width, height);
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
                    while (y < Height)
                    {
                        int y_frame = (y == 0 ? 6 : (Height - y <= 16 ? 0 : 3));
                        int y_base = y_frame == 3 ? 8 : 16;
                        temp_height = y_base;
                        if (Height - (y + y_base) < y_base && Height - (y + y_base) != 0)
                            temp_height = Height - (y + y_base);
                        int x = 0;
                        int temp_width;
                            while (x < WIDTH)
                            {
                                int x_frame = (x == 0 ? 1 : (WIDTH - x <= 16 ? 3 : 2));
                                int x_base = x_frame == 3 ? 16 : 8;
                                temp_width = x_base;
                                if (WIDTH - (x + x_base) < x_base && WIDTH - (x + x_base) != 0)
                                    temp_width = WIDTH - (x + x_base);
                                // Draw
                                src_rect = this.src_rect(y_frame + x_frame, temp_width, temp_height);
                                if (mirrored) offset.X = src_rect.Width - offset.X;
                                sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset, src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);

                                x += temp_width;
                            }
                        y += temp_height;
                    }
                    // Draw team name
                    sprite_batch.Draw(texture, (loc + draw_vector()) - draw_offset, new Rectangle(32, 128 + 8 * Team, 56, 8),
                        tint, angle, offset - new Vector2(8, 2), scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
