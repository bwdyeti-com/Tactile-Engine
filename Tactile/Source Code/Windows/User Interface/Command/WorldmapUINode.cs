using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile.Windows.UserInterface.Command
{
    class WorldmapUINode : TextUINode
    {
        private string NormalRank = "", HardRank = "";
        private RightAdjustedText Rank;
        private Maybe<Difficulty_Modes> Difficulty;

        internal WorldmapUINode(
                string helpLabel,
                TextSprite text,
                int width)
            : base(helpLabel, text, width)
        {
            Rank = new RightAdjustedText();
            Rank.draw_offset = new Vector2(width, 0);
            Rank.SetFont(Tactile.Config.UI_FONT + "L", Global.Content, "Yellow", Tactile.Config.UI_FONT);
        }

        internal void set_rank(string rank, Maybe<Difficulty_Modes> difficulty)
        {
            Rank.text = rank;
            Difficulty = difficulty;

            refresh_rank(Global.game_system.Difficulty_Mode);
        }

        internal void refresh_rank(Difficulty_Modes mode)
        {
            if (Difficulty.IsNothing || mode == Difficulty)
                Rank.SetColor(Global.Content, "Yellow");
            else
                Rank.SetColor(Global.Content, "Grey");
        }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);
            Rank.update();
            refresh_rank(Global.game_system.Difficulty_Mode);
        }

        protected override void mouse_off_graphic()
        {
            base.mouse_off_graphic();
            Rank.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            base.mouse_highlight_graphic();
            Rank.tint = Tactile.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();
            Rank.tint = Tactile.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);
            Rank.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
