using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Windows.Command
{
    class Window_Command_Scroll_Arrow : Window_Command_Scroll
    {
        protected Page_Arrow Up_Page_Arrow, Down_Page_Arrow;

        public override float stereoscopic
        {
            set
            {
                base.stereoscopic = value;
                Up_Page_Arrow.stereoscopic = value;
                Down_Page_Arrow.stereoscopic = value;
            }
        }

        public Window_Command_Scroll_Arrow(
            Vector2 loc, int width, int rows, List<string> strs)
            : base(loc, width, rows, strs) { }
        public Window_Command_Scroll_Arrow(
            Window_Command_Scroll window, Vector2 loc, int width, int rows, List<string> strs)
            : base(window, loc, width, rows, strs) { }

        protected override void set_items(List<string> strs)
        {
            base.set_items(strs);

            // Page Arrows
            Up_Page_Arrow = new Page_Arrow();
            Up_Page_Arrow.loc = new Vector2(Window_Img.width / 2 + 8, 0);
            Up_Page_Arrow.angle = MathHelper.PiOver2;
            Down_Page_Arrow = new Page_Arrow();
            Down_Page_Arrow.loc = new Vector2(Window_Img.width / 2 + 8, this.rows * 16 + 16);
            Down_Page_Arrow.mirrored = true;
            Down_Page_Arrow.angle = MathHelper.PiOver2;

            Up_Page_Arrow.ArrowClicked += Up_Page_Arrow_ArrowClicked;
            Down_Page_Arrow.ArrowClicked += Down_Page_Arrow_ArrowClicked;

            refresh_scroll_visibility();
        }

        protected override void refresh_scroll_visibility()
        {
            Up_Page_Arrow.visible = Scroll > 0;
            Down_Page_Arrow.visible = Scroll < this.total_rows - (this.rows);
        }

        #region Update
        protected override void update_movement(bool input)
        {
            Up_Page_Arrow.update();
            Down_Page_Arrow.update();

            if (Global.Input.mouse_triggered(MouseButtons.Left, false))
            { }

            //Down_Page_Arrow.angle += MathHelper.Pi / 360f;
            //Down_Page_Arrow.mirrored = !Down_Page_Arrow.mirrored;

            if (Global.Input.mouseScroll > 0)
            {
                Up_Page_Arrow_ArrowClicked(this, null);
            }
            else if (Global.Input.mouseScroll < 0)
            {
                Down_Page_Arrow_ArrowClicked(this, null);
            }
            else
            {
                Up_Page_Arrow.UpdateInput(-(loc + draw_offset));
                Down_Page_Arrow.UpdateInput(-(loc + draw_offset));
            }

            base.update_movement(input);
        }

        private void Up_Page_Arrow_ArrowClicked(object sender, EventArgs e)
        {
            if (Scroll > 0)
            {
                Global.game_system.play_se(System_Sounds.Menu_Move1);
                Scroll--;
                refresh_scroll_visibility();
            }
        }
        private void Down_Page_Arrow_ArrowClicked(object sender, EventArgs e)
        {
            if (Scroll < this.total_rows - this.rows)
            {
                Global.game_system.play_se(System_Sounds.Menu_Move1);
                Scroll++;
                refresh_scroll_visibility();
            }
        }

        protected override void refresh_layout()
        {
            base.refresh_layout();
            refresh_scroll();
            Up_Page_Arrow.loc = new Vector2(Window_Img.width / 2 + 8, 0);
            Down_Page_Arrow.loc = new Vector2(Window_Img.width / 2 + 8, this.rows * 16 + 16);
            refresh_scroll_visibility();
        }
        #endregion

        public override void draw(SpriteBatch sprite_batch)
        {
            base.draw(sprite_batch);
            if (active)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                // Page Arrows
                Up_Page_Arrow.draw(sprite_batch, -(loc + draw_vector()));
                Down_Page_Arrow.draw(sprite_batch, -(loc + draw_vector()));
                sprite_batch.End();
            }
        }
    }
}
