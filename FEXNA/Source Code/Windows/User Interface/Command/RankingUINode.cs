using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA;
using FEXNA.Graphics.Text;

namespace FEXNA.Windows.UserInterface.Command
{
    class RankingUINode : TextUINode
    {
        //private FE_Text TurnsRank, CombatRank, ExpRank, CompletionLetter;
        private FE_Text Rank;
        private FE_Text_Int RankValue;

        internal RankingUINode(
                string helpLabel,
                FE_Text text,
                int width,
                Game_Ranking ranking)
            : base(helpLabel, text, width)
        {
            /* //Debug
            TurnsRank = ranking_text(ranking.turns_letter);
            TurnsRank.draw_offset = new Vector2(32, 0);

            CombatRank = ranking_text(ranking.combat_letter);
            CombatRank.draw_offset = new Vector2(40, 0);

            ExpRank = ranking_text(ranking.exp_letter);
            ExpRank.draw_offset = new Vector2(48, 0);

            CompletionLetter = ranking_text(ranking.completion_letter);
            CompletionLetter.draw_offset = new Vector2(56, 0);*/

            RankValue = new FE_Text_Int();
            RankValue.Font = "FE7_Text";
            RankValue.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_Blue");
            RankValue.text = ranking.score.ToString();
            RankValue.draw_offset = new Vector2(48 - 0, 0);

            Rank = ranking_text(ranking.rank);
            Rank.Font = "FE7_TextL";
            Rank.draw_offset = new Vector2(48 + 4, 0);
        }

        private FE_Text ranking_text(string text)
        {
            var rank = new FE_Text();
            rank.Font = "FE7_Text";
            rank.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_Blue");
            rank.text = text;
            return rank;
        }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);

            /* //Debug
            TurnsRank.update();
            CombatRank.update();
            ExpRank.update();
            CompletionLetter.update();*/
            RankValue.update();
            Rank.update();
        }

        protected override void mouse_off_graphic()
        {
            base.mouse_off_graphic();

            /* //Debug
            TurnsRank.tint = Color.White;
            CombatRank.tint = Color.White;
            ExpRank.tint = Color.White;
            CompletionLetter.tint = Color.White;*/
            RankValue.tint = Color.White;
            Rank.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            base.mouse_highlight_graphic();

            /* //Debug
            TurnsRank.tint = FEXNA.Config.MOUSE_OVER_ELEMENT_COLOR;
            CombatRank.tint = FEXNA.Config.MOUSE_OVER_ELEMENT_COLOR;
            ExpRank.tint = FEXNA.Config.MOUSE_OVER_ELEMENT_COLOR;
            CompletionLetter.tint = FEXNA.Config.MOUSE_OVER_ELEMENT_COLOR;*/
            RankValue.tint = FEXNA.Config.MOUSE_OVER_ELEMENT_COLOR;
            Rank.tint = FEXNA.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();

            /* //Debug
            TurnsRank.tint = FEXNA.Config.MOUSE_PRESSED_ELEMENT_COLOR;
            CombatRank.tint = FEXNA.Config.MOUSE_PRESSED_ELEMENT_COLOR;
            ExpRank.tint = FEXNA.Config.MOUSE_PRESSED_ELEMENT_COLOR;
            CompletionLetter.tint = FEXNA.Config.MOUSE_PRESSED_ELEMENT_COLOR;*/
            RankValue.tint = FEXNA.Config.MOUSE_PRESSED_ELEMENT_COLOR;
            Rank.tint = FEXNA.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);

            /* //Debug
            TurnsRank.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            CombatRank.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            ExpRank.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            CompletionLetter.draw(sprite_batch, draw_offset - (loc + draw_vector()));*/
            RankValue.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            Rank.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
