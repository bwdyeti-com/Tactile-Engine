using Microsoft.Xna.Framework;

namespace Tactile.Menus.Map.Unit.Target
{
    interface ITargetMenu
    {
        int UnitId { get; }
        
        bool IsWindowA<K>();

        bool ManualTargeting { get; }
        bool HasTarget { get; }

        Vector2 TargetLoc { get; }
        Vector2 LastTargetLoc { get; }

        bool IsRescueDropMenu { get; }
    }
}
