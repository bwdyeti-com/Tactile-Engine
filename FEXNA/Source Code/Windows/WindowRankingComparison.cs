using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;

namespace FEXNA.Windows
{
    class WindowRankingComparison : Graphic_Object
    {
        private Game_Ranking Ranking, PreviousRanking;
        private System_Color_Window Window_Img;
        private List<FE_Text> Text;
        private List<Weapon_Triangle_Arrow> Arrows;

        public int Width { get { return 120; } }

        public WindowRankingComparison() : this(new Game_Ranking()) { }
        public WindowRankingComparison(Game_Ranking ranking)
        {
            Ranking = ranking;
            InitializeImages();
        }
        public WindowRankingComparison(Game_Ranking ranking, Game_Ranking previousRanking)
        {
            Ranking = ranking;
            PreviousRanking = previousRanking;

            InitializeImages();
        }

        private void InitializeImages()
        {
            Window_Img = new SystemWindowHeadered();
            Window_Img.width = this.Width;
            Window_Img.set_lines(4);
            Window_Img.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            Text = new List<FE_Text>();
            // Chapter name and difficulty
            var name = new FE_Text(
                "FE7_Text",
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow"),
                new Vector2(12, 8),
                Global.data_chapters[Ranking.ChapterId].ShortName);
            name.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            Text.Add(name);
            var difficulty = new FE_Text_Int(
                "FE7_Text",
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue"),
                new Vector2(Window_Img.width - 12, 8),
                Ranking.Difficulty.ToString());
            difficulty.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            Text.Add(difficulty);

            for (int i = 0; i < 4; i++)
            {
                var label = new FE_Text();
                label.loc = new Vector2(8, 24 + i * 16);
                label.Font = "FE7_Text";
                label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
                label.stereoscopic = Config.RANKING_WINDOW_DEPTH;
                Text.Add(label);
            }
            Text[Text.Count - 4].text = "Turns";
            Text[Text.Count - 3].text = "Combat";
            Text[Text.Count - 2].text = "Experience";
            Text[Text.Count - 1].text = "Total";
            //Text[3].text = "MVP";
            for (int i = 0; i < 4; i++)
            {
                var value = new FE_Text_Int();
                value.loc = new Vector2(Window_Img.width - 8, 24 + i * 16);
                value.Font = "FE7_Text";
                value.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
                value.stereoscopic = Config.RANKING_WINDOW_DEPTH;
                Text.Add(value);
            }
            Text[Text.Count - 4].text = Ranking.turns.ToString();
            Text[Text.Count - 3].text = Ranking.combat.ToString();
            Text[Text.Count - 2].text = Ranking.exp.ToString();
            Text[Text.Count - 1].text = Ranking.score.ToString();

            Arrows = new List<Weapon_Triangle_Arrow>();
            if (PreviousRanking == null)
            {
                Window_Img.tint = new Color(192, 192, 192, 255);
            }
            else
            {
                for (int i = 4; i >= 1; i--)
                {
                    var text = Text[Text.Count - i];
                    text.loc.X -= 4;

                    var arrow = new Weapon_Triangle_Arrow();
                    arrow.loc = text.loc + new Vector2(-12, 0);
                    arrow.stereoscopic = Config.RANKING_WINDOW_DEPTH;
                    Arrows.Add(arrow);
                }
                CompareRanking(0, Ranking.turns, PreviousRanking.turns);
                CompareRanking(1, Ranking.combat, PreviousRanking.combat);
                CompareRanking(2, Ranking.exp, PreviousRanking.exp);
                CompareRanking(3, Ranking.score, PreviousRanking.score);
            }
        }

        private void CompareRanking(int arrowIndex, int score, int previousScore)
        {
            if (score != previousScore)
                Arrows[arrowIndex].value = score > previousScore ?
                    WeaponTriangle.Advantage : WeaponTriangle.Disadvantage;
        }

        public void Update()
        {
            foreach (var text in Text)
                text.update();
            foreach (var arrow in Arrows)
                arrow.update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Window_Img.draw(spriteBatch, -this.loc);
            if (Window_Img.visible)
            {
                foreach (FE_Text text in Text)
                    text.draw(spriteBatch, -this.loc);
                foreach (var arrow in Arrows)
                    arrow.draw(spriteBatch, -this.loc);
            }
            spriteBatch.End();
        }
    }
}
