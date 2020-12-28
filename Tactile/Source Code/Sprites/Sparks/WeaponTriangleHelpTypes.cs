using Microsoft.Xna.Framework;
using TactileLibrary;

namespace Tactile
{
    class WeaponTriangleHelpTypes : WeaponTriangleHelp
    {
        public WeaponTriangleHelpTypes(
            WeaponType type, WeaponType advType, WeaponType disType)
        {
            var icon = new Weapon_Type_Icon();
            icon.loc = new Vector2(0, 0);
            icon.offset = icon.size / 2;
            icon.index = type.IconIndex;
            Icon1 = icon;

            var adv_icon = new Weapon_Type_Icon();
            adv_icon.loc = new Vector2(12, 20);
            adv_icon.offset = adv_icon.size / 2;
            adv_icon.index = advType.IconIndex;
            Icon2 = adv_icon;

            if (disType != null)
            {
                var dis_icon = new Weapon_Type_Icon();
                dis_icon.loc = new Vector2(-12, 20);
                dis_icon.offset = dis_icon.size / 2;
                dis_icon.index = disType.IconIndex;
                Icon3 = dis_icon;
            }
        }
    }
}
