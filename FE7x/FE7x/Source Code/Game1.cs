using System;
using System.Reflection;
using System.Collections.Generic;
//#if DEBUG
using System.Diagnostics;
//#endif
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
#if MONOGAME
using Microsoft.Xna.Framework.Audio;
#else
using MonoGame.Framework.Audio;
#endif
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using FEXNA;
using FEXNAVersionExtension;

using System.Runtime.InteropServices; //fullscreen test

namespace FE7x
{
    public class Game1 : Game
    {
        #region Constants
        public const int FRAME_RATE = 60;
        readonly static Version OLDEST_ALLOWED_SUSPEND_VERSION = new Version(0, 6, 7, 0);
        readonly static Version OLDEST_ALLOWED_SAVE_VERSION = new Version(0, 4, 4, 0);

        const string SAVESTATE_FILENAME = "savestate1";
        const string SUSPEND_FILENAME = "suspend";
        const string SAVE_LOCATION = "Save";

        const int ZOOM_MAX = 5;
        const int ZOOM_MIN = 1;
        public const int Window_Width = Config.WINDOW_WIDTH;
        public const int Window_Height = Config.WINDOW_HEIGHT;

#if DEBUG //Cheat codes
        const bool HYPER_SPEED_ENABLED = true;
        const bool PAUSE_ENABLED = true;
        const bool SAVESTATE_ENABLED = true;
#else
        const bool HYPER_SPEED_ENABLED = false; // This is prone to softlocking, don't turn it on for release
        const bool PAUSE_ENABLED = false;
        const bool SAVESTATE_ENABLED = false;
#endif
        const int HYPER_SPEED_MULT = 15;

        const int RENDER_TARGETS = 4;

#if WINDOWS || MONOMAC
        const bool METRICS_ENABLED = Config.METRICS_ENABLED;
#else
        const bool METRICS_ENABLED = false;
#endif
        const int METRICS_SENDING_ATTEMPTS = 1;
        #endregion

        #region Fields
#if WINDOWS || MONOMAC
        static int ZOOM = 1;
#elif XBOX
        static int ZOOM = 2;
#else
        static int ZOOM = 1;
#endif

        protected int Savestate_File_Id = -1;
        protected bool Savestate_Testing = false;

        protected int Screen_Width, Screen_Height;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Camera camera;
        Rectangle Screen_Draw_Rect;

        StorageDevice device;
        IAsyncResult result;

        bool Paused = false;
        bool Pause_Pressed = false, Next_Pressed = false;
        bool Frame_Advanced = false;
        bool F1_Pressed = false, F12_Pressed = false;

        KeyboardState PreviousKeyState;

        // Textures
        RenderTarget2D[] ShaderRenderTargets = new RenderTarget2D[RENDER_TARGETS];
        RenderTarget2D Stereoscopic_Render_Target;
        RenderTarget2D FinalRender;

        private float TouchCursorOpacity;
        private Vector2 TouchCursorLoc;
        private Texture2D MouseCursorTexture;

        Thread MoveRangeUpdateThread = null, GraphicsLoadingThread = null, UpdateCheckThread = null;
        List<Thread> MetricsThreads = new List<Thread>();
        object MetricsLock = new object();

        protected bool Quick_Load = false;
#if DEBUG || GET_FPS

        protected Stopwatch FramerateStopWatch = new Stopwatch();
        protected TimeSpan FramerateTime = new TimeSpan();
        protected int FramerateFrames = 0;
        protected double CurrentFrameRate = FRAME_RATE;
#endif
#if DEBUG
        const bool DEBUG_VIEW = true;

#endif
#if DEBUGMONITOR
        Debug_Monitor.DebugMonitorForm MonitorForm;
#endif
        protected Stopwatch Process_Stop_Watch = new Stopwatch();
        protected float Process_Time = 0;
        protected int Process_Frames = 0;

        protected Stopwatch Hyperspeed_Stop_Watch = new Stopwatch();

#if DEBUG
        protected string Map_Editor_Map, Map_Editor_Units, Map_Editor_Units_Source;
#endif

        public static int FILE_ID = 1;
        private static Assembly GAME_ASSEMBLY { get { return Global.GAME_ASSEMBLY; } }

        private bool STARTING = true;
        #endregion

        #region Accessors
        public static int zoom
        {
            get { return Global.fullscreen ? 2 : ZOOM; }
            private set { ZOOM = value; }
        }

        public static int render_target_zoom
        {
            get { return 1; }// Global.fullscreen ? 2 : 1; } //Yeti
        }
        #endregion

        //fullscreen test
        #region fullscreen
        private bool fullscreen = false;
#if WINDOWS && !MONOGAME

        private void FullScreen()
        {
            Fullscreen.fullscreen(Window.Handle);
        }
        private void Restore()
        {
            System.Windows.Forms.Form.FromHandle(Window.Handle).FindForm().WindowState =
                System.Windows.Forms.FormWindowState.Normal;
            System.Windows.Forms.Form.FromHandle(Window.Handle).FindForm().FormBorderStyle =
                System.Windows.Forms.FormBorderStyle.FixedDialog;
            System.Windows.Forms.Form.FromHandle(Window.Handle).FindForm().TopMost = false;
        }
#elif __ANDROID__
        public static int STATUS_BAR_HEIGHT;

        protected bool Started;
        public bool started { get { return Started; } }
        protected bool In_Background, Ready_To_Resume;
        protected bool Has_Been_Backgrounded;
#endif
        #endregion

        public Game1(string[] args)
        {
            Global.GAME_ASSEMBLY = Assembly.GetExecutingAssembly();

            /*AppDomain newDomain4Process = AppDomain.CreateDomain("newDomain4Process");
            string wuh = Path.GetDirectoryName(GAME_ASSEMBLY.Location);
            //Assembly processLibrary = newDomain4Process.Load(Path.Combine(Path.GetDirectoryName(GAME_ASSEMBLY.Location), "OggSharpLibrary.dll"));
            Assembly processLibrary = newDomain4Process.Load("FEXNA.dll");
            AppDomain.Unload(newDomain4Process);*/


#if WINDOWS || MONOMAC
            if (METRICS_ENABLED)
                Metrics_Handler.enable(GAME_ASSEMBLY);
#endif

            Global.RUNNING_VERSION = System.Reflection.Assembly.GetAssembly(typeof(Global)).GetName().Version;
            graphics = new GraphicsDeviceManager(this);
            set_initial_resolution();

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

        /* //Debug
        private string game_name()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            if (assembly == null)
            {
                var methodFrames = new StackTrace().GetFrames().Select(t => t.GetMethod()).ToArray();
                MethodBase entryMethod = null;
                // Find Game.Update method
                for (int i = 0; i + 1 < methodFrames.Length; i++)
                {
                    var method = methodFrames[i] as MethodInfo;
                    var calling_method = methodFrames[i + 1] as MethodInfo;
                    if (method == null || calling_method == null)
                        continue;

                    if (method.Name == "Update" && method.ReturnType == typeof(void) &&
                            calling_method.Name == "DoUpdate" &&
                            calling_method.ReturnType == typeof(void))
                        entryMethod = method;
                }

                if (entryMethod == null)
                    return "this game";

                assembly = entryMethod.Module.Assembly;
            }

            return string.Format("\"{0}\"", assembly.GetName().Name);
        }*/

        protected void set_initial_resolution()
        {
#if __ANDROID__
            Screen_Width = graphics.PreferredBackBufferWidth;
            Screen_Height = graphics.PreferredBackBufferHeight - STATUS_BAR_HEIGHT;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
#else
            Screen_Width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            Screen_Height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
#endif
            int zoom_max = Math.Min( // this is a bad solution, need a catch block that's elegant //Yeti
                ZOOM_MAX, Screen_Width / Window_Width);
            zoom_max = Math.Min(
                zoom_max, (Screen_Height - 64) / Window_Height);

            Global.set_zoom_limits(ZOOM_MIN, zoom_max);
            Global.zoom = ZOOM;

            //this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / (float)FRAME_RATE); //Debug
            this.TargetElapsedTime = TimeSpan.FromTicks(
                (long)(TimeSpan.TicksPerSecond * (1.0f / (float)FRAME_RATE)));
            refresh_zoom(true);

            // This was commented out, for some reason I've forgotten
            // But commenting this out causes multiple instances to lag each other //Debug
            graphics.SynchronizeWithVerticalRetrace = false;

            IsFixedTimeStep = false;
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

            camera = new Camera(Window_Width, Window_Height, Vector2.Zero);
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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            MouseCursorTexture =
                Content.Load<Texture2D>(@"Graphics/Pictures/MouseCursor");

            Global.init(this, Content, Services);

            //Graphics_Loading_Thread = new Thread(new ThreadStart(load_graphics_content));
            //Graphics_Loading_Thread.Start();

            refresh_effect_projection();
            initialize_text_glow();
            set_render_targets();
        }

        private void refresh_effect_projection()
        {
            if (Global.effect_shader() != null)
            {
                Effect shader = Global.effect_shader();
                SetBlurEffectParameters(shader, 1f / (float)Window_Width, 1f / (float)Window_Height);
                /*Matrix projection = Matrix.CreateOrthographicOffCenter(
                    0, Window_Width * render_target_zoom, Window_Height * render_target_zoom, 0, -10000, 10000);
                Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

                Global.effect_shader().Parameters["World"].SetValue(Matrix.Identity);
                Global.effect_shader().Parameters["View"].SetValue(Matrix.Identity);
                Global.effect_shader().Parameters["Projection"].SetValue(halfPixelOffset * projection);
                
                Global.effect_shader().Parameters["MatrixTransform"].SetValue(Matrix.Identity);*/
            }
        }

        #region Blur Calculation
        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        /// <remarks>
        /// This function was originally provided in the BloomComponent class in the 
        /// Bloom Postprocess sample.
        /// </remarks>
        void SetBlurEffectParameters(Effect effect, float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = effect.Parameters["SampleWeights"];
            offsetsParameter = effect.Parameters["SampleOffsets"];

            if (weightsParameter != null && offsetsParameter != null)
            {
                // Look up how many samples our gaussian blur effect supports.
                int sampleCount = weightsParameter.Elements.Count;

                // Create temporary arrays for computing our filter settings.
                float[] sampleWeights = new float[sampleCount];
                Vector2[] sampleOffsets = new Vector2[sampleCount];

                // The first sample always has a zero offset.
                sampleWeights[0] = ComputeGaussian(0);
                sampleOffsets[0] = new Vector2(0);

                // Maintain a sum of all the weighting values.
                float totalWeights = sampleWeights[0];

                // Add pairs of additional sample taps, positioned
                // along a line in both directions from the center.
                for (int i = 0; i < sampleCount / 2; i++)
                {
                    // Store weights for the positive and negative taps.
                    float weight = ComputeGaussian(i + 1);

                    sampleWeights[i * 2 + 1] = weight;
                    sampleWeights[i * 2 + 2] = weight;

                    totalWeights += weight * 2;

                    // To get the maximum amount of blurring from a limited number of
                    // pixel shader samples, we take advantage of the bilinear filtering
                    // hardware inside the texture fetch unit. If we position our texture
                    // coordinates exactly halfway between two texels, the filtering unit
                    // will average them for us, giving two samples for the price of one.
                    // This allows us to step in units of two texels per sample, rather
                    // than just one at a time. The 1.5 offset kicks things off by
                    // positioning us nicely in between two texels.
                    float sampleOffset = i * 2 + 1.5f;

                    Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                    // Store texture coordinate offsets for the positive and negative taps.
                    sampleOffsets[i * 2 + 1] = delta;
                    sampleOffsets[i * 2 + 2] = -delta;
                }

                // Normalize the list of sample weightings, so they will always sum to one.
                for (int i = 0; i < sampleWeights.Length; i++)
                {
                    sampleWeights[i] /= totalWeights;
                }

                // Tell the effect about our new filter settings.
                weightsParameter.SetValue(sampleWeights);
                offsetsParameter.SetValue(sampleOffsets);
            }
        }

        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        /// <remarks>
        /// This function was originally provided in the BloomComponent class in the 
        /// Bloom Postprocess sample.
        /// </remarks>
        static float ComputeGaussian(float n)
        {
            const float BLUR_AMOUNT = 2f;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * BLUR_AMOUNT)) *
                           Math.Exp(-(n * n) / (2 * BLUR_AMOUNT * BLUR_AMOUNT)));
        }
        #endregion

        private void set_render_targets()
        {
            dispose_render_targets();
            try
            {
                for (int i = 0; i < RENDER_TARGETS; i++)
                {
                    RenderTarget2D target = CloneRenderTarget(graphics.GraphicsDevice, 1);
                    ShaderRenderTargets[i] = target;
                }
                if (true)//fullscreen)
                    Stereoscopic_Render_Target = CloneRenderTarget(graphics.GraphicsDevice, 1, true);
                FinalRender = CloneRenderTarget(graphics.GraphicsDevice, 1);
            }
            catch (OutOfMemoryException e)
            {
                Global.zoom = 1;
                dispose_render_targets();
                return;
            }
        }

        public void dispose_render_targets()
        {
            foreach (RenderTarget2D render_target in ShaderRenderTargets)
                if (render_target != null)
                    render_target.Dispose();
            if (Stereoscopic_Render_Target != null)
                Stereoscopic_Render_Target.Dispose();
            if (FinalRender != null)
                FinalRender.Dispose();

            for (int i = 0; i < ShaderRenderTargets.Length; i++)
                ShaderRenderTargets[i] = null;
            Stereoscopic_Render_Target = null;
            FinalRender = null;
            try
            {
                graphics.GraphicsDevice.Present();
            }
#if MONOGAME
            catch (Exception e)
            {
                throw;
#else
            catch (DeviceLostException e)
            {
                System.Threading.Thread.Sleep(500);
#endif
            }
        }

        private static RenderTarget2D CloneRenderTarget(GraphicsDevice device, int numberLevels)
        {
            return CloneRenderTarget(device, numberLevels, false);
        }
        private static RenderTarget2D CloneRenderTarget(GraphicsDevice device, int numberLevels, bool stereo)
        {
            return new RenderTarget2D(device,
                //device.PresentationParameters.BackBufferWidth / zoom * (stereo ? 2 : 1),
                //device.PresentationParameters.BackBufferHeight / zoom,
                Window_Width * render_target_zoom * (stereo ? 2 : 1),
                Window_Height * render_target_zoom,
                false,
#if MONOGAME
                SurfaceFormat.Color,
#else
                device.DisplayMode.Format,
#endif
                DepthFormat.None,
                0,
                RenderTargetUsage.PreserveContents);
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
            if (MoveRangeUpdateThread != null)
            {
                end_move_range_thread();
            }
            end_threads();
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
                    if (Global.Input.touch_pressed(false))
                    {
                        TouchCursorLoc = Global.Input.touchPressPosition;
                        TouchCursorOpacity += 0.5f;
                    }
                    else
                    {
                        TouchCursorOpacity -= 0.075f;
                    }
                    TouchCursorOpacity = MathHelper.Clamp(TouchCursorOpacity, 0, 1);
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
                //update_text_glow();

                update_io();
                update_scene_change();
                if (Global.scene != null)
                {
                    // Suspend
                    update_suspend();
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
            update_screen_scale();
            update_text_glow(i + 1);
            Hyperspeed_Stop_Watch.Stop(); //Debug

            PreviousKeyState = key_state;
        }

        private void update_open_game_site()
        {
            if (Global.VisitGameSiteCall)
            {
                System.Diagnostics.Process.Start(
                    string.Format("http://{0}", Update_Checker.GAME_DOWNLOAD));
#if !__MOBILE__
                if (this.fullscreen)
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

        private void update_screen_scale()
        {
            Vector2 game_size = new Vector2(graphics.PreferredBackBufferWidth,
                graphics.PreferredBackBufferHeight);
            Vector2 render_size = new Vector2(
                ShaderRenderTargets[0].Width,
                ShaderRenderTargets[0].Height);
            Vector2 window_size = new Vector2(
                graphics.GraphicsDevice.DisplayMode.Width,
                graphics.GraphicsDevice.DisplayMode.Height);
#if __MOBILE__
            FEXNA.Input.update_screen_scale(true ? 
#else
            FEXNA.Input.update_screen_scale(Global.fullscreen ?
#endif
 game_size / render_size : new Vector2(Global.zoom),
                window_size);
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

            // If zoom value changed
            if (Global.zoom != ZOOM || this.fullscreen != Global.fullscreen)//fullscreen test // graphics.IsFullScreen)
            {
                refresh_zoom();
                // Create new properly sized render targets
                set_render_targets();
                return true;
            }

            return false;
        }

        private void refresh_zoom()
        {
            refresh_zoom(false);
        }
        private void refresh_zoom(bool initial_set)
        {
            ZOOM = Global.zoom;
            // Resize window
            if (this.fullscreen != Global.fullscreen) //fullscreen test
            {
                this.fullscreen = Global.fullscreen;
#if !MONOGAME
                if (this.fullscreen)
                    FullScreen();
                else
                    Restore();
#elif MONOMAC || WINDOWS
#if MONOMAC
                bool regain_focus = fullscreen != graphics.IsFullScreen;
#endif
                graphics.IsFullScreen = fullscreen;
#if MONOMAC
                // going to or from fullscreen loses focus on the window, it's still on the program? //Yeti
                //if (regain_focus)
                //    this.Window.MakeCurrent();
#endif

#endif
            }
#if !MONOGAME
            //graphics.IsFullScreen = Global.fullscreen;
            graphics.PreferredBackBufferWidth = (int)(Global.fullscreen ? Fullscreen.ScreenX(Window.Handle) : Window_Width * zoom);
            graphics.PreferredBackBufferHeight = (int)(Global.fullscreen ? Fullscreen.ScreenY(Window.Handle) : Window_Height * zoom);
#elif MONOMAC || WINDOWS
            graphics.PreferredBackBufferWidth = (int)(Global.fullscreen ? GraphicsDevice.DisplayMode.Width : Window_Width * zoom);
            graphics.PreferredBackBufferHeight = (int)(Global.fullscreen ? GraphicsDevice.DisplayMode.Height : Window_Height * zoom);
#elif __MOBILE__
            graphics.PreferredBackBufferWidth = Screen_Width;
            graphics.PreferredBackBufferHeight = Screen_Height;
#else
            graphics.PreferredBackBufferWidth = (int)(Window_Width * zoom);
            graphics.PreferredBackBufferHeight = (int)(Window_Height * zoom);
#endif
            if (!initial_set)
            {
                refresh_effect_projection();
                graphics.ApplyChanges();
            }
            //graphics.GraphicsDevice.Viewport = new Viewport(
            //    0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
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
            TimeSpan elapsed = Paused ? TimeSpan.FromMilliseconds(1f / Config.FRAME_RATE) : FramerateStopWatch.Elapsed;
            FramerateTime += elapsed.TotalSeconds >= 1 ? TimeSpan.FromSeconds(1) : elapsed;
            //FramerateTime += Math.Min(1, Paused ? (1f / Config.FRAME_RATE) :
            //    (float)(FramerateStopWatch.ElapsedTicks / ((double)Stopwatch.Frequency)));
            FramerateFrames++;
            FramerateStopWatch.Restart();
            if (FramerateTime.TotalSeconds >= 1)
            {
                CurrentFrameRate = FramerateTime.TotalSeconds * FramerateFrames;
                FramerateTime -= TimeSpan.FromSeconds(1);
                // I think I'm using the wrong method for this, should be handle_frame_timing() //Yeti
                //if (FramerateTime.TotalSeconds > 5)
                //    FramerateTime = TimeSpan.FromSeconds(5);
                FramerateFrames = 0;

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
            bool f1_pressed = false;
            if (SAVESTATE_ENABLED)
            {
                if (!Paused)
                {
                    KeyboardState key_state = Keyboard.GetState();
                    f1_pressed = key_state.IsKeyDown(Keys.F1);
                    if (f1_pressed && !F1_Pressed && (key_state.IsKeyDown(Keys.LeftShift) || key_state.IsKeyDown(Keys.LeftShift)))
                    {
                        if (Global.savestate_ready)
                        {
                            Global.play_se(System_Sounds.Confirm);
                            Global.scene.suspend();
                            Global.savestate = true;
                        }
                        else
                            Global.play_se(System_Sounds.Buzzer);
                        F1_Pressed = true;
                    }
                }
            }

            while (true)
            {
                // Select storage
                if ((Global.Loading_Suspend || Global.load_save_file || Global.load_save_info) && device == null)
                    Global.storage_selection_requested = true;

                if (Global.storage_selection_requested)
                {
#if XBOX
                if (!Guide.IsVisible)
#endif
                    {
                        result = StorageDevice.BeginShowSelector(null, null);
#if !XBOX
                        while (!result.IsCompleted)
                            result = StorageDevice.BeginShowSelector(null, null);
#endif
                    }
                    if (result.IsCompleted)
                    {
                        device = StorageDevice.EndShowSelector(result);
                        Global.storage_selection_requested = false;
                        // If no device selected and trying to load suspend
                        if (device == null && Global.Loading_Suspend)
                        {
                            Global.play_se(System_Sounds.Buzzer);
                            Global.reset_suspend_load();
                        }
                    }
                    else
                        break;
                }
                // Saving suspends or files is handled in a method below, this handles more utility actions and loading
                // Config
                else if (Global.load_config)
                {
                    if (device != null && device.IsConnected)
                    {
                        load_config();
                    }
                    Global.load_config = false;
                }
                else if (Global.save_config)
                {
                    if (device != null && device.IsConnected)
                    {
                        save_config();
                    }
                    Global.save_config = false;
                }
                #region Delete (File, Map Save, Suspend)
                // Delete File
                else if (Global.delete_file)
                {
                    Global.file_deleted();
                    if (device != null && device.IsConnected)
                    {
                        Global.save_files_info.Remove(Global.start_game_file_id);
                        if (Global.suspend_file_info != null &&
                                Global.suspend_file_info.save_id == Global.start_game_file_id)
                            Global.suspend_file_info = null;

                        delete_save_file(Global.start_game_file_id);
                        Global.start_game_file_id = -1;
                        // Rechecks the most recent suspend and reloads save info
                        load_suspend_info();
                        load_save_info();
                        Global.load_save_info = false;
                    }
                }
                // Delete Map Save
                else if (Global.delete_map_save)
                {
                    Global.map_save_deleted();
                    if (device != null && device.IsConnected)
                    {
                        // Same question as below //Yeti
                        delete_file(map_save_filename(FILE_ID));
                        delete_file(suspend_filename(FILE_ID));
                        // Rechecks the most current suspend
                        load_suspend_info(update_file_id: false);
                        // Updates the save info for the selected file
                        load_save_info();
                        Global.load_save_info = false;
                    }
                }
                // Delete Suspend
                else if (Global.delete_suspend)
                {
                    Global.suspend_deleted();
                    if (device != null && device.IsConnected)
                    {
                        delete_file(suspend_filename(FILE_ID));
                        // Rechecks the most current suspend
                        load_suspend_info(update_file_id: false);
                        // Updates the save info for the selected file
                        load_save_info();
                        Global.load_save_info = false;
                    }
                }
                #endregion
                #region Load (Save Info, Suspend, Save File)
                // Load Save Info
                else if (Global.load_save_info)
                {
                    if (device != null && device.IsConnected)
                    {
                        // Load progress data
                        load_progress();
                        // Rechecks the most recent suspend and reloads save info
                        load_suspend_info();
                        load_save_info();
                    }
                    Global.load_save_info = false;
                }
                // Load Suspend
                else if (Global.Loading_Suspend)
                {
                    if (device != null && device.IsConnected)
                    {
                        // If a specific file is selected, use it
                        if (Global.start_game_file_id != -1)
                        {
                            FILE_ID = Global.start_game_file_id;
                            Global.start_game_file_id = -1;
                        }
                        // Otherwise loading the most recent suspend
                        else
                            FILE_ID = Global.suspend_file_info.save_id;
                        // Load save file first, then test if the suspend works
                        if (load_file() && test_load(true))
                        {
                            // Then load relevant suspend
                            if (Global.scene.scene_type != "Scene_Soft_Reset")
                            {
                                Global.cancel_sound();
                                Global.play_se(System_Sounds.Confirm);
                            }
                            load();
#if DEBUG
                            if (Savestate_File_Id != -1)
                            {
                                FILE_ID = Savestate_File_Id;
                                DEBUG_FILE_ID_TEST = FILE_ID;
                                Savestate_File_Id = -1;
                            }
#endif
                        }
                        else
                        {
                            if (Global.scene.scene_type != "Scene_Soft_Reset")
                                Global.play_se(System_Sounds.Buzzer);
                            Global.scene.reset_suspend_filename();
                        }
                    }
                    else
                        Global.play_se(System_Sounds.Buzzer);
                    Global.reset_suspend_load();
                }
                // Load Save File
                else if (Global.load_save_file)
                {
                    if (device != null && device.IsConnected)
                    {
                        if (Global.start_game_file_id != -1)
                        {
                            FILE_ID = Global.start_game_file_id;
                            Global.start_game_file_id = -1;
                        }
                        DEBUG_FILE_ID_TEST = FILE_ID;
                        load_file();
                        Global.game_options.post_read();
                    }
                    else
                    {
                        reset_file();
                    }
                    Global.load_save_file = false;
                }
                #endregion
                // Copy File
                else if (Global.copying)
                {
                    // Why is there a breakpoint here though //Debug
                    Global.copying = false;
                    if (device != null && device.IsConnected)
                    {
                        Global.save_files_info[Global.move_to_file_id] = new FEXNA.IO.Save_Info(Global.save_files_info[Global.start_game_file_id]);
                        Global.save_files_info[Global.move_to_file_id].reset_suspend_exists();

                        move_file(Global.start_game_file_id, Global.move_to_file_id, true);
                        Global.start_game_file_id = -1;
                        Global.move_to_file_id = -1;

                    }
                }
                // Move File
                else if (Global.move_file)
                {
                    // Why is there a breakpoint here though //Debug
                    Global.move_file = false;
                    if (device != null && device.IsConnected)
                    {
                        Global.save_files_info[Global.move_to_file_id] = new FEXNA.IO.Save_Info(Global.save_files_info[Global.start_game_file_id]);
                        Global.save_files_info.Remove(Global.start_game_file_id);

                        move_file(Global.start_game_file_id, Global.move_to_file_id);
                        Global.start_game_file_id = -1;
                        Global.move_to_file_id = -1;
                    }
                }
                else
                {
                    // Load suspend with F1, in Debug
                    if (SAVESTATE_ENABLED)
                    {
                        if (Global.savestate_load_ready)
                            if (f1_pressed && !F1_Pressed)
                            {
                                if (device != null && device.IsConnected)
                                {
                                    Quick_Load = true;
                                    // Tells the game to ignore suspend id/current save id disparity temporarily
                                    Savestate_Testing = true;
                                    // Checks if the suspend can be loaded, and sets Savestate_File_Id to its file id
                                    if (test_load(SAVESTATE_FILENAME + Config.SAVE_FILE_EXTENSION, true))
                                    {
                                        FILE_ID = Savestate_File_Id;
                                        DEBUG_FILE_ID_TEST = FILE_ID;
                                        Savestate_File_Id = -1;
                                        load_file();
                                        Global.cancel_sound();
                                        load(SAVESTATE_FILENAME + Config.SAVE_FILE_EXTENSION);
                                        Global.current_save_id = FILE_ID;
                                    }
                                    else
                                    {
                                        if (Global.scene.scene_type == "Scene_Title")
                                            Global.play_se(System_Sounds.Buzzer);
                                        Global.scene.reset_suspend_filename();
                                    }
                                    Savestate_Testing = false;
                                }
                            }
                    }
                    break;
                }
            }
            F1_Pressed = f1_pressed;
        }
        protected int DEBUG_FILE_ID_TEST;

        private void update_scene_change()
        {
            // Change scene
            if (Global.new_scene != "")
            {
                change_scene(Global.new_scene);
            }
        }

        private void update_suspend()
        {
            if (Global.scene.suspend_calling && !Global.scene.suspend_blocked())
            {
                if (device != null && device.IsConnected)
                {
                    bool map_save = Global.scene.is_map_save_filename;
                    if (SAVESTATE_ENABLED && Global.savestate)
                    {
                        save(SAVESTATE_FILENAME + Config.SAVE_FILE_EXTENSION);
                        Global.savestate = false;
                    }
                    else
                    {
                        if (save())
                            if (map_save)
                                Global.map_save_created();
                    }
                    // Write progress data
                    save_progress();
                    Global.scene.reset_suspend_calling();
                    Global.scene.reset_suspend_filename();
                    if (Global.return_to_title)
#if DEBUG
                        if (Global.UnitEditorActive)
                            Global.scene_change("Scene_Map_Unit_Editor");
                        else
#endif
                            change_scene("Scene_Title_Load");
                }
            }
            if (Global.scene.save_data_calling)
            {
                if (device != null && device.IsConnected)
                {
                    if (Global.start_new_game)
                    {
                        FILE_ID = Global.start_game_file_id;
                        DEBUG_FILE_ID_TEST = FILE_ID;

                        Global.start_new_game = false;
                        Global.start_game_file_id = -1;
                    }
                    if (DEBUG_FILE_ID_TEST != FILE_ID)
                    {
                        throw new Exception();
                    }
                    save_file(FILE_ID);
                    // Update and write progress data
                    Global.progress.update_progress(Global.save_file);
                    save_progress();
                    Global.scene.EndSaveData();
                }
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
                    /*// Waits for 
                    if (MetricsThread != null)
                    {
                        if (MetricsThread.ThreadState == System.Threading.ThreadState.Running)
                        { }
                        MetricsThread.Join();
                        MetricsThread = null;
                    }*/
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
                            //MetricsThread = new Thread(() => send_metrics_to_server(Global.metrics_data, Global.metrics_gameplay_data));
                            //MetricsThread.Start();
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
                    Global.current_save_id = FILE_ID;
#if DEBUG
                    if (new_scene == "Start_Game" || new_scene == "Debug_Start")
                    {
                        if (new_scene == "Debug_Start")
                        {
                            DEBUG_FILE_ID_TEST = FILE_ID = 1;
                            Global.current_save_id = FILE_ID;
                        }
                        new_scene = "Start_Game";
#else
                    if (new_scene == "Start_Game")
                    {
#endif
                        load_file();
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
                        // Switch to the appropriate save file
                        FILE_ID = Global.current_save_id;
                        load_file();
                        Global.game_options.post_read();
                        Global.game_temp = new Game_Temp();



                        end_move_range_thread();

                        Global.start_unit_editor_playtest();

                        move_range_update_thread();
                    }
                    break;
#endif
                case "Load_Suspend":
                    Global.current_save_id = FILE_ID;

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
                    //Global.Audio.clear_map_theme(); //@Debug
                    Global.change_to_new_scene("Scene_Title_Load");
                    break;
                case "Scene_Title":
                    //Global.Audio.clear_map_theme(); //@Debug
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
                Global.Audio.stop_bgs(); //Debug
                Global.Audio.stop_me(); //Debug
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
        // Theoretically a dictionary of color arrays representing the pixel data of each glowing font texture, not actually used??? //Yeti
        private Dictionary<string, Color[]> glow_data = new Dictionary<string, Color[]>();
        // A dictionary of color arrays representing the pixel data of each glowing font texture at each glow frame
        private Dictionary<string, Color[][]> glow_data2 = new Dictionary<string, Color[][]>();
        private void initialize_text_glow()
        {
            foreach (string name in TEXT_GLOW.Keys)
            {
                Texture2D green_text = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + name);
                //glow_data[name] = new Color[green_text.Width * green_text.Height]; //Yeti
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

            //if (Glow_Timer % 4 == 0) // Debug
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
            Screen_Draw_Rect = new Rectangle(0, 0, Window_Width, Window_Height);
#if __ANDROID__
            // Halves draw rate in Android
            // replace false with some global override for forcing full frame rate
            if ((!Off_Draw_Frame || false) && (!Paused || Frame_Advanced))
#else
            if (!Paused || Frame_Advanced)
#endif
            {
                Frame_Advanced = false;
                // Reset data
                Global.palette_pool.update();
                if (ShaderRenderTargets[0] == null)
                    return;
                clear_render_targets();

                camera.pos = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
                camera.offset = new Vector2(ShaderRenderTargets[0].Width / 2, ShaderRenderTargets[0].Height / 2);
                Vector2 ratio = new Vector2(screen_size_ratio);
                camera.zoom = ratio;
                //camera.zoom = Vector2.One * 4;// ratio;
                //camera.zoom = Vector2.One;

                // Always draw the screen normally to FinalRender, for screenshotting/suspend images
                draw_scene(spriteBatch, Stereoscopic_Mode.Center);
                // Copy render to final render
                FEXNARenderTarget2DExtension.RenderTarget2DExtensions.raw_copy_render_target(
                    ShaderRenderTargets[0], spriteBatch, GraphicsDevice, FinalRender);

                // Draws scene
                if (!Global.stereoscopic)
                {
                    // Draw rendertarget to screen
                    draw_to_screen(spriteBatch);
                }
                else
                {
                    // Clear the stereo render target to black
                    GraphicsDevice.SetRenderTarget(Stereoscopic_Render_Target);
                    GraphicsDevice.Clear(Color.Black);

                    draw_scene(spriteBatch, Stereoscopic_Mode.Left);
                    draw_scene(spriteBatch, Stereoscopic_Mode.Right);

                    // If Anaglyph, copy back to rendertarget[0] since it's only normal screen size
                    // Maybe the scene renderer should always draw to Stereoscopic_Render_Target as if it's non-anaglyph mode //Yeti
                    // And then this block would take the two halves and combine them on ShaderRenderTargets[0] in anaglyph //Yeti
                    if (Global.anaglyph_mode)
                    {
                        GraphicsDevice.SetRenderTarget(ShaderRenderTargets[0]);
                        GraphicsDevice.Clear(Color.Transparent);
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
                        spriteBatch.Draw(Stereoscopic_Render_Target, new Rectangle(
                            0, 0, Stereoscopic_Render_Target.Width / 2, Stereoscopic_Render_Target.Height),
                            new Rectangle(0, 0, ShaderRenderTargets[0].Width, ShaderRenderTargets[0].Height), Color.White);
                        spriteBatch.End();
                    }
                    // Else that thing is not true, so halve the x scale of the camera
                    else
                        camera.zoom *= new Vector2(0.5f, 1);

                    // Draw rendertarget to screen
                    if (Global.anaglyph_mode)
                        draw_to_screen(spriteBatch, ShaderRenderTargets[0]);
                    else
                        draw_to_screen(spriteBatch, Stereoscopic_Render_Target);
                }

                GraphicsDevice.SetRenderTarget(null);

                base.Draw(gameTime);
            }
            else
                draw_to_screen(spriteBatch, !Global.stereoscopic ? ShaderRenderTargets[0] : Stereoscopic_Render_Target);

            // Save a screenshot if F12 is pressed
            update_screenshot(FinalRender);

#if __ANDROID__
            Off_Draw_Frame = !Off_Draw_Frame;
#endif

#if DEBUG && (__MOBILE__ || GET_FPS)
            // Draw frame rate in debug mode on mobile
            var font = Global.Content.Load<SpriteFont>(@"DiagFont");
            string fps_text;
            Vector2 loc;
            float width_ratio = graphics.PreferredBackBufferWidth /
                (float)Config.WINDOW_WIDTH / 2;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            fps_text = string.Format("{0:0.0}fps", CurrentFrameRate);
            fps_text += string.Format("\nMemory: {0}KB", GC.GetTotalMemory(false) / 1024);
            loc = new Vector2(4, 4);
            spriteBatch.DrawString(
                font,
                fps_text,
                loc,
                Color.White,
                0f, Vector2.Zero, width_ratio, SpriteEffects.None, 0f);
            spriteBatch.End();
#endif

#if DEBUGMONITOR
            if (MonitorForm != null)
                MonitorForm.invalidate_monitor();
#endif
        }

        private float screen_size_ratio
        {
            get
            {
                if (graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight >
                        Window_Width / (float)Window_Height)
                    return graphics.PreferredBackBufferHeight / (float)ShaderRenderTargets[0].Height;
                else
                    return graphics.PreferredBackBufferWidth / (float)ShaderRenderTargets[0].Width;
            }
        }

        private void clear_render_targets()
        {
            foreach (RenderTarget2D render_target in ShaderRenderTargets)
            {
                GraphicsDevice.SetRenderTarget(render_target);
                GraphicsDevice.Clear(Color.Transparent);
            }
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Transparent);
        }

        private void draw_scene(SpriteBatch sprite_batch, bool clear_render_target = false)
        {
            GraphicsDevice.SetRenderTarget(ShaderRenderTargets[0]);
            if (clear_render_target)
                GraphicsDevice.Clear(Color.Transparent);
            Global.scene.draw(sprite_batch, GraphicsDevice, ShaderRenderTargets);
        }

        private void draw_scene(SpriteBatch sprite_batch, Stereoscopic_Mode stereo)
        {
            Stereoscopic_Graphic_Object.stereoscopic_view = stereo;
            draw_scene(spriteBatch, stereo == Stereoscopic_Mode.Right);

            if (stereo == Stereoscopic_Mode.Center)
                return;

            GraphicsDevice.SetRenderTarget(Stereoscopic_Render_Target);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            // In anaglyph, left is drawn in red channel, right is drawn cyan
            if (Global.anaglyph_mode)
            {
                Color anaglyph_color = stereo == Stereoscopic_Mode.Left ? new Color(255, 0, 0, 0) : new Color(0, 255, 255, 0);
                spriteBatch.Draw(ShaderRenderTargets[0], new Rectangle(
                    0, 0, Stereoscopic_Render_Target.Width / 2, Stereoscopic_Render_Target.Height),
                    new Rectangle(0, 0, ShaderRenderTargets[0].Width, ShaderRenderTargets[0].Height), anaglyph_color);
            }
            // Otherwise, the left channel is drawn on the left half of the screen and the right on the right, and the display sorts it out
            else
            {
                int x = stereo == Stereoscopic_Mode.Left ? 0 : Stereoscopic_Render_Target.Width / 2;
                    spriteBatch.Draw(ShaderRenderTargets[0], new Rectangle(
                        x, 0, Stereoscopic_Render_Target.Width / 2, Stereoscopic_Render_Target.Height),
                        new Rectangle(0, 0, ShaderRenderTargets[0].Width, ShaderRenderTargets[0].Height), Color.White);
            }
            spriteBatch.End();
        }

        private void draw_to_screen(SpriteBatch sprite_batch)
        {
            draw_to_screen(sprite_batch, ShaderRenderTargets[0]);
        }
        private void draw_to_screen(SpriteBatch sprite_batch, RenderTarget2D render_target)
        {
            RenderTarget2D stereo_target = null;
            if (render_target == Stereoscopic_Render_Target)
            {
                for (int i = 0; i < ShaderRenderTargets.Length; i++)
                {
                    GraphicsDevice.SetRenderTarget(ShaderRenderTargets[i]);
                    GraphicsDevice.Clear(Color.Transparent);
                }

                stereo_target = ShaderRenderTargets[RENDER_TARGETS - 2];
            }

            // Copy the render target to another render target and draw the mouse on it
            RenderTarget2D temp_target =
                render_target == ShaderRenderTargets[RENDER_TARGETS - 1] ?
                ShaderRenderTargets[RENDER_TARGETS - 2] :
                ShaderRenderTargets[RENDER_TARGETS - 1];

            if (Global.stereoscopic && !Global.anaglyph && stereo_target != null)
            {
                copy_mouse_render(
                    sprite_batch, render_target, temp_target, Stereoscopic_Mode.Left);
                copy_mouse_render(
                    sprite_batch, render_target, stereo_target, Stereoscopic_Mode. Right);
            }
            else
                copy_mouse_render(sprite_batch, render_target, temp_target, Stereoscopic_Mode.Center);

            GraphicsDevice.SetRenderTarget(temp_target);

            render_target = temp_target;

            Effect shader = Global.effect_shader();

            // Change back to the back buffer, so the final rendertarget is available as a texture
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            shader = Global.effect_shader(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            // Determines resize scale to best fit game ratio to window/screen size
            Vector2 width_ratio = new Vector2(graphics.PreferredBackBufferWidth / (float)render_target.Width);
            float ratio = screen_size_ratio;

            Matrix shader_matrix = screen_space_matrix(
                new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                new Vector2(render_target.Width, render_target.Height), width_ratio, ratio);
            shader.Parameters["MatrixTransform"].SetValue(shader_matrix);
            shader.CurrentTechnique = shader.Techniques["Normal"];

#if DEBUG && !MONOGAME
            // Draw a debug view screen
            if (DEBUG_VIEW && !Global.fullscreen && !Global.stereoscopic && zoom >= 3)
            {
                draw_debug_view(sprite_batch, render_target, shader);
            } else
#endif
            {
#if !__ANDROID__
                if (!Global.fullscreen && shader.CurrentTechnique == shader.Techniques["Normal"])
                    shader = null;
#endif
                if (Global.stereoscopic && !Global.anaglyph && stereo_target != null)
                    draw_to_screen(
                        sprite_batch, render_target, stereo_target, shader, camera.matrix, width_ratio, ratio);
                else
                    draw_to_screen(
                        sprite_batch, render_target, shader, camera.matrix, width_ratio, ratio);
            }
        }

        private void copy_mouse_render(
            SpriteBatch sprite_batch,
            RenderTarget2D render_target,
            RenderTarget2D temp_target,
            Stereoscopic_Mode stereo)
        {
            GraphicsDevice.SetRenderTarget(temp_target);
            GraphicsDevice.Clear(Color.Transparent);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null, null, null);
            switch (stereo)
            {
                case Stereoscopic_Mode.Center:
                default:
                    sprite_batch.Draw(render_target, Vector2.Zero, Color.White);
                    break;
                case Stereoscopic_Mode.Left:
                    sprite_batch.Draw(render_target, Vector2.Zero,
                        new Rectangle(0, 0,
                            render_target.Width / 2, render_target.Height),
                        Color.White);
                    break;
                case Stereoscopic_Mode.Right:
                    sprite_batch.Draw(render_target, Vector2.Zero,
                        new Rectangle(render_target.Width / 2, 0,
                            render_target.Width / 2, render_target.Height),
                        Color.White);
                    break;
            }
#if !__MOBILE__
            if (FEXNA.Input.IsControllingOnscreenMouse)
            {
                spriteBatch.Draw(MouseCursorTexture,
                    Global.Input.mousePosition,
                    null, Color.White, 0f, new Vector2(20, 20), 1f, SpriteEffects.None, 0f);
            }
            else if (FEXNA.Input.ControlScheme == ControlSchemes.Touch)
            {
                Color tint = new Color(
                    TouchCursorOpacity, TouchCursorOpacity, TouchCursorOpacity, TouchCursorOpacity);
                spriteBatch.Draw(MouseCursorTexture,
                    TouchCursorLoc,
                    null, tint, 0f, new Vector2(20, 20), 1f, SpriteEffects.None, 0f);
            }
#endif
            sprite_batch.End();
        }

        private void draw_debug_view(
            SpriteBatch sprite_batch, RenderTarget2D render_target, Effect shader)
        {
            int width = ShaderRenderTargets[0].Width;
            int height = ShaderRenderTargets[0].Height;

            // Draw 2x scale render in top right
            shader = Global.effect_shader(width * (zoom - 1), height * (zoom - 1));
            draw_to_debug_screen(sprite_batch, render_target, null, zoom - 1, new Vector2(width, 0));
            // Draw 1x scale render in top left
            shader = Global.effect_shader(width * 1, height * 1);
            draw_to_debug_screen(sprite_batch, render_target, null, 1, new Vector2(0, height * (zoom - 2) / 2));
            // Draw 1x scale color deficient renders across the bottom
            shader.CurrentTechnique = shader.Techniques["Protanopia"];
            draw_to_debug_screen(sprite_batch, render_target, shader, 1,
                new Vector2(width * 0, height * (zoom - 1)));
            shader.CurrentTechnique = shader.Techniques["Deuteranopia"];
            draw_to_debug_screen(sprite_batch, render_target, shader, 1,
                new Vector2(width * 1, height * (zoom - 1)));
            shader.CurrentTechnique = shader.Techniques["Tritanopia"];
            draw_to_debug_screen(sprite_batch, render_target, shader, 1,
                new Vector2(width * 2, height * (zoom - 1)));
            // Draw labels for the different renders
            var font = Global.Content.Load<SpriteFont>(@"DiagFont");
            Vector2 text_size;
            string debug_text;
            Vector2 loc;

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            debug_text = string.Format("{0}x scale", zoom - 1);
            text_size = font.MeasureString(debug_text);
            loc = new Vector2(width - 4, 4) -
                new Vector2(text_size.X, 0);
            loc = new Vector2((int)loc.X, (int)loc.Y);
            sprite_batch.DrawString(
                font,
                debug_text,
                loc,
                Color.White);

            debug_text = "1x scale";
            text_size = font.MeasureString(debug_text);
            loc = new Vector2(width * 0.5f, height * ((zoom - 2) * 0.5f) - 4) -
                text_size / new Vector2(2, 1);
            loc = new Vector2((int)loc.X, (int)loc.Y);
            sprite_batch.DrawString(
                font,
                debug_text,
                loc,
                Color.White);

            debug_text = "Protanopia(-R) | Deuteranopia(-G) | Tritanopia(-B)";
            text_size = font.MeasureString(debug_text);
            loc = new Vector2(width * 0.5f, height * (zoom - 1) - 4) -
                text_size / new Vector2(2, 1);
            loc = new Vector2((int)loc.X, (int)loc.Y);
            sprite_batch.DrawString(
                font,
                debug_text,
                loc,
                Color.White);
            sprite_batch.End();
        }

        private void draw_to_debug_screen(SpriteBatch sprite_batch, RenderTarget2D render_target, Effect shader, float scale, Vector2 loc)
        {
            int width = ShaderRenderTargets[0].Width;
            int height = ShaderRenderTargets[0].Height;

            Vector2 width_ratio = new Vector2(scale);
            float ratio = scale;
            camera.pos = loc + new Vector2(width * scale / 2,
                height * scale / 2);
            camera.zoom = new Vector2(ratio);
            if (shader != null)
            {
                Matrix shader_matrix = screen_space_matrix(
                    new Vector2(width * scale, height * scale),
                    new Vector2(width, height),
                        width_ratio, (width * scale) / graphics.PreferredBackBufferWidth,
                    loc - new Vector2(
                        (graphics.PreferredBackBufferWidth - width) / 2,
                        (graphics.PreferredBackBufferHeight - height) / 2));
                shader.Parameters["MatrixTransform"].SetValue(shader_matrix);
            }
            draw_to_screen(sprite_batch, render_target, shader, camera.matrix, width_ratio, ratio);
        }

        private void draw_to_screen(SpriteBatch sprite_batch, RenderTarget2D render_target,
            Effect shader, Matrix m, Vector2 width_ratio, float ratio)
        {
            being_draw_to_screen_spritebatch(
                sprite_batch, new Vector2(render_target.Width, render_target.Height),
                shader, m, width_ratio, ratio);

            if (Global.stereoscopic && !Global.anaglyph_mode)
            {
                sprite_batch.Draw(
                    render_target,
                    new Vector2(0, 0),
                    new Rectangle(0, 0,
                        (int)(render_target.Width * (camera.zoom.X * 2 / ratio)),
                        render_target.Height),
                    Color.White);
            }
            else
            {
                sprite_batch.Draw(render_target, Vector2.Zero, Color.White);
            }

            sprite_batch.End();
        }
        private void draw_to_screen(
            SpriteBatch sprite_batch, RenderTarget2D leftTarget, RenderTarget2D rightTarget,
            Effect shader, Matrix m, Vector2 width_ratio, float ratio)
        {
            being_draw_to_screen_spritebatch(
                sprite_batch, new Vector2(leftTarget.Width * 1.5f, leftTarget.Height),
                shader, m, width_ratio, ratio);

            sprite_batch.Draw(
                leftTarget,
                new Vector2(0, 0),
                new Rectangle(0, 0,
                    (int)(leftTarget.Width * (camera.zoom.X * 2 / ratio)),
                    leftTarget.Height),
                Color.White);

            sprite_batch.Draw(rightTarget,
                new Vector2(rightTarget.Width * (width_ratio.X / camera.zoom.X) / 2, 0),
                new Rectangle(
                    0, 0,
                    (int)(rightTarget.Width * 2 * (camera.zoom.X / ratio)),
                    rightTarget.Height),
                Color.White);

            sprite_batch.End();
        }

        private void being_draw_to_screen_spritebatch(
            SpriteBatch sprite_batch, Vector2 size,
            Effect shader, Matrix m, Vector2 width_ratio, float ratio)
        {
#if !__MOBILE__
            if (Global.fullscreen)
#endif
            {
                begin_fullscreen_spritebatch(sprite_batch,
                    size, shader, width_ratio, ratio);
            }
#if !__MOBILE__
            else
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.PointClamp, null, null, shader, m);
#endif
        }

        private void begin_fullscreen_spritebatch(
            SpriteBatch sprite_batch, Vector2 render_target_size, Effect shader,
            Vector2 width_ratio, float ratio)
        {
            if (shader != null)
            {
                shader.CurrentTechnique = shader.Techniques["Coverage_Shader"];
                shader.Parameters["game_size"].SetValue(render_target_size);
                shader.Parameters["display_scale"].SetValue(new Vector2(ratio));

                // 'Disables' the shader and uses normal lerp, for testing that the positioning is right //Debug
                if (false)
                {
                    shader.Parameters["game_size"].SetValue(
                        new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
                    shader.Parameters["display_scale"].SetValue(new Vector2(1));
                }

                Matrix fullscreen_matrix = screen_space_matrix(
                    new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                    render_target_size, width_ratio, ratio);
                shader.Parameters["MatrixTransform"].SetValue(fullscreen_matrix);
            }
            // Matrix parameter doesn't actually work when using a custom vertex shader, herp
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null, null, shader);

            //sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, //Debug
            //    SamplerState.LinearClamp, null, null, null, m);
        }

        private Matrix screen_space_matrix(Vector2 target_size, Vector2 source_render_size, Vector2 width_ratio, float ratio)
        {
            return screen_space_matrix(target_size, source_render_size, width_ratio, ratio, Vector2.Zero);
        }
        private Matrix screen_space_matrix(Vector2 target_size, Vector2 source_render_size, Vector2 width_ratio, float ratio, Vector2 offset)
        {
            Matrix matrix = Matrix.Identity *
                // Move top left corner to the origin
                Matrix.CreateTranslation(new Vector3(1f, -1f, 0)) *
                // Move downright another pixel, for some reason
                Matrix.CreateTranslation(new Vector3(1f / (Window_Width * ratio), -1f / (Window_Height * ratio), 0)) *
                // Move the the center of the render to the center of the screen
                Matrix.CreateTranslation(new Vector3(
                    -(1 / (target_size.X / source_render_size.X)),
                    (1 / (target_size.Y / source_render_size.Y)), 0));
            if (Global.stereoscopic && !Global.anaglyph_mode)
            {
                //fullscreen_matrix *= Matrix.CreateTranslation(new Vector3(width_ratio.X / (ratio / 2), 0, 0));
                matrix *= Matrix.CreateTranslation(new Vector3((0.5f / width_ratio.X) - (1 / ratio), 0, 0)) *
                    Matrix.CreateScale(0.5f, 1, 1);
            }
            matrix *=
                // Scale up to the size of the screen
                Matrix.CreateScale(ratio, ratio, 1) *
                // Adjust for weird half pixel offset
                Matrix.CreateTranslation(new Vector3( // Can't actually do the half pixel before scaling because it messes up! //Debug
                    -(1f / target_size.X),
                    (1f / target_size.Y), 0)) *
                // Add offset
                Matrix.CreateTranslation(new Vector3(
                    offset.X / (graphics.PreferredBackBufferWidth / 2),
                    -offset.Y / (graphics.PreferredBackBufferHeight / 2), 0));
            return matrix;
        }

        private void update_screenshot(RenderTarget2D render_target)
        {
#if WINDOWS || MONOMAC
            bool f12_pressed = false;
            KeyboardState key_state = Keyboard.GetState();
            f12_pressed = key_state.IsKeyDown(Keys.F12);

            if (f12_pressed && !F12_Pressed)
            {
                string path = System.IO.Path.GetDirectoryName(GAME_ASSEMBLY.Location);
                //string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
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

                    using (FileStream fs = new FileStream(Path.Combine(
                            path, filename + i.ToString("D4") + ".png"),
                        FileMode.OpenOrCreate))
                    {
                        render_target.SaveAsPng(fs, render_target.Width, render_target.Height); // save render target to disk
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

        #region Save/Load Data
        protected string save_location()
        {
#if WINDOWS
#if !MONOGAME
            return SAVE_LOCATION;
#else
            return string.Format("{0}\\Save\\AllPlayers", GAME_ASSEMBLY.GetName().Name);
#endif
#else
            return GAME_ASSEMBLY.GetName().Name;
            //return GAME_ASSEMBLY.GetName().Name + "\\" + SAVE_LOCATION;
#endif
        }

        protected string convert_save_name()
        {
            // If the scene has a specific filename it wants
            if (Global.scene != null && !string.IsNullOrEmpty(Global.scene.suspend_filename))
            {
                if (Global.scene.is_map_save_filename)
                    return FILE_ID + Global.scene.suspend_filename + Config.SAVE_FILE_EXTENSION;
                else
                    return Global.scene.suspend_filename;
            }
            // Otherwise load the suspend for the current file
            else
                return suspend_filename(FILE_ID);
        }

        private string suspend_filename(int id)
        {
            return id + SUSPEND_FILENAME + Config.SAVE_FILE_EXTENSION;
        }
        private string map_save_filename(int id)
        {
            return id + Config.MAP_SAVE_FILENAME + Config.SAVE_FILE_EXTENSION;
        }

        private bool test_load(bool suspend)
        {
            return test_load(convert_save_name(), suspend);
        }
        protected bool test_load(string filename, bool suspend)
        {
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                /* Call FileExists */
                // Check to see whether the save exists.
                if (container == null || !container.FileExists(filename))
                {
                    Global.suspend_load_successful = false;
                    return false;
                }
                // Is a suspend/checkpoint/savestate, make sure the file opens properly, check that the version isn't too old, and test the file id
                else if (suspend)
                {
                    using (Stream stream = container.OpenFile(
                        filename, FileMode.Open, FileAccess.Read))
                    {
                        try
                        {
                            using (BinaryReader reader = new BinaryReader(stream))
                            {
                                Global.LOADED_VERSION = load_version(reader);
                                if (!Global.LOADED_VERSION.older_than(OLDEST_ALLOWED_SUSPEND_VERSION))
                                {
                                    DateTime modified_time = DateTime.FromBinary(reader.ReadInt64());
                                    FEXNA.IO.Suspend_Info info = FEXNA.IO.Suspend_Info.read(reader);
                                    // If the file id isn't the currently active one, return false
                                    Savestate_File_Id = info.save_id;
                                    if (!Savestate_Testing)
                                    {
                                        if (Savestate_File_Id != FILE_ID)
                                        {
#if DEBUG
                                            Print.message(string.Format(
                                                "Trying to load a suspend for file {0},\n" +
                                                "but the suspend was saved to slot {1}.\n" +
                                                "This would fail in release.",
                                                FILE_ID, Savestate_File_Id));
                                            Savestate_File_Id = FILE_ID;
                                        }
                                        else
#else
                                            Global.suspend_load_successful = false;
                                            return false;
                                        }
#endif
                                            Savestate_File_Id = -1;
                                    }
                                    Savestate_Testing = false;
                                }
                            }
                        }
                        catch (EndOfStreamException e)
                        {
                            Global.suspend_load_successful = false;
                            return false;
                        }
                    }
                }
                // Saves Files/etc, check that the file opens and isn't too out of date
                else
                {
                    using (Stream stream = container.OpenFile(
                        filename, FileMode.Open, FileAccess.Read))
                    {
                        try
                        {
                            using (BinaryReader reader = new BinaryReader(stream))
                                Global.LOADED_VERSION = load_version(reader);
                            if (!valid_save_version(Global.LOADED_VERSION))
                                return false;
                        }
                        catch (EndOfStreamException e)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        #region Progress Data
        protected void save_progress()
        {
            string filename = "progress" + Config.SAVE_FILE_EXTENSION;
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                // Close the wait handle
                result.AsyncWaitHandle.Close();

                if (container != null)
                {
                    // Delete file if it already exists
                    if (container.FileExists(filename))
                        container.DeleteFile(filename);
                    using (Stream stream = container.CreateFile(filename))
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            Version version = Global.RUNNING_VERSION;
                            writer.Write(version.Major);
                            writer.Write(version.Minor);
                            writer.Write(version.Build);
                            writer.Write(version.Revision);

                            Global.progress.write(writer);
                        }
                    }
                }
            }
        }
        protected void load_progress()
        {
            string filename = "progress" + Config.SAVE_FILE_EXTENSION;
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                // Close the wait handle
                result.AsyncWaitHandle.Close();
                // If the file doesn't exist, return
                if (container == null || !container.FileExists(filename))
                {
                    return;
                }
                using (Stream stream = container.OpenFile(
                    filename, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Version progress_version = load_version(reader);

                            FEXNA.IO.Save_Progress progress = FEXNA.IO.Save_Progress.read(reader, progress_version);
                            Global.progress.combine_progress(progress);
                        }
                    }
                    catch (EndOfStreamException e)
                    {
                    }
                }
            }
        }
        #endregion

        #region Save File
        protected void save_file(int id)
        {
            string filename = id.ToString() + Config.SAVE_FILE_EXTENSION;
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();

                if (container != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Write the save to a memory stream to make sure the save is successful, before actually writing it to file
                        // Make all the saving more like this //Debug
                        using (BinaryWriter writer = new BinaryWriter(ms))
                        {
                            Version version = Global.RUNNING_VERSION;
                            writer.Write(version.Major);
                            writer.Write(version.Minor);
                            writer.Write(version.Build);
                            writer.Write(version.Revision);
                            /* Call Serialize */
                            Global.game_options.write(writer);
                            Global.save_file.write(writer);
                        }

                        /* Call FileExists */
                        // Check to see whether the save exists.
                        if (container.FileExists(filename))
                            // Delete it so that we can create one fresh.
                            container.DeleteFile(filename);
                        /* Create Stream object */
                        // Create the file.
                        using (Stream stream = container.CreateFile(filename))
                        {
                            /* Create XmlSerializer */
                            // Convert the object to XML data and put it in the stream.
                            //XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData));
                            using (BinaryWriter writer = new BinaryWriter(stream))
                            {
                                writer.Write(ms.GetBuffer());
                            }
                            /* Close the Stream */
                            // Close the file.
                        }
                        /* Dispose the StorageContainer */
                        // Dispose the container, to commit changes.
                    }
                }
            }
        }

        protected void reset_file()
        {
            if (!Global.ignore_options_load)
                Global.game_options = new Game_Options();
            Global.save_file = new FEXNA.IO.Save_File();
        }

        protected bool load_file()
        {
            string filename = FILE_ID.ToString() + Config.SAVE_FILE_EXTENSION;
            reset_file();
            if (!test_load(filename, false))
            {
                return false;
            }
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                /* Call FileExists */
                // Check to see whether the save exists.
                if (container == null || !container.FileExists(filename))
                {
                    return false;
                }
                /* Create Stream object */
                // Open the file.
                // Add a IOException handler for if the file is being used by another process //Yeti
                using (Stream stream = container.OpenFile(
                    filename, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        /* Create XmlSerializer */
                        // Convert the object to XML data and put it in the stream.
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Version v;
                            Save_File_Data data;
                            if (load_file(stream, out v, out data))
                            {
                                Global.LOADED_VERSION = v;

                                if (false)//Global.LOADED_VERSION.Major == 0 && Global.LOADED_VERSION.Minor == 3 && Global.LOADED_VERSION.Build == 1)
                                {
                                    Global.game_options = data.Options;
                                    Global.save_file = data.File;
                                }
                                else
                                {
                                    Global.game_options = data.Options;
                                    Global.save_file = data.File;
                                }
                            }
                            else
                            {
                                reset_file();
                                return false;
                            }
                        }
                    }
                    catch (EndOfStreamException e)
                    {
                        reset_file();
                        return false;
                    }
                }
                /* Dispose the StorageContainer */
                // Dispose the container, to commit changes.
            }
            STARTING = false;
            return true;
        }

        private bool load_file(Stream stream, out Version v, out Save_File_Data data)
        {
            /* Create XmlSerializer */
            // Convert the object to XML data and put it in the stream.
            using (BinaryReader reader = new BinaryReader(stream))
            {
                v = load_version(reader);
                if (!valid_save_version(v))
                    throw new EndOfStreamException();

                /* Call Deserialize */
                if (false)//Global.LOADED_VERSION.Major == 0 && Global.LOADED_VERSION.Minor == 3 && Global.LOADED_VERSION.Build == 1)
                {
                    data = load_file_v_0_4_4_0(reader);
                }
                else
                {
                    data = load_file_v_0_4_4_0(reader);
                }
                return true;
            }

            v = new Version();
            data = new Save_File_Data();
            return false;
        }

        protected bool move_file(int id, int move_to_id, bool copying = false)
        {
            string filename = id.ToString() + Config.SAVE_FILE_EXTENSION;
            if (!test_load(filename, false))
            {
                return false;
            }
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                /* Call FileExists */
                // Check to see whether the save exists.
                if (container == null || !container.FileExists(filename))
                {
                    return false;
                }
                string target_filename = move_to_id.ToString() + Config.SAVE_FILE_EXTENSION;
                // Delete old target files
                delete_save_file(move_to_id, container);

                // Suspend should be deleted, to prevent abuse // actually no //Debug
                //if (container.FileExists(suspend_filename(id)))
                //    container.DeleteFile(suspend_filename(id));

                // Copy old file to new location
                using (Stream move_from_stream = container.OpenFile(
                    filename, FileMode.Open, FileAccess.Read))
                using (Stream stream = container.CreateFile(target_filename))
                {
                    byte[] buffer = new byte[move_from_stream.Length];
                    move_from_stream.Read(buffer, 0, buffer.Length);
                    stream.Write(buffer, 0, buffer.Length);
                }

                // If not copying (ie just moving)
                if (!copying)
                {
                    // Copy the suspend and map save
                    copy_suspend(id, move_to_id, container);
                    copy_map_save(id, move_to_id, container);

                    // And then delete the old files
                    delete_save_file(id, container);
                }
            }
            return true;
        }


        private void copy_suspend(int id, int move_to_id, StorageContainer container)
        {
            string source_suspend_filename = suspend_filename(id);
            string target_suspend_filename = suspend_filename(move_to_id);

            copy_suspend(id, move_to_id, container, source_suspend_filename, target_suspend_filename);
        }
        private void copy_suspend(int id, int move_to_id, StorageContainer container,
            string source_suspend_filename, string target_suspend_filename)
        {
            if (container.FileExists(source_suspend_filename))
                using (Stream move_from_stream = container.OpenFile(
                    source_suspend_filename, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(move_from_stream))
                    {
                        try
                        {
                            Global.LOADED_VERSION = load_version(reader);
                            // If the map save is valid
                            if (!Global.LOADED_VERSION.older_than(OLDEST_ALLOWED_SUSPEND_VERSION))
                            {
                                DateTime modified_time = DateTime.FromBinary(reader.ReadInt64());
                                FEXNA.IO.Suspend_Info info = FEXNA.IO.Suspend_Info.read(reader);

                                if (info.save_id == id)
                                    using (Stream stream = container.CreateFile(target_suspend_filename))
                                    {
                                        info.save_id = move_to_id;
                                        using (BinaryWriter writer = new BinaryWriter(stream))
                                        {
                                            writer.Write(Global.LOADED_VERSION.Major);
                                            writer.Write(Global.LOADED_VERSION.Minor);
                                            writer.Write(Global.LOADED_VERSION.Build);
                                            writer.Write(Global.LOADED_VERSION.Revision);
                                            writer.Write(modified_time.ToBinary());
                                            info.write(writer);
                                            // Move the actual map save data, everything after the info
                                            byte[] buffer = new byte[move_from_stream.Length - move_from_stream.Position];
                                            move_from_stream.Read(buffer, 0, buffer.Length);
                                            stream.Write(buffer, 0, buffer.Length);
                                        }
                                    }
                            }
                            // If the map save is too old to read properly, just move everything in it
                            else
                            {
                                using (Stream stream = container.CreateFile(target_suspend_filename))
                                {
                                    move_from_stream.Position = 0;
                                    byte[] buffer = new byte[move_from_stream.Length];
                                    move_from_stream.Read(buffer, 0, buffer.Length);
                                    stream.Write(buffer, 0, buffer.Length);
                                }
                            }
                        }
                        catch (EndOfStreamException e)
                        {
#if DEBUG
                            Print.message("Failed to copy suspend");
#endif
                        }
                    }
                }
        }
        private void copy_map_save(int id, int move_to_id, StorageContainer container)
        {
            string source_suspend_filename = map_save_filename(id);
            string target_suspend_filename = map_save_filename(move_to_id);

            copy_suspend(id, move_to_id, container, source_suspend_filename, target_suspend_filename);
        }

        protected bool delete_save_file(int id)
        {
            string filename = id.ToString() + Config.SAVE_FILE_EXTENSION;
            if (!test_load(filename, false))
            {
                return false;
            }
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                delete_save_file(id, container);
            }
            return true;
        }
        protected bool delete_save_file(int id, StorageContainer container)
        {
            string filename = id.ToString() + Config.SAVE_FILE_EXTENSION;
            // Close the wait handle.
            result.AsyncWaitHandle.Close();
            /* Call FileExists */
            // Check to see whether the save exists.
            if (container == null || !container.FileExists(filename))
            {
                return false;
            }
            container.DeleteFile(filename);
            if (container.FileExists(suspend_filename(id)))
                container.DeleteFile(suspend_filename(id));
            if (container.FileExists(map_save_filename(id)))
                container.DeleteFile(map_save_filename(id));

            return true;
        }

        protected bool delete_file(string filename)
        {
            if (!test_load(filename, false))
            {
                return false;
            }
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                /* Call FileExists */
                // Check to see whether the save exists.
                if (container == null || !container.FileExists(filename))
                {
                    return false;
                }
                container.DeleteFile(filename);
            }
            return true;
        }

        private Save_File_Data load_file_v_0_4_0_2(BinaryReader reader)
        {
            return new Save_File_Data
            {
                Options = Game_Options.read(reader),
                File = FEXNA.IO.Save_File.read(reader)
            };
        }
        private Save_File_Data load_file_v_0_4_4_0(BinaryReader reader)
        {
            return new Save_File_Data
            {
                Options = Game_Options.read(reader),
                File = FEXNA.IO.Save_File.read(reader)
            };
        }
        #endregion

        #region Map Save
        protected bool save()
        {
            return save(convert_save_name());
        }
        protected bool save(string filename)
        {
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();

                if (container != null)
                {
                    /* Call FileExists */
                    // Check to see whether the save exists.
                    if (container.FileExists(filename))
                        // Delete it so that we can create one fresh.
                        container.DeleteFile(filename);
                    /* Create Stream object */
                    // Create the file.
                    using (Stream stream = container.CreateFile(filename))
                    {
                        /* Create XmlSerializer */
                        // Convert the object to XML data and put it in the stream.
                        //XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData));
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            // Save FinalRender to a byte[]
                            byte[] screenshot;
                            using (MemoryStream ms = new MemoryStream())
                            {
                                FinalRender.SaveAsPng(ms, FinalRender.Width, FinalRender.Height);
                                screenshot = ms.ToArray();
                            }
                            Global.save_suspend(writer, FILE_ID, screenshot);
                        }
                        /* Close the Stream */
                        // Close the file.
                    }
                    /* Dispose the StorageContainer */
                    // Dispose the container, to commit changes.
                    return true;
                }
            }
            return false;
        }

        protected void load()
        {
            load(convert_save_name());
        }
        protected void load(string filename)
        {
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {

                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                /* Call FileExists */
                // Check to see whether the save exists.
                if (container == null || !container.FileExists(filename))
                {
                    Global.suspend_load_successful = false;
                    Quick_Load = false;
                    return;
                }
                /* Create Stream object */
                // Open the file.
                using (Stream stream = container.OpenFile(
                    filename, FileMode.Open, FileAccess.Read))
                {
                    Scene_Base old_scene = Global.scene;
                    try
                    {
                        /* Create XmlSerializer */
                        // Convert the object to XML data and put it in the stream.
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Global.LOADED_VERSION = load_version(reader);
                            // Wait for move range update thread to finish
                            end_move_range_thread();
                            MoveRangeUpdateThread = null;
                            /* Call Deserialize */
                            int file_id;
                            bool load_successful = Global.load_suspend(
                                reader, out file_id,
                                Global.LOADED_VERSION,
                                OLDEST_ALLOWED_SUSPEND_VERSION);

                            if (load_successful)
                            {
#if !DEBUG
                                if (FILE_ID != file_id) //Yeti
                                {
                                    throw new Exception();
                                }
#endif
                                FILE_ID = file_id;
                                DEBUG_FILE_ID_TEST = FILE_ID; //Yeti
                                Global.game_options.post_read();
                                //Global.Audio.clear_map_theme(); //@Debug
                                if (!Global.Audio.stop_me(true))
                                    Global.Audio.BgmFadeOut(20);
                                Global.Audio.stop_bgs();
                            }
                            else
                                throw new EndOfStreamException("Load unsuccessful");
                        }

                        if (Quick_Load)
                        {
                            // Resume Arena
                            if (Global.in_arena())
                            {
                                change_scene("Scene_Arena");
                                move_range_update_thread();
                            }
                            else
                            {
                                Global.suspend_finish_load(false);
                                move_range_update_thread();
                            }
                        }
                        else
                            Global.suspend_load_successful = true;
                    }
                    catch (EndOfStreamException e)
                    {
                        Global.suspend_load_successful = false;
                        end_move_range_thread();
                        MoveRangeUpdateThread = null;
                        Global.suspend_load_fail(old_scene);

                        Print.message("Suspend file not in the right format");
                    }
                }
                /* Dispose the StorageContainer */
                // Dispose the container, to commit changes.
            }
            var key_state = new KeyboardState();
            var controller_state = new GamePadState();
            Global.update_input(this, new GameTime(), IsActive, key_state, controller_state);
            Quick_Load = false;
        }
        #endregion

        #region Save Info
        protected void load_suspend_info(bool update_file_id = true)
        {
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                /* Call FileExists */
                // Check to see whether the save exists.
                if (container == null)
                {
                    Global.suspend_file_info = null;
                    return;
                }

                // Get valid suspend file names
                List<string> suspend_files = new List<string>(
                    container.GetFileNames("*" + SUSPEND_FILENAME + Config.SAVE_FILE_EXTENSION)
                        .Select(x => Path.GetFileName(x)));
                int i = 0;
                int test;
                while (i < suspend_files.Count)
                {
                    // Gets the file id from the file name
                    string suspend_file = suspend_files[i];
                    suspend_file = suspend_file.Substring(0, suspend_file.Length - (SUSPEND_FILENAME.Length + Config.SAVE_FILE_EXTENSION.Length));
                    // If the id is a number, keep it in the list
                    if (suspend_file.Length > 0 && int.TryParse(suspend_file, System.Globalization.NumberStyles.None, System.Globalization.NumberFormatInfo.InvariantInfo, out test))
                        i++;
                    else
                        suspend_files.RemoveAt(i);
                }
                // If no files are valid, return
                if (suspend_files.Count == 0)
                {
                    Global.suspend_file_info = null;
                    return;
                }
                // Get the newest file
                DateTime modified_time = new DateTime();
                int index = -1;
                for (i = 0; i < suspend_files.Count; i++)
                {
                    try
                    {
                        using (Stream stream = container.OpenFile(
                            suspend_files[i], FileMode.Open, FileAccess.Read))
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Global.LOADED_VERSION = load_version(reader);
                            if (!Global.LOADED_VERSION.older_than(OLDEST_ALLOWED_SUSPEND_VERSION))
                            {
                                DateTime time = DateTime.FromBinary(reader.ReadInt64());
                                Global.suspend_file_info = FEXNA.IO.Suspend_Info.read(reader);
                                int filename_id = Convert.ToInt32(suspend_files[i].Substring(0,
                                    suspend_files[i].Length - (SUSPEND_FILENAME.Length + Config.SAVE_FILE_EXTENSION.Length)));
                                // If the file id in the data does not match the id in the filename, or there isn't a save for this id
                                if (Global.suspend_file_info.save_id != filename_id ||
                                        !container.FileExists(Global.suspend_file_info.save_id.ToString() + Config.SAVE_FILE_EXTENSION))
                                    continue;
                                if (index == -1 || time > modified_time)
                                {
                                    index = i;
                                    modified_time = time;
                                }
                            }
                        }
                    }
                    catch (EndOfStreamException e) { }
                }
                // No suspends found
                if (index == -1)
                {
                    Global.suspend_file_info = null;
                    return;
                }

                string filename = suspend_files[index];
                /* Create Stream object */
                // Open the file.
                using (Stream stream = container.OpenFile(
                    filename, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        /* Create XmlSerializer */
                        // Convert the object to XML data and put it in the stream.
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Global.LOADED_VERSION = load_version(reader);
                            /* Call Deserialize */
                            if (!Global.LOADED_VERSION.older_than(OLDEST_ALLOWED_SUSPEND_VERSION))
                            {
                                DateTime time = DateTime.FromBinary(reader.ReadInt64());
                                Global.suspend_file_info = FEXNA.IO.Suspend_Info.read(reader);
                                if (update_file_id)
                                    FILE_ID = Global.suspend_file_info.save_id;
                            }
                            else
                            {
                                Global.suspend_file_info = new FEXNA.IO.Suspend_Info();
                            }
                        }
                    }
                    catch (EndOfStreamException e)
                    {
                        Global.suspend_file_info = null;
                    }
                }
                /* Dispose the StorageContainer */
                // Dispose the container, to commit changes.
            }
        }
        protected void load_save_info()
        {
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                // Get valid save file names
                List<string> save_files = new List<string>(
                    container.GetFileNames("*" + Config.SAVE_FILE_EXTENSION)
                        .Select(x => Path.GetFileName(x)));
                int i = 0;
                int test;
                while (i < save_files.Count)
                {
                    string save_file = save_files[i];
                    save_file = save_file.Substring(0, save_file.Length - (Config.SAVE_FILE_EXTENSION.Length));
                    if (save_file.Length > 0 && int.TryParse(save_file, System.Globalization.NumberStyles.None, System.Globalization.NumberFormatInfo.InvariantInfo, out test) &&
                            (test < Config.SAVES_PER_PAGE * Config.SAVE_PAGES))
                        i++;
                    else
                        save_files.RemoveAt(i);
                }
                // If no files are valid, return
                if (save_files.Count == 0)
                {
                    Global.save_files_info = null;
                    Global.suspend_files_info = null;
                    Global.checkpoint_files_info = null;
                    Global.latest_save_id = -1;
                    return;
                }

                // If any old data needs referenced
                var old_save_files_info = Global.save_files_info;
                // Get the newest file
                Global.save_files_info = new Dictionary<int, FEXNA.IO.Save_Info>();
                Global.suspend_files_info = new Dictionary<int, FEXNA.IO.Suspend_Info>();
                Global.checkpoint_files_info = new Dictionary<int, FEXNA.IO.Suspend_Info>();
                DateTime modified_time = new DateTime();
                int index = -1;
                i = 0;
                while (i < save_files.Count)
                {
                    using (Stream stream = container.OpenFile(
                        save_files[i], FileMode.Open, FileAccess.Read))
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        int file_id = Convert.ToInt32(save_files[i].Substring(0, save_files[i].Length - (Config.SAVE_FILE_EXTENSION.Length)));
                        Global.LOADED_VERSION = load_version(reader);
                        try
                        {
                            if (!valid_save_version(Global.LOADED_VERSION))
                                throw new EndOfStreamException(
                                    "Save file is too outdated or from a newer version");

                            Save_File_Data data = load_file_v_0_4_4_0(reader);
                            // Updates the progress data with this save file
                            Global.progress.update_progress(data.File);
                            bool suspend_exists = container.FileExists(suspend_filename(file_id));
                            bool map_save_exists = container.FileExists(map_save_filename(file_id));
                            FEXNA.IO.Save_Info info;
                            FEXNA.IO.Suspend_Info suspend_info = null;
                            // Check if the map save actually exists
                            if (map_save_exists)
                            {
                                Version v = Global.LOADED_VERSION;
                                using (Stream suspend_stream = container.OpenFile(
                                    map_save_filename(file_id), FileMode.Open, FileAccess.Read))
                                {
                                    try
                                    {
                                        using (BinaryReader suspend_reader = new BinaryReader(suspend_stream))
                                        {
                                            Global.LOADED_VERSION = load_version(suspend_reader);
                                            /* Call Deserialize */
                                            if (!Global.LOADED_VERSION.older_than(OLDEST_ALLOWED_SUSPEND_VERSION))
                                            {
                                                DateTime time = DateTime.FromBinary(suspend_reader.ReadInt64());
                                                suspend_info = FEXNA.IO.Suspend_Info.read(suspend_reader);
                                                Global.checkpoint_files_info[file_id] = suspend_info;
                                            }
                                            else
                                                map_save_exists = false;
                                        }
                                    }
                                    catch (EndOfStreamException e)
                                    {
                                        map_save_exists = false;
                                    }
                                }
                                Global.LOADED_VERSION = v;
                            }
                            // If a suspend exists, always use its data for the save file instead of the most recent save
                            if (suspend_exists)
                            {
                                Version v = Global.LOADED_VERSION;
                                using (Stream suspend_stream = container.OpenFile(
                                    suspend_filename(file_id), FileMode.Open, FileAccess.Read))
                                {
                                    try
                                    {
                                        using (BinaryReader suspend_reader = new BinaryReader(suspend_stream))
                                        {
                                            Global.LOADED_VERSION = load_version(suspend_reader);
                                            /* Call Deserialize */
                                            if (!Global.LOADED_VERSION.older_than(OLDEST_ALLOWED_SUSPEND_VERSION))
                                            {
                                                DateTime time = DateTime.FromBinary(suspend_reader.ReadInt64());
                                                suspend_info = FEXNA.IO.Suspend_Info.read(suspend_reader);
                                                Global.suspend_files_info[file_id] = suspend_info;
                                            }
                                            else
                                                suspend_exists = false;
                                        }
                                    }
                                    catch (EndOfStreamException e)
                                    {
                                        suspend_exists = false;
                                    }
                                }
                                Global.LOADED_VERSION = v;
                            }

                            if (suspend_info != null)
                            {
                                info = FEXNA.IO.Save_Info.get_save_info(file_id, data.File, suspend_info, map_save: map_save_exists, suspend: suspend_exists);
                            }
                            else
                                info = FEXNA.IO.Save_Info.get_save_info(file_id, data.File, suspend_exists);

                            // Copy transient file info (last chapter played, last time started)
                            if (old_save_files_info != null &&
                                old_save_files_info.ContainsKey(file_id))
                            {
                                var old_info = old_save_files_info[file_id];
                                info.CopyTransientInfo(old_info);
                            }

                            // Set the file info into the dictionary
                            Global.save_files_info[file_id] = info;

                            if (index == -1 || info.time > modified_time)
                            {
                                index = file_id;
                                modified_time = info.time;
                                if (STARTING)
                                {
                                    Global.game_options = data.Options;  //Debug // This needs to only happen when just starting the game
                                    STARTING = false;
                                }
                            }
                            i++;
                        }
                        catch (EndOfStreamException e)
                        {
                            save_files.RemoveAt(i);
                            continue;
                        }
                    }
                }
                // If no files were loaded successfully, the index will still be -1
                if (index == -1)
                {
                    // Since nothing was loaded, null things back out
                    Global.save_files_info = null;
                    Global.suspend_files_info = null;
                    Global.checkpoint_files_info = null;
                    Global.latest_save_id = -1;
                    return;
                }
                Global.latest_save_id = index;
                /* Dispose the StorageContainer */
                // Dispose the container, to commit changes.
            }

            refresh_suspend_screenshots();
        }

        private bool valid_save_version(Version loadedVersion)
        {
            // If game version is older than the save, don't load the save
            if (Global.save_version_too_new(loadedVersion))
                return false;

            if (loadedVersion.older_than(OLDEST_ALLOWED_SAVE_VERSION))
                return false;

            return true;
        }

        private void refresh_suspend_screenshots()
        {
            // Dispose old screenshots
            Global.dispose_suspend_screenshots();
            // Load new ones from each suspend info
            if (Global.suspend_files_info != null)
            {
                // Associate each suspend info with its loaded screenshot
                if (Global.suspend_file_info != null)
                    Global.suspend_file_info.load_screenshot("recent");

                foreach (var suspend in Global.suspend_files_info)
                {
                    string name = suspend_filename(suspend.Key);
                    suspend.Value.load_screenshot(name);
                }
                foreach (var map_save in Global.checkpoint_files_info)
                {
                    string name = map_save_filename(map_save.Key);
                    map_save.Value.load_screenshot(name);
                }
            }
        }
        #endregion

        private Version load_version(BinaryReader reader)
        {
            return new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }
        #endregion

        #region Save/Load Config
        protected void save_config()
        {
            string filename = "config" + Config.SAVE_FILE_EXTENSION;
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                // Close the wait handle
                result.AsyncWaitHandle.Close();

                if (container != null)
                {
                    // Delete file if it already exists
                    if (container.FileExists(filename))
                        container.DeleteFile(filename);
                    using (Stream stream = container.CreateFile(filename))
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            Version version = Global.RUNNING_VERSION;
                            writer.Write(version.Major);
                            writer.Write(version.Minor);
                            writer.Write(version.Build);
                            writer.Write(version.Revision);

                            writer.Write(ZOOM);
                            writer.Write(Global.fullscreen);
                            writer.Write(Global.stereoscopic_level);
                            writer.Write(Global.anaglyph);
                            writer.Write((int)Global.metrics);
                            writer.Write(Global.updates_active);
                            writer.Write(Global.rumble);
                            FEXNA.Input.write(writer);
                        }
                    }
                }
            }
        }
        protected void load_config()
        {
            string filename = "config" + Config.SAVE_FILE_EXTENSION;
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer(save_location(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = device.EndOpenContainer(result))
            {
                // Close the wait handle
                result.AsyncWaitHandle.Close();
                // If the file doesn't exist, return
                if (container == null || !container.FileExists(filename))
                {
                    return;
                }
                using (Stream stream = container.OpenFile(
                    filename, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Global.LOADED_VERSION = load_version(reader);
                            if (Global.LOADED_VERSION.older_than(0, 4, 2, 0))
                            {
                                Global.zoom = reader.ReadInt32();
                                Global.fullscreen = false;
                                Global.stereoscopic_level = 0;
                                Global.anaglyph = true;
                                bool unused = reader.ReadBoolean();
                                FEXNA.Input.read(reader);
                            }
                            else if (Global.LOADED_VERSION.older_than(0, 4, 6, 3))
                            {
                                Global.zoom = reader.ReadInt32();
                                Global.fullscreen = reader.ReadBoolean();
                                Global.stereoscopic_level = reader.ReadInt32();
                                Global.anaglyph = reader.ReadBoolean();
                                FEXNA.Input.read(reader);
                            }
                            else if (Global.LOADED_VERSION.older_than(0, 5, 0, 6))
                            {
                                Global.zoom = reader.ReadInt32();
                                Global.fullscreen = reader.ReadBoolean();
                                Global.stereoscopic_level = reader.ReadInt32();
                                Global.anaglyph = reader.ReadBoolean();
                                Global.metrics = (Metrics_Settings)reader.ReadInt32();
                                FEXNA.Input.read(reader);
                            }
                            else
                            {
                                Global.zoom = reader.ReadInt32();
                                Global.fullscreen = reader.ReadBoolean();
                                Global.stereoscopic_level = reader.ReadInt32();
                                Global.anaglyph = reader.ReadBoolean();
                                Global.metrics = (Metrics_Settings)reader.ReadInt32();
                                Global.updates_active = reader.ReadBoolean();
                                Global.rumble = reader.ReadBoolean();
                                FEXNA.Input.read(reader);
                            }
                        }
                    }
                    catch (EndOfStreamException e)
                    {
                        reset_config();
                    }
                }
            }
        }

        private void reset_config()
        {
            Global.zoom = 1;
            Global.fullscreen = false;
            Global.stereoscopic_level = 0;
            Global.anaglyph = true;
            FEXNA.Input.default_controls();
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
    }

    struct Save_File_Data
    {
        public Game_Options Options;
        public FEXNA.IO.Save_File File;
    }
}
