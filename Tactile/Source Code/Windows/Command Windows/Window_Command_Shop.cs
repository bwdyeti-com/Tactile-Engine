using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Windows.UserInterface.Command;
using TactileLibrary;

namespace Tactile.Windows.Command
{
    class Window_Command_Shop : Window_Command_Scrollbar
    {
        private bool Buying;
        private int ActorId;
        private int BuyPriceMod;
        private List<Item_Data> ShopItems;
        private List<TextSprite> Headers = new List<TextSprite>();
        private StatusWindowDivider Window_Divider;

        protected Game_Actor actor
        {
            get
            {
                if (!Global.game_actors.ContainsKey(ActorId))
                    return null;
                return Global.game_actors[ActorId];
            }
        }

        internal override int rows { get { return Rows; } }

        public Window_Command_Shop(Window_Command_Scrollbar window,
            Vector2 loc, int width, int rows,
            bool buying, int actorId, int buyPriceMod,
            IEnumerable<Item_Data> items)
        {
            Rows = Math.Max(1, rows);
            Buying = buying;
            ActorId = actorId;
            BuyPriceMod = buyPriceMod;
            ShopItems = items.Select(x => new Item_Data(x)).ToList();
            this.glow = true;

            initialize(loc, width, null);
            Items.WrapVerticalMove = false;
            if (window != null)
            {
                if (num_items() > 0)
                    this.immediate_index = window.index;
                this.scroll = window.scroll;

                int target_y = 16 * Scroll;
                ScrollOffset.Y = target_y;
            }
        }

        protected override void initialize(Vector2 loc, int width, List<string> strs)
        {
            // Headers
            for (int i = 0; i < 5; i++)
            {
                Headers.Add(new TextSprite());
                Headers[i].SetFont(Config.UI_FONT, Global.Content, "White");
            }
            Headers[0].loc = new Vector2(24, 8);
            Headers[1].loc = new Vector2(104, 8);
            Headers[2].loc = new Vector2(144, 8);
            Headers[3].loc = new Vector2(184, 8);
            Headers[4].loc = new Vector2(216, 8);
            Headers[0].text = "Item";
            Headers[1].text = "Uses";
            Headers[2].text = "Price";
            Headers[3].text = "Stock";
            Headers[4].text = "Owned";

            Window_Divider = new StatusWindowDivider();
            Window_Divider.SetWidth(width - 32);
            Window_Divider.loc = new Vector2(16, 20);

            base.initialize(loc, width, strs);
        }

        protected override void set_default_offsets(int width)
        {
            this.text_offset = new Vector2(0, 20);
            Size_Offset = new Vector2(0, 20);
            this.glow_width = width - (24 + (int)(Text_Offset.X * 2));
            Bar_Offset = new Vector2(0, 0);
        }

        protected override void add_commands(List<string> strs)
        {
            if (ShopItems == null)
                return;
            int count = ShopItems.Count;

            var nodes = new List<CommandUINode>();
            for (int i = 0; i < count; i++)
            {
                var item_node = item("", i);
                if (item_node != null)
                {
                    item_node.loc = item_loc(nodes.Count);
                    nodes.Add(item_node);
                }
            }

            set_nodes(nodes);
        }

        protected override CommandUINode item(object value, int i)
        {
            var item_data = ShopItems[i];
            var text = Buying ? new Shop_Item() : new Shop_Sell_Item();
            text.set_image(this.actor, new Item_Data(
                item_data.Type, item_data.Id),
                //Buying ? item_data.Uses : -1, item_data.item_cost(Buying, BuyPriceMod)); //Debug
                item_data.Uses, item_data.item_cost(Buying, BuyPriceMod));
            var text_node = new ItemUINode("", text, this.column_width - 8);
            text_node.loc = item_loc(i);
            return text_node;
        }

        protected override void on_index_changed(int oldIndex)
        {
            base.on_index_changed(oldIndex);
            refresh();
        }

        protected override void update_commands(bool input)
        {
            base.update_commands(input);
            if (is_help_active)
                Help_Window.update();
        }

        public override Rectangle scissor_rect()
        {
            Rectangle rect = base.scissor_rect();
            rect.Y += (int)Text_Offset.Y;
            rect.Height -= (int)Text_Offset.Y;
            return Scene_Map.fix_rect_to_screen(rect);
        }

        protected void refresh()
        {
            if (is_help_active)
            {
                Help_Window.set_item(ShopItems[this.index], this.actor);
                update_help_loc();
            }
        }

        #region Help
        public virtual void open_help()
        {
            Help_Window = new Window_Help();
            Help_Window.set_item(ShopItems[this.index], this.actor);
            Help_Window.loc = loc + item_loc(this.index) + Text_Offset +
                new Vector2(0, -Scroll * 16);
            update_help_loc();
            Global.game_system.play_se(System_Sounds.Help_Open);
        }

        public virtual void close_help()
        {
            Help_Window = null;
            Global.game_system.play_se(System_Sounds.Help_Close);
        }

        protected virtual void update_help_loc()
        {
            Help_Window.set_loc(
                loc + new Vector2(0, 8 + base.Items.ActiveNodeIndex * 16) +
                Text_Offset + new Vector2(0, -Scroll * 16));
        }
        #endregion

        protected override Vector2 help_draw_vector()
        {
            return base.help_draw_vector();
        }

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            base.draw_window(sprite_batch);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Window_Divider.draw(sprite_batch, -(this.loc + draw_vector()));
            foreach (TextSprite text in Headers)
                text.draw(sprite_batch, -(this.loc + draw_vector()));
            sprite_batch.End();
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            base.draw(sprite_batch);

            if (visible)
            {
                draw_help(sprite_batch, false);
            }
        }

        public void draw_help(SpriteBatch sprite_batch)
        {
            draw_help(sprite_batch, true);
        }
        protected void draw_help(SpriteBatch sprite_batch, bool called)
        {
            if (called == Manual_Help_Draw)
                if (is_help_active)
                    Help_Window.draw(sprite_batch, -help_draw_vector());
        }
    }
}
