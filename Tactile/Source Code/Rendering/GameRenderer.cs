using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Rendering
{
    class GameRenderer
    {
        const int ZOOM_MAX = 5;
        const int ZOOM_MIN = 1;

        const int RENDER_TARGETS = 4;

        private static int WindowWidth { get { return Config.WINDOW_WIDTH; } }
        private static int WindowHeight { get { return Config.WINDOW_HEIGHT; } }

#if DEBUG
        const bool DEBUG_VIEW = true;
#endif

#if WINDOWS || MONOMAC
        //@Debug: why is this static
        // Or if a public constant is needed, why isn't this constant
        // or why can't we get with a property
        public static int ZOOM = 2;
#elif XBOX
        public static int ZOOM = 2;
#else
        public static int ZOOM = 1;
#endif

        private int ScreenWidth, ScreenHeight;

        private GraphicsDevice GraphicsDevice;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Camera camera;
        private IFullscreenService Fullscreen;

        // Textures
        private RenderTarget2D[] ShaderRenderTargets = new RenderTarget2D[RENDER_TARGETS];
        private RenderTarget2D StereoscopicRenderTarget;
        private RenderTarget2D FinalRender;

        private float TouchCursorOpacity;
        private Vector2 TouchCursorLoc;
        private Texture2D MouseCursorTexture;

        public static int zoom
        {
            get { return GameRenderer.IsFullscreen ? 2 : ZOOM; }
            private set { ZOOM = value; }
        }

        private static bool IsFullscreen
        {
            get { return Global.gameSettings.Graphics.Fullscreen; }
        }

        public static int RenderTargetZoom
        {
            get { return 1; }
        }

        public GameRenderer(Game game, IFullscreenService fullscreen)
        {
            Fullscreen = fullscreen;
            graphics = new GraphicsDeviceManager(game);
            SetInitialResolution();

            camera = new Camera(WindowWidth, WindowHeight, Vector2.Zero);
        }

        private void SetInitialResolution()
        {
#if __ANDROID__
            ScreenWidth = graphics.PreferredBackBufferWidth;
            ScreenHeight = graphics.PreferredBackBufferHeight - STATUS_BAR_HEIGHT;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
#else
            ScreenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            ScreenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
#endif
            int zoomMax = Math.Min( // this is a bad solution, need a catch block that's elegant //@Yeti
                ZOOM_MAX, ScreenWidth / WindowWidth);
            zoomMax = Math.Min(
                zoomMax, (ScreenHeight - 64) / WindowHeight);

            Global.gameSettings.Graphics.SetZoomLimits(ZOOM_MIN, zoomMax);

            RefreshZoom(true);

            // This was commented out, for some reason I've forgotten
            // But commenting this out causes multiple instances to lag each other //@Debug
            graphics.SynchronizeWithVerticalRetrace = false;
        }

#if __ANDROID__
        private static int STATUS_BAR_HEIGHT;

        public static void SetStatusBarHeight(int height)
        {
            STATUS_BAR_HEIGHT = height;
        }
#endif

        public void CreateSpriteBatch(Game game)
        {
            GraphicsDevice = game.GraphicsDevice;
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }
        public void LoadContent(ContentManager Content)
        {
            MouseCursorTexture =
                Content.Load<Texture2D>(@"Graphics/Pictures/MouseCursor");

            RefreshEffectProjection();
            SetRenderTargets();
        }

        private void RefreshEffectProjection()
        {
            if (Global.effect_shader() != null)
            {
                Effect shader = Global.effect_shader();
                SetBlurEffectParameters(shader, 1f / (float)WindowWidth, 1f / (float)WindowHeight);
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

        #region Render Targets
        public void SetRenderTargets()
        {
            DisposeRenderTargets();
            try
            {
                for (int i = 0; i < RENDER_TARGETS; i++)
                {
                    RenderTarget2D target = CloneRenderTarget(graphics.GraphicsDevice, 1);
                    ShaderRenderTargets[i] = target;
                }
                if (true)//fullscreen) //@Debug
                    StereoscopicRenderTarget = CloneRenderTarget(graphics.GraphicsDevice, 1, true);
                FinalRender = CloneRenderTarget(graphics.GraphicsDevice, 1);
            }
            catch (OutOfMemoryException e)
            {
                Global.gameSettings.Graphics.ConfirmSetting(
                    Tactile.Options.GraphicsSetting.Zoom, 0, 1);
                DisposeRenderTargets();
                return;
            }
        }

        private void DisposeRenderTargets()
        {
            foreach (RenderTarget2D renderTarget in ShaderRenderTargets)
                if (renderTarget != null)
                    renderTarget.Dispose();
            if (StereoscopicRenderTarget != null)
                StereoscopicRenderTarget.Dispose();
            if (FinalRender != null)
                FinalRender.Dispose();

            for (int i = 0; i < ShaderRenderTargets.Length; i++)
                ShaderRenderTargets[i] = null;
            StereoscopicRenderTarget = null;
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
                WindowWidth * RenderTargetZoom * (stereo ? 2 : 1),
                WindowHeight * RenderTargetZoom,
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
        #endregion

        public bool ZoomNeedsRefresh
        {
            get { return Global.gameSettings.Graphics.Zoom != ZOOM; }
        }

        public bool UpdateZoom()
        {
            // If zoom value changed
            if (this.ZoomNeedsRefresh || Fullscreen.NeedsRefresh(GameRenderer.IsFullscreen))
            {
                RefreshZoom();
                // Create new properly sized render targets
                SetRenderTargets();
                return true;
            }

            return false;
        }

        private void RefreshZoom()
        {
            RefreshZoom(false);
        }
        private void RefreshZoom(bool initialSet)
        {
            ZOOM = Global.gameSettings.Graphics.Zoom;

            // refresh fullscreen
            Fullscreen.SetFullscreen(GameRenderer.IsFullscreen, graphics);

            int fullscreenWidth = Fullscreen.WindowWidth(GraphicsDevice);
            int fullscreenHeight = Fullscreen.WindowHeight(GraphicsDevice);
#if !MONOGAME
            graphics.PreferredBackBufferWidth = (int)(GameRenderer.IsFullscreen ? fullscreenWidth : WindowWidth * zoom);
            graphics.PreferredBackBufferHeight = (int)(GameRenderer.IsFullscreen ? fullscreenHeight : WindowHeight * zoom);
#elif MONOMAC || WINDOWS
            graphics.PreferredBackBufferWidth = (int)(GameRenderer.IsFullscreen ? fullscreenHeight : WindowWidth * zoom);
            graphics.PreferredBackBufferHeight = (int)(GameRenderer.IsFullscreen ? fullscreenHeight : WindowHeight * zoom);
#elif __MOBILE__
            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;
#else
            graphics.PreferredBackBufferWidth = (int)(WindowWidth * zoom);
            graphics.PreferredBackBufferHeight = (int)(WindowHeight * zoom);
#endif
            if (!initialSet)
            {
                RefreshEffectProjection();
                graphics.ApplyChanges();
            }
        }

        public void MinimizeFullscreen(Game game)
        {
            Fullscreen.MinimizeFullscreen(game);
        }

        #region Update
        public void UpdateTouch()
        {
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

        public void UpdateScreenScale()
        {
            Vector2 gameSize = new Vector2(graphics.PreferredBackBufferWidth,
                graphics.PreferredBackBufferHeight);
            Vector2 renderSize = new Vector2(
                ShaderRenderTargets[0].Width,
                ShaderRenderTargets[0].Height);
            Vector2 windowSize = new Vector2(
                graphics.GraphicsDevice.DisplayMode.Width,
                graphics.GraphicsDevice.DisplayMode.Height);
#if __MOBILE__
            Tactile.Input.update_screen_scale(true ? 
#else
            Tactile.Input.update_screen_scale(GameRenderer.IsFullscreen ?
#endif
 gameSize / renderSize : new Vector2(Global.gameSettings.Graphics.Zoom),
                windowSize);
        }
        #endregion

        #region Draw
        public void Draw()
        {
            // Reset data
            Global.palette_pool.update();
            if (ShaderRenderTargets[0] == null)
                return;
            ClearRenderTargets();

            camera.pos = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
            camera.offset = new Vector2(ShaderRenderTargets[0].Width / 2, ShaderRenderTargets[0].Height / 2);
            Vector2 ratio = new Vector2(ScreenSizeRatio);
            camera.zoom = ratio;

            // Always draw the screen normally to FinalRender, for screenshotting/suspend images
            DrawScene(spriteBatch, Stereoscopic_Mode.Center);
            // Copy render to final render
            TactileRenderTarget2DExtension.RenderTarget2DExtensions.raw_copy_render_target(
                ShaderRenderTargets[0], spriteBatch, GraphicsDevice, FinalRender);

            // Draws scene
            if (!Global.gameSettings.Graphics.Stereoscopic)
            {
                // Draw rendertarget to screen
                DrawToScreen(spriteBatch);
            }
            else
            {
                // Clear the stereo render target to black
                GraphicsDevice.SetRenderTarget(StereoscopicRenderTarget);
                GraphicsDevice.Clear(Color.Black);

                DrawScene(spriteBatch, Stereoscopic_Mode.Left);
                DrawScene(spriteBatch, Stereoscopic_Mode.Right);

                // If Anaglyph, copy back to rendertarget[0] since it's only normal screen size
                // Maybe the scene renderer should always draw to StereoscopicRenderTarget as if it's non-anaglyph mode //@Yeti
                // And then this block would take the two halves and combine them on ShaderRenderTargets[0] in anaglyph //@Yeti
                if (Global.gameSettings.Graphics.AnaglyphMode)
                {
                    GraphicsDevice.SetRenderTarget(ShaderRenderTargets[0]);
                    GraphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
                    spriteBatch.Draw(StereoscopicRenderTarget, new Rectangle(
                        0, 0, StereoscopicRenderTarget.Width / 2, StereoscopicRenderTarget.Height),
                        new Rectangle(0, 0, ShaderRenderTargets[0].Width, ShaderRenderTargets[0].Height), Color.White);
                    spriteBatch.End();
                }
                // Else that thing is not true, so halve the x scale of the camera
                else
                    camera.zoom *= new Vector2(0.5f, 1);

                // Draw rendertarget to screen
                if (Global.gameSettings.Graphics.AnaglyphMode)
                    DrawToScreen(spriteBatch, ShaderRenderTargets[0]);
                else
                    DrawToScreen(spriteBatch, StereoscopicRenderTarget);
            }

            GraphicsDevice.SetRenderTarget(null);
        }
        public void RedrawPreviousFrame()
        {
            DrawToScreen(spriteBatch, !Global.gameSettings.Graphics.Stereoscopic ?
                ShaderRenderTargets[0] : StereoscopicRenderTarget);
        }

#if DEBUG && (__MOBILE__ || GET_FPS)
        public void DrawMobileFps(double currentFrameRate)
        {
            var font = Global.Content.Load<SpriteFont>(@"DiagFont");
            string fpsText;
            Vector2 loc;
            float widthRatio = graphics.PreferredBackBufferWidth /
                (float)Config.WINDOW_WIDTH / 2;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            fpsText = string.Format("{0:0.0}fps", currentFrameRate);
            fpsText += string.Format("\nMemory: {0}KB", GC.GetTotalMemory(false) / 1024);
            loc = new Vector2(4, 4);
            spriteBatch.DrawString(
                font,
                fpsText,
                loc,
                Color.White,
                0f, Vector2.Zero, widthRatio, SpriteEffects.None, 0f);
            spriteBatch.End();
        }
#endif


        private float ScreenSizeRatio
        {
            get
            {
                if (graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight >
                        WindowWidth / (float)WindowHeight)
                    return graphics.PreferredBackBufferHeight / (float)ShaderRenderTargets[0].Height;
                else
                    return graphics.PreferredBackBufferWidth / (float)ShaderRenderTargets[0].Width;
            }
        }

        private void ClearRenderTargets()
        {
            foreach (RenderTarget2D renderTarget in ShaderRenderTargets)
            {
                GraphicsDevice.SetRenderTarget(renderTarget);
                GraphicsDevice.Clear(Color.Transparent);
            }
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Transparent);
        }

        private void DrawScene(SpriteBatch spriteBatch, bool clearRenderTarget = false)
        {
            GraphicsDevice.SetRenderTarget(ShaderRenderTargets[0]);
            if (clearRenderTarget)
                GraphicsDevice.Clear(Color.Transparent);
            Global.scene.draw(spriteBatch, GraphicsDevice, ShaderRenderTargets);
        }

        private void DrawScene(SpriteBatch spriteBatch, Stereoscopic_Mode stereo)
        {
            Stereoscopic_Graphic_Object.stereoscopic_view = stereo;
            DrawScene(this.spriteBatch, stereo == Stereoscopic_Mode.Right);

            if (stereo == Stereoscopic_Mode.Center)
                return;

            GraphicsDevice.SetRenderTarget(StereoscopicRenderTarget);
            this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            // In anaglyph, left is drawn in red channel, right is drawn cyan
            if (Global.gameSettings.Graphics.AnaglyphMode)
            {
                Color anaglyphColor = stereo == Stereoscopic_Mode.Left ? new Color(255, 0, 0, 0) : new Color(0, 255, 255, 0);
                this.spriteBatch.Draw(ShaderRenderTargets[0], new Rectangle(
                    0, 0, StereoscopicRenderTarget.Width / 2, StereoscopicRenderTarget.Height),
                    new Rectangle(0, 0, ShaderRenderTargets[0].Width, ShaderRenderTargets[0].Height), anaglyphColor);
            }
            // Otherwise, the left channel is drawn on the left half of the screen and the right on the right, and the display sorts it out
            else
            {
                int x = stereo == Stereoscopic_Mode.Left ? 0 : StereoscopicRenderTarget.Width / 2;
                this.spriteBatch.Draw(ShaderRenderTargets[0], new Rectangle(
                    x, 0, StereoscopicRenderTarget.Width / 2, StereoscopicRenderTarget.Height),
                    new Rectangle(0, 0, ShaderRenderTargets[0].Width, ShaderRenderTargets[0].Height), Color.White);
            }
            this.spriteBatch.End();
        }

        private void DrawToScreen(SpriteBatch spriteBatch)
        {
            DrawToScreen(spriteBatch, ShaderRenderTargets[0]);
        }
        private void DrawToScreen(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            RenderTarget2D stereoTarget = null;
            if (renderTarget == StereoscopicRenderTarget)
            {
                for (int i = 0; i < ShaderRenderTargets.Length; i++)
                {
                    GraphicsDevice.SetRenderTarget(ShaderRenderTargets[i]);
                    GraphicsDevice.Clear(Color.Transparent);
                }

                stereoTarget = ShaderRenderTargets[RENDER_TARGETS - 2];
            }

            // Copy the render target to another render target and draw the mouse on it
            RenderTarget2D tempTarget =
                renderTarget == ShaderRenderTargets[RENDER_TARGETS - 1] ?
                ShaderRenderTargets[RENDER_TARGETS - 2] :
                ShaderRenderTargets[RENDER_TARGETS - 1];

            if (Global.gameSettings.Graphics.Stereoscopic &&
                !Global.gameSettings.Graphics.Anaglyph && stereoTarget != null)
            {
                CopyMouseRender(
                    spriteBatch, renderTarget, tempTarget, Stereoscopic_Mode.Left);
                CopyMouseRender(
                    spriteBatch, renderTarget, stereoTarget, Stereoscopic_Mode.Right);
            }
            else
                CopyMouseRender(spriteBatch, renderTarget, tempTarget, Stereoscopic_Mode.Center);

            GraphicsDevice.SetRenderTarget(tempTarget);

            renderTarget = tempTarget;

            Effect shader = Global.effect_shader();

            // Change back to the back buffer, so the final rendertarget is available as a texture
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            shader = Global.effect_shader(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            // Determines resize scale to best fit game ratio to window/screen size
            Vector2 widthRatio = new Vector2(graphics.PreferredBackBufferWidth / (float)renderTarget.Width);
            float ratio = ScreenSizeRatio;

            Matrix shaderMatrix = ScreenSpaceMatrix(
                new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                new Vector2(renderTarget.Width, renderTarget.Height), widthRatio, ratio);
            shader.Parameters["MatrixTransform"].SetValue(shaderMatrix);
            shader.CurrentTechnique = shader.Techniques["Normal"];

#if DEBUG && !MONOGAME
            // Draw a debug view screen
            if (DEBUG_VIEW && !GameRenderer.IsFullscreen &&
                !Global.gameSettings.Graphics.Stereoscopic && zoom >= 3)
            {
                DrawDebugView(spriteBatch, renderTarget, shader);
            }
            else
#endif
            {
#if !__ANDROID__
                if (!GameRenderer.IsFullscreen && shader.CurrentTechnique == shader.Techniques["Normal"])
                    shader = null;
#endif
                if (Global.gameSettings.Graphics.Stereoscopic &&
                        !Global.gameSettings.Graphics.Anaglyph && stereoTarget != null)
                    DrawToScreen(
                        spriteBatch, renderTarget, stereoTarget, shader, camera.matrix, widthRatio, ratio);
                else
                    DrawToScreen(
                        spriteBatch, renderTarget, shader, camera.matrix, widthRatio, ratio);
            }
        }

        private void CopyMouseRender(
            SpriteBatch spriteBatch,
            RenderTarget2D renderTarget,
            RenderTarget2D tempTarget,
            Stereoscopic_Mode stereo)
        {
            GraphicsDevice.SetRenderTarget(tempTarget);
            GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null, null, null);
            switch (stereo)
            {
                case Stereoscopic_Mode.Center:
                default:
                    spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
                    break;
                case Stereoscopic_Mode.Left:
                    spriteBatch.Draw(renderTarget, Vector2.Zero,
                        new Rectangle(0, 0,
                            renderTarget.Width / 2, renderTarget.Height),
                        Color.White);
                    break;
                case Stereoscopic_Mode.Right:
                    spriteBatch.Draw(renderTarget, Vector2.Zero,
                        new Rectangle(renderTarget.Width / 2, 0,
                            renderTarget.Width / 2, renderTarget.Height),
                        Color.White);
                    break;
            }
#if !__MOBILE__
            if (Tactile.Input.IsControllingOnscreenMouse)
            {
                this.spriteBatch.Draw(MouseCursorTexture,
                    Global.Input.mousePosition,
                    null, Color.White, 0f, new Vector2(20, 20), 1f, SpriteEffects.None, 0f);
            }
            else if (Tactile.Input.ControlScheme == ControlSchemes.Touch)
            {
                Color tint = new Color(
                    TouchCursorOpacity, TouchCursorOpacity, TouchCursorOpacity, TouchCursorOpacity);
                this.spriteBatch.Draw(MouseCursorTexture,
                    TouchCursorLoc,
                    null, tint, 0f, new Vector2(20, 20), 1f, SpriteEffects.None, 0f);
            }
#endif
            spriteBatch.End();
        }

        private void DrawDebugView(
            SpriteBatch spriteBatch, RenderTarget2D renderTarget, Effect shader)
        {
            int width = ShaderRenderTargets[0].Width;
            int height = ShaderRenderTargets[0].Height;

            // Draw 2x scale render in top right
            shader = Global.effect_shader(width * (zoom - 1), height * (zoom - 1));
            DrawToDebugScreen(spriteBatch, renderTarget, null, zoom - 1, new Vector2(width, 0));
            // Draw 1x scale render in top left
            shader = Global.effect_shader(width * 1, height * 1);
            DrawToDebugScreen(spriteBatch, renderTarget, null, 1, new Vector2(0, height * (zoom - 2) / 2));
            // Draw 1x scale color deficient renders across the bottom
            shader.CurrentTechnique = shader.Techniques["Protanopia"];
            DrawToDebugScreen(spriteBatch, renderTarget, shader, 1,
                new Vector2(width * 0, height * (zoom - 1)));
            shader.CurrentTechnique = shader.Techniques["Deuteranopia"];
            DrawToDebugScreen(spriteBatch, renderTarget, shader, 1,
                new Vector2(width * 1, height * (zoom - 1)));
            shader.CurrentTechnique = shader.Techniques["Tritanopia"];
            DrawToDebugScreen(spriteBatch, renderTarget, shader, 1,
                new Vector2(width * 2, height * (zoom - 1)));
            // Draw labels for the different renders
            var font = Global.Content.Load<SpriteFont>(@"DiagFont");
            Vector2 textSize;
            string debugText;
            Vector2 loc;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            debugText = string.Format("{0}x scale", zoom - 1);
            textSize = font.MeasureString(debugText);
            loc = new Vector2(width - 4, 4) -
                new Vector2(textSize.X, 0);
            loc = new Vector2((int)loc.X, (int)loc.Y);
            spriteBatch.DrawString(
                font,
                debugText,
                loc,
                Color.White);

            debugText = "1x scale";
            textSize = font.MeasureString(debugText);
            loc = new Vector2(width * 0.5f, height * ((zoom - 2) * 0.5f) - 4) -
                textSize / new Vector2(2, 1);
            loc = new Vector2((int)loc.X, (int)loc.Y);
            spriteBatch.DrawString(
                font,
                debugText,
                loc,
                Color.White);

            debugText = "Protanopia(-R) | Deuteranopia(-G) | Tritanopia(-B)";
            textSize = font.MeasureString(debugText);
            loc = new Vector2(width * 0.5f, height * (zoom - 1) - 4) -
                textSize / new Vector2(2, 1);
            loc = new Vector2((int)loc.X, (int)loc.Y);
            spriteBatch.DrawString(
                font,
                debugText,
                loc,
                Color.White);
            spriteBatch.End();
        }

        private void DrawToDebugScreen(SpriteBatch spriteBatch, RenderTarget2D renderTarget, Effect shader, float scale, Vector2 loc)
        {
            int width = ShaderRenderTargets[0].Width;
            int height = ShaderRenderTargets[0].Height;

            Vector2 widthRatio = new Vector2(scale);
            float ratio = scale;
            camera.pos = loc + new Vector2(width * scale / 2,
                height * scale / 2);
            camera.zoom = new Vector2(ratio);
            if (shader != null)
            {
                Matrix shaderMatrix = ScreenSpaceMatrix(
                    new Vector2(width * scale, height * scale),
                    new Vector2(width, height),
                        widthRatio, (width * scale) / graphics.PreferredBackBufferWidth,
                    loc - new Vector2(
                        (graphics.PreferredBackBufferWidth - width) / 2,
                        (graphics.PreferredBackBufferHeight - height) / 2));
                shader.Parameters["MatrixTransform"].SetValue(shaderMatrix);
            }
            DrawToScreen(spriteBatch, renderTarget, shader, camera.matrix, widthRatio, ratio);
        }

        private void DrawToScreen(SpriteBatch spriteBatch, RenderTarget2D renderTarget,
            Effect shader, Matrix m, Vector2 widthRatio, float ratio)
        {
            BeginDrawToScreenSpriteBatch(
                spriteBatch, new Vector2(renderTarget.Width, renderTarget.Height),
                shader, m, widthRatio, ratio);

            if (Global.gameSettings.Graphics.Stereoscopic &&
                !Global.gameSettings.Graphics.AnaglyphMode)
            {
                spriteBatch.Draw(
                    renderTarget,
                    new Vector2(0, 0),
                    new Rectangle(0, 0,
                        (int)(renderTarget.Width * (camera.zoom.X * 2 / ratio)),
                        renderTarget.Height),
                    Color.White);
            }
            else
            {
                spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            }

            spriteBatch.End();
        }
        private void DrawToScreen(
            SpriteBatch spriteBatch, RenderTarget2D leftTarget, RenderTarget2D rightTarget,
            Effect shader, Matrix m, Vector2 widthRatio, float ratio)
        {
            BeginDrawToScreenSpriteBatch(
                spriteBatch, new Vector2(leftTarget.Width * 1.5f, leftTarget.Height),
                shader, m, widthRatio, ratio);

            spriteBatch.Draw(
                leftTarget,
                new Vector2(0, 0),
                new Rectangle(0, 0,
                    (int)(leftTarget.Width * (camera.zoom.X * 2 / ratio)),
                    leftTarget.Height),
                Color.White);

            spriteBatch.Draw(rightTarget,
                new Vector2(rightTarget.Width * (widthRatio.X / camera.zoom.X) / 2, 0),
                new Rectangle(
                    0, 0,
                    (int)(rightTarget.Width * 2 * (camera.zoom.X / ratio)),
                    rightTarget.Height),
                Color.White);

            spriteBatch.End();
        }

        private void BeginDrawToScreenSpriteBatch(
            SpriteBatch spriteBatch, Vector2 size,
            Effect shader, Matrix m, Vector2 widthRatio, float ratio)
        {
#if !__MOBILE__
            if (GameRenderer.IsFullscreen)
#endif
            {
                BeginFullscreenSpriteBatch(spriteBatch,
                    size, shader, widthRatio, ratio);
            }
#if !__MOBILE__
            else
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.PointClamp, null, null, shader, m);
#endif
        }

        private void BeginFullscreenSpriteBatch(
            SpriteBatch spriteBatch, Vector2 renderTargetSize, Effect shader,
            Vector2 widthRatio, float ratio)
        {
            if (shader != null)
            {
                shader.CurrentTechnique = shader.Techniques["Coverage_Shader"];
                shader.Parameters["game_size"].SetValue(renderTargetSize);
                shader.Parameters["display_scale"].SetValue(new Vector2(ratio));

                // 'Disables' the shader and uses normal lerp, for testing that the positioning is right //@Debug
                if (false)
                {
                    shader.Parameters["game_size"].SetValue(
                        new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
                    shader.Parameters["display_scale"].SetValue(new Vector2(1));
                }

                Matrix fullscreenMatrix = ScreenSpaceMatrix(
                    new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                    renderTargetSize, widthRatio, ratio);
                shader.Parameters["MatrixTransform"].SetValue(fullscreenMatrix);
            }
            // Matrix parameter doesn't actually work when using a custom vertex shader, herp
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null, null, shader);
        }

        private Matrix ScreenSpaceMatrix(Vector2 targetSize, Vector2 sourceRenderSize, Vector2 widthRatio, float ratio)
        {
            return ScreenSpaceMatrix(targetSize, sourceRenderSize, widthRatio, ratio, Vector2.Zero);
        }
        private Matrix ScreenSpaceMatrix(Vector2 targetSize, Vector2 sourceRenderSize, Vector2 widthRatio, float ratio, Vector2 offset)
        {
            Matrix matrix = Matrix.Identity *
                // Move top left corner to the origin
                Matrix.CreateTranslation(new Vector3(1f, -1f, 0)) *
                // Move downright another pixel, for some reason
                Matrix.CreateTranslation(new Vector3(1f / (WindowWidth * ratio), -1f / (WindowHeight * ratio), 0)) *
                // Move the the center of the render to the center of the screen
                Matrix.CreateTranslation(new Vector3(
                    -(1 / (targetSize.X / sourceRenderSize.X)),
                    (1 / (targetSize.Y / sourceRenderSize.Y)), 0));
            if (Global.gameSettings.Graphics.Stereoscopic &&
                !Global.gameSettings.Graphics.AnaglyphMode)
            {
                matrix *= Matrix.CreateTranslation(new Vector3((0.5f / widthRatio.X) - (1 / ratio), 0, 0)) *
                    Matrix.CreateScale(0.5f, 1, 1);
            }
            matrix *=
                // Scale up to the size of the screen
                Matrix.CreateScale(ratio, ratio, 1) *
                // Adjust for weird half pixel offset
                Matrix.CreateTranslation(new Vector3( // Can't actually do the half pixel before scaling because it messes up! //@Debug
                    -(1f / targetSize.X),
                    (1f / targetSize.Y), 0)) *
                // Add offset
                Matrix.CreateTranslation(new Vector3(
                    offset.X / (graphics.PreferredBackBufferWidth / 2),
                    -offset.Y / (graphics.PreferredBackBufferHeight / 2), 0));
            return matrix;
        }

        public void SaveScreenshot(Stream stream)
        {
            FinalRender.SaveAsPng(stream, FinalRender.Width, FinalRender.Height);
        }
        #endregion
    }
}
