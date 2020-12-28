using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Graphics.Windows
{
    internal class System_Color_Window : WindowPanel
    {
        public readonly static string FILENAME = @"Graphics/Windowskins/WindowPanel";

        public bool small = false;
        protected int Color_Override = -1;

        #region Accessors
        public int color_override
        {
            get { return Color_Override; }
            set
            {
                Color_Override = (int)MathHelper.Clamp(value, -1, Constants.Team.NUM_TEAMS - 1);
            }
        }
        public int window_color
        {
            get
            {
                return Color_Override != -1 ? Color_Override : Global.game_options.window_color;
            }
        }

        protected override Vector2 SrcOffset
        {
            get
            {
                return new Vector2(!small ? 0 : src_rect_size.X,
                    src_rect_size.Y * window_color);
            }
        }
        #endregion

        public System_Color_Window() : base(Global.Content.Load<Texture2D>(System_Color_Window.FILENAME)) { }
        public System_Color_Window(Texture2D texture) : base(texture) { }
        public System_Color_Window(Texture2D texture,
            int leftWidth, int centerWidth, int rightWidth,
            int topHeight, int centerHeight, int bottomHeight)
            : base(texture, Vector2.Zero,
                leftWidth, centerWidth, rightWidth,
                topHeight, centerHeight, bottomHeight) { }
    }
}
