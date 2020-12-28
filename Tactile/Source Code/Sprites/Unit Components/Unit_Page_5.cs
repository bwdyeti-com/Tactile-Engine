using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Unit_Page_5 : Unit_Page
    {
        const int ICON_SPACING = 20;
        
        protected List<TextSprite> WLvls = new List<TextSprite>();

        public Unit_Page_5(Game_Unit unit)
        {
            // WLvls
            int i = 0;
            for (int index = 0; index < Global.weapon_types.Count - 1; index++)
            {
                var weapon_type = Global.weapon_types[index + 1];
                if (!weapon_type.DisplayedInStatus)
                    continue;

                string color = (unit.actor.weapon_level_letter(weapon_type) ==
                    TactileLibrary.Data_Weapon.WLVL_LETTERS[TactileLibrary.Data_Weapon.WLVL_LETTERS.Length - 1] ? "Green" : "Blue");
                WLvls.Add(new TextSprite());
                WLvls[i].loc = new Vector2(16 + ICON_SPACING * i, 0);
                WLvls[i].SetFont(Config.UI_FONT + "L", Global.Content, color, Config.UI_FONT);
                WLvls[i].text = unit.actor.weapon_level_letter(weapon_type);

                i++;
            }
        }

        public override void update()
        {
            foreach (TextSprite wlvl in WLvls)
                wlvl.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector();
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            foreach (TextSprite ally in WLvls)
                ally.draw(sprite_batch, draw_offset - loc);
            sprite_batch.End();
        }
    }
}
