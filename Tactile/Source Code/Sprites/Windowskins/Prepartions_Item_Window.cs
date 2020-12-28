using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Graphics.Windows
{
    class Prepartions_Item_Window : System_Color_Window
    {
        protected bool Item_Window = false;
        protected bool Darkened = false;

        #region Accessors
        public bool darkened { set { Darkened = value; } }

        protected override Vector2 SrcOffset
        {
            get
            {
                return new Vector2(
                    (!Item_Window ? 0 : src_rect_size.X) +
                        (!Darkened ? 0 : (src_rect_size.X * 2)),
                    src_rect_size.Y * window_color);
            }
        }
        #endregion

        public Prepartions_Item_Window(bool item_window)
        {
            Item_Window = item_window;
            texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Trade_Window");
        }
    }
}
