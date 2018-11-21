//Sparring
using Microsoft.Xna.Framework;
using FEXNA.Graphics.Windows;

namespace FEXNA.Graphics.Preparations
{
    class Sparring_Stats_Window : Prep_Stats_Window
    {
        public Sparring_Stats_Window(Game_Unit unit) : base(unit) { }

        #region Accessors
        public bool darkened
        {
            set
            {
                ((Prepartions_Item_Window)Stats_Window).darkened = value;
            }
        }
        #endregion

        protected override int WIDTH()
        {
            return 152;
        }
        protected override int SPACING()
        {
            return 72;
        }
        protected override int HEIGHT()
        {
            return 112;
        }

        protected override void initialize_window()
        {
            Stats_Window = new Prepartions_Item_Window(true);
            Stats_Window.loc = new Vector2(0, 0);
            Stats_Window.width = WIDTH() - 8;
            Stats_Window.height = HEIGHT() - 8;
            Stats_Window.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;

            Stats_Window.offset = new Vector2(-4, 32 - 4);
        }
    }
}
