using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile
{
    class SoloAnim_Page : Unit_Page
    {
        protected TextSprite Class, Anim;
        protected Unit_Window_Inventory Inventory;
        protected int Unit_Id, Mode;

        protected Game_Unit unit { get { return Global.game_map.units[Unit_Id]; } }

        public SoloAnim_Page(Game_Unit unit)
        {
            Unit_Id = unit.id;
            // Class
            Class = new TextSprite();
            Class.loc = new Vector2(0, 0);
            Class.SetFont(Config.UI_FONT, Global.Content, "White");
            Class.text = unit.actor.class_name;
            // Inventory
            Inventory = new Unit_Window_Inventory();
            Inventory.set_images(unit);
            Inventory.loc = new Vector2(88, 0);
            // Anim
            Mode = unit.actor.individual_animation;
            Anim = new TextSprite();
            Anim.loc = new Vector2(88 + 96 + 8, 0);
            Anim.SetFont(Config.UI_FONT);
            refresh();
        }

        protected void refresh()
        {
            Anim.text = Constants.OptionsConfig.OPTIONS_DATA[
                (int)Constants.Options.Animation_Mode].Options[Mode].Name;
            Anim.SetColor(Global.Content, Mode == (int)Constants.Animation_Modes.Map ? "Grey" : "Green");
            Anim.offset.X = Anim.text_width / 2;
        }

        public override void update()
        {
            if (Mode != unit.actor.individual_animation)
            {
                Mode = unit.actor.individual_animation;
                refresh();
            }
            Inventory.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector();
            Inventory.draw(sprite_batch, draw_offset - loc, Scissor_State);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            Class.draw(sprite_batch, draw_offset - loc);
            Anim.draw(sprite_batch, draw_offset - loc);
            sprite_batch.End();
        }
    }
}
