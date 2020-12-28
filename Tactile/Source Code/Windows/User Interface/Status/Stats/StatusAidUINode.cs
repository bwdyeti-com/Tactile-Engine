using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Windows.UserInterface.Status
{
    class StatusAidUINode : StatusStatUINode
    {
        protected Func<Game_Unit, int> AidTypeFormula;
        protected Icon_Sprite Aid_Icon;

        internal StatusAidUINode(
            string helpLabel,
            string label,
            Func<Game_Unit, string> statFormula,
            Func<Game_Unit, int> aidTypeFormula,
            int textOffset = 48)
                : base(helpLabel, label, statFormula, textOffset)
        {
            AidTypeFormula = aidTypeFormula;

            Aid_Icon = new Icon_Sprite();
            Aid_Icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Aid");
            Aid_Icon.size = new Vector2(16, 16);
            Aid_Icon.columns = 1;
            Aid_Icon.draw_offset = new Vector2(textOffset, 0);

            Size = new Vector2(textOffset + 24, 16);
        }

        internal override void refresh(Game_Unit unit)
        {
            base.refresh(unit);
            if (AidTypeFormula != null)
                Aid_Icon.index = AidTypeFormula(unit);
        }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);
            Aid_Icon.update();
        }

        protected override void mouse_off_graphic()
        {
            base.mouse_off_graphic();
            Aid_Icon.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            base.mouse_highlight_graphic();
            Aid_Icon.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();
            Aid_Icon.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);

            Aid_Icon.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
