using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Unit_Window_Inventory : Unit_Info_Inventory
    {
        protected override bool display_full_inventory(Game_Unit unit)
        {
            return true;
        }

        protected override bool scissor()
        {
            return true;
        }

        protected override bool equipped_first()
        {
            return true;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            foreach (Item_Icon_Sprite icon in Icons)
                icon.draw(sprite_batch, draw_offset - (loc - offset));
        }
    }
}
