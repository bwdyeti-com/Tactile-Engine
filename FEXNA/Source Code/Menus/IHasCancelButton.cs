using Microsoft.Xna.Framework;

namespace FEXNA.Menus
{
    interface IHasCancelButton
    {
        bool HasCancelButton { get; }
        Vector2 CancelButtonLoc { get; }
    }
}
