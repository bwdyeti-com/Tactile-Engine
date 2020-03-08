using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA.Windows.UserInterface.Command.Config
{
    class SliderUINode : NumberUINode
    {
        private Stat_Bar Bar;
        private int GaugeMin, GaugeMax;

        internal SliderUINode(
                string helpLabel,
                string str,
                int width,
                int gaugeMin,
                int gaugeMax,
                int gaugeWidth)
            : base(helpLabel, str, width)
        {
            GaugeMin = gaugeMin;
            GaugeMax = gaugeMax;

            Text = new FE_Text();
            Text.Font = "FE7_Text";
            Text.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Fonts\FE7_Text_White");
            Text.text = str;

            Bar = new Stat_Bar();
            Bar.offset = new Vector2(-120 - 2, -8);
            Bar.bar_width = gaugeWidth;

            Value = new FE_Text_Int();
            Value.draw_offset = new Vector2(120 + gaugeWidth + 8 + 24, 0);
            Value.Font = "FE7_Text";
            Value.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Fonts\FE7_Text_White");
            set_value(-1);

            Size = new Vector2(width, 16);
        }
        
        internal void set_value(int value, string text)
        {
            Value.text = text;

            if (GaugeMax != GaugeMin)
                Bar.SetFillWidth(Bar.bar_width, value, GaugeMin, GaugeMax);
            else
                Bar.SetFillWidth(0);
        }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);
            Bar.update();
        }
        
        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);
            Bar.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
