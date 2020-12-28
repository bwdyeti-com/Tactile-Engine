using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Trade_Target_Item : Status_Item
    {
        public override void set_image(Game_Actor actor, TactileLibrary.Item_Data item_data)
        {
            if (item_data.non_equipment)
            {
                Icon.texture = null;
                Name.text = "Nothing";
                change_text_color("Grey");
                Uses.text = "";
            }
            else
                base.set_image(actor, item_data);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Name.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(16, 0) - offset));
            Uses.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(96, 0) - offset));
            Icon.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) - offset));
        }
    }
}
