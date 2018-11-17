using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA
{
    class Status_Item : Item_Display
    {
        protected FE_Text Uses, Slash, Use_Max;

        public override Color tint
        {
            get { return base.tint; }
            set
            {
                base.tint = value;
                Uses.tint = value;
                Slash.tint = value;
                Use_Max.tint = value;
            }
        }

        public Status_Item() : base()
        {
            Uses = new FE_Text_Int();
            Uses.Font = "FE7_Text";
            Slash = new FE_Text();
            Slash.Font = "FE7_Text";
            Use_Max = new FE_Text_Int();
            Use_Max.Font = "FE7_Text";
        }

        public override void set_image(Game_Actor actor, Data_Equipment item)
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }
        public virtual void set_image(Game_Actor actor, Item_Data item_data)
        {
            if (item_data.non_equipment)
            {
                Icon.texture = null;
                Name.text = "";
                Uses.text = "";
                Slash.text = "";
                Use_Max.text = "";
            }
            else
            {
                Data_Equipment item = item_data.to_equipment;
                // Icon
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
                Slash.text = "/";
                Use_Max.text = item.Uses < 0 ? "--" : item.Uses.ToString();

                set_text_color(actor, item);
            }
        }

        public override void set_text_color(bool useable)
        {
            base.set_text_color(useable);

            string font_name = useable ? @"Graphics/Fonts/FE7_Text_Blue" : @"Graphics/Fonts/FE7_Text_Grey";
            Uses.texture = Global.Content.Load<Texture2D>(font_name);
            Slash.texture = Global.Content.Load<Texture2D>(font_name);
            Use_Max.texture = Global.Content.Load<Texture2D>(font_name);
        }

        sealed public override void update()
        {
            base.update();

            Uses.update();
            Slash.update();
            Use_Max.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            if (Uses != null)
            {
                Uses.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(96, 0) - offset));
                Slash.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(97, 0) - offset));
                Use_Max.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(120, 0) - offset));
            }
            base.draw(sprite_batch, draw_offset);
        }
    }
}
