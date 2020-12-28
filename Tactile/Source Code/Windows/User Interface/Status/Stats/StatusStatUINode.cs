using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Windows.UserInterface.Status
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
            Text = new RightAdjustedText();
            Text.draw_offset = draw_offset;
            Text.SetFont(Config.UI_FONT, Global.Content, "Blue");
        }
    }
}
