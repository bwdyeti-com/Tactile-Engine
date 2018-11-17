using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA.Windows.Command
{
    class Window_Command_Supply : Window_Command_Scrollbar
    {
        private List<Status_Item> ConvoyItems;
        protected List<SupplyItem> SupplyList;
        private Face_Sprite Face;
        internal float PageOffset;

        internal override int rows { get { return Rows; } }

        public Window_Command_Supply(Vector2 loc, int width, int rows) :
            base(loc, width, rows, null) { }

        protected override void set_default_offsets(int width)
        {
            base.set_default_offsets(width);
            this.text_offset = Vector2.Zero;
        }

        internal void refresh_items(List<SupplyItem> supplies, Game_Actor actor)
        {
            var items = new List<Status_Item>();
            for (int i = 0; i < supplies.Count; i++)
            {
                var item = new Status_Item();
                item.set_image(
                    actor, supplies[i].get_item());
            }
            if (items.Count == 0)
                items.Add(new ConvoyItemNothing());

            SupplyList = supplies;
            ConvoyItems = items;

            set_items(null);

            refresh_face();
        }
        internal void refresh_stacked_items(
            List<SupplyItem> supplies, Game_Actor actor, List<int> itemCounts, bool sameUses)
        {
            var items = new List<Status_Item>();
            for (int i = 0; i < supplies.Count; i++)
            {
                FEXNA_Library.Item_Data item_data;
                var supply_item = supplies[i].get_item();
                if (sameUses)
                    item_data = supply_item;
                else
                    item_data = new FEXNA_Library.Item_Data(supply_item.Type, supply_item.Id);

                Status_Item item_listing = new Convoy_Item();
                if (supplies[i].Convoy)
                {
                    item_listing = new Convoy_Item();
                    (item_listing as Convoy_Item).set_image(actor, item_data, itemCounts[i]);
                }
                else
                {
                    item_listing = new Convoy_Item(); //Debug
                    (item_listing as Convoy_Item).set_image(actor, item_data, 0);
                    //item_listing = new Status_Item();
                    //item_listing.set_image(actor, item_data);
                }
                //// If the item is a weapon and can't be equipped, color it grey // This should already be handled though //Yeti
                //if (item_data.is_weapon && !actor.is_equippable(Global.data_weapons[item_data.Id]))
                //    item_listing.change_text_color("Grey");

                items.Add(item_listing);
            }
            if (items.Count == 0)
                items.Add(new ConvoyItemNothing());

            SupplyList = supplies;
            ConvoyItems = items;

            set_items(null);

            refresh_face();
        }

        protected override void add_commands(List<string> strs)
        {
            var nodes = new List<CommandUINode>();

            if (ConvoyItems == null)
            {
                set_nodes(nodes);
                return;
            }

            int count = ConvoyItems.Count;

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
            var text_node = new ItemUINode("", ConvoyItems[i], this.column_width - 8);
            text_node.loc = item_loc(i);
            return text_node;
        }

        protected override void on_index_changed(int oldIndex)
        {
            base.on_index_changed(oldIndex);

            // Face
            refresh_face();
        }

        private void refresh_face()
        {
            if (this.index < SupplyList.Count && !SupplyList[this.index].Convoy)
            {
                Game_Actor actor = Global.game_actors[SupplyList[this.index].ActorId];
                Face = new Face_Sprite(actor.face_name, true);
                if (actor.generic_face)
                    Face.recolor_country(actor.name_full);
                Face.expression = Face.status_frame;
                Face.phase_in();
                Face.tint = new Color(128, 128, 128, 128);
                Face.idle = true;
                Face.loc = new Vector2(Width / 2, Rows * 16 + 8 + 2);
                Face.mirrored = false;
            }
            else
                Face = null;
        }

        protected override void update_commands(bool input)
        {
            base.update_commands(input);
            if (Face != null)
                Face.update();
        }

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            base.draw_window(sprite_batch);
            if (Face != null)
                Face.draw(sprite_batch, -(loc + draw_vector()));
        }

        protected override void draw_text(SpriteBatch sprite_batch)
        {
            Items.Draw(sprite_batch,
                -(loc + text_draw_vector() + new Vector2(PageOffset, 0)));
        }
    }
}
