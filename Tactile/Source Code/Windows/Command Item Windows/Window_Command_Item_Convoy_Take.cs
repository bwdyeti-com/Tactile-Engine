using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Windows.Command.Items
{
    class Window_Command_Item_Convoy_Take : Window_Command_Item
    {
        protected List<TactileLibrary.Item_Data> Items_Data;

        public Window_Command_Item_Convoy_Take(int unit_id, Vector2 loc)
        {
            Unit_Id = unit_id;
            initialize(loc, WIDTH, new List<string>());
        }

        protected override void item_initialize(Vector2 loc, int width, List<string> strs) { }

        /* //Debug
        protected override void set_items(List<string> strs)
        {
            List<string> item_names = new List<string>();
            Items = new List<Status_Item>();
            Text_Imgs = new List<Sprite>();

            Index_Redirect.Clear();
            set_items(get_equipment());
        }
        protected override void set_items(List<TactileLibrary.Item_Data> items)
        {
            if (items == null)
                return;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Id != 0)
                {
                    add_equipment(actor(), i, items[i]);
                }
            }
            Window_Img.set_lines(Items.Count, (int)Size_Offset.Y);

            refresh_item_stats();
        }*/

        public void set_item_data(List<TactileLibrary.Item_Data> items)
        {
            int index = this.index;
            Items_Data = items;
            refresh_items();
            immediate_index = index;
        }

        protected override List<TactileLibrary.Item_Data> get_equipment()
        {
            return Items_Data;
        }

        #region Draw
        protected override bool show_equipped()
        {
            return false;
        }

        protected override void draw_bar(SpriteBatch sprite_batch)
        {
            if (active)
                base.draw_bar(sprite_batch);
        }
        #endregion
    }
}
