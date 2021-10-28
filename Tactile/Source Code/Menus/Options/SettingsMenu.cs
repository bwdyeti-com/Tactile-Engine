using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Tactile.Options;
using Tactile.Windows.Options;

namespace Tactile.Menus.Options
{
    class SettingsMenu : CommandMenu
    {
        private bool MenuSettingSelected = false;
        private byte IgnoreInput = 0;

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

        public event EventHandler<EventArgs> OpenSubMenu;
        private void OnOpenSubMenu(EventArgs e)
        {
            if (OpenSubMenu != null)
                OpenSubMenu(this, e);
        }

        protected override void Activate()
        {
            base.Activate();
            Window.visible = true;
        }

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = base.CanceledTriggered(active);
            if (active)
            {
                cancel |= Global.Input.KeyPressed(Keys.Escape);
            }
            return cancel;
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
                CancelButton.Update(active);
            bool cancel = CanceledTriggered(active);

            // Ignore inputs after remapping controls/any confirm,
            // to avoid double inputs
            if (IgnoreInput > 0)
            {
                IgnoreInput--;
                return;
            }

            // Setting selected
            if (MenuSettingSelected)
            {
                // Input remapping
                if (settingsWindow.KeyboardRemapSetting)
                {
                    UpdateKeyboardRemap(settingsWindow, cancel);
                }
                else if (settingsWindow.GamepadRemapSetting)
                {
                    UpdateGamepadRemap(settingsWindow, cancel);
                }
                else
                {
                    if (cancel ||
                        Global.Input.triggered(Inputs.B))
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
                        IgnoreInputs();
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
                else if (Window.is_selected() || Global.Input.KeyPressed(Keys.Enter))
                {
                    SelectItem(true);
                }
            }

            if (active && settingsWindow.OpenSubMenu)
            {
                OnOpenSubMenu(new EventArgs());
                Window.visible = false;
                settingsWindow.ClearSubMenu();
            }
        }

        private void UpdateKeyboardRemap(SettingsWindow settingsWindow, bool cancel)
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
                            IgnoreInputs();

                            success = true;
                            break;
                        }
                    }
                    if (!success)
                        Global.game_system.play_se(System_Sounds.Buzzer);
                }
            }
        }
        private void UpdateGamepadRemap(SettingsWindow settingsWindow, bool cancel)
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
                var pressed_keys = Global.Input.PressedButtons();
                if (pressed_keys.Any())
                {
                    bool success = false;
                    foreach (Buttons button in Input.REMAPPABLE_BUTTONS
                            .Intersect(pressed_keys))
                    {
                        if (settingsWindow.RemapInput(button))
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            MenuSettingSelected = false;
                            settingsWindow.ConfirmSetting();
                            IgnoreInputs();

                            success = true;
                            break;
                        }
                    }
                    if (!success)
                        Global.game_system.play_se(System_Sounds.Buzzer);
                }
            }
        }

        public ISettings GetSubSettings()
        {
            var settingsWindow = Window as SettingsWindow;
            return settingsWindow.GetSubSettings();
        }

        private void IgnoreInputs()
        {
            IgnoreInput = 1;
        }

        protected override void SelectItem(bool playConfirmSound = false)
        {
            var settingsWindow = Window as SettingsWindow;

            if (settingsWindow.IsSettingEnabled())
            {
                if (playConfirmSound)
                    Global.game_system.play_se(System_Sounds.Confirm);
                MenuSettingSelected = settingsWindow.SelectSetting(true);

                // If a setting is not selected (presumably because a button
                // or submenu was pressed), ignore inputs temporarily
                if (!MenuSettingSelected)
                    IgnoreInputs();
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }
    }
}
