using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Android_MonoGame_Test
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });
            Font = Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Convo_Yellow");
            ShaderEffect = Content.Load<Effect>(@"Effect");
            Matrix projection = Matrix.CreateOrthographicOffCenter(
                //0, width * render_target_zoom, height * render_target_zoom, 0, -10000, 10000); //Debug
                0, graphics.PreferredBackBufferWidth * 1,
                graphics.PreferredBackBufferHeight * 1,
                0, -10000, 10000);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            ShaderEffect.Parameters["World"].SetValue(Matrix.Identity);
            ShaderEffect.Parameters["View"].SetValue(Matrix.Identity);
            ShaderEffect.Parameters["Projection"].SetValue(halfPixelOffset * projection);

            ShaderEffect.Parameters["MatrixTransform"].SetValue(Matrix.Identity);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            if (Text == null)
            {
                Text = new FEXNA.Graphics.Text.FE_Text();
                Text.loc = new Vector2(8, 8);
                Text.Font = "FE7_Convo";
                Text.text = "Here is some test text to see if this is working.\nLet's hope it is!";
                Text.texture = Font;
                Text.scale = new Vector2(4);
            }

            // TODO: Add your update logic here
            wah = new Vector2(r.Next(0, graphics.PreferredBackBufferWidth - graphics.PreferredBackBufferWidth / 10),
                r.Next(0, graphics.PreferredBackBufferHeight - graphics.PreferredBackBufferHeight / 10));

            float hue = (float)(gameTime.TotalGameTime.TotalSeconds * 60) % 360;
            Tint = FEXNA_Library.Color_Util.HSLToColor(hue, 1f, 0.5f);

            base.Update(gameTime);
        }

        Color Tint;
        Texture2D Pixel;
        Texture2D Font;
        Effect ShaderEffect;
        FEXNA.Graphics.Text.FE_Text Text;
        Vector2 wah = new Vector2();
        System.Random r = new System.Random();

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            FEXNA.Tone tone = new FEXNA.Tone(Tint.R / 2, Tint.G / 2, Tint.B / 2, 0);
            ShaderEffect.CurrentTechnique = ShaderEffect.Techniques["Tone"];
            ShaderEffect.Parameters["tone"].SetValue(tone.to_vector_4());
            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, ShaderEffect);
            //spriteBatch.Draw(Font, new Rectangle(0, 0,
            //    Font.Width, Font.Height),
            //    Color.White);
            Text.draw(spriteBatch);
            spriteBatch.Draw(Pixel, new Rectangle((int)wah.X, (int)wah.Y,
                graphics.PreferredBackBufferWidth / 10, graphics.PreferredBackBufferHeight / 10),
                Tint);//Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
