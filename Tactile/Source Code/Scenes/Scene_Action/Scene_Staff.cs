using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TactileLibrary;
using TactileListExtension;
using TactileWeaponExtension;

namespace Tactile
{
    class Scene_Staff : Scene_Battle
    {
        public Scene_Staff() { }

        protected override void initialize_base()
        {
            Scene_Type = "Scene_Staff";
            main_window();
            camera = new Camera(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT, Vector2.Zero);
        }

        protected override void play_battle_theme()
        {
            switch (((Staff_Data)Combat_Data).mode)
            {
                case Staff_Modes.Heal:
                    Global.game_state.play_staff_theme();
                    break;
                case Staff_Modes.Status_Inflict:
                    Global.game_state.play_attack_staff_theme();
                    break;
            }
        }

        #region Phase Updates
        protected override void update_phase_1()
        {
            base.update_phase_1();
        }
        #endregion

        #region Attack
        protected override bool hit_effects(Battler_Sprite battler_1_sprite, Battler_Sprite battler_2_sprite, bool reverse, bool magic)
        {
            switch (((Staff_Data)Combat_Data).mode)
            {
                case Staff_Modes.Heal:
                    return heal_hit_effects(battler_1_sprite, battler_2_sprite, reverse, magic);
                case Staff_Modes.Status_Inflict:
                    return status_inflict_hit_effects(battler_1_sprite, battler_2_sprite, reverse, magic);
                default:
                    return base.hit_effects(battler_1_sprite, battler_2_sprite, reverse, magic);
            }
        }

        protected bool heal_hit_effects(Battler_Sprite battler_1_sprite, Battler_Sprite battler_2_sprite, bool reverse, bool magic)
        {
            // Finished with hit
            if (Timer > 3)
            {
                return true;
            }
            else if (Timer == 0)
            {
                bool life_drain = Combat_Data.Data[Attack_Id].Key.Result.delayed_life_steal;
                attack_dmg(battler_1_sprite.battler, battler_2_sprite.battler, Combat_Data.Data[Attack_Id].Key);
                if (magic)
                    battler_1_sprite.end_spell(Hit, Crit, Distance);
                else
                    hit_freeze(battler_1_sprite);
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

        protected bool status_inflict_hit_effects(Battler_Sprite battler_1_sprite, Battler_Sprite battler_2_sprite, bool reverse, bool magic)
        {
            // Finished with hit
            if (Timer > 3)
            {
                return true;
            }
            else if (Timer == 0)
            {
                bool life_drain = Combat_Data.Data[Attack_Id].Key.Result.delayed_life_steal;
                attack_dmg(battler_1_sprite.battler, battler_2_sprite.battler, Combat_Data.Data[Attack_Id].Key);
                if (magic)
                    battler_1_sprite.end_spell(Hit, Crit, Distance);
                else
                    hit_freeze(battler_1_sprite);
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

        protected override void hit_flash(Battler_Sprite battler_1_sprite, Battler_Sprite battler_2_sprite, int time)
        {
            if (Dmg_Dealt)
                base.hit_flash(battler_1_sprite, battler_2_sprite, time);
        }
        #endregion
    }
}
