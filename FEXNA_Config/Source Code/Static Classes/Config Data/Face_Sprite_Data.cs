using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FEXNA_Library;

namespace FEXNA
{
    public static class Face_Sprite_Data
    {
        // When this is moved from here to the editor, the class rename form will need updated to modify this //Yeti
#if DEBUG
        public const string DEFAULT_FACE = "Generic"; //Debug
#else
        public const string DEFAULT_FACE = "Generic";
#endif
        public const string DEFAULT_BATTLE_DEATH_ENEMY = "BrigandM-0";
        public const string DEFAULT_BATTLE_DEATH_ENEMY_COLOR = "Bandit";

        public const int MESSAGE_LINES = 3;
        public readonly static int DEFAULT_EMOTIONS = 3;

        public readonly static int BATTLE_EMOTE_COUNT = 4;
        public readonly static int FACE_COUNT = 6;
        public const bool FACE_TONE = true;
        public const bool GENERIC_FACES = true;
        public const int WORLD_MAP_Y_OFFSET = 12;
        public readonly static Vector2 STATUS_FACE_SIZE = new Vector2(80, 72);
        public readonly static Vector2 MINI_FACE_SIZE = new Vector2(32, 32);
        public readonly static Vector2 BATTLE_FACE_SIZE = new Vector2(106, 28);
        public readonly static Vector2 EYES_FACE_SIZE = new Vector2(32, 16);
        public readonly static Vector2 MOUTH_FACE_SIZE = new Vector2(32, 16);

        public readonly static Dictionary<string, string> CLASS_RENAME = new Dictionary<string, string>
        {
            { "Swordmaster", "Myrmidon" },
            { "Rogue", "Thief" },
            { "Longbowman", "Archer" },
            { "Arbalest", "Crossbowman" },
            { "Halberdier", "Soldier" },
            { "Dragon Master", "Dracoknight" },
            { "Berserker", "Brigand" },
            { "Warrior", "Fighter" },
            { "Raider", "Pirate" },
            { "Hero", "Mercenary" },
            { "Nomad Trooper", "Nomad" },
            { "Paladin", "Gendarme" },
            { "Cataphract", "Cohort" },
            { "Centurion", "Phalanx" },
            { "General", "Lieutenant" },
            { "Stahlfaust", "Zweihander" },
            { "Falcoknight", "Pegasus Knight" },
            { "Bishop", "Monk" },
            { "Sage", "Mage" },
            { "Valkyrie", "Troubadour" },
            { "Hex", "Witch" },
            { "Warlock", "Sorcerer" },
            { "Druid", "Shaman" },
            { "Justice", "Diviner" },
            { "Elder", "Scholar" }
        };
        
        public readonly static Dictionary<string, string> FACE_RENAME =
            new Dictionary<string, string>
        {
            { "Citizen_Man", "Villager_ApprenticeM" },
            { "Citizen_Woman", "Woman1Villager" },
            { "Citizen_Girl", "Girl1Villager" },
        };
        
        public readonly static Dictionary<string, FaceToGeneric> FACE_TO_GENERIC_RENAME =
            new Dictionary<string, FaceToGeneric>
        {
            { "Sally",      new FaceToGeneric("SoldierM-1",     "Tuscana") },
            { "Cuthbert",   new FaceToGeneric("SoldierM-2",     "Tuscana") },

            { "Tory_P",     new FaceToGeneric("MercenaryM-2",   "Bern") },
            { "Steers",     new FaceToGeneric("MercenaryM-3",   "Pirate") },
            { "Kris",       new FaceToGeneric("ArcherM-3",      "Etruria") },
            { "James",      new FaceToGeneric("MonkM-3",        "Etruria") },
            { "Caphel",     new FaceToGeneric("CohortM-3",      "Etruria") },
            { "Gavrilo",    new FaceToGeneric("MageM-3",        "Etruria") },
            { "Haster",     new FaceToGeneric("VanguardF-3",    "Etruria") },
            
            { "Vak",        new FaceToGeneric("SorcererF-3",    "Bandit") },
            { "Connor",     new FaceToGeneric("LieutenantM-3",  "Ostia", "Traitor") },
            
            { "Alcherion",  new FaceToGeneric("LieutenantM-3",  "Immortal") },
            { "Cantabiel",  new FaceToGeneric("SorcererF-3",    "Immortal") },

            { "Alec",       new FaceToGeneric("Mage KnightF-3", "Member") },
            { "Stephen",    new FaceToGeneric("DivinerM-3",     "Member") },
        };
        public readonly static Dictionary<string, FaceCountryRecolor> FACE_COUNTRY_RENAME =
            new Dictionary<string, FaceCountryRecolor>
        {
            { "Etruria_Hemda",      new FaceCountryRecolor("Etruria") },
            { "Merc_Bern",          new FaceCountryRecolor("Bern") },
            { "Merc_Ilia",          new FaceCountryRecolor("Ilia") },
            { "Merc_IliaM",         new FaceCountryRecolor("Ilia") },
            { "Merc_Bern2",         new FaceCountryRecolor("Bern") },
            { "Merc_Etruria",       new FaceCountryRecolor("Etruria") },
            { "Merc_Rebel",         new FaceCountryRecolor("Rebel") },
            { "Merc_Sacae",         new FaceCountryRecolor("Sacae") },
            { "Merc_Western",       new FaceCountryRecolor("Western") },
            { "Bandit_Falsaron",    new FaceCountryRecolor("Bandit") },
            { "Pirate",             new FaceCountryRecolor("Bandit") },
            { "Pirate_Rebel",       new FaceCountryRecolor("Rebel") },
            { "Citizen_Etruria",    new FaceCountryRecolor("Etruria") },
            
            { "Traitor",            new FaceCountryRecolor("Ostia", true) },
            { "Immortal",           new FaceCountryRecolor("Bern") },
            
            { "Junior_Oldest",      new FaceCountryRecolor("Ostia") },
            { "Junior_Middle",      new FaceCountryRecolor("Ostia") },
            { "Junior_Youngest",    new FaceCountryRecolor("Ostia") }
        };
        
        #region Derived Face Data
        public static int EmotionCount(Face_Data faceData)
        {
            return EmotionCount(faceData.Emotions);
        }
        private static int EmotionCount(int emotions)
        {
            return emotions > 0 ? emotions : DEFAULT_EMOTIONS;
        }

        public static int FramesHeight
        {
            get { return (int)EYES_FACE_SIZE.Y + (int)MOUTH_FACE_SIZE.Y; }
        }
        public static int FooterHeight
        {
            get
            {
                // Battler face section height plus miniface height
                return BATTLE_EMOTE_COUNT * (int)BATTLE_FACE_SIZE.Y + (int)MINI_FACE_SIZE.Y;
            }
        }

        public static int EmotionHeight(Face_Data faceData, int textureHeight)
        {
            return EmotionHeight(EmotionCount(faceData), textureHeight);
        }
        private static int EmotionHeight(int emotions, int textureHeight)
        {
            // If there is no footer
            if (textureHeight - FooterHeight <= 0)
                return textureHeight;
            
            int height = textureHeight;
            // Remove footer
            height -= Face_Sprite_Data.FooterHeight;
            // Divide by number of emotion variations
            height /= emotions;
            return height;
        }

        public static int FaceWidth(Face_Data faceData, int textureWidth)
        {
            int width = textureWidth;
            if (faceData.Asymmetrical)
                width /= 2;
            return width;
        }
        public static int FaceHeight(Face_Data faceData, int textureHeight)
        {
            return FaceHeight(EmotionCount(faceData), textureHeight);
        }
        private static int FaceHeight(int emotions, int textureHeight)
        {
            return Math.Max(FramesHeight, EmotionHeight(emotions, textureHeight) - FramesHeight);
        }
        #endregion
    }
}
