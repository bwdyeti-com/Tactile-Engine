using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA.Windows.UserInterface.Status
{
    class StatusLivesUINode : StatusUINode
    {
        protected Casual_Mode_Lives Lives;

        internal StatusLivesUINode(
                string helpLabel)
            : base(helpLabel)
        {
            Lives = new Casual_Mode_Lives();
            Lives.draw_offset = new Vector2(0, 0);

            Size = new Vector2(16, 16);
        }

        internal override void refresh(Game_Unit unit)
        {
            Lives.refresh(unit);
            _enabled = Casual_Mode_Lives.lives_visible(unit);
        }

        protected override void update_graphics(bool activeNode)
        {
            Lives.update();
        }

        protected override void mouse_off_graphic()
        {
            Lives.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            Lives.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            Lives.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Lives.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
