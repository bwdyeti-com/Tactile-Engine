using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Windows.Command;
using TactileLibrary;

namespace Tactile.Windows
{
    enum PrepCheckMapResults {
        ViewMap, Formation, Options, Save, StartChapter, Cancel, Info, None }

    class Window_Setup_CheckMap : Map.Map_Window_Base
    {
        const int BLACK_SCEEN_FADE_TIMER = 16;
        const int BLACK_SCREEN_HOLD_TIMER = 12;
        const int MAP_DARKEN_TIME = 8;

        protected bool StartingMap = false;
        protected int Map_Darken_Time = 0;
        private bool Hiding = false;
        protected Window_Command Command_Window;
        protected Sprite Map_Darken;
        protected Button_Description Start, B_Button, R_Button;

        private Maybe<PrepCheckMapResults> SelectedIndex = Maybe<PrepCheckMapResults>.Nothing;

        #region Accessors
        new public bool closed { get { return Black_Screen_Timer <= 0 && Map_Darken_Time >= MAP_DARKEN_TIME && _Closing; } }

        public bool starting_map { get { return StartingMap; } }

        public int index
        {
            get { return Command_Window.index; }
            set { Command_Window.immediate_index = value; }
        }

        public bool ready
        {
            get { return this.ready_for_inputs && Map_Darken_Time == 0 && !Hiding; }
        }

        public override bool HidesParent
        {
            get
            {
                return this.DataDisplayed || (_Closing && StartingMap);
            }
        }
        #endregion

        public Window_Setup_CheckMap()
        {
            initialize_sprites();
            update_black_screen();
        }

        protected override void set_black_screen_time()
        {
            Black_Screen_Fade_Timer = BLACK_SCEEN_FADE_TIMER;
            Black_Screen_Hold_Timer = BLACK_SCREEN_HOLD_TIMER;
            base.set_black_screen_time();
        }

        protected void initialize_sprites()
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
            update_map_darken_tint();
            // Command Window
            Command_Window = new Window_Command(new Vector2(Config.WINDOW_WIDTH / 2 - 40, 32),
                80, new List<string> { "View Map", "Formation", "Options", "Save" });
            Command_Window.text_offset = new Vector2(8, 0);
            Command_Window.glow = true;
            Command_Window.bar_offset = new Vector2(-8, 0);

            refresh_input_help();
        }

        protected void refresh_input_help()
        {
            Start = Button_Description.button(Inputs.Start,
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen"), new Rectangle(142, 41, 32, 16));
            Start.loc = new Vector2(Config.WINDOW_WIDTH / 2, Config.WINDOW_HEIGHT) + new Vector2(-128, -16);
            Start.offset = new Vector2(0, 3);
            B_Button = Button_Description.button(Inputs.B,
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen"), new Rectangle(123, 105, 24, 16));
            B_Button.loc = new Vector2(Config.WINDOW_WIDTH / 2, Config.WINDOW_HEIGHT) + new Vector2(-16, -16);
            B_Button.offset = new Vector2(0, 3);
            R_Button = Button_Description.button(Inputs.R,
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen"), new Rectangle(126, 122, 24, 16));
            R_Button.loc = new Vector2(Config.WINDOW_WIDTH / 2, Config.WINDOW_HEIGHT) + new Vector2(80, -16);
            R_Button.offset = new Vector2(0, 2);
        }

        public PrepCheckMapResults SelectedOption
        {
            get
            {
                if (SelectedIndex.IsSomething)
                    return SelectedIndex.ValueOrDefault;
                return (PrepCheckMapResults)Command_Window
                    .selected_index().Index;
            }
        }

        public event EventHandler<EventArgs> Selected;
        protected void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        internal void HideToViewMap()
        {
            Hide(PrepCheckMapResults.ViewMap);
        }

        internal void HideToChangeFormation()
        {
            Hide(PrepCheckMapResults.Formation);
        }

        private void Hide(PrepCheckMapResults option)
        {
            if (Hidden != null)
            {
                SelectedIndex = option;
                Hiding = true;
                Map_Darken_Time = MAP_DARKEN_TIME;
                update_map_darken_tint();
            }
        }

        public event EventHandler<EventArgs> Hidden;

        internal void Show()
        {
            Black_Screen_Timer = 0;
            update_black_screen();
            Map_Darken_Time = MAP_DARKEN_TIME;
            update_map_darken_tint();
        }

        #region Update
        protected override void UpdateMenu(bool active)
        {
            if (Map_Darken_Time > 0)
            {
                Map_Darken_Time--;
                update_map_darken_tint();
                if (Map_Darken_Time == 0)
                {
                    if (Hiding)
                    {
                        Hidden(this, new EventArgs());
                    }
                }
            }
            bool input = active && ready;
            Command_Window.update(input);
            update_ui(input);

            base.UpdateMenu(active);
        }

        protected override void UpdateAncillary()
        {
            if (Input.ControlSchemeSwitched)
                refresh_input_help();
        }

        private void update_ui(bool input)
        {
            Start.Update(input);
            B_Button.Update(input);
            R_Button.Update(input);

            if (input)
            {
                SelectedIndex = Maybe<PrepCheckMapResults>.Nothing;

                if (Global.Input.triggered(Inputs.Start) ||
                    Start.consume_trigger(MouseButtons.Left) ||
                    Start.consume_trigger(TouchGestures.Tap))
                {
                    SelectedIndex = PrepCheckMapResults.StartChapter;
                    OnSelected(new EventArgs());
                }
                else if (B_Button.consume_trigger(MouseButtons.Left) ||
                    B_Button.consume_trigger(TouchGestures.Tap))
                {
                    SelectedIndex = PrepCheckMapResults.Cancel;
                    OnSelected(new EventArgs());
                }
                else if (R_Button.consume_trigger(MouseButtons.Left) ||
                    R_Button.consume_trigger(TouchGestures.Tap))
                {
                    SelectedIndex = PrepCheckMapResults.Info;
                    OnSelected(new EventArgs());
                }
            }
        }

        protected override void update_input(bool active)
        {
            if (active)
            {
                if (Command_Window.is_canceled())
                {
                    SelectedIndex = PrepCheckMapResults.Cancel;
                    OnSelected(new EventArgs());
                }
                else if (Command_Window.is_selected())
                {
                    OnSelected(new EventArgs());
                }
            }
        }

        protected void update_map_darken_tint()
        {
            int a = Hiding ? Map_Darken_Time : (MAP_DARKEN_TIME - Map_Darken_Time);
            a = a * 128 / MAP_DARKEN_TIME;
            Map_Darken.tint = new Color(0, 0, 0, a);
        }

        new public void close()
        {
            close(false);
        }
        public void close(bool start)
        {
            OnClosing(new EventArgs());
            StartingMap = start;
            _Closing = true;
            set_black_screen_time();
        }
        #endregion

        #region Draw
        public override void Draw(SpriteBatch sprite_batch)
        {
            if (this.DataDisplayed)
            {
                draw_map_darken(sprite_batch);

                if (Map_Darken_Time == 0)
                {
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    // //Yeti
                    Start.Draw(sprite_batch);
                    B_Button.Draw(sprite_batch);
                    R_Button.Draw(sprite_batch);
                    sprite_batch.End();

                    Command_Window.draw(sprite_batch);
                }
            }

            base.Draw(sprite_batch);
        }

        public void draw_map_darken(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Map_Darken.draw(sprite_batch);
            sprite_batch.End();
        }
        #endregion
    }
}
