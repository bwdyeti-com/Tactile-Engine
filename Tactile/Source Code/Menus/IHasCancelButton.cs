using Microsoft.Xna.Framework;

namespace Tactile.Menus
{
    interface IHasCancelButton
    {
        bool HasCancelButton { get; }
        Vector2 CancelButtonLoc { get; }
    }
}
