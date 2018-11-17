using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA.Debug_Monitor
{
    class DebugSwitchDisplay : DebugDisplay
    {
        const int FADE_TIME = 40;
        readonly static Color BG_DESELECTED_COLOR = new Color(80, 80, 80);

        protected intFunc UpdateFunction;
        private int Index = -1;
        protected List<FE_Text> Texts = new List<FE_Text>();
        protected List<Sprite> Bgs = new List<Sprite>();
        protected List<int> BgFadeTimes = new List<int>();
        protected List<Color> BgColors = new List<Color>();

        #region Accessors
        internal int index { set { Index = value; } }
        #endregion

        protected DebugSwitchDisplay() { }
        internal DebugSwitchDisplay(intFunc update_function, List<Tuple<int, string, Color>> commands)
        {
            UpdateFunction = update_function;

            int x = 0;
            foreach(var tuple in commands)
            {
                FE_Text text = new FE_Text();
                text.loc = new Vector2(x + 4, 0);
                text.Font = "FE7_Text";
                text.text = tuple.Item2;
                Texts.Add(text);

                Sprite bg = new Sprite();
                bg.loc = new Vector2(x, 0);
                bg.scale = new Vector2(tuple.Item1 / 16f, 1f);
                bg.tint = tuple.Item3;
                Bgs.Add(bg);

                BgFadeTimes.Add(Index == BgFadeTimes.Count ? FADE_TIME : 0);

                BgColors.Add(tuple.Item3);

                x += tuple.Item1;
            }
            Width = commands.Select(tuple => tuple.Item1).Sum();
        }

        internal override void update()
        {
            if (this.is_update_frame)
                index = UpdateFunction();

            for (int i = 0; i < Bgs.Count; i++)
            {
                if (Index == i)
                    BgFadeTimes[i] = FADE_TIME;
                else if (BgFadeTimes[i] > 0)
                    BgFadeTimes[i]--;

                int r = (int)((BgColors[i].R - BG_DESELECTED_COLOR.R) * (BgFadeTimes[i] / (float)FADE_TIME) + BG_DESELECTED_COLOR.R);
                int g = (int)((BgColors[i].G - BG_DESELECTED_COLOR.G) * (BgFadeTimes[i] / (float)FADE_TIME) + BG_DESELECTED_COLOR.G);
                int b = (int)((BgColors[i].B - BG_DESELECTED_COLOR.B) * (BgFadeTimes[i] / (float)FADE_TIME) + BG_DESELECTED_COLOR.B);
                Bgs[i].tint = new Color(r, g, b);
            }
        }

        internal override void draw(SpriteBatch sprite_batch, ContentManager content)
        {
            sprite_batch.Begin();
            for (int i = 0; i < Texts.Count; i++)
            {
                Bgs[i].draw(sprite_batch, content.Load<Texture2D>(@"Graphics/White_Square"), -loc);
                Texts[i].draw(sprite_batch,
                    content.Load<Texture2D>(string.Format(@"Graphics/Fonts/FE7_Text_{0}", i == Index ? "White" : "Grey")), -loc);
            }
            sprite_batch.End();
        }
    }
}
