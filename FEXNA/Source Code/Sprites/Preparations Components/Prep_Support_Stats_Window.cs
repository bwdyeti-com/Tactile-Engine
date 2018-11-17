using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Windows;

namespace FEXNA.Graphics.Preparations
{
    class Prep_Support_Stats_Window : Graphic_Object
    {
        const int WIDTH = 152;
        const int HEIGHT = 112;

        protected System_Color_Window Stats_Window;
        private Status_Support_Bonuses Bonuses;
        protected Status_Bonus_Background Bonus_Bg;

        public Prep_Support_Stats_Window(Game_Unit unit)
        {
            // Stats Window
            initialize_window();
            // Bonuses
            Bonuses = new Status_Support_Bonuses();
            Bonuses.loc = Stats_Window.loc + new Vector2(20, 8);
            Bonuses.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;
            // Bonus Bg
            Bonus_Bg = new Status_Bonus_Background();
            Bonus_Bg.loc = Stats_Window.loc + new Vector2(12, -8);
            Bonus_Bg.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;

            set_images(unit.actor);
        }

        protected virtual void initialize_window()
        {
            Stats_Window = new Prepartions_Item_Window(true);
            Stats_Window.loc = new Vector2(0, 0);
            Stats_Window.width = WIDTH - 8;
            Stats_Window.height = HEIGHT - 8;
            Stats_Window.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;

            Stats_Window.offset = new Vector2(-4, 32 - 4);
        }

        public void set_images(Game_Actor actor, Game_Actor target = null)
        {
            Bonuses.set_images(null, actor);
            Bonuses.set_next_level_bonus(actor, target);
        }

        public void update()
        {
            // Stats Window
            Stats_Window.update();
            Bonuses.update();
        }

        public void draw(SpriteBatch sprite_batch)
        {
            // Stats Window
            Stats_Window.draw(sprite_batch, -this.loc);
            Bonus_Bg.draw(sprite_batch, -this.loc);
            Bonuses.draw(sprite_batch, -this.loc);

        }
    }
}
