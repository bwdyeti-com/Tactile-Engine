using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Menus;
using FEXNA.Menus.Map;
using FEXNA.Windows.Command;
using FEXNA.Windows.Command.Items;
using FEXNA.Windows.Map;
using FEXNA.Windows.Map.Items;
using FEXNA.Windows.Target;
using FEXNA.Windows.UserInterface.Command;
using FEXNA_Library;
using EnumExtension;
using FEXNAWeaponExtension;

namespace FEXNA
{
    [Flags]
    public enum Canto_Records : int
    {
        None = 0,
        Horse = 1 << 0,
        Trade = 1 << 1,
        Take = 1 << 2,
        Give = 1 << 3,
        Talk = 1 << 4,
        Supply = 1 << 5,
        Dash = 1 << 6, // Skills: Dash
    }
#if DEBUG
    partial class Scene_Map : IMapMenuHandler, IMapDebugMenuHandler
#else
    partial class Scene_Map : IMapMenuHandler
#endif
    {
        int Unit_Window_Index = 0;
        Window_Command Unit_Command_Window;

        protected MenuManager MapMenu;

        Window_Command_Item Item_Window;
        Window_Item_Options Item_Options;
        Window_Command Discard_Confirm;
        Window_Command_Item Attack_Item_Window, Staff_Item_Window, Dance_Item_Window;
        Window_Command_Item Discard_Window, Repair_Item_Window, AssembleItemWindow;
        Window_Trade Trade_Window;
        Window_Steal Steal_Window;
        Window_Command ConstructWindow;
        Window_Target_Unit Unit_Target_Window;
        Window_Target_Location Loc_Target_Window;
        Window_Status Status_Window;
        Window_Business Shop_Window;
        private Window_Supply Supply_Window;
        Parchment_Confirm_Window Map_Save_Confirm_Window;
        private Window_Ranking Ranking_Window;
        private Window_Minimap Minimap;
        private Button_Description CancelButton;

        int Item = 0; // Difference between different item lists (equip, attack, trade)
        // Canto situation record
        // 0 = no canto, 1 = horse canto,
        // 2 = take canto, 3 = horse take canto,
        // 4 = trade canto, 5 = horse trade canto
        // 6 = give canto, 7 = horse give canto
        // 8 = give and trade canto, 9 = horse give and trade canto
        Canto_Records Canto = Canto_Records.None;
        protected int Unit_Id;
        protected List<int> Index_Redirect;
        List<int> Useable_Weapons, Useable_Staves;
        protected bool Overwriting_Checkpoint = false;

        #region Accessors
        public bool manual_targeting
        {
            get
            {
                if (Unit_Target_Window != null)
                    return Unit_Target_Window.manual_targeting;
                if (Loc_Target_Window != null)
                    return Loc_Target_Window.manual_targeting;
                return false;
            }
        }
        #endregion

        public bool target_window_up
        {
            get
            {
                if (Unit_Target_Window != null && Unit_Target_Window.active)
                    return true;
                if (Loc_Target_Window != null && Loc_Target_Window.active)
                    return true;
                return false;
            }
        }
        public bool target_window_has_target
        {
            get
            {
                return Unit_Target_Window != null &&
                    !Unit_Target_Window.manual_targeting &&
                    Unit_Target_Window.has_target;
            }
        }
        public Vector2 target_window_target_loc
        {
            get
            {
                if (Unit_Target_Window != null)
                    return Unit_Target_Window.target_loc(Unit_Target_Window.target);
                if (Loc_Target_Window != null)
                    return Loc_Target_Window.target_loc(Loc_Target_Window.target);
                throw new NullReferenceException(
                    "Tried to get target window loc, but\nthere is no target window");
            }
        }
        public Vector2 target_window_last_target_loc
        {
            get
            {
                if (Unit_Target_Window != null)
                    return Unit_Target_Window.target_loc(
                        Unit_Target_Window.targets[Unit_Target_Window.LastTargetIndex]);
                if (Loc_Target_Window != null)
                    return Loc_Target_Window.target_loc(
                        Loc_Target_Window.targets[Loc_Target_Window.LastTargetIndex]);
                throw new NullReferenceException(
                    "Tried to get target window loc, but\nthere is no target window");
            }
        }

        public bool combat_target_window_up
        {
            get
            {
                return this.target_window_up &&
                    Unit_Target_Window != null &&
                    Unit_Target_Window is FEXNA.Windows.Target.Window_Target_Combat;
            }
        }

        public bool drop_target_window_up
        {
            get
            {
                return this.target_window_up &&
                    Unit_Target_Window != null &&
                    Unit_Target_Window is FEXNA.Windows.Target.Window_Target_Rescue &&
                    (Unit_Target_Window as FEXNA.Windows.Target.Window_Target_Rescue).mode == 1;
            }
        }

        protected void update_menu_calls()
        {
            update_preparations_menu_calls();
            if (Global.game_temp.shop_call)
                open_shop_menu();
            if (Global.game_temp.unit_menu_call)
                if (!Global.game_map.get_selected_unit().is_in_motion())
                {
                    call_unit_menu();
                    open_unit_menu();
                }
            if (Global.game_temp.map_menu_call)
            {
                call_map_menu();
                open_map_menu();
            }
            if (Global.game_temp.status_menu_call)
                open_status_menu();
            if (Global.game_temp.discard_menu_call && !is_map_popup_active())
                open_discard_menu();
            if (Global.game_temp.minimap_call && Minimap == null)
                open_minimap_menu();
            if (Global.game_temp.map_save_call)
                open_map_save();
            if (Global.game_temp.rankings_call)
                open_rankings();
        }

        protected void update_menu()
        {
            if (update_preparations())
            {
                if (Global.game_system.is_interpreter_running)
                    if (Discard_Window != null)
                    {
                        if (Discard_Window.visible)
                        {
                            Discard_Window.update();
                            update_cancel_button();
                            update_discard_menu();
                            return;
                        }
                        // If the inventory is still too full after discarding, the discard_window just goes inactive until the map popup closes
                        else if (!is_map_popup_active())
                        {
                            open_discard_menu();
                        }
                    }
                return;
            }
            if (update_menu_map())
                return;
            if (update_menu_unit())
                return;
            if (Status_Window != null)
            {
                Status_Window.Update(true);
                update_status_menu();
                return;
            }
            if (Shop_Window != null)
            {
                Shop_Window.Update(true);
                update_shop_menu();
                return;
            }
            if (Discard_Window != null)
            {
                if (Discard_Window.visible)
                {
                    Discard_Window.update();
                    update_cancel_button();
                    update_discard_menu();
                    return;
                }
                // If the inventory is still too full after discarding, the discard_window just goes inactive until the map popup closes
                else if (!is_map_popup_active())
                {
                    open_discard_menu();
                }
            }
            if (Minimap != null)
            {
                Minimap.update();
                update_minimap();
                return;
            }
            if (Map_Save_Confirm_Window != null)
            {
                Map_Save_Confirm_Window.update();
                update_map_save();
                return;
            }
            if (Ranking_Window != null)
            {
                Ranking_Window.update();
                update_rankings();
                return;
            }
        }

        protected virtual bool update_menu_map()
        {
            if (MapMenu != null)
            {
                MapMenu.Update();
                if (MapMenu != null && MapMenu.Finished)
                    MapMenu = null;
                return true;
            }

            return false;
        }

        protected virtual bool update_menu_unit()
        {
            if (is_unit_command_window_open)
            {
                update_cancel_button();

                if (unit_command_window_active)
                {
                    update_unit_command_window();
                    if (Unit_Window_Index != unit_command_window_index)
                        Global.game_map.range_start_timer = 0;
                    Unit_Window_Index = unit_command_window_index;
                    update_unit_command_menu();
                    return true;
                }
            }

            #region Item Window
            if (Item_Window != null)
            {
                if (Discard_Confirm != null && Discard_Confirm.active)
                {
                    Item_Window.update_graphics();
                    Discard_Confirm.update();
                    update_discard_confirm();
                    return true;
                }
                else if (Repair_Item_Window != null && Repair_Item_Window.active)
                {
                    Repair_Item_Window.update();
                    if (Unit_Window_Index != Repair_Item_Window.index)
                        Global.game_map.range_start_timer = 0;
                    Unit_Window_Index = Repair_Item_Window.index;
                    update_repair_item_menu();
                    return true;
                }
                else if (Item_Options != null && Item_Options.active)
                {
                    Item_Window.update_graphics();
                    Item_Options.update();
                    update_item_options();
                    return true;
                }
                else if (Item_Window.active)
                {
                    Item_Window.update();
                    if (Unit_Window_Index != Item_Window.index)
                        Global.game_map.range_start_timer = 0;
                    Unit_Window_Index = Item_Window.index;
                    update_item_menu();
                    return true;
                }
            }
            #endregion
            #region Attack Window
            if (Attack_Item_Window != null)
            {
                if (Attack_Item_Window.active)
                {
                    Attack_Item_Window.update();
                    if (Unit_Window_Index != Attack_Item_Window.index &&
                        Attack_Item_Window is Window_Command_Item_Attack)
                    {
                        Game_Unit unit = Global.game_map.units[Unit_Id];
                        Global.game_map.range_start_timer = 0;
                        Global.game_temp.temp_attack_range = Global.game_map.get_unit_range(new HashSet<Vector2> { unit.loc },
                            unit.min_range(Attack_Item_Window.redirect(), ((Window_Command_Item_Attack)Attack_Item_Window).skill),
                            unit.max_range(Attack_Item_Window.redirect(), ((Window_Command_Item_Attack)Attack_Item_Window).skill),
                            Global.data_weapons[unit.items[Attack_Item_Window.redirect()].Id].range_blocked_by_walls());
                    }
                    Unit_Window_Index = Attack_Item_Window.index;
                    update_attack_item_menu();
                    return true;
                }
                if (Unit_Target_Window != null)
                {
                    Unit_Target_Window.update();
                    update_attack_target_menu();
                    return true;
                }
            }
            #endregion
            #region Staff Window
            if (Staff_Item_Window != null)
            {
                if (Staff_Item_Window.active)
                {
                    Staff_Item_Window.update();
                    if (Unit_Window_Index != Staff_Item_Window.index)
                    {
                        Game_Unit unit = Global.game_map.units[Unit_Id];
                        Global.game_map.range_start_timer = 0;
                        Global.game_temp.temp_staff_range = Global.game_map.get_unit_range(
                            new HashSet<Vector2> { unit.loc },
                            unit.min_range(Staff_Item_Window.redirect()),
                            unit.max_range(Staff_Item_Window.redirect()),
                            Global.data_weapons[unit.items[Staff_Item_Window.redirect()].Id].range_blocked_by_walls());
                    }
                    Unit_Window_Index = Staff_Item_Window.index;
                    update_staff_item_menu();
                    return true;
                }
                if (Unit_Target_Window != null)
                {
                    Unit_Target_Window.update();
                    update_staff_target_menu();
                    return true;
                }
            }
            #endregion
            #region Dance Window
            if (Dance_Item_Window != null && Dance_Item_Window.active)
            {
                Dance_Item_Window.update();
                update_dance_item_menu();
                return true;
            }
            #endregion
            #region Construct Window
            if (ConstructWindow != null)
            {
                if (AssembleItemWindow != null && AssembleItemWindow.active)
                {
                    AssembleItemWindow.update();
                    update_assemble_item_menu();
                    return true;
                }
                else if (ConstructWindow.active)
                {
                    ConstructWindow.update();
                    update_construct_menu();
                    return true;
                }
            }
            #endregion
            #region Target Window
            if (Unit_Target_Window != null)
            {
                if (Unit_Target_Window is Window_Target_Rescue && Unit_Target_Window.active)
                {
                    Unit_Target_Window.update();
                    update_rescue_target_menu();
                    return true;
                }
                else if (Unit_Target_Window is Window_Target_Trade)
                {
                    if (Unit_Target_Window is Window_Target_Steal)
                    {
                        if (Unit_Target_Window.active)
                        {
                            Unit_Target_Window.update();
                            update_steal_target_menu();
                            return true;
                        }
                        if (Steal_Window.active)
                        {
                            Steal_Window.update();
                            update_steal_menu();
                            return true;
                        }
                    }
                    else
                    {
                        if (Unit_Target_Window.active)
                        {
                            Unit_Target_Window.update();
                            update_trade_target_menu();
                            return true;
                        }
                        if (Trade_Window.active)
                        {
                            Trade_Window.update();
                            update_trade_menu();
                            return true;
                        }
                    }
                }
                else if (Unit_Target_Window is Window_Target_Dance)
                {
                    if (Unit_Target_Window.active)
                    {
                        Unit_Target_Window.update();
                        update_dance_target_menu();
                        return true;
                    }
                }
                else if (Unit_Target_Window is Window_Target_Sacrifice)
                {
                    if (Unit_Target_Window.active)
                    {
                        Unit_Target_Window.update();
                        update_sacrifice_target_menu();
                        return true;
                    }
                }
                else if (Unit_Target_Window is Window_Target_Talk)
                {
                    if (Unit_Target_Window is Window_Target_Support)
                    {
                        if (Unit_Target_Window.active)
                        {
                            Unit_Target_Window.update();
                            update_support_target_menu();
                            return true;
                        }
                    }
                    else
                    {
                        if (Unit_Target_Window.active)
                        {
                            Unit_Target_Window.update();
                            update_talk_target_menu();
                            return true;
                        }
                    }
                }
            }
            if (Loc_Target_Window != null)
            {
                if (Loc_Target_Window is Window_Target_Door)
                {
                    Loc_Target_Window.update();
                    update_door_target_menu();
                    return true;
                }
                else if (Loc_Target_Window is Window_Target_Construct)
                {
                    Loc_Target_Window.update();
                    update_assemble_target_menu();
                    return true;
                }
            }
            #endregion
            if (Supply_Window != null)
            {
                Supply_Window.Update(true);
                update_supply_menu();
                return true;
            }
            return false;
        }

        #region Unit Command Menu
        protected void call_unit_menu()
        {
            Global.game_temp.menuing = true;
            Global.game_temp.menu_call = false;
            Global.game_temp.unit_menu_call = false;
            Unit_Id = Global.game_system.Selected_Unit_Id;
            Global.game_system.play_se(System_Sounds.Open);
        }

        internal static bool continuous_move_canto(Canto_Records canto)
        {
            if (canto == Canto_Records.Horse)
                return true;

            // Skills: Dash
            if (canto.HasEnumFlag(Canto_Records.Dash))
                return true;

            return false;
        }
        internal static bool canto_allows_normal_actions(Canto_Records canto)
        {
            if (continuous_move_canto(canto))
                return false;
            if (canto.HasEnumFlag(Canto_Records.Take))
                return false;

            return true;
        }
        internal static bool canto_allows_take_drop(Canto_Records canto)
        {
            // Skills: Dash
            if (canto.HasEnumFlag(Canto_Records.Dash))
                return false;

            if (canto.HasEnumFlag(Canto_Records.Take))
                return true;
            return false;
        }
        internal static bool canto_allows_item(Canto_Records canto)
        {
            if (!canto_allows_normal_actions(canto))
            {
                // This can only be allowed if post-dash you can only equip, not use
                //if (!canto.HasEnumFlag(Canto_Records.Horse) && canto.HasEnumFlag(Canto_Records.Dash)) //Debug
                //    return true;

                return false;
            }

            return true;
        }
        internal static bool canto_allows_trade(Canto_Records canto)
        {
            if (canto.HasEnumFlag(Canto_Records.Trade))
                return false;
            if (canto_allows_item(canto))
            {
                if (canto.HasEnumFlag(Canto_Records.Dash))
                    return false;
            }
            if (!canto_allows_normal_actions(canto))
                return false;

            return true;
        }

        protected void open_unit_menu()
        {
            open_unit_menu(Global.game_system.Menu_Canto);
        }
        protected virtual void open_unit_menu(Canto_Records canto)
        {
            Global.game_system.Menu_Canto = Canto_Records.None;
            List<string> commands = new List<string>();
            List<int> disabled = new List<int>();
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
            Index_Redirect = new List<int>();
            Game_Unit unit = Global.game_map.units[Unit_Id];
            unit.actor.remove_broken_items();

            // If canto state is null but the unit is cantoing, set to horse canto
            if (canto == Canto_Records.None && Canto == Canto_Records.None)
                Canto = (unit.cantoing && unit.has_canto() ? Canto_Records.Horse : Canto_Records.None);
            else if (canto != Canto_Records.None)
                Canto = canto;

            if (canto_allows_normal_actions(Canto))
            {
                List<int>[] range_ary;
                // enemies: attack
                range_ary = unit.enemies_in_range();
                List<int> enemy_range = range_ary[0];
                Useable_Weapons = range_ary[1];
                // enemies: staves
                Useable_Staves = new List<int>();
                range_ary = unit.enemies_in_staff_range();
                Useable_Staves.AddRange(range_ary[1]);
                // allies: staves
                range_ary = unit.allies_in_staff_range();
                Useable_Staves.AddRange(range_ary[1]);
                // other: staves
                range_ary = unit.untargeted_staff_range();
                List<int> other_staff_range = range_ary[0];
                Useable_Staves.AddRange(range_ary[1]);
                Useable_Staves = Useable_Staves.Distinct().ToList(); //ListOrEquals
                // allies
                List<int> ally_range = unit.allies_in_range(1);
                // Attack
                if (Useable_Weapons.Count > 0)
                {
                    List<int> item_indices = unit.weapon_indices(Useable_Weapons);
                    Global.game_temp.temp_attack_range = unit.get_weapon_range(item_indices, new HashSet<Vector2> { unit.loc });
                    //Global.game_temp.temp_attack_range = Global.game_map.get_unit_range(new List<Vector2> { unit.loc },
                    //    unit.min_range_absolute(), unit.max_range_absolute()); // this should care about the exact weapons, I think //Yeti
                    Global.game_map.range_start_timer = 0;
                    commands.Add("Attack");
                    Index_Redirect.Add(0);
                }
                // Staff
                if (Useable_Staves.Count > 0)
                {
                    List<int> item_indices = unit.weapon_indices(Useable_Staves);
                    Global.game_temp.temp_staff_range = unit.get_weapon_range(item_indices, new HashSet<Vector2> { unit.loc });
                    //Global.game_temp.temp_staff_range = Global.game_map.get_unit_range(new List<Vector2> { unit.loc },
                    //    unit.min_range_absolute(true), unit.max_range_absolute(true)); // this should care about the exact weapons, I think //Yeti
                    Global.game_map.range_start_timer = 0;
                    commands.Add("Staff");
                    Index_Redirect.Add(1);
                }
                // Seize
                if (Global.game_map.get_seize_points(unit.team, unit.group).Contains(unit.loc) && unit.can_seize)
                {
                    commands.Add("Seize");
                    Index_Redirect.Add(14);
                }
                // Escape
                if (Global.game_map.escape_point_locations(unit.team, unit.group)
                    .Contains(unit.loc) && unit.can_seize)
                {
                    commands.Add("Escape");
                    Index_Redirect.Add(19);
                }
                // Dance
                if (unit.can_dance())
                    if (unit.dance_targets().Count > 0)
                    {
                        commands.Add(unit.dance_name());
                        Index_Redirect.Add(16);
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
                    Index_Redirect.Add(20);
                }
                // Steal
                if (unit.can_steal())
                    if (unit.steal_targets().Count > 0)
                    {
                        commands.Add("Steal");
                        Index_Redirect.Add(13);
                        if (unit.actor.is_full_items)
                            disabled.Add(13);
                    }
                // Support
                if (unit.can_support())
                {
                    commands.Add("Support");
                    Index_Redirect.Add(17);
                }
                // Talk
                if (!Canto.HasEnumFlag(Canto_Records.Talk) && unit.can_talk())
                {
                    Global.game_temp.temp_talk_range.Clear();
                    foreach (int id in unit.talk_targets())
                        Global.game_temp.temp_talk_range.Add(Global.game_map.units[id].loc);
                    Global.game_map.range_start_timer = 0;
                    commands.Add("Talk");
                    Index_Redirect.Add(8);
                }
                // Visit
                if (unit.can_visit())
                {
                    string visit_name = Global.game_map.visit_locations[unit.loc].Name;
                    commands.Add(!string.IsNullOrEmpty(visit_name) ? visit_name : "Visit");
                    Index_Redirect.Add(7);
                }
                // Chest
                if (unit.can_open_chest(true))
                    if (Global.game_map.chest_locations.ContainsKey(unit.loc))
                    {
                        commands.Add("Chest");
                        Index_Redirect.Add(11);
                    }
                // Door
                if (unit.can_open_door(true))
                    if (unit.door_targets().Count > 0)
                    {
                        commands.Add("Door");
                        Index_Redirect.Add(12);
                    }
                // Shop/arena
                if (Global.game_map.shops.ContainsKey(unit.loc))
                {
                    // Shop
                    if (!Global.game_map.shops[unit.loc].arena)
                    {
                        commands.Add(Global.game_map.shops[unit.loc].face.Split('-')[0]);
                        Index_Redirect.Add(9);
                    }
                    // Arena
                    else
                        if (unit.actor.can_arena())
                        {
                            commands.Add(Global.game_map.shops[unit.loc].face.Split('-')[0]);
                            Index_Redirect.Add(10);
                        }
                }
                // Secret shop/arena
                if (Global.game_map.secret_shops.ContainsKey(unit.loc) && unit.is_member())
                {
                    // Secret Shop
                    if (!Global.game_map.secret_shops[unit.loc].arena)
                    {
                        commands.Add(Global.game_map.secret_shops[unit.loc].face.Split('-')[0]);
                        Index_Redirect.Add(29);
                    }
                    // Secret Arena
                    else
                        if (unit.actor.can_arena())
                        {
                            commands.Add(Global.game_map.secret_shops[unit.loc].face.Split('-')[0]);
                            Index_Redirect.Add(30);
                        }
                }
                // Rescue/Take/Give/Drop
                if (Canto == Canto_Records.None ||
                    (!Canto.HasEnumFlag(Canto_Records.Give) && !Canto.HasEnumFlag(Canto_Records.Take)))
                {
                    if (unit.is_rescuing)
                    {
                        // Drop
                        //bool can_drop = false; //Debug
                        /*foreach (Vector2 loc in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
                            if (!Global.game_map.is_off_map(loc + unit.loc))
                                if (!Global.game_map.is_blocked(loc + unit.loc, unit.rescuing))
                                    if (Pathfinding.passable(Global.game_map.units[unit.rescuing], loc + unit.loc))
                                    can_drop = true;*/
                        if (unit.can_drop())
                        {
                            commands.Add("Drop");
                            Index_Redirect.Add(2);
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
                            Index_Redirect.Add(6);
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
                            Index_Redirect.Add(2);
                            if (unit.is_rescue_blocked())
                                disabled.Add(2);
                        }
                        if (can_take)
                        {
                            commands.Add("Take");
                            Index_Redirect.Add(6);
                            if (unit.is_rescue_blocked())
                                disabled.Add(6);
                        }
                    }
                }
            }
            // Item
            if (canto_allows_item(Canto))
            {
                if (unit.actor.has_items)
                {
                    commands.Add("Item");
                    Index_Redirect.Add(3);
                }
            }
            // Trade
            if (canto_allows_trade(Canto))
            {
                if (unit.can_trade)
                {
                    commands.Add("Trade");
                    Index_Redirect.Add(4);
                }
            }
            else if (canto_allows_take_drop(Canto))
            {
                // Take to Drop
                /*bool can_drop = false; //Debug
                foreach (Vector2 loc in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
                    if (!Global.game_map.is_off_map(loc + unit.loc))
                        if (!Global.game_map.is_blocked(loc + unit.loc, unit.rescuing))
                            if (Pathfinding.passable(Global.game_map.units[unit.rescuing], loc + unit.loc))
                                can_drop = true;*/
                if (unit.can_drop())
                {
                    commands.Add("Drop");
                    Index_Redirect.Add(2);
                }
            }
            if (!continuous_move_canto(Canto) && !Canto.HasEnumFlag(Canto_Records.Supply))
            {
                if (unit.can_supply())
                {
                    // Supply
                    commands.Add("Supply");
                    Index_Redirect.Add(18);
                }
            }

            if (commands.Count == 0 && unit.cantoing)
            {
                // Wait
                add_wait_command(commands);
                // Status
                commands.Add("Status");
                Index_Redirect.Add(15);
            }
            else
            {
                // Status
                commands.Add("Status");
                Index_Redirect.Add(15);
                // Wait
                add_wait_command(commands);
            }

            skill_commands(ref commands, unit);

            Input.clear_locked_repeats();
            new_unit_command_window(commands, 56);
            for (int i = 0; i < commands.Count; i++)
            {
                // Trade, Give/Take
                if (Index_Redirect[i] == 4 || Index_Redirect[i] == 6)
                    Unit_Command_Window.set_text_color(i, "Green");
                // Supply
                if (Index_Redirect[i] == 18)
                    Unit_Command_Window.set_text_color(i, "Green");
                // Talk
                if (Index_Redirect[i] == 8 && Constants.Gameplay.TALKING_IS_FREE_ACTION)
                    Unit_Command_Window.set_text_color(i, "Green");

                if (disabled.Contains(Index_Redirect[i]))
                    Unit_Command_Window.set_text_color(i, "Grey");
            }
        }

        private void add_wait_command(List<string> commands)
        {
            if (!this.wait_blocked)
            {
                commands.Add("Wait");
                Index_Redirect.Add(5);
            }
        }

        private bool wait_blocked
        {
            get
            {
                //Yeti
                if (Scene_Map.intro_chapter_options_blocked())
                    return Global.game_map.teams[Constants.Team.ENEMY_TEAM].Count > 0;
                return false;
            }
        }

        protected virtual void update_unit_command_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (unit_command_is_canceled) //Yeti Global.Input.triggered(Inputs.B)) //Yeti
            {
                if (command_skill_cancel_unit_command(unit))
                {
                    return;
                }

                // If horse canto, cancels to wait only option
                if (Canto != Canto_Records.Horse && Canto.HasEnumFlag(Canto_Records.Horse))
                    Canto = Canto_Records.Horse;

                // If not in a foot unit canto
                if (Canto == Canto_Records.None || Canto == Canto_Records.Horse)
                {
                    cancel_unit_command(unit);

                    return;
                }
                else
                    Global.game_system.play_se(System_Sounds.Buzzer);
            }
            else if (unit_command_is_selected) //Global.Input.triggered(Inputs.A)) //Yeti
            {
                unit_menu_select(Index_Redirect[unit_command_window_selected_index], unit);
            }
        }

        private void cancel_unit_command(Game_Unit unit, bool clear_canto = true)
        {
            Global.game_temp.menuing = false;
            Global.game_map.get_selected_unit().command_menu_close();
            close_unit_menu(true, clear_canto);

            // If the unit has no move range, don't bother making it visible
            if (unit.mov <= 0 && Canto == Canto_Records.None)
            {
                unit.cancel_move();
                Global.game_system.Selected_Unit_Id = -1;
                Global.game_map.highlight_test();
                Global.game_system.play_se(System_Sounds.Cancel);
            }
            else
                Global.game_system.play_se(System_Sounds.Unit_Select);
        }

        protected virtual void unit_menu_select(int option, Game_Unit unit)
        {
            if (unit_menu_select_skill(option, unit))
                return;
            switch (option)
            {
                #region 0: Attack
                case 0:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    unit_command_window_active = false;
                    unit_command_window_visible = false;
                    Global.game_map.range_start_timer = 0;
                    Attack_Item_Window = new Window_Command_Item_Attack(Unit_Id, new Vector2(24, 8));
                    Attack_Item_Window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                    Attack_Item_Window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                    Attack_Item_Window.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
                    Global.game_temp.temp_attack_range = Global.game_map.get_unit_range(new HashSet<Vector2> { unit.loc },
                        unit.min_range(Attack_Item_Window.redirect()), unit.max_range(Attack_Item_Window.redirect()),
                        Global.data_weapons[unit.items[Attack_Item_Window.redirect()].Id].range_blocked_by_walls());
                    break;
                #endregion
                #region 1: Staff
                case 1:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    unit_command_window_active = false;
                    unit_command_window_visible = false;
                    Global.game_map.range_start_timer = 0;
                    Staff_Item_Window = new Window_Command_Item_Staff(Unit_Id, new Vector2(24, 8));
                    Staff_Item_Window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                    Staff_Item_Window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                    Staff_Item_Window.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
                    Global.game_temp.temp_staff_range = Global.game_map.get_unit_range(
                        new HashSet<Vector2> { unit.loc },
                        unit.min_range(Staff_Item_Window.redirect()),
                        unit.max_range(Staff_Item_Window.redirect()),
                        Global.data_weapons[unit.items[Staff_Item_Window.redirect()].Id].range_blocked_by_walls());
                    break;
                #endregion
                #region 2: Rescue/Drop
                case 2:
                    if (!unit.is_rescuing && unit.is_rescue_blocked())
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Unit_Target_Window = new Window_Target_Rescue(
                            unit.id, unit.is_rescuing ? 1 : 0, new Vector2(4, 0));
                        unit_command_window_active = false;
                        unit_command_window_visible = false;
                        Global.player.facing = 4;
                        Global.player.update_cursor_frame();
                    }
                    break;
                #endregion
                #region 3: Item
                case 3: //
                    Global.game_system.play_se(System_Sounds.Confirm);
                    unit_command_window_active = false;
                    unit_command_window_visible = false;
                    Global.game_map.range_start_timer = 0;
                    Item_Window = new Window_Command_Item(Unit_Id, new Vector2(24, 8));
                    Item_Window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                    Item_Window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                    Item_Window.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
                    break;
                #endregion
                #region 4: Trade
                case 4:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Unit_Target_Window = new Window_Target_Trade(unit.id, new Vector2(4, 0));
                    unit_command_window_active = false;
                    unit_command_window_visible = false;
                    Global.player.facing = 4;
                    Global.player.update_cursor_frame();
                    break;
                #endregion
                #region 5: Wait
                case 5:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    unit.start_wait();
                    Global.game_temp.menuing = false;
                    close_unit_menu(true);
                    suspend();
                    break;
                #endregion
                #region 6: Take/Give
                case 6:
                    if (!unit.is_rescuing && unit.is_rescue_blocked())
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Unit_Target_Window = new Window_Target_Rescue(unit.id, unit.is_rescuing ? 3 : 2, new Vector2(0, 0));
                        unit_command_window_active = false;
                        unit_command_window_visible = false;
                        Global.player.facing = 4;
                        Global.player.update_cursor_frame();
                    }
                    break;
                #endregion
                #region 7, 11: Visit, Chest
                case 7:
                case 11:
                    Global.game_system.play_se(System_Sounds.Confirm);

                    Global.game_map.remove_updated_move_range(Unit_Id);
                    Global.game_map.clear_move_range();
                    Global.game_map.range_start_timer = 0;
                    Global.game_state.call_visit(option == 7 ? FEXNA.State.Visit_Modes.Visit : FEXNA.State.Visit_Modes.Chest, Unit_Id, unit.loc);
                    Global.game_temp.menuing = false;
                    close_unit_menu(true);

                    unit.cantoing = false;
                    // Lock in unit movement
                    unit.moved();
                    if (unit.has_canto() && !unit.full_move())
                        unit.cantoing = true;
                    break;
                #endregion
                #region 8: Talk
                case 8:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Unit_Target_Window = new Window_Target_Talk(unit.id, new Vector2(4, 0));
                    unit_command_window_active = false;
                    unit_command_window_visible = false;
                    Global.player.facing = 4;
                    Global.player.update_cursor_frame();
                    break;
                #endregion
                #region 9, 10, 29, 30: Shop, Arena
                case 9:
                case 10:
                case 29:
                case 30:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Global.game_temp.call_shop(option == 29 || option == 30);
                    Global.game_system.Shopper_Id = Unit_Id;
                    Global.game_system.Shop_Loc = unit.loc;
                    Global.game_temp.menuing = false;
                    close_unit_menu(true, false);
                    Global.game_map.move_range_visible = false;
                    break;
                #endregion
                #region 12: Door
                case 12:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Loc_Target_Window = new Window_Target_Door(unit.id, new Vector2(4, 0));
                    unit_command_window_active = false;
                    unit_command_window_visible = false;
                    Global.player.facing = 4;
                    Global.player.update_cursor_frame();
                    break;
                #endregion
                #region 13: Steal
                case 13:
                    if (unit.actor.is_full_items)
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        //Audio.play_se("System_Sounds", "Help_Open"); //Yeti
                        //open help window saying you're full, lock command_window until help window is cleared //Yeti
                    }
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Unit_Target_Window = new Window_Target_Steal(unit.id, new Vector2(4, 0));
                        unit_command_window_active = false;
                        unit_command_window_visible = false;
                        Global.player.facing = 4;
                        Global.player.update_cursor_frame();
                    }
                    break;
                #endregion
                #region 14: Seize
                case 14:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Global.game_map.seize_point(unit.team, unit.loc);
                    unit.start_wait();
                    Global.game_system.Selected_Unit_Id = -1;
                    Global.game_temp.menuing = false;
                    close_unit_menu(true);
                    suspend();
                    break;
                #endregion
                #region 15: Status
                case 15:
                    unit_command_window_visible = false;
                    unit_command_window_active = false;
                    Global.game_temp.status_unit_id = Unit_Id;
                    Global.game_temp.status_team = unit.team;
                    open_status_menu();
                    break;
                #endregion
                #region 16: Dance
                case 16:
                    if (unit.has_dancer_ring())
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        unit_command_window_active = false;
                        unit_command_window_visible = false;
                        Dance_Item_Window = new Window_Command_Item_Dance(Unit_Id, new Vector2(24, 8));
                        Dance_Item_Window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                        Dance_Item_Window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                        Dance_Item_Window.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
                    }
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Unit_Target_Window = new Window_Target_Dance(unit.id, false, new Vector2(4, 0));
                        unit_command_window_active = false;
                        unit_command_window_visible = false;
                        Global.player.facing = 4;
                        Global.player.update_cursor_frame();
                    }
                    break;
                #endregion
                #region 17: Support
                case 17:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Unit_Target_Window = new Window_Target_Support(unit.id, new Vector2(4, 0));
                    unit_command_window_active = false;
                    unit_command_window_visible = false;
                    Global.player.facing = 4;
                    Global.player.update_cursor_frame();
                    break;
                #endregion
                #region 18: Supply
                case 18:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    unit_command_window_visible = false;
                    unit_command_window_active = false;
                    Supply_Window = new Window_Supply(unit);
                    break;
                #endregion
                #region 19: Escape
                case 19:
                    Global.game_system.play_se(System_Sounds.Confirm);

                    Global.game_map.remove_updated_move_range(Unit_Id);
                    Global.game_map.clear_move_range();
                    Global.game_map.range_start_timer = 0;
                    Global.game_state.call_visit(FEXNA.State.Visit_Modes.Escape, Unit_Id, unit.loc);
                    Global.game_temp.menuing = false;
                    close_unit_menu(true);
                    Global.game_system.Selected_Unit_Id = -1;
                    
                    unit.cantoing = false;
                    // Lock in unit movement
                    unit.moved();
                    if (unit.has_canto() && !unit.full_move())
                        unit.cantoing = true;
                    break;
                #endregion
                #region 20: Construct
                case 20:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    unit_command_window_active = false;
                    unit_command_window_visible = false;

                    int width = 72;
                    ConstructWindow = new Window_Command(
                        new Vector2(8 + (Global.player.is_on_left() ?
                            (Config.WINDOW_WIDTH - (width + 16)) : 0), 24),
                            width, new List<string> { "Assemble", "Reload", "Reclaim" });
                    ConstructWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                    if (!(unit.can_assemble() && unit.assemble_targets().Count > 0))
                        ConstructWindow.set_text_color(0, "Grey");
                    if (!(unit.can_reload() && unit.reload_targets().Count > 0))
                        ConstructWindow.set_text_color(1, "Grey");
                    if (!(unit.can_reclaim() && unit.reclaim_targets().Count > 0))
                        ConstructWindow.set_text_color(2, "Grey");
                    break;
                #endregion
            }
        }

        #region Attack
        protected void update_attack_item_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Attack_Item_Window.is_help_active)
            {
                if (Attack_Item_Window.is_canceled() || cancel_button_triggered)
                    Attack_Item_Window.close_help();
            }
            else
            {
                if (Attack_Item_Window.getting_help())
                    Attack_Item_Window.open_help();
                else if (Attack_Item_Window.is_canceled() || cancel_button_triggered)
                {
                    cancel_attack();
                    Attack_Item_Window.restore_equipped();
                    unit.actor.staff_fix();
                    List<int> item_indices = unit.weapon_indices(Useable_Weapons);
                    Global.game_temp.temp_attack_range = unit.get_weapon_range(item_indices, new HashSet<Vector2> { unit.loc });
                    //Global.game_temp.temp_attack_range = Global.game_map.get_unit_range(new List<Vector2> { unit.loc },
                    //    unit.min_range_absolute(), unit.max_range_absolute()); // this should care about the exact weapons, I think //Yeti
                    Global.game_map.range_start_timer = 0;
                    Global.game_system.play_se(System_Sounds.Cancel);
                    unit_command_window_visible = true;
                    unit_command_window_active = true;
                    Attack_Item_Window = null;
                    return;
                }
                else if (Attack_Item_Window.is_selected())
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    unit.equip(Attack_Item_Window.redirect() + 1);
                    unit.actor.organize_items();

                    unit.using_siege_engine = Attack_Item_Window.redirect() ==
                        Siege_Engine.SIEGE_INVENTORY_INDEX;
                    int item_index = unit.using_siege_engine ? Attack_Item_Window.redirect() : 0;

                    if (Global.game_options.combat_window == 1)
                        Unit_Target_Window = new Window_Target_Combat_Detail(
                            Unit_Id, item_index, new Vector2(4, 4),
                            (Attack_Item_Window as Window_Command_Item_Attack).skill);
                    else
                        Unit_Target_Window = new Window_Target_Combat(
                            Unit_Id, item_index, new Vector2(4, 4),
                            (Attack_Item_Window as Window_Command_Item_Attack).skill);
                    Attack_Item_Window.active = false;
                    Attack_Item_Window.visible = false;
                    Global.player.facing = 4;
                    Global.player.update_cursor_frame();
                }
            }
        }

        protected void cancel_attack()
        {
            // Skills: Swoop
            Global.game_map.units[Unit_Id].swoop_activated = false;
            // Skills: Trample
            Global.game_map.units[Unit_Id].trample_activated = false;
            // Skills: Old Swoop //Debug
            Global.game_map.units[Unit_Id].old_swoop_activated = false;
            // Skills: Masteries
            Global.game_map.units[Unit_Id].reset_masteries();
        }

        protected void update_attack_target_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (false) // Attack_Item_Window.is_getting_help() //Yeti
            {

            }
            else
            {
                if (Unit_Target_Window.is_canceled() || cancel_button_triggered) //Yeti
                {
                    Unit_Target_Window.cancel();
                    Global.game_map.range_start_timer = 0;
                    Global.game_system.play_se(System_Sounds.Cancel);
                    // Sends cursor back to unit
                    Global.player.instant_move = true;
                    Global.player.loc = unit.loc;
                    // Makes cursor invisible again
                    Global.player.facing = 6;
                    Global.player.frame = 1;
                    unit.using_siege_engine = false;
                    // Skills
                    if (!reopen_skill_attack_target_window(unit))
                    {
                        if (unit.called_masteries.Any())
                            Attack_Item_Window = new Window_Command_Item_Attack(Unit_Id, new Vector2(24, 8),
                                unit.called_masteries.First());
                        else
                            Attack_Item_Window = new Window_Command_Item_Attack(Unit_Id, new Vector2(24, 8));
                    }
                    Attack_Item_Window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                    Attack_Item_Window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                    Attack_Item_Window.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
                    Unit_Target_Window = null;
                    return;
                }
                else if (Unit_Target_Window.is_selected())
                {
                    Unit_Target_Window.accept();
                    confirm_combat();
                }
            }
        }

        protected void confirm_combat()
        {
            Global.game_system.play_se(System_Sounds.Confirm); // was there a reason this didn't exist here in FEXP //Yeti
            //Global.game_system.Selected_Unit_Id = -1; //Debug
            Global.game_map.clear_move_range();
            // Lock in unit movement
            Game_Unit unit = Global.game_map.units[Unit_Id];
            unit.moved();
            if (unit.trample_activated)
                unit.set_trample_loc();
            if (unit.old_swoop_activated)
            {
                Global.game_state.call_battle(Unit_Id,
                    unit.enemies_in_old_swoop_range(unit.facing)[0]);
                sort_aoe_targets();
                // Adds to list of attacked units for this turn
                foreach (int id in Global.game_system.Aoe_Targets)
                    if (Global.game_map.units.ContainsKey(id) &&
                            unit.is_attackable_team(Global.game_map.units[id]))
                        unit.add_attack_target(id);

                unit.same_target_support_gain_display(Global.game_system.Aoe_Targets);
            }
            else
            {
                Global.game_state.call_battle(Unit_Id, Unit_Target_Window.target);
                // Adds to list of attacked units for this turn
                if (Global.game_map.units.ContainsKey(Global.game_system.Battler_2_Id) &&
                        unit.is_attackable_team(Global.game_map.units[Global.game_system.Battler_2_Id]))
                {
                    unit.add_attack_target(Global.game_system.Battler_2_Id);

                    unit.same_target_support_gain_display(Global.game_system.Battler_2_Id);
                }
            }

            Global.game_temp.clear_temp_range();
            Global.game_temp.menuing = false;
            close_unit_menu(true);
            Attack_Item_Window = null;
            Unit_Target_Window = null;
        }

        protected void sort_aoe_targets()
        {
            Global.game_system.Aoe_Targets.Sort(delegate(int a, int b) {
                return Global.game_map.unit_distance(Unit_Id, a) - Global.game_map.unit_distance(Unit_Id, b); });
        }
        #endregion

        #region Staff
        protected void update_staff_item_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Staff_Item_Window.is_help_active)
            {
                if (Staff_Item_Window.is_canceled() || cancel_button_triggered)
                    Staff_Item_Window.close_help();
            }
            else
            {
                if (Staff_Item_Window.getting_help())
                    Staff_Item_Window.open_help();
                else if (Staff_Item_Window.is_canceled() || cancel_button_triggered)
                {
                    cancel_staff();
                    Staff_Item_Window.restore_equipped();
                    unit.actor.staff_fix();
                    List<int> item_indices = unit.weapon_indices(Useable_Staves);
                    Global.game_temp.temp_staff_range = unit.get_weapon_range(item_indices, new HashSet<Vector2> { unit.loc });
                    //Global.game_temp.temp_staff_range = Global.game_map.get_unit_range(new List<Vector2> { unit.loc },
                    //    unit.min_range_absolute(true), unit.max_range_absolute(true)); // this should care about the exact weapons, I think //Yeti
                    Global.game_map.range_start_timer = 0;
                    Global.game_system.play_se(System_Sounds.Cancel);
                    unit_command_window_visible = true;
                    unit_command_window_active = true;
                    Staff_Item_Window = null;
                    return;
                }
                else if (Staff_Item_Window.is_selected())
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    unit.equip(Staff_Item_Window.redirect() + 1);
                    unit.actor.organize_items();
                    Unit_Target_Window = new Window_Target_Staff(Unit_Id, 0, new Vector2(0, 0));
                    Staff_Item_Window.active = false;
                    Staff_Item_Window.visible = false;
                    Global.player.facing = Unit_Target_Window.manual_targeting ? 2 : 4;
                    Global.player.update_cursor_frame();
                }
            }
        }

        protected void cancel_staff()
        {

        }

        protected void update_staff_target_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (false) // Staff_Item_Window.is_getting_help() //Yeti
            {

            }
            else
            {
                if (Unit_Target_Window.is_canceled() || cancel_button_triggered)
                {
                    Global.game_map.range_start_timer = 0;
                    Global.game_system.play_se(System_Sounds.Cancel);
                    // Sends cursor back to unit
                    Global.player.instant_move = true;
                    Global.player.loc = unit.loc;
                    // Makes cursor invisible again
                    Global.player.facing = 6;
                    Global.player.frame = 1;
                    Staff_Item_Window = new Window_Command_Item_Staff(Unit_Id, new Vector2(24, 8));
                    Staff_Item_Window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                    Staff_Item_Window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                    Staff_Item_Window.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
                    Unit_Target_Window = null;
                    return;
                }
                else if (Unit_Target_Window.is_selected())
                {
                    if (manual_targeting &&
                            !Global.game_temp.temp_staff_range.Contains(Global.player.loc))
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    else
                        confirm_staff();
                }
            }
        }

        protected void confirm_staff()
        {
            Global.game_system.play_se(System_Sounds.Confirm); // was there a reason this didn't exist here in FEXP //Yeti
            Global.game_map.clear_move_range();
            // Lock in unit movement
            Game_Unit unit = Global.game_map.units[Unit_Id];
            unit.moved();

            Global.game_state.call_staff(
                Unit_Id, Unit_Target_Window.target,
                Unit_Target_Window.manual_targeting ?
                    Global.player.loc :
                    Global.game_map.attackable_map_object(Unit_Target_Window.target).loc);
            if (unit.actor.weapon.Hits_All_in_Range())
            {
                foreach (int id in Global.game_system.Aoe_Targets)
                {
                    if (Global.game_map.units.ContainsKey(id))
                    {
                        // Adds to list of attacked units for this turn
                        if (unit.is_attackable_team(Global.game_map.units[id]))
                        {
                            unit.add_attack_target(id);

                            unit.same_target_support_gain_display(id);
                        }
                        else
                            unit.heal_support_gain_display(id);
                    }
                }
            }
            else
            {
                if (Global.game_map.units.ContainsKey(Global.game_system.Battler_2_Id))
                {
                    // Adds to list of attacked units for this turn
                    if (unit.is_attackable_team(Global.game_map.units[Global.game_system.Battler_2_Id]))
                    {
                        unit.add_attack_target(Global.game_system.Battler_2_Id);

                        unit.same_target_support_gain_display(Global.game_system.Battler_2_Id);
                    }
                    else
                        unit.heal_support_gain_display(Global.game_system.Battler_2_Id);
                }
            }

            Global.game_temp.clear_temp_range();
            Global.game_temp.menuing = false;
            close_unit_menu(true);
            Staff_Item_Window = null;
            Unit_Target_Window = null;
        }
        #endregion

        #region Dance
        protected void update_dance_item_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Dance_Item_Window.is_help_active)
            {
                if (Dance_Item_Window.is_canceled() || cancel_button_triggered)
                    Dance_Item_Window.close_help();
            }
            else
            {
                if (Dance_Item_Window.getting_help())
                    Dance_Item_Window.open_help();
                else if (Dance_Item_Window.is_canceled() || cancel_button_triggered)
                {
                    Global.game_map.range_start_timer = 0;
                    Global.game_system.play_se(System_Sounds.Cancel);
                    unit_command_window_visible = true;
                    unit_command_window_active = true;
                    Dance_Item_Window = null;
                    return;
                }
                else if (Dance_Item_Window.is_selected())
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    // This should really never come up but just in case
                    unit.using_siege_engine = Dance_Item_Window.redirect() ==
                        Siege_Engine.SIEGE_INVENTORY_INDEX;
                    Unit_Target_Window = new Window_Target_Dance(unit.id, Dance_Item_Window.redirect() >= 0, new Vector2(4, 0));
                    Dance_Item_Window.active = false;
                    Dance_Item_Window.visible = false;
                    Global.player.facing = 4;
                    Global.player.update_cursor_frame();
                }
            }
        }

        protected void update_dance_target_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Unit_Target_Window.is_canceled() || cancel_button_triggered)
            {
                Global.game_map.range_start_timer = 0;
                Global.game_system.play_se(System_Sounds.Cancel);
                // Sends cursor back to unit
                Global.player.instant_move = true;
                Global.player.loc = unit.loc;
                // Makes cursor invisible again
                Global.player.facing = 6;
                Global.player.frame = 1;
                if (Dance_Item_Window != null)
                {
                    Dance_Item_Window.active = true;
                    Dance_Item_Window.visible = true;
                }
                else
                {
                    unit_command_window_visible = true;
                    unit_command_window_active = true;
                }
                Unit_Target_Window = null;
                return;
            }
            else if (Unit_Target_Window.is_selected())
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Global.game_map.remove_updated_move_range(Unit_Id);
                Global.game_map.clear_move_range();
                Global.game_map.range_start_timer = 0;
                Global.game_state.call_dance(Unit_Id, Unit_Target_Window.targets[Unit_Target_Window.index], Dance_Item_Window == null ? -1 : Dance_Item_Window.redirect());
                Global.game_temp.menuing = false;
                close_unit_menu(true);
                Unit_Target_Window = null;
                Dance_Item_Window = null;

                unit.cantoing = false;
                // Lock in unit movement
                unit.moved();
                if (unit.has_canto() && !unit.full_move())
                    unit.cantoing = true;
            }
        }
        #endregion

        #region Construct
        protected void update_construct_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (ConstructWindow.is_canceled() || cancel_button_triggered)
            {
                Global.game_map.range_start_timer = 0;
                Global.game_system.play_se(System_Sounds.Cancel);
                unit_command_window_visible = true;
                unit_command_window_active = true;
                AssembleItemWindow = null;
                ConstructWindow = null;
                return;
            }
            else if (ConstructWindow.is_selected())
            {
                switch(ConstructWindow.selected_index())
                {
                    case 0:
                        if (unit.can_assemble() && unit.assemble_targets().Count > 0)
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            ConstructWindow.active = false;
                            ConstructWindow.visible = false;
                            AssembleItemWindow = new Window_Command_Item_Assemble(
                                Unit_Id, new Vector2(24, 8));
                            AssembleItemWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                            AssembleItemWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                            AssembleItemWindow.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
                        }
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                    case 1:
                        if (unit.can_reload() && unit.reload_targets().Count > 0)
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            ConstructWindow.active = false;
                            ConstructWindow.visible = false;
                            Global.game_system.play_se(System_Sounds.Confirm);
                            Loc_Target_Window = new Window_Target_Construct(
                                unit.id, ConstructionModes.Reload, new Vector2(4, 0));
                            Global.player.facing = 4;
                            Global.player.update_cursor_frame();
                        }
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                    case 2:
                        if (unit.can_reclaim() && unit.reclaim_targets().Count > 0)
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            ConstructWindow.active = false;
                            ConstructWindow.visible = false;
                            Global.game_system.play_se(System_Sounds.Confirm);
                            Loc_Target_Window = new Window_Target_Construct(
                                unit.id, ConstructionModes.Reclaim, new Vector2(4, 0));
                            Global.player.facing = 4;
                            Global.player.update_cursor_frame();
                        }
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                }
            }
        }

        protected void update_assemble_item_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (AssembleItemWindow.is_help_active)
            {
                if (AssembleItemWindow.is_canceled() || cancel_button_triggered)
                    AssembleItemWindow.close_help();
            }
            else
            {
                if (AssembleItemWindow.getting_help())
                    AssembleItemWindow.open_help();
                else if (AssembleItemWindow.is_canceled() || cancel_button_triggered)
                {
                    Global.game_map.range_start_timer = 0;
                    Global.game_system.play_se(System_Sounds.Cancel);
                    ConstructWindow.visible = true;
                    ConstructWindow.active = true;
                    AssembleItemWindow = null;
                    return;
                }
                else if (AssembleItemWindow.is_selected())
                {
                    AssembleItemWindow.active = false;
                    AssembleItemWindow.visible = false;
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Loc_Target_Window = new Window_Target_Construct(
                        unit.id, ConstructionModes.Assemble, new Vector2(4, 0));
                    Global.player.facing = 4;
                    Global.player.update_cursor_frame();
                }
            }
        }

        protected void update_assemble_target_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Loc_Target_Window.is_canceled() || cancel_button_triggered)
            {
                Global.game_map.range_start_timer = 0;
                Global.game_system.play_se(System_Sounds.Cancel);
                // Sends cursor back to unit
                Global.player.instant_move = true;
                Global.player.loc = unit.loc;
                // Makes cursor invisible again
                Global.player.facing = 6;
                Global.player.frame = 1;
                switch ((Loc_Target_Window as Window_Target_Construct).Mode)
                {
                    case ConstructionModes.Assemble:
                    default:
                        AssembleItemWindow.active = true;
                        AssembleItemWindow.visible = true;
                        break;
                    case ConstructionModes.Reload:
                    case ConstructionModes.Reclaim:
                        ConstructWindow.active = true;
                        ConstructWindow.visible = true;
                        break;
                }
                Loc_Target_Window = null;
                return;
            }
            else if (Loc_Target_Window.is_selected())
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Global.game_map.remove_updated_move_range(Unit_Id);
                Global.game_map.clear_move_range();
                Global.game_map.range_start_timer = 0;

                Vector2 target_loc = Loc_Target_Window.targets[Loc_Target_Window.index];
                switch ((Loc_Target_Window as Window_Target_Construct).Mode)
                {
                    case ConstructionModes.Assemble:
                    default:
                        // Simplified add siege engine @Debug
                        Item_Data item;
                        if (AssembleItemWindow.redirect() < unit.actor.whole_inventory.Count)
                        {
                            item = unit.actor.whole_inventory[AssembleItemWindow.redirect()];
                            unit.actor.discard_item(AssembleItemWindow.redirect());
                        }
                        else
                        {
                            int index = AssembleItemWindow.redirect() -
                                unit.actor.whole_inventory.Count;
                            List<Item_Data> convoy =
                                Global.game_battalions.convoy(Global.battalion.convoy_id);
                            var siege_engines = convoy
                                .Where(x => x.is_weapon && x.to_weapon.Ballista())
                                .ToList();

                            item = siege_engines[index];
                            Global.game_battalions.remove_item_from_convoy(
                                Global.battalion.convoy_id, convoy.IndexOf(item));
                        }
                        Global.game_map.add_siege_engine(target_loc, item);
                        //Global.game_state.call_assemble(
                        //    Unit_Id,
                        //    target_loc,
                        //    AssembleItemWindow.redirect());
                        break;
                    case ConstructionModes.Reload:
                        // Simplified reload siege engine @Debug
                        var siege = Global.game_map.get_siege(target_loc);
                        siege.reload();

                        //Global.game_state.call_reload(
                        //    Unit_Id,
                        //    target_loc);
                        ConstructWindow.active = true;
                        break;
                    case ConstructionModes.Reclaim:
                        siege = Global.game_map.get_siege(target_loc);
                        Global.game_battalions.add_item_to_convoy(siege.item);
                        Global.game_battalions.sort_convoy(
                            Global.battalion.convoy_id);
                        Global.game_map.remove_siege_engine(siege.id);

                        //Global.game_state.call_reclaim(
                        //    Unit_Id,
                        //    target_loc);
                        ConstructWindow.visible = true;
                        break;
                }

                Global.game_temp.menuing = false;
                close_unit_menu(true);
                Loc_Target_Window = null;
                AssembleItemWindow = null;
                ConstructWindow = null;

                unit.cantoing = false;
                // Lock in unit movement
                unit.moved();
                if (unit.has_canto() && !unit.full_move())
                    unit.cantoing = true;



                // Simplified wait @Debug
                unit.start_wait();
                suspend();
            }
        }
        #endregion

        #region Steal
        protected void update_steal_target_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Unit_Target_Window.is_canceled() || cancel_button_triggered)
            {
                Global.game_map.range_start_timer = 0;
                Global.game_system.play_se(System_Sounds.Cancel);
                // Sends cursor back to unit
                Global.player.instant_move = true;
                Global.player.loc = unit.loc;
                // Makes cursor invisible again
                Global.player.facing = 6;
                Global.player.frame = 1;
                unit_command_window_visible = true;
                unit_command_window_active = true;
                Unit_Target_Window = null;
                return;
            }
            else if (Unit_Target_Window.is_selected())
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Steal_Window = new Window_Steal(Unit_Id, Unit_Target_Window.targets[Unit_Target_Window.index]);
                Unit_Target_Window.active = false;
                Unit_Target_Window.visible = false;
                // Makes cursor invisible again
                Global.player.facing = 6;
                Global.player.frame = 1;
            }
        }

        protected void update_steal_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Steal_Window.is_help_active)
            {
                if (Steal_Window.is_canceled() || cancel_button_triggered)
                    Steal_Window.close_help();
            }
            else
            {
                if (Steal_Window.getting_help())
                    Steal_Window.open_help();
                else if (Steal_Window.is_canceled() || cancel_button_triggered)
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    // Sends cursor back to unit
                    Global.player.instant_move = true;
                    // Makes cursor visible again
                    Global.player.facing = 4;
                    Unit_Target_Window.visible = true;
                    Unit_Target_Window.active = true;
                    Steal_Window = null;
                    return;
                }
                else if (Steal_Window.is_selected())
                {
                    if (Steal_Window.is_equipped(Steal_Window.index))
                        Steal_Window.equipped_steal_help();
                    else if (!Steal_Window.can_steal(Steal_Window.index))
                        Steal_Window.cant_steal_help();
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Global.game_map.remove_updated_move_range(Unit_Id);
                        Global.game_map.clear_move_range();
                        Global.game_map.range_start_timer = 0;
                        Global.game_state.call_steal(Unit_Id, Steal_Window.unit.id, Steal_Window.index);
                        // Adds to list of attacked units for this turn
                        if (Global.game_map.units.ContainsKey(Global.game_system.Battler_2_Id) &&
                                unit.is_attackable_team(Global.game_map.units[Global.game_system.Battler_2_Id]))
                        {
                            unit.add_attack_target(Global.game_system.Battler_2_Id);

                            unit.same_target_support_gain_display(Global.game_system.Battler_2_Id);
                        }
                        Global.game_temp.menuing = false;
                        close_unit_menu(true);
                        Unit_Target_Window = null;
                        Steal_Window = null;

                        unit.cantoing = false;
                        // Lock in unit movement
                        unit.moved();
                        if (unit.has_canto() && !unit.full_move())
                            unit.cantoing = true;
                    }
                }
            }
        }
        #endregion

        #region Support
        protected void update_support_target_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Unit_Target_Window.is_canceled() || cancel_button_triggered)
            {
                Global.game_map.range_start_timer = 0;
                Global.game_system.play_se(System_Sounds.Cancel);
                // Sends cursor back to unit
                Global.player.instant_move = true;
                Global.player.loc = unit.loc;
                // Makes cursor invisible again
                Global.player.facing = 6;
                Global.player.frame = 1;
                unit_command_window_visible = true;
                unit_command_window_active = true;
                Unit_Target_Window = null;
                return;
            }
            else if (Unit_Target_Window.is_selected())
            {
                Global.game_system.play_se(System_Sounds.Confirm);

                Global.game_map.remove_updated_move_range(Unit_Id);
                Global.game_map.clear_move_range();
                Global.game_map.range_start_timer = 0;
                Global.game_state.call_support(unit.id, Unit_Target_Window.target);
                Global.game_temp.menuing = false;
                Unit_Target_Window = null;
                close_unit_menu(true);

                unit.cantoing = false;
                // Lock in unit movement
                unit.moved();
                if (unit.has_canto() && !unit.full_move())
                    unit.cantoing = true;
                Global.player.facing = 6;
                Global.player.frame = 1;
            }
        }
        #endregion

        #region Talk
        protected void update_talk_target_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Unit_Target_Window.is_canceled() || cancel_button_triggered)
            {
                Global.game_map.range_start_timer = 0;
                Global.game_system.play_se(System_Sounds.Cancel);
                // Sends cursor back to unit
                Global.player.instant_move = true;
                Global.player.loc = unit.loc;
                // Makes cursor invisible again
                Global.player.facing = 6;
                Global.player.frame = 1;
                unit_command_window_visible = true;
                unit_command_window_active = true;
                Unit_Target_Window = null;
                return;
            }
            else if (Unit_Target_Window.is_selected())
            {
                Global.game_system.play_se(System_Sounds.Confirm);

                Global.game_map.remove_updated_move_range(Unit_Id);
                Global.game_map.clear_move_range();
                Global.game_map.range_start_timer = 0;

                Global.game_state.activate_talk(Unit_Id, Unit_Target_Window.target);
                Global.game_temp.menuing = false;
                Unit_Target_Window = null;
                close_unit_menu(true, !Constants.Gameplay.TALKING_IS_FREE_ACTION);

                unit.cantoing = false;
                // Lock in unit movement
                unit.moved();
                if (Constants.Gameplay.TALKING_IS_FREE_ACTION)
                {
                    Global.game_system.Menu_Canto = Canto | Canto_Records.Talk |
                        ((unit.has_canto() && !unit.full_move()) ? Canto_Records.Horse : Canto_Records.None);
                    unit.cantoing = true;
                }
                else if (unit.has_canto() && !unit.full_move())
                    unit.cantoing = true;
                Global.player.facing = 6;
                Global.player.frame = 1;
            }
        }
        #endregion

        #region Door
        protected void update_door_target_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Loc_Target_Window.is_canceled() || cancel_button_triggered)
            {
                Global.game_map.range_start_timer = 0;
                Global.game_system.play_se(System_Sounds.Cancel);
                // Sends cursor back to unit
                Global.player.instant_move = true;
                Global.player.loc = unit.loc;
                // Makes cursor invisible again
                Global.player.facing = 6;
                Global.player.frame = 1;
                unit_command_window_visible = true;
                unit_command_window_active = true;
                Loc_Target_Window = null;
                return;
            }
            else if (Loc_Target_Window.is_selected())
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Global.game_map.remove_updated_move_range(Unit_Id);
                Global.game_map.clear_move_range();
                Global.game_map.range_start_timer = 0;
                Global.game_state.call_visit(FEXNA.State.Visit_Modes.Door, Unit_Id, Loc_Target_Window.target);
                Global.game_temp.menuing = false;
                close_unit_menu(true);
                Loc_Target_Window = null;

                unit.cantoing = false;
                // Lock in unit movement
                unit.moved();
                if (unit.has_canto() && !unit.full_move())
                    unit.cantoing = true;
            }
        }
        #endregion

        #region Rescue
        protected void update_rescue_target_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Unit_Target_Window.is_canceled() || cancel_button_triggered)
            {
                Global.game_map.range_start_timer = 0;
                Global.game_system.play_se(System_Sounds.Cancel);
                // Sends cursor back to unit
                Global.player.instant_move = true;
                Global.player.loc = unit.loc;
                // Makes cursor invisible again
                Global.player.facing = 6;
                Global.player.frame = 1;
                unit_command_window_visible = true;
                unit_command_window_active = true;
                Unit_Target_Window = null;
                return;
            }
            else if (Unit_Target_Window.is_selected())
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Global.game_map.clear_move_range();
                // Lock in unit movement
                unit.moved();
                rescue_mode_action(unit);
                Global.game_temp.menuing = false;
                close_unit_menu(true);
                Unit_Target_Window = null;
            }
        }

        protected void rescue_mode_action(Game_Unit unit)
        {
            switch (((Window_Target_Rescue)Unit_Target_Window).mode)
            {
                case 0:
                    unit.rescue_ally(Unit_Target_Window.targets[Unit_Target_Window.index]);

                    unit.rescue_support_gain_display(Unit_Target_Window.targets[Unit_Target_Window.index]);
                    break;
                case 1:
                    int val = Unit_Target_Window.targets[Unit_Target_Window.index];
                    unit.drop_ally(new Vector2(val % Global.game_map.width,
                        val / Global.game_map.width));
                    break;
                case 2:
                    unit.take_ally(Unit_Target_Window.targets[Unit_Target_Window.index]);
                    //Global.game_system.Menu_Canto = Canto_Record.Take | (Canto & (Canto_Record.Trade | Canto_Record.Supply)); //Debug
                    Global.game_system.Menu_Canto = Canto | Canto_Records.Take;
                    break;
                case 3:
                    unit.give_ally(Unit_Target_Window.targets[Unit_Target_Window.index]);
                    //Global.game_system.Menu_Canto = Canto_Record.Give | (Canto & (Canto_Record.Trade | Canto_Record.Supply)); //Debug
                    Global.game_system.Menu_Canto = Canto | Canto_Records.Give;
                    break;
            }
            rescue_mode_action_skill(unit);
        }
        #endregion

        #region Item
        protected void update_item_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Item_Window.is_help_active)
            {
                if (Item_Window.is_canceled() || cancel_button_triggered)
                    Item_Window.close_help();
            }
            else
            {
                if (Item_Window.getting_help())
                    Item_Window.open_help();
                else if (Item_Window.is_canceled() || cancel_button_triggered)
                {
                    Item_Window.restore_equipped();
                    unit.actor.staff_fix();
                    Global.game_map.range_start_timer = 0;
                    Global.game_system.play_se(System_Sounds.Cancel);
                    unit_command_window_visible = true;
                    unit_command_window_active = true;
                    Item_Window = null;
                    return;
                }
                else if (Item_Window.is_selected())
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    bool can_trade = canto_allows_trade(Canto);
                    if (can_trade)
                        can_trade &= unit.can_trade;

                    Item_Options = new Window_Item_Options(unit, can_trade, new Vector2(104 + 24, 32 + 16 * Item_Window.index),
                        Item_Window.index, Item_Window.equipped == Item_Window.index + 1);
                    Item_Options.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                    Item_Options.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                    Item_Window.active = false;
                    Item_Options.active = true;
                    Item_Options.index = 0;
                }
            }
        }

        protected void update_item_options()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Item_Options.is_canceled() || cancel_button_triggered)
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                Item_Window.active = true;
                Item_Options = null;
            }
            else if (Item_Options.is_selected())
            {
                // No guarantee that doing something with items isn't changing some unit's move range in some way
                Global.game_map.remove_updated_move_range(Unit_Id);
                switch (Item_Options.redirect)
                {
                    // Equip
                    case 0:
                        if (unit.actor.is_equippable(Global.data_weapons[unit.actor.items[Item_Window.index].Id]))
                        {
                            if (Item_Options.unequip)
                            {
                                Global.game_system.play_se(System_Sounds.Cancel);
                                unit.actor.unequip();
                            }
                            else
                            {
                                Global.game_system.play_se(System_Sounds.Confirm);
                                unit.equip(Item_Window.index + 1);
                            }
                            unit.actor.organize_items();
                            Item_Window.refresh_items();
                            Item_Window.index = 0;
                            Item_Window.active = true;
                            Item_Options = null;
                        }
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer); // should pop up a help window //Yeti
                        break;
                    // Use
                    case 1:
                        if (Combat.can_use_item(unit, unit.actor.items[Item_Window.redirect()].Id))
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            Item_Window.restore_equipped();
                            if (unit.actor.items[Item_Window.redirect()].to_item.targets_inventory())
                            {
                                Global.game_map.range_start_timer = 0;
                                Repair_Item_Window = new Window_Command_Item_Target_Inventory(Unit_Id, new Vector2(24, 8), Item_Window.index);
                                Repair_Item_Window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                                Repair_Item_Window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                                Repair_Item_Window.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
                                Item_Options.active = false;
                                Item_Options.visible = false;
                                Item_Window.visible = false;
                            }
                            else
                            {
                                use_item(unit);
                            }
                        }
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                        // Discard
                    case 3:
                        if (Item_Options.can_discard)
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            Discard_Confirm = new Window_Command(Item_Options.loc + new Vector2(32, 8 + Item_Options.index * 16), 40, new List<string> { "Yes", "No" });
                            Discard_Confirm.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                            Discard_Confirm.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                            Discard_Confirm.small_window = true;
                            Discard_Confirm.active = true;
                            Item_Options.active = false;
                            Discard_Confirm.index = 0;
                        }
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                }
            }
        }

        protected void update_repair_item_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Repair_Item_Window.is_help_active)
            {
                if (Repair_Item_Window.is_canceled() || cancel_button_triggered)
                    Repair_Item_Window.close_help();
            }
            else
            {
                if (Repair_Item_Window.getting_help())
                    Item_Window.open_help();
                else if (Repair_Item_Window.is_canceled() || cancel_button_triggered)
                {
                    Repair_Item_Window.restore_equipped();
                    unit.actor.staff_fix();
                    Global.game_map.range_start_timer = 0;
                    Global.game_system.play_se(System_Sounds.Cancel);
                    Item_Options.active = true;
                    Item_Options.visible = true;
                    Item_Window.visible = true;
                    Repair_Item_Window = null;
                    return;
                }
                else if (Repair_Item_Window.is_selected())
                {
                    if (unit.actor.items[Item_Window.redirect()].to_item.can_target_item(unit.actor.items[Repair_Item_Window.redirect()]))
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Item_Window.restore_equipped();
                        Global.game_system.Item_Inventory_Target = Repair_Item_Window.redirect();
                        use_item(unit);
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                }
            }
        }

        protected void use_item(Game_Unit unit)
        {
            Global.game_map.clear_move_range();
            unit.cantoing = false;
            // Lock in unit movement
            unit.moved();
            Global.game_state.call_item(Unit_Id, Item_Window.redirect());

            Global.game_temp.menuing = false;
            close_unit_menu(false);
            Item_Window = null;
            Item_Options = null;
            Repair_Item_Window = null;
        }

        protected void update_discard_confirm()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Discard_Confirm.is_canceled())
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                Discard_Confirm = null;
                Item_Options = null;
                Item_Window.active = true;
            }
            else if (Discard_Confirm.is_selected() || cancel_button_triggered)
            {
                var discard_index = Discard_Confirm.selected_index();
                if (discard_index.IsNothing)
                    discard_index = 1;

                switch (discard_index)
                {
                    // Yes
                    case 0:
                        Global.game_system.play_se(System_Sounds.Confirm);
                        unit.actor.discard_item(Item_Window.redirect());
                        Discard_Confirm = null;
                        Item_Options = null;
                        // Unit window has to restart in case options have changed
                        int command_index = Index_Redirect[unit_command_window_index];
                        open_unit_menu(Canto);
                        // If discarded all items
                        if (unit.actor.has_no_items)
                        {
                            Unit_Command_Window.index = 0;
                            unit_command_window_visible = true;
                            unit_command_window_active = true;
                            Item_Window = null;
                        }
                        else
                        {
                            Unit_Command_Window.index = Index_Redirect.IndexOf(command_index);
                            unit_command_window_visible = false;
                            unit_command_window_active = false;
                            Item_Window = new Window_Command_Item(Unit_Id, new Vector2(24, 8));
                            Item_Window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
                            Item_Window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
                            Item_Window.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
                        }
                        break;
                    // No
                    case 1:
                        Global.game_system.play_se(System_Sounds.Cancel);
                        Discard_Confirm = null;
                        Item_Options = null;
                        Item_Window.active = true;
                        break;
                }
            }
        }
        #endregion

        #region Trade
        protected void update_trade_target_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Unit_Target_Window.is_canceled() || cancel_button_triggered)
            {
                Global.game_map.range_start_timer = 0;
                Global.game_system.play_se(System_Sounds.Cancel);
                // Sends cursor back to unit
                Global.player.instant_move = true;
                Global.player.loc = unit.loc;
                // Makes cursor invisible again
                Global.player.facing = 6;
                Global.player.frame = 1;
                unit_command_window_visible = true;
                unit_command_window_active = true;
                Unit_Target_Window = null;
                return;
            }
            else if (Unit_Target_Window.is_selected())
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Trade_Window = new Window_Trade(unit.actor.id,
                    Global.game_map.units[Unit_Target_Window.targets[Unit_Target_Window.index]].actor.id, -1);
                Trade_Window.stereoscopic = Config.PREPTRADE_WINDOWS_DEPTH;
                Trade_Window.face_stereoscopic = Config.PREPTRADE_FACES_DEPTH;
                Trade_Window.help_stereoscopic = Config.PREPTRADE_HELP_DEPTH;

                Unit_Target_Window.active = false;
                Unit_Target_Window.visible = false;
                // Makes cursor invisible again
                Global.player.facing = 6;
                Global.player.frame = 1;
                Traded = false;
            }
        }

        protected bool Traded = false;
        protected void update_trade_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (Trade_Window.is_help_active)
            {
                if (Trade_Window.is_canceled() || cancel_button_triggered)
                    Trade_Window.close_help();
            }
            else
            {
                if (Trade_Window.getting_help())
                    Trade_Window.open_help();
                else if (Trade_Window.is_canceled() || cancel_button_triggered)
                {
                    if (Trade_Window.mode > 0)
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        Trade_Window.cancel();
                    }
                    else if (Traded)
                    {
                        CancelButton.description = "Cancel";
                        // No guarantee that doing something with items isn't changing some unit's move range in some way
                        Global.game_map.remove_updated_move_range(Unit_Id);
                        Global.game_map.remove_updated_move_range(Unit_Target_Window.targets[Unit_Target_Window.index]);
                        Global.game_system.play_se(System_Sounds.Cancel);
                        // Lock in unit movement
                        unit.moved();
                        unit.queue_move_range_update();
                        // Sends cursor back to unit
                        Global.player.instant_move = true;
                        Global.player.loc = unit.loc;
                        // Switch to map
                        Trade_Window.staff_fix();
                        Trade_Window = null;
                        Unit_Target_Window = null;
                        close_unit_menu(true, false);
                        unit.cantoing = true;
                        //open_unit_menu(Canto_Record.Trade | (Canto & Canto_Record.Give) | //Debug
                        //    ((unit.has_canto() && !unit.full_move()) ? Canto_Record.Horse : Canto_Record.None));
                        open_unit_menu(Canto | Canto_Records.Trade |
                            ((unit.has_canto() && !unit.full_move()) ? Canto_Records.Horse : Canto_Records.None));
                        unit.open_move_range();
                        Global.game_map.move_range_visible = false;
                    }
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        // Sends cursor back to unit
                        Global.player.instant_move = true;
                        // Makes cursor visible again
                        Global.player.facing = 4;
                        Unit_Target_Window.visible = true;
                        Unit_Target_Window.active = true;
                        Trade_Window = null;
                    }
                    return;
                }
                else if (Trade_Window.is_selected())
                {
                    if (Trade_Window.ready)
                        if (Trade_Window.enter())
                        {
                            Traded = true;
                            CancelButton.description = "Close";
                        }
                }
            }
        }
        #endregion

        #region Supply
        protected void update_supply_menu()
        {
            Game_Unit unit = null;
            if (!Global.game_system.preparations)
                unit = Global.game_map.units[Unit_Id];

            if (Supply_Window.closed)
            {
                if (Supply_Window.traded)
                {
                    // No guarantee that doing something with items isn't changing some unit's move range in some way
                    Global.game_map.remove_updated_move_range(Unit_Id);
                    // Lock in unit movement
                    unit.moved();
                    unit.queue_move_range_update();
                    // Sends cursor back to unit
                    Global.player.instant_move = true;
                    Global.player.loc = unit.loc;
                    // Switch to map
                    close_unit_menu(true, false);
                    unit.cantoing = true;
                    open_unit_menu(Canto | Canto_Records.Supply |
                        ((unit.has_canto() && !unit.full_move()) ? Canto_Records.Horse : Canto_Records.None));
                    unit.open_move_range();
                    Global.game_map.move_range_visible = false;
                }
                else
                {
                    unit_command_window_visible = true;
                    unit_command_window_active = true;
                }

                Supply_Window = null;
            }
        }
        #endregion

        // Close
        protected void close_unit_menu(bool move_range_visible = true, bool clear_canto = true)
        {
            Unit_Command_Window = null;
            if (move_range_visible)
                Global.game_map.move_range_visible = true;
            if (clear_canto)
                Canto = Canto_Records.None;

            CancelButton = null;
        }

        protected void new_unit_command_window(List<string> commands, int width)
        {
            Unit_Command_Window = new Window_Command(
                new Vector2(8 + (Global.player.is_on_left() ? (Config.WINDOW_WIDTH - (width + 16)) : 0), 24), width, commands);
            Unit_Command_Window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            Unit_Command_Window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;

            create_cancel_button();
        }

        private void create_cancel_button()
        {
            CancelButton = Button_Description.button(Inputs.B,
                Global.player.is_on_left() ? Config.WINDOW_WIDTH - (32 + 48) : 32);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
        }

        protected bool is_unit_command_window_open { get { return Unit_Command_Window != null; } }

        protected void update_unit_command_window()
        {
            Unit_Command_Window.update();
        }

        private void update_cancel_button()
        {
            if (CancelButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    create_cancel_button();
                bool input = Supply_Window == null;
                CancelButton.Update(input);
            }
        }

        protected int unit_command_window_index { get { return Unit_Command_Window.index; } }

        protected Maybe<int> unit_command_window_selected_index { get { return Unit_Command_Window.selected_index(); } }

        protected bool unit_command_is_selected { get { return unit_command_window_selected_index.IsSomething; } }

        protected bool unit_command_is_canceled
        {
            get
            {
                // Don't cancel by tapping outside the window if it would remove options
                if (Canto == Canto_Records.None || Canto == Canto_Records.Horse)
                    if (Global.Input.gesture_triggered(TouchGestures.Tap))
                        return true;
                return Unit_Command_Window.is_canceled() || cancel_button_triggered;
            }
        }

        private bool cancel_button_triggered
        {
            get
            {
                return CancelButton.consume_trigger(MouseButtons.Left) ||
                    CancelButton.consume_trigger(TouchGestures.Tap) ||
                    Global.Input.mouse_click(MouseButtons.Right);
            }
        }

        protected bool unit_command_window_active
        {
            get { return Unit_Command_Window.active; }
            set { Unit_Command_Window.active = value; }
        }
        protected bool unit_command_window_visible
        {
            get { return Unit_Command_Window.visible; }
            set { Unit_Command_Window.visible = value; }
        }

        // Draw Ranges
        protected void draw_menu_ranges(SpriteBatch sprite_batch)
        {
            if (is_unit_command_window_open)
            {
                int width = Attack_Range_Texture.Width / 4;
                int timer = Math.Min(Global.game_map.range_start_timer, width - 1);
                Rectangle rect = new Rectangle(((Global.game_map.move_range_anim_count / 4) / 4) * width,
                        ((Global.game_map.move_range_anim_count / 4) % 4) * width + (width - timer), timer, timer);
                // Temp Attack Range
                if (Index_Redirect[unit_command_window_index] == 0 &&
                    (unit_command_window_active || Attack_Item_Window != null))
                {
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    int opacity = 144;
                    Color color = new Color(opacity, opacity, opacity, opacity);
                    foreach (Vector2 loc in Global.game_temp.temp_attack_range)
                    {
                        sprite_batch.Draw(Attack_Range_Texture,
                            loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                            rect, color);
                    }
                    sprite_batch.End();
                }
                // Temp Staff Range
                if (Index_Redirect[unit_command_window_index] == 1 &&
                    (unit_command_window_active || Staff_Item_Window != null))
                {
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    int opacity = 144;
                    Color color = new Color(opacity, opacity, opacity, opacity);
                    foreach (Vector2 loc in Global.game_temp.temp_staff_range)
                    {
                        sprite_batch.Draw(Staff_Range_Texture,
                            loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                            rect, color);
                    }
                    sprite_batch.End();
                }
                // Temp Talk Range
                if (Index_Redirect[unit_command_window_index] == 8 && (unit_command_window_active))
                //if (Index_Redirect[unit_command_window_index] != 0 && Index_Redirect[unit_command_window_index] != 1 &&
                //    !attack_skill_menu_ids().Contains(unit_command_window_index) &&
                //    unit_command_window_index < BASE_MASTERY_MENU_ID &&
                //    (unit_command_window_active))
                {
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    int opacity = 144;
                    Color color = new Color(opacity, opacity, opacity, opacity);
                    foreach (Vector2 loc in Global.game_temp.temp_talk_range)
                    {
                        sprite_batch.Draw(Talk_Range_Texture,
                            loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                            rect, color);
                    }
                    sprite_batch.End();
                }
                draw_menu_ranges_skill(sprite_batch, width, timer, rect);
            }
        }
        #endregion

        #region Map Command Menu
        protected void call_map_menu()
        {
            Global.game_map.clear_move_range();
            Global.game_temp.menuing = true;
            Global.game_temp.menu_call = false;
            Global.game_temp.map_menu_call = false;
            Global.game_system.play_se(System_Sounds.Open);
        }

        protected virtual void open_map_menu()
        {
            MapMenu = new MapMenuManager(this);
        }

        protected void close_map_menu()
        {
            Global.game_temp.menuing = false;
            MapMenu = null;
            Global.game_map.highlight_test();
        }

        #region IMapMenuHandler
        public void MapMenuSuspend()
        {
            MapMenu = null;
            Global.game_temp.menuing = false;
            suspend();
#if DEBUG
            // Don't reset while playtesting, the user probably doesn't want to go back to the editor
            if (!Global.UnitEditorActive)
            {
#endif
                Global.return_to_title = true;
#if DEBUG
            }
#endif
        }

        public void MapMenuEndTurn()
        {
            MapMenu = null;
            Global.game_state.end_turn();
            Global.game_temp.menuing = false;
            suspend();
        }
        #endregion

#if DEBUG
        internal virtual void open_debug_menu()
        {
            if (Global.game_state.is_player_turn && Global.game_state.is_map_ready(false) &&
                Global.game_map.get_selected_unit() == null &&
                !Global.game_temp.menu_call && !Global.game_temp.menuing &&
                Minimap == null)
            {
                if (Global.game_temp.highlighted_unit_id != -1)
                {
                    Global.game_map.get_highlighted_unit().highlighted = false;
                    Global.game_temp.highlighted_unit_id = -1;

                }
                Global.game_map.clear_move_range();
                Global.game_temp.menuing = true;
                Global.game_temp.menu_call = false;
                Global.game_temp.map_menu_call = false;
                Global.game_system.play_se(System_Sounds.Open);

                MapMenu = new MapDebugMenuManager(this);
            }
        }

        #region IMapDebugMenuHandler
        public void DebugMenuRefreshUnit(Game_Unit unit)
        {
            unit.refresh_unit();

            close_map_menu();
        }

        public void DebugMenuDeleteUnit(Game_Unit unit)
        {
            Global.game_map.add_true_dying_unit_animation(unit.id);

            close_map_menu();
        }

        public void DebugMenuSkipChapter()
        {
            close_map_menu();

            Global.game_temp.chapter_skipped = true;
            Global.game_state.activate_event_by_name("Victory", false);
        }

        public void DebugMenuHealUnit(Game_Unit unit)
        {
            unit.recover_hp();

            close_map_menu();
        }

        public void DebugMenuSupportGainUnit(Game_Unit unit)
        {
            foreach (int actor_id in unit.actor.support_candidates())
            {
                Global.game_state.remove_support_this_chapter(
                    unit.actor.id, actor_id);
                if (unit.actor.support_possible(actor_id))
                    while (!unit.actor.has_points_for_support(actor_id))
                    {
                        unit.actor.chapter_support_gain(actor_id);
                    }
            }

            close_map_menu();
        }

        public void DebugMenuLevelUpUnit(Game_Unit unit)
        {
            if (unit.actor.can_promote() && unit.actor.promotion_level() &&
                unit.actor.level == unit.actor.level_cap())
            {
                Global.game_system.play_se(System_Sounds.Status_Page_Change);
                unit.actor.exp = 0;
                unit.actor.quick_promotion((int)unit.actor.promotes_to(false));
                unit.refresh_sprite();
                unit.update_move_range();
            }
            else if (unit.actor.exp_gain_possible() > 0)
            {
                Global.game_system.play_se(System_Sounds.Level_Up_Stat);
                unit.actor.instant_level = true;
                unit.actor.exp += Constants.Actor.EXP_TO_LVL;
            }
        }

        public void DebugMenuToggleFog()
        {
            Global.game_map.fow = !Global.game_map.fow;
            Global.game_map.refresh_move_ranges(true);
            Global.game_map.wait_for_move_update();

            close_map_menu();
        }

        public void DebugMenuToggleInfiniteMove()
        {
            Game_Unit.INFINITE_MOVE_ALLOWED = !Game_Unit.INFINITE_MOVE_ALLOWED;
            Global.game_map.refresh_move_ranges(true);
            Global.game_map.wait_for_move_update();

            close_map_menu();
        }

        public void DebugMenuToggleAI()
        {
            Game_AI.AI_ENABLED = !Game_AI.AI_ENABLED;

            close_map_menu();
        }
        #endregion
#endif
        #endregion

        #region Status Menu
        protected void open_status_menu()
        {
            Global.game_temp.menuing = true;
            Global.game_temp.menu_call = false;
            Global.game_temp.status_menu_call = false;
            Global.game_system.play_se(System_Sounds.Confirm);
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
            Status_Window = new Window_Status(team, id);
        }

        protected void update_status_menu()
        {
            if (Status_Window.closed)
            {
                if (is_unit_command_window_open)
                {
                    unit_command_window_visible = true;
                    unit_command_window_active = true;
                }
                else
                    Global.game_temp.menuing = false;
                Global.game_temp.status_team = 0;
                close_status_menu();
            }
        }

        protected void close_status_menu()
        {
            if (Status_Window != null)
            {
                Status_Window.jump_to_unit();
                Status_Window.close();
                Status_Window = null;
            }
        }
        #endregion

        #region Discard Menu
        protected void open_discard_menu()
        {
            Unit_Id = Global.game_system.Discarder_Id;
            Global.game_temp.menuing = true;
            Global.game_temp.discard_menuing = true;
            Global.game_temp.menu_call = false;
            Global.game_temp.discard_menu_call = false;
            Global.game_system.play_se(System_Sounds.Open);
            if (Global.battalion.convoy_ready_for_sending)
                Discard_Window = new Window_Command_Item_Send(Unit_Id, new Vector2(24, 8));
            else
                Discard_Window = new Window_Command_Item_Discard(Unit_Id, new Vector2(24, 8));
            Discard_Window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            Discard_Window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            Discard_Window.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;

            create_cancel_button();
        }

        protected void update_discard_menu()
        {
            Game_Unit unit = Global.game_map.units[Unit_Id];
            if (((Window_Command_Item_Discard)Discard_Window).confirming)
            {
                if (((Window_Command_Item_Discard)Discard_Window).confirm_ready)
                {
                    if ((Discard_Window as Window_Command_Item_Discard).confirm_is_canceled())
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        ((Window_Command_Item_Discard)Discard_Window).cancel();
                        Discard_Window.active = true;
                        return;
                    }
                    else if ((Discard_Window as Window_Command_Item_Discard).confirm_is_selected())
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        switch (((Window_Command_Item_Discard)Discard_Window).confirm_index)
                        {
                            // Yes
                            case 0:
                                Discard_Window.restore_equipped();
                                FEXNA_Library.Item_Data discarded_item =
                                    unit.actor.whole_inventory[Discard_Window.redirect()];
                                if (Global.battalion.convoy_ready_for_sending)
                                {
                                    Global.game_battalions.add_item_to_convoy(discarded_item);
                                    set_item_sent_popup(discarded_item, 240);
                                }
                                else
                                    set_item_drop_popup(discarded_item, 240);
                                unit.actor.discard_item(Discard_Window.redirect());
                                unit.actor.organize_items();
                                if (unit.actor.too_many_items)
                                {
                                    Discard_Window.visible = false;
                                    Discard_Window.active = false;
                                }
                                else
                                {
                                    Discard_Window = null;
                                    CancelButton = null;
                                    if (!Global.game_system.preparations)
                                        Global.game_temp.menuing = false;
                                    Global.game_temp.discard_menuing = false;
                                    Global.game_temp.force_send_to_convoy = false;
                                    Unit_Id = -1;
                                }
                                return;
                            // No
                            case 1:
                                ((Window_Command_Item_Discard)Discard_Window).cancel();
                                Discard_Window.active = true;
                                return;
                        }
                    }
                }
            }
            else if (Discard_Window.is_help_active)
            {
                if (Discard_Window.is_canceled() || cancel_button_triggered)
                    Discard_Window.close_help();
            }
            else
            {
                if (Discard_Window.getting_help())
                    Discard_Window.open_help();
                else if (Discard_Window.is_canceled() || cancel_button_triggered)
                {
                    Global.game_system.play_se(System_Sounds.Buzzer);
                    return;
                }
                else if (Discard_Window.is_selected())
                {
                    ((Window_Command_Item_Discard)Discard_Window).confirm();
                    Discard_Window.active = false;
                    
                }
            }
        }
        #endregion

        #region Minimap
        protected void open_minimap_menu()
        {
            Global.game_system.play_se(System_Sounds.Minimap_Open);
            info_windows_offscreen();
            Minimap = new Window_Minimap();
        }

        protected void update_minimap()
        {
            if (Minimap.active)
            {
                if (Minimap.closing)
                {
                    Minimap = null;
                    // Clear menu call flag
                    Global.game_temp.minimap_call = false;
                    Global.game_map.highlight_test();
                    return;
                }
                else if (Global.Input.triggered(Inputs.B) ||
                    Global.Input.triggered(Inputs.Start) ||
                    Global.Input.mouse_click(MouseButtons.Right) ||
                    Global.Input.gesture_triggered(TouchGestures.PinchOut))
                {
                    Global.game_system.play_se(System_Sounds.Minimap_Close);
                    Minimap.close();
                }
            }
        }

        public bool is_minimap_busy()
        {
            if (Minimap == null)
                return false;
            return !Minimap.active;
        }
        #endregion

        #region Shop Menu
        protected void open_shop_menu()
        {
            bool already_in_menu = Global.game_state.is_menuing;

            Global.game_temp.menuing = true;
            Global.game_temp.menu_call = false;
            Global.game_system.SecretShop = Global.game_temp.secret_shop_call;
            Global.game_temp.reset_shop_call();

            if (Global.game_system.preparations && already_in_menu)
            {
                Shop_Window = new Window_Shop(
                    Global.game_system.Shopper_Id,
                    Global.game_battalions.active_convoy_shop);
            }
            else
            {
                if (Global.game_map.get_shop().arena)
                    Shop_Window = new Window_Arena(Global.game_system.Shopper_Id, Global.game_map.get_shop(), false);
                else
                {
                    int actor_id = -1;
                    if (Global.game_system.Shopper_Id != -1)
                        actor_id = Global.game_map.units[Global.game_system.Shopper_Id].actor.id;
                    Shop_Window = new Window_Shop(actor_id, Global.game_map.get_shop());
                }
            }
            Shop_Window.Shop_Close += Shop_Window_Shop_Close;
        }

        void Shop_Window_Shop_Close(object sender, EventArgs e)
        {
        }

        public void resume_arena()
        {
            // selected unit = battler
            Global.game_temp.menuing = true;
            Global.game_temp.menu_call = false;
            Global.game_temp.reset_shop_call();
            Unit_Id = Global.game_system.Shopper_Id = Global.game_system.Battler_1_Id;

            Shop_Window = new Window_Arena(Global.game_system.Shopper_Id, Global.game_map.get_shop(), true);
            Shop_Window.Shop_Close += Shop_Window_Shop_Close;
        }

        protected void update_shop_menu()
        {
            // When the shop is exited
            if (Shop_Window.closed)
            {
                if (!Global.game_system.preparations)
                {
                    Game_Unit unit = Global.game_map.units[Unit_Id];
                    // If unit used the shop and can't cancel their action
                    if (Shop_Window.traded)
                    {
                        // Lock in unit movement
                        if (unit.is_dead && Global.game_state.arena)
                        {
                            unit.gladiator = true;
                            Global.game_state.call_shop_suspend();
                        }
                        else
                        {
                            unit.moved();
                            Global.game_map.remove_updated_move_range(Unit_Id);
                            Global.game_state.call_shop_suspend();
                            if (!Global.game_state.arena)
                            {
                                if (!unit.has_canto() || unit.full_move())
                                    unit.start_wait();
                            }
                            if (!Global.game_system.In_Arena)
                            {
                                Global.game_map.clear_move_range();
                                Global.game_temp.menuing = false;
                            }
                        }
                        close_unit_menu(true);
                    }
                    else
                    {
                        if (unit.cantoing)
                            open_unit_menu(Canto);
                        else
                        {
                            Global.game_temp.menuing = false;
                            unit.command_menu_close();
                            close_unit_menu(true);
                        }
                    }

                    if (!Global.game_system.is_loss() && !Global.game_system.In_Arena)
                        Global.game_state.resume_turn_theme(true);
                }
                else
                {
                    Global.game_temp.menuing = false;
                    Global.game_state.play_preparations_theme();
                }

                Shop_Window = null;
            }
        }
        #endregion

        #region Map Save
        protected void open_map_save()
        {
            Global.game_temp.menuing = true;
            Global.game_temp.menu_call = false;
            Global.game_temp.map_save_call = false;

            Overwriting_Checkpoint = Global.current_save_info.map_save_exists;
            if (Overwriting_Checkpoint)
            {
                Map_Save_Confirm_Window = new Parchment_Confirm_Window();
                Map_Save_Confirm_Window.set_text("Overwrite checkpoint?");
                Map_Save_Confirm_Window.add_choice("Yes", new Vector2(16, 16));
                Map_Save_Confirm_Window.add_choice("No", new Vector2(56, 16));
                Map_Save_Confirm_Window.size = new Vector2(112, 48);
            }
            else
            {
                Map_Save_Confirm_Window = new Parchment_Info_Window();
                Map_Save_Confirm_Window.set_text("Checkpoint Saved.");
                Map_Save_Confirm_Window.size = new Vector2(112, 32);

                Suspend_Filename = Config.MAP_SAVE_FILENAME;
                suspend();
            }
            Map_Save_Confirm_Window.loc = new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2 - Map_Save_Confirm_Window.size / 2;
        }

        protected void update_map_save()
        {
            if (!Overwriting_Checkpoint)
            {
                if (Map_Save_Confirm_Window.is_ready)
                {
                    if (Map_Save_Confirm_Window.is_selected() ||
                        Map_Save_Confirm_Window.is_canceled())
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Map_Save_Confirm_Window = null;
                        if (!Global.game_system.preparations)
                            Global.game_temp.menuing = false;
                    }
                }
            }
            else
            {
                if (Map_Save_Confirm_Window.is_ready)
                {
                    if (Map_Save_Confirm_Window.is_canceled())
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        Map_Save_Confirm_Window = null;
                        if (!Global.game_system.preparations)
                            Global.game_temp.menuing = false;
                        return;
                    }
                    else if (Map_Save_Confirm_Window.is_selected())
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        switch (Map_Save_Confirm_Window.index)
                        {
                            // Yes
                            case 0:
                                Suspend_Filename = Config.MAP_SAVE_FILENAME;
                                suspend();
                                break;
                            // No
                            case 1:
                                break;
                        }
                        Map_Save_Confirm_Window = null;
                        if (!Global.game_system.preparations)
                            Global.game_temp.menuing = false;
                    }
                }
            }
        }
        #endregion

        #region Rankings
        protected void open_rankings()
        {
            Global.game_temp.menuing = true;
            Global.game_temp.menu_call = false;
            Global.game_temp.rankings_call = false;

            Global.Audio.BgmFadeOut(15);
            //Global.Audio.play_se("Battle Sounds", "Promotion3");
            Global.Audio.play_se("System Sounds", "Arena_Victory");
            Ranking_Window = new Window_Ranking();
        }

        protected void update_rankings()
        {
            if (Ranking_Window.is_ready)
            {
                Ranking_Window = null;
                Global.game_temp.menuing = false;
                return;
            }
        }
        #endregion

        protected virtual void draw_menus(SpriteBatch sprite_batch)
        {
            if (MapMenu != null)
                MapMenu.Draw(sprite_batch);

            if (is_unit_command_window_open)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                CancelButton.Draw(sprite_batch);
                sprite_batch.End();
            }
            if (!Global.game_system.preparations) // This should be modified so it works for the prep version too //Yeti
                if (Supply_Window != null) Supply_Window.Draw(sprite_batch);

            if (is_unit_command_window_open)
                Unit_Command_Window.draw(sprite_batch);

            if (Item_Window != null) Item_Window.draw(sprite_batch);
            if (Repair_Item_Window != null) Repair_Item_Window.draw(sprite_batch);
            if (Attack_Item_Window != null) Attack_Item_Window.draw(sprite_batch);
            if (Staff_Item_Window != null) Staff_Item_Window.draw(sprite_batch);
            if (Dance_Item_Window != null) Dance_Item_Window.draw(sprite_batch);
            if (AssembleItemWindow != null) AssembleItemWindow.draw(sprite_batch);

            if (Trade_Window != null) Trade_Window.draw(sprite_batch);
            if (Steal_Window != null) Steal_Window.draw(sprite_batch);
            if (ConstructWindow != null) ConstructWindow.draw(sprite_batch);
            if (Item_Options != null) Item_Options.draw(sprite_batch);
            if (Unit_Target_Window != null) Unit_Target_Window.draw(sprite_batch);
            if (Loc_Target_Window != null) Loc_Target_Window.draw(sprite_batch);
            if (Status_Window != null) Status_Window.Draw(sprite_batch);
            if (Shop_Window != null) Shop_Window.Draw(sprite_batch);
            if (Map_Save_Confirm_Window != null) Map_Save_Confirm_Window.draw(sprite_batch);
            if (Ranking_Window != null) Ranking_Window.draw(sprite_batch);
        }

        protected void draw_discard(SpriteBatch sprite_batch)
        {
            if (Discard_Window != null)
                Discard_Window.draw(sprite_batch);

            if (Discard_Confirm != null)
                Discard_Confirm.draw(sprite_batch);
        }

        protected virtual void clear_menus()
        {
            close_map_menu();
            //close_unit_menu(); //Debug
            Unit_Command_Window = null;
            CancelButton = null;

            MapMenu = null;

            Item_Window = null;
            Item_Options = null;
            Discard_Confirm = null;
            Attack_Item_Window = null;
            Staff_Item_Window = null;
            Dance_Item_Window = null;
            AssembleItemWindow = null;
            Discard_Window = null;
            Trade_Window = null;
            Steal_Window = null;
            ConstructWindow = null;
            Unit_Target_Window = null;
            Loc_Target_Window = null;
            Status_Window = null;
            // should Shop_Window null? //Yeti
            Supply_Window = null;

            Map_Save_Confirm_Window = null;
        }

        internal static bool intro_chapter_options_blocked()
        {
            return Global.game_system.chapter_id == "Pre";
        }

        internal static bool debug_chapter_options_blocked()
        {
            return false;
        }
    }
}
