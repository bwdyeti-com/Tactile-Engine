using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using FEXNA.Graphics.Text;
using FEXNA.Windows.UserInterface.Command;
using FEXNA_Library;

namespace FEXNA.Windows.Command.Items
{
    class Window_Command_Item_Assemble : Window_Command_Item_Attack
    {
        public Window_Command_Item_Assemble(int unit_id, Vector2 loc)
            : base(unit_id, loc, "") { }

        protected override List<FEXNA_Library.Item_Data> get_equipment()
        {
            // We're assembling a siege engine,
            // we can't assemble from the siege engine we're standing on that's stupid
            var result = actor().whole_inventory.ToList();
            // But also return any valid items from the convoy
            List<Item_Data> convoy = null;
            if (Global.game_battalions.contains_convoy(Global.battalion.convoy_id))
                convoy = Global.game_battalions.convoy(Global.battalion.convoy_id);
            if (convoy != null)
            {
                result.AddRange(convoy.Where(x => x.is_weapon && x.to_weapon.Ballista()));
            }

            return result;
        }

        protected override void add_commands(List<string> strs)
        {
            if (get_equipment() == null)
                return;
            int count = get_equipment().Count;

            var nodes = new List<CommandUINode>();
            // Add siege engines
            for (int i = 0; i < count; i++)
            {
                var item_node = item("", i);
                if (item_node != null)
                {
                    item_node.loc = item_loc(nodes.Count);
                    nodes.Add(item_node);
                    Index_Redirect.Add(i);
                }
            }

            set_nodes(nodes);
        }

        protected override bool is_valid_item(List<Item_Data> items, int i)
        {
            var item_data = items[i];
            if (item_data.non_equipment || !item_data.is_weapon)
                return false;

            FEXNA_Library.Data_Weapon weapon = item_data.to_weapon;
            return weapon.Ballista();
        }

        protected override CommandUINode item(string str, int i)
        {
            var item_data = items(i);
            if (!is_valid_item(get_equipment(), i))
                return null;

            var text = new Status_Item();
            text.set_image(actor(), item_data);
            text.set_text_color(true);

            var text_node = new ItemUINode("", text, this.column_width);
            text_node.loc = item_loc(i);
            return text_node;
        }

        protected override bool show_equipped()
        {
            return false;
        }
    }
}
