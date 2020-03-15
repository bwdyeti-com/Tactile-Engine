using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FEXNA;

namespace FE7x
{
    public class Game1 : Game
    {
        private GameLoop Loop;

#if DEBUGMONITOR
        Debug_Monitor.DebugMonitorForm MonitorForm;
#endif
        
#if __ANDROID__ 
        protected bool Started;
        public bool started { get { return Started; } }
        protected bool In_Background, Ready_To_Resume;
        protected bool Has_Been_Backgrounded;
#endif

        public Game1(string[] args)
        {
            var gameAsm = Assembly.GetExecutingAssembly();
            Global.GAME_ASSEMBLY = gameAsm;

#if DEBUG //FEGame
            // Just some code to ensure I don't accidently distribute FE7x credentials, remove after scrubbing
            System.Diagnostics.Debug.Assert(gameAsm.ManifestModule.Name == "FE7x.exe", "whoops mistakes");
#endif

            Global.RUNNING_VERSION = Assembly.GetAssembly(typeof(Global)).GetName().Version;
            SetInitialFramerateValues();

            var fullscreenService = new FullscreenService(this);
            var metricsHandler = new Metrics_Handler();
            var updateChecker = new Update_Checker();
            Loop = new GameLoop(this, fullscreenService, metricsHandler, updateChecker);
            
            Content.RootDirectory = "Content";

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
                            string map, units, unitsSource;

                            map = args[1];
                            units = args[2];
                            if (args.Length == 4)
                                unitsSource = args[3];
                            else
                                unitsSource = units;

                            Loop.SetUnitEditorValues(map, units, unitsSource);
                        }
                        break;
#endif
                }
        }
        private void SetInitialFramerateValues()
        {
            this.TargetElapsedTime = TimeSpan.FromTicks(
                (long)(TimeSpan.TicksPerSecond * (1.0f / (float)GameLoop.FRAME_RATE)));

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
            base.Initialize();

            Loop.Initialize();

            this.IsMouseVisible = true;

#if DEBUGMONITOR
            open_debug_monitor();
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

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Loop.LoadContent(this);
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

            Loop.UnloadContent();

            // Cancel controller rumble
            Global.Rumble.StopRumble();
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            Loop.GameGainedFocus();

            base.OnActivated(sender, args);
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            Loop.GameLostFocus();

            base.OnDeactivated(sender, args);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            Loop.CancelGraphicsLoadingThread();

            Global.Rumble.StopRumble();

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

#if __ANDROID__
            if (In_Background)
            {
                if (Ready_To_Resume)
                {
                    Loop.SetRenderTargets();
                    // Recreate dynamic textures here
                    In_Background = false;
                }
                Ready_To_Resume = false;
                return;
            }
#endif

            foreach (var time in Loop.Update(gameTime))
                base.Update(time);

            this.IsMouseVisible = FEXNA.Input.MouseVisible || Loop.Paused;
            
            update_debug_monitor();
        }

        private void update_debug_monitor()
        {
            KeyboardState key_state = Keyboard.GetState();

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
            bool offFrame = false;
#if __ANDROID__
            // Halves draw rate in Android
            // replace false with some global override for forcing full frame rate
            offFrame = Off_Draw_Frame && !(false);
#endif

            Loop.Draw(gameTime, offFrame);
            base.Draw(gameTime);

#if __ANDROID__
            Off_Draw_Frame = !Off_Draw_Frame;
#endif

#if DEBUGMONITOR
            if (MonitorForm != null)
                MonitorForm.invalidate_monitor();
#endif
        }
        #endregion
    }
}
