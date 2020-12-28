using System;

namespace TactileLibrary.EventArg
{
    public delegate void IntEventHandler(object sender, IntEventArgs e);
    public class IntEventArgs : EventArgs
    {
        public int Value { get; private set; }

        public IntEventArgs(int value)
        {
            Value = value;
        }
    }
}
