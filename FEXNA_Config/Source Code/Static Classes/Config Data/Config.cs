using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FEXNA_Library;

namespace FEXNA
{
    public enum Mode_Styles { Casual, Standard, Classic }
    public enum Difficulty_Modes { Normal, Hard }// , Lunatic } // Lunatic_Plus? //Debug
    public enum Stat_Labels { Hp, Pow, Skl, Spd, Lck, Def, Res, Con, Mov }
    public enum Combat_Stat_Labels { Dmg, Def, Hit, Avo, Crt, Dod }
    public enum Convoy_Stack_Types { None, Use, Full }
    public enum Gladiators
    {
        Myrm, Thif, ArcM, ArcF, Crsb, Sold, Drac, Figh, Brig, Pira, Merc, Noma, Coht, Vngd, Gdrm, Phal, Lieu, Zwei, Pega,
        Monk, Mag1, Mag2, Sorc, Sham, Divi, Schl,
        Swor, Rogu, Long, Halb, Dgm1, Dgm2, Zerk, War1, War2, Hro1, Hro2, Hro3, Nmdt, Pal1, Pal2, Ctt1, Ctt2, Gene, Flkn,
        Bshp, Sag1, Sag2, Mgkt, Valk, WrcM, WrcF, Drud, Just
    }
    public class Config
    {
        public const int FRAME_RATE = 60;
        public const int WINDOW_WIDTH = 20 * 16;
        public const int WINDOW_HEIGHT = 12 * 16;
        //public const int WINDOW_WIDTH = 24 * 16;
        //public const int WINDOW_HEIGHT = 24 * 16;
         
        public const string SAVE_FILE_EXTENSION = ".sav";
        public const string MAP_SAVE_FILENAME = "map_save";
        public const int SAVES_PER_PAGE = 5;
        public const int SAVE_PAGES = 20;
        public const bool METRICS_ENABLED = true; // false; //FEGame
        public const bool UPDATE_CHECK_ENABLED = true; // false; //FEGame

        public const bool PREP_MAP_BACKGROUND = true; // Does the preparations screen use the map as a background
        public readonly static Color MOUSE_PRESSED_ELEMENT_COLOR = new Color(176, 176, 176, 255); // Tint to use for UI elements when the mouse is pressed on them
        public readonly static Color MOUSE_OVER_ELEMENT_COLOR = new Color(224, 224, 224, 255);

        #region Stereoscopic 3D
        // Values that are not integers will be blurry to some degree
        // I think, it might be multiples of 2?
        #region Title
        public const float TITLE_BG_DEPTH = 8;
        public const float TITLE_SWORD_DEPTH = 3;
        public const float TITLE_LOGO_DEPTH = 2;
        public const float TITLE_CHOICE_DEPTH = 0;
        public const float TITLE_MENU_DEPTH = 1;
        public const float TITLE_OPTIONS_DEPTH = -1;
#if DEBUG
        public const float TESTBATTLE_DATA_DEPTH = -1;
#endif

        #region Class Reel
        public const float REEL_NAME_PLAQUE_DEPTH = 0;
        public const float REEL_NAME_LETTERS_DEPTH = -1;
        public const float REEL_BURST_DEPTH = 2;

        public const float REEL_BG_DEPTH = 8;
        public const float REEL_TEXT_BOX_DEPTH = 0;
        public const float REEL_CLASS_NAME_DEPTH = 2;
        public const float REEL_CLASS_NAME_SHADOW_DEPTH = 4;
        public const float REEL_STATS_DEPTH = 0;
        public const float REEL_WEAPON_ICONS_DEPTH = 1;
        #endregion
        #endregion

        #region Battle Scene
        public const float BATTLE_BG_DEPTH = 8;
        public const float BATTLE_PLATFORM_BASE_DEPTH = 2;
        public const float BATTLE_PLATFORM_TOP_DEPTH_OFFSET = 6;
        public const float BATTLE_BATTLERS_DEPTH = 4;
        public const float BATTLE_HUD_DEPTH = 0;
        public const float BATTLE_HITSPARK_DEPTH = 2;
        public const float BATTLE_CRITSPARK_DEPTH = 0;
        public const float BATTLE_MISSNODAMAGE_DEPTH = 2;
        public const float BATTLE_EXP_DEPTH = -2;
        public const float BATTLE_LEVEL_UP_DEPTH = -2;
        #endregion

        #region Map Scene
        public const float MAP_MAP_DEPTH = 4;
        public const float MAP_UNITS_DEPTH = 4;
        public const float MAP_STATUS_ICON_DEPTH = 3.5f; //3 //Debug
        public const float MAP_HPGAUGE_DEPTH = 3.75f; //3.5f
        public const float MAP_CURSOR_DEPTH = 3.5f; //3
        public const float MAP_MOVE_RANGE_DEPTH = 4;
        public const float MAP_BALLISTA_HIGHEST_DEPTH = 1;
        public const float MAP_INFO_DEPTH = 0;
        public const float MAP_INFO_BURST_DEPTH = 2;
        public const float MAP_MENU_DEPTH = 0; //Yeti
        public const float MAP_POPUP_DEPTH = 0;
        public const float MAP_WEATHER_DEPTH = 1;
        public const float MAP_WEATHER_OFF_FRAME_DEPTH_OFFSET = 0.5f;

        #region Victory Screen
        public const float RANKING_BURST_DEPTH = 2;
        public const float RANKING_BANNER_DEPTH = 0;
        public const float RANKING_WINDOW_DEPTH = 1;
        public const float RANKING_ICON_DEPTH = 1;
        public const float RANKING_ICON_MAX_DEPTH = -8;
        #endregion
        #endregion

        #region Convo
        public const float CONVO_BG_DEPTH = 4;
        public const float CONVO_FACE_DEPTH = 1;
        public const float CONVO_TEXT_DEPTH = 0;
        public const float CONVO_LOCATION_DEPTH = 0;
        public const float CONVO_BACKLOG_DEPTH = -1;
        #endregion

        #region World Map
        public const float WMAP_ZOOMED_DEPTH = 2;
        public const float WMAP_MINIMAP_DEPTH = 0;

        #region Chapter Transition
        public const float CH_TRANS_BG_DEPTH = 4;
        public const float CH_TRANS_SIGIL_DEPTH = 2;
        public const float CH_TRANS_BANNER_DEPTH = 0;
        #endregion
        #endregion

        #region Map Menus
        public const float MAPMENU_BG_DEPTH = 4;

        public const float MAPCOMMAND_WINDOW_DEPTH = 2; //0; // Set to a higher number for now for testing //Yeti
        public const float MAPCOMMAND_DATA_DEPTH = -4; //0; // Set to a lower number for now for testing //Yeti
        public const float MAPCOMMAND_HELP_DEPTH = -1;

        #region Status
        public const float STATUS_TOP_PANEL_DEPTH = 0;
        public const float STATUS_FACE_DEPTH = 1;
        public const float STATUS_FACE_BG_DEPTH = 2;
        public const float STATUS_LEFT_WINDOW_DEPTH = 1;
        public const float STATUS_RIGHT_WINDOW_DEPTH = 1;
        public const float STATUS_ARROW_DEPTH = 0;
        public const float STATUS_HELP_DEPTH = -1;
        #endregion

        #region Unit Menu
        public const float UNIT_BANNER_DEPTH = -1;//0 //Debug
        public const float UNIT_WINDOW_DEPTH = 0;//1
        public const float UNIT_ARROWS_DEPTH = -1;//0
        public const float UNIT_SORT_DEPTH = -1;//0
        public const float UNIT_HELP_DEPTH = -1;
        #endregion

        #region Options
        public const float OPTIONS_BANNER_DEPTH = 0;
        public const float OPTIONS_OPTIONS_DEPTH = 2;
        public const float OPTIONS_ARROWS_DEPTH = 1;
        public const float OPTIONS_DESC_DEPTH = 1;
        public const float OPTIONS_CURSOR_DEPTH = 0;
        #endregion

        #region Data Menu
        public const float DATA_BANNER_DEPTH = 0;
        public const float DATA_TEAMS_DEPTH = 2;
        public const float DATA_WINDOW_DEPTH = 1;
        public const float DATA_LEADER_DEPTH = 3;
        public const float DATA_DATA_DEPTH = 2;
        #endregion
        #endregion

        #region Preparations
        public const float PREP_BG_DEPTH = 4;

        // Preparations
        public const float PREPMAIN_BANNER_DEPTH = 0;
        public const float PREPMAIN_WINDOW_DEPTH = 0;
        public const float PREPMAIN_INFO_DEPTH = 1;
        public const float PREPMAIN_DATA_DEPTH = 0;
        public const float PREPMAIN_TALK_DEPTH = -1;

        // Units
        public const float PREPUNIT_WINDOW_DEPTH = 0;
        public const float PREPUNIT_UNIT_INFO_DEPTH = 1;
        public const float PREPUNIT_INPUTHELP_DEPTH = 2;

        // Items
        public const float PREPITEM_BATTALION_DEPTH = 0;
        public const float PREPITEM_BATTALION_DIMMED_DEPTH = 2;
        public const float PREPITEM_UNIT_DEPTH = 1;
        public const float PREPITEM_UNIT_DIMMED_DEPTH = 3;
        public const float PREPITEM_WINDOW_DEPTH = 0;
        public const float PREPITEM_FUNDS_DEPTH = 0;

        // Trade
        public const float PREPTRADE_NAMES_DEPTH = 0;
        public const float PREPTRADE_WINDOWS_DEPTH = 0;
        public const float PREPTRADE_FACES_DEPTH = 1;
        public const float PREPTRADE_HELP_DEPTH = -1;

        // Convoy
        public const float CONVOY_BANNER_DEPTH = 2;
        public const float CONVOY_ICON_DEPTH = 0;
        public const float CONVOY_STOCK_DEPTH = 0;
        public const float CONVOY_WINDOW_DEPTH = 0;
        public const float CONVOY_INVENTORY_DEPTH = 2;
        public const float CONVOY_SUPPLY_DEPTH = 2;
        public const float CONVOY_SELECTION_DEPTH = 0;
        public const float CONVOY_ARROWS_DEPTH = 1;
        public const float CONVOY_INPUTHELP_DEPTH = 0;
        public const float CONVOY_HELP_DEPTH = -1;
        #endregion
        #endregion

        #region orphaned stuff
        // Move to classes
        public const int PROMOTION_LVL = 10;
        public readonly static int[] LEVEL_UP_PROMOTION = new int[] { 0 }; // Tiers that are allowed to promote by level up

        // On promotion, only gain Anima wexp if the actor would gain all Anima types or doesn't have Anima at all yet?
        // Thus Mage Knight promotion could give Wind, so a
        //     Troub => Mage Knight would have Wind, but a Fire Mage => Mage Knight
        //     would only retain Fire instead
        public const bool SINGLE_ANIMA_PROMOTION = false;

        // Mounted classes, that use a fixed value - con for aid
        // In order based on which should be used first to determine base aid
        public readonly static ClassTypeSet[] MOUNTED_CLASS_TYPES = new ClassTypeSet[]
        {
            new ClassTypeSet(ClassTypes.FDragon),
            new ClassTypeSet(ClassTypes.Flier),
            new ClassTypeSet(ClassTypes.Cavalry, ClassTypes.Mage),
            new ClassTypeSet(ClassTypes.Cavalry),
        };
        public readonly static Dictionary<ClassTypeSet, int> MOUNTED_CLASS_AID = new Dictionary<ClassTypeSet, int>
        {
            { new ClassTypeSet(ClassTypes.FDragon), 25 },
            { new ClassTypeSet(ClassTypes.Flier), 20 },
            { new ClassTypeSet(ClassTypes.Cavalry, ClassTypes.Mage), 20 },
            { new ClassTypeSet(ClassTypes.Cavalry), 25 },
        };
        public readonly static ClassTypes[] CLASS_TYPE_ICONS = new ClassTypes[] { // Class types that have icons for effectiveness
            ClassTypes.Cavalry, ClassTypes.Armor, ClassTypes.Flier, ClassTypes.FDragon, ClassTypes.Swordsman, ClassTypes.Dragon, ClassTypes.Mage };

        public readonly static HashSet<ClassTypes> IGNORE_TERRAIN_AVO = new HashSet<ClassTypes> { ClassTypes.Cavalry, ClassTypes.Flier };
        public readonly static HashSet<ClassTypes> IGNORE_TERRAIN_DEF = new HashSet<ClassTypes> { ClassTypes.Flier };



        // This should really be pretty much anywhere else //Yeti
        public static int DEFAULT_VISION_RANGE = 3;
        public readonly static Dictionary<int, int> CLASS_VISION_BONUS = new Dictionary<int, int> {
            { 17, 5 }, // Thief
            { 52, 5 }, // Assassin
            { 53, 5 } // Rogue
        };
        public const int FLARE_VISION = 5; // Base vision range for Flare staff
        #endregion


        #region Animation
        public const int BATTLER_ANIMA_DONE_TIME = 5;
        public const int BATTLER_SPELL_DONE_TIME = 2;
        public readonly static int BATTLER_SIZE = 192;
        #endregion

        #region Arena
        public const int MIN_WAGER = 500;
        public const int WAGER_VARIANCE = 800;

        public const bool SUPPORTS_IN_ARENA = false;

        public readonly static Dictionary<Difficulty_Modes, int> ARENA_LVL_BONUS = new Dictionary<Difficulty_Modes, int>
        {
            { Difficulty_Modes.Normal, -1 },
            { Difficulty_Modes.Hard,    6 },
        };

        public const int MIN_ARENA_HIT = 40;
        public const int MAX_ARENA_DMG_ROUNDS = 8; // Max rounds worth of damage the player can take to kill

        public readonly static Dictionary<int, KeyValuePair<int[], int[]>> ARENA_WEAPON_TYPES =
                new Dictionary<int, KeyValuePair<int[], int[]>> {
            #region Weapons
            { 1, new KeyValuePair<int[], int[]>(new int[] { 1 },    new int[] {   3,   8,  16 }) },
            { 2, new KeyValuePair<int[], int[]>(new int[] { 1 },    new int[] {  33,  38,  46 }) },
            { 3, new KeyValuePair<int[], int[]>(new int[] { 1 },    new int[] {  59,  64,  71 }) },
            { 4, new KeyValuePair<int[], int[]>(new int[] { 2 },    new int[] {  83,  85,  90 }) },
            //{ 5, new KeyValuePair<int[], int[]>(new int[] { 1, 2 }, new int[] { 101, 102, 106 }) },
            //{ 6, new KeyValuePair<int[], int[]>(new int[] { 1, 2 }, new int[] { 111, 112, 116 }) },
            //{ 7, new KeyValuePair<int[], int[]>(new int[] { 1, 2 }, new int[] { 121, 122, 126 }) },
            //{ 8, new KeyValuePair<int[], int[]>(new int[] { 1, 2 }, new int[] { 131, 132, 136 }) },
            //{ 9, new KeyValuePair<int[], int[]>(new int[] { 1, 2 }, new int[] { 141, 142, 146 }) },
            // lol the spells aren't all done
            { 5, new KeyValuePair<int[], int[]>(new int[] { 1, 2 }, new int[] { 101, 102 }) },
            { 6, new KeyValuePair<int[], int[]>(new int[] { 1, 2 }, new int[] { 111, 112 }) },
            { 7, new KeyValuePair<int[], int[]>(new int[] { 1, 2 }, new int[] { 121, 122 }) },
            { 8, new KeyValuePair<int[], int[]>(new int[] { 1, 2 }, new int[] { 131, 132 }) },
            { 9, new KeyValuePair<int[], int[]>(new int[] { 1, 2 }, new int[] { 141, 142 }) }
            #endregion
        };

        // symbol => id, weapon type, gender, con
        public readonly static Dictionary<int, Dictionary<Gladiators, Gladiator_Data>> GLADIATORS = new Dictionary<int, Dictionary<Gladiators, Gladiator_Data>> {
            #region Tier 1
            { 1, new Dictionary<Gladiators, Gladiator_Data> {
                { Gladiators.Myrm, new Gladiator_Data(16, 1, 0) }, // Myrmidon
                { Gladiators.Thif, new Gladiator_Data(17, 1, 0) }, // Thief
                { Gladiators.ArcM, new Gladiator_Data(19, 4, 0) }, // ArcherM
                { Gladiators.ArcF, new Gladiator_Data(19, 4, 1) }, // ArcherF
                { Gladiators.Crsb, new Gladiator_Data(20, 4, 0) }, // Crossbowman
                { Gladiators.Sold, new Gladiator_Data(22, 2, 0) }, // Soldier
                { Gladiators.Drac, new Gladiator_Data(23, 2, 0) }, // Dracoknight
                { Gladiators.Figh, new Gladiator_Data(24, 3, 0) }, // Fighter
                { Gladiators.Brig, new Gladiator_Data(25, 3, 0) }, // Brigand
                { Gladiators.Pira, new Gladiator_Data(26, 3, 0) }, // Pirate
                { Gladiators.Merc, new Gladiator_Data(27, 1, 0) }, // Mercenary
                { Gladiators.Noma, new Gladiator_Data(29, 4, 0) }, // Nomad
                { Gladiators.Coht, new Gladiator_Data(30, 3, 0) }, // Cohort
                { Gladiators.Vngd, new Gladiator_Data(31, 2, 0) }, // Vanguard
                { Gladiators.Gdrm, new Gladiator_Data(32, 1, 0) }, // Gendarme
                { Gladiators.Phal, new Gladiator_Data(33, 3, 0) }, // Phalanx
                { Gladiators.Lieu, new Gladiator_Data(34, 2, 0) }, // Lieutenant
                { Gladiators.Zwei, new Gladiator_Data(35, 1, 0) }, // Zweihander
                { Gladiators.Pega, new Gladiator_Data(37, 2, 1) }, // Pegasus Knight
                { Gladiators.Monk, new Gladiator_Data(39, 8, 0) }, // Monk
                { Gladiators.Mag1, new Gladiator_Data(40, 5, 0) }, // Mage          (Fire)
                { Gladiators.Mag2, new Gladiator_Data(40, 7, 1) }, // Mage          (Wind)
                { Gladiators.Sorc, new Gladiator_Data(44, 9, 0) }, // Sorcerer
                { Gladiators.Sham, new Gladiator_Data(45, 6, 0) }, // Shaman        (Thunder)
                { Gladiators.Divi, new Gladiator_Data(47, 8, 0) }, // Diviner
                { Gladiators.Schl, new Gladiator_Data(48, 9, 0) }, // Scholar
            }},
            #endregion
            #region Tier 2
            { 2, new Dictionary<Gladiators, Gladiator_Data> {
                { Gladiators.Swor, new Gladiator_Data(51, 1, 0) }, // Swordmaster
                { Gladiators.Rogu, new Gladiator_Data(53, 1, 0) }, // Rogue
                { Gladiators.Long, new Gladiator_Data(55, 4, 1) }, // Longbowman
                { Gladiators.Halb, new Gladiator_Data(59, 2, 0) }, // Halberdier
                { Gladiators.Dgm1, new Gladiator_Data(61, 2, 0) }, // Dragon Master (Lance)
                { Gladiators.Dgm2, new Gladiator_Data(61, 3, 0) }, // Dragon Master (Axe)
                { Gladiators.Zerk, new Gladiator_Data(62, 3, 0) }, // Berserker
                { Gladiators.War1, new Gladiator_Data(63, 3, 0) }, // Warrior       (Axe)
                { Gladiators.War2, new Gladiator_Data(63, 4, 0) }, // Warrior       (Bow)
                { Gladiators.Hro1, new Gladiator_Data(65, 1, 0) }, // Hero          (Sword)
                { Gladiators.Hro2, new Gladiator_Data(65, 3, 0) }, // Hero          (Axe)
                //{ Gladiators.Hro3, new Gladiator_Data(65, 4, 0) }, // Hero          (Bow)
                { Gladiators.Nmdt, new Gladiator_Data(67, 4, 0) }, // Nomad Trooper
                { Gladiators.Pal1, new Gladiator_Data(70, 1, 0) }, // Paladin       (Sword)
                { Gladiators.Pal2, new Gladiator_Data(70, 2, 0) }, // Paladin       (Lance)
                { Gladiators.Ctt1, new Gladiator_Data(71, 1, 0) }, // Cataphract    (Sword)
                { Gladiators.Ctt2, new Gladiator_Data(71, 3, 0) }, // Cataphract    (Axe)
                { Gladiators.Gene, new Gladiator_Data(73, 2, 0) }, // General
                { Gladiators.Flkn, new Gladiator_Data(76, 2, 1) }, // Falcoknight
                { Gladiators.Bshp, new Gladiator_Data(78, 8, 0) }, // Bishop
                { Gladiators.Sag1, new Gladiator_Data(79, 5, 0) }, // Sage          (Fire)
                { Gladiators.Sag2, new Gladiator_Data(79, 6, 1) }, // Sage          (Thunder)
                { Gladiators.Mgkt, new Gladiator_Data(80, 7, 1) }, // Mage Knight   (Wind)
                { Gladiators.Valk, new Gladiator_Data(81, 8, 1) }, // Valkyrie
                { Gladiators.WrcM, new Gladiator_Data(84, 9, 0) }, // WarlockM
                { Gladiators.WrcF, new Gladiator_Data(84, 9, 1) }, // WarlockF
                { Gladiators.Drud, new Gladiator_Data(86, 6, 0) }, // Druid         (Thunder)
                { Gladiators.Just, new Gladiator_Data(88, 8, 0) }, // Justice
            }},
            #endregion
        };
        #endregion


        #region Conversation
        public const bool MOVE_SPEAKER_TO_FRONT = false; // If true, whenever a face sprite becomes the active speaker it moves in front of all others
        public const float FACE_TONE_PERCENT = 0.6f;

        public const int FACE_SPRITE_MOVEMENT_BOB_TIME = 8; // Time in frames a face sprite ducks down at the end of a movement

        public const int CONVO_BACKLOG_LINES = 256;
        public const int CONVO_BACKLOG_FADE_TIME = 16;
        public const int CONVO_BACKLOG_PAN_IN_TIME = 6;
        public const int CONVO_BACKLOG_PAN_OUT_TIME = 8;
        public const int CONVO_BACKLOG_BG_OPACITY = 160;
        public const int CONVO_BACKLOG_MAX_SCROLL_SPEED = 4;
        public const float CONVO_BACKLOG_TOUCH_SCROLL_FRICTION = 0.95f;
        #endregion

        #region Events
        public readonly static string[] EVENT_NAME_DELIMITER = new string[] { ", " };
        public const bool BASE_EVENT_ACTIVATED_INVISIBLE = false; // If true base events that have already been activated are unlisted, instead of greyed out
        public readonly static int EVENT_DATA_LENGTH = 100;
        public readonly static int EVENT_DATA_MONITOR_PAGE_SIZE = 20;
        #endregion

        #region Icons
        public readonly static int ITEM_ICON_SIZE = 16;
        public readonly static int SKILL_ICON_SIZE = 16;
        #endregion


        #region Player
        public const int CURSOR_TIME = 32;
        #endregion

        #region System
        //public const int RNG_VALUES = 1024;
        public const int CHARACTER_TIME = 72;
        public readonly static int[] CHARACTER_IDLE_ANIM_TIMES = new int[] { 32, 4, 32, 4 }; // Time for each frame of the map sprite idle anim
        public readonly static int[] CHARACTER_IDLE_ANIM_FRAMES = new int[] { 0, 1, 2, 1 }; // Map sprite idle frame order
        public readonly static int[] CHARACTER_HIGHLIGHT_ANIM_TIMES = new int[] { 20, 4, 20, 4 }; // Time for each frame of the map sprite idle anim
        public readonly static int[] CHARACTER_HIGHLIGHT_ANIM_FRAMES = new int[] { 0, 1, 2, 1 }; // Map sprite idle frame order
        public readonly static int[] CHARACTER_MOVING_ANIM_TIMES = new int[] { 13, 6, 13, 6 }; // Time for each frame of the map sprite idle anim
        public readonly static int[] CHARACTER_MOVING_ANIM_FRAMES = new int[] { 0, 1, 2, 3 }; // Map sprite idle frame order
        public const int RESCUE_TIME = 32; // Period of rescue/boss icon animation
        public const int RESCUE_VISIBLE_TIME = 20; // Rescue/boss icon visible time out of period
        public const int MOVE_RANGE_TIME = 64;
        //public const int STATUS_TIME = 96;
        #endregion

        #region Title
        public const int CLASS_REEL_WAIT_TIME = 15;
        public const int TITLE_GAME_START_TIME = 30;

        public readonly static string[] SPLASH_SCREENS = new string[] { "splashes1", "splashes2" };
        public const int SPLASH_TIME = 300;
        public const int SPLASH_FADE_TIME = 60;
        public const int SPLASH_INITIAL_BLACK_TIME = 40;
        #endregion

        #region Unit
        public const int FORMATION_CHANGE_STEPS = 16;

        public readonly static Vector2 OFF_MAP = new Vector2(-15, -15); // Location to use as the general purpose off the map location (for rescued units, gladiators, etc)

        public const float UNIT_TONE_PERCENT = 0.5f;

        public const int SIMPLE_HPGAUGE_MAX_HUE = 120; // Hue when simple hp gauge is full
        public const int ADVANCED_HPGAUGE_MAX_HUE = 270; // Hue for capped hp value

        public const int MAP_STATUS_EFFECT_TIME = 90; // Time in frames to display a status effect before cycling to the next

        public const int UNIT_BLINK_PERIOD = 60;
        public const int UNIT_BLINK_TIME = 3;

        public const bool DANGEROUS_UNIT_WARNING = true; // When a unit is selected, opponents with effective weapons will be marked out
        #endregion

        #region Weather
        public const int DEFAULT_WEATHER_SPRITE_COUNT = 32;
        public const int RAIN_SPRITE_COUNT = 32;
        public const int SNOW_SPRITE_COUNT = 24;
        #endregion

        public const bool SUSPEND_AFTER_AI_SELECTION = false; // Save to the suspend file each time the AI decides on what unit to move next
    }
}
