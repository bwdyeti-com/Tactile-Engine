using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FEXNA.Graphics.Help;
using FEXNA.Graphics.Preparations;
using FEXNA.Windows.Command.Items;
using Microsoft.Xna.Framework.Graphics;
using FEXNA_Library;

namespace FEXNA.Menus.Preparations
{
    class ItemUseMenu : Menus.Map.Unit.Item.ItemMenu
    {
        private int ActorId;
        private bool UsingItem;
        protected Prep_Stats_Window Stats_Window;
        private Item_Break_Popup Stats_Popup;
        private Sprite Stats_Info_Bg;
        private Button_Description RButton;

        public ItemUseMenu(int actorId, Vector2 loc, IHasCancelButton menu = null) : base(null, menu)
        {
            ActorId = actorId;

            CreateItemWindow(actorId, loc);
            CreateStatsWindow();

            Stats_Info_Bg = new Sprite();
            Stats_Info_Bg.texture = Global.Content.Load<Texture2D>(
                @"Graphics\White_Square");
            Stats_Info_Bg.dest_rect = new Rectangle(Config.WINDOW_WIDTH - 160, Config.WINDOW_HEIGHT - 58, 160, 52);
            Stats_Info_Bg.tint = new Color(0f, 0f, 0f, 0.5f);

            RButton = Button_Description.button(Inputs.R,
                Global.Content.Load<Texture2D>(
                    @"Graphics\Windowskins\Preparations_Screen"),
                new Rectangle(126, 122, 24, 16));
            RButton.loc = new Vector2(216, 172) + new Vector2(60, -16);
            RButton.offset = new Vector2(-1, -1);
            RButton.stereoscopic = Config.PREPITEM_FUNDS_DEPTH;
        }

        private void CreateItemWindow(int actorId, Vector2 loc)
        {
            var window = new Window_Command_Item_Preparations(
                actorId, loc, true, true);
            window.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
            window.active = true;
            window.ShowHeader();

            Window = window;
        }
        private void CreateStatsWindow(bool createPopup = false, Data_Item item = null)
        {
            // Create a unit for the stats window
            Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, ActorId, "");

            Stats_Window = new Prep_Stats_Window(Global.game_map.last_added_unit);
            Stats_Window.loc = new Vector2(Config.WINDOW_WIDTH - 160, Config.WINDOW_HEIGHT - 136);

            if (createPopup)
            {
                Stats_Popup = new Stat_Boost_Popup(
                    item.Id, !item.is_weapon, Global.game_map.last_added_unit, -1, true);
                Stats_Popup.loc = new Vector2(
                    (Config.WINDOW_WIDTH - 80) - Stats_Popup.width / 2,
                    Config.WINDOW_HEIGHT - 56);
                Stats_Popup.stereoscopic = Config.PREPITEM_WINDOW_DEPTH;
            }

            Global.game_map.completely_remove_unit(Global.game_map.last_added_unit.id);
        }

        public Game_Actor Actor { get { return Global.game_actors[ActorId]; } }

        public Window_Command_Item_Preparations_Repair GetRepairWindow()
        {
            var window = new Window_Command_Item_Preparations_Repair(
                ActorId, Window.loc, true, Window.index);
            window.active = true;
            window.ShowHeader();
            return window;
        }

        public void HideWindow()
        {
            Window.visible = false;
        }
        public void ShowWindow()
        {
            Window.visible = true;
        }

        protected override void UpdateMenu(bool active)
        {
            if (Stats_Popup != null)
                Stats_Popup.update();

            if (!Stats_Window.is_ready)
            {
                Stats_Window.update();
                if (Stats_Window.is_ready)
                    FinishUsingItem();
            }
            else
            {
                Stats_Window.update();
                if (Stats_Popup != null)
                {
                    if (Stats_Popup.finished)
                        FinishUsingItem();
                }
                else if (UsingItem)
                {
                    FinishUsingItem();
                }
            }

            if (active && UsingItem && !Stats_Window.is_ready || Stats_Popup != null)
            {
                if (Global.Input.triggered(Inputs.A) ||
                        Global.Input.triggered(Inputs.B) ||
                        Global.Input.mouse_click(MouseButtons.Left) ||
                        Global.Input.gesture_triggered(TouchGestures.Tap))
                {
                    Stats_Window.cancel_stats_gain();
                    FinishUsingItem();
                }
            }

            base.UpdateMenu(active && Stats_Popup == null);
        }
        
        protected override void Activate()
        {
            if (!UsingItem)
            {
                Window.greyed_cursor = false;
                Window.active = true;
            }
            base.Activate();
        }
        protected override void Deactivate()
        {
            Window.greyed_cursor = true;
            Window.active = false;
            base.Deactivate();
        }

        public override bool HidesParent { get { return false; } }

        protected override void SelectItem(bool playConfirmSound = false)
        {
            if (CanUse())
            {
                base.SelectItem(playConfirmSound);
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        protected override void Cancel()
        {
            var itemWindow = Window as Window_Command_Item;

            Global.game_system.play_se(System_Sounds.Cancel);
            itemWindow.restore_equipped();
            OnCanceled(new EventArgs());
        }

        #region Item Use
        public bool CanUse()
        {
            FEXNA_Library.Data_Equipment item = this.SelectedItemData.to_equipment;
            if (!item.is_weapon && this.Actor.prf_check(item) && Combat.can_use_item(this.Actor, item.Id, false))
                return true;
            return false;
        }

        public void UseItem()
        {
            UsingItem = true;
            Deactivate();
        }

        public void CreateStatsPopup(Data_Item item, Dictionary<Boosts, int> boosts)
        {
            CreateStatsWindow(true, item);
            Stats_Window.gain_stats(boosts);
        }

        public void CreateRepairPopup(int inventoryTarget)
        {
            // Maybe make a sparkle on the use count for the repaired item, and update its value??? //Yeti
            Stats_Popup = new Item_Repair_Popup(
                this.Actor.items[inventoryTarget].Id,
                this.Actor.items[inventoryTarget].is_item,
                128);
            Stats_Popup.loc = new Vector2((Config.WINDOW_WIDTH - 80) - Stats_Popup.width / 2, Config.WINDOW_HEIGHT - 56);
            Stats_Popup.stereoscopic = Config.PREPITEM_WINDOW_DEPTH;
        }

        private void FinishUsingItem()
        {
            Stats_Popup = null;
            this.Actor.use_item((Window as Window_Command_Item).redirect());
            
            if (this.Actor.has_items)
            {
                int index = Window.index;
                CreateItemWindow(ActorId, Window.loc);
                CreateStatsWindow();
                
                Window.immediate_index = index;

                UsingItem = false;
                Activate();
            }
            else
            {
                OnCanceled(new EventArgs());
            }
        }

        public ScreenFadeMenu PromotionScreenFade(Maybe<int> promotionId = default(Maybe<int>))
        {
            ScreenFadeMenu result = new PromotionFadeMenu(
                ActorId, this.SelectedItem, promotionId);
            return result;
        }
        #endregion

        public override void Draw(SpriteBatch spriteBatch)
        {
            var itemWindow = Window as Window_Command_Item_Preparations;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Stats_Info_Bg.draw(spriteBatch);
            spriteBatch.End();

            base.Draw(spriteBatch);

            if (Stats_Popup != null)
                Stats_Popup.draw(spriteBatch);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Stats_Window.draw(spriteBatch);
            if (!Window.is_help_active)
                RButton.Draw(spriteBatch, -new Vector2(0, 20));
            spriteBatch.End();

            itemWindow.draw_help(spriteBatch);
        }
    }
}
