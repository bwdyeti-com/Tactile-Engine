using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Windows;

namespace FEXNA.Graphics.Preparations
{
    class Prep_Support_List_Window : Graphic_Object
    {
        const int WIDTH = 136;
        const int HEIGHT = 96;

        protected System_Color_Window Stats_Window;
        private Status_Support_List Supports;

        public Prep_Support_List_Window(Game_Actor actor)
        {
            // Stats Window
            initialize_window();
            // Bonuses
            Supports = new Status_Support_List();
            Supports.loc = Stats_Window.loc + new Vector2((WIDTH - 80) / 2, 8);
            Supports.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;

            set_images(actor);
        }

        protected virtual void initialize_window()
        {
            Stats_Window = new Prepartions_Item_Window(true);
            Stats_Window.loc = new Vector2(8, 0);
            Stats_Window.width = WIDTH - 8;
            Stats_Window.height = HEIGHT - 8;
            Stats_Window.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;

            Stats_Window.offset = new Vector2(-4, -4);
        }

        public void set_images(Game_Actor actor)
        {
            Supports.set_images(actor);
        }

        public void update()
        {
            // Stats Window
            Stats_Window.update();
            Supports.update();
        }

        public void draw(SpriteBatch sprite_batch)
        {
            // Stats Window
            Stats_Window.draw(sprite_batch, -this.loc);
            Supports.draw(sprite_batch, -this.loc);
        }
    }
}
