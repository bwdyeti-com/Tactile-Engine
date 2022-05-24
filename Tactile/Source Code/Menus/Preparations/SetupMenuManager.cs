using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Menus.Map.Unit.Item;
using Tactile.Menus.Options;
using Tactile.Windows;
using Tactile.Windows.Command;
using Tactile.Windows.Map;
using Tactile.Windows.Map.Items;
using Tactile.Windows.UserInterface.Command;
using TactileLibrary;

namespace Tactile.Menus.Preparations
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
                itemMenu.ActorId = currentActor;
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
            itemsMenu.UnitSelected += ItemsMenu_UnitSelected;
            itemsMenu.Status += preparationsMenu_Status;
            itemsMenu.TradeSelected += ItemsMenu_TradeSelected;
            itemsMenu.Trade += itemsMenu_Trade;
            itemsMenu.Convoy += itemsMenu_Convoy;
            itemsMenu.Use += ItemsMenu_Use;
            itemsMenu.List += itemsMenu_List;
            itemsMenu.Shop += itemsMenu_Shop;
            itemsMenu.Closed += menu_Closed;
            AddMenu(itemsMenu);

            if (returningToItemUse)
            {
                Global.game_map.completely_remove_unit(Global.game_map.last_added_unit.id);

                // Add unit menu
                AddItemCommandMenu(itemsMenu, null, true);

                // Add use menu
                if (itemsMenu.actor.has_items)
                {
                    RemoveTopMenu();
                    AddUseMenu(itemsMenu);
                }

                var fadeMenu = PromotionFadeMenu.PromotionEndFade();
                fadeMenu.Finished += menu_Closed;
                AddMenu(fadeMenu);
            }
        }

        // Select unit in items menu
        protected void ItemsMenu_UnitSelected(object sender, EventArgs e)
        {
            var itemsMenu = (sender as Window_Prep_Items);
            AddItemCommandMenu(itemsMenu);
        }

        private void AddItemCommandMenu(Window_Prep_Items itemsMenu, ItemUseMenu useMenu = null, bool toUseItem = false)
        {
            var itemCommandMenu = itemsMenu.GetCommandMenu();

            if (toUseItem)
                itemCommandMenu.ToUseItem();
            if (useMenu != null)
                itemCommandMenu.SetCursorLoc(useMenu.CurrentCursorLoc);

            itemCommandMenu.Selected += ItemCommandMenu_Selected;
            itemCommandMenu.Canceled += ItemCommandMenu_Canceled;
            AddMenu(itemCommandMenu);
        }

        private void ItemCommandMenu_Selected(object sender, EventArgs e)
        {
            var itemCommandMenu = (sender as ItemsCommandMenu);
            var itemsMenu = (Menus.ElementAt(1) as Window_Prep_Items);

            itemsMenu.CommandSelection(itemCommandMenu.SelectedIndex);
            
            itemCommandMenu.Refresh();
        }

        private void RefreshItemMenu()
        {
            var itemCommandMenu = (Menus.ElementAt(1) as ItemsCommandMenu);
            itemCommandMenu.Refresh();

            var itemsMenu = (Menus.ElementAt(2) as Window_Prep_Items);
            itemsMenu.refresh();
        }

        private void ItemCommandMenu_Canceled(object sender, EventArgs e)
        {
            menu_Closed(sender, e);

            var itemsMenu = (Menus.Peek() as Window_Prep_Items);
            itemsMenu.cancel_unit_selection();
        }

        // Open status screen from items menu
        protected void preparationsMenu_Status(object sender, EventArgs e)
        {
            Global.game_temp.menu_call = false;
            Global.game_temp.status_menu_call = false;

            if (!(sender is PreparationsBaseMenu))
                if (sender is Window_Prep_Items)
                {
                    throw new NotImplementedException();
                }
            var itemsMenu = (sender as PreparationsBaseMenu);
            int actorId = itemsMenu.ActorId;

            var statusMenu = new Window_Status(Global.battalion.actors, actorId, true);
            statusMenu.Closed += statusMenu_Closed;
            AddMenu(statusMenu);
        }

        // Select trade in items menu
        protected void ItemsMenu_TradeSelected(object sender, EventArgs e)
        {
            RemoveTopMenu();

            var itemsMenu = (Menus.Peek() as Window_Prep_Items);
            itemsMenu.trade();
        }

        // Open trade menu
        protected void itemsMenu_Trade(object sender, EventArgs e)
        {
            var itemsMenu = (sender as Window_Prep_Items);

            var tradeMenu = new PrepTradeMenu(
                itemsMenu.trading_actor_id, itemsMenu.ActorId);
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
                var convoyMenu = new Window_Supply(itemsMenu.ActorId);
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
            RefreshItemMenu();
        }

        // Open item use menu
        private void ItemsMenu_Use(object sender, EventArgs e)
        {
            var itemsMenu = (sender as Window_Prep_Items);

            CommandMenu itemCommandMenu = null;
            if (Menus.Peek() is CommandMenu)
            {
                itemCommandMenu = Menus.Peek() as CommandMenu;
                RemoveTopMenu();
            }

            AddUseMenu(itemsMenu, itemCommandMenu);
        }

        private void AddUseMenu(Window_Prep_Items itemsMenu, CommandMenu itemCommandMenu = null)
        {
            itemsMenu.StartUse();
            var useMenu = itemsMenu.GetUseMenu();

            if (itemCommandMenu != null)
                useMenu.SetCursorLoc(itemCommandMenu.CurrentCursorLoc);

            useMenu.Selected += UseMenu_Selected;
            useMenu.Canceled += UseMenu_Canceled;
            AddMenu(useMenu);
        }

        private void UseMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);

            var useMenu = (sender as ItemUseMenu);

            var actor = useMenu.Actor;
            int itemIndex = useMenu.SelectedItem;
            var itemData = actor.items[itemIndex];
            
            if (itemData.to_item.targets_inventory())
            {
                var repairWindow = useMenu.GetRepairWindow();
                var repairMenu = new ItemRepairMenu(
                    actor.id, itemIndex, repairWindow, useMenu);
                repairMenu.Selected += RepairMenu_Selected;
                repairMenu.Canceled += RepairMenu_Canceled;
                AddMenu(repairMenu);

                useMenu.HideWindow();
            }
            else
            {
                Vector2 cursorLoc = useMenu.CurrentCursorLoc;
                var useConfirmWindow = UseConfirmMenu(cursorLoc);

                var useConfirmMenu = new ConfirmationMenu(useConfirmWindow);
                useConfirmMenu.Confirmed += UseConfirmMenu_Confirmed;
                useConfirmMenu.Canceled += UseConfirmMenu_Canceled;
                AddMenu(useConfirmMenu);
            }
        }

        private void UseMenu_Canceled(object sender, EventArgs e)
        {
            ItemUseMenu useMenu = (sender as ItemUseMenu);

            menu_Closed(sender, e);
            var itemsMenu = (Menus.Peek() as Window_Prep_Items);
            itemsMenu.cancel_use();
            AddItemCommandMenu(itemsMenu, useMenu, true);
        }

        private void RepairMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            
            var repairMenu = (sender as ItemRepairMenu);
            
            Vector2 cursorLoc = repairMenu.CurrentCursorLoc;
            var useConfirmWindow = UseConfirmMenu(cursorLoc);

            var useConfirmMenu = new ConfirmationMenu(useConfirmWindow);
            useConfirmMenu.Confirmed += TargetedUseConfirmMenu_Confirmed;
            useConfirmMenu.Canceled += UseConfirmMenu_Canceled;
            AddMenu(useConfirmMenu);
        }
        private void RepairMenu_Canceled(object sender, EventArgs e)
        {
            var useConfirmMenu = (sender as ConfirmationMenu);
            var useMenu = (Menus.ElementAt(1) as ItemUseMenu);

            useMenu.ShowWindow();

            menu_Closed(sender, e);
        }

        private Preparations_Confirm_Window UseConfirmMenu(Vector2 cursorLoc)
        {
            var useConfirmWindow = new Preparations_Confirm_Window();
            useConfirmWindow.set_text("Will you really use it?");
            useConfirmWindow.add_choice("Yes", new Vector2(24, 12));
            useConfirmWindow.add_choice("No", new Vector2(64, 12));
            useConfirmWindow.size = new Vector2(136, 40);
            useConfirmWindow.loc = new Vector2(Config.WINDOW_WIDTH - 156, Config.WINDOW_HEIGHT - 60);
            useConfirmWindow.index = 1;
            useConfirmWindow.current_cursor_loc = cursorLoc;

            return useConfirmWindow;
        }

        private void TargetedUseConfirmMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);

            var useConfirmMenu = sender as ConfirmationMenu;
            var repairMenu = (Menus.ElementAt(1) as ItemRepairMenu);
            var useMenu = (Menus.ElementAt(2) as ItemUseMenu);

            UseItem(useConfirmMenu, useMenu, repairMenu);
        }
        private void UseConfirmMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);

            var useConfirmMenu = sender as ConfirmationMenu;
            var useMenu = (Menus.ElementAt(1) as ItemUseMenu);

            UseItem(useConfirmMenu, useMenu, null);
        }
        private void UseConfirmMenu_Canceled(object sender, EventArgs e)
        {
            var useConfirmMenu = (sender as ConfirmationMenu);
            var itemMenu = (Menus.ElementAt(1) as CommandMenu);

            itemMenu.SetCursorLoc(useConfirmMenu.CurrentCursorLoc);

            menu_Closed(sender, e);
        }

        private void UseItem(
            ConfirmationMenu useConfirmMenu,
            ItemUseMenu useMenu,
            ItemRepairMenu repairMenu)
        {
            Game_Actor actor = useMenu.Actor;
            Data_Item item = useMenu.SelectedItemData.to_item;

            // Use -1 if nothing
            int inventoryTarget = -1;
            if (repairMenu != null)
                inventoryTarget = repairMenu.SelectedItem;

            // If promoting
            if (actor.PromotedBy(item))
            {
                // If there are multiple promotion choices
                if (actor.PromotedBy(item) &&
                    actor.NeedsPromotionMenu)
                {
                    Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, actor.id, "");
                    var unit = Global.game_map.last_added_unit;

                    var promotionChoiceMenu = new PromotionChoiceMenu(unit);
                    promotionChoiceMenu.Selected += PromotionChoiceMenu_Selected;
                    promotionChoiceMenu.Canceled += PromotionChoiceMenu_Canceled;
                    promotionChoiceMenu.Closed += menu_Closed;
                    promotionChoiceMenu.Confirmed += PromotionChoiceMenu_Confirmed;
                    AddMenu(promotionChoiceMenu);

                    var promotionMenuFadeIn = promotionChoiceMenu.FadeInMenu(false);
                    promotionMenuFadeIn.Finished += menu_Closed;
                    AddMenu(promotionMenuFadeIn);
                }
                else
                {
                    Promote(useMenu);
                }
            }
            else
            {
                RemoveTopMenu(useConfirmMenu);
                if (repairMenu != null)
                    RemoveTopMenu(repairMenu);

                Global.game_system.play_se(System_Sounds.Gain);
                Dictionary<Boosts, int> boosts = actor.item_boosts(item);
                // Apply item effect
                actor.item_effect(item, inventoryTarget);
                actor.recover_hp();

                useMenu.UseItem();
                useMenu.ShowWindow();
                if (item.is_stat_booster() || item.is_growth_booster())
                {
                    useMenu.CreateStatsPopup(item, boosts);
                }
                else if (item.can_repair)
                {
                    useMenu.CreateRepairPopup(inventoryTarget);
                }
            }
        }

        private void Promote(ItemUseMenu useMenu, Maybe<int> promotionId = default(Maybe<int>))
        {
            Game_Actor actor = useMenu.Actor;
            int itemIndex = useMenu.SelectedItem;

            Global.game_system.Preparations_Actor_Id = actor.id;
            Global.game_temp.preparations_item_index = itemIndex;

            var promotionFadeMenu = useMenu.PromotionScreenFade(promotionId);
            promotionFadeMenu.Finished += PromotionFadeMenu_Finished;
            AddMenu(promotionFadeMenu);
        }

        private void PromotionFadeMenu_Finished(object sender, EventArgs e)
        {
            var promotionFadeMenu = (sender as PromotionFadeMenu);
            RemoveTopMenu();

            promotionFadeMenu.CallPromotion();

            // Clear menus down to items screen
            while (!(Menus.Peek() is Window_Prep_Items))
                RemoveTopMenu();
            
            // Black out the screen while promotion initializes
            var itemsMenu = (Menus.Peek() as Window_Prep_Items);
            itemsMenu.Promote();
        }

        private void PromotionChoiceMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Open);
            var promotionChoiceMenu = (sender as PromotionChoiceMenu);

            var promotionConfirmWindow = new Window_Command(
                promotionChoiceMenu.WindowLoc + new Vector2(64, 24),
                48,
                new List<string> { "Change", "Cancel" });
            promotionConfirmWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            promotionConfirmWindow.small_window = true;

            var promotionConfirmMenu = new CommandMenu(promotionConfirmWindow, promotionChoiceMenu);
            promotionConfirmMenu.Selected += PromotionConfirmMenu_Selected;
            promotionConfirmMenu.Canceled += menu_Closed;
            AddMenu(promotionConfirmMenu);
        }

        private void PromotionChoiceMenu_Canceled(object sender, EventArgs e)
        {
            var promotionChoiceMenu = (sender as PromotionChoiceMenu);
            var promotionMenuFadeOut = promotionChoiceMenu.FadeOutMenu();
            promotionMenuFadeOut.Finished += menu_Closed;
            AddMenu(promotionMenuFadeOut);
            // Remove the unit that was added for the promotion choice menu

            Global.game_map.completely_remove_unit(Global.game_map.last_added_unit.id);
        }

        private void PromotionConfirmMenu_Selected(object sender, EventArgs e)
        {
            var promotionConfirmMenu = (sender as CommandMenu);
            var selected = promotionConfirmMenu.SelectedIndex;
            menu_Closed(sender, e);

            switch (selected)
            {
                // Change
                case 0:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    var promotionChoiceMenu = (Menus.Peek() as PromotionChoiceMenu);
                    if (promotionChoiceMenu.AnimatedConfirm)
                        promotionChoiceMenu.AnimateConfirmation();
                    else
                        PromotionChoiceMenu_Confirmed(promotionChoiceMenu, e);
                    break;
                // Cancel
                case 1:
                    Global.game_system.play_se(System_Sounds.Cancel);
                    break;
            }
        }

        private void PromotionChoiceMenu_Confirmed(object sender, EventArgs e)
        {
            var promotionChoiceMenu = (sender as PromotionChoiceMenu);
            int promotion = promotionChoiceMenu.PromotionChoice;

            // Remove the unit that was added for the promotion choice menu
            Global.game_map.completely_remove_unit(Global.game_map.last_added_unit.id);

            var useMenu = (Menus.ElementAt(2) as ItemUseMenu);

            Promote(useMenu, promotion);
        }

        // Open list menu
        protected void itemsMenu_List(object sender, EventArgs e)
        {
            var itemsMenu = (sender as Window_Prep_Items);

            if (Global.battalion.actors.Count > 1 ||
                Global.battalion.has_convoy)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                var listMenu = new WindowItemList(itemsMenu.ActorId);
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
                Global.game_system.Shopper_Id = itemsMenu.ActorId;
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
            RefreshItemMenu();
        }
        #endregion

        #region Options
        // Open options menu
        protected void AddOptionsMenu(bool soloAnimAllowed = true)
        {
            var settingsTopMenu = new GameplaySettingsTopMenu(soloAnimAllowed);
            settingsTopMenu.Selected += SettingsTopMenu_Selected;
            settingsTopMenu.Closed += optionsMenu_Closed;
            settingsTopMenu.AddToManager(new MenuCallbackEventArgs(this.AddMenu, this.menu_Closed));
        }

        protected virtual void optionsMenu_Closed(object sender, EventArgs e)
        {
            menu_Closed(sender, e);
        }

        private void SettingsTopMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var settingsTopMenu = (sender as GameplaySettingsTopMenu);

            int index = settingsTopMenu.Index;
            switch (index)
            {
                case 0:
                    var optionsMenu = new Window_Options(settingsTopMenu.SoloAnimAllowed);
                    optionsMenu.SoloAnim += optionsMenu_SoloAnim;
                    optionsMenu.Closed += menu_Closed;
                    AddMenu(optionsMenu);
                    break;
                case 1:
                    OpenSettingsMenu(Global.gameSettings.General, settingsTopMenu);
                    break;
                case 2:
                    OpenSettingsMenu(Global.gameSettings.Graphics, settingsTopMenu);
                    break;
                case 3:
                    OpenSettingsMenu(Global.gameSettings.Audio, settingsTopMenu);
                    break;
                case 4:
                    OpenSettingsMenu(Global.gameSettings.Controls, settingsTopMenu);
                    break;
            }
        }

        private void OpenSettingsMenu(Tactile.Options.ISettings settings, IHasCancelButton parent)
        {
            SettingsMenu settingsMenu;
            settingsMenu = new SettingsMenu(settings, parent);
            settingsMenu.OpenSubMenu += SettingsMenu_OpenSubMenu;
            settingsMenu.OpenSettingList += SettingsMenu_OpenSettingList;
            settingsMenu.Canceled += settingsMenu_Canceled;
            AddMenu(settingsMenu);
        }

        private void SettingsMenu_OpenSubMenu(object sender, EventArgs e)
        {
            var parentMenu = (sender as SettingsMenu);
            var settings = parentMenu.GetSubSettings();
            OpenSettingsMenu(settings, parentMenu);
        }

        private void SettingsMenu_OpenSettingList(object sender, EventArgs e)
        {
            var parentMenu = (sender as SettingsMenu);
            var settingListWindow = parentMenu.GetSettingListWindow();

            var settingListCommandMenu = new CommandMenu(settingListWindow, parentMenu);
            settingListCommandMenu.Selected += SettingListCommandMenu_Selected;
            settingListCommandMenu.Canceled += SettingListCommandMenu_Canceled;
            AddMenu(settingListCommandMenu);
        }

        void SettingListCommandMenu_Selected(object sender, EventArgs e)
        {
            var settingListCommandMenu = sender as CommandMenu;
            var settingsMenu = (Menus.ElementAt(1) as SettingsMenu);

            int settingIndex = settingListCommandMenu.SelectedIndex.ValueOrDefault;
            if (settingsMenu.SelectSettingListItem(settingIndex))
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                RemoveTopMenu();
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        void SettingListCommandMenu_Canceled(object sender, EventArgs e)
        {
            RemoveTopMenu();
        }

        void settingsMenu_Canceled(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Cancel);
            MenuHandler.SetupSaveConfig();

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
        void SetupSaveConfig();
        void SetupSave();
    }
}
