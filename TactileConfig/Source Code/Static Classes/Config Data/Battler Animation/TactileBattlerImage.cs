using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TactileLibrary;
using TactileLibrary.Battler;
using IntExtension;

namespace Tactile
{
    public class TactileBattlerImage
    {
        const int IDLE_ANIM = 1;
        const int AVOID_ANIM = 2;
        const int AVOID_RETURN_ANIM = 3;
        const int GET_HIT_ANIM = 4;
        const int GET_CRIT_ANIM = 5;
        // If you change the number of shared, reserved animations from the default 10, this allows not re-listing every attack anim
        const int ATTACK_ANIM_OFFSET = 0;

        #region Skill Timings
        public readonly static Dictionary<int, int[]> SKILL_ACTIVATION_FRAME = new Dictionary<int, int[]> {
            { 51, new int[] {  6,  6 }}, // Swordmaster
            { 59, new int[] {  3,  3 }}, // Halberdier
            { 70, new int[] { 10, 10 }}, // Paladin
            { 76, new int[] { 10, 10 }}, // Falcoknight
            { 88, new int[] {  6,  6 }} // Justice
        };
        #endregion

        public static Battle_Animation_Association_Data animation_set(Animation_Setup_Data battler)
        {
            if (Animation_Battler_Data != null)
                return Animation_Battler_Data.animation_set(battler.Key, battler.Gender);

            return null;
        }

        #region Animation Values
        #region Attack
        // Returns the animation numbers of the given class as it starts to attack
        public static List<int> attack_animation_value(Animation_Setup_Data battler, int offset, bool crit, bool hit)
        {
            offset += ATTACK_ANIM_OFFSET;
            switch (battler.Class_Id)
            {
                #region 51: Swordmaster
                case 51:
                    // Astra
                    if (battler.Astra_Activated)
                    {
                        if (battler.Astra_Missed)
                            return offset.list_add(new List<int>() { 17, 41 });
                        else
                            switch(battler.Astra_Count)
                            {
                                case 4:
                                    return offset.list_add(new List<int>() { 17, 18, crit ? 32 : 19 });
                                case 3:
                                    return offset.list_add(new List<int>() { crit ? 34 : 22 });
                                case 2:
                                    return offset.list_add(new List<int>() { crit ? 36 : 25 });
                                case 1:
                                    return offset.list_add(new List<int>() { crit ? 38 : 28 });
                                default:
                                    return offset.list_add(new List<int>() { crit ? 40 : 31 });
                            }
                    }
                    break;
                #endregion
            }

            var anim_set = animation_set(battler);
            if (anim_set != null)
            {
                AttackAnims anim = AttackAnims.Attack;
                // Hit
                anim |= AttackAnims.HitDefined;
                if (hit)
                    anim |= AttackAnims.Hit;
                // Crit
                anim |= AttackAnims.CritDefined;
                if (crit)
                    anim |= AttackAnims.Crit;
                // Skill
                anim |= AttackAnims.SkillDefined;
                if (battler.Skill)
                    anim |= AttackAnims.SkillActive;
                var attack = anim_set.attack_animation(battler.anim_type, battler.Distance, anim);
                return offset.list_add(attack);
            }

            return new List<int>();
        }

        // Returns the animation numbers of the given class when frozen after attacking
        public static List<int> still_animation_value(Animation_Setup_Data battler, int offset, bool crit, bool hit)
        {
            offset += ATTACK_ANIM_OFFSET;
            switch (battler.Class_Id)
            {
                #region 51: Swordmaster
                case 51:
                    // Astra
                    if (battler.Astra_Activated && battler.Astra_Count > 0)
                    {
                        switch (battler.Astra_Count)
                        {
                            case 4:
                                return offset.list_add(new List<int>() { crit ? 33 : 20 });
                            case 3:
                                return offset.list_add(new List<int>() { crit ? 35 : 23 });
                            case 2:
                                return offset.list_add(new List<int>() { crit ? 37 : 26 });
                            default:
                                return offset.list_add(new List<int>() { crit ? 39 : 29 });
                        }
                    }
                    break;
                #endregion
            }

            var anim_set = animation_set(battler);
            if (anim_set != null)
            {
                AttackAnims anim = AttackAnims.Hold;
                // Hit
                anim |= AttackAnims.HitDefined;
                if (hit)
                    anim |= AttackAnims.Hit;
                // Crit
                anim |= AttackAnims.CritDefined;
                if (crit)
                    anim |= AttackAnims.Crit;
                // Skill
                anim |= AttackAnims.SkillDefined;
                if (battler.Skill)
                    anim |= AttackAnims.SkillActive;
                return offset.list_add(anim_set.attack_animation(battler.anim_type, battler.Distance, anim));
            }

            return new List<int>();
        }

        // Returns the animation numbers of the given class as it returns after attack
        public static List<int> return_animation_value(Animation_Setup_Data battler, int offset, bool crit, bool hit, bool no_damage)
        {
            offset += ATTACK_ANIM_OFFSET;
            switch (battler.Class_Id)
            {
                #region 51: Swordmaster
                case 51:
                    // Astra
                    if (battler.Astra_Activated && battler.Astra_Count > 0)
                    {
                        switch (battler.Astra_Count)
                        {
                            case 4:
                                return offset.list_add(new List<int>() { 21 });
                            case 3:
                                return offset.list_add(new List<int>() { 24 });
                            case 2:
                                return offset.list_add(new List<int>() { 27 });
                            default:
                                return offset.list_add(new List<int>() { 30 });
                        }
                    }
                    break;
                #endregion
            }

            var anim_set = animation_set(battler);
            if (anim_set != null)
            {
                AttackAnims anim = AttackAnims.Return;
                // Hit
                anim |= AttackAnims.HitDefined;
                if (hit)
                    anim |= AttackAnims.Hit;
                // Crit
                anim |= AttackAnims.CritDefined;
                if (crit)
                    anim |= AttackAnims.Crit;
                // No Damage
                anim |= AttackAnims.DamageDefined;
                if (!no_damage)
                    anim |= AttackAnims.CausedDamage;
                // Skill
                anim |= AttackAnims.SkillDefined;
                if (battler.Skill)
                    anim |= AttackAnims.SkillActive;
                return offset.list_add(anim_set.attack_animation(battler.anim_type, battler.Distance, anim));
            }

            return new List<int>();
        }
        #endregion

        /// <summary>
        /// Returns the animation numbers for idle animations
        /// </summary>
        public static List<int> idle_animation_value(Animation_Setup_Data battler, int offset)
        {
            switch (battler.Class_Id)
            {
            }

            var anim_set = animation_set(battler);
            if (anim_set != null)
                return offset.list_add(anim_set.Idle);

            return offset.list_add(new List<int> { IDLE_ANIM });
        }
        /// <summary>
        /// Returns the animation numbers for avoid animations
        /// </summary>
        public static List<int> avoid_animation_value(Animation_Setup_Data battler, int offset)
        {
            switch (battler.Class_Id)
            {
            }

            var anim_set = animation_set(battler);
            if (anim_set != null)
                return offset.list_add(anim_set.Avoid);

            return offset.list_add(new List<int> { AVOID_ANIM });
        }
        /// <summary>
        /// Returns the animation numbers for avoid return animations
        /// </summary>
        public static List<int> avoid_return_animation_value(Animation_Setup_Data battler, int offset)
        {
            switch (battler.Class_Id)
            {
            }

            var anim_set = animation_set(battler);
            if (anim_set != null)
                return offset.list_add(anim_set.AvoidReturn);

            return offset.list_add(new List<int> { AVOID_RETURN_ANIM });
        }
        /// <summary>
        /// Returns the animation numbers for gethit animations
        /// </summary>
        public static List<int> get_hit_animation_value(Animation_Setup_Data battler, int offset, bool crit)
        {
            if (false) //Debug
            switch (battler.Class_Id)
            {
                #region 25: Brigand
                case 25:
                #endregion
                    if (!battler.Equipped)
                        return offset.list_add(new List<int> { IDLE_ANIM });
                    break;

                #region 44: Sorcerer
                case 44:
                #endregion
                    if (battler.Gender % 2 == 1)
                        return offset.list_add(new List<int> { IDLE_ANIM });
                    break;
                    
                #region classes without throw axe or unarmed frames
                #endregion
                    if (!battler.Equipped || battler.Weapon_Thrown)
                        return offset.list_add(new List<int> { IDLE_ANIM });
                    break;

                #region classes without throw axe frames
                #endregion
                #region 24: Fighter
                case 24:
                #endregion
                    if (battler.Weapon_Thrown)
                        return offset.list_add(new List<int> { IDLE_ANIM });
                    break;

            }

            var anim_set = animation_set(battler);
            if (anim_set != null)
            {
                if (!crit)
                    return offset.list_add(anim_set.GetHit);
                else
                    return offset.list_add(anim_set.get_crit);
            }

            return offset.list_add(new List<int> { crit ? GET_CRIT_ANIM : GET_HIT_ANIM });
        }

        /// <summary>
        /// Returns the animation numbers for pre-battle animations
        /// </summary>
        public static List<int> pre_battle_animation_value(Animation_Setup_Data battler, int offset)
        {
            offset += ATTACK_ANIM_OFFSET;
            switch (battler.Class_Id)
            {
                #region 61: Dragon Master
                case 61:
                    if (battler.Swoop_Activated)
                    {
                        return offset.list_add(new List<int> { 23 });
                    }
                    break;
                #endregion
            }

            var anim_set = animation_set(battler);
            if (anim_set != null)
                return offset.list_add(anim_set.PreFight);

            return new List<int>();
        }

        #region Dance
        public static List<int> dance_attack_animation_value(Animation_Setup_Data battler, int offset)
        {
            offset += ATTACK_ANIM_OFFSET;
            switch (battler.Class_Id)
            {
            }

            var anim_set = animation_set(battler);
            if (anim_set != null)
                return offset.list_add(anim_set.Dance);

            return new List<int>();
        }

        public static List<int> dance_still_animation_value(Animation_Setup_Data battler, int offset)
        {
            offset += ATTACK_ANIM_OFFSET;
            switch (battler.Class_Id)
            {
            }

            var anim_set = animation_set(battler);
            if (anim_set != null)
                return offset.list_add(anim_set.DanceHold);

            return new List<int>();
        }

        public static List<int> dance_return_animation_value(Animation_Setup_Data battler, int offset)
        {
            offset += ATTACK_ANIM_OFFSET;
            switch (battler.Class_Id)
            {
            }

            var anim_set = animation_set(battler);
            if (anim_set != null)
                return offset.list_add(anim_set.DanceReturn);

            return new List<int>();
        }
        #endregion

        #region Spells
        /// <summary>
        /// Returns the animation numbers of the given spell's start up
        /// </summary>
        public static List<int> spell_attack_animation_value(int weapon_id, int offset, bool magic, int distance, bool hit)
        {
            switch (weapon_id)
            {
                case 191: // Fire Breath
                case 192: // Flame Stone
                    return offset.list_add(new List<int>() { 401 });
            }
            if (!magic)
                return new List<int>();

            switch (weapon_id)
            {
                case 18: // Runesword
                    return offset.list_add(distance == 1 ? new List<int>() { 260 } : new List<int>() { 261 });
                case 101: // Fire
                    return offset.list_add(distance == 1 ? new List<int>() { 5, 3 } : new List<int>() { 2, 3 });
                case 45: // Flame Lance
                case 102: // Elfire
                    return offset.list_add(distance == 1 ? new List<int>() { 5, (hit ? 7 : 3) } : new List<int>() { 2, (hit ? 7 : 3) });
                case 103: // Arcfire
                    return offset.list_add(distance == 1 ? new List<int>() { 5, (hit ? 9 : 3) } : new List<int>() { 2, (hit ? 9 : 3) });
                case 111: // Thunder
                    return offset.list_add(distance == 1 ? new List<int>() { 64 } : new List<int>() { 62 });
                case 70: // Bolt Axe
                case 112: // Elthunder
                    return offset.list_add(distance == 1 ? new List<int>() { 67 } : new List<int>() { 65 });
                case 113: // Arcthunder
                    return offset.list_add(distance == 1 ? new List<int>() { 71 } : new List<int>() { 68 });
                case 115: // Bolting
                    return offset.list_add(new List<int>() { 74 });
                case 121: // Wind
                    return offset.list_add(distance == 1 ? new List<int>() { 125 } : new List<int>() { 122 });
                case 15: // Wind Sword
                case 122: // Elwind
                    return offset.list_add(new List<int>() { distance == 1 ? 131 : 126, hit ? 127 : 128 });
                case 123: // Arcwind
                    return offset.list_add(new List<int>() { distance == 1 ? 131 : 126, hit ? 127 : 128 }); //Yeti
                case 12: // Light Brand
                case 131: // Lightning
                    return offset.list_add(distance == 1 ? new List<int>() { 183 } : new List<int>() { 181 });
                case 132: // Shine
                    return offset.list_add(distance == 1 ? new List<int>() { 187, 185 } : new List<int>() { 184, 185 });
                case 133: // Divine
                    return offset.list_add(distance == 1 ? new List<int>() { 191, 189 } : new List<int>() { 188, 189 });
                case 134: // Resire
                    return offset.list_add(distance == 1 ? new List<int>() { 260 } : new List<int>() { 261 }); //Yeti
                case 135: // Purge
                    return offset.list_add(new List<int>() { 192, 193, 194, 195 });
                case 141: // Shade
                    return offset.list_add(distance == 1 ? new List<int>() { 243 } : new List<int>() { 241 });
                case 142: // Flux
                    return offset.list_add(distance == 1 ? new List<int>() { 247 } : new List<int>() { 244 });
                case 143: // Worm
                    if (hit)
                        return offset.list_add(distance == 1 ? new List<int>() { 253, 250 } : new List<int>() { 249, 250 });
                    else
                        return offset.list_add(distance == 1 ? new List<int>() { 253 } : new List<int>() { 249 });
                case 144: // Hekat
                    return offset.list_add(distance == 1 ? new List<int>() { 270, 268 } : new List<int>() { 267, 268 });
                case 146: // Fenrir
                    return offset.list_add(distance == 1 ? new List<int>() { 278, 276 } : new List<int>() { 275, 276 });
                case 151: // Heal
                    return offset.list_add(new List<int>() { 301 });
                case 152: // Mend
                    return offset.list_add(new List<int>() { 302 });
                case 153: // Recover
                    return offset.list_add(new List<int>() { 303 });
                case 154: // Physic
                    return offset.list_add(distance == 1 ? new List<int>() { 306, 305 } : new List<int>() { 304, 305 });
                case 158: // Restore
                    return offset.list_add(new List<int>() { 327 });
                case 160: // Barrier
                    return offset.list_add(new List<int>() { 330 });
                case 161: // Silence
                    return offset.list_add(new List<int>() { (distance == 1 ? 313 : (distance == 2 ? 314 : 311)) });
                case 162: // Sleep
                    return offset.list_add(new List<int>() { (distance == 1 ? 319 : (distance == 2 ? 320 : 315)), 316, 317 });
                case 167: // Fix
                    return offset.list_add(distance == 1 ? new List<int>() { 309, 308 } : new List<int>() { 307, 308 });
                case 169: // Slow
                    return offset.list_add(new List<int>() { (distance == 1 ? 319 : 315), 316, 317 }); //Yeti
                case 170: // Renewal
                    return offset.list_add(new List<int>() { 301 }); //Yeti
            }
            return new List<int>();
        }

        /// <summary>
        /// Returns the animation numbers of the given spell's start up, played behind characters but above platform
        /// </summary>
        public static List<int> spell_attack_animation_value_bg1(int weapon_id, int offset, bool magic, int distance, bool hit)
        {
            if (!magic)
                return new List<int>();

            switch (weapon_id)
            {
                case 113: // Arcthunder
                    return offset.list_add(distance == 1 ? new List<int>() { 73 } : new List<int>() { 70 });
                case 142: // Flux
                    return offset.list_add(distance == 1 ? new List<int>() { 248 } : new List<int>() { 245 });
                case 143: // Worm
                    return offset.list_add(distance == 1 ? new List<int>() { 254 } : new List<int>() { 251 });
            }
            return new List<int>();
        }

        /// <summary>
        /// Returns the animation numbers of the given spell's start up, played behind the platform
        /// </summary>
        public static List<int> spell_attack_animation_value_bg2(int weapon_id, int offset, bool magic, int distance, bool hit)
        {
            if (!magic)
                return new List<int>();

            switch (weapon_id)
            {
                // Luce lol //Yeti
            }
            return new List<int>();
        }

        /// <summary>
        /// Returns the animation numbers of the given spell's start up, played above the HUD
        /// </summary>
        public static List<int> spell_attack_animation_value_fg(int weapon_id, int offset, bool magic, int distance, bool hit)
        {
            if (!magic)
                return new List<int>();

            switch (weapon_id)
            {
                case 115: // Bolting
                    return offset.list_add(new List<int>() { 76 });
            }
            return new List<int>();
        }

        /// <summary>
        /// Returns the animation numbers of the given spell's ending
        /// </summary>
        public static List<int> spell_end_animation_value(int weapon_id, int offset, bool magic, bool hit)
        {
            switch (weapon_id)
            {
                case 191: // Fire Breath
                case 192: // Flame Stone
                    return offset.list_add(new List<int>() { 402 });
            }
            if (!magic)
                return new List<int>();

            switch (weapon_id)
            {
                case 18: // Runesword
                    return offset.list_add(hit ? new List<int>() { 262 } : new List<int>() { 262, 266 });
                case 101: // Fire
                    return offset.list_add(hit ? new List<int>() { 4 } : new List<int>() { 6 });
                case 45: // Flame Lance
                case 102: // Elfire
                    return offset.list_add(hit ? new List<int>() { 8 } : new List<int>() { 6 });
                case 103: // Arcfire
                    return offset.list_add(hit ? new List<int>() { 10 } : new List<int>() { 6 });
                case 111: // Thunder
                    return offset.list_add(new List<int>() { 63 });
                case 70: // Bolt Axe
                case 112: // Elthunder
                    return offset.list_add(new List<int>() { 66 });
                case 113: // Arcthunder
                    return offset.list_add(hit ? new List<int>() { 69 } : new List<int>() { 72 });
                case 115: // Bolting
                    return offset.list_add(new List<int>() { 75 });
                case 121: // Wind
                    return offset.list_add(hit ? new List<int>() { 123 } : new List<int>() { 124 });
                case 15: // Wind Sword
                case 122: // Elwind
                    return offset.list_add(hit ? new List<int>() { 129 } : new List<int>() { 130 });
                case 123: // Arcwind
                    return offset.list_add(hit ? new List<int>() { 129 } : new List<int>() { 130 }); //Yeti
                case 12: // Light Brand
                case 131: // Lightning
                    return offset.list_add(new List<int>() { 182 });
                case 132: // Shine
                    return offset.list_add(new List<int>() { 186 });
                case 133: // Divine
                    return offset.list_add(new List<int>() { 190 });
                case 134: // Resire
                    return offset.list_add(hit ? new List<int>() { 262 } : new List<int>() { 262, 266 }); //Yeti
                case 135: // Purge
                    return offset.list_add(new List<int>() { 196 });
                case 141: // Shade
                    return offset.list_add(new List<int>() { 242 });
                case 142: // Flux
                    return offset.list_add(new List<int>() { 246 });
                case 143: // Worm
                    return offset.list_add(hit ? new List<int>() { 252 } : new List<int>() { 255 });
                case 144: // Hekat
                    return offset.list_add(new List<int>() { 269 });
                case 146: // Fenrir
                    return offset.list_add(new List<int>() { 277 });
                case 158: // Restore
                    return offset.list_add(new List<int>() { 328 });
                case 160: // Barrier
                    return offset.list_add(new List<int>() { 331 });
                case 161: // Silence
                    return offset.list_add(new List<int>() { 312 });
                case 162: // Sleep
                    return offset.list_add(new List<int>() { 318 });
                case 169: // Slow
                    return offset.list_add(new List<int>() { 318 }); //Yeti
            }
            return new List<int>();
        }

        /// <summary>
        /// Returns the animation numbers of the given spell's life drain effect, after hit but before recovery
        /// </summary>
        public static List<int> spell_lifedrain_animation_value(int weapon_id, int offset, bool magic, int distance)
        {
            if (!magic)
                return new List<int>();

            switch (weapon_id)
            {
                case 18: // Runesword
                    return offset.list_add(new List<int>() { distance == 1 ? 263 : 264 });
                case 134: // Resire
                    return offset.list_add(new List<int>() { distance == 1 ? 263 : 264 }); //Yeti
            }
            return new List<int>();
        }
        /// <summary>
        /// Returns the animation numbers of the given spell's life drain effect, starts at the same time as recovery
        /// </summary>
        public static List<int> spell_lifedrain_end_animation_value(int weapon_id, int offset, bool magic)
        {
            if (!magic)
                return new List<int>();

            switch (weapon_id)
            {
                case 18: // Runesword
                    return offset.list_add(new List<int>() { 265 });
                case 134: // Resire
                    return offset.list_add(new List<int>() { 265 }); //Yeti
            }
            return new List<int>();
        }
        #endregion

        public static List<int> refresh_animation_value(Animation_Setup_Data battler, int offset, int ring_id)
        {
            switch (ring_id)
            {
                case 91: // Deus's Geas
                    return offset.list_add(new List<int>() { 7 });
                case 92: // Mulciber's Iron
                    return offset.list_add(new List<int>() { 8 });
                case 93: // Set's Litany
                    return offset.list_add(new List<int>() { 9 });
                case 94: // Mot's Mercy
                    return offset.list_add(new List<int>() { 10 });
                case 95: // El's Passage
                    return offset.list_add(new List<int>() { 11 });
                default: // No Ring
                    return offset.list_add(new List<int>() { 6 });
            }
            return new List<int>();
        }
        #endregion

        #region Promotion
        public readonly static Promotion_Effect[] PROMOTION_START = new Promotion_Effect[]
        {
            new Promotion_Effect{ time = 116, tint = new Color(255, 255, 255, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(240, 240, 240, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(224, 224, 224, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(192, 192, 192, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(176, 176, 176, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(160, 160, 160, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(128, 128, 128, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(112, 112, 112, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(80, 80, 80, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(64, 64, 64, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(48, 48, 48, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(32, 32, 32, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(0, 0, 0, 0) }
        };

        public readonly static Promotion_Effect[] PROMOTION_END = new Promotion_Effect[]
        {
            new Promotion_Effect{ time = 128, tint = new Color(0, 0, 0, 0) },
            new Promotion_Effect{ time = 1, tint = new Color(32, 32, 32, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(48, 48, 48, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(64, 64, 64, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(80, 80, 80, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(112, 112, 112, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(128, 128, 128, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(160, 160, 160, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(176, 176, 176, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(192, 192, 192, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(224, 224, 224, 255) },
            new Promotion_Effect{ time = 1, tint = new Color(240, 240, 240, 255) },
            new Promotion_Effect{ time = 23, tint = new Color(255, 255, 255, 255) },
            new Promotion_Effect{ time = 16, tint = new Color(255, 255, 255, 255), scene_flash_time = 66,
                sprite_flash_time = 125, sprite_tint = new Color(160, 255, 104)},
            new Promotion_Effect{ time = 1, tint = new Color(255, 255, 255, 255), sound = "Promotion3" }
            //new Promotion_Effect{ time = 1, tint = new Color(255, 255, 255, 255), scene_flash_time = 66 },
            //new Promotion_Effect{ time = 1, tint = new Color(255, 255, 255, 255), sound = "Promotion3",
            //    sprite_flash_time = 125, sprite_tint = new Color(160, 255, 104) }
        };
        #endregion

        public readonly static int[] Single_Gender_Map_Sprite = new int[] { 22, 23, 31, 32, 34, 53, 60, 61, 69, 70, 71, 72, 73, 106, 107 };
        public readonly static int[] Single_Gender_Battle_Sprite = new int[] { 22, 23, 34, 60, 61, 71, 72, 73 };

        internal static IBattlerAnimsService Animation_Battler_Data { get; private set; }
        public static IBattlerAnimsService animation_battler_data
        {
            get { return Animation_Battler_Data; }
            set
            {
                if (Animation_Battler_Data == null)
                    Animation_Battler_Data = value;
            }
        }
    }

    public struct Promotion_Effect
    {
        public int time;
        public Color tint;
        public string sound;
        public int sprite_flash_time;
        public Color sprite_tint;
        public int scene_flash_time;
    }

    public class Animation_Setup_Data
    {
        public string Key { get; protected set; }
        public int Distance { get; protected set; }
        public int Class_Id { get; protected set; }
        public int Gender { get; protected set; }
        public bool Equipped { get; protected set; }
        public int Weapon_Main_Type { get; protected set; }
        public bool Weapon_Thrown { get; protected set; }
        public bool Weapon_Is_Staff { get; protected set; }
        public bool MWeapon { get; protected set; }
        public bool Skill { get; protected set; }

        public AnimTypeKeys anim_type
        {
            get
            {
                if (MWeapon)
                    return AnimTypeKeys.MWeapon;
                else if (Weapon_Thrown)
                    return AnimTypeKeys.ThrownWeapon;
                else
                    return AnimTypeKeys.NormalAttack;
            }
        }

        //Yeti
        // probably recode this to use a dictionary with string keys for skills
        // then the value would be a dictionary<object, object> or something idk
        public bool Astra_Activated { get; protected set; }
        public bool Astra_Missed { get; protected set; }
        public int Astra_Count { get; protected set; }
        public bool Swoop_Activated { get; protected set; }
    }

    public class Test_Animation_Setup_Data : Animation_Setup_Data
    {
        public Test_Animation_Setup_Data(string key, int class_id, int weapon_type)
        {
            Key = key;
            Class_Id = class_id;

            Weapon_Main_Type = weapon_type;
            throw new NotImplementedException();
            Equipped = Weapon_Main_Type != 0; //Weapon_Types.None;
            throw new NotImplementedException();
            //Weapon_Is_Staff = Weapon_Main_Type == Weapon_Types.Staff;
        }

        public int distance { set { Distance = value; } }
        public int gender { set { Gender = value; } }
        public bool weapon_is_thrown { set { Weapon_Thrown = value; } }
        public bool magic_weapon { set { MWeapon = value; } }
        public bool skill_active { set { Skill = value; } }
    }

    public interface IBattlerAnimsService
    {
        Battle_Animation_Association_Data animation_set(string name, int gender);
    }
}
