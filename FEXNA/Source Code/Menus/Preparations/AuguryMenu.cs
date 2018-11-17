using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Windows.Command;

namespace FEXNA.Menus.Preparations
{
    class AuguryMenu : BaseMenu
    {
        const int FADE_TIME = 16;

        private bool Event_Started = false;
        internal bool Event_Ended { get; private set; }
        private int Black_Screen_Timer = 0;
        private Sprite Black_Screen;

        #region Accessors
        public bool start_event { get { return Event_Started && Black_Screen_Timer == 0; } }
        #endregion

        public AuguryMenu()
        {
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 0);
            Black_Screen_Timer = FADE_TIME;
        }

        public event EventHandler<EventArgs> StartEvent;

        public event EventHandler<EventArgs> Closed;
        protected void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }

        #region IMenu
        public override bool HidesParent { get { return false; } }

        protected override void UpdateMenu(bool active)
        {
            if (Event_Started && Black_Screen_Timer == 0)
                Black_Screen_Timer = FADE_TIME;

            if (!Global.game_system.is_interpreter_running)
            {
                if (Event_Started)
                {
                    Black_Screen_Timer--;
                    if (Black_Screen_Timer == 0)
                    {
                        Event_Ended = true;
                        OnClosed(new EventArgs());
                    }
                    Black_Screen.tint = new Color(0, 0, 0, Black_Screen_Timer * (256 / FADE_TIME));
                }
                else
                {
                    Black_Screen_Timer--;
                    if (Black_Screen_Timer == 0)
                    {
                        Event_Started = true;
                        if (StartEvent != null)
                            StartEvent(this, new EventArgs());
                    }
                    Black_Screen.tint = new Color(0, 0, 0, Math.Min(255, 256 - Black_Screen_Timer * (256 / FADE_TIME)));
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            if (Black_Screen != null)
                Black_Screen.draw(spriteBatch);
            spriteBatch.End();
        }
        #endregion
    }
}
