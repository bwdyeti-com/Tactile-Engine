using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;

namespace FEXNA.Windows.Map
{
    class WindowReadyUnits : Stereoscopic_Graphic_Object
    {
        private WindowPanel Window;
        private FE_Text Label, Count;

        public int Width { get { return 88; } }

        public WindowReadyUnits()
        {
            Window = new System_Color_Window();
            Window.offset = new Vector2(8, 8);
            Window.width = this.Width;
            Window.height = 32;

            Label = new FE_Text("FE7_Text",
                Global.Content.Load<Texture2D>(@"Graphics\Fonts\FE7_Text_Yellow"),
                new Vector2(0, 0), "Ready Units");
            int ready_units = Global.game_map.active_team
                .Select(x => Global.game_map.units[x])
                .Count(x => x.ready);
            string count_color = ready_units > 0 ? "Blue" : "Grey";
            Count = new FE_Text_Int("FE7_Text",
                Global.Content.Load<Texture2D>(
                    string.Format(@"Graphics\Fonts\FE7_Text_{0}", count_color)),
                new Vector2(this.Width - 16, 0), ready_units.ToString());
        }

        public void update()
        {
            Window.update();

            Label.update();
            Count.update();
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 draw_offset = default(Vector2))
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Window.draw(spriteBatch, draw_offset - this.loc);

            Label.draw(spriteBatch, draw_offset - this.loc);
            Count.draw(spriteBatch, draw_offset - this.loc);
            spriteBatch.End();
        }
    }
}
