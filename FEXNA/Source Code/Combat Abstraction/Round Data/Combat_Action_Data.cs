using System.Collections.Generic;

namespace FEXNA
{
    enum Combat_Action_Triggers { Attack, Skill, Hit, Return, End }
    class Combat_Action_Data
    {
        public int Trigger;
        public int? Dmg1, Dmg2, Hit1, Skl1, Hit2, Crt1, Crt2, Skl2;
        public List<string> Skill_Ids = new List<string>();
        public int Battler_Index;

        public List<int?> combat_stats
        {
            set
            {
                Hit1 = value[0];
                Dmg1 = value[1];
                Crt1 = value[2];
                Skl1 = value[3];
                Hit2 = value[4];
                Dmg2 = value[5];
                Crt2 = value[6];
                Skl2 = value[7];
            }
        }
    }
}
