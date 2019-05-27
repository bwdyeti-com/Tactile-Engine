using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Menus;
using FEXNA.Menus.Map;
using FEXNA.Menus.Map.Unit;
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
    partial class Scene_Map : IMapMenuHandler, IUnitMenuHandler, IMapDebugMenuHandler
#else
    partial class Scene_Map : IMapMenuHandler, IUnitMenuHandler
#endif
    {
        protected MenuManager MapMenu;
        protected UnitMenuManager UnitMenu;
        
        Parchment_Confirm_Window Map_Save_Confirm_Window;
        private Window_Ranking Ranking_Window;
        private Window_Minimap Minimap;

        protected bool Overwriting_Checkpoint = false;

        #region Accessors
        public bool manual_targeting
        {
            get
            {
                if (UnitMenu != null)
                    return UnitMenu.ManualTargeting;
                return false;
            }
        }
        #endregion

        public bool target_window_up
        {
            get
            {
                if (UnitMenu != null)
                    return UnitMenu.TargetWindowUp;
                return false;
            }
        }
        public bool target_window_has_target
        {
            get
            {
                if (UnitMenu != null)
                    return UnitMenu.TargetWindowHasTarget;
                return false;
            }
        }
        public Vector2 target_window_target_loc
        {
            get
            {
                if (UnitMenu != null)
                    return UnitMenu.TargetWindowTargetLoc;
                throw new NullReferenceException(
                    "Tried to get target window loc, but\nthere is no target window");
            }
        }
        public Vector2 target_window_last_target_loc
        {
            get
            {
                if (UnitMenu != null)
                    return UnitMenu.TargetWindowLastTargetLoc;
                throw new NullReferenceException(
                    "Tried to get target window loc, but\nthere is no target window");
            }
        }

        public bool combat_target_window_up
        {
            get
            {
                if (UnitMenu != null)
                    return UnitMenu.CombatTargetWindowUp;
                return false;
            }
        }

        public bool drop_target_window_up
        {
            get
            {
                if (UnitMenu != null)
                    return UnitMenu.DropTargetWindowUp;
                return false;
            }
        }

        protected void update_menu_calls()
        {
            update_preparations_menu_calls();
            if (Global.game_temp.preview_shop_call)
                preview_shop();
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
                //@Debug: needed for discarding at least, shouldn't possibly
                // interfere with anything else?
                if (update_menu_unit())
                    return;
                return;
            }
            if (update_menu_map())
                return;
            if (update_menu_unit())
                return;
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
            if (UnitMenu != null)
            {
                UnitMenu.Update();
                if (UnitMenu != null && UnitMenu.Finished)
                    UnitMenu = null;
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

        protected virtual void open_unit_menu()
        {
            UnitMenu = UnitMenuManager.CommandMenu(this);
        }

        #region Attack
        
        private static void sort_aoe_targets(int unitId)
        {
            Global.game_system.Aoe_Targets.Sort(delegate(int a, int b) {
                return Global.game_map.unit_distance(unitId, a) - Global.game_map.unit_distance(unitId, b); });
        }
        #endregion
        
        // Close
        protected void close_unit_menu(bool move_range_visible = true)
        {
            UnitMenu = null;
            if (move_range_visible)
                Global.game_map.move_range_visible = true;

            // Reset context sensitive values on unit menu close
            Global.game_temp.ResetContextSensitiveUnitControl();
        }
        
        protected bool is_unit_command_window_open { get { return UnitMenu != null; } }
        
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
                if (UnitMenu.ShowAttackRange)
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
                if (UnitMenu.ShowStaffRange)
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
                if (UnitMenu.ShowTalkRange)
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

        #region IUnitMenuHandler
        public void UnitMenuAttack(Game_Unit unit, int targetId)
        {
            Global.game_map.clear_move_range();
            // Lock in unit movement
            unit.moved();
            // Skills: Trample
            if (unit.trample_activated)
                unit.set_trample_loc();
            // Skills: Old Swoop //@Debug
            if (unit.old_swoop_activated)
            {
                Global.game_state.call_battle(unit.id,
                    unit.enemies_in_old_swoop_range(unit.facing)[0]);
                sort_aoe_targets(unit.id);
                // Adds to list of attacked units for this turn
                foreach (int id in Global.game_system.Aoe_Targets)
                    if (Global.game_map.units.ContainsKey(id) &&
                            unit.is_attackable_team(Global.game_map.units[id]))
                        unit.add_attack_target(id);

                unit.same_target_support_gain_display(Global.game_system.Aoe_Targets);
            }
            else
            {
                Global.game_state.call_battle(unit.id, targetId);
                // Adds to list of attacked units for this turn
                if (Global.game_map.units.ContainsKey(targetId) &&
                        unit.is_attackable_team(Global.game_map.units[targetId]))
                {
                    unit.add_attack_target(targetId);

                    unit.same_target_support_gain_display(targetId);
                }
            }

            Global.game_temp.clear_temp_range();
            Global.game_temp.menuing = false;
            close_unit_menu(true);
        }

        public void UnitMenuStaff(Game_Unit unit, int targetId, Vector2 targetLoc)
        {
            Global.game_map.clear_move_range();
            // Lock in unit movement
            unit.moved();

            Global.game_state.call_staff(
                unit.id, targetId,
                targetLoc);

            Global.game_state.call_battle(unit.id, targetId);
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
                if (Global.game_map.units.ContainsKey(targetId))
                {
                    // Adds to list of attacked units for this turn
                    if (unit.is_attackable_team(Global.game_map.units[targetId]))
                    {
                        unit.add_attack_target(targetId);

                        unit.same_target_support_gain_display(targetId);
                    }
                    else
                        unit.heal_support_gain_display(targetId);
                }
            }
            
            Global.game_temp.clear_temp_range();
            Global.game_temp.menuing = false;
            close_unit_menu(true);
        }

        public void UnitMenuRescue(Game_Unit unit, int targetId)
        {
            Global.game_map.clear_move_range();
            // Lock in unit movement
            unit.moved();

            unit.rescue_ally(targetId);
            unit.rescue_support_gain_display(targetId);
            
            Global.game_temp.menuing = false;
            close_unit_menu(true);
        }
        public void UnitMenuDrop(Game_Unit unit, Vector2 targetLoc)
        {
            Global.game_map.clear_move_range();
            // Lock in unit movement
            unit.moved();

            unit.drop_ally(targetLoc);
            
            Global.game_temp.menuing = false;
            close_unit_menu(true);
        }

        public void UnitMenuUseItem(Game_Unit unit, int itemIndex, Maybe<Vector2> targetLoc)
        {
            Global.game_map.clear_move_range();
            unit.cantoing = false;
            // Lock in unit movement
            unit.moved();

            if (targetLoc.IsSomething)
            {
                Global.game_state.call_item(
                    unit.id, itemIndex, targetLoc);
            }
            else
                Global.game_state.call_item(unit.id, itemIndex);

            Global.game_temp.menuing = false;
            close_unit_menu(false);
        }

        public void UnitMenuWait(Game_Unit unit)
        {
            unit.start_wait();
            Global.game_temp.menuing = false;
            close_unit_menu(true);
            suspend();
        }

        public void UnitMenuTake(Game_Unit unit, int targetId, Canto_Records canto)
        {
            Global.game_map.clear_move_range();
            // Lock in unit movement
            unit.moved();
            
            unit.take_ally(targetId);
            Global.game_system.Menu_Canto = canto | Canto_Records.Take;

            Global.game_temp.menuing = false;
            close_unit_menu(true);
        }
        public void UnitMenuGive(Game_Unit unit, int targetId, Canto_Records canto)
        {
            Global.game_map.clear_move_range();
            // Lock in unit movement
            unit.moved();

            unit.give_ally(targetId);
            Global.game_system.Menu_Canto = canto | Canto_Records.Give;

            Global.game_temp.menuing = false;
            close_unit_menu(true);
        }

        public void UnitMenuTalk(Game_Unit unit, int targetId, Canto_Records canto)
        {
            Global.game_map.remove_updated_move_range(unit.id);
            Global.game_map.clear_move_range();
            Global.game_map.range_start_timer = 0;

            Global.game_state.activate_talk(unit.id, targetId);
            Global.game_temp.menuing = false;
            close_unit_menu(true);

            unit.cantoing = false;
            // Lock in unit movement
            unit.moved();
            bool cantoMovement = unit.has_canto() && !unit.full_move();
            if (Constants.Gameplay.TALKING_IS_FREE_ACTION)
            {
                Global.game_system.Menu_Canto = canto | Canto_Records.Talk |
                    (cantoMovement ? Canto_Records.Horse : Canto_Records.None);
                unit.cantoing = true;
            }
            else if (cantoMovement)
                unit.cantoing = true;
            Global.player.facing = 6;
            Global.player.frame = 1;
        }

        public void UnitMenuDoor(Game_Unit unit, Vector2 targetLoc)
        {
            Global.game_map.remove_updated_move_range(unit.id);
            Global.game_map.clear_move_range();
            Global.game_map.range_start_timer = 0;

            Global.game_state.call_visit(FEXNA.State.Visit_Modes.Door, unit.id, targetLoc);
            Global.game_temp.menuing = false;
            close_unit_menu(true);

            unit.cantoing = false;
            // Lock in unit movement
            unit.moved();
            if (unit.has_canto() && !unit.full_move())
                unit.cantoing = true;
        }

        public void UnitMenuSteal(Game_Unit unit, int targetId, int itemIndex)
        {
            Global.game_map.remove_updated_move_range(unit.id);
            Global.game_map.clear_move_range();
            Global.game_map.range_start_timer = 0;

            Global.game_state.call_steal(unit.id, targetId, itemIndex);
            // Adds to list of attacked units for this turn
            if (Global.game_map.units.ContainsKey(Global.game_system.Battler_2_Id) &&
                    unit.is_attackable_team(Global.game_map.units[Global.game_system.Battler_2_Id]))
            {
                unit.add_attack_target(Global.game_system.Battler_2_Id);

                unit.same_target_support_gain_display(Global.game_system.Battler_2_Id);
            }
            Global.game_temp.menuing = false;
            close_unit_menu(true);
            
            unit.cantoing = false;
            // Lock in unit movement
            unit.moved();
            if (unit.has_canto() && !unit.full_move())
                unit.cantoing = true;
        }

        public void UnitMenuSeize(Game_Unit unit)
        {
            Global.game_map.seize_point(unit.team, unit.loc);
            Global.game_system.Selected_Unit_Id = -1;

            Global.game_temp.menuing = false;
            close_unit_menu(true);
            unit.start_wait();
            suspend();
        }

        public void UnitMenuDance(Game_Unit unit, int targetId, int ringIndex)
        {
            Global.game_map.remove_updated_move_range(unit.id);
            Global.game_map.clear_move_range();
            Global.game_map.range_start_timer = 0;

            Global.game_state.call_dance(unit.id, targetId, ringIndex);
            Global.game_temp.menuing = false;
            close_unit_menu(true);
            
            unit.cantoing = false;
            // Lock in unit movement
            unit.moved();
            if (unit.has_canto() && !unit.full_move())
                unit.cantoing = true;
        }

        public void UnitMenuSupport(Game_Unit unit, int targetId)
        {
            Global.game_map.remove_updated_move_range(unit.id);
            Global.game_map.clear_move_range();
            Global.game_map.range_start_timer = 0;

            Global.game_state.call_support(unit.id, targetId);
            Global.game_temp.menuing = false;
            close_unit_menu(true);

            unit.cantoing = false;
            // Lock in unit movement
            unit.moved();
            if (unit.has_canto() && !unit.full_move())
                unit.cantoing = true;
            Global.player.facing = 6;
            Global.player.frame = 1;
        }

        public void UnitMenuEscape(Game_Unit unit)
        {
            Global.game_map.remove_updated_move_range(unit.id);
            Global.game_map.clear_move_range();
            Global.game_map.range_start_timer = 0;

            Global.game_state.call_visit(FEXNA.State.Visit_Modes.Escape, unit.id, unit.loc);
            Global.game_temp.menuing = false;
            close_unit_menu(true);
            Global.game_system.Selected_Unit_Id = -1;

            unit.cantoing = false;
            // Lock in unit movement
            unit.moved();
            if (unit.has_canto() && !unit.full_move())
                unit.cantoing = true;
        }

        public void UnitMenuAssemble(Game_Unit unit, int itemIndex, Vector2 targetLoc)
        {
            //Global.game_state.call_assemble( //@Debug
            //    unit.id,
            //    target_loc,
            //    itemIndex);

            // Simplified add siege engine //@Debug
            Item_Data item;
            // Take the item out of its inventory
            // If the item is in the unit inventory
            if (itemIndex < unit.actor.whole_inventory.Count)
            {
                item = unit.actor.whole_inventory[itemIndex];
                unit.actor.discard_item(itemIndex);
            }
            // Else it's in the convoy
            else
            {
                int index = itemIndex -
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
            Global.game_map.add_siege_engine(targetLoc, item);

            EndConstruct(unit);
        }
        public void UnitMenuReload(Game_Unit unit, Vector2 targetLoc)
        {
            //Global.game_state.call_reload( //@Debug
            //    unit.id,
            //    target_loc);

            // Simplified reload siege engine //@Debug
            var siege = Global.game_map.get_siege(targetLoc);
            siege.reload();

            EndConstruct(unit);
        }
        public void UnitMenuReclaim(Game_Unit unit, Vector2 targetLoc)
        {
            //Global.game_state.call_reclaim( //@Debug
            //    unit.id,
            //    target_loc);

            // Simplified reclaim siege engine //@Debug
            var siege = Global.game_map.get_siege(targetLoc);
            if (!siege.item.out_of_uses)
            {
                Global.game_battalions.add_item_to_convoy(siege.item);
                Global.game_battalions.sort_convoy(
                    Global.battalion.convoy_id);
            }
            Global.game_map.remove_siege_engine(siege.id);

            EndConstruct(unit);
        }

        private void EndConstruct(Game_Unit unit)
        {
            Global.game_temp.menuing = false;
            close_unit_menu(true);

            unit.cantoing = false;
            // Lock in unit movement
            unit.moved();
            if (unit.has_canto() && !unit.full_move())
                unit.cantoing = true;



            // Simplified wait @Debug
            unit.start_wait();
            suspend();
        }

        // Skills: Savior
        public void UnitMenuShelter(Game_Unit unit, int targetId)
        {
            Global.game_map.clear_move_range();
            // Lock in unit movement
            unit.moved();

            unit.cover_ally(targetId);
            unit.rescue_support_gain_display(targetId);

            Global.game_temp.menuing = false;
            close_unit_menu(true);
        }
        public void UnitMenuRefuge(Game_Unit unit, int targetId)
        {
            Global.game_map.clear_move_range();
            // Lock in unit movement
            unit.moved();

            unit.take_refuge(targetId);
            unit.rescue_support_gain_display(targetId);

            Global.game_temp.menuing = false;
            close_unit_menu(true);
        }

        // Skills: Sacrifice
        public void UnitMenuSacrifice(Game_Unit unit, int targetId)
        {
            Global.game_map.remove_updated_move_range(unit.id);
            Global.game_map.clear_move_range();
            Global.game_map.range_start_timer = 0;

            Global.game_state.call_sacrifice(unit.id, targetId);
            Global.game_temp.menuing = false;
            close_unit_menu(true);

            unit.cantoing = false;
            // Lock in unit movement
            unit.moved();
            if (unit.has_attack_canto() && !unit.full_move())
                unit.cantoing = true;
        }

        public void UnitMenuDiscard(Game_Unit unit, int index)
        {
            FEXNA_Library.Item_Data discarded_item =
                unit.actor.whole_inventory[index];
            if (Global.battalion.convoy_ready_for_sending)
            {
                Global.game_battalions.add_item_to_convoy(discarded_item);
                set_item_sent_popup(discarded_item, 240);
            }
            else
                set_item_drop_popup(discarded_item, 240);

            unit.actor.discard_item(index);
            unit.actor.organize_items();

            // If the inventory is still too full after discarding,
            // the discard menu just goes inactive until the map popup closes
            if (unit.actor.too_many_items)
            {
                UnitMenu = UnitMenuManager.ReopenDiscard(this);
            }
            else
            {
                UnitMenu = null;
                if (!Global.game_system.preparations)
                    Global.game_temp.menuing = false;
                Global.game_temp.discard_menuing = false;
                Global.game_temp.force_send_to_convoy = false;
            }
        }
        #endregion
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
            Global.game_system.play_se(System_Sounds.Confirm);
            UnitMenu = UnitMenuManager.StatusScreen(this);
        }
        #endregion

        #region Discard Menu
        protected void open_discard_menu()
        {
            Global.game_system.play_se(System_Sounds.Open);
            UnitMenu = UnitMenuManager.Discard(this);
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
        protected void preview_shop()
        {
            UnitMenu = UnitMenuManager.PreviewShop(this, Global.game_map.get_shop());
        }
        
        public void resume_arena()
        {
            UnitMenu = UnitMenuManager.ResumeArena(this);
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
            if (UnitMenu != null)
                if (!Global.game_temp.discard_menuing)
                    UnitMenu.Draw(sprite_batch);
            
            if (Map_Save_Confirm_Window != null) Map_Save_Confirm_Window.draw(sprite_batch);
            if (Ranking_Window != null) Ranking_Window.draw(sprite_batch);
        }

        protected void draw_discard(SpriteBatch sprite_batch)
        {
            if (UnitMenu != null)
                if (Global.game_temp.discard_menuing)
                    UnitMenu.Draw(sprite_batch);
        }

        protected virtual void clear_menus()
        {
            close_map_menu();

            MapMenu = null;
            UnitMenu = null;

            Map_Save_Confirm_Window = null;
        }

        internal static bool intro_chapter_options_blocked()
        {
            return Global.game_system.chapter_id == "Pre";
        }

        internal static bool debug_chapter_options_blocked()
        {
            //FEGame
            if (Global.game_system.chapter_id == "Ch23")
                return true;

            return false;
        }
    }
}
