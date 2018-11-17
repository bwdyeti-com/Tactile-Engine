using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA.Windows.UserInterface.Status
{
    class StatusSupportBonusUINode : StatusStatUINode
    {
        private Func<Game_Actor, string> ActorTextFormula;
        protected FE_Text Bonus;

        internal StatusSupportBonusUINode(
            string helpLabel,
            string label,
            Func<Game_Unit, string> statFormula,
            Func<Game_Actor, string> actorStatFormula,
            int textOffset = 48)
            : base(helpLabel, label, statFormula, textOffset)
        {
            ActorTextFormula = actorStatFormula;
            Size = new Vector2(textOffset, 16);

            Bonus = new FE_Text();
            Bonus.draw_offset = new Vector2(textOffset + 0, 0);
            Bonus.Font = "FE7_TextBonus";
        }

        internal void refresh(Game_Actor actor)
        {
            Text.text = ActorTextFormula(actor);
            stat_bonus(0);
        }
        internal void stat_bonus(int value)
        {
            Bonus.text = value == 0 ? "" : string.Format(
                "{0}{1}", value > 0 ? "+" : "", value);
            Bonus.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_" + (value > 0 ? "Green" : "Red"));
        }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);
            Bonus.update();
        }

        protected override void mouse_off_graphic()
        {
            base.mouse_off_graphic();
            Bonus.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            base.mouse_highlight_graphic();
            Bonus.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();
            Bonus.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);
            Bonus.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
