using System;
using System.Collections.Generic;
using System.Linq;
using FEXNA.Graphics.Text;
using FEXNA.Windows.Command;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FEXNA.Menus.Title
{
    class ConfigOptionsMenu : BaseMenu
    {
        const int TIMER_MAX = 16;

        private bool Accepting = false;
        private bool Closing = false;
        private int Timer = TIMER_MAX;
        private int OptionsTimer = 0;
        private Window_Config_Options OptionsWindow;
        private FE_Text VersionNumber;
        private Sprite BlackScreen;

        public bool IsReady { get { return Timer <= 0 && !Closing; } }

        public float Stereoscopic
        {
            set
            {
                OptionsWindow.stereoscopic = value;
                VersionNumber.stereoscopic = value;
            }
        }

        public override bool HidesParent { get { return false; } }

        public ConfigOptionsMenu()
        {
            OptionsWindow = new Window_Config_Options();

            BlackScreen = new Sprite();
            BlackScreen.texture =
                Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            BlackScreen.dest_rect = new Rectangle(
                0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            BlackScreen.tint = new Color(0, 0, 0, 0);
            // Version Number
            VersionNumber = new FE_Text();
            VersionNumber.loc = new Vector2(8, Config.WINDOW_HEIGHT - 16);
            VersionNumber.Font = "FE7_Text";
            VersionNumber.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_White");
            VersionNumber.text = string.Format("v {0}", Global.RUNNING_VERSION);
#if PRERELEASE || DEBUG
            VersionNumber.text += " - Private Beta";
#endif
        }

        private void SelectOption()
        {
            Accepting = true;
            OptionsWindow.select_option(Accepting);
        }
        private void CancelSelecting()
        {
            Accepting = false;
            OptionsWindow.select_option(Accepting);
        }

        #region Events
        public event EventHandler<EventArgs> Canceled;
        protected void OnCanceled(EventArgs e)
        {
            if (Canceled != null)
                Canceled(this, e);
        }

        public event EventHandler<EventArgs> Closed;
        protected void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }
        #endregion

        protected override void UpdateMenu(bool active)
        {
            if (Timer > 0)
            {
                Timer--;
                BlackScreen.tint = new Color(0, 0, 0,
                    (Closing ? Timer : (TIMER_MAX - Timer)) * 128 / TIMER_MAX);
            }

            if (OptionsTimer <= 0)
            {
                OptionsWindow.update(active && this.IsReady && !Accepting);

                UpdateInput();
            }
            else
                OptionsTimer--;
        }

        private void UpdateInput()
        {
            if (Timer == 0 && Closing)
                OnClosed(new EventArgs());
            else if (Accepting)
            {
                // Changing key config
                if (OptionsWindow.index >= OptionsWindow.NonControlOptions)
                {
                    UpdateKeyConfig();
                }
                else
                {
                    if (Global.Input.KeyPressed(Keys.Enter) ||
                        Global.Input.triggered(Inputs.A) ||
                        Global.Input.triggered(Inputs.Start))
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Set();
                        CancelSelecting();
                    }
                    else if (Global.Input.triggered(Inputs.B) ||
                        Global.Input.KeyPressed(Keys.Escape))
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        OptionsWindow.reset();
                        CancelSelecting();
                    }
                }
            }
            else
            {
                if (OptionsWindow.is_selected())
                {
                    if (OptionsWindow.is_option_enabled)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        // Reset to default controls 
                        if (OptionsWindow.index == OptionsWindow.NonControlOptions)
                            Input.default_keys();
                        else
                            SelectOption();
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                }
                else if (OptionsWindow.is_canceled())
                {
                    OnCanceled(new EventArgs());
                    Timer = TIMER_MAX;
                    Closing = true;
                }
            }
        }

        #region Controls
        public void Set()
        {
            switch (OptionsWindow.index)
            {
                // Zoom
                case (int)Config_Options.Zoom:
                    SetZoom();
                    break;
                // Fullscreen
                case (int)Config_Options.Fullscreen:
                    SetFullscreen();
                    break;
                // Stereoscopic
                case (int)Config_Options.Stereoscopic:
                    SetStereoscopic();
                    break;
                // Anaglyph
                case (int)Config_Options.Anaglyph:
                    SetAnaglyph();
                    break;
                // Metrics
                case (int)Config_Options.Metrics:
                    SetMetrics();
                    break;
                // Check for Updates
                case (int)Config_Options.Check_For_Updates:
                    SetUpdates();
                    break;
                // Rumble
                case (int)Config_Options.Rumble:
                    SetRumble();
                    break;
            }
        }

        // Zoom
        private void SetZoom()
        {
            Global.zoom = OptionsWindow.Zoom;
        }

        // Fullscreen
        private void SetFullscreen()
        {
            Global.fullscreen = OptionsWindow.Fullscreen;
        }

        // Stereoscopic 3D
        private void SetStereoscopic()
        {
            Global.stereoscopic_level = OptionsWindow.StereoscopicLevel;
        }

        // Anaglyph
        private void SetAnaglyph()
        {
            Global.anaglyph = OptionsWindow.Anaglyph;
        }

        // Metrics
        private void SetMetrics()
        {
            Global.metrics = OptionsWindow.Metrics ?
                Metrics_Settings.On : Metrics_Settings.Off;
        }

        // Check for Updates
        private void SetUpdates()
        {
            Global.updates_active = OptionsWindow.Updates;
        }

        // Rumble
        private void SetRumble()
        {
            Global.rumble = OptionsWindow.Rumble;
        }

        // Controls
        public void UpdateKeyConfig()
        {
            //KeyboardState state = new KeyboardState(
            //    Keys.OemAuto, Keys.Attn, Keys.Zoom); //@Debug
            if (Global.Input.KeyPressed(Keys.Escape))
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                CancelSelecting();
            }
            else
            {
                var pressed_keys = Global.Input.PressedKeys();
                foreach (Keys key in Input.REMAPPABLE_KEYS.Keys
                        .Intersect(pressed_keys))
                    if (Input.remap_key(OptionsWindow.index -
                        (OptionsWindow.NonControlOptions + 1), key))
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        CancelSelecting();
                        break;
                    }
            }

            if (!Accepting)
                OptionsTimer = 2;
        }
        #endregion

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            BlackScreen.draw(spriteBatch);
            spriteBatch.End();

            if (this.IsReady)
            {
                OptionsWindow.draw(spriteBatch);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                VersionNumber.draw(spriteBatch);
                spriteBatch.End();
            }
        }
    }
}
