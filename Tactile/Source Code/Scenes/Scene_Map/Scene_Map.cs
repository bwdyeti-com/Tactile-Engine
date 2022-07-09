using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;
using TactileLibrary;
using EnumExtension;
using TactileRectangleExtension;

namespace Tactile
{
    [Flags]
    enum UnitIcons : int
    {
        None = 0,
        Rescuing =  1 << 0,
        Boss =      1 << 1,
        Dangerous = 1 << 2,
        Protect =   1 << 3
    }

    internal partial class Scene_Map : Scene_Level_Up
    {
        internal const string DEFAULT_MAP_SPRITE = "MapRecruitF";

        protected static List<List<string>> Loaded_Map_Textures = new List<List<string>>();
        protected Character_Sprite Player_Sprite;
        protected Dictionary<int, Character_Sprite> Map_Sprites = new Dictionary<int, Character_Sprite>();
        protected Dictionary<int, Map_Status_Effect> Status_Sprites = new Dictionary<int, Map_Status_Effect>();
        private Turn_Change_Effect Turn_Change;
        private Battle_Transition_Effect Battle_Transition;
        protected static List<ContentManager> Map_Content = new List<ContentManager>();
        private Tilemap @Tilemap;
        protected int Weather;
        private Weather_Handler Weather_Effect;
        private Status_Heal_Effect Status_Heal;
        protected Camera camera;
        protected RasterizerState Unit_Transition_State = new RasterizerState { ScissorTestEnable = true };
        private Hand_Cursor Formation_Hand1;
        private Hand_Cursor Formation_Hand2;
        private List<int> DeferredUnitIds = new List<int>();
        // Screen Color
        protected Sprite Screen_Color;
        protected Color Screen_Color_Target;
        protected int Screen_Color_Duration;
        // Map Spell Darken
        protected int Map_Spell_Darken = 255;
        protected int Map_Spell_Darken_Timer = 0;
        // Turn Skip Text
        private TextSprite Turn_Skip_Text;
        protected int Turn_Skip_Timer = 0;
        // Chapter Transition
        private Chapter_Transition_Effect Chapter_Transition;
        // Map Transition
        protected int Transition_Timer = 0;
        protected bool Map_Transition = false;
        protected int Black_Screen_Time = 0;
        // Map Effect
        protected Map_Effect Unit_Map_Effect;
        // Popup
        private Popup Map_Popup;
        // Game Over
        private GameOver_Background Game_Over;
        // Range Textures
        protected Texture2D Move_Range_Texture, Attack_Range_Texture, Staff_Range_Texture, Talk_Range_Texture, Move_Arrow_Texture,
            All_Enemy_Attack_Range_Texture, All_Enemy_Staff_Range_Texture, Enemy_Attack_Range_Texture, Enemy_Staff_Range_Texture,
            Rescue_Icon, Boss_Icon, DangerIcon, Talk_Icon, SupportIcons, Siege_Ammo_Icon, Flare, White_Square;
        // Map Alpha
        protected static Texture2D Current_Map_Alpha, Map_Alpha_Target, Map_Alpha_Source;
        protected Color[] Map_Alpha_Data;
        protected int Map_Alpha_Timer, Map_Alpha_Duration;
        protected int Suspend_Fade_Timer = 0;

        protected int TILE_SIZE { get { return Constants.Map.TILE_SIZE; } }

        #region Accessors
        public bool map_transition { get { return Map_Transition; } }
        public bool map_transition_ready { get { return Map_Transition && Transition_Timer < 0; } }
        public bool map_transition_running { get { return Black_Screen_Time > 0 || (!Map_Transition && Transition_Timer > 0); } }
        #endregion

        public Scene_Map()
        {
            initialize_base();
        }

        protected override void initialize_base()
        {
            Scene_Type = "Scene_Map";
            main_window();
            camera = new Camera(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT, Vector2.Zero);
        }

        public static void set_content(GameServiceContainer services)
        {
            Map_Content.Add(new ContentManagers.ThreadSafeContentManager(services, "Content"));
            for (int i = 0; i <= Constants.Team.NUM_TEAMS; i++)
            {
                Map_Content.Add(new ContentManagers.ThreadSafeContentManager(services, "Content"));
                Loaded_Map_Textures.Add(new List<string>());
            }
        }

        public void reset_map()
        {
            reset_map(false);
        }
        public void reset_map(bool reset_content)
        {
            base.reset();
            Map_Sprites.Clear();
            Status_Sprites.Clear();
            Turn_Change = null;
            Unit_Map_Effect = null;
            Map_Popup = null;
            clear_graphic_objects();
            if (reset_content)
            {
                Map_Content[0].Unload();
                foreach (ContentManager content in Map_Content)
                {
                    //content.Unload();
                }
                for (int i = 0; i < Loaded_Map_Textures.Count; i++)
                {
                    //Loaded_Map_Textures[i].Clear();
                }
            }
            init_sprites();
            Global.game_map.clear_move_range();
            Move_Range_Texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/MoveRange");
            Attack_Range_Texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/AttackRange");
            Staff_Range_Texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/StaffRange");
            Talk_Range_Texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/TalkRange");
            Move_Arrow_Texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/MoveArrow");
            All_Enemy_Attack_Range_Texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/AllEnemyAttackRange");
            All_Enemy_Staff_Range_Texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/AllEnemyStaffRange");
            Enemy_Attack_Range_Texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/EnemyAttackRange");
            Enemy_Staff_Range_Texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/EnemyStaffRange");
            Rescue_Icon = Global.Content.Load<Texture2D>(@"Graphics/Characters/RescueIcon");
            Boss_Icon = Global.Content.Load<Texture2D>(@"Graphics/Characters/BossIcon");
            DangerIcon = Global.Content.Load<Texture2D>(@"Graphics/Characters/DangerIcon");
            Talk_Icon = Global.Content.Load<Texture2D>(@"Graphics/Characters/TalkIcon");
            SupportIcons = Global.Content.Load<Texture2D>(@"Graphics/Characters/Support");
            Siege_Ammo_Icon = Global.Content.Load<Texture2D>(@"Graphics/Characters/Ballista_Ammo");
            Flare = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Flare");
            White_Square = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            dispose_map_alpha();
            Weather = 0;
            Weather_Effect = null;
            Status_Heal = null;
            Formation_Hand1 = new Hand_Cursor();
            Formation_Hand1.loc = new Vector2(0, -12);
            Formation_Hand1.offset = new Vector2(0, 16);
            Formation_Hand1.angle = MathHelper.PiOver2;
            Formation_Hand2 = new Hand_Cursor();
            Formation_Hand2.loc = new Vector2(0, -12);
            Formation_Hand2.offset = new Vector2(0, 16);
            Formation_Hand2.angle = MathHelper.PiOver2;
            Screen_Color = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            Screen_Color.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Screen_Color_Target = Screen_Color.tint = Color.Transparent;
            Map_Spell_Darken = 255;
            Map_Spell_Darken_Timer = 0;
            Screen_Color_Duration = 0;
            // Cache textures that will be used by the map, but not immediately
            Battle_Transition = new Battle_Transition_Effect(Global.Content.Load<Texture2D>(@"Graphics/Pictures/Turn_Change"));
            create_info_windows();
        }

        protected override void clear_graphic_objects()
        {
            base.clear_graphic_objects();
            clear_menus();
            clear_combat();
        }
        
        public void set_map()
        {
            set_map(Global.data_chapters[Global.game_system.New_Chapter_Id]);
        }
        public void set_map(Data_Chapter chapter)
        {
            set_map(chapter.Id, "", "", chapter.Event_Data_Id);
            //set_map(chapter.Id, chapter.Map_Data_Id, chapter.Unit_Data_Id, chapter.Event_Data_Id); //Debug
        }
        public void set_map(string map_data_key, string unit_data_key, string event_data_key)
        {
            set_map(Global.game_state.chapter_id, map_data_key, unit_data_key, event_data_key);
        }
        public virtual void set_map(string chapter_id, string map_data_key, string unit_data_key, string event_data_key)
        {
            reset_map(true);
            // Map Data
            Data_Map map_data;
            if (map_data_key == "")
                map_data = new Data_Map();
            else
            {
                Data_Map loaded_map_data = get_map_data(map_data_key);
                map_data = new Data_Map(loaded_map_data.values, loaded_map_data.GetTileset());
            }

            if (Global.game_state.turn > 0)
                Global.battalion.refresh_deployed();
            Global.game_state.setup(chapter_id, map_data, unit_data_key, event_data_key);
            set_map_texture();
            Global.player.center();
            Global.game_system.Instant_Move = true;
            Global.game_state.update();
            Global.game_system.update();
        }

#if !MONOGAME && DEBUG
        public void set_map(Scene_Map_Unit_Editor oldScene)
        {
            string chapter_id, map_data_key, unit_data_key, event_data_key;
            oldScene.get_map_data(
                out chapter_id, out map_data_key, out unit_data_key, out event_data_key);

            set_map(chapter_id, map_data_key, unit_data_key, event_data_key);
        }
#endif

        public Data_Map get_map_data(string map_name)
        {
            return new Data_Map(Map_Content[0].Load<Data_Map>(@"Data/Map Data/Maps/" + map_name));
        }

        public void set_map_texture()
        {
            Turn_Skip_Text = new TextSprite();
            Turn_Skip_Text.SetFont(Config.UI_FONT, Global.Content, "White");
            Turn_Skip_Text.loc = new Vector2(264, 160);
            Turn_Skip_Text.text = "Skipping...";
            Turn_Skip_Timer = 0;

            List<Texture2D> animated_textures = new List<Texture2D>();
            foreach (string filename in Global.game_map.animated_tileset_names)
                animated_textures.Add(Map_Content[0].Load<Texture2D>(@"Graphics/Autotiles/" + filename));
            @Tilemap = new Tilemap(
                Map_Content[0].Load<Texture2D>(@"Graphics/Tilesets/" + Global.game_map.tileset_name), animated_textures,
                Global.game_map.animated_tileset_data);
            @Tilemap.stereoscopic = Config.MAP_MAP_DEPTH;
        }

        public void suspend_fade_in()
        {
            Suspend_Fade_Timer = Constants.Map.SUSPEND_FADE_TIME;
        }

        public override bool suspend_blocked()
        {
            return (Global.game_temp.menu_call || Global.game_temp.menuing) && !Global.game_system.preparations;
        }

        #region Map Alpha
        public void set_map_alpha_texture(float[,] alpha_data)
        {
            set_map_alpha_texture(alpha_data, 0);
        }
        public void set_map_alpha_texture(float[,] alpha_data, int time)
        {
            // If no data to set, dispose and return
            if (alpha_data.GetLength(0) == 0)
            {
                dispose_map_alpha();
                return;
            }
            // Else dispose target since it will either be replaced or not used
            else
            {
                // Dispose target alpha state
                if (Map_Alpha_Target != null)
                    if (!Map_Alpha_Target.IsDisposed)
                        Map_Alpha_Target.Dispose();
                Map_Alpha_Target = null;
                // Dispose original alpha state
                if (Map_Alpha_Source != null)
                    if (!Map_Alpha_Source.IsDisposed)
                        Map_Alpha_Source.Dispose();
                Map_Alpha_Source = null;
            }

            // Creates a texture representing the target state, with the size of the alpha data
            Map_Alpha_Target = (Global.Content as ContentManagers.ThreadSafeContentManager)
                .texture_from_size(alpha_data.GetLength(0), alpha_data.GetLength(1));

            Color[] data = new Color[Map_Alpha_Target.Width * Map_Alpha_Target.Height];

            int alpha;
            for (int y = 0; y < Map_Alpha_Target.Height; y++)
                for (int x = 0; x < Map_Alpha_Target.Width; x++)
                {
                    alpha = (int)MathHelper.Clamp(
                        Global.game_map.Tile_Alpha[x, y] * (256f / (Constants.Map.ALPHA_MAX)) *
                            (256 - Global.game_map.min_alpha) / 256 + Global.game_map.min_alpha,
                        Global.game_map.min_alpha, 255);
                    data[x + y * Map_Alpha_Target.Width] = new Color(alpha, alpha, alpha, 255);
                }
            Map_Alpha_Target.SetData<Color>(data);

            // If time is 0, or the size of the existing data is not equal to the size of the target data, set alpha to target instantly
            if (time == 0 || (Current_Map_Alpha != null && Current_Map_Alpha.Width != Map_Alpha_Target.Width && Current_Map_Alpha.Height != Map_Alpha_Target.Height))
            {
                if (Current_Map_Alpha != null)
                    if (!Current_Map_Alpha.IsDisposed)
                        Current_Map_Alpha.Dispose();

                Current_Map_Alpha = Map_Alpha_Target;
                Current_Map_Alpha.Name = "Map Alpha Texture";

                // Load the current alpha texture state into an array
                if (Map_Alpha_Data == null || Map_Alpha_Data.Length != Current_Map_Alpha.Width * Current_Map_Alpha.Height)
                    Map_Alpha_Data = new Color[Current_Map_Alpha.Width * Current_Map_Alpha.Height];
                Current_Map_Alpha.GetData(Map_Alpha_Data);

                Map_Alpha_Target = null;

                Map_Alpha_Duration = 0;
                Map_Alpha_Timer = 0;
            }
            else
            {
                // Create an array to store the alpha data in
                if (Map_Alpha_Data == null || Map_Alpha_Data.Length != alpha_data.Length)
                    Map_Alpha_Data = new Color[alpha_data.Length];

                // If the current data is null, generate a new texture for it
                if (Current_Map_Alpha == null)
                {
                    Current_Map_Alpha = (Global.Content as ContentManagers.ThreadSafeContentManager)
                        .texture_from_size(alpha_data.GetLength(0), alpha_data.GetLength(1));
                    Current_Map_Alpha.Name = "Map Alpha Texture";

                    for (int y = 0; y < Map_Alpha_Target.Height; y++)
                        for (int x = 0; x < Map_Alpha_Target.Width; x++)
                            Map_Alpha_Data[x + y * Map_Alpha_Target.Width] = Color.White;
                    Current_Map_Alpha.SetData<Color>(Map_Alpha_Data);
                }
                else
                    Current_Map_Alpha.GetData(Map_Alpha_Data);

                // Sets alpha source to the current state of the alpha
                Map_Alpha_Source = (Global.Content as ContentManagers.ThreadSafeContentManager).texture_from_size(Current_Map_Alpha.Width, Current_Map_Alpha.Height);
                Map_Alpha_Source.SetData<Color>(Map_Alpha_Data);

                Map_Alpha_Duration = time;
                Map_Alpha_Timer = 0;
            }
        }

        protected void update_map_alpha()
        {
            if (Map_Alpha_Duration != 0)
            {
                Map_Alpha_Timer++;
                // If done, set the alpha set to the target
                if (Map_Alpha_Duration - Map_Alpha_Timer <= 0)
                {
                    if (Current_Map_Alpha != null)
                        if (!Current_Map_Alpha.IsDisposed)
                            Current_Map_Alpha.Dispose();

                    Current_Map_Alpha = Map_Alpha_Target;
                    Current_Map_Alpha.Name = "Map Alpha Texture";
                    Map_Alpha_Target = null;
                    // Load the current alpha texture state into an array
                    if (Map_Alpha_Data == null || Map_Alpha_Data.Length != Current_Map_Alpha.Width * Current_Map_Alpha.Height)
                        Map_Alpha_Data = new Color[Current_Map_Alpha.Width * Current_Map_Alpha.Height];
                    Current_Map_Alpha.GetData(Map_Alpha_Data);

                    if (Map_Alpha_Source != null)
                        if (!Map_Alpha_Source.IsDisposed)
                            Map_Alpha_Source.Dispose();

                    Map_Alpha_Duration = 0;
                    Map_Alpha_Timer = 0;
                }
                else
                {
                    // isn't it super wasteful to dispose here, when I can just reuse the existing texture?
                    //Current_Map_Alpha.Dispose();
                    //Current_Map_Alpha = (Global.Content as ContentManagers.ThreadSafeContentManager).texture_from_size(Map_Alpha_Source.Width, Map_Alpha_Source.Height);

                    // This should never come up though, honestly //Debug
                    if (Map_Alpha_Data == null || Map_Alpha_Data.Length != Current_Map_Alpha.Width * Current_Map_Alpha.Height)
                        Map_Alpha_Data = new Color[Current_Map_Alpha.Width * Current_Map_Alpha.Height];

                    Color[] map_data = new Color[Current_Map_Alpha.Width * Current_Map_Alpha.Height];
                    Color[] target_data = new Color[Current_Map_Alpha.Width * Current_Map_Alpha.Height];
                    
                    Map_Alpha_Source.GetData(map_data);
                    Map_Alpha_Target.GetData(target_data);

                    for (int y = 0; y < Current_Map_Alpha.Height; y++)
                        for (int x = 0; x < Current_Map_Alpha.Width; x++)
                        {
                            Map_Alpha_Data[x + y * Current_Map_Alpha.Width] = new Color(
                                target_data[x + y * Current_Map_Alpha.Width].ToVector3() * Map_Alpha_Timer / Map_Alpha_Duration +
                                    (map_data[x + y * Current_Map_Alpha.Width].ToVector3() * (Map_Alpha_Duration - Map_Alpha_Timer) / Map_Alpha_Duration));
                        }
                    Current_Map_Alpha.SetData<Color>(Map_Alpha_Data);
                }
            }
        }

        protected void dispose_map_alpha()
        {
            if (Current_Map_Alpha != null)
                if (!Current_Map_Alpha.IsDisposed)
                    Current_Map_Alpha.Dispose();
            Current_Map_Alpha = null;
            Map_Alpha_Data = null;

            Map_Alpha_Data = null;
            if (Map_Alpha_Target != null)
                if (!Map_Alpha_Target.IsDisposed)
                    Map_Alpha_Target.Dispose();
            Map_Alpha_Target = null;

            if (Map_Alpha_Source != null)
                if (!Map_Alpha_Source.IsDisposed)
                    Map_Alpha_Source.Dispose();
            Map_Alpha_Source = null;

            Map_Alpha_Duration = 0;
            Map_Alpha_Timer = 0;
        }

        protected Color get_unit_tint(Vector2 loc)
        {
            if (!Global.shader_exists)
                return Color.White;
            loc = new Vector2(MathHelper.Clamp(loc.X, 0, Global.game_map.width - 1),
                MathHelper.Clamp(loc.Y, 0, Global.game_map.height - 1));
            int x = (int)((loc.X + 0.5f) * Constants.Map.ALPHA_GRANULARITY);
            int y = (int)((loc.Y + 0.5f) * Constants.Map.ALPHA_GRANULARITY);
            return get_tint(x, y);
        }

        protected Color get_tint(int x, int y)
        {
            if ((Global.game_map.min_alpha == 255 && Map_Alpha_Duration == 0) || Current_Map_Alpha == null || Map_Alpha_Data == null)
                return Color.White;
            return Map_Alpha_Data[x + y * Current_Map_Alpha.Width];
        }
        #endregion

        public void change_turn(int turn)
        {
            Turn_Change = new Turn_Change_Effect(turn);
            Turn_Change.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Turn_Change");
        }

        public bool is_changing_turn()
        {
            return Turn_Change != null;
        }

        public void set_screen_color(Color color)
        {
            set_screen_color(color, 0);
        }
        public void set_screen_color(Color color, int duration)
        {
            Screen_Color_Duration = Math.Max(0, duration) + 1;
            Screen_Color_Target = color;
            update_screen_color();
        }

        public void spell_brighten(bool brighten)
        {
            Map_Spell_Darken_Timer = (brighten ? 1 : -1) *
                Constants.Map.MAP_SPELL_DARKEN_TIME;
        }

        public void reset_skip_timer()
        {
            Turn_Skip_Timer = 0;
        }

        public void change_tile(Vector2 loc, int id, bool roof = false)
        {
            @Tilemap.change_tile(loc, id, roof);
        }

        public void gameover()
        {
            Game_Over = new GameOver_Background();
        }

        public bool is_gameover()
        {
            return Game_Over != null;
        }

        #region Update
        public override void update()
        {
            if (update_soft_reset() || is_test_battle)
                return;
            // Game Over
            if (is_gameover())
            {
                Game_Over.update();
                if (Game_Over.fading_in)
                {
                    Global.game_system.update_timers();
                    Global.game_map.update_characters_while_waiting();
                    @Tilemap.update();
                    update_sprites();
                }
                if (Game_Over.finished)
                {
#if DEBUG
                    if (Global.UnitEditorActive)
                        Global.scene_change("Scene_Map_Unit_Editor");
                    else
#endif
                        Global.scene_change("Scene_Title_Load");
                }
            }
            // Fade screen in when loading a suspend
            else if (Suspend_Fade_Timer > 0)
            {
                if (Suspend_Fade_Timer > 0)
                    Suspend_Fade_Timer--;
                Global.game_system.update_timers();
                Global.game_map.update_characters_while_waiting();
                @Tilemap.update();
                update_weather();
                update_sprites();
            }
            // Normal processing
            else if (!Global.game_map.move_ranges_need_update)
            {
                update_normal_processing();
            }
            else
            {
                if (Global.game_map.move_ranges_need_update)
                    Global.game_map.run_move_range_update();
                Global.game_state.update_ai_skip();
                @Tilemap.update();
                update_sprites();
            }

            Global.game_state.update_tone();
            float skip_opacity =
                ((Turn_Skip_Timer > Constants.Map.TURN_SKIP_TEXT_FLASH_TIME / 2) ?
                    (Constants.Map.TURN_SKIP_TEXT_FLASH_TIME - Turn_Skip_Timer) :
                    Turn_Skip_Timer) /
                (float)(Constants.Map.TURN_SKIP_TEXT_FLASH_TIME / 2);
            Turn_Skip_Text.tint = new Color(
                skip_opacity, skip_opacity, skip_opacity, skip_opacity);
            Turn_Skip_Timer = (Turn_Skip_Timer + 1) % Constants.Map.TURN_SKIP_TEXT_FLASH_TIME;
            update_screen_color();
            update_spell_brightness();
        }

        private void update_normal_processing()
        {
            update_message();
            @Tilemap.update();
            update_data();
            update_weather();
            if (Black_Screen_Time > 0)
                Black_Screen_Time--;
            else
                Transition_Timer--;
            base.update();
            update_menu();
            update_menu_calls();
            if (Global.game_map.move_ranges_need_update)
                Global.game_map.run_move_range_update();
            Global.game_state.update_ai_skip();
            update_sprites();
            update_combat();
            if (Chapter_Transition != null)
            {
                Chapter_Transition.update();
                if (Chapter_Transition.done)
                {
                    Chapter_Transition = null;
                    //Audio.se_stop(); //Yeti
                }
            }
        }

        protected void update_screen_color()
        {
            if (Screen_Color_Duration >= 1)
            {
                if (Screen_Color_Duration == 1)
                    Screen_Color.tint = Screen_Color_Target;
                else
                {
                    Screen_Color.tint = new Color(
                        (byte)((Screen_Color.tint.R * (Screen_Color_Duration - 1) +
                            Screen_Color_Target.R) / Screen_Color_Duration),
                        (byte)((Screen_Color.tint.G * (Screen_Color_Duration - 1) +
                            Screen_Color_Target.G) / Screen_Color_Duration),
                        (byte)((Screen_Color.tint.B * (Screen_Color_Duration - 1) +
                            Screen_Color_Target.B) / Screen_Color_Duration),
                        (byte)((Screen_Color.tint.A * (Screen_Color_Duration - 1) +
                            Screen_Color_Target.A) / Screen_Color_Duration));
                }
                Screen_Color_Duration--;
            }
        }

        protected void update_spell_brightness()
        {
            if (Map_Spell_Darken_Timer != 0)
            {
                // Brightening
                if (Map_Spell_Darken_Timer > 0)
                {
                    Map_Spell_Darken_Timer--;
                    if (Map_Spell_Darken_Timer == 0)
                        Map_Spell_Darken = 255;
                    else
                        Map_Spell_Darken =
                            (Constants.Map.MAP_SPELL_DARKEN_MIN * Map_Spell_Darken_Timer) /
                                Constants.Map.MAP_SPELL_DARKEN_TIME +
                            (256 * (Constants.Map.MAP_SPELL_DARKEN_TIME - Map_Spell_Darken_Timer)) /
                                Constants.Map.MAP_SPELL_DARKEN_TIME;
                }
                // Darkening
                else
                {
                    Map_Spell_Darken_Timer++;
                    if (Map_Spell_Darken_Timer == 0)
                        Map_Spell_Darken = Constants.Map.MAP_SPELL_DARKEN_MIN;
                    else
                        Map_Spell_Darken = (256 * (-1 * Map_Spell_Darken_Timer)) /
                                Constants.Map.MAP_SPELL_DARKEN_TIME +
                            (Constants.Map.MAP_SPELL_DARKEN_MIN * (Constants.Map.MAP_SPELL_DARKEN_TIME - (-1 * Map_Spell_Darken_Timer))) /
                                Constants.Map.MAP_SPELL_DARKEN_TIME;
                }
            }
        }

        public override void update_data()
        {
            UpdateInfoWindowInputs();
            Global.player.update();
            Global.game_state.update();
            update_info_windows();
            Global.game_system.update();
        }

        protected void update_weather()
        {
            if (Weather_Effect != null)
                Weather_Effect.update(Global.game_map.display_loc);
            if (Global.game_state.weather != Weather)
            {
                switch ((Weather_Types)Global.game_state.weather)
                {
                    case Weather_Types.Clear:
                        Weather_Effect = null;
                        break;
                    case Weather_Types.Rain:
                        Weather_Effect = new Rain_Handler(Global.game_map.display_loc);
                        Weather_Effect.stereoscopic = Config.MAP_WEATHER_DEPTH;
                        break;
                    case Weather_Types.Snow:
                        Weather_Effect = new Snow_Handler(Global.game_map.display_loc);
                        Weather_Effect.stereoscopic = Config.MAP_WEATHER_DEPTH;
                        break;
                    default:
                        Weather_Effect = null;
                        break;
                }
                Weather = Global.game_state.weather;
            }
        }

        protected override bool has_convo_scene_button
        {
            get
            {
                if (Global.game_state.ai_skipping_allowed)
                    return false;
                else if (Global.game_state.switching_ai_skip)
                    return false;
                else if (!Global.game_state.is_map_ready() &&
                        (Message_Window == null || !Message_Window.active))
                    return false;
                return true;
            }
        }

        protected override bool skip_convo_button_active
        {
            get
            {
                if (is_chapter_change_visible)
                    return false;
                if (Ranking_Window != null)
                    return false;

                if (Global.game_state.ai_skipping_allowed)
                    return true;

                return base.skip_convo_button_active;
            }
        }
        #endregion

        #region Map Sprite Handling
        public static void set_recolors()
        {
            string[] name_ary;
            foreach (string filename in Global.loaded_files) // loaded files shouldn't be public //Debug
            {
                name_ary = filename.Split('\\');
                if (name_ary.Length > 1 && name_ary[0] == "Graphics" && name_ary[1] == "Characters")
                {
                    if (name_ary.Length == 3 && name_ary[2].Substring(0, 3) == "Map")
                        for (int i = 0; i <= Constants.Team.NUM_TEAMS; i++)
                            get_team_map_sprite(i, name_ary[2]);
                }
            }
        }

        internal static Texture2D get_team_map_sprite(int team, string name)
        {
            if (!Global.content_exists(@"Graphics/Characters/" + name))
            {
                name = DEFAULT_MAP_SPRITE + (name.Substring(name.Length - 5, 5) == "_move" ? "_move" : "");
                //return null;
            }
            // Note: Team 0 is greyed out

            Texture2D texture;
            if (!Loaded_Map_Textures[team].Contains(name))
            {
                texture = Map_Content[team + 1].Load<Texture2D>(@"Graphics/Characters/" + name);
                recolor_map_sprite(team, texture);
                Loaded_Map_Textures[team].Add(name);
            }

            texture = Map_Content[team + 1].Load<Texture2D>(@"Graphics/Characters/" + name);
            return texture;
        }

        protected static void recolor_map_sprite(int team, Texture2D texture)
        {
            int width = texture.Width;
            int height = texture.Height;
            // Gets base texture colors
            Color[] texture_data = new Color[texture.Width * texture.Height];
            texture.GetData(texture_data);
            // Writes colors to texture
            for (int y = 0; y < texture.Height; y++)
                for (int x = 0; x < texture.Width; x++)
                {
                    if (Global.Map_Sprite_Colors.data.Keys.Contains(texture_data[x + y * width]))
                        texture_data[x + y * width] = Global.Map_Sprite_Colors.data[texture_data[x + y * width]][team];
                }

            texture.SetData(texture_data);
        }

        public void set_map_effect(Vector2 loc, int type, int id)
        {
            Unit_Map_Effect = new Map_Effect(type, id);
            Unit_Map_Effect.loc = loc * TILE_SIZE + new Vector2(TILE_SIZE, TILE_SIZE) / 2;
            string name = Unit_Map_Effect.filename;
            if (name == "")
                Unit_Map_Effect = null;
            else
                Unit_Map_Effect.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + name);
            Unit_Map_Effect.stereoscopic = Config.MAP_STATUS_ICON_DEPTH;
        }

        public void set_ballista_effect(Vector2 loc, Vector2 dest_loc)
        {
            Unit_Map_Effect = new Ballista_Bolt(loc, dest_loc);
        }

        public bool is_map_effect_active()
        {
            return Unit_Map_Effect != null;
        }

        public bool is_map_effect_hit()
        {
            if (Unit_Map_Effect == null)
                return true;
            return Unit_Map_Effect.hit;
        }

        private void set_popup(Popup popup)
        {
            Map_Popup = popup;
            Map_Popup.stereoscopic = Config.MAP_POPUP_DEPTH;
        }

        public void set_popup(string str, int time)
        {
            set_popup(str, time, 128);
        }
        public void set_popup(string str, int time, int width)
        {
            set_popup(new Popup(str, time, width));
        }

        public void set_item_popup(Item_Data item_data, int time)
        {
            set_popup(new Item_Gain_Popup(item_data.Id, item_data.is_item, time));
        }
        public void set_item_loss_popup(Item_Data item_data, int time)
        {
            set_popup(new Item_Loss_Popup(item_data.Id, item_data.is_item, time));
        }
        public void set_item_drop_popup(Item_Data item_data, int time)
        {
            set_popup(new Item_Drop_Popup(item_data.Id, item_data.is_item, time));
        }
        public void set_item_sent_popup(Item_Data item_data, int time)
        {
            set_popup(new Item_Sent_Popup(item_data.Id, item_data.is_item, time));
        }
        public void set_item_steal_popup(Item_Data item_data, int time)
        {
            set_popup(new Item_Steal_Popup(item_data.Id, item_data.is_item, time));
        }
        public void set_item_stolen_popup(Item_Data item_data, int time)
        {
            set_popup(new Item_Stolen_Popup(item_data.Id, item_data.is_item, time));
        }
        public void set_item_received_popup(Item_Data item_data, int time)
        {
            set_popup(new Item_Received_Popup(item_data.Id, item_data.is_item, time));
        }

        public void set_gold_gain_popup(int value)
        {
            set_popup(new Gold_Gain_Popup(value));
        }

        public void set_stat_gain_popup(Item_Data item_data, Game_Unit unit, int time)
        {
            set_popup(new Stat_Boost_Popup(item_data.Id, item_data.is_item, unit, time));
        }
        public void set_repair_popup(Item_Data item_data, Game_Unit unit, int time)
        {
            set_popup(new Item_Repair_Popup(item_data.Id, item_data.is_item, time));
        }

        public bool is_map_popup_active()
        {
            return Map_Popup != null;
        }
        #endregion

        #region Sprite Handling
        protected override ContentManager level_up_content()
        {
            return Global.Content;
        }

        public virtual void init_sprites()
        {
            // Character Sprite
            Player_Sprite = new Character_Sprite(Global.Content.Load<Texture2D>(@"Graphics/Characters/Cursor"));
            Player_Sprite.offset = new Vector2(16, 16);
            Player_Sprite.stereoscopic = Config.MAP_CURSOR_DEPTH;
            // Status Effects
            foreach (int id in Global.data_statuses.Keys)
            {
                if (Map_Animations.STATUS_EFFECT_IDS.ContainsKey(id))
                {
                    Data_Status status = Global.data_statuses[id];
                    Status_Sprites.Add(id, new Map_Status_Effect(3, Map_Animations.status_effect_id(id)));
                    string name = Status_Sprites[id].filename;
                    if (name == "")
                        Status_Sprites.Remove(id);
                    else
                        Status_Sprites[id].texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + name);
                    Status_Sprites[id].stereoscopic = Config.MAP_STATUS_ICON_DEPTH;
                }
            }
            Map_Status_Effect.status_ids = Status_Sprites.Keys.ToList();
        }

        public void re_add_map_sprites()
        {
            Map_Sprites.Clear();
            Global.game_map.init_sprites();
        }

        public Character_Sprite add_map_sprite(int id)
        {
            Map_Sprites.Add(id, new Character_Sprite());
            Map_Sprites[id].draw_offset = new Vector2(TILE_SIZE / 2, TILE_SIZE);
            Map_Sprites[id].stereoscopic = Config.MAP_UNITS_DEPTH;
            update_map_sprite_status(id);
            return Map_Sprites[id];
        }

        public override void remove_map_sprite(int id)
        {
            Map_Sprites.Remove(id);
        }

        public void refresh_map_sprite(
            int id, int team, string filename, bool moving)
        {
            Character_Sprite sprite = Map_Sprites[id];
            refresh_map_sprite(sprite, team, filename, moving);
        }
        public static void refresh_map_sprite(
            Character_Sprite sprite,
            int team,
            string filename,
            bool moving)
        {
            sprite.finish_animation();
            Texture2D texture = get_team_map_sprite(
                team, Game_Actors.map_sprite_name(filename, moving));
            sprite.texture = texture;
            if (sprite.texture != null)
            {
                if (!moving)
                {
                    sprite.facing_count = 3;
                    sprite.frame_count = 3;
                    sprite.offset = new Vector2(
                        (texture.Width / sprite.frame_count) / 2,
                        (texture.Height / sprite.facing_count) - 8);
                }
                else
                {
                    sprite.facing_count = 4;
                    sprite.frame_count = 4;
                    sprite.offset = new Vector2(
                        (texture.Width / sprite.frame_count) / 2,
                        Math.Min((texture.Height / sprite.facing_count),
                            (texture.Height / (sprite.facing_count * 2)) + 20));
                }
            }
        }

        public void update_map_sprite_status(int id)
        {
            if (!Map_Sprites.ContainsKey(id) || !Global.game_map.units.ContainsKey(id))
                return;
            Map_Sprites[id].update_status(Global.game_map.units[id].actor.states);
        }

        public override void update_sprites()
        {
            // Cursor
            Player.update_anim();
            Global.player.update_sprite(Player_Sprite);
            // Map Sprites
            foreach (KeyValuePair<int, Character_Sprite> sprite in Map_Sprites)
            {
                Global.game_map.displayed_map_object(sprite.Key).update_sprite(sprite.Value);
            }
            // Status Effects
            Character_Sprite.update_status_timer();
            foreach (KeyValuePair<int, Map_Status_Effect> sprite in Status_Sprites)
            {
                sprite.Value.update();
            }
            // Turn Change
            if (Turn_Change != null)
            {
                Turn_Change.update();
                if (Turn_Change.is_finished)
                {
                    Turn_Change = null;
                    Global.game_state.turn_change_end();
                }
            }
            // Status Heal
            if (Status_Heal != null)
            {
                Status_Heal.update();
                if (Status_Heal.is_finished)
                    Status_Heal = null;
            }
            // Formation
            if ((Global.game_system.preparations && Changing_Formation)
#if !MONOGAME && DEBUG
                    || (Global.game_state.moving_editor_unit)
#endif
                    )
                Formation_Hand1.update();

            if (Unit_Map_Effect != null)
            {
                Unit_Map_Effect.update();
                if (Unit_Map_Effect.finished)
                    Unit_Map_Effect = null;
            }
            if (Map_Popup != null)
            {
                Map_Popup.update();
                if (Map_Popup.finished)
                    Map_Popup = null;
            }
            update_map_alpha();
        }

        public void set_status_heal(Game_Unit unit)
        {
            Global.Audio.play_se("Map Sounds", "Status_Heal");
            Status_Heal = new Status_Heal_Effect();
            Status_Heal.mirrored = unit.has_flipped_map_sprite;
            Status_Heal.loc = unit.loc * TILE_SIZE +
                new Vector2(TILE_SIZE / 2, TILE_SIZE);
        }

        public void set_map_animation(int id, int type, int anim_id)
        {
            Map_Sprites[id].set_animation(Global.game_map.units[id], Map_Animations.unit_data(type, anim_id));
        }

        public bool map_animation_finished(int id)
        {
            return !Map_Sprites[id].animation_active;
        }

        public void transition_out()
        {
            Transition_Timer = Constants.Map.MAP_TRANSITION_TIME;
            Map_Transition = true;
        }

        public void transition_in()
        {
            Transition_Timer = Constants.Map.MAP_TRANSITION_TIME;
            Map_Transition = false;
        }

        public void black_screen(int time)
        {
            Black_Screen_Time = time;
        }

        public void chapter_change()
        {
            Chapter_Transition = new Chapter_Transition_Effect(
                Global.data_chapters[Global.game_state.chapter_id]);
        }

        public void cancel_chapter_change()
        {
            if (Chapter_Transition != null)
                return;
            Chapter_Transition.clear();
        }

        public bool is_changing_chapters { get { return Chapter_Transition != null && !Chapter_Transition.ready; } }
        public bool is_chapter_change_visible { get { return Chapter_Transition != null; } }

        protected static Vector2 rescue_icon_draw_vector()
        {
            return Stereoscopic_Graphic_Object.graphic_draw_offset(Config.MAP_STATUS_ICON_DEPTH);
        }
        protected static Vector2 hp_gauge_draw_vector()
        {
            return Stereoscopic_Graphic_Object.graphic_draw_offset(Config.MAP_HPGAUGE_DEPTH);
        }
        protected static Vector2 move_range_draw_vector()
        {
            return Stereoscopic_Graphic_Object.graphic_draw_offset(Config.MAP_MOVE_RANGE_DEPTH);
        }
        #endregion

        #region Draw
        public override void draw(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            if (!Global.game_state.skip_ai_turn_active)
            {
                if (!Global.game_system.In_Arena && !is_test_battle && !(Global.game_state.transition_to_battle && Global.game_system.preparations))
                {
                    Effect map_shader = Global.effect_shader();
                    if (map_shader != null)
                    {
                        map_shader.Parameters["alpha_offset"].SetValue(
                            new Vector2(Global.game_map.display_x,
                                Global.game_map.display_y));
                        map_shader.Parameters["map_size"].SetValue(
                            new Vector2(Global.game_map.width,
                                Global.game_map.height));
                        map_shader.Parameters["game_size"].SetValue(
                            new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT));
                    }
                    draw_scene(sprite_batch, device, render_targets);
                    //draw_rotated_scene(sprite_batch, device, render_targets);
                }

                if (true)//!is_message_window_active)
                {
                    draw_weather(sprite_batch);
                }

                // Draw menus
                draw_info_windows(sprite_batch);
                draw_menus(sprite_batch, device, render_targets);
                draw_minimap(sprite_batch, device, render_targets);
                draw_map_combat(sprite_batch);

                // Turn Change Effect
                if (Turn_Change != null)
                {
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    Turn_Change.draw(sprite_batch);
                    sprite_batch.End();
                }
                draw_battle_transition(sprite_batch);
                //draw_chapter_transition(sprite_batch); //Debug
                draw_map_level_up(sprite_batch);
                
                if (is_strict_map_scene)
                {
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    Screen_Color.draw(sprite_batch);
                    sprite_batch.End();
                    // Draw Convos
                    draw_message(sprite_batch, device, render_targets);
                }
                if (false)//is_message_window_active)
                {
                    draw_weather(sprite_batch);
                }
                // If fading in from suspending
                if (Suspend_Fade_Timer > 0)
                {
                    draw_suspend_fade(sprite_batch, device, render_targets);
                }
                // If switching too/from skipping AI turn, screen needs to be black/darkening
                else if (Global.game_state.switching_ai_skip_counter > 0 || Transition_Timer > 0 || Map_Transition || Black_Screen_Time > 0)
                {
                    draw_ai_skip_switch(sprite_batch, device, render_targets);
                }
                draw_chapter_transition(sprite_batch);
            }
            else
            {
                device.SetRenderTarget(render_targets[0]);
                device.Clear(Color.Transparent);
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Turn_Skip_Text.draw(sprite_batch);
                sprite_batch.End();
            }
        }

        protected override void draw_message(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            // Message
            if (Message_Window != null)
            {
                device.SetRenderTarget(render_targets[1]);
                device.Clear(Color.Transparent);
                Message_Window.draw_background(sprite_batch);
                device.SetRenderTarget(render_targets[0]);
                Effect map_shader = Global.effect_shader();
                if (map_shader != null)
                {
                    map_shader.CurrentTechnique = map_shader.Techniques["Tone"];
                    map_shader.Parameters["tone"].SetValue(Global.game_state.screen_tone.to_vector_4(1.0f));
                }
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, map_shader);
                sprite_batch.Draw(render_targets[1], Vector2.Zero, Color.White);
                sprite_batch.End();

                device.SetRenderTarget(render_targets[1]);
                device.Clear(Color.Transparent);
                Message_Window.draw_faces(sprite_batch);
                device.SetRenderTarget(render_targets[0]);
                if (map_shader != null)
                {
                    map_shader.CurrentTechnique = map_shader.Techniques["Tone"];
                    map_shader.Parameters["tone"].SetValue(Global.game_state.screen_tone.to_vector_4(Config.FACE_TONE_PERCENT));
                }
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, map_shader);
                sprite_batch.Draw(render_targets[1], Vector2.Zero, Color.White);
                sprite_batch.End();

                Message_Window.draw_foreground(sprite_batch);
            }

            draw_message_overlay(sprite_batch, device, render_targets);
        }

        protected override void draw_message_overlay(
            SpriteBatch spriteBatch,
            GraphicsDevice device,
            RenderTarget2D[] renderTargets)
        {
            base.draw_message_overlay(spriteBatch, device, renderTargets);

            // Item discard
            draw_discard(spriteBatch, device, renderTargets);
            // Popup
            if (Map_Popup != null)
            {
                Map_Popup.draw(spriteBatch);
            }
            // Game Over
            if (Game_Over != null)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                Game_Over.draw(spriteBatch);
                spriteBatch.End();
            }
        }

        protected virtual void draw_map_level_up(SpriteBatch sprite_batch)
        {
            draw_level_up(sprite_batch, Global.game_map.display_loc);
        }

        protected virtual void draw_scene(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            camera.pos = Vector2.Zero;
            camera.offset = Vector2.Zero;
            camera.zoom = Vector2.One;
            camera.angle = 0f;

            #region Map and Idle Units
            // Base map
            draw_map(sprite_batch, device, render_targets);

            // Move ranges
            draw_ranges(sprite_batch);

            // If the map has clearing roof tiles, draw the units that would be under them and then draw the roof tiles on top of them
            var roof_tiles = @Tilemap.roof_tiles;
            if (roof_tiles.Count > 0)
            {
                draw_units(sprite_batch, device, render_targets, true, roof_tiles);
                // Copies the map with units on it to render_targets[2]
                device.SetRenderTarget(render_targets[2]);
                device.Clear(Color.Transparent);
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                sprite_batch.Draw(render_targets[0], Vector2.Zero, Color.White);
                sprite_batch.End();

                // Draws the roof tiles to render_targets[0]
                draw_map(sprite_batch, device, render_targets, roof: true);

                // Copies the roof tiles to render_targets[2] on top of the current map, then moves it all back to render_targets[0]
                //device.SetRenderTarget(render_targets[2]);
                device.SetRenderTarget(render_targets[1]);
                device.Clear(Color.Transparent); //Debug
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                sprite_batch.Draw(render_targets[2], Vector2.Zero, Color.White);
                sprite_batch.Draw(render_targets[0], Vector2.Zero, Color.White);
                sprite_batch.End();

                device.SetRenderTarget(render_targets[0]);
                device.Clear(Color.Transparent);
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                sprite_batch.Draw(render_targets[1], Vector2.Zero, Color.White);
                sprite_batch.End();
            }

            // Map grid
            draw_map_grid(sprite_batch, false);

            // Tile outlines
            DrawTileOutlines(sprite_batch, device, render_targets);

            // Idle units (that aren't under a roof)
            draw_units(sprite_batch, device, render_targets, false, roof_tiles);
            #endregion

            draw_arrow(sprite_batch);

            #region Active Units
            // Draw active units on render target 1, then copy them with tone on render target 0
            device.SetRenderTarget(render_targets[1]);
            device.Clear(Color.Transparent);
            draw_active_units(sprite_batch);
            // Unit tone
            device.SetRenderTarget(render_targets[0]);
            Effect map_shader = Global.effect_shader();
            if (map_shader != null)
            {
                map_shader.CurrentTechnique = map_shader.Techniques["Tone"];
                map_shader.Parameters["tone"].SetValue(Global.game_state.screen_tone.to_vector_4(Config.UNIT_TONE_PERCENT));
            }
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, map_shader);
            sprite_batch.Draw(render_targets[1], Vector2.Zero, Color.White);
            sprite_batch.End();

            // Status animations and icons
            get_deferred_unit_ids();
            List<int> units = units_to_draw(DeferredUnitIds, false, true);

            // Units hidden
            if (!Global.game_map.UnitsHidden)
            {
                draw_unit_status(sprite_batch, units);
                if (Suspend_Fade_Timer == 0)
                    draw_unit_icons(sprite_batch, units);
                draw_unit_support_gain(sprite_batch);
            }
            // Unit effect and flares
            draw_effects(sprite_batch);
            #endregion

            // Draw Player
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            draw_formation_change2(sprite_batch);
            Player_Sprite.draw(sprite_batch, Global.game_map.display_loc);
            draw_formation_change1(sprite_batch);
            sprite_batch.End();

            // Units hidden
            if (!Global.game_map.UnitsHidden)
            {
                if (Suspend_Fade_Timer == 0)
                    draw_unit_icons_above_cursor(sprite_batch, units);
            }
        }

        //@Debug: are these final
        private void DrawTileOutlines(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            // Units hidden
            if (Global.game_map.UnitsHidden)
                return;

            foreach (var outline in Global.game_map.tile_outlines)
            {
                // Draw each outline in greyscale on its own to a render target
                device.SetRenderTarget(render_targets[1]);
                device.Clear(Color.Transparent);

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                for (int i = -1; i < 3; i++)
                {
                    float j = ((Global.game_system.total_play_time + i * 30) % 90) / 90f;
                    if (i == -1)
                        j = 0;
                    Color tint = Color.White * (1 - (float)Math.Pow(j, 0.75f));
                    foreach (var edge in outline.get_edges())
                    {
                        Vector2 loc = new Vector2(edge.X, edge.Y) * TILE_SIZE;
                        loc += new Vector2(0, -j * TILE_SIZE * 0.75f);
                        sprite_batch.Draw(White_Square,
                            loc - Global.game_map.display_loc,
                            new Rectangle(0, 0, 16, 16), tint,
                            edge.Width == 1 ? 0f : MathHelper.PiOver2,
                            new Vector2(0, 8), new Vector2(TILE_SIZE / 16f, 2 / 16f),
                            SpriteEffects.None, 0f);
                    }
                }
                sprite_batch.End();

                // Then copy the completed outline to the render
                device.SetRenderTarget(render_targets[0]);
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.PointClamp, null, null);
                sprite_batch.Draw(render_targets[1], Vector2.Zero, outline.Tint);
                sprite_batch.End();
            }
        }

        float doop = 0f;
        protected void draw_rotated_scene(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            camera.pos = Vector2.Zero + new Vector2(160, 96);
            camera.offset = Vector2.Zero + new Vector2(160, 96);
            camera.zoom = new Vector2(1.4f, 0.8f);
            camera.angle = (float)(Math.PI/4);
            doop += (float)(Math.PI/240);
            //camera.angle = doop;

            // Base map
            device.SetRenderTarget(render_targets[0]);
            device.Clear(Color.Transparent);
            draw_raw_map(sprite_batch, true, fog: false);

            // Map Alpha
            device.SetRenderTarget(render_targets[1]);
            device.Clear(Color.Transparent);
            //Map_Effect.Parameters["map_alpha"].SetValue(bloop);
            Effect alpha_shader = Global.effect_shader();
            if (Current_Map_Alpha != null)
            {
                sprite_batch.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
                if (alpha_shader != null)
                {
                    alpha_shader.CurrentTechnique = alpha_shader.Techniques["Map_Lighting"];
#if __ANDROID__
                    // There has to be a way to do this for both
                    alpha_shader.Parameters["Map_Alpha"].SetValue(Current_Map_Alpha);
#else
                    sprite_batch.GraphicsDevice.Textures[1] = Current_Map_Alpha;
#endif
                }
            }
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, alpha_shader);
            sprite_batch.Draw(render_targets[0], Vector2.Zero,
                new Color(Map_Spell_Darken, Map_Spell_Darken, Map_Spell_Darken, 255));
            sprite_batch.End();
#if __ANDROID__
            // There has to be a way to do this for both
            if (alpha_shader != null)
                alpha_shader.Parameters["Map_Alpha"].SetValue((Texture2D)null);
#else
            sprite_batch.GraphicsDevice.Textures[1] = null;
#endif

            // Map tone
            device.SetRenderTarget(render_targets[0]);
            device.Clear(Color.Black);
            if (Global.game_map.width <= 0)
                return;

            Effect map_shader = Global.effect_shader();
            if (map_shader != null)
            {
                map_shader.CurrentTechnique = map_shader.Techniques["Tone"];
                map_shader.Parameters["tone"].SetValue(Global.game_state.screen_tone.to_vector_4(1.0f));
            }
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, map_shader);
            sprite_batch.Draw(render_targets[1], Vector2.Zero, Color.White);
            sprite_batch.End();

            device.SetRenderTarget(render_targets[1]);
            device.Clear(Color.Transparent);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            sprite_batch.Draw(render_targets[0], Vector2.Zero, Color.White);
            sprite_batch.End();

            draw_ranges(sprite_batch);
            // Move Arrow
            draw_arrow(sprite_batch);
            // Draw Player
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Player_Sprite.draw(sprite_batch, Global.game_map.display_loc);
            sprite_batch.End();

            // Modifies Map view angle
            device.SetRenderTarget(render_targets[0]);
            device.Clear(Color.Black);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null, null, camera.matrix);
            sprite_batch.Draw(render_targets[1], Vector2.Zero, Color.White);
            sprite_batch.End();

            device.SetRenderTarget(render_targets[1]);
            device.Clear(Color.Transparent);
            // Units
            draw_units(sprite_batch, true);
            // Active units
            draw_active_units(sprite_batch);
            // Unit tone
            device.SetRenderTarget(render_targets[0]);
            if (map_shader != null)
            {
                map_shader.CurrentTechnique = map_shader.Techniques["Tone"];
                map_shader.Parameters["tone"].SetValue(Global.game_state.screen_tone.to_vector_4(Config.UNIT_TONE_PERCENT));
            }
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, map_shader);
            sprite_batch.Draw(render_targets[1], Vector2.Zero, Color.White);
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            draw_formation_change2(sprite_batch);
            draw_formation_change1(sprite_batch);
            sprite_batch.End();
        }

        protected Rectangle battle_transition_rect()
        {
            if (!Global.game_state.transition_to_battle)
                return new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Vector2 center = Vector2.Zero;
            // Promotion
            if (Global.game_system.Class_Changer != -1)
            {
                Game_Unit battler_1 = Global.game_map.units[Global.game_system.Class_Changer];
                center = (battler_1.loc * TILE_SIZE) + Vector2.One * (TILE_SIZE / 2);
            }
            // Dance
            else if (Global.game_state.dancer_id != -1)
            {
                Game_Unit battler_1 = Global.game_map.units[Global.game_state.dancer_id];
                Game_Unit battler_2 = Global.game_map.units[Global.game_state.dance_target_id];
                center = ((battler_1.loc + battler_2.loc) * TILE_SIZE / 2) + Vector2.One * (TILE_SIZE / 2);
            }
            // Battle
            else if (Global.game_state.battler_1_id != -1)
            {
                Game_Unit battler_1 = Global.game_map.units[Global.game_state.battler_1_id];
                if (Global.game_state.battler_2_id != -1)
                {
                    Game_Unit battler_2 = Global.game_map.units[Global.game_state.battler_2_id];
                    center = ((battler_1.loc + battler_2.loc) * TILE_SIZE / 2) + Vector2.One * (TILE_SIZE / 2);
                }
                else
                    center = (battler_1.loc * TILE_SIZE) + Vector2.One * (TILE_SIZE / 2);
            }
            center -= Global.game_map.display_loc;
            center = Vector2.Transform(center, camera.matrix);
            int width = Global.game_state.battle_transition_timer * 24;
            int height = Global.game_state.battle_transition_timer * 15;
            Rectangle result = new Rectangle((int)center.X - width / 2, (int)center.Y - height / 2, width, height);
            result.X += (int)Stereoscopic_Graphic_Object.graphic_draw_offset(Config.MAP_MAP_DEPTH).X;
            return fix_rect_to_screen(result);
        }

        public static Rectangle fix_rect_to_screen(Rectangle rect)
        {
            // Fix x/width
            if (rect.X < 0)
            {
                rect.Width = Math.Max(0, rect.Width + rect.X);
                rect.X = 0;
            }
            if (rect.X + rect.Width > Config.WINDOW_WIDTH)
            {
                rect.X = Math.Min(rect.X, Config.WINDOW_WIDTH);
                rect.Width = Config.WINDOW_WIDTH - rect.X;
            }
            // Fix y/height
            if (rect.Y < 0)
            {
                rect.Height = Math.Max(0, rect.Height + rect.Y);
                rect.Y = 0;
            }
            if (rect.Y + rect.Height > Config.WINDOW_HEIGHT)
            {
                rect.Y = Math.Min(rect.Y, Config.WINDOW_HEIGHT);
                rect.Height = Config.WINDOW_HEIGHT - rect.Y;
            }
            return rect;
        }

        protected void draw_battle_transition(SpriteBatch sprite_batch)
        {
            // Because the center area of the transition gets drawn on top at full value, this is probably unnecessary //Yeti
            // Just have the render target draw of the background be the one to darken the screen
            if (Global.game_state.transition_to_battle)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                Battle_Transition.draw(sprite_batch, battle_transition_rect(),
                    new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT));
                sprite_batch.End();
            }
        }

        protected void draw_chapter_transition(SpriteBatch sprite_batch)
        {
            if (Chapter_Transition != null)
            {
                Chapter_Transition.draw(sprite_batch);
            }
        }

        #region Draw Map

        /// <summary>
        /// Draws the map onto render_targets[0].
        /// </summary>
        /// <param name="sprite_batch">The active SpriteBatch</param>
        /// <param name="device">The game's GraphicsDevuce object</param>
        /// <param name="render_targets">A of render targets to draw on</param>
        /// <param name="roof">If true, draws map tiles that are fading out and are "above" units under them; otherwise draws the base map.</param>
        protected void draw_map(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets, bool roof = false)
        {
            // Draw fog tiles to render target 1
            device.SetRenderTarget(render_targets[1]);
            device.Clear(Color.Transparent);
            draw_raw_map(sprite_batch, false, fog: true, roof: roof);

            // Copy fog tiles with the fog effect to render target 0
            device.SetRenderTarget(render_targets[0]);
            device.Clear(Color.Transparent);

            Color fog_color;
            Effect map_shader;
            fog_effects(out fog_color, out map_shader);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, null, null, map_shader);
            sprite_batch.Draw(render_targets[1], Vector2.Zero, fog_color);
            sprite_batch.End();

            // Draw regular tiles on top of fog tiles
            draw_raw_map(sprite_batch, false, fog: false, roof: roof);

            // Draw map to render target 1 with map alpha effects applied
            device.SetRenderTarget(render_targets[1]);
            device.Clear(Color.Transparent);

            Effect alpha_shader = Global.effect_shader();
            if (Current_Map_Alpha != null)
            {
                sprite_batch.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
                if (alpha_shader != null)
                {
                    alpha_shader.CurrentTechnique = alpha_shader.Techniques["Map_Lighting"];
#if __ANDROID__
                    // There has to be a way to do this for both
                    alpha_shader.Parameters["Map_Alpha"].SetValue(Current_Map_Alpha);
#else
                    sprite_batch.GraphicsDevice.Textures[1] = Current_Map_Alpha;
#endif
                }
            }
            // Darken screen for spells if needed
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, null, null, alpha_shader);
            sprite_batch.Draw(render_targets[0], Vector2.Zero,
                new Color(Map_Spell_Darken, Map_Spell_Darken, Map_Spell_Darken, 255));
            sprite_batch.End();
#if __ANDROID__
            // There has to be a way to do this for both
            if (alpha_shader != null)
                alpha_shader.Parameters["Map_Alpha"].SetValue((Texture2D)null);
#else
            sprite_batch.GraphicsDevice.Textures[1] = null;
#endif

            // Draw map to render target 0 with map tone applied
            device.SetRenderTarget(render_targets[0]);
            device.Clear(roof ? Color.Transparent : Color.Black);
            if (Global.game_map.width <= 0)
                return;

            map_shader = Global.effect_shader();
            if (map_shader != null)
            {
                map_shader.CurrentTechnique = map_shader.Techniques["Tone"];
                map_shader.Parameters["tone"].SetValue(Global.game_state.screen_tone.to_vector_4(1.0f));
            }
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, null, null, map_shader);
            sprite_batch.Draw(render_targets[1], Vector2.Zero, Color.White);
            sprite_batch.End();
        }

        private void fog_effects(out Color fog_color, out Effect map_shader)
        {
            fog_color = Color.White;
            map_shader = Global.effect_shader();
            if (map_shader != null)
            {
                map_shader.CurrentTechnique = map_shader.Techniques["Tone"];
                // make this a game_map variable that can be set through events //Yeti
                //map_shader.Parameters["tone"].SetValue(new Tone(40, 40, 40, 72).to_vector_4()); //bright
                //map_shader.Parameters["tone"].SetValue(new Tone(-40, -40, -40, 72).to_vector_4()); //dark
                map_shader.Parameters["tone"].SetValue(Global.game_map.fow_color.to_vector_4());
            }
            else
                fog_color = new Color(168, 168, 168, 255);
        }

        protected void draw_raw_map(SpriteBatch sprite_batch, bool rotated, bool fog = false, bool roof = false)
        {
            // Draws the map tiles
            lock (Global.game_map.fow_visibility)
            {
                if (!roof)
                    @Tilemap.draw(sprite_batch, rotated, fog);
                @Tilemap.draw_changing_tiles(sprite_batch, rotated, fog, roof);
            }
        }

        protected void draw_map_grid(SpriteBatch sprite_batch, bool rotated)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            // Draws the grid over the map
            @Tilemap.draw_grid(sprite_batch, rotated);
            // Draws the outer border of the grid
            @Tilemap.draw_grid_outline(sprite_batch, rotated);
            sprite_batch.End();
        }
        #endregion

        #region Draw Ranges
        protected void draw_ranges(SpriteBatch sprite_batch)
        {
            if (Global.game_map.is_enemy_range_visible())
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                // should probably use lock instead? //Yeti
                Global.game_map.drawing_enemy_range = true;
                if (Global.game_map.all_enemy_displayed_staff.Count > 0 || Global.game_map.enemy_displayed_staff.Count > 0 ||
                        Global.game_map.all_enemy_displayed_attack.Count > 0 || Global.game_map.enemy_displayed_attack.Count > 0)
                    draw_enemy_ranges(sprite_batch);
                Global.game_map.drawing_enemy_range = false;

                if (Global.game_system.Selected_Unit_Id == -1)
                    draw_auras(sprite_batch);

                sprite_batch.End();
            }

            if (Global.game_map.move_range_visible)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                if (Global.game_map.move_range.Count > 0 || Global.game_map.attack_range.Count > 0 ||
                        Global.game_map.staff_range.Count > 0 || Global.game_map.talk_range.Count > 0)
                    draw_basic_ranges(sprite_batch);
                sprite_batch.End();
            }
            draw_menu_ranges(sprite_batch);
        }

        protected void draw_enemy_ranges(SpriteBatch sprite_batch)
        {
            int x = Enemy_Attack_Range_Texture.Width / 4;
            int y = Enemy_Attack_Range_Texture.Height / 4;
            Vector2 display_loc = Global.game_map.display_loc;
            // All Enemy Staff Range
            //@Yeti enumeration modified errors
            foreach (KeyValuePair<Vector2, int> pair in Global.game_map.all_enemy_displayed_staff)
            {
                sprite_batch.Draw(All_Enemy_Staff_Range_Texture, pair.Key * TILE_SIZE + move_range_draw_vector() - display_loc,
                    new Rectangle((pair.Value % 4) * x, (pair.Value / 4) * y, x, y), Color.White);
            }
            // Enemy Staff Range
            foreach (KeyValuePair<Vector2, int> pair in Global.game_map.enemy_displayed_staff)
            {
                sprite_batch.Draw(Enemy_Staff_Range_Texture, pair.Key * TILE_SIZE + move_range_draw_vector() - display_loc,
                    new Rectangle((pair.Value % 4) * x, (pair.Value / 4) * y, x, y), Color.White);
            }
            // All Enemy Attack Range
            foreach (KeyValuePair<Vector2, int> pair in Global.game_map.all_enemy_displayed_attack)
            {
                sprite_batch.Draw(All_Enemy_Attack_Range_Texture, pair.Key * TILE_SIZE + move_range_draw_vector() - display_loc,
                    new Rectangle((pair.Value % 4) * x, (pair.Value / 4) * y, x, y), Color.White);
            }
            // Enemy Attack Range
            foreach (KeyValuePair<Vector2, int> pair in Global.game_map.enemy_displayed_attack)
            {
                sprite_batch.Draw(Enemy_Attack_Range_Texture, pair.Key * TILE_SIZE + move_range_draw_vector() - display_loc,
                    new Rectangle((pair.Value % 4) * x, (pair.Value / 4) * y, x, y), Color.White);
            }
        }

        protected void draw_auras(SpriteBatch sprite_batch)
        {
            List<int> aura_units = new List<int>();
            foreach (KeyValuePair<int, Game_Unit> pair in Global.game_map.units)
                if (pair.Value.has_aura())
                    aura_units.Add(pair.Key);
            if (aura_units.Count <= 0)
                return;

            HashSet<Vector2> already_used = new HashSet<Vector2>();
            foreach (string auraKey in Game_Unit.AURA_COLOR_ORDER)
            {
                // A list of locations that are colored by this aura already, if multiple units with the aura are close together
                already_used.Clear();
                foreach (int unit_id in aura_units)
                {
                    string unitAuraKey = Global.game_map.units[unit_id].visible_aura();
                    if (unitAuraKey == auraKey)
                    {
                        draw_aura(sprite_batch, unit_id, already_used);
                    }
                }
            }
            // Draw auras for units that somehow don't have their aura ordered
            foreach (int unit_id in aura_units)
                draw_aura(sprite_batch, unit_id, already_used);
        }

        private void draw_aura(SpriteBatch sprite_batch, int unit_id, HashSet<Vector2> already_used)
        {
            Vector2 display_loc = Global.game_map.display_loc;
            UnitAura aura = Game_Unit.SKILL_AURAS[
                Global.game_map.units[unit_id].visible_aura()];
            Color tint = Color.Lerp(aura.PrimaryColor, aura.SecondaryColor,
                Global.game_system.play_time_sine_wave(aura.ColorPeriod));
            // Gets the area around the unit for the aura
            HashSet<Vector2> locs = Pathfind.get_range_around(
                new HashSet<Vector2> { Global.game_map.units[unit_id].loc }, Global.game_map.units[unit_id].aura_radius(), 0, false);
            foreach (Vector2 loc in locs)
            {
                if (Global.game_map.fow && !Global.game_map.units[unit_id].is_player_allied &&
                        !Global.game_map.fow_visibility[Constants.Team.PLAYER_TEAM].Contains(loc))
                    continue;
                if (already_used.Contains(loc))
                    continue;
                already_used.Add(loc);
                //sprite_batch.Draw(White_Square, loc * 16 + move_range_draw_vector() - display_loc,
                //    new Rectangle(0, 0, 16, 16), tint); //Debug
                sprite_batch.Draw(White_Square, loc * TILE_SIZE + move_range_draw_vector() - display_loc,
                    new Rectangle(0, 0, 16, 16), tint, 0f, Vector2.Zero, TILE_SIZE / 16f, SpriteEffects.None, 0f);
            }
        }

        protected void draw_basic_ranges(SpriteBatch sprite_batch)
        {
            int opacity = Global.game_system.Selected_Unit_Id == -1 ?
                (Changing_Formation ? Constants.Map.PASSIVE_FORMATION_MOVE_RANGE_OPACITY : Constants.Map.PASSIVE_MOVE_RANGE_OPACITY) :
                (Changing_Formation ? Constants.Map.ACTIVE_FORMATION_MOVE_RANGE_OPACITY : Constants.Map.ACTIVE_MOVE_RANGE_OPACITY);
            Color color = new Color(opacity, opacity, opacity, opacity);

            Global.game_map.draw_basic_ranges(sprite_batch, Move_Range_Texture, Attack_Range_Texture, Staff_Range_Texture, Talk_Range_Texture,
                move_range_draw_vector(), color);
        }
        #endregion

        protected void draw_arrow(SpriteBatch sprite_batch)
        {
            if (Global.game_map.move_range_visible)
            {
                int width = Move_Arrow_Texture.Width / 4;
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                foreach (Move_Arrow_Data loc in Global.game_map.move_arrow)
                {
                    sprite_batch.Draw(Move_Arrow_Texture,
                        new Vector2(loc.X, loc.Y) * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc,
                        new Rectangle(((loc.Frame - 1) % 4) * width,
                            ((loc.Frame - 1) / 4) * width + (Global.game_map.get_selected_unit().team - 1) * (width * 4),
                            width, width), Color.White);
                }
                sprite_batch.End();
            }
        }

        #region Draw Units
        /// <summary>
        /// Draws inactive units onto render_targets[1], then copies them to render_targets[0] with tone.
        /// </summary>
        /// <param name="sprite_batch">The active SpriteBatch</param>
        /// <param name="device">The game's GraphicsDevuce object</param>
        /// <param name="render_targets">A of render targets to draw on</param>
        protected virtual void draw_units(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets,
            bool roof, HashSet<Vector2> roof_tiles)
        {
            // Units hidden
            if (Global.game_map.UnitsHidden)
                return;

            // Draw units on render target 1, then copy them with tone on render target 0
            device.SetRenderTarget(render_targets[1]);
            device.Clear(Color.Transparent);
            draw_units(sprite_batch, roof: roof, roof_tiles: roof_tiles);

            // Unit tone
            device.SetRenderTarget(render_targets[0]);
            Effect map_shader = Global.effect_shader();
            if (map_shader != null)
            {
                map_shader.CurrentTechnique = map_shader.Techniques["Tone"];
                map_shader.Parameters["tone"].SetValue(Global.game_state.screen_tone.to_vector_4(Config.UNIT_TONE_PERCENT));
            }
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, map_shader);
            sprite_batch.Draw(render_targets[1], Vector2.Zero, Color.White);
            sprite_batch.End();
        }

        protected virtual void draw_units(SpriteBatch sprite_batch, bool rotated = false, bool roof = false, HashSet<Vector2> roof_tiles = null)
        {
            // Units hidden
            if (Global.game_map.UnitsHidden)
                return;

            // Units to skip and draw at the end
            get_deferred_unit_ids();
            List<int> units = units_to_draw(DeferredUnitIds, rotated);
            if (roof_tiles != null && roof_tiles.Count > 0)
                units = units.Where(x => roof_tiles.Contains(Global.game_map.units[x].loc) == roof).ToList();

            draw_map_objects(sprite_batch, false, roof, roof_tiles);
            draw_map_objects(sprite_batch, true, roof, roof_tiles);
            draw_idle_units(sprite_batch, units);
            //draw_unit_status(sprite_batch, units);
            //draw_unit_icons(sprite_batch, units);
        }

        protected List<int> units_to_draw(
            List<int> deferredUnits, bool rotated, bool ignoreBounds = false)
        {
            // Units to skip and draw at the end
            List<int> units = new List<int>(Global.game_map.units.Count);
            Rectangle area = Tilemap.view_area(rotated);
            area.X = area.X - 1;
            area.Y = area.Y - 1;
            area.Width = area.Width + 2;
            area.Height = area.Height + 2;
            foreach (Game_Unit unit in Global.game_map.units.Values)
            {
                if (deferredUnits.Contains(unit.id))
                    continue;

                if (ignoreBounds || area.Contains((int)unit.loc.X, (int)unit.loc.Y))
                {
                    units.Add(unit.id);
                }
            }
            return sort_units(units);
        }

        protected void draw_map_objects(SpriteBatch sprite_batch, bool fog, bool roof = false, HashSet<Vector2> roof_tiles = null)
        {
            // Begin sprite batch
            sprite_batch.GraphicsDevice.ScissorRectangle = battle_transition_rect();
            if (sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0 && sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0)
            {
                Color fog_color = Color.White;
                Effect map_shader = null;
                if (fog)
                    fog_effects(out fog_color, out map_shader);

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Unit_Transition_State,
                    map_shader);
                // Draw siege engines
                IEnumerable<int> siege_keys = Global.game_map.siege_engines.Keys;
                if (roof_tiles != null && roof_tiles.Count > 0)
                    siege_keys = siege_keys.Where(x => roof_tiles.Contains(Global.game_map.siege_engines[x].loc) == roof);
                foreach (int id in siege_keys)
                {
                    Siege_Engine siege = Global.game_map.siege_engines[id];
                    Game_Unit unit = siege.Unit;
                    bool tile_visible = !Global.game_map.fow ||
                        Global.game_map.fow_visibility[Constants.Team.PLAYER_TEAM]
                            .Contains(siege.loc);
                    if (!tile_visible ^ fog)
                        continue;
                    if (tile_visible)
                    {
                        // If a unit is already on this siege engine and thus displaying a modified sprite, skip drawing the empty sprite
                        if (unit != null && !unit.sprite_moving && !unit.battling && unit.is_on_siege())
                            continue;
                    }
                    Map_Sprites[id].tint = get_unit_tint(siege.loc);
                    Map_Sprites[id].draw(sprite_batch, Global.game_map.display_loc, camera.matrix);
                }
                // Draw Light Runes
                var light_runes = Global.game_map.enumerate_light_runes();
                if (roof_tiles != null && roof_tiles.Count > 0)
                    light_runes = light_runes.Where(x => roof_tiles.Contains(x.loc) == roof);
                foreach (var rune in light_runes)
                {
                    bool tile_visible = !Global.game_map.fow ||
                        Global.game_map.fow_visibility[Constants.Team.PLAYER_TEAM]
                            .Contains(rune.loc);
                    if (!tile_visible ^ fog)
                        continue;
                    Map_Sprites[rune.id].draw(sprite_batch, Global.game_map.display_loc, camera.matrix);
                }
                // End sprite batch
                sprite_batch.End();
            }
        }

        protected void draw_idle_units(SpriteBatch sprite_batch, List<int> units)
        {
            // Begin map sprite batch
            sprite_batch.GraphicsDevice.ScissorRectangle = battle_transition_rect();
            // This wasn't working for some reason, 0 width and height made it ignore the scissor
            if (sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0 && sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0)
            {
                Effect map_shader = Global.effect_shader();
                foreach (int id in units)
                {
                    if (!Global.game_map.units[id].visible_by())
                        continue;
                    // Adjusts unit brightness by map alpha
                    Map_Sprites[id].tint = get_unit_tint(Global.game_map.units[id].loc);
                    // Tints the unit red if it has its attack range marked
                    if (Global.game_map.range_enemies.Contains(id) &&
                        Constants.Team.PLAYABLE_TEAMS.Contains(Global.game_state.team_turn))
                    {
                        enemy_range_unit_tint(id);
                    }

                    Effect unit_shader = null;
                    // Blinks the unit outline if their mastery is ready
                    if (Global.game_system.unit_blink &&
                        Global.game_map.units[id].is_blinking &&
                        !Global.game_map.is_off_map(Global.game_map.units[id].loc) &&
                        Global.Map_Sprite_Colors.data.ContainsKey(new Color(64, 56, 56, 255)))
                    {
                        unit_shader = map_shader;
                        if (unit_shader != null)
                        {
                            unit_shader.CurrentTechnique = unit_shader.Techniques["Outline_Glow"];
                            unit_shader.Parameters["tone"].SetValue(
                                Global.Map_Sprite_Colors.data[
                                new Color(64, 56, 56, 255)][Global.game_map.units[id].ready ? Global.game_map.units[id].team : 0].ToVector4());
                            unit_shader.Parameters["color_shift"].SetValue(Global.game_map.units[id].blink_color.ToVector4());
                        }
                    }
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Unit_Transition_State, unit_shader);
                    Map_Sprites[id].draw(sprite_batch, Global.game_map.display_loc, camera.matrix);
                    sprite_batch.End();
                }
                // Draws hp gauges
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Unit_Transition_State);
                if (Global.game_state.hp_gauges_visible && !Global.game_system.is_interpreter_running)
                    foreach (int id in units)
                    {
                        if (!Global.game_map.units[id].visible_by() || Global.game_map.is_off_map(Global.game_map.units[id].loc))
                            continue;
                        Map_Sprites[id].draw_hp(sprite_batch, Global.game_map.display_loc - hp_gauge_draw_vector(), camera.matrix);
                    }
                // End sprite batch
                sprite_batch.End();
            }
        }

        protected void draw_unit_status(SpriteBatch sprite_batch, List<int> units)
        {
            draw_unit_status(sprite_batch, units, false);
        }
        protected void draw_unit_status(SpriteBatch sprite_batch, List<int> units, bool active)
        {
            // Draws status effects
            sprite_batch.GraphicsDevice.ScissorRectangle = battle_transition_rect();
            if (sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0 && sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Unit_Transition_State);
                foreach (int id in units)
                {
                    // Among active units, the ones that draw status icons include: dance target
                    if (active && (id != Global.game_state.dance_target_id))
                        continue;
                    var unit = Global.game_map.units[id];
                    // If not visible to the player, skip
                    if (!unit.visible_by() || unit.is_rescued)
                        continue;
                    if (!unit.sprite_moving)
                        Map_Sprites[id].draw_status(sprite_batch, Status_Sprites, Global.game_map.display_loc, camera.matrix);
                }
                // End sprite batch
                sprite_batch.End();
            }
        }

        protected void draw_unit_icons(SpriteBatch sprite_batch, List<int> units)
        {
            // Draws icons
            if (Global.game_map.icons_visible)
            {
                // Begin map sprite batch
                sprite_batch.GraphicsDevice.ScissorRectangle = battle_transition_rect();
                if (sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0 && sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0)
                {
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Unit_Transition_State);
                    // Draws siege uses icons
                    if (Global.game_system.Selected_Unit_Id == -1 && Global.game_state.is_map_ready(true))
                    {
                        foreach (int id in Global.game_map.siege_engines.Keys)
                        {
                            Siege_Engine siege = Global.game_map.siege_engines[id];
                            Game_Unit unit = siege.Unit;
                            if (unit != null && unit.highlighted)
                                //if (unit != null && !unit.sprite_moving && !unit.battling && unit.is_on_siege())
                                continue;
                            if (siege.has_full_ammo)
                                continue;
                            sprite_batch.Draw(Siege_Ammo_Icon,
                                Vector2.Transform(siege.pixel_loc + rescue_icon_draw_vector() +
                                new Vector2(TILE_SIZE / 2, TILE_SIZE) - Global.game_map.display_loc, camera.matrix),
                                new Rectangle(8 * siege.item.Uses, 0, 8, 8),
                                !siege.ammo_count_greyed ? Color.White : new Color(128, 128, 128), 0f,
                                new Vector2(8, 8), 1f, SpriteEffects.None, 0f);
                        }
                    }
                    // Draws rescue/boss/danger icons
                    foreach (int id in units)
                    {
                        if (!Global.game_map.units[id].visible_by())
                            continue;
                        Game_Unit unit = Global.game_map.units[id];
                        //if (unit.is_rescuing && !unit.highlighted && unit.is_ally)
                        bool rescuing = unit.is_rescuing && !unit.highlighted; //Multi
                        bool boss = unit.boss;
                        bool dangerous = false;
                        if (Config.DANGEROUS_UNIT_WARNING)
                            dangerous = Global.game_system.Selected_Unit_Id != -1 &&
                                unit.is_attackable_team(Global.game_map.get_selected_unit()) &&
                                unit.any_effective_weapons(Global.game_map.get_selected_unit());

                        UnitIcons icon = displayed_unit_icon(rescuing, boss, dangerous);

                        if (icon != UnitIcons.None)
                        {
                            Texture2D unit_icon_texture;
                            Rectangle unit_icon_rect;
                            switch (icon)
                            {
                                case UnitIcons.Rescuing:
                                default:
                                    unit_icon_texture = Rescue_Icon;
                                    unit_icon_rect = new Rectangle(
                                        (Global.game_map.units[unit.rescuing].team - 1) *
                                            (Rescue_Icon.Width / Constants.Team.NUM_TEAMS),
                                        0,
                                        Rescue_Icon.Width / Constants.Team.NUM_TEAMS,
                                        Rescue_Icon.Height);
                                    break;
                                case UnitIcons.Boss:
                                    unit_icon_texture = Boss_Icon;
                                    unit_icon_rect = Boss_Icon.Bounds;
                                    break;
                                case UnitIcons.Dangerous:
                                    unit_icon_texture = DangerIcon;
                                    unit_icon_rect = new Rectangle(
                                        (unit.team - 1) * (DangerIcon.Width /
                                            Constants.Team.NUM_TEAMS),
                                        0,
                                        DangerIcon.Width / Constants.Team.NUM_TEAMS,
                                        DangerIcon.Height);
                                    break;
                            }

                            sprite_batch.Draw(unit_icon_texture,
                                Vector2.Transform(unit.pixel_loc +
                                    rescue_icon_draw_vector() +
                                    new Vector2(TILE_SIZE / 2, TILE_SIZE) -
                                    Global.game_map.display_loc, camera.matrix),
                                unit_icon_rect,
                                Color.White, 0f, new Vector2(16, 24), 1f, SpriteEffects.None, 0f);
                        }
                    }
                    // End sprite batch
                    sprite_batch.End();
                }
            }
        }

        protected void draw_unit_icons_above_cursor(SpriteBatch spriteBatch, List<int> units)
        {
            // Draws icons
            if (Global.game_map.icons_visible)
            {
                // Begin map sprite batch
                spriteBatch.GraphicsDevice.ScissorRectangle = battle_transition_rect();
                if (spriteBatch.GraphicsDevice.ScissorRectangle.Width > 0 && spriteBatch.GraphicsDevice.ScissorRectangle.Width > 0)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Unit_Transition_State);

                    // Draws talk icons
                    // This was visible in item use, there might be other cases //Yeti
                    if (Global.game_system.Selected_Unit_Id != -1)
                        foreach (int id in units)
                        {
                            if (id == Global.game_system.Selected_Unit_Id || !Global.game_map.units[id].visible_by())
                                continue;

                            Game_Unit selected_unit = Global.game_map.units[Global.game_system.Selected_Unit_Id];
                            Game_Unit unit = Global.game_map.units[id];
                            // If unit selected with move range up
                            if (Global.game_map.move_range_visible && Global.game_state.is_map_ready() &&
                                // If the selected unit can talk to this unit
                                Global.game_state.can_talk(Global.game_system.Selected_Unit_Id, id) ||
                                // If this unit can talk to the selected unit, and this unit is a PC and the selected is not
                                (!selected_unit.is_active_player_team &&
                                unit.is_active_player_team && Global.game_state.can_talk(id, Global.game_system.Selected_Unit_Id)))
                            {
                                // If the unit isn't currently highlit
                                if (!unit.highlighted) //Multi
                                {
                                    Vector2 loc = unit.pixel_loc;
                                    DrawTalkBubble(spriteBatch, loc);
                                }
                            }

                            // If the unit has supported the selected unit,
                            // and combat targeting or preparations
                            else if ((this.combat_target_window_up || Global.game_system.preparations) &&
                                (selected_unit.actor.bond == unit.actor.id ||
                                selected_unit.actor.get_support_level(unit.actor.id) > 0))
                            {
                                int support_level = selected_unit.actor.bond == unit.actor.id ?
                                    Constants.Support.BOND_SUPPORT_RANK :
                                    selected_unit.actor.get_support_level(unit.actor.id);
                                Vector2 support_bubble_loc = unit.pixel_loc + rescue_icon_draw_vector() +
                                        new Vector2(TILE_SIZE / 2, TILE_SIZE) - Global.game_map.display_loc;

                                Color tint = Color.White;
                                if (Global.game_map.unit_distance(selected_unit.id, unit.id) > Constants.Support.SUPPORT_RANGE ||
                                        (unit.is_rescued && unit.rescued != selected_unit.id))
                                    tint = new Color(176, 176, 176, 255);

                                spriteBatch.Draw(SupportIcons,
                                    Vector2.Transform(support_bubble_loc, camera.matrix),
                                    new Rectangle((support_level - 1) * 32, 0, 32, 32), tint, 0f, new Vector2(16, 24),
                                    1f, SpriteEffects.None, 0f);
                            }
                        }

                    // End sprite batch
                    spriteBatch.End();
                }
            }
        }

        private void DrawTalkBubble(SpriteBatch spriteBatch, Vector2 loc)
        {
            Vector2 talkBubbleLoc = loc + rescue_icon_draw_vector() +
                    new Vector2(TILE_SIZE / 2, TILE_SIZE) - Global.game_map.display_loc;

            Vector2 actualLoc = talkBubbleLoc;
            actualLoc.X = MathHelper.Clamp(actualLoc.X,
                TILE_SIZE,
                Config.WINDOW_WIDTH - TILE_SIZE * 5 / 4);
            actualLoc.Y = MathHelper.Clamp(actualLoc.Y,
                TILE_SIZE * 3 / 2,
                Config.WINDOW_HEIGHT - TILE_SIZE / 4);

            Color tint = Color.White;
            if (talkBubbleLoc != actualLoc)
            {
                tint = new Color(208, 208, 208, 255);
            }

            int frame = 0;
            if (talkBubbleLoc != actualLoc)
            {
                if (Math.Abs(talkBubbleLoc.X - actualLoc.X) >
                    Math.Abs(talkBubbleLoc.Y - actualLoc.Y))
                {
                    if (talkBubbleLoc.X < actualLoc.X)
                        frame = 4;
                    else
                        frame = 6;
                }
                else
                {
                    if (talkBubbleLoc.Y > actualLoc.Y)
                        frame = 2;
                    else
                        frame = 8;
                }
            }

            int cellSize = Talk_Icon.Bounds.Height;
            Rectangle srcRect = new Rectangle(
                cellSize * (frame / 2), 0, cellSize, cellSize);
            spriteBatch.Draw(Talk_Icon,
                Vector2.Transform(actualLoc, camera.matrix),
                srcRect, tint, 0f, new Vector2(16, 24),
                1f, SpriteEffects.None, 0f);
        }

        private UnitIcons displayed_unit_icon(bool rescuing, bool boss, bool dangerous)
        {
            UnitIcons icon_flags = UnitIcons.None;
            if (rescuing)
                icon_flags |= UnitIcons.Rescuing;
            if (boss)
                icon_flags |= UnitIcons.Boss;
            if (dangerous)
                icon_flags |= UnitIcons.Dangerous;

            int icon_count = 0;
            for (int i = 1; i > 0 && i < int.MaxValue; i *= 2)
                if (icon_flags.HasEnumFlag((UnitIcons)i))
                    icon_count++;

            if (icon_count == 0)
                return UnitIcons.None;

            int index = Global.game_map.icon_loops % icon_count;

            int j = 0;
            int k = 1;
            do
            {
                if (icon_flags.HasEnumFlag((UnitIcons)k))
                {
                    if (j == index)
                        return (UnitIcons)k;
                    j++;
                }
                k *= 2;
            } while (k > 0 && k < int.MaxValue);

            return UnitIcons.None;
        }

        private void draw_unit_support_gain(SpriteBatch spriteBatch)
        {
            if (Global.game_state.support_gain_active)
            {
                spriteBatch.GraphicsDevice.ScissorRectangle = battle_transition_rect();
                if (spriteBatch.GraphicsDevice.ScissorRectangle.Width > 0 && spriteBatch.GraphicsDevice.ScissorRectangle.Width > 0)
                {
                    // Normal support gains
                    var parameters = Global.game_state.SupportGainGfx;
                    DrawSupportGains(spriteBatch, parameters, Global.game_state.SupportGainIds);

                    // Ready support gains
                    parameters = Global.game_state.SupportGainReadyGfx;
                    DrawSupportGains(spriteBatch, parameters, Global.game_state.SupportGainReadyIds);
                }
            }
        }

        private void DrawSupportGains(SpriteBatch spriteBatch, SpriteParameters parameters, IEnumerable<int> unitIds)
        {
            if (!unitIds.Any())
                return;

            // Begin map sprite batch
            Effect map_shader = Global.effect_shader();
            if (map_shader != null)
            {
                map_shader.CurrentTechnique = map_shader.Techniques["Technique1"];
                map_shader.Parameters["color_shift"].SetValue(parameters.ColorShift.ToVector4());
                map_shader.Parameters["opacity"].SetValue(1f);
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Unit_Transition_State, map_shader);

            foreach (int id in unitIds
                .Where(x => Global.game_map.units.ContainsKey(x)))
            {
                if (!Global.game_map.units[id].visible_by())
                    continue;
                Game_Unit unit = Global.game_map.units[id];

                Vector2 support_bubble_loc = unit.pixel_loc + rescue_icon_draw_vector() +
                        new Vector2(TILE_SIZE / 2, -TILE_SIZE * 3 / 8) - Global.game_map.display_loc;

                spriteBatch.Draw(SupportIcons,
                    Vector2.Transform(support_bubble_loc + parameters.Location, camera.matrix),
                    parameters.SrcRect, parameters.Tint, 0f,
                    new Vector2(4, 4) - parameters.Offset,
                    parameters.Scale, SpriteEffects.None, 0f);
            }

            // End sprite batch
            spriteBatch.End();
        }

        protected virtual void draw_active_units(SpriteBatch sprite_batch)
        {
            // Units hidden
            if (Global.game_map.UnitsHidden)
                return;

            // Units to skip and draw at the end
            get_deferred_unit_ids();
            DeferredUnitIds = sort_units(DeferredUnitIds);
            // Draw deferred units last
            Effect map_shader = Global.effect_shader();
            foreach (int id in DeferredUnitIds)
            {
                Game_Unit unit = Global.game_map.units[id];
                if (!unit.visible_by())
                    continue;

                // Maybe don't do this, always draw the units anyway since the hit is meager and it simplifies //Debug
                sprite_batch.GraphicsDevice.ScissorRectangle = battle_transition_rect();
                if (sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0 && sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0)
                {
                    // Adjusts unit brightness by map alpha
                    Map_Sprites[id].tint = get_unit_tint(unit.loc);
                    if (Global.game_map.range_enemies.Contains(id) && Global.game_map.is_enemy_range_visible())
                    {
                        enemy_range_unit_tint(id);
                    }

                    if (map_shader != null)
                    {
                        if (Global.game_state.new_turn_unit_id == id && Status_Heal != null)
                            map_shader.CurrentTechnique = map_shader.Techniques["Outline_Glow"];
                        else
                            map_shader.CurrentTechnique = map_shader.Techniques["Technique1"];
                    }

                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Unit_Transition_State, map_shader);
                    if (map_shader != null)
                    {
                        if (Global.game_state.new_turn_unit_id == id && Status_Heal != null)
                            unit.set_new_turn_sprite_batch_effects(map_shader);
                        else
                            unit.set_sprite_batch_effects(map_shader);
                    }
                    Map_Sprites[id].draw(sprite_batch, Global.game_map.display_loc, camera.matrix);
                    sprite_batch.End();
                }
            }
            draw_unit_status(sprite_batch, DeferredUnitIds, true);
            if (map_shader != null)
            {
                // Status healing effect
                if (Global.game_state.new_turn_unit_id != -1 && Map_Sprites[Global.game_state.new_turn_unit_id].texture != null && Status_Heal != null)
                {
                    Character_Sprite new_turn_map_sprite = Map_Sprites[Global.game_state.new_turn_unit_id];
                    int width = new_turn_map_sprite.texture.Width;
                    int height = new_turn_map_sprite.texture.Height;

                    map_shader.CurrentTechnique = map_shader.Techniques["Mask"];
                    map_shader.Parameters["mask_rect"].SetValue(new_turn_map_sprite.src_rect.to_vector4() /
                        new Vector4(width, height,
                        width, height));
                    Vector2 mask_size_ratio = new Vector2(Status_Heal.src_rect.Width / (float)width,
                        Status_Heal.src_rect.Height / (float)height);
                    //Vector2 blug = Vector2.One / mask_size_ratio; //Debug
                    //blug = new Vector2(
                    //    (float)Math.Pow(1 / mask_size_ratio.X, 1),
                    //    (float)Math.Pow(1 / mask_size_ratio.Y, 1));
                    map_shader.Parameters["mask_size_ratio"].SetValue(Vector2.One / mask_size_ratio);
                    Vector2 sprite_offset = (new_turn_map_sprite.offset +
                        new Vector2(new_turn_map_sprite.src_rect.X, new_turn_map_sprite.src_rect.Y)) /
                        new Vector2(width, height);
                    Vector2 effect_offset = Status_Heal.offset /
                        new Vector2(Status_Heal.src_rect.Width, Status_Heal.src_rect.Height);
                    //blug = effect_offset * mask_size_ratio - sprite_offset;
                    map_shader.Parameters["alpha_offset"].SetValue(-(effect_offset * mask_size_ratio - sprite_offset));

#if __ANDROID__
                    // There has to be a way to do this for both
                    map_shader.Parameters["Map_Alpha"].SetValue(new_turn_map_sprite.texture);
#else
                    sprite_batch.GraphicsDevice.Textures[1] = new_turn_map_sprite.texture;
#endif
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Unit_Transition_State, map_shader);
                    Status_Heal.draw(sprite_batch, Global.game_map.display_loc, camera.matrix);
                    sprite_batch.End();
                }
            }
        }

        protected void get_deferred_unit_ids()
        {
            DeferredUnitIds.Clear();
            // Highlighted ally
            if (Global.game_temp.highlighted_unit_id != -1 && Global.game_map.get_highlighted_unit() != null)
                if (Global.game_map.units[Global.game_temp.highlighted_unit_id].is_ally && //Multi
                        !Global.game_map.units[Global.game_temp.highlighted_unit_id].unselectable)
                    DeferredUnitIds.Add(Global.game_temp.highlighted_unit_id);
            bool skip_selected = false;
            // Battlers
            if (Global.game_system.Battler_1_Id != -1)
            {
                DeferredUnitIds.Add(Global.game_system.Battler_1_Id);
                skip_selected = true;
            }
            if (Global.game_system.Battler_2_Id != -1)
                DeferredUnitIds.Add(Global.game_system.Battler_2_Id);
            foreach (int aoe_target_id in Global.game_state.aoe_targets)
                DeferredUnitIds.Add(aoe_target_id);
            if (Global.game_state.battler_1_id != -1)
            {
                DeferredUnitIds.Add(Global.game_state.battler_1_id);
                skip_selected = true;
            }
            if (Global.game_state.battler_2_id != -1)
                DeferredUnitIds.Add(Global.game_state.battler_2_id);
            // Stealer
            if (Global.game_state.stealer_id != -1)
            {
                DeferredUnitIds.Add(Global.game_state.stealer_id);
                skip_selected = true;
            }
            if (Global.game_state.steal_target_id != -1)
                DeferredUnitIds.Add(Global.game_state.steal_target_id);
            // Active AI unit
            if (Global.game_state.active_ai_unit != -1)
                DeferredUnitIds.Add(Global.game_state.active_ai_unit);
            // Item User
            if (Global.game_state.item_user != null)
            {
                DeferredUnitIds.Add(Global.game_state.item_user.id);
                skip_selected = true;
            }
            // Rescue
            if (Global.game_state.rescue_moving_unit != null)
            {
                DeferredUnitIds.Add(Global.game_state.rescue_moving_unit.id);
                skip_selected = true;
            }
            // Dancer
            if (Global.game_state.dancer_id != -1)
            {
                DeferredUnitIds.Add(Global.game_state.dancer_id);
                skip_selected = true;
            }
            if (Global.game_state.dance_target_id != -1)
                DeferredUnitIds.Add(Global.game_state.dance_target_id);
            // New Turn Unit
            if (Global.game_state.new_turn_unit_id != -1)
                DeferredUnitIds.Add(Global.game_state.new_turn_unit_id);
            // Selected unit
            if (!skip_selected && Global.game_system.Selected_Unit_Id != -1)
                DeferredUnitIds.Add(Global.game_system.Selected_Unit_Id);
            // Dying units
                foreach (int dying_id in Global.game_map.dying_units)
                    DeferredUnitIds.Add(dying_id);

            DeferredUnitIds.RemoveAll(x => !Global.game_map.units.ContainsKey(x));
            //deferred_units = deferred_units.Intersect(Global.game_map.units.Keys).ToList(); //Debug
        }

        protected List<int> sort_units(List<int> units)
        {
            units.Sort(delegate(int a, int b)
            {
                Game_Unit unit1 = Global.game_map.units[a]; Game_Unit unit2 = Global.game_map.units[b];
                Vector2 loc1 = unit1.real_loc; Vector2 loc2 = unit2.real_loc;
                return (loc1.Y == loc2.Y ? unit2.team - unit1.team : (int)(loc1.Y - loc2.Y));
            });
            return units;
        }

        private void enemy_range_unit_tint(int id)
        {
            Map_Sprites[id].tint = new Color(
                Map_Sprites[id].tint.R,
                (byte)(Map_Sprites[id].tint.G *
                    Constants.Map.ENEMY_RANGE_TINT_MULT),
                (byte)(Map_Sprites[id].tint.B *
                    Constants.Map.ENEMY_RANGE_TINT_MULT),
                Map_Sprites[id].tint.A);
        }
        #endregion

        private void draw_effects(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            // Flares
            float flare_opacity = 0.33f;
            if (Global.game_map.icon_loops % 3 == 0)
            {
                float flare_timer = Global.game_map.icon_timer;
                if (flare_timer > 0.5f)
                    flare_timer = 1 - flare_timer;
                flare_opacity += 0.33f * (flare_timer * 2);
            }

            foreach (var flare in Global.game_map.flares)
            {
                float remaining = (float)Math.Sqrt(flare.remaining());
                Vector2 loc = flare.loc + new Vector2(0.5f, 0.25f - 0.125f * remaining) - (Global.game_map.display_loc / TILE_SIZE);
                loc.X -= (Config.WINDOW_WIDTH / (float)TILE_SIZE) / 2;
                loc.X *= (1 + remaining / 20f);
                loc.X += (Config.WINDOW_WIDTH / (float)TILE_SIZE) / 2;
                loc *= TILE_SIZE;
                sprite_batch.Draw(Flare, loc, Flare.Bounds,
                    new Color(flare_opacity, flare_opacity, flare_opacity, 0) * remaining, 0f,
                    new Vector2(Flare.Width, Flare.Height) / 2, Vector2.One * 2f * remaining, SpriteEffects.None, 0f);
            }
            // Unit Effect
            if (Unit_Map_Effect != null)
                Unit_Map_Effect.draw(sprite_batch, Global.game_map.display_loc, camera.matrix);
            sprite_batch.End();
        }

        protected void draw_formation_change1(SpriteBatch sprite_batch)
        {
            if ((Global.game_system.preparations && Changing_Formation)
#if !MONOGAME && DEBUG
                || (Global.game_state.moving_editor_unit)
#endif
                )
            {
                if (Global.game_system.Selected_Unit_Id > -1)
                {
                    Formation_Hand1.draw(sprite_batch, Global.game_map.display_loc - Player_Sprite.loc, camera.matrix);
                }
            }
        }

        protected void draw_formation_change2(SpriteBatch sprite_batch)
        {
            if (Global.game_system.preparations && Changing_Formation)
            {
                if (Global.game_system.Selected_Unit_Id > -1)
                {
                    Formation_Hand2.draw(sprite_batch,
                        Global.game_map.display_loc - Map_Sprites[Global.game_system.Selected_Unit_Id].loc, camera.matrix);
                }
                else if (Global.game_map.changing_formation)
                {
                    if (Global.game_map.formation_unit_id2 != -1)
                    {
                        if (Map_Sprites[Global.game_map.formation_unit_id1].loc.Y <
                            Map_Sprites[Global.game_map.formation_unit_id2].loc.Y)
                        {
                            Formation_Hand2.draw(sprite_batch,
                                Global.game_map.display_loc - Map_Sprites[Global.game_map.formation_unit_id1].loc, camera.matrix);
                            Formation_Hand2.draw(sprite_batch,
                                Global.game_map.display_loc - Map_Sprites[Global.game_map.formation_unit_id2].loc, camera.matrix);
                        }
                        else
                        {
                            Formation_Hand2.draw(sprite_batch,
                                Global.game_map.display_loc - Map_Sprites[Global.game_map.formation_unit_id2].loc, camera.matrix);
                            Formation_Hand2.draw(sprite_batch,
                                Global.game_map.display_loc - Map_Sprites[Global.game_map.formation_unit_id1].loc, camera.matrix);
                        }
                    }
                    else
                        Formation_Hand2.draw(sprite_batch,
                            Global.game_map.display_loc - Map_Sprites[Global.game_map.formation_unit_id1].loc, camera.matrix);
                }
            }
        }

        protected bool weather_visible { get { return Weather_Effect != null && !(Global.game_system.preparations && Global.game_state.item_active); } }

        protected void draw_weather(SpriteBatch sprite_batch, float opacity = 1f)
        {
            if (weather_visible)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                Weather_Effect.draw(sprite_batch, Global.game_map.display_loc, Matrix.Identity, opacity);
                sprite_batch.End();
            }
        }
        protected void draw_upper_weather(SpriteBatch sprite_batch, float opacity = 1f)
        {
            if (weather_visible)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                Weather_Effect.draw_upper(sprite_batch, Global.game_map.display_loc, Matrix.Identity, opacity);
                sprite_batch.End();
            }
        }
        protected void draw_lower_weather(SpriteBatch sprite_batch, float opacity = 1f)
        {
            if (weather_visible)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                Weather_Effect.draw_lower(sprite_batch, Global.game_map.display_loc, Matrix.Identity, opacity);
                sprite_batch.End();
            }
        }

        protected void draw_suspend_fade(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            device.SetRenderTarget(render_targets[1]);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null);
            sprite_batch.Draw(render_targets[0], Vector2.Zero, Color.White);
            sprite_batch.End();

            int alpha = (Constants.Map.SUSPEND_FADE_TIME - Suspend_Fade_Timer) * 255 /
                Constants.Map.SUSPEND_FADE_TIME;
            device.SetRenderTarget(render_targets[0]);
            device.Clear(Color.Black);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            sprite_batch.Draw(render_targets[1], Vector2.Zero, new Color(alpha, alpha, alpha, alpha));
            sprite_batch.End();
        }

        protected void draw_ai_skip_switch(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            float opacity = 0;
            if (Black_Screen_Time > 0)
                opacity = 0;
            else if (Global.game_state.switching_ai_skip_counter > 0)
            {
                opacity = (Global.game_state.switching_ai_skip_counter /
                    (float)Constants.Map.SKIP_AI_SWTICH_TIME);
                opacity = (Global.game_state.skip_ai_state == State.Ai_Turn_Skip_State.SkipEnd ? 1 - opacity : opacity);
            }
            else if (Transition_Timer > 0 || Map_Transition)
            {
                opacity = (Transition_Timer / (float)Constants.Map.MAP_TRANSITION_TIME);
                opacity = (!Map_Transition ? 1 - opacity : opacity);
            }
            device.SetRenderTarget(render_targets[1]);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null);
            sprite_batch.Draw(render_targets[0], Vector2.Zero, Color.White);
            sprite_batch.End();
            device.SetRenderTarget(render_targets[0]);
            device.Clear(Color.Black);
            Color skip_color = new Color(opacity, opacity, opacity, opacity);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            sprite_batch.Draw(render_targets[1], Vector2.Zero, skip_color);
            sprite_batch.End();
        }

        float minimap_scale = 1f;
        float minimap_angle = 0f;
        protected void draw_minimap(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
#if DEBUG
            if (Global.Input.pressed(Inputs.Left))
            {
                minimap_scale -= 0.01f;
                minimap_angle -= 0.01f;
            }
            else if (Global.Input.pressed(Inputs.Right))
            {
                minimap_scale += 0.01f;
                minimap_angle += 0.01f;
            }
#endif
            if (Minimap != null)
            {
                // Draw darkening background
                Minimap.draw_background(sprite_batch);

                // Draw minimap map
                device.SetRenderTarget(render_targets[1]);
                device.Clear(Color.Transparent);
                Minimap.draw_map(sprite_batch);
                Minimap.draw_sprites(sprite_batch);
                // Draw map again, with the edges clipped based on how visible the map is
                // Ugly hack //Yeti
                // This doesn't work right with higher than 1 zoom if rendertarget[1] is just drawn to rendertarget[0]
                // problems with the pixel shader/etc
                device.SetRenderTarget(render_targets[2]);
                device.Clear(Color.Transparent);
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
                sprite_batch.Draw(render_targets[1], new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT),
                    new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT), Color.White);
                sprite_batch.End();

                // Mask map
                device.SetRenderTarget(render_targets[0]);
                sprite_batch.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
                Effect map_shader = Global.effect_shader();
                if (map_shader != null)
                {
                    map_shader.CurrentTechnique = map_shader.Techniques["Minimap_Mask"];
                    map_shader.Parameters["minimap_scale"].SetValue(1 / Minimap.scale);
                    map_shader.Parameters["minimap_angle"].SetValue(Minimap.angle);
                    // This also had to be added
                    map_shader.Parameters["game_size"].SetValue(new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT));

#if __ANDROID__
                    // There has to be a way to do this for both
                    map_shader.Parameters["Map_Alpha"].SetValue(White_Square);
#else
                    sprite_batch.GraphicsDevice.Textures[1] = White_Square;
#endif
                }
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, map_shader);
                sprite_batch.Draw(render_targets[2], Vector2.Zero,
                    new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT), Color.White, 0f,
                    Vector2.Zero, 1f, SpriteEffects.None, 0f);
                sprite_batch.End();

                // Draw white view rectangle on map
                Minimap.draw_view(sprite_batch);
            }
        }
        #endregion

        #region Dispose
        public override void dispose()
        {
            base.dispose();
            clear_menus();
        }
        #endregion
    }
}
