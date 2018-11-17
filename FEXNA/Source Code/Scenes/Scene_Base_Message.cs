using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;

namespace FEXNA
{
    partial class Scene_Base
    {
        const int SKIP_INTERACTION_TIMEOUT = 240;

        protected Window_Message Message_Window;

        protected virtual bool has_convo_scene_button { get { return true; } }

        private bool SkipButtonsOnScreen = false;
        private Button_Description SkipButton, SceneButton;
        private Vector2 SkipButtonOffset;
        private int TimeSinceSkipInteraction = 0;
        private bool _EventSkip;

        protected bool EventSkip { get { return _EventSkip; } }

        private void create_convo_skip_buttons()
        {
            SkipButton = Button_Description.button(Inputs.Start,
                Config.WINDOW_WIDTH - 40);
            SkipButton.description = "Skip";
            SkipButton.stereoscopic = Config.CONVO_TEXT_DEPTH;

            if (has_convo_scene_button)
            {
                SceneButton = Button_Description.button(Inputs.B,
                    Config.WINDOW_WIDTH - 80);
                SceneButton.description = "Scene";
                SceneButton.stereoscopic = Config.CONVO_TEXT_DEPTH;
            }
        }

        protected virtual void main_window()
        {
            Message_Window = new Window_Message();
            Message_Window.stereoscopic = Config.CONVO_TEXT_DEPTH;
            Message_Window.face_stereoscopic = Config.CONVO_FACE_DEPTH;
        }

        public bool is_message_window_active
        {
            get
            {
                if (Message_Window != null)
                {
                    if (Message_Window.active && !Message_Window.waiting_for_event)
                        return true;
                    if (Message_Window.is_clearing_faces)
                        return true;
                }
                return false;
            }
        }

        public bool is_message_window_waiting
        {
            get
            {
                if (Message_Window != null)
                {
                    if (Message_Window.waiting_for_event)
                        return true;
                }
                return false;
            }
        }

        public virtual void new_message_window()
        {
            if (Message_Window == null)
                return;
            Message_Window.reset(true);
        }

        public virtual void append_message()
        {
            if (Message_Window == null)
                return;
            Message_Window.append_new_text();
        }

        public virtual void resume_message()
        {
            if (Message_Window == null)
                return;
            Message_Window.resume();
        }

        public virtual void end_waiting_message_window()
        {
            if (Message_Window == null)
                return;
            Message_Window.terminate_message();
        }

        public void message_reverse()
        {
            message_reverse(true);
        }
        public void message_reverse(bool val)
        {
            Message_Window.reverse = val;
        }

        public bool message_window_skipping_event
        {
            get
            {
                // Event skip button pressed
                if (this.message_skip_button_pressed)
                    return true;

                if (Message_Window != null)
                    return Message_Window.event_skip;
                return false;
            }
        }

        public bool message_skip_button_pressed
        {
            get
            {
                // Event skip button pressed
                if (_EventSkip)
                {
                    if (Message_Window == null || !Message_Window.active)
                        return true;
                }

                return false;
            }
        }

        public bool message_background
        {
            get
            {
                if (Message_Window == null)
                    return false; return Message_Window.background_active;
            }
        }

        public virtual void event_skip()
        {
            if (Message_Window != null)
            {
                Message_Window.event_skip = false;
                Message_Window.terminate_message(true);
            }
        }

        #region Update
        protected virtual void update_message()
        {
            update_message_skip_buttons();

            if (Message_Window != null)
            {
                bool active = is_message_window_active;
                Message_Window.update();
                if (!is_message_window_active && active)
                    Global.dispose_face_textures();
            }
        }

        protected virtual bool skip_convo_button_active
        {
            get
            {
                bool no_convo = Message_Window == null || !Message_Window.active;
                bool result =
                    Global.game_system.is_interpreter_running || !no_convo;

                if (Message_Window != null)
                    result &= !Message_Window.backlog_active &&
                        !Message_Window.closing;

                return result;
            }
        }

        protected void update_message_skip_buttons()
        {
            _EventSkip = false;

            bool no_convo = Message_Window == null || !Message_Window.active;
            bool skip_button_active = this.skip_convo_button_active;
            
            // Create buttons if needed
            if (SkipButton == null)
            {
                if (skip_button_active)
                {
                    create_convo_skip_buttons();
                    SkipButtonOffset = new Vector2(0, 16);
                    SkipButtonsOnScreen = false;
                }
            }

            if (SkipButton != null)
            {
                // Bring buttons onscreen if needed
                if (!SkipButtonsOnScreen)
                {
                    SkipButtonOffset.Y = MathHelper.Min(16, SkipButtonOffset.Y + 2);
                    if (skip_button_active &&
                        Global.Input.gesture_triggered(TouchGestures.SwipeUp))
                    {
                        SkipButtonsOnScreen = true;
                        TimeSinceSkipInteraction = 0;
                    }
                }
                // Move buttons offscreen if needed
                else if (SkipButtonsOnScreen)
                {
                    if (TimeSinceSkipInteraction < SKIP_INTERACTION_TIMEOUT)
                        TimeSinceSkipInteraction++;

                    SkipButtonOffset.Y = MathHelper.Max(0, SkipButtonOffset.Y - 2);
                    if (!skip_button_active ||
                        TimeSinceSkipInteraction >= SKIP_INTERACTION_TIMEOUT ||
                        (skip_button_active &&
                            Global.Input.gesture_triggered(TouchGestures.SwipeDown)))
                    {
                        SkipButtonsOnScreen = false;
                    }
                }

                skip_button_active &= SkipButtonsOnScreen && SkipButtonOffset.Y == 0;

                TextSkips skip = TextSkips.None;

                if (Input.ControlSchemeSwitched ||
                        (has_convo_scene_button && SceneButton == null))
                    create_convo_skip_buttons();
                if (SceneButton != null && !has_convo_scene_button)
                    SceneButton = null;

                // Update scene button and consume inputs
                if (SceneButton != null)
                {
                    SceneButton.Update(skip_button_active && !no_convo,
                        -SkipButtonOffset);
                    if (no_convo)
                        SceneButton.tint = new Color(128, 128, 128, 255);
                    if (SceneButton.consume_trigger(MouseButtons.Left) ||
                            SceneButton.consume_trigger(TouchGestures.Tap))
                    {
                        skip = TextSkips.NextScene;
                        TimeSinceSkipInteraction = SKIP_INTERACTION_TIMEOUT - 60;
                    }
                }

                // Update skip button and consume inputs
                SkipButton.Update(skip_button_active, -SkipButtonOffset);
                if (SkipButton.consume_trigger(MouseButtons.Left) ||
                        SkipButton.consume_trigger(TouchGestures.Tap))
                {
                    skip = TextSkips.SkipEvent;
                    TimeSinceSkipInteraction = 0;
                }

                if (!no_convo)
                {
                    Message_Window.ConvoSkip = skip;
                }
                else if (skip == TextSkips.SkipEvent)
                {
                    _EventSkip = true;
                }
            }
        }
        #endregion

        #region Draw
        protected void draw_message(SpriteBatch sprite_batch)
        {
            if (Message_Window != null)
            {
                Message_Window.draw_background(sprite_batch);
                Message_Window.draw_faces(sprite_batch);
                Message_Window.draw_foreground(sprite_batch);
            }

            draw_message_overlay(sprite_batch);
        }

        protected virtual void draw_message_overlay(SpriteBatch spriteBatch)
        {

            if (SkipButton != null && Input.ControlScheme == ControlSchemes.Touch)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                SkipButton.Draw(spriteBatch, -SkipButtonOffset);
                if (SceneButton != null)
                    SceneButton.Draw(spriteBatch, -SkipButtonOffset);
                spriteBatch.End();
            }
        }
        #endregion
    }
}
