﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Graphics.Preparations;
using Tactile.Graphics.Text;
using Tactile.Menus.Preparations;
using Tactile.Windows.Command;
using TactileLibrary;

namespace Tactile
{
    class Window_Setup : Windows.Map.Map_Window_Base
    {
        const int BLACK_SCEEN_FADE_TIMER = 16;
        const int BLACK_SCREEN_HOLD_TIMER = 40;
        const int BLACK_SCREEN_SHORT_HOLD_TIME = 12;

        private int PressStartFlicker = 0;
        private bool PressedStart = false;
        protected Window_Command CommandWindow;
        protected Sprite Banner;
        protected TextSprite Goal;
        private ChapterLabel ChapterLabel;
        protected Sprite InfoWindow;
        private Sprite CommandHelpWindow;
        protected TextSprite HelpText;
        private Sprite InfoLabel;
        protected RightAdjustedText AvgLvl, EnemyAvgLvl, Funds;
        //private Sprite AvgRating, EnemyAvgRating; //Debug
        protected Play_Time_Counter Counter;
        protected Sprite Map_Darken;

        protected Button_Description StartButton;

        #region Accessors
        public bool pressed_start { get { return PressedStart; } }

        public int index
        {
            get { return CommandWindow.index; }
            set { CommandWindow.index = value; }
        }

        internal virtual ConsumedInput selected_index { get { return CommandWindow.selected_index(); } }

        internal bool start_ui_button_pressed
        {
            get
            {
                return StartButton.consume_trigger(MouseButtons.Left) ||
                    StartButton.consume_trigger(TouchGestures.Tap);
            }
        }

        public bool ready { get { return this.ready_for_inputs; } }

        internal bool input_ready
        {
            get
            {
                return this.ready &&
                    !Global.game_system.is_interpreter_running &&
                    !Global.scene.is_message_window_active;
            }
        }

        protected virtual Vector2 command_window_loc { get { return new Vector2(8, 32); } }
        #endregion

        public Window_Setup(bool deploy_units)
        {
            initialize_sprites();
            if (!deploy_units)
                set_black_screen_time(true);
            update_black_screen();
            if (deploy_units)
                add_deploying_units();
            refresh();
        }

        protected override void set_black_screen_time()
        {
            set_black_screen_time(false);
        }
        private void set_black_screen_time(bool short_fade)
        {
            Black_Screen_Fade_Timer = BLACK_SCEEN_FADE_TIMER;
            Black_Screen_Hold_Timer = short_fade ? BLACK_SCREEN_SHORT_HOLD_TIME : BLACK_SCREEN_HOLD_TIMER;
            base.set_black_screen_time();
        }

        public void skip_black_screen()
        {
            Black_Screen_Timer = 0;
            UpdateMenu(false);
        }

        protected virtual void initialize_sprites()
        {
            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 255);
            // Map Darken
            Map_Darken = new Sprite();
            Map_Darken.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Map_Darken.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Map_Darken.tint = new Color(0, 0, 0, 128);
            // Banner
            Banner = new Sprite();
            Banner.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
            Banner.loc = new Vector2(0, 0);
            Banner.src_rect = new Rectangle(0, 0, 320, 40);
            Banner.tint = new Color(255, 255, 255, 128);
            Banner.stereoscopic = Config.PREPMAIN_BANNER_DEPTH;
            Goal = new TextSprite();
            Goal.loc = new Vector2(Config.WINDOW_WIDTH - 144/2, 20);
            Goal.SetFont(Config.UI_FONT, Global.Content, "White");
            Goal.stereoscopic = Config.PREPMAIN_BANNER_DEPTH;
            ChapterLabel = new ChapterLabel();
            ChapterLabel.loc = new Vector2(Config.WINDOW_WIDTH - 144, 4);
            ChapterLabel.stereoscopic = Config.PREPMAIN_BANNER_DEPTH;
            ChapterLabel.SetChapter(Global.data_chapters[Global.game_state.chapter_id]);
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Preparation_Background");
            (Background as Menu_Background).vel = new Vector2(0, -1 / 3f);
            (Background as Menu_Background).tile = new Vector2(1, 2);
            Background.stereoscopic = Config.PREP_BG_DEPTH;
            // Command Window
            CommandWindow = new Window_Command(command_window_loc, 80, command_window_string());
            CommandWindow.text_offset = new Vector2(8, 0);
            CommandWindow.glow = true;
            CommandWindow.bar_offset = new Vector2(-8, 0);
            CommandWindow.stereoscopic = Config.PREPMAIN_WINDOW_DEPTH;
            // Info Window
            InfoWindow = new Sprite();
            InfoWindow.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
            InfoWindow.loc = new Vector2(8, 128);
            InfoWindow.src_rect = new Rectangle(208, 40, 112, 64);
            InfoWindow.stereoscopic = Config.PREPMAIN_DATA_DEPTH;
            // Help Window
            CommandHelpWindow = new Sprite();
            CommandHelpWindow.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
            CommandHelpWindow.loc = new Vector2(160, 40);
            CommandHelpWindow.src_rect = new Rectangle(184, 104, 136, 112);
            CommandHelpWindow.stereoscopic = Config.PREPMAIN_INFO_DEPTH;
            // Help Text
            HelpText = new TextSprite();
            HelpText.loc = new Vector2(176, 48);
            HelpText.SetFont(Config.UI_FONT, Global.Content, "White");
            HelpText.text = "";
            HelpText.stereoscopic = Config.PREPMAIN_INFO_DEPTH;
            // Info Label
            InfoLabel = new Sprite();
            InfoLabel.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Preparation_Info_Text");
            InfoLabel.loc = new Vector2(16, 136);
            InfoLabel.offset = new Vector2(-5, -2);
            InfoLabel.src_rect = new Rectangle(0, 0, InfoLabel.texture.Width, 28);
            InfoLabel.stereoscopic = Config.PREPMAIN_DATA_DEPTH;
            // Info
            AvgLvl = new RightAdjustedText();
            AvgLvl.loc = new Vector2(108 - 32, 136);
            AvgLvl.SetFont(Config.UI_FONT, Global.Content, "Blue");
            AvgLvl.stereoscopic = Config.PREPMAIN_DATA_DEPTH;
            EnemyAvgLvl = new RightAdjustedText();
            EnemyAvgLvl.loc = new Vector2(108, 136);
            EnemyAvgLvl.SetFont(Config.UI_FONT, Global.Content, "Red");
            EnemyAvgLvl.stereoscopic = Config.PREPMAIN_DATA_DEPTH;
            Funds = new RightAdjustedText();
            Funds.loc = new Vector2(100, 152);
            Funds.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Funds.stereoscopic = Config.PREPMAIN_DATA_DEPTH;
            Counter = new Play_Time_Counter();
            Counter.loc = new Vector2(20, 168);
            Counter.stereoscopic = Config.PREPMAIN_DATA_DEPTH;

            create_start_button();
        }

        protected virtual void create_start_button()
        {
            StartButton = Button_Description.button(Inputs.Start,
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen"),
                new Rectangle(142, 41, 32, 16));
            StartButton.loc = new Vector2(
                Config.WINDOW_WIDTH - 80, Config.WINDOW_HEIGHT - 16);
            StartButton.stereoscopic = Config.PREPMAIN_INFO_DEPTH;
        }

        protected virtual List<string> command_window_string()
        {
            return new List<string> { "Pick Units", "Items", "Fortune", "Check Map", "Save" };
        }

        public virtual void refresh()
        {
            if (Global.battalion.actors.Count > 0)
                CommandWindow.set_text_color(1, "White");
            else
                CommandWindow.set_text_color(1, "Grey");

            // Does augury exist?
            if (Global.game_state.augury_event_exists())
                CommandWindow.set_text_color(2, "White");
            else
                CommandWindow.set_text_color(2, "Grey");

            // Objective text
            Goal.text = Global.game_system.Objective_Text;
            Goal.offset = new Vector2(Font_Data.text_width(Goal.text) / 2, 0);
            
            // Info window
            // Rating
            float rating_factor;
            float player_rating = Global.battalion.deployed_average_rating;
            float enemy_rating = Global.battalion.enemy_rating;

            EnemyAvgLvl.SetColor(Global.Content, "Yellow");

            rating_factor = (float)Math.Log(Global.battalion.enemy_threat, 2);
            int r = (int)(255 * MathHelper.Clamp(1 + rating_factor, 0, 1));
            int g = (int)(255 * MathHelper.Clamp(1 - rating_factor, 0, 1));
            EnemyAvgLvl.tint = new Color(r, g, 255);

            rating_factor = Global.battalion.enemy_threat;
            EnemyAvgLvl.text = string.Format("{0}x", rating_factor.ToString("0.00"));

            // Funds
            Funds.text = Global.battalion.gold.ToString();
        }

        protected virtual void refresh_text()
        {
            switch ((PreparationsChoices)CommandWindow.index)
            {
                // Pick Units
                case PreparationsChoices.Pick_Units:
                    HelpText.text = Global.system_text["Prep Pick Units"];
                    break;
                // Items
                case PreparationsChoices.Trade:
                    HelpText.text = Global.system_text["Prep Items"];
                    break;
                // Fortune
                case PreparationsChoices.Fortune:
                    if (Global.game_state.augury_event_exists())
                        HelpText.text = Global.system_text["Prep Fortune"];
                    else
                        HelpText.text = Global.system_text["Prep Disabled"];
                    break;
                // Check Map
                case PreparationsChoices.Check_Map:
                    HelpText.text = Global.system_text["Prep Check Map"];
                    break;
                // Save
                case PreparationsChoices.Save:
                    HelpText.text = Global.system_text["Prep Save"];
                    break;
            }
        }

        protected void add_deploying_units()
        {
            int deployPointIndex = 0;
            bool any_units_added = false;
            // Add forced units
            int forcedCount = 0;
            while (deployPointIndex < Global.game_map.deployment_points.Count && forcedCount < Global.game_map.forced_deployment.Count)
            {
                // If deployment point already used
                if (Global.game_map.get_unit(Global.game_map.deployment_points[deployPointIndex]) != null)
                    deployPointIndex++;
                // Else if actor already deployed
                else if (Global.game_map.is_actor_deployed(Global.game_map.forced_deployment[forcedCount]))
                    forcedCount++;
                // Else deploy unit
                else
                {
                    bool unit_added = Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Global.game_map.deployment_points[deployPointIndex],
                        Global.game_map.forced_deployment[forcedCount], "");
                    if (unit_added)
                    {
                        Global.game_map.last_added_unit.fix_unit_location(true);
                        Global.game_map.last_added_unit.queue_move_range_update();
                        any_units_added = true;
                    }
                    deployPointIndex++;
                    forcedCount++;
                }
            }
            // Sorts battalion because reasons
            Global.battalion.sort_by_deployed();

            // Add other units
            int deployedCount = 0;
            while (deployPointIndex < Global.game_map.deployment_points.Count && deployedCount < Global.battalion.actors.Count)
            {
                // If deployment point already used
                if (Global.game_map.get_unit(Global.game_map.deployment_points[deployPointIndex]) != null)
                    deployPointIndex++;
                // Else if actor already deployed
                else if (Global.battalion.is_actor_deployed(deployedCount))
                    deployedCount++;
                // Else deploy unit
                else
                {
                    bool unit_added = Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Global.game_map.deployment_points[deployPointIndex],
                        Global.battalion.actors[deployedCount], "");
                    if (unit_added)
                    {
                        Global.game_map.last_added_unit.fix_unit_location(true);
                        Global.game_map.last_added_unit.queue_move_range_update();
                        any_units_added = true;
                    }
                    deployPointIndex++;
                    deployedCount++;
                }
            }
            Global.battalion.sort_by_deployed();
            if (any_units_added)
                Global.game_map.refresh_move_ranges();
        }

        public void refresh_deployed_units(List<int>[] unit_changes)
        {
            if (unit_changes[0].Count == 0 && unit_changes[1].Count == 0)
                return;
            // Remove undeployed units
            foreach (int index in unit_changes[0])
            {
                int id = Global.game_map.get_unit_id_from_actor(Global.battalion.actors[index]);
                Global.game_map.remove_unit(id);
            }
            // Add deployed units
            int i = 0;
            for(int j = 0; j < unit_changes[1].Count; j++)
                while (i < Global.game_map.deployment_points.Count)
                {
                    // If deployment point already used
                    if (Global.game_map.get_unit(Global.game_map.deployment_points[i]) != null)
                        i++;
                    // Else deploy unit
                    else
                    {
                        bool unit_added = Global.game_map.add_actor_unit(
                            Constants.Team.PLAYER_TEAM,
                            Global.game_map.deployment_points[i],
                            Global.battalion.actors[unit_changes[1][j]], "");
                        if (unit_added)
                        {
                            Global.game_map.last_added_unit.fix_unit_location(true);
                            Global.game_map.last_added_unit.queue_move_range_update();
                        }
                        i++;
                        break;
                    }
                }
            Global.battalion.sort_by_deployed();
            Global.game_map.clear_updated_move_ranges();
            Global.game_map.refresh_move_ranges();
        }

        public int SelectedOption
        {
            get
            {
                return CommandWindow
                    .selected_index().Index;
            }
        }

        public event EventHandler<EventArgs> Selected;
        protected void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        public event EventHandler<EventArgs> Start;
        protected void OnStart(EventArgs e)
        {
            if (Start != null)
                Start(this, e);
        }

        public event EventHandler<EventArgs> CheckMap;

        #region Update
        protected override void UpdateMenu(bool active)
        {
            Counter.update();
            CommandWindow.update(active && this.input_ready && CommandWindow.active);
            if (CommandWindow.is_cursor_moving)
                HelpText.text = "";
            if (HelpText.text == "" && !CommandWindow.is_cursor_moving)
                refresh_text();
            PressStartFlicker = (PressStartFlicker + 1) % 8;

            bool was_ready = ready;

            base.UpdateMenu(active);

            if (!was_ready && ready)
                Global.game_system.Preparation_Events_Ready = true;
        }

        protected override void update_input(bool active)
        {
            StartButton.Update(active && input_ready);

            if (active && input_ready)
            {
                bool pressedStart = false;
                if (Global.Input.triggered(Inputs.Start) ||
                        this.start_ui_button_pressed)
                    pressedStart = true;

                if (CommandWindow.is_selected())
                {
                    OnSelected(new EventArgs());
                }
                else if (CommandWindow.is_canceled())
                {
                    command_window_canceled();
                }
                else if (pressedStart)
                {
                    OnStart(new EventArgs());
                }
            }
        }

        protected override void UpdateAncillary()
        {
            if (Input.ControlSchemeSwitched)
                create_start_button();
        }

        protected virtual void command_window_canceled()
        {
            if (CheckMap != null)
                CheckMap(this, new EventArgs());
        }
        
        new public void close()
        {
            close(false);
        }
        public void close(bool start)
        {
            PressedStart = start;
            PressStartFlicker = 0;
            _Closing = true;
            Black_Screen_Hold_Timer = start ? BLACK_SCREEN_HOLD_TIMER : BLACK_SCREEN_SHORT_HOLD_TIME;
            Black_Screen_Timer = Black_Screen_Hold_Timer + (Black_Screen_Fade_Timer * 2);
            if (Black_Screen != null)
                Black_Screen.visible = true;
        }
        #endregion

        #region Draw
        public override void Draw(SpriteBatch sprite_batch)
        {
            base.Draw(sprite_batch);
        }

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Banner.draw(sprite_batch);
            Goal.draw(sprite_batch);
            ChapterLabel.draw(sprite_batch);
            // Windows
            InfoWindow.draw(sprite_batch);
            CommandHelpWindow.draw(sprite_batch);
            HelpText.draw(sprite_batch);
            sprite_batch.End();

            CommandWindow.draw(sprite_batch);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            draw_info(sprite_batch);
            if (!PressedStart || PressStartFlicker < 4)
            {
                StartButton.Draw(sprite_batch);
            }
            sprite_batch.End();
        }

        protected override void draw_background(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (Config.PREP_MAP_BACKGROUND)
            {
                if (this.DataDisplayed)
                    Map_Darken.draw(sprite_batch);
            }
            else
            {
                if (Background != null)
                    Background.draw(sprite_batch);
            }
            sprite_batch.End();
        }

        protected virtual void draw_info(SpriteBatch sprite_batch)
        {
            // Labels
            InfoLabel.draw(sprite_batch);
            // Data
            //AvgRating.draw(sprite_batch); //Debug
            //EnemyAvgRating.draw(sprite_batch);
            AvgLvl.draw(sprite_batch);
            EnemyAvgLvl.draw(sprite_batch);
            Funds.draw(sprite_batch);
            Counter.draw(sprite_batch);
        }
        #endregion
    }
}
