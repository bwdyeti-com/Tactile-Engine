using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    class SoloAnim_Page : Unit_Page
    {
        protected FE_Text Class, Anim;
        protected Unit_Window_Inventory Inventory;
        protected int Unit_Id, Mode;

        protected Game_Unit unit { get { return Global.game_map.units[Unit_Id]; } }

        public SoloAnim_Page(Game_Unit unit)
        {
            Unit_Id = unit.id;
            // Class
            Class = new FE_Text();
            Class.loc = new Vector2(0, 0);
            Class.Font = "FE7_Text";
            Class.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Class.text = unit.actor.class_name;
            // Inventory
            Inventory = new Unit_Window_Inventory();
            Inventory.set_images(unit);
            Inventory.loc = new Vector2(88, 0);
            // Anim
            Mode = unit.actor.individual_animation;
            Anim = new FE_Text();
            Anim.loc = new Vector2(88 + 96 + 8, 0);
            Anim.Font = "FE7_Text";
            refresh();
        }

        protected void refresh()
        {
            Anim.text = Constants.OptionsConfig.OPTIONS_DATA[
                (int)Constants.Options.Animation_Mode].Options[Mode].Name;
            Anim.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_"  +
                (Mode == (int)Constants.Animation_Modes.Map ? "Grey" : "Green"));
            Anim.offset.X = Font_Data.text_width(Anim.text, Anim.Font) / 2;
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
