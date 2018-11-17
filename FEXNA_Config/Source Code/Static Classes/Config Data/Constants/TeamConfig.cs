using System.Collections.Generic;
using System.Linq;

namespace FEXNA.Constants
{
    public class Team
    {
        public const int NUM_TEAMS = 4;
        public const int PLAYER_TEAM = 1;
        public const int ENEMY_TEAM = 2;
        public const int CITIZEN_TEAM = 3;
        public const int INTRUDER_TEAM = 4;
        public readonly static int[] PLAYABLE_TEAMS = new int[] { PLAYER_TEAM }; // Teams that are human controlled
        public readonly static int[][] TEAM_GROUPS = { new int[] { 1, 3 }, new int[] { 2 }, new int[] { 4 } };

        public readonly static Dictionary<int, string> TEAM_NAMES = new Dictionary<int, string>
        {
            { PLAYER_TEAM, "Player" },
            { ENEMY_TEAM, "Enemy" },
            { CITIZEN_TEAM, "Citizen" },
            { INTRUDER_TEAM, "Intruder" },
        };

        public static string team_name(int team_id)
        {
            if (TEAM_NAMES.ContainsKey(team_id))
                return TEAM_NAMES[team_id];
            return string.Format("Team {0}", team_id);
        }

        public const bool FLIP_ENEMY_MAP_SPRITES = true;
        public static bool flipped_map_sprite(int team)
        {
            if (!FLIP_ENEMY_MAP_SPRITES)
                return false;
            return team % 2 == 0;
        }

        /// <summary>
        /// Checks if a team is an enemy of the player team.
        /// </summary>
        public static bool opponent_team(int team)
        {
            var allied_groups = TEAM_GROUPS.Where(x => x.Contains(PLAYER_TEAM));
            return !allied_groups.Any(x => x.Contains(team));
        }

        // Get a collection of the given team and all its allied teams.
        public static IEnumerable<int> allied_teams(int team)
        {
            var allied_groups = TEAM_GROUPS.Where(x => x.Contains(team));
            return allied_groups
                .SelectMany(x => x)
                .Distinct();
        }
    }
}
