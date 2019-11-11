using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Windows.Command.Items
{
    class Window_Command_Item_Target_Inventory : Window_Command_Item
    {
        private int Active_Item;
        private Hand_Cursor Active_Item_Cursor;

        public Window_Command_Item_Target_Inventory(int unit_id, Vector2 loc, int active_item)
        {
            Unit_Id = unit_id;

            Active_Item = active_item;
            initialize(loc, WIDTH, new List<string>());
            this.immediate_index = active_item;

            Active_Item_Cursor = new Hand_Cursor();
            Active_Item_Cursor.tint = new Color(192, 192, 192, 255);
            Active_Item_Cursor.draw_offset = new Vector2(-16, 0);
            Active_Item_Cursor.force_loc(UICursor.target_loc);
        }

        protected override List<FEXNA_Library.Item_Data> get_equipment()
        {
            // Can't target/repair siege engine, so only uses actor items, never unit list
            return actor().whole_inventory;
        }

        protected override CommandUINode item(string str, int i)
        {
            var item_data = items(i);
            if (!is_valid_item(get_equipment(), i))
                return null;

            var text = new Inventory_Target_Item();
            text.set_image(actor(), item_data, Active_Item);
            var text_node = new ItemUINode("", text, this.column_width);
            text_node.loc = item_loc(i);
            return text_node;
        }

        #region Draw
        public override void draw_cursor(SpriteBatch sprite_batch)
        {
            Active_Item_Cursor.draw(sprite_batch, -(loc + text_draw_vector()));
            base.draw_cursor(sprite_batch);
        }
        #endregion
    }
}
