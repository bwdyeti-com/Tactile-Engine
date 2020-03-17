using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    public class Text_Box : Sprite
    {
        protected int Width, Height;

        #region Accessors
        public int width
        {
            get { return Width; }
            set { Width = value; }
        }

        public int height
        {
            get { return Height; }
            set { Height = value; }
        }

        public override int opacity
        {
            get { return tint.R; }
            set
            {
                byte opacity = (byte)MathHelper.Clamp(value, 0, 255);
                tint = new Color(opacity, opacity, opacity, (opacity * 31 / 32));
            }
        }
        #endregion

        public Text_Box() { }

        public Text_Box(int width, int height)
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Message_Window");
            Width = width;
            Height = height;
            opacity = 255;
        }

        new public virtual Rectangle src_rect(int frame, int width, int height)
        {
            int x = 0;
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
            }
            return new Rectangle();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    // Window
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    draw_window(sprite_batch, draw_offset);
                    sprite_batch.End();
                }
        }

        protected virtual void draw_window(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            Rectangle src_rect;
            int y = 0;
            int temp_height;
            while (y < Height)
            {
                temp_height = 8;
                if (Height - (y + 8) < 8 && Height - (y + 8) != 0)
                    temp_height = Height - (y + 8);
                int x = 0;
                int temp_width;
                while (x < Width)
                {
                    temp_width = 8;
                    if (Width - (x + 8) < 8 && Width - (x + 8) != 0)
                        temp_width = Width - (x + 8);
                    if (x == 0)
                    {
                        src_rect = this.src_rect((y == 0 ? 7 : (Height - y <= 8 ? 1 : 4)),
                            temp_width, temp_height);
                        if (mirrored) offset.X = src_rect.Width - offset.X;
                        sprite_batch.Draw(texture, loc - draw_offset, src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                    else if (Width - x <= 8)
                    {
                        src_rect = this.src_rect((y == 0 ? 9 : (Height - y <= 8 ? 3 : 6)),
                            temp_width, temp_height);
                        if (mirrored) offset.X = src_rect.Width - offset.X;
                        sprite_batch.Draw(texture, loc - draw_offset, src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                    else
                    {
                        src_rect = this.src_rect((y == 0 ? 8 : (Height - y <= 8 ? 2 : 5)),
                            temp_width, temp_height);
                        if (mirrored) offset.X = src_rect.Width - offset.X;
                        sprite_batch.Draw(texture, loc - draw_offset, src_rect, tint, angle, offset - new Vector2(x, y), scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }
                    x += temp_width;
                }
                y += temp_height;
            }
        }
    }
}
