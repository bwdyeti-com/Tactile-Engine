using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tactile.Windows.Map
{
    class Window_Unit_Actor : Window_Unit
    {
        List<Game_Unit> Units = new List<Game_Unit>();

        #region Accessor Overrides
        protected override Game_Unit _unit(int id)
        {
            return Units[id];
        }
        #endregion

        public Window_Unit_Actor()
        {
        }
        //public Window_Unit_Actor(int battalion)
        //{
        //    Battalion = battalion;
        //}

        protected override List<int> determine_team()
        {
            List<int> team = new List<int>();
            foreach (int id in Global.battalion.actors)
            {
                team.Add(Units.Count);
                Units.Add(new Game_Unit(Units.Count, id));
            }
            return team;
        }
    }
}
