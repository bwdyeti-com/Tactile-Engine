using System;

namespace FEXNA.Windows.UserInterface.Status
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
            Label.Font = "FE7_TextL";
        }
    }
}
