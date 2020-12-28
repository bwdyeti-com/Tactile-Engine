using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TactileLibrary;

namespace Tactile
{
    class WeaponTriangleHelpSet : Sprite
    {
        private List<WeaponTriangleHelp> WTSet = new List<WeaponTriangleHelp>();

        public void set_item(Data_Equipment item)
        {
            WTSet.Clear();

            if (item != null && item.is_weapon)
            {
                var weapon = item as Data_Weapon;

                int x = 0;

                add_wta(weapon.Reaver(), weapon.main_type(), ref x);
                if (weapon.Scnd_Type != 0)
                    add_wta(weapon.Reaver(), weapon.scnd_type(), ref x);
            }
        }

        private void add_wta(bool reaver, WeaponType type, ref int x)
        {
            if (type.WtaTypes.Count > 0 || type.WtaReaverTypes.Count > 0 ||
                type.WtaRanges.Count > 0 || type.WtdRanges.Count > 0)
            {
                var wta_types = reaver ? type.WtaReaverTypes : type.WtaTypes;
                foreach (int other_type in wta_types)
                {
                    WeaponType adv_type = Global.weapon_types[other_type];
                    WeaponType dis_type = get_disadvantage_type(type, adv_type, reaver);

                    WeaponTriangleHelpTypes result;
                    if (reaver)
                        result = new WeaponTriangleHelpReaverTypes(type, adv_type, dis_type);
                    else
                        result = new WeaponTriangleHelpTypes(type, adv_type, dis_type);
                    result.draw_offset = new Vector2(x, 0);
                    WTSet.Add(result);

                    x -= 48;
                }
            }

            if (type.ParentKey > 0)
            {
                type = Global.weapon_types[type.ParentKey];
                add_wta(reaver, type, ref x);
            }
        }

        private static WeaponType get_disadvantage_type(WeaponType type, WeaponType adv_type, bool reaver)
        {
            if (reaver)
            {
                if (Global.weapon_types
                    .Any(weapon_type => adv_type.WtaReaverTypes.Contains(weapon_type.Key) &&
                        weapon_type.WtaReaverTypes.Contains(type.Key)))
                {
                    return Global.weapon_types
                        .First(weapon_type => adv_type.WtaReaverTypes.Contains(weapon_type.Key) &&
                            weapon_type.WtaReaverTypes.Contains(type.Key));
                }
            }
            else
            {
                if (Global.weapon_types
                    .Any(weapon_type => adv_type.WtaTypes.Contains(weapon_type.Key) &&
                        weapon_type.WtaTypes.Contains(type.Key)))
                {
                    return Global.weapon_types
                        .First(weapon_type => adv_type.WtaTypes.Contains(weapon_type.Key) &&
                            weapon_type.WtaTypes.Contains(type.Key));
                }
            }
            return null;
        }

        public override void update()
        {
            foreach (var wta in WTSet)
                wta.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (this.visible)
            {
                Vector2 offset = this.loc + draw_vector();
                foreach (var wta in WTSet)
                    wta.draw(sprite_batch, draw_offset - offset);
            }
        }
    }
}
