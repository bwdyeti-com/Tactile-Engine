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
        protected bool Confirming = false;

        #region Accessors
        public bool confirming { get { return Confirming; } }

        public bool confirm_ready { get { return (Help_Window as Window_Confirmation).is_ready; } }

        public int confirm_index { get { return (Help_Window as Window_Confirmation).index; } }
        #endregion

        public Window_Command_Item_Discard(int unit_id, Vector2 loc)
        {
            Unit_Id = unit_id;
            initialize(loc, WIDTH, new List<string>());
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
            var item_data = get_equipment()[i];
            if (!is_valid_item(get_equipment(), i))
                return null;

            var text = new Status_Item();
            text.set_image(actor(), item_data);
            if (i >= Constants.Actor.NUM_ITEMS)
                text.change_text_color("Blue");

            var text_node = new ItemUINode("", text, this.column_width);
            text_node.loc = item_loc(i);
            return text_node;
        }

        protected override void update_input(bool input)
        {
            if (!Confirming)
                base.update_input(input);
        }

        protected override void update_ui(bool input)
        {
            base.update_ui(input && !confirming);

            if (input && confirming && confirm_ready)
            {
#if DEBUG
                throw new System.NotImplementedException();
#endif
            }
        }

        public bool confirm_is_selected()
        {
            return (Help_Window as Window_Confirmation).is_selected();
        }
        public bool confirm_is_canceled()
        {
            return (Help_Window as Window_Confirmation).is_canceled();
        }

        public void confirm()
        {
            Confirming = true;
            Window_Confirmation confirm_window = new Window_Confirmation();
            confirm_window.loc = loc + new Vector2(0, 24 + redirect() * 16);
            string text = string.Format("{0} a{1} {2}?\n",
                drop_text(), new char[] { 'A', 'E', 'I', 'O', 'U' }.Contains(current_item.Name[0]) ? "n" : "", current_item.Name);
            confirm_window.set_text(text);
            confirm_window.add_choice("Yes", new Vector2(16, 16));
            confirm_window.add_choice("No", new Vector2(56, 16));
            confirm_window.index = 1;
            Help_Window = confirm_window;
            update_help_loc();
        }

        protected virtual string drop_text()
        {
            return "Drop";
        }

        public void cancel()
        {
            Confirming = false;
            Help_Window = null;
        }

        protected override void update_help_loc()
        {
            Help_Window.set_loc(loc + new Vector2(Confirming ? 44 : 0, 8 + redirect() * 16));
        }
    }
}
