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
        internal static Animation_Setup_Data animation_processor(BattlerSpriteData battler, int distance)
        {
            return new Animation_Setup_Data_Processor(battler, distance);
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
        internal static List<int> attack_animation_value(BattlerSpriteData battler, bool crit, int distance, bool hit)
        {
            bool mweapon = battler.MWeapon(distance);
            return FE_Battler_Image.attack_animation_value(
                animation_processor(battler, distance), battler.AnimationGroupOffset, crit, hit);
        }

        // Returns the animation numbers of the given class when frozen after attacking
        internal static List<int> still_animation_value(BattlerSpriteData battler, bool crit, int distance, bool hit)
        {
            bool mweapon = battler.MWeapon(distance);
            return FE_Battler_Image.still_animation_value(
                animation_processor(battler, distance), battler.AnimationGroupOffset, crit, hit);
        }

        // Returns the animation numbers of the given class as it returns after attack
        internal static List<int> return_animation_value(BattlerSpriteData battler, bool crit, int distance, bool hit, bool no_damage)
        {
            bool mweapon = battler.MWeapon(distance);
            return FE_Battler_Image.return_animation_value(
                animation_processor(battler, distance), battler.AnimationGroupOffset, crit, hit, no_damage);
        }
        #endregion

        #region Dance
        internal static List<int> dance_attack_animation_value(BattlerSpriteData battler)
        {
            return FE_Battler_Image.dance_attack_animation_value(
                animation_processor(battler, 1), battler.AnimationGroupOffset); // Distance? //Yeti
        }

        internal static List<int> dance_still_animation_value(BattlerSpriteData battler)
        {
            return FE_Battler_Image.dance_still_animation_value(
                animation_processor(battler, 1), battler.AnimationGroupOffset); // Distance? //Yeti
        }

        internal static List<int> dance_return_animation_value(BattlerSpriteData battler)
        {
            return FE_Battler_Image.dance_return_animation_value(
                animation_processor(battler, 1), battler.AnimationGroupOffset); // Distance? //Yeti
        }
        #endregion

        /// <summary>
        /// Returns the animation numbers for idle animations
        /// </summary>
        internal static List<int> idle_animation_value(BattlerSpriteData battler, int distance)
        {
            bool mweapon = battler.MWeapon(distance);
            // this needs to take forceably unequipped anims for dancers into account //Yeti
            return FE_Battler_Image.idle_animation_value(
                animation_processor(battler, distance), battler.AnimationGroupOffset);
        }
        /// <summary>
        /// Returns the animation numbers for avoid animations
        /// </summary>
        internal static List<int> avoid_animation_value(BattlerSpriteData battler, int distance)
        {
            bool mweapon = battler.MWeapon(distance);
            // this needs to take forceably unequipped anims for dancers into account //Yeti
            return FE_Battler_Image.avoid_animation_value(
                animation_processor(battler, distance), battler.AnimationGroupOffset);
        }
        /// <summary>
        /// Returns the animation numbers for avoid return animations
        /// </summary>
        internal static List<int> avoid_return_animation_value(BattlerSpriteData battler, int distance)
        {
            bool mweapon = battler.MWeapon(distance);
            // this needs to take forceably unequipped anims for dancers into account //Yeti
            return FE_Battler_Image.avoid_return_animation_value(
                animation_processor(battler, distance), battler.AnimationGroupOffset);
        }
        /// <summary>
        /// Returns the animation numbers for gethit animations
        /// </summary>
        internal static List<int> get_hit_animation_value(BattlerSpriteData battler, bool crit, int distance)
        {
            bool mweapon = battler.MWeapon(distance);
            // this needs to take forceably unequipped anims for dancers into account //Yeti
            return FE_Battler_Image.get_hit_animation_value(
                animation_processor(battler, distance), battler.AnimationGroupOffset, crit);
        }

        /// <summary>
        /// Returns the animation numbers for pre-battle animations
        /// </summary>
        internal static List<int> pre_battle_animation_value(BattlerSpriteData battler, int distance)
        {
            bool mweapon = battler.MWeapon(distance);
            // this needs to take forceably unequipped anims for dancers into account //Yeti
            return FE_Battler_Image.pre_battle_animation_value(
                animation_processor(battler, distance), battler.AnimationGroupOffset);
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

        internal static List<int> refresh_animation_value(BattlerSpriteData battler, int ring_id)
        {
            int offset = Global.animation_group("Effects");
            return FE_Battler_Image.refresh_animation_value(animation_processor(battler, 1), offset, ring_id); // Distance? //Yeti
        }
        #endregion

        internal static List<int> correct_animation_value(int anim_id, BattlerSpriteData battler)
        {
            int terrain;
            // Pirate Crit Dust
            if (anim_id == Global.animation_group("Pirate-Axe") + 22)
            {
                terrain = Global.scene.scene_type == "Class_Reel" ? ((Scene_Class_Reel)Global.scene).terrain_tag :
                    Global.game_map.terrain_id(battler.Unit.loc);
                if (Global.data_terrains.ContainsKey(terrain) && Global.data_terrains[terrain].Dust_Type == 1)
                    return new List<int> { anim_id + 1 };
                return new List<int> { anim_id };
            }
            // Dust
            if (anim_id == Global.animation_group("Effects") + 1)
            {
                terrain = Global.scene.scene_type == "Class_Reel" ? ((Scene_Class_Reel)Global.scene).terrain_tag :
                    Global.game_map.terrain_id(battler.Unit.loc);
                if (Global.data_terrains.ContainsKey(terrain) && Global.data_terrains[terrain].Dust_Type == 1)
                    return new List<int> { anim_id + 1 };
                return new List<int> { anim_id };
            }
            // Spiral Dive
            if (anim_id == Global.animation_group("Skills") + 4)
            {
                FEXNA_Library.Data_Weapon weapon = battler.Weapon;
                if (weapon.main_type().Name == "Sword")
                    return new List<int> { anim_id + 1 };
                return new List<int> { anim_id };
            }
            return new List<int> { anim_id };
        }
    }

    /*struct Battler_Sprite_Data
    {
        public string filename;
        public int y;
        public Texture2D texture;
    }*/
}
