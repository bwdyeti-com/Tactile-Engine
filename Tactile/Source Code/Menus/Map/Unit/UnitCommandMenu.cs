using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Windows.Command;
using Tactile.Windows.Map;
using EnumExtension;
using TactileWeaponExtension;

namespace Tactile.Menus.Map.Unit
{
    partial class UnitCommandMenu : CommandMenu
    {
        internal int UnitId { get; private set; }
        private List<int> IndexRedirect;
        private List<int> Disabled;
        internal Canto_Records Canto { get; private set; }
        private List<int> UseableWeapons, UseableStaves;

        public UnitCommandMenu(int unitId)
        {
            UnitId = unitId;
            Canto = Canto_Records.None;

            RefreshCommands();
            
            CreateCancelButton();
        }

        #region Commands
        public void RefreshCommands()
        {
            Canto_Records canto = Global.game_system.Menu_Canto;
            Global.game_system.Menu_Canto = Canto_Records.None;
            RefreshCommands(canto);
        }
        public void RefreshCommands(Canto_Records canto)
        {
            List<string> commands = SetCommands(canto);
            var window = NewUnitWindow(commands, 56);
            Window = window;
        }

        private List<string> SetCommands(Canto_Records canto)
        {
            Disabled = new List<int>();
            List<string> commands = new List<string>();
            // Actions:
            //   0 = Attack
            //   1 = Staff
            //   2 = Drop/Rescue
            //   3 = Item
            //   4 = Trade
            //   5 = Wait
            //   6 = Give/Take
            //   7 = Visit
            //   8 = Talk
            //   9 = Shop/Armory
            //  10 = Arena
            //  11 = Chest
            //  12 = Door
            //  13 = Steal
            //  14 = Seize
            //  15 = Status
            //  16 = Dance
            //  17 = Support
            //  18 = Supply
            //  19 = Escape
            //  20 = Construct
            //  29 = Secret Shop/Armory
            //  30 = Arena
            IndexRedirect = new List<int>();
            Game_Unit unit = Global.game_map.units[UnitId];
            unit.actor.remove_broken_items();

            Canto = Canto_Records.None;
            // If canto state is null but the unit is cantoing, set to horse canto
            if (canto == Canto_Records.None && Canto == Canto_Records.None)
                //Canto = (unit.cantoing && unit.has_canto() ? Canto_Records.Horse : Canto_Records.None); //@Debug
                Canto = (unit.cantoing ? Canto_Records.Horse : Canto_Records.None);
            else if (canto != Canto_Records.None)
                Canto = canto;

            if (CantoAllowsNormalActions(Canto))
            {
                List<int>[] range_ary;
                // enemies: attack
                range_ary = unit.enemies_in_range();
                List<int> enemy_range = range_ary[0];
                UseableWeapons = range_ary[1];
                // enemies: staves
                UseableStaves = new List<int>();
                range_ary = unit.enemies_in_staff_range();
                UseableStaves.AddRange(range_ary[1]);
                // allies: staves
                range_ary = unit.allies_in_staff_range();
                UseableStaves.AddRange(range_ary[1]);
                // other: staves
                range_ary = unit.untargeted_staff_range();
                List<int> other_staff_range = range_ary[0];
                UseableStaves.AddRange(range_ary[1]);
                UseableStaves = UseableStaves.Distinct().ToList();
                // allies
                List<int> ally_range = unit.allies_in_range(1);
                // Attack
                if (UseableWeapons.Count > 0)
                {
                    List<int> item_indices = unit.weapon_indices(UseableWeapons);
                    Global.game_temp.temp_attack_range = unit.get_weapon_range(item_indices, new HashSet<Vector2> { unit.loc });
                    //Global.game_temp.temp_attack_range = Global.game_map.get_unit_range(new List<Vector2> { unit.loc },
                    //    unit.min_range_absolute(), unit.max_range_absolute()); // this should care about the exact weapons, I think //Yeti
                    Global.game_map.range_start_timer = 0;
                    commands.Add("Attack");
                    IndexRedirect.Add(0);
                }
                // Staff
                if (UseableStaves.Count > 0)
                {
                    List<int> item_indices = unit.weapon_indices(UseableStaves);
                    Global.game_temp.temp_staff_range = unit.get_weapon_range(item_indices, new HashSet<Vector2> { unit.loc });
                    //Global.game_temp.temp_staff_range = Global.game_map.get_unit_range(new List<Vector2> { unit.loc },
                    //    unit.min_range_absolute(true), unit.max_range_absolute(true)); // this should care about the exact weapons, I think //Yeti
                    Global.game_map.range_start_timer = 0;
                    commands.Add("Staff");
                    IndexRedirect.Add(1);
                }
                // Seize
                if (Global.game_map.get_seize_points(unit.team, unit.group).Contains(unit.loc) && unit.can_seize)
                {
                    commands.Add("Seize");
                    IndexRedirect.Add(14);
                }
                // Escape
                if (Global.game_map.escape_point_locations(unit.team, unit.group).Contains(unit.loc))
                {
                    var escapePoint = Global.game_map.escape_point_data(unit, unit.loc);
                    if (!escapePoint.LordOnly || unit.can_seize)
                    {
                        commands.Add("Escape");
                        IndexRedirect.Add(19);
                    }
                }
                // Dance
                if (unit.can_dance())
                    if (unit.dance_targets().Count > 0)
                    {
                        commands.Add(unit.dance_name());
                        IndexRedirect.Add(16);
                    }
                // Construct
                bool can_construct = false;
                if (unit.can_assemble() && unit.assemble_targets().Count > 0)
                    can_construct = true;
                if (unit.can_reload() && unit.reload_targets().Count > 0)
                    can_construct = true;
                if (unit.can_reclaim() && unit.reclaim_targets().Count > 0)
                    can_construct = true;
                if (can_construct)
                {
                    commands.Add("Construct");
                    IndexRedirect.Add(20);
                }
                // Steal
                if (unit.can_steal())
                    if (unit.steal_targets().Count > 0)
                    {
                        commands.Add("Steal");
                        IndexRedirect.Add(13);
                        if (unit.actor.is_full_items)
                            Disabled.Add(13);
                    }
                // Support
                if (unit.can_support())
                {
                    commands.Add("Support");
                    IndexRedirect.Add(17);
                }
                // Talk
                if (!Canto.HasEnumFlag(Canto_Records.Talk) && unit.can_talk())
                {
                    Global.game_temp.temp_talk_range.Clear();
                    foreach (int id in unit.talk_targets())
                        Global.game_temp.temp_talk_range.Add(Global.game_map.units[id].loc);
                    Global.game_map.range_start_timer = 0;
                    commands.Add("Talk");
                    IndexRedirect.Add(8);
                }
                // Visit
                if (unit.can_visit())
                {
                    string visit_name = Global.game_map.visit_locations[unit.loc].Name;
                    commands.Add(!string.IsNullOrEmpty(visit_name) ? visit_name : "Visit");
                    IndexRedirect.Add(7);
                }
                // Chest
                if (unit.can_open_chest(true))
                    if (Global.game_map.chest_locations.ContainsKey(unit.loc))
                    {
                        commands.Add("Chest");
                        IndexRedirect.Add(11);
                    }
                // Door
                if (unit.can_open_door(true))
                    if (unit.door_targets().Count > 0)
                    {
                        commands.Add("Door");
                        IndexRedirect.Add(12);
                    }
                // Shop/arena
                if (Global.game_map.shops.ContainsKey(unit.loc))
                {
                    // Shop
                    if (!Global.game_map.shops[unit.loc].arena)
                    {
                        commands.Add(Global.game_map.shops[unit.loc].face.Split('-')[0]);
                        IndexRedirect.Add(9);
                    }
                    // Arena
                    else
                        if (unit.actor.can_arena())
                    {
                        commands.Add(Global.game_map.shops[unit.loc].face.Split('-')[0]);
                        IndexRedirect.Add(10);
                    }
                }
                // Secret shop/arena
                if (Global.game_map.secret_shops.ContainsKey(unit.loc) && unit.is_member())
                {
                    // Secret Shop
                    if (!Global.game_map.secret_shops[unit.loc].arena)
                    {
                        commands.Add(Global.game_map.secret_shops[unit.loc].face.Split('-')[0]);
                        IndexRedirect.Add(29);
                    }
                    // Secret Arena
                    else
                        if (unit.actor.can_arena())
                    {
                        commands.Add(Global.game_map.secret_shops[unit.loc].face.Split('-')[0]);
                        IndexRedirect.Add(30);
                    }
                }
                // Rescue/Take/Give/Drop
                if (Canto == Canto_Records.None ||
                    (!Canto.HasEnumFlag(Canto_Records.Give) && !Canto.HasEnumFlag(Canto_Records.Take)))
                {
                    if (unit.is_rescuing)
                    {
                        if (unit.can_drop())
                        {
                            commands.Add("Drop");
                            IndexRedirect.Add(2);
                        }
                        // Give
                        bool can_give = false;
                        foreach (int id in ally_range)
                        {
                            if (Global.game_map.units[id].different_team(unit))
                                continue;
                            if (Global.game_map.units[id].is_rescuing)
                                continue;
                            if (!Global.game_map.units[id].is_rescue_blocked() &&
                                    Global.game_map.units[id].can_rescue(Global.game_map.units[unit.rescuing]))
                                can_give = true;
                        }
                        if (can_give)
                        {
                            commands.Add("Give");
                            IndexRedirect.Add(6);
                        }
                    }
                    else if (ally_range.Count > 0)
                    {
                        // Rescue/Take
                        bool can_rescue = false, can_take = false;
                        foreach (int id in ally_range)
                        {
                            if (unit.can_rescue(Global.game_map.units[id]))
                                can_rescue = true;

                            // Prevent taking from other teams
                            if (Global.game_map.units[id].different_team(unit))
                                continue;
                            if (Global.game_map.units[id].is_rescuing)
                                if (unit.can_rescue(Global.game_map.units[Global.game_map.units[id].rescuing]))
                                    can_take = true;
                        }
                        if (can_rescue)
                        {
                            commands.Add("Rescue");
                            IndexRedirect.Add(2);
                            if (unit.is_rescue_blocked())
                                Disabled.Add(2);
                        }
                        if (can_take)
                        {
                            commands.Add("Take");
                            IndexRedirect.Add(6);
                            if (unit.is_rescue_blocked())
                                Disabled.Add(6);
                        }
                    }
                }
            }
            // Item
            if (CantoAllowsItem(Canto))
            {
                if (unit.actor.has_items)
                {
                    commands.Add("Item");
                    IndexRedirect.Add(3);
                }
            }
            // Trade
            if (CantoAllowsTrade(Canto))
            {
                if (unit.can_trade)
                {
                    commands.Add("Trade");
                    IndexRedirect.Add(4);
                }
            }
            else if (CantoAllowsTakeDrop(Canto))
            {
                if (unit.can_drop())
                {
                    commands.Add("Drop");
                    IndexRedirect.Add(2);
                }
            }
            if (!ContinuousMoveCanto(Canto) && !Canto.HasEnumFlag(Canto_Records.Supply))
            {
                if (unit.can_supply())
                {
                    // Supply
                    commands.Add("Supply");
                    IndexRedirect.Add(18);
                }
            }

            if (commands.Count == 0 && unit.cantoing)
            {
                // Wait
                AddWaitCommand(commands);
                // Status
                commands.Add("Status");
                IndexRedirect.Add(15);
            }
            else
            {
                // Status
                commands.Add("Status");
                IndexRedirect.Add(15);
                // Wait
                AddWaitCommand(commands);
            }

            AddSkillCommands(ref commands, unit);

            //@Debug: why?
            Input.clear_locked_repeats();

            return commands;
        }

        private void AddWaitCommand(List<string> commands)
        {
            if (!this.WaitBlocked)
            {
                commands.Add("Wait");
                IndexRedirect.Add(5);
            }
        }

        private bool WaitBlocked
        {
            get
            {
                //Yeti
                if (Scene_Map.intro_chapter_options_blocked())
                    return Global.game_map.teams[Constants.Team.ENEMY_TEAM].Count > 0;
                return false;
            }
        }

        private static bool ContinuousMoveCanto(Canto_Records canto)
        {
            if (canto == Canto_Records.Horse)
                return true;

            // Skills: Dash
            if (canto.HasEnumFlag(Canto_Records.Dash))
                return true;

            return false;
        }
        private static bool CantoAllowsNormalActions(Canto_Records canto)
        {
            if (ContinuousMoveCanto(canto))
                return false;
            if (canto.HasEnumFlag(Canto_Records.Take))
                return false;

            return true;
        }
        private static bool CantoAllowsTakeDrop(Canto_Records canto)
        {
            // Skills: Dash
            if (canto.HasEnumFlag(Canto_Records.Dash))
                return false;

            if (canto.HasEnumFlag(Canto_Records.Take))
                return true;
            return false;
        }
        private static bool CantoAllowsItem(Canto_Records canto)
        {
            if (!CantoAllowsNormalActions(canto))
            {
                // This can only be allowed if post-dash you can only equip, not use
                //if (!canto.HasEnumFlag(Canto_Records.Horse) && canto.HasEnumFlag(Canto_Records.Dash)) //Debug
                //    return true;

                return false;
            }

            return true;
        }
        private static bool CantoAllowsTrade(Canto_Records canto)
        {
            if (canto.HasEnumFlag(Canto_Records.Trade))
                return false;
            if (CantoAllowsItem(canto))
            {
                if (canto.HasEnumFlag(Canto_Records.Dash))
                    return false;
            }
            if (!CantoAllowsNormalActions(canto))
                return false;

            return true;
        }

        public bool CantoAllowsTrade()
        {
            return CantoAllowsTrade(Canto);
        }
        #endregion

        private Window_Command NewUnitWindow(List<string> commands, int width)
        {
            var window = new Window_Command(
                new Vector2(8 + (Global.player.is_on_left() ? (Config.WINDOW_WIDTH - (width + 16)) : 0), 24), width, commands);
            window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;

            for (int i = 0; i < commands.Count; i++)
            {
                // Trade, Give/Take
                if (IndexRedirect[i] == 4 || IndexRedirect[i] == 6)
                    window.set_text_color(i, "Green");
                // Supply
                if (IndexRedirect[i] == 18)
                    window.set_text_color(i, "Green");
                // Talk
                if (IndexRedirect[i] == 8 && Constants.Gameplay.TALKING_IS_FREE_ACTION)
                    window.set_text_color(i, "Green");

                // Disabled commands
                if (Disabled.Contains(IndexRedirect[i]))
                    window.set_text_color(i, "Grey");
            }

            // Automatically selects a specific menu option
            if (Global.game_temp.SelectedMoveMenuChoice.IsSomething)
            {
                if (IndexRedirect.Contains(Global.game_temp.SelectedMoveMenuChoice))
                    window.immediate_index =
                        IndexRedirect.IndexOf(Global.game_temp.SelectedMoveMenuChoice);
            }
            return window;
        }

        private void CreateCancelButton()
        {
            CreateCancelButton(
                Global.player.is_on_left() ? Config.WINDOW_WIDTH - (32 + 48) : 32,
                Config.MAPCOMMAND_WINDOW_DEPTH);
        }

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = base.CanceledTriggered(active);
            if (active)
            {
                // If right clicked or tapped on nothing in particular
                cancel |= Global.Input.mouse_click(MouseButtons.Right) ||
                    Global.Input.gesture_triggered(TouchGestures.Tap);
            }
            return cancel;
        }

        public int SelectedCommand
        {
            get
            {
                return IndexRedirect[Window.selected_index().ValueOrDefault];
            }
        }

        public int CommandAtIndex
        {
            get
            {
                return IndexRedirect[Window.index];
            }
        }
        public void MoveToCommand(int command)
        {
            int index = IndexRedirect.IndexOf(command);
            if (index >= 0)
                Window.index = index;
        }

        // This should probably be handled somewhere else? //@Debug
        public void RefreshTempAttackRange()
        {
            Game_Unit unit = Global.game_map.units[UnitId];
            List<int> item_indices = unit.weapon_indices(UseableWeapons);
            Global.game_temp.temp_attack_range = unit.get_weapon_range(
                item_indices, new HashSet<Vector2> { unit.loc });
            //Global.game_temp.temp_attack_range = Global.game_map.get_unit_range(new List<Vector2> { unit.loc },
            //    unit.min_range_absolute(), unit.max_range_absolute()); // this should care about the exact weapons, I think //Yeti
        }
        public void RefreshTempAttackRange(int itemIndex, string skill = "")
        {
            Game_Unit unit = Global.game_map.units[UnitId];
            var weapon = Global.data_weapons[unit.items[itemIndex].Id];

            Global.game_temp.temp_attack_range = Global.game_map.get_unit_range(
                new HashSet<Vector2> { unit.loc },
                unit.min_range(itemIndex, skill),
                unit.max_range(itemIndex, skill),
                weapon.range_blocked_by_walls());
        }

        // This should probably be handled somewhere else? //@Debug
        public void RefreshTempStaffRange()
        {
            Game_Unit unit = Global.game_map.units[UnitId];
            List<int> item_indices = unit.weapon_indices(UseableWeapons);
            Global.game_temp.temp_staff_range = unit.get_weapon_range(
                item_indices, new HashSet<Vector2> { unit.loc });
            //Global.game_temp.temp_staff_range = Global.game_map.get_unit_range(new List<Vector2> { unit.loc },
            //    unit.min_range_absolute(), unit.max_range_absolute()); // this should care about the exact weapons, I think //Yeti
        }
        public void RefreshTempStaffRange(int itemIndex)
        {
            Game_Unit unit = Global.game_map.units[UnitId];
            var weapon = Global.data_weapons[unit.items[itemIndex].Id];
            Global.game_temp.temp_staff_range = Global.game_map.get_unit_range(
                new HashSet<Vector2> { unit.loc },
                unit.min_range(itemIndex), unit.max_range(itemIndex),
                weapon.range_blocked_by_walls());
        }
    }
}
