using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile.Windows.Map
{
    class WindowReadyUnits : Stereoscopic_Graphic_Object
    {
        private WindowPanel Window;
        private TextSprite Label, Count;

        public int Width { get { return 88; } }

        public WindowReadyUnits()
        {
            Window = new System_Color_Window();
            Window.offset = new Vector2(8, 8);
            Window.width = this.Width;
            Window.height = 32;

            Label = new TextSprite(
                Config.UI_FONT, Global.Content, "Yellow",
                new Vector2(0, 0),
                "Ready Units");
            string count_color = Global.game_map.ready_movable_units ? "Blue" : "Grey";

            int ready_units = Global.game_map.active_team
                .Select(x => Global.game_map.units[x])
                .Count(x => x.ready);
            Count = new RightAdjustedText(
                Config.UI_FONT, Global.Content, count_color,
                new Vector2(this.Width - 16, 0),
                ready_units.ToString());
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
