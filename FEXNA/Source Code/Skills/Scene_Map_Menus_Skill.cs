using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Windows.Command.Items;
using FEXNA.Windows.Target;
using IntExtension;
using FEXNAWeaponExtension;

namespace FEXNA
{
    partial class Scene_Map
    {
        protected void draw_menu_ranges_skill(SpriteBatch sprite_batch, int width, int timer, Rectangle rect)
        {
            // Skills: Swoop
            // Temp Swoop Range
            if (Global.game_temp.temp_skill_ranges.ContainsKey("SWOOP") &&
                UnitMenu.ShowSkillRange("SWOOP"))
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                int opacity = 144;
                Color color = new Color(opacity, opacity, opacity, opacity);
                foreach (Vector2 loc in Global.game_temp.temp_skill_ranges["SWOOP"])
                {
                    sprite_batch.Draw(Attack_Range_Texture,
                        loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                        rect, color);
                }
                sprite_batch.End();
            }
            // Skills: Trample
            // Temp Swoop Range
            if (Global.game_temp.temp_skill_ranges.ContainsKey("TRAMPLE") &&
                UnitMenu.ShowSkillRange("TRAMPLE"))
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                int opacity = 144;
                Color color = new Color(opacity, opacity, opacity, opacity);
                foreach (Vector2 loc in Global.game_temp.temp_skill_move_ranges["TRAMPLE"])
                {
                    sprite_batch.Draw(Move_Range_Texture,
                        loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                        rect, color);
                }
                foreach (Vector2 loc in Global.game_temp.temp_skill_ranges["TRAMPLE"])
                {
                    sprite_batch.Draw(Attack_Range_Texture,
                        loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                        rect, color);
                }
                sprite_batch.End();
            }
            // Skills: Old Swoop //@Debug
            // Temp Old Swoop Range
            if (Global.game_temp.temp_skill_ranges.ContainsKey("OLDSWOOP") &&
                UnitMenu.ShowSkillRange("OLDSWOOP"))
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                int opacity = 144;
                Color color = new Color(opacity, opacity, opacity, opacity);
                foreach (Vector2 loc in Global.game_temp.temp_skill_ranges["OLDSWOOP"])
                {
                    sprite_batch.Draw(Attack_Range_Texture,
                        loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                        rect, color);
                }
                sprite_batch.End();
            }
            // Skills: Masteries
            for (int i = 0; i < Game_Unit.MASTERIES.Count; i++)
            {
                string skill = Game_Unit.MASTERIES[i];
                if (UnitMenu.ShowSkillRange(skill))
                {
                    // Temp Skill Range
                    if (Global.game_temp.temp_skill_ranges.ContainsKey(skill))
                    {
                        sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                        int opacity = 144;
                        Color color = new Color(opacity, opacity, opacity, opacity);
                        foreach (Vector2 loc in Global.game_temp.temp_skill_ranges[skill])
                        {
                            sprite_batch.Draw(Attack_Range_Texture,
                                loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                                rect, color);
                        }
                        sprite_batch.End();
                    }
                    // Temp Attack Range (once the skill is active for menuing)
                    else
                    {
                        sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                        int opacity = 144;
                        Color color = new Color(opacity, opacity, opacity, opacity);
                        foreach (Vector2 loc in Global.game_temp.temp_attack_range)
                        {
                            sprite_batch.Draw(Attack_Range_Texture,
                                loc * TILE_SIZE + move_range_draw_vector() - Global.game_map.display_loc + new Vector2(0, width - timer),
                                rect, color);
                        }
                        sprite_batch.End();
                    }
                }
            }
        }
    }
}
