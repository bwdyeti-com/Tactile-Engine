using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Unit_Page_3 : Unit_Page
    {
        protected Unit_Window_Inventory Inventory;
        protected List<Icon_Sprite> Skill_Icons = new List<Icon_Sprite>();

        public Unit_Page_3(Game_Unit unit)
        {
            // Inventory
            Inventory = new Unit_Window_Inventory();
            Inventory.set_images(unit);
            // Skills
            foreach (int i in unit.actor.skills)
            {
                Skill_Icons.Add(new Icon_Sprite());
                if (Global.content_exists(@"Graphics/Icons/" + Global.data_skills[i].Image_Name))
                    Skill_Icons[Skill_Icons.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + Global.data_skills[i].Image_Name);
                Skill_Icons[Skill_Icons.Count - 1].size = new Vector2(Config.SKILL_ICON_SIZE, Config.SKILL_ICON_SIZE);
                Skill_Icons[Skill_Icons.Count - 1].loc = new Vector2(
                    104 + (Skill_Icons.Count - 1) * Config.SKILL_ICON_SIZE,
                    (16 - Config.SKILL_ICON_SIZE) / 2);
                Skill_Icons[Skill_Icons.Count - 1].index = Global.data_skills[i].Image_Index;
            }
        }

        public override void update()
        {
            Inventory.update();
            foreach (Icon_Sprite icon in Skill_Icons)
                icon.update();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Vector2 loc = this.loc + draw_vector();
            Inventory.draw(sprite_batch, draw_offset - loc);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State);
            foreach (Icon_Sprite icon in Skill_Icons)
                icon.draw(sprite_batch, draw_offset - loc);
            sprite_batch.End();
        }
    }
}
