using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FEXNA.Options;
using FEXNA.Windows.Options;

namespace FEXNA.Menus.Options
{
    class SettingsMenu : CommandMenu
    {
        private bool MenuSettingSelected = false;

        public SettingsMenu(ISettings settings, IHasCancelButton menu = null)
            : base()
        {
            OpenWindow(settings);
            CreateCancelButton(menu);
        }

        private void OpenWindow(ISettings settings)
        {
            Vector2 loc = new Vector2(48, 16);
            int width = 232;
            
            Window = new SettingsWindow(loc, width, settings);
            Window.stereoscopic = Config.TITLE_CHOICE_DEPTH;
        }

        protected override bool CanceledTriggered(bool active)
        {
            return base.CanceledTriggered(active);
        }
        
        protected override void UpdateMenu(bool active)
        {
            var settingsWindow = Window as SettingsWindow;

            if (this.HideCursorWhileInactive)
                Window.active = active;

            int index = Window.index;
            Window.update(active && !MenuSettingSelected);
            if (index != Window.index)
                OnIndexChanged(new EventArgs());

            if (CancelButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    CreateCancelButton(this);
                CancelButton.Update(active);
            }
            bool cancel = CanceledTriggered(active);

            // Setting selected
            if (MenuSettingSelected)
            {
                // Input remapping
                if (settingsWindow.InputRemapSetting)
                {
                    // Cancel remapping with escape
                    if (cancel ||
                        Global.Input.KeyPressed(Keys.Escape))
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        MenuSettingSelected = false;
                        settingsWindow.CancelSetting();
                    }
                    else
                    {
                        var pressed_keys = Global.Input.PressedKeys();
                        if (pressed_keys.Any())
                        {
                            bool success = false;
                            foreach (Keys key in Input.REMAPPABLE_KEYS.Keys
                                    .Intersect(pressed_keys))
                            {
                                if (settingsWindow.RemapInput(key))
                                {
                                    Global.game_system.play_se(System_Sounds.Confirm);
                                    MenuSettingSelected = false;
                                    settingsWindow.ConfirmSetting();

                                    success = true;
                                    break;
                                }
                            }
                            if (!success)
                                Global.game_system.play_se(System_Sounds.Buzzer);
                        }
                    }
                }
                else
                {
                    if (cancel ||
                        Global.Input.triggered(Inputs.B) ||
                        Global.Input.KeyPressed(Keys.Escape))
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        MenuSettingSelected = false;
                        settingsWindow.CancelSetting();
                    }
                    else if (Global.Input.KeyPressed(Keys.Enter) ||
                        Global.Input.triggered(Inputs.A) ||
                        Global.Input.triggered(Inputs.Start))
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        MenuSettingSelected = false;
                        settingsWindow.ConfirmSetting();
                    }
                }
            }
            // Navigating settings
            else
            {
                if (cancel)
                {
                    Cancel();
                }
                else if (Window.is_selected())
                {
                    SelectItem(true);
                }
            }
        }

        protected override void SelectItem(bool playConfirmSound = false)
        {
            var settingsWindow = Window as SettingsWindow;

            if (settingsWindow.IsSettingEnabled())
            {
                if (playConfirmSound)
                    Global.game_system.play_se(System_Sounds.Confirm);
                MenuSettingSelected = settingsWindow.SelectSetting(true);
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }
    }
}
