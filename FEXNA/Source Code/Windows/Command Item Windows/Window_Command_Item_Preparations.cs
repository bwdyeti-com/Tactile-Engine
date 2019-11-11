using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Preparations;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Windows.Command.Items
{
    class Window_Command_Item_Preparations : Window_Command_Item
    {
        protected bool Face_Shown = true, Using_Item;
        protected Pick_Units_Items_Header ItemHeader;

        #region Accessors
        public bool face_shown { set { Face_Shown = value; } }

        public bool darkened { set { ((Prepartions_Item_Window)Window_Img).darkened = value; } }
        #endregion

        protected Window_Command_Item_Preparations() { }
        public Window_Command_Item_Preparations(int actor_id, Vector2 loc, bool facing_right) : this(actor_id, loc, facing_right, false) { }
        public Window_Command_Item_Preparations(int actor_id, Vector2 loc, bool facing_right, bool using_item)
        {
            this.text_offset = new Vector2(0, -4);
            Actor_Id = actor_id;
            active = false;
            Using_Item = using_item;
            initialize(loc, WIDTH);

            (ItemInfo as PreparationsStatsPanel).face_values(
                facing_right, Window_Img);
        }

        protected virtual void initialize(Vector2 loc, int width)
        {
            base.initialize(loc, width, new List<string>());
            Window_Img = new Prepartions_Item_Window(true);
            Window_Img.width = width;
            Window_Img.height = Constants.Actor.NUM_ITEMS * 16 + 8;
        }

        protected override void item_initialize(Vector2 loc, int width, List<string> strs)
        {
            ItemInfo = new PreparationsStatsPanel(actor());
            ItemInfo.loc = loc + new Vector2(width, 40);
        }

        protected override CommandUINode item(string str, int i)
        {
            var item_data = items(i);
            if (!is_valid_item(get_equipment(), i))
                return null;

            var text = Using_Item ? new Prep_Using_Item() : new Status_Item();
            text.set_image(actor(), item_data);
            var text_node = new ItemUINode("", text, this.column_width);
            text_node.loc = item_loc(i);
            return text_node;
        }

        protected override bool should_refresh_info()
        {
            return Using_Item && ItemInfo != null;
        }

        protected override void equip_actor() { }

        public void ShowHeader()
        {
            ItemHeader = new Pick_Units_Items_Header(this.actor().id, this.width);
            ItemHeader.loc = -new Vector2(4, 36);
            ItemHeader.stereoscopic = Config.PREPITEM_UNIT_DEPTH;
        }
        public void HideHeader()
        {
            ItemHeader = null;
        }

        #region Draw
        public override void draw(SpriteBatch sprite_batch)
        {
            if (visible)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                draw_window(sprite_batch);
                sprite_batch.End();

                // Info
                draw_info(sprite_batch);

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                // Text
                draw_text(sprite_batch);
                sprite_batch.End();

                if (ItemHeader != null)
                    ItemHeader.draw(sprite_batch, -this.loc);
            }
        }

        public override void draw_help(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Cursor
            draw_cursor(sprite_batch);
            sprite_batch.End();

            base.draw_help(sprite_batch, false);
        }

        protected override void draw_bar(SpriteBatch sprite_batch) { }
        #endregion
    }
}
