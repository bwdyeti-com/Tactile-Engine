using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile;
using Tactile.Graphics.Text;

namespace Tactile.Windows.UserInterface.Command
{
    class RankingUINode : TextUINode
    {
        private TextSprite Rank;
        private RightAdjustedText RankValue;

        internal RankingUINode(
                string helpLabel,
                TextSprite text,
                int width,
                Game_Ranking ranking)
            : base(helpLabel, text, width)
        {
            RankValue = new RightAdjustedText();
            RankValue.SetFont(Tactile.Config.UI_FONT, Global.Content, "Blue");
            RankValue.text = ranking.score.ToString();
            RankValue.draw_offset = new Vector2(56 - 0, 0);

            Rank = ranking_text(ranking.rank);
            Rank.SetFont(Tactile.Config.UI_FONT + "L", Tactile.Config.UI_FONT);
            Rank.draw_offset = new Vector2(56 + 4, 0);
        }

        private TextSprite ranking_text(string text)
        {
            var rank = new TextSprite();
            rank.SetFont(Tactile.Config.UI_FONT, Global.Content, "Blue");
            rank.text = text;
            return rank;
        }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);

            RankValue.update();
            Rank.update();
        }

        protected override void mouse_off_graphic()
        {
            base.mouse_off_graphic();

            RankValue.tint = Color.White;
            Rank.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            base.mouse_highlight_graphic();

            RankValue.tint = Tactile.Config.MOUSE_OVER_ELEMENT_COLOR;
            Rank.tint = Tactile.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();

            RankValue.tint = Tactile.Config.MOUSE_PRESSED_ELEMENT_COLOR;
            Rank.tint = Tactile.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);

            RankValue.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            Rank.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
