using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    class Unit_Page_4 : Unit_Page
    {
        protected FE_Text Atk, Hit, Avoid, Crit, Dod, AS, Rng;

        public Unit_Page_4(Game_Unit unit)
        {
            var stats = new Calculations.Stats.BattlerStats(unit.id);

            // If the unit has a weapon equipped
            bool has_weapon = stats.has_weapon;
            // If said weapon is not a staff
            bool non_staff = stats.has_non_staff_weapon;

            // Atk
            Atk = new FE_Text_Int();
            Atk.loc = new Vector2(24, 0);
            Atk.Font = "FE7_Text";
            Atk.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Atk.text = !non_staff ? "--" : stats.dmg().ToString();
            // Atk
            Hit = new FE_Text_Int();
            Hit.loc = new Vector2(56, 0);
            Hit.Font = "FE7_Text";
            Hit.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Hit.text = !non_staff ? "--" : stats.hit().ToString();
            // Avoid
            Avoid = new FE_Text_Int();
            Avoid.loc = new Vector2(88, 0);
            Avoid.Font = "FE7_Text";
            Avoid.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Avoid.text = stats.avo().ToString();
            // Crit
            Crit = new FE_Text_Int();
            Crit.loc = new Vector2(120, 0);
            Crit.Font = "FE7_Text";
            Crit.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Crit.text = !non_staff ? "--" : stats.crt().ToString();
            // Dod
            Dod = new FE_Text_Int();
            Dod.loc = new Vector2(144, 0);
            Dod.Font = "FE7_Text";
            Dod.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            Dod.text = stats.dodge().ToString();
            // AS
            AS = new FE_Text_Int();
            AS.loc = new Vector2(176, 0);
            AS.Font = "FE7_Text";
            AS.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            AS.text = unit.atk_spd().ToString();
            // Rng
            Rng = new FE_Text_Int();
            Rng.Font = "FE7_Text";
            Rng.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            if (!has_weapon)
            {
                Rng.loc = new Vector2(212, 0);
                Rng.text = "--";
            }
            else
            {
                int min_range = unit.min_range();
                int max_range = unit.max_range();
                if (unit.actor.weapon.Mag_Range)
                {
                    Rng.loc = new Vector2(216, 0);
                    Rng.text = "Mag/2";
                }
                else if (min_range == max_range)
                {
                    Rng.loc = new Vector2(208, 0);
                    Rng.text = min_range.ToString();
                }
                else
                {
                    Rng.loc = new Vector2(216, 0);
                    Rng.text = min_range.ToString() + "-" + max_range.ToString();
                }
            }
        }

        public override void update()
        {
            Atk.update();
            Hit.update();
            Avoid.update();
            Crit.update();
            Dod.update();
            AS.update();
            Rng.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector();
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            Atk.draw(sprite_batch, draw_offset - loc);
            Hit.draw(sprite_batch, draw_offset - loc);
            Avoid.draw(sprite_batch, draw_offset - loc);
            Crit.draw(sprite_batch, draw_offset - loc);
            Dod.draw(sprite_batch, draw_offset - loc);
            AS.draw(sprite_batch, draw_offset - loc);
            Rng.draw(sprite_batch, draw_offset - loc);
            sprite_batch.End();
        }
    }
}
