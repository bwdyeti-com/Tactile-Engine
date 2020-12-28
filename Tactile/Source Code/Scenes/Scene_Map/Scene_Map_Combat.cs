using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    partial class Scene_Map
    {
        private Map_Combat_HUD Combat_HUD;
        private Spark Map_Miss_Spark, Map_NoDamage_Spark;
        protected Exp_Gauge Exp_Gauge;

        public void create_hud(Combat_Data data)
        {
            Combat_HUD = new Map_Combat_HUD(new List<Texture2D> {
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Map_Combat_HUD"),
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/" + Config.UI_FONT + "_" + "White"), //@Debug
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/" + Config.COMBAT_DIGITS_FONT)}, //@Debug
                data);
        }
        public void create_hud(int unit_id)
        {
            Combat_HUD = new Map_Combat_HUD(new List<Texture2D> {
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Map_Combat_HUD"),
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/" + Config.UI_FONT + "_" + "White"), //@Debug
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/" + Config.COMBAT_DIGITS_FONT)}, //@Debug
                unit_id);
        }
        public void create_hud(int unit_id1, int unit_id2)
        {
            Combat_HUD = new Map_Combat_HUD(new List<Texture2D> {
                Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Map_Combat_HUD"),
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/" + Config.UI_FONT + "_" + "White"), //@Debug
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/" + Config.COMBAT_DIGITS_FONT)}, //@Debug
                unit_id1, unit_id2);
        }

        public void create_miss_map_spark(Vector2 loc)
        {
            Map_Miss_Spark = new Miss_Map_Spark();
            Map_Miss_Spark.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + Miss_Map_Spark.FILENAME);
            Map_Miss_Spark.loc = loc + new Vector2(-23, 1);
        }

        public void create_nodamage_map_spark(Vector2 loc)
        {
            Map_NoDamage_Spark = new NoDamage_Map_Spark();
            Map_NoDamage_Spark.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + NoDamage_Map_Spark.FILENAME);
            Map_NoDamage_Spark.loc = loc + new Vector2(-34, 1);
        }

        public void create_exp_gauge(int base_exp)
        {
            Exp_Gauge = new Exp_Gauge(base_exp);
            Exp_Gauge.stereoscopic = Config.BATTLE_EXP_DEPTH;
        }

        public void gain_exp()
        {
            if (Exp_Gauge != null)
                Exp_Gauge.exp++;
        }

        public void lose_exp()
        {
            if (Exp_Gauge != null)
                Exp_Gauge.exp--;
        }

        public void set_hud_attack_id(int id)
        {
            if (Combat_HUD != null)
            {
                Combat_HUD.set_attack_id(id);
                Combat_HUD.update_battle_stats();
            }
        }

        public void set_hud_action_id(int id)
        {
            if (Combat_HUD != null)
                Combat_HUD.set_action_id(id);
        }

        public void refresh_hud()
        {
            if (Combat_HUD != null)
                Combat_HUD.update_battle_stats();
        }

        public void hud_crit_shake()
        {
            if (Combat_HUD != null)
                Combat_HUD.shake();
        }

        public bool combat_hud_ready()
        {
            if (Map_Miss_Spark != null || Map_NoDamage_Spark != null)
                return false;
            if (Combat_HUD != null)
                return Combat_HUD.is_ready();
            return true;
        }

        private void update_combat()
        {
            if (Map_Miss_Spark != null)
            {
                Map_Miss_Spark.update();
                if (Map_Miss_Spark.completed())
                    Map_Miss_Spark = null;
            }
            if (Map_NoDamage_Spark != null)
            {
                Map_NoDamage_Spark.update();
                if (Map_NoDamage_Spark.completed())
                    Map_NoDamage_Spark = null;
            }
            if (Combat_HUD != null)
                Combat_HUD.update();
            if (Exp_Gauge != null)
                Exp_Gauge.update();
            update_skill_gain();
            update_wlvl();
            update_wbreak();
        }

        #region Skill Gain
        private bool SkillGainCalling = false;
        private bool SkillGaining = false;
        private int SkillGainAction = 0;
        private int SkillGainTimer = 0;
        private List<int> GainedSkills;
        private Popup SkillGainPopup;

        public void skill_gain(int id)
        {
            if (!is_skill_gaining())
            {
                GainedSkills = Global.game_map.units[id].actor.skills_gained_on_level().ToList();
                SkillGainCalling = true;
            }
        }
        public bool promotion_skill_gain(int id, int old_class_id, int old_level)
        {
            if (!is_skill_gaining())
                if (Global.game_map.units[id].actor.skills_gained_on_promotion(old_class_id, old_level).Any())
                {
                    GainedSkills = Global.game_map.units[id].actor.skills_gained_on_promotion(old_class_id, old_level).ToList();
                    SkillGainCalling = true;
                    return true;
                }
            return false;
        }

        public bool is_skill_gaining()
        {
            return SkillGainCalling || SkillGaining;
        }

        protected virtual void create_skill_gain_popup(List<int> skill_ids)
        {
            SkillGainPopup = new Skill_Gain_Popup(skill_ids, false);
            SkillGainPopup.loc = new Vector2((Config.WINDOW_WIDTH - SkillGainPopup.Width) / 2, 80);
        }

        private void update_skill_gain()
        {
            if (is_leveling_up())
                return;
            if (SkillGainCalling)
            {
                SkillGaining = true;
                SkillGainCalling = false;
            }
            if (SkillGaining)
            {
                bool cont = false;
                while (!cont)
                {
                    switch (SkillGainAction)
                    {
                        case 0:
                            switch (SkillGainTimer)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                case 9:
                                case 10:
                                case 11:
                                case 12:
                                case 13:
                                case 14:
                                case 15:
                                case 16:
                                case 17:
                                    SkillGainTimer++;
                                    break;
                                case 18:
                                    Global.game_system.play_se(System_Sounds.Gain);
                                    create_skill_gain_popup(GainedSkills);
                                    SkillGainAction = 1;
                                    SkillGainTimer = 0;
                                    break;
                            }
                            break;
                        case 1:
                            if (SkillGainPopup == null)
                                SkillGainAction = 2;
                            break;
                        case 2:
                            SkillGaining = false;
                            SkillGainAction = 0;
                            SkillGainTimer = 0;
                            GainedSkills.Clear();
                            break;
                    }
                    cont = true;
                }
                if (SkillGainPopup != null)
                {
                    SkillGainPopup.update();
                    if (SkillGainPopup.finished)
                        SkillGainPopup = null;
                }
            }
        }

        protected void draw_skill_gain_popup(SpriteBatch sprite_batch)
        {
            if (SkillGainPopup != null)
            {
                SkillGainPopup.draw(sprite_batch);
            }
        }
        #endregion

        #region Wlvl
        private bool WLvl_Calling = false;
        private bool WLvling_Up = false;
        private int WLvl_Action = 0;
        private int WLvl_Timer = 0;
        private int WLvl_Battler_Id = -1;
        protected Popup WLvl_Popup;

        public void wlvl_up(int id)
        {
            if (!is_wlvling_up())
            {
                WLvl_Battler_Id = id;
                WLvl_Calling = true;
            }
        }

        public bool is_wlvling_up()
        {
            return WLvl_Calling || WLvling_Up;
        }

        protected virtual void create_wlvl_popup(int weapon, int newRank)
        {
            WLvl_Popup = new Weapon_Level_Popup(weapon, newRank, false);
            WLvl_Popup.loc = new Vector2((Config.WINDOW_WIDTH - WLvl_Popup.Width) / 2, 80);
        }

        private void update_wlvl()
        {
            if (WLvl_Calling)
            {
                WLvling_Up = true;
                WLvl_Calling = false;
            }
            if (WLvling_Up)
            {
                bool cont = false;
                while (!cont)
                {
                    switch (WLvl_Action)
                    {
                        case 0:
                            switch (WLvl_Timer)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                case 9:
                                case 10:
                                case 11:
                                case 12:
                                case 13:
                                case 14:
                                case 15:
                                case 16:
                                case 17:
                                    WLvl_Timer++;
                                    break;
                                case 18:
                                    Global.game_system.play_se(System_Sounds.Gain);
                                    int type_index = Global.game_map.units[WLvl_Battler_Id].actor.wlvl_up_type();
                                    var type = Global.weapon_types[type_index];
                                    int rank = Global.game_map.units[WLvl_Battler_Id].actor.get_weapon_level(type);
                                    create_wlvl_popup(type_index, rank);
                                    WLvl_Action = 1;
                                    WLvl_Timer = 0;
                                    break;
                            }
                            break;
                        case 1:
                            if (WLvl_Popup == null)
                                WLvl_Action = 2;
                            break;
                        case 2:
                            WLvling_Up = false;
                            WLvl_Action = 0;
                            WLvl_Timer = 0;
                            Global.game_map.units[WLvl_Battler_Id].actor.clear_wlvl_up();
                            WLvl_Battler_Id = -1;
                            break;
                    }
                    cont = true;
                }
                if (WLvl_Popup != null)
                {
                    WLvl_Popup.update();
                    if (WLvl_Popup.finished)
                        WLvl_Popup = null;
                }
            }
        }

        protected void draw_wlvl_popup(SpriteBatch sprite_batch)
        {
            if (WLvl_Popup != null)
            {
                WLvl_Popup.draw(sprite_batch);
            }
        }
        #endregion

        #region Weapon Break
        private bool WBreak_Calling = false;
        private bool WBreaking = false;
        private int WBreak_Action = 0;
        private int WBreak_Timer = 0;
        protected int[] WBreak_Item = new int[0];
        protected Popup WBreak_Popup;

        public void wbreak(int[] item_data)
        {
            if (!is_wbreaking())
            {
                WBreak_Item = item_data;
                WBreak_Calling = true;
            }
        }

        public bool is_wbreaking()
        {
            return WBreak_Calling || WBreaking;
        }

        protected virtual void create_wbreak_popup()
        {
            WBreak_Popup = new Item_Break_Popup(WBreak_Item[1], WBreak_Item[0] == 1, false);
            WBreak_Popup.loc = new Vector2(96, 80);
        }

        private void update_wbreak()
        {
            if (WBreak_Calling)
            {
                WBreaking = true;
                WBreak_Calling = false;
            }
            if (WBreaking)
            {
                bool cont = false;
                while (!cont)
                {
                    switch (WBreak_Action)
                    {
                        case 0:
                            switch (WBreak_Timer)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                case 9:
                                case 10:
                                case 11:
                                case 12:
                                case 13:
                                case 14:
                                case 15:
                                case 16:
                                case 17:
                                    WBreak_Timer++;
                                    break;
                                case 18:
                                    Global.game_system.play_se(System_Sounds.Loss);
                                    create_wbreak_popup();
                                    WBreak_Action = 1;
                                    WBreak_Timer = 0;
                                    break;
                            }
                            break;
                        case 1:
                            if (WBreak_Popup == null)
                                WBreak_Action = 2;
                            break;
                        case 2:
                            WBreaking = false;
                            WBreak_Action = 0;
                            WBreak_Timer = 0;
                            WBreak_Item = new int[0];
                            break;
                    }
                    cont = true;
                }
                if (WBreak_Popup != null)
                {
                    WBreak_Popup.update();
                    if (WBreak_Popup.finished)
                        WBreak_Popup = null;
                }
            }
        }

        protected void draw_wbreak_popup(SpriteBatch sprite_batch)
        {
            if (WBreak_Popup != null)
            {
                WBreak_Popup.draw(sprite_batch);
            }
        }
        #endregion

        public void clear_combat()
        {
            Map_Miss_Spark = null;
            Map_NoDamage_Spark = null;
            Combat_HUD = null;
            Exp_Gauge = null;
        }

        public void clear_exp()
        {
            Exp_Gauge = null;
        }

        protected virtual void draw_map_combat(SpriteBatch sprite_batch)
        {
            if (Map_Miss_Spark != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Map_Miss_Spark.draw(sprite_batch, Global.game_map.display_loc);
                sprite_batch.End();
            }
            if (Map_NoDamage_Spark != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Map_NoDamage_Spark.draw(sprite_batch, Global.game_map.display_loc);
                sprite_batch.End();
            }
            draw_skill_gain_popup(sprite_batch);
            draw_wlvl_popup(sprite_batch);
            draw_wbreak_popup(sprite_batch);
            if (Combat_HUD != null)
            {
                sprite_batch.GraphicsDevice.ScissorRectangle = Combat_HUD.scissor_rect();
                if (sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0 && sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0)
                {
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Combat_HUD.raster_state);
                    Combat_HUD.draw(sprite_batch);
                    sprite_batch.End();
                }
            }
            if (Exp_Gauge != null)
            {
                sprite_batch.GraphicsDevice.ScissorRectangle = Exp_Gauge.scissor_rect();
                if (sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0 && sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0)
                {
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Exp_Gauge.raster_state);
                    Exp_Gauge.draw(sprite_batch);
                    sprite_batch.End();
                }
            }
        }
    }
}
