using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using FEXNA.Metrics;
using FEXNA_Library;
using ListExtension;
using FEXNADictionaryExtension;
using FEXNAListExtension;
using FEXNAVector2Extension;
using FEXNAVersionExtension;

namespace FEXNA
{
    partial class Game_State
    {
        private string Chapter_Id;
        private int Team_Turn = 0, Turn = 0, Previous_Turn = 0;
        private int Turn_Change = -1;
        private bool Changing_Turn = false;
        private bool Turn_End_Prompt = false;
        private string Turn_Theme = "";
        private bool Near_Victory = false;
        private int Skipped_Turn_Action = 0;
        private List<int> Skipped_Turns = new List<int>();
        private bool Same_Team_Turn = false;
        private bool AutoTurnEndBlocked = false;
        private Vector2 Temp_Player_Loc = Vector2.Zero;
        internal int No_Input_Timer = 15; // private //Yeti
        private int Weather = 0;
        private List<Talk_Event> Talk_Events = new List<Talk_Event>();
        private bool Supports_Blocked = false;
        private List<int[]> Blocked_Supports = new List<int[]>();
        private Tone Target_Screen_Tone = new Tone(0, 0, 0, 0);
        private List<Battle_Convo> Battle_Convos = new List<Battle_Convo>();
        private Dictionary<int, string> Death_Quotes = new Dictionary<int, string>();
        private Dictionary<int, bool> Casual_Death_Quote_Blocked = new Dictionary<int, bool>();
        internal Dictionary<int, string> Unit_Battle_Themes = new Dictionary<int, string>(); //private //Yeti
        private List<Home_Base_Event_Data> Home_Base_Events = new List<Home_Base_Event_Data>();
        private Gameplay_Metrics Metrics;

        internal bool Update_Victory_Theme = false; //private //Yeti
        private Tone Screen_Tone = new Tone(0, 0, 0, 0), Source_Screen_Tone = new Tone(0, 0, 0, 0);
        private int Tone_Timer;
        internal int Tone_Time_Max { get; private set; } //private //Yeti
        public Vector2? prev_player_loc = null;
#if !MONOGAME && DEBUG
        internal bool Moving_Editor_Unit = false; //private //Yeti
#endif

        #region Serialization
        public void write_map_stuff(BinaryWriter writer)
        {
            writer.Write(Chapter_Id);

            writer.Write(Team_Turn);
            writer.Write(Turn);
            writer.Write(Previous_Turn);
            writer.Write(Turn_Change);
            writer.Write(Changing_Turn);
            writer.Write(Turn_End_Prompt);
            writer.Write(Turn_Theme);
            writer.Write(Near_Victory);
            writer.Write(Skipped_Turn_Action);
            Skipped_Turns.write(writer);
            writer.Write(Same_Team_Turn);
            writer.Write(AutoTurnEndBlocked);
            Temp_Player_Loc.write(writer);
            writer.Write(No_Input_Timer);
            writer.Write(Weather);
            Talk_Events.write(writer);
            writer.Write(Supports_Blocked);
            Blocked_Supports.write(writer);
            Target_Screen_Tone.write(writer);
            Battle_Convos.write(writer);
            Death_Quotes.write(writer);
            Casual_Death_Quote_Blocked.write(writer);
            Unit_Battle_Themes.write(writer);
            Home_Base_Events.write(writer);
            Metrics.write(writer);
        }

        public void read_map_stuff(BinaryReader reader)
        {
            Chapter_Id = reader.ReadString();
            Global.Chapter_Text_Content.Unload();
            load_chapter_text();

            Team_Turn = reader.ReadInt32();
            Turn = reader.ReadInt32();
            Previous_Turn = reader.ReadInt32();
            Turn_Change = reader.ReadInt32();
            Changing_Turn = reader.ReadBoolean();
            Turn_End_Prompt = reader.ReadBoolean();
            Turn_Theme = reader.ReadString();
            Near_Victory = reader.ReadBoolean();
            Skipped_Turn_Action = reader.ReadInt32();
            Skipped_Turns.read(reader);
            Same_Team_Turn = reader.ReadBoolean();
            if (!Global.LOADED_VERSION.older_than(0, 5, 5, 1))
                AutoTurnEndBlocked = reader.ReadBoolean();
            Temp_Player_Loc = Temp_Player_Loc.read(reader);
            No_Input_Timer = reader.ReadInt32();
            Weather = reader.ReadInt32();
            Talk_Events.read(reader);
            Supports_Blocked = reader.ReadBoolean();
            Blocked_Supports.read(reader);
            Screen_Tone = Tone.read(reader);
            Target_Screen_Tone = Screen_Tone;
            Battle_Convos.read(reader);
            Death_Quotes.read(reader);
            if (!Global.LOADED_VERSION.older_than(0, 4, 7, 1))
                Casual_Death_Quote_Blocked.read(reader);
            Unit_Battle_Themes.read(reader);
            Home_Base_Events.read(reader);
            Metrics = Gameplay_Metrics.read(reader);
            if (Global.LOADED_VERSION.older_than(0, 5, 5, 0))
            {
                bool ch6_line = reader.ReadBoolean(); //Yeti
            }
        }
        #endregion

        #region Accessors
        public string chapter_id { get { return Chapter_Id; } }
        internal Data_Chapter chapter { get { return string.IsNullOrEmpty(Chapter_Id) ? null : Global.data_chapters[Chapter_Id]; } }

        public bool is_battle_map { get { return Turn > 0 || Global.game_system.preparations; } }

        public int turn { get { return Turn; } }
        public int team_turn
        {
            get
            {
                return Global.game_system.preparations || Global.scene.is_worldmap_scene ?
                    Constants.Team.PLAYABLE_TEAMS.Min() : Team_Turn;
            }
        }
        public bool is_changing_turns
        {
            get
            {

                if (Global.game_system.preparations)
                    return false;
                Scene_Map scene_map = get_scene_map();
                if (scene_map == null)
                    return false;

                return scene_map.is_changing_turn() || Changing_Turn || Turn_Change > -1 || new_turn_active;
            }
        }

        public int wait_time { set { No_Input_Timer = Math.Max(No_Input_Timer, value); } }

        public int weather
        {
            get { return Weather; }
            set
            {
                if (Weather != value)
                    Global.game_map.refresh_move_ranges(true);
                Weather = value;
            }
        }

        internal bool supports_blocked { get { return Supports_Blocked; } }

        public Tone screen_tone
        {
            get { return Screen_Tone; }
            set { Screen_Tone = value; }
        }

        public List<Battle_Convo> battle_convos { get { return Battle_Convos; } }

        internal Gameplay_Metrics metrics { get { return Metrics; } }


        public bool is_menuing { get { return Global.game_temp.menuing || shop_suspend_active; } }

#if !MONOGAME && DEBUG
        public bool moving_editor_unit
        {
            get { return Moving_Editor_Unit; }
            set { Moving_Editor_Unit = value; }
        }
#endif
        #endregion

        private void reset_map_stuff(bool reset_events = true)
        {
            // Test code, for two part maps the turns need to reset, Global.game_system.chapter_turn will track the real turn //Debug
#if DEBUG
            if (Global.scene.scene_type != "Scene_Map_Unit_Editor")
            {
#endif
                Turn = 0;
                Team_Turn = 0;
                Previous_Turn = 0;
#if DEBUG
            }
#endif      

            // Make sure this actually resets everything //Yeti
            Weather = (int)Weather_Types.Clear;
            Talk_Events.Clear();
            Battle_Convos.Clear();
            Death_Quotes.Clear();
            Casual_Death_Quote_Blocked.Clear();
            Unit_Battle_Themes.Clear();

            if (reset_events)
            {
                Global.game_temp.scripted_battle_stats = null;
                Supports_Blocked = false;
                Blocked_Supports.Clear();
                if (Constants.Support.NEW_MAP_COUNTS_AS_SEPARATE_CHAPTER)
                    reset_support_data();
                Home_Base_Events.Clear();

                // Reset unsaved data
                Update_Victory_Theme = false;
                Screen_Tone = new Tone(0, 0, 0, 0);
                Source_Screen_Tone = new Tone(0, 0, 0, 0);
                Target_Screen_Tone = new Tone(0, 0, 0, 0);
                Tone_Timer = 0;
                Tone_Time_Max = 0;
                prev_player_loc = null;
#if !MONOGAME && DEBUG
                Moving_Editor_Unit = false;
#endif
            }
        }

        private void reset_data(bool reset_events = true)
        {   
            Global.game_temp.highlighted_unit_id = -1;
            Global.game_system.Selected_Unit_Id = -1;
            reset_map_stuff(reset_events);
        }

        public void setup(string id, Data_Map map_data, string unit_data_name, string event_name)
        {
            Global.game_map.setup_units(unit_data_name);
            setup(id, map_data, event_name);
        }
        public void setup(string id, Data_Map map_data, Map_Unit_Data unit_data, string event_name)
        {
            Global.game_map.setup_units(id, unit_data);
            setup(id, map_data, event_name);
        }
        private void setup(string id, Data_Map map_data, string event_name)
        {
            if (Chapter_Id != id)
            {
                Chapter_Id = id;
                Global.game_system.change_chapter(Chapter_Id);
                Global.Chapter_Text_Content.Unload();
                if (Global.data_chapters.ContainsKey(Chapter_Id))
                    load_chapter_text();
                else
                    Global.chapter_text = new Dictionary<string, string>();
            }
            bool new_events = !string.IsNullOrEmpty(event_name);
            if (new_events)
            {
                // Set up handler for events
                Event_Handler.Name = event_name;
                Event_Handler.load_events();
            }

            reset_data(new_events);

            if (new_events)
                activate_autorun_events();

            Global.game_map.setup(id, map_data);
        }

        private void load_chapter_text()
        {
            if (Global.content_exists(@"Data/Text/" + this.chapter.Text_Key))
                Global.chapter_text = Global.Chapter_Text_Content.Load<Dictionary<string, string>>(@"Data/Text/" + this.chapter.Text_Key);
            else
                Global.chapter_text = new Dictionary<string, string>();
        }

        #region Update
        protected void input_handling()
        {
            Game_Unit highlighted_unit = null;
            // Makes sure the highlighted unit is actually highlighted
            if (Global.game_map.units.Keys.Contains(Global.game_temp.highlighted_unit_id))
            {
                // Test with the unit if it thinks it's still highlit
                Game_Unit highlighted_test = Global.game_map.units[Global.game_temp.highlighted_unit_id];
                if (!highlighted_test.highlighted)
                    Global.game_temp.highlighted_unit_id = -1;
                else
                    highlighted_unit = Global.game_map.units[Global.game_temp.highlighted_unit_id];
            }
            else
                Global.game_temp.highlighted_unit_id = -1;
            if (No_Input_Timer > 0)
                return;
            bool selected_moving = false;
            Game_Unit selected_unit = null;
            if (Global.game_system.Selected_Unit_Id > -1)
            {
                selected_unit = Global.game_map.units[Global.game_system.Selected_Unit_Id];
                selected_moving = !selected_unit.is_on_square;
            }
            Game_Unit status_unit = get_status_unit();

            if (!Global.game_temp.menu_call)
            {
                if (Global.player.is_on_square && !no_cursor && !Global.scene.is_message_window_active)
                {
#if !MONOGAME && DEBUG
                    if (Moving_Editor_Unit)
                    {
                        moving_editor_unit_input_handling(selected_unit, highlighted_unit, status_unit, selected_moving);
                    }
                    else
#endif
                        if (Global.game_system.preparations)
                        {
                            if (((Scene_Map)Global.scene).changing_formation && Global.game_system.Selected_Unit_Id > -1)
                                formation_input_handling(selected_unit, highlighted_unit, status_unit, selected_moving);
                            else
                                preparations_input_handling(selected_unit, highlighted_unit, status_unit, selected_moving);
                        }
                        else
                            normal_input_handling(selected_unit, highlighted_unit, status_unit, selected_moving);
                }
            }
        }

        private Game_Unit get_status_unit()
        {
            Game_Unit status_unit = Global.game_map.get_unit(Global.player.loc);
            if (status_unit != null)
#if DEBUG
                if (status_unit.is_dead || status_unit.is_rescued) //Yeti
#else
                if (status_unit.is_dead || status_unit.is_rescued || !status_unit.visible_by())
#endif
                    status_unit = null;
            return status_unit;
        }

        protected void normal_input_handling(Game_Unit selected_unit, Game_Unit highlighted_unit, Game_Unit status_unit, bool selected_moving)
        {
            // Unit selected
            if (Global.game_system.Selected_Unit_Id != -1)
            {
                // A button
                if (Global.Input.triggered(Inputs.A) ||
                    (Global.Input.mouse_triggered(MouseButtons.Left) &&
                    Global.player.at_mouse_loc))
                {
                    // Move unit
                    Global.game_map.move_selected_unit(selected_unit);
                    return;
                }
                // B button
                else if (Global.Input.triggered(Inputs.B) ||
                    ((status_unit == null || status_unit == selected_unit) &&
                        Global.Input.mouse_triggered(MouseButtons.Right)))
                {
                    // Clear enemy move range
                    if (!selected_unit.is_active_team || selected_unit.unselectable) //Multi
                    {
                        Global.game_map.deselect_unit();
                        return;
                    }
                    // Deselect the current unit
                    else if (selected_unit.is_on_square && selected_unit.sprite_moving)
                    {
                        if (selected_unit.cannot_cancel_move())
                            // Can't clear move range
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        else
                        {
                            selected_unit.cancel_move();
                        }
                        return;
                    }
                }
                // Screen tap
                else if (Global.Input.gesture_triggered(TouchGestures.TapNoDouble))
                {
                    // Move unit
                    Vector2 tap_loc = Global.Input.gesture_loc(TouchGestures.TapNoDouble);
                    if (Global.game_map.screen_loc_in_view(tap_loc))
                    {
                        // Map to onscreen
                        tap_loc = Global.game_map.screen_loc_to_tile(tap_loc);
                        // Must be in the playable space
                        if (!Global.game_map.is_off_map(tap_loc))
                        {
                            Global.player.instant_move = true;
                            Global.player.force_loc(tap_loc);
                            Global.game_map.move_selected_unit(selected_unit);
                            return;
                        }
                    }
                }
            }
            // Unit highlighted
            else if (Global.game_temp.highlighted_unit_id > -1)
            {
                // Select the current unit
                if (Global.Input.triggered(Inputs.A) ||
                    (Global.Input.mouse_triggered(MouseButtons.Left) &&
                    Global.player.at_mouse_loc))
                {
                    Global.game_map.select_unit(highlighted_unit);
                    return;
                }
                // Open the map menu
                else if (Global.Input.triggered(Inputs.Select))
                {
                    Global.game_map.open_map_menu(highlighted_unit);
                    return;
                }
                // Close enemy range
                else if (Global.Input.triggered(Inputs.B))
                {
                    Global.game_map.deselect_enemy_range_unit(highlighted_unit, team_turn);
                    return;
                }
            }
            // No unit
            else
            {
                // A button
                // Select button
                if (Global.Input.triggered(Inputs.A) || Global.Input.triggered(Inputs.Select) ||
                    (Global.Input.mouse_click(MouseButtons.Left) &&
                    Global.player.at_mouse_loc))
                {
                    Global.game_map.open_map_menu(highlighted_unit);
                    return;
                }
            }

            // Start button
            if (Global.Input.triggered(Inputs.Start))
            {
                Global.game_map.open_minimap(highlighted_unit);
            }
            // X button
            else if (Global.Input.triggered(Inputs.X) ||
                (Global.Input.mouse_triggered(MouseButtons.Middle) &&
                Global.player.at_mouse_loc))
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(!selected_moving); // how would this ever though //Debug
#endif
                if (!selected_moving)
                    Global.game_map.try_toggle_enemy_range(status_unit, team_turn);
            }
            // R button
            else if (Global.Input.triggered(Inputs.R) ||
                (Global.Input.mouse_triggered(MouseButtons.Right) &&
                Global.player.at_mouse_loc))
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(!selected_moving); // how would this ever though //Debug
#endif
                if (!selected_moving)
                {
                    if (status_unit != null)
                    {
                        status_unit.status();
                    }
                }
            }
            // L button
            else if ((selected_unit != null || highlighted_unit == null) ?
                Global.Input.triggered(Inputs.L) :
                Global.Input.repeated(Inputs.L)) //Debug
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(!selected_moving); // how would this ever though //Debug
#endif
                Global.game_map.next_unit(highlighted_unit, selected_moving);
            }

            // Screen tap
            else if (Global.Input.gesture_triggered(TouchGestures.Tap, false))
            {
                // Select unit
                if (move_to_touch_location(TouchGestures.Tap))
                {
                    Global.game_map.highlight_test();

                    if (Global.game_map.units.Keys.Contains(Global.game_temp.highlighted_unit_id))
                    {
                        highlighted_unit = Global.game_map.units[Global.game_temp.highlighted_unit_id];
                        if (Global.game_map.select_unit(highlighted_unit, true))
                        {
                            // Consume tap input so it doesn't become a deferred tap
                            Global.Input.gesture_triggered(TouchGestures.Tap);
                        }
                    }
                    return;
                }
            }
            // Screen tap (confirming no double tap)
            else if (Global.Input.gesture_triggered(TouchGestures.TapNoDouble))
            {
                // Select unit
                if (move_to_touch_location(TouchGestures.TapNoDouble))
                {
                    Global.game_map.highlight_test();

                    if (Global.game_map.units.Keys.Contains(Global.game_temp.highlighted_unit_id))
                    {
                        highlighted_unit = Global.game_map.units[Global.game_temp.highlighted_unit_id];
                        Global.game_map.select_unit(highlighted_unit);
                    }
                    else
                        Global.game_map.open_map_menu(highlighted_unit);
                    return;
                }
            }
            // Double tap
            else if (Global.Input.gesture_triggered(TouchGestures.DoubleTap))
            {
                if (move_to_touch_location(TouchGestures.DoubleTap))
                {
                    Global.game_map.highlight_test();
                    status_unit = get_status_unit();
#if DEBUG
                    System.Diagnostics.Debug.Assert(!selected_moving); // how would this ever though //Debug
#endif
                    if (!selected_moving)
                        Global.game_map.try_toggle_enemy_range(status_unit, team_turn);
                }
            }
            // Long Press
            else if (Global.Input.gesture_triggered(TouchGestures.LongPress))
            {
                // Select unit
                if (move_to_touch_location(TouchGestures.LongPress))
                {
                    Global.game_map.highlight_test();
                    status_unit = get_status_unit();
#if DEBUG
                    System.Diagnostics.Debug.Assert(!selected_moving); // how would this ever though //Debug
#endif
                    if (!selected_moving)
                    {
                        if (status_unit != null)
                        {
                            status_unit.status();
                        }
                    }
                }
            }
            // Pinch In
            else if (Global.Input.gesture_triggered(TouchGestures.PinchIn))
            {
                Global.game_map.open_minimap(highlighted_unit);
            }
        }

        internal bool move_to_touch_location(TouchGestures gesture)
        {
            Vector2 gesture_loc = Global.Input.gesture_loc(gesture);
            // Must be in view
            if (Global.game_map.screen_loc_in_view(gesture_loc))
            {
                // Map to onscreen
                gesture_loc = Global.game_map.screen_loc_to_tile(gesture_loc);
                // Must be in the playable space
                if (!Global.game_map.is_off_map(gesture_loc))
                {
                    Global.player.instant_move = true;
                    Global.player.force_loc(gesture_loc, false);
                    return true;
                }
            }
            return false;
        }

        protected void preparations_input_handling(Game_Unit selected_unit, Game_Unit highlighted_unit, Game_Unit status_unit, bool selected_moving)
        {
            // B button
            if (Global.game_system.Selected_Unit_Id != -1 && !is_menuing)
            {
                selected_unit = Global.game_map.units[Global.game_system.Selected_Unit_Id];
                // Close the unit's move range
                if (!(Global.game_temp.menu_call))
                {
                    if (Global.Input.triggered(Inputs.B) ||
                        ((status_unit == null || status_unit == selected_unit) &&
                            Global.Input.mouse_triggered(MouseButtons.Right)))
                    {
                        Global.game_map.deselect_unit();
                        return;
                    }
                }
            }
            if (Global.Input.triggered(Inputs.B))
            {
                if (!is_menuing && is_map_ready() &&
                    Global.game_system.Selected_Unit_Id == -1 &&
                     !Global.game_map.scrolling)
                {
                    if (Global.game_temp.highlighted_unit_id == -1 ||
                        highlighted_unit.is_player_allied)
                    {
                        Global.game_temp.map_menu_call = true;
                        Global.game_temp.menu_call = true;
                    }
                }
                
                Global.game_map.deselect_enemy_range_unit(highlighted_unit, Constants.Team.PLAYER_TEAM);
            }
            // A button
            else if (Global.Input.triggered(Inputs.A) ||
                (Global.Input.mouse_triggered(MouseButtons.Left) &&
                Global.player.at_mouse_loc))
            {
                if (!is_menuing && is_map_ready())
                {
                    // Select the current unit
                    if (Global.game_temp.highlighted_unit_id > -1 &&
                        Global.game_system.Selected_Unit_Id == -1)
                    {
                        Global.game_map.select_preparations_unit(highlighted_unit);
                    }
                    // If unit is selected
                    else if (Global.game_system.Selected_Unit_Id > -1)
                    {
                        // Close the unit's move range
                        Global.game_map.deselect_unit();
                    }
                    else if (Global.game_system.Selected_Unit_Id == -1 &&
                        Global.game_temp.highlighted_unit_id == -1 && !Global.game_map.scrolling)
                    {
                        if (Global.game_map.shops.ContainsKey(Global.player.loc) &&
                            !Global.game_map.get_shop(Global.player.loc).arena)
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            Global.game_temp.call_shop(false);
                            Global.game_system.Shopper_Id = -1;
                            Global.game_system.Shop_Loc = Global.player.loc;
                        }
                        else
                        {
                            Global.game_temp.map_menu_call = true;
                            Global.game_temp.menu_call = true;
                        }
                    }
                }
            }
            // Select button
#if DEBUG
            else if (Global.Input.triggered(Inputs.Select) && Global.scene.scene_type != "Scene_Map_Unit_Editor")
#else
                    else if (Global.Input.triggered(Inputs.Select))
#endif
            {
                Global.game_map.open_map_menu(highlighted_unit);
            }
            // Start button
            else if (Global.Input.triggered(Inputs.Start))
            {
                Global.game_map.open_minimap(highlighted_unit);
            }
            // X button
            else if (Global.Input.triggered(Inputs.X) ||
                (Global.Input.mouse_triggered(MouseButtons.Middle) &&
                    Global.player.at_mouse_loc))
            {
                if (!selected_moving)
                    Global.game_map.try_toggle_enemy_range(highlighted_unit, Constants.Team.PLAYER_TEAM);
            }
            // R button
            else if (Global.Input.triggered(Inputs.R) ||
                (Global.Input.mouse_triggered(MouseButtons.Right) &&
                Global.player.at_mouse_loc))
            {
                if (!is_menuing && is_map_ready() && !selected_moving)
                {
                    if (status_unit != null)
                    {
                        status_unit.status();
                    }
                }
            }
            // L button
            else if ((selected_unit != null || highlighted_unit == null) ?
                Global.Input.triggered(Inputs.L) :
                Global.Input.repeated(Inputs.L)) //Debug
            {
                Global.game_map.next_unit(highlighted_unit, selected_moving);
            }
            
            // Screen tap
            else if (Global.Input.gesture_triggered(TouchGestures.Tap, false))
            {
                // Select unit
                if (move_to_touch_location(TouchGestures.Tap))
                {
                    Global.game_map.highlight_test();

                    if (Global.game_map.units.Keys.Contains(Global.game_temp.highlighted_unit_id))
                    {
                        highlighted_unit = Global.game_map.units[Global.game_temp.highlighted_unit_id];
                        if (Global.game_map.select_preparations_unit(highlighted_unit, true))
                        {
                            // Consume tap input so it doesn't become a deferred tap
                            Global.Input.gesture_triggered(TouchGestures.Tap);
                        }
                    }
                    return;
                }
            }
            // Screen tap (confirming no double tap)
            else if (Global.Input.gesture_triggered(TouchGestures.TapNoDouble))
            {
                // Select unit
                if (move_to_touch_location(TouchGestures.TapNoDouble))
                {
                    // If unit is selected
                    if (Global.game_system.Selected_Unit_Id > -1)
                    {
                        // Close the unit's move range
                        Global.game_map.deselect_unit();
                        Global.game_map.highlight_test();
                        return;
                    }
                        
                    Global.game_map.highlight_test();

                    if (Global.game_system.Selected_Unit_Id == -1 &&
                        Global.game_temp.highlighted_unit_id == -1 &&
                        !Global.game_map.scrolling)
                    {
                        if (Global.game_map.shops.ContainsKey(Global.player.loc) &&
                            !Global.game_map.get_shop(Global.player.loc).arena)
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            Global.game_temp.call_shop(false);
                            Global.game_system.Shopper_Id = -1;
                            Global.game_system.Shop_Loc = Global.player.loc;
                        }
                        else
                        {
                            Global.game_temp.map_menu_call = true;
                            Global.game_temp.menu_call = true;
                        }
                    }
                    else if (Global.game_map.units.Keys.Contains(
                        Global.game_temp.highlighted_unit_id))
                    {
                        highlighted_unit = Global.game_map.units[Global.game_temp.highlighted_unit_id];
                        Global.game_map.select_preparations_unit(highlighted_unit);
                    }
                    return;
                }
            }
            // Double tap
            else if (Global.Input.gesture_triggered(TouchGestures.DoubleTap))
            {
                if (move_to_touch_location(TouchGestures.DoubleTap))
                {
                    Global.game_map.highlight_test();
                    status_unit = get_status_unit();
#if DEBUG
                    System.Diagnostics.Debug.Assert(!selected_moving); // how would this ever though //Debug
#endif
                    if (!selected_moving)
                        Global.game_map.try_toggle_enemy_range(status_unit, team_turn);
                }
            }
            // Long Press
            else if (Global.Input.gesture_triggered(TouchGestures.LongPress))
            {
                // Select unit
                if (move_to_touch_location(TouchGestures.LongPress))
                {
                    Global.game_map.highlight_test();
                    status_unit = get_status_unit();
#if DEBUG
                    System.Diagnostics.Debug.Assert(!selected_moving); // how would this ever though //Debug
#endif
                    if (!selected_moving)
                    {
                        if (status_unit != null)
                        {
                            status_unit.status();
                        }
                    }
                }
            }
            // Pinch In
            else if (Global.Input.gesture_triggered(TouchGestures.PinchIn))
            {
                Global.game_map.open_minimap(highlighted_unit);
            }
        }

        protected void formation_input_handling(Game_Unit selected_unit, Game_Unit highlighted_unit, Game_Unit status_unit, bool selected_moving)
        {
            // B button
            if (Global.Input.triggered(Inputs.B) ||
                Global.Input.mouse_triggered(MouseButtons.Right))
            {
                if (Global.game_system.Selected_Unit_Id != -1 && !is_menuing)
                {
                    selected_unit = Global.game_map.units[Global.game_system.Selected_Unit_Id];
                    if (!(Global.game_temp.menu_call))
                    {
                        Global.game_map.deselect_unit(false);
                        Global.game_map.range_start_timer = 0;
                    }
                }
                else
                    Global.game_map.deselect_enemy_range_unit(highlighted_unit, team_turn);
            }
            // A button
            else if (Global.Input.triggered(Inputs.A) ||
                (Global.Input.mouse_triggered(MouseButtons.Left) &&
                Global.player.at_mouse_loc))
            {
                if (!is_menuing && is_map_ready())
                {
                    Global.game_map.switch_formation(selected_unit);
                }
            }
            // X button
            else if (Global.Input.triggered(Inputs.X) ||
                (Global.Input.mouse_triggered(MouseButtons.Middle) &&
                Global.player.at_mouse_loc))
            {
                if (!selected_moving)
                    Global.game_map.try_toggle_enemy_range(highlighted_unit, team_turn);
            }

            // Screen tap
            else if (Global.Input.gesture_triggered(TouchGestures.Tap, false))
            {
                if (move_to_touch_location(TouchGestures.Tap))
                {
                    if (Global.game_map.deployment_points.Contains(Global.player.loc))
                    {
                        if (!is_menuing && is_map_ready())
                        {
                            Global.game_map.switch_formation(selected_unit);
                            // Consume tap input so it doesn't become a deferred tap
                            Global.Input.gesture_triggered(TouchGestures.Tap);
                        }
                    }
                    return;
                }
            }
            // Screen tap (confirming no double tap)
            else if (Global.Input.gesture_triggered(TouchGestures.TapNoDouble))
            {
                if (move_to_touch_location(TouchGestures.TapNoDouble))
                {
                    if (Global.game_system.Selected_Unit_Id != -1 && !is_menuing)
                    {
                        Game_Unit unit_here = Global.game_map.get_unit(Global.player.loc);
                        if (unit_here != null &&
                                !Global.game_map.deployment_points.Contains(Global.player.loc))
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        else if (!(Global.game_temp.menu_call))
                        {
                            Global.game_map.deselect_unit(false);
                            Global.game_map.range_start_timer = 0;
                        }
                        return;
                    }
                }
            }
            // Double tap
            else if (Global.Input.gesture_triggered(TouchGestures.DoubleTap))
            {
                if (move_to_touch_location(TouchGestures.DoubleTap))
                {
                    Global.game_map.highlight_test();
                    status_unit = get_status_unit();
#if DEBUG
                    System.Diagnostics.Debug.Assert(!selected_moving); // how would this ever though //Debug
#endif
                    if (!selected_moving)
                        Global.game_map.try_toggle_enemy_range(status_unit, team_turn);
                }
            }
        }

#if !MONOGAME && DEBUG
        protected void moving_editor_unit_input_handling(Game_Unit selected_unit, Game_Unit highlighted_unit, Game_Unit status_unit, bool selected_moving)
        {
            // B button
            if (Global.Input.triggered(Inputs.B) ||
                Global.Input.mouse_triggered(MouseButtons.Right))
            {
                if (Global.game_system.Selected_Unit_Id != -1 && !is_menuing)
                {
                    Moving_Editor_Unit = false;
                    Global.game_map.deselect_unit();
                }
            }
            // A button
            else if (Global.Input.triggered(Inputs.A) ||
                (Global.Input.mouse_triggered(MouseButtons.Left) &&
                Global.player.at_mouse_loc))
            {
                if (!is_menuing && is_map_ready())
                {
                    if (status_unit != null)
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Moving_Editor_Unit = false;
                        ((Scene_Map_Unit_Editor)get_scene_map()).move_unit();
                        ((Scene_Map_Unit_Editor)get_scene_map()).set_map();
                        Global.game_system.Selected_Unit_Id = -1;
                        Global.game_map.move_range_visible = true;
                        Global.game_map.highlight_test();
                    }
                }
            }
        }
#endif

        public void update_tone()
        {
            if (Tone_Timer > 0)
            {
                Tone_Timer--;
                Screen_Tone.r = (int)((Source_Screen_Tone.r * (Tone_Timer / (float)Tone_Time_Max)) +
                    (Target_Screen_Tone.r * ((Tone_Time_Max - Tone_Timer) / (float)Tone_Time_Max)));
                Screen_Tone.g = (int)((Source_Screen_Tone.g * (Tone_Timer / (float)Tone_Time_Max)) +
                    (Target_Screen_Tone.g * ((Tone_Time_Max - Tone_Timer) / (float)Tone_Time_Max)));
                Screen_Tone.b = (int)((Source_Screen_Tone.b * (Tone_Timer / (float)Tone_Time_Max)) +
                    (Target_Screen_Tone.b * ((Tone_Time_Max - Tone_Timer) / (float)Tone_Time_Max)));
                Screen_Tone.a = (int)((Source_Screen_Tone.a * (Tone_Timer / (float)Tone_Time_Max)) +
                    (Target_Screen_Tone.a * ((Tone_Time_Max - Tone_Timer) / (float)Tone_Time_Max)));
                if (Tone_Timer == 0)
                    Tone_Time_Max = 0;
            }
        }

        public void change_screen_tone(int r, int g, int b, int a, int time)
        {
            Tone_Timer = Tone_Time_Max = Math.Max(0, time);
            Source_Screen_Tone = Screen_Tone;
            Target_Screen_Tone = new Tone(r, g, b, a);
            if (Tone_Timer == 0)
                Screen_Tone = Target_Screen_Tone;
        }

        protected void update_main_turn_change()
        {
            // This needs to not stop on non-player turns on turn 0??? //@Yeti
            if (Changing_Turn)
            {
                if (No_Input_Timer <= 0)
                {
                    // Checks for events from skipped turns (teams that have no units)
                    if (Skipped_Turns.Count > 0 || Skipped_Turn_Action > 0)
                    {
                        switch (Skipped_Turn_Action)
                        {
                            // Called first, from a turn that ended normally
                            case 3:
                                turn_end_events();
                                Skipped_Turn_Action = 0;
                                break;
                            // Loops through these three, starting and ending turns that no units exist for
                            case 0:
                                // If units got added to the team so it
                                // shouldn't be skipped any more
                                if (!SkipTeamTurn(Skipped_Turns[0]))
                                {
                                    Skipped_Turns.Clear();
                                    break;
                                }

                                Team_Turn = Skipped_Turns[0];
                                // This used to be checked in case 2,
                                // but that would not work
                                // It never triggered incorrectly because the
                                // player turn is basically never skipped //@Debug
                                if (Team_Turn <= Previous_Turn || Previous_Turn == 0)
                                    next_turn();
                                Skipped_Turns.RemoveAt(0);
                                turn_start_events();
                                Skipped_Turn_Action = 1;
                                break;
                            case 1:
                                turn_end_events();
                                Skipped_Turn_Action = 2;
                                break;
                            case 2:
                                Skipped_Turn_Action = 0;
                                Previous_Turn = Team_Turn;
                                break;
                        }
                    }
                    else
                    {
                        if (turn_change())
                            Changing_Turn = false;
                    }
                }
            }
            else
            { 
                if (!Global.game_temp.menuing)
                    ally_turn_end_check();
            }
            if (Turn_Change > -1)
            {
                get_scene_map().change_turn(Turn_Change);
                Turn_Change = -2;
            }
        }

        public void turn_change_end()
        {
            // Turn_Change is -2 while the phase change graphic is visible, then becomes -1 when the active turn is not changing
            if (Turn_Change == -2)
            {
                Turn_Change = -1;
                Same_Team_Turn = false;
                New_Turn_Calling = true;
                Turn_End_Prompt = is_player_turn && Global.game_options.auto_turn_end == 2;
            }
        }
        #endregion

        #region Events
        // Talk Events
        public void add_unit_talk_event(int id1, int id2, string eventName, bool bothWays)
        {
            if (Global.game_map.units.ContainsKey(id1) &&
                    Global.game_map.units.ContainsKey(id2))
                add_talk_event(
                    Global.game_map.units[id1].actor.id,
                    Global.game_map.units[id2].actor.id,
                    eventName, bothWays);
        }
        public void add_actor_unit_talk_event(int actorId1, int id2, string eventName, bool bothWays)
        {
            if (Global.game_map.units.ContainsKey(id2))
                add_talk_event(actorId1, Global.game_map.units[id2].actor.id,
                    eventName, bothWays);
        }
        public void add_talk_event(int actorId1, int actorId2, string event_name, bool both_ways)
        {
            int i = 0;
            // If this pair already has a talk event, remove it
            Talk_Events = Talk_Events
                .Where(x => !x.for_these_actors(actorId1, actorId2))
                .ToList();

            Talk_Events.Add(new Talk_Event { ActorId1 = actorId1, ActorId2 = actorId2,
                Event_Name = event_name, Both_Ways = both_ways });
        }
        
        public void remove_unit_talk_event(int id1, int id2)
        {
            if (Global.game_map.units.ContainsKey(id1) &&
                    Global.game_map.units.ContainsKey(id2))
            {
                var unit1 = Global.game_map.units[id1];
                var unit2 = Global.game_map.units[id2];

                remove_talk_event(unit1.actor.id, unit2.actor.id);
            }
        }
        public void remove_actor_unit_talk_event(int actorId1, int id2)
        {
            if (Global.game_map.units.ContainsKey(id2))
            {
                var unit2 = Global.game_map.units[id2];

                remove_talk_event(actorId1, unit2.actor.id);
            }
        }
        public void remove_talk_event(int actorId1, int actorId2)
        {
            Talk_Events = Talk_Events
                .Where(x => !x.for_these_actors(actorId1, actorId2))
                .ToList();
        }

        public void activate_talk(int unitId1, int unitId2)
        {
            if (Global.game_map.units.ContainsKey(unitId1) &&
                    Global.game_map.units.ContainsKey(unitId2))
            {
                var unit1 = Global.game_map.units[unitId1];
                var unit2 = Global.game_map.units[unitId2];

                int i = 0;
                while (i < Talk_Events.Count)
                {
                    if (Talk_Events[i].for_these_units(unit1, unit2))
                    {
                        unit1.talk_support_gain_display(unitId2);

                        unit1.actor.talk_support_gain(unit2.actor.id);
                        unit2.actor.talk_support_gain(unit1.actor.id);

                        call_talk(Talk_Events[i].Event_Name, unitId1);
                        Talk_Events.RemoveAt(i);
                        Global.game_map.remove_updated_move_range(unitId2);
                        return;
                    }
                    else
                        i++;
                }
            }
        }

        public bool can_talk(int unitId1, int unitId2)
        {
            if (Global.game_map.units.ContainsKey(unitId1) &&
                    Global.game_map.units.ContainsKey(unitId2))
            {
                var unit1 = Global.game_map.units[unitId1];
                var unit2 = Global.game_map.units[unitId2];

                return Talk_Events.Any(x => x.for_these_units(unit1, unit2));
            }

            return false;
        }

        public HashSet<int> talk_targets(int unitId)
        {
            if (Global.game_map.units.ContainsKey(unitId))
            {
                var unit1 = Global.game_map.units[unitId];
                var actor_ids = new HashSet<int>(Talk_Events
                    // Get talk events where this unit is one of the talkers
                    .Where(x => x.for_these_units(unit1))
                    // Get the actor id of the other unit for those events
                    .Select(x => x.other_unit_id(unit1)));
                // Convert those actor ids into unit ids
                return new HashSet<int>(Global.game_map.units
                    .Values
                    .Where(x => actor_ids.Contains(x.actor.id))
                    .Select(x => x.id));
            }

            return null;
        }

        // Blocked Supports
        public void block_supports()
        {
            Supports_Blocked = true;
        }
        public void block_support(int id1)
        {
            Blocked_Supports.Add(new int[] { id1 });
        }
        public void block_support(int id1, int id2)
        {
            Blocked_Supports.Add(new int[] { id1, id2 });
        }

        public bool is_support_blocked(int actor_id1, int actor_id2, bool activation_only = false)
        {
            // If all supports are blocked for this chapter
            if (Supports_Blocked)
                return true;
            // Loop through blocked support specific actors
            if (activation_only && Blocked_Supports.Where(x => x.Length == 1).Any(x => x[0] == actor_id1))
                    return true;
            // Loop through blocked support pairs
            foreach (var blocked in Blocked_Supports.Where(x => x.Length == 2))
            {
                // If the event blocked list has this pair
                if ((blocked[0] == actor_id1 && blocked[1] == actor_id2) ||
                        (blocked[1] == actor_id1 && blocked[0] == actor_id2))
                    return true;
            }
            // If the actors have already supported this chapter
            if (Constants.Support.ONE_SUPPORT_PER_CHAPTER)
            {
                if (!Scene_Map.debug_chapter_options_blocked())
                {
                    if (activation_only)
                        if (supports_this_chapter.ContainsKey(actor_id1) &&
                            supports_this_chapter[actor_id1].Contains(actor_id2))
                            return true;
                }
            }
            return false;
        }

        // Home Base Events

        public void add_base_event(string name, int priority, string event_name)
        {
            Home_Base_Events.Add(new Home_Base_Event_Data { Name = name, Priority = priority, Event_Name = event_name });
        }

        public List<string> base_event_names()
        {
            if (Config.BASE_EVENT_ACTIVATED_INVISIBLE)
                return Home_Base_Events.Where(x => !x.Activated).Select(x => x.Name).ToList();
            else
                return Home_Base_Events.Select(x => x.Name).ToList();
        }

        public List<int> base_event_priorities()
        {
            if (Config.BASE_EVENT_ACTIVATED_INVISIBLE)
                return Home_Base_Events.Where(x => !x.Activated).Select(x => x.Priority).ToList();
            else
                return Home_Base_Events.Select(x => x.Priority).ToList();
        }

        public bool base_event_ready(int index)
        {
            if (Config.BASE_EVENT_ACTIVATED_INVISIBLE)
                return Home_Base_Events.Where(x => !x.Activated).Count() > index;
            else
                return !Home_Base_Events[index].Activated;
        }

        public bool has_ready_base_events()
        {
            //return Home_Base_Events.Count > 0 && Home_Base_Events.Any(x => !x.Activated); //Debug
            return Home_Base_Events.Any(x => !x.Activated);
        }

        public bool activate_base_event(int index)
        {
            if (base_event_ready(index))
            {
                if (Config.BASE_EVENT_ACTIVATED_INVISIBLE)
                    index = Home_Base_Events.IndexOf(Home_Base_Events.Where(x => !x.Activated).ToList()[index]);
                activate_event_by_name(Home_Base_Events[index].Event_Name);
                Home_Base_Events[index].Activated = true;
                return true;
            }
            return false;
        }

        public void clear_home_base_events()
        {
            Home_Base_Events.Clear();
        }

        public bool augury_event_exists()
        {
            return Event_Handler.event_data.Events.Any(x => x.name == "Augury");
        }
        public void activate_augury_event()
        {
            activate_event_by_name("Augury");
        }
        #endregion

        #region Map Controls
        // Battle Convos
        public void add_battle_convo(int id1, int id2, string value)
        {
            Battle_Convos.Add(new Battle_Convo { Id1 = id1, Id2 = id2, Value = value });
        }

        public void clear_battle_convo(int index)
        {
            if (index < Battle_Convos.Count)
                Battle_Convos[index].Activated = true;
        }

        // Death Quotes
        public void add_death_quote(int id, string value, bool casual_blocked)
        {
            Death_Quotes[id] = value;
            Casual_Death_Quote_Blocked[id] = casual_blocked;
        }

        public string get_death_quote(int id)
        {
            string quote = "", ch_quote = "";
            bool casual_blocked = false;
            // If the actor has a specific quote for this chapter
            if (Death_Quotes.ContainsKey(Global.game_map.units[id].actor.id))
            {
                if (Death_Quotes[Global.game_map.units[id].actor.id] == "null")
                    return "";
                if (Global.death_quotes.ContainsKey(Death_Quotes[Global.game_map.units[id].actor.id]))
                {
                    quote = Death_Quotes[Global.game_map.units[id].actor.id];
                    casual_blocked = Casual_Death_Quote_Blocked[Global.game_map.units[id].actor.id];
                }
            }
            // If they have a death quote
            else if (Global.death_quotes.ContainsKey(Global.game_map.units[id].actor.name_full))
                quote = Global.game_map.units[id].actor.name_full;
            // If neither
            if (string.IsNullOrEmpty(quote) && string.IsNullOrEmpty(ch_quote))
                return "";

            // If casual quote isn't blocked
            if (!casual_blocked)
                // If they have lives remaining and a casual death quote
                if (Global.game_map.units[id].actor.lives != 1 &&
                    Global.game_system.Style == Mode_Styles.Casual &&
                    Global.game_map.units[id].is_ally)
                {
                    // Tries to find a casual mode death quote for this specific chapter
                    var casual_quote = get_casual_death_quote(id, ch_quote);
                    if (casual_quote.IsSomething)
                        return casual_quote;
                    // Tries to find a casual mode death quote for this character at all
                    casual_quote = get_casual_death_quote(id, quote);
                    if (casual_quote.IsSomething)
                        return casual_quote;
                }
            return quote;
        }
        private Maybe<string> get_casual_death_quote(int id, string quote)
        {
            // Current life of the actor; how many times they'll have died after this quote plays
            int current_life = Constants.Actor.CASUAL_MODE_LIVES - (Global.game_map.units[id].actor.lives - 1);
            if (Global.death_quotes.ContainsKey(string.Format("{0} Casual{1}", quote, current_life)))
                return string.Format("{0} Casual{1}", quote, current_life);
            if (Global.death_quotes.ContainsKey(quote + " Casual"))
                return quote + " Casual";
            return default(Maybe<string>);
        }

        // Battle Themes
        public void add_battle_theme(int id, string value)
        {
            Unit_Battle_Themes[id] = value;
        }

        // Metrics
        internal void set_pc_starting_stats(Game_Unit unit) //private // should be an event? //Yeti
        {
            Metrics.set_pc_starting_stats(new HashSet<Game_Unit> { unit });
        }

        internal void add_combat_metric(CombatTypes type, int attacker_id, int? target_id, int attacker_hp, int target_hp,
            int weapon_1_id, int weapon_2_id, int attacker_attacks, int target_attacks)
        {
            if (Metrics != null && !Global.game_system.In_Arena)
                Metrics.add_combat(Global.game_system.chapter_turn, type, Global.game_map.units[attacker_id], attacker_hp, weapon_1_id, attacker_attacks,
                    target_id == null ? null : new List<Combat_Map_Object> { Global.game_map.attackable_map_object((int)target_id) }, new List<int> { target_hp },
                    new List<int> { weapon_2_id }, new List<int> { target_attacks });
        }
        internal void add_combat_metric(CombatTypes type, int attacker_id, List<int> target_ids, int attacker_hp, List<int> target_hps, int weapon_1_id, int attacker_attacks)
        {
            if (Metrics != null && !Global.game_system.In_Arena)
                Metrics.add_combat(Global.game_system.chapter_turn, type, Global.game_map.units[attacker_id], attacker_hp, weapon_1_id, attacker_attacks,
                    target_ids.Select(x => Global.game_map.attackable_map_object(x)).ToList(), target_hps,
                    target_ids.Select(x => -1).ToList(), target_ids.Select(x => 0).ToList());
        }

        internal void add_item_metric(Game_Unit unit, int item_index) //private //Yeti
        {
            if (Metrics != null)
                Metrics.add_item(Global.game_system.chapter_turn, unit, new Item_Data(item_user.actor.items[item_index]));
        }
        #endregion

        public Game_Ranking calculate_ranking_progress()
        {
            // If it's still the first turn, no accurate data can be gathered
            if (Global.game_system.chapter_turn <= 1 || Metrics == null)
            {
                return null;
            }
            // Get how many enemies there are and how many are left
            IEnumerable<int> enemy_teams = Constants.Team.TEAM_GROUPS
                .Where(x => !x.Contains(Constants.Team.PLAYER_TEAM))
                .SelectMany(x => x)
                .Distinct();
            int killed_enemies = Metrics.killed_units(Constants.Team.PLAYER_TEAM, enemy_teams);
            int enemies_remaining = Global.game_map.units.Values.Count(x => x.is_opposition);

            return Global.game_system.calculate_ranking_progress(this.chapter, killed_enemies, enemies_remaining);
        }

        #region Music
        public void play_turn_theme(int volume = 100, bool fade_in = false, int teamTurn = -1)
        {
            if (this.chapter == null)
                return;

            if (teamTurn == -1)
                teamTurn = Team_Turn;
            Turn_Theme = this.chapter.Turn_Themes[teamTurn];
            if (teamTurn == Constants.Team.PLAYER_TEAM && near_victory())
                Turn_Theme = Constants.Audio.Bgm.VICTORY_THEME;
            Near_Victory = near_victory();
            play_turn_theme(Turn_Theme);
        }
        public void play_turn_theme(string name, bool force_restart_theme = false)
        {
            Turn_Theme = name;
            if (string.IsNullOrEmpty(Turn_Theme))
                return;

            if (force_restart_theme)
                Global.Audio.PlayMapTheme(name);
            else
                Global.Audio.ResumeMapTheme(name);
        }

        protected bool near_victory()
        {
            // Count the number of remaining enemies
            int enemies = Constants.Team.TEAM_GROUPS
                .Where(x => !x.Contains(Constants.Team.PLAYER_TEAM))
                .SelectMany(x => x)
                .Sum(x => Global.game_map.teams[x].Count);
            /*for (int i = 0; i < Config.TEAM_GROUPS.Length; i++)
            {
                int[] group = Config.TEAM_GROUPS[i];
                if (!group.Contains(Config.PLAYER_TEAM))
                    foreach (int team in group)
                        enemies += Teams[team].Count;
            }*/
#if DEBUG
            if (Global.scene.scene_type == "Scene_Map_Unit_Editor")
                return enemies <= -1; //Debug
#endif
            //Yeti
            if (Scene_Map.intro_chapter_options_blocked())
                return false;
            return enemies <= 1; //Debug
        }

        protected void update_victory_theme()
        {
            if (Update_Victory_Theme && is_map_ready())
            {
                Update_Victory_Theme = false;
                if (Team_Turn == Constants.Team.PLAYER_TEAM && Near_Victory != near_victory())
                {
                    Near_Victory = near_victory();
                    Global.Audio.BgmFadeOut(30);
                    play_turn_theme();
                }
            }
        }

        public void resume_turn_theme()
        {
            resume_turn_theme(false);
        }
        public void resume_turn_theme(bool fade)
        {
            if (!Global.game_system.preparations && !Global.game_system.is_victory()) //Yeti
                Global.Audio.ResumeMapTheme(Turn_Theme);
        }

        public string battle_theme()
        {
            return CombatState.battle_theme;
        }

        public void play_staff_theme()
        {
            Global.Audio.PlayBattleTheme(Constants.Audio.Bgm.STAFF_THEME);
        }

        public void play_attack_staff_theme()
        {
            Global.Audio.PlayBattleTheme(Constants.Audio.Bgm.ATTACK_STAFF_THEME);
        }

        public void play_dance_theme()
        {
            if (dancer_id != -1)
                switch (Global.game_map.units[dancer_id].actor.class_id) { } //Yeti
            Global.Audio.PlayBattleTheme(Constants.Audio.Bgm.DANCE_THEME); //Yeti
        }

        public void play_promotion_theme()
        {
            Global.Audio.PlayBattleTheme(Constants.Audio.Bgm.PROMOTION_THEME);
        }

        public void play_preparations_theme()
        {
            Global.Audio.PlayBgm(Constants.Audio.Bgm.PREPARATIONS_THEME, forceRestart: true);
        }
        #endregion

        #region Turn Change
        internal void block_auto_turn_end()
        {
            AutoTurnEndBlocked = true;
        }

        internal void allow_auto_turn_end()
        {
            AutoTurnEndBlocked = false;
        }

        public bool ally_turn_end_check(bool ally_turn = true, bool test_only = false)
        {
            if (!is_player_turn && ally_turn)
                return false;

            if (Global.game_map.units_waiting())
                AutoTurnEndBlocked = false;
            if (AutoTurnEndBlocked)
                return false;
#if DEBUG
            if (Global.scene.scene_type == "Scene_Map_Unit_Editor")
                return false;
#endif
            if (Global.game_system.preparations)
                return false;
            // Unless in battle or showing a message
            if (!combat_active && !Global.scene.is_message_window_active)
            {
                // If auto ending turns isn't off
                if (Global.game_options.auto_turn_end != 1)
                {
                    bool active_turn_over = Global.game_map.active_team_turn_over;
                    if (!active_turn_over)
                        return false;
                    else if (test_only)
                        return Global.game_options.auto_turn_end == 0;
                    // If any team has units
                    if (Global.game_map.teams.Any(x => x.Any()))
                    {
                        if (active_turn_over && is_map_ready())
                        {
                            if (Global.game_options.auto_turn_end == 0)
                                activate_player_ai();
                            else if (Global.game_options.auto_turn_end == 2)
                            {
                                if (Turn_End_Prompt)
                                {
                                    Turn_End_Prompt = false;
                                    Global.game_temp.map_menu_call = true;
                                    Global.game_temp.menu_call = true;
                                    Global.game_temp.end_turn_highlit = true;
                                }
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal void update_autoend_turn()
        {
            // If autoend turn is not on prompt mode
            if (Global.game_options.auto_turn_end != 2)
                Turn_End_Prompt = false;
            else if (!ally_turn_end_check(test_only: true))
                Turn_End_Prompt = true;
        }

        protected bool turn_change()
        {
            if (Global.game_system.preparations || Global.game_system.is_interpreter_running)
                return false;
            int change_to = determine_next_turn(Previous_Turn);
            if (Skipped_Turns.Count > 0)
                return false;

            if (Turn <= 0)
                Temp_Player_Loc = Global.player.loc;
            Team_Turn = change_to;
            // Add one to turn if active team is same or earlier than previous
            if (Team_Turn <= Previous_Turn || Previous_Turn == 0)
                next_turn();
            // If same team turn
            if (Team_Turn == Previous_Turn)
                Same_Team_Turn = true;
            // Else change to new team's turn theme
            else
            {
                //Global.Audio.clear_map_theme(); //@Debug
                Global.Audio.BgmFadeOut(60);
                play_turn_theme();
            }
            // Is there a reason this resets this to 0
            // Maybe it should use -1 instead //@Debug
            Previous_Turn = -1;
            // Autocursor
            if (is_player_turn)
                Global.player.autocursor(Temp_Player_Loc);
            else
                Global.player.ai_autocursor(Team_Turn);
            Turn_Change = Team_Turn;
            Global.game_map.move_range_visible = true;
            return true;
        }

        protected int determine_next_turn(int change_from)
        {
            if (Global.scene.scene_type == "Scene_Map_Unit_Editor")
                return Constants.Team.PLAYABLE_TEAMS.Min();
            // Checks if any teams have units
            bool all_teams_empty = true;
            foreach (List<int> team in Global.game_map.teams)
            {
                if (team.Count > 0)
                {
                    all_teams_empty = false;
                    break;
                }
            }
            // If there are no units on any team, skip to the next player turn
            if (all_teams_empty)
            {
                Skipped_Turns.Clear();
                for (int i = 0; i < Constants.Team.NUM_TEAMS - 1; i++)
                    Skipped_Turns.Add((Constants.Team.PLAYER_TEAM + i) %
                        (Constants.Team.NUM_TEAMS) + 1);
                return Constants.Team.PLAYER_TEAM;
            }

            int team_max = Constants.Team.NUM_TEAMS;
            int next_turn_value = change_from;
            bool cont = false;
            while (!cont)
            {
                cont = true;
                // Team turn is 0 on turn 0
                if (change_from == Constants.Team.PLAYER_TEAM)
                {
                    Temp_Player_Loc = Global.player.loc;
                }
                next_turn_value = (change_from % team_max) + 1;
                // End turn for all units on the previous team
                foreach (int id in Global.game_map.teams[change_from])
                {
                    Global.game_map.units[id].end_turn();
                }

                if (SkipTeamTurn(next_turn_value))
                {
                    Skipped_Turns.Add(next_turn_value);
                    cont = false;
                    change_from = next_turn_value;
                }
            }
            return next_turn_value;
        }

        private bool SkipTeamTurn(int team)
        {
            // Check if team has any non-rescued units
            return !Global.game_map.teams[team]
                .Any(x => !Global.game_map.units[x].is_rescued);
        }

        protected void next_turn()
        {
            Turn++;
            Global.game_map.next_turn();
            if (is_battle_map)
            {
                Global.game_system.chapter_turn++; // = Turn; //Debug
                // At the start of the first turn
                if (Global.game_system.chapter_turn == 1) // if (Turn == 1) //Debug
                {
#if DEBUG   
                    if (Global.scene.scene_type != "Scene_Map_Unit_Editor")
                    {
#endif
                        Global.game_system.set_gameplay_start();
                        Global.game_system.set_deployed_unit_stats(
                            Global.game_map.teams[Constants.Team.PLAYER_TEAM].Count,
                            Global.battalion.deployed_average_level);
                        if (Metrics != null)
                            Metrics.set_pc_starting_stats(
                                Global.game_map.units
                                    .Values
                                    .Where(x => x.is_ally));
#if DEBUG
                    }
#endif
                }
            }
        }

        public void end_turn()
        {
            if (Constants.Team.PLAYABLE_TEAMS.Contains(Team_Turn))
                activate_player_ai();
            else
                change_turn();
        }

        public void change_turn()
        {
            if (is_player_turn)
                Global.game_map.refresh_move_ranges(true);
            // End of turn events need to run and finish before team turn value changes
            Skipped_Turn_Action = 3;
            Previous_Turn = Team_Turn;
            Changing_Turn = true;
            AutoTurnEndBlocked = false;
            Global.game_map.highlight_test();
            wait_time = 10;
        }

        public void end_preparations()
        {
            Global.game_map.set_default_team_leader();
            wait_time = 2;
        }

        public bool is_player_turn
        {
            get
            {
                return Constants.Team.PLAYABLE_TEAMS.Contains(Team_Turn) ||
                    Global.game_system.preparations;
            }
        }
        #endregion

        public bool is_info_ready
        {
            get
            {
                if (ai_active) return false;
                if (Global.game_map.is_move_range_active && Global.game_system.Selected_Unit_Id != -1) return false;
                if (Global.Input.speed_up_input()) return false;
                if (is_menuing) return false;
                if (Global.game_temp.minimap_call) return false;
                if (!is_map_ready()) return false;
                if (is_changing_turns) return false;
                if (get_scene_map() == null) return false;
                return true;
            }
        }

        public bool hp_gauges_visible
        {
            get
            {
                if (!is_map_ready() || Global.game_temp.menu_call || is_menuing) return false;
                if (is_changing_turns) return false;
                if (get_scene_map() == null) return false;
                return true;
            }
        }

        public bool is_enemy_info_ready
        {
            get
            {
                if (ai_active)
                    return false;
                if (!Global.game_map.is_move_range_active ||
                        Global.game_system.Selected_Unit_Id == -1)// || //Debug
                    //!Global.game_map.units[Global.game_system.Selected_Unit_Id].is_active_team) //Multi
                    return false;
                if (is_menuing) return false;
                if (Global.game_temp.minimap_call) return false;
                if (!is_map_ready()) return false;
                if (is_changing_turns) return false;
                if (get_scene_map() == null) return false;
                return true;
            }
        }

        public bool is_button_description_ready
        {
            get
            {
                if (ai_active) return false;
                if (is_menuing) return false;
                if (Global.game_temp.minimap_call) return false;
                if (!is_map_ready()) return false;
                if (is_changing_turns) return false;
                if (get_scene_map() == null) return false;
                if (get_scene_map().changing_formation && Global.game_system.Selected_Unit_Id != -1) return false;
                return true;
            }
        }

        public bool no_cursor
        {
            get
            {
                if (is_changing_turns) return true;
                if (player_ai_active) return true;
                if (Global.game_temp.minimap_call) return true;
                if (!is_battle_map) return true;
                return false;
            }
        }

        public bool no_highlight()
        {
            return combat_active ||
                //support_active //Debug
                is_changing_turns || visit_active || Global.game_map.units_waiting() ||
                support_convo_active || talk_active || item_active ||
                dance_active || steal_active || block_active || chapter_end_active ||
                Global.game_map.changing_formation || no_cursor;
        }

        public bool is_map_ready()
        {
            return is_map_ready(false);
        }
        public bool is_map_ready(bool lite) //Yeti
        {
            if (combat_active) return false;
            if (item_active) return false;
            if (skills_active) return false;
            if (exp_active) return false;
            if (visit_active) return false;
            if (Global.game_map.units_true_dying)
                return false;

            if (Global.game_map.units_waiting())
                return false;
            if (support_active) return false;
            if (talk_active) return false;
            if (new_turn_active) return false;
            if (rescue_active) return false;
            if (block_active) return false;
            if (chapter_end_active) return false;
            if (shop_suspend_active) return false;
            if (Global.game_map.changing_formation) return false;

            if (Global.game_temp.menu_call) return false;
            if (Global.game_temp.status_menu_call) return false;

            if (get_scene_map() != null)
            if (get_scene_map().is_changing_chapters) return false;
            if (Global.scene.returning_to_title) return false;
            // Simple map ready stuff (input handling's check)
            if (lite) return true;
            if (Global.scene.is_message_window_active) return false;
            Scene_Map scene_map = get_scene_map();
            if (scene_map != null)
            {
                if (Global.game_system.is_interpreter_running) return false;
                if (scene_map.is_minimap_busy()) return false;
                if (Global.return_to_title) return false;
            }
            return true;
        }
    }
}
