using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Gauges;

namespace Tactile.Windows.UserInterface.Command.Config
{
    class SliderUINode : NumberUINode
    {
        private Slider Bar;
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

            Text = new TextSprite();
            Text.SetFont(Tactile.Config.UI_FONT, Global.Content, "White");
            Text.text = str;

            Bar = new Slider();
            Bar.offset = new Vector2(-120 - 2, -8);
            Bar.bar_width = gaugeWidth;

            Value = new RightAdjustedText();
            Value.draw_offset = new Vector2(120 + gaugeWidth + 8 + 24, 0);
            Value.SetFont(Tactile.Config.UI_FONT, Global.Content, "White");
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

        protected override void mouse_off_graphic()
        {
            base.mouse_off_graphic();
            Bar.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            base.mouse_highlight_graphic();
            Bar.tint = Tactile.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();
            Bar.tint = Tactile.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);
            Bar.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
