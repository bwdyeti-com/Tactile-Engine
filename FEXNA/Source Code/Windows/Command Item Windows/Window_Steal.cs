using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Windows.UserInterface.Command;
using FEXNA_Library;

namespace FEXNA.Windows.Command.Items
{
    class Window_Steal : Window_Command_Item
    {
        protected int Stealer_Id;

        #region Accessors
        protected Game_Unit stealer { get { return Global.game_map.units[Stealer_Id]; } }
        #endregion

        public Window_Steal(int stealer_id, int unit_id)
        {
            WIDTH = 136;
            Stealer_Id = stealer_id;
            Unit_Id = unit_id;
            initialize(new Vector2(152, 48), WIDTH, new List<string>());
        }

        protected override void item_initialize(Vector2 loc, int width, List<string> strs)
        {
            ItemInfo = new StealStatsPanel(actor());
            ItemInfo.loc = loc - new Vector2(136 - 40, 16);
        }

        protected override List<Item_Data> get_equipment()
        {
            return actor().items;
        }

        protected override CommandUINode item(string str, int i)
        {
            var item_data = get_equipment()[i];
            if (!is_valid_item(get_equipment(), i))
                return null;

            var text = new Steal_Item();
            (text as Steal_Item).set_image(stealer, unit, i);
            if (i >= Constants.Actor.NUM_ITEMS)
                text.change_text_color("Blue");

            var text_node = new ItemUINode("", text, this.column_width);
            text_node.loc = item_loc(i);
            return text_node;
        }

        protected override bool should_refresh_info()
        {
            return false;
        }

        public bool can_steal(int i)
        {
            return stealer.can_steal_item(unit, i);
        }

        public bool is_equipped(int i)
        {
            return unit.actor.equipped == i + 1;
        }

        #region Help
        public void equipped_steal_help()
        {
            no_steal();
            Help_Window.set_text("Equipped weapons, magic, and\nstaves can't be stolen.");
        }

        public void cant_steal_help()
        {
            no_steal();
            Help_Window.set_text("Stealing this item is\ntoo great a burden.");
        }

        protected void no_steal()
        {
            Movement_Locked = true;
            Help_Window = new Window_Help();
            Help_Window.loc = loc + new Vector2(0, 0 + redirect() * 16);
            update_help_loc();
            Global.Audio.play_se("System_Sounds", "Cancel");
            Global.game_system.play_se(System_Sounds.Help_Open);
        }

        public override void close_help()
        {
            Movement_Locked = false;
            Help_Window = null;
            Global.game_system.play_se(System_Sounds.Help_Close);
        }
        #endregion

        protected override void equip_actor() { }

        protected override bool show_equipped()
        {
            return false;
        }
    }
}
