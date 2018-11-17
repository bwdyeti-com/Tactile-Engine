using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class FE_Battler_Image_Wrapper
    {
        internal static Animation_Setup_Data animation_processor(Game_Unit battler, int distance)
        {
            return new Animation_Setup_Data_Processor(battler, distance);
        }

        internal static int offset(Game_Unit battler)
        {
            string name = string.Format("{0}-{1}",
                battler.actor.class_name_full, battler.actor.used_weapon_type());
            return Global.animation_group(name);
        }

        #region Skill Timings
        internal readonly static Dictionary<int, int[]> SKILL_ACTIVATION_FRAME = new Dictionary<int, int[]> {
            { 51, new int[] {  6,  6 }}, // Swordmaster
            { 59, new int[] {  3,  3 }}, // Halberdier
            { 70, new int[] { 10, 10 }}, // Paladin
            { 76, new int[] { 10, 10 }}, // Falcoknight
            { 88, new int[] {  6,  6 }} // Justice
        };
        #endregion

        #region Animation Values
        #region Attack
        // Returns the animation numbers of the given class as it starts to attack
        internal static List<int> attack_animation_value(Game_Unit battler, bool crit, int distance, bool hit)
        {
            FEXNA_Library.Data_Weapon weapon = battler.actor.weapon;
            bool mweapon = weapon != null &&
                ((distance >= 2 && weapon.is_ranged_magic()) || weapon.is_always_magic());
            return FE_Battler_Image.attack_animation_value(
                animation_processor(battler, distance), offset(battler), crit, hit);
        }

        // Returns the animation numbers of the given class when frozen after attacking
        internal static List<int> still_animation_value(Game_Unit battler, bool crit, int distance, bool hit)
        {
            FEXNA_Library.Data_Weapon weapon = battler.actor.weapon;
            bool skill = battler.actor.skill_activated && !battler.skip_skill_effect;
            bool mweapon = (distance >= 2 && weapon.is_ranged_magic()) || weapon.is_always_magic();
            return FE_Battler_Image.still_animation_value(
                animation_processor(battler, distance), offset(battler), crit, hit);
        }

        // Returns the animation numbers of the given class as it returns after attack
        internal static List<int> return_animation_value(Game_Unit battler, bool crit, int distance, bool hit, bool no_damage)
        {
            FEXNA_Library.Data_Weapon weapon = battler.actor.weapon;
            bool skill = battler.actor.skill_activated && !battler.skip_skill_effect;
            bool mweapon = (distance >= 2 && weapon.is_ranged_magic()) || weapon.is_always_magic();
            return FE_Battler_Image.return_animation_value(
                animation_processor(battler, distance), offset(battler), crit, hit, no_damage);
        }
        #endregion

        #region Dance
        internal static List<int> dance_attack_animation_value(Game_Unit battler)
        {
            return FE_Battler_Image.dance_attack_animation_value(
                animation_processor(battler, 1), offset(battler)); // Distance? //Yeti
        }

        internal static List<int> dance_still_animation_value(Game_Unit battler)
        {
            return FE_Battler_Image.dance_still_animation_value(
                animation_processor(battler, 1), offset(battler)); // Distance? //Yeti
        }

        internal static List<int> dance_return_animation_value(Game_Unit battler)
        {
            return FE_Battler_Image.dance_return_animation_value(
                animation_processor(battler, 1), offset(battler)); // Distance? //Yeti
        }
        #endregion

        /// <summary>
        /// Returns the animation numbers for idle animations
        /// </summary>
        internal static List<int> idle_animation_value(Game_Unit battler, int distance)
        {
            FEXNA_Library.Data_Weapon weapon = battler.actor.weapon;
            bool skill = battler.actor.skill_activated && !battler.skip_skill_effect;
            bool mweapon = weapon == null ? false : (distance >= 2 && weapon.is_ranged_magic()) || weapon.is_always_magic();
            // this needs to take forceably unequipped anims for dancers into account //Yeti
            return FE_Battler_Image.idle_animation_value(
                animation_processor(battler, distance), offset(battler));
        }
        /// <summary>
        /// Returns the animation numbers for avoid animations
        /// </summary>
        internal static List<int> avoid_animation_value(Game_Unit battler, int distance)
        {
            FEXNA_Library.Data_Weapon weapon = battler.actor.weapon;
            bool skill = battler.actor.skill_activated && !battler.skip_skill_effect;
            bool mweapon = weapon == null ? false : (distance >= 2 && weapon.is_ranged_magic()) || weapon.is_always_magic();
            // this needs to take forceably unequipped anims for dancers into account //Yeti
            return FE_Battler_Image.avoid_animation_value(
                animation_processor(battler, distance), offset(battler));
        }
        /// <summary>
        /// Returns the animation numbers for avoid return animations
        /// </summary>
        internal static List<int> avoid_return_animation_value(Game_Unit battler, int distance)
        {
            FEXNA_Library.Data_Weapon weapon = battler.actor.weapon;
            bool skill = battler.actor.skill_activated && !battler.skip_skill_effect;
            bool mweapon = weapon == null ? false : (distance >= 2 && weapon.is_ranged_magic()) || weapon.is_always_magic();
            // this needs to take forceably unequipped anims for dancers into account //Yeti
            return FE_Battler_Image.avoid_return_animation_value(
                animation_processor(battler, distance), offset(battler));
        }
        /// <summary>
        /// Returns the animation numbers for gethit animations
        /// </summary>
        internal static List<int> get_hit_animation_value(Game_Unit battler, bool crit, int distance)
        {
            FEXNA_Library.Data_Weapon weapon = battler.actor.weapon;
            bool skill = battler.actor.skill_activated && !battler.skip_skill_effect;
            bool mweapon = weapon == null ? false : (distance >= 2 && weapon.is_ranged_magic()) || weapon.is_always_magic();
            // this needs to take forceably unequipped anims for dancers into account //Yeti
            return FE_Battler_Image.get_hit_animation_value(
                animation_processor(battler, distance), offset(battler), crit);
        }

        /// <summary>
        /// Returns the animation numbers for pre-battle animations
        /// </summary>
        internal static List<int> pre_battle_animation_value(Game_Unit battler, int distance)
        {
            FEXNA_Library.Data_Weapon weapon = battler.actor.weapon;
            bool skill = battler.actor.skill_activated && !battler.skip_skill_effect;
            bool mweapon = weapon == null ? false : (distance >= 2 && weapon.is_ranged_magic()) || weapon.is_always_magic();
            // this needs to take forceably unequipped anims for dancers into account //Yeti
            return FE_Battler_Image.pre_battle_animation_value(
                animation_processor(battler, distance), offset(battler));
        }

        #region Spells
        /// <summary>
        /// Returns the animation numbers of the given spell's start up
        /// </summary>
        internal static List<int> spell_attack_animation_value(int weapon_id, bool magic, int distance, bool hit)
        {
            int offset = Global.animation_group("Spells");
            return FE_Battler_Image.spell_attack_animation_value(weapon_id, offset, magic, distance, hit);
        }

        /// <summary>
        /// Returns the animation numbers of the given spell's start up, played behind characters but above platform
        /// </summary>
        internal static List<int> spell_attack_animation_value_bg1(int weapon_id, bool magic, int distance, bool hit)
        {
            int offset = Global.animation_group("Spells");
            return FE_Battler_Image.spell_attack_animation_value_bg1(weapon_id, offset, magic, distance, hit);
        }

        /// <summary>
        /// Returns the animation numbers of the given spell's start up, played behind the platform
        /// </summary>
        internal static List<int> spell_attack_animation_value_bg2(int weapon_id, bool magic, int distance, bool hit)
        {
            int offset = Global.animation_group("Spells");
            return FE_Battler_Image.spell_attack_animation_value_bg2(weapon_id, offset, magic, distance, hit);
        }

        /// <summary>
        /// Returns the animation numbers of the given spell's start up, played above the HUD
        /// </summary>
        internal static List<int> spell_attack_animation_value_fg(int weapon_id, bool magic, int distance, bool hit)
        {
            int offset = Global.animation_group("Spells");
            return FE_Battler_Image.spell_attack_animation_value_fg(weapon_id, offset, magic, distance, hit);
        }

        /// <summary>
        /// Returns the animation numbers of the given spell's ending
        /// </summary>
        internal static List<int> spell_end_animation_value(int weapon_id, bool magic, bool hit)
        {
            int offset = Global.animation_group("Spells");
            return FE_Battler_Image.spell_end_animation_value(weapon_id, offset, magic, hit);
        }

        /// <summary>
        /// Returns the animation numbers of the given spell's life drain effect, after hit but before recovery
        /// </summary>
        internal static List<int> spell_lifedrain_animation_value(int weapon_id, bool magic, int distance)
        {
            int offset = Global.animation_group("Spells");
            return FE_Battler_Image.spell_lifedrain_animation_value(weapon_id, offset, magic, distance);
        }
        /// <summary>
        /// Returns the animation numbers of the given spell's life drain effect, starts at the same time as recovery
        /// </summary>
        internal static List<int> spell_lifedrain_end_animation_value(int weapon_id, bool magic)
        {
            int offset = Global.animation_group("Spells");
            return FE_Battler_Image.spell_lifedrain_end_animation_value(weapon_id, offset, magic);
        }
        #endregion

        internal static List<int> refresh_animation_value(Game_Unit battler, int ring_id)
        {
            int offset = Global.animation_group("Effects");
            return FE_Battler_Image.refresh_animation_value(animation_processor(battler, 1), offset, ring_id); // Distance? //Yeti
        }
        #endregion

        internal static List<int> correct_animation_value(int anim_id, Game_Unit battler)
        {
            int terrain;
            // Pirate Crit Dust
            if (anim_id == Global.animation_group("Pirate-Axe") + 22)
            {
                terrain = Global.scene.scene_type == "Class_Reel" ? ((Scene_Class_Reel)Global.scene).terrain_tag :
                    Global.game_map.terrain_id(battler.loc);
                if (Global.data_terrains.ContainsKey(terrain) && Global.data_terrains[terrain].Dust_Type == 1)
                    return new List<int> { anim_id + 1 };
                return new List<int> { anim_id };
            }
            // Dust
            if (anim_id == Global.animation_group("Effects") + 1)
            {
                terrain = Global.scene.scene_type == "Class_Reel" ? ((Scene_Class_Reel)Global.scene).terrain_tag :
                    Global.game_map.terrain_id(battler.loc);
                if (Global.data_terrains.ContainsKey(terrain) && Global.data_terrains[terrain].Dust_Type == 1)
                    return new List<int> { anim_id + 1 };
                return new List<int> { anim_id };
            }
            // Spiral Dive
            if (anim_id == Global.animation_group("Skills") + 4)
            {
                FEXNA_Library.Data_Weapon weapon = battler.actor.weapon;
                if (weapon.main_type().Name == "Sword")
                    return new List<int> { anim_id + 1 };
                return new List<int> { anim_id };
            }
            return new List<int> { anim_id };
        }

        internal static bool test_for_battler(Game_Unit unit)
        {
            return test_for_battler(unit, unit.actor.used_weapon_type());
        }
        internal static bool test_for_battler(Game_Unit unit, string weapon_type)
        {
            return !string.IsNullOrEmpty(anim_name(unit, weapon_type).name);
        }

        internal static string battler_name(Game_Unit unit)
        {
            return battler_name(unit, unit.actor.used_weapon_type());
        }
        internal static string battler_name(Game_Unit unit, string weapon_type)
        {
            int gender = unit.actor.gender;
            if (FE_Battler_Image.Single_Gender_Battle_Sprite.Contains(unit.actor.class_id))
                gender = (gender / 2) * 2;
            string name;
            // Check if the unit has a personal sprite
            name = unit.actor.name_full + "-" + unit.actor.class_name_full + gender.ToString();
            if (!Global.content_exists(@"Graphics/Animations/" + name + "-" + weapon_type))
            {
                // Check for a generic sprite based on class/gender
                name = unit.actor.class_name_full + gender.ToString();
                if (!Global.content_exists(@"Graphics/Animations/" + name + '-' + weapon_type))
                {
                    // Use generic base gender sprite based on class/base gender
                    gender %= 2;
                    name = unit.actor.class_name_full + gender.ToString();
                    if (!Global.content_exists(@"Graphics/Animations/" + name + '-' + weapon_type))
                        return "";
                }
            }
            return name;
        }

        internal static Anim_Sprite_Data anim_name(Game_Unit unit)
        {
            return anim_name(unit, unit.actor.used_weapon_type());
        }
        internal static Anim_Sprite_Data anim_name(Game_Unit unit, string weapon_type)
        {
            int gender = unit.actor.gender;
            if (FE_Battler_Image.Single_Gender_Battle_Sprite.Contains(unit.actor.class_id))
                gender = (gender / 2) * 2;
            if (string.IsNullOrEmpty(weapon_type))
                return new Anim_Sprite_Data { name = "", gender = gender };

            string name;
            // Check if the unit has a personal sprite
            //name = unit.actor.name_full + "-" + unit.actor.class_name_full + //Debug
            //    gender.ToString() + "-" + weapon;
            name = string.Format("{0}-{1}{2}-{3}", unit.actor.name_full, unit.actor.class_name_full, gender, weapon_type);
            if (!Global.content_exists(@"Graphics/Animations/" + name))
            {
                // Check for a generic sprite based on class/gender
                name = unit.actor.class_name_full + gender.ToString() +
                    '-' + weapon_type;
                if (!Global.content_exists(@"Graphics/Animations/" + name))
                {
                    // Use generic base gender sprite based on class/base gender
                    gender %= 2;
                    name = unit.actor.class_name_full + gender.ToString() +
                        '-' + weapon_type;
                    if (!Global.content_exists(@"Graphics/Animations/" + name))
                    {
                        // Try one last time with the other gender, to at least show something
                        gender = 1 - gender;
                        name = unit.actor.class_name_full + gender.ToString() +
                            '-' + weapon_type;
                        if (!Global.content_exists(@"Graphics/Animations/" + name))
                            return new Anim_Sprite_Data { name = "", gender = gender };
                    }
                }
            }
            return new Anim_Sprite_Data { name = name, gender = gender };
        }
    }

    /*struct Battler_Sprite_Data
    {
        public string filename;
        public int y;
        public Texture2D texture;
    }*/

    struct Anim_Sprite_Data
    {
        public string name;
        public int gender;
    }

    class Animation_Setup_Data_Processor : Animation_Setup_Data
    {
        public Animation_Setup_Data_Processor(Game_Unit battler, int distance)
        {
            Key = string.Format("{0}-{1}", battler.actor.class_name_full,
                battler.actor.used_weapon_type());
            Distance = distance;
            Class_Id = battler.actor.class_id;
            Gender = battler.actor.gender;

            FEXNA_Library.Data_Weapon weapon = battler.actor.weapon;
            Equipped = weapon != null;
            if (Equipped)
            {
                Weapon_Main_Type = weapon.Main_Type;
                Weapon_Thrown = weapon.Thrown(); // Factor out to actor or something to make weapons throwable by skills??? //Debug
                Weapon_Is_Staff = weapon.is_staff();
                MWeapon = (distance >= 2 && weapon.is_ranged_magic()) || weapon.is_always_magic();
            }
            Skill = battler.actor.skill_activated && !battler.skip_skill_effect;
            // Skills
            Astra_Activated = battler.actor.astra_activated;
            Astra_Missed = battler.actor.astra_missed;
            Astra_Count = battler.actor.astra_count;
            Swoop_Activated = battler.swoop_activated;
        }
    }
}
