//#define WINDOWTEST
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    public class SceneContentLoad : Scene_Base
    {
        const int FADE_OUT_TIME = 8;
        const int LOADS_PER_FRAME = 4;

#if WINDOWTEST
        //Yeti
        FEXNA.Graphics.Windows.WindowPanel Window;
        int WindowIndex = 0;
        static Tuple<string, int[], Vector2>[] WINDOWS =
        {
            Tuple.Create(@"Graphics/Windowskins/FE_Window", (int[])null, Vector2.Zero),
            Tuple.Create(@"Graphics/Windowskins/TestWindow",
                new int[] { 8, 8, 8, 8, 8, 8 }, new Vector2(0, 168)),
            Tuple.Create(@"Graphics/Windowskins/TestWindow",
                new int[] { 8, 8, 8, 16, 8, 8 }, new Vector2(0, 136)),
            Tuple.Create(@"Graphics/Windowskins/TestWindow",
                new int[] { 16, 8, 8, 24, 8, 16 }, new Vector2(0, 88)),
            Tuple.Create(@"Graphics/Windowskins/TestWindow",
                new int[] { 16, 8, 16, 8, 16, 8 }, new Vector2(0, 56)),
            Tuple.Create(@"Graphics/Windowskins/TestWindow",
                new int[] { 16, 8, 48, 8, 0, 8 }, new Vector2(0, 40)),
            Tuple.Create(@"Graphics/Windowskins/TestWindow",
                new int[] { 16, 8, 64, 16, 8, 16 }, new Vector2(0, 0)),
        };
        Rectangle ScissorRect;
        protected RasterizerState ScissorState = new RasterizerState { ScissorTestEnable = true };
        FE_Text LoremText;
        Sprite Background;
#endif




        protected bool LoadingComplete = false;
        protected IEnumerator<string> ContentLoader;

        private int LoadTimer, FadeOutTimer;
        private FE_Text LoadText;

        protected bool ready_to_change_scene
        {
            get
            {
                return LoadingComplete && FadeOutTimer > FADE_OUT_TIME;
            }
        }

        public SceneContentLoad()
        {
            SoftResetBlocked = true;

            Global.Content.Load<Texture2D>(
                @"Graphics/White_Square");
            Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_Green");
            Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_White");

            LoadText = new FE_Text();
            LoadText.loc = new Vector2(
                Config.WINDOW_WIDTH - 48,
                Config.WINDOW_HEIGHT - 24);
            LoadText.Font = "FE7_Text";
            //LoadText.texture = Global.Content.Load<Texture2D>(
            //    @"Graphics/Fonts/FE7_Text_Green");
            LoadText.text = "Loading...";


#if WINDOWTEST
            //Yeti
            change_window();

            LoremText = new FE_Text();
            LoremText.loc = new Vector2(8, 8);
            LoremText.Font = "FE7_Text";
            LoremText.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_White");
            LoremText.text =
@"Here is some test text.
Lorem ipsum dolor sit amet, consectetur
adipiscing elit. Donec nec sodales purus.
Sed in iaculis purus. Mauris dolor dui,
consequat vel elit eget, euismod vestibulum
eros. Fusce nec facilisis est, eget suscipit
velit. Nullam vel fermentum lectus. Orci
varius natoque penatibus et magnis dis
parturient montes, nascetur ridiculus mus.
Vestibulum dolor massa, finibus a mi sed,
blandit maximus ex. Nam et ligula aliquet,
blandit arcu eget, congue nisl. Suspendisse
convallis euismod augue, quis scelerisque
nisl consectetur vitae. Aenean nec nisl
vitae libero semper laoreet.";
            Background = new Sprite(Global.Content.Load<Texture2D>(
                @"Graphics/Panoramas/Hill"));
#endif
        }

        public override void update_data()
        {
            base.update_data();

            LoadTimer = (LoadTimer + 1) % 60;


#if WINDOWTEST
            //Yeti
            if (!Global.Input.pressed(Inputs.Start))
                return;
#endif



            if (ContentLoader != null && !LoadingComplete)
            {
                for (int i = 0; i < LOADS_PER_FRAME; i++)
                    if (!ContentLoader.MoveNext())
                    {
                        load_complete();
                        break;
                    }
            }
        }

        public override void update_sprites()
        {
            var colors = new Dictionary<int, string>();

            int timer = LoadTimer / 4;
            if (timer > 0)
                colors.Add(0, "FE7_Text_White");
            colors.Add(timer, "FE7_Text_Green");
            colors.Add(timer + 1, "FE7_Text_White");

            LoadText.text_colors = colors;

            if (LoadingComplete)
            {
                FadeOutTimer++;
                int alpha = (FADE_OUT_TIME - FadeOutTimer) * 255 / FADE_OUT_TIME;
                LoadText.opacity = alpha;
            }

            LoadText.update();


#if WINDOWTEST
            //Yeti
            if (Global.Input.triggered(Inputs.A))
            {
                WindowIndex = (WindowIndex + 1) % WINDOWS.Length;
                change_window();
            }
            Window.width = Math.Max(0, (int)Global.Input.mousePosition.X / 8 * 8 - 8);
            Window.height = Math.Max(0, (int)Global.Input.mousePosition.Y / 8 * 8 - 8);

            LoremText.loc = new Vector2(8, 8);
            if (WINDOWS[WindowIndex].Item2 != null)
                LoremText.loc = new Vector2(WINDOWS[WindowIndex].Item2[0],
                    WINDOWS[WindowIndex].Item2[3]);
            LoremText.loc += Window.loc;

            ScissorRect = new Rectangle(
                (int)LoremText.loc.X, (int)LoremText.loc.Y,
                Window.body_width, Window.body_height);

            ScissorRect.Width = Math.Min(
                ScissorRect.Width, Config.WINDOW_WIDTH - ScissorRect.X);
            ScissorRect.Height = Math.Min(
                ScissorRect.Height, Config.WINDOW_HEIGHT - ScissorRect.Y);

            LoadText.text = string.Format("{0}, {1}", Window.width, Window.height);
#endif
        }


#if WINDOWTEST
        private void change_window()
        {
            var window = WINDOWS[WindowIndex];
            if (window.Item2 == null)
                Window = new FEXNA.Graphics.Windows.WindowPanel(Global.Content.Load<Texture2D>(
                    window.Item1), window.Item3);
            else
                Window = new FEXNA.Graphics.Windows.WindowPanel(Global.Content.Load<Texture2D>(
                    window.Item1), window.Item3,
                    window.Item2[0], window.Item2[1], window.Item2[2],
                    window.Item2[3], window.Item2[4], window.Item2[5]);
            Window.loc = new Vector2(8, 8);
        }
#endif

        protected void load_complete()
        {
            LoadingComplete = true;
        }

        public override void draw(
            SpriteBatch sprite_batch,
            GraphicsDevice device,
            RenderTarget2D[] render_targets)
        {
            device.SetRenderTarget(render_targets[0]);
            device.Clear(Color.Transparent);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            LoadText.draw_multicolored(sprite_batch);


#if WINDOWTEST
            //Yeti
            Background.draw(sprite_batch);
            Window.draw(sprite_batch);
#endif



            sprite_batch.End();


#if WINDOWTEST
            //Yeti
            sprite_batch.GraphicsDevice.ScissorRectangle = ScissorRect;
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                null, null, ScissorState);
            LoremText.draw(sprite_batch);
            sprite_batch.End();
#endif
        }
    }
}
