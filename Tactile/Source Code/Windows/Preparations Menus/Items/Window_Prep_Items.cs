using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Graphics.Preparations;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using Tactile.Menus;
using Tactile.Menus.Preparations;
using Tactile.Windows.Command;
using Tactile.Windows.Command.Items;
using Tactile.Windows.UserInterface.Command;
using TactileLibrary;

namespace Tactile
{
    enum PrepItemsInputResults { None, OpenTrade, Status, List, Supply, Shop, Closing }

    internal class Window_Prep_Items : PreparationsBaseMenu
    {
        protected bool Unit_Selected = false;

        private bool Trading = false, Using = false, Promoting = false;
        private int Trading_Actor_Id;
        private Window_Command_Item_Preparations Item_Window_1, Item_Window_2;
        private Sprite Funds_Banner;
        private TextSprite Funds_Label, Funds_Value, Funds_G;

        #region Accessors
        public bool unit_selected { get { return Unit_Selected; } }

        public bool trading { get { return Trading; } }

        public virtual bool is_help_active { get { return (Item_Window_1 != null && Item_Window_1.is_help_active); } }

        protected override bool ready_for_inputs
        {
            get
            {
                if (Using || Promoting)
                    return false;
                return base.ready_for_inputs;
            }
        }

        public int trading_actor_id { get { return Trading_Actor_Id; } }
        #endregion

        public Window_Prep_Items() : this(false) { }
        public Window_Prep_Items(bool returning_to_item_use)
        {
            returning_to_menu(returning_to_item_use);
        }

        protected void returning_to_menu(bool returning_to_item_use)
        {
            if (returning_to_item_use)
            {
                select_unit();
                Black_Screen_Timer = 0;
            }
        }
        
        protected override void InitializeSprites()
        {
            Funds_Banner = new Sprite();
            Funds_Banner.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Windowskins\Preparations_Screen");
            Funds_Banner.loc = new Vector2(216, 172);
            Funds_Banner.src_rect = new Rectangle(0, 64, 104, 24);
            Funds_Banner.offset = new Vector2(0, 1);
            Funds_Banner.stereoscopic = Config.PREPITEM_FUNDS_DEPTH;
            Funds_Label = new TextSprite();
            Funds_Label.loc = Funds_Banner.loc + new Vector2(12, 0);
            Funds_Label.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Funds_Label.text = "Funds";
            Funds_Label.stereoscopic = Config.PREPITEM_FUNDS_DEPTH;
            Funds_Value = new RightAdjustedText();
            Funds_Value.loc = Funds_Banner.loc + new Vector2(92, 0);
            Funds_Value.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Funds_Value.stereoscopic = Config.PREPITEM_FUNDS_DEPTH;
            Funds_G = new TextSprite();
            Funds_G.loc = Funds_Banner.loc + new Vector2(92, 0);
            Funds_G.SetFont(Config.UI_FONT + "L", Global.Content, "Yellow", Config.UI_FONT);
            Funds_G.text = "G";
            Funds_G.stereoscopic = Config.PREPITEM_FUNDS_DEPTH;
            
            base.InitializeSprites();
        }
        
        private Vector2 item_window_1_loc()
        {
            return new Vector2(
                Config.WINDOW_WIDTH / 2 - 144,
                ((Config.WINDOW_HEIGHT / 16) -
                    (Global.ActorConfig.NumItems + 1)) * 16 + 8);
        }

        private Vector2 item_window_2_loc()
        {
            return item_window_1_loc() + new Vector2(144, 0);
        }

        public override void refresh()
        {
            Funds_Value.text = Global.battalion.gold.ToString();
            // Item Windows
            if (!Trading)
            {
                Item_Window_1 = GetItemWindow(item_window_1_loc(), true);
                if (Unit_Selected && !Trading)
                    Item_Window_1.ShowHeader();
                Item_Window_2 = null;
            }
            else
            {
                Item_Window_2 = GetItemWindow(item_window_2_loc(), false);
            }
        }

        private Window_Command_Item_Preparations GetItemWindow(Vector2 loc, bool facingRight, bool usingItem = false)
        {
            var window = new Window_Command_Item_Preparations(
                UnitWindow.actor_id, loc, facingRight, usingItem);
            window.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
            return window;
        }

        public virtual ItemUseMenu GetUseMenu()
        {
            return new ItemUseMenu(UnitWindow.actor_id, item_window_1_loc());
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
        
        public event EventHandler<EventArgs> TradeSelected;
        public event EventHandler<EventArgs> Trade;
        public event EventHandler<EventArgs> Convoy;
        public event EventHandler<EventArgs> Use;
        public event EventHandler<EventArgs> List;
        public event EventHandler<EventArgs> Shop;

        protected void OnTradeSelected(EventArgs e)
        {
            if (TradeSelected != null)
                TradeSelected(this, e);
        }
        protected void OnConvoy(EventArgs e)
        {
            if (Convoy != null)
                Convoy(this, e);
        }
        protected void OnUse(EventArgs e)
        {
            if (Use != null)
                Use(this, e);
        }
        protected void OnList(EventArgs e)
        {
            if (List != null)
                List(this, e);
        }

        protected override void Activate()
        {
            UnitWindow.active = true;
            UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DEPTH;
            base.Activate();
        }

        protected override void Deactivate()
        {
            if (!Trading)
            {
                UnitWindow.active = false;
                UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DIMMED_DEPTH;
            }
            base.Deactivate();
        }

        public void Promote()
        {
            Promoting = true;
            Black_Screen.visible = true;
            Black_Screen.TintA = 255;
        }

        #region Update
        protected override void UpdateMenu(bool active)
        {
            if (Item_Window_1 != null)
                Item_Window_1.update(false);
            if (Item_Window_2 != null)
                Item_Window_2.update(false);

            base.UpdateMenu(active && this.ready_for_inputs);

            if (Promoting)
                Black_Screen.visible = true;
        }
        
        public override void CancelUnitSelecting()
        {
            if (Trading)
                cancel_trade();
            else
                close();
        }
        public override bool SelectUnit(int index)
        {
            if (Trading)
            {
                if (confirm_trade())
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    if (Trade != null)
                        Trade(this, new EventArgs());
                    return true;
                }
                else
                {
                    Global.game_system.play_se(System_Sounds.Buzzer);
                    return false;
                }
            }
            else
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                select_unit();
                return true;
            }
        }

        public virtual ItemsCommandMenu GetCommandMenu()
        {
            return new ItemsCommandMenu(ActorId);
        }

        protected override void UpdateUnitWindow(bool active)
        {
            UnitWindow.update(active && this.ready_for_inputs && (Trading || !Unit_Selected));
        }

        public virtual bool select_unit(bool resetSelect = true)
        {
            Unit_Selected = true;
            if (resetSelect)
            {
                UnitWindow.set_selected_loc();
                UnitWindow.ResetTradeIndex();
            }
            OnUnitSelected(new EventArgs());
            Item_Window_1.ShowHeader();
            return true;
        }

        public virtual void cancel_unit_selection()
        {
            UnitWindow.refresh_scroll(true);
            Item_Window_1.HideHeader();
            Unit_Selected = false;
        }
        
        public virtual void CommandSelection(Maybe<int> selectedIndex)
        {
            switch (selectedIndex)
            {
                // Trade
                case 0:
                    if (Global.battalion.actors.Count <= 1)
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    else
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        OnTradeSelected(new EventArgs());
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
                    if (Global.battalion.has_convoy)
                        give_all();
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                // Use
                case 4:
                    if (CanSelectUse())
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        OnUse(new EventArgs());
                    }
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                // Restock
                case 5:
                    if (Global.battalion.has_convoy)
                        restock();
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                // Optimize
                case 6:
                    if (Global.battalion.has_convoy)
                        optimize();
                    else
                        Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
                // Shop
                case 7:
                    if (Shop != null)
                        Shop(this, new EventArgs());
                    break;
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
            Item_Window_1.HideHeader();

            UnitWindow.stereoscopic = Config.PREPITEM_BATTALION_DEPTH;
            Trading = true;
            Trading_Actor_Id = ActorId;
            UnitWindow.trading = true;
            UnitWindow.active = true;
            refresh();
        }

        public void cancel_trade()
        {
            Trading = false;
            UnitWindow.trading = false;
            select_unit(false);
            refresh();
        }

        private bool confirm_trade()
        {
            // Actor can't trade with themself
            if (ActorId == Trading_Actor_Id)
                return false;
            // If neither actor has items
            else if (Global.game_actors[ActorId].has_no_items &&
                    Global.game_actors[Trading_Actor_Id].has_no_items)
                return false;

            return true;
        }

        // List maybe

        #region Use
        protected bool CanSelectUse()
        {
            return this.actor.can_use_convoy_item() && Use != null;
        }

        public bool can_use()
        {
            TactileLibrary.Data_Equipment item = this.actor.items[Item_Window_1.redirect()].to_equipment;
            if (!item.is_weapon && this.actor.prf_check(item) && Combat.can_use_item(this.actor, item.Id, false))
                return true;
            return false;
        }

        public void StartUse()
        {
            Using = true;
        }
        
        public void cancel_use()
        {
            Using = false;
            refresh();
        }
        #endregion

        // Give All 
        public void give_all()
        {
            Game_Actor actor = this.actor;
            if (!actor.CanGiveAny)
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
            if (this.actor.restock())
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
        
        new public void close()
        {
            _Closing = true;
            Black_Screen_Timer = Black_Screen_Hold_Timer + (Black_Screen_Fade_Timer * 2);
            if (Black_Screen != null)
                Black_Screen.visible = true;
            Global.game_system.Preparations_Actor_Id = ActorId;
        }
        #endregion

        #region Draw
        protected override void draw_window(SpriteBatch sprite_batch)
        {
            UnitWindow.draw(sprite_batch);
            DrawStatsWindow(sprite_batch);

            DrawHeader(sprite_batch);
            
            if (Item_Window_1 != null && !Using)
                Item_Window_1.draw_help(sprite_batch);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (!Trading && !Unit_Selected)
            {
                ChooseUnitWindow.draw(sprite_batch);
                ChooseUnitLabel.draw(sprite_batch, -ChooseUnitWindow.loc);
            }
            sprite_batch.End();
            
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Labels
            if (!Trading)
            {
                if (!Using)
                {
                    Funds_Banner.draw(sprite_batch);
                    Funds_Label.draw(sprite_batch);
                    Funds_G.draw(sprite_batch);
                    Funds_Value.draw(sprite_batch);
                    if (!Unit_Selected)
                        RButton.Draw(sprite_batch);
                }
            }
            // Data
            sprite_batch.End();
        }

        protected override void DrawStatsWindow(SpriteBatch spriteBatch)
        {
            //Item Windows
            if (!Using)
            {
                if (Item_Window_1 != null)
                    Item_Window_1.draw(spriteBatch);
                if (Item_Window_2 != null)
                    Item_Window_2.draw(spriteBatch);
            }
        }
        #endregion
    }
}
