using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Scene_Splash : Scene_Base
    {
        private readonly static string GAME_SITE = "yoursite.herethough/yourgame";

        private readonly static string[] DISCLAIMERS =
        {
#if PRERELEASE || DEBUG
            @"This is a preview build of {0}
intended for authorized testers only.

Please respect the developers and their
work and wait for an official release.

Visit http://{1} for more information."
#else
            @"Welcome to the beta build of {0}!

This game is still being developed, so new content will be
added and existing content may be modified or removed.
Expect unfinished features, and you might encounter bugs.

Visit http://{1} for more information,
or to give any feedback, and above all, have fun!"
#endif
        };

        private int Timer = -(Config.SPLASH_INITIAL_BLACK_TIME + 1);
        private int Image_Index = 0;
        private Sprite Splash, BlackScreen;
        private TextSprite PreviewString;
        private int DisclaimerIndex = 0;

        public Scene_Splash()
        {
            Scene_Type = "Scene_Splash";
            Splash = new Sprite();
            BlackScreen = new Sprite();
            BlackScreen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            BlackScreen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            BlackScreen.tint = new Color(0, 0, 0, 255);

            PreviewString = new TextSprite(
                Config.UI_FONT, Global.Content, "White",
                new Vector2(48, 24));
            set_disclaimer_text();

            set_splash_image();
            Global.game_map = null;
        }

        private string game_name()
        {
            return string.Format("\"{0}\"", Global.GAME_ASSEMBLY.GetName().Name);
        }

        protected void set_disclaimer_text()
        {
            PreviewString.text = "";
            if (DisclaimerIndex < DISCLAIMERS.Length)
            {
                Image_Index = -1;
                string disclaimer = string.Format(DISCLAIMERS[DisclaimerIndex],
                    game_name(), GAME_SITE);
                PreviewString.text = disclaimer;
            }
        }

        protected void set_splash_image()
        {
            if (Image_Index >= 0)
                Splash.texture = Global.Content.Load<Texture2D>(@"Graphics/Titles/" + Config.SPLASH_SCREENS[Image_Index]);
            BlackScreen.TintA = 255;
            //Splash.opacity = 0; //Debug
        }

        public override void update()
        {
            if (update_soft_reset())
                return;
            Timer++; //Debug

            if (Timer < 0)
                BlackScreen.TintA = 255;
            else if (Timer <= Config.SPLASH_FADE_TIME)
                BlackScreen.TintA = (byte)(255 - (255 * Timer) / Config.SPLASH_FADE_TIME);
                //Splash.opacity = (255 * Timer) / Config.SPLASH_FADE_TIME; //Debug
            else if (Config.SPLASH_TIME - Timer <= Config.SPLASH_FADE_TIME)
                BlackScreen.TintA = (byte)(255 - (255 * (Config.SPLASH_TIME - Timer)) / Config.SPLASH_FADE_TIME);
                //Splash.opacity = (255 * (Config.SPLASH_TIME - Timer)) / Config.SPLASH_FADE_TIME; //Debug

            if (Timer >= -(Config.SPLASH_INITIAL_BLACK_TIME - 10))
            {
                // Skip to title with start
                if (Global.Input.triggered(Inputs.Start) ||
                        Global.Input.gesture_triggered(TouchGestures.LongPress))
                    Global.scene_change("Scene_Title");
                else if (Timer >= Config.SPLASH_TIME ||
                    Global.Input.triggered(Inputs.A) ||
                    Global.Input.any_mouse_triggered ||
                    Global.Input.gesture_triggered(TouchGestures.Tap))
                {
                    Timer = 0;
                    if (DisclaimerIndex <= DISCLAIMERS.Length)
                    {
                        DisclaimerIndex++;
                        set_disclaimer_text();
                    }
                    if (DisclaimerIndex >= DISCLAIMERS.Length)
                    {
                        Image_Index++;
                        // Advance to title load if out of splash screens
                        if (Image_Index >= Config.SPLASH_SCREENS.Length)
                            Global.scene_change("Scene_Title_Load");
                        else
                            set_splash_image();
                    }
                }
            }

            base.update();
        }

        public override void draw(
            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
            Microsoft.Xna.Framework.Graphics.GraphicsDevice device,
            Microsoft.Xna.Framework.Graphics.RenderTarget2D[] render_targets)
        {
            base.draw(spriteBatch, device, render_targets);

            device.SetRenderTarget(render_targets[0]);
            device.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Splash.draw(spriteBatch);

            if (DisclaimerIndex < DISCLAIMERS.Length)
                PreviewString.draw(spriteBatch);

            BlackScreen.draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
