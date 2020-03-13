using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Help;

namespace FEXNA.Windows.UserInterface.Command.Config
{
    class GamepadRemapUINode : ConfigUINode
    {
        private FE_Text Text;
        private Button_Description Value;
        private Inputs Input;

        internal GamepadRemapUINode(
                string helpLabel,
                Inputs input,
                string str,
                int width)
            : base(helpLabel)
        {
            Input = input;

            RefreshButton();

            Text = new FE_Text();
            Text.Font = "FE7_Text";
            Text.texture = Global.Content.Load<Texture2D>(@"Graphics\Fonts\FE7_Text_White");
            Text.draw_offset = new Vector2(8, 0);
            Text.text = str;

            Size = new Vector2(width, 16);
        }

        public void RefreshButton()
        {
            Buttons button = FEXNA.Input.PadRedirect(Input);

            Button_Description buttonIcon = Button_Description.button(button);
            buttonIcon.description = "";
            buttonIcon.draw_offset = new Vector2(120, 0);
            buttonIcon.ColonVisible(false);
            Value = buttonIcon;
        }

        internal override void set_text_color(string color) { }

        protected override void update_graphics(bool activeNode)
        {
            Text.update();
            Value.Update();
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
            Value.Draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
