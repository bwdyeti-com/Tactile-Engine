using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Windows.Command.Items
{
    class Window_Command_Item_Discard : Window_Command_Item
    {
        public Window_Command_Item_Discard(int unit_id, Vector2 loc)
        {
            Unit_Id = unit_id;
            initialize(loc, WIDTH, new List<string>());
            ItemInfo.SetWTHelpVisible(false);
        }

        protected override List<FEXNA_Library.Item_Data> get_equipment()
        {
            // Can't discard siege engine, so only uses actor items, never unit list
            return actor().whole_inventory;
        }

        protected override void equip_actor()
        {
            if (current_item_data.is_weapon)
            {
                actor().equip(redirect() + 1);
            }
        }

        protected override CommandUINode item(string str, int i)
        {
            var item_data = items(i);
            if (!is_valid_item(get_equipment(), i))
                return null;

            var text = new DiscardItem();
            text.set_image(actor(), item_data);
            // Make the new item blue
            if (i >= Constants.Actor.NUM_ITEMS)
                text.change_text_color("Blue");

            var text_node = new ItemUINode("", text, this.column_width);
            text_node.loc = item_loc(i);
            return text_node;
        }
        
        public virtual string drop_text()
        {
            return "Drop";
        }
    }
}
