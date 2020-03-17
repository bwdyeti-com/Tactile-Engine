#if DEBUG
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Windows.Command;

namespace FEXNA.Menus.Map
{
    class MapDebugMenu : CommandMenu
    {
        public MapDebugMenu() : base(new_map_window(96))
        {
            Game_Unit unit = Global.game_map.get_unit(Global.player.loc);
            if (unit == null)
            {
                Window.set_text_color((int)DebugMenuOptions.RefreshUnit, "Grey");
                Window.set_text_color((int)DebugMenuOptions.DeleteUnit, "Grey");
                Window.set_text_color((int)DebugMenuOptions.Heal, "Grey");
                Window.set_text_color((int)DebugMenuOptions.MaxSupport, "Grey");
                Window.set_text_color((int)DebugMenuOptions.LevelUp, "Grey");
            }
            else if (unit.actor.exp_gain_possible() <= 0 && !unit.actor.can_promote())
            {
                Window.set_text_color((int)DebugMenuOptions.LevelUp, "Grey");
            }

            create_cancel_button();
        }

        private static bool show_menu_on_right
        {
            get
            {
                return Global.player.is_on_left();
            }
        }

        private static Window_Command new_map_window(int width)
        {
            List<string> commands = new List<string> {
            "Refresh", "Delete Unit", "Skip Chapter",
            "Heal", "Support++", "Level Up",
            Global.game_map.fow ? "Toggle Fog Off" : "Toggle Fog On",
            "Toggle Inf Mov",
            Game_AI.AI_ENABLED ? "Toggle AI Off" : "Toggle AI On" };

            var window = new Window_Command_Scrollbar(
                new Vector2(8 + (Global.player.is_on_left() ?
                    (Config.WINDOW_WIDTH - (width + 16)) : 0), 24),
                width, 8, commands);
            window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;

            return window;
        }

        private void create_cancel_button()
        {
            CreateCancelButton(
                show_menu_on_right ? Config.WINDOW_WIDTH - (32 + 48) : 32,
                Config.MAPCOMMAND_WINDOW_DEPTH);
        }

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = base.CanceledTriggered(active);
            if (active)
            {
                // If right clicked or tapped on nothing in particular
                cancel |= Global.Input.mouse_click(MouseButtons.Right) ||
                    Global.Input.gesture_triggered(TouchGestures.Tap);
            }
            return cancel;
        }

        public DebugMenuOptions SelectedOption
        {
            get
            {
                if (Window.is_selected())
                    return (DebugMenuOptions)Window.selected_index().ValueOrDefault;
                return DebugMenuOptions.None;
            }
        }

        #region IMenu
        protected override void UpdateMenu(bool active)
        {
            Window.update(active);

            if (CancelButton != null)
                CancelButton.Update(active);
            bool cancel = CanceledTriggered(active);

            if (cancel)
            {
                Cancel();
            }
            else if (Window.is_selected())
            {
                switch (SelectedOption)
                {
                    case DebugMenuOptions.SkipChapter:
                        Global.Audio.play_se("System Sounds", "Confirm");
                        SelectItem();
                        break;
                    case DebugMenuOptions.ToggleFog:
                    case DebugMenuOptions.InfiniteMove:
                    case DebugMenuOptions.ToggleAI:
                        SelectItem(true);
                        break;
                    default:
                        SelectItem();
                        break;
                }
            }
        }

        protected override void UpdateAncillary()
        {
            if (CancelButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    create_cancel_button();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            CancelButton.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(spriteBatch);
        }
        #endregion
    }
}
#endif
