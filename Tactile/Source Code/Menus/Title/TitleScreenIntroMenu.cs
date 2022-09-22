using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Menus.Title
{
    class TitleIntroMenu : BaseMenu
    {
        protected const int SWORD_BASE_Y = 96;
        protected const int SWORD_BASE_X = 0;
        const int SWORD_X_MOVE = -192;
        protected const int SUB_LOGO_X = 0;

        protected int Action = 0;
        protected int Timer = 0;
        protected Sprite Sword;
        protected Sprite SubLogo;
        protected Sprite Flash;

        public TitleIntroMenu()
        {
            InitializeImages();
        }

        protected virtual void InitializeImages()
        {
            Sword = new Sprite();
            Sword.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Titles/TitleSword");
            Sword.loc = new Vector2(SWORD_BASE_X + SWORD_X_MOVE, SWORD_BASE_Y);
            Sword.offset = new Vector2(0, Sword.texture.Height / 2);
            Sword.stereoscopic = Config.TITLE_SWORD_DEPTH;
            SubLogo = new Sprite();
            SubLogo.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Titles/TitleSubLogo");
            SubLogo.loc = new Vector2(SUB_LOGO_X + 52 - 8, 96);
            SubLogo.offset = new Vector2(0, SubLogo.texture.Height / 2);
            SubLogo.tint = new Color(0, 0, 0, 0);
            SubLogo.stereoscopic = Config.TITLE_LOGO_DEPTH;
            Flash = new Sprite();
            Flash.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Flash.tint = new Color(0, 0, 0, 0);
            Flash.dest_rect = new Rectangle(
                0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
        }

        public event EventHandler<EventArgs> Closed;
        private void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }

        protected override void UpdateMenu(bool active)
        {
            if (Global.Input.triggered(Inputs.Start) ||
                    Global.Input.triggered(Inputs.A) ||
                    Global.Input.any_mouse_triggered ||
                    Global.Input.gesture_triggered(TouchGestures.Tap) ||
                    Global.Input.gesture_triggered(TouchGestures.LongPress))
                OnClosed(new EventArgs());
            else
            {
                switch (Action)
                {
                    // Pause
                    case 0:
                        Timer++;
                        if (Timer > 60)
                        {
                            Action++;
                            Timer = 0;
                        }
                        break;
                    // FE Logo Appears
                    case 1:
                        SubLogo.loc.X++;
                        int opacity = Math.Min(SubLogo.tint.A + 32, 255);
                        SubLogo.tint = new Color(opacity, opacity, opacity, opacity);
                        if (opacity >= 255)
                            Action++;
                        break;
                    // Pause again
                    case 2:
                        Timer++;
                        if (Timer > 60)
                        {
                            Action++;
                            Timer = 0;
                        }
                        break;
                    // Slide logo
                    case 3:
                        SubLogo.loc.X -= 4;
                        if (SubLogo.loc.X <= SUB_LOGO_X)
                            Action++;
                        break;
                    // Pause again
                    case 4:
                        Timer++;
                        if (Timer > 6)
                        {
                            Action++;
                            Timer = 0;
                        }
                        break;
                    // Sword moves
                    case 5:
                        if (Sword.loc.X < SWORD_BASE_X)
                        {
                            Sword.loc.X += 16;
                            if (Sword.loc.X >= SWORD_BASE_X)
                                Action++;
                        }
                        else if (Sword.loc.X > SWORD_BASE_X)
                        {
                            Sword.loc.X -= 16;
                            if (Sword.loc.X <= SWORD_BASE_X)
                                Action++;
                        }
                        else
                            Action++;
                        break;
                    // Screen flash
                    case 6:
                        switch (Timer)
                        {
                            case 0:
                                Flash.tint = new Color(255, 255, 255, 255);
                                break;
                            case 2:
                                // One frame long black flash between the white flash? @Debug
                                //Flash.tint = new Color(0, 0, 0, 255); //@Debug
                                OnClosed(new EventArgs());
                                break;
                        }
                        Timer++;
                        break;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Sword.draw(spriteBatch);
            SubLogo.draw(spriteBatch);
            Flash.draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
