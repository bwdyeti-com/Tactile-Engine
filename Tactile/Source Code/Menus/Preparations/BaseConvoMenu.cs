using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows;
using Tactile.Windows.Command;

namespace Tactile.Menus.Preparations
{
    class BaseConvoMenu : CommandMenu
    {
        const int FADE_TIME = 16;

        private bool Event_Selected = false;
        private bool Event_Started = false;
        private int Black_Screen_Timer = 0;
        private Sprite Black_Screen;

        #region Accessors
        public bool event_selected { get { return Event_Selected; } }

        public bool start_event { get { return Event_Started && Black_Screen_Timer == 0; } }

        public bool event_ending { get { return Event_Started && Black_Screen_Timer > 0; } }

        public bool event_ended { get { return Event_Started && !Event_Selected; } }
        #endregion

        public BaseConvoMenu(Vector2 loc)
            : base(BaseConvoWindow(loc)) { }

        private static Window_Command BaseConvoWindow(Vector2 loc)
        {
            return new Window_Base_Convos(loc);
        }

        protected override void UpdateMenu(bool active)
        {
            base.UpdateMenu(active &&
                !Event_Selected &&
                !Global.game_system.is_interpreter_running);

            // Reset fade timer
            if (!Global.game_system.is_interpreter_running &&
                Event_Selected && Event_Started && Black_Screen_Timer == 0)
            {
                Black_Screen_Timer = FADE_TIME;
                if (EventStopped != null)
                    EventStopped(this, new EventArgs());
            }

            if (!Global.game_system.is_interpreter_running && Event_Selected)
            {
                if (Event_Started)
                {
                    // Fade in to menu
                    Black_Screen_Timer--;
                    if (Black_Screen_Timer == 0)
                    {
                        EndEvent(this, new EventArgs());
                        Event_Selected = false;
                        Event_Started = false;
                    }
                    Black_Screen.tint = new Color(0, 0, 0, Black_Screen_Timer * (256 / FADE_TIME));
                }
                else
                {
                    // Fade out to base event
                    Black_Screen_Timer--;
                    if (Black_Screen_Timer == 0)
                    {
                        StartEvent(this, new EventArgs());
                        Event_Started = true;
                    }
                    Black_Screen.tint = new Color(0, 0, 0, Math.Min(255, 256 - Black_Screen_Timer * (256 / FADE_TIME)));
                }
            }
        }

        public event EventHandler<EventArgs> StartEvent;
        public event EventHandler<EventArgs> EventStopped;
        public event EventHandler<EventArgs> EndEvent;

        public bool SelectEvent()
        {
            if (!Global.game_state.base_event_ready(Window.index) ||
                    StartEvent == null || EndEvent == null)
                return false;
            else
            {
                Event_Selected = true;
                Black_Screen = new Sprite();
                Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
                Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                Black_Screen.tint = new Color(0, 0, 0, 0);
                Black_Screen_Timer = FADE_TIME;
                return true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Event_Started)
            {
                base.Draw(spriteBatch);
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (Black_Screen != null)
                Black_Screen.draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
