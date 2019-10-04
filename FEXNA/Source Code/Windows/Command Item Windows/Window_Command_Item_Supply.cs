using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Windows.Command.Items
{
    class Window_Command_Item_Supply : Window_Command_Item
    {
        protected bool Restocking;

        public Window_Command_Item_Supply(int actor_id, Vector2 loc, bool restocking)
        {
            Actor_Id = actor_id;
            Restocking = restocking;
            //Unit_Id = unit_id;
            active = false;
            initialize(loc, WIDTH, new List<string>());
            Window_Img.set_lines(Constants.Actor.NUM_ITEMS, (int)Size_Offset.Y);
        }

        protected override void item_initialize(Vector2 loc, int width, List<string> strs)
        {
        }

        protected override CommandUINode item(string str, int i)
        {
            var item_data = items(i);
            if (!is_valid_item(get_equipment(), i))
                return null;

            var text = Restocking ? new Restock_Item() : new Status_Item();
            text.set_image(actor(), item_data);
            var text_node = new ItemUINode("", text, this.column_width);
            text_node.loc = item_loc(i);
            return text_node;
        }

        public override void open_help()
        {
            base.open_help();
            Help_Window.set_screen_bottom_adjustment(-16);
            update_help_loc();
        }

        #region Draw
        protected override void draw_bar(SpriteBatch sprite_batch)
        {
            if (active)
                base.draw_bar(sprite_batch);
        }
        #endregion
    }
}

