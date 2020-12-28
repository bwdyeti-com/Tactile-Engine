using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile
{
    class Skill_Gain_Popup : Popup
    {
        List<Icon_Sprite> Icons;
        TextSprite Text;

        public Skill_Gain_Popup(List<int> skill_ids)
        {
            initialize(skill_ids, true);
        }
        public Skill_Gain_Popup(List<int> skill_ids, bool battle_scene)
        {
            initialize(skill_ids, battle_scene);
        }

        protected void initialize(List<int> skill_ids, bool battle_scene)
        {
#if DEBUG
            if (skill_ids.Count > 1)
            { } //throw new System.Exception();
#endif
            Width = 80 + skill_ids.Count * Config.SKILL_ICON_SIZE;
            Timer_Max = 97;
            if (battle_scene)
                texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Combat_Popup");
            else
            {
                Window = new System_Color_Window();
                Window.width = Width;
                Window.height = 32;
            }

            // Skill icons
            Icons = new List<Icon_Sprite>();
            for (int i = 0; i < skill_ids.Count; i++)
            {
                var icon = new Icon_Sprite();
                if (Global.content_exists(@"Graphics/Icons/" + Global.data_skills[skill_ids[i]].Image_Name))
                    icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + Global.data_skills[skill_ids[i]].Image_Name);
                icon.size = new Vector2(Config.SKILL_ICON_SIZE, Config.SKILL_ICON_SIZE);
                icon.loc = (new Vector2(16) - icon.size / 2) + new Vector2(Config.SKILL_ICON_SIZE * i, 0);
                icon.index = Global.data_skills[skill_ids[i]].Image_Index;
                Icons.Add(icon);
            }
            // Text
            Text = new TextSprite();
            Text.loc = new Vector2(8 + skill_ids.Count * Config.SKILL_ICON_SIZE, 8);
            Text.SetFont(Config.UI_FONT, Global.Content, "White");
            Text.text = "Skill acquired.";
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (visible)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                if (Window != null)
                    Window.draw(sprite_batch, -(loc + draw_vector()));
                else
                    draw_panel(sprite_batch, Width);
                foreach (var icon in Icons)
                    icon.draw(sprite_batch, -(loc + draw_vector()));
                Text.draw(sprite_batch, -(loc + draw_vector()));
                sprite_batch.End();
            }
        }
    }
}
