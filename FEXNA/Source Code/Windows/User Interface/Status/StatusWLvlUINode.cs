using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA.Windows.UserInterface.Status
{
    class StatusWLvlUINode : StatusUINode
    {
        protected Func<Game_Unit, WLvlState> WLvlFormula;
        protected Weapon_Level_Gauge WLvl;

        internal StatusWLvlUINode(
                string helpLabel,
                WeaponType type,
                Func<Game_Unit, WLvlState> wlvlFormula)
            : base(helpLabel)
        {
            WLvlFormula = wlvlFormula;

            WLvl = new Weapon_Level_Gauge(type.Key);
            WLvl.draw_offset = new Vector2(-8, -8);

            Size = new Vector2(56, 16);
        }

        internal override void refresh(Game_Unit unit)
        {
            if (WLvlFormula != null)
            {
                var state = WLvlFormula(unit);

                WLvl.set_data(
                    state.Progress,
                    state.IsCapped ? "Green" : "Blue",
                    state.Rank);
            }
        }

        protected override void update_graphics(bool activeNode)
        {
            WLvl.update();
        }

        protected override void mouse_off_graphic()
        {
            WLvl.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            WLvl.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            WLvl.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            WLvl.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }

        public void DrawGaugeBg(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            WLvl.draw_bg(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
    
    struct WLvlState
    {
        internal string Rank;
        internal float Progress;
        internal bool IsCapped;
    }
}
