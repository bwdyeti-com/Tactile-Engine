using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Weapon_Level_Rank : Weapon_Level_Gauge
    {
        public Weapon_Level_Rank() : base(0) { }
        public Weapon_Level_Rank(int type) : base(type) { }

        protected override void initialize_letter()
        {
            WLvl_Letter = new RightAdjustedText();
            WLvl_Letter.loc = new Vector2(32, 8);
            WLvl_Letter.SetFont(Config.UI_FONT + "L", Config.UI_FONT);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            WLvl_Icon.draw(sprite_batch, draw_offset - (this.loc + draw_vector()));
            WLvl_Letter.draw(sprite_batch, draw_offset - (this.loc + draw_vector()));
        }
        public override void draw_bg(SpriteBatch sprite_batch, Vector2 draw_offset) { }
    }
}
