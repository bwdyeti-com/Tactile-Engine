using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows.Command;
using Tactile.Windows.Command.Items;
using Tactile.Graphics.Help;
using Tactile.Graphics.Text;

namespace Tactile.Windows.Map.Items
{
    class WindowItemList : WindowItemManagement
    {
        private bool Giving = false;
        private bool HardSwitch = false;
        //protected Window_Command Command_Window; //Debug
        private Face_Sprite Face;
        private Sprite Nameplate;
        private TextSprite Name;
        private TextSprite Owner;
        private Button_Description SwitchButton;

        #region Accessors
        public bool giving { get { return Giving; } }
        public bool taking { get { return !Giving; } }
        public bool selecting_take { get { return Item_Selection_Window != null; } }

        public bool can_give
        {
            get
            {
                return actor.num_items > 0 &&
                    Global.battalion.has_convoy &&
                    !Global.battalion.is_convoy_full;
            }
        }
        public bool can_take
        {
            get
            {
                return !actor.is_full_items;
            }
        }
        #endregion

        public WindowItemList(int actorId)
            : base(actorId)
        {
            Supply_Window.active = true;
            supply_window_index_changed();
        }

        protected override void initialize_sprites()
        {
            base.initialize_sprites();

            // Face
            Face = new Face_Sprite(this.actor.face_name, true);
            if (this.actor.generic_face)
                Face.recolor_country(this.actor.name_full);
            Face.expression = Face.status_frame;
            Face.phase_in();
            Face.loc = this.item_window_loc + new Vector2(144 / 2, 12 +
                (int)Math.Max(0, ((Face.src_rect.Height - Face.eyes_offset.Y) - 48) * 0.75f));
            Face.stereoscopic = Config.PREPTRADE_NAMES_DEPTH;
            Face.mirrored = true;

            Nameplate = new Sprite();
            Nameplate.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
            Nameplate.loc = new Vector2(0, 0);
            Nameplate.src_rect = new Rectangle(0, 40, 56, 24);
            Nameplate.offset = new Vector2(56, 1);
            Nameplate.tint = new Color(224, 224, 224, 192);
            Nameplate.mirrored = true;
            Nameplate.stereoscopic = Config.PREPTRADE_NAMES_DEPTH;
            Name = new TextSprite();
            Name.loc = Nameplate.loc + new Vector2(24, 0);
            Name.SetFont(Config.UI_FONT, Global.Content, "White");
            Name.text = this.actor.name;
            Name.offset = new Vector2(Font_Data.text_width(Name.text) / 2, 0);
            Name.stereoscopic = Config.PREPTRADE_NAMES_DEPTH;

            Owner = new TextSprite();
            Owner.loc = Stock_Banner.loc + new Vector2(8, 0);
            Owner.SetFont(Config.UI_FONT);
            Owner.SetTextFontColor(0, "White");
            Owner.text = "Owner: ";
            Owner.stereoscopic = Config.PREPTRADE_NAMES_DEPTH;
        }

        protected override void refresh_input_help()
        {
            base.refresh_input_help();
            SwitchButton = Button_Description.button(Inputs.X, new Vector2(176, 8));
            SwitchButton.description = "Switch";
            SwitchButton.stereoscopic = Config.CONVOY_INPUTHELP_DEPTH;
        }

        protected override void initialize_supply_window()
        {
            Supply_Window = new Window_Supply_Items_List(
                actor.id, new Vector2(Config.WINDOW_WIDTH - 152, 24));
        }

        protected override void supply_window_index_changed()
        {
            if (!Supply_Window.can_take)
            {
                HelpFooter.refresh(this.unit, null);

                Owner.text = "Owner: ---";
                Owner.SetTextFontColor(7, "Grey");
            }
            else
            {
                HelpFooter.refresh(
                    this.unit, Supply_Window.active_item.get_item());

                string name;
                if (Supply_Window.active_item.Convoy)
                {
                    name = "Convoy";
                    Owner.SetTextFontColor(7, "Yellow");
                }
                else
                {
                    name = Global.game_actors[Supply_Window.active_item.ActorId].name;
                    Owner.SetTextFontColor(7, "White");
                }
                Owner.text = string.Format("Owner: {0}", name);
            }
        }

        protected override void UpdateMenu(bool active)
        {
            SwitchButton.Update(active);
            base.UpdateMenu(active && this.ready);
            Face.update();
        }

        protected override void update_ui(bool active)
        {
            if (active)
            {
                if (Global.Input.triggered(Inputs.X) ||
                    SwitchButton.consume_trigger(MouseButtons.Left) ||
                    SwitchButton.consume_trigger(TouchGestures.Tap))
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move2);
                    switch_giving();
                    HardSwitch = this.giving;
                    Supply_Window.show_type_icon();
                    return;
                }
            }

            if (active && this.taking && this.can_give)
            {
                // Pressed left on the first weapon type
                // Press right on the last weapon type
                if (Global.game_system.Supply_Item_Type == 0 &&
                    Global.Input.repeated(Inputs.Left))
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move2);
                    Supply_Window.change_page(0);
                    switch_giving();
                    Supply_Window.hide_type_icon();
                    return;
                }
                else if (Global.game_system.Supply_Item_Type ==
                        Supply_Window.supply_type_count - 1 &&
                        Global.Input.repeated(Inputs.Right))
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move2);
                    Supply_Window.change_page(0);
                    switch_giving();
                    Supply_Window.hide_type_icon();
                    return;
                }
            }
            int item_index = Item_Window.index;
            base.update_ui(active);
            if (this.giving && item_index == Item_Window.index)
            {
                if (Global.Input.repeated(Inputs.Left))
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move2);
                    if (!HardSwitch)
                        Supply_Window.change_page(Supply_Window.supply_type_count - 1);
                    switch_giving();
                }
                else if (Global.Input.repeated(Inputs.Right))
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move2);
                    if (!HardSwitch)
                        Supply_Window.change_page(0);
                    switch_giving();
                }
            }
        }

        protected override void update_input(bool active)
        {
            if (Giving)
                update_unit_inventory(active);
            else
                update_taking(active);
        }

        private void update_unit_inventory(bool active)
        {
            if (active)
            {
                if (is_help_active)
                {
                    if (Item_Window.is_canceled())
                        close_help();
                }
                else if (giving)
                {
                    if (Item_Window.is_canceled())
                    {
                        Global.game_system.play_se(System_Sounds.Cancel);
                        close();
                    }
                    else if (Item_Window.is_selected())
                        give();
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
                    if (is_help_active)
                    {
                        if (Item_Selection_Window.is_canceled())
                            close_help();
                    }
                    else
                    {
                        if (Item_Selection_Window.is_canceled())
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
                    if (is_help_active)
                    {
                        if (Supply_Window.is_canceled())
                            close_help();
                    }
                    else
                    {
                        if (Supply_Window.is_canceled())
                        {
                            Global.game_system.play_se(System_Sounds.Cancel);
                            close();
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

        public void switch_giving()
        {
            if (Giving)
            {
                Supply_Window.active = true;
                Item_Window.active = false;
                Giving = false;
                supply_window_index_changed();
            }
            else
            {
                if (can_give)
                {
                    Item_Window.active = true;
                    Supply_Window.active = false;
                    Giving = true;
                    item_window_index_changed();
                }
                else
                    Global.game_system.play_se(System_Sounds.Buzzer);
            }
            HardSwitch = false;
        }

        public void cancel_trading()
        {
            throw new NotImplementedException();
            /*
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

            unit.equip(Item_Window.equipped);
            unit.actor.staff_fix();
            refresh_item_window();
            HelpFooter.refresh(this.unit, null);*/
        }

        public void give()
        {
            TactileLibrary.Item_Data item_data = actor.items[Item_Window.index];
            if (!can_give || !actor.can_give(item_data))
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
                return;
            }

            Global.game_battalions.add_item_to_convoy(actor.items[Item_Window.index]);
            actor.discard_item(Item_Window.index);
            if (can_give)
                Global.game_system.play_se(System_Sounds.Confirm);
            else
                Global.game_system.play_se(System_Sounds.Cancel);
            Traded = true;
            refresh();
            item_window_index_changed();
            // Add jumping to the correct page and probably jumping to the correct line for the item here //Debug?
            Supply_Window.jump_to(item_data);
            if (can_give)
                Item_Window.active = true;
            else
                switch_giving();
        }

        public void take()
        {
            if (!can_take)
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
                return;
            }

            if (Item_Selection_Window != null)
            {
                var stacked_items = Supply_Window.active_stacked_items();
                actor.gain_item(stacked_items[Item_Selection_Window.index].acquire_item());
                if (Item_Selection_Window.item_count == 1)
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    cancel_selecting_take();
                }
                else
                    Global.game_system.play_se(System_Sounds.Confirm);
                Traded = true;
                refresh();
                supply_window_index_changed();
            }
            else
            {
                if (Supply_Window.can_take_active_item(this.actor))
                {
                    var item_data = Supply_Window.active_item.acquire_item();
                    actor.gain_item(item_data);
                    Global.game_system.play_se(System_Sounds.Confirm);
                    Traded = true;
                    refresh();
                    supply_window_index_changed();
                }
                else
                    Global.game_system.play_se(System_Sounds.Buzzer);
            }
        }

        #region Help
        public override void open_help()
        {
            if (Giving)
            {
                Item_Window.open_help();
            }
            else
            {
                if (Item_Selection_Window != null)
                    Item_Selection_Window.open_help();
                else
                    Supply_Window.open_help();
            }
        }

        public override void close_help()
        {
            if (Giving)
            {
                Item_Window.close_help();
            }
            else
            {
                if (Item_Selection_Window != null)
                    Item_Selection_Window.close_help();
                else
                    Supply_Window.close_help();
            }
        }
        #endregion

        protected override void draw_header(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Nameplate.draw(sprite_batch);
            Name.draw(sprite_batch);

            Stock_Banner.draw(sprite_batch);
            Owner.draw_multicolored(sprite_batch);
            SwitchButton.Draw(sprite_batch);
            sprite_batch.End();

            Face.draw(sprite_batch);
        }

        protected override void draw_command_windows(SpriteBatch spriteBatch)
        {
            if (!Giving)
            {
                Item_Window.draw(spriteBatch);
                Supply_Window.draw(spriteBatch);
            }
            else
            {
                Supply_Window.draw(spriteBatch);
                Item_Window.draw(spriteBatch);
            }
        }
    }
}
