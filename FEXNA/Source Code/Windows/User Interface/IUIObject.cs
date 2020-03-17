using System;
using Microsoft.Xna.Framework;

namespace FEXNA.Windows.UserInterface
{
    [Flags]
    enum ControlSet : byte
    {
        None = 0,
        Buttons = 1 << 0,
        MouseMove = 1 << 1,
        MouseButtons = 1 << 2,
        Touch = 1 << 3,
        Mouse = MouseMove | MouseButtons,
        Movement = Buttons | MouseMove | Touch,
        All = Buttons | Mouse | Touch,
        Disabled = 1 << 4
    }

    interface IUIObject
    {
        void UpdateInput(Vector2 drawOffset = default(Vector2));

        Rectangle OnScreenBounds(Vector2 drawOffset);
    }
}
