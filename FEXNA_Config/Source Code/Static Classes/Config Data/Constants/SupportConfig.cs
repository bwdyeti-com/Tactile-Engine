using System.Collections.Generic;
using FEXNA_Library;

namespace FEXNA.Constants
{
    public class Support
    {
        // Maximum support levels an actor can gain
        public const int SUPPORT_TOTAL = 5;
        // Radius of support bonuses
        public const int SUPPORT_RANGE = 3;

        // The letters labelling each support level
        // Also implicitly the number of support levels
        public readonly static List<string> SUPPORT_LETTERS = new List<string> { "-", "C", "B", "A", "S" };
        public const int MAX_SUPPORT_LEVEL = 3;
        public const int BOND_SUPPORT_RANK = 4;
        public const bool PLAYER_SUPPORT_ONLY = true; // Can only player units gain support points

        public const int MAX_SUPPORT_POINTS = 999; // The maximum number of points a support rank can require; support progress won't count higher
        public const int ADJACENT_SUPPORT_POINTS = 3;
        public const int SAME_TARGET_SUPPORT_POINTS = 2;
        public const int HEAL_SUPPORT_POINTS = 3;
        public const int RESCUE_SUPPORT_POINTS = 5;
        public const int TALK_SUPPORT_POINTS = 5;
        public const int CHAPTER_SUPPORT_POINTS = 10;

        public const bool ONE_SUPPORT_PER_CHAPTER = true;
        public const bool BASE_COUNTS_AS_SEPARATE_CHAPTER = true;
        public const bool NEW_MAP_COUNTS_AS_SEPARATE_CHAPTER = false;

        // Supports for actors that are presumed forced, so they count against
        //     the remaining support count even before they're activated
        public readonly static Dictionary<int, HashSet<int>> RESERVED_SUPPORTS = new Dictionary<int, HashSet<int>>
        {
        };

        public readonly static Dictionary<Affinities, float[]> AFFINITY_BOOSTS = new Dictionary<Affinities, float[]>
        {
            //                                   Atk,  Def,  Hit,  Avo, Crit,  Dod
            { Affinities.Thunder, new float[] { 0.5f, 0.5f, 2.5f,   0f, 2.5f,   0f } }, // 3Off, 1Def
            { Affinities.Dark,    new float[] { 0.5f, 0.5f,   0f, 2.5f,   0f, 2.5f } }, // 1Off, 3Def
            { Affinities.Anima,   new float[] { 0.5f, 0.5f, 2.5f, 2.5f,   0f,   0f } }, // 2Off, 2Def
            { Affinities.Earth,   new float[] { 0.5f, 0.5f,   0f,   0f, 2.5f, 2.5f } }, // 2Off, 2Def
            { Affinities.Fire,    new float[] { 0.5f,   0f, 2.5f, 2.5f,   0f, 2.5f } }, // 2Off, 2Def
            { Affinities.Water,   new float[] { 0.5f,   0f,   0f, 2.5f, 2.5f, 2.5f } }, // 2Off, 2Def
            { Affinities.Ice,     new float[] {   0f, 0.5f, 2.5f, 2.5f, 2.5f,   0f } }, // 2Off, 2Def
            { Affinities.Light,   new float[] {   0f, 0.5f, 2.5f,   0f, 2.5f, 2.5f } }, // 2Off, 2Def
            { Affinities.Wind,    new float[] {   0f,   0f, 2.5f, 2.5f, 2.5f, 2.5f } }, // 2Off, 2Def
            
            /*
            { Affinities.Thunder, new float[] { 0.5f, 0.5f, 2.5f,   0f, 2.5f,   0f } },
            { Affinities.Dark,    new float[] { 0.5f, 0.5f, 2.5f,   0f,   0f, 2.5f } },
            { Affinities.Anima,   new float[] { 0.5f, 0.5f,   0f, 2.5f,   0f, 2.5f } },
            { Affinities.Earth,   new float[] { 0.5f, 0.5f,   0f,   0f, 2.5f, 2.5f } },
            { Affinities.Fire,    new float[] { 0.5f,   0f, 2.5f, 2.5f, 2.5f,   0f } },
            { Affinities.Water,   new float[] { 0.5f,   0f, 2.5f, 2.5f,   0f, 2.5f } },
            { Affinities.Ice,     new float[] {   0f, 0.5f, 2.5f, 2.5f, 2.5f,   0f } },
            { Affinities.Light,   new float[] {   0f, 0.5f,   0f, 2.5f, 2.5f, 2.5f } },
            { Affinities.Wind,    new float[] {   0f,   0f, 2.5f, 2.5f, 2.5f, 2.5f } },
             */

            { Affinities.None,    new float[] { 0,0,0,0,0,0 } },
        };
        // The support bonus for a bond, default is one support rank worth of every stat
        public readonly static float[] BOND_BOOSTS = new float[] { 1f, 1f, 5f, 5f, 5f, 5f };

        // Growth rate change to apply to generics
        public const int AFFINITY_GROWTH_MOD = 5;
        // Stats that each affinity affects
        // The first list in each entry is boosted by that affinity,
        //     the second group is reduced
        public readonly static Dictionary<Affinities, List<Stat_Labels>[]> AFFINITY_GROWTHS =
            new Dictionary<Affinities, List<Stat_Labels>[]>
        {
            { Affinities.Thunder, new List<Stat_Labels>[] {
                new List<Stat_Labels> { Stat_Labels.Pow, Stat_Labels.Skl },
                new List<Stat_Labels> { Stat_Labels.Spd, Stat_Labels.Lck } } },
            { Affinities.Dark, new List<Stat_Labels>[] {
                new List<Stat_Labels> { Stat_Labels.Lck, Stat_Labels.Def },
                new List<Stat_Labels> { Stat_Labels.Skl, Stat_Labels.Spd } } },
            { Affinities.Anima, new List<Stat_Labels>[] {
                new List<Stat_Labels> { Stat_Labels.Pow, Stat_Labels.Res },
                new List<Stat_Labels> { Stat_Labels.Lck, Stat_Labels.Def } } },
            { Affinities.Earth, new List<Stat_Labels>[] {
                new List<Stat_Labels> { Stat_Labels.Skl, Stat_Labels.Def },
                new List<Stat_Labels> { Stat_Labels.Spd, Stat_Labels.Res } } },
            { Affinities.Fire, new List<Stat_Labels>[] {
                new List<Stat_Labels> { Stat_Labels.Pow, Stat_Labels.Lck },
                new List<Stat_Labels> { Stat_Labels.Skl, Stat_Labels.Def } } },
            { Affinities.Water, new List<Stat_Labels>[] {
                new List<Stat_Labels> { Stat_Labels.Spd, Stat_Labels.Lck },
                new List<Stat_Labels> { Stat_Labels.Def, Stat_Labels.Res } } },
            { Affinities.Ice, new List<Stat_Labels>[] {
                new List<Stat_Labels> { Stat_Labels.Spd, Stat_Labels.Res },
                new List<Stat_Labels> { Stat_Labels.Pow, Stat_Labels.Lck } } },
            { Affinities.Light, new List<Stat_Labels>[] {
                new List<Stat_Labels> { Stat_Labels.Def, Stat_Labels.Res },
                new List<Stat_Labels> { Stat_Labels.Pow, Stat_Labels.Skl } } },
            { Affinities.Wind, new List<Stat_Labels>[] {
                new List<Stat_Labels> { Stat_Labels.Skl, Stat_Labels.Spd },
                new List<Stat_Labels> { Stat_Labels.Pow, Stat_Labels.Res } } }
        };
    }
}
