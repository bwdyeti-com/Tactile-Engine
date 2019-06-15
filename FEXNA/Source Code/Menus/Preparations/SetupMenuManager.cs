using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using FEXNA.Windows;
using FEXNA.Windows.Map;
using FEXNA.Windows.Map.Items;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Menus.Preparations
{
    abstract class SetupMenuManager<T>
        : InterfaceHandledMenuManager<T> where T : ISetupMenuHandler
    {
        public SetupMenuManager(T handler) : base(handler) { }

        public virtual void ResumeItemUse() { }

        #region Status
        // Open status menu opened from pick units/organize
        protected void overviewMenu_Status(object sender, EventArgs e)
        {
            var overviewMenu = (sender as Window_Prep_Unit_Overview);
            int actorId = overviewMenu.actor_id;

            var statusMenu = new Window_Status(Global.battalion.actors, actorId, true);
            statusMenu.Closed += statusMenu_Closed;
            AddMenu(statusMenu);
        }

        // Status menu canceled
        protected void statusMenu_Closed(object sender, EventArgs e)
        {
            var statusMenu = sender as Window_Status;
            int currentUnit = statusMenu.current_unit;
            int currentActor = statusMenu.current_actor;

            Global.game_temp.status_team = 0;
            statusMenu.close();
            RemoveTopMenu();

            if (Menus.Peek() is Window_Unit)
            {
                var unitMenu = Menus.Peek() as Window_Unit;
                unitMenu.unit_index = currentUnit;
            }
            else if (Menus.Peek() is PreparationsBaseMenu)
            {
                var itemMenu = Menus.Peek() as PreparationsBaseMenu;
                itemMenu.ActorId = currentActor;
            }
            else if (Menus.Peek() is Window_Prep_Items)
            {
                var itemMenu = Menus.Peek() as Window_Prep_Items;
                itemMenu.actor_id = currentActor;
            }
            else if (Menus.Peek() is Window_Prep_Unit_Overview)
            {
                var overviewMenu = Menus.Peek() as Window_Prep_Unit_Overview;
                overviewMenu.actor_id = currentActor;
            }
        }
        #endregion

        #region Unit
        // Open unit menu opened from pick units/organize
        protected void overviewMenu_Unit(object sender, EventArgs e)
        {
            var overviewMenu = (sender as Window_Prep_Unit_Overview);
            int actorId = overviewMenu.actor_id;

            var unitMenu = new Window_Unit();
            if (overviewMenu is Window_Prep_PickUnits)
                unitMenu.pickunits_window = overviewMenu as Window_Prep_PickUnits;
            unitMenu.ActorId = actorId;
            unitMenu.Status += unitMenu_Status;
            unitMenu.Closing += unitMenu_Closing;
            AddUnitMenuCancelEvent(unitMenu);
            AddMenu(unitMenu);
        }

        protected virtual void AddUnitMenuCancelEvent(Window_Unit unitMenu)
        {
            unitMenu.Closed += menu_Closed;
        }

        // Open status screen from unit menu
        protected void unitMenu_Status(object sender, EventArgs e)
        {
            Global.game_temp.menu_call = false;
            Global.game_temp.status_menu_call = false;

            var unitWindow = (sender as Window_Unit);
            List<int> team = unitWindow.team;
            int index = team.IndexOf(Global.game_temp.status_unit_id);
            index = Math.Max(0, index);

            var statusMenu = new Window_Status(team, index);
            statusMenu.Closed += statusMenu_Closed;
            AddMenu(statusMenu);
        }

        // Unit menu closing
        protected void unitMenu_Closing(object sender, EventArgs e)
        {
            var unitMenu = (sender as Window_Unit);
            int actorId = unitMenu.ActorId;

            if (actorId != -1 && Input.ControlScheme != ControlSchemes.Mouse)
            {
                var overviewMenu = (Menus.ElementAt(1) as Window_Prep_Unit_Overview);
                if (overviewMenu != null)
                {
                    overviewMenu.actor_id = actorId;
                }
            }
        }
        #endregion

        #region Items
        protected void AddItemMenu(bool returningToItemUse = false)
        {
            var itemsMenu = new Window_Prep_Items(returningToItemUse);
            itemsMenu.Status += itemsMenu_Status;
            itemsMenu.Trade += itemsMenu_Trade;
            itemsMenu.Convoy += itemsMenu_Convoy;
            itemsMenu.List += itemsMenu_List;
            itemsMenu.Shop += itemsMenu_Shop;
            itemsMenu.Closed += menu_Closed;
            AddMenu(itemsMenu);
        }

        // Open status screen from items menu
        protected void preparationsMenu_Status(object sender, EventArgs e)
        {
            Global.game_temp.menu_call = false;
            Global.game_temp.status_menu_call = false;

            int actorId;
            if (sender is Window_Prep_Items)
            {
                var itemsMenu = (sender as Window_Prep_Items);
                actorId = itemsMenu.actor_id;
            }
            else
            {
                var itemsMenu = (sender as PreparationsBaseMenu);
                actorId = itemsMenu.ActorId;
            }

            var statusMenu = new Window_Status(Global.battalion.actors, actorId, true);
            statusMenu.Closed += statusMenu_Closed;
            AddMenu(statusMenu);
        }

        // Open trade menu
        protected void itemsMenu_Trade(object sender, EventArgs e)
        {
            var itemsMenu = (sender as Window_Prep_Items);

            var tradeMenu = new PrepTradeMenu(
                itemsMenu.trading_actor_id, itemsMenu.actor_id);
            tradeMenu.Closing += tradeMenu_Closing;
            tradeMenu.Closed += menu_Closed;
            AddMenu(tradeMenu);
        }

        // Trade menu closing
        private void tradeMenu_Closing(object sender, EventArgs e)
        {
            var itemsMenu = (Menus.ElementAt(1) as Window_Prep_Items);
            itemsMenu.refresh_trade();
        }

        // Open convoy menu
        protected void itemsMenu_Convoy(object sender, EventArgs e)
        {
            var itemsMenu = (sender as Window_Prep_Items);

            if (Global.battalion.has_convoy)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                var convoyMenu = new Window_Supply(itemsMenu.actor_id);
                convoyMenu.Closing += convoyMenu_Closing;
                convoyMenu.Closed += menu_Closed;
                AddMenu(convoyMenu);
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        // Convoy menu closing
        private void convoyMenu_Closing(object sender, EventArgs e)
        {
            var itemsMenu = (Menus.ElementAt(1) as Window_Prep_Items);
            itemsMenu.refresh();
        }

        // Open list menu
        protected void itemsMenu_List(object sender, EventArgs e)
        {
            var itemsMenu = (sender as Window_Prep_Items);

            if (Global.battalion.actors.Count > 1 ||
                Global.battalion.has_convoy)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                var listMenu = new WindowItemList(itemsMenu.actor_id);
                listMenu.Closing += convoyMenu_Closing;
                listMenu.Closed += menu_Closed;
                AddMenu(listMenu);
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        // Open shop menu
        protected void itemsMenu_Shop(object sender, EventArgs e)
        {
            var itemsMenu = (sender as Window_Prep_Items);

            if (Global.battalion.has_convoy &&
                Global.game_battalions.active_convoy_shop != null)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Global.game_system.Shopper_Id = itemsMenu.actor_id;
                var shopMenu = new Window_Shop(
                    Global.game_system.Shopper_Id,
                    Global.game_battalions.active_convoy_shop);
                shopMenu.Shop_Close += shopMenu_Shop_Close;
                shopMenu.Closed += menu_Closed;
                AddMenu(shopMenu);
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        // Shop menu closing
        private void shopMenu_Shop_Close(object sender, EventArgs e)
        {
            var itemsMenu = (Menus.ElementAt(1) as Window_Prep_Items);
            itemsMenu.refresh();
        }
        #endregion

        #region Options
        // Open options menu
        protected void AddOptionsMenu(bool soloAnimAllowed = true)
        {
            var optionsMenu = new Window_Options(soloAnimAllowed);
            optionsMenu.SoloAnim += optionsMenu_SoloAnim;
            optionsMenu.Closed += optionsMenu_Closed;
            AddMenu(optionsMenu);
        }

        protected virtual void optionsMenu_Closed(object sender, EventArgs e)
        {
            menu_Closed(sender, e);
        }

        // Open unit screen from options to change solo anims
        private void optionsMenu_SoloAnim(object sender, EventArgs e)
        {
            var soloAnimMenu = new Window_SoloAnim();
            soloAnimMenu.Status += unitMenu_Status;
            soloAnimMenu.Closed += menu_Closed;
            AddMenu(soloAnimMenu);
        }
        #endregion

        #region Save
        // Open save menu
        protected void AddSaveMenu()
        {
            Global.game_temp.map_save_call = false;
            bool overwrite = Global.current_save_info.map_save_exists;

            Parchment_Confirm_Window saveConfirmWindow;
            if (overwrite)
            {
                saveConfirmWindow = new Parchment_Confirm_Window();
                saveConfirmWindow.set_text("Overwrite checkpoint?");
                saveConfirmWindow.add_choice("Yes", new Vector2(16, 16));
                saveConfirmWindow.add_choice("No", new Vector2(56, 16));
                saveConfirmWindow.size = new Vector2(112, 48);
            }
            else
            {
                saveConfirmWindow = new Parchment_Info_Window();
                saveConfirmWindow.set_text("Checkpoint Saved.");
                saveConfirmWindow.size = new Vector2(112, 32);
            }
            saveConfirmWindow.loc =
                new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2 -
                saveConfirmWindow.size / 2;

            var saveConfirmMenu = new ConfirmationMenu(saveConfirmWindow);
            if (overwrite)
            {
                saveConfirmMenu.Confirmed += saveConfirmMenu_OverwriteConfirmed;
                saveConfirmMenu.Canceled += menu_Closed;
            }
            else
            {
                saveConfirmMenu.Confirmed += saveConfirmMenu_Close;

                MenuHandler.SetupSave();
            }
            AddMenu(saveConfirmMenu);
        }

        // Save overwrite confirmed
        private void saveConfirmMenu_OverwriteConfirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            RemoveTopMenu();

            MenuHandler.SetupSave();
        }

        // Close save complete menu
        private void saveConfirmMenu_Close(object sender, EventArgs e)
        {
            RemoveTopMenu();

            Global.game_system.play_se(System_Sounds.Confirm);
        }
        #endregion

        protected void menu_Closed(object sender, EventArgs e)
        {
            RemoveTopMenu();
        }
    }

    interface ISetupMenuHandler : IMenuHandler
    {
        void SetupSave();
    }
}
