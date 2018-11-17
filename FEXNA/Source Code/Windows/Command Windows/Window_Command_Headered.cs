using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;

namespace FEXNA.Windows.Command
{
    class Window_Command_Headered : Window_Command
    {
        private FE_Text Header;

        public Window_Command_Headered(
            string header, Vector2 loc, int width, List<string> strs)
            : base(loc, width, strs)
        {
            Header = new FE_Text();
            Header.draw_offset = new Vector2(8, -8);
            Header.Font = "FE7_Text";
            Header.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
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
