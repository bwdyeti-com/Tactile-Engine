using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Windows.Command
{
    abstract class Window_Command_Scroll : Window_Command
    {
        protected int Rows = 1;
        protected int Scroll = 0;
        protected Vector2 ScrollOffset = Vector2.Zero;
        protected bool ManualScroll = false;

        private readonly static RasterizerState Raster_State = new RasterizerState { ScissorTestEnable = true };

        #region Accessors
        public int scroll
        {
            get { return Scroll; }
            set
            {
                Scroll = value;
                refresh_scroll_visibility();
            }
        }

        internal override int rows { get { return Math.Min(Rows, this.total_rows); } }
        protected int total_rows { get { return base.rows; } }
        #endregion

        protected Window_Command_Scroll() { }
        public Window_Command_Scroll(
            Vector2 loc, int width, int rows, List<string> strs)
        {
            Rows = Math.Max(1, rows);
            initialize(loc, width, strs);
        }
        public Window_Command_Scroll(Window_Command_Scroll window, Vector2 loc, int width, int rows, List<string> strs)
        {
            Rows = Math.Max(1, rows);
            initialize(loc, width, strs);
            if (window != null)
            {
                this.immediate_index = window.index;
                Scroll = window.Scroll;

                int target_y = 16 * Scroll;
                ScrollOffset.Y = target_y;
            }
        }

        protected override void set_nodes(List<CommandUINode> nodes)
        {
            Items = new PartialRangeVisibleUINodeSet<CommandUINode>(nodes);
            Items.CursorMoveSound = System_Sounds.Menu_Move1;
            Items.WrapVerticalMove = true;

            UICursor = new UICursor<CommandUINode>(Items);
            UICursor.draw_offset = new Vector2(-16, 0);
            UICursor.ratio = new int[] { 1, 1 };
        }

        protected abstract void refresh_scroll_visibility();

        public Rectangle scissor_rect()
        {
            int width = Window_Img == null ? this.Width : Window_Img.width;
            Rectangle rect = new Rectangle((int)loc.X + 8, (int)loc.Y + 8, width - 16, Rows * 16);
            rect.X += (int)Text_Offset.X;
            rect.Y += (int)Text_Offset.Y;
            rect.Width += (int)(Size_Offset.X - Text_Offset.X);
            rect.Height += (int)(Size_Offset.Y - Text_Offset.Y);
            return Scene_Map.fix_rect_to_screen(rect);
        }

        protected override Vector2 text_draw_vector()
        {
            return base.text_draw_vector() - ScrollOffset;
        }

        #region Update
        protected override void update_ui(bool input)
        {
            base.update_ui(input);

            if (!ManualScroll)
            {
                int target_y = 16 * Scroll;
                if (Math.Abs(ScrollOffset.Y - target_y) <= 4)
                    ScrollOffset.Y = target_y;
                if (Math.Abs(ScrollOffset.Y - target_y) <= 16)
                    ScrollOffset.Y = Additional_Math.int_closer((int)ScrollOffset.Y, target_y, 4);
                else
                    ScrollOffset.Y = ((int)(ScrollOffset.Y + target_y)) / 2;
            }
        }

        protected override Vector2 cursor_loc(int index = -1)
        {
            Vector2 loc = base.cursor_loc(index);
            return loc - new Vector2(0, Scroll * 16);
        }

        protected override void update_movement(bool input)
        {
            int index = this.index;

            if (num_items() > 0)
                (Items as PartialRangeVisibleUINodeSet<CommandUINode>)
                    .Update(
                        input && !Movement_Locked,
                        visible_indexes_range().Enumerate(),
                        -(this.loc + text_draw_vector()));
            UICursor.update();
            if (Grey_Cursor != null)
                Grey_Cursor.force_loc(UICursor.target_loc);

            if (index != this.index)
                on_index_changed(index);

            if (input)
            {
                if (Global.Input.touch_pressed(false))
                {
                    Vector2 loc = this.loc + Text_Offset + new Vector2(8, 8);
                    if (Global.Input.touch_rectangle(
                        Services.Input.InputStates.Triggered,
                        new Rectangle((int)loc.X, (int)loc.Y,
                            text_area_width, this.rows * 16)))
                    {
                        ManualScroll = true;
                        Console.WriteLine("starting scroll");
                    }
                }
                else
                    ManualScroll = false;

                if (ManualScroll &&
                    Global.Input.gesture_triggered(TouchGestures.VerticalDrag))
                {
                    ScrollOffset.Y += -Global.Input.verticalDragVector.Y;
                    ScrollOffset.Y = MathHelper.Clamp(ScrollOffset.Y,
                        0, (this.total_rows - this.rows) * 16);
                    refresh_scroll();
                }
            }
        }

        protected override void on_index_changed(int oldIndex)
        {
            base.on_index_changed(oldIndex);

            // Not sure what this is //Yeti
            if (Input.ControlScheme != ControlSchemes.Buttons)
            {
                if (Input.IsControllingOnscreenMouse)
                {

                }
            }
            else
            {
                Scroll = Module.normal_scroll(
                    this.index / Columns, Scroll, this.rows, this.total_rows);
                refresh_scroll_visibility();
            }

        }

        private FEXNA_Library.IntRange visible_indexes_range()
        {
            int min = Math.Min(Scroll * Columns, num_items() - 1);
            int max = Math.Min(min + this.rows * Columns, num_items());
            return new FEXNA_Library.IntRange(min, max - 1);
        }

        protected override void update_input(bool input)
        {
            if (Movement_Locked || !input)
                return;
        }

        public void refresh_scroll(bool correctToScrollRange = true)
        {
            if (ManualScroll)
            {
                Scroll = (int)Math.Round(ScrollOffset.Y / 16);
            }
            else
            {
                int row = this.index / Columns;
                if (correctToScrollRange)
                {
                    // Scroll up
                    while (Scroll > 0 && (row <= Scroll || Scroll + Rows > this.total_rows))
                        Scroll--;
                    // scroll down
                    while (Scroll < this.total_rows - Rows && (row + 1 - Rows) >= Scroll)
                        Scroll++;
                }
                ScrollOffset.Y = Scroll * 16;
            }
            refresh_scroll_visibility();
        }
        #endregion
        
        #region Draw
        public override void draw(SpriteBatch sprite_batch)
        {
            if (visible)
            {
                draw_window(sprite_batch);
                // Text
                Rectangle rect = scissor_rect();

                if (rect.Width > 0 && rect.Height > 0)
                {
                    sprite_batch.GraphicsDevice.ScissorRectangle = rect;
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Raster_State);
                    draw_text(sprite_batch);
                    sprite_batch.End();
                }

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                // Cursor
                if (!Manual_Cursor_Draw)
                    draw_cursor(sprite_batch);
                sprite_batch.End();
            }
        }

        new protected virtual void draw_window(SpriteBatch sprite_batch)
        {
            if (Window_Img != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                // Window background
                Window_Img.draw(sprite_batch, - (loc + draw_vector()));
                sprite_batch.End();
            }

            Rectangle rect = scissor_rect();
            if (rect.Width > 0 && rect.Height > 0)
            {
                sprite_batch.GraphicsDevice.ScissorRectangle = rect;
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Raster_State);
                draw_bar(sprite_batch);
                sprite_batch.End();
            }
        }

        protected override void draw_bar(SpriteBatch sprite_batch)
        {
            //Vector2 loc = cursor_loc() + text_draw_vector() + Bar_Offset + new Vector2(8, 8); //Debug
            Vector2 loc = cursor_loc() + base.text_draw_vector() + Bar_Offset + new Vector2(8, 8);
            draw_bar(sprite_batch, loc);
        }
        #endregion
    }
}
