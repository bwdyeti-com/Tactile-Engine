using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Windows.UserInterface.Command.Config
{
    class NumberUINode : ConfigLabeledUINode
    {
        protected TextSprite Value;

        internal NumberUINode(
                string helpLabel,
                string str,
                int width)
            : base(helpLabel, str)
        {
            Value = new TextSprite();
            Value.draw_offset = new Vector2(120, 0);
            Value.SetFont(Tactile.Config.UI_FONT, Global.Content, "White");
            set_value(-1);

            Size = new Vector2(width, 16);
        }

        protected override void set_label_color(string color)
        {
            base.set_label_color(color);
            set_text_color(color);
        }

        internal override void set_text_color(string color)
        {
            if (this.locked)
                color = "Grey";
            Value.SetColor(Global.Content, color);
        }

        internal virtual void set_value(int value)
        {
            Value.text = value.ToString();
        }
        internal virtual void set_text(string text)
        {
            Value.text = text;
        }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);
            Value.update();
        }

        protected override void mouse_off_graphic()
        {
            base.mouse_off_graphic();
            Value.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            base.mouse_highlight_graphic();
            Value.tint = Tactile.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();
            Value.tint = Tactile.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);
            Value.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
