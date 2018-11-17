using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Windows.Command;
using FEXNA.Windows.Map;

namespace FEXNA.Menus.Map
{
    class MapCommandMenu : CommandMenu
    {
        private WindowReadyUnits ReadyUnitsWindow;

        public MapCommandMenu() : base(new_map_window(56))
        {
            if (this.data_option_blocked)
                Window.set_text_color(1, "Grey");
            if (this.end_option_blocked)
                Window.set_text_color(4, "Grey");

            create_cancel_button();

            ReadyUnitsWindow = new WindowReadyUnits();
            ReadyUnitsWindow.loc = new Vector2(
                24 + (show_menu_on_right ? 0 : (Config.WINDOW_WIDTH - (ReadyUnitsWindow.Width + 32))),
                Config.WINDOW_HEIGHT - 32);
            ReadyUnitsWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
        }

        private static bool show_menu_on_right
        {
            get
            {
                // Unlike the unit menu that should always appear on the opposite side
                // from the cursor, so it doesn't cover up the active unit,
                // this menu appears on the same side as the cursor when
                // using mouse/touch controls
                return Global.player.is_on_left() ^
                    Input.ControlScheme != ControlSchemes.Buttons;
            }
        }

        private static Window_Command new_map_window(int width)
        {
            List<string> commands =
                new List<string> { "Unit", "Data", "Options", "Suspend", "End" };
            var window = new Window_Command(
                new Vector2(8 + (show_menu_on_right ?
                    (Config.WINDOW_WIDTH - (width + 16)) : 0), 24),
                width, commands);
            window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            if (Global.game_temp.end_turn_highlit)
            {
                window.immediate_index = (int)Map_Menu_Options.End;
                Global.game_temp.end_turn_highlit = false;
            }
            return window;
        }

        private void create_cancel_button()
        {
            CreateCancelButton(
                show_menu_on_right ? Config.WINDOW_WIDTH - (32 + 48) : 32,
                Config.MAPCOMMAND_WINDOW_DEPTH);
        }

        protected override bool CanceledTriggered
        {
            get
            {
                bool cancel = base.CanceledTriggered;
                // If right clicked or tapped on nothing in particular
                cancel |= Global.Input.mouse_click(MouseButtons.Right) ||
                    Global.Input.gesture_triggered(TouchGestures.Tap);
                return cancel;
            }
        }

        private bool data_option_blocked
        {
            get
            {
                //Yeti
                if (Scene_Map.intro_chapter_options_blocked())
                    return true;
                return false;
            }
        }

        private bool end_option_blocked
        {
            get
            {
                //Yeti
                if (Scene_Map.intro_chapter_options_blocked())
                    if (Global.game_map.teams[Constants.Team.ENEMY_TEAM].Count > 0)
                        // If any PCs ready
                        return Global.game_map.active_team
                            .Select(x => Global.game_map.units[x])
                            .Count(x => x.ready) > 0;
                return false;
            }
        }

        public Map_Menu_Options SelectedOption
        {
            get
            {
                if (Window.is_selected())
                    return (Map_Menu_Options)Window.selected_index().ValueOrDefault;
                return Map_Menu_Options.None;
            }
        }

        #region IMenu
        protected override void UpdateMenu(bool active)
        {
            Window.update(active);
            if (ReadyUnitsWindow != null)
                ReadyUnitsWindow.update();

            if (CancelButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    create_cancel_button();
                CancelButton.Update(active);
            }
            bool cancel = this.CanceledTriggered;

            if (cancel)
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                OnCanceled(new EventArgs());
                return;
            }
            else if (Window.is_selected())
            {
                switch ((Map_Menu_Options)Window.selected_index().ValueOrDefault)
                {
                    case Map_Menu_Options.Unit:
                        Global.game_system.play_se(System_Sounds.Confirm);
                        OnSelected(new EventArgs());
                        break;
                    case Map_Menu_Options.Data:
                        if (this.data_option_blocked)
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            OnSelected(new EventArgs());
                        }
                        break;
                    case Map_Menu_Options.Options:
                        Global.game_system.play_se(System_Sounds.Confirm);
                        OnSelected(new EventArgs());
                        break;
                    case Map_Menu_Options.Suspend:
                        Global.Audio.play_se("System Sounds", "Confirm");
                        //Global.game_system.play_se(System_Sounds.Confirm); //Yeti

                        OnSelected(new EventArgs());
                        break;
                    case Map_Menu_Options.End:
                        if (this.end_option_blocked)
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            OnSelected(new EventArgs());
                        }
                        break;
                }
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            CancelButton.Draw(spriteBatch);
            spriteBatch.End();

            if (ReadyUnitsWindow != null)
                ReadyUnitsWindow.Draw(spriteBatch);
            base.Draw(spriteBatch);
        }
        #endregion
    }
}
