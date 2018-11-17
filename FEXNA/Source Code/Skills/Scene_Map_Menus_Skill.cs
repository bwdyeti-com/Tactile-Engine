using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Windows.Command.Items;
using FEXNA.Windows.Target;
using IntExtension;
using FEXNAWeaponExtension;

namespace FEXNA
{
    enum Skill_Menu_Ids : int
    {
        Cover = 0,
        Dash = 1,
        Swoop = 2,
        Trample = 3,
        Sacrifice = 4,
        Refuge = 5,
        OldSwoop = 20
    }
    partial class Scene_Map
    {
        const int BASE_SKILL_MENU_ID = 100;
        const int BASE_MASTERY_MENU_ID = 200;

        //Debug
        /*protected static List<int> attack_skill_menu_ids()
        {
            return BASE_SKILL_MENU_ID.list_add(new List<int> {
                (int)Skill_Menu_Ids.Swoop,
                (int)Skill_Menu_Ids.Trample,
                (int)Skill_Menu_Ids.OldSwoop });
        }*/

        protected void skill_commands(ref List<string> commands, Game_Unit unit)
        {
            // Actions:
            //   100 = Cover
            //   101 = Dash
            //   102 = Swoop
            //   103 = Trample
            //   104 = Sacrifice
            //   105 = Refuge
            //   120 = Old Swoop //Debug
            if (canto_allows_normal_actions(Canto))
            {
                // Skills: Savior
                if (commands.Contains("Rescue") && unit.has_cover())
                {
                    List<int> ally_range = unit.allies_in_range(1);
                    bool can_rescue = false;
                    foreach (int id in ally_range)
                        if (unit.can_rescue(Global.game_map.units[id]))
                            if (Pathfind.passable(unit, Global.game_map.units[id].loc))
                            {
                                can_rescue = true;
                                break;
                            }
                    if (can_rescue)
                    {
                        int index = commands.IndexOf("Rescue");
                        commands.Insert(index + 1, "Shelter");
                        add_skill_index(index, Skill_Menu_Ids.Cover);
                    }
                }
                {
                    List<int> ally_range = unit.allies_in_range(1);
                    bool can_take_refuge = false;
                    foreach (int id in ally_range)
                    {
                        Game_Unit target = Global.game_map.units[id];
                        if (target.has_refuge() && target.can_rescue(unit))
                        {
                            can_take_refuge = true;
                            break;
                        }
                    }
                    if (can_take_refuge)
                    {
                        // Try placing after rescue
                        int index = commands.IndexOf("Rescue") + 1;
                        if (index <= 0)
                        {
                            // Try placing before Item
                            index = commands.IndexOf("Item");
                            if (index < 0)
                                // Place before status and wait, at least
                                index = Math.Min(
                                    commands.IndexOf("Status"), commands.IndexOf("Wait"));
                        }
                        commands.Insert(index + 0, "Refuge");
                        add_skill_index(index - 1, Skill_Menu_Ids.Refuge);
                    }
                }
                // Skills: Dash
                if (unit.actor.has_skill("DASH"))
                {
                    if (unit.base_mov > 0 && (unit.turn_start_loc != unit.loc ||
                        Canto != Canto_Records.None))
                    {
                        int index = Math.Min(
                            commands.IndexOf("Status"), commands.IndexOf("Wait"));
                        commands.Insert(index + 0, "Dash");
                        add_skill_index(index - 1, Skill_Menu_Ids.Dash);
                    }
                }
                // Skills: Swoop
                if (unit.actor.has_skill("SWOOP"))
                {
                    List<int>[] ary = unit.enemies_in_swoop_range();
                    List<int> enemy_range = ary[0];
                    if (enemy_range.Count > 0)
                    {
                        Global.game_temp.temp_skill_ranges["SWOOP"] = unit.swoop_range();
                        Global.game_map.range_start_timer = 0;
                        int index = commands.IndexOf("Attack");
                        commands.Insert(index + 1, "Swoop");
                        add_skill_index(index, Skill_Menu_Ids.Swoop);
                    }
                }
                // Skills: Trample
                if (unit.actor.has_skill("TRAMPLE"))
                {
                    List<int>[] ary = unit.enemies_in_trample_range();
                    List<int> enemy_range = ary[0];
                    if (enemy_range.Count > 0)
                    {
                        Global.game_temp.temp_skill_ranges["TRAMPLE"] = unit.trample_range();
                        Global.game_temp.temp_skill_move_ranges["TRAMPLE"] = unit.trample_move_range();
                        Global.game_map.range_start_timer = 0;
                        int index = commands.IndexOf("Attack");
                        commands.Insert(index + 1, "Trample");
                        add_skill_index(index, Skill_Menu_Ids.Trample);
                    }
                }
                // Skills: Sacrifice
                if (unit.actor.has_skill("SACRIFICE"))
                {
                    if (unit.actor.hp > 1)
                    {
                        List<int> ally_range = unit.allies_in_range(1);
                        bool can_heal = false;
                        foreach (int id in ally_range)
                            if (!Global.game_map.units[id].actor.is_full_hp())
                            {
                                can_heal = true;
                                break;
                            }
                        if (can_heal)
                        {
                            int index = commands.IndexOf("Attack");
                            commands.Insert(index + 1, "Sacrifice");
                            add_skill_index(index, Skill_Menu_Ids.Sacrifice);
                        }
                    }
                }
                // Skills: Old Swoop //Debug
                if (unit.actor.has_skill("OLDSWOOP"))
                {
                    List<int>[] ary = unit.enemies_in_old_swoop_range();
                    List<int> enemy_range = ary[0];
                    if (enemy_range.Count > 0)
                    {
                        Global.game_temp.temp_skill_ranges["OLDSWOOP"] = unit.old_swoop_range();
                        Global.game_map.range_start_timer = 0;
                        int index = commands.IndexOf("Attack");
                        commands.Insert(index + 1, "OldSwoop");
                        add_skill_index(index, Skill_Menu_Ids.OldSwoop);
                    }
                }
                // Skills: Masteries
                for (int i = 0; i < Game_Unit.MASTERIES.Count; i++)
                {
                    if (unit.actor.has_skill(Game_Unit.MASTERIES[i]) && unit.is_mastery_ready(Game_Unit.MASTERIES[i]))
                    {
                        string skill = Game_Unit.MASTERIES[i];
                        List<int>[] range_ary = unit.enemies_in_range(skill);
                        if (range_ary[1].Count > 0)
                        {
                            List<int> item_indices = unit.weapon_indices(range_ary[1]);
                            Global.game_temp.temp_skill_ranges[skill] = unit.get_weapon_range(item_indices, new HashSet<Vector2> { unit.loc }, skill);

                            //Global.game_temp.temp_skill_ranges[skill] = Global.game_map.get_unit_range(new List<Vector2> { unit.loc },
                            //    unit.min_range_absolute(skill), unit.max_range_absolute(skill), Game_Unit.mastery_blocked_through_walls(skill));
                            Global.game_map.range_start_timer = 0;
                            int index = commands.IndexOf("Attack");
                            commands.Insert(index + 1, Global.skill_from_abstract(skill).Name);
                            Index_Redirect.Insert(index + 1, BASE_MASTERY_MENU_ID + i);
                        }
                    }
                }
            }
        }

        private void add_skill_index(int index, Skill_Menu_Ids skill)
        {
            Index_Redirect.Insert(index + 1, BASE_SKILL_MENU_ID + (int)skill);
        }

        private bool reopen_skill_attack_target_window(Game_Unit unit)
        {
            // Skills: Swoop
            if (unit.swoop_activated)
            {
                Global.game_temp.temp_skill_ranges["SWOOP"] = unit.swoop_range();
                Attack_Item_Window = new Window_Command_Item_Swoop(Unit_Id, new Vector2(24, 8));
            }
            // Skills: Trample
            else if (unit.trample_activated)
            {
                Global.game_temp.temp_skill_ranges["TRAMPLE"] = unit.trample_range();
                Global.game_temp.temp_skill_move_ranges["TRAMPLE"] = unit.trample_move_range();
                Attack_Item_Window = new Window_Command_Item_Swoop(Unit_Id, new Vector2(24, 8));
            }
            // Skills: Old Swoop //Debug
            else if (unit.old_swoop_activated)
            {
                Global.game_temp.temp_skill_ranges["OLDSWOOP"] = unit.old_swoop_range();
                Attack_Item_Window = new Window_Command_Item_Swoop(Unit_Id, new Vector2(24, 8));
            }
            else
                return false;
            return true;
        }

        private bool command_skill_cancel_unit_command(Game_Unit unit)
        {
            if (unit.DashActivated)
            {
                Global.game_system.Menu_Canto = Canto;
                cancel_unit_command(unit);
                return true;
            }
            return false;
        }

        protected bool unit_menu_select_skill(int option, Game_Unit unit)
        {
            // Skills: Masteries
            if (option >= BASE_MASTERY_MENU_ID && option < BASE_MASTERY_MENU_ID + Game_Unit.MASTERIES.Count)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Unit_Command_Window.active = false;
                Unit_Command_Window.visible = false;
                Global.game_map.range_start_timer = 0;
                string skill = Game_Unit.MASTERIES[option - BASE_MASTERY_MENU_ID];
                unit.call_mastery(skill);
                Attack_Item_Window = new Window_Command_Item_Attack(Unit_Id, new Vector2(24, 8), skill);
                Global.game_temp.temp_attack_range = Global.game_map.get_unit_range(new HashSet<Vector2> { unit.loc },
                    unit.min_range(Attack_Item_Window.redirect(), skill), unit.max_range(Attack_Item_Window.redirect(), skill),
                    Global.data_weapons[unit.items[Attack_Item_Window.redirect()].Id].range_blocked_by_walls());
            }
            else
                switch (option)
                {
                    // Skills: Savior
                    case BASE_SKILL_MENU_ID + (int)Skill_Menu_Ids.Cover:
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Unit_Target_Window = new Window_Target_Rescue(unit.id, 4, new Vector2(-80, 0));
                        Unit_Command_Window.active = false;
                        Unit_Command_Window.visible = false;
                        Global.player.facing = 4;
                        Global.player.update_cursor_frame();
                        break;
                    // Skills: Dash
                    case BASE_SKILL_MENU_ID + (int)Skill_Menu_Ids.Dash:
                        Global.game_system.play_se(System_Sounds.Confirm);

                        Global.game_temp.menuing = false;
                        Global.game_system.Menu_Canto = Canto | Canto_Records.Dash;
                        close_unit_menu(true);

                        unit.activate_dash();
                        break;
                    // Skills: Swoop
                    case BASE_SKILL_MENU_ID + (int)Skill_Menu_Ids.Swoop:
                        Global.game_system.play_se(System_Sounds.Confirm);
                        unit.swoop_activated = true;
                        Unit_Command_Window.active = false;
                        Unit_Command_Window.visible = false;
                        Global.game_map.range_start_timer = 0;
                        Attack_Item_Window = new Window_Command_Item_Swoop(Unit_Id, new Vector2(24, 8));
                        Global.game_temp.temp_skill_ranges["SWOOP"] = unit.swoop_range();
                        break;
                    // Skills: Trample
                    case BASE_SKILL_MENU_ID + (int)Skill_Menu_Ids.Trample:
                        Global.game_system.play_se(System_Sounds.Confirm);
                        unit.trample_activated = true;
                        Unit_Command_Window.active = false;
                        Unit_Command_Window.visible = false;
                        Global.game_map.range_start_timer = 0;
                        Attack_Item_Window = new Window_Command_Item_Trample(Unit_Id, new Vector2(24, 8));
                        Global.game_temp.temp_skill_ranges["TRAMPLE"] = unit.trample_range();
                        Global.game_temp.temp_skill_move_ranges["TRAMPLE"] = unit.trample_move_range();
                        break;
                    // Skills: Sacrifice
                    case BASE_SKILL_MENU_ID + (int)Skill_Menu_Ids.Sacrifice:
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Unit_Target_Window = new Window_Target_Sacrifice(Unit_Id, new Vector2(0, 0));
                        Unit_Command_Window.active = false;
                        Unit_Command_Window.visible = false;
                        Global.player.facing = 4;
                        Global.player.update_cursor_frame();
                        break;
                    // Skills: Savior
                    case BASE_SKILL_MENU_ID + (int)Skill_Menu_Ids.Refuge:
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Unit_Target_Window = new Window_Target_Rescue(unit.id, 5, new Vector2(-80, 0));
                        Unit_Command_Window.active = false;
                        Unit_Command_Window.visible = false;
                        Global.player.facing = 4;
                        Global.player.update_cursor_frame();
                        break;
                    // Skills: Old Swoop //Debug
                    case BASE_SKILL_MENU_ID + (int)Skill_Menu_Ids.OldSwoop:
                        Global.game_system.play_se(System_Sounds.Confirm);
                        unit.old_swoop_activated = true;
                        Unit_Command_Window.active = false;
                        Unit_Command_Window.visible = false;
                        Global.game_map.range_start_timer = 0;
                        Attack_Item_Window = new Window_Command_Item_Swoop(Unit_Id, new Vector2(24, 8));
                        Global.game_temp.temp_skill_ranges["OLDSWOOP"] = unit.old_swoop_range();
                        break;
                    default:
                        return false;
                }
            return true;
        }

        protected void rescue_mode_action_skill(Game_Unit unit)
        {
            switch ((Unit_Target_Window as Window_Target_Rescue).mode)
            {
                // Skills: Savior
                case 4:
                    unit.cover_ally(Unit_Target_Window.targets[Unit_Target_Window.index]);
                    break;
                case 5:
                    unit.take_refuge(Unit_Target_Window.targets[Unit_Target_Window.index]);
                    break;
            }
        }

        protected void draw_menu_ranges_skill(SpriteBatch sprite_batch, int width, int timer, Rectangle rect)
        {
            // Skills: Swoop
            // Temp Swoop Range
            if (Global.game_temp.temp_skill_ranges.ContainsKey("SWOOP") &&
                Index_Redirect[Unit_Command_Window.index] == BASE_SKILL_MENU_ID + (int)Skill_Menu_Ids.Swoop && 
                (Unit_Command_Window.active || Attack_Item_Window != null))
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                int opacity = 144;
                Color color = new Color(opacity, opacity, opacity, opacity);
                foreach (Vector2 loc in Global.game_temp.temp_skill_ranges["SWOOP"])
                {
                    sprite_batch.Draw(Attack_Range_Texture,
                        loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                        rect, color);
                }
                sprite_batch.End();
            }
            // Skills: Trample
            // Temp Swoop Range
            if (Global.game_temp.temp_skill_ranges.ContainsKey("TRAMPLE") &&
                Index_Redirect[Unit_Command_Window.index] == BASE_SKILL_MENU_ID + (int)Skill_Menu_Ids.Trample && 
                (Unit_Command_Window.active || Attack_Item_Window != null))
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                int opacity = 144;
                Color color = new Color(opacity, opacity, opacity, opacity);
                foreach (Vector2 loc in Global.game_temp.temp_skill_move_ranges["TRAMPLE"])
                {
                    sprite_batch.Draw(Move_Range_Texture,
                        loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                        rect, color);
                }
                foreach (Vector2 loc in Global.game_temp.temp_skill_ranges["TRAMPLE"])
                {
                    sprite_batch.Draw(Attack_Range_Texture,
                        loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                        rect, color);
                }
                sprite_batch.End();
            }
            // Skills: Old Swoop //Debug
            // Temp Old Swoop Range
            if (Global.game_temp.temp_skill_ranges.ContainsKey("OLDSWOOP") &&
                Index_Redirect[Unit_Command_Window.index] == BASE_SKILL_MENU_ID + (int)Skill_Menu_Ids.OldSwoop &&
                (Unit_Command_Window.active || Attack_Item_Window != null))
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                int opacity = 144;
                Color color = new Color(opacity, opacity, opacity, opacity);
                foreach (Vector2 loc in Global.game_temp.temp_skill_ranges["OLDSWOOP"])
                {
                    sprite_batch.Draw(Attack_Range_Texture,
                        loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                        rect, color);
                }
                sprite_batch.End();
            }
            // Skills: Masteries
            for (int i = 0; i < Game_Unit.MASTERIES.Count; i++)
            {
                string skill = Game_Unit.MASTERIES[i];
                if (Index_Redirect[Unit_Command_Window.index] == BASE_MASTERY_MENU_ID + i)
                {
                    if (Global.game_temp.temp_skill_ranges.ContainsKey(skill) && Unit_Command_Window.active)
                    {
                        sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                        int opacity = 144;
                        Color color = new Color(opacity, opacity, opacity, opacity);
                        foreach (Vector2 loc in Global.game_temp.temp_skill_ranges[skill])
                        {
                            sprite_batch.Draw(Attack_Range_Texture,
                                loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                                rect, color);
                        }
                        sprite_batch.End();
                    }
                    // Temp Attack Range
                    else if (Attack_Item_Window != null)
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
                }
            }
        }



        #region Sacrifice
        protected void update_sacrifice_target_menu()
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
                Global.game_state.call_sacrifice(Unit_Id, Unit_Target_Window.targets[Unit_Target_Window.index]);
                Global.game_temp.menuing = false;
                close_unit_menu(true);
                Unit_Target_Window = null;

                unit.cantoing = false;
                // Lock in unit movement
                unit.moved();
                if (unit.has_attack_canto() && !unit.full_move())
                    unit.cantoing = true;
            }
        }
        #endregion
    }
}
