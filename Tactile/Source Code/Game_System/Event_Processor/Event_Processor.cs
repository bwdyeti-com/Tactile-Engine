using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TactileVersionExtension;

namespace Tactile
{
    partial class Event_Processor
    {
        // Commands that don't break out of skipping the AI turn
        readonly static int[] NO_ACTION_COMMANDS = new int[] { 1, 5, 8, 14, 15, 16, 22, 24, 25, 26, 27, 28, 29, 30, 32, 33, 34, 36, 43, 44, 46,
            52, 53, 54, 55, 56, 57, 58, 59, 61, 62, 64, 66, 81, 83, 84, 85, 86, 91, 92, 93, 94, 95, 96, 97, 98,
            101, 102, 103, 111, 112, 121, 122, 123, 129, 130, 172, 201, 202, 203, 204, 205, 211, 212, 213, 214, 215, 250, 252 };

#if DEBUG
        //@Debug: Example variable names from 7x:
        internal readonly static Dictionary<int, string> SWITCH_NAMES = new Dictionary<int, string>
        {
            { 97, "Ch4x was played" },
            { 98, "-NPC- talked to in Ch10" },
            { 99, "Ch7x was played" },
        };
        internal readonly static Dictionary<int, string> VARIABLE_NAMES = new Dictionary<int, string>
        {
        };

        internal static string switch_name(int index)
        {
            if (SWITCH_NAMES.ContainsKey(index))
                return SWITCH_NAMES[index];
            return string.Format("Switch({0:00})", index);
        }
        internal static string variable_name(int index)
        {
            if (VARIABLE_NAMES.ContainsKey(index))
                return VARIABLE_NAMES[index];
            return string.Format("Var({0:00})", index);
        }
#endif

        private int Id, Parent_Id = -1, Index;

        private string Key;
        private bool Finished = false;
        private int Wait_Time = 0;
        private TactileLibrary.Event_Data event_data;
        private List<int> Indent = new List<int>();
        private int Skip_Block = 0, SkipElseBlock = -1;
        private bool Skipping = false, SkipOverride = false, SkipOverrideEnd = false;
        private bool StartSkip = false;
        private bool Skip_Transition_Start = false; // Doesn't do anything? //Debug
        private bool Unit_Moved = false, Fow_Unit_Moved = false;
        private bool Battle_Wait = false, Rescue_Wait = false;
        private bool Ignore_Move_Update = false;
        private HashSet<int> Skip_ElseIf_Indents = new HashSet<int>();

        #region Serialization
        /*internal static void get_data(Event_Variable_Data<bool> switches, Event_Variable_Data<int> variables) //Debug
        {
            Event_Variable_Data<bool>.copy(SWITCHES, switches);
            Event_Variable_Data<int>.copy(VARIABLES, variables);
        }

        internal static void set_data(Event_Variable_Data<bool> switches, Event_Variable_Data<int> variables)
        {
            Event_Variable_Data<bool>.copy(switches, SWITCHES);
            Event_Variable_Data<int>.copy(variables, VARIABLES);
        }*/

        public void write(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Parent_Id);
            writer.Write(Index);
        }

        public static Event_Processor read(BinaryReader reader)
        {
            Event_Processor processor = new Event_Processor(reader.ReadInt32());
            if (!Global.LOADED_VERSION.older_than(0, 4, 5, 0))
                processor.Parent_Id = reader.ReadInt32();
            processor.Index = reader.ReadInt32();
            return processor;
        }
        #endregion

        #region Accessors
        static Event_Variable_Data<bool> SWITCHES
        {
            get { return Global.game_system.SWITCHES; }
            set { Global.game_system.SWITCHES = value; }
        }
        static Event_Variable_Data<int> VARIABLES
        {
            get { return Global.game_system.VARIABLES; }
            set { Global.game_system.VARIABLES = value; }
        }

        public int id { get { return Id; } }

        internal int parent_id
        {
            get { return Parent_Id; }
            set { Parent_Id = value; }
        }

        private bool has_parent { get { return Parent_Id != -1; } }

        public string key { get { return Key; } }

        public bool finished
        {
            get { return Finished && Wait_Time == 0; }
        }

        internal string name { get { return event_data.name; } }

        private TactileLibrary.Event_Control command { get { return event_data.data[Index]; } }

        private bool unit_moved
        {
            set
            {
                Unit_Moved = value;
                Fow_Unit_Moved = value;
            }
        }

        public bool battle_wait { get { return Battle_Wait; } }
        public bool rescue_wait { get { return Rescue_Wait; } }

        // I don't know why this would let preparations get control again just because an event was waiting, but there may be a reason
        // Might have something to do with command_chapter(), since that sets wait_time to 1 at one point
        // public bool waiting_for_prep { get { return Wait_Time > 0 || (Index >= event_data.data.Count ? false : command.Key == 128); } } //Debug
        public bool waiting_for_prep { get { return Index >= event_data.data.Count ? false : command.Key == 128; } }
        #endregion

        public override string ToString()
        {
            return string.Format("Event Processor: {0}, on command index {1}", name, Index);
        }

        internal static bool get_switch(int id)
        {
            return SWITCHES[id];
        }

        public static void reset_variables()
        {
        }

        public Event_Processor(int id)
        {
            Id = id;
            event_data = (Id == -1 ? null : Global.game_state.event_handler.event_data.Events[Id]);
            Key = Global.game_state.event_handler.name + Id;
            initialize();
        }
        public Event_Processor(TactileLibrary.Event_Data data)
        {
            Id = -1;
            event_data = data;
            Key = "";
            initialize();
        }

        private void initialize()
        {
            Index = 0;
            int indent = 0;
            bool if_control = false;
            // Sets up indentation for conditional blocks
            for (int i = 0; i < event_data.data.Count; i++)
            {
                TactileLibrary.Event_Control control = event_data.data[i];
                // Skip comments
                if (control.Key == 102)
                {
                    Indent.Add(indent);
                    continue;
                }

                // If statement (or), Else statement, ElseIf
                if (if_control && (control.Key == 202 || control.Key == 203 || control.Key == 205))
                    Indent.Add(indent - 1);
                // EndIf statement
                else if (if_control && control.Key == 204)
                {
                    indent--;
                    Indent.Add(indent);
                    if (indent == 0)
                        if_control = false;
                }
                // Otherwise
                else
                {
                    Indent.Add(indent);
                    // If statement
                    if (control.Key == 201)
                    {
                        if_control = true;
                        indent++;
                    }
                }
            }
        }

        private bool is_debug_event()
        {
            if (name == "Debug Playtest")
                return true;
            return false;
        }

        public void update()
        {
#if !DEBUG
            // Don't run debug event in Release
            if (is_debug_event())
            {
                Index = event_data.data.Count;
                Finished = true;
                return;
            }
#endif

            if (Global.scene.is_strict_map_scene && ((Scene_Map)Global.scene).is_changing_chapters)
                return;

            do
            {
                update_skip();
                if (Wait_Time > 0)
                {
                    //Global.game_map.wait_time = 2;
                    Wait_Time--;
                    return;
                }

                if (this.continue_event)
                {
                    Battle_Wait = false;
                    Rescue_Wait = false;
                    while (process_command()) { }
                    if (!SkipOverride && !SkipOverrideEnd)
                        Skipping = false;
                    Skip_Transition_Start = false;
                }
            } while (SkipOverrideEnd);
        }

        private bool continue_event
        {
            get
            {
                bool ignore_move_update = Ignore_Move_Update;
                Ignore_Move_Update = false;
                // If there is no active message window
                if (Global.scene.is_message_window_active)
                    return false;
                // If the scene is not a map
                if (!Global.scene.is_map_scene)
                    return true;
                // all this bunch of stuff
                if (Global.game_state.switching_ai_skip_counter <= 0 &&
                        (!Global.scene.is_strict_map_scene || !((Scene_Map)Global.scene).is_map_popup_active()))
                    // If units don't need their move ranges updated; ignored once when the previous event ended earlier this frame 
                    if (!Global.game_map.move_ranges_need_update || ignore_move_update)
                        // If there are no menus open, or it's perparations and the game is ready to run events during preparations and no one is discarding
                        if (!Global.game_state.is_menuing ||
                                (Global.game_system.preparations && Global.game_system.Preparation_Events_Ready && !Global.game_temp.discard_menuing))
                            if (!Global.scene.suspend_calling &&
                                (!Battle_Wait || !Global.game_state.combat_active) &&
                                (!Rescue_Wait || !Global.game_state.rescue_active) &&
                                !Global.game_state.chapter_end_active &&
                                !Global.game_state.exp_active)
                            {
                                return true;
                            }
                return false;
            }
        }

        private bool process_command()
        {
            if (event_data == null || Index >= event_data.data.Count)
            {
                if (!Global.map_exists || !Global.game_map.units_dying)
                    Finished = true;
                return false;
            }
            if (!NO_ACTION_COMMANDS.Contains(command.Key) && Global.scene.is_map_scene)
            {
                if (try_cancel_ai_skip())
                    return false;
            }
            switch (command.Key)
            {
                // Prep Message
                case 1:
                    return command_prepmessage();
                // Run Message
                case 2:
                    return command_runmessage();
                // Wait
                case 3:
                    return command_wait();
                // Change Chapter
                case 4:
                    return command_chapter();
                // Change screen tone
                case 5:
                    return command_tone();
                // Warp Player
                case 6:
                    return command_player_warp();
                // Close Message
                case 7:
                    throw new NotImplementedException();
                    Index++;
                    return true;
                // Follow Moving Unit
                case 8:
                    return command_follow_unit();
                // Warp Unit
                case 10:
                    return command_unit_warp();
                // Move Unit
                case 11:
                    return command_unit_move();
                // Wait for Move
                case 12:
                    return command_wait_for_move();
                // Chapter Change Effect
                case 13:
                    return command_chapter_change();
                // Add Battle Convo
                case 14:
                    return command_battle_convo();
                // Temp Death Quote
                case 15:
                    return command_death_quote();
                // Change Gold
                case 16:
                    return command_change_gold();
                // Set Screen Color
                case 17:
                    return command_screen_color();
                // Show Popup
                case 18:
                    return command_popup();
                // Show Gold Gain Popup
                case 19:
                    return command_gold_gain_popup();
                // Unit Battle Theme
                case 20:
                    return command_unit_theme();
                // Add Unit
                case 21:
                    return command_add_unit();
                // Change Mission
                case 22:
                    return command_change_mission();
                // Change Team
                case 23:
                    return command_change_team();
                // Change Group
                case 24:
                    return command_change_group();
                // Change Team Name
                case 25:
                    return command_change_team_name();
                // Set Boss
                case 26:
                    return command_set_boss();
                // Set Drops
                case 27:
                    return command_set_drops();
                // Add Talk Event
                case 28:
                    return command_add_talk();
                // Add Deployment Point
                case 29:
                    return command_add_deployment();
                // Add to Battalion
                case 30:
                    return command_add_to_battalion();
                // Target Tile
                case 31:
                    return command_target_tile();
                // Force Deployment
                case 32:
                    return command_force_deployment();
                // Set Convoy
                case 33:
                    return command_set_convoy();
                // Convoy Item Gain
                case 34:
                    return command_convoy_item_gain();
                // Remove Unit
                case 35:
                    return command_remove_unit();
                // Remove Talk Event
                case 36:
                    return command_remove_talk();
                // Set FoW
                case 37:
                    return command_set_fow();
                // Set FoW Vision
                case 38:
                    return command_fow_vision();
                // Set Weather
                case 39:
                    return command_set_weather();
                // FoW Light Source
                case 40:
                    return command_fow_light_source();
                // Change Tile Id
                case 41:
                    return command_change_tile();
                // Pillage Tile
                case 42:
                    return command_pillage_tile();
                // Add Area Background
                case 43:
                    return command_area_background();
                // Add Destroyable Object
                case 44:
                    return command_add_destroyable();
                // Import Map Area
                case 45:
                    return command_import_map_area();
                // Add Siege Engine
                case 46:
                    return command_add_siege();
                // Edit Tile Outlines
                case 47:
                    return command_edit_tile_outlines();
                // AI Target Map Object
                case 48:
                    return command_ai_target_map_object();
                // Change class
                case 51:
                    return command_class_change();
                // Exp Gain
                case 52:
                    return command_exp();
                // Item Gain
                case 53:
                    return command_item_gain();
                // Discard Item
                case 54:
                    return command_item_discard();
                // WExp Gain
                case 55:
                    return command_wexp();
                // Support
                case 56:
                    return command_support();
                // Boss Hard Mode Stats
                case 57:
                    return command_boss_hard_mode();
                // Block Support
                case 58:
                    return command_block_support();
                // Change Status
                case 59:
                    return command_change_status();
                // Heal Actors
                case 61:
                    return command_heal_actors();
                // Set Unit Ready
                case 62:
                    return command_unit_ready();
                // Set Map Edge Offsets
                case 63:
                    return command_map_edge();
                // Unload Actor
                case 64:
                    return command_unload_actor();
                // Rescue
                case 65:
                    return command_rescue();
                // Change Actor Name
                case 66:
                    return command_change_name();
                // Set Min Alpha
                case 71:
                    return command_min_alpha();
                // Refresh Alpha
                case 72:
                    return command_refresh_alpha();
                // Add Alpha Source
                case 73:
                    return command_add_alpha();
                // Clear Alpha Sources
                case 74:
                    return command_clear_alpha();
                // Set Ally Alpha
                case 75:
                    return command_ally_alpha();
                // Blacken Screen
                case 76:
                    return command_blacken_screen();
                // Add Visit Location
                case 81:
                    return command_add_visit();
                // Remove Visit Location
                case 82:
                    return command_remove_visit();
                // Add Shop Location
                case 83:
                    return command_add_shop();
                // Add Arena Location
                case 86:
                    return command_add_arena();
                // Add Escape Point
                case 91:
                    return command_add_escape();
                // Add Seize Point
                case 92:
                    return command_add_seize();
                // Add Defend Area
                case 93:
                    return command_add_defend();
                // Add Unit Seek Location
                case 94:
                    return command_add_unit_seek();
                // Add Team Seek Location
                case 95:
                    return command_add_team_seek();
                // Change Objective
                case 96:
                    return command_change_objective();
                // Set Team Leader
                case 97:
                    return command_set_leader();
                // Loss On Death
                case 98:
                    return command_loss_on_death();
                // Debug print
                case 101:
#if DEBUG
                    Print.message(string.Join("\n", command.Value), "Event Debug");
#endif
                    Index++;
                    return true;
                // ASMC
                case 103:
                    return command_custom();
                // Set Switch
                case 111:
                    return command_set_switch();
                // Set Variable
                case 112:
                    return command_set_variable();
                // Return to Title
                case 121:
                    return command_title();
                // Game Over
                case 122:
                    return command_gameover();
                // Return to World Map
                case 123:
                    return command_worldmap();
                // Preparations
                case 124:
                    return command_preparations();
                // Map Save
                case 125:
                    return command_map_save();
                // End Chapter
                case 126:
                    return command_end_chapter();
                // Home Base
                case 127:
                    return command_home_base();
                // Wait for Preparations
                case 128:
                    return command_wait_for_prep();
                // Add Base Event
                case 129:
                    return command_add_base_event();
                // Gain Completion Points
                case 130:
                    return command_gain_completion_points();
                // Play BGM
                case 131:
                    return command_play_bgm();
                // Fade BGM
                case 132:
                    return command_fade_bgm();
                // Play SFX
                case 133:
                    return command_play_sfx();
                // Play BGS
                case 134:
                    return command_play_bgs();
                // Stop BGS
                case 135:
                    return command_stop_bgs();
                // Fade SFX
                case 136:
                    return command_fade_sfx();
                // Stop SFX
                case 137:
                    return command_stop_sfx();
                // Duck BGM
                case 138:
                    return command_duck_bgm();
                // Center Worldmap Camera
                case 141:
                    return command_center_worldmap();
                // Worldmap Dot
                case 142:
                    return command_worldmap_dot();
                // Worldmap Arrow
                case 143:
                    return command_worldmap_arrow();
                // Worldmap Remove Dot
                case 144:
                    return command_worldmap_remove_dot();
                // Worldmap Beacon
                case 145:
                    return command_worldmap_beacon();
                // Worldmap Remove Beacon
                case 146:
                    return command_worldmap_remove_beacon();
                // Worldmap Zoomed Out
                case 147:
                    return command_worldmap_zoomed_out();
                // Worldmap Unit
                case 151:
                    return command_worldmap_unit();
                // Wmap Queue Unit Move
                case 152:
                    return command_worldmap_queue_unit_move();
                // Wmap Queue Unit Idle
                case 153:
                    return command_worldmap_queue_unit_idle();
                // Wmap Queue Unit Pose
                case 154:
                    return command_worldmap_queue_unit_pose();
                // Wmap Queue Unit Remove
                case 155:
                    return command_worldmap_queue_unit_remove();
                // Wmap Clear Removing
                case 156:
                    return command_worldmap_clear_removing();
                // Wmap Queue Track Unit
                case 157:
                    return command_worldmap_queue_unit_tracking();
                // Wmap Wait for Units
                case 158:
                    return command_worldmap_wait_for_unit_move();
                // Preset RNs
                case 171:
                    return command_preset_rns();
                // Scripted Battle
                case 172:
                    return command_scripted_battle();
                // Scripted Battle Params
                case 173:
                    return command_scripted_battle_params();
                // If statement
                case 201:
                    return command_if();
                // If statement (or) // This should never come up on its own
                case 202:
                    Index++;
                    return true;
                // Else statement, ElseIf statement
                case 203:
                case 205:
                    return command_else();
                // EndIf statement // This does nothing on its own
                case 204:
                    Index++;
                    return true;
                // Skip block start
                case 211:
                    if (SkipOverride)
                        throw new InvalidOperationException(string.Format(
                            "Encountered a Skip Block Start while already in a Skip Override block, at line {1} of event {0}",
                            event_data.name, Index));
                    if (SkipElseBlock >= Skip_Block)
                        throw new InvalidOperationException(string.Format(
                            "Starting a skip block within a skip else is not allowed, at line {1} of event {0}",
                            event_data.name, Index));
                    return command_skip();
                // Skip block if skipped
                case 212:
                    if (SkipOverride)
                        throw new InvalidOperationException(string.Format(
                            "Encountered a Skip Block Else while already in a Skip Override block, at line {1} of event {0}",
                            event_data.name, Index));
                    return command_skip_else();
                // Skip block end
                case 213:
                    if (SkipOverride)
                        throw new InvalidOperationException(string.Format(
                            "Encountered a Skip Block End while already in a Skip Override block, at line {1} of event {0}",
                            event_data.name, Index));
                    // This command should only be hit at the end of a skip else, when the else should end
                    // Or at the end of an unskipped block that has no else
                    decrement_skip_block();
                    Skipping = false;
                    Index++;
                    return true;
                // Skip override start // This does nothing on its own
                case 214:
                    if (SkipOverride)
                        throw new InvalidOperationException(string.Format(
                            "Encountered a Skip Override Start while already in a Skip Override block, at line {1} of event {0}",
                            event_data.name, Index));
                    Index++;
                    return true;
                // Skip override end
                case 215:
                    return command_skip_override_end();
                // Delete Event
                case 250:
                    return command_delete();
                // Cancel Other Events
                case 251:
                    return command_cancel_events();
                // Call Event
                case 252:
                    return command_call_event();
                default:
                    Index++;
                    return true;
            }
        }

        private bool try_cancel_ai_skip()
        {
            if (!Global.game_state.skip_ai_turn_activating)
                return false;

            switch (Global.game_state.skip_ai_state)
            {
                case State.Ai_Turn_Skip_State.SkipStart:
                    Global.game_state.cancel_ai_skip();
                    break;
                case State.Ai_Turn_Skip_State.Skipping:
                case State.Ai_Turn_Skip_State.SkipEnd:
                    Global.game_state.switch_out_of_ai_skip();
                    break;
            }
            return true;
        }

        internal void start_skip()
        {
            StartSkip = true;
        }

        private void update_skip()
        {
            bool started_skipping = false;
            // If not already skipping, and inside a skip block
            if (!Skipping && Skip_Block > 0 && Skip_Block > SkipElseBlock)
            {
                if (!Global.map_exists || !Global.game_state.combat_active)
                {
                    Skipping = StartSkip ||
                        Global.scene.message_window_skipping_event ||
                        (Global.Input.triggered(Inputs.Start) &&
                            !Global.scene.is_message_window_active);
                    started_skipping = Skipping;
                }
            }
            else if (SkipOverrideEnd)
            {
                SkipOverrideEnd = false;
                Skipping = true;
                started_skipping = true;
            }
            StartSkip = false;

            // Player pressed start
            if (started_skipping)
            {
                // Cancel player target tile
                if (Global.player != null)
                    Global.player.cancel_target_tile();

                Skip_Transition_Start = Global.scene.message_background;
                if (Skip_Transition_Start)
                {

                }
                // Finds the first skip else statement
                bool break_loop = false;
                int skip_indent = Skip_Block;
                while (Index < event_data.data.Count)
                {
                    switch (event_data.data[Index].Key)
                    {
                        case 211:
                            Skip_Block++;
                            break;
                        // Skip Block Else
                        case 212:
                            if (Skip_Block == skip_indent)
                            {
                                break_loop = true;
                                SkipElseBlock = Skip_Block;
                            }
                            break;
                        // Skip Block End
                        case 213:
                            if (Skip_Block == skip_indent)
                            {
                                break_loop = true;
                                command_skip_end();
                            }
                            decrement_skip_block();
                            break;
                        // Skip Override Start
                        case 214:
                            if (Skip_Block == skip_indent)
                            {
                                SkipOverride = true;
                                break_loop = true;
                            }
                            break;
                        // Skip Override End
                        case 215:
                            if (Skip_Block == skip_indent)
                                throw new InvalidOperationException(string.Format(
                                    "While skipping event \"{0}\", at line {1} encountered a Skip Override End (command 215) without a preceding Skip Override Start.",
                                    event_data.name, Index));
                            break;
                        default:
                            Index++;
                            break;
                    }
                    if (break_loop)
                        break;
                }
                if (Index < event_data.data.Count)
                    Index++;
                // Tell the scene the event was skipped, closing the message window
                // This also skips to loading the chapter when world map events are skipped
                Global.scene.event_skip();
                Wait_Time = 0;
            }
        }

        private void decrement_skip_block()
        {
            if (Skip_Block == SkipElseBlock)
                SkipElseBlock = -1;
            Skip_Block--;
        }

        public void ignore_move_update()
        {
            Ignore_Move_Update = true;
        }

        public void end(Event_Processor parent = null)
        {
            if (Global.scene.is_map_scene)
                Global.game_state.event_handler.event_completed(name);

            if (parent == null)
            {
                if (Unit_Moved)
                {
                    Global.game_map.refresh_move_ranges(true);
                }
            }
            else
                parent.Unit_Moved |= Unit_Moved;
        }

#if DEBUG
        internal string event_command_string(int offset)
        {
            if (Index + offset >= event_data.data.Count || Index + offset < 0)
                return "-----";
            var event_command = event_data.data[Index + offset];

            string eventString;
            // If comment
            if (event_command.Key == 102)
                eventString = "// " + event_command.Value.FirstOrDefault();
            else
                eventString = string.Format("{0}: {1}", event_command.Key, event_control_name(event_command.Key));
            switch (event_command.Key)
            {
                // Wait
                case 3:
                    if (offset == -1)
                        eventString += string.Format(" {0}", Wait_Time);
                    else if (offset >= 0)
                        eventString += string.Format(" {0}", process_number(event_command.Value[0]));
                    break;
            }

            return string.Format("({0})  {1}{2}",
                (Index + offset).ToString("D4"),
                new string(' ', Indent[Index + offset] * 4),
                eventString);
        }

        private string event_control_name(int key)
        {
            if (!Event_Control_Names.CONTROLS.ContainsKey(key))
                throw new KeyNotFoundException(string.Format("No event control name for event code {0}", key));
            return Event_Control_Names.CONTROLS[key];
        }
#endif
    }
}
