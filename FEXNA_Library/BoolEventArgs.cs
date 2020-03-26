using System;

namespace FEXNA_Library.EventArg
{
    public delegate void BoolEventHandler(object sender, BoolEventArgs e);
    public class BoolEventArgs : EventArgs
    {
        public bool Value { get; private set; }

        public BoolEventArgs(bool value)
        {
            Value = value;
        }
    }
}
