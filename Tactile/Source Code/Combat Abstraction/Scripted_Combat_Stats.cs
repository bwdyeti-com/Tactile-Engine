using System.Collections.Generic;

namespace Tactile
{
    enum Attack_Results { Hit, Crit, Miss, End }

    class Scripted_Combat_Script
    {
        private int Kill = 0;
        private bool Scene_Battle;
        public List<Scripted_Combat_Stats> Stats { get; private set; }

        public int kill
        {
            get { return Kill; }
            set { Kill = value; }
        }
        public bool scene_battle
        {
            get { return Scene_Battle; }
            set { Scene_Battle = value; }
        }

        public Scripted_Combat_Script()
        {
            this.Stats = new List<Scripted_Combat_Stats>();
        }

        public void Add(Scripted_Combat_Stats stats)
        {
            Stats.Add(stats);
        }
    }

    class Scripted_Combat_Stats
    {
        public int Attacker;
        public List<int> Stats_1;
        public List<int> Stats_2;
        public Attack_Results Result;
        public int Damage;
    }
}
