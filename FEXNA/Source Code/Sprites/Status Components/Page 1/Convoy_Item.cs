using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA
{
    class Convoy_Item : Status_Item
    {
        public Convoy_Item()
        {
            Use_Max = new FE_Text();
            Use_Max.Font = "FE7_TextS";
        }

        public override void set_image(Game_Actor actor, Item_Data item_data)
        {
            set_image(actor, item_data, 1);
        }
        public void set_image(Game_Actor actor, Item_Data item_data, int count)
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
                Icon.texture = Global.Content.Load<Texture2D>(string.Format(@"Graphics/Icons/{0}", item.Image_Name));
                Icon.index = item.Image_Index;
                // Name
                Name.text = item.Name;
                // Uses
                Uses.text = item_data.Uses < 0 ? "--" : item_data.Uses.ToString();
                if (count == 0)
                {
                    Uses.draw_offset = new Vector2(0, 0);
                    Slash.text = "";
                    Use_Max.text = "";
                }
                else
                {
                    Uses.draw_offset = new Vector2(0, 0);
                    Slash.text = "x";
                    Use_Max.text = count < 0 ? "--" : count.ToString();
                }

                Slash.visible = Use_Max.text.Length <= 2;
                Use_Max.draw_offset = new Vector2(
                    Use_Max.text.Length <= 2 ? 8 : 0, 0);

                set_text_color(actor, item);
            }
        }
        public override void set_text_color(bool useable)
        {
            change_text_color(useable ? "White" : "Grey");

            string color = useable ? "Blue" : "Grey";
            Uses.texture = Global.Content.Load<Texture2D>(string.Format(@"Graphics/Fonts/FE7_Text_{0}", color));
            Slash.texture = Global.Content.Load<Texture2D>(string.Format(@"Graphics/Fonts/FE7_Text_{0}", color));
            Use_Max.texture = Global.Content.Load<Texture2D>(string.Format(@"Graphics/Fonts/FE7_Text_{0}", color));
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Name.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(16, 0) - offset));
            if (Uses != null)
            {
                Uses.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(96, 0) - offset));
                Slash.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(97, 0) - offset));
                Use_Max.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(96, 0) - offset));
                Icon.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) - offset));
            }
        }
    }
}
