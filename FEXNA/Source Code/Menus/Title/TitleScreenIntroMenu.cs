using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Menus.Title
{
    class TitleIntroMenu : BaseMenu
    {
        protected const int SWORD_BASE_Y = -44;
        const int SWORD_Y_MOVE = -272;

        protected int Action = 0;
        protected int Timer = 0;
        protected Sprite Sword;
        protected Sprite FELogo;
        protected Sprite Flash;

        public TitleIntroMenu()
        {
            InitializeImages();
        }

        protected virtual void InitializeImages()
        {
            Sword = new Sprite();
            Sword.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Pictures/Title Sword");
            Sword.loc = new Vector2(Config.WINDOW_WIDTH / 2,
                SWORD_BASE_Y + SWORD_Y_MOVE);
            Sword.offset = new Vector2(Sword.texture.Width / 2, 0);
            Sword.stereoscopic = Config.TITLE_SWORD_DEPTH;
            FELogo = new Sprite();
            FELogo.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Pictures/Fire Emblem Logo");
            FELogo.loc = new Vector2(44 + 52, 56 - 8 + 8);
            FELogo.tint = new Color(0, 0, 0, 0);
            FELogo.stereoscopic = Config.TITLE_LOGO_DEPTH;
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
                    Global.Input.gesture_triggered(TouchGestures.Tap))
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
                        FELogo.loc.Y++;
                        int opacity = Math.Min(FELogo.tint.A + 32, 255);
                        FELogo.tint = new Color(opacity, opacity, opacity, opacity);
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
                        FELogo.loc.X -= 4;
                        if (FELogo.loc.X <= 36)
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
                    // Sword moves down
                    case 5:
                        Sword.loc.Y += 16;
                        if (Sword.loc.Y >= SWORD_BASE_Y)
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
            FELogo.draw(spriteBatch);
            Flash.draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
