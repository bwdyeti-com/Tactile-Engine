using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Graphics.Text;
using Tactile.Menus;
using Tactile.Windows.Command;

namespace Tactile.Windows
{
    class PrepTradeMenu : Map.Map_Window_Base, ISelectionMenu, IHasCancelButton
    {
        const int BLACK_SCEEN_FADE_TIMER = 8;
        const int BLACK_SCREEN_HOLD_TIMER = 4;

        protected Window_Trade TradeWindow;
        protected Sprite Banner_1, Banner_2;
        protected Button_Description R_Button;
        protected Button_Description CancelButton;
        protected TextSprite Name_1, Name_2;

        #region Accessors
        public bool ready { get { return this.ready_for_inputs; } }

        public int mode { get { return TradeWindow.mode; } }

        public bool is_help_active { get { return TradeWindow.is_help_active; } }
        #endregion

        public PrepTradeMenu(int id1, int id2)
        {
            initialize_sprites(id1, id2);
            CreateCancelButton();
            update_black_screen();
        }

        private void CreateCancelButton(IHasCancelButton menu = null)
        {
            if (menu != null && menu.HasCancelButton)
            {
                CreateCancelButton(
                    (int)menu.CancelButtonLoc.X,
                    Config.MAPCOMMAND_WINDOW_DEPTH);
            }
            else
            {
                CreateCancelButton(
                    Config.WINDOW_WIDTH - (32 + 48),
                    Config.MAPCOMMAND_WINDOW_DEPTH);
            }
        }
        public void CreateCancelButton(int x, float depth = 0)
        {
            CancelButton = Button_Description.button(Inputs.B, x);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = depth;
        }

        protected bool CanceledTriggered(bool active)
        {
            bool cancel = TradeWindow.is_canceled();
            if (active)
            {
                if (CancelButton != null)
                {
                    cancel |= CancelButton.consume_trigger(MouseButtons.Left) ||
                        CancelButton.consume_trigger(TouchGestures.Tap);
                }
            }
            return cancel;
        }

        protected override void set_black_screen_time()
        {
            Black_Screen_Fade_Timer = BLACK_SCEEN_FADE_TIMER;
            Black_Screen_Hold_Timer = BLACK_SCREEN_HOLD_TIMER;
            base.set_black_screen_time();
        }

        protected void initialize_sprites(int id1, int id2)
        {
            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 255);
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Preparation_Background");
            (Background as Menu_Background).vel = new Vector2(0, -1 / 3f);
            (Background as Menu_Background).tile = new Vector2(1, 2);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
            // Trade Window
            TradeWindow = new Window_Trade(id1, id2, -1);
            TradeWindow.glow = true;
            TradeWindow.stereoscopic = Config.PREPTRADE_WINDOWS_DEPTH;
            TradeWindow.face_stereoscopic = Config.PREPTRADE_FACES_DEPTH;
            TradeWindow.help_stereoscopic = Config.PREPTRADE_HELP_DEPTH;
            //
            Banner_1 = new Sprite();
            Banner_1.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
            Banner_1.loc = new Vector2(0, 0);
            Banner_1.src_rect = new Rectangle(0, 40, 56, 24);
            Banner_1.offset = new Vector2(56, 1);
            Banner_1.tint = new Color(224, 224, 224, 192);
            Banner_1.mirrored = true;
            Banner_1.stereoscopic = Config.PREPTRADE_NAMES_DEPTH;
            Banner_2 = new Sprite();
            Banner_2.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
            Banner_2.loc = new Vector2(264, 0);
            Banner_2.src_rect = new Rectangle(0, 40, 56, 24);
            Banner_2.offset = new Vector2(0, 1);
            Banner_2.tint = new Color(224, 224, 224, 192);
            Banner_2.stereoscopic = Config.PREPTRADE_NAMES_DEPTH;
            Name_1 = new TextSprite();
            Name_1.loc = Banner_1.loc + new Vector2(24, 0);
            Name_1.SetFont(Config.UI_FONT, Global.Content,"White");
            Name_1.text = Global.game_actors[id1].name;
            Name_1.offset = new Vector2(Font_Data.text_width(Name_1.text) / 2, 0);
            Name_1.stereoscopic = Config.PREPTRADE_NAMES_DEPTH;
            Name_2 = new TextSprite();
            Name_2.loc = Banner_2.loc + new Vector2(32, 0);
            Name_2.SetFont(Config.UI_FONT, Global.Content, "White");
            Name_2.text = Global.game_actors[id2].name;
            Name_2.offset = new Vector2(Font_Data.text_width(Name_2.text) / 2, 0);
            Name_2.stereoscopic = Config.PREPTRADE_NAMES_DEPTH;

            refresh_input_help();
        }

        protected void refresh_input_help()
        {
            R_Button = Button_Description.button(Inputs.R,
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen"), new Rectangle(126, 122, 24, 16));
            R_Button.loc = new Vector2(192, Config.WINDOW_HEIGHT - 16);
            R_Button.offset = new Vector2(0, 0);
            R_Button.stereoscopic = Config.PREPTRADE_NAMES_DEPTH;

            // Hide buttons that don't really do anything for touch
            R_Button.Visible = Input.ControlScheme != ControlSchemes.Touch;
        }

        public ConsumedInput selected_index()
        {
            return TradeWindow.selected_index();
        }

        public bool is_selected()
        {
            return TradeWindow.is_selected();
        }

        public ConsumedInput help_index()
        {
            return TradeWindow.help_index();
        }

        public bool getting_help()
        {
            return TradeWindow.getting_help();
        }

        public bool is_canceled()
        {
            return TradeWindow.is_canceled();
        }

        public void reset_selected() { }

        #region Update
        protected override void UpdateMenu(bool active)
        {
            TradeWindow.update(active && ready);
            base.UpdateMenu(active);
        }

        protected override void UpdateAncillary()
        {
            if (Input.ControlSchemeSwitched)
            {
                refresh_input_help();
                CreateCancelButton(this);
            }
        }

        protected override void update_input(bool active)
        {
            active &= !_Closing;

            if (CancelButton != null)
                CancelButton.Update(active);
            bool cancel = CanceledTriggered(active);

            if (active)
            {
                if (TradeWindow.ready)
                {
                    if (TradeWindow.is_help_active)
                    {
                        if (cancel)
                            TradeWindow.close_help();
                    }
                    else
                    {
                        if (TradeWindow.getting_help())
                            TradeWindow.open_help();
                        else if (cancel)
                        {
                            Global.game_system.play_se(System_Sounds.Cancel);
                            // An item is selected, deselect it
                            if (TradeWindow.mode > 0)
                            {
                                TradeWindow.cancel();
                            }
                            // Nothing selected, close the menu
                            else
                            {
                                TradeWindow.staff_fix();
                                close();
                                OnClosing(new EventArgs());
                            }
                            return;
                        }
                        else if (TradeWindow.is_selected())
                        {
                            TradeWindow.enter();
                        }
                    }
                }
            }
        }

        new public void close()
        {
            _Closing = true;
            Black_Screen_Timer = Black_Screen_Hold_Timer + (Black_Screen_Fade_Timer * 2);
            if (Black_Screen != null)
                Black_Screen.visible = true;
        }
        #endregion

        public bool enter()
        {
            return TradeWindow.enter();
        }

        public void cancel()
        {
            TradeWindow.cancel();
        }

        public void staff_fix()
        {
            TradeWindow.staff_fix();
        }

        public void open_help()
        {
            TradeWindow.open_help();
        }

        public virtual void close_help()
        {
            TradeWindow.close_help();
        }

        #region Draw
        protected override void draw_window(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Banner_1.draw(sprite_batch);
            Banner_2.draw(sprite_batch);
            Name_1.draw(sprite_batch);
            Name_2.draw(sprite_batch);
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            R_Button.Draw(sprite_batch);
            sprite_batch.End();

            if (CancelButton != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                CancelButton.Draw(sprite_batch);
                sprite_batch.End();
            }

            TradeWindow.draw(sprite_batch);
        }
        #endregion

        #region IHasCancelButton
        public bool HasCancelButton { get { return CancelButton != null; } }
        public Vector2 CancelButtonLoc { get { return CancelButton.loc; } }
        #endregion
    }
}
