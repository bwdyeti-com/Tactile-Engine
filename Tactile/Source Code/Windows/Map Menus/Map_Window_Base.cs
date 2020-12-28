using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Menus;

namespace Tactile.Windows.Map
{
    abstract class Map_Window_Base : BaseMenu
    {
        const int BLACK_SCEEN_FADE_TIMER = 4;
        const int BLACK_SCREEN_HOLD_TIMER = 8;

        protected bool _Closing = false;
        new protected bool Visible = false; // @Debug: needs a more useful name like 'ContentsVisible' or 'FadedIn'
        protected int Black_Screen_Fade_Timer = BLACK_SCEEN_FADE_TIMER;
        protected int Black_Screen_Hold_Timer = BLACK_SCREEN_HOLD_TIMER;
        protected int Black_Screen_Timer;
        protected int Map_Sprite_Frame = -1;

        protected Sprite Background;
        protected Sprite Black_Screen;
        protected Window_Help Help_Window;
        protected Hand_Cursor Cursor; // Eventually this should be possible to remove, with all things that use it using their own UICursor instead //Yeti

        #region Accessors
        public bool closing { get { return _Closing; } }

        public bool closed { get { return Black_Screen_Timer <= 0 && _Closing; } }

        new public bool IsVisible { get { return Visible; } } // @Debug: this shouldn't need to be public, or the same name

        protected virtual bool ready_for_inputs { get { return !_Closing && Black_Screen_Timer <= 0; } }

        protected int BlackFadeMidpoint
        {
            get { return Black_Screen_Fade_Timer + (Black_Screen_Hold_Timer / 2); }
        }

        protected bool AtBlackFadeMidpoint
        {
            get { return Black_Screen_Timer == this.BlackFadeMidpoint; }
        }

        protected bool DataDisplayed
        {
            get { return (Black_Screen_Timer <= this.BlackFadeMidpoint) ^ _Closing; }
        }
        #endregion

        public Map_Window_Base()
        {
            set_black_screen_time();
        }

        protected virtual void set_black_screen_time()
        {
            Black_Screen_Timer = Black_Screen_Hold_Timer + (Black_Screen_Fade_Timer * 2);
        }

        public void black_screen()
        {
            Black_Screen_Timer = Black_Screen_Hold_Timer + Black_Screen_Fade_Timer;
            UpdateMenu(false);
        }

        public event EventHandler<EventArgs> Closing;
        protected void OnClosing(EventArgs e)
        {
            if (Closing != null)
                Closing(this, e);
        }

        public event EventHandler<EventArgs> Closed;
        protected void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }

        #region Update
        protected override void UpdateMenu(bool active)
        {
            // Black Screen
            update_black_screen();
            // Inputs
            update_input(active);
            if (Background != null)
                Background.update();
        }

        protected abstract void update_input(bool active);

        protected virtual void close()
        {
            OnClosing(new EventArgs());
            _Closing = true;
            Black_Screen_Timer = Black_Screen_Hold_Timer + (Black_Screen_Fade_Timer * 2);
            if (Black_Screen != null)
                Black_Screen.visible = true;
        }

        public override bool HidesParent { get { return false; } }

        protected void update_black_screen()
        {
            if (Black_Screen != null)
                Black_Screen.visible = Black_Screen_Timer > 0;
            if (Black_Screen_Timer > 0)
            {
                Black_Screen_Timer--;
                if (Black_Screen != null)
                {
                    if (Black_Screen_Timer > Black_Screen_Fade_Timer + (Black_Screen_Hold_Timer / 2))
                        Black_Screen.TintA = (byte)Math.Min(255,
                            (Black_Screen_Hold_Timer + (Black_Screen_Fade_Timer * 2) - Black_Screen_Timer) * (256 / Black_Screen_Fade_Timer));
                    else
                        Black_Screen.TintA = (byte)Math.Min(255,
                            Black_Screen_Timer * (256 / Black_Screen_Fade_Timer));
                }
                if (this.AtBlackFadeMidpoint)
                {
                    black_screen_switch();
                }

                if (_Closing && Black_Screen_Timer == 0)
                    OnClosed(new EventArgs());
            }
        }

        protected virtual void black_screen_switch()
        {
            Visible = !Visible;
        }
        #endregion

        #region Draw
        public override void Draw(SpriteBatch sprite_batch)
        {
            if (this.DataDisplayed)
            {
                // Background
                draw_background(sprite_batch);

                draw_window(sprite_batch);
            }
            // Black Screen
            if (Black_Screen != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Black_Screen.draw(sprite_batch);
                sprite_batch.End();
            }
        }

        protected virtual void draw_background(SpriteBatch sprite_batch)
        {
            if (Background != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Background.draw(sprite_batch);
                sprite_batch.End();
            }
        }

        protected virtual void draw_window(SpriteBatch sprite_batch) { }
        #endregion
    }
}
