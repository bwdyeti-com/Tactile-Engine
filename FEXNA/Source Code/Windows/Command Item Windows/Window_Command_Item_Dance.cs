using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using FEXNA.Graphics.Text;
using FEXNA.Windows.UserInterface.Command;
using FEXNA_Library;

namespace FEXNA.Windows.Command.Items
{
    class Window_Command_Item_Dance : Window_Command_Item_Attack
    {
        public Window_Command_Item_Dance(int unit_id, Vector2 loc)
            : base(unit_id, loc, "") { }

        protected override void add_commands(List<string> strs)
        {
            if (get_equipment() == null)
                return;
            int count = get_equipment().Count;

            var nodes = new List<CommandUINode>();
            // Add default dance
            if (unit.dance_targets(true).Count > 0)
            {
                var dance_node = text_item(unit.dance_name(), 0);
                if (dance_node != null)
                {
                    nodes.Add(dance_node);
                    Index_Redirect.Add(-1);
                }
            }
            // Add rings
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
            if (item_data.non_equipment || !item_data.is_item)
                return false;

            FEXNA_Library.Data_Item item = item_data.to_item;
            return item.Dancer_Ring;
        }

        public override void open_help()
        {
            if (unit.dance_targets(true).Count > 0 ||
                    Index_Redirect[this.index] > -1) // get UINodes Items index //Debug
                base.open_help();
        }

        protected override bool show_equipped()
        {
            return false;
        }
    }
}
