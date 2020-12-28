using System.Collections.Generic;
using System.Linq;

namespace Tactile.Calculations.LevelUp.Stats
{
    class LevelUpStatSet
    {
        private List<LevelUpStats> Levels = new List<LevelUpStats>();

        public int LevelCount { get { return Levels.Count; } }

        public void AddLevel(LevelUpStats level)
        {
            Levels.Add(level);
        }

        public LevelUpStats GetLevel(int index)
        {
            return Levels[index];
        }
        
        public int StatGains(Stat_Labels stat)
        {
            return StatGains((int)stat);
        }
        private int StatGains(int i)
        {
            return Levels.Sum(x => x.StatGains[i]);
        }
    }
}
