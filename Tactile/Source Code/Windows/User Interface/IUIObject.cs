using System;
using Microsoft.Xna.Framework;

namespace Tactile.Windows.UserInterface
{
    [Flags]
    enum ControlSet : byte
    {
        None = 0,
        Buttons = 1 << 0,
        MouseMove = 1 << 1,
        MouseButtons = 1 << 2,
        TouchMove = 1 << 3,
        TouchButtons = 1 << 4,
        Mouse = MouseMove | MouseButtons,
        Touch = TouchMove | TouchButtons,
        Pointing = Mouse | Touch,
        Movement = Buttons | MouseMove | TouchMove,
        All = Buttons | Mouse | Touch,
        Disabled = 1 << 5
    }

    interface IUIObject
    {
        void UpdateInput(Vector2 drawOffset = default(Vector2));

        Rectangle OnScreenBounds(Vector2 drawOffset);

        bool MouseOver(Vector2 drawOffset = default(Vector2));
    }
}
