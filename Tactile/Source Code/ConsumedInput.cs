using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tactile
{
    struct ConsumedInput
    {
        public ControlSchemes Control { get; private set; }
        public int Index { get; private set; }

        public ConsumedInput(ControlSchemes control, int index)
        {
            Control = control;
            Index = index;
        }

        public bool IsSomething { get { return Control != ControlSchemes.None; } }
        public bool IsNothing { get { return Control == ControlSchemes.None; } }
    }
}
