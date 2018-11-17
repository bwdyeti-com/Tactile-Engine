using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Graphics.Preparations;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.Command;
using FEXNA.Windows.Command.Items;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA
{
    enum PrepItemsInputResults { None, OpenTrade, Status, List, Supply, Shop, Closing }

    internal class Window_Prep_Items : Windows.Map.Map_Window_Base
    {
        const int BLACK_SCEEN_FADE_TIMER = 8;
        const int BLACK_SCREEN_HOLD_TIMER = 4;
        const int PROMOTION_TIME = 8;

        protected bool Active = true;
        protected bool Unit_Selected = false;
        protected bool Trading = false, Using = false, Confirming_Use = false, Promoting = false;
        private bool Selecting_Inventory_Target = false;
        protected int Trading_Actor_Id;
        private int Promotion_Timer = 0;
        protected Window_Prep_Items_Unit UnitWindow;
        protected Window_Command_Item_Preparations Item_Window_1, Item_Window_2;
        private Window_Command_Item_Preparations Repair_Item_Window;
        protected Prep_Stats_Window Stats_Window;
        private Sprite Stats_Info_Bg;
        protected WindowPanel Choose_Unit_Window;
        protected Window_Command Command_Window;
        protected Preparations_Confirm_Window Use_Confirm_Window;
        protected Pick_Units_Items_Header Item_Header;
        //private Sprite Funds_Banner, R_Button; //Debug
        private Sprite Funds_Banner;
        private Button_Description R_Button;
        protected FE_Text Funds_Label, Funds_Value, Funds_G, Choose_Unit_Label;
        protected Item_Break_Popup Stats_Popup;

        #region Accessors
        public bool active { set { Active = value; } }

        public bool unit_selected { get { return Unit_Selected; } }

        public bool trading { get { return Trading; } }
        public bool selecting_inventory_target { get { return Selecting_Inventory_Target; } }
        public bool using_item { get { return Using; } }
        public bool confirming_use { get { return Confirming_Use; } }
        public bool promoting { get { return Promoting ; } }

        public virtual bool is_help_active { get { return (Item_Window_1 != null && Item_Window_1.is_help_active) || (Repair_Item_Window != null && Repair_Item_Window.is_help_active); } }

        public bool ready
        {
            get
            {
                if (gaining_stats)
                    return false;
                if (selecting_inventory_target)
                    return Repair_Item_Window.active || confirming_use;
                if (using_item)
                    return Item_Window_1.active || confirming_use;
                return this.ready_for_inputs;
            }
        }
        public bool gaining_stats { get { return (Stats_Window != null && !Stats_Window.is_ready) || (Stats_Popup != null && !Stats_Popup.finished); } }

        public int actor_id
        {
            get
            {
                return UnitWindow.actor_id;
            }
            set
            {
                UnitWindow.actor_id = value;
                UnitWindow.refresh_scroll();
                refresh();
            }
        }
        public Game_Actor actor { get { return Global.game_actors[actor_id]; } }

        public int index { get { return Command_Window.index; } }

        public int trading_actor_id { get { return Trading_Actor_Id; } }
        #endregion

        public Window_Prep_Items() : this(false) { }
        public Window_Prep_Items(bool returning_to_item_use)
        {
            initialize_sprites();
            update_black_screen();
            returning_to_menu(returning_to_item_use);
        }

        protected virtual void returning_to_menu(bool returning_to_item_use)
        {
            if (returning_to_item_use)
            {
                select_unit();
                if (actor.has_items)
                {
                    start_use(true);
                    Item_Window_1.immediate_index = Global.game_temp.preparations_item_index;
                }
                Command_Window.immediate_index = 4;
            }
        }

        protected override void set_black_screen_time()
        {
            Black_Screen_Fade_Timer = BLACK_SCEEN_FADE_TIMER;
            Black_Screen_Hold_Timer = BLACK_SCREEN_HOLD_TIMER;
            base.set_black_screen_time();
        }

        protected virtual void initialize_sprites()
        {
            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(
                @"Graphics\White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 255);
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Pictures\Preparation_Background");
            (Background as Menu_Background).vel = new Vector2(0, -1 / 3f);
            (Background as Menu_Background).tile = new Vector2(1, 2);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
            // Unit Window
            set_unit_window();
            UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DEPTH;
            UnitWindow.IndexChanged += UnitWindow_IndexChanged;
            // Choose Unit Window
            Choose_Unit_Window = new WindowPanel(Global.Content.Load<Texture2D>(
                @"Graphics\Windowskins\Preparations_Item_Options_Window"));
            Choose_Unit_Window.loc = new Vector2(Config.WINDOW_WIDTH - 120, Config.WINDOW_HEIGHT - 80);
            Choose_Unit_Window.width = 80;
            Choose_Unit_Window.height = 40;
            Choose_Unit_Window.stereoscopic = Config.PREPITEM_WINDOW_DEPTH;
            // Command Window
            set_command_window();
            // //Yeti
            Funds_Banner = new Sprite();
            Funds_Banner.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Windowskins\Preparations_Screen");
            Funds_Banner.loc = new Vector2(216, 172);
            Funds_Banner.src_rect = new Rectangle(0, 64, 104, 24);
            Funds_Banner.offset = new Vector2(0, 1);
            Funds_Banner.stereoscopic = Config.PREPITEM_FUNDS_DEPTH;
            Funds_Label = new FE_Text();
            Funds_Label.loc = Funds_Banner.loc + new Vector2(12, 0);
            Funds_Label.Font = "FE7_Text";
            Funds_Label.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Fonts\FE7_Text_Yellow");
            Funds_Label.text = "Funds";
            Funds_Label.stereoscopic = Config.PREPITEM_FUNDS_DEPTH;
            Funds_Value = new FE_Text_Int();
            Funds_Value.loc = Funds_Banner.loc + new Vector2(92, 0);
            Funds_Value.Font = "FE7_Text";
            Funds_Value.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Fonts\FE7_Text_Blue");
            Funds_Value.stereoscopic = Config.PREPITEM_FUNDS_DEPTH;
            Funds_G = new FE_Text();
            Funds_G.loc = Funds_Banner.loc + new Vector2(92, 0);
            Funds_G.Font = "FE7_TextL";
            Funds_G.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Fonts\FE7_Text_Yellow");
            Funds_G.text = "G";
            Funds_G.stereoscopic = Config.PREPITEM_FUNDS_DEPTH;
            Choose_Unit_Label = new FE_Text();
            Choose_Unit_Label.loc = new Vector2(12, 4);
            Choose_Unit_Label.Font = "FE7_Text";
            Choose_Unit_Label.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Fonts\FE7_Text_White");
            set_label("Choose unit");
            Choose_Unit_Label.stereoscopic = Config.PREPITEM_WINDOW_DEPTH;

            refresh_input_help();

            refresh();
        }

        protected void refresh_input_help()
        {
            R_Button = Button_Description.button(Inputs.R,
                Global.Content.Load<Texture2D>(
                    @"Graphics\Windowskins\Preparations_Screen"),
                new Rectangle(126, 122, 24, 16));
            R_Button.loc = Funds_Banner.loc + new Vector2(60, -16);
            R_Button.offset = new Vector2(-1, -1);
            R_Button.stereoscopic = Config.PREPITEM_FUNDS_DEPTH;
        }

        protected void set_label(string str)
        {
            Choose_Unit_Label.text = str;
        }

        protected void label_visible(bool visible)
        {
            Choose_Unit_Window.visible = visible;
            Choose_Unit_Label.visible = visible;
        }

        protected virtual void set_unit_window()
        {
            UnitWindow = new Window_Prep_Items_Unit();
        }

        protected virtual void set_command_window()
        {
            List<string> strs = new List<string> { "Trade", "List", "Convoy", "Give All", "Use", "Restock", "Optimize" };
            if (Global.game_system.home_base || (Global.battalion.convoy_id > -1 && Global.game_battalions.active_convoy_shop != null))
                strs.Add("Shop");
            Command_Window = new Window_Command(new Vector2(Config.WINDOW_WIDTH - 128, Config.WINDOW_HEIGHT - 100), 56, strs);
            if (Global.battalion.actors.Count <= 1)
                Command_Window.set_text_color(0, "Grey");
            if (Global.battalion.actors.Count <= 1 &&
                    Global.battalion.convoy_id == -1)
                Command_Window.set_text_color(1, "Grey");
            if (Global.battalion.convoy_id == -1)
            {
                Command_Window.set_text_color(2, "Grey");
                Command_Window.set_text_color(3, "Grey");
                Command_Window.set_text_color(5, "Grey");
                Command_Window.set_text_color(6, "Grey");
                Command_Window.set_text_color(7, "Grey");
            }
            else if (Global.game_battalions.active_convoy_shop == null)
                Command_Window.set_text_color(7, "Grey");
            Command_Window.size_offset = new Vector2(0, -8);
            Command_Window.text_offset = new Vector2(0, -4);
            Command_Window.glow_width = 56 - 8;
            Command_Window.glow = true;
            Command_Window.bar_offset = new Vector2(-8, 0);
            Command_Window.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Windowskins\Preparations_Item_Options_Window");
            Command_Window.color_override = 0;
            Command_Window.set_columns(2);
            Command_Window.stereoscopic = Config.PREPITEM_WINDOW_DEPTH;
        }

        private Vector2 item_window_1_loc()
        {
            return new Vector2(
                UnitWindow.loc.X + 4,
                ((Config.WINDOW_HEIGHT / 16) -
                    (Constants.Actor.NUM_ITEMS + 1)) * 16 + 8);
        }

        private Vector2 item_window_2_loc()
        {
            return new Vector2(
                UnitWindow.loc.X + 148,
                ((Config.WINDOW_HEIGHT / 16) -
                    (Constants.Actor.NUM_ITEMS + 1)) * 16 + 8);
        }

        public virtual void refresh()
        {
            refresh_convoy_use_color();
            Funds_Value.text = Global.battalion.gold.ToString();
            // Item Windows
            if (!Trading)
            {
                Item_Window_1 = new Window_Command_Item_Preparations(
                    UnitWindow.actor_id, item_window_1_loc(), true);
                Item_Window_1.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
                Item_Window_2 = null;
            }
            else
            {
                Item_Window_2 = new Window_Command_Item_Preparations(
                    UnitWindow.actor_id, item_window_2_loc(), false);
                Item_Window_2.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
            }
        }

        protected virtual void refresh_convoy_use_color()
        {
            if (actor.can_use_convoy_item())
                Command_Window.set_text_color(4, "White");
            else
                Command_Window.set_text_color(4, "Grey");
        }

        public virtual void refresh_trade()
        {
            Item_Window_1 = new Window_Command_Item_Preparations(
                Trading_Actor_Id, item_window_1_loc(), true);
            Item_Window_1.darkened = true;
            Item_Window_1.stereoscopic = Config.PREPITEM_UNIT_DIMMED_DEPTH;
            Item_Window_2 = new Window_Command_Item_Preparations(
                UnitWindow.actor_id, item_window_2_loc(), false);
            Item_Window_2.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
        }

        private void refresh_stats()
        {
            Item_Window_1 = new Window_Command_Item_Preparations(
                UnitWindow.actor_id, item_window_1_loc(), true, true);
            Item_Window_1.stereoscopic = Config.PREPITEM_UNIT_DEPTH;

            create_stats_window();
            Stats_Info_Bg = new Sprite();
            Stats_Info_Bg.texture = Global.Content.Load<Texture2D>(
                @"Graphics\White_Square");
            Stats_Info_Bg.dest_rect = new Rectangle(Config.WINDOW_WIDTH - 160, Config.WINDOW_HEIGHT - 58, 160, 52);
            Stats_Info_Bg.tint = new Color(0f, 0f, 0f, 0.5f);
        }

        protected virtual void create_stats_window()
        {
            Stats_Window = new Prep_Stats_Window(Global.game_map.last_added_unit);
            Stats_Window.loc = new Vector2(Config.WINDOW_WIDTH - 160, Config.WINDOW_HEIGHT - 136);
        }

        public event EventHandler<EventArgs> Status;
        protected void OnStatus(EventArgs e)
        {
            if (Status != null)
                Status(this, e);
        }

        public event EventHandler<EventArgs> Trade;
        public event EventHandler<EventArgs> Convoy;
        public event EventHandler<EventArgs> List;
        public event EventHandler<EventArgs> Shop;

        protected void OnConvoy(EventArgs e)
        {
            if (Convoy != null)
                Convoy(this, e);
        }
        protected void OnList(EventArgs e)
        {
            if (List != null)
                List(this, e);
        }

        #region Update
        protected override void UpdateMenu(bool active)
        {
            update_unit_window(active);

            if (Command_Window != null)
                update_command_window(active);
            // Item window up
            if (Stats_Window != null || Stats_Popup != null)
            {
                if (Stats_Popup != null)
                    Stats_Popup.update();
                if (Stats_Window != null)
                {
                    if (!Stats_Window.is_ready)
                    {
                        Stats_Window.update();
                        if (Stats_Window.is_ready)
                            finish_using_item();
                    }
                    else
                    {
                        Stats_Window.update();
                        if (Stats_Popup != null)
                        {
                            if (Stats_Popup.finished)
                                finish_using_item();
                        }
                        else if (using_item && !ready)
                        {
                            finish_using_item();
                        }
                    }
                }
            }
            if (Item_Window_1 != null)
                Item_Window_1.update(Item_Window_1.active);
            if (Repair_Item_Window != null)
                Repair_Item_Window.update(Repair_Item_Window.active);
            if (Use_Confirm_Window != null)
                Use_Confirm_Window.update();

            if (active && this.using_item && this.gaining_stats)
            {
                if (Global.Input.triggered(Inputs.A) ||
                        Global.Input.triggered(Inputs.B) ||
                        Global.Input.mouse_click(MouseButtons.Left) ||
                        Global.Input.gesture_triggered(TouchGestures.Tap))
                    skip_stat_gain();
            }

            base.UpdateMenu(active && this.ready);

            if (Promoting)
            {
                Black_Screen.visible = true;
                if (Promotion_Timer > 0)
                {
                    Promotion_Timer--;
                    Black_Screen.tint = new Color(0f, 0f, 0f, (PROMOTION_TIME - Promotion_Timer) * (1f / PROMOTION_TIME));
                    if (Promotion_Timer == 0)
                        promote();
                }
            }
            if (Input.ControlSchemeSwitched)
                refresh_input_help();
        }

        private void UnitWindow_IndexChanged(object sender, EventArgs e)
        {
            refresh();
        }

        protected virtual void update_unit_window(bool active)
        {
            UnitWindow.update(active && ready && (Trading || !Unit_Selected));
        }
        protected virtual void update_command_window(bool active)
        {
            Command_Window.update(active && ready && (!Trading && !Using && Unit_Selected));
        }

        public virtual bool select_unit()
        {
            Unit_Selected = true;
            UnitWindow.active = false;
            UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DIMMED_DEPTH;
            Item_Header = new Pick_Units_Items_Header(UnitWindow.actor_id, Item_Window_1.width);
            Item_Header.loc = Item_Window_1.loc - new Vector2(4, 36);
            Item_Header.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
            return true;
        }

        public virtual void cancel_unit_selection()
        {
            Unit_Selected = false;
            UnitWindow.active = true;
            UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DEPTH;
            Item_Header = null;
        }

        // Update input modes
        protected override void update_input(bool active)
        {
            if (!active)
                return;

            #region Trading
            if (this.trading)
            {
                if (Global.Input.triggered(Inputs.B))
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    cancel_trade();
                    //Setup_Window.refresh(); //Debug
                }
                else
                {
                    var selected_index = UnitWindow.consume_triggered(
                        Inputs.A, MouseButtons.Left, TouchGestures.Tap);
                    if (selected_index.IsSomething)
                    {
                        if (confirm_trade())
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            if (Trade != null)
                                Trade(this, new EventArgs());
                        }
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        }
                    }

                    var status_index = UnitWindow.consume_triggered(
                        Inputs.R, MouseButtons.Right, TouchGestures.LongPress);
                    if (status_index.IsSomething)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        OnStatus(new EventArgs());
                    }
                }
            }
            #endregion
            #region Using Item
            else if (this.using_item)
            {
                if (!this.promoting)
                {
                    if (this.is_help_active)
                    {
                        if (Item_Window_1.is_canceled())
                            close_help();
                    }
                    else if (this.confirming_use)
                    {
                        if (Use_Confirm_Window.is_canceled())
                        {
                            cancel_use_confirm();
                        }
                        else if (Use_Confirm_Window.is_selected())
                        {
                            accept_use();
                        }
                    }
                    #region Selecting Item Target
                    else if (this.selecting_inventory_target)
                    {
                        if (Repair_Item_Window.is_canceled())
                        {
                            Global.game_system.play_se(System_Sounds.Cancel);
                            cancel_use();
                        }
                        else if (Repair_Item_Window.is_selected())
                        {
                            if (valid_inventory_target())
                            {
                                Global.game_system.play_se(System_Sounds.Confirm);
                                confirm_use();
                            }
                            else
                                Global.game_system.play_se(System_Sounds.Buzzer);
                        }
                        else if (Repair_Item_Window.getting_help())
                            open_help();
                    }
                    #endregion
                    else
                    {
                        if (Item_Window_1.is_canceled())
                        {
                            Global.game_system.play_se(System_Sounds.Cancel);
                            cancel_use();
                        }
                        else if (Item_Window_1.is_selected())
                        {
                            if (can_use())
                            {
                                Global.game_system.play_se(System_Sounds.Confirm);
                                confirm_use();
                            }
                            else
                                Global.game_system.play_se(System_Sounds.Buzzer);
                        }
                        else if (Item_Window_1.getting_help())
                            open_help();
                    }
                }
            }
            #endregion
            #region Unit Selected
            else if (this.unit_selected)
            {
                update_selected_input();
            }
            #endregion
            else
            {
                update_base_input();
            }
        }

        protected virtual void update_selected_input()
        {
            if (Command_Window.is_canceled())
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                cancel_unit_selection();
            }
            else if (Command_Window.is_selected())
            {
                switch (Command_Window.selected_index())
                {
                    // Trade
                    case 0:
                        if (Global.battalion.actors.Count <= 1)
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            trade();
                        }
                        break;
                    // List
                    case 1:
                        OnList(new EventArgs());
                        break;
                    // Supply
                    case 2:
                        OnConvoy(new EventArgs());
                        break;
                    // Give All
                    case 3:
                        if (Global.battalion.convoy_id > -1)
                            give_all();
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                    // Use
                    case 4:
                        if (actor.can_use_convoy_item())
                        {
                            Global.game_system.play_se(System_Sounds.Confirm);
                            start_use();
                        }
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                    // Restock
                    case 5:
                        if (Global.battalion.convoy_id > -1)
                            restock();
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                    // Optimize
                    case 6:
                        if (Global.battalion.convoy_id > -1)
                            optimize();
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;
                    // Shop
                    case 7:
                        if (Shop != null)
                            Shop(this, new EventArgs());
                        break;
                    /*// Restock All //Debug
                    case 7:
                        if (Global.battalion.convoy_id > -1)
                            Prep_Items_Window.restock_all();
                        else
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        break;*/
                }
            }
            else if (Command_Window.getting_help())
            {
            }
        }

        private void update_base_input()
        {
            // Close this window
            if (Global.Input.triggered(Inputs.B))
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                close();
            }
            
            // Select unit
            var selected_index = UnitWindow.consume_triggered(
                Inputs.A, MouseButtons.Left, TouchGestures.Tap);
            if (selected_index.IsSomething)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                select_unit();
            }

            // Status screen
            var status_index = UnitWindow.consume_triggered(
                Inputs.R, MouseButtons.Right, TouchGestures.LongPress);
            if (status_index.IsSomething)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                OnStatus(new EventArgs());
            }
        }

        // Trade
        public void trade()
        {
            Item_Window_1.darkened = true;
            Item_Window_1.stereoscopic = Config.PREPITEM_UNIT_DIMMED_DEPTH;
            UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DEPTH;
            Trading = true;
            Trading_Actor_Id = actor_id;
            UnitWindow.trading = true;
            UnitWindow.active = true;
            refresh();
            Item_Header = null;
        }

        public void cancel_trade()
        {
            Item_Window_1.darkened = false;
            Item_Window_1.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
            Trading = false;
            UnitWindow.trading = false;
            UnitWindow.active = false;
            UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DIMMED_DEPTH;
            refresh();
            Item_Header = new Pick_Units_Items_Header(UnitWindow.actor_id, Item_Window_1.width);
            Item_Header.loc = Item_Window_1.loc - new Vector2(4, 36);
            Item_Header.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
        }

        private bool confirm_trade()
        {
            if (actor_id == Trading_Actor_Id)
                return false;
            else if (Global.game_actors[actor_id].has_no_items &&
                    Global.game_actors[Trading_Actor_Id].has_no_items)
                return false;
            return true;
        }

        // List maybe

        #region Use
        public bool can_use()
        {
            FEXNA_Library.Data_Equipment item = actor.items[Item_Window_1.redirect()].to_equipment;
            if (!item.is_weapon && actor.prf_check(item) && Combat.can_use_item(actor, item.Id, false))
                return true;
            return false;
        }

        public bool valid_inventory_target()
        {
            return actor.items[Item_Window_1.redirect()].to_item.can_target_item(actor.items[Repair_Item_Window.redirect()]);
        }

        public void start_use()
        {
            start_use(false);
        }
        private void start_use(bool unit_already_added)
        {
            Using = true;
            if (!unit_already_added)
                Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, UnitWindow.actor_id, "");
            refresh_stats();
            Global.game_map.completely_remove_unit(Global.game_map.last_added_unit.id);
            Item_Window_1.active = true;
            if (!unit_already_added)
            {
                Item_Window_1.current_cursor_loc = Command_Window.current_cursor_loc;
            }
        }

        public void cancel_use()
        {
            if (Repair_Item_Window != null)
            {
                Selecting_Inventory_Target = false;
                Item_Window_1.visible = true;
                Item_Window_1.active = true;
                Item_Window_1.current_cursor_loc = Repair_Item_Window.current_cursor_loc;
                Repair_Item_Window = null;
            }
            else
            {
                Item_Window_1.active = false;
                Command_Window.current_cursor_loc = Item_Window_1.current_cursor_loc;
                refresh();
                Using = false;
                Stats_Window = null;
                Stats_Info_Bg = null;
            }
        }

        public void confirm_use()
        {
            if (actor.items[Item_Window_1.redirect()].to_item.targets_inventory() && Repair_Item_Window == null)
            {
                Selecting_Inventory_Target = true;
                Repair_Item_Window = new Window_Command_Item_Preparations_Repair(
                    UnitWindow.actor_id, item_window_1_loc(), true, Item_Window_1.index);
                Repair_Item_Window.active = true;

                Item_Window_1.visible = false;
            }
            else
            {
                Confirming_Use = true;
                Use_Confirm_Window = new Preparations_Confirm_Window();
                Use_Confirm_Window.set_text("Will you really use it?");
                Use_Confirm_Window.add_choice("Yes", new Vector2(24, 12));
                Use_Confirm_Window.add_choice("No", new Vector2(64, 12));
                Use_Confirm_Window.size = new Vector2(136, 40);
                Use_Confirm_Window.loc = new Vector2(Config.WINDOW_WIDTH - 156, Config.WINDOW_HEIGHT - 60);
                Use_Confirm_Window.index = 1;

                if (Selecting_Inventory_Target)
                {
                    Use_Confirm_Window.current_cursor_loc = Repair_Item_Window.current_cursor_loc;
                    Repair_Item_Window.greyed_cursor = true;
                    Repair_Item_Window.active = false;
                }
                else
                {
                    Use_Confirm_Window.current_cursor_loc = Item_Window_1.current_cursor_loc;
                    Item_Window_1.greyed_cursor = true;
                }
            }
            Item_Window_1.active = false;
        }

        public void accept_use()
        {
            if (Use_Confirm_Window.index == 0)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                use_item();
            }
            else
                cancel_use_confirm();
        }

        public void cancel_use_confirm()
        {
            Global.game_system.play_se(System_Sounds.Cancel);
            Confirming_Use = false;
            if (Selecting_Inventory_Target)
            {
                Repair_Item_Window.current_cursor_loc = Use_Confirm_Window.current_cursor_loc;
                Repair_Item_Window.greyed_cursor = false;
            }
            else
            {
                Item_Window_1.current_cursor_loc = Use_Confirm_Window.current_cursor_loc;
                Item_Window_1.greyed_cursor = false;
            }
            Use_Confirm_Window = null;
            if (Selecting_Inventory_Target)
                Repair_Item_Window.active = true;
            else
                Item_Window_1.active = true;
        }

        private void use_item()
        {
            Confirming_Use = false;
            int index = Item_Window_1.index;
            int inventory_target_index = Repair_Item_Window != null ? Repair_Item_Window.redirect() : -1;
            FEXNA_Library.Data_Item item = actor.items[Item_Window_1.redirect()].to_item;

            Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, UnitWindow.actor_id, "");

            // If promoting
            if (actor.promotes_to() != null && item.Promotes.Contains(actor.class_id))
            {
                // Apply any non-promotion effects of the item
                actor.item_effect(item, inventory_target_index);
                actor.recover_hp();
                // Set variables to switch to promoting
                Promotion_Timer = PROMOTION_TIME;
                Promoting = true;
                Use_Confirm_Window.active = false;
                Black_Screen.visible = true;
            }
            else
            {
                Global.game_system.play_se(System_Sounds.Gain);
                Use_Confirm_Window = null;
                Dictionary<FEXNA_Library.Boosts, int> boosts = actor.item_boosts(item);
                // Apply item effect
                actor.item_effect(item, inventory_target_index);
                actor.recover_hp();

                if (item.is_stat_booster() || item.is_growth_booster())
                //if (boosts.Keys.Count > 0) //Debug
                {
                    create_stats_window();
                    Stats_Window.gain_stats(boosts);

                    Stats_Popup = new Stat_Boost_Popup(
                        item.Id, !item.is_weapon, Global.game_map.last_added_unit, -1, true);
                    Stats_Popup.loc = new Vector2((Config.WINDOW_WIDTH - 80) - Stats_Popup.width / 2, Config.WINDOW_HEIGHT - 56);
                    Stats_Popup.stereoscopic = Config.PREPITEM_WINDOW_DEPTH;
                }
                else if (item.can_repair)
                {
                    // Maybe make a sparkle on the use count for the repaired item, and update its value??? //Yeti
                    Stats_Popup = new Item_Repair_Popup(actor.items[inventory_target_index].Id, actor.items[inventory_target_index].is_item, 128);
                    Stats_Popup.loc = new Vector2((Config.WINDOW_WIDTH - 80) - Stats_Popup.width / 2, Config.WINDOW_HEIGHT - 56);
                    Stats_Popup.stereoscopic = Config.PREPITEM_WINDOW_DEPTH;
                }
            }
        }

        private void promote()
        {
            Global.game_system.Preparations_Actor_Id = actor_id;

            Global.game_state.call_item(Global.game_map.last_added_unit.id, Item_Window_1.redirect());
            Global.game_temp.preparations_item_index = Item_Window_1.redirect();
        }

        public void skip_stat_gain()
        {
            if (Stats_Window != null)
                Stats_Window.cancel_stats_gain();
            finish_using_item();
        }

        private void finish_using_item()
        {
            Stats_Popup = null;
            Item_Window_1.greyed_cursor = false;
            if (!Promoting)
                actor.use_item(Item_Window_1.redirect());

            if (Selecting_Inventory_Target)
            {
                Selecting_Inventory_Target = false;
                Repair_Item_Window = null;
                Item_Window_1.visible = true;
            }
            if (actor.has_items)
            {
                int item_index = Item_Window_1.index;
                refresh_stats();
                if (!Promoting)
                    Global.game_map.completely_remove_unit(Global.game_map.last_added_unit.id);
                Item_Window_1.active = true;

                Item_Window_1.immediate_index = item_index;
            }
            else
            {
                Item_Window_1.active = false;
                refresh();
                Using = false;
                Stats_Window = null;
                Stats_Info_Bg = null;

            }
            Item_Header = new Pick_Units_Items_Header(UnitWindow.actor_id, Item_Window_1.width);
            Item_Header.loc = Item_Window_1.loc - new Vector2(4, 36);
            Item_Header.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
        }
        #endregion

        // Give All 
        public void give_all()
        {
            if (Global.battalion.is_convoy_full || actor.whole_inventory
                .All(x => x.blank_item || !actor.can_give(x)))
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
                return;
            }
            for (int i = actor.num_items - 1; !Global.battalion.is_convoy_full && i >= 0; i--)
            {
                if (!actor.whole_inventory[i].blank_item &&
                    actor.can_give(actor.whole_inventory[i]))
                {
                    Global.game_battalions.add_item_to_convoy(actor.items[i]);
                    actor.discard_item(i);
                }
            }

            Global.game_system.play_se(System_Sounds.Confirm);
            refresh();
        }

        // Restock
        public void restock()
        {
            if (actor.restock())
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                refresh();
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        // Shop?

        // Restock All
        public void restock_all()
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            for (int i = 0; i < Global.battalion.actors.Count; i++)
                Global.game_actors[Global.battalion.actors[i]].restock();
            refresh();
        }

        // Optimize
        public void optimize()
        {
            Global.battalion.optimize_inventory(this.actor);

            Global.game_system.play_se(System_Sounds.Confirm);
            refresh();
        }

        public void open_help()
        {
            if (Repair_Item_Window != null)
                Repair_Item_Window.open_help();
            else
                Item_Window_1.open_help();
        }

        public void close_help()
        {
            if (Repair_Item_Window != null)
                Repair_Item_Window.close_help();
            else
                Item_Window_1.close_help();
        }

        new public void close()
        {
            _Closing = true;
            Black_Screen_Timer = Black_Screen_Hold_Timer + (Black_Screen_Fade_Timer * 2);
            if (Black_Screen != null)
                Black_Screen.visible = true;
            Global.game_system.Preparations_Actor_Id = actor_id;
        }
        #endregion

        #region Draw
        protected override void draw_window(SpriteBatch sprite_batch)
        {
            UnitWindow.draw(sprite_batch);

            if (Using && Stats_Window != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                Stats_Info_Bg.draw(sprite_batch);
                sprite_batch.End();
            }
            //Item Windows
            if (Item_Window_1 != null)
                Item_Window_1.draw(sprite_batch);
            if (Repair_Item_Window != null)
                Repair_Item_Window.draw(sprite_batch);
            else// if (Stats_Window != null) //Debug
            {
                draw_stats_window(sprite_batch);
            }
            if (Item_Window_2 != null)
                Item_Window_2.draw(sprite_batch);
            draw_header(sprite_batch);

            if (Using && Stats_Window != null)
            {
                draw_stats_window(sprite_batch);
            }
            if (Item_Window_1 != null)
                Item_Window_1.draw_help(sprite_batch);
            if (Repair_Item_Window != null)
                Repair_Item_Window.draw_help(sprite_batch);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (!Trading && !Unit_Selected)
            {
                Choose_Unit_Window.draw(sprite_batch);
                Choose_Unit_Label.draw(sprite_batch, -Choose_Unit_Window.loc);
            }
            sprite_batch.End();

            if (Command_Window != null)
                if (!Trading && !Using && Unit_Selected)
                    Command_Window.draw(sprite_batch);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Labels
            if (!Trading)
            {
                if (!labels_visible())
                {
                    if (!is_help_active)
                        R_Button.Draw(sprite_batch, -new Vector2(0, 20));
                }
                else
                {
                    Funds_Banner.draw(sprite_batch);
                    Funds_Label.draw(sprite_batch);
                    Funds_G.draw(sprite_batch);
                    Funds_Value.draw(sprite_batch);
                    R_Button.Draw(sprite_batch);
                }
            }
            // Data
            sprite_batch.End();

            draw_confirm_window(sprite_batch);
        }

        protected virtual void draw_confirm_window(SpriteBatch sprite_batch)
        {
            if (Use_Confirm_Window != null)
                Use_Confirm_Window.draw(sprite_batch);
        }

        protected virtual bool labels_visible()
        {
            return !(Using || Stats_Window != null);
        }

        protected virtual void draw_stats_window(SpriteBatch sprite_batch)
        {
            if (Stats_Window != null)
            {
                if (Stats_Popup != null)
                    Stats_Popup.draw(sprite_batch);

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Stats_Window.draw(sprite_batch);
                sprite_batch.End();
            }
        }

        protected virtual void draw_header(SpriteBatch sprite_batch)
        {
            if (Item_Header != null)
                Item_Header.draw(sprite_batch);
        }
        #endregion
    }
}
