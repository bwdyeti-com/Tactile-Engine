using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA.Windows.UserInterface.Status
{
    class StatusStatUINode : StatusLabeledTextUINode
    {
        internal StatusStatUINode(
            string helpLabel,
            string label,
            Func<Game_Unit, string> statFormula,
            int textOffset = 48)
            : base(helpLabel, label, statFormula, textOffset)
        {
            Size = new Vector2(textOffset, 16);
        }

        protected override void initialize_text(Vector2 draw_offset)
        {
            Text = new FE_Text_Int();
            Text.draw_offset = draw_offset;
            Text.Font = "FE7_Text";
            Text.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
        }
    }
}
