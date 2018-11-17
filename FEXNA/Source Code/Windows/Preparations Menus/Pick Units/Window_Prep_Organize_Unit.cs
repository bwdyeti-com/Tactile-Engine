using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Preparations;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Preparations;

namespace FEXNA
{
    class Window_Prep_Organize_Unit : Window_Prep_Items_Unit
    {
        const int COLUMNS = 2;
        const int ROW_SIZE = 16;
        const int ROWS = (Config.WINDOW_HEIGHT - 64) / ROW_SIZE;

        private bool UnitSelected = false;
        private Pick_Units_Header Unit_Header;

        protected override void initialize()
        {
            WIDTH = unit_spacing() * columns() + 8 + 16;
            HEIGHT = rows() * ROW_SIZE + 8;
            loc = new Vector2((Config.WINDOW_WIDTH - WIDTH), 20);
            Unit_Scissor_Rect = new Rectangle((int)loc.X - 8, (int)loc.Y + 4, WIDTH, HEIGHT - 4);
            initialize_sprites();
            initialize_index();
        }

        protected override void initialize_sprites()
        {
            // Window
            Window_Img = new System_Color_Window();
            Window_Img.width = WIDTH;
            Window_Img.height = HEIGHT + 12;
            Window_Img.offset = new Vector2(0, 8);
            // UI Nodes
            refresh_nodes();

            Rows = (int)Math.Ceiling(Global.battalion.actors.Count / (float)columns());
            // Scrollbar
            if (Rows > rows())
            {
                Scrollbar = new Scroll_Bar(rows() * row_size() - 16, Rows, rows(), 0);
                Scrollbar.loc = loc + new Vector2(WIDTH + 2 - 16, 12);

                Scrollbar.UpArrowClicked += Scrollbar_UpArrowClicked;
                Scrollbar.DownArrowClicked += Scrollbar_DownArrowClicked;
            }
            // Unit Header
            Unit_Header = new Pick_Units_Header(width + 8);
            Unit_Header.loc = new Vector2(-8, -20);
            // Cursor
            Selected_Cursor = new Hand_Cursor();
            Selected_Cursor.loc = cursor_loc() + new Vector2(8, 4);
            Selected_Cursor.tint = new Color(192, 192, 192, 255);
        }

        protected override int columns()
        {
            return COLUMNS;
        }

        protected override int rows()
        {
            return ROWS;
        }
        protected override int row_size()
        {
            return ROW_SIZE;
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
            Vector2 loc = this.loc + draw_vector();
            base.draw_window(sprite_batch);
            Unit_Header.draw(sprite_batch, -loc);
        }

        protected override void draw_selected_cursor(SpriteBatch sprite_batch)
        {
            if (UnitSelected)
                Selected_Cursor.draw(sprite_batch, Offset - draw_vector());
        }
        #endregion
    }
}
