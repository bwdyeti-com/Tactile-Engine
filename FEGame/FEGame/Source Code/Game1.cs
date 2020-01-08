using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
#if MONOGAME
using Microsoft.Xna.Framework.Audio;
#else
using MonoGame.Framework.Audio;
#endif
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FEXNA;
using FEXNA.IO;
using FEXNA.Rendering;

using System.Runtime.InteropServices; //fullscreen test

namespace FEGame
{
    public class Game1 : Game, ISaveCallbacker
    {
        #region Constants
        public const int FRAME_RATE = 60;

#if DEBUG
        const bool HYPER_SPEED_ENABLED = true;
        const bool PAUSE_ENABLED = true;
#else
        const bool HYPER_SPEED_ENABLED = false; // This is prone to softlocking, don't turn it on for release
        const bool PAUSE_ENABLED = false;
#endif
        const int HYPER_SPEED_MULT = 15;

#if WINDOWS || MONOMAC
        const bool METRICS_ENABLED = Config.METRICS_ENABLED;
#else
        const bool METRICS_ENABLED = false;
#endif
        const int METRICS_SENDING_ATTEMPTS = 1;
        #endregion

        #region Fields
        private GameRenderer Renderer;
        private GameIOHandler IOHandler;

        bool Paused = false;
        bool Pause_Pressed = false, Next_Pressed = false;
        bool Frame_Advanced = false;
        bool F1_Pressed = false, F12_Pressed = false;

        KeyboardState PreviousKeyState;

        Thread MoveRangeUpdateThread = null, GraphicsLoadingThread = null, UpdateCheckThread = null;
        List<Thread> MetricsThreads = new List<Thread>();
        object MetricsLock = new object();

#if DEBUG || GET_FPS
        protected Stopwatch FramerateStopWatch = new Stopwatch();
        protected TimeSpan FramerateTime = new TimeSpan();
        protected int FramerateFrames = 0;
        protected double CurrentFrameRate = FRAME_RATE;
#endif

#if DEBUGMONITOR
        Debug_Monitor.DebugMonitorForm MonitorForm;
#endif
        protected float Process_Time = 0;

        protected Stopwatch Hyperspeed_Stop_Watch = new Stopwatch();

#if DEBUG
        protected string Map_Editor_Map, Map_Editor_Units, Map_Editor_Units_Source;
#endif

        private static Assembly GAME_ASSEMBLY { get { return Global.GAME_ASSEMBLY; } }
        #endregion
        
#if __ANDROID__
        protected bool Started;
        public bool started { get { return Started; } }
        protected bool In_Background, Ready_To_Resume;
        protected bool Has_Been_Backgrounded;
#endif

        public Game1(string[] args)
        {
            Global.GAME_ASSEMBLY = Assembly.GetExecutingAssembly();
            
#if WINDOWS || MONOMAC
            if (METRICS_ENABLED)
                Metrics_Handler.enable(GAME_ASSEMBLY);
#endif

            Global.RUNNING_VERSION = System.Reflection.Assembly.GetAssembly(typeof(Global)).GetName().Version;
            SetInitialFramerateValues();
            var fullscreenService = new FullscreenService(this);
            Renderer = new GameRenderer(this, fullscreenService);
            IOHandler = new GameIOHandler(this, Renderer);

            Content.RootDirectory = "Content";
#if !MONOGAME
            // Setup our OpenALSoundController to handle our SoundBuffer pools
            OpenALInterface.create_sound_controller();
#endif

#if XBOX
            Components.Add(new GamerServicesComponent(this));
#endif
            Global.storage_selection_requested = true;
            if (args.Length > 0)
                switch (args[0])
                {
#if DEBUG
                    case "UnitEditor":
                        if (args.Length == 3 || args.Length == 4)
                        {
                            Map_Editor_Map = args[1];
                            Map_Editor_Units = args[2];
                            if (args.Length == 4)
                                Map_Editor_Units_Source = args[3];
                            else
                                Map_Editor_Units_Source = Map_Editor_Units;

                        }
                        break;
#endif
                }
        }
        private void SetInitialFramerateValues()
        {
            this.TargetElapsedTime = TimeSpan.FromTicks(
                (long)(TimeSpan.TicksPerSecond * (1.0f / (float)FRAME_RATE)));

            this.IsFixedTimeStep = false;
            this.InactiveSleepTime = TimeSpan.FromTicks(0);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Events
            Global.send_metrics_to_server += Global_send_metrics_to_server;
            Global.check_for_updates_from_server += Global_check_for_updates_from_server;
            Global.set_update_uri(Update_Checker.GAME_DOWNLOAD);

            FEXNA.Input.default_controls();

            base.Initialize();

            this.IsMouseVisible = true;
#if WINDOWS || MONOMAC
            Global.load_config = true;
#endif

            // Set initial scene
#if DEBUG
            if (!string.IsNullOrEmpty(Map_Editor_Map))
                Global.scene_change("Scene_Map_Unit_Editor");
            else
#endif
                Global.scene = new SceneInitialLoad();

#if DEBUGMONITOR
            open_debug_monitor();
#endif
#if DEBUG && !MONOGAME
            FEXNA_Library.EventInput.Initialize(this.Window);
            FEXNA_Library.EventInput.CharEntered += new FEXNA_Library.EventArg.CharEnteredHandler(EventInput_CharEntered);
#endif
#if __ANDROID__
            Started = true;
#endif
        }

#if DEBUGMONITOR
        private void open_debug_monitor()
        {
            MonitorForm = new Debug_Monitor.DebugMonitorForm(this);
            MonitorForm.set_event_data_size(
                Config.EVENT_DATA_MONITOR_PAGE_SIZE, Config.EVENT_DATA_LENGTH);
            MonitorForm.FormClosing += MonitorForm_FormClosing;
            MonitorForm.Show();
        }
#endif

#if !MONOGAME && DEBUG
        void EventInput_CharEntered(object sender, FEXNA_Library.EventArg.CharacterEventArgs e)
        {
            Global.set_text(e.Character);
        }
#endif

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            Renderer.CreateSpriteBatch(this);

            Global.init(this, Content, Services);
            Renderer.LoadContent(Content);

            initialize_text_glow();
        }

#if __ANDROID__
        public void move_to_background()
        {
            In_Background = true;
            Has_Been_Backgrounded = true;
            Ready_To_Resume = false;
        }

        public void resume_processing()
        {
            Ready_To_Resume = true;
        }
#endif

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Global.dispose_battle_textures();
            Global.dispose_face_textures();
            Global.dispose_miniface_textures();
            Global.dispose_suspend_screenshots();
            // TODO: Unload any non ContentManager content here
            Global.palette_pool.Dispose();

            // End threads
            if (MoveRangeUpdateThread != null)
            {
                end_move_range_thread();
            }
            end_threads();

            // Cancel controller rumble
            GamePad.SetVibration(PlayerIndex.One, 0, 0);
            GamePad.SetVibration(PlayerIndex.Two, 0, 0);
            GamePad.SetVibration(PlayerIndex.Three, 0, 0);
            GamePad.SetVibration(PlayerIndex.Four, 0, 0);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            if (GraphicsLoadingThread != null)
            {
                GraphicsLoadingThread.Abort();
                GraphicsLoadingThread.Join();
                GraphicsLoadingThread = null;
            }

            GamePad.SetVibration(PlayerIndex.One, 0, 0);
            GamePad.SetVibration(PlayerIndex.Two, 0, 0);
            GamePad.SetVibration(PlayerIndex.Three, 0, 0);
            GamePad.SetVibration(PlayerIndex.Four, 0, 0);

            base.OnExiting(sender, args);
        }

#if DEBUGMONITOR
        void MonitorForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            MonitorForm = null;
        }
#endif

        #region Update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Global.quitting)
                this.Exit();

            KeyboardState key_state = Keyboard.GetState();
            GamePadState controller_state = GamePad.GetState(PlayerIndex.One);

#if !MONOGAME
            OpenALInterface.update();
#elif __ANDROID__
            if (In_Background)
            {
                if (Ready_To_Resume)
                {
                    set_render_targets();
                    // Recreate dynamic textures here
                    In_Background = false;
                }
                Ready_To_Resume = false;
                return;
            }
#endif
#if DEBUG
            if (FEXNA.Global.Input.pressed(Inputs.Select))
            {
                if (Global.stereoscopic_level < Global.MAX_STEREOSCOPIC_LEVEL && Global.Input.triggered(Inputs.Right))
                    Global.stereoscopic_level = Math.Min(Global.stereoscopic_level + 5, Global.MAX_STEREOSCOPIC_LEVEL);
                if (Global.stereoscopic_level > 0 && Global.Input.triggered(Inputs.Left))
                    Global.stereoscopic_level = Math.Max(Global.stereoscopic_level - 5, 0);
            }
#endif
            if (update_zoom())
            {
                PreviousKeyState = key_state;
                return;
            }
            int update_loops = update_framerate(gameTime, key_state, controller_state);
            update_initial_load();

            int i;
            for (i = 0; ; i++)
            {
                if (!Paused)
                {
                    Global.update_input(this, gameTime, IsActive, key_state, controller_state);
                    Global.Rumble.Update(gameTime);
                    Renderer.UpdateTouch();
                }

                update_pause(key_state, controller_state);
                bool process_next_frame = (!Paused || Frame_Advanced);

                if (i > 0)
                {
#if !MONOGAME
                    OpenALInterface.update();
#endif
                }
                // Don't update audio handling when the game is paused,
                // to better debug fades etc
                if (process_next_frame)
                    Global.Audio.update();
                Stereoscopic_Graphic_Object.update_stereoscopy();

                update_io();
                update_scene_change();
                if (Global.scene != null)
                {
                    // Suspend
                    IOHandler.UpdateSuspend();
                    // Update Scene
                    if (process_next_frame)
                        update_scene(gameTime, key_state, controller_state);
                }

                this.IsMouseVisible = FEXNA.Input.MouseVisible || Paused;

                base.Update(gameTime);
                if (update_loops > 1)
                {
                    float update_loop_time = (float)(Hyperspeed_Stop_Watch.ElapsedTicks / ((double)Stopwatch.Frequency));
                    // If a normal frame worth of time has passed, or if the fixed number of hyperspeed loops have occurred
                    if (update_loop_time > 1f / FRAME_RATE || i >= update_loops - 1)
                    {
                        break;
                    }
#if DEBUG
                    else
                        FramerateFrames++;
#endif
                }
                else
                    break;
            }

            update_open_game_site();
            update_debug_monitor(key_state);

            Global.Audio.post_update();
            Renderer.UpdateScreenScale();
            update_text_glow(i + 1);
            Hyperspeed_Stop_Watch.Stop(); //@Debug

            PreviousKeyState = key_state;
        }

        private void update_open_game_site()
        {
            if (Global.VisitGameSiteCall)
            {
                System.Diagnostics.Process.Start(
                    string.Format("http://{0}", Update_Checker.GAME_DOWNLOAD));
#if !__MOBILE__
                if (Global.fullscreen)
                {
#if !MONOGAME
                    System.Windows.Forms.Form.FromHandle(Window.Handle).FindForm().WindowState =
                        System.Windows.Forms.FormWindowState.Minimized;
#elif MONOMAC
                    this.Window.WindowState = MonoMac.OpenGL.WindowState.Minimized;
#endif
                }
#endif
                Global.game_site_visited();
            }
        }

        private void update_debug_monitor(KeyboardState key_state)
        {
#if DEBUGMONITOR
            if (MonitorForm != null)
            {
                Global.debug_monitor.update();
                MonitorForm.Invalidate();
            }
#endif
#if DEBUG
            if (key_state.IsKeyDown(Keys.OemTilde))
            {
#if DEBUGMONITOR
                if (MonitorForm == null)
                {
                    open_debug_monitor();
                }
                else
#endif
                    Global.open_debug_menu();
            }
#endif
        }

        #region Zoom
        private bool update_zoom()
        {
#if !__MOBILE__
            KeyboardState key_state = Keyboard.GetState();
            if ((key_state.IsKeyDown(Keys.LeftAlt) || key_state.IsKeyDown(Keys.RightAlt)) &&
                key_state.IsKeyDown(Keys.Enter) && !PreviousKeyState.IsKeyDown(Keys.Enter))
            {
                Global.fullscreen = !Global.fullscreen;
                Global.save_config = true;
            }
#endif

            return Renderer.UpdateZoom();
        }
        #endregion

        #region Initial Load
        private void update_initial_load()
        {
            if (GraphicsLoadingThread != null)
            {
                if (!GraphicsLoadingThread.IsAlive)
                {
                    GraphicsLoadingThread.Abort();
                    GraphicsLoadingThread.Join();
                    GraphicsLoadingThread = null;
                }
            }

            if (GraphicsLoadingThread == null)
            {
                if (Global.start_initial_load)
                {
                    GraphicsLoadingThread = new Thread(
                        new ThreadStart(load_content_thread));
                    GraphicsLoadingThread.Start();
                }
            }
        }

        private void load_content_thread()
        {
            Global.run_initial_load_content();

            Global.end_load_content();
        }
        #endregion

        #region Framerate/speed
        private int update_framerate(GameTime gameTime,
            KeyboardState key_state, GamePadState controller_state)
        {
            // Frame rate
            handle_frame_timing(gameTime);

            int update_loops;
            // If hyperspeed is enabled, and the key pressed
            if (hyperspeed_active(key_state, controller_state))
                update_loops = HYPER_SPEED_MULT;
            // Normal speed
            else
                update_loops = 1;

            update_fps();
            Hyperspeed_Stop_Watch.Restart();
            return update_loops;
        }

        private void handle_frame_timing(GameTime game_time)
        {
            Process_Time += (float)game_time.ElapsedGameTime.TotalSeconds;
            if (Process_Time > 5)
                Process_Time = 5;
            if (Process_Time < (1f / FRAME_RATE))
            {
                if (!IsFixedTimeStep)
                {
                    var sleep_time = TimeSpan.FromTicks(
                        (long)(TimeSpan.TicksPerSecond * ((1f / FRAME_RATE) - Process_Time)));
                    Thread.Sleep(sleep_time);
                }
            }
            Process_Time -= 1f / FRAME_RATE;
        }

        private void update_fps()
        {
#if DEBUG || GET_FPS
            // Frame rate
            FramerateStopWatch.Stop();
            TimeSpan elapsed = Paused ?
                TimeSpan.FromMilliseconds(1f / Config.FRAME_RATE) :
                FramerateStopWatch.Elapsed;
            FramerateTime += elapsed.TotalSeconds >= 1 ? TimeSpan.FromSeconds(1) : elapsed;
            FramerateFrames++;
            FramerateStopWatch.Restart();
            if (FramerateTime.TotalSeconds >= 1)
            {
                CurrentFrameRate = FramerateTime.TotalSeconds * FramerateFrames;
                FramerateTime -= TimeSpan.FromSeconds(1);
                FramerateFrames = 0;

                // Show the framerate in the window title
#if !__MOBILE__
                string title = string.Format("{0}: {1}fps",
                    GAME_ASSEMBLY.GetName().Name, (int)CurrentFrameRate);
                if ((int)CurrentFrameRate > FRAME_RATE)
                    title += string.Format(" ({0}%)",
                        (int)((100 * CurrentFrameRate) / FRAME_RATE));
#if DEBUG
                if (Global.UnitEditorActive)
                    title += string.Format(" - Map: {0} Units: {1}",
                        Map_Editor_Map, Map_Editor_Units);
#endif
                this.Window.Title = title;
#endif
            }
#endif
        }

        private static bool hyperspeed_active(KeyboardState loop_key_state, GamePadState loop_pad_state)
        {
            if (HYPER_SPEED_ENABLED)
            {
#if DEBUG
                if (loop_key_state.IsKeyDown(Keys.Space) || loop_pad_state.IsButtonDown(Buttons.RightTrigger))
#else
                if (loop_key_state.IsKeyDown(Keys.Space))
#endif
                    return true;
            }
            return false;
        }
        #endregion

        private void update_io()
        {
            bool createSaveState = false, loadSaveState = false;
            
            if (!Paused)
            {
                KeyboardState key_state = Keyboard.GetState();
                bool f1_pressed = key_state.IsKeyDown(Keys.F1);
                bool f1Triggered = f1_pressed && !F1_Pressed;
                F1_Pressed = f1_pressed;

                if (f1Triggered)
                {
                    if (key_state.IsKeyDown(Keys.LeftShift) ||
                            key_state.IsKeyDown(Keys.LeftShift))
                        createSaveState = true;
                    else
                        loadSaveState = true;
                }
            }

            IOHandler.UpdateIO(createSaveState, loadSaveState);
        }

        private void update_scene_change()
        {
            // Change scene
            if (Global.new_scene != "")
            {
                change_scene(Global.new_scene);
            }
        }
        
        private void update_pause(
            KeyboardState key_state, GamePadState controller_state)
        {
            // Test for pausing
            bool frame_started_paused = Paused;
            bool pause_pressed = false;
            bool next_pressed = false;
            if (pause_enabled())
            {
#if DEBUG
                pause_pressed = key_state.IsKeyDown(Keys.P) || controller_state.IsButtonDown(Buttons.LeftTrigger);
#else
                pause_pressed = key_state.IsKeyDown(Keys.P);
#endif
                next_pressed = key_state.IsKeyDown(Keys.N);

                Frame_Advanced = (next_pressed && !Next_Pressed);
                // If P was triggered or N was triggered and we're not paused
                if ((pause_pressed && !Pause_Pressed) || (!Paused && Frame_Advanced))
                    Paused = !Paused;
            }

            Pause_Pressed = pause_pressed;
            Next_Pressed = next_pressed;
        }

        private void update_scene(GameTime gameTime,
            KeyboardState key_state, GamePadState controller_state)
        {
            if (Paused)
                Frame_Advanced = true;
            if (!Global.scene.skip_frame())
            {
                if (Paused)
                    Global.update_input(this, gameTime, IsActive, key_state, controller_state);
#if !MONOGAME && DEBUG
                Global.update_text_input();
#endif
                Global.update_scene(Keyboard.GetState());
            }
        }

        protected bool pause_enabled()
        {
#if !MONOGAME && DEBUG
            bool unit_editor = Global.scene != null &&
                Global.scene.scene_type == "Scene_Map_Unit_Editor";
#else
            bool unit_editor = false;
#endif
            return Paused || (PAUSE_ENABLED && !unit_editor);
        }
        #endregion

        private void free_network_threads()
        {
            const int THREAD_MAX = 8;
            // While there are too many threads active
            while (MetricsThreads.Count >= THREAD_MAX)
            {
                // If any threads are done, remove them
                Thread finished_metrics_thread = MetricsThreads
                    .FirstOrDefault(x =>
                        x.ThreadState != System.Threading.ThreadState.Unstarted &&
                        x.ThreadState != System.Threading.ThreadState.Running);
                if (finished_metrics_thread != null)
                {
#if DEBUG
                    var thread_state = finished_metrics_thread.ThreadState;
#endif
                    finished_metrics_thread.Join();
                    MetricsThreads.Remove(finished_metrics_thread);
                }
                // Else wait for the oldest thread to finish
                else
                {
                    MetricsThreads[0].Join();
                    MetricsThreads.RemoveAt(0);
                }
            }
        }

        #region Metrics
        void Global_send_metrics_to_server(object sender, EventArgs e)
        {
            // Send Metrics to Server
            if (Global.sending_metrics)
            {
                if (METRICS_ENABLED)
                {
                    free_network_threads();
                    Thread connection_test_thread = new Thread(() => Metrics_Handler.test_connection());
                    connection_test_thread.Name = "Testing network connection";
                    connection_test_thread.Start();
                    MetricsThreads.Add(connection_test_thread);

                    free_network_threads();
                    lock (MetricsLock)
                        // Tested again here, in case it was turned off between entering the if and getting control of the lock
                        if (Global.sending_metrics)
                        {
                            Thread metrics_thread = new Thread(() => send_metrics_to_server(Global.metrics_data, Global.metrics_gameplay_data));
                            metrics_thread.Name = "Sending metrics";
                            metrics_thread.Start();
                            MetricsThreads.Add(metrics_thread);
                        }
                }
                else
                    Global.metrics_sent(false);
            }
        }

        private void send_metrics_to_server(string query, string post)
        {
            Thread.Sleep(10);
            FEXNA_Library.Maybe<bool> result = default(FEXNA_Library.Maybe<bool>);
#if WINDOWS || MONOMAC
                // Tries to send the metrics METRICS_SENDING_ATTEMPTS times; stops if any attempt succeeds
            for (int i = 0; i < METRICS_SENDING_ATTEMPTS && result.IsNothing; i++)
            {
                result = Metrics_Handler.send_data(query, post);
                if (result.IsNothing)
                    Thread.Sleep(10);
            }
#if DEBUG
            Debug.Assert(result.IsSomething, string.Format("Metrics sending failed after {0} attempts.", METRICS_SENDING_ATTEMPTS));
            Debug.Assert(result.IsNothing || result.ValueOrDefault, "Metrics sending succeeded, but the data was invalid");
#endif
#endif
            lock (MetricsLock)
                Global.metrics_sent(result.ValueOrDefault);
        }
        #endregion

        #region Check for Updates
        void Global_check_for_updates_from_server(object sender, EventArgs e)
        {
            if (!Global.updates_active)
                return;
            
            // Check update from Server
            if (UpdateCheckThread != null)
            {
                if (UpdateCheckThread.ThreadState == System.Threading.ThreadState.Unstarted ||
                        UpdateCheckThread.ThreadState == System.Threading.ThreadState.Running)
                    return;
                else
                {
                    UpdateCheckThread.Join();
                    UpdateCheckThread = null;
                }
            }

            free_network_threads();
            Thread connection_test_thread = new Thread(() => Update_Checker.test_connection());
            connection_test_thread.Name = "Testing network connection";
            connection_test_thread.Start();
            MetricsThreads.Add(connection_test_thread);

            UpdateCheckThread = new Thread(() => check_for_update());
            UpdateCheckThread.Name = "Checking for updates";
            UpdateCheckThread.Start();
        }

        private void check_for_update()
        {
            Thread.Sleep(10);
            Tuple<Version, DateTime, string> result = null;
#if WINDOWS || MONOMAC
            // Tries to check for updates 10 times; stops if any attempt succeeds
            for (int i = 0; i < 1 && result == null; i++)
            {
                result = Update_Checker.check_for_update();
                if (result == null)
                    Thread.Sleep(10);
            }
#if DEBUG
            Debug.Assert(result != null, "Update check failed after 1 attempt(s).");
#endif
#endif
            Global.update_found(result);
        }
        #endregion

        private void change_scene(string new_scene)
        {
            Global.return_to_title = false;
            if (Global.map_exists && MoveRangeUpdateThread == null)
                move_range_update_thread();
            bool scene_changed = true;
            string text = Global.game_temp.message_text;
            Global.game_temp.message_text = null;
            switch (new_scene)
            {
                case "Scene_Splash":
                    Global.scene = new Scene_Splash();
                    break;
                case "Scene_Soft_Reset":
                    end_move_range_thread();
                    MoveRangeUpdateThread = null;
                    Global.scene = new Scene_Soft_Reset();
                    break;
#if DEBUG
                case "Debug_Start":
#endif
                case "Start_Game":
                case "Scene_Worldmap":

                    IOHandler.RefreshSaveId();
#if DEBUG
                    IOHandler.RefreshDebugFileId(new_scene == "Debug_Start");
                    
                    if (new_scene == "Start_Game" || new_scene == "Debug_Start")
                    {
                        new_scene = "Start_Game";
#else
                    if (new_scene == "Start_Game")
                    {
#endif
                        IOHandler.LoadFile();
                        Global.game_options.post_read();
                        Global.game_temp = new Game_Temp();
                    }
                    Global.change_to_new_scene(new_scene);
                    break;
                case "Scene_Save":
                    Global.change_to_new_scene("Scene_Save");
                    break;
                case "Start_Chapter":
                    end_move_range_thread();


                    Global.start_chapter();


                    move_range_update_thread();
                    break;
#if !MONOGAME && DEBUG
                case "Scene_Map_Unit_Editor":
                    end_move_range_thread();

                    Global.start_unit_editor(
                        Map_Editor_Units, Map_Editor_Map, Map_Editor_Units_Source);

                    move_range_update_thread();
                    break;
                case "Scene_Map_Playtest":
                    if (!Global.scene.is_unit_editor)
                    {
                        Print.message("Failed to start playtest, not currently in unit editor");
                        scene_changed = false;
                    }
                    else
                    {
                        IOHandler.RefreshFileId();
                        IOHandler.LoadFile();
                        Global.game_options.post_read();
                        Global.game_temp = new Game_Temp();



                        end_move_range_thread();

                        Global.start_unit_editor_playtest();

                        move_range_update_thread();
                    }
                    break;
#endif
                case "Load_Suspend":
                    IOHandler.RefreshSaveId();

                    // Resume Arena
                    if (Global.in_arena())
                    {
                        change_scene("Scene_Arena");
                        move_range_update_thread();
                        return;
                    }
                    else
                    {
                        Global.suspend_finish_load(true);
                        move_range_update_thread();
                    }
                    break;
                case "Scene_Map":
                    Global.change_to_new_scene("Scene_Map");
                    Global.init_map();
                    break;
                case "Scene_Dance":
                case "Scene_Staff":
                case "Scene_Arena":
                case "Scene_Battle":
                case "Scene_Promotion":
#if DEBUG
                case "Scene_Test_Battle":
                    if (new_scene == "Scene_Test_Battle")
                    {
                        Global.change_to_new_scene("Scene_Test_Battle");
                    }
                    else
                    {
#endif
                        Global.change_to_new_scene(new_scene);
                        Global.initialize_action_scene(new_scene == "Scene_Arena", new_scene == "Scene_Promotion");
#if DEBUG
                    }
#endif
                    break;
                case "Scene_Title_Load":
                    Global.reset_system();
                    Global.change_to_new_scene("Scene_Title_Load");
                    break;
                case "Scene_Title":
                    Global.reset_system();
                    Global.clear_events();
                    Global.change_to_new_scene("Scene_Title");
                    break;
                case "Scene_Class_Reel":
                    Global.game_temp = new Game_Temp();
                    Global.new_game_actors();
                    Global.change_to_new_scene("Scene_Class_Reel");
                    break;
                default:
                    scene_changed = false;
#if DEBUG
                    Print.message("Non-existant scene type called: " + Global.new_scene);
#endif
                    break;
            }

            if (scene_changed)
            {
                Global.dispose_battle_textures();
                Global.dispose_face_textures();
                Global.Audio.stop_bgs(); //@Debug
                Global.Audio.stop_me(); //@Debug
            }
            else
                Global.game_temp.message_text = text;
            Global.scene_change("");
        }

        #region Text Glow
        readonly static Dictionary<string, Color[]> TEXT_GLOW = new Dictionary<string, Color[]>
        {
            { "Green", new Color[]
                {
                    new Color(72, 232, 32),
                    new Color(96, 240, 56),
                    new Color(120, 240, 88),
                    new Color(144, 240, 112),
                    new Color(168, 240, 144),
                    new Color(192, 248, 168),
                    new Color(216, 248, 200),
                    new Color(240, 248, 224)
                }
            },
            { "Red", new Color[]
                {
                    new Color(224, 96, 80),
                    new Color(224, 104, 88),
                    new Color(232, 144, 120),
                    new Color(232, 168, 144),
                    new Color(240, 184, 160),
                    new Color(248, 208, 184),
                    new Color(248, 224, 208),
                    new Color(248, 232, 224)
                }
            }
        };
        private int Glow_Timer = 0;
        // A dictionary of color arrays representing the pixel data of each glowing font texture at each glow frame
        private Dictionary<string, Color[][]> glow_data2 = new Dictionary<string, Color[][]>();
        private void initialize_text_glow()
        {
            foreach (string name in TEXT_GLOW.Keys)
            {
                Texture2D green_text = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + name);
                glow_data2[name] = new Color[TEXT_GLOW[name].Length][];
                for (int i = 0; i < TEXT_GLOW[name].Length; i++)
                {
                    // Copies the raw pixel data of the graphic to the array
                    glow_data2[name][i] = new Color[green_text.Width * green_text.Height];
                    green_text.GetData<Color>(glow_data2[name][i]);
                    // Then for the one color that should be different each frame, changes that color for the current frame
                    for (int y = 0; y < green_text.Height; y++)
                        for (int x = 0; x < green_text.Width; x++)
                            if (glow_data2[name][i][x + y * green_text.Width] == TEXT_GLOW[name][0])
                                glow_data2[name][i][x + y * green_text.Width] = TEXT_GLOW[name][i];
                }
            }
        }
        private void update_text_glow(int ticks = 1)
        {
            int glow_timer = Glow_Timer;
            Glow_Timer = (Glow_Timer + ticks) % (TEXT_GLOW["Green"].Length * 8);
            
            if (Enumerable.Range(1, ticks).Any(x => (glow_timer + x) % 4 == 0))
            {
                glow_timer = Glow_Timer;
                int current_index = glow_timer >= (TEXT_GLOW["Green"].Length * 4) ? (TEXT_GLOW["Green"].Length * 2) - (glow_timer / 4 + 1) : (glow_timer / 4);
                glow_timer = (glow_timer - 4 + TEXT_GLOW["Green"].Length * 8) % (TEXT_GLOW["Green"].Length * 8);
                int old_index = glow_timer >= (TEXT_GLOW["Green"].Length * 4) ? (TEXT_GLOW["Green"].Length * 2) - (glow_timer / 4 + 1) : (glow_timer / 4);

                if (current_index != old_index)
                {
                    foreach (string name in TEXT_GLOW.Keys)
                    {
                        Texture2D green_text = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + name);
                        green_text.SetData<Color>(glow_data2[name][old_index]);
                    }
                }
            }
        }
        #endregion

        #region Draw
#if __ANDROID__
        private bool Off_Draw_Frame = false;
#endif
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
#if __ANDROID__
            // Halves draw rate in Android
            // replace false with some global override for forcing full frame rate
            if ((!Off_Draw_Frame || false) && (!Paused || Frame_Advanced))
#else
            if (!Paused || Frame_Advanced)
#endif
            {
                Frame_Advanced = false;

                Renderer.Draw();

                base.Draw(gameTime);
            }
            else
                Renderer.RedrawPreviousFrame();

            // Save a screenshot if F12 is pressed
            update_screenshot();

#if __ANDROID__
            Off_Draw_Frame = !Off_Draw_Frame;
#endif

#if DEBUG && (__MOBILE__ || GET_FPS)
            // Draw frame rate in debug mode on mobile
            Renderer.DrawMobileFps(CurrentFrameRate);
#endif

#if DEBUGMONITOR
            if (MonitorForm != null)
                MonitorForm.invalidate_monitor();
#endif
        }

        private void update_screenshot()
        {
#if WINDOWS || MONOMAC
            bool f12_pressed = false;
            KeyboardState key_state = Keyboard.GetState();
            f12_pressed = key_state.IsKeyDown(Keys.F12);

            if (f12_pressed && !F12_Pressed)
            {
                string path = System.IO.Path.GetDirectoryName(GAME_ASSEMBLY.Location);
                path = Path.Combine(path, "Screenshots");

                try
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    string filename = System.IO.Path.GetFileNameWithoutExtension(GAME_ASSEMBLY.Location) + "_";
                    int i = 0;
                    while (File.Exists(Path.Combine(
                            path, filename + i.ToString("D4") + ".png")))
                        i++;

                    // save render target to disk
                    using (FileStream fs = new FileStream(Path.Combine(
                            path, filename + i.ToString("D4") + ".png"),
                        FileMode.OpenOrCreate))
                    {
                        Renderer.SaveScreenshot(fs);
                    }
                }
                // Could not save screenshot because no folder permissions
                catch (UnauthorizedAccessException ex)
                {
                    Global.play_se(System_Sounds.Buzzer);
                    return;
                }
            }
            F12_Pressed = f12_pressed;
#endif
        }
        #endregion
        
        private void move_range_update_thread()
        {
            if (MoveRangeUpdateThread != null)
            {
                end_move_range_thread();
            }
            MoveRangeUpdateThread = new Thread(new ThreadStart(Global.map_update_move_range_loop));
            MoveRangeUpdateThread.Name = "Move range update";
            MoveRangeUpdateThread.Start();
        }

        private void end_move_range_thread()
        {
            if (MoveRangeUpdateThread != null)
            {
                MoveRangeUpdateThread.Abort();
                MoveRangeUpdateThread.Join();
            }
        }

        private void end_threads()
        {
            if (GraphicsLoadingThread != null)
            {
                GraphicsLoadingThread.Abort();
                GraphicsLoadingThread.Join();
            }
            if (UpdateCheckThread != null)
            {
                UpdateCheckThread.Abort();
                UpdateCheckThread.Join();
            }
            foreach(var thread in MetricsThreads)
            {
                thread.Abort();
                thread.Join();
            }
        }

        #region ISaveCallbacker
        public void TitleLoadScene()
        {
            change_scene("Scene_Title_Load");
        }
        public void ArenaScene()
        {
            change_scene("Scene_Arena");
        }

        public void StartMoveRangeThread()
        {
            move_range_update_thread();
        }
        public void EndMoveRangeThread()
        {
            end_move_range_thread();
            MoveRangeUpdateThread = null;
        }

        public void FinishLoad()
        {
            var key_state = new KeyboardState();
            var controller_state = new GamePadState();
            Global.update_input(this, new GameTime(), IsActive, key_state, controller_state);
        }
        #endregion
    }
}
