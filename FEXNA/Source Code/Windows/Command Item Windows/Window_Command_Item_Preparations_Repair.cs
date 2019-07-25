using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Windows.Command.Items
{
    class Window_Command_Item_Preparations_Repair : Window_Command_Item_Preparations
    {
        private int Active_Item = -1;
        private Hand_Cursor Active_Item_Cursor;

        public Window_Command_Item_Preparations_Repair(int actor_id, Vector2 loc, bool facing_right, int active_item)
            : base(actor_id, loc, facing_right, true)
        {
            // Set items again, after setting the Active_Item
            Active_Item = active_item;
            add_commands(new List<string>());
            refresh_equipped_tag();

            this.immediate_index = active_item;

            Active_Item_Cursor = new Hand_Cursor();
            Active_Item_Cursor.tint = new Color(192, 192, 192, 255);
            Active_Item_Cursor.draw_offset = new Vector2(-16, 0);
            Active_Item_Cursor.force_loc(UICursor.target_loc);
        }

        protected override CommandUINode item(string str, int i)
        {
            var item_data = get_equipment()[i];
            if (!is_valid_item(get_equipment(), i))
                return null;

            var text = Using_Item ? new Inventory_Target_Item() : new Status_Item();
            if (Using_Item)
                (text as Inventory_Target_Item).set_image(actor(), item_data, Active_Item);
            else
                text.set_image(actor(), item_data);
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
