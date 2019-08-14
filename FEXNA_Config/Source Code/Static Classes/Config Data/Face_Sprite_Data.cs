using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FEXNA_Library;

namespace FEXNA
{
    public class Face_Sprite_Data
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
        #region Emotion List
        public readonly static Dictionary<string, int> EMOTION_LIST = new Dictionary<string, int> {
            { "Uther", 4 },
            { "Uther_Wounded", 4 },
            { "Uther_Wounded2", 4 },
			{ "Eagler", 2 },
            { "Toni", 1 },
            { "Cybil", 2 },
            { "Magnus", 2 },
            { "Eiry", 1 },
            { "Augustus", 2 },
            { "Leonard", 1 },
            { "Niime", 2 },
            { "Bennet", 1 },
            { "Wallace", 2 },
			{ "Chester", 2 },
            { "Roeis", 2 },
            { "Hyde", 1 },

            { "Eliza", 2 },
			{ "Brendan", 1 },
			{ "Melanie", 1 },
			{ "Zephyr", 2 },
			{ "Vaida", 2 },
			{ "Elle", 2 },
			{ "William", 2 },

            { "Tristan", 2 },

            { "Lionel", 3 },

			{ "Elbert", 4 },
			{ "Belmont", 2 },
			{ "Solomon", 2 },
			{ "Solomon_Hooded", 2 },
            
			{ "Isaac", 2 },
			{ "Meredith", 2 },
			{ "Deacon", 1 },

            { "Mazda", 1 },
			{ "Darin", 2 },
			{ "Uriel", 1 },
			{ "Milo", 2 },
			{ "Richard", 2 },
			{ "Hubart", 2 },
			{ "Ephidel_Hooded", 2 },
            
			{ "Ulric", 2 },
            { "Desmond", 2 },
			{ "Kalten", 2 },
			{ "Bart", 2 },
			{ "Orun", 2 },
			{ "Pellenore", 1 },
            
            { "Anoleis", 1 },
            { "Sarathi", 1 },
            { "Beck", 1 },
            { "Boston", 1 },
			{ "Crane", 3 },
            
            { "MyrmidonM", 1 },
            { "ThiefM", 1 },
            { "ArcherM", 2 },
            { "ArcherF", 1 },
            { "SoldierM", 1 },
            { "FighterM", 1 },
            { "BrigandM", 2 },
            { "PirateM", 1 },
            { "MercenaryM", 2 },
            { "VanguardM", 1 },
            { "VanguardF", 1 },
            { "GendarmeM", 1 },
            { "CohortM", 1 },
            { "LieutenantM", 1 },
            { "Pegasus KnightF", 1 },
            { "MonkM", 1 },
            { "MageM", 2 },
            { "Mage KnightF", 2 },
            { "TroubadourF", 2 },
            { "SorcererM", 1 },
			{ "DivinerM", 1 },
            
            { "Man1Villager", 1 },
            { "Man4Villager", 1 },
            { "Man6Villager", 1 },
            { "Woman1Villager", 2 },
            { "Woman2Villager", 2 },
            { "Woman4Villager", 2 },
            
            { "Armory", 1 },
            { "Vendor", 1 },
            { "Secret", 1 },
            { "Arena", 1 },
            { "Generic", 2 },
        };
        #endregion

        #region Face Offsets
        // Eyes, mouth, status offset, status expression?
        public readonly static Dictionary<string, Face_Sprite_Data> FACE_OFFSETS = new Dictionary<string, Face_Sprite_Data> {
            { "Uther",           new Face_Sprite_Data(new Vector2(28, 27), new Vector2(28, 43), new Vector2( 7, 3), 0) },
            { "Uther_Wounded",   new Face_Sprite_Data(new Vector2(28, 27), new Vector2(28, 43), new Vector2( 7, 3), 0) },
            { "Uther_Wounded2",  new Face_Sprite_Data(new Vector2(28, 27), new Vector2(28, 43), new Vector2( 7, 3), 0) },
            { "Marcus",          new Face_Sprite_Data(new Vector2(26, 23), new Vector2(26, 39), new Vector2( 9, 0), 0) },
            { "Harken",          new Face_Sprite_Data(new Vector2(28, 24), new Vector2(28, 40), new Vector2(13, 0), 0) },
            { "Isadora",         new Face_Sprite_Data(new Vector2(26, 33), new Vector2(26, 49), new Vector2(11, 0), 1) },
			{ "Eagler",          new Face_Sprite_Data(new Vector2(33, 22), new Vector2(33, 38), new Vector2(11, 2), 0) },
            { "Madelyn",         new Face_Sprite_Data(new Vector2(24, 24), new Vector2(24, 40), new Vector2( 8, 0), 0) },
            { "Toni",            new Face_Sprite_Data(new Vector2(31, 22), new Vector2(31, 38), new Vector2(10, 0), 1) },
            { "Hassar",          new Face_Sprite_Data(new Vector2(27, 24), new Vector2(27, 40), new Vector2(10, 0), 0) },
            { "Cybil",           new Face_Sprite_Data(new Vector2(33, 18), new Vector2(33, 34), new Vector2(14, 0), 0) },
            { "Cybil_2",         new Face_Sprite_Data(new Vector2(29, 23), new Vector2(29, 49), new Vector2( 8, 0), 0) },
            { "Magnus",          new Face_Sprite_Data(new Vector2(15, 33), new Vector2(15, 49), new Vector2( 3, 3), 0) },
            { "Eiry",            new Face_Sprite_Data(new Vector2(28, 32), new Vector2(28, 48), new Vector2(12, 7), 0) },
            { "Augustus",        new Face_Sprite_Data(new Vector2(30, 23), new Vector2(30, 39), new Vector2(11, 9), 0) },
            { "Leonard",         new Face_Sprite_Data(new Vector2(27, 23), new Vector2(27, 39), new Vector2( 9, 4), 1) },
            { "Niime",           new Face_Sprite_Data(new Vector2(23, 35), new Vector2(23, 51), new Vector2( 7,10), 1) },
            { "Bennet",          new Face_Sprite_Data(new Vector2(33, 20), new Vector2(33, 36), new Vector2( 6, 0), 0) },
            { "Wallace",         new Face_Sprite_Data(new Vector2(19, 20), new Vector2(19, 36), new Vector2(10, 0), 1) },
            { "Abelia",          new Face_Sprite_Data(new Vector2(22, 23), new Vector2(22, 39), new Vector2(10, 0), 0) },
			{ "Chester",         new Face_Sprite_Data(new Vector2(30, 21), new Vector2(30, 37), new Vector2(10, 2), 0) },
            { "Roeis",           new Face_Sprite_Data(new Vector2(28, 23), new Vector2(28, 39), new Vector2(14, 0), 1) },
            { "Hyde",            new Face_Sprite_Data(new Vector2(29, 32), new Vector2(29, 48), new Vector2( 8, 3), 0) },
            
            { "Tristan",         new Face_Sprite_Data(new Vector2(48, 16), new Vector2(48, 32), new Vector2(22, 0), 1, 2) },
            { "Fargus",          new Face_Sprite_Data(new Vector2(36, 25), new Vector2(36, 41), new Vector2(17, 0), 0) },
            
            { "Eliza",           new Face_Sprite_Data(new Vector2(28, 21), new Vector2(28, 37), new Vector2(12, 0), 0) },
			{ "Brendan",         new Face_Sprite_Data(new Vector2(28, 21), new Vector2(28, 37), new Vector2(14, 2), 0) },
			{ "Melanie",         new Face_Sprite_Data(new Vector2(29, 28), new Vector2(29, 44), new Vector2( 7, 4), 0) },
			{ "Zephyr",          new Face_Sprite_Data(new Vector2(27, 23), new Vector2(27, 39), new Vector2(11, 2), 1) },
			{ "Vaida",           new Face_Sprite_Data(new Vector2(55, 19), new Vector2(55, 35), new Vector2(19, 0), 0, 10) },
			{ "Elle",            new Face_Sprite_Data(new Vector2(31, 20), new Vector2(31, 36), new Vector2(11, 1), 0) },
			{ "William",         new Face_Sprite_Data(new Vector2(28, 24), new Vector2(28, 40), new Vector2(10, 2), 1) },

            { "Lionel",          new Face_Sprite_Data(new Vector2(26, 25), new Vector2(26, 41), new Vector2( 6, 2), 0) },
            
			{ "Elbert",          new Face_Sprite_Data(new Vector2(29, 20), new Vector2(29, 36), new Vector2(12, 0), 0) },
			{ "Belmont",         new Face_Sprite_Data(new Vector2(25, 23), new Vector2(25, 39), new Vector2(10, 2), 0) },
			{ "Solomon",         new Face_Sprite_Data(new Vector2(26, 21), new Vector2(26, 37), new Vector2(12, 1), 0) },
			{ "Solomon_Hooded",  new Face_Sprite_Data(new Vector2(26, 21), new Vector2(26, 37), new Vector2(12, 1), 0) },
            
			{ "Isaac",           new Face_Sprite_Data(new Vector2(27, 19), new Vector2(27, 35), new Vector2(10, 0), 0) },
			{ "Meredith",        new Face_Sprite_Data(new Vector2(25, 21), new Vector2(25, 37), new Vector2(12, 0), 1) },
			{ "Deacon",          new Face_Sprite_Data(new Vector2(30, 20), new Vector2(30, 36), new Vector2(12, 2), 0) },

            { "Mazda",           new Face_Sprite_Data(new Vector2(29, 22), new Vector2(29, 38), new Vector2( 7, 0), 0) },
			{ "Darin",           new Face_Sprite_Data(new Vector2(24, 20), new Vector2(24, 36), new Vector2( 4, 1), 0) },
			{ "Edeleisse",       new Face_Sprite_Data(new Vector2(51, 22), new Vector2(51, 38), new Vector2(15, 0), 1, 15) },
			{ "Horace",          new Face_Sprite_Data(new Vector2(29, 28), new Vector2(29, 44), new Vector2( 8, 4), 1) },
			{ "Uriel",           new Face_Sprite_Data(new Vector2(39, 23), new Vector2(39, 39), new Vector2(10, 0), 0) },
			{ "Milo",            new Face_Sprite_Data(new Vector2(43, 20), new Vector2(43, 36), new Vector2(16, 1), 0, 10) },
			{ "Richard",         new Face_Sprite_Data(new Vector2(28, 22), new Vector2(28, 38), new Vector2(11, 0), 0) },
            { "Hubart",          new Face_Sprite_Data(new Vector2(26, 31), new Vector2(26, 47), new Vector2( 9, 7), 0) },
			{ "Crucius",         new Face_Sprite_Data(new Vector2(28, 28), new Vector2(28, 44), new Vector2( 8, 1), 0) },
            { "Ephidel_Hooded",  new Face_Sprite_Data(new Vector2(29, 24), new Vector2(21, 40), new Vector2(13, 0), 1) },

			{ "Ulric",           new Face_Sprite_Data(new Vector2(29, 29), new Vector2(29, 45), new Vector2( 8, 4), 0) },
            { "Desmond",         new Face_Sprite_Data(new Vector2(27, 20), new Vector2(27, 36), new Vector2(10, 0), 0) },
			{ "Kalten",          new Face_Sprite_Data(new Vector2(46, 24), new Vector2(46, 40), new Vector2(26, 3), 0) },
			{ "Bart",            new Face_Sprite_Data(new Vector2(35, 19), new Vector2(35, 35), new Vector2(15, 0), 0) },
			{ "Orun",            new Face_Sprite_Data(new Vector2(39, 26), new Vector2(39, 42), new Vector2( 0, 0), 0, 3) },
			{ "Pellenore",       new Face_Sprite_Data(new Vector2(28, 27), new Vector2(28, 43), new Vector2( 0, 0), 0) },

            { "Anoleis",         new Face_Sprite_Data(new Vector2(25, 18), new Vector2(25, 34), new Vector2( 9, 3), 0) },
            { "Sarathi",         new Face_Sprite_Data(new Vector2(28, 22), new Vector2(28, 38), new Vector2( 7, 0), 0) },
            { "Beck",            new Face_Sprite_Data(new Vector2(30, 21), new Vector2(30, 37), new Vector2(11, 0), 0) },
            { "Boston",          new Face_Sprite_Data(new Vector2(27, 17), new Vector2(27, 33), new Vector2(11, 0), 0) },
			{ "Crane",           new Face_Sprite_Data(new Vector2(44, 24), new Vector2(44, 40), new Vector2( 8, 4), 0) },
            
            { "MyrmidonM",       new Face_Sprite_Data(new Vector2(30, 35), new Vector2(30, 51), new Vector2(10,11), 0) },
            { "ThiefM",          new Face_Sprite_Data(new Vector2(41, 17), new Vector2(41, 33), new Vector2(14, 0), 0) },
            { "ArcherM",         new Face_Sprite_Data(new Vector2(30, 46), new Vector2(30, 62), new Vector2(10,21), 0) },
            { "ArcherF",         new Face_Sprite_Data(new Vector2(25, 36), new Vector2(25, 52), new Vector2( 7,15), 0) },
            { "SoldierM",        new Face_Sprite_Data(new Vector2(33, 21), new Vector2(33, 37), new Vector2(12, 0), 0) },
            { "FighterM",        new Face_Sprite_Data(new Vector2(28, 27), new Vector2(28, 43), new Vector2( 7, 0), 0) },
            { "BrigandM",        new Face_Sprite_Data(new Vector2(31, 19), new Vector2(31, 35), new Vector2(13, 0), 0) },
            { "PirateM",         new Face_Sprite_Data(new Vector2(25, 28), new Vector2(25, 44), new Vector2(18, 3), 0) },
            { "MercenaryM",      new Face_Sprite_Data(new Vector2(25, 29), new Vector2(25, 45), new Vector2(10, 6), 0) },
            { "VanguardM",       new Face_Sprite_Data(new Vector2(38, 22), new Vector2(38, 38), new Vector2(14, 1), 0) },
            { "VanguardF",       new Face_Sprite_Data(new Vector2(37, 20), new Vector2(37, 36), new Vector2(14, 0), 0) },
            { "GendarmeM",       new Face_Sprite_Data(new Vector2(38, 22), new Vector2(38, 38), new Vector2(14, 1), 0) },
            { "CohortM",         new Face_Sprite_Data(new Vector2(27, 27), new Vector2(27, 43), new Vector2(12, 5), 0) },
            { "LieutenantM",     new Face_Sprite_Data(new Vector2(46, 20), new Vector2(46, 36), new Vector2(26, 9), 0) },
            { "Pegasus KnightF", new Face_Sprite_Data(new Vector2(29, 20), new Vector2(29, 36), new Vector2( 8, 0), 0) },
            { "MonkM",           new Face_Sprite_Data(new Vector2(39, 31), new Vector2(39, 47), new Vector2(14,11), 0) },
            { "MageM",           new Face_Sprite_Data(new Vector2(38, 28), new Vector2(38, 44), new Vector2(15, 6), 0) },
            { "Mage KnightF",    new Face_Sprite_Data(new Vector2(40, 24), new Vector2(40, 40), new Vector2(23, 1), 0) },
            { "TroubadourF",     new Face_Sprite_Data(new Vector2(40, 24), new Vector2(40, 40), new Vector2(23, 1), 0) },
			{ "SorcererM",       new Face_Sprite_Data(new Vector2(37, 30), new Vector2(37, 46), new Vector2(14, 7), 0) },
			{ "DivinerM",        new Face_Sprite_Data(new Vector2(30, 26), new Vector2(30, 42), new Vector2(11, 1), 0) },
            
            { "Man1Villager",    new Face_Sprite_Data(new Vector2(30, 25), new Vector2(30, 41), new Vector2( 8, 0), 0) },
            { "Man4Villager",    new Face_Sprite_Data(new Vector2(26, 20), new Vector2(26, 36), new Vector2( 8, 0), 0) },
            { "Man6Villager",    new Face_Sprite_Data(new Vector2(22, 28), new Vector2(22, 44), new Vector2( 8, 0), 0) },
            { "Woman1Villager",  new Face_Sprite_Data(new Vector2(29, 32), new Vector2(29, 48), new Vector2(13, 8), 0) },
            { "Woman2Villager",  new Face_Sprite_Data(new Vector2(29, 21), new Vector2(29, 37), new Vector2( 8, 0), 0) },
            { "Woman4Villager",  new Face_Sprite_Data(new Vector2(27, 21), new Vector2(27, 37), new Vector2( 8, 0), 0) },

            { "Armory",          new Face_Sprite_Data(new Vector2(38,  5), new Vector2(38, 21), new Vector2( 0, 0), 0) },
            { "Vendor",          new Face_Sprite_Data(new Vector2(39,  9), new Vector2(39, 25), new Vector2( 0, 0), 0) },
            { "Secret",          new Face_Sprite_Data(new Vector2(37, 12), new Vector2(37, 28), new Vector2( 0, 0), 0) },
            { "Arena",           new Face_Sprite_Data(new Vector2(38,  0), new Vector2(38, 16), new Vector2( 0, 0), 0) },
            { "Generic",         new Face_Sprite_Data(new Vector2(23, 35), new Vector2(23, 51), new Vector2( 7,10), 1) },
        };
        #endregion
        
        #region Default Pitches
        public readonly static Dictionary<string, int> DEFAULT_PITCHES = new Dictionary<string, int> {
            { "Uther",           100 },
            { "Uther_Wounded",   100 },
            { "Marcus",          100 },
			{ "Harken",          115 },
			{ "Isadora",         152 },
			{ "Eagler",           85 },
			{ "Madelyn",         150 },
            { "Toni",            130 },
            { "Hassar",          105 },
            { "Cybil",           144 },
            { "Magnus",           95 },
            { "Eiry",            148 },
            { "Augustus",        101 },
            { "Lenny",           120 },
            { "Leonard",         120 },
            { "Niime",           110 },
            { "Bennet",          108 },
            { "Wallace",          83 },
			{ "Abelia",          130 },
			{ "Chester",          92 },
            { "Roeis",            80 },
            { "Hyde",            110 },
            { "Fargus",           90 },
            
            { "Tristan",          92 },
            { "Celeste",         120 },
            { "Shen",             90 },
            { "Gado",             79 },
            { "Reno",            116 },
            { "Jericho",          81 },
            { "Elaice",          133 },
            { "Kirin",           119 },
            
            { "Eliza",           138 },
			{ "Brendan",          74 },
			{ "Melanie",         126 },
			{ "Zephyr",          126 },
			{ "Vaida",           128 },
			{ "Elle",            129 },
			{ "William",         101 },

            { "Lionel",           98 },
            
			{ "Elbert",          109 },
			{ "Belmont",          92 },
			{ "Solomon",          70 },
            
			{ "Isaac",            78 },
			{ "Meredith",        112 },
			{ "Deacon",           98 },

            { "Mazda",            85 },
			{ "Darin",            95 },
			{ "Edeleisse",       111 },
			{ "Horace",           89 },
			{ "Uriel",           108 },
			{ "Milo",             98 },
			{ "Richard",         112 },
            { "Hubart",           84 },
			{ "Crucius",          84 },
			{ "Ephidel",          91 },

			{ "Valeria",         138 },
            
            { "Tory",            100 },
            { "Caphel",           79 },
            { "Gavrilo",         100 },
            { "Haster",          110 },

			{ "Connor",           89 },

			{ "Ulric",            84 },
            { "Desmond",         114 },
            { "Royal Guard",      90 },
			{ "Kalten",           90 },
            { "Elric",           100 },
            { "Thor",             90 },
			{ "Bart",             78 },
            { "Sally",           108 },
            { "Cuthbert",        100 },
			{ "Orun",            123 },
			{ "Pellenore",       102 },

            { "Anoleis",         108 },
            { "Sarathi",          89 },
            { "Beck",             96 },
			{ "Alec",            122 },
            { "Boston",           90 },
			{ "Stephen",          90 },
			{ "Crane",           108 },
            
            { "Manhunter",       108 },
            { "Bandit",           98 },
            { "Thief",           100 },
            { "Archer",          110 },
            { "Sorcerer",         90 },
            { "Lieutenant",       90 },
            { "Crossbowman",      95 },
            { "Scout",           120 },
            { "Pegasus Knight",  120 },
            { "Myrmidon",        100 },
            { "Mercenary",       100 },
            { "Berserker",        98 },
            { "General",          90 },
            
            { "Villager",        100 }, //Debug
            { "Page",            100 }, //Debug
            { "Messenger",       100 }, //Debug
            { "Valet",           100 }, //Debug
            { "Lycians",         100 },
            
            { "MyrmidonM",       100 },
            { "ThiefM",          100 },
            { "ArcherM",         100 },
            { "ArcherF",         130 },
            { "SoldierM",        100 },
            { "FighterM",        100 },
            { "BrigandM",        100 },
            { "PirateM",         100 },
            { "MercenaryM",      100 },
            { "VanguardM",       100 },
            { "VanguardF",       130 },
            { "GendarmeM",       100 },
            { "CohortM",         100 },
            { "LieutenantM",     100 },
            { "Pegasus KnightF", 130 },
            { "MonkM",           100 },
            { "Mage KnightF",    130 },
            { "TroubadourF",     130 },
			{ "DivinerM",        100 },
            
            { "Man1Villager",    100 },
            { "Man4Villager",    100 },
            { "Man6Villager",    100 },
            { "Woman1Villager",  100 },
            { "Woman2Villager",  100 },
            { "Woman4Villager",  100 },

            { "Armory",          100 },
            { "Vendor",          100 },
            { "Secret",          100 },
            { "Arena",           100 },
            { "Generic",         100 },
        };
        #endregion

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

        public readonly static Face_Sprite_Data FACE_DATA_DEFAULT = new Face_Sprite_Data();
        protected Vector2 Eyes_Offset = new Vector2(24, 24),
            Mouth_Offset = new Vector2(24, 40),
            Status_Offset = new Vector2(8, 0);
        protected int Status_Frame = 0, Placement_Offset = 0;

        #region Accessors
        public Vector2 eyes_offset { get { return Eyes_Offset; } }
        public Vector2 mouth_offset { get { return Mouth_Offset; } }
        public Vector2 status_offset { get { return Status_Offset; } }
        public int status_frame { get { return Status_Frame; } }
        public int placement_offset { get { return Placement_Offset; } }
        #endregion

        public Face_Sprite_Data() { }
        public Face_Sprite_Data(Vector2 eyes_offset, Vector2 mouth_offset, Vector2 status_offset, int status_frame) :
            this(eyes_offset, mouth_offset, status_offset, status_frame, 0) { }
        public Face_Sprite_Data(Vector2 eyes_offset, Vector2 mouth_offset, Vector2 status_offset, int status_frame, int placement_offset)
        {
            Eyes_Offset = eyes_offset;
            Mouth_Offset = mouth_offset;
            Status_Offset = status_offset;
            Status_Frame = status_frame;
            Placement_Offset = placement_offset;
        }

        public Face_Data to_face_data(string name)
        {
            Face_Data result = new Face_Data();

            result.Name = name;
            result.Emotions = EMOTION_LIST.ContainsKey(name) ? EMOTION_LIST[name] : DEFAULT_EMOTIONS;
            result.Pitch = DEFAULT_PITCHES.ContainsKey(name) ? DEFAULT_PITCHES[name] : 100;
            result.EyesOffset = Eyes_Offset;
            result.MouthOffset = Mouth_Offset;
            result.StatusOffset = Status_Offset;
            result.StatusFrame = Status_Frame;
            result.PlacementOffset = Placement_Offset;
            result.ForceEyesClosed = false;
            result.Asymmetrical = false;
            result.ClassCard = false;

            return result;
        }
    }
}
