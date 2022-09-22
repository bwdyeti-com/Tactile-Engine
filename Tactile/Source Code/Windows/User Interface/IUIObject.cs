using System;
using Microsoft.Xna.Framework;

namespace Tactile.Windows.UserInterface
{
    [Flags]
    enum ControlSet : byte
    {
        None = 0,
        PadMove = 1 << 0,
        PadButtons = 1 << 1,
        MouseMove = 1 << 2,
        MouseButtons = 1 << 3,
        TouchMove = 1 << 4,
        TouchButtons = 1 << 5,

        Pad = PadMove | PadButtons,
        Mouse = MouseMove | MouseButtons,
        Touch = TouchMove | TouchButtons,

        Pointing = Mouse | Touch,
        Movement = PadMove | MouseMove | TouchMove,

        All = Pad | Mouse | Touch,

        Disabled = 1 << 7
    }

    interface IUIObject
    {
        void UpdateInput(Vector2 drawOffset = default(Vector2));

        Rectangle OnScreenBounds(Vector2 drawOffset);

        bool MouseOver(Vector2 drawOffset = default(Vector2));
    }
}
