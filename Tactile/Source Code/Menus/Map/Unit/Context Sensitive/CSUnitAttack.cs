using Microsoft.Xna.Framework;

namespace Tactile.Menus.Map.ContextSensitive
{
    class CSUnitAttack
    {
        public Vector2 MoveLoc { get; private set; }
        private Vector2 CursorLoc;
        private int WeaponIndex;

        public CSUnitAttack(
            Vector2 moveLoc,
            Vector2 cursorLoc,
            int weaponIndex)
        {
            MoveLoc = moveLoc;

            CursorLoc = cursorLoc;
            WeaponIndex = weaponIndex;
        }

        public void Apply()
        {
            Global.game_temp.ContextSensitiveUnitMenuAttack(
                CursorLoc,
                CursorLoc,
                WeaponIndex);
        }
    }
}
