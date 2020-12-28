namespace Tactile
{
    class WeaponTypeService : TactileLibrary.IWeaponTypeService
    {
        public TactileLibrary.WeaponType type(int key)
        {
            if (key >= 0 && key < Global.weapon_types.Count)
                return Global.weapon_types[key];
            return null;
        }
    }
}
