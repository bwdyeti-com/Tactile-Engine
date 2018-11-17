using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA;
using FEXNA.Graphics.Text;

namespace FEXNA.Windows.UserInterface.Command
{
    class WorldmapUINode : TextUINode
    {
        private string NormalRank = "", HardRank = "";
        private FE_Text_Int Rank;
        private Difficulty_Modes Difficulty;
        private Texture2D ActiveDifficultyTexture, InactiveDifficultyTexture;

        internal WorldmapUINode(
                string helpLabel,
                FE_Text text,
                int width)
            : base(helpLabel, text, width)
        {
            ActiveDifficultyTexture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            InactiveDifficultyTexture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");

            Rank = new FE_Text_Int();
            Rank.draw_offset = new Vector2(width, 0);
            Rank.Font = "FE7_TextL";

            Rank.texture = ActiveDifficultyTexture;
        }

        internal void set_rank(string rank, Difficulty_Modes difficulty)
        {
            Rank.text = rank;
            Difficulty = difficulty;

            refresh_rank(Global.game_system.Difficulty_Mode);
        }

        internal void refresh_rank(Difficulty_Modes mode)
        {
            if (mode == Difficulty)
                Rank.texture = ActiveDifficultyTexture;
            else
                Rank.texture = InactiveDifficultyTexture;
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
            Rank.tint = FEXNA.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();
            Rank.tint = FEXNA.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);
            Rank.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
