using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;

namespace Tactile.Windows.Map
{
    class Window_Prep_Organize : Window_Prep_Unit_Overview
    {
        internal const bool SWAP = false;

        private int SelectedUnit = -1;
        private int SelectionMoveIndex = -1;
        protected Button_Description CancelButton;

        private bool unit_selected { get { return SelectedUnit > -1; } }

        public Window_Prep_Organize()
        {
            initialize_sprites();
            update_black_screen();
        }

        protected override void initialize_sprites()
        {
            // Unit Window
            Unit_Window = new Window_Prep_Organize_Unit();
            Unit_Window.stereoscopic = Config.PREPUNIT_WINDOW_DEPTH;
            Unit_Window.IndexChanged += Unit_Window_IndexChanged;

            base.initialize_sprites();
        }

        protected override void refresh_input_help()
        {
            CancelButton = Button_Description.button(Inputs.B, Backing_1.loc + new Vector2(32, 0));
            CancelButton.description = "Cancel";
            CancelButton.offset = new Vector2(2, -2);
            CancelButton.stereoscopic = Config.PREPUNIT_INPUTHELP_DEPTH;

            Select = Button_Description.button(Inputs.Select,
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen"), new Rectangle(150, 73, 48, 16));
            Select.loc = Backing_1.loc + new Vector2(0, 16 - 1);
            Select.offset = new Vector2(-12, -1);
            Select.stereoscopic = Config.PREPUNIT_INPUTHELP_DEPTH;
        }

        protected override void refresh()
        {
            if (!SWAP)
                if (this.unit_selected)
                    return;

            base.refresh();
        }

        protected override void update_input(bool active)
        {
            bool input = active && this.ready;

            CancelButton.Update(input);
            Select.Update(input);

            if (input)
            {
                // Close this window
                if (Global.Input.triggered(Inputs.B) ||
                    CancelButton.consume_trigger(MouseButtons.Left) ||
                    CancelButton.consume_trigger(TouchGestures.Tap))
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    if (this.unit_selected)
                    {
                        cancel_unit_selected();
                    }
                    else
                    {
                        close();
                    }
                    return;
                }
                else if (Global.Input.triggered(Inputs.Select) ||
                    Select.consume_trigger(MouseButtons.Left) ||
                    Select.consume_trigger(TouchGestures.Tap))
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    OnUnit(new EventArgs());
                    return;
                }

                // Select unit
                var selected_index = Unit_Window.consume_triggered(
                    Inputs.A, MouseButtons.Left, TouchGestures.Tap);
                if (selected_index.IsSomething)
                {
                    select_unit(selected_index); //Debug
                    return;
                }

                // Status screen
                var status_index = Unit_Window.consume_triggered(
                    Inputs.R, MouseButtons.Right, TouchGestures.LongPress);
                if (status_index.IsSomething)
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    OnStatus(new EventArgs());
                }
            }
        }

        protected override void Unit_Window_IndexChanged(object sender, System.EventArgs e)
        {
            if (!SWAP)
            {
                if (this.unit_selected)
                {
                    if (SelectionMoveIndex != Unit_Window.index)
                    {
                        Global.battalion.move_actor(SelectionMoveIndex, Unit_Window.index);
                        SelectionMoveIndex = Unit_Window.index;
                        Unit_Window.refresh_nodes();
                    }
                }
            }

            base.Unit_Window_IndexChanged(sender, e);
        }

        private void select_unit(int index)
        {
            if (this.unit_selected)
            {
                if (index == SelectedUnit)
                    cancel_unit_selected();
                else
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    if (SWAP)
                        Global.battalion.switch_actors(SelectedUnit, index);
                    else
                        Global.battalion.move_actor(SelectionMoveIndex, index);
                    Unit_Window.refresh_nodes();
                    (Unit_Window as Window_Prep_Organize_Unit).unit_selected(false);
                    SelectedUnit = -1;
                    SelectionMoveIndex = -1;
                    Unit_Window.actor_id = Global.battalion.actors[index];

                    refresh();
                }
            }
            else
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                SelectedUnit = index;
                SelectionMoveIndex = index;
                (Unit_Window as Window_Prep_Organize_Unit).unit_selected(true);
            }
        }

        private void cancel_unit_selected()
        {
            Global.game_system.play_se(System_Sounds.Cancel);
            int selected = SelectedUnit;
            (Unit_Window as Window_Prep_Organize_Unit).unit_selected(false);
            if (!SWAP)
            {
                Global.battalion.move_actor(SelectionMoveIndex, selected);
                Unit_Window.refresh_nodes();
            }
            SelectedUnit = -1;
            SelectionMoveIndex = -1;
            Unit_Window.actor_id = Global.battalion.actors[selected];
        }

        protected override void draw_info(SpriteBatch sprite_batch)
        {
            base.draw_info(sprite_batch);
            CancelButton.Draw(sprite_batch);
        }
    }
}
