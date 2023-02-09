using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Tactile.Map;
using Tactile.Menus.Map.Unit.Item;
using Tactile.Menus.Map.Unit.Target;
using Tactile.Windows.Command;
using Tactile.Windows.Command.Items;
using Tactile.Windows.Map.Items;
using Tactile.Windows.Target;
using Tactile.Windows.UserInterface.Command;
using TactileLibrary;
using EnumExtension;

namespace Tactile.Menus.Map.Unit
{
    partial class UnitMenuManager : InterfaceHandledMenuManager<IUnitMenuHandler>
    {
        private Maybe<int> TradeInitialIndex = Maybe<int>.Nothing;
        private Maybe<int> DancerRingIndex = Maybe<int>.Nothing;

        // Make sure system menu canto is only ever used when the menu is
        // closing before planning to reopen after completing an action;
        // this ensures the value doesn't need to be reset

        private Dictionary<int, Action<Game_Unit>> SimpleCommands;
        private Dictionary<int, Action<Game_Unit, UnitCommandMenu>> TargetCommands;

        protected UnitMenuManager(IUnitMenuHandler handler)
            : base(handler) { }

        public static UnitMenuManager CommandMenu(IUnitMenuHandler handler)
        {
            var manager = new UnitMenuManager(handler);
            manager.SetCommands();

            var unitMenu = new UnitCommandMenu(Global.game_system.Selected_Unit_Id);
            unitMenu.IndexChanged += manager.unitMenu_IndexChanged;
            unitMenu.Selected += manager.unitMenu_Selected;
            unitMenu.Canceled += manager.unitMenu_Canceled;
            manager.AddMenu(unitMenu);

            return manager;
        }

        public static UnitMenuManager StatusScreen(IUnitMenuHandler handler)
        {
            var manager = new UnitMenuManager(handler);
            
            // Add status menu
            Global.game_temp.menuing = true;
            Global.game_temp.menu_call = false;
            Global.game_temp.status_menu_call = false;

            List<int> team = new List<int>();
            if (Global.game_map.preparations_unit_team != null)
                team.AddRange(Global.game_map.preparations_unit_team);
            else
            {
#if DEBUG
                if (Global.scene.scene_type == "Scene_Map_Unit_Editor")
                    team.AddRange(Global.game_map.teams[Global.game_temp.status_team]);
                else
#endif
                    // Only list units that are on the map or rescued (rescued units can be off map)
                    team.AddRange(Global.game_map.teams[Global.game_temp.status_team]
                        .Where(x => x == Global.game_temp.status_unit_id ||
                            !Global.game_map.is_off_map(Global.game_map.units[x].loc) || Global.game_map.units[x].is_rescued));
            }
            int id = 0;
            for (int i = 0; i < team.Count; i++)
            {
                int unit_id = team[i];
                if (Global.game_temp.status_unit_id == unit_id)
                {
                    id = i;
                    break;
                }
            }
            
            var statusMenu = new Window_Status(team, id);
            statusMenu.Closed += manager.statusScreen_Closed;
            manager.AddMenu(statusMenu);

            return manager;
        }

        public static UnitMenuManager Discard(IUnitMenuHandler handler)
        {
            var manager = new UnitMenuManager(handler);

            int unitId = Global.game_system.Discarder_Id;
            Global.game_temp.menuing = true;
            Global.game_temp.discard_menuing = true;
            Global.game_temp.menu_call = false;
            Global.game_temp.discard_menu_call = false;

            Window_Command_Item_Discard discardWindow;
            if (Global.battalion.convoy_ready_for_sending)
                discardWindow = new Window_Command_Item_Send(unitId, new Vector2(24, 8));
            else
                discardWindow = new Window_Command_Item_Discard(unitId, new Vector2(24, 8));
            discardWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            discardWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            discardWindow.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;

            var discardMenu = new DiscardMenu(discardWindow);
            discardMenu.Selected += manager.discardMenu_Selected;
            manager.AddMenu(discardMenu);

            return manager;
        }
        public static UnitMenuManager ReopenDiscard(IUnitMenuHandler handler)
        {
            var manager = Discard(handler);

            var discardMenu = (manager.Menus.ElementAt(0) as DiscardMenu);
            discardMenu.WaitForPopup();

            return manager;
        }

        public static UnitMenuManager DialoguePrompt(
            IUnitMenuHandler handler,
            int variableId,
            List<string> dialogueChoices)
        {
            var manager = new UnitMenuManager(handler);

            Global.game_temp.menuing = true;
            Global.game_temp.prompt_menuing = true;
            Global.game_temp.menu_call = false;

            const int PROMPT_ROWS = 6;
            int width = dialogueChoices.Max(x => Font_Data.text_width(x, Config.UI_FONT));
            width = Math.Max(width, 48);
            width = width + (width % 8 == 0 ? 0 : (8 - width % 8)) + 32;
            int height = PROMPT_ROWS * Font_Data.Data[Config.UI_FONT].CharHeight + 16;

            var dialoguePromptWindow = new Window_Command_Scrollbar(
                new Vector2(
                    (Config.WINDOW_WIDTH - width) / 2,
                    (Config.WINDOW_HEIGHT - height) / 2),
                width,
                PROMPT_ROWS,
                dialogueChoices);
            dialoguePromptWindow.text_offset = new Vector2(8, 0);
            dialoguePromptWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            dialoguePromptWindow.small_window = true;

            var dialoguePromptMenu = new DialoguePromptMenu(dialoguePromptWindow, variableId);
            dialoguePromptMenu.Selected += manager.dialoguePromptMenu_Selected;
            manager.AddMenu(dialoguePromptMenu);

            return manager;
        }

        public static UnitMenuManager ConfirmationPrompt(
            IUnitMenuHandler handler,
            int switchId,
            string caption)
        {
            var manager = new UnitMenuManager(handler);

            Global.game_temp.menuing = true;
            Global.game_temp.prompt_menuing = true;
            Global.game_temp.menu_call = false;

            var confirmationPromptWindow = new Parchment_Confirm_Window();
            confirmationPromptWindow.set_text(caption);
            confirmationPromptWindow.add_choice("Yes", new Vector2(16, 16));
            confirmationPromptWindow.add_choice("No", new Vector2(56, 16));
            confirmationPromptWindow.size = new Vector2(
                confirmationPromptWindow.size.X,
                caption.Split(new char[] { '\n' }, StringSplitOptions.None).Count() * 16 + 32);
            confirmationPromptWindow.index = 0;
            confirmationPromptWindow.loc = new Vector2(
                (Config.WINDOW_WIDTH - confirmationPromptWindow.size.X) / 2,
                (Config.WINDOW_HEIGHT - confirmationPromptWindow.size.Y) / 2);

            var confirmationPromptMenu = new ConfirmationPromptMenu(confirmationPromptWindow, switchId);
            confirmationPromptMenu.Confirmed += manager.confirmationPromptMenu_Confirmed;
            confirmationPromptMenu.Canceled += manager.confirmationPromptMenu_Canceled;
            manager.AddMenu(confirmationPromptMenu);

            return manager;
        }

        public static UnitMenuManager ResumeArena(IUnitMenuHandler handler)
        {
            var manager = new UnitMenuManager(handler);

            // Add arena menu
            Global.game_temp.menuing = true;
            Global.game_temp.menu_call = false;
            Global.game_temp.reset_shop_call();

            Global.game_system.Shopper_Id = Global.game_system.Battler_1_Id;

            Window_Arena shopMenu = new Window_Arena(
                Global.game_system.Shopper_Id, Global.game_map.get_shop(), true);
            shopMenu.Closed += manager.arenaMenu_Closed;
            manager.AddMenu(shopMenu);

            return manager;
        }

        public static UnitMenuManager PreviewShop(IUnitMenuHandler handler, Shop_Data shop)
        {
            var manager = new UnitMenuManager(handler);

            // Add shop menu
            Global.game_temp.menuing = true;
            Global.game_temp.menu_call = false;
            Global.game_system.SecretShop = false;
            Global.game_temp.reset_shop_call();
            
            Window_Business shopMenu;
            if (shop.arena)
                // Arenas shouldn't be previewed, but //@Debug
                shopMenu = new Window_Arena(-1, shop, false);
            else
            {
                shopMenu = new Window_Shop(-1, shop);
            }
            shopMenu.Closed += manager.shopPreviewMenu_Closed;
            manager.AddMenu(shopMenu);
            
            return manager;
        }

        public static UnitMenuManager PromotionChoice(
            IUnitMenuHandler handler,
            int unitId,
            bool skipFadeIn = false,
            bool animateConfirm = false)
        {
            var manager = new UnitMenuManager(handler);

            // Add promotion choice menu
            Global.game_temp.menuing = true;
            Global.game_temp.ResetPromotionChoiceCall();

            var unit = Global.game_map.units[unitId];
            manager.AddPromotionChoiceMenu(unit, true, skipFadeIn, animateConfirm);

            return manager;
        }

        private void SetCommands()
        {
            SimpleCommands = new Dictionary<int, Action<Game_Unit>>();
            // 0: Attack
            SimpleCommands.Add(0, (Game_Unit unit) => Attack(unit));
            // 1: Staff
            SimpleCommands.Add(1, (Game_Unit unit) => Staff(unit));
            // 3: Item
            SimpleCommands.Add(3, (Game_Unit unit) => Item(unit));
            // 5: Wait
            SimpleCommands.Add(5, (Game_Unit unit) => Wait(unit));
            // 7, 11: Visit, Chest
            SimpleCommands.Add(7, (Game_Unit unit) => Visit(unit, State.Visit_Modes.Visit));
            SimpleCommands.Add(11, (Game_Unit unit) => Visit(unit, State.Visit_Modes.Chest));
            // 9, 10, 29, 30: Shop, Arena (Secret Shop, Secret Arena)
            SimpleCommands.Add(9, (Game_Unit unit) => Shop(unit, false));
            SimpleCommands.Add(10, (Game_Unit unit) => Shop(unit, false));
            SimpleCommands.Add(29, (Game_Unit unit) => Shop(unit, true));
            SimpleCommands.Add(30, (Game_Unit unit) => Shop(unit, true));
            // 14: Seize
            SimpleCommands.Add(14, (Game_Unit unit) => Seize(unit));
            // 15: Status
            SimpleCommands.Add(15, (Game_Unit unit) => Status(unit));
            // 16: Dance
            SimpleCommands.Add(16, (Game_Unit unit) => Dance(unit));
            // 18: Supply
            SimpleCommands.Add(18, (Game_Unit unit) => Supply(unit));
            // 19: Escape
            SimpleCommands.Add(19, (Game_Unit unit) => Escape(unit));
            // 20: Construct
            SimpleCommands.Add(20, (Game_Unit unit) => Construct(unit));

            TargetCommands = new Dictionary<int, Action<Game_Unit, UnitCommandMenu>>();
            // 2: Rescue
            TargetCommands.Add(2, (Game_Unit unit, UnitCommandMenu menu) => Rescue(unit, menu));
            // 4: Trade
            TargetCommands.Add(4, (Game_Unit unit, UnitCommandMenu menu) => Trade(unit, menu));
            // 6: Take
            TargetCommands.Add(6, (Game_Unit unit, UnitCommandMenu menu) => Take(unit, menu));
            // 8: Talk
            TargetCommands.Add(8, (Game_Unit unit, UnitCommandMenu menu) => Talk(unit, menu));
            // 12: Door
            TargetCommands.Add(12, (Game_Unit unit, UnitCommandMenu menu) => Door(unit, menu));
            // 13: Steal
            TargetCommands.Add(13, (Game_Unit unit, UnitCommandMenu menu) => Steal(unit, menu));
            // 17: Support
            TargetCommands.Add(17, (Game_Unit unit, UnitCommandMenu menu) => Support(unit, menu));

            AddSkillCommands();
        }

        private void unitMenu_IndexChanged(object sender, EventArgs e)
        {
            Global.game_map.range_start_timer = 0;
        }

        // Selected an item in the unit menu
        private void unitMenu_Selected(object sender, EventArgs e)
        {
            var unitMenu = (sender as UnitCommandMenu);
            Game_Unit unit = Global.game_map.units[unitMenu.UnitId];
            UnitMenuSelect(unitMenu.SelectedCommand, unitMenu, unit);
        }

        private void unitMenu_Canceled(object sender, EventArgs e)
        {
            var unitMenu = (sender as UnitCommandMenu);
            Canto_Records canto = unitMenu.Canto;

            if (CancelCommandSkillCommand(unitMenu))
            {
                return;
            }

            // If horse canto, cancels to wait only option
            if (canto != Canto_Records.Horse && canto.HasEnumFlag(Canto_Records.Horse))
                canto = Canto_Records.Horse;

            // If not in a foot unit canto
            if (canto == Canto_Records.None || canto == Canto_Records.Horse)
            {
                menu_ClosedCanceled(unitMenu, new EventArgs());

                return;
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        private void UnitMenuSelect(int selectedOption, UnitCommandMenu unitMenu, Game_Unit unit)
        {
            // If the player selects a menu option other than the
            // context sensitive one, reset context sensitive values
            if (Global.game_temp.SelectedMoveMenuChoice.IsSomething &&
                    Global.game_temp.SelectedMoveMenuChoice != selectedOption)
                Global.game_temp.ResetContextSensitiveUnitMenu();

            if (SimpleCommands.ContainsKey(selectedOption))
                SimpleCommands[selectedOption](unit);
            else if (TargetCommands.ContainsKey(selectedOption))
                TargetCommands[selectedOption](unit, unitMenu);
            else
            {
                switch (selectedOption)
                {
                    default:
                        if (UnitMenuSelectSkill(selectedOption, unitMenu, unit))
                            return;

                        Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                }
            }
        }

        #region Commands
        #region 0: Attack
        private void Attack(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var unitMenu = (Menus.Peek() as UnitCommandMenu);
            Global.game_map.range_start_timer = 0;

            var attackItemWindow = new Window_Command_Item_Attack(unit.id, new Vector2(24, 8));
            // Automatically select a specific weapon
            if (Global.game_temp.SelectedMoveAttackItemIndex.IsSomething)
            {
                int index = attackItemWindow.GetRedirect(
                    Global.game_temp.SelectedMoveAttackItemIndex);
                if (index != -1)
                    attackItemWindow.immediate_index = index;
            }
            attackItemWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            attackItemWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            attackItemWindow.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
            unitMenu.RefreshTempAttackRange(attackItemWindow.redirect());

            var attackMenu = new AttackItemMenu(attackItemWindow, unitMenu);
            attackMenu.IndexChanged += attackMenu_IndexChanged;
            attackMenu.Selected += attackMenu_Selected;
            attackMenu.Canceled += attackMenu_Canceled;
            AddMenu(attackMenu);
        }

        private void attackMenu_IndexChanged(object sender, EventArgs e)
        {
            var attackMenu = (sender as AttackItemMenu);
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);
            Game_Unit unit = attackMenu.Unit;

            Global.game_map.range_start_timer = 0;
            unitMenu.RefreshTempAttackRange(attackMenu.SelectedItem, attackMenu.Skill);
        }

        private void attackMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var attackMenu = (sender as AttackItemMenu);
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);
            
            Game_Unit unit = attackMenu.Unit;
            unit.equip(attackMenu.SelectedItem + 1);
            unit.actor.organize_items();

            // A weapon was selected and probably moved to the top of
            // the list, so reset this
            Global.game_temp.ResetContextSensitiveSelectedItem();

            unit.using_siege_engine = attackMenu.SelectedItem ==
                Siege_Engine.SiegeInventoryIndex;
            int item_index = unit.using_siege_engine ? attackMenu.SelectedItem : 0;

            // Automatically select initial target location
            Maybe<Vector2> attackLoc = Global.game_temp.SelectedAttackLoc;

            Window_Target_Combat targetWindow;
            if (Global.game_options.combat_window == 1)
                targetWindow = new Window_Target_Combat_Detail(
                    unit.id, item_index, new Vector2(4, 4),
                    attackMenu.Skill,
                    attackLoc);
            else
                targetWindow = new Window_Target_Combat(
                    unit.id, item_index, new Vector2(4, 4),
                    attackMenu.Skill,
                    attackLoc);
            var targetMenu = new UnitTargetMenu(targetWindow, unitMenu);
            targetMenu.Selected += attackTargetMenu_Selected;
            targetMenu.Canceled += attackStaffTargetMenu_Canceled;
            AddMenu(targetMenu);

            Global.player.facing = 4;
            Global.player.update_cursor_frame();
        }

        private void attackMenu_Canceled(object sender, EventArgs e)
        {
            var attackMenu = (sender as AttackItemMenu);
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);
            Game_Unit unit = attackMenu.Unit;

            CancelAttackSkills(unit);

            unitMenu.RefreshTempAttackRange();

            menu_Closed(sender, e);
        }
        private void CancelAttackSkills(Game_Unit unit)
        {
            // Skills: Swoop
            unit.swoop_activated = false;
            // Skills: Trample
            unit.trample_activated = false;
            // Skills: Old Swoop //@Debug
            unit.old_swoop_activated = false;
            // Skills: Masteries
            unit.reset_masteries();
        }

        private void attackTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as UnitTargetMenu);
            var attackMenu = (Menus.ElementAt(1) as AttackItemMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
            MenuHandler.UnitMenuAttack(unit, targetMenu.SelectedUnitId);
        }

        private void attackStaffTargetMenu_Canceled(object sender, EventArgs e)
        {
            var attackMenu = (Menus.ElementAt(1) as ItemMenu);
            Game_Unit unit = attackMenu.Unit;

            unitTargetMenu_Canceled(sender, e);

            unit.using_siege_engine = false;
            attackMenu.Reset();
            CancelAttackTargetSkills(unit);
        }
        private void CancelAttackTargetSkills(Game_Unit unit)
        {
            // Skills: Swoop
            if (unit.swoop_activated)
            {
                Global.game_temp.temp_skill_ranges["SWOOP"] = unit.swoop_range();
            }
            // Skills: Trample
            else if (unit.trample_activated)
            {
                Global.game_temp.temp_skill_ranges["TRAMPLE"] = unit.trample_range();
                Global.game_temp.temp_skill_move_ranges["TRAMPLE"] = unit.trample_move_range();
            }
            // Skills: Old Swoop //Debug
            else if (unit.old_swoop_activated)
            {
                Global.game_temp.temp_skill_ranges["OLDSWOOP"] = unit.old_swoop_range();
            }
        }
        #endregion

        #region 1: Staff
        private void Staff(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var unitMenu = (Menus.Peek() as UnitCommandMenu);
            Global.game_map.range_start_timer = 0;

            var staffItemWindow = new Window_Command_Item_Staff(unit.id, new Vector2(24, 8));
            staffItemWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            staffItemWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            staffItemWindow.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
            unitMenu.RefreshTempStaffRange(staffItemWindow.redirect());

            var staffMenu = new ItemMenu(staffItemWindow, unitMenu);
            staffMenu.IndexChanged += staffMenu_IndexChanged;
            staffMenu.Selected += staffMenu_Selected;
            staffMenu.Canceled += staffMenu_Canceled;
            AddMenu(staffMenu);
        }


        private void staffMenu_IndexChanged(object sender, EventArgs e)
        {
            var staffMenu = (sender as ItemMenu);
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);
            Game_Unit unit = staffMenu.Unit;

            Global.game_map.range_start_timer = 0;
            unitMenu.RefreshTempStaffRange(staffMenu.SelectedItem);
        }

        private void staffMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var staffMenu = (sender as ItemMenu);
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);

            Game_Unit unit = staffMenu.Unit;
            unit.equip(staffMenu.SelectedItem + 1);
            unit.actor.organize_items();
            
            // Unlikely, but in case
            unit.using_siege_engine = staffMenu.SelectedItem ==
                Siege_Engine.SiegeInventoryIndex;
            int item_index = unit.using_siege_engine ? staffMenu.SelectedItem : 0;
            
            var targetWindow = new Window_Target_Staff(
                unit.id, item_index, new Vector2(0, 0));
            var targetMenu = new UnitTargetMenu(targetWindow, unitMenu);
            targetMenu.Selected += staffTargetMenu_Selected;
            targetMenu.Canceled += attackStaffTargetMenu_Canceled;
            AddMenu(targetMenu);

            Global.player.facing = 4;
            Global.player.update_cursor_frame();
        }

        private void staffMenu_Canceled(object sender, EventArgs e)
        {
            var staffMenu = (sender as ItemMenu);
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);
            Game_Unit unit = staffMenu.Unit;

            CancelStaffSkills(unit);
            if (!unit.actor.staff_fix())
                unit.actor.organize_items();

            unitMenu.RefreshTempStaffRange();

            menu_Closed(sender, e);
        }
        private void CancelStaffSkills(Game_Unit unit)
        {
        }

        private void staffTargetMenu_Selected(object sender, EventArgs e)
        {
            if (this.ManualTargeting &&
                    !Global.game_temp.temp_staff_range.Contains(Global.player.loc))
                Global.game_system.play_se(System_Sounds.Buzzer);
            else
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                var targetMenu = (sender as UnitTargetMenu);
                var attackMenu = (Menus.ElementAt(1) as ItemMenu);
                targetMenu.Accept();

                Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
                int targetId = -1;
                Vector2 targetLoc;
                if (this.ManualTargeting)
                {
                    targetLoc = Global.player.loc;
                }
                else
                {
                    targetId = targetMenu.SelectedUnitId;
                    targetLoc = Global.game_map.attackable_map_object(targetId).loc;
                }
                MenuHandler.UnitMenuStaff(unit, targetId, targetLoc);
            }
        }
        #endregion

        #region 2: Rescue/Drop
        private void Rescue(Game_Unit unit, UnitCommandMenu unitMenu)
        {
            if (!unit.is_rescuing && unit.is_rescue_blocked())
                Global.game_system.play_se(System_Sounds.Buzzer);
            else
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                var targetWindow = new Window_Target_Rescue(
                    unit.id, unit.is_rescuing ? 1 : 0, new Vector2(4, 0));
                var targetMenu = new UnitTargetMenu(targetWindow, unitMenu);
                // Drop
                if (unit.is_rescuing)
                    targetMenu.Selected += dropTargetMenu_Selected;
                // Rescue
                else
                    targetMenu.Selected += rescueTargetMenu_Selected;
                targetMenu.Canceled += unitTargetMenu_Canceled;
                AddMenu(targetMenu);

                Global.player.facing = 4;
                Global.player.update_cursor_frame();
            }
        }

        private void rescueTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as UnitTargetMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
            MenuHandler.UnitMenuRescue(unit, targetMenu.SelectedUnitId);
        }
        private void dropTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as UnitTargetMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
            int val = targetMenu.SelectedUnitId;
            Vector2 dropLocation = new Vector2(val % Global.game_map.width,
                val / Global.game_map.width);

            MenuHandler.UnitMenuDrop(unit, dropLocation);
        }
        #endregion

        #region 3: Item
        private void Item(Game_Unit unit)
        {
            var unitMenu = (Menus.Peek() as UnitCommandMenu);

            Global.game_system.play_se(System_Sounds.Confirm);
            Global.game_map.range_start_timer = 0;
            OpenItemMenu(unit, unitMenu);
        }

        private void OpenItemMenu(Game_Unit unit, UnitCommandMenu unitMenu)
        {
            var itemWindow = new Window_Command_Item(unit.id, new Vector2(24, 8));
            itemWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            itemWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            itemWindow.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;

            var itemMenu = new ItemMenu(itemWindow, unitMenu);
            itemMenu.Selected += itemMenu_Selected;
            itemMenu.Canceled += menu_Closed;
            AddMenu(itemMenu);
        }

        private void itemMenu_Selected(object sender, EventArgs e)
        {
            var itemMenu = (sender as ItemMenu);
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);

            if (itemMenu.SelectedItem == Siege_Engine.SiegeInventoryIndex)
                Global.game_system.play_se(System_Sounds.Buzzer);
            else
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Game_Unit unit = itemMenu.Unit;
                bool canTrade = unitMenu.CantoAllowsTrade();
                if (canTrade)
                    canTrade &= unit.can_trade;

                var itemOptionsWindow = new Window_Item_Options(
                    unit,
                    canTrade,
                    new Vector2(104 + 24, 32 + 16 * itemMenu.SelectedItem),
                    itemMenu.SelectedItem,
                    itemMenu.EquippedSelected);
                itemOptionsWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                itemOptionsWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                itemOptionsWindow.index = 0;

                var itemOptionsMenu = new ItemOptionsMenu(itemOptionsWindow, itemMenu);
                itemOptionsMenu.Selected += itemOptionsMenu_Selected;
                itemOptionsMenu.Canceled += menu_Closed;
                AddMenu(itemOptionsMenu);
            }
        }

        private void itemOptionsMenu_Selected(object sender, EventArgs e)
        {
            var itemOptionsMenu = (sender as ItemOptionsMenu);
            var itemMenu = (Menus.ElementAt(1) as ItemMenu);

            var unit = itemMenu.Unit;
            int itemIndex = itemMenu.SelectedItem;
            // No guarantee that doing something with items isn't changing some unit's move range in some way
            Global.game_map.remove_updated_move_range(unit.id);
            switch (itemOptionsMenu.SelectedOption)
            {
                // Equip
                case 0:
                    if (unit.actor.is_equippable(Global.data_weapons[unit.actor.items[itemIndex].Id]))
                    {
                        if (itemOptionsMenu.Unequips)
                        {
                            Global.game_system.play_se(System_Sounds.Cancel);
                            unit.actor.unequip();
                        }
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Open);
                            unit.equip(itemIndex + 1);
                        }
                        unit.actor.organize_items();
                        itemMenu.RefreshInventory();

                        menu_Closed(itemOptionsMenu, e);
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer); // should pop up a help window //@Yeti
                    break;
                // Use
                case 1:
                    var itemData = unit.actor.items[itemIndex];
                    if (Combat.can_use_item(unit, itemData.Id))
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        itemMenu.RestoreEquipped();
                        if (unit.actor.PromotedBy(itemData.to_item) &&
                            unit.actor.NeedsPromotionMenu)
                        {
                            AddPromotionChoiceMenu(unit);
                        }
                        else
                        {
                            UseItem(itemMenu, itemOptionsMenu);
                        }
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                // Trade
                case 2:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    itemMenu.RestoreEquipped();
                    // Close item menus
                    menu_Closed(sender, e);
                    RemoveTopMenu();

                    var unitMenu = (Menus.Peek() as UnitCommandMenu);
                    OpenTradeTargetMenu(unit, unitMenu, itemIndex);
                    break;
                // Discard
                case 3:
                    if (itemOptionsMenu.CanDiscard)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        var discardConfirmWindow = new Window_Command(
                            itemOptionsMenu.WindowLoc +
                                new Vector2(32, 8 + itemOptionsMenu.Index * 16),
                            40,
                            new List<string> { "Yes", "No" });
                        discardConfirmWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                        discardConfirmWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                        discardConfirmWindow.small_window = true;
                        discardConfirmWindow.immediate_index = 1;

                        var discardConfirmMenu = new CommandMenu(discardConfirmWindow, itemMenu);
                        discardConfirmMenu.Selected += discardConfirmMenu_Selected;
                        discardConfirmMenu.Canceled += menu_Closed;
                        AddMenu(discardConfirmMenu);
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
            }
        }

        private void UseItem(ItemMenu itemMenu, ItemOptionsMenu itemOptionsMenu = null)
        {
            var unit = itemMenu.Unit;
            int itemIndex = itemMenu.SelectedItem;
            var itemData = unit.actor.items[itemIndex];

            if (itemData.to_item.targets_inventory())
            {
                // Close item options
                if (itemOptionsMenu != null)
                    menu_Closed(itemOptionsMenu, new EventArgs());

                Global.game_map.range_start_timer = 0;
                var repairItemWindow = new Window_Command_Item_Target_Inventory(
                    unit.id,
                    new Vector2(24, 8),
                    itemIndex);
                repairItemWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                repairItemWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                repairItemWindow.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;

                var repairMenu = new ItemMenu(repairItemWindow, itemMenu);
                repairMenu.Selected += repairMenu_Selected;
                repairMenu.Canceled += menu_Closed;
                AddMenu(repairMenu);
            }
            else if (itemData.to_item.is_placeable())
            {
                // Close item options
                if (itemOptionsMenu != null)
                    menu_Closed(itemOptionsMenu, new EventArgs());

                var targetWindow = new Window_Target_Placeable(
                    unit.id, new Vector2(4, 0));
                var targetMenu = new LocationTargetMenu(targetWindow, itemMenu);
                targetMenu.Selected += placeableTargetMenu_Selected;
                targetMenu.Canceled += locationTargetMenu_Canceled;
                AddMenu(targetMenu);

                Global.player.facing = 4;
                Global.player.update_cursor_frame();
            }
            else
            {
                MenuHandler.UnitMenuUseItem(unit, itemIndex, Maybe<Vector2>.Nothing, Maybe<int>.Nothing);
            }
        }

        private void AddPromotionChoiceMenu(
            Game_Unit unit,
            bool standaloneMenu = false,
            bool skipFadeIn = false,
            bool animateConfirm = false)
        {
            var promotionChoiceMenu = new PromotionChoiceMenu(unit, standaloneMenu, animateConfirm);
            promotionChoiceMenu.Selected += PromotionChoiceMenu_Selected;
            promotionChoiceMenu.Canceled += PromotionChoiceMenu_Canceled;
            promotionChoiceMenu.Closed += menu_Closed;
            promotionChoiceMenu.Confirmed += PromotionChoiceMenu_Confirmed;
            AddMenu(promotionChoiceMenu);

            if (skipFadeIn)
            {
                promotionChoiceMenu.FadeShow();
            }
            else
            {
                var promotionMenuFadeIn = promotionChoiceMenu.FadeInMenu(false);
                promotionMenuFadeIn.Finished += menu_Closed;
                AddMenu(promotionMenuFadeIn);
            }

            //@Debug: Undecided if this looks better before
            // or after the fade out
            Global.game_map.HideUnits();
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
        }

        private void PromotionConfirmMenu_Selected(object sender, EventArgs e)
        {
            var promotionConfirmMenu = (sender as CommandMenu);
            var selected = promotionConfirmMenu.SelectedIndex;
            menu_Closed(sender, e);

            switch (selected.Index)
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
            var unit = promotionChoiceMenu.Unit;
            int promotion = promotionChoiceMenu.PromotionChoice;

            // If selecting promotion on its own
            if (Menus.Count <= 2 || !(Menus.ElementAt(2) is ItemMenu))
            {
                MenuHandler.UnitMenuPromotionChoice(unit, promotion);
            }
            else
            {
                var itemMenu = (Menus.ElementAt(2) as ItemMenu);

                int itemIndex = itemMenu.SelectedItem;
                var itemData = unit.actor.items[itemIndex];

                MenuHandler.UnitMenuUseItem(unit, itemIndex, Maybe<Vector2>.Nothing, promotion);
            }
        }

        private void placeableTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as LocationTargetMenu);
            var itemMenu = (Menus.ElementAt(1) as ItemMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
            int itemIndex = itemMenu.SelectedItem;

            Global.game_map.remove_updated_move_range(targetMenu.UnitId);
            Global.game_map.range_start_timer = 0;

            MenuHandler.UnitMenuUseItem(unit, itemIndex, targetMenu.SelectedLoc, Maybe<int>.Nothing);
        }

        private void repairMenu_Selected(object sender, EventArgs e)
        {
            var repairMenu = (sender as ItemMenu);
            var itemMenu = (Menus.ElementAt(1) as ItemMenu);

            var unit = repairMenu.Unit;
            var usedItem = unit.actor.whole_inventory[itemMenu.SelectedItem].to_item;
            Item_Data targetItem = unit.actor.whole_inventory[repairMenu.SelectedItem];
            // If the target cannot be repaired
            if (!usedItem.can_target_item(targetItem))
            {
                if (usedItem.can_repair)
                {
                    if (!targetItem.is_weapon)
                    {
                        OpenRepairBlockedMenu(repairMenu,
                            "Items cannot be repaired.");
                        return;
                    }
                    else if (targetItem.to_weapon.is_staff())
                    {
                        OpenRepairBlockedMenu(repairMenu,
                            "Staves cannot be repaired.");
                        return;
                    }
                    else if (targetItem.infinite_uses || targetItem.Uses >= targetItem.max_uses)
                    {
                        OpenRepairBlockedMenu(repairMenu,
                            "This weapon is already\nin pristine condition.");
                        return;
                    }
                }

                Global.game_system.play_se(System_Sounds.Buzzer);
                return;
            }

            Global.game_system.play_se(System_Sounds.Confirm);
            // No guarantee that doing something with items isn't changing some unit's move range in some way
            Global.game_map.remove_updated_move_range(unit.id);
            repairMenu.RestoreEquipped();
            Global.game_system.Item_Inventory_Target = repairMenu.SelectedItem;
            MenuHandler.UnitMenuUseItem(unit, itemMenu.SelectedItem, Maybe<Vector2>.Nothing, Maybe<int>.Nothing);
        }

        private void OpenRepairBlockedMenu(ItemMenu repairMenu, string message)
        {
            Global.game_system.play_se(System_Sounds.Help_Open);
            var helpWindow = new Window_Help();
            helpWindow.loc = repairMenu.WindowLoc + new Vector2(8, 8 + repairMenu.Index * 16);
            helpWindow.set_loc(repairMenu.WindowLoc + new Vector2(0, 8 + repairMenu.Index * 16));
            helpWindow.set_text(message);

            var repairBlockedMenu = new SpriteMenu(helpWindow, false);
            repairBlockedMenu.Selected += repairBlockedMenu_Closed;
            repairBlockedMenu.Closed += repairBlockedMenu_Closed;
            repairBlockedMenu.AddToManager(new MenuCallbackEventArgs(this.AddMenu, repairBlockedMenu_Closed));
        }

        private void repairBlockedMenu_Closed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Help_Close);
            RemoveTopMenu();
        }

        private void discardConfirmMenu_Selected(object sender, EventArgs e)
        {
            var discardConfirmMenu = (sender as CommandMenu);
            var selected = discardConfirmMenu.SelectedIndex;
            // Close item options menus
            menu_Closed(sender, e);
            RemoveTopMenu();

            var itemMenu = (Menus.Peek() as ItemMenu);
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);
            Game_Unit unit = itemMenu.Unit;
            
            switch (selected.Index)
            {
                // Yes
                case 0:
                    Global.game_system.play_se(System_Sounds.Open);
                    unit.actor.discard_item(itemMenu.SelectedItem);
                    // Update attack range
                    Global.game_map.remove_updated_move_range(unit.id);
                    // Unit window has to restart in case options have changed
                    int command = unitMenu.CommandAtIndex;
                    unitMenu.RefreshCommands(unitMenu.Canto);
                    // Close item menu
                    RemoveTopMenu();

                    // If unit has items left, re-open item menu
                    if (!unit.actor.has_no_items)
                    {
                        unitMenu.MoveToCommand(command);

                        OpenItemMenu(unit, unitMenu);
                    }
                    break;
                // No
                case 1:
                    Global.game_system.play_se(System_Sounds.Cancel);
                    break;
            }
        }

        private void discardMenu_Selected(object sender, EventArgs e)
        {
            var discardMenu = (sender as DiscardMenu);
            string text = discardMenu.DropText;

            var discardConfirmWindow = new Window_Confirmation();
            discardConfirmWindow.loc = discardMenu.SelectedOptionLoc;
            discardConfirmWindow.set_text(text);
            discardConfirmWindow.add_choice("Yes", new Vector2(16, 16));
            discardConfirmWindow.add_choice("No", new Vector2(56, 16));
            discardConfirmWindow.index = 1;
            if (discardConfirmWindow.loc.Y + discardConfirmWindow.size.Y >= Config.WINDOW_HEIGHT)
                discardConfirmWindow.loc = discardMenu.SelectedOptionLoc -
                    new Vector2(0, discardConfirmWindow.size.Y + 16);

            var suspendConfirmMenu = new ConfirmationMenu(discardConfirmWindow);
            suspendConfirmMenu.Confirmed += discardMenu_Confirmed;
            suspendConfirmMenu.Canceled += menu_Closed;
            AddMenu(suspendConfirmMenu);
        }
        private void discardMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var discardMenu = (Menus.ElementAt(1) as DiscardMenu);
            Game_Unit unit = discardMenu.Unit;
            int index = discardMenu.SelectedItem;

            discardMenu.RestoreEquipped();
            Menus.Clear();
            MenuHandler.UnitMenuDiscard(unit, index);
        }
        #endregion

        #region 4: Trade
        private void Trade(Game_Unit unit, UnitCommandMenu unitMenu)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            OpenTradeTargetMenu(unit, unitMenu);
        }

        private void OpenTradeTargetMenu(
            Game_Unit unit,
            UnitCommandMenu unitMenu,
            Maybe<int> selectedItemIndex = default(Maybe<int>))
        {
            var targetWindow = new Window_Target_Trade(unit.id, new Vector2(4, 0));
            TradeInitialIndex = selectedItemIndex;
            var targetMenu = new UnitTargetMenu(targetWindow, unitMenu);
            targetMenu.Selected += tradeTargetMenu_Selected;
            targetMenu.Canceled += unitTargetMenu_Canceled;
            AddMenu(targetMenu);

            Global.player.facing = 4;
            Global.player.update_cursor_frame();
        }

        private void tradeTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);

            var targetMenu = (sender as UnitTargetMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];

            var tradeWindow = new Window_Trade(unit.actor.id,
                Global.game_map.units[targetMenu.SelectedUnitId].actor.id, -1);
            tradeWindow.stereoscopic = Config.PREPTRADE_WINDOWS_DEPTH;
            tradeWindow.face_stereoscopic = Config.PREPTRADE_FACES_DEPTH;
            tradeWindow.help_stereoscopic = Config.PREPTRADE_HELP_DEPTH;
            // If trading from the item screen, select the initial item
            if (TradeInitialIndex.IsSomething)
                tradeWindow.SelectItem(TradeInitialIndex);

            var tradeMenu = new TradeMenu(tradeWindow, targetMenu);
            tradeMenu.Closed += tradeMenu_Closed;
            AddMenu(tradeMenu);
            
            // Makes cursor invisible again
            Global.player.facing = 6;
            Global.player.frame = 1;
        }

        private void tradeMenu_Closed(object sender, EventArgs e)
        {
            var tradeMenu = (sender as TradeMenu);
            // Unit traded, return to command menu
            if (tradeMenu.Traded)
            {
                var targetMenu = (Menus.ElementAt(1) as UnitTargetMenu);
                Game_Unit unit = Global.game_map.units[targetMenu.UnitId];

                // No guarantee that doing something with items isn't changing some unit's move range in some way
                Global.game_map.remove_updated_move_range(unit.id);
                Global.game_map.remove_updated_move_range(targetMenu.Target);
                // Lock in unit movement
                unit.moved();
                unit.queue_move_range_update();
                // Sends cursor back to unit
                Global.player.instant_move = true;
                Global.player.loc = unit.loc;
                tradeMenu.StaffFix();
                unit.cantoing = true;

                // Remove menus back to the command menu
                while (!(Menus.Peek() is UnitCommandMenu))
                {
                    RemoveTopMenu();
                    if (!Menus.Any())
                        throw new IndexOutOfRangeException();
                }
                var unitMenu = (Menus.Peek() as UnitCommandMenu);
                unitMenu.RefreshCommands(unitMenu.Canto | Canto_Records.Trade |
                    ((unit.has_canto() && !unit.full_move()) ? Canto_Records.Horse : Canto_Records.None));
                unit.open_move_range();
                Global.game_map.move_range_visible = false;
            }
            // Canceled without trading
            else
            {
                // Makes cursor visible again
                Global.player.facing = 4;

                menu_Closed(sender, e);
            }
        }
        #endregion

        // 5: Wait
        private void Wait(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            MenuHandler.UnitMenuWait(unit);
        }

        #region 6: Give/Take
        private void Take(Game_Unit unit, UnitCommandMenu unitMenu)
        {
            if (!unit.is_rescuing && unit.is_rescue_blocked())
                Global.game_system.play_se(System_Sounds.Buzzer);
            else
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                var targetWindow = new Window_Target_Rescue(
                    unit.id, unit.is_rescuing ? 3 : 2, new Vector2(0, 0));
                var targetMenu = new UnitTargetMenu(targetWindow, unitMenu);
                // Give
                if (unit.is_rescuing)
                    targetMenu.Selected += giveTargetMenu_Selected;
                // Take
                else
                    targetMenu.Selected += takeTargetMenu_Selected;
                targetMenu.Canceled += unitTargetMenu_Canceled;
                AddMenu(targetMenu);

                Global.player.facing = 4;
                Global.player.update_cursor_frame();
            }
        }

        private void takeTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as UnitTargetMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);

            MenuHandler.UnitMenuTake(unit, targetMenu.SelectedUnitId, unitMenu.Canto);
        }
        private void giveTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as UnitTargetMenu);
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];

            MenuHandler.UnitMenuGive(unit, targetMenu.SelectedUnitId, unitMenu.Canto);
        }
        #endregion

        // 7, 11: Visit, Chest
        private void Visit(Game_Unit unit, Tactile.State.Visit_Modes visitMode)
        {
            Global.game_system.play_se(System_Sounds.Confirm);

            Global.game_map.remove_updated_move_range(unit.id);
            Global.game_map.clear_move_range();
            Global.game_map.range_start_timer = 0;
            Global.game_state.call_visit(visitMode, unit.id, unit.loc);
            Global.game_temp.menuing = false;
            CloseCommandMenu();

            unit.cantoing = false;
            // Lock in unit movement
            unit.moved();
            if (unit.has_canto() && !unit.full_move())
                unit.cantoing = true;
        }

        #region 8: Talk
        private void Talk(Game_Unit unit, UnitCommandMenu unitMenu)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetWindow = new Window_Target_Talk(unit.id, new Vector2(4, 0));
            var targetMenu = new UnitTargetMenu(targetWindow, unitMenu);
            targetMenu.Selected += talkTargetMenu_Selected;
            targetMenu.Canceled += unitTargetMenu_Canceled;
            AddMenu(targetMenu);

            Global.player.facing = 4;
            Global.player.update_cursor_frame();
        }

        private void talkTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as UnitTargetMenu);
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
            MenuHandler.UnitMenuTalk(unit, targetMenu.Target, unitMenu.Canto);
        }
        #endregion

        #region 9, 10, 29, 30: Shop, Arena
        private void Shop(Game_Unit unit, bool secret)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            Global.game_system.Shopper_Id = unit.id;
            Global.game_system.Shop_Loc = unit.loc;
            Global.game_system.SecretShop = secret;
            Global.game_map.move_range_visible = false;

            Window_Business shopMenu;
            var shop = Global.game_map.get_shop();
            if (shop.arena)
                shopMenu = new Window_Arena(Global.game_system.Shopper_Id, shop, false);
            else
            {
                int actor_id = Global.game_map.units[Global.game_system.Shopper_Id].actor.id;
                shopMenu = new Window_Shop(actor_id, shop);
            }
            shopMenu.Shop_Close += shopMenu_Shop_Close;
            shopMenu.Closed += shopMenu_Closed;
            AddMenu(shopMenu);
        }

        // Shop menu closing
        private void shopMenu_Shop_Close(object sender, EventArgs e)
        {
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);
            unitMenu.IsVisible = false;
        }
        // Shop menu closed
        private void shopMenu_Closed(object sender, EventArgs e)
        {
            var shopMenu = (sender as Window_Business);
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);
            Game_Unit unit = Global.game_map.units[unitMenu.UnitId];

            // If unit used the shop and can't cancel their action
            if (shopMenu.traded)
            {
                // Lock in unit movement
                unit.moved();
                Global.game_map.remove_updated_move_range(unit.id);
                // If not entering the arena
                if (!Global.game_system.In_Arena)
                {
                    // Skip this when entering the arena
                    Global.game_state.call_shop_suspend();

                    if (!unit.has_canto() || unit.full_move())
                        unit.start_wait();
                    Global.game_map.clear_move_range();
                    Global.game_temp.menuing = false;
                }

                // Close shop menu
                menu_Closed(sender, e);
                CloseCommandMenu(true);
            }
            else
            {
                // Close shop menu
                menu_Closed(sender, e);

                // If not unit cantoing, cancel the menu completely
                // Otherwise the unit menu will reappear
                if (!unit.cantoing)
                {
                    menu_ClosedCanceled(unitMenu, new EventArgs());
                }
            }

            // If not entering the arena
            if (!Global.game_system.In_Arena)
                Global.game_state.resume_turn_theme(true);
        }

        // Shop menu closed
        private void shopPreviewMenu_Closed(object sender, EventArgs e)
        {
            Global.game_temp.menuing = false;
            Menus.Clear();
            Global.game_map.move_range_visible = true;

            Global.game_state.resume_turn_theme(true);
        }

        private void arenaMenu_Closed(object sender, EventArgs e)
        {
            var arenaMenu = (sender as Window_Arena);
            Game_Unit unit = arenaMenu.unit;

            // Unit used the shop and can't cancel their action
            // Killed in the arena
            if (unit.is_dead)
            {
                unit.gladiator = true;
                Global.game_state.CleanupArena();
            }
            // Otherwise shop is just closing
            else
            {
                // Lock in unit movement
                unit.moved();
                Global.game_map.remove_updated_move_range(unit.id);
                Global.game_state.CleanupArena();

                Global.game_map.clear_move_range();
            }

            // Close shop menu
            menu_Closed(sender, e);
            CloseCommandMenu(true);

            // If not game over from arena death
            if (!Global.game_system.is_loss())
                Global.game_state.resume_turn_theme(true);
        }
        #endregion

        #region 12: Door
        private void Door(Game_Unit unit, UnitCommandMenu unitMenu)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetWindow = new Window_Target_Door(unit.id, new Vector2(4, 0));
            var targetMenu = new LocationTargetMenu(targetWindow, unitMenu);
            targetMenu.Selected += doorTargetMenu_Selected;
            targetMenu.Canceled += locationTargetMenu_Canceled;
            AddMenu(targetMenu);

            Global.player.facing = 4;
            Global.player.update_cursor_frame();
        }

        private void doorTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as LocationTargetMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
            MenuHandler.UnitMenuDoor(unit, targetMenu.Target);
        }
        #endregion

        #region 13: Steal
        private void Steal(Game_Unit unit, UnitCommandMenu unitMenu)
        {
            if (unit.actor.is_full_items)
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                //Audio.play_se("System_Sounds", "Help_Open"); //@Yeti
                //open help window saying you're full, lock menu until help window is cleared //@Yeti
            }
            else
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                var targetWindow = new Window_Target_Steal(unit.id, new Vector2(4, 0));
                var targetMenu = new UnitTargetMenu(targetWindow, unitMenu);
                targetMenu.Selected += stealTargetMenu_Selected;
                targetMenu.Canceled += unitTargetMenu_Canceled;
                AddMenu(targetMenu);

                Global.player.facing = 4;
                Global.player.update_cursor_frame();
            }
        }

        private void stealTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as UnitTargetMenu);
            targetMenu.Accept();

            var stealWindow = new Window_Steal(targetMenu.UnitId, targetMenu.SelectedUnitId);
            var stealMenu = new StealMenu(stealWindow, targetMenu);
            stealMenu.Selected += stealMenu_Selected;
            stealMenu.Canceled += stealMenu_Closed;
            AddMenu(stealMenu);
        }

        private void stealMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);

            var stealMenu = (sender as StealMenu);
            var targetMenu = (Menus.ElementAt(1) as UnitTargetMenu);
            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];

            MenuHandler.UnitMenuSteal(unit, targetMenu.Target, stealMenu.SelectedItem);
        }

        private void stealMenu_Closed(object sender, EventArgs e)
        {
            // Makes cursor visible again
            Global.player.facing = 4;
            menu_Closed(sender, e);
        }
        #endregion

        // 14: Seize
        private void Seize(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            MenuHandler.UnitMenuSeize(unit);
        }

        #region 15: Status
        private void Status(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            unit.status();
            Global.game_temp.status_menu_call = false;
            
            List<int> team = new List<int>();
            {
                // Only list units that are on the map or rescued (rescued units can be off map)
                team.AddRange(Global.game_map.teams[Global.game_temp.status_team]
                    .Where(x => x == Global.game_temp.status_unit_id ||
                        !Global.game_map.is_off_map(Global.game_map.units[x].loc) || Global.game_map.units[x].is_rescued));
            }
            int index = team.IndexOf(Global.game_temp.status_unit_id);
            index = Math.Max(0, index);

            var statusMenu = new Window_Status(team, index);
            statusMenu.Closed += statusMenu_Closed;
            AddMenu(statusMenu);
        }

        private void statusMenu_Closed(object sender, EventArgs e)
        {
            var statusMenu = (sender as Window_Status);

            Global.game_temp.status_team = 0;
            statusMenu.close();
            RemoveTopMenu();
        }

        private void statusScreen_Closed(object sender, EventArgs e)
        {
            var statusMenu = (sender as Window_Status);

            Global.game_temp.menuing = false;
            Global.game_temp.status_team = 0;
            statusMenu.jump_to_unit();
            statusMenu.close();
            Menus.Clear();
        }
        #endregion

        #region 16: Dance
        private void Dance(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var unitMenu = (Menus.Peek() as UnitCommandMenu);

            if (unit.has_dancer_ring())
            {
                var danceItemWindow = new Window_Command_Item_Dance(unit.id, new Vector2(24, 8));
                danceItemWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                danceItemWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                danceItemWindow.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;

                var danceMenu = new ItemMenu(danceItemWindow, unitMenu);
                danceMenu.Selected += danceMenu_Selected;
                danceMenu.Canceled += menu_Closed;
                AddMenu(danceMenu);
            }
            else
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                OpenDanceTargetMenu(unit, unitMenu);
            }
        }

        private void OpenDanceTargetMenu(
            Game_Unit unit,
            IHasCancelButton parentMenu,
            Maybe<int> selectedItemIndex = default(Maybe<int>))
        {
            DancerRingIndex = selectedItemIndex;

            // This should really never come up but just in case
            unit.using_siege_engine = DancerRingIndex.IsSomething &&
                DancerRingIndex == Siege_Engine.SiegeInventoryIndex;

            var targetWindow = new Window_Target_Dance(
                unit.id,
                DancerRingIndex.IsSomething && DancerRingIndex > -1,
                new Vector2(4, 0));
            var targetMenu = new UnitTargetMenu(targetWindow, parentMenu);
            targetMenu.Selected += danceTargetMenu_Selected;
            targetMenu.Canceled += danceTargetMenu_Canceled;
            AddMenu(targetMenu);

            Global.player.facing = 4;
            Global.player.update_cursor_frame();
        }

        private void danceMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var itemMenu = (sender as ItemMenu);
            OpenDanceTargetMenu(itemMenu.Unit, itemMenu, itemMenu.SelectedItem);
        }

        private void danceTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as UnitTargetMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
            MenuHandler.UnitMenuDance(
                unit,
                targetMenu.SelectedUnitId,
                DancerRingIndex.OrIfNothing(-1));
        }

        private void danceTargetMenu_Canceled(object sender, EventArgs e)
        {
            var targetMenu = (sender as UnitTargetMenu);
            Global.game_map.units[targetMenu.UnitId].using_siege_engine = false;
            unitTargetMenu_Canceled(sender, e);
        }
        #endregion

        #region 17: Support
        private void Support(Game_Unit unit, UnitCommandMenu unitMenu)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetWindow = new Window_Target_Support(unit.id, new Vector2(4, 0));
            var targetMenu = new UnitTargetMenu(targetWindow, unitMenu);
            targetMenu.Selected += supportTargetMenu_Selected;
            targetMenu.Canceled += unitTargetMenu_Canceled;
            AddMenu(targetMenu);

            Global.player.facing = 4;
            Global.player.update_cursor_frame();
        }

        private void supportTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as UnitTargetMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
            MenuHandler.UnitMenuSupport(unit, targetMenu.Target);
        }
        #endregion

        #region 18: Supply
        private void Supply(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var supplyMenu = new Window_Supply(unit);
            supplyMenu.Closing += supplyMenu_Closing;
            supplyMenu.Closed += menu_Closed;
            AddMenu(supplyMenu);
        }

        private void supplyMenu_Closing(object sender, EventArgs e)
        {
            var supplyMenu = (sender as Window_Supply);
            if (supplyMenu.traded)
            {
                var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);
                Game_Unit unit = Global.game_map.units[unitMenu.UnitId];

                // No guarantee that doing something with items isn't changing some unit's move range in some way
                Global.game_map.remove_updated_move_range(unit.id);
                // Lock in unit movement
                unit.moved();
                unit.queue_move_range_update();
                // Sends cursor back to unit
                Global.player.instant_move = true;
                Global.player.loc = unit.loc;
                // Switch to map
                unit.cantoing = true;

                unitMenu.RefreshCommands(unitMenu.Canto | Canto_Records.Supply |
                    ((unit.has_canto() && !unit.full_move()) ? Canto_Records.Horse : Canto_Records.None));
                unit.open_move_range();
                Global.game_map.move_range_visible = false;
            }
        }
        #endregion

        // 19: Escape
        private void Escape(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            MenuHandler.UnitMenuEscape(unit);
        }

        #region 20: Construct
        private void Construct(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var unitMenu = (Menus.Peek() as UnitCommandMenu);

            int width = 72;
            var constructWindow = new Window_Command(
                new Vector2(8 + (Global.player.is_on_left() ?
                    (Config.WINDOW_WIDTH - (width + 16)) : 0), 24),
                    width, new List<string> { "Assemble", "Reload", "Reclaim" });
            constructWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            if (!(unit.can_assemble() && unit.assemble_targets().Count > 0))
                constructWindow.set_text_color(0, "Grey");
            if (!(unit.can_reload() && unit.reload_targets().Count > 0))
                constructWindow.set_text_color(1, "Grey");
            if (!(unit.can_reclaim() && unit.reclaim_targets().Count > 0))
                constructWindow.set_text_color(2, "Grey");
            
            var constructMenu = new CommandMenu(constructWindow, unitMenu);
            constructMenu.HideParent(true);
            constructMenu.Selected += constructMenu_Selected;
            constructMenu.Canceled += menu_Closed;
            AddMenu(constructMenu);
        }

        private void constructMenu_Selected(object sender, EventArgs e)
        {
            var constructMenu = (sender as CommandMenu);
            var unitMenu = (Menus.ElementAt(1) as UnitCommandMenu);
            Game_Unit unit = Global.game_map.units[unitMenu.UnitId];

            switch (constructMenu.SelectedIndex.Index)
            {
                // Assemble
                case 0:
                    if (unit.can_assemble() && unit.assemble_targets().Count > 0)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        var assembleWindow = new Window_Command_Item_Assemble(
                            unit.id, new Vector2(24, 8));
                        assembleWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                        assembleWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                        assembleWindow.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;

                        var assembleMenu = new ItemMenu(assembleWindow, constructMenu);
                        assembleMenu.Selected += assembleMenu_Selected;
                        assembleMenu.Canceled += menu_Closed;
                        AddMenu(assembleMenu);
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                // Reload
                case 1:
                    if (unit.can_reload() && unit.reload_targets().Count > 0)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        var targetWindow = new Window_Target_Construct(
                            unit.id, ConstructionModes.Reload, new Vector2(4, 0));
                        var targetMenu = new LocationTargetMenu(targetWindow, constructMenu);
                        targetMenu.Selected += reloadTargetMenu_Selected;
                        targetMenu.Canceled += locationTargetMenu_Canceled;
                        AddMenu(targetMenu);

                        Global.player.facing = 4;
                        Global.player.update_cursor_frame();
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                // Reclaim
                case 2:
                    if (unit.can_reclaim() && unit.reclaim_targets().Count > 0)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        var targetWindow = new Window_Target_Construct(
                            unit.id, ConstructionModes.Reclaim, new Vector2(4, 0));
                        var targetMenu = new LocationTargetMenu(targetWindow, constructMenu);
                        targetMenu.Selected += reclaimTargetMenu_Selected;
                        targetMenu.Canceled += locationTargetMenu_Canceled;
                        AddMenu(targetMenu);

                        Global.player.facing = 4;
                        Global.player.update_cursor_frame();
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
            }
        }

        private void assembleMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var assembleMenu = (sender as ItemMenu);
            var unit = assembleMenu.Unit;
            
            var itemData = assembleMenu.SelectedItemData;
            
            var targetWindow = new Window_Target_Construct(
                unit.id, ConstructionModes.Assemble, new Vector2(4, 0),
                itemData.Id);
            var targetMenu = new LocationTargetMenu(targetWindow, assembleMenu);
            targetMenu.Selected += assembleTargetMenu_Selected;
            targetMenu.Canceled += locationTargetMenu_Canceled;
            AddMenu(targetMenu);

            Global.player.facing = 4;
            Global.player.update_cursor_frame();
        }

        private void assembleTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as LocationTargetMenu);
            targetMenu.Accept();

            Vector2 targetLoc;
            Game_Unit unit;
            StartConstruct(sender, out unit, out targetLoc);

            var itemMenu = (Menus.ElementAt(1) as ItemMenu);
            int itemIndex = itemMenu.SelectedItem;
            MenuHandler.UnitMenuAssemble(unit, itemIndex, targetLoc);
        }
        private void reloadTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as LocationTargetMenu);
            targetMenu.Accept();

            Vector2 targetLoc;
            Game_Unit unit;
            StartConstruct(sender, out unit, out targetLoc);

            MenuHandler.UnitMenuReload(unit, targetLoc);
        }
        private void reclaimTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as LocationTargetMenu);
            targetMenu.Accept();

            Vector2 targetLoc;
            Game_Unit unit;
            StartConstruct(sender, out unit, out targetLoc);
            
            MenuHandler.UnitMenuReclaim(unit, targetLoc);
        }

        private static void StartConstruct(object sender, out Game_Unit unit, out Vector2 targetLoc)
        {
            var targetMenu = (sender as LocationTargetMenu);

            unit = Global.game_map.units[targetMenu.UnitId];
            Global.game_map.remove_updated_move_range(targetMenu.UnitId);
            Global.game_map.clear_move_range();
            Global.game_map.range_start_timer = 0;

            targetLoc = targetMenu.SelectedLoc;
        }
        #endregion
        #endregion
        
        // Close the menu
        void menu_ClosedCanceled(object sender, EventArgs e)
        {
            var unitMenu = (sender as UnitCommandMenu);
            Game_Unit unit = Global.game_map.units[unitMenu.UnitId];
            unit.command_menu_close();

            CloseCommandMenu();

            // If the unit has no move range, don't bother making it visible
            if (unit.mov <= 0 && unitMenu.Canto == Canto_Records.None)
            {
                unit.cancel_move();
                Global.game_system.Selected_Unit_Id = -1;
                Global.game_map.highlight_test();
                Global.game_system.play_se(System_Sounds.Cancel);
            }
            else
                Global.game_system.play_se(System_Sounds.Unit_Select);
        }

        private void CloseCommandMenu(bool moveRangeVisible = true)
        {
            Global.game_temp.menuing = false;
            Menus.Clear();

            if (moveRangeVisible)
                Global.game_map.move_range_visible = true;

            // Reset context sensitive values on unit menu close
            Global.game_temp.ResetContextSensitiveUnitControl();
        }

        protected void menu_Closed(object sender, EventArgs e)
        {
            RemoveTopMenu();
        }

        #region Target Window
        private void unitTargetMenu_Canceled(object sender, EventArgs e)
        {
            var targetMenu = (sender as UnitTargetMenu);
            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];

            closeTargetMenu(sender, e, unit);
        }
        private void locationTargetMenu_Canceled(object sender, EventArgs e)
        {
            var targetMenu = (sender as LocationTargetMenu);
            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];

            closeTargetMenu(sender, e, unit);
        }
        private void closeTargetMenu(object sender, EventArgs e, Game_Unit unit)
        {
            Global.game_map.range_start_timer = 0;
            // Sends cursor back to unit
            Global.player.instant_move = true;
            Global.player.loc = unit.loc;
            // Makes cursor invisible again
            Global.player.facing = 6;
            Global.player.frame = 1;

            menu_Closed(sender, e);
        }

        public bool ManualTargeting
        {
            get
            {
                foreach(var menu in Menus)
                {
                    if (menu is ITargetMenu)
                    {
                        var targetMenu = menu as ITargetMenu;
                        return targetMenu.ManualTargeting;
                    }
                }

                return false;
            }
        }

        public bool TargetWindowUp
        {
            get
            {
                if (Menus.Any())
                {
                    var topMenu = Menus.Peek();
                    if (topMenu is ITargetMenu)
                        return true;
                }

                return false;
            }
        }
        public bool TargetWindowHasTarget
        {
            get
            {
                foreach(var menu in Menus)
                {
                    if (menu is ITargetMenu)
                    {
                        var targetMenu = menu as ITargetMenu;
                        if (targetMenu.IsWindowA<Window_Target_Unit>())
                        {
                            return !targetMenu.ManualTargeting &&
                                targetMenu.HasTarget;
                        }
                    }
                }

                return false;
            }
        }
        public Vector2 TargetWindowTargetLoc
        {
            get
            {
                foreach (var menu in Menus)
                {
                    if (menu is ITargetMenu)
                    {
                        var targetMenu = menu as ITargetMenu;
                        return targetMenu.TargetLoc;
                    }
                }

                throw new NullReferenceException(
                    "Tried to get target window loc, but\nthere is no target window");
            }
        }
        public Vector2 TargetWindowLastTargetLoc
        {
            get
            {
                foreach (var menu in Menus)
                {
                    if (menu is ITargetMenu)
                    {
                        var targetMenu = menu as ITargetMenu;
                        return targetMenu.LastTargetLoc;
                    }
                }

                throw new NullReferenceException(
                    "Tried to get target window loc, but\nthere is no target window");
            }
        }

        public bool CombatTargetWindowUp
        {
            get
            {
                if (TargetWindowUp)
                {
                    var targetMenu = (Menus.Peek() as ITargetMenu);
                    return targetMenu.IsWindowA<Window_Target_Combat>();
                }

                return false;
            }
        }

        public bool DropTargetWindowUp
        {
            get
            {
                if (TargetWindowUp)
                {
                    var targetMenu = (Menus.Peek() as ITargetMenu);
                    return targetMenu.IsRescueDropMenu;
                }

                return false;
            }
        }
        #endregion

        private void dialoguePromptMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var dialoguePromptMenu = (sender as DialoguePromptMenu);

            int index = dialoguePromptMenu.SelectedIndex.Index;
            Global.game_temp.LastDialoguePrompt = index + 1;
            if (dialoguePromptMenu.VariableId >= 0)
                Global.game_system.VARIABLES[dialoguePromptMenu.VariableId] =
                    Global.game_temp.LastDialoguePrompt;

            Global.game_temp.menuing = false;
            Global.game_temp.prompt_menuing = false;
            Menus.Clear();
        }

        private void confirmationPromptMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var confirmationPromptMenu = (sender as ConfirmationPromptMenu);

            Global.game_temp.LastConfirmationPrompt = true;
            if (confirmationPromptMenu.SwitchId >= 0)
                Global.game_system.SWITCHES[confirmationPromptMenu.SwitchId] =
                    Global.game_temp.LastConfirmationPrompt;

            Global.game_temp.menuing = false;
            Global.game_temp.prompt_menuing = false;
            Menus.Clear();
        }
        private void confirmationPromptMenu_Canceled(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var confirmationPromptMenu = (sender as ConfirmationPromptMenu);

            Global.game_temp.LastConfirmationPrompt = false;
            if (confirmationPromptMenu.SwitchId >= 0)
                Global.game_system.SWITCHES[confirmationPromptMenu.SwitchId] =
                    Global.game_temp.LastConfirmationPrompt;

            Global.game_temp.menuing = false;
            Global.game_temp.prompt_menuing = false;
            Menus.Clear();
        }

        public bool ShowAttackRange
        {
            get
            {
                for (int i = 0; i < Menus.Count; i++)
                {
                    if (Menus.ElementAt(i) is UnitCommandMenu)
                    {
                        var unitMenu = (Menus.ElementAt(i) as UnitCommandMenu);
                        // Attack
                        if (unitMenu.CommandAtIndex == 0)
                            return true;
                        break;
                    }
                    else if (Menus.ElementAt(i) is LocationTargetMenu)
                    {
                        var menu = (Menus.ElementAt(i) as LocationTargetMenu);
                        return menu.IsWindowA<Window_Target_Construct>();
                    }
                }
                return false;
            }
        }
        public bool ShowStaffRange
        {
            get
            {
                for (int i = 0; i < Menus.Count; i++)
                {
                    if (Menus.ElementAt(i) is UnitCommandMenu)
                    {
                        var unitMenu = (Menus.ElementAt(i) as UnitCommandMenu);
                        // Staff
                        if (unitMenu.CommandAtIndex == 1)
                            return true;
                        break;
                    }
                }
                return false;
            }
        }
        public bool ShowTalkRange
        {
            get
            {
                for (int i = 0; i < Menus.Count; i++)
                {
                    if (Menus.ElementAt(i) is UnitCommandMenu)
                    {
                        var unitMenu = (Menus.ElementAt(i) as UnitCommandMenu);
                        // Talk
                        if (unitMenu.CommandAtIndex == 8)
                            return true;
                        break;
                    }
                }
                return false;
            }
        }
    }

    partial interface IUnitMenuHandler : IMenuHandler
    {
        void UnitMenuAttack(Game_Unit unit, int targetId);
        void UnitMenuStaff(Game_Unit unit, int targetId, Vector2 targetLoc);
        void UnitMenuRescue(Game_Unit unit, int targetId);
        void UnitMenuDrop(Game_Unit unit, Vector2 targetLoc);
        void UnitMenuUseItem(Game_Unit unit, int itemIndex, Maybe<Vector2> targetLoc, Maybe<int> promotionId);
        void UnitMenuWait(Game_Unit unit);
        void UnitMenuTake(Game_Unit unit, int targetId, Canto_Records canto);
        void UnitMenuGive(Game_Unit unit, int targetId, Canto_Records canto);
        void UnitMenuTalk(Game_Unit unit, int targetId, Canto_Records canto);
        void UnitMenuDoor(Game_Unit unit, Vector2 targetLoc);
        void UnitMenuSteal(Game_Unit unit, int targetId, int itemIndex);
        void UnitMenuSeize(Game_Unit unit);
        void UnitMenuDance(Game_Unit unit, int targetId, int ringIndex);
        void UnitMenuSupport(Game_Unit unit, int targetId);
        void UnitMenuEscape(Game_Unit unit);
        void UnitMenuAssemble(Game_Unit unit, int itemIndex, Vector2 targetLoc);
        void UnitMenuReload(Game_Unit unit, Vector2 targetLoc);
        void UnitMenuReclaim(Game_Unit unit, Vector2 targetLoc);

        void UnitMenuDiscard(Game_Unit unit, int index);
        void UnitMenuPromotionChoice(Game_Unit unit, int promotionId);
    }
}
