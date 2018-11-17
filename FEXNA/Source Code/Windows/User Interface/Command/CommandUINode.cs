using System.Collections.Generic;
using FEXNA.Windows.UserInterface;

namespace FEXNA.Windows.UserInterface.Command
{
    abstract class CommandUINode : UINode
    {
        protected bool _enabled = true;

        internal override bool Enabled { get { return _enabled; } }
        internal string HelpLabel { get; private set; }

        protected override IEnumerable<Inputs> ValidInputs
        {
            get
            {
                yield return Inputs.A;
                yield return Inputs.R;
            }
        }
        protected override bool RightClickActive { get { return true; } }

        internal CommandUINode(string helpLabel)
        {
            HelpLabel = helpLabel;
        }

        internal abstract void set_text_color(string color);
    }
}
