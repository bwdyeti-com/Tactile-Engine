namespace FEXNA.Constants
{
    public enum StatLabelColoring
    {
        None,
        Averages, // Stat labels are colored based on how far from the average the stat value is
        Growths // Stat labels are colored based on the unit's growth in that stat
    }

    public class Actor
    {
        public const char ACTOR_NAME_DELIMITER = '_';
        public const char BUILD_NAME_DELIMITER = '-';

        public const int MAX_ACTOR_COUNT = short.MaxValue / 2; // Non-generic Ids must <= this value; generics start counting after this
        public const int MAX_HP = 80; // Normal max hp, used for gauge lengths
        public const int MAX_STAT = 30; // Normal max core stat, used for gauge lengths
        public const float HP_VALUE = 0.5f; // The value of hp, compared to other stats
        public const int LUCK_CAP = 30;
        public const float LOW_HEALTH_RATE = 0.4f;//0.33f; //Debug

        public const int LVL_CAP = 20;
        public const int TIER0_LVL_CAP = 10;
        public const int EXP_TO_LVL = 100;
        public const int EXP_PER_ENEMY = 100; // The total exp an individual enemy can give; -1 is infinite
        public const bool EXP_PER_ENEMY_KILL_EXCEPTION = true; // If true, the exp gained from the killing blow will always be the full amount
        public const bool CONSERVE_WASTED_GROWTHS = true;
        public const int CONSERVED_GROWTH_MAX_PER_STAT = 50;
        public const bool ACTOR_GAINED_HP_HEAL = true; // Should max hp gained from level ups/stat boosters be healed?
        public const bool CITIZENS_GAIN_EXP = true; // Non-generic player allied AI can gain exp (cannot gain levels)
        public const bool SEMIFIXED_LEVELS_AT_PREPARATIONS = true; // Should levels gained during preparations, from BExp etc, be semi-fixed
        public const bool SEMIFIXED_FIRST_LEVEL_UP = true; // Should first level a unit gains always be semi-fixed
        public const bool NO_EMPTY_LEVELS = true; // Should levels with no stat gains give at least one point if no stats are already capped

        public const float GENERIC_RANDOM_LEVEL_PERCENT = 1f / 8; // The percentage of generic unit levels that give rng based stat ups instead of fixed
        public const int GENERIC_RANDOM_LEVEL_MAX = 4; // Maximum amount of a generic unit's levels that should be random
        public const bool GENERIC_AUTO_WEXP = true; // Should generic units automatically gain enough wexp to use weapons in their inventory
        public const bool GENERIC_ACTOR_RANDOM_AFFINITIES = true;

        public const StatLabelColoring STAT_LABEL_COLORING = StatLabelColoring.Averages;
        public const bool STAT_COLORS_ONLY_IN_PREP = false; // Does stat label coloring only occur in preparations and the world map
        public const bool ONLY_PC_STAT_COLORS = true; // Are status screen averages only shown for PCs
        public const int GROWTH_AVERAGE_COLOR_MIN = 0; // For growth stat coloring. Anything equal or lesser will be the darkest red.
        public const int GROWTH_AVERAGE_COLOR_MED = 30; // For growth stat coloring. Anything equal will be exact white.
        public const int GROWTH_AVERAGE_COLOR_MAX = 60; // For growth stat coloring. Anything equal or greater will be the brightest green.

        public const bool CHILD_WEAPON_TYPE_ALLOWS_PARENT = true; // Having ranks in a child weapon type allows using all of its ancestors
        public const bool PARENT_WEAPON_TYPE_ALLOWS_CHILD = true; // Having ranks in a parent weapon type allows using all of its children
        public const bool SHOW_ALL_ACTOR_WEAPON_TYPES = false; // Dynamically adds hidden weapon types to the status screen if the actor has them

        public const int NUM_ITEMS = 6;
        public const bool ALLOW_UNEQUIP = true;
        public const bool ONE_S_RANK = true;
        public const int S_RANK_BONUS = 5;

        public const int CASUAL_MODE_LIVES = 3;
    }
}
