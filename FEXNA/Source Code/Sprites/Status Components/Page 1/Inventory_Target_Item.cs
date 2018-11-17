using Microsoft.Xna.Framework.Graphics;
using FEXNA_Library;

namespace FEXNA
{
    class Inventory_Target_Item : Status_Item
    {
        public void set_image(Game_Actor actor, Item_Data item_data, int active_item)
        {
            base.set_image(actor, item_data);
            if (!item_data.non_equipment)
            {
                Data_Equipment item = item_data.to_equipment;

                set_text_color(actor.items[active_item].to_item.can_target_item(item_data));
            }
        }
    }
}
