using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA.Windows.UserInterface.Command.Config
{
    class ConfigTextUINode : ConfigUINode
    {
        protected FE_Text Text, Value;

        internal ConfigTextUINode(
                string helpLabel,
                string str,
                int width)
            : base(helpLabel)
        {
            Text = new FE_Text();
            Text.Font = "FE7_Text";
            Text.texture = Global.Content.Load<Texture2D>(@"Graphics\Fonts\FE7_Text_White");
            Text.text = str;

            Value = new FE_Text();
            Value.draw_offset = new Vector2(120, 0);
            Value.Font = "FE7_Text";
            Value.texture = Global.Content.Load<Texture2D>(@"Graphics\Fonts\FE7_Text_White");

            Size = new Vector2(width, 16);
        }

        protected override void set_label_color(string color)
        {
            base.set_label_color(color);
            Text.texture = Global.Content.Load<Texture2D>(
                string.Format(@"Graphics/Fonts/FE7_Text_{0}", color));
        }

        internal override void set_text_color(string color)
        {
            if (this.locked)
                color = "Grey";
            Value.texture = Global.Content.Load<Texture2D>(
                string.Format(@"Graphics/Fonts/FE7_Text_{0}", color));
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
            Text.tint = FEXNA.Config.MOUSE_OVER_ELEMENT_COLOR;
            Value.tint = FEXNA.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            Text.tint = FEXNA.Config.MOUSE_PRESSED_ELEMENT_COLOR;
            Value.tint = FEXNA.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Text.draw(sprite_batch, draw_offset - (loc + draw_vector()));
            Value.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
