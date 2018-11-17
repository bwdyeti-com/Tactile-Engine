using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using ListExtension;
using FEXNAListExtension;

namespace FEXNA
{
    partial class Game_Map
    {
        protected int Selected_Move_Total = 0;
        protected bool Show_All_Enemy_Range = false;
        protected List<int> Range_Enemies = new List<int>();
        protected Dictionary<Vector2, int> All_Enemy_Displayed_Attack = new Dictionary<Vector2, int>();
        protected Dictionary<Vector2, int> All_Enemy_Displayed_Staff = new Dictionary<Vector2, int>();
        protected Dictionary<Vector2, int> Enemy_Displayed_Attack = new Dictionary<Vector2, int>();
        protected Dictionary<Vector2, int> Enemy_Displayed_Staff = new Dictionary<Vector2, int>();

        protected List<int> Updated_Move_Range_Units = new List<int>();
        protected List<int> Updated_Attack_Range_Units = new List<int>();
        protected List<int> Updated_Staff_Range_Units = new List<int>();
        private object Move_Range_Lock = new object();

        #region Serialization
        public void move_range_write(BinaryWriter writer)
        {
            writer.Write(Show_All_Enemy_Range);
            Range_Enemies.write(writer);
        }

        public void move_range_read(BinaryReader reader)
        {
            Show_All_Enemy_Range = reader.ReadBoolean();
            Range_Enemies.read(reader);
        }
        #endregion

        #region Accessors
        public Dictionary<Vector2, int> all_enemy_displayed_attack { get { return All_Enemy_Displayed_Attack; } }
        public Dictionary<Vector2, int> all_enemy_displayed_staff { get { return All_Enemy_Displayed_Staff; } }
        public Dictionary<Vector2, int> enemy_displayed_attack { get { return Enemy_Displayed_Attack; } }
        public Dictionary<Vector2, int> enemy_displayed_staff { get { return Enemy_Displayed_Staff; } }

        public List<int> range_enemies { get { return Range_Enemies; } }

        public List<int> updated_move_range_units { get { return Updated_Move_Range_Units; } }
        public List<int> updated_attack_range_units { get { return Updated_Attack_Range_Units; } }
        public List<int> updated_staff_range_units { get { return Updated_Staff_Range_Units; } }
        #endregion

        public void show_move_range(int id)
        {
            Game_Unit unit = units[id];
            check_update_unit_move_range(unit);
            Move_Range.UnionWith(unit.move_range);
            //Move_Range = Move_Range.Distinct().ToList(); //ListOrEquals //HashSet
            if (unit.is_active_team)
            {
                Talk_Range.UnionWith(unit.talk_range);
                //Talk_Range = Talk_Range.Distinct().ToList(); //ListOrEquals //HashSet
            }

            Selected_Move_Total = 0;
            range_start_timer = 0;
        }

        public void show_attack_range(int id)
        {
            Game_Unit unit = units[id];

            lock (Move_Range_Lock)
            {
                Attack_Range.UnionWith(unit.attack_range);
                //Attack_Range = Attack_Range.Distinct().ToList(); //ListOrEquals //HashSet
                Staff_Range.UnionWith(unit.staff_range);
                //Staff_Range = Staff_Range.Distinct().ToList(); //ListOrEquals //HashSet

                //Attack_Range = Attack_Range.Except(Talk_Range).ToList();
                //Attack_Range = Attack_Range.Except(Move_Range).ToList();
                //Staff_Range = Staff_Range.Except(Talk_Range).ToList();
                //Staff_Range = Staff_Range.Except(Move_Range).ToList();
                //Staff_Range = Staff_Range.Except(Attack_Range).ToList();
                Attack_Range.ExceptWith(Talk_Range); //HashSet
                Attack_Range.ExceptWith(Move_Range);
                Staff_Range.ExceptWith(Talk_Range);
                Staff_Range.ExceptWith(Move_Range);
                Staff_Range.ExceptWith(Attack_Range);
                Selected_Move_Total = 0;
            }
        }

        internal void check_update_unit_move_range(Game_Unit unit)
        {
            if (!Updated_Move_Range_Units.Contains(unit.id))
                unit.update_move_range();
        }

        #region Move Range Update Checking
        public void add_updated_move_range(int id)
        {
            if (!Updated_Move_Range_Units.Contains(id))
                Updated_Move_Range_Units.Add(id);
        }
        public void add_updated_attack_range(int id)
        {
            if (!Updated_Attack_Range_Units.Contains(id))
                Updated_Attack_Range_Units.Add(id);
        }
        public void add_updated_staff_range(int id)
        {
            if (!Updated_Staff_Range_Units.Contains(id))
                Updated_Staff_Range_Units.Add(id);
        }

        public void remove_updated_move_range(int id)
        {
            Updated_Move_Range_Units.Remove(id);
            remove_updated_attack_range(id);
            remove_updated_staff_range(id);
        }
        public void remove_updated_attack_range(int id)
        {
            Updated_Attack_Range_Units.Remove(id);
        }
        public void remove_updated_staff_range(int id)
        {
            Updated_Staff_Range_Units.Remove(id);
        }

        public void clear_updated_move_ranges()
        {
            Updated_Move_Range_Units.Clear();
            clear_updated_attack_ranges();
            clear_updated_staff_ranges();
        }
        public void clear_updated_attack_ranges()
        {
            Updated_Attack_Range_Units.Clear();
        }
        public void clear_updated_staff_ranges()
        {
            Updated_Staff_Range_Units.Clear();
        }
        #endregion

        #region Map tests
        public bool is_off_map(Vector2 loc)
        {
            return is_off_map(loc, true);
        }
        public bool is_off_map(Vector2 loc, bool restrict_to_playable)
        {
            if (is_off_map_x(loc.X, restrict_to_playable))
                return true;
            if (is_off_map_y(loc.Y, restrict_to_playable))
                return true;
            return false;
        }
        public bool is_off_map_x(float x)
        {
            return is_off_map_x(x, true);
        }
        public bool is_off_map_x(float x, bool restrict_to_playable)
        {
            if (restrict_to_playable)
            {
                if (x < edge_offset_left) return true;
                if ((x + edge_offset_right) >= this.width) return true;
            }
            else
            {
                if (x < 0) return true;
                if (x >= this.width) return true;
            }
            return false;
        }
        public bool is_off_map_y(float y)
        {
            return is_off_map_y(y, true);
        }
        public bool is_off_map_y(float y, bool restrict_to_playable)
        {
            if (restrict_to_playable)
            {
                if (y < edge_offset_top) return true;
                if ((y + edge_offset_bottom) >= this.height) return true;
            }
            else
            {
                if (y < 0) return true;
                if (y >= this.height) return true;
            }
            return false;
        }

        public bool is_off_map_edge_x(float x)
        {
            if (x < -1) return true;
            if (x > this.width) return true;
            return false;
        }
        public bool is_off_map_edge_y(float y)
        {
            if (y < -1) return true;
            if (y > this.height) return true;
            return false;
        }

        public bool is_off_screen(Vector2 loc)
        {
            if (loc.X < Target_Display_X) return true;
            if (loc.Y < Target_Display_Y) return true;
            if (loc.X >= Target_Display_X + (Config.WINDOW_WIDTH / TILE_SIZE)) return true;
            if (loc.Y >= Target_Display_Y + (Config.WINDOW_HEIGHT / TILE_SIZE)) return true;
            return false;
        }

        public Vector2 on_map(Vector2 loc)
        {
            Vector2 result = new Vector2(
                MathHelper.Clamp(loc.X, edge_offset_left,
                    this.width - (edge_offset_right + 1)),
                MathHelper.Clamp(loc.Y, edge_offset_top,
                    this.height - (edge_offset_bottom + 1)));
            return result;
        }

        public bool is_blocked(Vector2 loc, int id)
        {
            return is_blocked(loc, id, Fow);
        }
        public bool is_blocked(Vector2 loc, int id, bool fow)
        {
            if (is_off_map(loc))
            {
#if DEBUG
                Print.message("immoissubke location[" + loc.X.ToString() + ", " + loc.Y.ToString() + "]");
#endif
                return true;
            }
            Game_Unit unit = get_unit(loc);
            // Falsely returns that no unit is at this tile if the unit can't see it
            if (unit != null && fow && !unit.visible_by(this.units[id].team))
                unit = null;
            // If there is a unit here and it's not the tested one
            if (unit != null && unit.id != id)
                return true;
            // Block moving onto light runes
            if (get_light_rune(loc) != null)
                return true;
            return false;
        }
        #endregion

        #region Attack Range
        public HashSet<Vector2> get_unit_range(HashSet<Vector2> move_range, int min_range, int max_range)
        {
            // Where is Config.BLOCK_FIRE_THROUGH_WALLS_DEFAULT even used
            return get_unit_range(move_range, min_range, max_range,
                Constants.Gameplay.BLOCK_FIRE_THROUGH_WALLS_DEFAULT);
        }
        public HashSet<Vector2> get_unit_range(HashSet<Vector2> move_range, int min_range, int max_range, bool walls)
        {
            if (move_range.Count == 0) return new HashSet<Vector2>();
            if (min_range == 0 || max_range == 0) return new HashSet<Vector2>();
            return Pathfind.get_range_around(move_range, max_range, min_range, walls);
        }

        public HashSet<Vector2> remove_blocked(HashSet<Vector2> move_range, int id)
        {
            return remove_blocked(move_range, id, false);
        }
        public HashSet<Vector2> remove_blocked(HashSet<Vector2> move_range, int id, bool ignore_fog_tiles)
        {
            HashSet<Vector2> list = new HashSet<Vector2>();
            foreach (Vector2 loc in move_range)
            {
                if ((ignore_fog_tiles ? Fow_Visibility[Constants.Team.PLAYER_TEAM].Contains(loc) : true) && is_blocked(loc, id, false)) // Might cause problems for AI //Debug
                    list.Add(loc);
            }
            move_range.ExceptWith(list);
            return move_range;
        }
        #endregion

        #region Enemy Ranges
        protected bool reset_enemy_range_data()
        {
            bool result = Show_All_Enemy_Range || Range_Enemies.Count > 0;
            Show_All_Enemy_Range = false;
            Range_Enemies.Clear();
            All_Enemy_Displayed_Attack.Clear();
            All_Enemy_Displayed_Staff.Clear();
            Enemy_Displayed_Attack.Clear();
            Enemy_Displayed_Staff.Clear();
            update_enemy_range();
            return result;
        }
        protected bool reset_individual_enemy_range_data()
        {
            bool result = Range_Enemies.Count > 0;
            if (!result) return result;
            Range_Enemies.Clear();
            All_Enemy_Displayed_Attack.Clear();
            All_Enemy_Displayed_Staff.Clear();
            Enemy_Displayed_Attack.Clear();
            Enemy_Displayed_Staff.Clear();
            update_enemy_range();
            return result;
        }

        protected bool toggle_enemy_range(int id)
        {
            if (Range_Enemies.Contains(id))
                remove_enemy_attack_range(id);
            else
            {
                if (this.units[id].attack_range.Count > 0 || this.units[id].staff_range.Count > 0)
                {
                    add_enemy_attack_range(id);
                    return true;
                }
            }
            return false;
        }

        protected bool toggle_all_enemy_range()
        {
            Show_All_Enemy_Range = !Show_All_Enemy_Range;
            update_enemy_range();
            if (Show_All_Enemy_Range)
            {
                if (All_Enemy_Displayed_Attack.Count > 0 || All_Enemy_Displayed_Staff.Count > 0 ||
                        Enemy_Displayed_Attack.Count > 0 || Enemy_Displayed_Staff.Count > 0)
                    return true;
                else
                    Show_All_Enemy_Range = false;
            }
            return false;
        }

        protected void add_enemy_attack_range(int id)
        {
            if (!Range_Enemies.Contains(id))
            {
                Range_Enemies.Add(id);
                add_enemy_ranges(id, Enemy_Displayed_Attack, Enemy_Displayed_Staff);
                refresh_enemy_range_frames(Enemy_Displayed_Attack, Enemy_Displayed_Staff);
                foreach (KeyValuePair<Vector2, int> pair in Enemy_Displayed_Attack)
                    All_Enemy_Displayed_Attack.Remove(pair.Key);
                foreach (KeyValuePair<Vector2, int> pair in Enemy_Displayed_Staff)
                    All_Enemy_Displayed_Staff.Remove(pair.Key);
            }
        }

        protected void remove_enemy_attack_range(int id)
        {
            if (Range_Enemies.Contains(id))
            {
                Range_Enemies.Remove(id);
                //if (!Show_All_Enemy_Range) //Yeti
                    update_enemy_range();
            }
        }

        protected bool Updating_Enemy_Range = false;
        protected bool Drawing_Enemy_Range = false;
        public bool drawing_enemy_range { set { Drawing_Enemy_Range = value; } }
        internal void update_enemy_range() //private //Yeti 
        {
            while (Drawing_Enemy_Range)
            {
                System.Threading.Thread.Sleep(1);
            }
            //add_enemy_ranges(50);
            Updating_Enemy_Range = true;
            All_Enemy_Displayed_Attack.Clear();
            All_Enemy_Displayed_Staff.Clear();
            Enemy_Displayed_Attack.Clear();
            Enemy_Displayed_Staff.Clear();
            List<int> ids = new List<int>();
            // Individual enemy range
            ids.AddRange(Range_Enemies);
            foreach (int id in ids)
            {
                if (Objects.unit_exists(id))
                    add_enemy_ranges(id, Enemy_Displayed_Attack, Enemy_Displayed_Staff);
                else
                    Range_Enemies.Remove(id);
            }
            // All enemy range
            ids.Clear();
            if (Show_All_Enemy_Range)
            {
                // Loop through teams
                for (int i = 1; i <= Constants.Team.NUM_TEAMS; i++)
                    // And then check each team group based on that team
                    foreach (int[] group in Constants.Team.TEAM_GROUPS)
                        // If there's a group that the player team is in and this one isn't
                        if (group.Contains(Global.game_state.team_turn > 0 ?
                            Global.game_state.team_turn : Constants.Team.PLAYER_TEAM) &&
                            !group.Contains(i)) //Multi
                        {
                            // Then this must be an enemy team
                            ids.AddRange(Teams[i]);
                            break;
                        }
                foreach (int id in ids)
                {
                    if (Objects.unit_exists(id))
                        add_enemy_ranges(id, All_Enemy_Displayed_Attack, All_Enemy_Displayed_Staff);
                    else
                        Range_Enemies.Remove(id);
                }
            }

            refresh_enemy_range_frames();
            foreach (KeyValuePair<Vector2, int> pair in Enemy_Displayed_Attack)
                All_Enemy_Displayed_Attack.Remove(pair.Key);
            foreach (KeyValuePair<Vector2, int> pair in Enemy_Displayed_Staff)
                All_Enemy_Displayed_Staff.Remove(pair.Key);
            Updating_Enemy_Range = false;
        }

        protected void add_enemy_ranges(int id, Dictionary<Vector2, int> attack, Dictionary<Vector2, int> staff)
        {
            Game_Unit enemy = this.units[id];
            if (!enemy.visible_by())
                return;
            foreach (Vector2 loc in enemy.attack_range)
                if (!attack.ContainsKey(loc))
                    attack.Add(loc, 0);
            foreach (Vector2 loc in enemy.staff_range)
                if (!staff.ContainsKey(loc))
                    staff.Add(loc, 0);
        }

        protected void refresh_enemy_range_frames()
        {
            refresh_enemy_range_frames(All_Enemy_Displayed_Attack, All_Enemy_Displayed_Staff);
            refresh_enemy_range_frames(Enemy_Displayed_Attack, Enemy_Displayed_Staff);
        }
        protected void refresh_enemy_range_frames(Dictionary<Vector2, int> attack, Dictionary<Vector2, int> staff)
        {
            int width = this.width;
            int height = this.height;
            // Attack
            HashSet<Vector2> keys = new HashSet<Vector2>();
            keys.UnionWith(attack.Keys);
            foreach (Vector2 key in keys)
            {
                int frame = 0;
                if (key.Y + 1 >= height || attack.ContainsKey(new Vector2(key.X, key.Y + 1)))
                    frame += 1;
                if (key.X - 1 < 0       || attack.ContainsKey(new Vector2(key.X - 1, key.Y)))
                    frame += 2;
                if (key.X + 1 >= width  || attack.ContainsKey(new Vector2(key.X + 1, key.Y)))
                    frame += 4;
                if (key.Y - 1 < 0       || attack.ContainsKey(new Vector2(key.X, key.Y - 1)))
                    frame += 8;
                attack[key] = frame;
            }
            // Staff
            keys.Clear();
            keys.UnionWith(staff.Keys);
            foreach (Vector2 key in keys)
            {
                int frame = 0;
                if (key.Y + 1 >= height || staff.ContainsKey(new Vector2(key.X, key.Y + 1)))
                    frame += 1;
                if (key.X - 1 < 0       || staff.ContainsKey(new Vector2(key.X - 1, key.Y)))
                    frame += 2;
                if (key.X + 1 >= width  || staff.ContainsKey(new Vector2(key.X + 1, key.Y)))
                    frame += 4;
                if (key.Y - 1 < 0       || staff.ContainsKey(new Vector2(key.X, key.Y - 1)))
                    frame += 8;
                staff[key] = frame;
            }
        }

        public bool is_enemy_range_visible()
        {
            // Show enemy ranges if dropping
            if (get_scene_map() != null &&
                    get_scene_map().drop_target_window_up)
                return true;

            if (!Global.game_map.move_range_visible)
                return false;
            if (Updating_Enemy_Range) return false;
            if (!Global.game_state.is_player_turn) return false;
            if (Global.game_state.is_changing_turns) return false;
            if (!Global.game_state.is_map_ready(true)) return false;
            if (Global.scene.is_message_window_active ||
                Global.game_system.is_interpreter_running) return false;
            return true;
        }
        #endregion

        public bool is_move_range_active { get { return Move_Range.Count > 0; } }

        public void update_move_arrow()
        {
            if (Global.game_temp.menu_call || Global.game_state.is_menuing ||
                (get_scene_map() != null && ((Scene_Map)Global.scene).changing_formation)) return;
            if (Global.game_system.Selected_Unit_Id == -1)
                Move_Arrow.Clear();
            else
            {
                Game_Unit selected_unit = get_selected_unit();
                int range = selected_unit.canto_mov;
                // If the cursor is in the move range
                if (Move_Range.Contains(Global.player.loc))
                {
                    if (Global.player.loc == selected_unit.loc)
                    {
                        Move_Arrow.Clear();
                    }
                    else
                    {
                        int x = (int)Global.player.loc.X;
                        int y = (int)Global.player.loc.Y;
                        // Test if the cursor location already exists in the move arrow
                        if (Move_Arrow.Count > 0)
                        {
                            // Test if the cursor is already at the endpoint
                            if (x == Move_Arrow[Move_Arrow.Count - 1].X && y == Move_Arrow[Move_Arrow.Count - 1].Y)
                                return;
                            // Test if the cursor is somewhere else on the list
                            bool already_on_list = false;
                            int i;
                            for (i = 0; i < Move_Arrow.Count - 1; i++)
                            {
                                if (Move_Arrow[i].X == x && Move_Arrow[i].Y == y)
                                {
                                    already_on_list = true;
                                    break;
                                }
                            }
                            if (already_on_list)
                            {
                                int j = Move_Arrow.Count - 1;
                                while (j >= i)
                                {
                                    Move_Arrow_Data loc = Move_Arrow[j];
                                    Selected_Move_Total -= selected_unit.move_cost(new Vector2(loc.X, loc.Y));
                                    if (Move_Arrow.Count >= 3)
                                        Move_Arrow[Move_Arrow.Count - 2].Frame =
                                            unfix_arrow_turn(Move_Arrow[Move_Arrow.Count - 1].Frame,
                                            Move_Arrow[Move_Arrow.Count - 2].Frame);
                                    Move_Arrow.pop();
                                    j--;
                                }
                            }
                        }

                        bool adjacent = Move_Arrow.Count == 1;
                        if (Move_Arrow.Count > 1)
                        {
                            Vector2 test_loc = new Vector2(
                                Move_Arrow[Move_Arrow.Count - 1].X, Move_Arrow[Move_Arrow.Count - 1].Y);
                            foreach (int[] test in new int[][] {
                                new int[] { -1, 0 }, new int[] { 1, 0 }, new int[] { 0, -1 }, new int[] { 0, 1 } })
                                if (test[0] + x == test_loc.X && test[1] + y == test_loc.Y)
                                {
                                    adjacent = true;
                                    break;
                                }
                        }
                        if (!adjacent)
                        { }
                        Selected_Move_Total += selected_unit.move_cost(Global.player.loc);
                        if (Selected_Move_Total > range || !adjacent)
                            reset_move_arrow(Global.player.loc, range);
                        else
                        {
                            Vector2 dir = new Vector2(x, y) - new Vector2(
                                Move_Arrow[Move_Arrow.Count - 1].X, Move_Arrow[Move_Arrow.Count - 1].Y);
                            Move_Arrow.Add(new Move_Arrow_Data(x, y, move_arrow_facing(dir)));
                            Move_Arrow[Move_Arrow.Count - 2].Frame =
                                fix_arrow_turn(Move_Arrow[Move_Arrow.Count - 1].Frame, Move_Arrow[Move_Arrow.Count - 2].Frame);
                        }
                    }
                }
            }
        }

        protected void reset_move_arrow(Vector2 target_loc, int range)
        {
            // this method throws index out of range sometimes \o_O?
#if DEBUG
            var move_arrow = new List<Move_Arrow_Data>(Move_Arrow);
#endif
            Move_Arrow.Clear();
            var map = new Pathfinding.UnitMovementMap(
                Global.game_system.Selected_Unit_Id);
            List<Vector2> route2 = map.convert_to_motions(
                map.get_route(target_loc, range));
            List<Vector2> route = map.convert_to_motions(
                map.get_reverse_route(target_loc, range));

            Game_Unit selected_unit = get_selected_unit();
            Vector2 current_loc = selected_unit.loc;

#if DEBUG
            if (route == null)
            {
                throw new Exception(string.Format(
                    "Attempted move route is null\nDisplayed move range likely doesn't match actual move range\n{0},{1} to {2},{3}",
                        (int)current_loc.X, (int)current_loc.Y,
                        (int)target_loc.X, (int)target_loc.Y));
                Move_Arrow = move_arrow;
                return;
            }
#endif

            if (route.Count == 0 || route.Count > range)
            {
                if (route.Count != route2.Count)
                { }
                for (int i = 0; i < route.Count; i++)
                    if (route[i] != route2[i])
                    { }
            }

            Selected_Move_Total = 0;
            Move_Arrow.Add(new Move_Arrow_Data((int)current_loc.X, (int)current_loc.Y, 15));

            while (route.Count > 0)
            {
                Vector2 dir = route.pop();
                current_loc += dir;
                Selected_Move_Total += selected_unit.move_cost(current_loc);
                Move_Arrow.Add(new Move_Arrow_Data((int)current_loc.X, (int)current_loc.Y, move_arrow_facing(dir)));
                Move_Arrow[Move_Arrow.Count - 2].Frame =
                    fix_arrow_turn(Move_Arrow[Move_Arrow.Count - 1].Frame, Move_Arrow[Move_Arrow.Count - 2].Frame);
            }
        }

        protected int move_arrow_facing(Vector2 dir)
        {
            if (dir == new Vector2(0, 1))
                return 2;
            else if (dir == new Vector2(-1, 0))
                return 4;
            else if (dir == new Vector2(1, 0))
                return 6;
            else if (dir == new Vector2(0, -1))
                return 8;
            return 15;
        }

        protected int fix_arrow_turn(int dir1, int dir2)
        {
            switch (dir1)
            {
                case 2:
                    switch (dir2)
                    {
                        case 2: return 10;
                        case 4: return 7;
                        case 6: return 1;
                        case 8: return 10;
                        case 11:
                        case 15: return 11;
                    }
                    break;
                case 4:
                    switch (dir2)
                    {
                        case 2: return 3;
                        case 4: return 9;
                        case 6: return 9;
                        case 8: return 1;
                        case 12:
                        case 15: return 12;
                    }
                    break;
                case 6:
                    switch (dir2)
                    {
                        case 2: return 5;
                        case 4: return 9;
                        case 6: return 9;
                        case 8: return 7;
                        case 13:
                        case 15: return 13;
                    }
                    break;
                case 8:
                    switch (dir2)
                    {
                        case 2: return 10;
                        case 4: return 5;
                        case 6: return 3;
                        case 8: return 10;
                        case 14:
                        case 15: return 14;
                    }
                    break;
            }
            return 15;
        }

        protected int unfix_arrow_turn(int dir1, int dir2)
        {
            switch (dir1)
            {
                case 2:
                    switch (dir2)
                    {
                        case 1: return 6;
                        case 3: return 2;
                        case 5: return 2;
                        case 7: return 4;
                        case 9: return 2;
                        case 10: return 2;
                    }
                    break;
                case 4:
                    switch (dir2)
                    {
                        case 1: return 8;
                        case 3: return 2;
                        case 5: return 4;
                        case 7: return 4;
                        case 9: return 4;
                        case 10: return 4;
                    }
                    break;
                case 6:
                    switch (dir2)
                    {
                        case 1: return 6;
                        case 3: return 6;
                        case 5: return 2;
                        case 7: return 8;
                        case 9: return 6;
                        case 10: return 6;
                    }
                    break;
                case 8:
                    switch (dir2)
                    {
                        case 1: return 8;
                        case 3: return 6;
                        case 5: return 4;
                        case 7: return 8;
                        case 9: return 8;
                        case 10: return 8;
                    }
                    break;
            }
            return dir2;
        }

        public void clear_move_range()
        {
            lock (Move_Range_Lock)
            {
                Move_Range.Clear();
                Attack_Range.Clear();
                Staff_Range.Clear();
                Talk_Range.Clear();
                Move_Arrow.Clear();
                Selected_Move_Total = 0;
            }
        }

        public void draw_basic_ranges(SpriteBatch sprite_batch, Texture2D move_range_texture, Texture2D attack_range_texture,
            Texture2D staff_range_texture, Texture2D talk_range_texture, Vector2 move_range_draw_vector, Color color)
        {
            lock (Move_Range_Lock)
            {
                int width = move_range_texture.Width / 4;
                int timer = Math.Min(range_start_timer, width - 1);//15); //Debug
                Rectangle rect = new Rectangle(((move_range_anim_count / 4) / 4) * width,
                        ((move_range_anim_count / 4) % 4) * width + (width - timer), timer, timer);
                Vector2 display_loc = this.display_loc;
                // Move Range
                foreach (Vector2 loc in Move_Range.Except(talk_range))
                    sprite_batch.Draw(move_range_texture, loc * TILE_SIZE + move_range_draw_vector - display_loc +
                        new Vector2(0, width - timer), rect, color);
                // Attack Range
                foreach (Vector2 loc in Attack_Range)
                    sprite_batch.Draw(attack_range_texture, loc * TILE_SIZE + move_range_draw_vector - display_loc +
                        new Vector2(0, width - timer), rect, color);
                // Staff Range
                foreach (Vector2 loc in Staff_Range)
                    sprite_batch.Draw(staff_range_texture, loc * TILE_SIZE + move_range_draw_vector - display_loc +
                        new Vector2(0, width - timer), rect, color);
                // Talk Range
                foreach (Vector2 loc in Talk_Range)
                    sprite_batch.Draw(talk_range_texture, loc * TILE_SIZE + move_range_draw_vector - display_loc +
                        new Vector2(0, width - timer), rect, color);
            }
        }
    }

    internal class Move_Arrow_Data
    {
        public int X;
        public int Y;
        public int Frame;

        #region Accessors
        public void write(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Frame);
        }

        public static Move_Arrow_Data read(BinaryReader reader)
        {
            return new Move_Arrow_Data(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }
        #endregion

        internal Move_Arrow_Data(int x, int y, int frame)
        {
            X = x;
            Y = y;
            Frame = frame;
        }
    }
}
