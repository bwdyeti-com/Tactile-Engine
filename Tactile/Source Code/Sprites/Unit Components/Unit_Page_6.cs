using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class Unit_Page_6 : Unit_Page
    {
        protected List<TextSprite> Allies = new List<TextSprite>();

        public Unit_Page_6(Game_Unit unit, int start_index)
        { 
            // Support Buddies
            List<int> ready_supports = unit.actor.ready_supports();
            for (int i = 0; i < Tactile.Windows.Map.Window_Unit.SUPPORTS_PER_PAGE; i++)
            {
                Allies.Add(new TextSprite());
                Allies[i].loc = new Vector2(8 + 56 * i, 0);
                Allies[i].SetFont(Config.UI_FONT);
                if (ready_supports.Count > (i + start_index) && Global.map_exists &&
                        Global.game_state.is_support_blocked(unit.actor.id, ready_supports[i], true))
                    Allies[i].SetColor(Global.Content, "Grey");
                else
                    Allies[i].SetColor(Global.Content, "White");
                Allies[i].text = (ready_supports.Count > (i + start_index)) ? Global.game_actors[ready_supports[i + start_index]].name : "---";
            }
        }

        public override void update()
        {
            foreach (TextSprite ally in Allies)
                ally.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector();
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            foreach(TextSprite ally in Allies)
                ally.draw(sprite_batch, draw_offset - loc);
            sprite_batch.End();
        }
    }
}
