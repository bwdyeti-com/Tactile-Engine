using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
#if __MOBILE__
using Android.App;
using Android.Content;
#endif
using Microsoft.Xna.Framework;
#if MONOGAME
using Microsoft.Xna.Framework.Audio;
#else
using MonoGame.Framework.Audio;
#endif
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tactile.IO;
using Tactile.Options;
using Tactile.Rendering;

namespace Tactile
{
    public class GameLoop: ISaveCallbacker
    {
        #region Constants
        public const int FRAME_RATE = 60;

#if DEBUG //Cheat codes
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

        private Game Game;
        private GameRenderer Renderer;
        private GameIOHandler IOHandler;
        private Settings GameSettings;

        private IMetricsService MetricsHandler;
        private IUpdateService UpdateChecker;
        
        public bool Paused { get; private set; }
        private bool PausePressed = false, NextPressed = false;
        private bool FrameAdvanced = false;
        private bool F1Pressed = false, F12Pressed = false;

        private KeyboardState PreviousKeyState;

        private Thread MoveRangeUpdateThread = null, GraphicsLoadingThread = null, UpdateCheckThread = null;
        private List<Thread> MetricsThreads = new List<Thread>();
        private object MetricsLock = new object();

#if DEBUG || GET_FPS
        private Stopwatch FramerateStopWatch = new Stopwatch();
        private TimeSpan FramerateTime = new TimeSpan();
        private int FramerateFrames = 0;
        private double CurrentFrameRate = FRAME_RATE;
#endif

        public bool GameInactive { get; private set; }
        private float ProcessTime = 0;

        private Stopwatch HyperspeedStopWatch = new Stopwatch();

#if DEBUG
        private string MapEditorMap, MapEditorUnits, MapEditorUnitsSource;
#endif

        public GameLoop(Game game,
            IFullscreenService fullscreenService,
            IMetricsService metricsHandler,
            IUpdateService updateChecker)
        {
            MetricsHandler = metricsHandler;
            UpdateChecker = updateChecker;

#if WINDOWS || MONOMAC
            if (METRICS_ENABLED)
                Global.metrics_allowed = true;
#endif
            if (Config.UPDATE_CHECK_ENABLED)
                Global.update_check_allowed = true;

            //@Yeti: make this work without Global later?
            GameSettings = new Settings();
            Global.gameSettings = GameSettings;

            Game = game;
            Renderer = new GameRenderer(game, fullscreenService);
            IOHandler = new GameIOHandler(this, Renderer);

#if !MONOGAME
            // Setup our OpenALSoundController to handle our SoundBuffer pools
            OpenALInterface.create_sound_controller();
#endif
        }

#if DEBUG
        public void SetUnitEditorValues(string map, string units, string unitsSource)
        {
            MapEditorMap = map;
            MapEditorUnits = units;
            MapEditorUnitsSource = unitsSource;
        }
#endif

        public void Initialize()
        {
            // Events
            Global.send_metrics_to_server += Global_SendMetricsToServer;
            Global.check_for_updates_from_server += Global_CheckForUpdatesFromServer;
            Global.set_update_uri(UpdateChecker.GameDownloadUrl);

#if WINDOWS || MONOMAC
            Global.load_config = true;
#endif

#if DEBUG && !MONOGAME
            TactileLibrary.EventInput.Initialize(Game.Window);
            TactileLibrary.EventInput.CharEntered += new TactileLibrary.EventArg.CharEnteredHandler(EventInput_CharEntered);
#endif

            // Set initial scene
#if DEBUG
            if (!string.IsNullOrEmpty(MapEditorMap))
                Global.scene_change("Scene_Map_Unit_Editor");
            else
#endif
                Global.scene = new SceneInitialLoad();
        }

#if !MONOGAME && DEBUG
        void EventInput_CharEntered(object sender, TactileLibrary.EventArg.CharacterEventArgs e)
        {
            Global.set_text(e.Character);
        }
#endif

        public void LoadContent(Game game)
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            Renderer.CreateSpriteBatch(game);

            Global.init(game, game.Content, game.Services);
            Renderer.LoadContent(game.Content);

            InitializeTextGlow();
        }

        public void UnloadContent()
        {
            Global.palette_pool.Dispose();

            // End threads
            if (MoveRangeUpdateThread != null)
            {
                EndMoveRangeThread();
            }
            EndThreads();
        }

        public void GameGainedFocus()
        {
            GameInactive = false;
        }

        public void GameLostFocus()
        {
            GameInactive = true;

#if !__MOBILE__
            if (Global.gameSettings.Graphics.MinimizeWhenInactive)
            {
                Renderer.MinimizeFullscreen(Game);
            }
#endif
        }

        public void CancelGraphicsLoadingThread()
        {
            if (GraphicsLoadingThread != null)
            {
                GraphicsLoadingThread.Abort();
                GraphicsLoadingThread.Join();
                GraphicsLoadingThread = null;
            }
        }

        public void SetRenderTargets()
        {
            Renderer.SetRenderTargets();
        }

        internal static void OpenHyperlink(string link)
        {
            string url = string.Format("http://{0}", link);
#if __MOBILE__
            var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
            intent.SetFlags(ActivityFlags.NewTask);
            Game.Activity.StartActivity(intent);
#else
            Process.Start(url);
#endif
        }

        #region Update
        public IEnumerable<GameTime> Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            GamePadState controllerState = GamePad.GetState(PlayerIndex.One);

#if !MONOGAME
            OpenALInterface.UpdateSfxPool();
            OpenALInterface.update();
#endif

#if DEBUG
            // Stereoscopy debugging
            if (Tactile.Global.Input.pressed(Inputs.Select))
            {
                if (Global.Input.triggered(Inputs.Right))
                {
                    Global.gameSettings.Graphics.ConfirmSetting(
                        Tactile.Options.GraphicsSetting.Stereoscopic, 0,
                        Global.gameSettings.Graphics.StereoscopicLevel + 5);
                }
                if (Global.Input.triggered(Inputs.Left))
                {
                    Global.gameSettings.Graphics.ConfirmSetting(
                        Tactile.Options.GraphicsSetting.Stereoscopic, 0,
                        Global.gameSettings.Graphics.StereoscopicLevel - 5);
                }
            }
#endif
            // If changing zoom or fullscreen, return
            if (UpdateZoom())
            {
                PreviousKeyState = keyState;
                yield break;
            }
            int updateLoops = UpdateFramerate(gameTime, keyState, controllerState);
            UpdateInitialLoad();

            int i;
            for (i = 0; ; i++)
            {
                if (!Paused)
                {
                    Global.update_input(Game, gameTime, Game.IsActive, keyState, controllerState);
                    Global.Rumble.Update(gameTime);
                    Renderer.UpdateTouch();
                }

                UpdatePause(keyState, controllerState);
                bool processNextFrame = (!Paused || FrameAdvanced);

                if (i > 0)
                {
#if !MONOGAME
                    OpenALInterface.UpdateSfxPool();
                    OpenALInterface.update();
#endif
                }
                // Don't update audio handling when the game is paused,
                // to better debug fades etc
                if (processNextFrame)
                    Global.Audio.update(GameInactive);
                Stereoscopic_Graphic_Object.update_stereoscopy();

                UpdateIO();
                UpdateSceneChange();
                if (Global.scene != null)
                {
                    // Suspend
                    IOHandler.UpdateSuspend();
                    // Update Scene
                    if (processNextFrame)
                        UpdateScene(gameTime, keyState, controllerState);
                }

                yield return gameTime;
                
                if (updateLoops > 1)
                {
                    float updateLoopTime = (float)(HyperspeedStopWatch.ElapsedTicks / ((double)Stopwatch.Frequency));
                    // If a normal frame worth of time has passed, or if the fixed number of hyperspeed loops have occurred
                    if (updateLoopTime > 1f / FRAME_RATE || i >= updateLoops - 1)
                    {
                        break;
                    }
#if DEBUG || GET_FPS
                    else
                        FramerateFrames++;
#endif
                }
                else
                    break;
            }

            UpdateOpenGameSite();

            Global.Audio.post_update();
            Renderer.UpdateScreenScale();
            UpdateTextGlow(i + 1);
            HyperspeedStopWatch.Stop(); //@Debug

            PreviousKeyState = keyState;
        }

        private void UpdateOpenGameSite()
        {
            if (Global.VisitGameSiteCall)
            {
                OpenHyperlink(UpdateChecker.GameDownloadUrl);

#if !__MOBILE__
                if (Global.gameSettings.Graphics.Fullscreen)
                {
                    Renderer.MinimizeFullscreen(Game);
                }
#endif

                Global.game_site_visited();
            }
        }

        #region Zoom
        private bool UpdateZoom()
        {
#if !__MOBILE__
            KeyboardState keyState = Keyboard.GetState();
            if ((keyState.IsKeyDown(Keys.LeftAlt) || keyState.IsKeyDown(Keys.RightAlt)) &&
                keyState.IsKeyDown(Keys.Enter) && !PreviousKeyState.IsKeyDown(Keys.Enter))
            {
                if (!Global.scene.fullscreen_switch_blocked())
                {
                    Global.gameSettings.Graphics.SwitchFullscreen();
                    Global.save_config = true;
                }
            }
#endif

            return Renderer.UpdateZoom();
        }
        #endregion

        #region Initial Load
        private void UpdateInitialLoad()
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
                        new ThreadStart(LoadContentThread));
                    GraphicsLoadingThread.Start();
                }
            }
        }

        private void LoadContentThread()
        {
            Global.run_initial_load_content();

            Global.end_load_content();
        }
        #endregion

        #region Framerate/speed
        private int UpdateFramerate(GameTime gameTime,
            KeyboardState keyState, GamePadState controllerState)
        {
            // Frame rate
            HandleFrameTiming(gameTime);

            int updateLoops;
            // If hyperspeed is enabled, and the key pressed
            if (HyperspeedActive(keyState, controllerState))
                updateLoops = HYPER_SPEED_MULT;
            // Normal speed
            else
                updateLoops = 1;

            UpdateFps();
            HyperspeedStopWatch.Restart();
            return updateLoops;
        }

        private void HandleFrameTiming(GameTime gameTime)
        {
            ProcessTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (ProcessTime > 5)
                ProcessTime = 5;
            if (ProcessTime < (1f / FRAME_RATE))
            {
                if (!Game.IsFixedTimeStep)
                {
                    var sleepTime = TimeSpan.FromTicks(
                        (long)(TimeSpan.TicksPerSecond * ((1f / FRAME_RATE) - ProcessTime)));
                    Thread.Sleep(sleepTime);
                }
            }
            ProcessTime -= 1f / FRAME_RATE;
        }

        private void UpdateFps()
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
                    Global.GAME_ASSEMBLY.GetName().Name, (int)CurrentFrameRate);
                if ((int)CurrentFrameRate > FRAME_RATE)
                    title += string.Format(" ({0}%)",
                        (int)((100 * CurrentFrameRate) / FRAME_RATE));
#if DEBUG
                if (Global.UnitEditorActive)
                    title += string.Format(" - Map: {0} Units: {1}",
                        MapEditorMap, MapEditorUnits);
#endif
                Game.Window.Title = title;
#endif
            }
#endif
        }

        private bool HyperspeedActive(KeyboardState loopKeyState, GamePadState loopPadState)
        {
            if (HYPER_SPEED_ENABLED)
            {
#if DEBUG
                if (loopKeyState.IsKeyDown(Keys.Space) || loopPadState.IsButtonDown(Buttons.RightTrigger))
#else
                if (loopKeyState.IsKeyDown(Keys.Space))
#endif
                    return true;
            }
            return false;
        }
        #endregion

        private void UpdateIO()
        {
            bool createSaveState = false, loadSaveState = false;

            if (!Paused)
            {
                KeyboardState keyState = Keyboard.GetState();
                bool f1Pressed = keyState.IsKeyDown(Keys.F1);
                bool f1Triggered = f1Pressed && !F1Pressed;
                F1Pressed = f1Pressed;

                if (f1Triggered)
                {
                    if (keyState.IsKeyDown(Keys.LeftShift) ||
                            keyState.IsKeyDown(Keys.LeftShift))
                        createSaveState = true;
                    else
                        loadSaveState = true;
                }
            }

            IOHandler.UpdateIO(createSaveState, loadSaveState);
        }

        private void UpdateSceneChange()
        {
            // Change scene
            if (Global.new_scene != "")
            {
                ChangeScene(Global.new_scene);
            }
        }

        private void UpdatePause(
            KeyboardState keyState, GamePadState controllerState)
        {
            // Test for pausing
            bool frameStartedPaused = Paused;
            bool pausePressed = false;
            bool nextPressed = false;
            if (PauseEnabled())
            {
#if DEBUG
                pausePressed = keyState.IsKeyDown(Keys.P) || controllerState.IsButtonDown(Buttons.LeftTrigger);
#else
                pausePressed = keyState.IsKeyDown(Keys.P);
#endif
                nextPressed = keyState.IsKeyDown(Keys.N);

                FrameAdvanced = (nextPressed && !NextPressed);
                // If P was triggered or N was triggered and we're not paused
                if ((pausePressed && !PausePressed) || (!Paused && FrameAdvanced))
                    Paused = !Paused;
            }

            PausePressed = pausePressed;
            NextPressed = nextPressed;
        }

        private void UpdateScene(GameTime gameTime,
            KeyboardState keyState, GamePadState controllerState)
        {
            if (Paused)
                FrameAdvanced = true;
            if (!Global.scene.skip_frame())
            {
                if (Paused)
                    Global.update_input(Game, gameTime, Game.IsActive, keyState, controllerState);
#if !MONOGAME && DEBUG
                Global.update_text_input();
#endif
                Global.update_scene(keyState);
            }
        }

        private bool PauseEnabled()
        {
#if !MONOGAME && DEBUG
            bool unitEditor = Global.scene != null &&
                Global.scene.scene_type == "Scene_Map_Unit_Editor";
#else
            bool unitEditor = false;
#endif
            return Paused || (PAUSE_ENABLED && !unitEditor);
        }
        #endregion
        
        private void FreeNetworkThreads()
        {
            const int THREAD_MAX = 8;
            // While there are too many threads active
            while (MetricsThreads.Count >= THREAD_MAX)
            {
                // If any threads are done, remove them
                Thread finishedMetricsThread = MetricsThreads
                    .FirstOrDefault(x =>
                        x.ThreadState != System.Threading.ThreadState.Unstarted &&
                        x.ThreadState != System.Threading.ThreadState.Running);
                if (finishedMetricsThread != null)
                {
#if DEBUG
                    var threadState = finishedMetricsThread.ThreadState;
#endif
                    finishedMetricsThread.Join();
                    MetricsThreads.Remove(finishedMetricsThread);
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
        private void Global_SendMetricsToServer(object sender, EventArgs e)
        {
            // Send Metrics to Server
            if (Global.sending_metrics)
            {
                if (METRICS_ENABLED)
                {
                    FreeNetworkThreads();
                    Thread connectionTestThread = new Thread(() => MetricsHandler.test_connection());
                    connectionTestThread.Name = "Testing network connection";
                    connectionTestThread.Start();
                    MetricsThreads.Add(connectionTestThread);

                    FreeNetworkThreads();
                    lock (MetricsLock)
                        // Tested again here, in case it was turned off between entering the if and getting control of the lock
                        if (Global.sending_metrics)
                        {
                            Thread metricsThread = new Thread(() => SendMetricsToServer(Global.metrics_data, Global.metrics_gameplay_data));
                            metricsThread.Name = "Sending metrics";
                            metricsThread.Start();
                            MetricsThreads.Add(metricsThread);
                        }
                }
                else
                    Global.metrics_sent(false);
            }
        }

        private void SendMetricsToServer(string query, string post)
        {
            Thread.Sleep(10);
            TactileLibrary.Maybe<bool> result = TactileLibrary.Maybe<bool>.Nothing;
#if WINDOWS || MONOMAC
            // Tries to send the metrics METRICS_SENDING_ATTEMPTS times; stops if any attempt succeeds
            for (int i = 0; i < METRICS_SENDING_ATTEMPTS && result.IsNothing; i++)
            {
                result = MetricsHandler.send_data(query, post);
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
        void Global_CheckForUpdatesFromServer(object sender, EventArgs e)
        {
            if (!Global.gameSettings.General.CheckForUpdates)
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

            FreeNetworkThreads();
            Thread connectionTestThread = new Thread(() => UpdateChecker.test_connection());
            connectionTestThread.Name = "Testing network connection";
            connectionTestThread.Start();
            MetricsThreads.Add(connectionTestThread);

            UpdateCheckThread = new Thread(() => CheckForUpdate());
            UpdateCheckThread.Name = "Checking for updates";
            UpdateCheckThread.Start();
        }

        private void CheckForUpdate()
        {
            Thread.Sleep(10);
            Tuple<Version, DateTime, string> result = null;
#if WINDOWS || MONOMAC
            // Tries to check for updates 10 times; stops if any attempt succeeds
            for (int i = 0; i < 1 && result == null; i++)
            {
                result = UpdateChecker.check_for_update();
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

        private void ChangeScene(string newScene)
        {
            Global.return_to_title = false;
            if (Global.map_exists && MoveRangeUpdateThread == null)
                StartMoveRangeThread();
            bool sceneChanged = true;
            string text = Global.game_temp.message_text;
            Global.game_temp.message_text = null;
            switch (newScene)
            {
                case "Scene_Splash":
                    Global.scene = new Scene_Splash();
                    break;
                case "Scene_Soft_Reset":
                    EndMoveRangeThread();
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
                    IOHandler.RefreshDebugFileId(newScene == "Debug_Start");

                    if (newScene == "Start_Game" || newScene == "Debug_Start")
                    {
                        newScene = "Start_Game";
#else
                    if (newScene == "Start_Game")
                    {
#endif
                        IOHandler.LoadFile(true);
                        Global.game_options.post_read();
                        Global.game_temp = new Game_Temp();
                    }
                    Global.change_to_new_scene(newScene);
                    break;
                case "Scene_Save":
                    Global.change_to_new_scene("Scene_Save");
                    break;
                case "Start_Chapter":
                    EndMoveRangeThread();


                    Global.start_chapter();


                    StartMoveRangeThread();
                    break;
#if !MONOGAME && DEBUG
                case "Scene_Map_Unit_Editor":
                    EndMoveRangeThread();

                    Global.start_unit_editor(
                        MapEditorUnits, MapEditorMap, MapEditorUnitsSource);

                    StartMoveRangeThread();
                    break;
                case "Scene_Map_Playtest":
                    if (!Global.scene.is_unit_editor)
                    {
                        Print.message("Failed to start playtest, not currently in unit editor");
                        sceneChanged = false;
                    }
                    else
                    {
                        IOHandler.RefreshFileId();
                        IOHandler.LoadFile(true);
                        Global.game_options.post_read();
                        Global.game_temp = new Game_Temp();



                        EndMoveRangeThread();

                        Global.start_unit_editor_playtest();

                        StartMoveRangeThread();
                    }
                    break;
#endif
                case "Load_Suspend":
                    IOHandler.RefreshSaveId();

                    // Resume Arena
                    if (Global.in_arena())
                    {
                        ChangeScene("Scene_Arena");
                        StartMoveRangeThread();
                        return;
                    }
                    else
                    {
                        Global.suspend_finish_load(true);
                        StartMoveRangeThread();
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
                    if (newScene == "Scene_Test_Battle")
                    {
                        Global.change_to_new_scene("Scene_Test_Battle");
                    }
                    else
                    {
#endif
                        Global.change_to_new_scene(newScene);
                        Global.initialize_action_scene(newScene == "Scene_Arena", newScene == "Scene_Promotion");
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
                    sceneChanged = false;
#if DEBUG
                    Print.message("Non-existant scene type called: " + Global.new_scene);
#endif
                    break;
            }

            if (sceneChanged)
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
        private int GlowTimer = 0;
        // A dictionary of color arrays representing the pixel data of each glowing font texture at each glow frame
        private Dictionary<string, Color[][]> GlowData = new Dictionary<string, Color[][]>();
        private void InitializeTextGlow()
        {
            foreach (string color in TEXT_GLOW.Keys)
            {
                Texture2D texture = Global.Content.Load<Texture2D>(
                    string.Format(@"Graphics/Fonts/{0}_{1}",
                    Config.UI_FONT, color));
                GlowData[color] = new Color[TEXT_GLOW[color].Length][];
                for (int i = 0; i < TEXT_GLOW[color].Length; i++)
                {
                    // Copies the raw pixel data of the graphic to the array
                    GlowData[color][i] = new Color[texture.Width * texture.Height];
                    texture.GetData<Color>(GlowData[color][i]);
                    // Then for the one color that should be different each frame, changes that color for the current frame
                    for (int y = 0; y < texture.Height; y++)
                        for (int x = 0; x < texture.Width; x++)
                            if (GlowData[color][i][x + y * texture.Width] == TEXT_GLOW[color][0])
                                GlowData[color][i][x + y * texture.Width] = TEXT_GLOW[color][i];
                }
            }
        }
        private void UpdateTextGlow(int ticks = 1)
        {
            int glowTimer = GlowTimer;
            GlowTimer = (GlowTimer + ticks) % (TEXT_GLOW["Green"].Length * 8);

            if (Enumerable.Range(1, ticks).Any(x => (glowTimer + x) % 4 == 0))
            {
                glowTimer = GlowTimer;
                int currentIndex = glowTimer >= (TEXT_GLOW["Green"].Length * 4) ? (TEXT_GLOW["Green"].Length * 2) - (glowTimer / 4 + 1) : (glowTimer / 4);
                glowTimer = (glowTimer - 4 + TEXT_GLOW["Green"].Length * 8) % (TEXT_GLOW["Green"].Length * 8);
                int oldIndex = glowTimer >= (TEXT_GLOW["Green"].Length * 4) ? (TEXT_GLOW["Green"].Length * 2) - (glowTimer / 4 + 1) : (glowTimer / 4);

                if (currentIndex != oldIndex)
                {
                    foreach (string color in TEXT_GLOW.Keys)
                    {
                        Texture2D text = Global.Content.Load<Texture2D>(
                            string.Format(@"Graphics/Fonts/{0}_{1}",
                            Config.UI_FONT, color));
                        text.SetData<Color>(GlowData[color][oldIndex]);
                    }
                }
            }
        }
        #endregion

        #region Draw
        public void Draw(GameTime gameTime, bool offFrame)
        {
            if (!offFrame && (!Paused || FrameAdvanced))
            {
                FrameAdvanced = false;

                Renderer.Draw();
            }
            else
                Renderer.RedrawPreviousFrame();

            // Save a screenshot if F12 is pressed
            UpdateScreenshot();

#if DEBUG && (__MOBILE__ || GET_FPS)
            // Draw frame rate in debug mode on mobile
            Renderer.DrawMobileFps(CurrentFrameRate);
#endif
        }

        private void UpdateScreenshot()
        {
#if WINDOWS || MONOMAC
            bool f12Pressed = false;
            KeyboardState keyState = Keyboard.GetState();
            f12Pressed = keyState.IsKeyDown(Keys.F12);

            if (f12Pressed && !F12Pressed)
            {
                string path = System.IO.Path.GetDirectoryName(Global.GAME_ASSEMBLY.Location);
                path = Path.Combine(path, "Screenshots");

                try
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    string filename = System.IO.Path.GetFileNameWithoutExtension(Global.GAME_ASSEMBLY.Location) + "_";
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
                catch (UnauthorizedAccessException e)
                {
                    Global.play_se(System_Sounds.Buzzer);
                    return;
                }
            }
            F12Pressed = f12Pressed;
#endif
        }
        #endregion

        private void EndMoveRangeThread()
        {
            if (MoveRangeUpdateThread != null)
            {
                MoveRangeUpdateThread.Abort();
                MoveRangeUpdateThread.Join();
            }
        }

        private void EndThreads()
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
            foreach (var thread in MetricsThreads)
            {
                thread.Abort();
                thread.Join();
            }
        }

        #region ISaveCallbacker
        public void TitleLoadScene()
        {
            ChangeScene("Scene_Title_Load");
        }
        public void ArenaScene()
        {
            ChangeScene("Scene_Arena");
        }
        
        public void StartMoveRangeThread()
        {
            if (MoveRangeUpdateThread != null)
            {
                EndMoveRangeThread();
            }
            MoveRangeUpdateThread = new Thread(new ThreadStart(Global.map_update_move_range_loop));
            MoveRangeUpdateThread.Name = "Move range update";
            MoveRangeUpdateThread.Start();
        }
        public void CloseMoveRangeThread()
        {
            EndMoveRangeThread();
            MoveRangeUpdateThread = null;
        }

        public void FinishLoad()
        {
            var keyState = new KeyboardState();
            var controllerState = new GamePadState();
            Global.update_input(Game, new GameTime(), Game.IsActive, keyState, controllerState);
        }
        #endregion
    }

    public interface IMetricsService
    {
        TactileLibrary.Maybe<bool> send_data(string query, string post);
        bool test_connection();
    }

    public interface IUpdateService
    {
        string GameDownloadUrl { get; }
        Tuple<Version, DateTime, string> check_for_update();
        bool test_connection();
    }
}
