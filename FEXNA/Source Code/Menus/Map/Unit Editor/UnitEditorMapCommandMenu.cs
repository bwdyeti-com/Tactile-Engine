#if !MONOGAME && DEBUG
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
    class UnitEditorMapCommandMenu : CommandMenu
    {
        public UnitEditorMapCommandMenu() : base(new_map_window(104))
        {
            if (Global.game_map.get_unit(Global.player.loc) != null)
            {
                Window.set_text_color(1, "Grey");
                Window.set_text_color(2, "Grey");
            }

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
                "Unit", "Add Unit", "Paste Unit", "Reinforcements", "Options",
                "Clear Units", "Mirror Units", "Playtest", "Revert", "Save", "Quit" };
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
        
        private int new_team(int old_team, bool left)
        {
            if (left)
                old_team--;
            else
                old_team++;

            old_team--;
            old_team = ((old_team + Constants.Team.NUM_TEAMS) % Constants.Team.NUM_TEAMS);
            old_team++;

            return old_team;
        }

        public Unit_Editor_Options SelectedOption
        {
            get
            {
                if (Window.is_selected())
                    return (Unit_Editor_Options)Window.selected_index().ValueOrDefault;
                return Unit_Editor_Options.None;
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
                CancelButton.Update(active);
            bool cancel = CanceledTriggered(active);

            if (cancel)
            {
                Cancel();
            }
            else if (Window.is_selected())
            {
                switch ((Unit_Editor_Options)Window.selected_index().ValueOrDefault)
                {
                    case Unit_Editor_Options.Unit:

                        if (Global.game_map.teams[Window_Unit_Team.TEAM].Count > 0)
                        {
                            SelectItem(true);
                        }
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                    case Unit_Editor_Options.Add_Unit: // Add Unit
                    case Unit_Editor_Options.Paste_Unit: // Paste Unit
                        if (Global.game_map.get_unit(Global.player.loc) != null)
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        else
                        {
                            SelectItem(true);
                        }
                        break;
                    case Unit_Editor_Options.Reinforcements: // Reinforcements
                    case Unit_Editor_Options.Options: // Options
                    case Unit_Editor_Options.Clear_Units: // Clear Units
                    case Unit_Editor_Options.Mirror_Units: // Mirror Units
                        SelectItem(true);
                        break;
                    case Unit_Editor_Options.Playtest: // Playtest map
                        SelectItem();
                        break;
                    case Unit_Editor_Options.Revert: // Revert to last save
                    case Unit_Editor_Options.Save: // Save
                    case Unit_Editor_Options.Quit: // Quit
                        SelectItem(true);
                        break;
                }
            }
            else
            {
                switch (Window.index)
                {
                    // Change Team
                    case (int)Unit_Editor_Options.Unit:
                        if (Global.Input.repeated(Inputs.Left) ||
                            Global.Input.repeated(Inputs.Right))
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                            int team = Window_Unit_Team.TEAM;
                            team = new_team(team, Global.Input.repeated(Inputs.Left));

                            Window_Unit_Team.TEAM = team;
                            Window.color_override = Window_Unit_Team.TEAM - 1;
                        }
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
