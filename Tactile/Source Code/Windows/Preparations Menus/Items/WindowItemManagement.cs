using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Graphics.Preparations;
using Tactile.Graphics.Text;
using Tactile.Windows.Command;
using Tactile.Windows.Command.Items;

namespace Tactile.Windows.Map.Items
{
    abstract class WindowItemManagement : Windows.Map.Map_Window_Base
    {
        const int BLACK_SCEEN_FADE_TIMER = 8;
        const int BLACK_SCREEN_HOLD_TIMER = 4;
        protected int Unit_Id;
        protected bool Is_Actor = false;
        protected bool Traded = false;
        protected Window_Command_Item_Supply Item_Window;
        protected Window_Supply_Items Supply_Window;
        protected Window_Command_Item_Convoy_Take Item_Selection_Window;
        protected Sprite Stock_Banner;
        protected Button_Description R_Button;

        protected Prep_Items_Help_Footer HelpFooter;

        #region Accessors
        protected Game_Unit unit { get { return Global.game_map.units[Unit_Id]; } }
        protected Game_Actor actor { get { return Global.game_actors[unit.actor.id]; } }

        public bool traded { get { return Traded; } }

        public bool ready { get { return Supply_Window.ready && this.ready_for_inputs; } }

        public bool is_help_active { get { return Item_Window.is_help_active || Supply_Window.is_help_active ||
            (Item_Selection_Window != null && Item_Selection_Window.is_help_active); } }

        protected Vector2 item_window_loc
        {
            get
            {
                return new Vector2(
                    8, Config.WINDOW_HEIGHT - (Global.ActorConfig.NumItems + 2) * 16);
            }
        }
        #endregion

        public WindowItemManagement(int actor_id)
        {
            Is_Actor = true;
            Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP,
                actor_id, "");
            Unit_Id = Global.game_map.last_added_unit.id;

            initialize_sprites();
            refresh_input_help();
            refresh();

            update_black_screen();
        }
        public WindowItemManagement(Game_Unit unit)
        {
            Unit_Id = unit.id;

            initialize_sprites();
            refresh_input_help();
            refresh();

            update_black_screen();
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
            Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 255);
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Preparation_Background");
            (Background as Menu_Background).vel = new Vector2(0, -1 / 3f);
            (Background as Menu_Background).tile = new Vector2(1, 2);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
            // Supply Window
            initialize_supply_window();
            Supply_Window.manual_cursor_draw = true;
            Supply_Window.stereoscopic = Config.CONVOY_SUPPLY_DEPTH;
            Supply_Window.arrow_stereoscopic = Config.CONVOY_ARROWS_DEPTH;
            Supply_Window.help_stereoscopic = Config.CONVOY_HELP_DEPTH;

            Stock_Banner = new Sprite();
            Stock_Banner.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
            Stock_Banner.loc = new Vector2(232, 8);
            Stock_Banner.src_rect = new Rectangle(0, 64, 88, 24);
            Stock_Banner.offset = new Vector2(0, 2);
            Stock_Banner.stereoscopic = Config.CONVOY_STOCK_DEPTH;

            HelpFooter = new Prep_Items_Help_Footer();
            HelpFooter.loc = new Vector2(0, Config.WINDOW_HEIGHT - 18);
            HelpFooter.stereoscopic = Config.CONVOY_INPUTHELP_DEPTH + 1;
        }

        protected virtual void initialize_supply_window()
        {
            Supply_Window = new Window_Supply_Items(
                actor.id, new Vector2(Config.WINDOW_WIDTH - 152, 24));
        }

        protected virtual void refresh_input_help()
        {
            /*R_Button = new Sprite();
            R_Button.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
            R_Button.loc = new Vector2(280, 176);
            R_Button.src_rect = new Rectangle(104, 120, 40, 16);
            R_Button.stereoscopic = Config.CONVOY_INPUTHELP_DEPTH;*/
            R_Button = Button_Description.button(Inputs.R,
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen"), new Rectangle(126, 122, 24, 16));
            R_Button.loc = new Vector2(276, 176);
            R_Button.offset = new Vector2(0, -2);
            R_Button.stereoscopic = Config.CONVOY_INPUTHELP_DEPTH;
        }

        protected virtual void refresh()
        {
            // Item Window
            refresh_item_window();

            Supply_Window.refresh();
            if (Item_Selection_Window != null)
                refresh_select_take();
        }
        protected virtual void refresh_item_window()
        {
            new_item_window();
            Item_Window.glow = true;
            Item_Window.manual_cursor_draw = true;
            Item_Window.manual_help_draw = true;
            Item_Window.stereoscopic = Config.CONVOY_INVENTORY_DEPTH;
            Item_Window.help_stereoscopic = Config.CONVOY_HELP_DEPTH;
        }
        protected virtual void new_item_window()
        {
            int index = Item_Window == null ? 0 : Item_Window.index;
            Item_Window = new Window_Command_Item_Supply(
                unit.actor.id, this.item_window_loc, false);
            if (Item_Window.num_items() > 0)
                Item_Window.immediate_index = index;
            Item_Window.active = false;
        }

        protected void refresh_select_take()
        {
            var stacked_items = Supply_Window.active_stacked_items();
            Item_Selection_Window.set_item_data(stacked_items
                .Select(x => x.get_item())
                .ToList());
        }

        #region Update
        protected override void UpdateMenu(bool active)
        {
            update_ui(active && this.ready);

            if (Item_Selection_Window != null)
                Item_Selection_Window.update();
            HelpFooter.update();

            base.UpdateMenu(active);
        }

        protected override void UpdateAncillary()
        {
            if (Input.ControlSchemeSwitched)
                refresh_input_help();
        }

        protected virtual void update_ui(bool active)
        {
            update_item_window();

            var supply = Supply_Window.can_take ?
                Supply_Window.active_item.get_item() : null;
            Supply_Window.update(active);
            if (supply != (Supply_Window.can_take ? Supply_Window.active_item.get_item() : null))
            {
                supply_window_index_changed();
            }
        }

        protected virtual void update_item_window()
        {
            int item_index = Item_Window.index;
            Item_Window.update();
            if (item_index != Item_Window.index)
            {
                item_window_index_changed();
            }
        }

        protected void item_window_index_changed()
        {
            if (this.unit.actor.has_no_items)
                HelpFooter.refresh(this.unit, null);
            else
                HelpFooter.refresh(this.unit, this.unit.actor.items[Item_Window.index]);
        }

        protected abstract void supply_window_index_changed();

        public void select_take()
        {
            if (Supply_Window.can_take_active_item(this.actor))
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Item_Selection_Window = new Window_Command_Item_Convoy_Take(
                    Unit_Id, Supply_Window.loc + new Vector2(8, 20));
                Item_Selection_Window.stereoscopic = Config.CONVOY_SELECTION_DEPTH;
                refresh_select_take();
                Supply_Window.active = false;
            }
            else
                Global.game_system.play_se(System_Sounds.Buzzer);
        }

        public void cancel_selecting_take()
        {
            Item_Selection_Window = null;
            Supply_Window.active = true;
        }

        new public void close()
        {
            Item_Window.restore_equipped();
            unit.actor.staff_fix();

            OnClosing(new EventArgs());

            Item_Window.active = false;
            Supply_Window.active = false;

            _Closing = true;
            Black_Screen_Timer = Black_Screen_Hold_Timer + (Black_Screen_Fade_Timer * 2);
            if (Black_Screen != null)
                Black_Screen.visible = true;
            if (Is_Actor)
                Global.game_map.remove_unit(Unit_Id);
        }
        #endregion

        #region Help
        public virtual void open_help()
        {
            if (Item_Selection_Window != null)
                Item_Selection_Window.open_help();
            else
                Supply_Window.open_help();
        }

        public virtual void close_help()
        {
            if (Item_Selection_Window != null)
                Item_Selection_Window.close_help();
            else
                Supply_Window.close_help();
        }
        #endregion

        #region Draw
        protected override void draw_window(SpriteBatch sprite_batch)
        {
            draw_header(sprite_batch);

            draw_command_windows(sprite_batch);
            if (Item_Selection_Window != null)
                Item_Selection_Window.draw(sprite_batch);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            HelpFooter.draw(sprite_batch);
            sprite_batch.End();

            Item_Window.draw_help(sprite_batch);
            Supply_Window.draw_help(sprite_batch);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            //Command_Window.draw(sprite_batch); //Debug
            Supply_Window.draw_cursor(sprite_batch);
            Item_Window.draw_cursor(sprite_batch);
            R_Button.Draw(sprite_batch);
            // Labels
            // Data
            sprite_batch.End();
        }

        protected abstract void draw_header(SpriteBatch sprite_batch);

        protected virtual void draw_command_windows(SpriteBatch spriteBatch)
        {
            Item_Window.draw(spriteBatch);
            Supply_Window.draw(spriteBatch);
        }
        #endregion
    }
}
