using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    class Unit_Page_5 : Unit_Page
    {
        const int ICON_SPACING = 20;
        
        protected List<FE_Text> WLvls = new List<FE_Text>();

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
                    FEXNA_Library.Data_Weapon.WLVL_LETTERS[FEXNA_Library.Data_Weapon.WLVL_LETTERS.Length - 1] ? "Green" : "Blue");
                WLvls.Add(new FE_Text());
                WLvls[i].loc = new Vector2(16 + ICON_SPACING * i, 0);
                WLvls[i].Font = "FE7_TextL";
                WLvls[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + color);
                WLvls[i].text = unit.actor.weapon_level_letter(weapon_type);

                i++;
            }
        }

        public override void update()
        {
            foreach (FE_Text wlvl in WLvls)
                wlvl.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector();
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            foreach (FE_Text ally in WLvls)
                ally.draw(sprite_batch, draw_offset - loc);
            sprite_batch.End();
        }
    }
}
