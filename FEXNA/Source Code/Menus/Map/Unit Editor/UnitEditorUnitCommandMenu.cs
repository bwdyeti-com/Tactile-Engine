#if !MONOGAME && DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Windows.Command;
using FEXNA.Windows.Map;
using FEXNA_Library.EventArg;

namespace FEXNA.Menus.Map
{
    class UnitEditorUnitCommandMenu : CommandMenu
    {
        public UnitEditorUnitCommandMenu() : base(new_map_window(104))
        {
            create_cancel_button();
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
            List<string> commands = new List<string> {
                "Edit Unit", "Move Unit", "Change Team", "Copy Unit", "Remove Unit" };
            var window = new Window_Command_Scrollbar(
                new Vector2(8 + (show_menu_on_right ?
                    (Config.WINDOW_WIDTH - (width + 16)) : 0), 24),
                width, 8, commands);
            window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            window.color_override = Window_Unit_Team.TEAM - 1;
            return window;
        }

        private void create_cancel_button()
        {
            CreateCancelButton(
                show_menu_on_right ? Config.WINDOW_WIDTH - (32 + 48) : 32,
                Config.MAPCOMMAND_WINDOW_DEPTH);
        }

        public event EventHandler<BoolEventArgs> TeamChanged;
        protected void OnTeamChanged(BoolEventArgs e)
        {
            if (TeamChanged != null)
                TeamChanged(this, e);
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

        public UnitEditorUnitOptions SelectedOption
        {
            get
            {
                if (Window.is_selected())
                    return (UnitEditorUnitOptions)Window.selected_index().ValueOrDefault;
                return UnitEditorUnitOptions.None;
            }
        }

        public Vector2 SelectedOptionLocation
        {
            get
            {
                return Window.loc + new Vector2(0,
                    (Window.selected_index().ValueOrDefault -
                        (Window as Window_Command_Scrollbar).scroll) * 16);
            }
        }

        #region IMenu
        protected override void UpdateMenu(bool active)
        {
            Window.update(active);

            if (CancelButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    create_cancel_button();
                CancelButton.Update(active);
            }
            bool cancel = CanceledTriggered(active);

            if (cancel)
            {
                Cancel();
            }
            else if (Window.is_selected())
            {
                switch ((UnitEditorUnitOptions)Window.selected_index().ValueOrDefault)
                {
                    case UnitEditorUnitOptions.EditUnit:
                    case UnitEditorUnitOptions.MoveUnit:
                    case UnitEditorUnitOptions.CopyUnit:
                    case UnitEditorUnitOptions.RemoveUnit:
                        Global.game_system.play_se(System_Sounds.Confirm);
                        OnSelected(new EventArgs());
                        break;
                }
            }
            else
            {
                switch (Window.index)
                {
                    // Change Team
                    case (int)UnitEditorUnitOptions.ChangeTeam:
                        if (Global.Input.repeated(Inputs.Left) ||
                            Global.Input.repeated(Inputs.Right))
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                            OnTeamChanged(new BoolEventArgs(Global.Input.repeated(Inputs.Right)));
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

            base.Draw(spriteBatch);
        }
        #endregion
    }
}
#endif
