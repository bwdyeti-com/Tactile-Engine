using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FEXNA.Windows.UserInterface.Command;
using FEXNA_Library;

namespace FEXNA.Windows.Command.Items
{
    class Window_Command_Item_Staff : Window_Command_Item_Attack
    {
        public Window_Command_Item_Staff(int unit_id, Vector2 loc)
            : base(unit_id, loc, "") { }

        protected override bool is_valid_item(List<Item_Data> items, int i)
        {
            if (unit.actor.is_equippable(items, i))
            {
                var item_data = items[i];
                Data_Weapon weapon = item_data.to_weapon;
                if (unit.actor.is_equippable(weapon) && weapon.is_staff())
                {
                    if (unit.allies_in_staff_range(new HashSet<Vector2> { unit.loc }, i)[0].Count > 0)
                        return true;
                    else if (unit.enemies_in_staff_range(new HashSet<Vector2> { unit.loc }, i)[0].Count > 0)
                        return true;
                    else if (unit.untargeted_staff_range(i)[1].Count > 0)
                        return true;
                }
            }
            return false;
        }

        protected override CommandUINode item(string str, int i)
        {
            var item_data = items(i);
            if (!is_valid_item(get_equipment(), i))
                return null;

            var text = new Attack_Item();
            text.set_image(actor(), item_data);
            var text_node = new ItemUINode("", text, this.column_width);
            text_node.loc = item_loc(i);
            return text_node;
        }
    }
}
