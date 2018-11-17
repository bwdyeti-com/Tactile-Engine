using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using FEXNAVector2Extension;

namespace FEXNA
{
    internal class Player : Map_Object
    {
        public static int cursor_anim_count = 0;

        protected Directions Dir8_Last_Frame = Directions.None;
        protected int Move_Timer = 0, Move_Sound_Timer = 0;
        protected bool Instant_Movement = true;
        protected int Target_Timer = -1;
        protected bool Next_Unit = false;
        protected int Following_Unit_Id = -1;

        protected DirectionFlags LockedInputs = DirectionFlags.None;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            Loc.write(writer);
            Real_Loc.write(writer);
            writer.Write(Facing);
            writer.Write(Frame);
            writer.Write((int)Dir8_Last_Frame);
            writer.Write(Move_Timer);
            writer.Write(Instant_Movement);
            writer.Write(Target_Timer);
        }

        public void read(BinaryReader reader)
        {
            Loc = Loc.read(reader);
            Real_Loc = Real_Loc.read(reader);
            Facing = reader.ReadInt32();
            Frame = reader.ReadInt32();
            Dir8_Last_Frame = (Directions)reader.ReadInt32();
            Move_Timer = reader.ReadInt32();
            Instant_Movement = reader.ReadBoolean();
            Target_Timer = reader.ReadInt32();
        }
        #endregion

        #region Accessors
        new public Vector2 loc
        {
            get { return Loc; }
            set { Loc = value; }
        }

        public int facing
        {
            set { Facing = value; }
        }

        public int frame
        {
            set { Frame = value; }
        }
        
        public bool instant_move
        {
            get
            {
                return Instant_Movement;
            }
            set { Instant_Movement = value; }
        }

        public bool next_unit
        {
            get { return Next_Unit; }
            set { Next_Unit = value; }
        }

        public int following_unit_id
        {
            get { return Following_Unit_Id; }
            set
            {
                // Only set to the new value if currently not following anyone
                if (!is_following_unit)
                    Following_Unit_Id = value;
            }
        }

        public bool is_following_unit { get { return Following_Unit_Id != -1; } }
        #endregion

        public Player()
        {
            Loc = Vector2.Zero;
            Facing = 6;
            Frame = 1;
        }

        #region Update
        public void update()
        {
            if (!Instant_Movement && Next_Unit)
            {
                if (!Global.game_map.scrolling)
                {
                    Next_Unit = false;
                }
            }
            if (is_movement_allowed())
                update_input();
            update_movement();
            if (Global.game_temp.minimap_call &&
                Input.ControlScheme != ControlSchemes.Touch)
            {
                //center(Loc);//Real_Loc / UNIT_TILE_SIZE); //Debug
                center(Real_Loc / UNIT_TILE_SIZE);
                Global.game_map.scroll_speed = (UNIT_TILE_SIZE / 4) * (speed_up_input() ? 2 : 1);
            }
        }

        internal bool is_movement_allowed()
        {
            if (Global.game_state.is_menuing &&
                    ((Scene_Map)Global.scene).target_window_up &&
                    (Input.ControlScheme != ControlSchemes.Buttons ||
                        ((Scene_Map)Global.scene).manual_targeting))
                return true;

            return (!(Global.game_temp.menu_call ||
                Global.game_state.is_menuing ||
                !(Global.game_system.preparations ||
                    (Global.game_state.is_player_turn &&
                    !Global.game_state.is_changing_turns)) ||
                Global.game_state.ai_active ||
                !Global.game_state.is_map_ready() ||
                Global.scene.is_message_window_active ||
                Global.game_temp.status_menu_call ||
                Global.game_system.is_interpreter_running ||
                Next_Unit)); //Yeti
        }

        private Vector2 mouse_loc
        {
            get
            {
                return Global.game_map.screen_loc_to_tile(
                    Global.Input.mousePosition);
            }
        }
        internal bool at_mouse_loc { get { return Loc == mouse_loc; } }

        private Vector2 touch_loc
        {
            get
            {
                return Global.game_map.screen_loc_to_tile(
                    Global.Input.touchPressPosition);
            }
        }
        internal bool at_touch_loc { get { return Loc == touch_loc; } }

        protected void update_input()
        {
            bool not_minimap_centered = false;

            if (Input.IsControllingOnscreenMouse &&
                (Global.game_map.scrolling || Global.Input.mouseMoved))
            {
                Vector2 target_loc = mouse_loc;
                if (should_move_to_direct_input_loc(ref target_loc))
                {
                    //Global.game_system.play_se(System_Sounds.Cursor_Move); //Debug

                    if (Global.game_temp.minimap_call)
                    {
                        if (Loc != center_cursor_loc(false))
                            not_minimap_centered = true;
                    }

                    Instant_Movement = true;
                    Loc = target_loc; //new Vector2((int)target_loc.X, (int)target_loc.Y); //already made made int values //Debug
                    ((Scene_Map)Global.scene).update_info_image(true);

                    if (not_minimap_centered)
                        center_cursor();
                }
                return;
            }
            else if (Input.ControlScheme == ControlSchemes.Touch)
            {
                bool not_manual_target_window = Global.game_state.is_menuing &&
                    ((Scene_Map)Global.scene).target_window_up &&
                    !((Scene_Map)Global.scene).manual_targeting;

                if (!not_manual_target_window &&
                    Global.Input.touch_triggered() && 
                    Global.game_map.screen_loc_in_view(
                        Global.Input.touchPressPosition))
                {
                    Vector2 target_loc = touch_loc;
                    if (should_move_to_direct_input_loc(ref target_loc))
                    {
                        Instant_Movement = true;
                        Loc = target_loc; //new Vector2((int)target_loc.X, (int)target_loc.Y); //already made made int values //Debug
                        ((Scene_Map)Global.scene).update_info_image(true);
                    }
                }

                if (Global.Input.gesture_triggered(TouchGestures.FreeDrag))
                {
                    Global.game_map.scroll_vel(
                        -Global.Input.freeDragVector / TILE_SIZE);
                }
                return;
            }
            
            Directions dir8 = Global.Input.dir8();
            if (dir8 == Directions.None)
                LockedInputs = DirectionFlags.None;
            if (LockedInputs != DirectionFlags.None)
            {
                if (Input.direction_to_flag(dir8) == LockedInputs)
                    dir8 = Directions.None;
            }
            if ((dir8 != Directions.None || !is_on_square) && Global.game_temp.minimap_call)
            {
                if (Loc != center_cursor_loc(false))
                    not_minimap_centered = true;
            }
            if (dir8 != Dir8_Last_Frame)
                Move_Timer = 0;
            bool moved = false;
            if (is_on_square)
                switch (dir8)
                {
                    case Directions.DownLeft:
                        moved = move_down_left();
                        break;
                    case Directions.Down:
                        moved = move_down();
                        break;
                    case Directions.DownRight:
                        moved = move_down_right();
                        break;
                    case Directions.Left:
                        moved = move_left();
                        break;
                    case Directions.Right:
                        moved = move_right();
                        break;
                    case Directions.UpLeft:
                        moved = move_up_left();
                        break;
                    case Directions.Up:
                        moved = move_up();
                        break;
                    case Directions.UpRight:
                        moved = move_up_right();
                        break;
                }
            if (moved)
                start_movement();
            if (is_on_square)
                Move_Timer += 1;
            Dir8_Last_Frame = dir8;
            if (Global.game_temp.minimap_call && (dir8 != 0 || !is_on_square))
            {
                if (not_minimap_centered)
                    center_cursor();
                //center(Loc);//Real_Loc / UNIT_TILE_SIZE); //Debug
                //center(Real_Loc / UNIT_TILE_SIZE);
                //Global.game_map.scroll_speed = (UNIT_TILE_SIZE / 4) * (speed_up_input() ? 2 : 1);
            }
        }

        /// <summary>
        /// Checks if should move to a location based on direct input, such as from mouse or touch.
        /// </summary>
        private bool should_move_to_direct_input_loc(ref Vector2 target_loc)
        {
            bool must_be_playable_map = true;
#if DEBUG
            if (Global.scene.scene_type == "Scene_Map_Unit_Editor")
                must_be_playable_map = false;
#endif
            if (must_be_playable_map && Global.game_map.is_off_map(
                target_loc, must_be_playable_map))
            {
                target_loc = Global.game_map.on_map(target_loc);
            }
            return (Loc != target_loc &&
                !Global.game_map.is_off_map(target_loc, must_be_playable_map));
        }

        protected void update_cursor_anim()
        {
            // Highlighting
            bool highlighting = false;
            Game_Unit unit = null;
            if (!Global.game_map.is_off_map(Loc, false))
                unit = Global.game_map.get_unit(Loc);
            if (!(unit == null))
                if (unit.highlighted && !Global.game_system.preparations)
                    if (unit.is_active_player_team) //Multi
                        highlighting = true;
            unit = null;
            // Selecting
            if (Global.game_system.Selected_Unit_Id != -1)
            {
                unit = Global.game_map.units[Global.game_system.Selected_Unit_Id];
            }
            if (update_target())
            {
                // If player turn and target window not up
                if (Global.game_state.is_player_turn && !((Scene_Map)Global.scene).target_window_up)
                {
                    // Invisible in battle/staff use/etc
                    if (!Global.game_state.is_map_ready() || Global.game_temp.menu_call || Global.game_state.is_menuing || Global.game_state.no_cursor)
                    {
                        Facing = 6;
                        Frame = 1;
                    }
                    // Formation change thing
                    else if (Global.game_system.preparations && ((Scene_Map)Global.scene).changing_formation && Global.game_system.Selected_Unit_Id > -1)
                    {
                        Facing = 6;
                        if (Global.game_map.deployment_points.Contains(Loc))
                            Frame = 2;
                        else
                            update_cursor_formation_frame();
                    }
#if !MONOGAME && DEBUG
                    else if (Global.game_state.moving_editor_unit)
                    {
                        Facing = 6;
                        if (Global.game_map.is_off_map(Loc, false) || Global.game_map.get_unit(Loc) == null)
                            Frame = 2;
                        else
                            update_cursor_formation_frame();
                    }
#endif
                    else if (highlighting)
                    {
                        Facing = 6;
                        Frame = 0;
                    }
                    else if (Global.game_state.is_player_turn && !Global.game_state.no_cursor)
                    {
                        Facing = 2;
                    }
                }
            }
            if (Facing < 6)
            {
                update_cursor_frame();
            }
            update_anim_counter();
        }

        public void update_cursor_frame()
        {
            if (cursor_anim_count >= 0 && cursor_anim_count < 20)
                Frame = 0;
            else if (cursor_anim_count >= 20 && cursor_anim_count < 22)
                Frame = 1;
            else if (cursor_anim_count >= 22 && cursor_anim_count < 30)
                Frame = 2;
            else if (cursor_anim_count >= 30 && cursor_anim_count < 32)
                Frame = 3;
        }

        protected void update_cursor_formation_frame()
        {
            if (cursor_anim_count >= 0 && cursor_anim_count < 8)
                Frame = 2;
            else if (cursor_anim_count >= 8 && cursor_anim_count < 32)
                Frame = 3;
        }

        protected bool update_target()
        {
            if (Target_Timer > -1)
            {
                // Should the cursor be a different color during events/pre-chapter? //Debug
                Facing = 4; //battle_map? 4 : 2 //Yeti
                return false;
            }
            return true;
        }
        
        public void target_tile(Vector2 target_loc)
        {
            target_tile(target_loc, 48);
        }
        public void target_tile(Vector2 target_loc, int time)
        {
            Target_Timer = time;
            Loc = target_loc;
            refresh_real_loc();
        }

        public void cancel_target_tile()
        {
            if (Target_Timer > -1)
            {
                Target_Timer = 0;
            }
        }

        public bool is_targeting()
        {
            return Target_Timer > 0;
        }

        public void center_cursor()
        {
            center_cursor(false);
        }
        public void center_cursor(bool fix_center)
        {
            loc = center_cursor_loc(fix_center);
            refresh_real_loc();
        }
        private Vector2 center_cursor_loc(bool fix_center)
        {
            Vector2 loc = Loc;
            if (fix_center)
            {
                loc = Global.game_map.display_loc / TILE_SIZE +
                    new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / (TILE_SIZE * 2);
                loc = new Vector2((int)loc.X, (int)loc.Y);
            }
            int map_width = Global.game_map.width,
                map_height = Global.game_map.height;
            int max_x, min_x, max_y, min_y;
            // Centers cursor on the x
            if (Config.WINDOW_WIDTH / TILE_SIZE > map_width)
            {
                // If the screen is bigger than the map
                max_x = Math.Min(
                    Config.WINDOW_WIDTH / (TILE_SIZE * 2),
                    map_width - Global.game_map.edge_offset_right);
                min_x = map_width / 2;
            }
            else
            {
                max_x = map_width - Config.WINDOW_WIDTH / (TILE_SIZE * 2);
                min_x = Config.WINDOW_WIDTH / (TILE_SIZE * 2);
                if (Global.game_options.controller == 2)
                {
                    max_x = max_x - Global.game_map.edge_offset_right;
                    min_x = min_x + Global.game_map.edge_offset_left;
                }
            }
            loc.X = MathHelper.Clamp(loc.X, min_x, max_x);

            // Centers cursor on the y
            if (Config.WINDOW_HEIGHT / TILE_SIZE > map_height)
            {
                // If the screen is bigger than the map
                max_y = Math.Min(
                    Config.WINDOW_HEIGHT / (TILE_SIZE * 2),
                    map_height - Global.game_map.edge_offset_bottom);
                min_y = map_height / 2;
            }
            else
            {
                max_y = map_height - Config.WINDOW_HEIGHT / (TILE_SIZE * 2);
                min_y = Config.WINDOW_HEIGHT / (TILE_SIZE * 2);
                if (Global.game_options.controller == 2)
                {
                    max_y = max_y - Global.game_map.edge_offset_bottom;
                    min_y = min_y + Global.game_map.edge_offset_top;
                }
            }
            loc.Y = MathHelper.Clamp(loc.Y, min_y, max_y);

            return loc;
        }

        public static void update_anim()
        {
            cursor_anim_count = (cursor_anim_count + 1) % Config.CURSOR_TIME;
        }

        protected void update_anim_counter()
        {
            if (Target_Timer > -1)
            {
                if (!Global.game_system.is_interpreter_running && Global.Input.pressed(Inputs.A))
                    Target_Timer = 0;

                if (Target_Timer == 0)
                {
                    Facing = 6;
                    Frame = 1;
                }
                Target_Timer--;
            }
        }

        public void update_movement()
        {
            if (is_following_unit)
            {
                update_following_unit();
                return;
            }
            bool target_window_up = ((Scene_Map)Global.scene).target_window_up && !((Scene_Map)Global.scene).manual_targeting;
            bool block_scroll =
                Input.ControlScheme == ControlSchemes.Touch &&
                (is_movement_allowed() || Global.game_temp.menu_call ||
                Global.game_temp.menuing);

            if (Instant_Movement || !Global.game_state.is_player_turn)
            {
                Instant_Movement = false;
                refresh_real_loc();
                if (!Global.game_system.is_interpreter_running)
                    if (center(Real_Loc / UNIT_TILE_SIZE, true, forced: !block_scroll))
                        Global.game_map.scroll_speed = -1;
            }
            else if (!target_window_up)
            {
                if (Global.game_state.is_menuing && !((Scene_Map)Global.scene).manual_targeting)
                    refresh_real_loc();
                else
                {
                    bool moved = (int)Real_Loc.X != (int)Loc.X * UNIT_TILE_SIZE ||
                        (int)Real_Loc.Y != (int)Loc.Y * UNIT_TILE_SIZE;
                    int dist = (UNIT_TILE_SIZE / 4) * (speed_up_input() ? 2 : 1);
                    if (Global.game_temp.minimap_call)
                        dist = (UNIT_TILE_SIZE / 2);
                    else
                    {
                        // If just started moving
                        if (moved && Real_Loc.X % UNIT_TILE_SIZE == 0 && Real_Loc.Y % UNIT_TILE_SIZE == 0)
                            if (Global.game_state.is_player_turn && !Global.game_state.no_cursor)
                            {
                                Global.game_system.play_se(System_Sounds.Cursor_Move);
                                Move_Sound_Timer = 4;
                                if (Global.scene.is_strict_map_scene)
                                    ((Scene_Map)Global.scene).update_info_image(true);
                            }
                    }
                    Vector2 real_loc = new Vector2(Additional_Math.int_closer((int)Real_Loc.X, (int)Loc.X * UNIT_TILE_SIZE, dist),
                        Additional_Math.int_closer((int)Real_Loc.Y, (int)Loc.Y * UNIT_TILE_SIZE, dist));
                    if (Real_Loc != real_loc)
                        Real_Loc = real_loc;
                    center(Real_Loc / UNIT_TILE_SIZE, true, forced: !block_scroll);
                    if (moved)
                        Global.game_map.scroll_speed = dist;
                }
            }
            else
            {
                Real_Loc.X = (int)(Real_Loc.X + Loc.X * UNIT_TILE_SIZE) / 2;
                Real_Loc.Y = (int)(Real_Loc.Y + Loc.Y * UNIT_TILE_SIZE) / 2;
                center(Real_Loc / UNIT_TILE_SIZE, true, forced: !block_scroll);
            }
        }
        private void update_following_unit()
        {
            Game_Unit unit = Global.game_map.units[Following_Unit_Id];
            Vector2 real_loc = unit.update_player_following_movement();

            int dist = (int)Math.Max(Math.Abs(Real_Loc.X - real_loc.X), Math.Abs(Real_Loc.Y - real_loc.Y));

            bool moved = (int)Real_Loc.X != (int)real_loc.X ||
                    (int)Real_Loc.Y != (int)real_loc.Y;
            Real_Loc = real_loc;

            center(Real_Loc / UNIT_TILE_SIZE, true);
            if (moved)
                Global.game_map.scroll_speed = dist;

            if (unit.move_route_empty && (Loc == Real_Loc / UNIT_TILE_SIZE || !moved))
                Following_Unit_Id = -1;
        }

        protected bool speed_up_input()
        {
            // Allow also using B? //Yeti
            return Global.Input.speed_up_input() || Global.game_temp.minimap_call;
        }

        protected void range_edge_check()
        {
            if (Global.game_temp.menuing)
                return;
            int selected_unit_id = Global.game_system.Selected_Unit_Id;
            if (selected_unit_id == -1)
                return;
            //@Debug
            //if (!Global.game_map.active_team.Contains(selected_unit_id))
            //    return; //Multi
            Vector2 test_vector = Vector2.Zero;
            Directions dir = Global.Input.dir8();
            switch (dir)
            {
                // Down left
                case Directions.DownLeft:
                    test_vector = new Vector2(is_left_edge() ? 0 : -1, is_bottom_edge() ? 0 : 1);
                    break;
                // Down
                case Directions.Down:
                    test_vector = new Vector2(0, 1);
                    break;
                // Down right
                case Directions.DownRight:
                    test_vector = new Vector2(is_right_edge() ? 0 : 1, is_bottom_edge() ? 0 : 1);
                    break;
                // Left
                case Directions.Left:
                    test_vector = new Vector2(-1, 0);
                    break;
                // Right
                case Directions.Right:
                    test_vector = new Vector2(1, 0);
                    break;
                // Up left
                case Directions.UpLeft:
                    test_vector = new Vector2(is_left_edge() ? 0 : -1, is_on_top() ? 0 : -1);
                    break;
                // Up
                case Directions.Up:
                    test_vector = new Vector2(0, -1);
                    break;
                // Up right
                case Directions.UpRight:
                    test_vector = new Vector2(is_right_edge() ? 0 : 1, is_top_edge() ? 0 : -1);
                    break;
            }
            if (!Global.game_map.move_range.Contains(Loc + test_vector) &&
                Global.game_map.move_range.Contains(Loc))
            {
                LockedInputs = Input.direction_to_flag(dir);
            }
        }

        protected bool is_bottom_edge()
        {
#if !MONOGAME && DEBUG
            if (Global.scene is Scene_Map_Unit_Editor && Global.game_options.controller != 2)
                return Loc.Y >= Global.game_map.height - 1;
#endif
            return Loc.Y >= Global.game_map.height - (Global.game_map.edge_offset_bottom + 1);
        }
        protected bool is_left_edge()
        {
#if !MONOGAME && DEBUG
            if (Global.scene is Scene_Map_Unit_Editor && Global.game_options.controller != 2)
                return Loc.X <= 0;
#endif
            return Loc.X <= Global.game_map.edge_offset_left;
        }
        protected bool is_right_edge()
        {
#if !MONOGAME && DEBUG
            if (Global.scene is Scene_Map_Unit_Editor && Global.game_options.controller != 2)
                return Loc.X >= Global.game_map.width - 1;
#endif
            return Loc.X >= Global.game_map.width - (Global.game_map.edge_offset_right + 1);
        }
        protected bool is_top_edge()
        {
#if !MONOGAME && DEBUG
            if (Global.scene is Scene_Map_Unit_Editor && Global.game_options.controller != 2)
                return Loc.Y <= 0;
#endif
            return Loc.Y <= Global.game_map.edge_offset_top;
        }
        #endregion

        public override void force_loc(Vector2 loc)
        {
            force_loc(loc, true);
        }
        public void force_loc(Vector2 loc, bool forceCenter)
        {
            base.force_loc(loc);
            center(Loc, true, forced: forceCenter);
        }

        public bool center(bool event_called = false)
        {
            return center(Loc, event_called: event_called, forced: true);
        }
        private bool center(
            Vector2 loc, bool soft_center = false,
            bool event_called = false, bool forced = true)
        {
            return Global.game_map.center(
                loc, soft_center, event_called, forced,
                can_pass_edges: false);
        }

        protected override Vector2 real_loc_on_map()
        {
            if (Input.ControlScheme == ControlSchemes.Mouse &&
                    Global.scene is Scene_Map &&
                    (Global.scene as Scene_Map).target_window_up &&
                    !(Global.scene as Scene_Map).manual_targeting)
                return (Global.scene as Scene_Map).target_window_last_target_loc *
                    UNIT_TILE_SIZE;
            return base.real_loc_on_map();
        }

        #region Movement
        protected bool move_down()
        {
            if (cursor_can_move())
            {
                if (!is_bottom_edge())
                {
                    Loc.Y += 1;
                    return true;
                }
            }
            return false;
        }

        protected bool move_left()
        {
            if (cursor_can_move())
            {
                if (!is_left_edge())
                {
                    Loc.X -= 1;
                    return true;
                }
            }
            return false;
        }

        protected bool move_right()
        {
            if (cursor_can_move())
            {
                if (!is_right_edge())
                {
                    Loc.X += 1;
                    return true;
                }
            }
            return false;
        }

        protected bool move_up()
        {
            if (cursor_can_move())
            {
                if (!is_top_edge())
                {
                    Loc.Y -= 1;
                    return true;
                }
            }
            return false;
        }

        protected bool move_down_left()
        {
            bool vert = move_down();
            bool hori = move_left();
            return (vert || hori);
        }

        protected bool move_down_right()
        {
            bool vert = move_down();
            bool hori = move_right();
            return (vert || hori);
        }

        protected bool move_up_left()
        {
            bool vert = move_up();
            bool hori = move_left();
            return (vert || hori);
        }

        protected bool move_up_right()
        {
            bool vert = move_up();
            bool hori = move_right();
            return (vert || hori);
        }

        protected bool cursor_can_move()
        {
            return (Move_Timer == 0 || Move_Timer >= 10 || speed_up_input());
        }

        protected void start_movement()
        {
            if (speed_up_input())
                Move_Timer = 0;
            else
                Move_Timer++;
            range_edge_check();
        }
        #endregion

        #region Autocursor
        public void autocursor(Vector2 loc, int temp_auto = -1, bool soft_center = true)
        {
            if (temp_auto == -1)
                temp_auto = Global.game_options.autocursor;
            switch (temp_auto)
            {
                case 0:
                    if (Global.game_map.active_team.Count > 0) //Multi
                    {
                        int id = Global.game_map.active_team.Contains(Global.game_map.team_leaders[Global.game_state.team_turn]) ?
                            Global.game_map.team_leaders[Global.game_state.team_turn] : Global.game_map.active_team.Min();
                        if (Global.game_map.units[id].is_rescued) //Multi
                            id = Global.game_map.units[id].rescued;
                        loc = Global.game_map.units[id].loc; //Multi
                    }
                    break;
                case 1:
                    break;
                case 2:
                    foreach (int id in Global.game_map.active_team) //Multi
                        if (Global.game_map.units[id].actor.name == "Madelyn")
                            loc = Global.game_map.units[id].loc;
                    break;
            }
            force_loc(loc);
            if (Global.game_map.center(Loc, soft_center, forced: true, can_pass_edges: false))
                Global.game_map.scroll_speed = -1;
        }

        public void ai_autocursor(int team)
        {
            int unit_id = Global.game_map.highest_priority_unit(team);
            if (unit_id != -1)
            {
                Loc = Global.game_map.units[unit_id].loc_on_map(); //.loc; // In case they're rescued //Debug
                if (center(Loc, true))
                    Global.game_map.scroll_speed = -1;
            }
        }
        #endregion

        #region Sprite Handling
        public override void update_sprite(Sprite sprite)
        {
            update_cursor_anim();
            base.update_sprite(sprite);
            // Update player sprite
            sprite.update();
            sprite.frame = (Facing / 2 - 1) * 4 + Frame;
            sprite.draw_offset = new Vector2(TILE_SIZE, TILE_SIZE) / 2;

            sprite.visible = true;
            if (Input.ControlScheme == ControlSchemes.Mouse && Facing == 2 &&
                Global.game_map.get_selected_unit() == null)
            {
                sprite.visible = false;
            }
        }
        #endregion
    }
}