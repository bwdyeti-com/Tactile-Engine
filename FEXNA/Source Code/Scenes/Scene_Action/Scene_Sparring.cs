//Sparring
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Scene_Sparring : Scene_Arena
    {
        const int MAX_SKIP_ROUNDS = 1024;
        internal const int HEALER_STAFF_ID = 151;

        private bool Sparring_Level_Up;
        private Window_Sparring_Result Sparring_Result;

        public Scene_Sparring() { }

        protected override void initialize_base()
        {
            base.initialize_base();
            Scene_Type = "Scene_Sparring";
        }

        public override void initialize_action(int distance)
        {
            base.initialize_action(distance);
            Can_Skip = true;
        }

        protected override void update_skip()
        {
            if (Phase > 1)
                Can_Skip = false;
            base.update_skip();
            if (Skipping)
            {
                if (Arena_Timer > 0)
                {
                    new_combat_data();
                    Arena_Timer = 0;
                }
                if (Skip_Timer == 39)
                {
                    while (!Combat_Data.kill)
                    {
                        if (Arena_Combat_Round >= MAX_SKIP_ROUNDS)
                        {
                            Combat_Data.Wexp1 = 10;
                            Combat_Data.Wexp2 = 10;
                            break;
                        }
                        Combat_Data.apply_combat(false, true);
                        new_combat_data();
                    }
                    end_skip();
                    Timer = 0;
                    return;
                }
            }
        }

        public override void update()
        {
            base.update();
        }

        protected override bool exp_gained()
        {
            return false;
        }

        protected override void crowd_cheer() { }

        protected override void end_crowd_cheer() { }

        #region Phase Updates
        protected override void post_round_skip_deny() { }

        protected override void update_yield() { }

        protected override void new_combat_data(int distance, bool initial)
        {
            Arena_Combat_Round++;
            int exp1 = 0, exp2 = 0, wexp1 = 0, wexp2 = 0;
            if (Combat_Data != null)
            {
                exp1 = Combat_Data.Exp_Gain1;
                exp2 = Combat_Data.Exp_Gain2;
                wexp1 = Combat_Data.Wexp1;
                wexp2 = Combat_Data.Wexp2;
            }
            Combat_Data = new Sparring_Combat_Data(Battler_1.id, Battler_2.id, distance);
            Combat_Data.Exp_Gain1 = Math.Max(exp1, Combat_Data.Exp_Gain1);
            Combat_Data.Exp_Gain2 = Math.Max(exp2, Combat_Data.Exp_Gain2);
            Combat_Data.Wexp1 += wexp1;
            Combat_Data.Wexp2 += wexp2;
            Global.game_state.combat_data = Combat_Data;
            if (!initial)
            {
                Attack_Id = 0;
                if (HUD != null)
                {
                    HUD.combat_data = Combat_Data;
                    refresh_stats();
                }
            }
        }

        protected override void apply_combat()
        {
            if (Combat_Data.Hp1 > 0 && Combat_Data.Hp2 > 0)
            {
                Combat_Data.apply_combat(false, true);
                // should Cancel propagate between rounds? //Yeti
                Phase = 1;
                Segment = 0;
                Timer = 0;
                Arena_Timer = 30;
            }
            else
                Combat_Data.apply_combat(false, true);
        }

        protected override void update_phase_3()
        {
            if (Sparring_Result == null)
                Sparring_Result = new Window_Sparring_Result(Combat_Data);
            else
            {
                Sparring_Result.update();
                if (Sparring_Result.level_up_index != -1)
                {
                    if (Sparring_Level_Up)
                    {
                        if (!is_leveling_up() && !is_skill_gaining())
                        {
                            Sparring_Result.level_up_finished();
                            Sparring_Level_Up = false;
                        }
                    }
                    if (!Sparring_Level_Up)
                        switch(Sparring_Result.level_up_index)
                        {
                            case 1:
                                sparring_level_up(Battler_1);
                                break;
                            case 2:
                                sparring_level_up(Battler_2);
                                break;
                            case 3:
                                sparring_level_up(Global.game_map.units[Global.game_map.get_unit_id_from_actor(Window_Sparring.Healer_Id)]);
                                break;
                            default:
                                Sparring_Result.level_up_finished();
                                break;
                        }
                }
                else if (Sparring_Result.promotion_indices.Count > 0)
                {
                    if (!Sparring_Level_Up)
                    {
                        List<int> unit_ids = new List<int>();
                        foreach (int id in Sparring_Result.promotion_indices)
                            switch (id)
                            {
                                case 1:
                                    unit_ids.Add(Battler_1.id);
                                    break;
                                case 2:
                                    unit_ids.Add(Battler_2.id);
                                    break;
                                case 3:
                                    unit_ids.Add(Global.game_map.get_unit_id_from_actor(Window_Sparring.Healer_Id));
                                    break;
                                default:
                                    Sparring_Result.level_up_finished();
                                    break;
                            }
                        if (unit_ids.Count > 0)
                        {
                            Level_Up_Spark_Played = false;
                            Sparring_Level_Up = true;
                            promote(unit_ids, -1);
                        }
                    }
                }
                else if (Sparring_Result.finished)
                    base.update_phase_3();
            }
        }
        #endregion

        private void sparring_level_up(Game_Unit battler)
        {
            level_up(battler.id);
            if (battler.actor.skills_gained_on_level().Any())
                skill_gain(battler.id);
            Sparring_Level_Up = true;
        }

        protected override void battle_end()
        {
            base.battle_end();
        }

        protected override void arena_weapon_reset()
        {
            Battler_1.recover_all();
            Battler_2.recover_all();
            Global.game_map.units[Global.game_map.get_unit_id_from_actor(
                Window_Sparring.Healer_Id)].recover_all();
            Battler_2.actor.equip(Battler_2.actor.equipped);
            base.arena_weapon_reset();
        }

        protected override void draw_skip(SpriteBatch sprite_batch)
        {
            base.draw_skip(sprite_batch);
            if (Sparring_Result != null)
                Sparring_Result.draw(sprite_batch);
        }
    }
}
