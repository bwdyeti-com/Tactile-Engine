using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Windows.UserInterface.Command.Config
{
    class SubmenuUINode : ConfigLabeledUINode
    {
        private Sprite Button;

        internal SubmenuUINode(
                string helpLabel,
                string str,
                int width)
            : base(helpLabel, str)
        {
            Button = new Sprite();
            Button.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/UIButtons");
            Button.src_rect = new Rectangle(0, 64, 16, 16);
            Button.draw_offset = new Vector2(120, 0);

            Size = new Vector2(width, 16);
        }
        
        internal override void set_text_color(string color) { }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);
            Button.update();
        }

        protected override void mouse_off_graphic()
        {
            base.mouse_off_graphic();
            Button.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            base.mouse_highlight_graphic();
            Button.tint = Tactile.Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();
            Button.tint = Tactile.Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);
            Button.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
