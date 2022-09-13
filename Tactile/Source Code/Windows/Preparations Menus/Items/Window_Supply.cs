using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Windows.Command;
using Tactile.Windows.Command.Items;

namespace Tactile.Windows.Map.Items
{
    enum Supply_Command_Window { All, Give, Restock, Take }
    class Window_Supply : WindowItemManagement
    {
        private bool Giving = false, Taking = false, Restocking = false;
        private Window_Command Command_Window;
        private Miniface Convoy_Face;
        private Sprite Face_Bg, Banner;
        private TextSprite Convoy_Label, Convoy_Text;
        private TextSprite Stock_Label, Stock_Value, Stock_Slash, Stock_Max;

        #region Accessors
        public bool giving { get { return Giving; } }
        public bool taking { get { return Taking; } }
        public bool restocking { get { return Restocking; } }
        /// <summary>
        /// Performing an supply action: giving, taking, or restocking.
        /// </summary>
        public bool trading { get { return Giving || Taking || Restocking; } }
        public bool selecting_take { get { return Item_Selection_Window != null; } }

        public bool can_give
        {
            get
            {
                return actor.num_items > 0 &&
                    !Global.game_battalions.active_convoy_is_full;
            }
        }
        public bool can_take
        {
            get
            {
                return actor.num_items < Global.ActorConfig.NumItems &&
                    Global.game_battalions.active_convoy_data.Count > 0;
            }
        }
        public bool can_restock { get { return actor.num_items > 0 && Global.game_battalions.active_convoy_data.Count > 0; } }

        private bool restock_blocked { get { return Global.scene.is_worldmap_scene; } }
        #endregion

        public Window_Supply(int actorId) : base(actorId) { }
        public Window_Supply(Game_Unit unit) : base(unit) { }

        protected override void initialize_sprites()
        {
            // Command Window
            create_command_window(Supply_Command_Window.All);

            base.initialize_sprites();

            Banner = new Sprite();
            Banner.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Banner.loc = new Vector2(0, 8);
            Banner.draw_offset = new Vector2(Config.WINDOW_WIDTH / 2, 4);
            Banner.offset = new Vector2(16 / 2, 0);
            Banner.scale = new Vector2(
                (Config.WINDOW_WIDTH + Math.Abs(Config.CONVOY_BANNER_DEPTH) * 4) / 16f,
                40 / 16f);
            Banner.tint = new Color(0, 0, 0, 128);
            Banner.stereoscopic = Config.CONVOY_BANNER_DEPTH;

            Face_Bg = new Sprite();
            Face_Bg.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
            Face_Bg.loc = Banner.loc + new Vector2(8, -8);
            Face_Bg.src_rect = new Rectangle(136, 136, 48, 64);
            Face_Bg.stereoscopic = Config.CONVOY_ICON_DEPTH;
            Convoy_Face = new Miniface();
            Convoy_Face.loc = Face_Bg.loc + new Vector2(24, 8);
            Convoy_Face.set_actor(Global.battalion.convoy_face_name);
            Convoy_Face.stereoscopic = Config.CONVOY_ICON_DEPTH;

            Convoy_Label = new TextSprite();
            Convoy_Label.loc = Face_Bg.loc + new Vector2(8, 40);
            Convoy_Label.SetFont(Config.UI_FONT, Global.Content, "White");
            Convoy_Label.text = "Convoy";
            Convoy_Label.stereoscopic = Config.CONVOY_ICON_DEPTH;
            Convoy_Text = new TextSprite();
            Convoy_Text.loc = Face_Bg.loc + new Vector2(48, 16);
            Convoy_Text.SetFont(Config.UI_FONT, Global.Content, "White");
            Convoy_Text.text = "What'll you do?";
            Convoy_Text.one_at_a_time = true;
            Convoy_Text.stereoscopic = Config.CONVOY_ICON_DEPTH;

            Stock_Label = new TextSprite();
            Stock_Label.loc = Stock_Banner.loc + new Vector2(8, 0);
            Stock_Label.SetFont(Config.UI_FONT, Global.Content, "White");
            Stock_Label.text = "Stock";
            Stock_Label.stereoscopic = Config.CONVOY_STOCK_DEPTH;
            Stock_Value = new RightAdjustedText();
            Stock_Value.loc = Stock_Banner.loc + new Vector2(56, 0);
            Stock_Value.SetFont(Config.UI_FONT);
            Stock_Value.stereoscopic = Config.CONVOY_STOCK_DEPTH;
            Stock_Slash = new TextSprite();
            Stock_Slash.loc = Stock_Banner.loc + new Vector2(56, 0);
            Stock_Slash.SetFont(Config.UI_FONT, Global.Content, "White");
            Stock_Slash.text = "/";
            Stock_Slash.stereoscopic = Config.CONVOY_STOCK_DEPTH;
            Stock_Max = new RightAdjustedText();
            Stock_Max.loc = Stock_Banner.loc + new Vector2(88, 0);
            Stock_Max.SetFont(Config.UI_FONT);
            Stock_Max.stereoscopic = Config.CONVOY_STOCK_DEPTH;
        }

        private void create_command_window(Supply_Command_Window type)
        {
            List<string> commands;
            switch (type)
            {
                case Supply_Command_Window.Give:
                    commands = new List<string> { "Give" };
                    break;
                case Supply_Command_Window.Take:
                    commands = new List<string> { "Take" };
                    break;
                case Supply_Command_Window.Restock:
                    commands = new List<string> { "Restock" };
                    break;
                default:
                    if (this.restock_blocked)
                        commands = new List<string> { "Give", "Take" };
                    else
                        commands = new List<string> { "Give", "Restock", "Take" };
                    break;
            }
            int width = 56;
            int i = Math.Max(0, (int)type - 1);
            Vector2 loc = new Vector2(
                (64 + 12) + ((i % 2) * (width - 16)),
                (36 - 2) + ((i / 2) * 16));
            //Vector2 loc = new Vector2( //Debug
            //    (64 + 36),
            //    (36 - 2));
            Command_Window = new Window_Command(loc, width, commands);
            Command_Window.set_columns(this.restock_blocked ? 1 : 2);
            Command_Window.glow_width = width - 8;
            Command_Window.glow = true;
            Command_Window.bar_offset = new Vector2(-8, 0);
            Command_Window.text_offset = new Vector2(0, -4);
            Command_Window.size_offset = new Vector2(-8, -8);
            Command_Window.greyed_cursor = type != Supply_Command_Window.All;
            Command_Window.active = type == Supply_Command_Window.All;
            Command_Window.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Item_Options_Window");
            Command_Window.immediate_index = 0;
            Command_Window.color_override = 0;
            Command_Window.stereoscopic = Config.CONVOY_WINDOW_DEPTH;
        }

        protected override void refresh()
        {
            base.refresh();

            // //Yeti
            bool convoy_full = Global.game_battalions.active_convoy_is_full;
            Stock_Value.text = Global.game_battalions.active_convoy_data.Count.ToString();
            Stock_Value.SetColor(Global.Content, convoy_full ? "Green" : "Blue");
            Stock_Max.text = Global.game_battalions.active_convoy_size.ToString();
            Stock_Max.SetColor(Global.Content, convoy_full ? "Green" : "Blue");
        }
        protected override void refresh_item_window()
        {
            base.refresh_item_window();

            Command_Window.set_text_color(0, can_give ? "White" : "Grey");
            if (this.restock_blocked)
            {
                Command_Window.set_text_color(1, can_take ? "White" : "Grey");
            }
            else
            {
                Command_Window.set_text_color(1, can_restock ? "White" : "Grey");
                Command_Window.set_text_color(2, can_take ? "White" : "Grey");
            }
        }
        protected override void new_item_window()
        {
            int index = Item_Window == null ? 0 : Item_Window.index;
            if (Item_Window != null)
                Item_Window.restore_equipped();

            Item_Window = new Window_Command_Item_Supply(
                unit.actor.id, new Vector2(
                    8, Config.WINDOW_HEIGHT - (Global.ActorConfig.NumItems + 2) * 16),
                Restocking);
            if (Item_Window.num_items() > 0)
                Item_Window.immediate_index = index;
            Item_Window.active = Giving || Restocking;
        }

        protected override void UpdateMenu(bool active)
        {
            Command_Window.update(active && !trading && ready);
            base.UpdateMenu(active && this.ready);
            Convoy_Text.update();
        }
        protected override void UpdateItemWindow()
        {
            int item_index = Item_Window.index;
            Item_Window.update(Giving || Restocking);
            if (item_index != Item_Window.index)
            {
                item_window_index_changed();
            }
        }

        protected override void supply_window_index_changed()
        {
            if (!Supply_Window.can_take)
                HelpFooter.refresh(this.unit, null);
            else
                HelpFooter.refresh(
                    this.unit, Supply_Window.active_item.get_item());
        }

        protected override void update_input(bool active)
        {
            if (!trading)
            {
                if (active)
                {
                    bool cancel =
                        CancelButton.consume_trigger(MouseButtons.Left) ||
                        CancelButton.consume_trigger(TouchGestures.Tap) ||
                        Command_Window.is_canceled();

                    if (cancel)
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        close();
                    }
                    else if (Command_Window.is_selected())
                    {
                        trade();
                    }
                    else if (Command_Window.getting_help()) { } //Yeti
                }
            }
            else if (Giving || Restocking)
                update_unit_inventory(active);
            else
                update_taking(active);
        }

        private void update_unit_inventory(bool active)
        {
            bool cancel =
                CancelButton.consume_trigger(MouseButtons.Left) ||
                CancelButton.consume_trigger(TouchGestures.Tap) ||
                Item_Window.is_canceled();

            if (active)
            {
                if (is_help_active)
                {
                    if (cancel)
                        close_help();
                }
                else if (giving)
                {
                    if (cancel)
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        cancel_trading();
                    }
                    else if (Item_Window.is_selected())
                        give();
                    else if (Item_Window.getting_help())
                        open_help();
                }
                else if (restocking)
                {
                    if (cancel)
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        cancel_trading();
                    }
                    else if (Item_Window.is_selected())
                        restock();
                    else if (Item_Window.getting_help())
                        open_help();
                }
            }
        }

        private void update_taking(bool active)
        {
            if (active)
            {
                if (selecting_take)
                {
                    bool cancel =
                        CancelButton.consume_trigger(MouseButtons.Left) ||
                        CancelButton.consume_trigger(TouchGestures.Tap) ||
                        Item_Selection_Window.is_canceled();

                    if (is_help_active)
                    {
                        if (cancel)
                            close_help();
                    }
                    else
                    {
                        if (cancel)
                        {
                            Global.game_system.play_se(System_Sounds.Cancel);
                            cancel_selecting_take();
                        }
                        else if (Item_Selection_Window.is_selected())
                            take();
                        else if (Item_Selection_Window.getting_help())
                            open_help();
                    }
                }
                else if (taking)
                {
                    bool cancel =
                        CancelButton.consume_trigger(MouseButtons.Left) ||
                        CancelButton.consume_trigger(TouchGestures.Tap) ||
                        Supply_Window.is_canceled();

                    if (is_help_active)
                    {
                        if (cancel)
                            close_help();
                    }
                    else
                    {
                        if (cancel)
                        {
                            Global.game_system.play_se(System_Sounds.Cancel);
                            cancel_trading();
                        }
                        else if (Supply_Window.is_selected())
                        {
                            if (Constants.Gameplay.CONVOY_ITEMS_STACK == Convoy_Stack_Types.Full)
                                select_take();
                            else
                                take();
                        }
                        else if (Global.Input.triggered(Inputs.X) &&
                            Constants.Gameplay.CONVOY_ITEMS_STACK == Convoy_Stack_Types.Full)
                        {
                            take();
                        }
                        else if (Supply_Window.getting_help())
                        {
                            open_help();
                        }
                    }
                }
            }
        }

        public void trade()
        {
            Supply_Command_Window selected_option;
            if (Command_Window.selected_index().Index == 0)
                selected_option = Supply_Command_Window.Give;
            else if (Command_Window.selected_index().Index == 1)
            {
                if (this.restock_blocked)
                    selected_option = Supply_Command_Window.Take;
                else
                    selected_option = Supply_Command_Window.Restock;
            }
            else
                selected_option = Supply_Command_Window.Take;

            trade(selected_option);
        }
        private void trade(Supply_Command_Window selectedOption)
        {
            switch (selectedOption)
            {
                case (Supply_Command_Window.Give):
                    if (can_give)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Item_Window.active = true;
                        Item_Window.current_cursor_loc = Command_Window.current_cursor_loc;
                        create_command_window(Supply_Command_Window.Give);
                        Giving = true;
                        item_window_index_changed();
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                case (Supply_Command_Window.Take):
                    if (can_take)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Supply_Window.active = true;
                        Supply_Window.current_cursor_loc =
                            Command_Window.current_cursor_loc +
                            new Vector2(0, 16 * Supply_Window.scroll);
                        create_command_window(Supply_Command_Window.Take);
                        //Supply_Window.refresh_cursor_loc(false); //Debug
                        Taking = true;
                        supply_window_index_changed();
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                case (Supply_Command_Window.Restock):
                    if (can_restock)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Restocking = true;
                        refresh_item_window();
                        Item_Window.active = true;
                        Item_Window.current_cursor_loc = Command_Window.current_cursor_loc;
                        create_command_window(Supply_Command_Window.Restock);
                        item_window_index_changed();
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
            }
        }

        public void cancel_trading()
        {
            create_command_window(Supply_Command_Window.All);
            var command = Restocking ? Supply_Command_Window.Restock :
                (Giving ? Supply_Command_Window.Give : Supply_Command_Window.Take);
            Command_Window.index = (int)command - 1;
            if (Taking)
                Command_Window.current_cursor_loc =
                    Supply_Window.current_cursor_loc -
                    new Vector2(0, 16 * Supply_Window.scroll);
            else
                Command_Window.current_cursor_loc = Item_Window.current_cursor_loc;

            //Item_Window.active = false; // unneeded since this window is about to be replaced //Debug
            Supply_Window.active = false;
            Giving = false;
            Taking = false;
            Restocking = false;

            Item_Window.restore_equipped();
            unit.actor.staff_fix();
            refresh_item_window();
            HelpFooter.refresh(this.unit, null);
        }

        public void give()
        {
            TactileLibrary.Item_Data item_data = actor.items[Item_Window.index];
            if (!actor.can_give(item_data))
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
                return;
            }

            Global.game_battalions.add_item_to_convoy(actor.items[Item_Window.index]);
            bool giving_equipped = Item_Window.index == actor.equipped - 1;
            actor.discard_item(Item_Window.index);

            // If can't give anymore
            if (!can_give)
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                cancel_trading();
            }
            else
                Global.game_system.play_se(System_Sounds.Confirm);
            Traded = true;
            if (Item_Window.index < Item_Window.equipped || Item_Window.equipped == 0)
            {
                actor.setup_items(false);
                Item_Window.refresh_equipped_tag();
            }
            refresh();
            item_window_index_changed();
            // Add jumping to the correct page and probably jumping to the correct line for the item here //Debug?
            Supply_Window.jump_to(item_data);
        }

        public void take()
        {
            if (Item_Selection_Window != null)
            {
                var stacked_items = Supply_Window.active_stacked_items();
                actor.gain_item(stacked_items[Item_Selection_Window.index].acquire_item());
                if (Item_Selection_Window.item_count == 1 || !can_take)
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    cancel_selecting_take();
                    if (!can_take)
                        cancel_trading();
                }
                else
                    Global.game_system.play_se(System_Sounds.Confirm);
                Traded = true;
                // Try to equip
                if (Item_Window.equipped == 0)
                {
                    actor.setup_items(false);
                    Item_Window.refresh_equipped_tag();
                }
                refresh();
                supply_window_index_changed();
            }
            else
            {
                if (Supply_Window.can_take_active_item(this.actor))
                {
                    var item_data = Supply_Window.active_item.acquire_item();
                    actor.gain_item(item_data);
                    if (!can_take)
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        cancel_trading();
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Confirm);
                    Traded = true;
                    // Try to equip
                    if (Item_Window.equipped == 0)
                    {
                        actor.setup_items(false);
                        Item_Window.refresh_equipped_tag();
                    }
                    refresh();
                    supply_window_index_changed();
                }
                else
                    Global.game_system.play_se(System_Sounds.Buzzer);
            }
        }

        public void restock()
        {
            if (actor.restock(Item_Window.index))
            {
                if (!can_restock)
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    cancel_trading();
                }
                else
                    Global.game_system.play_se(System_Sounds.Confirm);
                Traded = true;
                refresh();
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        #region Help
        public override void open_help()
        {
            if (Giving || Restocking)
            {
                Item_Window.open_help();
            }
            else if (Taking)
            {
                if (Item_Selection_Window != null)
                    Item_Selection_Window.open_help();
                else
                    Supply_Window.open_help();
            }
            else { }
        }

        public override void close_help()
        {
            if (Giving || Restocking)
            {
                Item_Window.close_help();
            }
            else if (Taking)
            {
                if (Item_Selection_Window != null)
                    Item_Selection_Window.close_help();
                else
                    Supply_Window.close_help();
            }
            else { }
        }
        #endregion

        protected override void draw_header(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Banner.draw(sprite_batch);
            Convoy_Text.draw(sprite_batch);
            Face_Bg.draw(sprite_batch);
            Convoy_Label.draw(sprite_batch);

            Stock_Banner.draw(sprite_batch);
            Stock_Label.draw(sprite_batch);
            Stock_Max.draw(sprite_batch);
            Stock_Slash.draw(sprite_batch);
            Stock_Value.draw(sprite_batch);
            sprite_batch.End();

            Convoy_Face.draw(sprite_batch);
        }

        protected override void draw_command_windows(SpriteBatch spriteBatch)
        {
            if (Taking)
            {
                Item_Window.draw(spriteBatch);
                Supply_Window.draw(spriteBatch);

                Command_Window.draw(spriteBatch);
            }
            else
            {
                Supply_Window.draw(spriteBatch);
                Item_Window.draw(spriteBatch);

                Command_Window.draw(spriteBatch);
            }
        }
    }
}
