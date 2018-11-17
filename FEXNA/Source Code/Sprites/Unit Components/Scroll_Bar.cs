using System;   
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Scroll_Bar : Sprite
    {
        const int TIME_MAX = 48;
        protected int Max_Items, Items_At_Once, Height;
        protected int Timer_Down = 0, Timer_Up = 0;
        protected bool Moving_Down, Moving_Up;
        protected int Scroll;
        protected bool Up_Arrow_Visible, Down_Arrow_Visible;


        public Scroll_Bar(int height, int max_items, int items_at_once, int initial_index)
        {
            offset.Y = 0;
            Height = height;
            Max_Items = max_items;
            height = Math.Max(1, height);
            Items_At_Once = items_at_once;
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/FE_Scroll_Bar");
            scroll = initial_index;
        }

        #region Accessors
        public int scroll
        {
            set
            {
                Up_Arrow_Visible = value > 0;
                Down_Arrow_Visible = value < (Max_Items - Items_At_Once);
                value = Math.Min(value, Max_Items - Items_At_Once);
                Scroll = value * (Height - (Height * Items_At_Once / Max_Items)) / (Max_Items - Items_At_Once);
            }
        }
        #endregion

        public void moving_up()
        {
            Moving_Up = true;
        }

        public void moving_down()
        {
            Moving_Down = true;
        }

        public override void update()
        {
            Timer_Up = (Timer_Up + (Moving_Up ? 4 :1)) % TIME_MAX;
            Timer_Down = (Timer_Down + (Moving_Down ? 4 : 1)) % TIME_MAX;
            Moving_Up = false;
            Moving_Down = false;
        }

        internal EventHandler UpArrowClicked;
        internal EventHandler DownArrowClicked;

        public void update_input(Vector2 draw_offset = default(Vector2))
        {
            if (Input.ControlScheme == ControlSchemes.Buttons)
                return;

            Vector2 loc = (this.loc + this.draw_offset) - draw_offset;
            Rectangle up_arrow_rect = new Rectangle(
                (int)loc.X, (int)loc.Y - 9, 8, 8);
            Rectangle down_arrow_rect = new Rectangle(
                (int)loc.X, (int)loc.Y + Height + 2, 8, 8);

            // Up Arrow clicked
            if (Global.Input.mouseScroll > 0 ||
                (Global.Input.mouse_down_rectangle(
                    MouseButtons.Left, up_arrow_rect) &&
                Global.Input.mouse_triggered(MouseButtons.Left, false)) ||
                Global.Input.gesture_rectangle(
                    TouchGestures.Tap, up_arrow_rect) ||
                Global.Input.gesture_rectangle(
                    TouchGestures.DoubleTap, up_arrow_rect))
            {
                if (UpArrowClicked != null)
                    UpArrowClicked(this, new EventArgs());
            }
            // Down Arrow clicked
            else if (Global.Input.mouseScroll < 0 ||
                (Global.Input.mouse_down_rectangle(
                    MouseButtons.Left, down_arrow_rect) &&
                Global.Input.mouse_triggered(MouseButtons.Left, false)) ||
                Global.Input.gesture_rectangle(
                    TouchGestures.Tap, down_arrow_rect) ||
                Global.Input.gesture_rectangle(
                    TouchGestures.DoubleTap, down_arrow_rect))
            {
                if (DownArrowClicked != null)
                    DownArrowClicked(this, new EventArgs());
            }
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    int y = -1;
                    // Bar
                    sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(0, y)) - draw_offset,
                        new Rectangle(0, 0, 8, 1), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    y++;
                    while (y < Height)
                    {
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(0, y)) - draw_offset,
                            new Rectangle(0, 1, 8, 1), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        y++;
                    }
                    sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(0, y)) - draw_offset,
                        new Rectangle(0, 2, 8, 1), tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    // Fill
                    y = 0;
                    while (y < Height * Items_At_Once / Max_Items)
                    {
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(0, y + Scroll)) - draw_offset,
                            new Rectangle(0, 3, 8, 1), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        y++;
                    }
                    // Arrows
                    if (Up_Arrow_Visible)
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(0, -7)) - draw_offset,
                            new Rectangle(8, 4 + 6 * (Timer_Up / 8), 8, 6), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    if (Down_Arrow_Visible)
                        sprite_batch.Draw(texture, (this.loc + draw_vector() + new Vector2(0, Height + 2)) - draw_offset,
                            new Rectangle(0, 4 + 6 * (Timer_Down / 8), 8, 6), tint, angle, offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
