using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Steal_Target_Item : Steal_Item
    {
        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Name.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(16, 0) - offset));
            Uses.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) + new Vector2(96, 0) - offset));
            Icon.draw(sprite_batch, draw_offset - ((this.loc + draw_vector()) - offset));
        }
    }
}
