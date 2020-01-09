using System.Collections.Generic;

namespace FEXNA
{
    struct Attack_Result
    {
        public int dmg, actual_dmg;
        public bool hit;
        public bool crt;
        public int wexp;
        public bool backfire;
        public bool kill;
        public int immediate_life_steal;
        public bool delayed_life_steal;
        public List<KeyValuePair<int, bool>> state_change;

        public bool status_inflicted()
        {
            foreach (KeyValuePair<int, bool> pair in state_change)
                if (pair.Value)
                    return true;
            return false;
        }

        public int status_inflict_map_id()
        {
            foreach (KeyValuePair<int, bool> pair in state_change)
                if (pair.Value && Global.data_statuses[pair.Key].Map_Anim_Id > 0)
                    return Global.data_statuses[pair.Key].Map_Anim_Id;
            return 0;
        }

        public int delayed_life_steal_amount
        {
            get
            {
                if (!delayed_life_steal)
                    return 0;
                return (int)(dmg * Constants.Combat.LIFE_STEAL_MULT);
            }
        }
    }
}
