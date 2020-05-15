using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ContentManagers;
using FEXNA.IO;
using FEXNA.Metrics;
using FEXNA.Services.Audio;
using FEXNA_Library;
using FEXNA_Library.Battler;
using FEXNA_Library.Palette;
using FEXNAVersionExtension;

namespace FEXNA
{
    enum Metrics_Settings { Not_Set, On, Off }
    public static partial class Global
    {
        private static System.Reflection.Assembly _GAME_ASSEMBLY;
        public static System.Reflection.Assembly GAME_ASSEMBLY
        {
            get { return _GAME_ASSEMBLY; }
            set
            {
                if (_GAME_ASSEMBLY == null)
                    _GAME_ASSEMBLY = value;
            }
        }

        static GameServiceContainer Services;
        private static GameServiceContainer services { get { return Services; } }

        public static void init(Game game, ContentManager content, GameServiceContainer services)
        {
            Content = new ThreadSafeContentManager(services, "Content");
            Battler_Content = new ThreadSafeContentManager(services, "Content");
            Chapter_Text_Content = new ThreadSafeContentManager(services, "Content");

            if (Services == null)
            {
                Services = new GameServiceContainer();

                Services.AddService(typeof(IAudioService), new Audio_Engine.Audio_Service());

                Services.AddService(
                    typeof(FEXNA.Services.Rumble.BaseRumbleService),
                    new FEXNA.Services.Rumble.RumbleService(game));
                Services.AddService(
                    typeof(FEXNA.Services.Input.BaseInputService),
                    new FEXNA.Services.Input.InputService(game));

                Item_Data.equipment_data = new Equipment_Service();
                Data_Class.class_data = new Class_Service();
                Data_Weapon.weapon_type_data = new WeaponTypeService();
                FE_Battler_Image.animation_battler_data = new BattlerAnimsService();

                Global.game_state = new Game_State();
                Global.game_options = new Game_Options();
                Global.game_system = new Game_System();
                Global.game_temp = new Game_Temp();

#if DEBUG && WINDOWS
                DebugMonitor = new Debug_Monitor.DebugMonitorState();
#endif
            }

            load_data_content();

            Scene_Map.set_content(services);
        }

        public static void update_input(Game game, GameTime gameTime, bool gameActive,
            Microsoft.Xna.Framework.Input.KeyboardState key_state,
            Microsoft.Xna.Framework.Input.GamePadState controller_state)
        {
            FEXNA.Input.update(gameActive, gameTime, key_state, controller_state);
            Input.UpdateKeyboardStart(key_state);
            Input.UpdateGamepadState(controller_state);
            FEXNA.Input.update_input_state(Input);
        }

        // Content
        public static ContentManager Content, Battler_Content, Chapter_Text_Content;

        static bool StartInitialLoad = false;
        public static bool start_initial_load { get { return StartInitialLoad; } }

        static bool RunningContentLoad = false;
        public static bool running_content_load { get { return RunningContentLoad; } }

        public static IEnumerator<string> start_initial_load_content()
        {


#if WINDOWTEST
            //Yeti
            return graphics_content_loader().GetEnumerator();
#endif


#if MONOGAME
            return graphics_content_loader().GetEnumerator();
#else
#if DEBUG
            Debug.Assert(!RunningContentLoad,
                "Tried to start initial content load but it's already running");
#endif
            StartInitialLoad = true;

            return null;
#endif
        }
        public static void run_initial_load_content()
        {
            StartInitialLoad = false;
            RunningContentLoad = true;

            load_graphics_content();
        }
        public static void end_load_content()
        {
            RunningContentLoad = false;
        }

        private static void load_data_content()
        {
            Loaded_Files = Content.Load<List<string>>(@"manifest");
            repair_loaded_filenames();

            Data_Actors = Content.Load<Dictionary<int, FEXNA_Library.Data_Actor>>(@"Data/Actor_Data");
#if DEBUG
            System.Diagnostics.Debug.Assert(Data_Actors.Keys.Max() <= Constants.Actor.MAX_ACTOR_COUNT,
                string.Format("An actor has a higher id than the maximum value \"{0}\"",
                Constants.Actor.MAX_ACTOR_COUNT));
#endif
            if (content_exists(@"Data\Generic_Actor_Data"))
                DataGenericActors = Content.Load<Dictionary<string, FEXNA_Library.Data_Generic_Actor>>(@"Data\Generic_Actor_Data");
            Data_Animations = Content.Load<Dictionary<int, FEXNA_Library.Battle_Animation_Data>>(@"Data/Animation_Data");
            Data_Animation_Groups = Content.Load<Dictionary<string, int>>(@"Data/Animation_Group_Data");
            Data_Battler_Animations = Content.Load<Dictionary<string, Battle_Animation_Association_Set>>(@"Data/Animation_Battler_Data");
            var chapters = Content.Load<List<FEXNA_Library.Data_Chapter>>(@"Data/Chapter_Data");
            Chapter_List = chapters.Select(x => x.Id).ToList();
            Data_Chapters = chapters.ToDictionary(x => x.Id, x => x);
            Data_Classes = Content.Load<Dictionary<int, FEXNA_Library.Data_Class>>(@"Data/Class_Data");
            Data_Items = Content.Load<Dictionary<int, FEXNA_Library.Data_Item>>(@"Data/Item_Data");
            Data_Skills = Content.Load<Dictionary<int, FEXNA_Library.Data_Skill>>(@"Data/Skill_Data");
            Data_Statuses = Content.Load<Dictionary<int, FEXNA_Library.Data_Status>>(@"Data/Status_Data");
            Data_Supports = load_supports(Content.Load<List<FEXNA_Library.Data_Support>>(@"Data/Support_Data"));
            Data_Terrains = Content.Load<Dictionary<int, FEXNA_Library.Data_Terrain>>(@"Data/Terrain_Data");
            Data_Tilesets = Content.Load<Dictionary<int, FEXNA_Library.Data_Tileset>>(@"Data/Tileset_Data");
            Data_Weapons = Content.Load<Dictionary<int, FEXNA_Library.Data_Weapon>>(@"Data/Weapon_Data");
            WeaponTypes = Content.Load<List<WeaponType>>(@"Weapon_Types");
            Frame_Data = Content.Load<Dictionary<string, FEXNA_Library.Frame_Data>>(@"Data/Frame_Data");
            if (content_exists("Face_Data"))
                FaceData = Content.Load<Dictionary<string, FEXNA_Library.Face_Data>>(@"Face_Data");
            BattlerPaletteData = Content.Load<Dictionary<string, SpritePalette>>(@"Battler_Palette_Data");
            BattlerRecolorData = Content.Load<Dictionary<string, RecolorData>>(@"Data/BattlerRecolorData");
            Face_Palette_Data = Content.Load<Dictionary<string, Color[]>>(@"Face_Palette_Data");
            Map_Sprite_Colors = Global.Content.Load<MapSpriteRecolorData>(@"MapSpriteRecolors");

            GlobalText = Global.Content.Load<Dictionary<string, string>>(@"Data/Text/Global");
            Battle_Text = Global.Content.Load<Dictionary<string, string>>(@"Data/Text/Battle Quotes");
            Death_Quotes = Global.Content.Load<Dictionary<string, string>>(@"Data/Text/Death Quotes");
            Supports = Global.Content.Load<Dictionary<string, string>>(@"Data/Text/Supports");
            System_Text = Global.Content.Load<Dictionary<string, string>>(@"Data/Text/System");

            Actor_Descriptions = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> description in Generic_Unit_Descriptions.DESCRIPTIONS)
                Actor_Descriptions[description.Key] = description.Value;
            foreach (FEXNA_Library.Data_Actor actor in Data_Actors.Values)
                Actor_Descriptions[actor.Name] = actor.Description;
            
            Palette_Pool = new Palette_Handler();
#if DEBUG
            Test_Battler_1 = new Test_Battle_Character_Data();
            Test_Battler_2 = new Test_Battle_Character_Data();
#endif
        }

        private static Dictionary<string, Data_Support> load_supports(List<Data_Support> list)
        {
            Dictionary<string, Data_Support> supports = new Dictionary<string, Data_Support>();
            foreach (Data_Support support in list)
            {
                supports[support.Key] = support;
            }
            return supports;
        }

        private static void load_graphics_content()
        {
            // Load all the map sprites into memory for each team
            // This is a big contributor to ram usage bloat (takes ~90MB, which is a pretty big case for using a pixel shader to palette them) //Yeti

            //Scene_Map.set_recolors(); //Debug

#if __ANDROID__ || DEBUG
            //return;
#endif
            string[] name_ary;
            for (int i = 0; i < Global.loaded_files.Count; i++)
            {
                string filename = Global.loaded_files[i];
                name_ary = filename.Split('/');
                bool proceed_with_load = false;

                if (name_ary.Length > 1 && name_ary[0] == "Graphics")
                {
                    // Skip animations, they're loaded in parallel during battle transition
                    if (name_ary[1] == "Animations")
                        continue; //Global.Battler_Content.Load<Texture2D>(filename);
                    // Skip
                    else if (name_ary[1] == "Characters") // || name_ary[1] == "Faces") //Debug
                        continue;
                    else
                        proceed_with_load = true;
                }

                if (proceed_with_load)
                {
                    Global.Content.Load<Texture2D>(filename);
                    // Insert waits to keep the main thread framerate consistent
                    System.Threading.Thread.Sleep(3);
                }
            }
        }

        private static IEnumerable<string> graphics_content_loader()
        {
            // Load all the map sprites into memory for each team
            // This is a big contributor to ram usage bloat (takes ~90MB, which is a pretty big case for using a pixel shader to palette them) //Yeti

            //Scene_Map.set_recolors(); //Debug

#if __ANDROID__ || DEBUG
            //return;
#endif
            string[] name_ary;
            for (int i = 0; i < Global.loaded_files.Count; i++)
            {
                string filename = Global.loaded_files[i];
                name_ary = filename.Split('/');
                bool proceed_with_load = false;

                if (name_ary.Length > 1 && name_ary[0] == "Graphics")
                {
                    // Skip animations, they're loaded in parallel during battle transition
                    if (name_ary[1] == "Animations")
                        continue; //Global.Battler_Content.Load<Texture2D>(filename);
                    // Skip
                    else if (name_ary[1] == "Characters") // || name_ary[1] == "Faces") //Debug
                        continue;
                    else
                        proceed_with_load = true;
                }

                if (proceed_with_load)
                {
                    Global.Content.Load<Texture2D>(filename);
                    yield return filename;
                }
            }
        }

        // Services
        internal static IAudioService Audio
        {
            get
            {
#if DEBUG
                Debug.Assert(Services != null);
#endif
                return ((IAudioService)Global.services.GetService(typeof(IAudioService)));
            }
        }
        public static FEXNA.Services.Rumble.BaseRumbleService Rumble
        {
            get
            {
#if DEBUG
                Debug.Assert(Services != null);
#endif
                return ((FEXNA.Services.Rumble.BaseRumbleService)
                    Global.services.GetService(
                        typeof(FEXNA.Services.Rumble.BaseRumbleService)));
            }
        }
        internal static FEXNA.Services.Input.IInputService Input
        {
            get
            {
#if DEBUG
                Debug.Assert(Services != null);
#endif
                return ((FEXNA.Services.Input.BaseInputService)
                    Global.services.GetService(
                        typeof(FEXNA.Services.Input.BaseInputService)));
            }
        }


#if WINDOWS && DEBUG
        public static string[] Text_Input = new string[2];
        public static string text_input
        {
            get
            {
                return Text_Input[0];
            }
        }
        public static void set_text(char character)
        {
            switch (character)
            {
                // Escape
                case (char)27:
                    break;
                default:
                    Text_Input[1] += character;
                    break;
            }
        }
        public static void update_text_input()
        {
            string str = Text_Input[1];
            Text_Input[1] = "";
            Text_Input[0] = str;
        }
        public static string append_text_input(string str)
        {
            str += text_input;
            if (text_input.Length > 0)
            {
                // If any character followed by a backspace is found, it removes the character
                str = Regex.Replace(str, @"([\w\s.,\$!\(\)-/\\\?;:#&""'\+])[\b]", "");
                // Removes extraneous backspaces, newlines, ?underscores?
                str = Regex.Replace(str, @"([\b\r\n_])", "");
                // Matches the text to only specific characters by removing everything else
                str = Regex.Replace(str, @"[^\w\s.,\$!\(\)-/\\\?;:#&""'\+]", "");
            }
            return str;

            //string input = text_input;
            //return Regex.Replace(str + input, @"([\w\s.,\$!\(\)-/\\\?;:#&""'\+])[\b]", "");
        }
#endif

        public static Version LOADED_VERSION = new Version();
        static Version Running_Version;
        public static Version RUNNING_VERSION
        {
            get { return Running_Version; }
            set
            {
                if (Running_Version == null)
                    Running_Version = value;
            }
        }

        #region Saving
        public static bool storage_selection_requested;

        // New Game
        static bool Start_New_Game = false;
        public static bool start_new_game
        {
            get { return Start_New_Game; }
            set { Start_New_Game = value; }
        }

        static int Start_Game_File_Id = -1;
        public static int start_game_file_id
        {
            get { return Start_Game_File_Id; }
            set { Start_Game_File_Id = value; }
        }

        static bool Return_To_Title = false;
        public static bool return_to_title
        {
            get { return Return_To_Title; }
            set { Return_To_Title = value; }
        }

        // Progress meta data completed on any file, such as what chapters
        // the player has seen, what supports have been acquired, etc
        static Save_Progress Progress = new Save_Progress();
        internal static Save_Progress progress { get { return Progress; } }

        static bool Load_Save_Info = false;
        public static bool load_save_info
        {
            get { return Load_Save_Info; }
            set { Load_Save_Info = value; }
        }
        static Suspend_Info Suspend_File_Info;
        internal static Suspend_Info suspend_file_info
        {
            get { return Suspend_File_Info; }
            set { Suspend_File_Info = value; }
        }


        static Dictionary<int, Save_Info> Save_Files_Info;
        internal static Dictionary<int, Save_Info> save_files_info
        {
            get { return Save_Files_Info; }
            set { Save_Files_Info = value; }
        }
        static Dictionary<int, Suspend_Info> Suspend_Files_Info;
        internal static Dictionary<int, Suspend_Info> suspend_files_info
        {
            get { return Suspend_Files_Info; }
            set { Suspend_Files_Info = value; }
        }
        static Dictionary<int, Suspend_Info> Checkpoint_Files_Info;
        internal static Dictionary<int, Suspend_Info> checkpoint_files_info
        {
            get { return Checkpoint_Files_Info; }
            set { Checkpoint_Files_Info = value; }
        }

        internal static Save_Info current_save_info
        {
            get
            {
                return (Save_Files_Info == null ||
                        !Save_Files_Info.ContainsKey(Current_Save_Id)) ?
                    null : Save_Files_Info[Current_Save_Id];
            }
        }
        public static void map_save_created()
        {
            current_save_info.map_save_exists = true;
        }
        static int Latest_Save_Id;
        public static int latest_save_id
        {
            get { return Latest_Save_Id; }
            set { Latest_Save_Id = value; }
        }
        static int Current_Save_Id;
        public static int current_save_id
        {
            set
            {
                Current_Save_Id = value;
                if (Global.current_save_info != null)
                {
                    Global.current_save_info.SetStartTime();
                }
            }
            get { return Current_Save_Id; }
        }
        #endregion

        #region Config
        static bool Save_Config = false;
        public static bool save_config
        {
            get { return Save_Config; }
            set { Save_Config = value; }
        }

        static bool Load_Config = false;
        public static bool load_config
        {
            get { return Load_Config; }
            set { Load_Config = value; }
        }

        // Shader
        public static bool shader_exists { get { return Content.Load<Effect>(@"Effect") != null; } } //Debug
        public static Effect effect_shader(int width = Config.WINDOW_WIDTH, int height = Config.WINDOW_HEIGHT)
        {
            Effect shader = Content.Load<Effect>(@"Effect");
            Matrix projection = Matrix.CreateOrthographicOffCenter(
                //0, width * render_target_zoom, height * render_target_zoom, 0, -10000, 10000); //Debug
                0, width * 1, height * 1, 0, -10000, 10000);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            shader.Parameters["World"].SetValue(Matrix.Identity);
            shader.Parameters["View"].SetValue(Matrix.Identity);
            shader.Parameters["Projection"].SetValue(halfPixelOffset * projection);

            shader.Parameters["MatrixTransform"].SetValue(Matrix.Identity);
            return shader;
        }

        // Metrics
        public static bool metrics_allowed = false;

        private static Metrics_Data Metrics_Data;
        public static string metrics_data { get { return Metrics_Data.query_string(); } }
        public static string metrics_gameplay_data { get { return Metrics_Data.gameplay_string(); } }

        static bool Sending_Metrics = false;
        public static bool sending_metrics { get { return Sending_Metrics; } }
        public static void send_metrics()
        {
            if (Global.gameSettings.General.Metrics == Metrics_Settings.On)
            {
                Gameplay_Metrics metrics = new Gameplay_Metrics(Global.game_state.metrics);
                metrics.set_pc_ending_stats();
                Metrics_Data = new Metrics_Data(metrics);
                Sending_Metrics = true;
                send_metrics_to_server(null, new EventArgs());
            }
        }
        public static void metrics_sent(bool result)
        {
            Sending_Metrics = false;
        }

        public static event EventHandler send_metrics_to_server;

        // Check for update
        public static bool update_check_allowed = false;

        internal static string UpdateUri { get; private set; }
        public static void set_update_uri(string uri)
        {
            if (string.IsNullOrEmpty(UpdateUri))
                UpdateUri = uri;
        }

        static bool Checking_For_Update = false;
        static bool IsUpdateFound;
        public static bool is_update_found { get { return IsUpdateFound; } }
        internal static Version UpdateVersion { get; private set; }
        internal static DateTime UpdateDate { get; private set; }
        internal static string UpdateDescription { get; private set; }

        public static void check_for_updates()
        {
            if (!IsUpdateFound)
            {
                Checking_For_Update = true;
                check_for_updates_from_server(null, new EventArgs());
            }
        }
        public static event EventHandler check_for_updates_from_server;
        
        public static void update_found(Tuple<Version, DateTime, string> result)
        {
            if (result != null && RUNNING_VERSION.older_than(result.Item1))
            {
                IsUpdateFound = true;
                UpdateVersion = result.Item1;
                UpdateDate = result.Item2;
                UpdateDescription = result.Item3;
            }
            Checking_For_Update = false;
        }

        public static bool VisitGameSiteCall { get; private set; }
        internal static void visit_game_site()
        {
            VisitGameSiteCall = true;
        }
        public static void game_site_visited()
        {
            VisitGameSiteCall = false;
        }

#if DEBUG && WINDOWS
        static Debug_Monitor.DebugMonitorState DebugMonitor;
        public static Debug_Monitor.DebugMonitorState debug_monitor
        {
            get { return DebugMonitor; }
            set
            {
                if (DebugMonitor == null)
                    DebugMonitor = value;
            }
        }
#endif
        #endregion

        // Exiting
        static bool Quitting = false;
        public static bool quitting { get { return Quitting; } }
        public static void quit()
        {
            Quitting = true;
        }

        #region Scene
        static Scene_Base Scene;

        internal static Scene_Base scene
        {
            get { return Scene; }
            set
            {
                if (Scene != null)
                    Scene.dispose();
                Scene = value;
                New_Scene = "";
            }
        }

        public static void change_to_new_scene(string scene)
        {
            switch (scene)
            {
                case "Start_Game":
                case "Scene_Worldmap":
                    string chapter_id = "";
                    if (scene != "Start_Game")
                    {
                        // Returning to the world map after beating a chapter
                        if (Global.map_exists)
                            // Game_System.chapter_id ? //@Yeti
                            chapter_id = game_state.chapter_id;
                    }

                    Scene = new Scene_Worldmap(chapter_id);

                    Game_System.Chapter_Save_Progression_Keys = new string[0];
                    break;
                case "Scene_Save":
                    Scene = new Scene_Save();
                    break;
                case "Scene_Map":
                    Battler_Content.Unload();
                    Scene = new Scene_Map();
                    break;
#if !MONOGAME && DEBUG
                case "Scene_Map_Unit_Editor":
                    Scene = new Scene_Map_Unit_Editor();
                    break;
#endif
                case "Scene_Battle":
                    Scene = new Scene_Battle();
                    break;
                case "Scene_Staff":
                    Scene = new Scene_Staff();
                    break;
                case "Scene_Dance":
                    Scene = new Scene_Dance();
                    break;
                case "Scene_Arena":
                    Scene = new Scene_Arena();
                    break;
                case "Scene_Promotion":
                    Scene = new Scene_Promotion();
                    break;
#if DEBUG
                case "Scene_Test_Battle":
                    Scene = new Scene_Test_Battle();
                        ((Scene_Action)Scene).initialize_action(Game_System.Arena_Distance);
                    break;
#endif
                case "Scene_Title_Load":
                    Scene = new Scene_Title(true);
                    break;
                case "Scene_Title":
                    Scene = new Scene_Title(false);
                    break;
                case "Scene_Class_Reel":
                    Scene = new Scene_Class_Reel();
                    break;
            }
        }
        public static void initialize_action_scene(bool arena = false, bool promotion = false)
        {
            int distance;
            if (arena)
                distance = Game_System.Arena_Distance;
            else if (promotion)
                distance = 1;
            else
                distance = battle_scene_distance;
            ((Scene_Action)Scene).initialize_action(distance);
            ((Scene_Action)Scene).reset_map();
            ((Scene_Action)Scene).set_map_texture();
            ((Scene_Action)Scene).re_add_map_sprites();
            ((Scene_Action)Scene).set_map_alpha_texture(Game_Map.Tile_Alpha);
        }
        public static void update_scene(Microsoft.Xna.Framework.Input.KeyboardState key_state)
        {
            if (Scene.scene_type == "Scene_Title")
                ((Scene_Title)Scene).update(key_state);
            else
                Scene.update();
        }

        // Scene Change
        static string New_Scene = "";
        // New scene data
        internal static int battle_scene_distance = 0;
        public static void scene_change(string new_scene)
        {
            New_Scene = new_scene;
        }

#if DEBUG
        public static bool UnitEditorActive { get; private set; }
#endif

        public static string new_scene { get { return New_Scene; } }

        public static void start_chapter()
        {
            Global.change_to_new_scene("Scene_Map");

            Global.reset_game_state();
            Global.game_map = new Game_Map();
            Global.player = new Player();

            // Trying to start this after everything else, instead of in the middle
            //move_range_update_thread(); //Debug
            Global.clear_events();

            //Global.Audio.clear_map_theme(); //@Debug

            Global.set_map_data();
            Global.game_state.reset_support_data();
            Global.change_game_state_turn();
        }

#if !MONOGAME && DEBUG
        public static void start_unit_editor(string chapterId, string mapDataKey, string unitDataKey)
        {
            UnitEditorActive = true;

            Global.game_temp = new Game_Temp();
            Global.game_battalions = new Game_Battalions();
            Global.game_actors = new Game_Actors();
            Global.change_to_new_scene("Scene_Map_Unit_Editor");

            Global.game_system.reset();
            Global.game_system.new_chapter(new List<string>(), Global.game_system.chapter_id, new Dictionary<string,string>());

            Global.reset_game_state();
            var old_map = Global.game_map;
            Global.game_map = new Game_Map();
            Global.player = new Player();

            // Trying to start this after everything else, instead of in the middle
            //move_range_update_thread(); //Debug
            Global.clear_events();

            //Global.Audio.clear_map_theme(); //@Debug

            Global.set_unit_editor_data(chapterId, mapDataKey, unitDataKey);
            Global.change_game_state_turn();

            if (old_map != null)
            {
                Global.game_map.set_scroll_loc(old_map.display_loc, true, true);
                Global.player.center_cursor(true);
            }
        }

        public static void start_unit_editor_playtest()
        {
            var old_scene = Global.scene;
            if (!(old_scene is Scene_Map_Unit_Editor))
                return;

            Difficulty_Modes difficulty = Global.game_system.Difficulty_Mode;
            var chapter = Global.data_chapters[Global.game_system.New_Chapter_Id];

            var previous_chapters = Global.save_file.valid_previous_chapters(chapter.Id);
            var previous_chapter_ids = previous_chapters
                .ToDictionary(p => p.Key, p => p.Value.Count == 0 ? "" : p.Value.Last());

            if (previous_chapters.Count == 0 || previous_chapters.Any(x => x.Value.Count == 0))
            {
                if (previous_chapters.Count > 0)
                    Print.message(string.Format(
                        "Could not load any save data\nfor \"{0}\", tried to use save file {1}",
                        Global.game_system.New_Chapter_Id, Global.current_save_id));
                previous_chapter_ids = previous_chapter_ids
                    .ToDictionary(p => p.Key, p => "");
                Global.game_system.reset();
                Global.game_system.reset_event_variables();
                int battalion_index = chapter.Battalion;
                Global.game_battalions.add_battalion(battalion_index);
                Global.game_battalions.current_battalion = battalion_index;
            }
            else
            {
                Global.save_file.load_data(chapter.Id, previous_chapter_ids, "");

                Global.game_actors.heal_battalion();
                Global.battalion.refresh_deployed();
            }
            Global.game_system.Difficulty_Mode = difficulty;

            if (Global.game_system.Style != Mode_Styles.Classic)
                Global.save_file.Difficulty = Global.game_system.Difficulty_Mode;
            Global.game_system.new_chapter(chapter.Prior_Chapters, chapter.Id,
                previous_chapter_ids);

            // these weren't copied in like the rest of the Scene_Worldmap.start_chapter() stuff? //Debug
            Global.game_temp = new Game_Temp();
            Global.save_file = null;


            Global.change_to_new_scene("Scene_Map");

            Global.reset_game_state();
            var old_map = Global.game_map;
            Global.game_map = new Game_Map();
            Global.player = new Player();

            // Trying to start this after everything else, instead of in the middle
            //move_range_update_thread(); //Debug
            Global.clear_events();

            //Global.Audio.clear_map_theme(); //@Debug

            (Scene as Scene_Map).set_map(old_scene as Scene_Map_Unit_Editor);
            Global.game_state.reset_support_data();
            Global.change_game_state_turn();

            if (old_map != null)
            {
                Global.game_map.set_scroll_loc(old_map.display_loc, true, true);
                Global.player.center_cursor(true);
            }
        }
#endif

        // Should happen automatically from called methods //Debug
        public static void new_game_actors()
        {
            Game_Actors = new Game_Actors();
        }

        // Scene Map
        public static void set_map_data()
        {
            ((Scene_Map)Scene).set_map();
        }

        public static void init_map()
        {
            ((Scene_Map)Scene).reset_map();
            ((Scene_Map)Scene).set_map_texture();
            ((Scene_Map)Scene).re_add_map_sprites();
            ((Scene_Map)Scene).set_map_alpha_texture(Global.game_map.Tile_Alpha);
        }

        public static void suspend_fade_in()
        {
            ((Scene_Map)Scene).suspend_fade_in();
        }

        public static void reset_game_state()
        {
            Global.game_state.reset();
        }
        public static void change_game_state_turn()
        {
            Global.game_state.change_turn();
        }

        public static void map_update_move_range_loop()
        {
            Global.game_map.update_move_range_loop();
        }

        public static bool map_exists { get { return Game_Map != null; } }

        // Unit Editor
        public static void set_unit_editor_data(string chapterId, string mapDataKey, string unitDataKey)
        {
            ((Scene_Map)Scene).set_map(chapterId, mapDataKey, unitDataKey, "");
        }
        #endregion

        #region Temporary Textures
        public static List<Texture2D> Battle_Textures = new List<Texture2D>();

        public static void dispose_battle_textures()
        {
            foreach (Texture2D texture in Battle_Textures)
                texture.Dispose();
            Battle_Textures.Clear();
        }

        //public static List<Texture2D> Face_Textures = new List<Texture2D>(); //Debug

        public static void dispose_face_textures()
        {
            //foreach (Texture2D texture in Face_Textures)
            //    texture.Dispose();
            //Face_Textures.Clear();
        }

        //public static List<Texture2D> Miniface_Textures = new List<Texture2D>(); //Debug

        public static void dispose_miniface_textures()
        {
            //foreach (Texture2D texture in Miniface_Textures)
            //    texture.Dispose();
            //Miniface_Textures.Clear();
        }

        public static Dictionary<string, Texture2D> SuspendScreenshots = new Dictionary<string, Texture2D>();

        public static void dispose_suspend_screenshots()
        {
            foreach (Texture2D texture in SuspendScreenshots.Values)
                texture.Dispose();
            SuspendScreenshots.Clear();
        }
        #endregion

        #region Text
        static Dictionary<string, string> Chapter_Text;
        internal static Dictionary<string, string> chapter_text
        {
            get { return Chapter_Text; }
            set { Chapter_Text = value; }
        }

        // Global Text
        static Dictionary<string, string> GlobalText;
        internal static Dictionary<string, string> global_text { get { return GlobalText; } }

        // Battle Text
        static Dictionary<string, string> Battle_Text;
        internal static Dictionary<string, string> battle_text { get { return Battle_Text; } }

        // Death Quotes
        static Dictionary<string, string> Death_Quotes;
        internal static Dictionary<string, string> death_quotes { get { return Death_Quotes; } }

        // Supports
        static Dictionary<string, string> Supports;
        internal static Dictionary<string, string> supports { get { return Supports; } }

        // System Text
        static Dictionary<string, string> System_Text;
        internal static Dictionary<string, string> system_text { get { return System_Text; } }

        // Actor Descriptions
        static Dictionary<string, string> Actor_Descriptions;
        internal static Dictionary<string, string> actor_descriptions { get { return Actor_Descriptions; } }
        #endregion

#if DEBUG
        public static void reseed_rng()
        {
            game_system.reseed_rng();
        }

        public static void open_debug_menu()
        {
            if (Global.scene is Scene_Map)
            {
                (Global.scene as Scene_Map).open_debug_menu();
            }
        }

        // Test Battlers
        static Test_Battle_Character_Data Test_Battler_1;
        internal static Test_Battle_Character_Data test_battler_1
        {
            get { return Test_Battler_1; }
            set { Test_Battler_1 = value; }
        }
        
        static Test_Battle_Character_Data Test_Battler_2;
        internal static Test_Battle_Character_Data test_battler_2
        {
            get { return Test_Battler_2; }
            set { Test_Battler_2 = value; }
        }
#endif

        #region Game Objects
        // Save File
        static Save_File @Save_File;

        internal static Save_File save_file
        {
            get { return @Save_File; }
            set { @Save_File = value; }
        }

        // Game Actors
        static Game_Actors @Game_Actors;

        internal static Game_Actors game_actors
        {
            get { return @Game_Actors; }
            set { @Game_Actors = value; }
        }

        // Game Battalions
        static Game_Battalions @Game_Battalions;

        internal static Game_Battalions game_battalions
        {
            get { return @Game_Battalions; }
            set { @Game_Battalions = value; }
        }
        internal static Battalion battalion
        {
            get
            {
                if (@Game_Battalions == null)
                    return null;
                return @Game_Battalions.battalion;
            }
        }

        // Game Config
        static Options.Settings GameSettings;

        internal static Options.Settings gameSettings
        {
            get { return GameSettings; }
            set { GameSettings = value; }
        }

        // Game Map
        static Game_Map @Game_Map;

        internal static Game_Map game_map
        {
            get { return @Game_Map; }
            set { @Game_Map = value; }
        }

        // Game Options
        static Game_Options @Game_Options;

        internal static Game_Options game_options
        {
            get { return @Game_Options; }
            set { @Game_Options = value; }
        }

        // Game State
        static Game_State @Game_State;

        internal static Game_State game_state
        {
            get { return @Game_State; }
            set { @Game_State = value; }
        }

        // Game System
        static Game_System @Game_System;

        internal static Game_System game_system
        {
            get { return @Game_System; }
            set { @Game_System = value; }
        }
        internal static void write_game_system(System.IO.BinaryWriter writer)
        {
            Game_System.write(writer);
        }
        internal static void write_events(System.IO.BinaryWriter writer)
        {
            Game_System.write_events(writer);
        }
        internal static void read_game_system(System.IO.BinaryReader reader)
        {
            Game_System = new Game_System();
            Game_System.read(reader);
        }
        internal static void read_events(System.IO.BinaryReader reader)
        {
            Game_System.read_events(reader);
        }

        public static void reset_system()
        {
            Game_System.reset();
        }
        public static void clear_events()
        {
            Game_System.clear_events();
        }
        internal static void play_se(System_Sounds sound)
        {
            Game_System.play_se(sound);
        }
        public static void cancel_sound()
        {
            Game_System.cancel_sound();
        }
        public static bool in_arena()
        {
            return Game_System.In_Arena;
        }

        // Game Temp
        static Game_Temp @Game_Temp;

        internal static Game_Temp game_temp
        {
            get { return @Game_Temp; }
            set { @Game_Temp = value; }
        }

        // Player
        static Player @Player;

        internal static Player player
        {
            get { return @Player; }
            set { @Player = value; }
        }
        #endregion

        #region Data
        // Data Actors
        static Dictionary<int, Data_Actor> Data_Actors;

        public static Dictionary<int, Data_Actor> data_actors
        {
            get { return Data_Actors; }
        }

        //Data Generic Actors
        static Dictionary<string, Data_Generic_Actor> DataGenericActors;

        public static Dictionary<string, Data_Generic_Actor> generic_actors
        {
            get { return DataGenericActors; }
        }

        // Data Animations
        static Dictionary<int, Battle_Animation_Data> Data_Animations;

        public static Dictionary<int, Battle_Animation_Data> data_animations
        {
            get { return Data_Animations; }
        }

        // Data Animation Groups
        static Dictionary<string, int> Data_Animation_Groups;
        
        public static Dictionary<string, int> data_animation_groups
        {
            get { return Data_Animation_Groups; }
        }

        public static int animation_group(string group)
        {
            if (Data_Animation_Groups.ContainsKey(group))
                return Data_Animation_Groups[group];
            if (!Data_Animation_Groups.ContainsKey("Spells"))
            {
#if DEBUG
                //throw new ArgumentOutOfRangeException("Invalid Animation Group");
#endif
            }
            return 0;
        }

        // Data Battler Animations
        static Dictionary<string, Battle_Animation_Association_Set> Data_Battler_Animations;

        public static Dictionary<string, Battle_Animation_Association_Set> data_battler_anims
        {
            get { return Data_Battler_Animations; }
        }

        // Chapter List
        public static List<string> Chapter_List { get; private set; }
        public static Data_Chapter chapter_by_index(int index)
        {
            return Data_Chapters[Chapter_List[index]];
        }

        // Data Chapters
        static Dictionary<string, Data_Chapter> Data_Chapters;

        public static Dictionary<string, Data_Chapter> data_chapters
        {
            get { return Data_Chapters; }
        }

        // Data Classes
        static Dictionary<int, Data_Class> Data_Classes;

        public static Dictionary<int, Data_Class> data_classes
        {
            get { return Data_Classes; }
        }

        // Data Items
        static Dictionary<int, Data_Item> Data_Items;

        public static Dictionary<int, Data_Item> data_items
        {
            get { return Data_Items; }
        }

        // Data Skill
        static Dictionary<int, Data_Skill> Data_Skills;

        public static Dictionary<int, Data_Skill> data_skills
        {
            get { return Data_Skills; }
        }

        public static Data_Skill skill_from_abstract(string id)
        {
            foreach (KeyValuePair<int, Data_Skill> pair in Data_Skills)
                if (pair.Value.Abstract == id)
                    return pair.Value;
            return null;
        }

        // Data Status
        static Dictionary<int, Data_Status> Data_Statuses;

        public static Dictionary<int, Data_Status> data_statuses
        {
            get { return Data_Statuses; }
        }

        // Data Support
        static Dictionary<string, Data_Support> Data_Supports;

        public static Dictionary<string, Data_Support> data_supports
        {
            get { return Data_Supports; }
        }

        // Data Terrain
        static Dictionary<int, Data_Terrain> Data_Terrains;

        public static Dictionary<int, Data_Terrain> data_terrains
        {
            get { return Data_Terrains; }
        }

        // Data Tilesets
        static Dictionary<int, Data_Tileset> Data_Tilesets;

        public static Dictionary<int, Data_Tileset> data_tilesets
        {
            get { return Data_Tilesets; }
        }

        // Data Weapons
        static Dictionary<int, Data_Weapon> Data_Weapons;

        public static Dictionary<int, Data_Weapon> data_weapons
        {
            get { return Data_Weapons; }
        }

        // Weapon Types
        static List<WeaponType> WeaponTypes;

        public static List<WeaponType> weapon_types
        {
            get { return WeaponTypes; }
        }

        // Frame Data
        static Dictionary<string, Frame_Data> Frame_Data;

        public static Dictionary<string, Frame_Data> frame_data
        {
            get { return Frame_Data; }
        }

        // Face Data
        static Dictionary<string, Face_Data> FaceData;

        public static Dictionary<string, Face_Data> face_data
        {
            get { return FaceData; }
        }

        // Palette Data
        static Dictionary<string, SpritePalette> BattlerPaletteData;

        public static Dictionary<string, SpritePalette> battlerPaletteData
        {
            get { return BattlerPaletteData; }
        }

        static Dictionary<string, RecolorData> BattlerRecolorData;

        public static Dictionary<string, RecolorData> battlerRecolorData
        {
            get { return BattlerRecolorData; }
        }

        static Dictionary<string, Color[]> Face_Palette_Data;

        public static Dictionary<string, Color[]> face_palette_data
        {
            get { return Face_Palette_Data; }
        }

        internal static MapSpriteRecolorData Map_Sprite_Colors { get; private set; }
        #endregion

        static Palette_Handler Palette_Pool;
        internal static Palette_Handler palette_pool
        {
            get { return Palette_Pool; }
        }

        // Loaded Files
        static List<string> Loaded_Files;

        internal static List<string> loaded_files
        {
            get { return Loaded_Files; }
        }

        public static void repair_loaded_filenames()
        {
            for (int i = 0; i < Loaded_Files.Count; i++)
            {
                Loaded_Files[i] = Loaded_Files[i].Replace("%28", "(");
                Loaded_Files[i] = Loaded_Files[i].Replace("%29", ")");
            }
        }

        internal static bool content_exists(string filename)
        {
#if DEBUG
            if (filename.Contains('\\'))
                System.Console.WriteLine(string.Format(
                    "Checked if file path with a backslash existed, standardize to forward slashes: \"{0}\"",
                    filename));
#endif
            if (Loaded_Files.Contains(filename))
                return true;
            if (Loaded_Files.Contains(filename.Replace('\\', '/')))
                return true;

            return false;
        }
    }
}
