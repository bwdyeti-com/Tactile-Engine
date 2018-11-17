using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Windows.Command
{
    class Window_Command_Scrollbar : Window_Command_Scroll
    {
        protected Scroll_Bar Scrollbar;

        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                if (Scrollbar != null)
                    Scrollbar.stereoscopic = value;
            }
        }

        protected Window_Command_Scrollbar() { }
        public Window_Command_Scrollbar(
            Vector2 loc, int width, int rows, List<string> strs)
            : base(loc, width, rows, strs) { }
        public Window_Command_Scrollbar(
            Window_Command_Scroll window, Vector2 loc, int width, int rows, List<string> strs)
            : base(window, loc, width, rows, strs) { }

        protected override void initialize(Vector2 loc, int width, List<string> strs)
        {
            set_default_offsets(width);
            base.initialize(loc, width, strs);
        }

        protected virtual void set_default_offsets(int width)
        {
            this.text_offset = new Vector2(8, 0);
            this.glow_width = width - (24 + (int)(Text_Offset.X * 2));
            Bar_Offset = new Vector2(0, 0);
            Size_Offset = new Vector2(-8, 0);
        }

        protected override void set_items(List<string> strs)
        {
            base.set_items(strs);
            initialize_scrollbar();

            refresh_scroll_visibility();
        }

        protected void initialize_scrollbar()
        {
            if (Scrollbar != null)
            {
                Scrollbar.UpArrowClicked -= Scrollbar_UpArrowClicked;
                Scrollbar.DownArrowClicked -= Scrollbar_DownArrowClicked;
            }
            Scrollbar = null;

            if (this.total_rows > this.rows)
            {
                Scrollbar = new Scroll_Bar(
                    this.rows * 16 - 16, this.total_rows, this.rows, 0);
                int width = Window_Img == null ? this.Width : Window_Img.width;
                Scrollbar.loc = new Vector2(width - 16, 16);
                Scrollbar.UpArrowClicked += Scrollbar_UpArrowClicked;
                Scrollbar.DownArrowClicked += Scrollbar_DownArrowClicked;
                Scrollbar.scroll = Scroll;
            }
        }

        protected override void refresh_scroll_visibility()
        {
            if (Scrollbar != null)
                Scrollbar.scroll = Scroll;
        }

        #region Update
        protected override void update_movement(bool input)
        {
            if (Scrollbar != null)
            {
                Scrollbar.update();
                if (input)
                    Scrollbar.update_input(-(loc + draw_offset +
                        new Vector2(0, Text_Offset.Y)));
            }

            base.update_movement(input);
        }

        private void Scrollbar_UpArrowClicked(object sender, EventArgs e)
        {
            if (Scroll > 0)
            {
                Global.game_system.play_se(System_Sounds.Menu_Move1);
                Scroll--;
                Scrollbar.scroll = Scroll;
            }
        }
        private void Scrollbar_DownArrowClicked(object sender, EventArgs e)
        {
            if (Scroll < this.total_rows - this.rows)
            {
                Global.game_system.play_se(System_Sounds.Menu_Move1);
                Scroll++;
                Scrollbar.scroll = Scroll;
            }
        }

        protected override void refresh_layout()
        {
            base.refresh_layout();
            refresh_scroll();
            initialize_scrollbar();
        }
        #endregion

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            base.draw_window(sprite_batch);

            if (Scrollbar != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Scrollbar.draw(sprite_batch, -(loc + draw_vector() + new Vector2(0, Text_Offset.Y)));
                sprite_batch.End();
            }
        }
    }
}
