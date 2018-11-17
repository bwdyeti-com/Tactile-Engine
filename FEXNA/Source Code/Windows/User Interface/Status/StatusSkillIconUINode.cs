using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA.Windows.UserInterface.Status
{
    class StatusSkillIconUINode : StatusUINode
    {
        protected Func<Game_Unit, SkillState> SkillFormula;
        protected Icon_Sprite SkillIcon;
        protected Mastery_Gauge Gauge;

        internal StatusSkillIconUINode(
                string helpLabel,
                Func<Game_Unit, SkillState> skillFormula)
            : base(helpLabel)
        {
            SkillFormula = skillFormula;

            SkillIcon = new Icon_Sprite();
            SkillIcon.size = new Vector2(Config.SKILL_ICON_SIZE, Config.SKILL_ICON_SIZE);
            SkillIcon.draw_offset = new Vector2(0, 0);

            Gauge = new Mastery_Gauge(0);
            Gauge.draw_offset = new Vector2(0, 0);

            Size = new Vector2(96, Config.SKILL_ICON_SIZE);
        }

        internal override void refresh(Game_Unit unit)
        {
            SkillIcon.visible = false;
            Gauge.visible = false;
            _enabled = false;

            if (SkillFormula != null)
            {
                var state = SkillFormula(unit);

                if (state.Skill != null)
                    refresh_skill(state);
            }
        }

        protected virtual void refresh_skill(SkillState state)
        {
            SkillIcon.visible = true;
            _enabled = true;

            SkillIcon.texture = null;
            if (Global.content_exists(@"Graphics/Icons/" + state.Skill.Image_Name))
            {
                SkillIcon.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + state.Skill.Image_Name);
                SkillIcon.index = state.Skill.Image_Index;
#if DEBUG
                SkillIcon.tint = Color.White;
            }
            else
            {
                SkillIcon.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
                SkillIcon.tint = Color.Black;
                SkillIcon.index = 0;
#endif
            }

            Gauge.charge(state.Charge);
            Gauge.visible = state.Charge >= 0;
        }

        protected override void update_graphics(bool activeNode)
        {
            SkillIcon.update();
            Gauge.update();
        }

        protected override void mouse_off_graphic()
        {
#if DEBUG
            if (SkillIcon.tint != Color.Black)
#endif
            SkillIcon.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
#if DEBUG
            if (SkillIcon.tint != Color.Black)
#endif
            SkillIcon.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
#if DEBUG
            if (SkillIcon.tint != Color.Black)
#endif
            SkillIcon.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            SkillIcon.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            Gauge.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
    
    struct SkillState
    {
        internal Data_Skill Skill;
        internal float Charge;
    }
}
