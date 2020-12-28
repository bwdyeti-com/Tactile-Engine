using System.Collections.Generic;

namespace Tactile.Constants
{
    public class Difficulty
    {
        public readonly static Dictionary<Difficulty_Modes, int> DIFFICULTY_COLOR_REDIRECT = new Dictionary<Difficulty_Modes, int>
        {
            { Difficulty_Modes.Normal, Constants.Team.PLAYER_TEAM - 1 },
            { Difficulty_Modes.Hard, Constants.Team.ENEMY_TEAM - 1 },
            //{ Difficulty_Modes.Lunatic, Constants.Team.INTRUDER_TEAM - 1 },
            //{ Difficulty_Modes.Lunatic_Plus, Constants.Team.CITIZEN_TEAM - 1 }
        };
        public readonly static Dictionary<Mode_Styles, int> STYLE_COLOR_REDIRECT = new Dictionary<Mode_Styles, int>
        {
            { Mode_Styles.Casual, Constants.Team.CITIZEN_TEAM - 1 },
            { Mode_Styles.Standard, Constants.Team.PLAYER_TEAM - 1 },
            { Mode_Styles.Classic, Constants.Team.ENEMY_TEAM - 1 },
        };
        // Saves append these after the chapter id to mark the difficulty; Normal has no entry
        public readonly static Dictionary<Difficulty_Modes, char> DIFFICULTY_SAVE_APPEND = new Dictionary<Difficulty_Modes, char>
        {
            { Difficulty_Modes.Hard, 'H' },
            //{ Difficulty_Modes.Lunatic, 'L' },
            //{ Difficulty_Modes.Lunatic_Plus, 'M' }
        };
        // Saves append these after the chapter id to mark the difficulty; Normal has no entry
        public readonly static Dictionary<Difficulty_Modes, char> DIFFICULTY_EVENT_APPEND = new Dictionary<Difficulty_Modes, char>
        {
            { Difficulty_Modes.Normal, 'N' },
            { Difficulty_Modes.Hard, 'H' },
            //{ Difficulty_Modes.Lunatic, 'L' },
            //{ Difficulty_Modes.Lunatic_Plus, 'M' }
        };
    }
}
