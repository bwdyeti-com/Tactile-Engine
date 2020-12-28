using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile.Windows.Command
{
    class Window_Command_Headered : Window_Command
    {
        private TextSprite Header;

        public Window_Command_Headered(
            string header, Vector2 loc, int width, List<string> strs)
            : base(loc, width, strs)
        {
            Header = new TextSprite();
            Header.draw_offset = new Vector2(8, -8);
            Header.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Header.text = header;
        }

        protected override void initialize_window()
        {
            Window_Img = new SystemWindowHeadered();
            Window_Img.offset = new Vector2(0, 16);
        }

        protected override void draw_text(SpriteBatch sprite_batch)
        {
            base.draw_text(sprite_batch);
            Header.draw(sprite_batch, -(loc + text_draw_vector()));
        }
    }
}
