using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TactileLibrary;
using TactileListExtension;
using TactileWeaponExtension;

namespace Tactile
{
    class Scene_Dance : Scene_Battle
    {
        public Scene_Dance() { }

        protected override void initialize_base()
        {
            Scene_Type = "Scene_Dance";
            main_window();
            camera = new Camera(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT, Vector2.Zero);
        }

        public override void initialize_action(int distance)
        {
            Battler_1 = Global.game_map.units[Global.game_state.dancer_id];
            if (Global.game_state.dance_target_id != -1)
                Battler_2 = Global.game_map.units[Global.game_state.dance_target_id];
            if (Battler_2 == null || Battler_2.same_team(Battler_1))
                Reverse = true;
            else
                Reverse = Battler_2.is_opposition;
            Real_Distance = Distance = distance;
            initialize_weather_condition();
        }

        protected override void setup_hud()
        {
            HUD = new Window_Battle_HUD(Battler_1.id, Battler_2.id, !Reverse, Distance);
        }

        protected override bool is_last_attack()
        {
            return true;
        }

        protected override void play_battle_theme()
        {
            Global.game_state.play_dance_theme();
        }

        #region Phase Updates

        protected override void update_phase_1()
        {
            base_update_phase_1();
            switch (Segment)
            {
                case 0:
                    bool cont = false;
                    while (!cont)
                    {
                        cont = true;
                        if (!Attack_Active)
                        {
                            Active_Battler = 1;
                            add_battle_action(Battle_Actions.New_Attack);
                        }
                        bool next_attack = attack(Battler_1, Battler_2, true);
                        Battler_1.update_attack_graphics();
                        Battler_2.update_attack_graphics();
                        if (next_attack)
                        {
                            Timer = 0;
                            Segment = 1;
                            Can_Skip = false;
                            refresh_stats();
                        }
                    }
                    break;
                case 1:
                    switch (Timer)
                    {
                        case 20:
                            Segment = 2;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                case 2:
                    Phase = 2;
                    Segment = 0;
                    break;
            }
        }

        protected override bool exp_gained()
        {
            // Exp gain
            if (Battler_1.allowed_to_gain_exp() && Battler_1.actor.can_level())
            {
                int exp = Battler_1.actor.exp;
                int exp_gain = Battler_1.dance_exp();

                if (exp_gain > 0 || Battler_1.actor.needed_levels > 0)
                {
                    create_exp_gauge(exp);
                    Exp_Gain = exp_gain;
                    Exp_Gauge.loc.Y = Config.WINDOW_HEIGHT - 24;
                    return true;
                }
            }
            return false;
        }
        #endregion

        protected override bool weapon_broke(int id)
        {
            return false;
        }

        protected override int weapon_id(int id)
        {
            switch (id)
            {
                case 1:
                    return Combat_Data.Weapon_1_Id;
                case 2:
                    return Combat_Data.Weapon_2_Id;
            }
            return -1;
        }

        protected override Data_Equipment equipment(int id)
        {
            switch (id)
            {
                case 1:
                    if (Global.game_state.dance_item == -1)
                        return null;
                    return Battler_1.items[Global.game_state.dance_item].to_equipment;
                case 2:
                    return null;
            }
            return null;
        }

        protected override int hp(int id)
        {
            switch (id)
            {
                case 1:
                    return Battler_1.actor.hp;
                case 2:
                    return Battler_2.actor.hp;
            }
            return -1;
        }

        protected override bool kill()
        {
            return false;
        }

        #region Attack
        #region Attack Animations
        protected override void attack_anim(Battler_Sprite battler_sprite)
        {
            battler_sprite.dance_attack();
        }

        protected override void hit_freeze(Battler_Sprite battler_sprite)
        {
            battler_sprite.dance_hit_freeze();
        }

        protected override void return_anim(Battler_Sprite battler_sprite)
        {
            battler_sprite.dance_return_anim();
        }

        protected override void attack_spell(Battler_Sprite battler_sprite)
        {
            battler_sprite.refersh_spell(Distance, Global.game_state.dance_item);
        }
        #endregion

        protected override void determine_hit(Battler_Sprite battler_1_sprite, Battler_Sprite battler_2_sprite)
        {
            Dmg = 0;
            Dmg_Dealt = false;
            Hit = true;
            Crit = false;
        }

        protected override bool hit_effects(Battler_Sprite battler_1_sprite, Battler_Sprite battler_2_sprite, bool reverse, bool magic)
        {
            return dance_hit_effects(battler_1_sprite, battler_2_sprite, reverse, magic);
        }

        protected bool dance_hit_effects(Battler_Sprite battler_1_sprite, Battler_Sprite battler_2_sprite, bool reverse, bool magic)
        {
            // Finished with hit
            if (Timer > 3)
            {
                return true;
            }
            else if (Timer == 0)
            {
                if (Global.game_state.dance_item == -1 || Constants.Combat.RING_REFRESH)
                    battler_2_sprite.end_grey_out();
                if (magic)
                    battler_1_sprite.end_spell(Hit, Crit, Distance);
                // Pan back on last hit
                if (Distance > 1 && is_last_attack())
                {
                    if (magic) Magic_Brighten = true;
                    add_battle_action(Battle_Actions.Pan_Hit_Over);
                }
                else
                {
                    if (magic) Magic_Brighten = true;
                    if (Distance > 1 && is_next_attacker_same())
                        add_battle_action(Battle_Actions.Wait_For_Pan);
                    add_battle_action(Battle_Actions.Hit_Over);
                }
            }
            // Pan back
            else if (!magic && Timer == 3)
            {
                if (Distance > 1)
                    pan(reverse ? -1 : 1);
            }
            return false;
        }

        protected override void hit_flash(Battler_Sprite battler_1_sprite, Battler_Sprite battler_2_sprite, int time) { }
        #endregion
    }
}
