using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Graphics.Text;

namespace Tactile.Windows.Map
{
    enum PrepPickUnitsInputResults { None, Status, UnitWindow, Closing }

    class Window_Prep_PickUnits : Window_Prep_Unit_Overview
    {
        const int START_BLACK_SCEEN_FADE_TIMER = 16;
        const int START_BLACK_SCREEN_HOLD_TIMER = 40;

        protected bool Pressed_Start = false;
        protected int Units_Deployed = 0;
        protected bool[] Unit_Deployed_Flags;
        protected List<int> Units_To_Deploy = new List<int>(), Units_To_UnDeploy = new List<int>();

        protected TextSprite Goal;
        protected Button_Description R_Button, Start;
        protected Sprite Backing_2;

        public bool pressed_start { get { return Pressed_Start; } }

        protected bool unit_count_maxed { get { return Units_Deployed >= Global.game_map.deployment_points.Count; } }

        public override bool HidesParent { get { return Pressed_Start; } }

        public Window_Prep_PickUnits()
        {
            int other_units = 0;
            foreach (int unit_id in Global.game_map.allies)
                if (Global.game_map.deployment_points.Contains(Global.game_map.units[unit_id].loc))
                    Units_Deployed++;
                else
                    other_units++;
            // Units
            Unit_Deployed_Flags = new bool[Global.battalion.actors.Count];
            for (int i = 0; i < Global.battalion.actors.Count; i++)
                Unit_Deployed_Flags[i] = Global.battalion.is_actor_deployed(i);

            initialize_sprites(other_units);
            update_black_screen();
        }

        protected void initialize_sprites(int otherUnits)
        {
            // Unit Window
            Unit_Window = new Window_Prep_PickUnits_Unit(Global.game_map.deployment_points.Count, Units_Deployed, otherUnits);
            Unit_Window.stereoscopic = Config.PREPUNIT_WINDOW_DEPTH;
            Unit_Window.IndexChanged += Unit_Window_IndexChanged;

            Backing_2 = new Sprite();
            Backing_2.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
            Backing_2.loc = new Vector2(176, 160);
            Backing_2.src_rect = new Rectangle(0, 144, 136, 32);
            Backing_2.tint = new Color(224, 224, 224, 128);
            Backing_2.stereoscopic = Config.PREPUNIT_INPUTHELP_DEPTH;

            base.initialize_sprites();

            // Goal
            Goal = new TextSprite();
            Goal.loc = Backing_1.loc + new Vector2(48, 0);
            Goal.SetFont(Config.UI_FONT, Global.Content, "White");
            Goal.stereoscopic = Config.PREPUNIT_INPUTHELP_DEPTH;
            Goal.text = Global.game_system.Objective_Text;
            Goal.offset = new Vector2(Font_Data.text_width(Goal.text) / 2, 0);
        }

        protected override void refresh_input_help()
        {
            R_Button = Button_Description.button(Inputs.R,
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen"), new Rectangle(126, 122, 24, 16));
            R_Button.loc = Backing_1.loc + new Vector2(32, 16 - 4);
            R_Button.offset = new Vector2(2, -2);
            R_Button.stereoscopic = Config.PREPUNIT_INPUTHELP_DEPTH;

            Start = Button_Description.button(Inputs.Start,
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen"), new Rectangle(142, 41, 32, 16));
            Start.loc = Backing_2.loc + new Vector2(32, 0 - 1);
            Start.offset = new Vector2(0, -1);
            Start.stereoscopic = Config.PREPUNIT_INPUTHELP_DEPTH;

            Select = Button_Description.button(Inputs.Select,
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen"), new Rectangle(150, 73, 48, 16));
            Select.loc = Backing_2.loc + new Vector2(24, 16 - 1);
            Select.offset = new Vector2(-12, -1);
            Select.stereoscopic = Config.PREPUNIT_INPUTHELP_DEPTH;
        }

        #region Update
        protected override void update_input(bool active)
        {
            bool input = active && this.ready;

            if (input)
            {
                // Close this window
                if (Global.Input.triggered(Inputs.B))
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    close();
                    return;
                }
                else if (Global.Input.triggered(Inputs.Start))
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    close(true);
                    return;
                }
                else if (Global.Input.triggered(Inputs.Select))
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
                    switch_unit(selected_index.Index);
                    return;
                }

                // Status screen
                var status_index = Unit_Window.consume_triggered(
                    Inputs.R, MouseButtons.Right, TouchGestures.LongPress);
                if (status_index.IsSomething)
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    OnStatus(new EventArgs());
                    return;
                }
            }
        }

        public bool switch_unit()
        {
            return switch_unit(Unit_Window.index);
        }
        public bool switch_unit(Game_Actor actor)
        {
            Unit_Window.actor_id = actor.id;
            return switch_unit();
        }
        public bool switch_unit(int index)
        {
            bool result = false;
            var unit_window = (Unit_Window as Window_Prep_PickUnits_Unit);

            if (Unit_Deployed_Flags[index])
            {
                // If forced, buzz
                if (Global.game_map.forced_deployment.Contains(actor_id))
                {
                    Global.game_system.play_se(System_Sounds.Buzzer);
                }
                // If pre-deployed, buzz
                else if (!Global.game_map.deployment_points.Contains(Global.game_map.units[
                    Global.game_map.get_unit_id_from_actor(actor_id)].loc))
                {
                    Global.game_system.play_se(System_Sounds.Buzzer);
                }
                else
                {
                    // If Unit was deployed and has been removed, add them back to the team
                    if (Units_To_UnDeploy.Contains(index))
                    {
                        // If too many units, buzz
                        if (unit_count_maxed)
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            Units_Deployed++;
                            Units_To_UnDeploy.Remove(index);
                            unit_window.refresh_unit(true);
                            result = true;
                        }
                    }
                    // Else undeploy unit
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        Units_Deployed--;
                        Units_To_UnDeploy.Add(index);
                        unit_window.refresh_unit(false);
                        result = true;
                    }
                }
            }
            else
            {
                // If Unit wasn't deployed and and wants to be, add them to the team
                if (!Units_To_Deploy.Contains(index))
                {
                    // If too many units, buzz
                    if (unit_count_maxed)
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Units_Deployed++;
                        Units_To_Deploy.Add(index);
                        unit_window.refresh_unit(true);
                        result = true;
                    }
                }
                // Else undeploy unit
                else
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    Units_Deployed--;
                    Units_To_Deploy.Remove(index);
                    unit_window.refresh_unit(false);
                    result = true;
                }
            }
            unit_window.unit_count = Units_Deployed;
            return result;
        }

        public bool actor_deployed(int actor_id)
        {
            bool result = false;
            int temp_actor_id = Unit_Window.actor_id;
            Unit_Window.actor_id = actor_id;
            if (!Units_To_UnDeploy.Contains(Unit_Window.index))
                if (Units_To_Deploy.Contains(Unit_Window.index) || Unit_Deployed_Flags[Unit_Window.index])
                    result = true;
            Unit_Window.actor_id = temp_actor_id;
            return result;
        }

        public List<int>[] unit_changes()
        {
            return new List<int>[] { Units_To_UnDeploy, Units_To_Deploy };
        }

        protected override void close()
        {
            close(false);
        }
        protected void close(bool start)
        {
            OnClosing(new EventArgs());

            Pressed_Start = start;
            if (Pressed_Start)
            {
                Black_Screen_Fade_Timer = START_BLACK_SCEEN_FADE_TIMER;
                Black_Screen_Hold_Timer = START_BLACK_SCREEN_HOLD_TIMER;
            }
            _Closing = true;
            Black_Screen_Timer = Black_Screen_Hold_Timer + (Black_Screen_Fade_Timer * 2);
            if (Black_Screen != null)
                Black_Screen.visible = true;
            Global.game_system.Preparations_Actor_Id = actor_id;
        }
        #endregion

        protected override void draw_info(SpriteBatch sprite_batch)
        {
            Backing_2.draw(sprite_batch);
            base.draw_info(sprite_batch);

            Goal.draw(sprite_batch);
            R_Button.Draw(sprite_batch);
            Start.Draw(sprite_batch);
        }
    }
}
