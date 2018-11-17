using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using FEXNA_Library;

namespace FEXNA.Windows.Command.Items
{
    class Window_Command_Item_Trample : Window_Command_Item_Attack
    {
        public Window_Command_Item_Trample(int unit_id, Vector2 loc)
            : base(unit_id, loc, "") { }

        protected override bool is_valid_item(List<Item_Data> items, int i)
        {
            var item_data = items[i];
            if (item_data.non_equipment || !item_data.is_weapon)
                return false;

            Data_Weapon weapon = item_data.to_weapon;
            if (unit.actor.is_equippable(weapon) && !weapon.is_staff() &&
                unit.actor.useable_melee_weapon(item_data))
            {
                return true;
            }
            return false;
        }
    }
}
