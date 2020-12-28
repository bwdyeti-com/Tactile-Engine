using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Windows.UserInterface.Command.Config
{
    class ConfigTextUINode : ConfigUINode
    {
        protected TextSprite Text, Value;

        internal ConfigTextUINode(
                string helpLabel,
                string str,
                int width)
            : base(helpLabel)
        {
            Text = new TextSprite();
            Text.SetFont(Tactile.Config.UI_FONT, Global.Content, "White");
            Text.text = str;

            Value = new TextSprite();
            Value.draw_offset = new Vector2(120, 0);
            Value.SetFont(Tactile.Config.UI_FONT, Global.Content, "White");

            Size = new Vector2(width, 16);
        }

        protected override void set_label_color(string color)
        {
            base.set_label_color(color);
            Text.SetColor(Global.Content, color);
        }

        internal override void set_text_color(string color)
        {
            if (this.locked)
                color = "Grey";
            Value.SetColor(Global.Content, color);
        }

        internal void set_text(string str)
        {
            Value.text = str;
        }

        protected override void update_graphics(bool activeNode)
        {
            Text.update();
            Value.update();
        }

        protected override void mouse_off_graphic()
        {
            Text.tint = Color.White;
            Value.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            Text.tint = Tactile.Config.MOUSE_OVER_ELEMENT_COLOR;
            Value.tint = Tactile.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            Text.tint = Tactile.Config.MOUSE_PRESSED_ELEMENT_COLOR;
            Value.tint = Tactile.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Text.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            Value.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
