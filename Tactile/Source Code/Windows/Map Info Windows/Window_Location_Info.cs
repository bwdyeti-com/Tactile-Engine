#if !MONOGAME && DEBUG
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Windows.Map.Info
{
    class Window_Location_Info : Window_Objective_Info
    {
        protected override void draw_images()
        {
            Objective.text = "X:" + (int)Global.player.loc.X + ", Y:" + (int)Global.player.loc.Y;
            Objective.offset.X = Font_Data.text_width(Objective.text, Config.INFO_FONT) / 2;
            Window_Img.lines = 1;
            refresh_positions();
        }
    }
}
#endif