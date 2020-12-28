using System;

namespace Tactile.Windows.UserInterface.Status
{
    class StatusStatLargeLabelUINode : StatusStatUINode
    {
        internal StatusStatLargeLabelUINode(
            string helpLabel,
            string label,
            Func<Game_Unit, string> statFormula,
            int textOffset = 48)
                : base(helpLabel, label, statFormula, textOffset) { }

        protected override void initialize_label(string label)
        {
            base.initialize_label(label);
            Label.SetFont(Config.UI_FONT + "L", Config.UI_FONT);
        }
    }
}
