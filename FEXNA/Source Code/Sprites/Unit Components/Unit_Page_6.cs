using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    class Unit_Page_6 : Unit_Page
    {
        protected List<FE_Text> Allies = new List<FE_Text>();

        public Unit_Page_6(Game_Unit unit, int start_index)
        { 
            // Support Buddies
            List<int> ready_supports = unit.actor.ready_supports();
            for (int i = 0; i < FEXNA.Windows.Map.Window_Unit.SUPPORTS_PER_PAGE; i++)
            {
                Allies.Add(new FE_Text());
                Allies[i].loc = new Vector2(8 + 56 * i, 0);
                Allies[i].Font = "FE7_Text";
                if (ready_supports.Count > (i + start_index) && Global.map_exists &&
                        Global.game_state.is_support_blocked(unit.actor.id, ready_supports[i], true))
                    Allies[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");
                else
                    Allies[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
                Allies[i].text = (ready_supports.Count > (i + start_index)) ? Global.game_actors[ready_supports[i + start_index]].name : "---";
            }
        }

        public override void update()
        {
            foreach (FE_Text ally in Allies)
                ally.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector();
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            foreach(FE_Text ally in Allies)
                ally.draw(sprite_batch, draw_offset - loc);
            sprite_batch.End();
        }
    }
}
