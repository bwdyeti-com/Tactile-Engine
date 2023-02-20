using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Tactile.Map; //Debug
using TactileLibrary;
using TactileStringExtension;
#if DEBUG
using System.Diagnostics;
#endif

namespace Tactile
{
    partial class Event_Processor
    {
        private static Random rand = new Random();

        private string process_lines(string str)
        {
            string[] lines = str.Split(new string[] { @"\n" }, StringSplitOptions.None);
            string result = lines[0];
            for (int i = 1; i < lines.Length; i++)
                result += "\n" + lines[i];
            return result;
        }

        private int process_number(string str)
        {
            return process_number(str, false);
        }
        private int process_number(string str, bool allow_unit)
        {
            if (str.substring(0, 8) == "PlayerId")
                return Constants.Team.PLAYER_TEAM;
            if (str.substring(0, 7) == "EnemyId")
                return Constants.Team.ENEMY_TEAM;
            if (str.substring(0, 9) == "CitizenId")
                return Constants.Team.CITIZEN_TEAM;
            if (str.substring(0, 10) == "IntruderId")
                return Constants.Team.INTRUDER_TEAM;
            if (str == "Gold")
                return Global.battalion.gold;
            if (str.substring(0, 22) == "First_Saved_Deployment")
            {
#if DEBUG
                Debug.Assert(Global.battalion.deployed_but_not_on_map.Count > 0);
#endif
                return Global.battalion.deployed_but_not_on_map[0];
            }
            if (str.substring(0, 20) == "Last_Battalion_Actor")
            {
#if DEBUG
                Debug.Assert(Global.battalion.actors.Count > 0);
#endif
                return Global.battalion.actors[Global.battalion.actors.Count - 1];
            }
            if (str.substring(0, 13) == "Visitor_Actor")
            {
#if DEBUG
                Debug.Assert(Global.game_state.event_caller_unit != null,
                    "Trying to get Visitor_Actor as an event value,\nbut no unit is currently a visitor\n(\"Global.game_state.event_caller_unit\" is null)");
#endif
                return Global.game_state.event_caller_unit.actor.id;
            }
            // Rankings
            if (str == "TurnsRanking")
             {
                return new Game_Ranking().turns;
            }
            if (str == "CombatRanking")
            {
                return new Game_Ranking().combat;
            }
            if (str == "ExpRanking")
            {
                return new Game_Ranking().exp;
            }
            if (str == "CompletionRanking")
            {
                return new Game_Ranking().completion;
            }

            if (str == "DefeatedAllies")
                return Global.game_map.defeated_ally_count();

            if (str.substring(0, 8) == "Variable")
                return VARIABLES[Convert.ToInt32(str.substring(8, str.Length - 8))];

            if (str.substring(0, 18) == "LastDialoguePrompt")
            {
#if DEBUG
                if (Global.game_temp.LastDialoguePrompt.IsNothing)
                    throw new ArgumentException();
#endif
                return Global.game_temp.LastDialoguePrompt;
            }

            var weapon_type = Global.weapon_types.FirstOrDefault(x => x.EventName == str);
            if (weapon_type != null)
                return Global.weapon_types.IndexOf(weapon_type);
            //if (str == "None") //Debug
            //    return (int)TactileLibrary.Weapon_Types.None;
            //else if (TactileLibrary.Data_Weapon.WEAPON_TYPE_NAMES.Contains(str))
            //    return TactileLibrary.Data_Weapon.WEAPON_TYPE_NAMES.ToList().IndexOf(str);

            if (allow_unit)
                return process_unit_id(str);
            else
                return Convert.ToInt32(str);
        }

#if DEBUG
        private int process_unit_id(string str, bool ignore_error = false)
#else
        private int process_unit_id(string str)
#endif
        {
            if (Global.game_map.unit_from_identifier(str) != -1)
                return Global.game_map.unit_from_identifier(str);
            if (str.substring(0, 12) == "Visitor_Unit")
                return Global.game_state.event_caller_unit.id;
            // This only works pre-battle because it gets set to -1 during battle setup //Yeti
            if (str.substring(0, 9) == "Battler_1")
                return Global.game_system.Battler_1_Id;
            if (str.substring(0, 9) == "Battler_2")
                return Global.game_system.Battler_2_Id;
            if (str.substring(0, 15) == "Last_Added_Unit")
                return Global.game_map.last_added_unit.id;
            if (str.substring(0, 19) == "Last_Battalion_Unit")
            {
#if DEBUG
                Debug.Assert(Global.battalion.actors.Count > 0);
#endif
                return Global.game_actors.get_unit_from_actor(Global.battalion.actors[Global.battalion.actors.Count - 1]);
            }
            if (str.substring(0, 11) == "Unit_at_Loc")
            {
                Vector2 loc = process_vector2(str.Substring(11));
                Game_Unit unit = Global.game_map.get_unit(loc);
                return unit == null ? 0 : unit.id;
            }
            if (str.substring(0, 13) == "Unit_of_Actor")
            {
                int actor_id = Convert.ToInt32(str.substring(13, str.Length - 13));
                return Global.game_actors.get_unit_from_actor(actor_id);
            }
            if (str.substring(0, 18) == "First_Generic_Unit")
            {
                string actor_name = str.substring(18, str.Length - 18);
                return Global.game_actors.get_first_generic_from_name(actor_name);
            }
            if (str.substring(0, 13) == "MainCharacter")
            {
                return Global.game_map.highest_priority_unit(Constants.Team.PLAYER_TEAM);
            }
            if (str.substring(0, 8) == "Variable")
                return VARIABLES[Convert.ToInt32(str.substring(8, str.Length - 8))];

            double num_test;
            if (double.TryParse(str, System.Globalization.NumberStyles.Any,
                    System.Globalization.NumberFormatInfo.InvariantInfo, out num_test))
                return Convert.ToInt32(str);
            else
            {
#if DEBUG
                if (!ignore_error)
                    Print.message(string.Format("Event string failed to convert to number:\n{0}", str));
#endif
                return 0;
            }
        }

        private float process_float(string str)
        {
            return (float)Convert.ToDouble(str, System.Globalization.CultureInfo.InvariantCulture);
        }

        private bool process_bool(string str)
        {
            if (str.substring(0, 6) == "Switch")
                return SWITCHES[Convert.ToInt32(str.substring(6, str.Length - 6))];

            if (str.substring(0, 22) == "LastConfirmationPrompt")
            {
#if DEBUG
                if (Global.game_temp.LastConfirmationPrompt.IsNothing)
                    throw new ArgumentException();
#endif
                return Global.game_temp.LastConfirmationPrompt;
            }

            return str == "true";
        }

        private Vector2 process_vector2(string str)
        {
            string[] str_ary = str.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            return new Vector2(process_number(str_ary[0]), process_number(str_ary[1]));
        }

        private Rectangle rect_from_edges(int x1, int y1, int x2, int y2)
        {
            int x1a = Math.Min(x1, x2);
            int y1a = Math.Min(y1, y2);
            int x2a = Math.Max(x1, x2);
            int y2a = Math.Max(y1, y2);
            return new Rectangle(x1a, y1a, (x2a + 1) - x1a, (y2a + 1) - y1a);
        }

        private void get_actor(string isUnitString, string idString, out Game_Actor actor)
        {
            Game_Unit unit;
            get_actor(isUnitString, idString, out actor, out unit);
        }
        private void get_actor(string isUnitString, string idString, out Game_Actor actor, out Game_Unit unit)
        {
            unit = null;
            actor = null;

            // Get unit, and then get that unit's actor
            int id;
            if (process_bool(isUnitString))
            {
#if DEBUG
                if (idString != "-1" && idString != "Last_Added_Unit")
                { } //Debug
#endif
                id = process_unit_id(idString);
                if (id == -1)
                    if (Global.game_map.last_added_unit != null)
                        unit = Global.game_map.last_added_unit;
                if (Global.game_map.units.ContainsKey(id))
                    unit = Global.game_map.units[id];

                if (unit != null)
                    actor = unit.actor;
            }
            // Get actor
            else
            {
                id = process_number(idString);
                if (Global.game_actors.ContainsKey(id))
                    actor = Global.game_actors[id];
            }
        }

#if DEBUG
        private ArgumentException event_case_missing_exception(string event_case, int key)
        {
            return new ArgumentException(string.Format(
                                    "Invalid command case \"{0}\" for event control {1}: {2}",
                                    event_case, key, event_control_name(key)));
        }
#endif

        // Prep Message
        private bool command_prepmessage()
        {
            // Value[0] = Message Source
            // Value[1] = Message Value
            //?Value[2] = Continuation text? (optional)

            // If this is a continuation text, only show this text if the text window isn't closed
            if (!(command.Value.Length > 2 && process_bool(command.Value[2])) ||
                Global.scene.is_message_window_waiting)
            {
                if (Global.game_temp.message_text == null)
                    Global.game_temp.message_text = "";
#if DEBUG
                if (command.Value.Length == 1)
                {
                    int test = 0;
                    test++;
                }
#endif
                switch (command.Value[0])
                {
                    case "Chapter":
#if DEBUG
                        if (!Global.chapter_text.ContainsKey(command.Value[1]))
                            Print.message(string.Format("No text with the key \"{0}\" exists for this chapter", command.Value[1]));
                        else
#endif
                            if (Global.chapter_text.ContainsKey(command.Value[1]))
                                Global.game_temp.message_text += Global.chapter_text[command.Value[1]];
                        break;
                    case "Global":
#if DEBUG
                        if (!Global.global_text.ContainsKey(command.Value[1]))
                            throw new System.IndexOutOfRangeException();
                        else
#endif
                            if (Global.global_text.ContainsKey(command.Value[1]))
                                Global.game_temp.message_text += Global.global_text[command.Value[1]];
                        break;
                    case "Death Quotes":
#if DEBUG
                        if (!Global.death_quotes.ContainsKey(command.Value[1]))
                            throw new System.IndexOutOfRangeException();
                        else
#endif
                            if (Global.death_quotes.ContainsKey(command.Value[1]))
                                Global.game_temp.message_text += Global.death_quotes[command.Value[1]];
                        break;
                    case "Supports":
#if DEBUG
                        if (!Global.supports.ContainsKey(command.Value[1]))
                            throw new System.IndexOutOfRangeException();
                        else
#endif
                            if (Global.supports.ContainsKey(command.Value[1]))
                                Global.game_temp.message_text += Global.supports[command.Value[1]];
                        break;
#if DEBUG
                    default:
                        throw event_case_missing_exception(command.Value[0], command.Key);
#endif
                }
            }
            Index++;
            return true;
        }

        // Run Message
        private bool command_runmessage()
        {
            // If starting a convo
            if (Global.game_temp.message_text != null)
            {
                if (string.IsNullOrEmpty(Global.game_temp.message_text))
                    Global.game_temp.message_text = null;
                else if (Global.scene.is_message_window_waiting)
                    Global.scene.append_message();
                else
                    Global.scene.new_message_window();
            }
            // Resumes a currently active convo that was paused for events
            else if (Global.scene.is_message_window_waiting)
            {
                Global.scene.resume_message();
                // If command.Value[0] is true, don't move past this even control until the convo is done
                if (command.Value.Length > 0 && process_bool(command.Value[0]))
                {
                    return false;
                }
            }
#if DEBUG
            else
            {
                //throw new ArgumentException("Event attempted to run a conversation with no text loaded");
            }
#endif
            Index++;
            return false;
        }

        // Wait
        private bool command_wait()
        {
            // Minus 1 because events continue to wait on 0
            Wait_Time = process_number(command.Value[0]) - 1;
            Wait_Time = Math.Max(0, Wait_Time);
            //Global.game_map.wait_time = 2;
            Index++;
            return Wait_Time < 0;
        }

        // Change Chapter
        private bool command_chapter()
        {
            // Value[0]   = load data from chapter, or send data keys directly
            // Value[1]   = immediate transition or black out screen
            // Value[2-4] = data
            if (Global.scene.is_strict_map_scene)
            {
                if ((Global.scene as Scene_Map).map_transition_running)
                    return false;
                // If the transition is immediate, or the map is ready for an immediate transition, or if this is in reaction to events being skipped
                if (command.Value[1] != "true" || (Global.scene as Scene_Map).map_transition_ready || Skipping)
                {
                    // If skipping this event, blacken the screen to hide the transition
                    if (Skipping)
                        (Global.scene as Scene_Map).black_screen(20);

                    if (process_bool(command.Value[1]))
                        (Global.scene as Scene_Map).transition_in();
                    // This prevents this event from being updated once instantly after changing maps, when the new map asks to update events
                    Wait_Time = 1;

                    if (process_bool(command.Value[0]))
                        (Global.scene as Scene_Map).set_map(Global.data_chapters[command.Value[2]]);
                    else
                        (Global.scene as Scene_Map).set_map(
                            (command.Value.Length <= 2 || command.Value[2] == "null") ? "" : command.Value[2],
                            (command.Value.Length <= 3 || command.Value[3] == "null") ? "" : command.Value[3],
                            (command.Value.Length <= 4 || command.Value[4] == "null") ? "" : command.Value[4]);
                    bool result = true;// command.Value[1] != "true";
                    Index++;
                    return result;
                }
                else if (process_bool(command.Value[1]) && !(Global.scene as Scene_Map).map_transition)
                {
                    (Global.scene as Scene_Map).transition_out();
                }
            }
            return false;
        }

        // Screen Tone
        private bool command_tone()
        {
            Global.game_state.change_screen_tone(process_number(command.Value[0]), process_number(command.Value[1]),
                process_number(command.Value[2]), process_number(command.Value[3]), process_number(command.Value[4]));
            Index++;
            return true;
        }

        // Warp Player
        private bool command_player_warp()
        {
            // If no parameters are given, just fixes the map position to its bounds
            if (command.Value.Length == 0)
            {
                Global.game_map.fix_scroll_loc();
            }
            else
            {
                // Value[0] = x
                // Value[1] = y
                //?Value[2] = center camera?
                //?Value[3] = center camera immediately?
                // Value[4] = camera x (optional)
                // Value[5] = camera y (optional)
                //?Value[6] = ignore camera restriction to playable? (optional)
                Global.player.force_loc(new Vector2(process_number(command.Value[0]), process_number(command.Value[1])));
                // If setting a specific coordinate for the camera
                if (command.Value.Length > 4)
                {
                    bool ignore_playable_restriction = command.Value.Length >= 7 && process_bool(command.Value[6]);
                    Global.game_map.set_scroll_loc(
                        new Vector2(process_number(command.Value[4]), process_number(command.Value[5])),
                        process_bool(command.Value[3]), ignore_playable_restriction);
                }
                // Should the camera be centered on the cursor?
                else if (process_bool(command.Value[2]))
                {
                    Global.player.center(event_called: true);
                    // Should the camera center instantly instead of panning?
                    if (process_bool(command.Value[3]))
                    {
                        Global.game_system.Instant_Move = true;
                        Global.game_map.update_scroll_position();
                    }
                }
                else
                {
                    int test = 0;
                    test++;
                }
            }
            Index++;
            return true;
        }

        // Follow Moving Unit
        private bool command_follow_unit()
        {
            // Value[0] = id
            int id = process_unit_id(command.Value[0]);
            Game_Unit unit = null;
            if (id == -1)
                if (Global.game_map.last_added_unit != null)
                    unit = Global.game_map.last_added_unit;
            if (Global.game_map.units.ContainsKey(id))
                unit = Global.game_map.units[id];
            if (unit != null)
            {
                if (unit.move_route_empty)
                    Global.player.following_unit_id = unit.id;
            }
            Index++;
            return true;
        }

        // Warp Unit
        private bool command_unit_warp()
        {
            // Value[0] = id
            // Value[1] = x
            // Value[2] = y
            int id = process_unit_id(command.Value[0]);
            if (id == -1)
                if (Global.game_map.last_added_unit != null)
                    //Global.game_map.last_added_unit.force_loc(new Vector2(process_number(command.Value[1]), process_number(command.Value[2])));
                    id = Global.game_map.last_added_unit.id;
            if (Global.game_map.units.ContainsKey(id))
            {
                Vector2 target_loc = new Vector2(process_number(command.Value[1]), process_number(command.Value[2]));
                if (!Global.game_map.units[id].move_route_empty || Global.game_map.units[id].loc != target_loc)
                {
                    Global.game_map.units[id].force_loc(target_loc);
                    // Move ranges will need to be updated
                    unit_moved = true;
                }
                else
                {
                    id = -1; //Debug breakpoint
                }
            }
            Index++;
            return true;
        }

        // Move Unit
        private bool command_unit_move()
        {
            // If units are dying, wait for them to finish
            if (Global.game_map.units_dying)
                return false;

            int id;
            Game_Unit unit = null;
            float move_speed = -1;
            Vector2 target_loc;

            switch (command.Value[0])
            {
                case "Unit to Loc":
                    // Value[0] = "Unit to Loc"
                    // Value[1] = id
                    // Value[2] = x, y
                    //?Value[3] = move speed
                    //?Value[4] = ignore units
                    id = process_unit_id(command.Value[1]);
                    if (id == -1)
                        if (Global.game_map.last_added_unit != null)
                            unit = Global.game_map.last_added_unit;
                    if (Global.game_map.units.ContainsKey(id))
                        unit = Global.game_map.units[id];

                    if (unit != null)
                    {
                        target_loc = process_vector2(command.Value[2]);
                        bool ignore_units = command.Value.Length <= 4 ? false : process_bool(command.Value[4]);
                        // Tries to move even if the unit is already at the location, in case they're standing on someone
                        if (!ignore_units || unit.loc != target_loc)
                        {
                            bool moved;
                            if (command.Value.Length > 3)
                            {
                                move_speed = process_float(command.Value[3]);
                                moved = unit.evented_move_to(target_loc, false, ignore_units, move_speed: move_speed);
                            }
                            else
                                moved = unit.evented_move_to(target_loc, false, ignore_units);
                            unit.update();
                            // Move ranges will need to be updated
                            unit_moved = true;
                        }
                    }
                    break;
                case "Unit Route":
                    // Value[0] = "Unit Route"
                    // Value[1] = id
                    // Value[2=>] = move route
                    if (try_cancel_ai_skip())
                        return false;

                    id = process_unit_id(command.Value[1]);
                    if (id == -1)
                        if (Global.game_map.last_added_unit != null)
                            unit = Global.game_map.last_added_unit;
                    if (Global.game_map.units.ContainsKey(id))
                        unit = Global.game_map.units[id];

                    if (unit != null)
                    {
                        List<Tuple<EventedMoveCommands, float>> move_route = get_move_route(
                            command.Value.Skip(2)).ToList();
                        unit.evented_move(move_route);
                    }
                    break;
                case "Swap Units":
                    // Value[0] = "Swap Units"
                    // Value[1] = unit 1 id
                    // Value[2] = unit 2 id
                    //?Value[3] = unit 1 speed
                    //?Value[4] = unit 2 speed
                    id = process_unit_id(command.Value[1]);
                    if (id == -1)
                        if (Global.game_map.last_added_unit != null)
                            unit = Global.game_map.last_added_unit;
                    if (Global.game_map.units.ContainsKey(id))
                        unit = Global.game_map.units[id];

                    if (unit != null)
                    {
#if DEBUG
                        if (command.Value.Length > 3)
                            throw new NotImplementedException("Move speed not implemented");
#endif
                        int other_id = process_unit_id(command.Value[2]);
                        Game_Unit other_unit = null;
                        if (other_id == -1)
                            if (Global.game_map.last_added_unit != null)
                                other_unit = Global.game_map.last_added_unit;
                        if (Global.game_map.units.ContainsKey(other_id))
                            other_unit = Global.game_map.units[other_id];

                        if (other_unit != null)
                        {
                            // They need to be different units
                            if (unit.id != other_unit.id)
                            {
                                unit.evented_switch_places(other_unit);

                                unit.update();
                                other_unit.update();
                                // Move ranges will need to be updated
                                unit_moved = true;
                            }
                        }
                    }
                    break;
                default:
                    // If 5 commands
                    // Value[0] = id
                    // Value[1] = x
                    // Value[2] = y
                    //?Value[3] = ignore terrain
                    //?Value[4] = ignore units
                    // If 2 commands
                    // Value[0] = unit 1 id
                    // Value[1] = unit 2 id
                    id = process_unit_id(command.Value[0]);
                    if (id == -1)
                        if (Global.game_map.last_added_unit != null)
                            unit = Global.game_map.last_added_unit;
                    if (Global.game_map.units.ContainsKey(id))
                        unit = Global.game_map.units[id];

                    if (unit != null)
                    {
                        // If two commands, have two units switch places instead
                        if (command.Value.Length == 2)
                        {
                            int other_id = process_unit_id(command.Value[1]);
                            Game_Unit other_unit = null;
                            if (other_id == -1)
                                if (Global.game_map.last_added_unit != null)
                                    other_unit = Global.game_map.last_added_unit;
                            if (Global.game_map.units.ContainsKey(other_id))
                                other_unit = Global.game_map.units[other_id];

                            if (other_unit != null)
                            {
                                // They need to be different units
                                if (unit.id != other_unit.id)
                                {
                                    unit.evented_switch_places(other_unit);

                                    unit.update();
                                    other_unit.update();
                                    // Move ranges will need to be updated
                                    unit_moved = true;
                                }
                            }
                        }
                        else
                        {
                            bool ignore_terrain = command.Value.Length <= 3 ? false : process_bool(command.Value[3]);
                            bool ignore_units = command.Value.Length <= 4 ? false : process_bool(command.Value[4]);
                            target_loc = new Vector2(process_number(command.Value[1]), process_number(command.Value[2]));
                            // Tries to move even if the unit is already at the location, in case they're standing on someone
                            if (!ignore_units || unit.loc != target_loc)
                            {
                                bool moved = unit.evented_move_to(target_loc, ignore_terrain, ignore_units);
                                unit.update();
                                // Move ranges will need to be updated
                                unit_moved = true;
                            }
                        }
                    }
                    break;
            }
            Index++;
            return true;
        }

        private IEnumerable<Tuple<EventedMoveCommands, float>> get_move_route(IEnumerable<string> move_list)
        {
            foreach (var move_code in move_list
                .Select(x => x.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)))
            {
                switch (move_code[0])
                {
                    case "Move":
                        yield return new Tuple<EventedMoveCommands, float>(
                            EventedMoveCommands.Move, process_number(move_code[1]));
                        break;
                    case "Highlight":
                        yield return new Tuple<EventedMoveCommands, float>(
                            EventedMoveCommands.Highlight, process_number(move_code[1]));
                        break;
                    case "Notice":
                        yield return new Tuple<EventedMoveCommands, float>(
                            EventedMoveCommands.Notice, 0);
                        break;
                    case "SetSpeed":
                        yield return new Tuple<EventedMoveCommands, float>(
                            EventedMoveCommands.SetSpeed, process_float(move_code[1]));
                        break;
#if DEBUG
                    default:
                        throw event_case_missing_exception(move_code[0], command.Key);
#endif
                }
            }
        }

        // Wait for Move
        private bool command_wait_for_move()
        {
            //?Value[0] = also wait for chapter transitions (optional)
            if (command.Value.Length >= 1 && process_bool(command.Value[0]) && Global.scene.is_map_scene)
                if ((Global.scene as Scene_Map).is_chapter_change_visible)
                    return false;
            if (waiting_for_move())
                return false;
            Index++;
            return true;
        }

        private bool waiting_for_move()
        {
            if (Global.scene.is_worldmap_scene)
            {
                if (((Scene_Worldmap)Global.scene).scrolling)
                    return true;
            }
            else
            {
                if (Global.game_map.scrolling || Global.player.is_targeting())
                    return true;
                if (Global.game_map.units_dying)
                    return true;
                foreach (Game_Unit unit in Global.game_map.units.Values)
                    if (unit.is_in_motion())
                        return true;

                if (Fow_Unit_Moved)
                {
                    Global.game_map.update_fow();
                    Fow_Unit_Moved = false;
                }
            }
            return false;
        }

        // 13: Chapter Change Effect
        private bool command_chapter_change()
        {
            if (!Global.scene.is_strict_map_scene)
                return false;
            ((Scene_Map)Global.scene).chapter_change();
            Index++;
            return false;
        }

        // Add Battle Convo
        private bool command_battle_convo()
        {
            // Value[0] = id1
            // Value[1] = id2
            // Value[2] = text value
            Global.game_state.add_battle_convo(process_unit_id(command.Value[0]), process_unit_id(command.Value[1]), command.Value[2]);
            Index++;
            return true;
        }

        // Temp Death Quote
        private bool command_death_quote()
        {
            // Value[0] = id is for a unit, or for an actor
            // Value[1] = id
            // Value[2] = text value
            //?Value[3] = casual death quote blocked, default false
            Game_Actor actor;
            get_actor(command.Value[0], command.Value[1], out actor);
            bool casual_blocked = command.Value.Length > 3 && process_bool(command.Value[3]);
            if (actor != null)
            {
                Global.game_state.add_death_quote(actor.id, command.Value[2], casual_blocked);
            }
            Index++;
            return true;
        }

        // 16: Change Gold
        private bool command_change_gold()
        {
            // Value[0] = gold amount
            Global.battalion.gold += process_number(command.Value[0]);
            Index++;
            return true;
        }

        // Set Screen Color
        private bool command_screen_color()
        {
            if (Global.scene.is_strict_map_scene)
            {
                if (command.Value.Length == 5)
                    ((Scene_Map)Global.scene).set_screen_color(new Color(process_number(command.Value[0]), process_number(command.Value[1]),
                        process_number(command.Value[2]), process_number(command.Value[3])), process_number(command.Value[4]));
                else
                    ((Scene_Map)Global.scene).set_screen_color(new Color(process_number(command.Value[0]), process_number(command.Value[1]),
                        process_number(command.Value[2]), process_number(command.Value[3])));
            }
            Index++;
            return true;
        }

        // Show Popup
        private bool command_popup()
        {
            // Value[0] = Type
            switch (command.Value[0])
            {
                case "Text":
                    // Value[1] = Text
                    // Value[2] = Time
                    // Value[3] = Width
                    if (Global.scene.is_strict_map_scene)
                    {
                        string[] lines = command.Value[1]
                            .Split(new string[] { @"\n" }, StringSplitOptions.None);
                        string text = string.Join("\n", lines);
                        ((Scene_Map)Global.scene).set_popup(text, process_number(command.Value[2]), process_number(command.Value[3]));
                    }
                    break;
                case "Weapon":
                case "Item":
                    // Value[1] = Text
                    // Value[2] = Time
                    if (Global.scene.is_strict_map_scene)
                    {
                        ((Scene_Map)Global.scene).set_item_popup(
                            new Item_Data(command.Value[0] == "Weapon" ? 0 : 1, process_number(command.Value[1]), 0), process_number(command.Value[2]));
                    }
                    break;
                case "WeaponLoss":
                case "ItemLoss":
                    // Value[1] = Text
                    // Value[2] = Time
                    if (Global.scene.is_strict_map_scene)
                    {
                        ((Scene_Map)Global.scene).set_item_loss_popup(
                            new Item_Data(command.Value[0] == "WeaponLoss" ? 0 : 1, process_number(command.Value[1]), 0), process_number(command.Value[2]));
                    }
                    break;
                case "Support":
                    if (Global.scene.is_strict_map_scene)
                    {
                        ((Scene_Map)Global.scene).set_popup("Support level increased!", 113);
                    }
                    break;
#if DEBUG
                default:
                    throw event_case_missing_exception(command.Value[0], command.Key);
#endif

            }
            Index++;
            return false;
        }

        // Show Gold Gain Popup
        private bool command_gold_gain_popup()
        {
            // Value[0] = Gold amount
            if (Global.scene.is_strict_map_scene)
            {
                ((Scene_Map)Global.scene).set_gold_gain_popup(process_number(command.Value[0]));
            }
            Index++;
            return false;
        }

        // Unit Battle Theme
        private bool command_unit_theme()
        {
            // Value[0] = id
            // Value[1] = bgm name
            int id = process_unit_id(command.Value[0]);
            if (id == -1)
                if (Global.game_map.last_added_unit != null)
                    id = Global.game_map.last_added_unit.id;
            if (id != -1 && id != 0)
                Global.game_state.add_battle_theme(id, command.Value[1]);
            Index++;
            return true;
        }

        // 21: Add Unit
        private bool command_add_unit()
        {
            // Value[0] = unit type: temporary for events, reinforcements from unit data file
            // Value[1] = Team id
            bool unit_added = true;
            int team = process_number(command.Value[1]);
            Vector2 loc;
            string identifier;
            switch (command.Value[0])
            {
                case "saved deployment":
                    if (Global.game_map.deployment_points.Any(x => Global.game_map.get_unit(x) == null))
                        loc = Global.game_map.deployment_points.First(x => Global.game_map.get_unit(x) == null);
                    else
                    {
                        loc = Vector2.Zero;
                        unit_added = false;
                    }
                    break;
                default:
                    loc = new Vector2(process_number(command.Value[2]), process_number(command.Value[3]));
                    break;
            }
            if (unit_added)
                switch (command.Value[0])
                {
                    //   Value[2] = x
                    //   Value[3] = y
                    //   Value[4] = actor id
                    //  ?Value[5] = identifier
                    case "actor":
                        identifier = command.Value.Length < 6 ? "" : command.Value[5];
                        unit_added = Global.game_map.add_actor_unit(team, loc, process_number(command.Value[4]), identifier);
                        break;
                    //   Value[2] = x
                    //   Value[3] = y
                    //   Value[4] = battalion member index
                    //  ?Value[5] = identifier
                    case "battalion":
                        identifier = command.Value.Length < 6 ? "" : command.Value[5];
                        unit_added = Global.game_map.add_actor_unit(team, loc,
                                Global.battalion.actors[process_number(command.Value[4])], identifier);
                        break;
                    case "undeployed battalion":
                        identifier = command.Value.Length < 6 ? "" : command.Value[5];
                        int battalion_index = process_number(command.Value[4]);
                        unit_added = Global.game_map.add_undeployed_battalion_unit(team, loc, battalion_index, identifier);
                        break;
                    //  ?Value[2] = identifier
                    case "saved deployment":
                        identifier = command.Value.Length <= 2 ? "" : command.Value[2];
                        unit_added = Global.battalion.deployed_but_not_on_map.Count > 0 &&
                            Global.game_map.add_actor_unit(team, loc, Global.battalion.deployed_but_not_on_map[0], identifier);
                        break;
                    //   Value[2] = x
                    //   Value[3] = y
                    //   Value[4] = class_id
                    //   Value[5] = gender
                    //  ?Value[6] = identifier
                    case "temp":
                        identifier = command.Value.Length < 7 ? "" : command.Value[6];
                        unit_added = Global.game_map.add_temp_unit(team, loc, process_number(command.Value[4]),
                            process_number(command.Value[5]), identifier);
                        break;
                    //   Value[2] = x
                    //   Value[3] = y
                    //   Value[4] = index
                    //  ?Value[5] = identifier
                    case "reinforcement":
                        identifier = command.Value.Length < 6 ? "" : command.Value[5];
                        int reinforcement_index = Global.game_map.find_reinforcement_index(command.Value[4]);
                        if (reinforcement_index == -1)
                            reinforcement_index = process_number(command.Value[4]);
                        unit_added = Global.game_map.add_reinforcement_unit(team, loc, reinforcement_index, identifier);
                        break;
                    default:
                        unit_added = false;
#if DEBUG
                        throw event_case_missing_exception(command.Value[0], command.Key);
#endif
                        break;
                }
            if (unit_added)
            {
                Global.game_map.last_added_unit.fix_unit_location(true);
                // Move ranges will need to be updated
                unit_moved = true;
            }
            Index++;
            return true;
        }

        // Change Mission
        private bool command_change_mission()
        {
            // Value[0] = id
            // Value[1] = mission id
            int id = process_unit_id(command.Value[0]);
            Game_Unit unit = null;
            if (id == -1)
                if (Global.game_map.last_added_unit != null)
                    unit = Global.game_map.last_added_unit;
            if (Global.game_map.units.ContainsKey(id))
                unit = Global.game_map.units[id];
            if (unit != null)
            {
                if (Game_AI.IMMOBILE_MISSIONS.Contains(unit.ai_mission))
                    Unit_Moved = true;
                unit.full_ai_mission = process_number(command.Value[1]);
                if (Game_AI.IMMOBILE_MISSIONS.Contains(unit.ai_mission))
                    Unit_Moved = true;
            }
            Index++;
            return true;
        }

        // Change Team
        private bool command_change_team()
        {
            // Value[0] = id
            // Value[1] = new team
            int id = process_unit_id(command.Value[0]);
            Game_Unit unit = null;
            if (id == -1)
                if (Global.game_map.last_added_unit != null)
                    //unit = Global.game_map.last_added_unit; //Debug
                    id = Global.game_map.last_added_unit.id;
            //if (Global.game_map.units.ContainsKey(id)) //Debug
            //    unit = Global.game_map.units[id];
            //if (unit != null)
            //{
            //    if (unit.team != process_number(command.Value[1]))
            //    {
                    Global.game_map.change_unit_team(id, process_number(command.Value[1]));
                    // Move ranges will need to be updated
                    unit_moved = true;
            //    }
            //}
            Index++;
            return true;
        }

        // Change Group
        private bool command_change_group()
        {
            // Value[0] = id
            // Value[1] = new group
            int id = process_unit_id(command.Value[0]);
            Game_Unit unit = null;
            if (id == -1)
                if (Global.game_map.last_added_unit != null)
                    unit = Global.game_map.last_added_unit;
            if (Global.game_map.units.ContainsKey(id))
                unit = Global.game_map.units[id];
            if (unit != null)
            {
                unit.group = process_number(command.Value[1]);
            }
            Index++;
            return true;
        }

        // Change Team Name
        private bool command_change_team_name()
        {
            // Value[0] = team id
            // Value[1] = group
            // Value[2] = name
            Global.game_map.set_group_name(process_number(command.Value[0]), process_number(command.Value[1]), command.Value[2]);
            Index++;
            return true;
        }

        // Set Boss
        private bool command_set_boss()
        {
            // Value[0] = id
            // Value[1] = value
            int id = process_unit_id(command.Value[0]);
            Game_Unit unit = null;
            if (id == -1)
                if (Global.game_map.last_added_unit != null)
                    unit = Global.game_map.last_added_unit;
            if (Global.game_map.units.ContainsKey(id))
                unit = Global.game_map.units[id];
            if (unit != null)
            {
                if (command.Value.Length < 2)
                    unit.boss = true;
                else
                    unit.boss = process_bool(command.Value[1]);
            }
            Index++;
            return true;
        }

        // Set Drops
        private bool command_set_drops()
        {
            // Value[0] = id
            // Value[1] = value
            int id = process_unit_id(command.Value[0]);
            Game_Unit unit = null;
            if (id == -1)
                if (Global.game_map.last_added_unit != null)
                    unit = Global.game_map.last_added_unit;
            if (Global.game_map.units.ContainsKey(id))
                unit = Global.game_map.units[id];
            if (unit != null)
            {
                if (command.Value.Length < 2)
                    unit.drops_item = true;
                else
                    unit.drops_item = process_bool(command.Value[1]);
            }
            Index++;
            return true;
        }

        // Add Talk Event
        private bool command_add_talk()
        {
            int unit_id1, unit_id2, actor_id1;

            switch (command.Value[0])
            {
                case "Units":
                    // Value[0] = "Units"
                    // Value[1] = unit id1
                    // Value[2] = unit id2
                    // Value[3] = event name
                    //?Value[4] = both ways? (optional)
#if DEBUG
                    try
                    {
#endif
                        unit_id1 = process_unit_id(command.Value[1]);
                        unit_id2 = process_unit_id(command.Value[2]);

                        Global.game_state.add_unit_talk_event(
                            unit_id1, unit_id2,
                            command.Value[3],
                            command.Value.Length <= 4 || process_bool(command.Value[4]));
#if DEBUG
                    }
                    catch (KeyNotFoundException ex)
                    {
                        Print.message(string.Format(
                            "Failed to add talk event \"{0}\":\n\n\"{1}\"",
                            command.Value[3], ex.Message));
                    }
#endif
                    break;
                case "Actor-Unit":
                    // Value[0] = "Actor-Unit"
                    // Value[1] = actor id1
                    // Value[2] = unit id2
                    // Value[3] = event name
                    //?Value[4] = both ways? (optional)
#if DEBUG
                    try
                    {
#endif
                        actor_id1 = process_number(command.Value[1]);
                        unit_id2 = process_unit_id(command.Value[2]);

                        Global.game_state.add_actor_unit_talk_event(
                            actor_id1, unit_id2,
                            command.Value[3],
                            command.Value.Length <= 4 || process_bool(command.Value[4]));
#if DEBUG
                    }
                    catch (KeyNotFoundException ex)
                    {
                        Print.message(string.Format(
                            "Failed to add talk event \"{0}\":\n\n\"{1}\"",
                            command.Value[3], ex.Message));
                    }
#endif
                    break;
                case "Actors":
                    // Value[0] = "Actors"
                    // Value[1] = actor id1
                    // Value[2] = actor id2
                    // Value[3] = event name
                    //?Value[4] = both ways? (optional)
                    Global.game_state.add_talk_event(
                        process_number(command.Value[1]),
                        process_number(command.Value[2]),
                        command.Value[3],
                        command.Value.Length <= 4 || process_bool(command.Value[4]));
                    break;
                default:
                    // Value[0] = actor id1
                    // Value[1] = actor id2
                    // Value[2] = event name
                    //?Value[3] = both ways? (optional)
                    Global.game_state.add_talk_event(
                        process_number(command.Value[0]),
                        process_number(command.Value[1]),
                        command.Value[2],
                        command.Value.Length <= 3 || process_bool(command.Value[3]));
                    break;
            }
            Unit_Moved = true;
            Index++;
            return true;
        }

        // Add Deployment Point
        private bool command_add_deployment()
        {
            // Value[0] = x
            // Value[1] = y
            Global.game_map.add_deployment(new Vector2(process_number(command.Value[0]), process_number(command.Value[1])));
            Index++;
            return true;
        }

        // 30: Add to Battalion
        private bool command_add_to_battalion()
        {
            switch (command.Value[0])
            {
                case "Add All Deployed":
                    // Value[0] = "Add All Deployed"
                    foreach (int unit_id in Global.game_map.teams[Constants.Team.PLAYER_TEAM])
                        Global.battalion.add_actor(Global.game_map.units[unit_id].actor.id);
                    break;
                case "Add Multiple":
                    for (int i = 1; i < command.Value.Length; i++)
                        Global.battalion.add_actor(process_number(command.Value[i]));
                    break;
                default:
                    // Value[0] = actor id
                    //?Value[1] = add instead of remove
                    // Maybe this should check if the unit is temporary, and not add them if so? //Yeti
                    // Also maybe check if they're out of lives, and the event code would need to explicitly heal them first if so //Yeti
                    if (command.Value.Length >= 2 && !process_bool(command.Value[1]))
                        Global.battalion.remove_actor(process_number(command.Value[0]));
                    else
                        Global.battalion.add_actor(process_number(command.Value[0]));
                    break;
            }
            Index++;
            return true;
        }

        // Target Tile
        private bool command_target_tile()
        {
            switch (command.Value[0])
            {
                case "Unit":
                    // Value[0] = "Unit"
                    // Value[1] = unit id
                    // Value[2] = time (optional)
                    int id = process_unit_id(command.Value[1]);
                    if (!Global.game_map.units.ContainsKey(id))
                    {
                        throw new ArgumentException("No unit with this id exists");
                        return true;
                    }
                    Game_Unit unit = Global.game_map.units[id];
                    if (command.Value.Length < 3)
                        Global.player.target_tile(unit.loc);
                    else
                        Global.player.target_tile(unit.loc, process_number(command.Value[2]));
                    Index++;
                    break;
                default:
                    // Value[0] = x
                    // Value[1] = y
                    // Value[2] = time (optional)
                    if (command.Value.Length < 3)
                        Global.player.target_tile(new Vector2(process_number(command.Value[0]), process_number(command.Value[1])));
                    else
                        Global.player.target_tile(new Vector2(process_number(command.Value[0]), process_number(command.Value[1])), process_number(command.Value[2]));
                    Index++;
                    break;
            }
            return true;
        }

        // Force Deployment
        private bool command_force_deployment()
        {
            for (int i = 0; i < command.Value.Length; i++)
            {
                // Value[0] = actor id
                int id = process_number(command.Value[i]);
                if (id == -1)
                    if (Global.game_map.last_added_unit != null)
                        id = Global.game_map.last_added_unit.actor.id;
                if (id != -1)
                {
                    if (Global.battalion.actors.Contains(id))
                        Global.game_map.add_forced_deployment(id);
                }
            }
            Index++;
            return true;
        }

        // Set Convoy
        private bool command_set_convoy()
        {
            // Value[0] = actor id
            Global.battalion.convoy_id = process_number(command.Value[0]);
            Index++;
            return true;
        }

        // 34: Convoy Item Gain
        private bool command_convoy_item_gain()
        {
            // Value[0-2] = item data
            Item_Data item;
            if (process_number(command.Value[2]) == 0)
                item = new Item_Data(process_number(command.Value[0]), process_number(command.Value[1]));
            else
                item = new Item_Data(process_number(command.Value[0]), process_number(command.Value[1]), process_number(command.Value[2]));

            Global.game_battalions.add_item_to_convoy(item, true);
            Index++;
            return true;
        }

        // 35: Remove Unit
        private bool command_remove_unit()
        {
            if (command.Value[0] == "Remove All")
            {
                // Value[0] = "Remove All"
                while (Global.game_map.units.Any())
                    Global.game_map.remove_unit(Global.game_map.units.First().Key);
            }
            else if (command.Value[0] == "Remove Team")
            {
                // Value[0] = "Remove Team"
                // Value[1] = team id
                //?Value[2] = group id
                int team = process_unit_id(command.Value[1]);
                int group = -1;
                if (command.Value.Length > 2)
                    group = process_unit_id(command.Value[2]);

                List<int> units = Global.game_map.units
                    .Where(x => x.Value.team == team && (group == -1 || x.Value.group == group))
                    .Select(x => x.Key)
                    .ToList();
                foreach(int id in units)
                {
                    Global.game_map.remove_unit(id);
                    // Move ranges will need to be updated
                    unit_moved = true;
                }
            }
            else
            {
                // Value[0] = unit id
                //?Value[1] = death animation? (optional)
                int id = process_unit_id(command.Value[0]);
                if (Global.game_map.units.ContainsKey(id))
                {
                    // If the death animation should play, set that up
                    if (command.Value.Length > 1 && process_bool(command.Value[1]))
                        Global.game_map.add_dying_unit_animation(id, true);
                    // Otherwise just instantly remove the unit from the map
                    else
                    {
                        Global.game_map.remove_unit(id);
                    }
                    // Move ranges will need to be updated
                    unit_moved = true;
                }
            }
            Index++;
            return true;
        }

        // 36: Remove Talk Event
        private bool command_remove_talk()
        {
            switch (command.Value[0])
            {
                case "Units":
                    // Value[0] = "Units"
                    // Value[1] = id1
                    // Value[2] = id2
                    Global.game_state.remove_unit_talk_event(process_unit_id(command.Value[1]), process_unit_id(command.Value[2]));
                    break;
                case "Actor-Unit":
                    // Value[0] = "Actor-Unit"
                    // Value[1] = actor id1
                    // Value[2] = unit id2
                    Global.game_state.remove_actor_unit_talk_event(process_number(command.Value[1]), process_unit_id(command.Value[2]));
                    break;
                case "Actors":
                    // Value[0] = "Actors"
                    // Value[1] = id1
                    // Value[2] = id2
                    Global.game_state.remove_talk_event(process_number(command.Value[1]), process_number(command.Value[2]));
                    break;
                default:
                    // Value[0] = id1
                    // Value[1] = id2
                    Global.game_state.remove_talk_event(process_number(command.Value[0]), process_number(command.Value[1]));
                    break;
            }

            Index++;
            return true;
        }

        // Set FoW
        private bool command_set_fow()
        {
            switch (command.Value[0])
            {
                case "Refresh":
                    Global.game_map.update_fow();
                    break;
                default:
                    // Value[0] = fow value
                    // Value[1] = R
                    // Value[2] = G
                    // Value[3] = B
                    // Value[4] = A
                    Global.game_map.fow = process_bool(command.Value[0]);
                    if (command.Value.Length > 1)
                        Global.game_map.fow_color = new Tone(process_number(command.Value[1]), process_number(command.Value[2]),
                            process_number(command.Value[3]), process_number(command.Value[4]));
                    break;
            }
            Index++;
            return true;
        }

        // Set FoW Vision
        private bool command_fow_vision()
        {
            // Value[0] = vision range
            Global.game_map.vision_range = process_number(command.Value[0]);
            Index++;
            return true;
        }

        // Set Weather
        private bool command_set_weather()
        {
            // Value[0] = id1
            // Value[1] = id2
            // Value[2] = event name
            Global.game_state.weather = process_number(command.Value[0]);
            Index++;
            return true;
        }

        // FoW Light Source
        private bool command_fow_light_source()
        {
            // Value[0] = Type
            // Value[1] = x, y
            Vector2 loc = process_vector2(command.Value[1]);
            switch (command.Value[0])
            {
                case "Add":
                    // Value[2] = Radius
                    var lightSource = new Fow_View_Object(
                        loc, process_number(command.Value[2]));
                    Global.game_map.add_vision_point(lightSource);
                    break;
#if DEBUG
                default:
                    throw event_case_missing_exception(command.Value[0], command.Key);
#endif
            }
            Index++;
            return true;
        }

        // Change Tile Id
        private bool command_change_tile()
        {
            // Value[0] = x
            // Value[1] = y
            // Value[2] = tile id x
            // Value[3] = tile id y
            Global.game_map.change_tile(new Vector2(process_number(command.Value[0]), process_number(command.Value[1])),
                process_number(command.Value[2]) + (process_number(command.Value[3]) % 32) * 32 + (process_number(command.Value[3]) / 32) * 8);
            // Move ranges will need to be updated
            unit_moved = true;
            Index++;
            return true;
        }

        // Pillage Tile
        private bool command_pillage_tile()
        {
            // Value[0] = x
            // Value[1] = y
            // Value[2] = width
            // Value[3] = height
            int width = 1, height = 1;
            if (command.Value.Length > 2)
                width = process_number(command.Value[2]);
            if (command.Value.Length > 3)
                height = process_number(command.Value[3]);
            int x = process_number(command.Value[0]);
            int y = process_number(command.Value[1]);

            Global.game_map.pillage(x, y, width, height);
            // Move ranges will need to be updated
            unit_moved = true;
            Index++;
            return true;
        }

        // Add Area Background
        private bool command_area_background()
        {
            // Value[0] = background
            // Value[1] = x1
            // Value[2] = y1
            // Value[3] = x2 (optional)
            // Value[4] = y2 (optional)
            if (command.Value.Length > 3)
                Global.game_map.add_area_back(command.Value[0], process_number(command.Value[1]), process_number(command.Value[2]),
                    process_number(command.Value[3]), process_number(command.Value[4]));
            else
                Global.game_map.add_area_back(command.Value[0], process_number(command.Value[1]), process_number(command.Value[2]),
                    process_number(command.Value[1]), process_number(command.Value[2]));
            Index++;
            return true;
        }

        // Add Destroyable Object
        private bool command_add_destroyable()
        {
            // Value[0] = x
            // Value[1] = y
            // Value[2] = hp
            // Value[3] = Event name
            Global.game_map.add_destroyable_object(new Vector2(process_number(command.Value[0]), process_number(command.Value[1])),
                process_number(command.Value[2]), command.Value[3]);
            Index++;
            return true;
        }

        // Import Map Area
        private bool command_import_map_area()
        {
            // Value[0] = Map Name
            // Value[1] = x
            // Value[2] = y
            // Value[3] = source x
            // Value[4] = source y
            // Value[5] = width
            // Value[6] = height
            // Value[7] = roof (optional)
            bool roof = command.Value.Length >= 8 && process_bool(command.Value[7]);
            Global.game_map.import_map_area(command.Value[0], new Vector2(process_number(command.Value[1]), process_number(command.Value[2])),
                new Rectangle(process_number(command.Value[3]), process_number(command.Value[4]),
                    process_number(command.Value[5]), process_number(command.Value[6])), roof);
            // Move ranges will need to be updated
            unit_moved = true;
            Index++;
            return true;
        }

        // Add Siege Engine
        private bool command_add_siege()
        {
            // Value[0] = x
            // Value[1] = y
            // Value[2] = item type
            // Value[3] = item id
            // Value[4] = item uses
            Item_Data item;
            if (process_number(command.Value[4]) == 0)
                item = new Item_Data(process_number(command.Value[2]), process_number(command.Value[3]));
            else
                item = new Item_Data(process_number(command.Value[2]), process_number(command.Value[3]), process_number(command.Value[4]));

            Global.game_map.add_siege_engine(new Vector2(process_number(command.Value[0]), process_number(command.Value[1])), item);
            Index++;
            return true;
        }

        // 47: Edit Tile Outlines
        private bool command_edit_tile_outlines()
        {
            // Value[0] = Type
            switch (command.Value[0])
            {
                case "New Outline":
                    // Value[1] = Type
                    // Value[2] = R
                    // Value[3] = G
                    // Value[4] = B
                    // Value[5] = A
                    Color tint = new Color(
                        process_number(command.Value[2]),
                        process_number(command.Value[3]),
                        process_number(command.Value[4]),
                        process_number(command.Value[5]));
                    Global.game_map.add_tile_outline_set(
                        (byte)process_number(command.Value[1]), tint);
                    break;
                case "Add Area":
                    // Value[1] = Index
                    // Value[2] = x1
                    // Value[3] = y1
                    // Value[4] = x2
                    // Value[5] = y2
                    Global.game_map.add_tile_outline_area(
                        process_number(command.Value[1]),
                        rect_from_edges(
                            process_number(command.Value[2]),
                            process_number(command.Value[3]),
                            process_number(command.Value[4]),
                            process_number(command.Value[5])));
                    break;
                case "Remove Area":
                    // Value[1] = Index
                    // Value[2] = x1
                    // Value[3] = y1
                    // Value[4] = x2
                    // Value[5] = y2
                    Global.game_map.remove_tile_outline_area(
                        process_number(command.Value[1]),
                        rect_from_edges(
                            process_number(command.Value[2]),
                            process_number(command.Value[3]),
                            process_number(command.Value[4]),
                            process_number(command.Value[5])));
                    break;
#if DEBUG
                default:
                    throw event_case_missing_exception(command.Value[0], command.Key);
#endif
            }
            Index++;
            return true;
        }

        // 48: AI Target Map Object
        private bool command_ai_target_map_object()
        {
            // Value[0] = Type
            // Value[1] = x, y
            // Value[2] = team
            Vector2 loc = process_vector2(command.Value[1]);
            int team = process_number(command.Value[2]);
            switch (command.Value[0])
            {
                case "Destroyable":
                case "Destructible":
                    Global.game_map.destroyable_add_enemy_team(loc, team);
                    break;
#if DEBUG
                default:
                    throw event_case_missing_exception(command.Value[0], command.Key);
#endif
            }
            Index++;
            return true;
        }

        // 51: Class Change
        private bool command_class_change()
        {
            Game_Actor actor;
            switch (command.Value[0])
            {
                case "Promotion":
                    // Value[0] = "Promotion"
                    // Value[1] = id is for a unit, or for an actor
                    // Value[2] = id
                    // Value[3] = index in promotion array (optional)
                    get_actor(command.Value[1], command.Value[2], out actor);
                    if (actor != null)
                    {
                        int promotionIndex = command.Value.Length >= 4 ?
                            process_number(command.Value[3]) : 0;
                        if (promotionIndex < actor.actor_class.Promotion.Count)
                        {
                            int promotionId = actor.actor_class.Promotion
                                .Keys
                                .OrderBy(x => x)
                                .ElementAt(promotionIndex);

                            actor.quick_promotion(promotionId);
                            foreach (Game_Unit unit in Global.game_map.units.Values)
                                if (unit.actor == actor)
                                    unit.refresh_sprite();
                            // Move ranges will need to be updated
                            unit_moved = true;
                        }
                    }
                    break;
                default:
                    // Value[0] = id is for a unit, or for an actor
                    // Value[1] = id
                    // Value[2] = new class id
                    // Value[3] = promote or class change
                    get_actor(command.Value[0], command.Value[1], out actor);
                    if (actor != null)
                    {
                        if (process_bool(command.Value[3]))
                            actor.quick_promotion(process_number(command.Value[2]));
                        else
                        {
                            throw new NotImplementedException();
                            actor.class_id = process_number(command.Value[2]);
                        }
                        foreach (Game_Unit unit in Global.game_map.units.Values)
                            if (unit.actor == actor)
                                unit.refresh_sprite();
                        // Move ranges will need to be updated
                        unit_moved = true;
                    }
                    break;
            }
            Index++;
            return true;
        }

        // 52: Exp
        private bool command_exp()
        {
            Game_Actor actor;
            int exp;
            bool show_exp_gauge = false;
            switch (command.Value[0])
            {
                case "Set Level":
                    // Value[0] = "Set Level"
                    // Value[1] = id is for a unit, or for an actor
                    // Value[2] = id
                    // Value[3] = exp value
                    get_actor(command.Value[1], command.Value[2], out actor);
                    if (actor != null)
                    {
                        int level = process_number(command.Value[3]);
                        exp = (level - actor.level) * Global.ActorConfig.ExpToLvl - actor.exp;
                        exp = Math.Max(exp, 0);
                    }
                    else
                        exp = 0;
                    break;
                default:
                    // Value[0] = id is for a unit, or for an actor
                    // Value[1] = id
                    // Value[2] = exp value
                    //?Value[3] = cap at one less than a level up?
                    //?Value[4] = should the exp gauge appear?
                    get_actor(command.Value[0], command.Value[1], out actor);
                    exp = process_number(command.Value[2]);
                    if (command.Value.Length >= 4 && process_bool(command.Value[3]))
                        exp = Math.Min(exp, (Global.ActorConfig.ExpToLvl - 1) - actor.exp);
                    if (actor != null)
                    {
                        show_exp_gauge = command.Value.Length >= 5 &&
                            process_bool(command.Value[4]) && process_bool(command.Value[0]);
                    }
                    break;
            }

            if (actor != null)
            {
                if (exp != 0)
                {
                    // If exp gauge should be visible, and working with a unit not an actor
                    if (show_exp_gauge)
                    {
                        Global.game_state.gain_exp(Global.game_map.get_unit_id_from_actor(actor.id), exp);
                    }
                    else
                    {
                        actor.instant_level = true;
                        actor.exp += exp;
                        // Turn off again, in case no level was gained
                        actor.instant_level = false;
                    }
                }
            }
            Index++;
            return !Global.game_state.exp_active;
        }

        // 53: Item Gain
        private bool command_item_gain()
        {
            // Value[0] = id is for a unit, or for an actor
            // Value[1] = id
            // Value[2-4] = item data
            // Value[5] = force allowing convoy (optional)
            Game_Actor actor;
            Game_Unit unit;
            get_actor(command.Value[0], command.Value[1], out actor, out unit);

            if (actor != null)
            {
                Item_Data item;
                if (process_number(command.Value[4]) == 0)
                    item = new Item_Data(process_number(command.Value[2]),
                        process_number(command.Value[3]));
                else
                    item = new Item_Data(
                        process_number(command.Value[2]),
                        process_number(command.Value[3]),
                        process_number(command.Value[4]));

                actor.gain_item(item);
                actor.staff_fix();
                if (actor.too_many_items)
                {
                    bool force_convoy_allowed = command.Value.Length >= 6 &&
                        process_bool(command.Value[5]);

                    if (unit == null)
                    {
#if DEBUG
                        int actor_unit_id = Global.game_actors.get_unit_from_actor(actor.id, true);
#else
                        int actor_unit_id = Global.game_actors.get_unit_from_actor(actor.id);
#endif
                        if (actor_unit_id != -1)
                            unit = Global.game_map.units[actor_unit_id];
                        else
                        {
                            // Probably send the item to the convoy??? and if the convoy doesn't exist then ahhhhh
                            // I guess check if the map has a unit with the actor id, if not make a temporary unit off the map
#if DEBUG
                            Print.message(string.Format(
                                "Actor {0} does not have a unit in play to gain\n" +
                                "an item, adding a temporary unit off the map", actor.id));
#endif
                            Global.game_map.add_actor_unit(1, Config.OFF_MAP, actor.id, "");
                            unit = Global.game_map.last_added_unit;
                        }
                    }
                    Global.game_temp.menu_call = true;
                    Global.game_temp.discard_menu_call = true;
                    Global.game_temp.force_send_to_convoy = force_convoy_allowed;
                    Global.game_system.Discarder_Id = unit.id;
                    Index++;
                    return false;
                }
                if (unit != null && !unit.is_player_team)
                {
                    bool playerAttackableTeam = unit.is_attackable_team(
                        Constants.Team.PLAYER_TEAM);
                    // If a chest was opened by a unit the player can attack
                    if (playerAttackableTeam)
                        if (Global.game_state.visit_active &&
                            Global.game_state.visit_mode == State.Visit_Modes.Chest)
                        {
                            // They can drop what they get from it
                            unit.drops_item = true;
                        }
                    while (actor.too_many_items)
                    {
#if DEBUG
                        Print.message(
                            "A non-player unit just gained an item,\n" +
                            "they need to drop the last item and also\n" +
                            "automatically throw something away if\n" +
                            "their inventory is full\n" +
                            "(keep at least one weapon and then\n" +
                            "throw out least valuable)");
#endif
                        //@Debug: actually select the best item to discard
                        actor.discard_item(actor.num_items - 1);
                    }
                }
            }
            Index++;
            return true;
        }

        // Discard Item
        private bool command_item_discard()
        {
            // Value[0] = id is for a unit, or for an actor
            // Value[1] = id
            // Value[2] = item index
            Game_Actor actor;
            get_actor(command.Value[0], command.Value[1], out actor);
            int item_index = process_number(command.Value[2]);
            if (actor != null)
            {
                if (item_index == -1)
                {
                    // Discard all items
                    while (actor.num_items > 0)
                        actor.discard_item(0);
                }
                else
                    actor.discard_item(item_index);
            }
            Index++;
            return true;
        }

        // WExp Gain
        private bool command_wexp()
        {
            // Value[0] = id is for a unit, or for an actor
            // Value[1] = id
            // Value[2] = weapon type
            // Value[3] = wexp value
            // Value[4] = set wexp? (optional)
            Game_Actor actor;
            get_actor(command.Value[0], command.Value[1], out actor);
            if (actor != null)
            {
                if (command.Value.Length >= 5 && process_bool(command.Value[4]))
                {
                    int wexp = 0;
                    // If the wexp value is a weapon rank letter, convert to that rank's value
                    if (Data_Weapon.WLVL_LETTERS.Contains(command.Value[3]))
                        wexp = Data_Weapon.WLVL_THRESHOLDS[Data_Weapon.WLVL_LETTERS.ToList().IndexOf(command.Value[3])];
                    else
                        wexp = process_number(command.Value[3]);

                    actor.wexp_set(Global.weapon_types[process_number(command.Value[2])], wexp, false);
                    actor.clear_wlvl_up();
                }
                else
                {
                    actor.wexp_gain(Global.weapon_types[process_number(command.Value[2])], process_number(command.Value[3]));
                    actor.clear_wlvl_up();
                }
            }
            Index++;
            return true;
        }

        // Support
        private bool command_support()
        {
            switch (command.Value[0])
            {
                case "Bond":
                    // Value[0] = "Bond"
                    // Value[1] = actor 1 id
                    // Value[2] = actor 2 id
                    //?Value[3] = reciprocate bond (default: true)
                    if (Global.game_actors.ContainsKey(process_number(command.Value[1])))
                        if (Global.game_actors.ContainsKey(process_number(command.Value[2])))
                        {
                            Global.game_actors[process_number(command.Value[1])].set_bond_partner(process_number(command.Value[2]));
                            if (command.Value.Length <= 3 || process_bool(command.Value[3]))
                                Global.game_actors[process_number(command.Value[2])].set_bond_partner(process_number(command.Value[1]));
                        }
                    break;
                case "Support Points":
                    // Value[0] = "Support Points"
                    // Value[1] = actor 1 id
                    // Value[2] = actor 2 id
                    // Value[3] = points to gain
                    int actor_id1 = process_number(command.Value[1]);
                    int actor_id2 = process_number(command.Value[2]);
                    int support_points = process_number(command.Value[3]);

                    if (Global.game_actors.ContainsKey(actor_id1))
                        if (Global.game_actors.ContainsKey(actor_id2))
                        {
                            Global.game_actors[actor_id1].try_support_gain(actor_id2, support_points);
                            Global.game_actors[actor_id2].try_support_gain(actor_id1, support_points);
                        }
                    break;
                default:
                    // Value[0] = actor 1 id
                    // Value[1] = actor 2 id
                    if (Global.game_actors.ContainsKey(process_number(command.Value[0])))
                        if (Global.game_actors.ContainsKey(process_number(command.Value[1])))
                        {
                            Global.game_actors[process_number(command.Value[0])].increase_support_level(process_number(command.Value[1]));
                            Global.game_actors[process_number(command.Value[1])].increase_support_level(process_number(command.Value[0]));
                        }
                    break;
            }
            Index++;
            return true;
        }

        // Boss Hard Mode Stats
        private bool command_boss_hard_mode()
        {
            // Value[0] = unit id
            int id = process_unit_id(command.Value[0]);
            if (Global.game_map.units.ContainsKey(id))
                Global.game_map.units[id].boss_hard_mode_bonuses();
            Index++;
            return true;
        }

        // Block Support
        private bool command_block_support()
        {
            if (command.Value.Length == 0)
                Global.game_state.block_supports();
            else if (command.Value.Length == 1)
            {
                // Value[0] = actor 1 id
                if (Global.game_actors.ContainsKey(process_number(command.Value[0])))
                    Global.game_state.block_support(process_number(command.Value[0]));
            }
            else
            {
                // Value[0] = actor 1 id
                // Value[1] = actor 2 id
                if (Global.game_actors.ContainsKey(process_number(command.Value[0])))
                    if (Global.game_actors.ContainsKey(process_number(command.Value[1])))
                        Global.game_state.block_support(process_number(command.Value[0]), process_number(command.Value[1]));
            }
            Index++;
            return true;
        }

        // Change Status
        private bool command_change_status()
        {
            //?Value[0] = id is for a unit, or for an actor
            // Value[1] = id
            // Value[2] = status id
            //?Value[3] = adding status, instead of removing?
            Game_Actor actor;
            Game_Unit unit;
            get_actor(command.Value[0], command.Value[1], out actor, out unit);

            if (actor != null)
            {
                if (process_bool(command.Value[3]))
                    actor.add_state(process_number(command.Value[2]));
                else
                    actor.remove_state(process_number(command.Value[2]));
                if (Global.scene.scene_type == "Scene_Map")
                {
                    if (unit != null)
                        ((Scene_Map)Global.scene).update_map_sprite_status(unit.id);
                    else
                        foreach (int id in Global.game_map.get_units_from_actor(actor.id))
                            ((Scene_Map)Global.scene).update_map_sprite_status(id);
                }
            }
            Index++;
            return true;
        }

        // 61: Heal Actors
        private bool command_heal_actors()
        {
            if (command.Value.Length == 0)
                Global.game_actors.heal_actors();
            else
            {
                // Value[0] = Actor id
                //?Value[1] = Set hp to amount instead of healing by amount?
                // Value[2] = Heal amount
                if (Global.game_actors.actor_loaded(process_number(command.Value[0])))
                {
                    Game_Actor actor = Global.game_actors[process_number(command.Value[0])];
                    // If no heal amount, set hp to full
                    if (command.Value.Length <= 2)
                        actor.hp = actor.maxhp;
                    else
                    {
                        if (process_bool(command.Value[1]))
                            actor.hp = process_number(command.Value[2]);
                        else
                            actor.hp += process_number(command.Value[2]);
                    }
                }
            }
            Index++;
            return true;
        }

        // Set Unit Ready
        private bool command_unit_ready()
        {
            // Value[0] = id
            //?Value[1] = ready
            int id = process_unit_id(command.Value[0]);
            Game_Unit unit = null;
            if (id == -1)
                if (Global.game_map.last_added_unit != null)
                    unit = Global.game_map.last_added_unit;
            if (Global.game_map.units.ContainsKey(id))
                unit = Global.game_map.units[id];
            if (unit != null)
            {
                if (process_bool(command.Value[1]) != (unit.ready && !Global.game_map.unit_waiting(unit.id)))
                {
                    if (process_bool(command.Value[1]))
                        unit.refresh_unit();
                    else if (unit.is_active_team)
                        unit.wait(false);
                }
                // Move ranges will need to be updated
                unit_moved = true;
            }
            Index++;
            return true;
        }

        // Set Map Edge Offsets
        private bool command_map_edge()
        {
            // Value[0] = Left Edge
            // Value[1] = Top Edge
            // Value[2] = Right Edge
            // Value[3] = Bottom Edge
            Global.game_map.map_edge_offsets = new Rectangle(
                process_number(command.Value[0]), process_number(command.Value[1]),
                process_number(command.Value[0]) + process_number(command.Value[2]),
                process_number(command.Value[1]) + process_number(command.Value[3]));
            // Move ranges will need to be updated
            unit_moved = true;
            Index++;
            return true;
        }

        // 64: Unload Actor
        private bool command_unload_actor()
        {
            // Value[0] = Actor id
            Global.game_actors.remove_actor(process_number(command.Value[0]));
            Index++;
            return true;
        }

        // 65: Rescue
        private bool command_rescue()
        {
            // Value[0] = rescuer unit id
            // Value[1] = rescuee unit id
            // Value[2] = Type: Map, Instant
            Game_Unit unit1 = null, unit2 = null;

            int id = process_unit_id(command.Value[0]);
            if (id == -1)
                if (Global.game_map.last_added_unit != null)
                    unit1 = Global.game_map.last_added_unit;
            if (Global.game_map.units.ContainsKey(id))
                unit1 = Global.game_map.units[id];

            id = process_unit_id(command.Value[1]);
            if (id == -1)
                if (Global.game_map.last_added_unit != null)
                    unit2 = Global.game_map.last_added_unit;
            if (Global.game_map.units.ContainsKey(id))
                unit2 = Global.game_map.units[id];

            // If both units aren't null, and aren't the same unit, and unit 1 is not already rescuing unit 2, and rescuing is allowed
            if (unit1 != null && unit2 != null && unit1 != unit2 &&
                unit1.rescued != unit2.id && unit1.can_rescue(unit2))
            {
                switch (command.Value[2])
                {
                    case "Map":
                    case "Map Cover":
                        // If either unit is moving, wait for them to finish
                        if (unit1.is_in_motion() || unit2.is_in_motion())
                            return false;
                        if (unit2.is_rescued)
                            throw new ArgumentException("Map rescue event control cannot be used to take.");
                        // The units have to already be adjacent
                        if (Global.game_map.unit_distance(unit1.id, unit2.id) == 1)
                        {
                            if (command.Value[2] == "Map")
                                unit1.rescue_ally(unit2.id);
                            else if (command.Value[2] == "Map Cover")
                                unit1.cover_ally(unit2.id);

                            Rescue_Wait = true;
                        }
                        break;
                    case "Instant":
                        if (unit2.is_rescued)
                        { }
                        else
                        {
                            unit1.rescuing = unit2.id;
                            unit2.rescued = unit1.id;
                            unit2.force_loc(Config.OFF_MAP);
                            unit2.queue_move_range_update(unit1);
                        }
                        break;
                    default:
#if DEBUG
                        throw event_case_missing_exception(command.Value[2], command.Key);
#endif
                        throw new ArgumentException();
                }
                Unit_Moved = true;
            }
            Index++;
            return true;
        }

        // 66: Change Actor Name
        private bool command_change_name()
        {
            // Value[0] = Actor id
            // Value[1] = New name
            Global.game_actors[process_number(command.Value[0])].name = command.Value[1];
            Index++;
            return true;
        }

        // Set Min Alpha
        private bool command_min_alpha()
        {
            // Value[0] = value
            Global.game_map.min_alpha = process_number(command.Value[0]);
            Index++;
            return true;
        }

        // Refresh Alpha
        private bool command_refresh_alpha()
        {
            // Value[0] = value
            Global.game_map.refresh_alpha(process_number(command.Value[0]));
            Index++;
            return true;
        }

        // Add Alpha Source
        private bool command_add_alpha()
        {
            // Value[0] = x
            // Value[1] = y
            // Value[2] = multiplier
            // Value[3] = divisor
            int value = process_number(command.Value[2]) *
                Constants.Map.ALPHA_MAX / process_number(command.Value[3]) - 1;
            if (value > 0)
            {
                Global.game_map.add_alpha_source(
                    new Vector2(process_number(command.Value[0]), process_number(command.Value[1])), value);
            }
            Index++;
            return true;
        }

        // Clear Alpha Sources
        private bool command_clear_alpha()
        {
            Global.game_map.clear_alpha();
            Index++;
            return true;
        }

        // Set Ally Alpha
        private bool command_ally_alpha()
        {
            // Value[0] = multiplier
            // Value[1] = divisor
            Global.game_map.ally_alpha = process_number(command.Value[0]) *
                Constants.Map.ALPHA_MAX / process_number(command.Value[1]) - 1;
            Index++;
            return true;
        }

        // Blacken Screen
        private bool command_blacken_screen()
        {
            // Value[0] = duration
            ((Scene_Map)Global.scene).black_screen(process_number(command.Value[0]));
            Index++;
            return true;
        }

        // Add Visit Location
        private bool command_add_visit()
        {
            // Value[0] = x
            // Value[1] = y
            // Value[2] = type
            // Value[3] = visit event
            switch (command.Value[2])
            {
                // Value[4] = can pillage?
                // Value[5] = pillage event
                case "Visit":
                    Global.game_map.add_visit(new Vector2(process_number(command.Value[0]), process_number(command.Value[1])),
                        command.Value[3], process_bool(command.Value[4]) ? command.Value[5] : "");
                    break;
                case "Chest":
                    Global.game_map.add_chest(new Vector2(process_number(command.Value[0]), process_number(command.Value[1])),
                        command.Value[3]);
                    break;
                case "Door":
                    Global.game_map.add_door(new Vector2(process_number(command.Value[0]), process_number(command.Value[1])),
                        command.Value[3]);
                    break;
                // Value[4] = visit name
                case "NamedVisit":
                    Global.game_map.add_visit(new Vector2(process_number(command.Value[0]), process_number(command.Value[1])),
                        command.Value[3], visit_name: command.Value[4]);
                    break;
#if DEBUG
                default:
                    throw event_case_missing_exception(command.Value[2], command.Key);
#endif
            }
            Index++;
            return true;
        }

        // Remove Visit Location
        private bool command_remove_visit()
        {
            if (command.Value.Length == 0)
            {
                // Close visit at the active visit location
                Global.game_map.remove_visit(Global.game_state.visit_loc);
            }
            else
            {
                // Value[0] = x
                // Value[1] = y
                Vector2 loc = new Vector2(
                    process_number(command.Value[0]),
                    process_number(command.Value[1]));
                // Visit
                Global.game_map.remove_visit(loc);
                // Destroyable
                var destroyable = Global.game_map.get_destroyable(loc);
                if (destroyable != null)
                    Global.game_map.remove_destroyable(destroyable.id);
            }
            Index++;
            return true;
        }

        // 83: Add Shop Location
        private bool command_add_shop()
        {
            bool base_shop = false;
            Shop_Data shop;
            Vector2 loc = Vector2.Zero;
            if (command.Value.Length <= 4)
            {
                // Value[0] = Store type (Base)
                // Value[1] = choice offsets
                // Value[2] = face (optional)
                // Value[3] = music (optional)
                switch (command.Value[0])
                {
                    case "Base":
                        if (Global.battalion.convoy_id == -1)
                        {
                            Index++;
                            return true;
                        }
                        base_shop = true;
                        int[] offsets = command.Value[1].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => process_number(x)).ToArray();

                        string face = command.Value.Length <= 2 ? "" : command.Value[2];
                        string music = command.Value.Length <= 3 ? "" : command.Value[3];
                        shop = new Shop_Data(face, music, offsets, false, false);
                        break;
                    default:
#if DEBUG
                        throw event_case_missing_exception(command.Value[0], command.Key);
#endif
                        throw new NotImplementedException();
                        Index++;
                        return true;
                }
            }
            else
            {
                // Value[0] = x
                // Value[1] = y
                // Value[2] = face
                // Value[3] = music
                // Value[4] = choice offsets
                // Value[5] = is secret?
                loc = new Vector2(process_number(command.Value[0]), process_number(command.Value[1]));
                int[] offsets = command.Value[4].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => process_number(x)).ToArray();
                /*int[] offsets = new int[4]; //Debug
                string[] offsets_str = command.Value[4].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < offsets.Length; i++)
                    offsets[i] = process_number(offsets_str[i]);*/
                shop = new Shop_Data(command.Value[2], command.Value[3], offsets, process_bool(command.Value[5]), false);
            }
            command_shop_text(shop);
            command_shop_inventory(shop);
            Global.game_map.add_shop(loc, shop, base_shop);
            Index++;
            return true;
        }

        // Add Arena Location
        private bool command_add_arena()
        {
            // Value[0] = x
            // Value[1] = y
            // Value[2] = face
            // Value[3] = music
            // Value[4] = choice offsets
            Vector2 loc = new Vector2(process_number(command.Value[0]), process_number(command.Value[1]));
            int[] offsets = new int[1];
            string[] offsets_str = command.Value[4].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < offsets.Length; i++)
                offsets[i] = process_number(offsets_str[i]);
            Shop_Data shop = new Shop_Data(command.Value[2], command.Value[3], offsets, false, true);
            command_shop_text(shop);
            Global.game_map.add_shop(loc, shop);
            Index++;
            return true;
        }

        // Shop Text
        private void command_shop_text(Shop_Data shop)
        {
            while (Index + 1 < event_data.data.Count && event_data.data[Index + 1].Key == 84)
            {
                Index++;
                shop.add_text(command.Value);
                //foreach(string str in command.Value) //Debug
                //    shop.add_text(str);
            }
        }

        // Shop Inventory
        private void command_shop_inventory(Shop_Data shop)
        {
            while (Index + 1 < event_data.data.Count && event_data.data[Index + 1].Key == 85)
            {
                Index++;
                shop.add_items(command.Value.Select(str =>
                {
                    string[] item_str = str.Split(
                        new string[] { ", " },
                        StringSplitOptions.RemoveEmptyEntries);
                    return new ShopItemData(
                        process_number(item_str[0]),
                        process_number(item_str[1]),
                        process_number(item_str[2]));
                }));
            }
        }

        // 91: Add Escape Point
        private bool command_add_escape()
        {
            // Value[0] = type
            // Value[1] = x
            // Value[2] = y
            // Value[3] = x to escape to
            // Value[4] = y to escape to
            Vector2 loc = new Vector2(process_number(command.Value[1]), process_number(command.Value[2]));
            Vector2 escape_to_loc = new Vector2(process_number(command.Value[3]), process_number(command.Value[4]));
            switch (command.Value[0])
            {
                case "Thief":
                    Global.game_map.add_thief_escape(loc, escape_to_loc);
                    break;
                case "Team":
                    // Value[5] = team
                    // Value[6] = group (optional)
                    int group = command.Value.Length > 6 ? process_number(command.Value[6]) : -1;
                    Global.game_map.add_team_escape(process_number(command.Value[5]), group, loc, escape_to_loc);
                    break;
                case "Player Event":
                    // Value[5] = event
                    //?Value[6] = lord only? (default true)
                    bool lordOnly = command.Value.Length > 6 ? process_bool(command.Value[6]) : true;
                    Global.game_map.add_player_event_escape(command.Value[5], loc, escape_to_loc, lordOnly);
                    break;
#if DEBUG
                default:
                    throw event_case_missing_exception(command.Value[0], command.Key);
#endif
            }
            Index++;
            return true;
        }

        // 92: Add Seize Point
        private bool command_add_seize()
        {
            // Value[0] = team id
            // Value[1] = x
            // Value[2] = y
            // Value[3] = group (optional)
            Vector2 loc = new Vector2(process_number(command.Value[1]), process_number(command.Value[2]));
            int group = command.Value.Length > 3 ? process_number(command.Value[3]) : -1;

            Global.game_map.add_seize_point(process_number(command.Value[0]), loc, group);
            Index++;
            return true;
        }

        // 93: Add Defend Area
        private bool command_add_defend()
        {
            // Value[0] = team id
            // Value[1] = x1
            // Value[2] = y1
            // Value[3] = x2
            // Value[4] = y2
            // Value[5] = group id (optional)
            if (command.Value.Length <= 5)
                Global.game_map.add_defend_area(process_number(command.Value[0]), -1,
                    process_number(command.Value[1]), process_number(command.Value[2]),
                    process_number(command.Value[3]), process_number(command.Value[4]));
            else
                Global.game_map.add_defend_area(process_number(command.Value[0]), process_number(command.Value[5]),
                    process_number(command.Value[1]), process_number(command.Value[2]),
                    process_number(command.Value[3]), process_number(command.Value[4]));
            Index++;
            return true;
        }

        // 94: Add Unit Seek Loc
        private bool command_add_unit_seek()
        {
            int id;
            Game_Unit unit;

            switch (command.Value[0])
            {
                case "Seek Unit":
                    // Value[0] = "Seek Unit"
                    // Value[1] = unit id
                    // Value[2] = target id
                    id = process_unit_id(command.Value[1]);
                    unit = null;
                    if (id == -1)
                    {
                        if (Global.game_map.last_added_unit != null)
                            unit = Global.game_map.last_added_unit;
                    }
                    else if (Global.game_map.units.ContainsKey(id))
                        unit = Global.game_map.units[id];

                    id = process_unit_id(command.Value[2]);
                    Game_Unit target = null;
                    if (id == -1)
                    {
                        if (Global.game_map.last_added_unit != null)
                            target = Global.game_map.last_added_unit;
                    }
                    else if (Global.game_map.units.ContainsKey(id))
                        target = Global.game_map.units[id];

                    if (unit != null && unit != null && unit != target)
                    {
                        Global.game_map.AddUnitSeek(unit.id, target.id);
                    }
                    break;
                default:
                    // Value[0] = unit id
                    // Value[1] = x
                    // Value[2] = y
                    id = process_unit_id(command.Value[0]);
                    unit = null;
                    if (id == -1)
                    {
                        if (Global.game_map.last_added_unit != null)
                            unit = Global.game_map.last_added_unit;
                    }
                    else if (Global.game_map.units.ContainsKey(id))
                        unit = Global.game_map.units[id];

                    if (unit != null)
                    {
                        Global.game_map.add_unit_seek(unit.id, process_number(command.Value[1]), process_number(command.Value[2]));
                    }
                    break;
            }

            Index++;
            return true;
        }

        // 95: Add Team Seek Loc
        private bool command_add_team_seek()
        {
            // Value[0] = team id
            // Value[1] = group
            // Value[2] = x
            // Value[3] = y
            Global.game_map.add_team_seek(process_number(command.Value[0]), process_number(command.Value[1]),
                process_number(command.Value[2]), process_number(command.Value[3]));
            Index++;
            return true;
        }

        // Change Objective
        private bool command_change_objective()
        {
            // Value[0] = Victory
            // Value[1] = Loss
            // Value[2] = Objective mode
            // Value[3] = Objective text
            Global.game_system.Victory_Text = process_lines(command.Value[0]);
            Global.game_system.Loss_Text = process_lines(command.Value[1]);
            Global.game_system.Objective_Text = process_lines(command.Value[3]);
            string[] mode = command.Value[2].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            Global.game_system.Objective_Mode = new int[] { process_number(mode[0]), mode.Length < 2 ? 0 : process_number(mode[1]) };
            if (Global.scene.scene_type == "Scene_Map")
                (Global.scene as Scene_Map).update_objective();
            Index++;
            return true;
        }

        // Set Team Leader
        private bool command_set_leader()
        {
            // Value[0] = team id
            // Value[1] = id
            int id = process_unit_id(command.Value[1]);
            Game_Unit unit = null;
            if (id == -1)
                if (Global.game_map.last_added_unit != null)
                    unit = Global.game_map.last_added_unit;
            if (Global.game_map.units.ContainsKey(id))
                unit = Global.game_map.units[id];
            if (unit != null)
            {
                Global.game_map.set_team_leader(process_number(command.Value[0]), unit.id);
            }
            Index++;
            return true;
        }

        // 98: Loss On Death
        private bool command_loss_on_death()
        {
            //?Value[0] = id is for a unit, or for an actor
            // Value[1] = id
            //?Value[2] = adding, instead of removing? (default true)
            Game_Actor actor;
            get_actor(command.Value[0], command.Value[1], out actor);
            
            if (actor != null)
            {
                bool add = command.Value.Length <= 2 || process_bool(command.Value[2]);
                if (add)
                    Global.game_system.add_loss_on_death(actor.id);
                else
                    Global.game_system.remove_loss_on_death(actor.id);
            }
            Index++;
            return true;
        }

        // 103: ASMC
        private bool command_custom()
        {
            Game_Actor actor = null, source_actor = null;
            // Value[0] = type
            switch (command.Value[0])
            {
                case "Unit Stat Fix":
                #region Unit Stat Fix; set some of a unit's stats to specific numbers, and add/subtract from the others to make up the difference
                    // Value[1] = unit id
                    // Value[2-8] = stats
                    int unit_id = process_unit_id(command.Value[1]);
                    if (unit_id == -1)
                        if (Global.game_map.last_added_unit != null)
                            actor = Global.game_map.last_added_unit.actor;
                    if (Global.game_map.units.ContainsKey(unit_id))
                        actor = Global.game_map.units[unit_id].actor;
                    
                    if (actor != null)
                    {
                        Dictionary<Stat_Labels, int> stat_fixes = new Dictionary<Stat_Labels, int>();
                        for (int i = 2; i < command.Value.Length; i++)
                        {
                            int value = process_number(command.Value[i]);
                            if (value >= 0)
                                stat_fixes[(Stat_Labels)(i - 2)] = value;
                        }
                        int actor_stat_total = Enumerable.Range(0, Game_Actor.LEVEL_UP_VIABLE_STATS)
                            .Select(x => (int)(actor.stat(x) * Game_Actor.GetStatValue(x)))
                            .Sum();
                        int statFixesTotal = stat_fixes
                            .Select(pair => (int)(pair.Value * Game_Actor.GetStatValue((int)pair.Key)))
                            .Sum();
                        if (stat_fixes.Any(p => actor.get_cap(p.Key) < p.Value))
#if DEBUG
                        {
                            string over_capped_stats = "";
                            foreach (Stat_Labels stat in stat_fixes.Where(p => actor.get_cap(p.Key) < p.Value).Select(p => p.Key).Reverse())
                            {
                                if (!string.IsNullOrEmpty(over_capped_stats))
                                    over_capped_stats = ", " + over_capped_stats;
                                over_capped_stats = stat + over_capped_stats;
                            }
                            Print.message(string.Format(
                                "Tried to fix stats for a unit,\nbut there were provided stats higher\nthan the actor's caps.\n{0}",
                                over_capped_stats));
                        }
#else
                        { }
#endif
                        else if (actor_stat_total < statFixesTotal)
#if DEBUG
                            Print.message(string.Format(
                                "Tried to fix the stats for a unit,\nbut the final stat total is larger\nthan the current total.\nAttempted total: {0}; Actor total: {1}",
                                statFixesTotal,
                                actor_stat_total));
#else
                        { }
#endif
                        else
                        {
                            HashSet<Stat_Labels> keys = new HashSet<Stat_Labels>(stat_fixes.Keys);
                            foreach (Stat_Labels key in keys)
                                if (stat_fixes[key] == actor.stat(key))
                                    stat_fixes[key] = -1;
                            int rating = actor.rating();
                            // While any stats haven't been processed yet
                            while (stat_fixes.Values.Any(x => x != -1))
                            {
                                // Randomly selects one
                                List<Stat_Labels> stats = stat_fixes.Where(p => p.Value != -1).Select(p => p.Key).ToList();
                                stats.Sort();

                                Stat_Labels stat = stats[Global.game_system.get_rng() % stats.Count];
                                int statRatio = Game_Actor.GetStatRatio((int)stat);
                                // Modifies the selected stat
                                int difference = stat_fixes[stat] - actor.stat(stat);
                                // If the stat is worth less than a point and we're not adjusting by a full stat worth, just adjust it for free
                                if (statRatio > 1 && Math.Abs(difference) < statRatio)
                                {
                                    actor.gain_stat(stat, difference);
                                    stat_fixes[stat] = -1;
                                    continue;
                                }

                                // Randomly selects another stat to move points between
                                List<Stat_Labels> other_stats = Enumerable.Range(0, Game_Actor.LEVEL_UP_VIABLE_STATS)
                                    .Select(x => (Stat_Labels)x).Where(x => !stat_fixes.ContainsKey(x) &&
                                        (difference > 0 ? !actor.get_capped(x) : actor.stat(x) > 0)).ToList();
                                // If somehow we fail to find another stat to give/take from, it still needs to adjust the target stat
                                //@Yeti: This should take points from multiple stats and not just one, right?
                                if (other_stats.Count > 0)
                                {
                                    Stat_Labels other_stat = other_stats[Global.game_system.get_rng() % other_stats.Count];
                                    int otherStatRatio = Game_Actor.GetStatRatio((int)other_stat);
                                    actor.gain_stat(other_stat, otherStatRatio * (difference <= 0 ? 1 : -1));
                                }
                                actor.gain_stat(stat, statRatio * (difference > 0 ? 1 : -1));
                                if (stat_fixes[stat] == actor.stat(stat))
                                    stat_fixes[stat] = -1;

                            }
                            rating = actor.rating();
                        }
                    }
                    break;
                #endregion
                case "Transfer Blessings":
                #region Transfer Blessings; increases the stats of an actor by the blessings of another actor
                    // Value[1] = Source Actor Id
                    // Value[2] = Target Actor Id
                    get_actor("false", command.Value[2], out actor);
                    get_actor("false", command.Value[1], out source_actor);
                    if (actor != null && source_actor != null)
                    {
                        actor.transfer_blessing(source_actor);
                    }
                    break;
                #endregion
#if DEBUG
                default:
                    throw event_case_missing_exception(command.Value[0], command.Key);
#endif
            }
            Index++;
            return true;
        }

        // Set Switch
        private bool command_set_switch()
        {
            // Value[0] = id
            // Value[1] = type: Set
            int id = process_number(command.Value[0]);
            if (id < 0 || id >= SWITCHES.Length)
            {
#if DEBUG
                Debug.Assert(id >= 0 && id < SWITCHES.Length, "Invalid Switch Id");
#endif
                Index++;
                return true;
            }
            switch (command.Value[1])
            {
                case "Set":
                    // Value[2] = new value
                    SWITCHES[id] = process_bool(command.Value[2]);
                    break;
#if DEBUG
                default:
                    throw event_case_missing_exception(command.Value[1], command.Key);
#endif
            }
            Index++;
            return true;
        }

        // Set Variable
        private bool command_set_variable()
        {
            // Value[0] = id
            // Value[1] = type: Set
            int id = process_number(command.Value[0]);
            int unit_id, actor_id;
            if (id < 0 || id >= VARIABLES.Length)
            {
#if DEBUG
                Debug.Assert(id >= 0 && id < VARIABLES.Length, "Invalid Variable Id");
#endif
                Index++;
                return true;
            }
            switch (command.Value[1])
            {
                case "Set":
                    // Value[2] = new value
                    VARIABLES[id] = process_number(command.Value[2], Global.scene.is_map_scene);
                    break;
                case "Turn":
                    // Value[2] = added to value
                    VARIABLES[id] = Global.game_state.turn + process_number(command.Value[2], true);
                    break;
                case "Operator":
                    // Value[2] = operator
                    // Value[3] = value1
                    // Value[4] = value2
                    int value1 = process_number(command.Value[3], Global.scene.is_map_scene);
                    int value2 = process_number(command.Value[4], Global.scene.is_map_scene);
                    switch (command.Value[2])
                    {
                        case "+":
                            VARIABLES[id] = value1 + value2;
                            break;
                        case "-":
                            VARIABLES[id] = value1 - value2;
                            break;
                        case "*":
                            VARIABLES[id] = value1 * value2;
                            break;
                        case "/":
                            VARIABLES[id] = value1 / value2;
                            break;
                        case "%":
                            VARIABLES[id] = value1 % value2;
                            break;
                        case "**":
                            VARIABLES[id] = (int)Math.Pow(value1, value2);
                            break;
                    }
                    break;
                case "Variable": // Probably deprecate this //Yeti
                    // Value[2] = operator
                    // Value[3] = other variable id
                    // Value[4] = value
                    int other_id = process_number(command.Value[3]);
                    int value = process_number(command.Value[4], Global.scene.is_map_scene);
                    switch (command.Value[2])
                    {
                        case "+":
                            VARIABLES[id] = VARIABLES[other_id] + value;
                            break;
                        case "-":
                            VARIABLES[id] = VARIABLES[other_id] - value;
                            break;
                        case "*":
                            VARIABLES[id] = VARIABLES[other_id] * value;
                            break;
                        case "/":
                            VARIABLES[id] = VARIABLES[other_id] / value;
                            break;
                        case "%":
                            VARIABLES[id] = VARIABLES[other_id] % value;
                            break;
                        case "**":
                            VARIABLES[id] = (int)Math.Pow(VARIABLES[other_id], value);
                            break;
                    }
                    break;
                // This uses RNs, so it will be consistent every time
                // It should never be called during a battle/etc, or it will severely break things
                case "RNG":
#if DEBUG
                    Debug.Assert(!Global.game_system.has_preset_rns);
#endif
                    VARIABLES[id] = Global.game_system.get_rng();
                    break;
                case "Unit X":
                    // Value[2] = unit id
                    unit_id = process_unit_id(command.Value[2]);
                    Game_Unit unit_x = null;
                    if (unit_id == -1)
                        if (Global.game_map.last_added_unit != null)
                            unit_x = Global.game_map.last_added_unit;
                    if (Global.game_map.units.ContainsKey(unit_id))
                        unit_x = Global.game_map.units[unit_id];
                    if (unit_x != null)
                        VARIABLES[id] = (int)unit_x.loc.X;
                    break;
                case "Unit Y":
                    // Value[2] = unit id
                    unit_id = process_unit_id(command.Value[2]);
                    Game_Unit unit_y = null;
                    if (unit_id == -1)
                        if (Global.game_map.last_added_unit != null)
                            unit_y = Global.game_map.last_added_unit;
                    if (Global.game_map.units.ContainsKey(unit_id))
                        unit_y = Global.game_map.units[unit_id];
                    if (unit_y != null)
                        VARIABLES[id] = (int)unit_y.loc.Y;
                    break;
                case "Actor Class":
                    // Value[2] = actor id
                    actor_id = process_number(command.Value[2]);
                    if (Global.game_actors.ContainsKey(actor_id))
                        VARIABLES[id] = Global.game_actors[actor_id].class_id;
                    break;
                // This only works pre-battle because it gets set to -1 during battle setup //Yeti
                case "Unit Opponent":
                    // Value[2] = unit id
                    unit_id = process_unit_id(command.Value[1]);
                    if (Global.game_map.units.ContainsKey(unit_id))
                    {
                        if (unit_id == Global.game_system.Battler_1_Id)
                            VARIABLES[id] = Global.game_system.Battler_2_Id;
                        else if (unit_id == Global.game_system.Battler_2_Id)
                            VARIABLES[id] = Global.game_system.Battler_1_Id;
                        else
                            VARIABLES[id] = -1;
                    }
                    else
                        VARIABLES[id] = -1;
                    break;
#if DEBUG
                default:
                    throw event_case_missing_exception(command.Value[1], command.Key);
#endif
            }
            Index++;
            return true;
        }

        // 113: Dialogue Prompt
        private bool command_dialogue_prompt()
        {
            // Value[0] = variable id
            // Value[1] = chapter message value
            // Value[2] = dialogue choices
            // 2 repeats
            if (!Global.scene.is_worldmap_scene)
            {
                int id = process_number(command.Value[0]);
                if (id < -1 || id >= VARIABLES.Length)
                {
#if DEBUG
                    Debug.Assert(id >= -1 && id < VARIABLES.Length, "Invalid Variable Id");
#endif
                    Index++;
                    return true;
                }
                // If text isn't already active, load the text that is supplied and start it
                if (!Global.scene.is_message_window_waiting)
                {
                    // If there is text to use
                    if (command.Value[1] != "null")
                    {
#if DEBUG
                        if (!Global.chapter_text.ContainsKey(command.Value[1]))
                            Print.message(string.Format("No text with the key \"{0}\" exists for this chapter", command.Value[1]));
                        else
#endif
                            if (Global.chapter_text.ContainsKey(command.Value[1]))
                            {
                                Global.game_temp.message_text = Global.chapter_text[command.Value[1]];
                                Global.scene.new_message_window();
                            }
                    }
                }
                // Get the choices
                List<string> dialogueChoices = command.Value.Skip(2).ToList();

                ((Scene_Map)Global.scene).DialoguePrompt(id, dialogueChoices);
            }
            Index++;
            return false;
        }

        // 114: Confirmation Prompt
        private bool command_confirmation_prompt()
        {
            // Value[0] = switch id
            // Value[1] = chapter message value
            // Value[2] = caption
            if (!Global.scene.is_worldmap_scene)
            {
                int id = process_number(command.Value[0]);
                if (id < -1 || id >= SWITCHES.Length)
                {
#if DEBUG
                    Debug.Assert(id >= -1 && id < SWITCHES.Length, "Invalid Switch Id");
#endif
                    Index++;
                    return true;
                }
                // If text isn't already active, load the text that is supplied and start it
                if (!Global.scene.is_message_window_waiting)
                {
                    // If there is text to use
                    if (command.Value[1] != "null")
                    {
#if DEBUG
                        if (!Global.chapter_text.ContainsKey(command.Value[1]))
                            Print.message(string.Format("No text with the key \"{0}\" exists for this chapter", command.Value[1]));
                        else
#endif
                            if (Global.chapter_text.ContainsKey(command.Value[1]))
                            {
                                Global.game_temp.message_text = Global.chapter_text[command.Value[1]];
                                Global.scene.new_message_window();
                            }
                    }
                }

                ((Scene_Map)Global.scene).ConfirmationPrompt(id, command.Value[2]);
            }
            Index++;
            return false;
        }

        // 121: Return to Title
        private bool command_title()
        {
#if DEBUG
            if (Global.UnitEditorActive)
                Global.scene_change("Scene_Map_Unit_Editor");
            else
#endif
                Global.scene_change("Scene_Title_Load");
            Index++;
            return false;
        }

        // 122: Game Over
        private bool command_gameover()
        {
            ((Scene_Map)Global.scene).gameover();
            Index++;
            return false;
        }

        // Return to World Map
        private bool command_worldmap()
        {
#if DEBUG
            if (Global.UnitEditorActive)
                Global.scene_change("Scene_Map_Unit_Editor");
            else
#endif
            {
                Global.scene_change("Scene_Save");
                if (command.Value.Length > 0)
                    Global.game_system.Chapter_Save_Progression_Keys = command.Value[0].Split(' ');
            }
            Index++;
            return false;
        }

        // 124: Preparations
        private bool command_preparations()
        {
            Index++;
            if (Global.scene.scene_type == "Scene_Map")
            {
                ((Scene_Map)Global.scene).activate_preparations();
                return false;
            }
            return true;
        }

        // 125: Map Save
        private bool command_map_save()
        {
            Index++;
            if (Global.game_system.Style != Mode_Styles.Classic && Global.scene.scene_type == "Scene_Map")
            {
                Global.game_temp.menu_call = true;
                Global.game_temp.map_save_call = true;
                return false;
            }
            return true;
        }

        // 126: End Chapter
        private bool command_end_chapter()
        {
            //?Value[0] = Show rankings (optional)
            //?Value[1] = Send metrics (optional)
            //?Value[2] = Gain support points (optional)
            if (Global.scene.scene_type == "Scene_Map")
            {
                bool show_rankings = true;
                bool send_metrics = true;
                bool gain_support_points = true;

                if (command.Value.Length >= 1)
                {
                    show_rankings = process_bool(command.Value[0]);
                    // Send metrics defaults to the same value as show rankings
                    // (If we're not showing rankings, there's probably nothing to send)
                    send_metrics = show_rankings;
                }
                if (command.Value.Length >= 2)
                    send_metrics = process_bool(command.Value[1]);
                if (command.Value.Length >= 3)
                    gain_support_points = process_bool(command.Value[2]);

                Global.game_state.call_chapter_end(
                    show_rankings, send_metrics, gain_support_points);

                Index++;
                return false;
            }
            Index++;
            return true;
        }

        // 127: Home Base
        private bool command_home_base()
        {
            // Value[0] = Background
            if (Global.scene.scene_type == "Scene_Map")
            {
                ((Scene_Map)Global.scene).activate_home_base(command.Value.Length > 0 ? command.Value[0] : "");
                Index++;
                return false;
            }
            Index++;
            return true;
        }

        // 128: Wait for Preparations
        private bool command_wait_for_prep()
        {
            if (!Global.scene.is_worldmap_scene)
            {
                if (Global.game_system.preparations)
                    return false;
            }
            Index++;
            Global.game_state.clear_home_base_events();
            return true;
        }

        // 129: Add Base Event
        private bool command_add_base_event()
        {
            // Value[0] = Name
            // Value[1] = Importance
            // Value[2] = Event to call name
            Global.game_state.add_base_event(command.Value[0], process_number(command.Value[1]), command.Value[2]);
            Index++;
            return true;
        }

        // 130: Gain Completion Points
        private bool command_gain_completion_points()
        {
            // Value[0] = Points to gain
            Global.game_system.chapter_completion += process_number(command.Value[0]);
            Index++;
            return true;
        }

        // Play BGM
        private bool command_play_bgm()
        {
            // Value[0] = BGM name
            //?Value[1] = Force restarting theme? (default false)
            // Value[2] = If turn theme, which phase? (optional)
            bool restart_theme = command.Value.Length > 1 && process_bool(command.Value[1]);
            if (command.Value[0] == "Turn Theme")
            {
                if (command.Value.Length > 2)
                {
                    int phase = process_number(command.Value[2]);
                    Global.game_state.play_turn_theme(teamTurn: phase);
                }
                else
                    Global.game_state.play_turn_theme();
            }
            else if (Global.scene.is_map_scene && !Global.game_state.is_battle_map)
            {
                // Play the event BGM as a turn theme, so that it will resume
                // after scripted battles
                Global.game_state.play_turn_theme(command.Value[0], restart_theme);
            }
            else
                Global.Audio.PlayBgm(command.Value[0], forceRestart: restart_theme);
            Index++;
            return true;
        }

        // Fade BGM
        private bool command_fade_bgm()
        {
            // Value[0] = Fade time (frames)
            int frames = process_number(command.Value[0]);
            if (frames <= 0)
                Global.Audio.StopBgm();
            else
                Global.Audio.BgmFadeOut(frames);
            Index++;
            return true;
        }

        // Play SFX
        private bool command_play_sfx()
        {
            // Value[0] Sound group
            // Value[1] = SFX name
            //?Value[2] = Duck BGM volume? (optional)
            bool duckBgm = command.Value.Length > 2 &&
                process_bool(command.Value[2]);

            if (duckBgm)
                Global.Audio.play_se(command.Value[0], command.Value[1], duckBgm: duckBgm);
            else if (command.Value[0] == "System Sounds")
                Global.game_system.play_system_se(command.Value[1]);
            else
                Global.Audio.play_se(command.Value[0], command.Value[1], duckBgm: duckBgm);
            Index++;
            return true;
        }

        // Play BGS
        private bool command_play_bgs()
        {
            // Value[0] = BGS name
            Global.Audio.play_bgs(command.Value[0]);
            Index++;
            return true;
        }

        // Stop BGS
        private bool command_stop_bgs()
        {
            Global.Audio.stop_bgs();
            Index++;
            return true;
        }

        // Fade SFX
        private bool command_fade_sfx()
        {
            // Value[0] = Fade time (frames)
            Global.Audio.sfx_fade(process_number(command.Value[0]));
            Index++;
            return true;
        }

        // Stop SFX
        private bool command_stop_sfx()
        {
            Global.Audio.stop_sfx();
            Index++;
            return true;
        }

        // 138: Duck BGM
        private bool command_duck_bgm()
        {
            //?Value[0] = End reduced BGM volume? (optional)
            bool endDucking = command.Value.Length > 0 &&
                process_bool(command.Value[0]);

            if (endDucking)
                Global.Audio.RestoreBgmVolume();
            else
                Global.Audio.DuckBgmVolume();
            Index++;
            return true;
        }

        // Center Worldmap Camera
        private bool command_center_worldmap()
        {
            // Value[0] = x, y
            // Value[1] = speed (optional)
            ((Scene_Worldmap)Global.scene).target_loc =
                process_vector2(command.Value[0]) - Constants.WorldMap.WORLDMAP_EVENT_OFFSET;
            if (command.Value.Length > 1)
                ((Scene_Worldmap)Global.scene).scroll_speed = process_float(command.Value[1]);
            Index++;
            return true;
        }

        // Worldmap Dot
        private bool command_worldmap_dot()
        {
            // Value[0] = team id
            // Value[1] = x, y
            ((Scene_Worldmap)Global.scene).add_dot(process_number(command.Value[0]), process_vector2(command.Value[1]));
            Index++;
            return true;
        }

        // Worldmap Arrow
        private bool command_worldmap_arrow()
        {
            // Value[0] = team id
            // Value[1] = speed
            // Value[2] = x waypoint, y waypoint
            // 2 repeats
            Vector2[] waypoints = new Vector2[command.Value.Length - 2];
            for (int i = 0; i < waypoints.Length; i++)
                waypoints[i] = process_vector2(command.Value[i + 2]);
            ((Scene_Worldmap)Global.scene).add_arrow(process_number(command.Value[0]), process_number(command.Value[1]), waypoints);
            Index++;
            return true;
        }

        // Worldmap Remove Dot
        private bool command_worldmap_remove_dot()
        {
            if (command.Value.Length <= 0)
            {
                // Remove all dots
                ((Scene_Worldmap)Global.scene).remove_dots();
            }
            else
            {
                // Value[0] = index
                ((Scene_Worldmap)Global.scene).remove_dot(process_number(command.Value[0]));
            }
            Index++;
            return true;
        }

        // Worldmap Beacon
        private bool command_worldmap_beacon()
        {
            // Value[0] = x, y
            ((Scene_Worldmap)Global.scene).add_beacon(process_vector2(command.Value[0]));
            Index++;
            return true;
        }

        // Worldmap Remove Beacon
        private bool command_worldmap_remove_beacon()
        {
            ((Scene_Worldmap)Global.scene).remove_beacon();
            Index++;
            return true;
        }

        // Worldmap Zoomed Out
        private bool command_worldmap_zoomed_out()
        {
            // Value[0] = zoom out?
            ((Scene_Worldmap)Global.scene).zoomed_map_visible = process_bool(command.Value[0]);
            Index++;
            return true;
        }

        // Worldmap Unit
        private bool command_worldmap_unit()
        {
            // Value[0] = team id
            // Value[1] = bool: Actor instead of generic?
            // Value[2] = Actor Id/Graphic Name
            // Value[3] = x, y
            string filename;
            if (process_bool(command.Value[1]))
                filename = Global.game_actors[process_number(command.Value[2])].map_sprite_name;
            else
            {
                filename = command.Value[2];
            }
            ((Scene_Worldmap)Global.scene).add_unit(process_number(command.Value[0]), filename, process_vector2(command.Value[3]));
            Index++;
            return true;
        }

        // Wmap Queue Unit Move
        private bool command_worldmap_queue_unit_move()
        {
            // Value[0] = index
            // Value[1] = speed
            // Value[2] = x waypoint, y waypoint
            // 2 repeats
            Vector2[] waypoints = new Vector2[command.Value.Length - 2];
            for (int i = 0; i < waypoints.Length; i++)
                waypoints[i] = process_vector2(command.Value[i + 2]);
            ((Scene_Worldmap)Global.scene).queue_unit_move(process_number(command.Value[0]), process_number(command.Value[1]), waypoints);
            Index++;
            return true;
        }

        // Wmap Queue Unit Idle
        private bool command_worldmap_queue_unit_idle()
        {
            // Value[0] = index
            ((Scene_Worldmap)Global.scene).queue_unit_idle(process_number(command.Value[0]));
            Index++;
            return true;
        }

        // Wmap Queue Unit Pose
        private bool command_worldmap_queue_unit_pose()
        {
            // Value[0] = index
            ((Scene_Worldmap)Global.scene).queue_unit_pose(process_number(command.Value[0]));
            Index++;
            return true;
        }

        // Wmap Queue Unit Remove
        private bool command_worldmap_queue_unit_remove()
        {
            // Value[0] = index
            // Value[1] = bool: immediately instead of queued?
            //?Value[2] = bool: kill instead of just fade out?
            bool kill = command.Value.Length <= 2 ? false : process_bool(command.Value[2]);
            ((Scene_Worldmap)Global.scene).queue_unit_remove(
                process_number(command.Value[0]), process_bool(command.Value[1]), kill);
            Index++;
            return true;
        }

        // Wmap Clear Removing
        private bool command_worldmap_clear_removing()
        {
            (Global.scene as Scene_Worldmap).clear_removing_units();
            Index++;
            return true;
        }

        // Wmap Queue Unit Track
        private bool command_worldmap_queue_unit_tracking()
        {
            // Value[0] = index
            // Value[1] = min x, min y
            // Value[2] = max x, max y
            ((Scene_Worldmap)Global.scene).queue_unit_tracking(
                process_number(command.Value[0]),
                process_vector2(command.Value[1]) - Constants.WorldMap.WORLDMAP_EVENT_OFFSET,
                process_vector2(command.Value[2]) - Constants.WorldMap.WORLDMAP_EVENT_OFFSET);
            Index++;
            return true;
        }

        // Wmap Wait for Units
        private bool command_worldmap_wait_for_unit_move()
        {
            if (((Scene_Worldmap)Global.scene).units_moving)
                return false;
            Index++;
            return true;
        }

        // Preset RNs
        private bool command_preset_rns()
        {
            switch (command.Value[0])
            {
                case "Clear":
                    Global.game_system.clear_preset_rns();
                    break;
                default:
                    // Value[] = RNs
                    for (int i = 0; i < command.Value.Length; i++)
                        Global.game_system.add_preset_rn(process_number(command.Value[i], Global.scene.is_map_scene));
                    break;
            }
            Index++;
            return true;
        }

        // Scripted Battle
        private bool command_scripted_battle()
        {
            // Value[0] = Battler 1 id
            // Value[1] = Battler 2 id
            //?Value[2] = Scene battle?

            if (Global.game_state.combat_active)
            {
                throw new InvalidOperationException("Attempted to start a scripted battle while\na battle is already running");
            }

            int attacker_id = process_unit_id(command.Value[0]);
            int target_id = process_unit_id(command.Value[1]);
            Global.game_temp.scripted_battle_stats.scene_battle = command.Value.Length <= 2 || process_bool(command.Value[2]);

            Global.game_temp.scripted_battle = true;
            Global.game_state.call_battle(attacker_id, target_id);

            Battle_Wait = true;

            Index++;
            return false;
        }

        // Scripted Battle Parameters
        private bool command_scripted_battle_params()
        {
            // Value[0] = Attacker
            // Value[1] = Stats 1
            // Value[2] = Stats 2
            // Value[3] = Hit, Crit, Miss?
            // Value[4] = Damage

            if (Global.game_state.combat_active)
            {
                throw new InvalidOperationException("Attempted to start a scripted battle while\na battle is already running");
            }
            string[] str_ary;

            var battle_script = new Scripted_Combat_Script();
            while (command.Key == 173)
            {
                Attack_Results result;
                switch (command.Value[3])
                {
                    case "Hit":
                        result = Attack_Results.Hit;
                        break;
                    case "Crit":
                        result = Attack_Results.Crit;
                        break;
                    case "Miss":
                        result = Attack_Results.Miss;
                        break;
                    // Sets the kill flag for this combat
                    case "Kill":
                        battle_script.kill = process_number(command.Value[0]);
                        Index++;
                        continue;
                    default:
                        result = Attack_Results.End;
                        break;
                }

                var combat_stats = new Scripted_Combat_Stats();
                combat_stats.Attacker = process_number(command.Value[0]);
                combat_stats.Result = result;
                combat_stats.Damage = process_number(command.Value[4]);

                str_ary = command.Value[1].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                combat_stats.Stats_1 = new List<int>();
                // If the length is 1 and the value is -1, all stats are --
                if (str_ary.Length > 1 || process_number(str_ary[0]) != -1)
                    for (int i = 0; i < str_ary.Length; i++)
                        combat_stats.Stats_1.Add(process_number(str_ary[i]));

                str_ary = command.Value[2].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                combat_stats.Stats_2 = new List<int>();
                // If the length is 1 and the value is -1, all stats are --
                if (str_ary.Length > 1 || process_number(str_ary[0]) != -1)
                    for (int i = 0; i < str_ary.Length; i++)
                        combat_stats.Stats_2.Add(process_number(str_ary[i]));
                battle_script.Add(combat_stats);

                Index++;
            }
            Global.game_temp.scripted_battle_stats = battle_script;

            return false;
        }

        // 201: If statement
        private bool command_if()
        {
#if DEBUG
            if (!IndentBlocks.ContainsKey(Index))
                throw new System.IndexOutOfRangeException("If statement somehow didn't form a block");
#endif
            var ifBlock = IndentBlocks[Index];

            for (; ; )
            {
                bool result = test_if_statement();

                if (result)
                {
                    // Skip any remaining if (or) statements
                    while (Index + 1 < event_data.data.Count &&
                            event_data.data[Index + 1].Key == 202 ||
                            // Also comments
                            event_data.data[Index + 1].Key == 102)
                        Index++;
                    break;
                }
                // The if statement was false, so use the else/endif/elseif statements in the same block
                else
                {
                    // Look for an elseif/else
                    bool elseIfFound = false;
                    var intermediateControls = ifBlock.IntermediateControlIndices;
                    for (int i = 0; i < intermediateControls.Count; i++)
                    {
                        int intermediateIndex = intermediateControls[i];
                        if (intermediateIndex <= Index)
                            continue;

                        // Else if
                        if (event_data.data[intermediateIndex].Key == 205)
                        {
                            Index = intermediateIndex;
                            elseIfFound = true;
                            break;
                        }
                        // Else
                        else if (event_data.data[intermediateIndex].Key == 203)
                        {
                            // Advance to the line after the else statement, then return
                            Index = intermediateIndex + 1;
                            return true;
                        }
                    }
                    // Find the endif and continue
                    if (!elseIfFound)
                    {
                        // If there is no matching block end, go to the end of the event
                        if (ifBlock.EndControlIndex == -1)
                            Index = event_data.data.Count;
                        else
                            Index = ifBlock.EndControlIndex + 1;
                        return true;
                    }
                }
            }
            Index++;
            return true;
        }

        private bool test_if_statement()
        {
            bool result = false;
            if (command.Value.Any())
            {
                string[] if_key = command.Value[0].Split('-');
                result = if_statement_switch(if_key);
                if (if_key.Length > 1 && if_key[1] == "Not")
                    result = !result;
            }
            // Skip comments
            while (Index + 1 < event_data.data.Count &&
                    event_data.data[Index + 1].Key == 102)
                Index++;
            // Check following if(or) tests
            if (Index + 1 < event_data.data.Count &&
                    event_data.data[Index + 1].Key == 202)
                // If the result is already true, short circuit the check
                if (!result)
                {
                    Index++;
                    result |= test_if_statement();
                }
            return result;
        }

        private bool if_statement_switch(string[] if_key)
        {
            bool result = false;

            int min, interval, max, id;
            Game_Unit unit = null;
            Game_Actor actor = null;
            switch (if_key[0])
            {
                // This version uses RNs, so it will be consistent every time
                // It should never be called during a battle/etc, or it will severely break things
                case "RNG":
                    // Value[1] = rate
#if DEBUG
                    Debug.Assert(!Global.game_system.has_preset_rns);
#endif
                    result = Global.game_system.roll_rng(process_number(command.Value[1]));
                    break;
                // This version uses just random numbers, if you want it to really be random
                case "Random":
                    // Value[1] = rate
                    result = process_number(command.Value[1]) < rand.Next(100);
                    break;
                case "Repeat":
                    if (Global.scene.is_map_scene)
                        result = Global.game_state.event_handler.is_event_repeat(event_data.name);
                    break;
                case "True":
                    result = true;
                    break;
                case "False":
                    result = false;
                    break;
                case "LastConfirmationPrompt":
                    result = process_bool("LastConfirmationPrompt");
                    break;
                case "Switch":
                    // Value[1] = id
                    bool switches_on = SWITCHES[process_number(command.Value[1])];
                    for (int i = 2; i < command.Value.Length; i++)
                        switches_on &= SWITCHES[process_number(command.Value[i])];
                    result = switches_on;
                    break;
                case "Variable":
                    // Value[1] = id
                    // Value[2] = min value
                    // Value[3] = value interval
                    // Value[4] = max value
                    int variable = VARIABLES[process_number(command.Value[1])];
                    min = process_number(command.Value[2], Global.scene.is_map_scene);
                    interval = process_number(command.Value[3], Global.scene.is_map_scene);
                    max = process_number(command.Value[4], Global.scene.is_map_scene);
                    switch (interval)
                    {
                        case -1:
                            result = variable >= min; // Greater than min
                            break;
                        case 0:
                            result = variable <= max; // Less than max
                            break;
                        default:
                            result = variable >= min && // Greater than min
                                (variable <= max) && // Less than max
                                (variable - min) % interval == 0; // Intervals from min
                            break;
                    }
                    break;
                case "Equals":
                    // Value[1] = value 1
                    // Value[2] = value 2
                    result = process_number(command.Value[1], Global.scene.is_map_scene) ==
                        process_number(command.Value[2], Global.scene.is_map_scene);
                    break;
                case "Operator":
                    // Value[1] = value1
                    // Value[2] = operator
                    // Value[3] = value2
                    int value1 = process_number(command.Value[1], Global.scene.is_map_scene);
                    int value2 = process_number(command.Value[3], Global.scene.is_map_scene);
                    result = operator_comparison(command.Value[2],
                        value1, value2);
                    break;
                case "S Rank":
                    result = new Game_Ranking().ranking_index <= 0; //@Debug
                    break;
                case "A Rank":
                    result = new Game_Ranking().ranking_index <= 1; //@Debug
                    break;
                case "B Rank":
                    result = new Game_Ranking().ranking_index <= 2; //@Debug
                    break;
                #region Map State
                case "In Combat":
                    result = Global.game_state.combat_active;
                    break;
                case "In Battle":
                    result = Global.game_state.battle_active;
                    break;
                case "In Staff":
                    result = Global.game_state.staff_active;
                    break;
                case "In Item Use":
                    result = Global.game_state.item_active;
                    break;
                case "FoW":
                    result = Global.game_map.fow;
                    break;
                case "Turn":
                    // Value[1] = min turn
                    // Value[2] = turn interval
                    // Value[3] = max turn
                    int turn = Global.game_state.turn;
                    min = process_number(command.Value[1]);
                    interval = process_number(command.Value[2]);
                    max = process_number(command.Value[3]);
                    result = turn >= min && // Greater than min
                        (max <= -1 || turn <= max) && // Less than max
                        (turn - min) % interval == 0; // Intervals from min
                    break;
                case "Phase":
                    // Value[1] = phase to check
                    result = Global.game_state.team_turn ==
                        process_number(command.Value[1]);
                    break;
                case "Tile Occupied":
                    // Value[1] = x
                    // Value[2] = y
                    // Checks every two lines as an x/y pair, true if any
                    for (int i = 1; i <= command.Value.Length - 2; i += 2)
                    {
                        Vector2 tileLoc = new Vector2(
                            process_number(command.Value[i]),
                            process_number(command.Value[i + 1]));
                        unit = Global.game_map.get_unit(tileLoc);
                        if (unit != null)
                        {
                            result = true;
                            break;
                        }
                    }
                    break;
                case "Tile Enemy Occupied":
                    // Value[1] = x
                    // Value[2] = y
                    // Value[3] = team
                    unit = Global.game_map.get_unit(new Vector2(process_number(command.Value[1]), process_number(command.Value[2])));
                    result = unit != null && unit.is_attackable_team(process_number(command.Value[3]));
                    break;
                case "Team Area":
                    // Value[1] = x1
                    // Value[2] = y1
                    // Value[3] = x2
                    // Value[4] = y2
                    // Value[5] = team id
                    result = Global.game_map.is_team_in_area(process_number(command.Value[1]), process_number(command.Value[2]),
                        process_number(command.Value[3]), process_number(command.Value[4]), process_number(command.Value[5]));
                    break;
                #endregion
                case "Unit Exists":
                    // Value[1] = unit id
#if DEBUG
                    result = Global.game_map.units.ContainsKey(process_unit_id(command.Value[1], ignore_error: true));
#else
                    result = Global.game_map.units.ContainsKey(process_unit_id(command.Value[1]));
#endif
                    break;
                case "Actor has Unit":
                    // Value[1] = actor id
                    result = Global.game_map.is_actor_deployed(process_number(command.Value[1]));
                    break;
                case "Support":
                    // Value[1] = actor 1 id
                    // Value[2] = actor 2 id
                    // Value[3] = support rank
                    string supportRank = command.Value[3];
                    int supportIndex = Constants.Support.SUPPORT_LETTERS.IndexOf(supportRank);
                    if (supportIndex != 0)
                    {
                        Game_Actor support_partner;
                        get_actor("false", command.Value[1], out actor);
                        get_actor("false", command.Value[2], out support_partner);
                        if (actor != null && support_partner != null)
                        {
                            result = actor.get_support_level(support_partner.id) >= supportIndex;
                        }
                    }
                    break;
                case "Siege Ammo":
                    // Value[1] = x, y
                    // Value[2] = uses
                    Vector2 siege_loc = process_vector2(command.Value[1]);
                    var siege = Global.game_map.get_siege(siege_loc);
                    if (siege != null)
                        result = siege.item.Uses >= process_number(command.Value[2]);
#if DEBUG
                    else
                    {
                        Print.message(string.Format(
                            "Event if statement \"Siege Ammo\" could not find\n" +
                            "siege engine at location {0}", siege_loc));
                    }
#endif
                    break;
                #region Unit State
                case "Level":
                    // Value[1] = id is for a unit, or for an actor?
                    // Value[2] = id
                    // Value[3] = level to check, returns true if actor level is greater than or equal
                    get_actor(command.Value[1], command.Value[2], out actor);
                    if (actor != null)
                        result = actor.level >= process_number(command.Value[3]);
                    break;
                case "Tier":
                    // Value[1] = id is for a unit, or for an actor?
                    // Value[2] = id
                    // Value[3] = tier to check, returns true if actor tier is greater than or equal
                    get_actor(command.Value[1], command.Value[2], out actor);
                    if (actor != null)
                        result = actor.tier >= process_number(command.Value[3]);
                    break;
                case "Weapon Level":
                case "WLvl":
                    // Value[1] = id is for a unit, or for an actor?
                    // Value[2] = id
                    // Value[3] = weapon type
                    // Value[4] = wexp value
                    get_actor(command.Value[1], command.Value[2], out actor);
                    if (actor != null)
                    {
                        var weaponType = Global.weapon_types[process_number(command.Value[3])];

                        int wexp = 0;
                        // If the wexp value is a weapon rank letter, convert to that rank's value
                        if (Data_Weapon.WLVL_LETTERS.Contains(command.Value[4]))
                            wexp = Data_Weapon.WLVL_THRESHOLDS[Data_Weapon.WLVL_LETTERS.ToList().IndexOf(command.Value[4])];
                        else
                            wexp = process_number(command.Value[4]);

                        result = actor.weapon_levels(weaponType) >= wexp;
                    }
                    break;
                case "Inventory Full":
                    // Value[1] = id is for a unit, or for an actor?
                    // Value[2] = id
                    get_actor(command.Value[1], command.Value[2], out actor);
                    if (actor != null)
                        result = actor.is_full_items;
                    break;
                // If this unit/actor has the given item
                case "Has Item":
                    // Value[1] = id is for a unit, or for an actor?
                    // Value[2] = id
                    // Value[3] = item type
                    // Value[4] = item id
                    get_actor(command.Value[1], command.Value[2], out actor);
                    if (actor != null)
                    {
                        var itemData = new Item_Data(process_number(command.Value[3]),
                            process_number(command.Value[4]));
                        result = actor.HasItem(itemData);
                    }
                    break;
                // If any actor in the battalion or the convoy have the given item
                case "Item Owned":
                    // Value[1] = item type
                    // Value[2] = item id
                    var ownedItemData = new Item_Data(process_number(command.Value[1]),
                        process_number(command.Value[2]));
                    result = Global.battalion.ItemOwned(ownedItemData);
                    break;
                case "Mission":
                    // Value[1] = id
                    // Value[2] = mission
                    id = process_unit_id(command.Value[1]);
                    if (id == -1)
                        if (Global.game_map.last_added_unit != null)
                            unit = Global.game_map.last_added_unit;
                    if (Global.game_map.units.ContainsKey(id))
                        result = (Global.game_map.units[id].ai_mission) == process_number(command.Value[2]);
                    break;
                case "Unit Gender":
                    // Value[1] = unit id
                    // Value[2] = gender
                    //?Value[3] = simplify to 0/1, default false
                    id = process_unit_id(command.Value[1]);
                    bool simplify_gender = command.Value.Length > 3 && process_bool(command.Value[3]);
                    if (Global.game_map.units.ContainsKey(id))
                    {
                        int actor_gender = Global.game_map.units[id].actor.gender % 2;
                        if (simplify_gender)
                            actor_gender %= 2;
                        result = actor_gender == process_number(command.Value[2]);
                    }
                    break;
                case "Visitor Team":
                    // Value[1] = checked team
                    if (Global.game_state.event_caller_unit != null)
                        result = Global.game_state.event_caller_unit.team == process_number(command.Value[1]);
                    break;
                // This only works pre-battle because it gets set to -1 during battle setup //Yeti
                case "Unit Fighting":
                    // Value[1] = unit id
                    id = process_unit_id(command.Value[1]);
                    if (Global.game_map.units.ContainsKey(id))
                        result = id == Global.game_system.Battler_1_Id || id == Global.game_system.Battler_2_Id;
                    break;
                // This only works pre-battle because it gets set to -1 during battle setup //Yeti
                case "Team Fighting":
                    // Value[1] = team id
                    int team_id = process_number(command.Value[1]);
                    result = (Global.game_system.Battler_1_Id != -1 && Global.game_map.units[Global.game_system.Battler_1_Id].team == team_id) ||
                        (Global.game_system.Battler_2_Id != -1 && Global.game_map.units[Global.game_system.Battler_2_Id].team == team_id);
                    break;
                case "Unit at Loc":
                    // Value[1] = x
                    // Value[2] = y
                    // Value[3] = unit id
                    unit = Global.game_map.get_unit(new Vector2(process_number(command.Value[1]), process_number(command.Value[2])));
                    result = (unit != null && unit.id == process_unit_id(command.Value[3]));
                    break;
                case "Actor at Loc":
                    // Value[1] = x
                    // Value[2] = y
                    // Value[3] = actor id
                    unit = Global.game_map.get_unit(new Vector2(process_number(command.Value[1]), process_number(command.Value[2])));
                    result = (unit != null && unit.actor.id == process_number(command.Value[3]));
                    break;
                case "Unit Area":
                    // Value[1] = x1
                    // Value[2] = y1
                    // Value[3] = x2
                    // Value[4] = y2
                    // Value[5] = unit id
                    id = process_unit_id(command.Value[5]);
                    if (Global.game_map.units.ContainsKey(id))
                        result = Global.game_map.is_unit_in_area(process_number(command.Value[1]), process_number(command.Value[2]),
                            process_number(command.Value[3]), process_number(command.Value[4]), id);
                    break;
                case "In Attack Range":
                    // Value[1] = unit id
                    id = process_unit_id(command.Value[1]);
                    if (Global.game_map.units.ContainsKey(id))
                    {
                        unit = Global.game_map.units[id];
                        var attackable = unit.get_attackable_units();
#if DEBUG
                        var in_range = attackable
                            .Where(x => Global.game_map.units[x].attack_range.Contains(
                                unit.loc))
                            .ToList();
#endif
                        result = attackable
                            .Any(x => Global.game_map.units[x].attack_range.Contains(
                                unit.loc));
                    }
                    break;
                case "Enemy in Range":
                    // Value[1] = unit id
                    id = process_unit_id(command.Value[1]);
                    if (Global.game_map.units.ContainsKey(id))
                    {
                        unit = Global.game_map.units[id];
                        var attackable = unit.get_attackable_units();
#if DEBUG
                        var in_range = attackable
                            .Where(x => unit.attack_range.Contains(
                                Global.game_map.units[x].loc))
                            .ToList();
#endif
                        result = attackable
                            .Any(x => unit.attack_range.Contains(
                                Global.game_map.units[x].loc));
                    }
                    break;
                case "Enemy in Staff Range":
                    // Value[1] = unit id
                    id = process_unit_id(command.Value[1]);
                    if (Global.game_map.units.ContainsKey(id))
                    {
                        unit = Global.game_map.units[id];
                        var attackable = unit.get_attackable_units();
#if DEBUG
                        var in_range = attackable
                            .Where(x => unit.staff_range.Contains(
                                Global.game_map.units[x].loc))
                            .ToList();
#endif
                        result = attackable
                            .Any(x => unit.staff_range.Contains(
                                Global.game_map.units[x].loc));
                    }
                    break;
                // For a given list of units, if any are dead or have a target in range
                case "Enemy Engaged":
                    // Value[1] = unit id
                    for (int i = 1; i < command.Value.Length; i++)
                    {
                        id = process_unit_id(command.Value[i]);
                        // Dead
                        if (Global.game_map.unit_defeated(id))
                        {
                            result = true;
                            break;
                        }
                        // Enemy in Range
                        if (Global.game_map.units.ContainsKey(id))
                        {
                            unit = Global.game_map.units[id];
                            var attackable = unit.get_attackable_units();
#if DEBUG
                            var in_range = attackable
                                .Where(x => unit.attack_range.Contains(
                                    Global.game_map.units[x].loc))
                                .ToList();
#endif
                            if (attackable
                                .Any(x => unit.attack_range.Contains(
                                    Global.game_map.units[x].loc)))
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                    break;
                case "Event Caller Unit Id":
                    // Value[1] = unit id
                    id = process_unit_id(command.Value[1]);
                    result = Global.game_state.event_caller_unit != null && Global.game_state.event_caller_unit.id == id;
                    break;
                case "Selected Unit Id":
                    // Value[1] = unit id
                    id = process_unit_id(command.Value[1]);
                    result = Global.game_system.Selected_Unit_Id > -1 && Global.game_system.Selected_Unit_Id == id;
                    break;
                case "Battalion":
                    // Value[1] = actor id
                    result = Global.battalion.actors.Contains(process_number(command.Value[1]));
                    break;
                case "Battalion Size":
                    // Value[1] = operator (<, >, <=, >=, ==, !=)
                    // Value[2] = size
                    int size = process_number(command.Value[2]);
                    result = operator_comparison(command.Value[1],
                        Global.battalion.actors.Count, size);
                    break;
                case "Dead":
                    // Value[1] = id is for a unit, or for an actor?
                    // Value[2] = id
                    if (process_bool(command.Value[1]))
                    {
                        int unit_id = process_unit_id(command.Value[2]);
                        result = Global.game_map.unit_defeated(unit_id);
                    }
                    else
                        result = Global.game_map.actor_defeated(process_number(command.Value[2]));
                    break;
                #endregion
                #region Deployment
                case "Undeployed Count":
                    // Value[1] = is battalion at least as large as this number?
                    //?Value[2] = also count immobile units?
                    bool immobile = command.Value.Length >= 2 && process_bool(command.Value[1]);
                    if (process_number(command.Value[1]) > 0)
                        result = Global.battalion.undeployed_actor(process_number(command.Value[1]) - 1, include_immobile: immobile) != -1;
                    else
                    {
#if DEBUG
                        Print.message(string.Format("Event \"{0}\" checking if the battalion has at least {1} units, which is always true",
                            event_data.name, process_number(command.Value[1])));
#endif
                        result = true;
                    }
                    break;
                case "Any Deployment Saved":
                    result = Global.battalion.deployed_but_not_on_map.Count > 0;
                    break;
                case "PC Deployment Saved":
                    // Value[1] = actor id
                    result = Global.battalion.deployed_but_not_on_map.Contains(process_number(command.Value[1]));
                    break;
                #endregion
                #region Visiting
                case "Seized":
                    // Value[1] = x
                    // Value[2] = y
                    result = Global.game_map.seized_points.Contains(new Vector2(process_number(command.Value[1]), process_number(command.Value[2])));
                    break;
                case "Visit Exists":
                    // Value[1] = x
                    // Value[2] = y
                    result = Global.game_map.visit_locations.ContainsKey(new Vector2(process_number(command.Value[1]), process_number(command.Value[2])));
                    break;
                case "Chest Exists":
                    // Value[1] = x
                    // Value[2] = y
                    result = Global.game_map.chest_locations.ContainsKey(new Vector2(process_number(command.Value[1]), process_number(command.Value[2])));
                    break;
                case "Door Exists":
                    // Value[1] = x
                    // Value[2] = y
                    result = Global.game_map.door_locations.ContainsKey(new Vector2(process_number(command.Value[1]), process_number(command.Value[2])));
                    break;
                case "Destroyable Exists":
                    // Value[1] = x
                    // Value[2] = y
                    result = Global.game_map.get_destroyable(new Vector2(process_number(command.Value[1]), process_number(command.Value[2]))) != null;
                    break;
                case "Actor Escaped":
                    // Value[1] = actor id
                    result = Global.game_map.actor_escaped(process_number(command.Value[1]));
                    break;
                case "Unit Escaped":
                    // Value[1] = unit id
                    result = Global.game_map.unit_escaped(process_unit_id(command.Value[1]));
                    break;
                #endregion
                case "TeamSmall":
                    // Value[1] = team id
                    // Value[2] = size, true if less than or equal
#if DEBUG
                    Print.message(
                        "Event if statement \"TeamSmall\" is deprecated,\n" +
                        "use \"Team Size\" instead.");
#endif
                    result = Global.game_map.teams[process_number(command.Value[1])].Count <= process_number(command.Value[2]);
                    break;
                case "Team Size":
                    // Value[1] = team id
                    // Value[2] = operator (<, >, <=, >=, ==, !=)
                    // Value[3] = size
                    team_id = process_number(command.Value[1]);
                    size = process_number(command.Value[3]);
                    result = operator_comparison(command.Value[2],
                        Global.game_map.teams[team_id].Count, size);
                    break;
                case "Actor Hp":
                    // Value[1] = actor id
                    // Value[2] = operator (<, >, <=, >=, ==, !=)
                    // Value[3] = value
                    get_actor("false", command.Value[1], out actor);
                    if (actor != null)
                    {
                        int hp = process_number(command.Value[3]);
                        result = operator_comparison(command.Value[2], actor.hp, hp);
                    }
                    break;
                case "Actor Full Health":
                    // Value[1] = actor id
                    get_actor("false", command.Value[1], out actor);
                    if (actor != null)
                        result = actor.is_full_hp();
                    break;
                case "Loss":
                    result = Global.game_system.is_loss();
                    break;
                case "HardMode":
                    result = Global.game_system.hard_mode;
                    break;
                #region Chapter Specific
                case "Ch7MaddyTalkChain":
                    // If Maddy or Eiry are't deployed, we're on Eagler
                    if (!Global.game_map.is_actor_deployed(6) || !Global.game_map.is_actor_deployed(11))
                        return false;
                    // And the inverse, if no Eagler or Marcus
                    if (!Global.game_map.is_actor_deployed(5) || !Global.game_map.is_actor_deployed(2))
                        return true;

                    int maddy_count = 0, eagler_count = 0;
                    if (Global.game_map.is_actor_deployed(11)) // Eiry
                        maddy_count++;
                    if (Global.game_map.is_actor_deployed(8)) // Hassar
                        maddy_count++;
                    if (Global.game_map.is_actor_deployed(13)) // Leonard
                        maddy_count++;
                    
                    if (Global.game_map.is_actor_deployed(2)) // Marcus
                        eagler_count++;
                    if (Global.game_map.is_actor_deployed(4)) // Isadora
                        eagler_count++;
                    if (Global.game_map.is_actor_deployed(3)) // Harken
                        eagler_count++;

                    result = maddy_count > eagler_count;
                    break;
                case "Tr3Best":
                    if (Global.game_actors[86].is_dead() || Global.game_actors[87].is_dead() || Global.game_actors[88].is_dead())
                    {
                        result = false;
                        break;
                    }
                    bool unit_dead = false;
                    foreach (int tr3_id in new List<int> { 41, 6, 8, 42, 43, 44, 45, 51, 52, 5, 7, 11, 15, 17, 107, 78 })
                        if (Global.game_actors[tr3_id].is_dead())
                        {
                            unit_dead = true;
                            break;
                        }
                    result = !unit_dead;
                    break;
                case "Tr3Average":
                    if (Global.game_actors[86].is_dead() || Global.game_actors[87].is_dead() || Global.game_actors[88].is_dead())
                    {
                        result = false;
                        break;
                    }
                    int dead = 0;
                    foreach (int tr3_id in new List<int> { 41, 6, 8, 42, 43, 44, 45, 51, 52, 5, 7, 11, 15, 17, 107, 78 })
                        if (Global.game_actors[tr3_id].is_dead())
                            dead++;
                    result = dead < 3;
                    break;
                #endregion
                default:
#if DEBUG
                    Print.message(string.Format("Unknown if statement \"{0}\" used in event \"{1}\"", if_key[0], event_data.name));
                    //Print.message("Unknown if statement \"" + if_key[0] + "\" used in event \"" + event_data.name + "\""); //Debug
#endif
                    break;
            }
            return result;
        }

        private bool operator_comparison(string operator_str, int value1, int value2)
        {
            switch (operator_str)
            {
                case "<":
                    return value1 < value2;
                case "<=":
                    return value1 <= value2;
                case ">":
                    return value1 > value2;
                case ">=":
                    return value1 >= value2;
                case "==":
                    return value1 == value2;
                case "!=":
                    return value1 != value2;
                default:
#if DEBUG
                    Print.message(string.Format(
                        "Invalid operator \"{0}\".\n" +
                        "Valid operators are [<, >, <=, >=, ==, !=]"), operator_str);
#endif
                    return false;
            }
        }

        // Else statement, ElseIf statement
        private bool command_else()
        {
            // Find the if block this control is connected to
            if (IndentBlocks.Any(x => x.Value.HasIntermediateIndex(Index)))
            {
                var ifBlock = IndentBlocks.First(x => x.Value.HasIntermediateIndex(Index)).Value;
                // Go to the end of the if block
                // If there is no matching block end, go to the end of the event
                if (ifBlock.EndControlIndex == -1)
                    Index = event_data.data.Count;
                else
                    Index = ifBlock.EndControlIndex + 1;
                return true;
            }

            // If no endif is found, break out of this event I suppose
            Index = event_data.data.Count;
            return true;
        }

        // Skip block start
        private bool command_skip()
        {
            Skip_Block++;
            Index++;
            return true;
        }

        // Skip block normal process end
        private bool command_skip_else()
        {
            // If this command runs, it means a skip block was completed without the player skipping it
            // Therefore, return to normal non-skip processing by jumping to after the first skip block end found
            //int indent = Indent[Index]; //Debug
            decrement_skip_block();
            Index++;

            // Finds the first skip complete statement //with the same indentation
            int skip_level = 0;
            while (Index < event_data.data.Count)
            {
                //if (Indent[Index] == indent && event_data.data[Index].Key == 213) //Debug
                if (event_data.data[Index].Key == 211)
                    skip_level++;
                else if (event_data.data[Index].Key == 213)
                {
                    if (skip_level > 0)
                        skip_level--;
                    else
                        break;
                }
                else
                    Index++;
            }
            Index++;
            return true;
        }

        // Skip block end
        private void command_skip_end()
        {
            //?Value[0] = If skipped and has parent event, end this event and skip parent event (optional)
            //?Value[1] = If ending this event to skip parent, delete this event (optional)

            if (!Skipping)
                return;

            if (command.Value.Length >= 1 && this.has_parent)
            {
                // Skip parent event
                if (process_bool(command.Value[0]))
                {
                    Global.game_system.skip_parent_event(Id);

                    // Delete this event
                    if (command.Value.Length >= 2 && process_bool(command.Value[1]))
                    {
                        command_delete();
                    }
                    Index = event_data.data.Count;
                }
            }
        }

        // Skip block override end
        private bool command_skip_override_end()
        {
            Index++;
            if (SkipOverride)
            {
                SkipOverrideEnd = true;
                SkipOverride = false;
                return false;
            }
            return true;
        }

        // 216: Skip cancel
        private bool command_skip_cancel()
        {
            if (Skipping)
            {
                StartSkip = false;
                Skipping = false;
                SkipOverride = false;
                SkipElseBlock = -1;
            }

            Index++;
            return true;
        }

        // 222: Loop end
        private bool command_loop_end()
        {
            // Find the loop block this end is connected to
            if (IndentBlocks.Any(x => x.Value.EndControlIndex == Index))
            {
                var loopBlock = IndentBlocks.First(x => x.Value.EndControlIndex == Index).Value;
                // Go back to the start of the loop
                Index = loopBlock.StartControlIndex + 1;
                return true;
            }

            Index++;
            return true;
        }

        // 223: Loop break
        private bool command_loop_break()
        {
            int indent = Indent[Index];
            // Work forwards from here to find the end of the active loop, if there is one
            //@Debug: this would break down if loops had an intermediate control, so... don't do that
            for (int i = Index + 1; i < event_data.data.Count; i++)
            {
                // If the indentation of this line is less than the current one,
                // this is the end of a block to check
                if (Indent[i] < indent)
                {
                    // If this is a loop end
                    if (event_data.data[i].Key == 222)
                    {
                        Index = i + 1;
                        return true;
                    }
                    // Else, reduce indent and keep checking
                    else
                        indent = Indent[i];
                }
            }

            // If no loop start is found, break out of this event I suppose
            Index = event_data.data.Count;
            return true;
        }

        // 224: Loop next
        private bool command_loop_next()
        {
            int indent = Indent[Index];
            // Work backwards from here to find the active loop, if there is one
            //@Debug: this would break down if loops had an intermediate control, so... don't do that
            for (int i = Index - 1; i >= 0; i--)
            {
                // If the indentation of this line is less than the current one,
                // this is the start of a new block to check
                if (Indent[i] < indent)
                {
                    // If this is a loop start
                    if (event_data.data[i].Key == 221)
                    {
                        Index = i + 1;
                        return true;
                    }
                    // Else, reduce indent and keep checking
                    else
                        indent = Indent[i];
                }
            }

            // If no loop start is found, ignore this statement I suppose
            Index++;
            return true;
        }

        // Delete
        private bool command_delete()
        {
            if (!this.has_parent && waiting_for_move())
                return false;
            if (Global.scene.is_map_scene)
                Global.game_state.event_handler.remove(event_data.name);
            Index = event_data.data.Count;
            return true;
        }

        // Cancel Other Events
        private bool command_cancel_events()
        {
            Global.game_system.cancel_further_events();
            Index++;
            return true;
        }

        // Call Event
        private bool command_call_event()
        {
            // Value[0] = Event name
            // Value[1] = run event now and not after this one finishes?
            bool insert = process_bool(command.Value[1]);
            if (Global.game_system.call_event(command.Value[0], insert) && insert)
            {
                Global.game_system.set_event_parent_id(Id);
            }
            Index++;
            return false;
        }
    }
}
