using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FEXNA
{
    public class Map_Animations
    {
        #region Effects
        #region Item Data
        public static int item_effect_id(int item_id)
        {
            if (ITEM_EFFECT_IDS.ContainsKey(item_id))
                return ITEM_EFFECT_IDS[item_id];
            return 1;
        }

        public readonly static Dictionary<int, int> ITEM_EFFECT_IDS = new Dictionary<int, int>
        {
            {  1,  1 }, // Vulnerary
            {  2,  2 }, // Potion
            {  3,  3 }, // Elixir
            {  4,  4 }, // Antitoxin
            {  5,  5 }, // Pure Water
            { 15, 15 }, // Torch
            { 91, 91 }, // Deus's Geas
            { 92, 91 }, // Mulciber's Iron
            { 93, 91 }, // Set's Litany
            { 94, 91 }, // Mot's Mercy
            { 95, 91 }, // El's Passage
        };

        public readonly static Dictionary<int, Map_Effect_Data> ITEM_MAP_EFFECTS =
            new Dictionary<int, Map_Effect_Data>
        {
            #region 1: Vulnerary
            { 1, new Map_Effect_Data { image = new KeyValuePair<string, int>("Vulnerary", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[] { 0, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 0, 32, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 0, 48, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 0, 64, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 1, 80, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 1, 96, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 1, 112, 240 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 1, 128, 224 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 2, 144, 208 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 2, 160, 192 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 2, 176, 176 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 2, 192, 160 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 3, 208, 144 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 3, 224, 128 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 3, 240, 112 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 3, 255, 96 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 4, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 5, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 6, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 7, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 8, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 9, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 10, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 11, 255, 96 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 11, 240, 112 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 11, 224, 128 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 208, 144 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 192, 160 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 176, 176 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 160, 192 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 144, 208 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 128, 224 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 112, 240 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 96, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 80, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 64, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 48, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 32, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 15, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 15, 0, 255 }, 1)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]> (1, new string[] { "s", "Heal" })
                    }
            }},
            #endregion
            #region 2: Potion
            { 2, new Map_Effect_Data { image = new KeyValuePair<string, int>("Potion", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[] { 0, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 0, 32, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 0, 48, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 0, 64, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 1, 80, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 1, 96, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 1, 112, 240 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 1, 128, 224 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 2, 144, 208 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 2, 160, 192 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 2, 176, 176 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 2, 192, 160 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 3, 208, 144 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 3, 224, 128 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 3, 240, 112 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 3, 255, 96 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 4, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 5, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 6, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 7, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 8, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 9, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 10, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 11, 255, 96 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 11, 240, 112 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 11, 224, 128 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 208, 144 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 192, 160 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 176, 176 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 160, 192 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 144, 208 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 128, 224 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 112, 240 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 96, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 80, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 64, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 48, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 32, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 15, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 15, 0, 255 }, 1)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]>(1, new string[] { "s", "Mend" })
                    }
            }},
            #endregion
            #region 3: Elixir
            { 3, new Map_Effect_Data { image = new KeyValuePair<string, int>("Elixir", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[] { 0, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 0, 32, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 0, 48, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 0, 64, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 1, 80, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 1, 96, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 1, 112, 240 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 1, 128, 224 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 2, 144, 208 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 2, 160, 192 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 2, 176, 176 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 2, 192, 160 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 3, 208, 144 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 3, 224, 128 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 3, 240, 112 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 3, 255, 96 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 4, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 5, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 6, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 7, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 8, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 9, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 10, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[] { 11, 255, 96 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 11, 240, 112 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 11, 224, 128 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 208, 144 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 192, 160 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 176, 176 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 160, 192 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 144, 208 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 128, 224 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 112, 240 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 96, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 80, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 64, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 48, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 32, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 15, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 15, 0, 255 }, 1)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]>(1, new string[] { "s", "Recover" })
                    }
            }},
            #endregion
            #region 4: Antitoxin
            { 4, new Map_Effect_Data { image = new KeyValuePair<string, int>("Antitoxin", 1),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[] { 0, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 1, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 2, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 4, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 5, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 6, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 7, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 8, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 9, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 10, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 11, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 12, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 13, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 14, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 15, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 16, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 17, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 18, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 19, 255, 255 }, 3),
                        new KeyValuePair<int[], int>(new int[] { 19, 0, 255 }, 33)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]> (2, new string[]{ "s", "Pure_Water" })
                    }
            }},
            #endregion
            #region 5: Pure Water
            { 5, new Map_Effect_Data { image = new KeyValuePair<string, int>("Pure Water", 1),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[] { 0, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 1, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 2, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 4, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 5, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 6, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 7, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 8, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 9, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 10, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 11, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 12, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 13, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 14, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 15, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 16, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 17, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 18, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 19, 255, 255 }, 3),
                        new KeyValuePair<int[], int>(new int[] { 19, 0, 255 }, 33)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]> (2, new string[]{ "s", "Pure_Water" })
                    }
            }},
            #endregion
            #region 15: Torch
            { 15, new Map_Effect_Data { image = new KeyValuePair<string, int>("Torch", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 5),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 3),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 5),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 3),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 5),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 3),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 5),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 3),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 4)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]>(1, new string[]{ "s", "Torch" }),
                        new KeyValuePair<int, string[]>(32, new string[]{ "torch" })
                    }
            }},
            #endregion
            #region 91: Ring Effect
            { 91, new Map_Effect_Data { image = new KeyValuePair<string, int>("Ring_Effect", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[] {  0, 255, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] {  1, 255, 255 },  2),
                        new KeyValuePair<int[], int>(new int[] {  2, 255, 255 },  2),
                        new KeyValuePair<int[], int>(new int[] {  3, 255, 255 },  2),
                        new KeyValuePair<int[], int>(new int[] {  4, 255, 255 },  2),
                        new KeyValuePair<int[], int>(new int[] {  5, 255, 255 },  2),
                        new KeyValuePair<int[], int>(new int[] {  6, 255, 255 },  2),
                        new KeyValuePair<int[], int>(new int[] {  7, 255, 255 },  2),
                        new KeyValuePair<int[], int>(new int[] {  8, 255, 255 },  2),
                        new KeyValuePair<int[], int>(new int[] {  9, 255, 255 },  2),
                        new KeyValuePair<int[], int>(new int[] { 10, 255, 255 },  2),
                        new KeyValuePair<int[], int>(new int[] { 11, 255, 255 },  2),
                        new KeyValuePair<int[], int>(new int[] { 12, 255, 255 }, 29),
                        new KeyValuePair<int[], int>(new int[] { 12, 240, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12, 224, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12, 208, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12, 192, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12, 176, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12, 160, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12, 144, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12, 128, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12, 112, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12,  96, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12,  80, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12,  64, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12,  48, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12,  32, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12,  16, 255 },  1),
                        new KeyValuePair<int[], int>(new int[] { 12,   0, 255 },  1),
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
        };
        #endregion

        #region Weapon Data
        public static int weapon_effect_id(int weapon_id)
        {
            if (WEAPON_EFFECT_IDS.ContainsKey(weapon_id))
                return WEAPON_EFFECT_IDS[weapon_id];
            return 1;
        }

        public readonly static Dictionary<int, int> WEAPON_EFFECT_IDS = new Dictionary<int, int>
        {
            { 151, 1 }, // Heal
            { 152, 2 }, // Mend
            { 153, 3 }, // Recover
            { 154, 1 }, // Physic
            { 156, 6 }, // Flare
            { 158, 4 }, // Restore
            { 160, 7 }, // Barrier
            { 162, 5 }, // Sleep
            { 169, 5 }, // Slow //Yeti
        };

        public readonly static Dictionary<int, Map_Effect_Data> WEAPON_MAP_EFFECTS =
            new Dictionary<int, Map_Effect_Data>
        {
            #region 1: Heal, Physic
            { 1, new Map_Effect_Data { image = new KeyValuePair<string, int>("Vulnerary", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 0, 32, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 0, 48, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 0, 64, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 80, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 96, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 112, 240 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 128, 224 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 144, 208 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 160, 192 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 176, 176 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 192, 160 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 208, 144 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 224, 128 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 240, 112 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 96 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 7, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 8, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 9, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 10, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 11, 255, 96 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 11, 240, 112 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 11, 224, 128 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 12, 208, 144 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 12, 192, 160 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 12, 176, 176 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 12, 160, 192 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 13, 144, 208 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 13, 128, 224 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 13, 112, 240 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 13, 96, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 14, 80, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 14, 64, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 14, 48, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 14, 32, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 15, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 15, 0, 255 }, 1)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]>(1, new string[]{ "s", "Heal" })
                    }
            }},
            #endregion
            #region 2: Mend
            { 2, new Map_Effect_Data { image = new KeyValuePair<string, int>("Potion", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 0, 32, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 0, 48, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 0, 64, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 80, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 96, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 112, 240 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 128, 224 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 144, 208 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 160, 192 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 176, 176 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 192, 160 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 208, 144 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 224, 128 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 240, 112 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 96 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 7, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 8, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 9, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 10, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 11, 255, 96 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 11, 240, 112 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 11, 224, 128 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 12, 208, 144 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 12, 192, 160 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 12, 176, 176 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 12, 160, 192 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 13, 144, 208 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 13, 128, 224 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 13, 112, 240 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 13, 96, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 14, 80, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 14, 64, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 14, 48, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 14, 32, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 15, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 15, 0, 255 }, 1)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]>(1, new string[]{ "s", "Mend" })
                    }
            }},
            #endregion
            #region 3: Recover
            { 3, new Map_Effect_Data { image = new KeyValuePair<string, int>("Elixir", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 0, 32, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 0, 48, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 0, 64, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 80, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 96, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 112, 240 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 128, 224 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 144, 208 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 160, 192 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 176, 176 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 192, 160 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 208, 144 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 224, 128 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 240, 112 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 96 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 7, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 8, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 9, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 10, 255, 96 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 11, 255, 96 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 11, 240, 112 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 11, 224, 128 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 12, 208, 144 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 12, 192, 160 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 12, 176, 176 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 12, 160, 192 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 13, 144, 208 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 13, 128, 224 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 13, 112, 240 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 13, 96, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 14, 80, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 14, 64, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 14, 48, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 14, 32, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 15, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 15, 0, 255 }, 1)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]>(1, new string[]{ "s", "Recover" })
                    }
            }},
            #endregion
            #region 4: Restore
            { 4, new Map_Effect_Data { image = new KeyValuePair<string, int>("Restore", 43),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 0, 255 }, 8),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 255 }, 3),
                        new KeyValuePair<int[], int>(new int[]{ 6, 224, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 192, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 160, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 128, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 160, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 192, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 224, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 224, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 192, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 160, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 128, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 160, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 192, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 224, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 255 }, 3),
                        new KeyValuePair<int[], int>(new int[]{ 6, 240, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 224, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 208, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 192, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 176, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 160, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 144, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 128, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 112, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 96, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 80, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 64, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 48, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 32, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 16, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 0, 255 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 6, 0, 255 }, 8),
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]>(1, new string[]{ "d" }),
                        new KeyValuePair<int, string[]>(10, new string[]{ "s", "Restore" }),
                        new KeyValuePair<int, string[]>(64, new string[]{ "b" })
                    }
            }},
            #endregion
            #region 5: Sleep
            { 5, new Map_Effect_Data { image = new KeyValuePair<string, int>("Sleep_Staff", 148),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 0, 255 }, 8),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 7, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 8, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 9, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 10, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 11, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 12, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 13, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 14, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 15, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 16, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 17, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 18, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 19, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 20, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 21, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 22, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 23, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 24, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 25, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 26, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 27, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 28, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 29, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 30, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 31, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 32, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 33, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 34, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 35, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 36, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 37, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 38, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 39, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 40, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 41, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 42, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 43, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 44, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 45, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 46, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 47, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 48, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 49, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 50, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 51, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 52, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 53, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 54, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 55, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 56, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 57, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 58, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 59, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 60, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 61, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 62, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 63, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 64, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 65, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 66, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 67, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 68, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 69, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 70, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 71, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 72, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 73, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 73, 0, 255 }, 53),
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]>(1, new string[]{ "d" }),
                        new KeyValuePair<int, string[]>(9, new string[]{ "s", "Sleep" }),
                        new KeyValuePair<int, string[]>(59, new string[]{ "s", "Sleep" }),
                        new KeyValuePair<int, string[]>(109, new string[]{ "s", "Sleep" }),
                        new KeyValuePair<int, string[]>(192, new string[]{ "b" })
                    }
            }},
            #endregion
            #region 6: Flare
            { 6, new Map_Effect_Data { image = new KeyValuePair<string, int>("Torch", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 5),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 3),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 5),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 3),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 5),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 3),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 5),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 3),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 4),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 4)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]>(1, new string[]{ "s", "Torch" }),
                        new KeyValuePair<int, string[]>(32, new string[]{ "torch" })
                    }
            }},
            #endregion
            #region 7: Barrier
            { 7, new Map_Effect_Data { image = new KeyValuePair<string, int>("Barrier", 48),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 0, 255 }, 8),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 255 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 0, 0, 255 }, 53),
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    {
                        new KeyValuePair<int, string[]>(1, new string[]{ "d" }),
                        new KeyValuePair<int, string[]>(9, new string[]{ "s", "Barrier" }),
                        new KeyValuePair<int, string[]>(92, new string[]{ "b" })
                    }
            }},
            #endregion
        };
        #endregion

        #region Skill Data
        public readonly static Dictionary<int, Map_Effect_Data> SKILL_MAP_EFFECTS =
            new Dictionary<int, Map_Effect_Data>
        {
        };
        #endregion

        #region Status Data
        public static int status_effect_id(int status_id)
        {
            if (STATUS_EFFECT_IDS.ContainsKey(status_id))
                return STATUS_EFFECT_IDS[status_id];
            return 1;
        }

        public readonly static Dictionary<int, int> STATUS_EFFECT_IDS = new Dictionary<int, int>
        {
            {  1,  1 }, // Poison Loop
            {  2,  2 }, // Silence Loop
            {  3,  3 }, // Sleep Loop
            {  4,  4 }, // Berserk Loop
            {  5,  5 }, // Slow Loop
            {  7,  7 }, // Drunk Loop
            {  8, 11 }, // Renewal Loop //Yeti
            { 11, 11 }, // Deus Loop
            { 12, 12 }, // Mulciber Loop
            { 13, 13 }, // Set Loop
            { 14, 14 }, // Mot Loop
            { 15, 15 }, // El Loop
        };

        public readonly static Dictionary<int, Map_Effect_Data> STATUS_MAP_EFFECTS =
            new Dictionary<int, Map_Effect_Data>
        {
            #region 1: Poison Loop
            { 1, new Map_Effect_Data { image = new KeyValuePair<string, int>("Poison", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 7, 255, 0 }, 16),
                        new KeyValuePair<int[], int>(new int[]{ 8, 255, 0 }, 24)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
            #region 2: Silence Loop
            { 2, new Map_Effect_Data { image = new KeyValuePair<string, int>("Silence", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 },  4),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 },  4),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 36),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 },  4),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 },  4),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 20)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
            #region 3: Sleep Loop
            { 3, new Map_Effect_Data { image = new KeyValuePair<string, int>("Sleep", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 16),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 }, 16),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 16),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 16),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 0 }, 16),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 0 }, 16),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 0 }, 16)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
            #region 4: Berserk Loop
            { 4, new Map_Effect_Data { image = new KeyValuePair<string, int>("Berserk", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 7, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 8, 255, 0 },  8)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
            #region 5: Slow Loop
            { 5, new Map_Effect_Data { image = new KeyValuePair<string, int>("Slow", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 },  60),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 },  60),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 },  60),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 },  60)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
            #region 7: Poison Loop
            { 7, new Map_Effect_Data { image = new KeyValuePair<string, int>("Drunk", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 0 },  8),
                        new KeyValuePair<int[], int>(new int[]{ 7, 255, 0 }, 16),
                        new KeyValuePair<int[], int>(new int[]{ 8, 255, 0 }, 24)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
            #region 11: Deus Loop
            { 11, new Map_Effect_Data { image = new KeyValuePair<string, int>("Ring_Buff", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{  0, 255, 0 }, 20),
                        new KeyValuePair<int[], int>(new int[]{ 10, 255, 0 }, 12)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
            #region 12: Mulciber Loop
            { 12, new Map_Effect_Data { image = new KeyValuePair<string, int>("Ring_Buff", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{  1, 255, 0 }, 20),
                        new KeyValuePair<int[], int>(new int[]{ 10, 255, 0 }, 12)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
            #region 13: Set Loop
            { 13, new Map_Effect_Data { image = new KeyValuePair<string, int>("Ring_Buff", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{  2, 255, 0 }, 20),
                        new KeyValuePair<int[], int>(new int[]{ 10, 255, 0 }, 12)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
            #region 14: Mot Loop
            { 14, new Map_Effect_Data { image = new KeyValuePair<string, int>("Ring_Buff", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{  3, 255, 0 }, 20),
                        new KeyValuePair<int[], int>(new int[]{ 10, 255, 0 }, 12)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
            #region 15: El Loop
            { 15, new Map_Effect_Data { image = new KeyValuePair<string, int>("Ring_Buff", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{  4, 255, 0 }, 20),
                        new KeyValuePair<int[], int>(new int[]{ 10, 255, 0 }, 12)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
            #region 31: Poison
            { 31, new Map_Effect_Data { image = new KeyValuePair<string, int>("Poison", 49),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{  9, 255, 0 }, 7),
                        new KeyValuePair<int[], int>(new int[]{ 10, 255, 0 }, 7),
                        new KeyValuePair<int[], int>(new int[]{ 11, 255, 0 }, 7),
                        new KeyValuePair<int[], int>(new int[]{  9, 255, 0 }, 7),
                        new KeyValuePair<int[], int>(new int[]{ 10, 255, 0 }, 7),
                        new KeyValuePair<int[], int>(new int[]{ 11, 255, 0 }, 7),
                        new KeyValuePair<int[], int>(new int[]{ 12, 255, 0 }, 6),
                        new KeyValuePair<int[], int>(new int[]{ 13, 255, 0 }, 6),
                        new KeyValuePair<int[], int>(new int[]{ 14, 255, 0 }, 5),
                        new KeyValuePair<int[], int>(new int[]{ 15, 255, 0 }, 5),
                        new KeyValuePair<int[], int>(new int[]{ 16, 255, 0 }, 5),
                        new KeyValuePair<int[], int>(new int[]{ 17, 255, 0 }, 4)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>>
                    { 
                        new KeyValuePair<int, string[]>(1, new string[]{ "s", "Poison" })
                    }
            }}
            #endregion
        };
        #endregion

        #region Generic Data
        public readonly static Dictionary<int, Map_Effect_Data> MAP_EFFECTS =
            new Dictionary<int, Map_Effect_Data>
        {
            #region 1: Snag Attack
            { 1, new Map_Effect_Data { image = new KeyValuePair<string, int>("Snag", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 6),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 7, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 8, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 9, 255, 0 }, 2)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> { }
            }},
            #endregion
            #region 2: Snag Destroy
            { 2, new Map_Effect_Data { image = new KeyValuePair<string, int>("Snag", 49),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[] {  0, 255, 0 }, 6),
                        new KeyValuePair<int[], int>(new int[] {  1, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[] { 10, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 11, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 12, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 13, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 14, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 15, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 16, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 17, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 18, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 19, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 20, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 21, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 22, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 23, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 24, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 25, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 26, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 27, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 28, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 29, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 30, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 31, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 32, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 33, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 34, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 35, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 36, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 37, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 38, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 39, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 40, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 41, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 42, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 43, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 44, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 45, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 46, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 47, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 48, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 49, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 50, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 51, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 52, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 53, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 54, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 55, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 56, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 57, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 58, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 59, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 60, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 61, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 62, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 63, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[] { 64, 255, 0 }, 1),
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> {
                        new KeyValuePair<int, string[]>(52, new string[] { "s", "Death" })
                    }
            }},
            #endregion
            #region 6: FoW Block
            { 6, new Map_Effect_Data { image = new KeyValuePair<string, int>("FoW-Block", 30),
                animation_data = new List<KeyValuePair<int[], int>>
                    {
                        new KeyValuePair<int[], int>(new int[]{ 8, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 0, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 1, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 2, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 3, 255, 0 }, 1),
                        new KeyValuePair<int[], int>(new int[]{ 4, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 6, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 7, 255, 0 }, 2),
                        new KeyValuePair<int[], int>(new int[]{ 5, 255, 0 }, 27),
                        new KeyValuePair<int[], int>(new int[]{ 8, 255, 0 }, 12)
                    },
                    processing_data = new List<KeyValuePair<int,string[]>> {
                        new KeyValuePair<int, string[]>(1, new string[]{ "s", "FoW_Surprise" })
                    }
            }},
            #endregion
        };
        #endregion

        public static Map_Effect_Data effect_data(ref int type, ref int id)
        {
            switch (type)
            {
                case 0:
                    if (ITEM_EFFECT_IDS.ContainsKey(id))
                        return ITEM_MAP_EFFECTS[id];
                    break;
                case 1:
                    if (WEAPON_MAP_EFFECTS.ContainsKey(id))
                        return WEAPON_MAP_EFFECTS[id];
                    break;
                case 2:
                    if (SKILL_MAP_EFFECTS.ContainsKey(id))
                        return SKILL_MAP_EFFECTS[id];
                    break;
                case 3:
                    if (STATUS_MAP_EFFECTS.ContainsKey(id))
                        return STATUS_MAP_EFFECTS[id];
                    break;
                case 4:
                    if (MAP_EFFECTS.ContainsKey(id))
                        return MAP_EFFECTS[id];
                    break;
            }
            type = 0;
            id = 1;
            return ITEM_MAP_EFFECTS[id];
        }
        #endregion

        #region Unit Animations
        #region Dance Anims
        protected static Dictionary<int, Map_Unit_Animation_Data> DANCE_ANIMS = new Dictionary<int, Map_Unit_Animation_Data>
        {
            #region 113: Minstrel Play
            { 113, new Map_Unit_Animation_Data { data = new List<Map_Unit_Animation_Data_Frame>
                {
                    new Map_Unit_Animation_Data_Frame { Frame_Index = 0, Time = 22, Change_Image = true, Moving = true },
                    new Map_Unit_Animation_Data_Frame { Frame_Index = 7, Time =  6, Change_Image = true, Moving = false },
                    new Map_Unit_Animation_Data_Frame { Frame_Index = 8, Time = 15 },
                    new Map_Unit_Animation_Data_Frame { Frame_Index = 7, Time =  3 },
                    new Map_Unit_Animation_Data_Frame { Frame_Index = 6, Time = 11 },
                    new Map_Unit_Animation_Data_Frame { Frame_Index = 7, Time =  5 },
                    new Map_Unit_Animation_Data_Frame { Frame_Index = 8, Time = 72 },
                },
                processing_data = new List<KeyValuePair<int,string[]>> {
                    new KeyValuePair<int, string[]>(37, new string[] { "s", "Minstrel_Strum" })
                }
            }},
            #endregion
        };
        #endregion

        public static Map_Unit_Animation_Data unit_data(int type, int id)
        {
            switch (type)
            {
                // Dance
                case 1:
                    if (DANCE_ANIMS.ContainsKey(id))
                        return DANCE_ANIMS[id];
                    break;
                // Sacrifice I guess? //Yeti
                case 2:
                    break;
            }
            return null;
        }
        #endregion
    }

    public class Map_Effect_Data
    {
        public KeyValuePair<string, int> image;
        public List<KeyValuePair<int[], int>> animation_data;
        public List<KeyValuePair<int, string[]>> processing_data;
    }

    public class Map_Unit_Animation_Data
    {
        public List<Map_Unit_Animation_Data_Frame> data;
        public List<KeyValuePair<int, string[]>> processing_data;
    }

    public struct Map_Unit_Animation_Data_Frame
    {
        public bool Change_Image;
        public string Image_Name;
        public Vector2 Image_Cells;
        public bool Moving;
        public int Time;
        public int Frame_Index;
    }
}
