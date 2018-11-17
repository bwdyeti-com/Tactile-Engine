using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Windows
{
    class Data_Ranking_Window : WindowPanel
    {
        readonly static string FILENAME = @"Graphics/Windowskins/Data_Ranking_Window";

        public Data_Ranking_Window()
            : base(Global.Content.Load<Texture2D>(Data_Ranking_Window.FILENAME))
        {
            TopHeight = 24;
        }
    }
}
