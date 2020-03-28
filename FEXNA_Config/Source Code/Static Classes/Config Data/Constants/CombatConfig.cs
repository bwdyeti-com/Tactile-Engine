using System.Collections.Generic;

namespace FEXNA.Constants
{
    public class Combat
    {
        public const int AOE_EXP_GAIN = 10; // Max exp gain when hitting multiple targets, for balance
        public const bool AOE_WEXP_GAIN = false;

        public const int CURSE_BACKFIRE_RATE = 1; // Chance for a cursed weapon to backfire with capped Luck
        public const float LIFE_STEAL_MULT = 0.5f; // Amount of damage to return as health for life steal attribute weapons
        public const int DBL_ATK_SPD = 4;
        public const float CRIT_MULT = 3f;
        public const int BASE_COMBAT_EXP = 10;
        public const int STEAL_EXP = 10;
        public const int DANCE_EXP = 10;
        public const bool RING_REFRESH = true;
        public const bool STAFF_HEAL_WITH_RES = true; // Staves heal with Res instead of Mag?
        public const float STAFF_HEAL_POW_RATE = 0.5f;
        public const bool HALVE_EFFECTIVENESS_ON_FORTS = true; // Halve the bonus damage of effective weapons on healing terrain?

        public const bool IMBUE_WITH_RES = true; // Using magic attacks with physical weapons uses Res intsead of Str?
        public const float MAGIC_WEAPON_STR_RATE = 1f; // Multiplier to imbued weapon user's atk stat when casting magic
        public const float MAGIC_WEAPON_MGT_RATE = 1f; // Multiplier to imbued weapon Mgt when casting magic
        public const float MAGIC_WEAPON_CRT_RATE = 0.5f; // Multiplier to imbued weapon final crt rate when casting magic

        public const bool HIT_OVERFLOW = true; // Does Hit over 100 convert into Crit?
        public const float HIT_OVERFLOW_RATE = 0.5f;

        public const bool BRAVE_BLOCKED_AGAINST_DESTROYABLE = true; // Are brave attacks disallowed when breaking destructible terrain?
        public const bool WEAPON_USE_MISS = true; // Are weapon uses still consumed on miss? (if true this overrides the two below)
        public const bool RANGED_USE_MISS = true; // Are thrown weapon/bow uses still consumed on miss?
        public const bool MAGIC_USE_MISS = true; // Are magic weapon uses still consumed on miss?
        public const bool AI_WEAPON_USE = true; // Do AI units use up weapons normally, as opposed to infinite everything
        // If true and AI_WEAPON_USE disabled, treats all AI attacks the same as PC misses for weapon usage
        // (so AI tomes/staves/ranged might be consumed, but melee weapons will still be infinite)
        public const bool AI_WEAPON_USE_MISS_OVERRIDE = false;

        public const int BARRIER_BONUS = 7;

        public readonly static Dictionary<Difficulty_Modes, float> NON_KILL_EXP_MULTIPLIER =
            new Dictionary<Difficulty_Modes, float>
            {
                { Difficulty_Modes.Normal, 1f },
                { Difficulty_Modes.Hard, 0.5f },
                //{ Difficulty_Modes.Lunatic, 0.5f },
                //{ Difficulty_Modes.Lunatic_Plus, 0.5f }
            };
        public readonly static Dictionary<Difficulty_Modes, float> KILL_EXP_MULTIPLIER =
            new Dictionary<Difficulty_Modes, float>
            {
                { Difficulty_Modes.Normal, 1f },
                { Difficulty_Modes.Hard, 1f },
                //{ Difficulty_Modes.Lunatic, 1f },
                //{ Difficulty_Modes.Lunatic_Plus, 1f }
            };
        public readonly static Dictionary<Difficulty_Modes, float> EXP_MULTIPLIER =
            new Dictionary<Difficulty_Modes, float>
            {
                { Difficulty_Modes.Normal, 1f },
                { Difficulty_Modes.Hard, 1f },
                //{ Difficulty_Modes.Lunatic, 1f },
                //{ Difficulty_Modes.Lunatic_Plus, 1f }
            };
    }
}
