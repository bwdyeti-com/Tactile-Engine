using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Menus.Title
{
    class TitleScreenMenu : TitleIntroMenu
    {
        private Title_Background Background;
        private Sprite ISLogo;
        private Press_Start StartImage;

        protected override void InitializeImages()
        {
            Background = new Title_Background();
            Background.stereoscopic = Config.TITLE_BG_DEPTH;
            Sword = new Sprite();
            Sword.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Titles/TitleSword");
            Sword.loc = new Vector2(SWORD_BASE_X, SWORD_BASE_Y);
            Sword.offset = new Vector2(0, Sword.texture.Height / 2);
            Sword.stereoscopic = Config.TITLE_SWORD_DEPTH;
            ISLogo = new Sprite();
            ISLogo.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Titles/TitleScreenLogo");
            ISLogo.loc = new Vector2(Config.WINDOW_WIDTH / 2, Config.WINDOW_HEIGHT / 2);
            ISLogo.offset = new Vector2(ISLogo.texture.Width / 2, ISLogo.texture.Height / 2);
            ISLogo.stereoscopic = Config.TITLE_LOGO_DEPTH;
            SubLogo = new Sprite();
            SubLogo.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Titles/TitleSubLogo");
            SubLogo.loc = new Vector2(SUB_LOGO_X, 96);
            SubLogo.offset = new Vector2(0, SubLogo.texture.Height / 2);
            SubLogo.stereoscopic = Config.TITLE_LOGO_DEPTH;
            StartImage = new Press_Start(Global.Content.Load<Texture2D>(
                @"Graphics/Pictures/Press Start"));
            StartImage.loc = new Vector2(Config.WINDOW_WIDTH / 2,
                Config.WINDOW_HEIGHT - 48);
            StartImage.visible = false;
            StartImage.stereoscopic = Config.TITLE_CHOICE_DEPTH;
            Flash = new Sprite();
            Flash.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Flash.tint = new Color(255, 255, 255, 255);
            Flash.dest_rect = new Rectangle(
                0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
        }

        public event EventHandler<EventArgs> PressedStart;
        protected void OnPressedStart(EventArgs e)
        {
            if (PressedStart != null)
                PressedStart(this, e);
        }

        public event EventHandler<EventArgs> ClassReel;
        protected bool OnClassReel(EventArgs e)
        {
            if (ClassReel != null)
            {
                ClassReel(this, e);
                return true;
            }
            return false;
        }

        protected override void Activate()
        {
            if (Action > 0)
            {
                Timer = 0;
                ShowPressStart();
            }
        }

        public void HideStart()
        {
            StartImage.visible = false;
        }

        protected override void UpdateMenu(bool active)
        {
            Background.update();
            StartImage.update();

            switch (Action)
            {
                // Fade In
                case 0:
                    Flash.opacity -= 32;
                    if (Flash.opacity <= 0)
                    {
                        switch (Timer)
                        {
                            case 32:
                                ShowPressStart();
                                Timer = 0;
                                Action = 1;
                                break;
                            default:
                                Timer++;
                                break;
                        }
                    }
                    break;
                // Wait for Input
                case 1:
                    if (Config.CLASS_REEL_WAIT_TIME > -1)
                    {
                        Timer++;
                        if (Timer > (Config.CLASS_REEL_WAIT_TIME * Config.FRAME_RATE))
                        {
                            if (OnClassReel(new EventArgs()))
                                return;
                        }
                    }
                    StartImage.opacity += 16;
                    if (active)
                    {
                        if (Global.Input.KeyPressed(Keys.Enter) ||
                            Global.Input.triggered(Inputs.Start) ||
                            Global.Input.triggered(Inputs.A) ||
                            Global.Input.any_mouse_triggered ||
                            Global.Input.gesture_triggered(TouchGestures.Tap))
                        {
                            Timer = 0;
                            Global.Audio.play_se("System Sounds", "Press_Start");
                            OnPressedStart(new EventArgs());
                        }
                    }
                    break;
            }
        }

        private void ShowPressStart()
        {
            StartImage.reset();
            StartImage.visible = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
                Background.draw(spriteBatch);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Sword.draw(spriteBatch);
                ISLogo.draw(spriteBatch);
                SubLogo.draw(spriteBatch);
                StartImage.draw(spriteBatch);
                Flash.draw(spriteBatch);
                spriteBatch.End();
        }
    }
}
