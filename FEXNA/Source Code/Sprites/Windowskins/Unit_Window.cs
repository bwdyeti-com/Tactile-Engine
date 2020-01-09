using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EnumExtension;

namespace FEXNA
{
    [Flags]
    enum Unit_Burst_Tail : int
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 3,
        Top = 4,
        Bottom = 8
    }
    class Unit_Window : Sprite
    {
        int Width, Height, Team;
        public Unit_Burst_Tail tail = Unit_Burst_Tail.None;

        public int team { set { Team = value; } }

        protected Unit_Window() { }
        public Unit_Window(int width, int height, int team)
        {
            Width = width;
            Height = height;
            Team = team;
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Unit_Info");
        }

        public void set_width(int value)
        {
            Width = value;
        }
        public void set_height(int value)
        {
            Height = value;
        }

        new public Rectangle src_rect(int frame, int width, int height)
        {
            int x = (Team - 1) * 24;
            int y = 0;
            switch (frame)
            {
                // Bottom
                case 1:
                    return new Rectangle(x + 0, y + 16, width, height);
                case 2:
                    return new Rectangle(x + 8, y + 16, width, height);
                case 3:
                    return new Rectangle(x + 16, y + 16, width, height);
                // Middle
                case 4:
                    return new Rectangle(x + 0, y + 8, width, height);
                case 5:
                    return new Rectangle(x + 8, y + 8, width, height);
                case 6:
                    return new Rectangle(x + 16, y + 8, width, height);
                // Top row
                case 7:
                    return new Rectangle(x + 0, y + 0, width, height);
                case 8:
                    return new Rectangle(x + 8, y + 0, width, height);
                case 9:
                    return new Rectangle(x + 16, y + 0, width, height);
                // Tail
                case 10:
                    return new Rectangle(x + 0, y + 24, width, height);
                case 11:
                    return new Rectangle(x + 8, y + 24, width, height);
                case 12:
                    return new Rectangle(x + 0, y + 32, width, height);
                case 13:
                    return new Rectangle(x + 8, y + 32, width, height);
            }
            return new Rectangle();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    // Window
                    Vector2 offset = this.offset;
                    if (Team >= 1 && Team <= Constants.Team.NUM_TEAMS)
                        draw_window(Width + 16, Height + 16, sprite_batch, draw_offset);
                }
        }

        protected void draw_window(int width, int height, SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            Rectangle src_rect;
            int y = 0;
            int temp_height;
            while (y < height)
            {
                temp_height = 8;
                if (height - (y + 8) < 8 && height - (y + 8) != 0)
                    temp_height = height - (y + 8);
                int x = 0;
                int temp_width;
                while (x < width)
                {
                    temp_width = 8;
                    if (width - (x + 8) < 8 && width - (x + 8) != 0)
                        temp_width = width - (x + 8);
                    int frame;
                    // Left edge
                    if (x == 0)
                    {
                        frame = (y == 0 ? 7 : (height - y <= 8 ? 1 : 4));
                    }
                    // Right edge
                    else if (width - x <= 8)
                    {
                        frame = (y == 0 ? 9 : (height - y <= 8 ? 3 : 6));
                    }
                    // Middle
                    else
                    {
                        frame = (y == 0 ? 8 : (height - y <= 8 ? 2 : 5));
                        if (frame == 2 && tail.HasEnumFlag(Unit_Burst_Tail.Bottom))
                        {
                            if (tail.HasEnumFlag(Unit_Burst_Tail.Middle))
                            {
                                    if (x == 24)
                                        frame = 11;
                            }
                            else if (tail.HasEnumFlag(Unit_Burst_Tail.Left))
                            {
                                    if (x == 16)
                                        frame = 11;
                            }
                            else if (tail.HasEnumFlag(Unit_Burst_Tail.Right))
                            {
                                if (width - x < 32 && width - x >= 24)
                                    frame = 10;
                            }
                        }
                        if (frame == 8 && tail.HasEnumFlag(Unit_Burst_Tail.Top))
                        {
                            if (tail.HasEnumFlag(Unit_Burst_Tail.Middle))
                            {
                                    if (x == 24)
                                        frame = 13;
                            }
                            else if (tail.HasEnumFlag(Unit_Burst_Tail.Left))
                            {
                                    if (x == 16)
                                        frame = 13;
                            }
                            else if (tail.HasEnumFlag(Unit_Burst_Tail.Right))
                            {
                                    if (width - x < 32 && width - x >= 24)
                                        frame = 12;
                            }
                        }
                    }
                    src_rect = this.src_rect(frame, temp_width, temp_height);
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(texture, loc - draw_offset, src_rect, tint, angle, offset - new Vector2(x, y), scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);

                    x += temp_width;
                }
                y += temp_height;
            }
        }
    }
}
