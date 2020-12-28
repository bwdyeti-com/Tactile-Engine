using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile
{
    class Shop_Item : Status_Item
    {
        protected RightAdjustedText Convoy;

        public override Color tint
        {
            get { return base.tint; }
            set
            {
                base.tint = value;
                Convoy.tint = value;
            }
        }

        public Shop_Item()
        {
            Slash = new RightAdjustedText();
            Slash.SetFont(Config.UI_FONT);
            Convoy = new RightAdjustedText();
            Convoy.SetFont(Config.UI_FONT);
        }

        public virtual void set_image(Game_Actor actor, Item_Data item_data, int stock, int price)
        {
            Data_Equipment item = item_data.to_equipment;
            if (Global.content_exists(@"Graphics/Icons/" + item.Image_Name))
            {
                Icon.texture = Global.Content.Load<Texture2D>(string.Format(@"Graphics/Icons/{0}", item.Image_Name));
                Icon.columns = (int)(Icon.texture.Width / Icon.size.X);
            }
            Icon.index = item.Image_Index;
            // Name
            Name.text = item.Name;
            // Uses
            Uses.text = item_data.Uses < 0 ? "--" : item_data.Uses.ToString();
            Slash.text = price.ToString();
            Use_Max.text = stock < 0 ? "--" : stock.ToString();
            // Num owned in battalion
            Convoy.text = Global.battalion.item_count(item_data).ToString();

            set_text_color(actor, item, item_data, price);
        }
        protected virtual void set_text_color(Game_Actor actor, Data_Equipment item, Item_Data item_data, int price)
        {
            bool useable;
            if (item.is_weapon && actor != null)
                useable = actor.is_equippable(item as Data_Weapon);
            else
                useable = true;

            set_text_color(useable, buyable_text(price, item_data));
        }
        protected virtual void set_text_color(bool useable, bool buyable)
        {
            change_text_color(useable ? "White" : "Grey");

            Uses.SetColor(Global.Content, useable ? "Blue" : "Grey");

            Slash.SetColor(Global.Content, buyable ? "Blue" : "Grey");
            Use_Max.SetColor(Global.Content, buyable ? "Blue" : "Grey");

            Convoy.SetColor(Global.Content, "Blue");
        }

        protected virtual bool buyable_text(int price, Item_Data item_data)
        {
            return price <= Global.battalion.gold;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Name.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(16, 0) - offset));
            Uses.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(112, 0) - offset));
            Slash.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(160, 0) - offset));
            Use_Max.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(200, 0) - offset));
            Convoy.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(232, 0) - offset));
            Icon.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) - offset));
        }
    }
}
