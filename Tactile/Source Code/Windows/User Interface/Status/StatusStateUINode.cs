using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile.Windows.UserInterface.Status
{
    class StatusStateUINode : StatusUINode
    {
        protected Func<Game_Unit, Tuple<int, int>> StateFormula;
        protected Status_Icon_Sprite StateIcon;

        internal StatusStateUINode(
                string helpLabel,
                Func<Game_Unit, Tuple<int, int>> skillFormula)
            : base(helpLabel)
        {
            StateFormula = skillFormula;

            StateIcon = new Status_Icon_Sprite();
            StateIcon.draw_offset = new Vector2(0, 0);

            Size = new Vector2(16, 16);
        }

        internal override void refresh(Game_Unit unit)
        {
            StateIcon.visible = false;
            _enabled = false;

            if (StateFormula != null)
            {
                var state = StateFormula(unit);

                if (state.Item1 > -1 && Global.data_statuses.ContainsKey(state.Item1))
                {
                    var status = Global.data_statuses[state.Item1];

                    StateIcon.visible = true;
                    _enabled = true;

                    StateIcon.index = status.Image_Index;
                    StateIcon.counter = state.Item2;
                }
            }
        }

        protected override void update_graphics(bool activeNode)
        {
            StateIcon.update();
        }

        protected override void mouse_off_graphic()
        {
            StateIcon.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            StateIcon.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            StateIcon.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            StateIcon.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
