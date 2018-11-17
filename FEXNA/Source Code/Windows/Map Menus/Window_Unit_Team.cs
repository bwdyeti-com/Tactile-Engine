#if !MONOGAME && WINDOWS
using System.Collections.Generic;

namespace FEXNA.Windows.Map
{
    class Window_Unit_Team : Window_Unit
    {
        internal static int TEAM = 1;

        protected override List<int> determine_team()
        {
            List<int> team = new List<int>();
            team.AddRange(Global.game_map.teams[TEAM]);
            return team;
        }
    }
}
#endif