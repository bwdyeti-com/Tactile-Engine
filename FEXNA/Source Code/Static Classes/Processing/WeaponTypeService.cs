namespace FEXNA
{
    class WeaponTypeService : FEXNA_Library.IWeaponTypeService
    {
        public FEXNA_Library.WeaponType type(int key)
        {
            if (key >= 0 && key < Global.weapon_types.Count)
                return Global.weapon_types[key];
            return null;
        }
    }
}
