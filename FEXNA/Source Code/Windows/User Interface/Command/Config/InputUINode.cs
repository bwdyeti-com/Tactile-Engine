using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Help;

namespace FEXNA.Windows.UserInterface.Command.Config
{
    class InputUINode : ConfigUINode
    {
        private Button_Description Text;
        private Keyboard_Icon Value;

        internal InputUINode(
                string helpLabel,
                Inputs input,
                string str,
                int width)
            : base(helpLabel)
        {
            var button = new Button_Description_360(input);
            button.description = str;
            button.draw_offset = new Vector2(8, 0);
            Text = button;

            Value = new Keyboard_Icon(
                input,
                Global.Content.Load<Texture2D>(
                    @"Graphics/Pictures/Buttons_Keyboard"), false);
            Value.refresh();
            Value.draw_offset = new Vector2(120, 0);

            Size = new Vector2(width, 16);
        }

        internal override void set_text_color(string color) { }

        protected override void update_graphics(bool activeNode)
        {
            Text.Update(false);
            Value.refresh();
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
            Text.Draw(sprite_batch, draw_offset - (loc + draw_vector()));
            Value.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
