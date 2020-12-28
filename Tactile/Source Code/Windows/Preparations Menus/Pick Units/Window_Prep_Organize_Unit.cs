using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Preparations;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using Tactile.Windows.UserInterface;
using Tactile.Windows.UserInterface.Preparations;

namespace Tactile
{
    class Window_Prep_Organize_Unit : Window_Prep_Items_Unit
    {
        const int COLUMNS = 2;
        const int ROW_SIZE = 16;
        const int ROWS = (Config.WINDOW_HEIGHT - 64) / ROW_SIZE;

        private bool UnitSelected = false;
        private Pick_Units_Header Unit_Header;

        public Window_Prep_Organize_Unit()
        {
            this.loc = new Vector2((Config.WINDOW_WIDTH - this.Width), 20);
        }

        #region WindowPrepActorList Abstract
        protected override int Columns { get { return COLUMNS; } }
        protected override int VisibleRows { get { return ROWS; } }
        protected override int RowSize { get { return ROW_SIZE; } }
        #endregion

        protected override int Width { get { return unit_spacing() * this.Columns + 8 + 16; } }

        protected override Rectangle Unit_Scissor_Rect
        {
            get
            {
                Vector2 loc = this.loc + ScissorRectOffset;
                return new Rectangle((int)loc.X, (int)loc.Y, this.Width, this.Height - 4);
            }
        }
        protected override Vector2 ScissorRectOffset { get { return new Vector2(-8, 4); } }

        protected override Vector2 ScrollbarLoc { get { return new Vector2(this.Width + 2 - 16, 12); } }

        protected override void initialize_sprites()
        {
            // Window
            Window_Img = new System_Color_Window();
            Window_Img.width = this.Width;
            Window_Img.height = this.Height + 12;
            Window_Img.offset = new Vector2(0, 8);
            // UI Nodes
            refresh_nodes();

            Rows = (int)Math.Ceiling(Global.battalion.actors.Count / (float)this.Columns);
            // Scrollbar
            if (Rows > this.VisibleRows)
            {
                Scrollbar = new Scroll_Bar(this.VisibleRows * this.RowSize - 16, Rows, this.VisibleRows, 0);
                Scrollbar.loc = this.ScrollbarLoc;

                Scrollbar.UpArrowClicked += Scrollbar_UpArrowClicked;
                Scrollbar.DownArrowClicked += Scrollbar_DownArrowClicked;
            }
            // Unit Header
            Unit_Header = new Pick_Units_Header(this.Width + 8);
            Unit_Header.loc = new Vector2(-8, -20);
            // Cursor
            Selected_Cursor = new Hand_Cursor();
            Selected_Cursor.loc = cursor_loc() + new Vector2(8, 4);
            Selected_Cursor.tint = new Color(192, 192, 192, 255);
        }
        
        protected override Vector2 unit_offset()
        {
            return new Vector2(-12 + 8, 0);
        }

        protected override int unit_spacing()
        {
            return 64;
        }

        public void unit_selected(bool value)
        {
            UnitSelected = value;
            if (UnitSelected)
            {
                set_selected_loc();

                if (Windows.Map.Window_Prep_Organize.SWAP)
                {
                    int index = this.index + 1;
                    if (index >= Global.battalion.actors.Count)
                        index = Math.Max(0, index - 2);
                    this.index = index;
                }

                refresh_scroll(false);
                update_cursor();
                UnitCursor.move_to_target_loc();
            }
        }

        #region Draw
        protected override void draw_window(SpriteBatch sprite_batch)
        {
            Vector2 offset = this.loc + draw_vector();

            base.draw_window(sprite_batch);
            Unit_Header.draw(sprite_batch, -offset);
        }

        protected override void draw_selected_cursor(SpriteBatch sprite_batch)
        {
            Vector2 offset = this.loc + draw_vector();

            if (UnitSelected)
                Selected_Cursor.draw(sprite_batch, Offset - offset);
        }
        #endregion
    }
}
