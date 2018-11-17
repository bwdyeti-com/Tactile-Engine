using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA.Windows.UserInterface.Status
{
    class StatusSkillUINode : StatusSkillIconUINode
    {
        protected FE_Text SkillName;

        internal StatusSkillUINode(
                string helpLabel,
                Func<Game_Unit, SkillState> skillFormula)
            : base(helpLabel, skillFormula)
        {
            SkillName = new FE_Text();
            SkillName.draw_offset = new Vector2(
                Config.SKILL_ICON_SIZE + 4,
                    //(int)Math.Ceiling((Config.SKILL_ICON_SIZE - 16) / 2f), //Debug
                (Config.SKILL_ICON_SIZE - 16) / 2);
            SkillName.Font = "FE7_Text";
            SkillName.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_White");
        }

        internal override void refresh(Game_Unit unit)
        {
            SkillName.visible = false;
            base.refresh(unit);
        }

        protected override void refresh_skill(SkillState state)
        {
            base.refresh_skill(state);
            SkillName.visible = true;
            SkillName.text = state.Skill.Name;
        }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);
            SkillName.update();
        }

        protected override void mouse_off_graphic()
        {
            base.mouse_off_graphic();
            SkillName.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            base.mouse_highlight_graphic();
            SkillName.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();
            SkillName.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);
            SkillName.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
