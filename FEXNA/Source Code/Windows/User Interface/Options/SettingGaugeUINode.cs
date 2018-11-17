using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA.Windows.UserInterface.Options
{
    class SettingGaugeUINode : SettingUINode
    {
        private Stat_Bar Bar;
        private string FormatString;
        private int GaugeMin, GaugeMax;

        protected override bool IsSlider { get { return true; } }

        internal SettingGaugeUINode(string label,
            int gaugeWidth, int gaugeMin, int gaugeMax, int textOffset)
        {
            FormatString = label;
            GaugeMin = gaugeMin;
            GaugeMax = gaugeMax;

            Bar = new Stat_Bar();
            Bar.offset = new Vector2(-2, -8);
            Bar.bar_width = gaugeWidth;

            Label = new FE_Text_Int("FE7_Text",
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey"),
                new Vector2(textOffset, 0),
                "");

            Size = new Vector2(Math.Max(gaugeWidth, textOffset), 16);
        }

        internal void refresh_value(int value)
        {
            Bar.fill_width = Bar.bar_width * (value - GaugeMin) / (GaugeMax - GaugeMin);
            Label.text = string.Format(FormatString, value);
        }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);
            Bar.update();
        }

        protected override float slide(Vector2 inputPosition, Vector2 drawOffset)
        {
            Vector2 slider_position = (inputPosition - HitBoxLoc(drawOffset + Bar.offset));
            return (slider_position.X - 4) / (Bar.bar_width - 8);
        }

        protected override void mouse_off_graphic()
        {
            //Bar.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            //Bar.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            //Bar.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);
            Bar.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
