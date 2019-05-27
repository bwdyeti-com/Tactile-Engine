using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Map;
using FEXNA.Menus.Map.ContextSensitive;
using FEXNA_Library;
using ArrayExtension;
using HashSetExtension;
using ListExtension;
using RectangleExtension;
using FEXNAArrayExtension;
using FEXNAVector2Extension;
using FEXNADictionaryExtension;
using FEXNAListExtension;
using FEXNAVersionExtension;

namespace FEXNA
{
    internal partial class Game_Map
    {
        private Data_Map Map_Data;
        private string Unit_Data_Name;
        private bool Is_Map_Scene = false;
        private Rectangle Map_Edge_Offsets;
        private float Target_Display_X = 0, Target_Display_Y = 0;
        private int Actual_Display_X = 192, Actual_Display_Y = 192;
        private bool Scrolling = false, Scrolling_Previous = false;
        private int Move_Range_Anim_Count = 0;
        private MapObjects Objects = new MapObjects();
        internal List<Tuple<int, bool>> Waiting_Units = new List<Tuple<int, bool>>(); //private //Yeti
        private bool Wait_Events_Run = false;
        private bool Wait_Suspend_Called = false;
        private List<Torch_Staff_Point> Torch_Staves = new List<Torch_Staff_Point>();
        private List<Fow_View_Object> VisionPoints = new List<Fow_View_Object>();
        private List<List<int>> Teams;
        private int[] Team_Leaders;
        private List<Dictionary<int, string>> Group_Names;
        private Dictionary<int, int> Defeated_Units = new Dictionary<int, int>();
        private HashSet<int> DefeatedAlliedUnits = new HashSet<int>();
        private Dictionary<int, int> Removed_Units = new Dictionary<int, int>();
        private Dictionary<int, int> EscapedUnits = new Dictionary<int, int>();
        private List<int> Forced_Deployment = new List<int>();
        private List<Vector2> Deployment_Points = new List<Vector2>();
        private HashSet<Vector2> Move_Range = new HashSet<Vector2>(), Attack_Range = new HashSet<Vector2>(),
            Staff_Range = new HashSet<Vector2>(), Talk_Range = new HashSet<Vector2>();
        private List<Move_Arrow_Data> Move_Arrow = new List<Move_Arrow_Data>();
        private bool Move_Range_Visible = true;
        internal bool Refresh_Move_Ranges = true; //private //Yeti
        private bool Refresh_All_Ranges = true;
        private Dictionary<int, HashSet<Vector2>> Team_Range_Updates = new Dictionary<int, HashSet<Vector2>>();
        private List<int> Unit_Range_Updates = new List<int>();
        private int[,] Unit_Locations = new int[,] { };
        private bool Fow;
        private int Vision_Range;
        private Tone Fow_Color = new Tone(40, 40, 40, 72);
        private bool Fow_Updated = false;
        private HashSet<Vector2>[] Fow_Visibility = new HashSet<Vector2>[0];
        private Dictionary<Vector2, Visit_Data> Visit_Locations = new Dictionary<Vector2, Visit_Data>(),
            Chest_Locations = new Dictionary<Vector2, Visit_Data>(),
            Door_Locations = new Dictionary<Vector2, Visit_Data>();
        private Dictionary<Vector2, Shop_Data> Shops = new Dictionary<Vector2, Shop_Data>();
        private Dictionary<Vector2, Shop_Data> SecretShops = new Dictionary<Vector2, Shop_Data>();
        private Dictionary<Vector2, Vector2> Thief_Escape_Points = new Dictionary<Vector2, Vector2>();
        private List<EscapePoint> EscapePoints;
        private Dictionary<int, HashSet<Vector2>>[] Seize_Points =
            new Dictionary<int, HashSet<Vector2>>[Constants.Team.NUM_TEAMS + 1];
        private HashSet<Vector2> Seized_Points = new HashSet<Vector2>();
        private List<Tuple<Rectangle, string>> Area_Background = new List<Tuple<Rectangle, string>>();
        private int Grid_Opacity = 32;
        private List<Vector2>[] Light_Sources = new List<Vector2>[0];
        private int Min_Alpha = 0;
        private int Ally_Alpha;
        private Dictionary<int, List<Rectangle>>[] Team_Defend_Areas;
        private Dictionary<int, Vector2> Unit_Seek_Locs;
        private Dictionary<int, Dictionary<int, Vector2>> Team_Seek_Locs;
        private List<TileOutlineData> TileOutlines;

        private Map_Unit_Data Unit_Data;
        private Vector2 Map_Edge_Offset;
        private int[,] Siege_Locations = new int[,] { };
        private int[,] Destroyable_Locations = new int[,] { };
        private HashSet<int> Waiting_Unit_Skip = new HashSet<int>();
        public int range_start_timer = 0;
        private int rescue_anim_timer = 0;
        private int rescue_anim_loops = 0;
        public List<int> move_sound_timers = new List<int> { 0, 0, 0 };
        public float[,] Tile_Alpha;
        private bool Controlled_Scroll = false;
        private Vector2 ScrollSpeed = Vector2.Zero;

        static Random rand = new Random();

        protected int TILE_SIZE { get { return Constants.Map.TILE_SIZE; } }
        protected int UNIT_TILE_SIZE { get { return Constants.Map.UNIT_TILE_SIZE; } }

        #region Serialization
        public void write(BinaryWriter writer)
        {
            Map_Data.write(writer);
            writer.Write(Unit_Data_Name);

            writer.Write(Is_Map_Scene);
            Map_Edge_Offsets.write(writer);
            writer.Write((double)Target_Display_X);
            writer.Write((double)Target_Display_Y);
            writer.Write(Actual_Display_X);
            writer.Write(Actual_Display_Y);
            writer.Write(Scrolling);
            Objects.write(writer);
            Waiting_Units.write(writer);
            Torch_Staves.write(writer);
            VisionPoints.write(writer);
            Teams.write(writer);
            Team_Leaders.write(writer);
            Group_Names.write(writer);
            Defeated_Units.write(writer);
            DefeatedAlliedUnits.write(writer);
            Removed_Units.write(writer);
            EscapedUnits.write(writer);
            Forced_Deployment.write(writer);
            Deployment_Points.write(writer);
            Move_Range.write(writer);
            Attack_Range.write(writer);
            Staff_Range.write(writer);
            Talk_Range.write(writer);
            Move_Arrow.write(writer);
            writer.Write(Move_Range_Visible); // Why is this marked? //Debug
            writer.Write(Refresh_Move_Ranges);
            writer.Write(Refresh_All_Ranges);
            Team_Range_Updates.write(writer);
            Unit_Range_Updates.write(writer);
            Unit_Locations.write(writer);
            writer.Write(Fow);
            writer.Write(Vision_Range);
            Fow_Color.write(writer);
            Fow_Visibility.write(writer);
            Visit_Locations.write(writer);
            Chest_Locations.write(writer);
            Door_Locations.write(writer);
            Shops.write(writer);
            SecretShops.write(writer);
            Thief_Escape_Points.write(writer);
            EscapePoints.write(writer);
            Seize_Points.write(writer);
            Seized_Points.write(writer);
            Area_Background.write(writer);
            writer.Write(Grid_Opacity);
            Light_Sources.write(writer);
            writer.Write(Min_Alpha);
            writer.Write(Ally_Alpha);
            Team_Defend_Areas.write(writer);
            Unit_Seek_Locs.write(writer);
            Team_Seek_Locs.write(writer);
            TileOutlines.write(writer);

            writer.Write(Last_Added_Unit_Id);

            move_range_write(writer);
        }

        public void read(BinaryReader reader)
        {
            Map_Data = new Data_Map();
            Map_Data.read(reader);

            Unit_Data_Name = reader.ReadString();
            load_unit_data();

            Is_Map_Scene = reader.ReadBoolean();
            map_edge_offsets = Map_Edge_Offsets.read(reader);
            Target_Display_X = (float)reader.ReadDouble();
            Target_Display_Y = (float)reader.ReadDouble();
            Actual_Display_X = reader.ReadInt32();
            Actual_Display_Y = reader.ReadInt32();
            Scrolling = reader.ReadBoolean();
            Objects.read(reader);
            fix_siege_locations();
            fix_destroyable_locations();
            Waiting_Units.read(reader);
            Torch_Staves.read(reader);
            if (!Global.LOADED_VERSION.older_than(0, 6, 4, 0)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                VisionPoints.read(reader);
            }
            Teams.read(reader);
            Team_Leaders = Team_Leaders.read(reader);
            Group_Names.read(reader);
            Defeated_Units.read(reader);
            if (!Global.LOADED_VERSION.older_than(0, 5, 6, 0)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                DefeatedAlliedUnits.read(reader);
            }
            if (!Global.LOADED_VERSION.older_than(0, 5, 1, 4)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                Removed_Units.read(reader);
            }
            if (!Global.LOADED_VERSION.older_than(0, 5, 6, 5)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                EscapedUnits.read(reader);
            }
            Forced_Deployment.read(reader);
            Deployment_Points.read(reader);
            Move_Range.read(reader);
            Attack_Range.read(reader);
            Staff_Range.read(reader);
            Talk_Range.read(reader);
            Move_Arrow.read(reader);
            Move_Range_Visible = reader.ReadBoolean();
            Refresh_Move_Ranges = reader.ReadBoolean();
            Refresh_All_Ranges = reader.ReadBoolean();
            Team_Range_Updates.read(reader);
            Unit_Range_Updates.read(reader);
            Unit_Locations = Unit_Locations.read(reader);
            Fow = reader.ReadBoolean();
            Vision_Range = reader.ReadInt32();
            Fow_Color = Tone.read(reader);
            Fow_Visibility = Fow_Visibility.read(reader);
            if (!Global.LOADED_VERSION.older_than(0, 5, 0, 8)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                Visit_Locations.read(reader);
                Chest_Locations.read(reader);
                Door_Locations.read(reader);
            }
            else
            {
                Dictionary<Vector2, string[]> visits = new Dictionary<Vector2, string[]>();
                // Visits
                visits.read(reader);
                Visit_Locations = visits.ToDictionary(p => p.Key, p =>
                    p.Value.Length == 1 ? new Visit_Data(p.Value[0]) : new Visit_Data(p.Value[0], p.Value[1]));
                // Chests
                visits.read(reader);
                Chest_Locations = visits.ToDictionary(p => p.Key, p => new Visit_Data(p.Value[0]));
                // Doors
                visits.read(reader);
                Door_Locations = visits.ToDictionary(p => p.Key, p => new Visit_Data(p.Value[0]));
            }
            if (!Global.LOADED_VERSION.older_than(0, 5, 0, 5)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                Shops.read(reader);
                SecretShops.read(reader);
            }
            else
            {
                Shops.read(reader);
                SecretShops = Shops.Where(x => x.Value.secret).ToDictionary(p => p.Key, p => p.Value);
                Shops = Shops.Where(x => !x.Value.secret).ToDictionary(p => p.Key, p => p.Value);
            }
            Thief_Escape_Points.read(reader);
            if (Global.LOADED_VERSION.older_than(0, 5, 0, 7)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                List<Dictionary<Vector2, Vector2>> old_escape_points = new List<Dictionary<Vector2,Vector2>>();
                old_escape_points.read(reader);
                EscapePoints = Enumerable.Range(0, old_escape_points.Count)
                    .SelectMany(team => old_escape_points[team].Select(escape_loc => new EscapePoint(escape_loc.Key, escape_loc.Value, team, -1, "")))
                    .ToList();
                /* //Debug
                Team_Escape_Points = old_escape_points.Select(x =>
                    {
                        var escape_point = new Team_Escape_Data();
                        escape_point.Add(-1, x);
                        return escape_point;
                    }).ToList();*/
            }
            else if (Global.LOADED_VERSION.older_than(0, 5, 6, 6)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                //var team_escape_points = new List<Dictionary<int, Dictionary<Vector2, Vector2>>>(); //Debug
                var team_escape_points = new List<Team_Escape_Data>();
                team_escape_points.read(reader);
                EscapePoints = Enumerable.Range(0, team_escape_points.Count)
                    .SelectMany(team => team_escape_points[team].SelectMany(x => x.Value.Select(escape_loc => new EscapePoint(escape_loc.Key, escape_loc.Value, team, x.Key, ""))))
                    .ToList();
            }
            else
                EscapePoints.read(reader);
            if (Global.LOADED_VERSION.older_than(0, 6, 3, 2)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                HashSet<Vector2>[] seize_points = new HashSet<Vector2>[Constants.Team.NUM_TEAMS + 1];
                seize_points = seize_points.read(reader);
                Seize_Points = seize_points
                    .Select(team => new Dictionary<int, HashSet<Vector2>>
                        { { -1, team } })
                    .ToArray();
            }
            else
                Seize_Points = Seize_Points.read(reader);
            Seized_Points.read(reader);
            Area_Background.read(reader);
            Grid_Opacity = reader.ReadInt32();
            Light_Sources = Light_Sources.read(reader);
            refresh_alpha();
            Min_Alpha = reader.ReadInt32();
            Ally_Alpha = reader.ReadInt32();
            Team_Defend_Areas = Team_Defend_Areas.read(reader);
            Unit_Seek_Locs.read(reader);
            Team_Seek_Locs.read(reader);
            if (!Global.LOADED_VERSION.older_than(0, 5, 5, 0)) // This is a suspend load, so this isn't needed for public release //Debug
            {
                TileOutlines.read(reader);
            }

            Last_Added_Unit_Id = reader.ReadInt32();

            move_range_read(reader);
        }

        public void load_suspend()
        {
            Global.game_state.refresh_ai_defend_area();
            Window_Minimap.clear();
            get_scene_map().reset_map(true);
            get_scene_map().set_map_texture();
            refresh_alpha();
            init_sprites();
            update_enemy_range();
            Wait_Suspend_Called = Waiting_Units.Count > 0;
        }
        #endregion

        #region Accessors
        public Data_Map map_data { get { return Map_Data; } }

        private int team_turn { get { return Global.game_state.team_turn; } }

        public Rectangle map_edge_offsets
        {
            get { return Map_Edge_Offsets; }
            set
            {
                Map_Edge_Offsets = value;
                Map_Edge_Offset = new Vector2(value.X, value.Y);
            }
        }
        public int edge_offset_left { get { return Map_Edge_Offsets.X; } }
        public int edge_offset_right { get { return Map_Edge_Offsets.Width - Map_Edge_Offsets.X; } }
        public int edge_offset_top { get { return Map_Edge_Offsets.Y; } }
        public int edge_offset_bottom { get { return Map_Edge_Offsets.Height - Map_Edge_Offsets.Y; } }

        public List<Torch_Staff_Point> flares { get { return Torch_Staves; } }

        public List<List<int>> teams { get { return Teams; } }

        public int[] team_leaders { get { return Team_Leaders; } }

        public List<int> allies { get { return Teams[Constants.Team.PLAYER_TEAM]; } }
        public List<int> active_team { get { return Teams[team_turn]; } }

        /// <summary>
        /// Returns whether the active team has any units still able to act.
        /// </summary>
        public bool active_team_turn_over
        {
            get
            {
                if (Global.game_options.auto_turn_end == 2)
                    return !this.ready_movable_units;
                else
                    return !this.active_team_ready_units.Any();
            }
        }
        
        public bool ready_movable_units
        {
            get
            {
                return this.active_team_ready_units
                    .Any(unit =>
                    {
                        return unit.mov > 0;
                    });
            }
        }

        /// <summary>
        /// Returns the units on the active team who are able to act.
        /// This means ready, not rescued, controllable, and on the map.
        /// </summary>
        public IEnumerable<Game_Unit> active_team_ready_units
        {
            get
            {
                return Teams[this.team_turn]
                    .Select(id => this.units[id])
                    .Where(unit =>
                    {
                        if (!unit.ready || unit.is_rescued || is_off_map(unit.loc) ||
                                unit.uncontrollable || unit.unselectable)
                            return false;
                        return true;
                    })
                    .ToList();
            }
        }

        internal Dictionary<int, Game_Unit> units { get { return Objects.units; } }
        internal Dictionary<int, Siege_Engine> siege_engines { get { return Objects.siege_engines; } }

        public Dictionary<string, int> unit_identifiers { get { return Objects.unit_identifiers; } }

        public string tileset_name
        {
            get
            {
                if (Global.data_tilesets.ContainsKey(Map_Data.GetTileset()))
                {
                    return Global.data_tilesets[Map_Data.GetTileset()].Graphic_Name;
                }
                else
                    return new Data_Tileset().Graphic_Name;
            }
        }

        public List<string> animated_tileset_names
        {
            get
            {
                if (Global.data_tilesets.ContainsKey(Map_Data.GetTileset()))
                {
                    return Global.data_tilesets[Map_Data.GetTileset()].Animated_Tile_Names;
                }
                else
                    return new Data_Tileset().Animated_Tile_Names;
            }
        }

        public List<Rectangle> animated_tileset_data
        {
            get
            {
                if (Global.data_tilesets.ContainsKey(Map_Data.GetTileset()))
                {
                    return Global.data_tilesets[Map_Data.GetTileset()].Animated_Tile_Data;
                }
                else
                    return new Data_Tileset().Animated_Tile_Data;
            }
        }

        public string tileset_battleback_suffix
        {
            get
            {
                if (Global.data_tilesets.ContainsKey(Map_Data.GetTileset()))
                {
                    return Global.data_tilesets[Map_Data.GetTileset()].BattlebackSuffix;
                }
                else
                    return "";
            }
        }

        public List<int> forced_deployment { get { return Forced_Deployment; } }

        public List<Vector2> deployment_points { get { return Deployment_Points; } }

        public HashSet<Vector2> move_range { get { return Move_Range; } }
        public HashSet<Vector2> attack_range { get { return Attack_Range; } }
        public HashSet<Vector2> staff_range { get { return Staff_Range; } }
        public HashSet<Vector2> talk_range { get { return Talk_Range; } }

        internal List<Move_Arrow_Data> move_arrow { get { return Move_Arrow; } }

        public int move_range_anim_count { get { return Move_Range_Anim_Count; } }

        public int display_x
        {
            get { return (Actual_Display_X * TILE_SIZE) / UNIT_TILE_SIZE; }
            set
            {
                value = value / TILE_SIZE;
                if (Global.game_system.Instant_Move)
                {
                    Target_Display_X = value;
                    Actual_Display_X = value * UNIT_TILE_SIZE;
                    scroll_speed = -1;
                }
                else
                {
                    Scrolling |= (value != Target_Display_X);
                    Target_Display_X = value;
                    scroll_speed = 0;
                }
            }
        }
        public int display_y
        {
            get { return (Actual_Display_Y * TILE_SIZE) / UNIT_TILE_SIZE; }
            set
            {
                value = value / TILE_SIZE;
                if (Global.game_system.Instant_Move)
                {
                    Target_Display_Y = value;
                    Actual_Display_Y = value * UNIT_TILE_SIZE;
                    scroll_speed = -1;
                }
                else
                {
                    Scrolling |= (value != Target_Display_Y);
                    Target_Display_Y = value;
                    scroll_speed = 0;
                }
            }
        }
        public Vector2 display_loc
        {
            get
            {
                return new Vector2(display_x, display_y);
            }
        }

        public float float_display_x
        {
            private get { return Target_Display_X; }
            set
            {
                float scroll = Math.Max(min_scroll_x, Math.Min(max_scroll_x, ((int)value) / (float)TILE_SIZE));
                if (Global.game_system.Instant_Move)
                {
                    Target_Display_X = scroll;
                    Actual_Display_X = (int)(scroll * UNIT_TILE_SIZE);
                    scroll_speed = -1;
                }
                else
                {
                    Scrolling |= (scroll != Target_Display_X);
                    Target_Display_X = scroll;
                    scroll_speed = 0;
                }
            }
        }
        public float float_display_y
        {
            private get { return Target_Display_Y; }
            set
            {
                float scroll = Math.Max(min_scroll_y, Math.Min(max_scroll_y, ((int)value) / (float)TILE_SIZE));
                if (Global.game_system.Instant_Move)
                {
                    Target_Display_Y = scroll;
                    Actual_Display_Y = (int)(scroll * UNIT_TILE_SIZE);
                    scroll_speed = -1;
                }
                else
                {
                    Scrolling |= (scroll != Target_Display_Y);
                    Target_Display_Y = scroll;
                    scroll_speed = 0;
                }
            }
        }

        public Vector2 target_display_loc
        {
            get
            {
                return new Vector2((int)(Target_Display_X * TILE_SIZE), (int)(Target_Display_Y * TILE_SIZE));
            }
        }

        public bool scrolling { get { return Scrolling; } }


        public bool move_range_visible
        {
            get { return Move_Range_Visible &&
                Waiting_Units.Count == 0 &&
                !Global.game_system.is_interpreter_running &&
                !Global.game_state.support_active; }
            set
            {
                Move_Range_Visible = value;
                // reset move range size counter, maybe //Yeti
            }
        }

        public bool move_ranges_need_update { get { return Refresh_Move_Ranges; } }

        public bool refresh_all_ranges { get { return Refresh_All_Ranges; } }

        public Dictionary<int, HashSet<Vector2>> team_range_updates
        {
            get { return Team_Range_Updates; }
            set { Team_Range_Updates = value; }
        }

        public List<int> unit_range_updates
        {
            get { return Unit_Range_Updates; }
            set { Unit_Range_Updates = value; }
        }

#if DEBUG
        private Dictionary<Vector2, int> unit_locations_dictionary
        {
            get
            {
                return Enumerable.Range(0, Unit_Locations.GetLength(0))
                .SelectMany(x => Enumerable.Range(0, Unit_Locations.GetLength(1))
                    .Select(y => new Vector2(x, y)))
                .Where(v => Unit_Locations[(int)v.X, (int)v.Y] != 0)
                .Select(v => new KeyValuePair<Vector2, int>(v, Unit_Locations[(int)v.X, (int)v.Y]))
                .OrderBy(p => p.Value)
                .ToDictionary(p => p.Key, p => p.Value);
            }
        }
#endif

        public bool fow
        {
            get { return Fow; }
            set
            {
                Fow = value;
                update_fow();
                if (!Refresh_Move_Ranges)
                    update_enemy_range();
            }
        }

        public int vision_range
        {
            get { return Vision_Range; }
            set
            {
                Vision_Range = value;
                update_fow();
                update_enemy_range();
            }
        }

        public Tone fow_color
        {
            get { return Fow_Color; }
            set { Fow_Color = value; }
        }

        public HashSet<Vector2>[] fow_visibility { get { return Fow_Visibility; } }

        internal Dictionary<Vector2, Visit_Data> visit_locations { get { return Visit_Locations; } }
        internal Dictionary<Vector2, Visit_Data> chest_locations { get { return Chest_Locations; } }
        internal Dictionary<Vector2, Visit_Data> door_locations { get { return Door_Locations; } }

        public bool any_pillage { get { return Visit_Locations.Any(pair => !string.IsNullOrEmpty(pair.Value.PillageEvent)); } }

        internal Dictionary<Vector2, Shop_Data> shops { get { return Shops; } }
        internal Dictionary<Vector2, Shop_Data> secret_shops { get { return SecretShops; } }

        public Dictionary<Vector2, Vector2> thief_escape_points { get { return Thief_Escape_Points; } }
        //public List<Team_Escape_Data> team_escape_points { get { return Team_Escape_Points; } } //Debug

        public HashSet<Vector2> seized_points { get { return Seized_Points; } }

        public int grid_opacity
        {
            get { return Global.game_options.grid * 32 / 8; }
            set { Grid_Opacity = (int)MathHelper.Clamp(value, 0, 255); }
        }

        public int min_alpha
        {
            get { return Min_Alpha; }
            set
            {
                Min_Alpha = value;
                refresh_alpha(Global.game_state.Tone_Time_Max);
            }
        }

        public int ally_alpha
        {
            get { return Ally_Alpha; }
            set
            {
                Ally_Alpha = (int)MathHelper.Clamp(value, -1, Constants.Map.ALPHA_MAX - 1);
            }
        }

        internal Dictionary<int, Vector2> unit_seek_locs { get { return Unit_Seek_Locs; } }
        internal Dictionary<int, Dictionary<int, Vector2>> team_seek_locs { get { return Team_Seek_Locs; } }

        internal List<TileOutlineData> tile_outlines { get { return TileOutlines; } }

        public bool icons_visible { get { return rescue_anim_timer < Config.RESCUE_VISIBLE_TIME; } }
        public float icon_timer { get { return rescue_anim_timer / (float)Config.RESCUE_TIME ; } }
        public int icon_loops { get { return rescue_anim_loops; } }
        #endregion

        public Game_Map()
        {
            map_edge_offsets = new Rectangle(2, 2, 4, 4); //Yeti
            Teams = new List<List<int>>();
            Group_Names = new List<Dictionary<int, string>>();
            EscapePoints = new List<EscapePoint>();
            //Team_Escape_Points = new List<Team_Escape_Data>(); //Debug
            Team_Leaders = new int[Constants.Team.NUM_TEAMS + 1];
            Fow_Visibility = new HashSet<Vector2>[Constants.Team.NUM_TEAMS + 1];
            Team_Defend_Areas = new Dictionary<int, List<Rectangle>>[Constants.Team.NUM_TEAMS + 1];
            for (int i = 0; i <= Constants.Team.NUM_TEAMS; i++)
            {
                Teams.Add(new List<int>());
                Group_Names.Add(new Dictionary<int, string>());
                //Team_Escape_Points.Add(new Team_Escape_Data()); //Debug
                Team_Leaders[i] = -1;
                Fow_Visibility[i] = new HashSet<Vector2>();
                Team_Defend_Areas[i] = new Dictionary<int, List<Rectangle>>();
            }
            Unit_Seek_Locs = new Dictionary<int, Vector2>();
            Team_Seek_Locs = new Dictionary<int, Dictionary<int, Vector2>>();
            TileOutlines = new List<TileOutlineData>();
            Pathfind.reset();
        }

        internal Scene_Map get_scene_map() //private //Yeti
        {
            if (Global.scene.scene_type == "Scene_Map")
                return (Scene_Map)Global.scene;
#if !MONOGAME && DEBUG
            else if (Global.scene.scene_type == "Scene_Map_Unit_Editor")
                return (Scene_Map_Unit_Editor)Global.scene;
#endif
            else
                return null;
        }

        internal Scene_Battle get_scene_battle() //private //Yeti
        {
            if (Global.scene.scene_type == "Scene_Battle")
                return (Scene_Battle)Global.scene;
            else
                return null;
        }

        protected void reset_data()
        {
            Torch_Staves.Clear();
            VisionPoints.Clear();
            Defeated_Units.Clear();
            DefeatedAlliedUnits.Clear();
            Removed_Units.Clear();
            EscapedUnits.Clear();
            Visit_Locations.Clear();
            Chest_Locations.Clear();
            Door_Locations.Clear();
            Shops.Clear();
            SecretShops.Clear();
            Thief_Escape_Points.Clear();
            clear_seize_points();
            Area_Background.Clear();
            Waiting_Units.Clear();
            Waiting_Unit_Skip.Clear();
            Unit_Seek_Locs.Clear();
            Team_Seek_Locs.Clear();
            TileOutlines.Clear();
            reset_enemy_range_data();
        }

        protected void load_unit_data()
        {
#if !MONOGAME && DEBUG
            if (Global.scene is Scene_Map_Unit_Editor)
            {
                throw new Exception();
            }
            if (Unit_Data_Name == Scene_Map_Unit_Editor.UnitDataKey)
                Unit_Data = Scene_Map_Unit_Editor.UnitData;
            else
#endif
            {
                if (Unit_Data_Name == "")
                    Unit_Data = new Map_Unit_Data();
                else
                {
                    try
                    {
                        Unit_Data = Global.Chapter_Text_Content.Load<Map_Unit_Data>(
                            @"Data/Map Data/Unit Data/" + Unit_Data_Name);
                    }
                    catch (ContentLoadException ex)
                    {
                        Print.message(string.Format(
                            "Failed to load unit data \"{0}\"", Unit_Data_Name));
                        throw;
                    }
                }
            }
        }

        public void setup_units(string unit_data_name)
        {
            Unit_Data_Name = unit_data_name;
            load_unit_data();
        }
        public void setup_units(string id, Map_Unit_Data unit_data)
        {
            Unit_Data_Name = id;
            Unit_Data = unit_data;
        }

        internal void setup(string id, Data_Map map_data)
        {
            Global.game_system.clear_loss_on_death();
            Map_Data = map_data;

            Pathfind.reset();
            reset_data();

            Unit_Locations = new int[this.width, this.height];
            Siege_Locations = new int[this.width, this.height];
            Destroyable_Locations = new int[this.width, this.height];

            Objects.reset();
            Deployment_Points.Clear();
            Forced_Deployment.Clear();

            if (Scene_Map.debug_chapter_options_blocked())
            {
                //add_light_rune(new Vector2(6, 5), 1); //Debug
                //add_light_rune(new Vector2(7, 5), 2);
                //add_light_rune(new Vector2(8, 5), 3);
                //add_light_rune(new Vector2(9, 5), 4);
                add_permanent_light_rune(new Vector2(16, 5), 2);
                add_permanent_light_rune(new Vector2(17, 6), 2);
                add_permanent_light_rune(new Vector2(18, 5), 2);

                add_permanent_light_rune(new Vector2(15, 9), 2);
                add_permanent_light_rune(new Vector2(17, 9), 2);
                add_permanent_light_rune(new Vector2(19, 9), 2);

                add_permanent_light_rune(new Vector2(11, 11), 2);
                add_permanent_light_rune(new Vector2(12, 10), 2);

                add_permanent_light_rune(new Vector2(28, 9), 2);

                add_permanent_light_rune(new Vector2(29, 23), 2);
                add_permanent_light_rune(new Vector2(30, 23), 2);
            }

            Last_Added_Unit_Id = 0;
            foreach (List<int> team in Teams)
                team.Clear();
            foreach (Dictionary<int, string> group in Group_Names)
                group.Clear();
            EscapePoints.Clear();
            /* //Debug
            foreach (var escape_point in Team_Escape_Points)
                escape_point.Clear();*/
            Fow = false;
            Vision_Range = Config.DEFAULT_VISION_RANGE;
            foreach (HashSet<Vector2> visibility in Fow_Visibility)
                visibility.Clear();

            foreach (var data in Unit_Data.Units)
            {
                add_unit(data.Key, data.Value);
            }

#if DEBUG
            if (false)
            {
                debug_map_stuff();
            }
#endif

            fix_unit_locations();
            refresh_move_ranges(true);

            Window_Minimap.clear();

            Light_Sources = new List<Vector2>[Constants.Map.ALPHA_MAX];
            for(int i = 0; i < Light_Sources.Length; i++)
                Light_Sources[i] = new List<Vector2>();
            Min_Alpha = 255;
            Ally_Alpha = -1;
            refresh_alpha();
            
#if DEBUG
            //generational_stat_test();
#endif
        }

        internal void init_sprites()
        {
            Objects.init_sprites();
        }

#if DEBUG
        protected void debug_map_stuff()
        {
            add_unit(new Vector2(2, 7), new Data_Unit("character", "Mundus", "62|Actor ID\n1|Team\n0|AI Priority\n3|AI Mission"));

            if (this.last_added_unit.actor.level == 10)
            {
                this.last_added_unit.actor.gain_item(new Item_Data(Item_Data_Type.Weapon, 1, -1));
                this.last_added_unit.actor.instant_level = true;
                this.last_added_unit.actor.exp += 1000;
                this.last_added_unit.actor.gain_item(new Item_Data(Item_Data_Type.Weapon, 12, 25));
                this.last_added_unit.actor.gain_item(new Item_Data(Item_Data_Type.Item, 15, 5));
                Global.game_actors[6].gain_item(new Item_Data(Item_Data_Type.Weapon, 161, 5));
                Global.game_actors[6].gain_item(new Item_Data(Item_Data_Type.Weapon, 162, 5));
                Global.game_actors[6].gain_item(new Item_Data(Item_Data_Type.Weapon, 156, 10));
                //Global.game_actors[1].gain_item(new Item_Data(Item_Data_Type.Item, 42, 1));
            }
            if (false)
            {
                Global.game_actors[8].hp -= 5;
                Global.game_actors[8].add_state(5);
                Global.game_actors[3].add_state(15);
            }
        }

        protected void generational_stat_test()
        {
            Global.game_system.In_Arena = true;

            //int[] class_ids = new int[] { 51, 52, 53, 55, 57, 59, 61, 62, 63, 64, 65, 67, 69, 70, 71, 72, 73, 74, 76,
            //    78, 79, 80, 81, 83, 84, 86, 88, 90 };
            int[] class_ids = new int[] { 51, 55, 59, 61, 62, 63, 65, 70, 73,
                79, 80, 81, 84, 88 };
            int[][] weapon_ids = new int[][] {
                new int[] { 1, 31, 56, 81, 101, 111, 121, 131, 141 },
                new int[] { 3, 33, 59, 83, 102, 112, 122, 132, 142 },
                new int[] { 8, 38, 64, 85, 104, 114, 124, 133, 143 },
                new int[] { 16, 46, 71, 90, 106, 116, 126, 136, 146 }
            };
            //Generic_Build[] builds = { Generic_Build.Weak, Generic_Build.Mid, Generic_Build.Normal, Generic_Build.Strong };
            Generic_Builds[] builds = { Generic_Builds.Strong };
            int generations = 30;
            int generation_size = 8;

            Random rand = new Random();

            // Set up opponents
            Dictionary<int, List<int>[]> enemy_ids = new Dictionary<int, List<int>[]>();
            foreach (int level in new int[] { 1, 10, 20 })
            {
                enemy_ids.Add(level, new List<int>[4]);
                foreach (Generic_Builds build in builds)
                {
                    enemy_ids[level][(int)build] = new List<int>();
                    foreach (int class_id in class_ids)
                    {
                        add_temp_unit(Constants.Team.ENEMY_TEAM, new Vector2(-10, -10), class_id, 0, "");
                        setup_test_generic(class_id, level, build, null);

                        enemy_ids[level][(int)build].Add(Last_Added_Unit_Id);
                    }
                }
            }

            int[] stat_totals = new int[] { 150, 150, 180, 225 };
            int[] stats = new int[7];
            for (int i = 0; i < stats.Length; i++)
                stats[i] = stat_totals[3] / 7;
            for (int i = 0; i < stat_totals[3] % 7; i++)
                stats[rand.Next(7)]++;
            int[] current_stats = new int[7];
            //for (int i = 0; i < stats.Length; i++)
            //    stats[i] = new int[] ;
            Dictionary<string, double> class_rates = new Dictionary<string, double>();
            Dictionary<string, int> class_fights = new Dictionary<string, int>();
            Dictionary<string, int> class_wins = new Dictionary<string, int>();
            Dictionary<int[], float> tested_spreads = new Dictionary<int[], float>();

            string output = "";
            string line = "";
            foreach (int stat in stats)
                line += stat + " ";
            output += "Initial stats:\n" + line + "\n";

            for (int i = 0; i < generations; i++)
            {
                line = "\n";

                int[][] next_gen = new int[generation_size][];

                // Generates next generation's stats
                for (int j = 0; j < next_gen.Length; j++)
                {
                    next_gen[j] = new int[stats.Length];
                    Array.Copy(stats, next_gen[j], stats.Length);

                    // Randomly modifies stats stat_count*2 times
                    for (int k = 0; k < stats.Length * 2; k++)
                    {
                        int index1 = rand.Next(stats.Length);
                        if (next_gen[j][index1] > 0)
                        {
                            next_gen[j][index1] += -1;
                            next_gen[j][rand.Next(stats.Length)] += 1;
                        }
                    }

                    next_gen[j][5] = next_gen[j][5] + next_gen[j][6];
                    next_gen[j][6] = next_gen[j][5];

                    if (rand.Next(2) == 0)
                    {
                        next_gen[j][6] = next_gen[j][5] / 2;
                        next_gen[j][5] = (next_gen[j][5] + 1) / 2;
                    }
                    else
                    {
                        next_gen[j][5] = next_gen[j][6] / 2;
                        next_gen[j][6] = (next_gen[j][6] + 1) / 2;
                    }
                }

                float[] favorability = new float[next_gen.Length];

                for (int j = 0; j < next_gen.Length; j++)
                {
                    if (false)//tested_spreads.ContainsKey(next_gen[j]))
                    {
                        favorability[j] = tested_spreads[next_gen[j]];
                        continue;
                    }
                    //List<KeyValuePair<float, string>> rates = new List<KeyValuePair<float, string>>();

                    List<float> unit_odds = new List<float>();

                    foreach (int level in new int[] { 1, 10, 20 })
                        foreach(Generic_Builds build in builds)
                            foreach (int class_id in class_ids)
                            {
                                Array.Copy(next_gen[j], current_stats, current_stats.Length);
                                current_stats[0] *= 2;
                                for (int k = 0; k < current_stats.Length; k++)
                                    current_stats[k] = (int)Math.Round((current_stats[k] * stat_totals[(int)build]) / ((float)stat_totals[3]));

                                add_temp_unit(Constants.Team.ENEMY_TEAM, new Vector2(-10, -10), class_id, 0, "");
                                setup_test_generic(class_id, level, build, current_stats);

                                List<float> class_odds = new List<float>();

                                // Has this unit fight against each opponent
                                foreach (int enemy_id in enemy_ids[level][(int)build])
                                {
                                    List<float> target_odds = new List<float>();
                                    Game_Unit enemy = this.units[enemy_id];
                                    for (int weapon_tier = 0; weapon_tier < weapon_ids.Length; weapon_tier++)
                                    {
                                        // Checks each weapon
                                        foreach (int weapon_id1 in weapon_ids[weapon_tier])
                                        {
                                            // Remove old weapons
                                            while (last_added_unit.actor.num_items > 0)
                                                last_added_unit.actor.drop_item();
                                            if (last_added_unit.actor.is_equippable(Global.data_weapons[weapon_id1]))
                                            {
                                                last_added_unit.actor.gain_item(new Item_Data(Item_Data_Type.Weapon, weapon_id1, 50));

                                                // Uses each enemy weapon
                                                foreach (int weapon_id2 in weapon_ids[weapon_tier])
                                                {
                                                    while (enemy.actor.num_items > 0)
                                                        enemy.actor.drop_item();
                                                    if (enemy.actor.is_equippable(Global.data_weapons[weapon_id2]))
                                                    {
                                                        // Only if they can fight
                                                        Data_Weapon weapon1 = Global.data_weapons[last_added_unit.actor.items[0].Id];
                                                        if (Global.data_weapons[weapon_id2].Max_Range >= weapon1.Min_Range &&
                                                            Global.data_weapons[weapon_id2].Min_Range <= weapon1.Max_Range)
                                                        {
                                                            enemy.actor.gain_item(new Item_Data(Item_Data_Type.Weapon, weapon_id2, 50));

                                                            int range = Math.Max(Global.data_weapons[last_added_unit.actor.items[0].Id].Min_Range,
                                                                Global.data_weapons[enemy.actor.items[0].Id].Min_Range);

                                                            float attack_odds = Combat.combat_odds(last_added_unit, enemy, range, null, true, true);
                                                            if (float.IsNaN(attack_odds))
                                                            { }
                                                            target_odds.Add(attack_odds);

                                                            float defense_odds = 1f - Combat.combat_odds(enemy, last_added_unit, range, null, true, true);
                                                            target_odds.Add(defense_odds);

                                                            if (!class_rates.ContainsKey(enemy.actor.class_name))
                                                                class_rates.Add(enemy.actor.class_name, 0d);
                                                            if (!class_fights.ContainsKey(enemy.actor.class_name))
                                                                class_fights.Add(enemy.actor.class_name, 0);
                                                            class_rates[enemy.actor.class_name] += (1f - attack_odds);
                                                            class_fights[enemy.actor.class_name]++;
                                                            class_rates[enemy.actor.class_name] += (1f - defense_odds);
                                                            class_fights[enemy.actor.class_name]++;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (target_odds.Count > 0)
                                    {
                                        class_odds.Add(target_odds.Average());
                                        if (!class_wins.ContainsKey(enemy.actor.class_name))
                                            class_wins.Add(enemy.actor.class_name, 0);
                                        if (target_odds.Average() < 0.5f)
                                            class_wins[enemy.actor.class_name]++;
                                        //rates.Add(new KeyValuePair<float, string>(class_odds.Average(), enemy.actor.class_name()));
                                    }
                                }
                                remove_unit(Last_Added_Unit_Id);
                                unit_odds.Add(class_odds.Average());
                            }
                    favorability[j] = unit_odds.Average();
                    tested_spreads[next_gen[j]] = favorability[j];
                }
                generation_sort(next_gen, favorability);
                Array.Copy(next_gen[next_gen.Length - 1], stats, stats.Length);

                foreach (int stat in stats)
                    line += stat + " ";
                line += favorability[favorability.Length - 1].ToString();
                output += line;
            }

            Print.message(output);

            string class_results = "";
            foreach (int class_id in class_ids)
            {
                class_results += class_wins[Global.data_classes[class_id].Name].ToString() + ", ";
                class_results += (class_rates[Global.data_classes[class_id].Name] / class_fights[Global.data_classes[class_id].Name]).ToString();
                class_results += " - " + Global.data_classes[class_id].Name;
                class_results += "\n";
            }
            Print.message(class_results);

            foreach(int key in enemy_ids.Keys)
                foreach(List<int> enemies in enemy_ids[key])
                    if (enemies != null)
                        while (enemies.Count > 0)
                        {
                            remove_unit(enemies[0]);
                            enemies.RemoveAt(0);
                        }

            Global.game_system.In_Arena = false;
        }

        protected void generation_sort(int[][] next_gen, float[] favorability)
        {
            System.Array.Sort(favorability, next_gen);
            //next_gen.Sort(delegate(int[] a, int[] b)
            //{
            //    return generation_sort_result(a, b);
            //});
        }

        protected int generation_sort_result(int[] a, int[] b)
        {
            return b[3] - a[3];
        }

        protected void setup_test_generic(int class_id, int level, Generic_Builds build, int[] growths)
        {
            // Sets opponent's stats
            last_added_unit.gladiator = true;
            last_added_unit.actor.name = "Gladiator";
            last_added_unit.actor.level_down();
            last_added_unit.actor.exp = 0;
            // Set opponent's level
            int exp_gain = 0;
            if (Global.data_classes.Any(x => x.Value.Tier == 0) && last_added_unit.actor.tier > 0) //Global.game_system.has_tier_0s and actor.tier > 0 //Yeti
                exp_gain += Constants.Actor.TIER0_LVL_CAP * Constants.Actor.EXP_TO_LVL;
            for (int i = 1; i < 2; i++)
                exp_gain += Constants.Actor.LVL_CAP * Constants.Actor.EXP_TO_LVL;
            if (level < 1)
            {
                exp_gain += (level + 1) * (-Constants.Actor.EXP_TO_LVL);
                level = 1;
            }
            else if (level > last_added_unit.actor.level_cap())
            {
                exp_gain += (level - last_added_unit.actor.level_cap()) *
                    Constants.Actor.EXP_TO_LVL;
                level = last_added_unit.actor.level_cap();
            }
            exp_gain = Math.Max(0, exp_gain);
            // 8 RNs, 1 for affinity, one each for 7 stats
            int[] wexp = Enumerable.Range(0, Global.weapon_types.Count - 1)
                .Select(x => FEXNA_Library.Data_Weapon.WLVL_THRESHOLDS[(int)FEXNA_Library.Weapon_Ranks.A])
                .ToArray();

            last_added_unit.actor.setup_generic(
                class_id, level, 0, exp_gain / Constants.Actor.EXP_TO_LVL, build,
                -1, true, growths, wexp: wexp);
            /* //Debug
            for (int weapon_type = 1; weapon_type < 10; weapon_type++)
                if (last_added_unit.actor.get_weapon_level(Global.weapon_types[weapon_type]) > 0)
                    last_added_unit.actor.wexp_set(Global.weapon_types[weapon_type],
                        FEXNA_Library.Data_Weapon.WLVL_THRESHOLDS[(int)FEXNA_Library.Weapon_Ranks.A], false);
            last_added_unit.actor.clear_wlvl_up();*/
        }
#endif

        #region Alpha
        public void refresh_alpha()
        {
            refresh_alpha(0);
        }
        public void refresh_alpha(int time)
        {
            // If alpha is irrelevant
            if (time == 0 && Min_Alpha == 255)
            {
                Tile_Alpha = new float[,] { { 1f } };
            }
            else
                set_map_alpha();
            if (get_scene_map() != null)
                get_scene_map().set_map_alpha_texture(Tile_Alpha, time);
        }

        void set_map_alpha()
        {
            float[,] tile_alpha = new float[
                this.width * Constants.Map.ALPHA_GRANULARITY,
                this.height * Constants.Map.ALPHA_GRANULARITY];
            if (this.width == 0 || this.height == 0)
            {
                Tile_Alpha = tile_alpha;
                return;
            }
            Dictionary<float, List<Vector2>> light_sources = new Dictionary<float, List<Vector2>>();
            List<Vector2>[] sources_with_units = new List<Vector2>[Light_Sources.Length];

            for (int i = 0; i < sources_with_units.Length; i++)
            {
                sources_with_units[i] = new List<Vector2>();
                sources_with_units[i].AddRange(Light_Sources[i]);
            }
            if (Ally_Alpha >= 0)
                for (int y = 0; y < this.height; y++)
                    for (int x = 0; x < this.width; x++)
                        if (get_unit(new Vector2(x, y)) != null && get_unit(new Vector2(x, y)).is_ally) //Multi
                        {
                            sources_with_units[Ally_Alpha].Add(new Vector2(x, y));
                        }

            for (int i = 0; i < sources_with_units.Length; i++)
                foreach (Vector2 source in sources_with_units[i])
                    for (int oy = 0; oy < Constants.Map.ALPHA_GRANULARITY; oy++)
                        for (int ox = 0; ox < Constants.Map.ALPHA_GRANULARITY; ox++)
                            lighting_add(light_sources, i + 1,
                                source * Constants.Map.ALPHA_GRANULARITY + new Vector2(ox, oy));
            // Loop through light sources
            while (light_sources.Count > 0)
            {
                float alpha = light_sources.Keys.Max();
                List<Vector2> temp_light_sources = new List<Vector2>();
                temp_light_sources.AddRange(light_sources[alpha]);
                temp_light_sources = temp_light_sources.Distinct().ToList(); //ListOrEquals
                foreach (Vector2 source in temp_light_sources)
                {
                    if (is_off_map(source / Constants.Map.ALPHA_GRANULARITY, false))
                        continue;
                    if (tile_alpha[(int)source.X, (int)source.Y] < alpha)
                    {
                        tile_alpha[(int)source.X, (int)source.Y] = alpha;
                        //foreach (Vector2 offset in new Vector2[] { //Debug
                        //    new Vector2(0, -1), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1) })
                        for (int oy = -1; oy <= 1; oy++)
                            for (int ox = -1; ox <= 1; ox++)
                            {
                                if (oy == 0 && ox == 0) continue;
                                if (oy != 0 && ox != 0) continue; //Debug
                                Vector2 offset = new Vector2(ox, oy);
                                if (!is_off_map(
                                    (source + offset) / Constants.Map.ALPHA_GRANULARITY,
                                    false))
                                {
                                    float cost = alpha_cost(
                                        (source + offset) /
                                            Constants.Map.ALPHA_GRANULARITY) *
                                            offset.Length();
                                    if (alpha - cost > 0)
                                        lighting_add(light_sources, alpha - cost, source + offset);
                                }
                            }
                    }
                }
                light_sources.Remove(alpha);
            }
            Tile_Alpha = tile_alpha;
        }

        void lighting_add(Dictionary<float, List<Vector2>> dict, float key, Vector2 value)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, new List<Vector2>());
            dict[key].Add(value);
        }

        protected int alpha_cost(Vector2 loc)
        {
            int cost = Global.data_terrains[Global.data_tilesets[Map_Data.GetTileset()].Terrain_Tags[
                Map_Data.GetValue((int)loc.X, (int)loc.Y)]].Move_Costs[Global.game_state.weather][0];
            if (cost == -1)
                return 4;
            return Math.Min(4, cost);
            return 1;
        }

        public void add_alpha_source(Vector2 loc, int value)
        {
            Light_Sources[value].Add(loc);
        }

        public void clear_alpha()
        {
            for (int i = 0; i < Light_Sources.Length; i++)
                Light_Sources[i].Clear();
        }

        public Color get_unit_tint(Vector2 loc)
        {
            int x = (int)((loc.X + 0.5f) * Constants.Map.ALPHA_GRANULARITY);
            int y = (int)((loc.Y + 0.5f) * Constants.Map.ALPHA_GRANULARITY);
            return get_tint(x, y);
        }

        public Color get_tint(int x, int y)
        {
            if (Min_Alpha == 255)
                return Color.White;
            //int alpha = (int)MathHelper.Clamp(
            //    Tile_Alpha[x, y] * (256f / Config.ALPHA_MAX),
            //    min_alpha, 255);
            int alpha = (int)MathHelper.Clamp(
                Tile_Alpha[x, y] * (256f / Constants.Map.ALPHA_MAX) *
                (256 - min_alpha) / 256 + min_alpha,
                min_alpha, 255);
            return new Color(alpha, alpha, alpha, 255);
        }
        #endregion

        public int width
        {
            get
            {
                if (Map_Data == null)
                    return 0;
                return Map_Data.Columns;
            }
        }

        public int height
        {
            get
            {
                if (Map_Data == null)
                    return 0;
                return Map_Data.Rows;
            }
        }

        public int terrain_tag(int x, int y)
        {
            return terrain_tag(new Vector2(x, y));
        }
        public int terrain_tag(Vector2 loc)
        {
            if (this.width == 0 || this.height == 0 || loc.X < 0 || loc.Y < 0 || loc.X >= this.width || loc.Y >= this.height)
                return 0;
            Data_Tileset tileset = Global.data_tilesets[Map_Data.GetTileset()];
            return tileset.Terrain_Tags[Map_Data.GetValue((int)loc.X, (int)loc.Y)];
        }

        public Data_Terrain terrain_data(Vector2 loc)
        {
            int tag = terrain_tag(loc);
            return terrain_data(tag);
        }
        public Data_Terrain terrain_data(int tag)
        {
            Data_Terrain terrain;
            if (Global.data_terrains.ContainsKey(tag))
                terrain = Global.data_terrains[tag];
            else
            {
                terrain = new Data_Terrain();
                terrain.Name = tag.ToString();
                terrain.Move_Costs = new int[][] {
                    new int[] { -1, -1, -1, -1, -1 },
                    new int[] { -1, -1, -1, -1, -1 },
                    new int[] { -1, -1, -1, -1, -1 } };
            }
            return terrain;
        }

        #region Units
        public void fix_unit_locations()
        {
            Unit_Locations = new int[this.width, this.height];
            foreach (Game_Unit unit in this.units.Values)
                unit.fix_unit_location();
        }

        internal void fix_unit_location(Game_Unit unit, Vector2 loc)
        {
            Unit_Locations[(int)loc.X, (int)loc.Y] = unit.id + 1;
        }

        internal void clear_unit_location(Game_Unit unit, Vector2 loc)
        {
            if (Unit_Locations[(int)loc.X, (int)loc.Y] == unit.id + 1)
                Unit_Locations[(int)loc.X, (int)loc.Y] = 0;
        }

        internal bool no_unit_at_location(Vector2 loc)
        {
            return Unit_Locations[(int)loc.X, (int)loc.Y] == 0;
        }

        internal Game_Unit get_unit(Vector2 loc)
        {
            if (this.width == 0 || this.height == 0)
                return null;
            if (is_off_map(loc, false))
                return null;
            int id = Unit_Locations[(int)loc.X, (int)loc.Y] - 1;
            // If no unit here
            if (id == -1)
                return null;
            if (this.units[id].loc != loc && !this.units[id].saving_ai_loc &&
                !Global.game_system.is_interpreter_running)
            {
#if DEBUG
                throw new Exception("Get unit method broke");
#endif
                fix_unit_locations();
                return get_unit(loc);
            }
            return this.units[id];
        }

        public int unit_from_identifier(string identifier)
        {
            if (this.unit_identifiers.ContainsKey(identifier))
                return this.unit_identifiers[identifier];
            return -1;
        }

        protected void fix_siege_locations()
        {
            Siege_Locations = new int[this.width, this.height];
            foreach (Siege_Engine siege in this.siege_engines.Values)
                Siege_Locations[(int)siege.loc.X, (int)siege.loc.Y] = siege.id + 1;

        }

        internal Siege_Engine get_siege(Vector2 loc)
        {
            if (this.width == 0 || this.height == 0)
                return null;
            if (is_off_map(loc, false))
                return null;

            int id = Siege_Locations[(int)loc.X, (int)loc.Y] - 1;
            if (id == -1)
                return null;
            if (this.siege_engines[id].loc != loc)
            {
#if DEBUG
                throw new Exception("Get siege method broke");
#endif
                fix_siege_locations();
                return get_siege(loc);
            }
            return this.siege_engines[id];
        }

        protected void fix_destroyable_locations()
        {
            Destroyable_Locations = new int[this.width, this.height];
            foreach (Destroyable_Object map_object in Objects.enumerate_destroyables())
                Destroyable_Locations[(int)map_object.loc.X, (int)map_object.loc.Y] = map_object.id + 1;

        }

        public Destroyable_Object get_destroyable(int id)
        {
            return Objects.destroyable(id);

        }
        public Destroyable_Object get_destroyable(Vector2 loc)
        {
            if (this.width == 0 || this.height == 0)
                return null;
            int id = Destroyable_Locations[(int)loc.X, (int)loc.Y] - 1;
            if (id == -1)
                return null;
            if (Objects.destroyable(id).loc != loc)
            {
#if DEBUG
                throw new Exception("Get destroyable object method broke");
#endif
                fix_destroyable_locations();
                return get_destroyable(loc);
            }
            return Objects.destroyable(id);
        }

        public IEnumerable<Destroyable_Object> enumerate_destroyables()
        {
            return Objects.enumerate_destroyables();
        }

        public void destroyable_add_enemy_team(Vector2 loc, int team)
        {
            var destroyable = get_destroyable(loc);
            if (destroyable != null)
                destroyable.AddEnemyTeam(team);
        }

        // This is not particularly performant, consider refactoring later
        public LightRune get_light_rune(Vector2 loc)
        {
            return enumerate_light_runes().FirstOrDefault(x => x.loc == loc);
        }

        public IEnumerable<LightRune> enumerate_light_runes()
        {
            return Objects.enumerate_light_runes();
        }

        public IEnumerable<Combat_Map_Object> enumerate_combat_objects()
        {
            return Objects.enumerate_combat_objects();
        }

        public Map_Object get_map_object(int id)
        {
            return Objects[id];
        }

        internal int get_unit_id_from_actor(int actor_id)
        {
            foreach (Game_Unit unit in this.units.Values)
                if (unit.actor.id == actor_id && (Preparations_Unit_Team == null || !Preparations_Unit_Team.Contains(unit.id)))
                    return unit.id;
            return -1;
        }
        internal Game_Unit get_unit_from_actor(int actor_id)
        {
            int unit_id = get_unit_id_from_actor(actor_id);
            return Objects.unit_exists(unit_id) ? this.units[unit_id] : null;
        }
        internal HashSet<int> get_units_from_actor(int actor_id)
        {
            HashSet<int> result = new HashSet<int>();
            foreach (Game_Unit unit in this.units.Values)
                if (unit.actor.id == actor_id && (Preparations_Unit_Team == null || !Preparations_Unit_Team.Contains(unit.id)))
                    result.Add(unit.id);
            return result;
        }

        public bool is_actor_deployed(int actorId)
        {
            return get_unit_id_from_actor(actorId) != -1;
        }

        internal Game_Unit get_highlighted_unit()
        {
            if (Global.game_temp.highlighted_unit_id != -1 &&
                    Objects.unit_exists(Global.game_temp.highlighted_unit_id))
                return this.units[Global.game_temp.highlighted_unit_id];
            return null;
        }

        internal Game_Unit get_selected_unit()
        {
            if (Global.game_system.Selected_Unit_Id != -1)
                return this.units[Global.game_system.Selected_Unit_Id];
            return null;
        }

        internal void kill_unit(Game_Unit unit, bool dead = true)
        {
            //Global.game_temp.enemy_ranges.Remove(unit.id);
            if (dead)
            {
                Defeated_Units[unit.id] = unit.actor.id;
                if (unit.is_player_allied)
                    DefeatedAlliedUnits.Add(unit.id);
            }
            Removed_Units[unit.id] = unit.actor.id;
            Teams[unit.team].Remove(unit.id);
            if (dead)
            {
                // Units lose lives only if they're an ally, and they're not the convoy or them dying causes a game over
                if (unit.is_ally && (!unit.is_convoy() || unit.loss_on_death))
                {
                    unit.actor.lose_a_life();
                    if (unit.actor.is_out_of_lives())
                    {
                        Global.battalion.remove_actor(unit.actor.id);
                        if (unit.is_player_team)
                            if (Global.game_state.is_battle_map)
                                Global.game_system.chapter_deaths++;
                    }
                }
                Global.game_system.update_victory();
            }
            add_unit_move_range_update(unit);
            //Team_Range_Updates.AddRange(unit.attackable_teams());
            //Team_Range_Updates = Team_Range_Updates.Distinct().ToList(); //ListOrEquals
        }

        internal void add_unit_move_range_update(Game_Unit unit)
        {
            add_unit_move_range_update(unit, unit.loc);
        }
        internal void add_unit_move_range_update(Game_Unit unit, Vector2 loc) //private //Yeti 
        {
            if (unit == null)
                return;
            // Might need to just refresh all, if there are weird skill cases where an ally boosts movement for nearby allies/etc //Yeti
            Unit_Range_Updates.Add(unit.id);
            remove_updated_move_range(unit.id);
            foreach(var pair in this.units)
                if (Global.game_state.can_talk(unit.id, pair.Key))
                {
                    Unit_Range_Updates.Add(pair.Key);
                    remove_updated_move_range(pair.Key);
                }
            if (unit.is_dead)
                unit.remove_old_unit_location();
            // Go through teams with enemies of this unit //Debug
            // Go through all teams because reasons
            // if an ally ends their move range on this unit their attack range for the tile isn't visible
            // So they need updated to make their attack range appear
            Vector2 move_start_loc = unit.turn_start_loc; //Debug
            move_start_loc = unit.move_start_loc;
            //foreach (int team in unit.attackable_teams())
            for (int team = 1; team <= Constants.Team.NUM_TEAMS; team++)
            {
                // Add an entry for this team
                if (!Team_Range_Updates.ContainsKey(team))
                    Team_Range_Updates.Add(team, new HashSet<Vector2>());
                Team_Range_Updates[team].Add(loc);
                //Team_Range_Updates[team].Add(unit);
                Team_Range_Updates[team].Add(move_start_loc);
                // This shouldn't be needed unless a unit somehow ends up in the same tile as another unit??? //Yeti
                if (unit.loc != move_start_loc)
                    foreach (Game_Unit other_unit in this.units.Values)
                        if (other_unit.loc == move_start_loc)
                        {
                            other_unit.fix_unit_location(true);
                            break;
                        }
            }
        }

        public void change_unit_team(int id, int team)
        {
            if (Objects.unit_exists(id))
            {
                Game_Unit unit = this.units[id];
                if (team < 1 || team > Constants.Team.NUM_TEAMS)
                {
                    throw new IndexOutOfRangeException(
                        string.Format("Unit {0} tried to join nonexistant team {1}", id, team));
                }
                if (unit.team != team)
                {
                    unit.change_team(team);
                    if (!unit.is_attackable_team(team_turn))
                        remove_enemy_attack_range(id);
                    unit.refresh_sprite();
                    unit.queue_move_range_update();
                }
            }
        }

        public int highest_priority_unit(int team_id, bool boss = false)
        {
            // Return team leader, if alive
            if (Objects.unit_exists(Team_Leaders[team_id]) && this.units[Team_Leaders[team_id]].visible_by())
                return Team_Leaders[team_id];
            // Search through bosses
            int unit_priority = -1;
            int unit_id = -1;
            // Duplicate code //Yeti
            foreach (int id in this.units.Keys)
            {
                var unit = this.units[id];
                if (unit.boss && unit.visible_by() && unit.team == team_id) // This should require the unit be on the given team, right...? //Yeti
                    if (unit.priority > unit_priority)
                    {
                        unit_id = id;
                        unit_priority = unit.priority;
                    }
            }
            if (boss || unit_id > -1)
                return unit_id;
            // Search through regular units
            unit_priority = -1;
            unit_id = -1;
            foreach (int id in this.units.Keys)
            {
                var unit = this.units[id];
                if (unit.visible_by() && unit.team == team_id) // This should require the unit be on the given team, right...? //Yeti
                    if (unit.priority > unit_priority)
                    {
                        unit_id = id;
                        unit_priority = unit.priority;
                    }
            }
            return unit_id;
        }

        public bool unit_defeated(int id)
        {
            return Defeated_Units.ContainsKey(id);
        }
        public bool actor_defeated(int id)
        {
            return Defeated_Units.ContainsValue(id);
        }

        public bool defeated_actor_was_ally(int id)
        {
            if (Defeated_Units.ContainsValue(id))
            {
                int unit_id = Defeated_Units.First(x => x.Value == id).Key;
                return DefeatedAlliedUnits.Contains(unit_id);
            }
            return false;
        }

        public int defeated_ally_count()
        {
            return DefeatedAlliedUnits.Count;
        }

        public int unit_distance(int id1, int id2)
        {
            // if scene_action return scene.distance //Yeti
            if (Global.scene.is_action_scene)
            {
#if DEBUG
                throw new Exception("wah");
#endif
            } // where is this called from that it needs to do this?
            return Objects.unit_distance(id1, id2);
        }

        public int combat_distance(int id1, int id2)
        {
            // if scene_action return scene.distance //Yeti
            if (Global.scene.is_action_scene)
            {
#if DEBUG
                throw new Exception("wah");
#endif
            } // where is this called from that it needs to do this?
            Game_Unit unit1 = this.units[id1];
            Combat_Map_Object unit2 = attackable_map_object(id2);
            if (unit1.combat_distance_override() > -1)
                return unit1.combat_distance_override();
            if (unit2.is_unit() && ((Game_Unit)unit2).combat_distance_override() > -1)
                return ((Game_Unit)unit2).combat_distance_override();
            return unit_distance(id1, id2);
        }

        public int distance(Vector2 a, Vector2 b)
        {
            return (int)(Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));
        }

        public Combat_Map_Object attackable_map_object(int id)
        {
            return Objects.attackable_map_object(id);
        }

        public Map_Object displayed_map_object(int id)
        {
            return Objects.displayed_map_object(id);
        }
        #endregion

        #region Move Range Update
        public void refresh_move_ranges()
        {
            refresh_move_ranges(false);
        }
        public void refresh_move_ranges(bool update_all)
        {
            Refresh_Move_Ranges = true;
            if (update_all)
            {
                Refresh_All_Ranges = true;
                clear_updated_move_ranges();
            }

            // Ugly hack to deal with dead units hanging around the map //Debug
            // When does this even happen, though //Yeti
            //List<int> ids = new List<int>(); // Below is a smarter way to initialize this list //Debug
            //foreach (int id in Units.Keys)
            //    ids.Add(id);
            var units = this.units.Values.ToList();
            // One cause is adding units that died during a chapter back in during ending events, they still have 0 hp and die again
            // Any situation where a unit with 0 hp is added will call this though
            foreach (var unit in units)
                if (unit.is_dead && !unit.dead)
                    unit.kill(false);
        }

        public void update_move_range_loop()
        {
            while (true)
            {
                if (Refresh_Move_Ranges && Run_Move_Update)
                {
                    update_move_ranges();
                }
                else
                    // Waits 1/10th of a frame each loop
                    System.Threading.Thread.Sleep(
                        TimeSpan.FromTicks((int)(TimeSpan.TicksPerSecond * (1.0f / ((float)Config.FRAME_RATE * 10)))));
            }
        }

        protected void update_move_ranges()
        {
            // Don't update this frame if events are running?
            bool do_refresh = true; // !Global.game_system.is_interpreter_running; //Debug
            if (do_refresh)
                foreach (Game_Unit unit in this.units.Values)
                    if (!unit.is_on_square)
                    {
                        // Don't update this frame if anyone is moving
                        do_refresh = false;
                        break;
                    }
            if (do_refresh)
            {
                lock (Move_Range_Lock)
                {
                    int i = 0;
                    update_fow();
                    Pathfind.reset();
                    foreach (Game_Unit unit in this.units.Values)
                    {
                        if (!unit.dead)
                        {
                            // If this unit should update
                            if (test_unit_move_range_update(unit))
                            {
                                check_update_unit_move_range(unit);
                                i++;
                            }
                        }
                    }

                    update_enemy_range();
                    Refresh_Move_Ranges = false;
                    Refresh_All_Ranges = false;
                    Team_Range_Updates.Clear();
                    Unit_Range_Updates.Clear();
                    Run_Move_Update = false;
                    if (Global.scene.is_map_scene)
                        highlight_test(); // Testing putting this here instead of at the end of the wait method //Debug
                }
            }
        }

        public void update_fow()
        {
            update_fow(Constants.Team.PLAYER_TEAM);
        }
        protected void update_fow(int updating_team)
        {
            Fow_Updated = true;
            if (!Fow)
            {
                foreach (HashSet<Vector2> visibility in Fow_Visibility)
                    visibility.Clear();
                return;
            }
            lock (Fow_Visibility)
            {
                // Calculate the sight ranges by team groups
                for (int i = 0; i < Constants.Team.TEAM_GROUPS.Length; i++)
                {
                    int[] group = Constants.Team.TEAM_GROUPS[i];
                    List<int> team = new List<int>();
                    foreach (int team_id in group)
                        team.AddRange(Teams[team_id]);
                    // Remove units that are rescued
                    int j = 0;
                    while (j < team.Count)
                    {
                        if (this.units[team[j]].is_rescued)
                            team.RemoveAt(j);
                        else
                            j++;
                    }
                    // Calculate visible area
                    List<Fow_View_Object> viewers = new List<Fow_View_Object>();
                    for (j = 0; j < team.Count; j++)
                    {
                        Game_Unit unit = this.units[team[j]];
                        viewers.Add(new Fow_View_Object(unit));
                    }

                    viewers.AddRange(Torch_Staves);
                    viewers.AddRange(VisionPoints);

                    HashSet<Vector2> visibility = Pathfind.fow_sight_area(viewers);
                    foreach (int team_id in group)
                        Fow_Visibility[team_id] = visibility;
                }
            }
        }

        protected bool test_unit_move_range_update(Game_Unit unit)
        {
            // Everyone needs updated
            if (Refresh_All_Ranges)
                return true;
            // Specific teams that need updated
            else if (Team_Range_Updates.ContainsKey(unit.team))
            {
                foreach (Vector2 loc in Team_Range_Updates[unit.team])
                    if (distance(unit.loc, loc) <= unit.mov)
                    {
                        remove_updated_move_range(unit.id);
                        return true;
                    }
            }
            // Specific units that need updated
            else if (Unit_Range_Updates.Contains(unit.id))
                return true;
            return false;
        }

        protected bool Run_Move_Update;
        public void run_move_range_update()
        {
            if (Run_Move_Update)
                return;
            if (Refresh_Move_Ranges)
            {
                // Don't update this frame if suspending
                // Don't update this frame if events are running?
                bool do_refresh = !Global.scene.suspend_calling; //!Global.game_system.is_interpreter_running; //Debug
                if (do_refresh)
                    foreach (Game_Unit unit in this.units.Values)
                        if (!unit.is_on_square)
                        {
                            // Don't update this frame if anyone is moving
                            do_refresh = false;
                            break;
                        }
                if (do_refresh)
                {
                    Pathfind.reset();
                    List<int> dead_units = new List<int>();
                    foreach (Game_Unit unit in this.units.Values)
                    {
                        // If the map has existant size, and a not-dead and not-gladiator unit is off the map, they shouldn't be off the map so kill them
                        if (this.width > 0 && is_off_map(unit.loc, false) && !unit.dead && !unit.is_rescued && !unit.gladiator)
                        {
#if DEBUG
                             System.Diagnostics.Debug.Assert(unit.rescued == 0,
                                 "Unit being removed for being off map\nshouldn't be rescued, but is in the middle\nof rescue code?");
#endif
                            unit.kill(false);
                            dead_units.Add(unit.id);
                        }
                        else if (unit.dead)
                            dead_units.Add(unit.id);
                    }
                    foreach (int id in dead_units)
                        remove_unit(id);
                    // Moved down from above
                    //Pathfinding.reset();
                    Run_Move_Update = true;
                    refresh_alpha(30);
                }
            }
        }

        internal void wait_for_move_update() //private //Yeti
        {
            for (; ; )
            {
                run_move_range_update();
                System.Threading.Thread.Sleep(1);
                if (!Refresh_Move_Ranges)
                    break;
            }
        }

        public void add_unit_wait(int unit_id, bool update_move_ranges)
        {
            Waiting_Units.Add(new Tuple<int, bool>(unit_id, update_move_ranges));
        }

        public bool unit_waiting(int unit_id)
        {
            foreach (Tuple<int, bool> pair in Waiting_Units)
                if (pair.Item1 == unit_id)
                    return true;
            return false;
        }
        public bool units_waiting()
        {
            return Waiting_Units.Count > 0;
        }

        public void add_unit_wait_skip(int unit_id)
        {
            Waiting_Unit_Skip.Add(unit_id);
        }
        public bool skip_unit_wait(int unit_id)
        {
            return Waiting_Unit_Skip.Contains(unit_id);
        }
        #endregion

        #region Update
        public bool update()
        {
            Scene_Map scene_map = get_scene_map();
            update_map_functions();

            if (Global.Input.soft_reset())
                return false;

            // Timer stuff
            update_timers();

            update_scroll_position();
            update_unit_wait();
            Objects.update();
            for (int i = 0; i < move_sound_timers.Count; i++)
                if (move_sound_timers[i] > 0) move_sound_timers[i]--;
            // Move arrow update
            update_move_arrow();
            return true;
        }

        protected void update_map_functions()
        {
            update_formation();
            update_dying_units();
        }

        protected void update_timers()
        {
            update_icon_timer();
            Move_Range_Anim_Count = (Move_Range_Anim_Count + 1) % Config.MOVE_RANGE_TIME;
            if (range_start_timer < 16)
                range_start_timer = range_start_timer + 2;
            if (range_start_timer == 2)
                range_start_timer = 5;
        }

        private void update_icon_timer()
        {
            rescue_anim_timer = (rescue_anim_timer + 1) % Config.RESCUE_TIME;
            if (rescue_anim_timer == 0)
                rescue_anim_loops = (rescue_anim_loops + 1) % 2882880; // Divisible by every integer from 1-16, as well as 32 and 64
        }

        protected void update_unit_wait()
        {
            int i = 0;
            while (i < Waiting_Units.Count)
            {
                if (!Objects.unit_exists(Waiting_Units[i].Item1))
                    Waiting_Units.RemoveAt(i);
                else
                    i++;
            }
            if (Waiting_Units.Count > 0)
            {
                if (!Global.game_system.is_interpreter_running)
                {
                    Global.game_state.allow_auto_turn_end();

                    if (!Wait_Events_Run)
                        Global.game_state.unit_wait_events();
                    if (Global.game_system.is_interpreter_running)
                    {
                        Wait_Events_Run = true;
                        Wait_Suspend_Called = false;
                        return;
                    }
                    foreach (Tuple<int, bool> pair in Waiting_Units)
                        this.units[pair.Item1].wait(pair.Item2);
                    Waiting_Units.Clear();
                    Waiting_Unit_Skip.Clear();
                    // This was getting called before the move range update loop could fire //Debug
                    // Which updated one unit early and also before fow updated
                    //highlight_test();
                }
                else
                {
                    if (!Wait_Suspend_Called) // I guess this isn't used anymore? //Debug
                    { }//Global.scene.suspend();
                    Wait_Suspend_Called = true;
                    return;
                }
            }
            Wait_Events_Run = false;
            Wait_Suspend_Called = false;
        }

        public void update_characters_while_waiting()
        {
            update_icon_timer();
            Move_Range_Anim_Count = (Move_Range_Anim_Count + 1) % Config.MOVE_RANGE_TIME;
            Objects.update();
        }

        public void highlight_test()
        {
            if (this.width == 0 || this.height == 0)
                return;

            //@Debug: changing_formation really needs to be a game_state property
            // Clear move range if no unit is selected
            if (Global.game_system.Selected_Unit_Id == -1 && !((Scene_Map)Global.scene).changing_formation)
            {
                clear_move_range();
            }
            // If player turn is ending return
            if (Global.game_state.ally_turn_end_check(true, true) || Global.game_system.is_interpreter_running)
                return;
            // If a unit is already highlighted
            Game_Unit highlighted_unit = get_highlighted_unit();
            if (highlighted_unit != null)
            {
                // Test with the unit if it is still highlighted
                highlighted_unit.highlight_test();
                if (highlighted_unit.highlighted)
                    return;
            }
            // Check unit at cursor's location
            Game_Unit unit = get_unit(Global.player.loc);
            if (unit != null)
            {
                bool must_be_playable_map = true;
#if DEBUG
                if (Global.scene.scene_type == "Scene_Map_Unit_Editor")
                    must_be_playable_map = false;
#endif
                if (!must_be_playable_map || !is_off_map(unit.loc))
                    unit.highlight_test();
            }
        }
        #endregion

        #region User Controls
        // A
        internal bool select_unit(
            Game_Unit highlighted_unit, bool onlyIfSelectable = false)
        {
#if DEBUG
            if (Global.scene.scene_type == "Scene_Map_Unit_Editor")
            {
                Global.game_system.Selected_Unit_Id = highlighted_unit.id;
                highlighted_unit.menu(Global.player.loc);
            }
            else
#endif
                // If it is the selected unit's turn and they are available
                if (highlighted_unit.is_active_team &&
                    !highlighted_unit.unselectable) //Multi
                {
                    Global.game_system.Selected_Unit_Id = highlighted_unit.id;
                    highlighted_unit.open_move_range();
                    highlight_test();
                    if (highlighted_unit.mov <= 0)
                        highlighted_unit.menu(highlighted_unit.loc);
                    else
                        Global.game_system.play_se(System_Sounds.Unit_Select);
                    return true;
                }
                // Else just show their move range
                else if (!onlyIfSelectable)
                {
                    Global.game_system.Selected_Unit_Id = highlighted_unit.id;
                    highlighted_unit.non_ally_move_range();
                    Global.game_temp.highlighted_unit_id = -1;
                    Global.game_system.play_se(System_Sounds.Open);
                    return true;
                }
            return false;
        }
        
        internal bool select_preparations_unit(
            Game_Unit highlighted_unit, bool onlyIfSelectable = false)
        {
            if (((Scene_Map)Global.scene).changing_formation)
            {
                if (highlighted_unit.is_active_team && Global.game_map.deployment_points.Contains(highlighted_unit.loc)) //Multi
                {
                    Global.game_system.Selected_Unit_Id = highlighted_unit.id;
                    Global.game_system.play_se(System_Sounds.Unit_Select);
                    return true;
                }
                else
                {
                    Global.game_system.play_se(System_Sounds.Buzzer);
                    return false;
                }
            }
            else
            {
                if (!highlighted_unit.is_player_allied &&
                        onlyIfSelectable)
                    return false;

                Global.game_system.Selected_Unit_Id = highlighted_unit.id;
                highlighted_unit.non_ally_move_range();
                Global.game_temp.highlighted_unit_id = -1;
                Global.game_system.play_se(System_Sounds.Open);
                return true;
            }
            return false;
        }

        internal void move_selected_unit(Game_Unit selected_unit)
        {
            Global.game_temp.ResetContextSensitiveUnitControl();

            // Move the selected unit if possible
            selected_unit = get_selected_unit();
            if (selected_unit.is_active_team &&
                !selected_unit.unselectable &&
                Global.player.is_on_square) // && !menu_call //Yeti //Multi
            {
                Vector2 moveLoc = Global.player.loc;
                move_selected_unit_to(selected_unit, moveLoc);
            }
            else if (!selected_unit.is_active_team || selected_unit.unselectable) //Multi
            {
                deselect_unit();
            }
        }

        private void move_selected_unit_to(Game_Unit selected_unit, Vector2 moveLoc)
        {
            var attack = context_sensitive_attack(selected_unit, moveLoc);
            if (attack != null)
            {
                attack.Apply();
                moveLoc = attack.MoveLoc;
            }
            
            Game_Unit unit_here = get_unit(moveLoc);

            // If tile is in range
            if (selected_unit.can_move_to(moveLoc, Move_Range))
            {
                // Tile okay to move to
                if (unit_here == null || unit_here == selected_unit)
                {
                    selected_unit.menu(moveLoc);
                }
                // Someone hidden is on the tile, go crash into them
                else if (!unit_here.visible_by(selected_unit.team))
                {
                    selected_unit.menu(moveLoc);
                }
                // Tile is blocked
                else
                {
                    // Reset if didn't move
                    Global.game_temp.ResetContextSensitiveUnitControl();

                    Global.game_system.play_se(System_Sounds.Buzzer);
                }
            }
            else
            {
                // Reset if didn't move
                Global.game_temp.ResetContextSensitiveUnitControl();

                // If using a pointing input scheme, and can cancel move, and no unit at cursor
                if (Input.ControlScheme != ControlSchemes.Buttons &&
                    !selected_unit.cannot_cancel_move() &&
                    no_unit_at_location(Global.player.loc))
                {
                    selected_unit.cancel_move();
                }
                // else buzz
                else
                {
                    // Can't clear move range
                    Global.game_system.play_se(System_Sounds.Buzzer);
                }
            }
        }

        private CSUnitAttack context_sensitive_attack(Game_Unit selectedUnit, Vector2 moveLoc)
        {
            Game_Unit unit_here = get_unit(moveLoc);

            // If should be trying to attack the unit that is here
            if (unit_here != null && unit_here != selectedUnit &&
                unit_here.visible_by() &&
                unit_here.is_attackable_team(selectedUnit))
            {
                // Determine if the target is in range
                if (selectedUnit.attack_range.Contains(moveLoc))
                {
                    // Check siege engines first
                    if (selectedUnit.can_use_siege())
                    {
                        var sieges = selectedUnit.siege_weapons_in_range(
                                this.siege_engines.Values, Move_Range)
                            .Where(x => !is_blocked(x.loc, selectedUnit.id));
                        sieges = selectedUnit.sieges_in_range_of_target(sieges, moveLoc);
                        sieges = sieges
                            .OrderBy(x => distance(x.loc, moveLoc))
                            .ThenBy(x => distance(x.loc, selectedUnit.loc))
                            .ToList();

                        if (sieges.Any())
                        {
                            return new CSUnitAttack(
                                sieges.First().loc,
                                Global.player.loc,
                                Siege_Engine.SIEGE_INVENTORY_INDEX);
                        }
                    }
                    
                    // Get attack range around the target for all weapons
                    HashSet<Vector2> attackRange =
                        selectedUnit.get_weapon_range(
                            selectedUnit.actor.useable_weapons(),
                            new HashSet<Vector2> { moveLoc });
                    // First check if any tile on the move arrow can reach attack range
                    for (int i = 0; i < Move_Arrow.Count; i++)
                    {
                        Vector2 arrowLoc = Move_Arrow[i].Loc;
                        int atkDist = distance(arrowLoc, moveLoc);
                        if (!is_blocked(arrowLoc, selectedUnit.id))
                            if (attackRange.Contains(arrowLoc))
                            {
                                return new CSUnitAttack(
                                    arrowLoc,
                                    Global.player.loc,
                                    selectedUnit.first_weapon_with_range(atkDist));
                            }
                    }

                    // Then check what movable tiles are in attack range
                    var moveRange = Move_Range
                        .Intersect(attackRange)
                        .Where(x => !is_blocked(x, selectedUnit.id))
                        .OrderBy(x => distance(x, selectedUnit.loc))
                        .ThenBy(x => distance(x, moveLoc))
                        .ToList();
                    if (moveRange.Any())
                    {
                        Vector2 loc = moveRange.First();
                        int atkDist = distance(loc, moveLoc);
                                
                        return new CSUnitAttack(
                            loc,
                            Global.player.loc,
                            selectedUnit.first_weapon_with_range(atkDist));
                    }
                }
            }
            
            return null;
        }

        // B
        internal void deselect_unit(bool clear_move_range = true)
        {
            if (clear_move_range)
                this.clear_move_range();
            Global.game_system.Selected_Unit_Id = -1;
            highlight_test();
            Global.game_system.play_se(System_Sounds.Cancel);
        }

        // X
        internal void try_toggle_enemy_range(Game_Unit unit, int ally_team)
        {
            if (!Global.game_state.is_menuing && Global.game_state.is_map_ready())
            {
                bool single_unit = false;
                if (unit != null)
                    if (unit.is_attackable_team(ally_team)) //Multi
                        single_unit = true;
                // If on a single enemy unit
                if (single_unit)
                    Global.game_system.play_se(
                        toggle_enemy_range(unit.id) ? System_Sounds.Open : System_Sounds.Cancel);
                else
                    Global.game_system.play_se(
                        toggle_all_enemy_range() ? System_Sounds.Open : System_Sounds.Cancel);
            }
        }

        internal void deselect_enemy_range_unit(Game_Unit highlighted_unit, int ally_team)
        {
            if (Global.game_system.Selected_Unit_Id == -1 &&
                       Global.game_temp.highlighted_unit_id != -1 &&
                       highlighted_unit.is_attackable_team(ally_team)) //Multi
            {
                if (Range_Enemies.Contains(Global.game_temp.highlighted_unit_id))
                {
                    remove_enemy_attack_range(Global.game_temp.highlighted_unit_id);
                    Global.game_system.play_se(System_Sounds.Cancel);
                }
                else
                {
                    if (reset_individual_enemy_range_data())
                        Global.game_system.play_se(System_Sounds.Cancel);
                }
            }
        }

        // Select
        internal void open_map_menu(Game_Unit highlighted_unit)
        {
            if (Global.game_system.Selected_Unit_Id == -1 && !Scrolling)
            {
                if (Global.game_temp.highlighted_unit_id != -1)
                {
                    highlighted_unit.highlighted = false;
                    Global.game_temp.highlighted_unit_id = -1;
                    clear_move_range();

                }
                Global.game_temp.map_menu_call = true;
                Global.game_temp.menu_call = true;
            }
        }

        // Start
        internal void open_minimap(Game_Unit highlighted_unit)
        {
            if (Global.game_system.Selected_Unit_Id == -1 &&
                (!Scrolling || Input.ControlScheme == ControlSchemes.Touch))
            {
                if (Global.game_temp.highlighted_unit_id != -1)
                {
                    highlighted_unit.highlighted = false;
                    Global.game_temp.highlighted_unit_id = -1;
                    clear_move_range();

                }
                Global.player.center_cursor(true);
                Global.game_temp.minimap_call = true;
            }
        }

        // L
        internal void next_unit(Game_Unit highlighted_unit, bool selected_moving)
        {
            if (!Scrolling && !Global.game_state.is_menuing && Global.game_state.is_map_ready() && !selected_moving)
            {
                bool unit_selected = Global.game_system.Selected_Unit_Id != -1;
                List<int> temp_team = new List<int>();
                // Jump back to selected unit
                if (unit_selected)
                    temp_team.Add(Global.game_system.Selected_Unit_Id);
                // Gets team in order
                else
                {
                    int team = highlighted_unit == null ?
                        (team_turn == 0 ? Constants.Team.PLAYER_TEAM : team_turn) :
                        highlighted_unit.team; //Multi
                    if (Teams[team].Contains(Team_Leaders[team]))
                    {
                        Game_Unit team_leader = this.units[Team_Leaders[team]];
                        if (team_leader.ready && !team_leader.is_rescued && team_leader.visible_by())
                            temp_team.Add(Team_Leaders[team]);
                    }
                    foreach (int id in Teams[team])
                    {
                        Game_Unit unit = this.units[id];
                        if (unit.ready && !unit.is_rescued && unit.visible_by() &&
                                !is_off_map(unit.loc) && !temp_team.Contains(id))
                            temp_team.Add(id);
                    }
                }
                if (temp_team.Count > 0)
                {
                    bool moved = true;
                    // If a unit is already highlighted
                    if (temp_team.Contains(Global.game_temp.highlighted_unit_id))
                    {
                        // And there are other units
                        if (temp_team.Count > 1)
                        {
                            get_highlighted_unit().highlighted = false;
                            Global.game_temp.highlighted_unit_id = temp_team[
                                (temp_team.IndexOf(Global.game_temp.highlighted_unit_id) + 1) %
                                temp_team.Count];
                        }
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Cancel);
                            moved = false;
                        }
                    }
                    else
                    {
                        if (Global.game_temp.highlighted_unit_id != -1)
                            get_highlighted_unit().highlighted = false;
                        Global.game_temp.highlighted_unit_id = temp_team[0];
                    }
                    if (moved)
                    {
                        Global.player.next_unit = true;
                        highlighted_unit = get_highlighted_unit();
                        Global.player.loc = highlighted_unit.loc;
                        if (unit_selected)
                            update_move_arrow();
                        else
                            highlighted_unit.highlighted = true;
                        Global.player.instant_move = true;
                        Global.player.force_loc(highlighted_unit.loc);
                        Global.game_system.play_se(System_Sounds.Cancel);
                        if (!unit_selected)
                            if (Global.scene.scene_type == "Scene_Map"
#if DEBUG
 || Global.scene.scene_type == "Scene_Map_Unit_Editor"
#endif
)
                                ((Scene_Map)Global.scene).update_info_image(true);
                    }
                }
                else
                    Global.game_system.play_se(System_Sounds.Cancel);
                //selected_unit = Units[Global.game_system.Selected_Unit_Id];
            }
        }

        internal void switch_formation(Game_Unit selected_unit)
        {
            if (!Deployment_Points.Contains(Global.player.loc))
                Global.game_system.play_se(System_Sounds.Buzzer);
            else
            {
                Game_Unit other_unit = get_unit(Global.player.loc);
                if (other_unit != null)
                    other_unit.formation_change(selected_unit.loc);
                selected_unit.formation_change(Global.player.loc);

                Global.game_system.Selected_Unit_Id = -1;
                Global.game_system.play_se(System_Sounds.Formation_Change);
                Move_Range_Visible = false;
                Changing_Formation = true;
                Formation_Unit_Id1 = selected_unit.id;
                if (other_unit != null)
                    Formation_Unit_Id2 = other_unit.id;
            }
        }
        #endregion

        #region Scrolling
        public float min_scroll_x
        {
            get
            {
                if (this.width <= Config.WINDOW_WIDTH / TILE_SIZE)
                    return -((Config.WINDOW_WIDTH / TILE_SIZE) - this.width) / 2f;
                return (restrict_scroll_to_playable ? edge_offset_left :
                    (edge_offset_left <= 0 || this.width * TILE_SIZE <= Config.WINDOW_WIDTH) ? 0 : 0.5f);
            }
        }
        public float min_scroll_y
        {
            get
            {
                if (this.height <= Config.WINDOW_HEIGHT / TILE_SIZE)
                    return -((Config.WINDOW_HEIGHT / TILE_SIZE) - this.height) / 2f;
                return (restrict_scroll_to_playable ? edge_offset_top :
                    (edge_offset_top <= 0 || this.height * TILE_SIZE <= Config.WINDOW_HEIGHT) ? 0 : 0.5f);
            }
        }
        public float max_scroll_x
        {
            get
            {
                if (this.width <= Config.WINDOW_WIDTH / TILE_SIZE)
                    return -((Config.WINDOW_WIDTH / TILE_SIZE) - this.width) / 2f;
                return ((this.width - (edge_offset_right <= 0 || restrict_scroll_to_playable ? edge_offset_right : 0.5f)) -
                    Config.WINDOW_WIDTH / TILE_SIZE);
            }
        }
        public float max_scroll_y
        {
            get
            {
                if (this.height <= Config.WINDOW_HEIGHT / TILE_SIZE)
                    return -((Config.WINDOW_HEIGHT / TILE_SIZE) - this.height) / 2f;
                return ((this.height - (edge_offset_bottom <= 0 || restrict_scroll_to_playable ? edge_offset_bottom : 0.5f)) -
                    Config.WINDOW_HEIGHT / TILE_SIZE);
            }
        }

        private bool restrict_scroll_to_playable
        {
            get
            {
                // If the camera wants to be locked to playable, and either no events are running or this is an instant event scroll
                //return Global.game_options.controller == 2 && (!Global.game_system.is_interpreter_running || Evented_Instant_Scroll); //Debug
                return Global.game_options.controller == 2 && (!Event_Called_Scroll || Evented_Instant_Scroll);
            }
        }

        public bool center(Vector2 loc, bool soft_center,
            bool event_called = false, bool forced = false, bool can_pass_edges = true)
        {
            if (Controlled_Scroll)
                return false; //Debug

            if (!event_called && !forced)
                if (Input.ControlScheme == ControlSchemes.Touch)
                    return false;

            ScrollSpeed = Vector2.Zero;

            Event_Called_Scroll = event_called;
            // Try this with an odd number of tiles onscreen //Yeti

            float center_x = (float)Math.Round((Config.WINDOW_WIDTH / TILE_SIZE) / 2f);
            float center_y = (float)Math.Round((Config.WINDOW_HEIGHT / TILE_SIZE) / 2f);

            float min_x = min_scroll_x;
            float min_y = min_scroll_y;
            float max_x = max_scroll_x;
            float max_y = max_scroll_y;

            float actual_display_x = Actual_Display_X / (float)UNIT_TILE_SIZE;
            float actual_display_y = Actual_Display_Y / (float)UNIT_TILE_SIZE;

            bool soft_scroll_started = false;

            if (soft_center)
            {
                // Tiles on either side of the center of the screen that the center point must be further away than to bother scrolling
                float ox1 = (Config.WINDOW_WIDTH / TILE_SIZE) / 2f - x_soft_center;
                float oy1 = (Config.WINDOW_HEIGHT / TILE_SIZE) / 2f - y_soft_center;
                // Center loc + ox/oy, ie the right/bottom edge of the space the center point must be further than
                float ox2 = center_x + ox1;
                float oy2 = center_y + oy1;

                bool temp_scrolling = Scrolling;
                bool invalid_x_scroll = actual_display_x < min_x || actual_display_x > max_x;
                bool invalid_y_scroll = actual_display_y < min_y || actual_display_y > max_y;
                // Scroll down
                if (invalid_y_scroll || loc.Y + 1 - actual_display_y > center_y + oy1)
                {
                    float scroll = Math.Max(min_y, Math.Min(max_y, loc.Y + 1 - oy2));
                    if (scroll == min_y || scroll == max_y)
                        float_display_y = scroll * TILE_SIZE;
                    else
                        display_y = (int)Math.Ceiling(scroll) * TILE_SIZE;
                }
                // Scroll left
                if (invalid_x_scroll || loc.X - actual_display_x < center_x - ox1)
                {
                    float scroll = Math.Max(min_x, Math.Min(max_x, loc.X - (Config.WINDOW_WIDTH / TILE_SIZE) + ox2));
                    if (scroll == min_x || scroll == max_x)
                        float_display_x = scroll * TILE_SIZE;
                    else
                        display_x = (int)Math.Floor(scroll) * TILE_SIZE;
                }
                // Scroll right
                if (invalid_x_scroll || loc.X + 1 - actual_display_x > center_x + ox1)
                {
                    float scroll = Math.Max(min_x, Math.Min(max_x, loc.X + 1 - ox2));
                    if (scroll == min_x || scroll == max_x)
                        float_display_x = scroll * TILE_SIZE;
                    else
                        display_x = (int)Math.Ceiling(scroll) * TILE_SIZE;
                }
                // Scroll up
                if (invalid_y_scroll || loc.Y - actual_display_y < center_y - oy1)
                {
                    float scroll = Math.Max(min_y, Math.Min(max_y, loc.Y - (Config.WINDOW_HEIGHT / TILE_SIZE) + oy2));
                    if (scroll == min_y || scroll == max_y)
                        float_display_y = scroll * TILE_SIZE;
                    else
                        display_y = (int)Math.Floor(scroll) * TILE_SIZE;
                }
                if (Scrolling && !temp_scrolling)
                {
                    soft_scroll_started = true;
                    scroll_speed = Math.Max(1, TILE_SIZE / 2);
                }
            }
            else
            {
                if (can_pass_edges)
                {
                    display_x = (int)Math.Round(Math.Max(min_x, Math.Min(max_x, loc.X - center_x))) * TILE_SIZE;
                    display_y = (int)Math.Round(Math.Max(min_y, Math.Min(max_y, loc.Y - center_y))) * TILE_SIZE;
                }
                else
                {
                    float_display_x = Math.Max(min_x, Math.Min(max_x, loc.X - center_x)) * TILE_SIZE;
                    float_display_y = Math.Max(min_y, Math.Min(max_y, loc.Y - center_y)) * TILE_SIZE;
                }
                scroll_speed = 0;
            }
            return soft_scroll_started;
        }

        private int x_soft_center { get { return Input.IsControllingOnscreenMouse ? 2 : 3; } }
        private int y_soft_center { get { return Input.IsControllingOnscreenMouse ? 2 : 3; } }

        private bool Event_Called_Scroll, Evented_Instant_Scroll;
        internal void set_scroll_loc(Vector2 loc, bool immediately, bool ignore_playable_restriction)
        {
            // If instantly moving the camera, will restrict the camera bounds to the playable area if that option is set by the player
            // Unless ignore_playable_restriction is true, then it will be overridden and can always go outside the playable space
            Evented_Instant_Scroll = immediately && !ignore_playable_restriction;
            Event_Called_Scroll = true;
            float_display_x = loc.X * TILE_SIZE;
            float_display_y = loc.Y * TILE_SIZE;
            Evented_Instant_Scroll = false;
            Event_Called_Scroll = false;
            // Should the camera position instantly instead of panning?
            if (immediately)
            {
                Global.game_system.Instant_Move = true;
                update_scroll_position();
                Scrolling = false;
            }
        }

        internal void fix_scroll_loc()
        {
            Evented_Instant_Scroll = true;
            float_display_x = Target_Display_X * TILE_SIZE;
            float_display_y = Target_Display_Y * TILE_SIZE;
            Evented_Instant_Scroll = false;
        }

        //static int MAX_SCROLL_SPEED = TILE_SIZE / 2; //8 //16 //Debug
        //static int MIN_SCROLL_SPEED = Math.Max(1, UNIT_TILE_SIZE / TILE_SIZE);
        public int scroll_speed = -1;
        public void update_scroll_position()
        {
            if (is_scroll_vel_over())
                ScrollSpeed = Vector2.Zero;
            else
            {
                scroll_loc(ScrollSpeed);
                ScrollSpeed *= 0.9f;
            }

            int display_x = (int)(Target_Display_X * UNIT_TILE_SIZE);
            int display_y = (int)(Target_Display_Y * UNIT_TILE_SIZE);

            int x_distance = 0, y_distance = 0;
            int x_move_sign, y_move_sign;

            int min_speed = Math.Max(scroll_speed, Math.Max(1, UNIT_TILE_SIZE / TILE_SIZE));
            int max_speed = UNIT_TILE_SIZE / 2;

            Scrolling = false;
            bool instant_move = Global.game_system.Instant_Move || Global.game_state.skip_ai_turn_active;
            if (Controlled_Scroll)
            {
                Scrolling = true;
                x_distance = (int)Math.Abs(Actual_Display_X - display_x);// / 2;
                x_move_sign = display_x < Actual_Display_X ? -1 : 1;
                y_distance = (int)Math.Abs(Actual_Display_Y - display_y);// / 2;
                y_move_sign = display_y < Actual_Display_Y ? -1 : 1;
            }
            else
            {
                // x scroll
                if ((Math.Abs(Actual_Display_X - display_x) <= min_speed || instant_move))
                {
                    Scrolling |= Math.Abs(Actual_Display_X - display_x) > 0;
                    Target_Display_X = ((int)(Target_Display_X * UNIT_TILE_SIZE)) / (float)UNIT_TILE_SIZE;
                    Actual_Display_X = (int)(Target_Display_X * UNIT_TILE_SIZE);
                    Global.game_system.Instant_Move = false;
                    x_distance = 0;
                    x_move_sign = 1;
                }
                else
                {
                    x_distance = (int)Math.Max(Math.Abs(Actual_Display_X - display_x) / 4, 1);
                    //x_distance = (int)Math.Max(Math.Abs(Actual_Display_X - display_x) / 2, 1);
                    Scrolling = true;
                    x_move_sign = display_x < Actual_Display_X ? -1 : 1;
                }
                // y scroll
                if ((Math.Abs(Actual_Display_Y - display_y) <= min_speed || instant_move))
                {
                    Scrolling |= Math.Abs(Actual_Display_Y - display_y) > 0;
                    Target_Display_Y = ((int)(Target_Display_Y * UNIT_TILE_SIZE)) / (float)UNIT_TILE_SIZE;
                    Actual_Display_Y = (int)(Target_Display_Y * UNIT_TILE_SIZE);
                    Global.game_system.Instant_Move = false;
                    y_distance = 0;
                    y_move_sign = 1;
                }
                else
                {
                    y_distance = (int)Math.Max(Math.Abs(Actual_Display_Y - display_y) / 4, 1);
                    //y_distance = (int)Math.Max(Math.Abs(Actual_Display_Y - display_y) / 2, 1);
                    Scrolling = true;
                    y_move_sign = display_y < Actual_Display_Y ? -1 : 1;
                }
            }

            //Debug
            if (Global.player.next_unit && !Scrolling_Previous && Scrolling)
                if (get_scene_map() != null)
                { }// get_scene_map().info_windows_offscreen(); // What's this for //Debug

            Scrolling_Previous = Scrolling;

            
#if DEBUG
            System.Diagnostics.Debug.Assert(x_distance >= 0 && y_distance >= 0);
#endif
            if ((x_distance != 0 || y_distance != 0) && !Controlled_Scroll)
            {
                float angle = ((float)Math.Atan2(
                    y_distance * y_move_sign, x_distance * x_move_sign));
                if (scroll_speed > 0 && scroll_speed >= min_speed)
                {
                    if (x_distance > 0)
                        x_distance = scroll_speed;
                    if (y_distance > 0)
                        y_distance = scroll_speed;
                }
                else
                {
                    if (x_distance > 0 && y_distance > 0)
                    {
                        int x_max, y_max;
                        if (x_distance < min_speed && y_distance < min_speed)
                        {
                            x_max = (int)Math.Round(Math.Abs(Math.Cos(angle) * min_speed));
                            y_max = (int)Math.Round(Math.Abs(Math.Sin(angle) * min_speed));

                            if (x_max > y_max)
                            {
                                y_distance = (min_speed * y_max) / x_max;
                                x_distance = min_speed;
                            }
                            else
                            {
                                x_distance = (min_speed * x_max) / y_max;
                                y_distance = min_speed;
                            }
                        }
                        else
                        {
                            x_max = (int)Math.Round(Math.Abs(Math.Cos(angle) * max_speed));
                            y_max = (int)Math.Round(Math.Abs(Math.Sin(angle) * max_speed));
                            if (x_distance > x_max || y_distance > y_max)
                            {
                                if (x_max > y_max)
                                {
                                    y_distance = (max_speed * y_max) / x_max;
                                    x_distance = max_speed;
                                }
                                else
                                {
                                    x_distance = (max_speed * x_max) / y_max;
                                    y_distance = max_speed;
                                }
                            }
                        }
                    }
                    else if (x_distance != 0)
                        x_distance = Math.Max(min_speed, Math.Min(max_speed, x_distance));
                    else
                        y_distance = Math.Max(min_speed, Math.Min(max_speed, y_distance));

                    if (x_distance > 0)
                        x_distance = Math.Max(scroll_speed, x_distance);
                    if (y_distance > 0)
                        y_distance = Math.Max(scroll_speed, y_distance);
                    //if (x_distance > 0) x_distance = scroll_speed > 0 ? scroll_speed : x_distance;
                    //if (y_distance > 0) y_distance = scroll_speed > 0 ? scroll_speed : y_distance;
                }
            }
            // Align scroll back onto tiles
            else if (!Controlled_Scroll)
            {
                bool move_to_closest = Input.ControlScheme == ControlSchemes.Touch;
                bool ignore_align = Input.ControlScheme == ControlSchemes.Touch &&
                    Global.player.is_movement_allowed();

                if (!ignore_align)
                {
                    scroll_speed = -1;
                    // If not scrolling and the scroll position isn't centered on any tiles
                    if (Target_Display_X > min_scroll_x &&
                        Target_Display_X < max_scroll_x &&
                        Target_Display_X != (int)Target_Display_X)
                    {
                        if (move_to_closest)
                            Target_Display_X = (float)Math.Round(Target_Display_X);
                        else
                            Target_Display_X = (float)Math.Ceiling(Target_Display_X);
                        Scrolling = true;
                    }
                    if (Target_Display_Y > min_scroll_y &&
                        Target_Display_Y < max_scroll_y &&
                        Target_Display_Y != (int)Target_Display_Y)
                    {
                        if (move_to_closest)
                            Target_Display_Y = (float)Math.Round(Target_Display_Y);
                        else
                            Target_Display_Y = (float)Math.Ceiling(Target_Display_Y);
                        Scrolling = true;
                    }
                }
            }

            Actual_Display_X += (x_distance * x_move_sign);
            Actual_Display_Y += (y_distance * y_move_sign);
            Controlled_Scroll = !is_scroll_vel_over();
            if (!Scrolling)
                Event_Called_Scroll = false;
        }

        public void scroll_loc(Vector2 loc, bool forceControlled = false)
        {
            scroll_x(loc.X, forceControlled);
            scroll_y(loc.Y, forceControlled);
        }
        public void scroll_x(float x, bool forceControlled = false)
        {
            //float min_x = (Global.game_options.controller == 2 ? map_edge_offset_x1 : 0); //Debug
            //float max_x = (this.width - (Global.game_options.controller == 2 ? map_edge_offset_x2 : 0));
            float min_x = min_scroll_x;
            float max_x = max_scroll_x + Config.WINDOW_WIDTH / TILE_SIZE;
            x = Math.Max(Math.Min(x, max_x - ((Config.WINDOW_WIDTH / TILE_SIZE) + Target_Display_X)), -(Target_Display_X - min_x));

            if (Global.game_system.Instant_Move)
            {
                if (x == 0)
                    return;
                Target_Display_X += (int)x;
                Actual_Display_X = (int)Target_Display_X * UNIT_TILE_SIZE;
                scroll_speed = -1;
            }
            else
            {
                if (x == 0 && !forceControlled)
                    return;
                Scrolling = true;
                Target_Display_X += x;
                scroll_speed = 0;
                Controlled_Scroll = true;
            }
        }
        public void scroll_y(float y, bool forceControlled = false)
        {
            //float min_y = (Global.game_options.controller == 2 ? map_edge_offset_y1 : 0); //Debug
            //float max_y = (this.height - (Global.game_options.controller == 2 ? map_edge_offset_y2 : 0));
            float min_y = min_scroll_y;
            float max_y = max_scroll_y + Config.WINDOW_HEIGHT / TILE_SIZE;
            y = Math.Max(Math.Min(y, max_y - ((Config.WINDOW_HEIGHT / TILE_SIZE) + Target_Display_Y)), -(Target_Display_Y - min_y));

            if (Global.game_system.Instant_Move)
            {
                if (y == 0)
                    return;
                Target_Display_Y += (int)y;
                Actual_Display_Y = (int)Target_Display_Y * UNIT_TILE_SIZE;
                scroll_speed = -1;
            }
            else
            {
                if (y == 0 && !forceControlled)
                    return;
                Scrolling = true;
                Target_Display_Y += y;
                scroll_speed = 0;
                Controlled_Scroll = true;
            }
        }

        public void scroll_vel(Vector2 vel)
        {
            ScrollSpeed = vel;
            Controlled_Scroll = true;
        }

        private bool is_scroll_vel_over()
        {
            return ScrollSpeed.LengthSquared() < 1f / (UNIT_TILE_SIZE * UNIT_TILE_SIZE);
        }

        protected bool can_scroll(int dir)
        {
            switch (dir)
            {
                // Up
                case 2:
                    return Target_Display_Y > 0;
                // Left
                case 4:
                    return Target_Display_X > 0;
                // Right
                case 6:
                    return Target_Display_X < this.width - Config.WINDOW_WIDTH / TILE_SIZE;
                // Down
                case 8:
                    return Target_Display_Y < this.height - Config.WINDOW_HEIGHT / TILE_SIZE;
            }
            return false;
        }

        /// <summary>
        /// Returns the distance the map would have to move, in tiles, to scroll to the given position
        /// </summary>
        /// <param name="dest_loc">Target location</param>
        public Vector2 scroll_dist(Vector2 dest_loc)
        {
            float y = 0;
            // Scroll Up
            if ((Target_Display_Y > (dest_loc.Y - y_soft_center) || dest_loc.Y < y_soft_center) && can_scroll(2)) // Constant for scroll area //Yeti
            {
                y = Math.Max((dest_loc.Y - y_soft_center) - Target_Display_Y, -Target_Display_Y);
            }
            // Scroll Down
            else if ((Target_Display_Y + (Config.WINDOW_HEIGHT / TILE_SIZE) < (dest_loc.Y + y_soft_center + 1) ||
                dest_loc.Y >= this.height - y_soft_center) && can_scroll(8)) // Constant for scroll area //Yeti
            {
                //y = Math.Min(((dest_loc.Y + y_soft_center + 1) - Config.WINDOW_HEIGHT) - Display_Y, (this.height - Config.WINDOW_HEIGHT) - Display_Y);

                // Scroll offset
                y = Math.Min(dest_loc.Y + y_soft_center + 1, this.height);
                y -= Config.WINDOW_HEIGHT / TILE_SIZE;
                // Subtract current scroll
                y -= Target_Display_Y;
            }

            float x = 0;
            // Scroll Left
            if ((Target_Display_X > (dest_loc.X - x_soft_center) || dest_loc.X < x_soft_center) && can_scroll(4)) // Constant for scroll area //Yeti
            {
                x = Math.Max((dest_loc.X - x_soft_center) - Target_Display_X, -Target_Display_X);
            }
            // Scroll Right
            else if ((Target_Display_X + (Config.WINDOW_WIDTH / TILE_SIZE) < (dest_loc.X + x_soft_center + 1) ||
                dest_loc.X >= this.width - x_soft_center) && can_scroll(6)) // Constant for scroll area //Yeti
            {
                //x = Math.Min(((dest_loc.X + x_soft_center + 1) - Config.WINDOW_WIDTH) - Display_X, (this.width - Config.WINDOW_WIDTH) - Display_X);

                // Scroll offset
                x = Math.Min(dest_loc.X + x_soft_center + 1, this.width);
                x -= Config.WINDOW_WIDTH / TILE_SIZE;
                // Subtract current scroll
                x -= Target_Display_X;
            }

            return new Vector2(x, y);
        }

        internal Vector2 screen_loc_to_tile(Vector2 loc)
        {
            Vector2 target_loc = (loc +
                this.display_loc) / TILE_SIZE;
            target_loc = new Vector2((int)target_loc.X, (int)target_loc.Y);
            return target_loc;
        }
        internal bool screen_loc_in_view(Vector2 target_loc)
        {
            if (target_loc.X < (int)this.display_loc.X % TILE_SIZE)
                return false;
            if (target_loc.X >= Config.WINDOW_WIDTH -
                    ((int)this.display_loc.X % TILE_SIZE))
                return false;
            if (target_loc.Y < (int)this.display_loc.Y % TILE_SIZE)
                return false;
            if (target_loc.Y >= Config.WINDOW_HEIGHT -
                    ((int)this.display_loc.Y % TILE_SIZE))
                return false;

            return true;
        }
        internal Rectangle tile_rect(Vector2 loc)
        {
            return new Rectangle(
                (int)(loc.X * TILE_SIZE - this.display_loc.X),
                (int)(loc.Y * TILE_SIZE - this.display_loc.Y),
                TILE_SIZE, TILE_SIZE);
        }
        #endregion

        #region Events
        public void add_visit(Vector2 loc, string visit_event, string pillage_event = "", string visit_name = "")
        {
            if (!Visit_Locations.ContainsKey(loc))
                Visit_Locations.Add(loc, new Visit_Data(visit_event, pillage_event, visit_name));
        }
        public void add_chest(Vector2 loc, string visit_event)
        {
            if (!Chest_Locations.ContainsKey(loc))
                Chest_Locations.Add(loc, new Visit_Data(visit_event));
        }
        public void add_door(Vector2 loc, string visit_event)
        {
            if (!Door_Locations.ContainsKey(loc))
                Door_Locations.Add(loc, new Visit_Data(visit_event));
        }

        public void remove_visit(Vector2 loc)
        {
            Visit_Locations.Remove(loc);
            Chest_Locations.Remove(loc);
            Door_Locations.Remove(loc);
        }

        public void activate_visit(Vector2 loc, bool pillage)
        {
            if (Visit_Locations.ContainsKey(loc))
                Global.game_state.activate_event_by_name(pillage ? Visit_Locations[loc].PillageEvent : Visit_Locations[loc].VisitEvent);
        }
        public void activate_chest(Vector2 loc)
        {
            if (Chest_Locations.ContainsKey(loc))
                Global.game_state.activate_event_by_name(Chest_Locations[loc].VisitEvent);
        }
        public void activate_door(Vector2 loc)
        {
            if (Door_Locations.ContainsKey(loc))
                Global.game_state.activate_event_by_name(Door_Locations[loc].VisitEvent);
        }
        public void activate_escape(Vector2 loc, Game_Unit unit)
        {
            var escape_point = Global.game_map.escape_point_data(unit, loc);
            Global.game_state.activate_event_by_name(escape_point.EventName);
        }

        public bool already_visited(Vector2 loc)
        {
            if (!Visit_Locations.ContainsKey(loc))
                return false;
            return Global.game_state.event_handler.is_event_repeat(Visit_Locations[loc].VisitEvent);
        }

        public void pillage(int x, int y, int width, int height)
        {
            if (Terrain_Data.PILLAGE_TERRAIN_CHANGE.ContainsKey(Map_Data.GetTileset()))
                // This assumes the tileset is 32 wide, which isn't always true //Yeti
                for (int oy = 0; oy < height; oy++)
                {
                    for (int ox = 0; ox < width; ox++)
                    {
                        Vector2 loc = new Vector2(x + ox, y + oy);
                        if (!is_off_map(loc, false))
                        {
                            int id = Map_Data.GetValue((int)loc.X, (int)loc.Y);
                            //Vector2 tileset_loc = new Vector2(id % 8, (id / 32) + ((id / 8) % 4) * 32);
                            Vector2 tileset_loc = new Vector2(id % 32, id / 32);
                            // This needs a second called from location in some situations, where two red houses overlap //Yeti
                            // Normal pillage
                            if (Terrain_Data.PILLAGE_TERRAIN_CHANGE[Map_Data.GetTileset()].ContainsKey(tileset_loc))
                            {
                                Vector2 new_id = Terrain_Data.PILLAGE_TERRAIN_CHANGE[Map_Data.GetTileset()][tileset_loc];
                                change_tile(loc, ((int)new_id.X) + (((int)new_id.Y) % 32) * 32 + (((int)new_id.Y) / 32) * 8);
                            }
                        }
                    }
                }
        }

        internal void add_shop(Vector2 loc, Shop_Data shop, bool home_base_shop = false)
        {
            if (home_base_shop)
                Global.game_battalions.set_convoy_shop(shop);
            else
            {
                if (shop.secret)
                {
                    if (SecretShops.ContainsKey(loc))
                        throw new ArgumentException(
                            string.Format("A secret shop already exists at {0}, {1}.\nRemove the existing secret shop first.", (int)loc.X, (int)loc.Y));
                    SecretShops.Add(loc, shop);
                }
                else
                {
                    if (Shops.ContainsKey(loc))
                        throw new ArgumentException(
                            string.Format("A shop already exists at {0}, {1}.\nRemove the existing shop first.", (int)loc.X, (int)loc.Y));
                    Shops.Add(loc, shop);
                }
            }
        }

        internal Shop_Data get_shop()
        {
            return get_shop(Global.game_system.Shop_Loc, Global.game_system.SecretShop);
        }
        internal Shop_Data get_shop(Vector2 loc, bool secret = false)
        {
            if (secret)
                return SecretShops[loc];
            else
                return Shops[loc];
        }

        public void add_thief_escape(Vector2 loc, Vector2 escape_to_loc)
        {
            if (!Thief_Escape_Points.ContainsKey(loc))
                Thief_Escape_Points.Add(loc, escape_to_loc);
        }
        public void add_team_escape(int team, int group, Vector2 loc, Vector2 escape_to_loc)
        {
            EscapePoints.Add(new EscapePoint(loc, escape_to_loc, team, group));
            //Team_Escape_Points[team].add_point(group, loc, escape_to_loc); //Debug
        }
        public void add_player_event_escape(string eventName, Vector2 loc, Vector2 escape_to_loc)
        {
            EscapePoints.Add(new EscapePoint(loc, escape_to_loc, Constants.Team.PLAYER_TEAM, -1, eventName));
            //PlayerEventEscapePoints[loc].add_point(group, loc, escape_to_loc); //Debug
        }

        internal HashSet<Vector2> escape_point_locations(int team, int group)
        {
            var team_escape_points = EscapePoints.Where(x => x.Team == team);
            // Get escape locs for this team that either have no group set or the passed in group value
            var group_escape_points = team_escape_points
                .Where(x => x.Group == group || x.Group == -1);

            HashSet<Vector2> points = new HashSet<Vector2>(group_escape_points.Select(x => x.Loc));

            /* //Debug
            if (group != -1 && Team_Escape_Points[team].ContainsKey(group))
                points.UnionWith(Team_Escape_Points[team][group].Keys);
            if (Team_Escape_Points[team].ContainsKey(-1))
                points.UnionWith(Team_Escape_Points[team][-1].Keys);*/
            return points;
        }

        internal EscapePoint escape_point_data(Game_Unit unit, Vector2 loc)
        {
            var team_escape_points = EscapePoints.Where(x => x.Team == unit.team);
            // Get escape locs for this team that either have no group set or the passed in group value
            var group_escape_points = team_escape_points
                .Where(x => x.Group == unit.group || x.Group == -1);

            return group_escape_points.First(x => x.Loc == loc);

            /* //Debug
            Maybe<Vector2> escape_point;
            // Check for the unit's group
            escape_point = Team_Escape_Points[unit.team].get_escape_point(unit.group, loc);
            if (escape_point.IsSomething)
                return escape_point;
            // Check for any group
            escape_point = Team_Escape_Points[unit.team].get_escape_point(-1, loc);
            if (escape_point.IsSomething)
                return escape_point;*/

            throw new ArgumentException();
        }

        internal void escape_unit(int unitId)
        {
            Game_Unit unit = this.units[unitId];
            EscapedUnits[unit.id] = unit.actor.id;
            kill_unit(unit, false);
        }

        internal bool unit_escaped(int unitId)
        {
            return EscapedUnits.ContainsKey(unitId);
        }
        internal bool actor_escaped(int actorId)
        {
            return EscapedUnits.ContainsValue(actorId);
        }

        public void add_seize_point(int team, Vector2 loc, int group)
        {
            if (!Seize_Points[team].ContainsKey(group))
                Seize_Points[team].Add(group, new HashSet<Vector2>());
            Seize_Points[team][group].Add(loc);
        }

        public void seize_point(int team, Vector2 loc)
        {
            var keys = Seize_Points[team].Keys.ToList();
            foreach (int group in keys)
            {
                if (Seize_Points[team][group].Contains(loc))
                {
                    Seize_Points[team][group].Remove(loc);
                    Seized_Points.Add(loc);
                }
            }
        }

        /// <summary>
        /// Gets the seize points associated with a team.
        /// If a group number is provided, returns the seize points for that
        /// group, as well as points any group on the team can seize.
        /// If group -1 is used instead, returns all seize points for the
        /// entire team.
        /// </summary>
        public IEnumerable<Vector2> get_seize_points(int team, int group = -1)
        {
            // Return all points for the whole team
            if (group == -1)
            {
                // All points for a team
                foreach (var points in Seize_Points[team])
                    foreach (var point in points.Value)
                        yield return point;
            }
            // Return all points for a specific group
            else
            {
                // Shared points for the entire team
                if (Seize_Points[team].ContainsKey(-1))
                    foreach (var point in Seize_Points[team][-1])
                        yield return point;
                // Group specific points
                if (Seize_Points[team].ContainsKey(group))
                    foreach (var point in Seize_Points[team][group])
                        yield return point;
            }
        }

        public void clear_seize_points()
        { 
            for (int i = 0; i < Seize_Points.Length; i++)
                Seize_Points[i] = new Dictionary<int,HashSet<Vector2>>();
            // Why doesn't this just use Clear() ? //Yeti
            Seized_Points = new HashSet<Vector2>();
        }

        public void apply_chapter_support()
        {
            if (!Global.game_state.supports_blocked)
            {
                var units = this.units.ToList();
                for (int i = 0; i < units.Count; i++)
                    for (int j = i + 1; j < units.Count; j++)
                    {
                        units[i].Value.actor.chapter_support_gain(units[j].Value.actor.id);
                        units[j].Value.actor.chapter_support_gain(units[i].Value.actor.id);
                    }
            }
        }

        public void add_deployment(Vector2 loc)
        {
            if (!Deployment_Points.Contains(loc) && get_unit(loc) == null)
                Deployment_Points.Add(loc);
        }

        public void view_deployments()
        {
            clear_move_range();
            Move_Range.UnionWith(Deployment_Points);
            Move_Range_Visible = true;
            range_start_timer = 0;
        }

        public void add_forced_deployment(int actor_id)
        {
            if (!Forced_Deployment.Contains(actor_id))
                Forced_Deployment.Add(actor_id);
        }

        public void change_tile(Vector2 loc, int new_tile_id)
        {
            if (is_off_map(loc, false))
                return;
            if (get_scene_map() != null)
                get_scene_map().change_tile(loc, Map_Data.GetValue((int)loc.X, (int)loc.Y));
            Map_Data.set_value((int)loc.X, (int)loc.Y, new_tile_id);
            Pathfind.reset();
            Window_Minimap.clear();
            clear_updated_move_ranges();
        }

        public void import_map_area(string map_name, Vector2 base_loc, Rectangle area, bool roof)
        {
            if (get_scene_map() != null)
            {
                Data_Map loaded_map_data = get_scene_map().get_map_data(map_name);

                for (int y = 0; y < area.Height; y++)
                    for (int x = 0; x < area.Width; x++)
                    {
                        Vector2 loc = base_loc + new Vector2(x, y);
                        Vector2 source_loc = new Vector2(area.X, area.Y) + new Vector2(x, y);
                        if (is_off_map(loc, false) || source_loc.X < 0 || source_loc.Y < 0 ||
                                source_loc.X >= loaded_map_data.Columns || source_loc.Y > loaded_map_data.Rows)
                            continue;
                        get_scene_map().change_tile(loc, Map_Data.GetValue((int)loc.X, (int)loc.Y), roof);
                        Map_Data.set_value((int)loc.X, (int)loc.Y, loaded_map_data.GetValue((int)source_loc.X, (int)source_loc.Y));
                    }
                Pathfind.reset();
                Window_Minimap.clear();
                clear_updated_move_ranges();
            }
        }

        public void add_area_back(string back, int x1, int y1, int x2, int y2)
        {
            int x1a = Math.Min(x1, x2);
            int y1a = Math.Min(y1, y2);
            int x2a = Math.Max(x1, x2);
            int y2a = Math.Max(y1, y2);
            Area_Background.Add(new Tuple<Rectangle, string>(new Rectangle(x1, y1, (x2 + 1) - x1, (y2 + 1) - y1), back));
        }

        public string forced_battleback(Vector2 loc)
        {
            foreach (Tuple<Rectangle, string> pair in Area_Background)
            {
                if (loc.X >= pair.Item1.X && loc.X < (pair.Item1.X + pair.Item1.Width))
                    if (loc.Y >= pair.Item1.Y && loc.Y < (pair.Item1.Y + pair.Item1.Height))
                        return pair.Item2;
            }
            return "";
        }

        public void add_defend_area(int team, int group_id, int x1, int y1, int x2, int y2)
        {
            if (x1 == -1 && y1 == -1 && x2 == -1 && y2 == -1)
                Team_Defend_Areas[team].Remove(group_id);
            else
            {
                if (!Team_Defend_Areas[team].ContainsKey(group_id))
                    Team_Defend_Areas[team].Add(group_id, new List<Rectangle>());
                int x1a = Math.Min(x1, x2);
                int y1a = Math.Min(y1, y2);
                int x2a = Math.Max(x1, x2);
                int y2a = Math.Max(y1, y2);
                Team_Defend_Areas[team][group_id].Add(new Rectangle(x1, y1, (x2 + 1) - x1, (y2 + 1) - y1));
            }
            Global.game_state.refresh_ai_defend_area();
        }
        internal IEnumerable<Rectangle> get_defend_area(int team, int groupId = -1)
        {
            if (Team_Defend_Areas[team].ContainsKey(-1))
                foreach (Rectangle rect in Team_Defend_Areas[team][-1])
                    yield return rect;
            if (Team_Defend_Areas[team].ContainsKey(groupId))
                foreach (Rectangle rect in Team_Defend_Areas[team][groupId])
                    yield return rect;
        }

        public void add_unit_seek(int unit_id, int x, int y)
        {
            add_unit_seek(unit_id, new Vector2(x, y));
        }
        public void add_unit_seek(int unit_id, Vector2 loc)
        {
            Unit_Seek_Locs[unit_id] = loc;
        }
        public void add_team_seek(int team, int group, int x, int y)
        {
            add_team_seek(team, group, new Vector2(x, y));
        }
        public void add_team_seek(int team, int group, Vector2 loc)
        {
            if (!Team_Seek_Locs.ContainsKey(team))
                Team_Seek_Locs[team] = new Dictionary<int,Vector2>();
            Team_Seek_Locs[team][group] = loc;
        }

        public void add_tile_outline_set(byte type, Color tint)
        {
            TileOutlines.Add(new TileOutlineData(type, tint));
        }
        public void add_tile_outline_area(int index, Rectangle area)
        {
            TileOutlines[index].add_area(area);
        }
        public void remove_tile_outline_area(int index, Rectangle area)
        {
            TileOutlines[index].remove_area(area);
        }
        #endregion

        internal void next_turn()
        {
            // Reload siege engines
            foreach (var pair in this.siege_engines)
            {
                pair.Value.new_turn();
                if (pair.Value.is_ready)
                {
                    Game_Unit unit = get_unit(pair.Value.loc);
                    if (unit != null && unit.can_use_siege())
                        unit.refresh_sprite();
                }
                pair.Value.refresh_sprite();
            }
            // Decrement light rune time
            foreach (var rune in this.enumerate_light_runes().ToList())
            {
                rune.new_turn();
                if (rune.is_dead)
                    Global.game_map.remove_light_rune(rune.id);
            }
            refresh_move_ranges(true);
        }

        internal void team_add(int team_id, Game_Unit unit)
        {
            if (!Teams[team_id].Contains(unit.id))
            {
                Teams[team_id].Add(unit.id);
                if (team_id == Constants.Team.PLAYER_TEAM && Global.game_state.turn > 0)
                    Global.game_state.set_pc_starting_stats(unit);
            }
        }

        public void team_remove(int team_id, int id)
        {
            if (Teams[team_id].Contains(id))
                Teams[team_id].Remove(id);
        }

        internal void set_default_team_leader()
        {
            // If no team leader already set
            if (Team_Leaders[Constants.Team.PLAYER_TEAM] == -1)
            {
                if (Global.battalion.actors.Count > 0)
                    set_team_leader(Constants.Team.PLAYER_TEAM,
                        get_unit_id_from_actor(Global.battalion.actors[0]));
            }
        }

        #region Map Controls
        public bool is_unit_in_area(int x1, int y1, int x2, int y2, int unit_id)
        {
            int x1a = Math.Min(x1, x2);
            int y1a = Math.Min(y1, y2);
            int x2a = Math.Max(x1, x2);
            int y2a = Math.Max(y1, y2);
            Game_Unit unit = this.units[unit_id];
            return ((unit.loc.X >= x1a) && (unit.loc.Y >= y1a) && (unit.loc.X <= x2a) && (unit.loc.Y <= y2a));
        }

        public bool is_team_in_area(int x1, int y1, int x2, int y2, int team_id)
        {
            foreach (int id in Teams[team_id])
                if (is_unit_in_area(x1, y1, x2, y2, id))
                    return true;
            return false;
        }

        public void set_group_name(int team, int group, string name)
        {
            Group_Names[team][group] = name;
        }

        public string group_name(int team, int group)
        {
            if (Group_Names[team].ContainsKey(group))
                return Group_Names[team][group];
            return "";
        }

        public void set_team_leader(int team, int unit_id)
        {
            Team_Leaders[team] = unit_id;
        }
        #endregion

        #region Sprite Handling
        public void update_sprites()
        {
            
        }
        #endregion

        internal void add_torch_staff(Vector2 loc)
        {
            Torch_Staves.Add(new Torch_Staff_Point(loc));
        }

        internal void update_new_turn_torch_staves()
        {
            // 1 rather than Constants.Team.PLAYER_TEAM so that is always activates at the start of each cycle
            if (team_turn == 1)
            {
                int i = 0;
                while (i < Torch_Staves.Count)
                {
                    if (Torch_Staves[i].decrease_vision())
                        Torch_Staves.RemoveAt(i);
                    else
                        i++;
                }
            }
        }

        internal void add_vision_point(Fow_View_Object visionPoint)
        {
            VisionPoints.Add(visionPoint);
        }
    }
}
