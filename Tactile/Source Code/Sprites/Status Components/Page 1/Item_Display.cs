using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile
{
    class Item_Display : Stereoscopic_Graphic_Object
    {
        protected TextSprite Name;
        protected Icon_Sprite Icon;

        public virtual Color tint
        {
            get { return Name.tint; }
            set
            {
                Name.tint = value;
                Icon.tint = value;
            }
        }

        public Item_Display()
        {
            Icon = new Icon_Sprite();
            Icon.size = new Vector2(Config.ITEM_ICON_SIZE, Config.ITEM_ICON_SIZE);
            Name = new TextSprite();
            Name.SetFont(Config.UI_FONT);
        }

        public virtual void set_text(string text)
        {
            // Name
            string font_name = "White";
            change_text_color(font_name);
            Name.text = text;
        }

        public virtual void set_image(Game_Actor actor, Data_Equipment item)
        {
            if (item == null)
            {
                Icon.texture = null;
                Name.text = "-----";
            }
            else
            {
                // Icon
                if (Global.content_exists(@"Graphics/Icons/" + item.Image_Name))
                    Icon.texture = Global.Content.Load<Texture2D>(string.Format(@"Graphics/Icons/{0}", item.Image_Name));
                Icon.index = item.Image_Index;
                Icon.columns = (int)(Icon.texture.Width / Icon.size.X);
                // Name
                Name.text = item.Name;

                set_text_color(actor, item);
            }
        }
        protected virtual void set_text_color(Game_Actor actor, Data_Equipment item)
        {
            bool useable;
            if (item.is_weapon)
                useable = actor.is_equippable(item as Data_Weapon);
            else
                useable = actor.prf_check(item) && ((item as Data_Item).Promotes.Count == 0) || Combat.can_use_item(actor, item.Id, false);

            set_text_color(useable);
        }
        public virtual void set_text_color(bool useable)
        {
            change_text_color(useable ? "White" : "Grey");
        }

        public void change_text_color(string color)
        {
            Name.SetColor(Global.Content, color);
        }

        public virtual void update()
        {
            Name.update();
            Icon.update();
        }

        public void draw(SpriteBatch sprite_batch)
        {
            draw(sprite_batch, Vector2.Zero);
        }
        public virtual void draw(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            Name.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(16, 0) - offset));
            if (Icon != null)
                Icon.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) - offset));
        }
    }
}
