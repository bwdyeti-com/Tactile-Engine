using System;
// Debug MessageBox lol
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace FEXNA
{
    public class Print
    {

        // Debug MessageBox lol
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern uint MessageBox(IntPtr hWnd, String text, String caption, uint type);

        public static void message(string str)
        {
            message(str, "MessageBox title");
        }
        public static void message(string str, string title)
        {
            // Debug MessageBox lol
#if !WINDOWS
            return;
#endif
            MessageBox(new IntPtr(0), str, title, 0);
        }
    }
}
