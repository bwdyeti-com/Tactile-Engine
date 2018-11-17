using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Graphics.Text;
using FEXNA.Map;
using FEXNA.Windows.Command;
using FEXNA_Library;

namespace FEXNA
{
    enum Shop_Messages {
        Intro, Question, Buy, Buy_Confirm, Buy_Else, Buy_Not_Enough,
        Buy_Nothing, Buy_Full, Buy_No_Merchant, Sell, Sell_Confirm, Sell_Else, Sell_Cant_Buy,
        Sell_Nothing, Anything_Else, Leave, Cancel,
        Send_Confirm, Send_Yes, Send_No, Merchant_Full }
    class Window_Shop : Window_Business
    {
        readonly static Shop_Messages[] WAIT_TEXT = {
            Shop_Messages.Intro, Shop_Messages.Buy_Not_Enough, Shop_Messages.Sell_Cant_Buy, Shop_Messages.Leave, Shop_Messages.Cancel,
            Shop_Messages.Buy_Full, Shop_Messages.Buy_No_Merchant, Shop_Messages.Buy_Nothing, Shop_Messages.Sell_Nothing,
            Shop_Messages.Send_Yes, Shop_Messages.Send_No, Shop_Messages.Merchant_Full };

        private int Actor_Id;
        private int Buy_Price_Mod;
        private bool Confirming_Send = false;

        private Window_Command_Shop Window;
        protected Button_Description CancelButton;

        #region Accessors
        protected Game_Actor actor
        {
            get
            {
                if (!Global.game_actors.ContainsKey(Actor_Id))
                    return null;
                return Global.game_actors[Actor_Id];
            }
        }

        protected override bool trading
        {
            get { return base.trading; }
            set
            {
                base.trading = value;
                if (!this.trading)
                    Window.active = false;
            }
        }

        protected int row_max { get { return On_Buy ? Shop.items.Count : actor.num_items; } }
        #endregion

        public Window_Shop() { }
        public Window_Shop(int actor_id, Shop_Data shop)
        {
            Actor_Id = actor_id;
            Shop = shop;
            if (!string.IsNullOrEmpty(Shop.song) || !Global.game_system.preparations)
                Global.Audio.BgmFadeOut(60);
            // This keeps the prices fixed even if the shopper sells their silver card or whatever
            Buy_Price_Mod = determine_buy_price_mod();
            initialize_images();
        }

        private int determine_buy_price_mod()
        {
            if (this.actor == null)
                return 1;
            return actor.buy_price_mod();
        }

        protected override int choice_offset()
        {
            switch ((Shop_Messages)Message_Id)
            {
                case Shop_Messages.Anything_Else:
                    return Shop.offsets[1];
                case Shop_Messages.Buy_Confirm:
                    return Shop.offsets[2];
                case Shop_Messages.Sell_Confirm:
                    return Shop.offsets[3];
                case Shop_Messages.Send_Confirm:
                    return Shop.offsets[4];
                default:
                    return Shop.offsets[0];
            }
        }

        #region Image Setup
        protected override void initialize_images()
        {
            Item_Rect = new Rectangle(40, 88, 248, 16 * ROWS);

            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 0);
            // Background
            Menu_Background background = new Menu_Background();
            background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Status_Background");
            background.vel = new Vector2(-0.25f, 0);
            background.tile = new Vector2(3, 2);
            Background = background;
            // Darkened Bar
            Darkened_Bar = new Sprite();
            Darkened_Bar.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Darkened_Bar.dest_rect = new Rectangle(0, 8, Config.WINDOW_WIDTH, 48);
            Darkened_Bar.tint = new Color(0, 0, 0, 128);

            // Data
            refresh_buy();
            // Portrait BG
            Portrait_Bg = new Sprite();
            if (Global.game_system.preparations && string.IsNullOrEmpty(Shop.face))
            {
                Portrait_Bg.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
                Portrait_Bg.src_rect = new Rectangle(136, 136, 48, 64);
                Portrait_Bg.loc = new Vector2(16, 0);

                Portrait_Label = new FE_Text();
                Portrait_Label.loc = Portrait_Bg.loc + new Vector2(8, 40);
                Portrait_Label.Font = "FE7_Text";
                Portrait_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
                Portrait_Label.text = "Convoy";
            }
            else
            {
                Portrait_Bg.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Shop_Portrait_bg");
                Portrait_Bg.src_rect = new Rectangle(0, 0, 57, 57);
                Portrait_Bg.loc = new Vector2(12, 4);
            }
            // Gold Window
            Gold_Window = new Sprite();
            Gold_Window.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Gold_Window");
            Gold_Window.loc = new Vector2(212, 48);
            // Gold_Data
            Gold_Data = new FE_Text_Int();
            Gold_Data.loc = new Vector2(216 + 48, 48);
            Gold_Data.Font = "FE7_Text";
            Gold_Data.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            redraw_gold();
            Gold_G = new FE_Text();
            Gold_G.loc = new Vector2(216 + 48, 48);
            Gold_G.Font = "FE7_TextL";
            Gold_G.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Gold_G.text = "G";
            // Text
            Message = new Message_Box(64, 8, 160, 2, false, "White");
            set_text(Shop_Messages.Intro);
            Message_Active = true;

            create_cancel_button();

            set_images();
        }

        protected void create_cancel_button()
        {
            CancelButton = Button_Description.button(Inputs.B,
                Config.WINDOW_WIDTH - 48);
            CancelButton.loc.Y = 0;
            CancelButton.description = "Cancel";
        }

        protected override bool is_wait_text(int message_id)
        {
            return WAIT_TEXT.Contains((Shop_Messages)message_id);
        }

        private void set_text(Shop_Messages message_id)
        {
            set_text((int)message_id);
        }

        private void refresh_buy(bool resetIndex = false)
        {
            /* //Debug
            Item_Data.Clear();
            for (int i = 0; i < Shop.items.Count; i++)
            {
                Item_Data.Add(new Shop_Item());
                Item_Data[i].set_image(actor, new Item_Data(Shop.items[i].Type, Shop.items[i].Id), Shop.items[i].Uses, item_cost(i));
                Item_Data[i].loc = Window.loc + new Vector2(8, 28 + i * 16);
            }*/

            if (resetIndex)
                Window = null;
            Window = new Window_Command_Shop(
                Window,
                new Vector2(Item_Rect.X - 8, Item_Rect.Y - 28),
                Item_Rect.Width + 16, ROWS,
                true, Actor_Id, Buy_Price_Mod, Shop.items);
            Window.active = Trading;
        }

        private void refresh_sell(bool resetIndex = false)
        {
            /* //Debug
            Item_Data.Clear();
            int count = actor.num_items;
            for (int i = 0; i < count; i++)
            {
                Item_Data.Add(new Shop_Sell_Item());
                Item_Data[i].set_image(actor, actor.items[i], -1, item_cost(i));
                Item_Data[i].loc = Window.loc + new Vector2(8, 28 + i * 16);
            }*/

            if (resetIndex)
                Window = null;
            Window = new Window_Command_Shop(
                Window,
                new Vector2(Item_Rect.X - 8, Item_Rect.Y - 28),
                Item_Rect.Width + 16, ROWS,
                false, Actor_Id, Buy_Price_Mod,
                Enumerable.Range(0, actor.num_items)
                    .Select(i => this.actor.items[i])
                    .ToList());
            Window.active = Trading;
        }
        #endregion

        #region Processing
        protected override int item_cost()
        {
            return item_cost(Window.index);
        }
        private int item_cost(int index)
        {
            int price = 0;
            Item_Data item_data = this.item_data(index);
            return item_data.item_cost(On_Buy, Buy_Price_Mod);
        }

        private Item_Data item_data(int index)
        {
            if (On_Buy)
            {
                if (index == -1 || index >= Shop.items.Count)
                    return new Item_Data();
                return Shop.items[index];
            }
            else
            {
                if (index == -1 || index >= actor.num_items)
                    return new Item_Data();
                return actor.items[index];
            }
        }

        private bool can_trade
        {
            get
            {
                if (On_Buy)
                    return (item_cost() <= Global.battalion.gold);
                else
                {
                    return actor.items[Window.index].to_equipment.Can_Sell;
                }
            }
        }

        private bool can_buy { get { return Shop.items.Count > 0; } }

        private bool can_sell { get { return actor.num_items > 0; } }

        private bool is_inventory_full { get { return actor.is_full_items; } }
        #endregion

        protected override void play_shop_theme()
        {
            if (!string.IsNullOrEmpty(Shop.song) || !Global.game_system.preparations)
                Global.Audio.PlayBgm(Shop.song);
        }

        #region Update
        protected override void UpdateMenu(bool active)
        {
            update_black_screen();
            if (Delay > 0)
                Delay--; // Does this do anything //Yeti
            Background.update();
            bool input = !Closing && Delay == 0 && Black_Screen_Timer == 0;

            Window.update(input && Trading && !Accepting && Message.text_end);
            if (Input.ControlSchemeSwitched)
                create_cancel_button();
            CancelButton.Update(input && !Accepting && Message.text_end);
            if (Choices != null)
            {
                Choices.Update(input && Message.text_end);
                update_cursor_location();
            }
            if (input)
            {
                if (Message_Active)
                    update_message();
                if (!Closing && Message.text_end)
                {
                    if (Trading && !Accepting && !Window.active)
                        Window.active = true;

                    else if (!Trading)
                        update_main_selection();
                    else if (!Accepting)
                        update_trading();
                    else if (!Confirming_Send)
                        update_accepting();
                    else
                        update_sending();
                }
            }

            Face.update();
        }

        protected override void update_message()
        {
            Message.update();
            if (Message.text_end && !Message.wait)
            {
                switch ((Shop_Messages)Message_Id)
                {
                    case Shop_Messages.Intro:
                        if (this.actor == null)
                        {
                            if (can_buy)
                            {
                                set_text(Shop_Messages.Buy);
                                this.trading = true;
                            }
                            else
                                set_text(Shop_Messages.Buy_Nothing);
                        }
                        else
                            set_text(Shop_Messages.Question);
                        break;
                    case Shop_Messages.Question:
                    case Shop_Messages.Anything_Else:
                    case Shop_Messages.Buy_Confirm:
                    case Shop_Messages.Sell_Confirm:
                    case Shop_Messages.Send_Confirm:
                        Message_Active = false;
                        string yes = "Buy", no = "Sell";
                        int choice_index = On_Buy ? 0 : 1;
                        if (Message_Id == (int)Shop_Messages.Buy_Confirm ||
                            Message_Id == (int)Shop_Messages.Sell_Confirm ||
                            Message_Id == (int)Shop_Messages.Send_Confirm)
                        {
                            yes = "Yes";
                            no = "No";
                            choice_index = On_Yes ? 0 : 1;
                        }
                        set_choices(choice_offset(), yes, no);
                        Choices.set_active_node(Choices[choice_index]);
                        update_cursor_location(true);
                        break;
                    case Shop_Messages.Buy:
                    case Shop_Messages.Sell:
                    case Shop_Messages.Buy_Else:
                    case Shop_Messages.Sell_Else:
                        Message_Active = false;
                        break;
                    case Shop_Messages.Buy_Not_Enough:
                        set_text(Shop_Messages.Buy);
                        break;
                    case Shop_Messages.Sell_Cant_Buy:
                        set_text(Shop_Messages.Sell);
                        break;
                    case Shop_Messages.Buy_Full:
                        set_text(Shop_Messages.Buy_No_Merchant);
                        break;
                    case Shop_Messages.Buy_Nothing:
                    case Shop_Messages.Sell_Nothing:
                        if (this.actor == null)
                        {
                            Message_Active = true;
                            Cursor.visible = false;
                            set_text(Shop_Messages.Leave);
                            Message.finished = true;
                        }
                        else
                        {
                            this.trading = false;
                            set_text(Shop_Messages.Anything_Else);
                        }
                        break;
                    case Shop_Messages.Buy_No_Merchant:
                    case Shop_Messages.Send_No:
                    case Shop_Messages.Merchant_Full:
                        this.trading = false;
                        set_text(Shop_Messages.Anything_Else);
                        break;
                    case Shop_Messages.Leave:
                    case Shop_Messages.Cancel:
                        close();
                        Black_Screen_Timer = BLACK_SCREEN_HOLD_TIMER + (BLACK_SCREEN_FADE_TIMER * 2);
                        if (!string.IsNullOrEmpty(Shop.song) || !Global.game_system.preparations)
                            Global.Audio.BgmFadeOut(60);
                        break;
                    case Shop_Messages.Send_Yes:
                        set_text(Shop_Messages.Buy_Else);
                        break;
                }
            }
        }

        protected override void update_main_selection()
        {
            bool on_buy = On_Buy;
            On_Buy = Choices.ActiveNodeIndex == 0;
            if (On_Buy != on_buy)
            {
                if (On_Buy)
                    refresh_buy(true);
                else
                    refresh_sell(true);
            }

            var selected = Choices.consume_triggered(
                Inputs.A, MouseButtons.Left, TouchGestures.Tap);

            if (selected.IsSomething)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Message_Active = true;
                if (On_Buy)
                {
                    if (can_buy)
                    {
                        set_text(Shop_Messages.Buy);
                        this.trading = true;
                    }
                    else
                        set_text(Shop_Messages.Buy_Nothing);
                }
                else
                {
                    if (can_sell)
                    {
                        set_text(Shop_Messages.Sell);
                        this.trading = true;
                    }
                    else
                        set_text(Shop_Messages.Sell_Nothing);
                }
                clear_choices();
            }
            else if (Global.Input.triggered(Inputs.B) ||
                CancelButton.consume_trigger(MouseButtons.Left) ||
                CancelButton.consume_trigger(TouchGestures.Tap))
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                Message_Active = true;
                set_text(Traded ? Shop_Messages.Leave : Shop_Messages.Cancel);
                Message.finished = true;
                clear_choices();
            }
        }

        private void update_trading()
        {
            if (Window.getting_help())
            {
                Window.open_help();
            }
            else if (Window.is_canceled() && Window.is_help_active)
            {
                Window.close_help();
            }
            else if (Window.is_canceled() ||
                (this.actor == null && Window.is_selected()) ||
                Global.Input.mouse_click(MouseButtons.Right) ||
                CancelButton.consume_trigger(MouseButtons.Left) ||
                CancelButton.consume_trigger(TouchGestures.Tap)) //Yeti
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                Message_Active = true;
                this.trading = false;
                if (this.actor == null)
                {
                    set_text(Shop_Messages.Leave);
                    Message.finished = true;
                    clear_choices();
                }
                else
                {
                    set_text(Shop_Messages.Anything_Else);
                }
            }
            else if (Window.is_selected())
            {
                int index = Window.selected_index();
                Window.index = index;

                if (Window.is_help_active)
                    Window.close_help();

                Message_Active = true;
                Window.active = false;
                if (is_inventory_full && On_Buy && Global.battalion.convoy_id == -1)
                    set_text(Shop_Messages.Buy_Full);
                else if (can_trade)
                {
                    Accepting = true;
                    set_text(On_Buy ? Shop_Messages.Buy_Confirm : Shop_Messages.Sell_Confirm);
                    On_Yes = true;
                    update_cursor_location(true);
                }
                else
                    set_text(On_Buy ? Shop_Messages.Buy_Not_Enough : Shop_Messages.Sell_Cant_Buy);
            }
        }

        private void update_accepting()
        {
            On_Yes = Choices.ActiveNodeIndex == 0;

            var selected = Choices.consume_triggered(
                Inputs.A, MouseButtons.Left, TouchGestures.Tap);

            if (selected.IsSomething)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                if (!On_Yes)
                    cancel_accepting();
                else
                {
                    int index = Window.index;

                    Message_Active = true;
                    if (is_inventory_full && On_Buy)
                    {
                        Confirming_Send = true;
                        set_text(Shop_Messages.Send_Confirm);
                        On_Yes = true;
                        update_cursor_location(true);
                    }
                    else
                    {
                        Accepting = false;
                        Traded = true;
                        if (On_Buy)
                            buy_item();
                        else
                            sell_item();
                        Global.Audio.play_se("System Sounds", "Gold_Change");
                    }
                }
                clear_choices();
            }
            else if (Global.Input.triggered(Inputs.B))
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                cancel_accepting();
                clear_choices();
            }
        }

        private void update_sending()
        {
            On_Yes = Choices.ActiveNodeIndex == 0;

            var selected = Choices.consume_triggered(
                Inputs.A, MouseButtons.Left, TouchGestures.Tap);

            if (selected.IsSomething)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                if (!On_Yes)
                {
                    cancel_accepting(Shop_Messages.Send_No);
                    this.trading = false;
                }
                else
                {
                    if (!Global.battalion.convoy_ready_for_sending)
                    {
                        cancel_accepting(Shop_Messages.Merchant_Full);
                        this.trading = false;
                    }
                    else
                    {
                        Accepting = false;
                        Confirming_Send = false;
                        Message_Active = true;
                        Traded = true;
                        buy_item(true);
                        Global.Audio.play_se("System Sounds", "Gold_Change");
                    }
                }
                clear_choices();
            }
            else if (Global.Input.triggered(Inputs.B))
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                cancel_accepting(Shop_Messages.Send_No);
                this.trading = false;
                clear_choices();
            }
        }
        #endregion

        #region Shop Processing
        private void buy_item(bool sending = false)
        {
            int index = Window.index;

            // Change gold
            Global.battalion.gold -= item_cost();
            redraw_gold();
            // Adds item
            var bought_item = new Item_Data(Shop.items[index].Type, Shop.items[index].Id);
            if (sending)
                Global.game_battalions.add_item_to_convoy(bought_item);
            else
                actor.gain_item(bought_item);
            //Removes item from shop
            if (Shop.items[index].Uses > 0)
                Shop.items[index].consume_use();
            if (Shop.items[index].Uses == 0)
                Shop.items.RemoveAt(index);
            // Redraw text and images
            refresh_buy();
            if (can_buy && (!is_inventory_full || Global.battalion.convoy_id > -1))
                set_text(Shop_Messages.Buy_Else);
            else
            {
                set_text(Shop_Messages.Anything_Else);
                this.trading = false;
            }
        }

        private void sell_item()
        {
            int index = Window.index;

            // Change gold
            Global.battalion.gold += item_cost();
            redraw_gold();
            // Removes item
            Item_Data sold_item = actor.drop_item(index);
            if (Global.game_system.preparations && Global.battalion.convoy_id > -1)
                Global.game_battalions.add_sold_home_base_item(sold_item);
            // Redraw text and images
            refresh_sell();
            if (can_sell)
                set_text(Shop_Messages.Sell_Else);
            else
            {
                set_text(Shop_Messages.Anything_Else);
                this.trading = false;
            }
        }

        private void cancel_accepting()
        {
            cancel_accepting(On_Buy ? Shop_Messages.Buy : Shop_Messages.Sell);
        }
        private void cancel_accepting(Shop_Messages message)
        {
            Confirming_Send = false;
            Accepting = false;
            Message_Active = true;
            set_text(message);
        }
        #endregion

        protected override void draw_background(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Background.draw(sprite_batch);
            sprite_batch.End();
        }

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            Window.draw(sprite_batch);

            if (!Closing && Input.ControlScheme != ControlSchemes.Buttons &&
                (Shop_Messages)Message_Id != Shop_Messages.Leave &&
                (Shop_Messages)Message_Id != Shop_Messages.Cancel)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                CancelButton.Draw(sprite_batch);
                sprite_batch.End();
            }
        }
    }
}
