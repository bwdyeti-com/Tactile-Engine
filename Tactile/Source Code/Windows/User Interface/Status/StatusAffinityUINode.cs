using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile.Windows.UserInterface.Status
{
    class StatusAffinityUINode : StatusUINode
    {
        protected Func<Game_Unit, Affinities> AffinityFormula;
        protected Icon_Sprite AffinityIcon;
        protected TextSprite Label;

        internal StatusAffinityUINode(
                string helpLabel,
                Func<Game_Unit, Affinities> affinityFormula)
            : base(helpLabel)
        {
            AffinityFormula = affinityFormula;

            AffinityIcon = new Icon_Sprite();
            AffinityIcon.draw_offset = new Vector2(40, 0);
            AffinityIcon.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Icons/Affinity Icons");
            AffinityIcon.size = new Vector2(16, 16);

            Label = new TextSprite();
            Label.draw_offset = new Vector2(0, 0);
            Label.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Label.text = "Affinity";

            Size = new Vector2(56, 16);
        }

        internal override void refresh(Game_Unit unit)
        {
            AffinityIcon.index = (int)AffinityFormula(unit);
        }

        protected override void update_graphics(bool activeNode)
        {
            AffinityIcon.update();
            Label.update();
        }

        protected override void mouse_off_graphic()
        {
            Label.tint = Color.White;
            AffinityIcon.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            Label.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
            AffinityIcon.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            Label.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
            AffinityIcon.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            AffinityIcon.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            Label.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
