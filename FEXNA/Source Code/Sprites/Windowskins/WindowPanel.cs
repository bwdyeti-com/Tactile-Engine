using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Windows
{
    internal class WindowPanel : Sprite
    {
        public int width = 0, height = 0;
        protected Vector2 _SrcOffset = Vector2.Zero;
        protected int LeftWidth = 8, CenterWidth = 8, RightWidth = 8;
        protected int TopHeight = 8, CenterHeight = 8, BottomHeight = 8;

        #region Accessors
        protected int top_height
        {
            get
            {
                return Math.Min(TopHeight,
                    (height * TopHeight) / (TopHeight + BottomHeight));
            }
        }
        protected int bottom_height
        {
            get { return Math.Min(BottomHeight, height - top_height); }
        }
        internal int body_height
        {
            get { return height - (top_height + bottom_height); }
        }
        internal int body_width
        {
            get
            {
                return Math.Max(0, this.width - (LeftWidth + Math.Min(16, RightWidth)));
            }
        }

        protected Vector2 src_rect_size
        {
            get
            {
                return new Vector2(
                    LeftWidth + CenterWidth + RightWidth,
                    TopHeight + CenterHeight + BottomHeight);
            }
        }

        protected virtual Vector2 SrcOffset { get { return _SrcOffset; } }
        #endregion

        public WindowPanel(Texture2D texture) : this(texture, default(Vector2)) { }
        public WindowPanel(Texture2D texture, Vector2 srcOffset)
            : this(texture, srcOffset, 8, 8, 8, 8, 8, 8) { }
        public WindowPanel(Texture2D texture, Vector2 srcOffset,
            int leftWidth, int centerWidth, int rightWidth,
            int topHeight, int centerHeight, int bottomHeight)
        {
            this.texture = texture;
            _SrcOffset = srcOffset;
            
            LeftWidth = leftWidth;
            CenterWidth = centerWidth;
            RightWidth = rightWidth;
            TopHeight = topHeight;
            CenterHeight = centerHeight;
            BottomHeight = bottomHeight;
        }

        public void set_lines(int lines)
        {
            set_lines(lines, 0);
        }
        public virtual void set_lines(int lines, int y_addition)
        {
            height = lines * 16 + TopHeight + BottomHeight + y_addition;
        }
        
        private void frame_size(int frame, out int width, out int height)
        {
            // Maybe use 0-8 instead of 1-9 //Yeti
            frame--;

            // Bottom
            if (frame / 3 < 1)
            {
                height = BottomHeight;
            }
            // Middle
            else if (frame / 3 < 2)
            {
                height = CenterHeight;
            }
            // Top
            else
            {
                height = TopHeight;
            }

            // Right
            if (frame / 3 < 1)
            {
                width = RightWidth;
            }
            // Middle
            else if (frame / 3 < 2)
            {
                width = CenterWidth;
            }
            // Left
            else
            {
                width = LeftWidth;
            }
        }

        new public virtual Rectangle src_rect(int frame, int width, int height)
        {
            if (frame >= 1 && frame <= 9)
            {
                // Maybe use 0-8 instead of 1-9 //Yeti
                frame--;

                int x = (int)SrcOffset.X;
                int y = (int)SrcOffset.Y;

                // Bottom
                if (frame / 3 < 1)
                {
                    y += TopHeight + CenterHeight;
                }
                // Middle
                else if (frame / 3 < 2)
                {
                    y += TopHeight;
                }

                // Right
                if (frame % 3 > 1)
                {
                    x += LeftWidth + CenterWidth;
                }
                // Middle
                else if (frame % 3 > 0)
                {
                    x += LeftWidth;
                }

                return new Rectangle(x, y, width, height);
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
                    // Top
                    draw_row(sprite_batch, 7, 0, top_height, top_height, draw_offset);
                    // Body
                    int center_height = this.height - (TopHeight + BottomHeight);
                    for (int y = 0; y < center_height; y += CenterHeight)
                    {
                        int dest_height = CenterHeight;
                        dest_height = Math.Min(dest_height, center_height - y);

                        if (dest_height == 0)
                            break;

                        draw_row(sprite_batch, 4,
                            y + top_height, dest_height, dest_height, draw_offset);
                    }
                    // Bottom
                    draw_row(sprite_batch, 1,
                        height - bottom_height, bottom_height, BottomHeight, draw_offset);
                }
        }

        private void draw_row(SpriteBatch spriteBatch, int baseFrame,
            int y, int height, int targetHeight, Vector2 draw_offset)
        {
            Vector2 offset = this.offset;
            Rectangle src_rect;

            // Left
            if (this.width >= LeftWidth)
            {
                src_rect = this.src_rect(baseFrame + 0, LeftWidth, height);
                if (height < targetHeight)
                    src_rect.Y += targetHeight - height;

                offset.X = mirrored ? src_rect.Width - this.offset.X : this.offset.X;
                spriteBatch.Draw(texture, (loc + draw_vector()) - draw_offset, src_rect, tint, angle,
                    offset - new Vector2(0, y),
                    scale, mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);

                // Center
                int center_width = this.width - (LeftWidth + RightWidth);
                center_width = Math.Max(0, center_width);
                for (int x = 0; x < center_width; x += CenterWidth)
                {
                    int dest_width = CenterWidth;
                    dest_width = Math.Min(dest_width, center_width - x);

                    src_rect = this.src_rect(baseFrame + 1, dest_width, height);
                    if (height < targetHeight)
                        src_rect.Y += targetHeight - height;

                    offset.X = mirrored ? src_rect.Width - this.offset.X : this.offset.X;
                    spriteBatch.Draw(texture, (loc + draw_vector()) - draw_offset, src_rect, tint, angle,
                        offset - new Vector2(x + LeftWidth, y),
                        scale, mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }

                // Right
                src_rect = this.src_rect(baseFrame + 2, RightWidth, height);
                if (height < targetHeight)
                    src_rect.Y += targetHeight - height;

                offset.X = mirrored ? src_rect.Width - this.offset.X : this.offset.X;
                spriteBatch.Draw(texture, (loc + draw_vector()) - draw_offset, src_rect, tint, angle,
                    offset - new Vector2(LeftWidth + center_width, y),
                    scale, mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
            }
        }
    }
}
