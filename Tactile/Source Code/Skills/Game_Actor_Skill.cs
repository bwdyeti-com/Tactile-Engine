using System;
using System.Collections.Generic;
using System.Linq;
using TactileLibrary;
using TactileStringExtension;

namespace Tactile
{
    partial class Game_Actor
    {
        protected List<int> Added_Attacks = new List<int>();
        protected bool Fatality = false;
        public bool skill_activated;
        public List<int> added_attacks { get { return Added_Attacks; } }
        public bool fatality { get { return Fatality; } }

        public void clear_added_attacks()
        {
            Added_Attacks.Clear();
        }

        #region Skill Setup
        public void reset_skills()
        {
            reset_skills(false);
        }
        public void reset_skills(bool full)
        {
            if (Astra_Count <= 0 || full)
            {
                Astra_Activated = false;
                Astra_Count = 0;
            }
            skill_activated = false;
            Astra_Missed = false;
            Luna_Activated = false;
            Sol_Activated = false;
            Bastion_Activated = false;
            SprlDve_Activated = false;
            Nova_Activated = false;

            Deter_Activated = false;
            Frenzy_Activated = false;
            Frenzy_Count = 0;
            AdeptActivated = false;
            AdeptCount = 0;

            Fatality = false;

            //skill_flash = false; //Yeti
            // After reset
            if (Astra_Activated || Frenzy_Activated || AdeptActivated)
                skill_activated = true;
        }

        public void activate_battle_skill(string skill_id)
        {
            activate_battle_skill(skill_id, false);
        }
        public void activate_battle_skill(string skill_id, bool defensive_atk_skl)
        {
            switch (skill_id)
            {
                case "ASTRA":
                    activate_astra();
                    break;
                case "LUNA":
                    activate_luna();
                    break;
                case "SOL":
                    activate_sol();
                    break;
                case "BASTION":
                    if (defensive_atk_skl)
                        activate_bastion();
                    break;
                case "SPRLDVE":
                    if (!defensive_atk_skl)
                        activate_sprldve();
                    break;
                case "NOVA":
                    activate_nova();
                    break;
                case "DETER":
                    activate_deter();
                    break;
                case "ADEPT":
                    activate_adept();
                    break;
                case "FRENZY":
                    activate_frenzy();
                    break;
            }
        }
        #endregion

        #region Growth
        internal int growth_bonus_skill(Stat_Labels i)
        {
            int n = 0;
            // Skills: Growth%+
            // Disallows skills from items/weapons/statuses when training
            List<int> growth_skills = Global.game_system.home_base ? skills : all_skills;
            switch (i)
            {
                case Stat_Labels.Hp:
                    // Skills: HP%+
                    foreach (int skill_id in growth_skills.Where(x => Global.data_skills[x].Abstract.substring(0, 4) == "HP%+"))
                    {
                        double str_test;
                        string name = Global.data_skills[skill_id].Abstract;
                        if (double.TryParse(name.substring(4, name.Length - 4), out str_test))
                            n += Convert.ToInt32(name.Substring(4, name.Length - 4));
                    }
                    break;
                case Stat_Labels.Pow:
                    // Skills: Pow%+
                    foreach (int skill_id in growth_skills.Where(x => Global.data_skills[x].Abstract.substring(0, 5) == "POW%+"))
                    {
                        double str_test;
                        string name = Global.data_skills[skill_id].Abstract;
                        if (double.TryParse(name.substring(5, name.Length - 5), out str_test))
                            n += Convert.ToInt32(name.Substring(5, name.Length - 5));
                    }
                    break;
                case Stat_Labels.Skl:
                    // Skills: Skl%+
                    foreach (int skill_id in growth_skills.Where(x => Global.data_skills[x].Abstract.substring(0, 5) == "SKL%+"))
                    {
                        double str_test;
                        string name = Global.data_skills[skill_id].Abstract;
                        if (double.TryParse(name.substring(5, name.Length - 5), out str_test))
                            n += Convert.ToInt32(name.Substring(5, name.Length - 5));
                    }
                    break;
                case Stat_Labels.Spd:
                    // Skills: Spd%+
                    foreach (int skill_id in growth_skills.Where(x => Global.data_skills[x].Abstract.substring(0, 5) == "SPD%+"))
                    {
                        double str_test;
                        string name = Global.data_skills[skill_id].Abstract;
                        if (double.TryParse(name.substring(5, name.Length - 5), out str_test))
                            n += Convert.ToInt32(name.Substring(5, name.Length - 5));
                    }
                    break;
                case Stat_Labels.Lck:
                    // Skills: Lck%+
                    foreach (int skill_id in growth_skills.Where(x => Global.data_skills[x].Abstract.substring(0, 5) == "LCK%+"))
                    {
                        double str_test;
                        string name = Global.data_skills[skill_id].Abstract;
                        if (double.TryParse(name.substring(5, name.Length - 5), out str_test))
                            n += Convert.ToInt32(name.Substring(5, name.Length - 5));
                    }
                    break;
                case Stat_Labels.Def:
                    // Skills: Def%+
                    foreach (int skill_id in growth_skills.Where(x => Global.data_skills[x].Abstract.substring(0, 5) == "DEF%+"))
                    {
                        double str_test;
                        string name = Global.data_skills[skill_id].Abstract;
                        if (double.TryParse(name.substring(5, name.Length - 5), out str_test))
                            n += Convert.ToInt32(name.Substring(5, name.Length - 5));
                    }
                    break;
                case Stat_Labels.Res:
                    // Skills: Res%+
                    foreach (int skill_id in growth_skills.Where(x => Global.data_skills[x].Abstract.substring(0, 5) == "RES%+"))
                    {
                        double str_test;
                        string name = Global.data_skills[skill_id].Abstract;
                        if (double.TryParse(name.substring(5, name.Length - 5), out str_test))
                            n += Convert.ToInt32(name.Substring(5, name.Length - 5));
                    }
                    break;
            }
            // Skills: Affinity+
            if (affin != Affinities.None && Constants.Support.AFFINITY_GROWTHS[affin][0].Contains(i))
                foreach (int skill_id in growth_skills.Where(x => Global.data_skills[x].Abstract.substring(0, 7) == "AFFIN%+"))
                {
                    // Add skill amount to positive affinity stats
                    double str_test;
                    string name = Global.data_skills[skill_id].Abstract;
                    if (double.TryParse(name.substring(7, name.Length - 7), out str_test))
                        n += Convert.ToInt32(name.Substring(7, name.Length - 7));
                }
            else if (affin != Affinities.None && Constants.Support.AFFINITY_GROWTHS[affin][1].Contains(i))
                foreach (int skill_id in growth_skills.Where(x => Global.data_skills[x].Abstract.substring(0, 7) == "AFFIN%+"))
                {
                    // Remove half skill amount to negative affinity stats
                    double str_test;
                    string name = Global.data_skills[skill_id].Abstract;
                    if (double.TryParse(name.substring(7, name.Length - 7), out str_test))
                        n -= Convert.ToInt32(name.Substring(7, name.Length - 7)) / 2;
                }

            return n;
        }
        #endregion

        #region Activation Skills
        // Astra
        public const int ASTRA_HITS = 5;
        protected bool Astra_Activated;
        protected bool Astra_Missed;
        public bool astra_activated { get { return Astra_Activated; } }
        public bool astra_missed { get { return Astra_Missed; } }
        protected int Astra_Count;
        public int astra_count
        {
            get { return Astra_Count; }
            set { Astra_Count = value; }
        }

        public void activate_astra()
        {
            Astra_Activated = true;
            skill_activated = true;
            Astra_Count = -1;
            //skill_flash = true; //Yeti
        }

        public void astra_hit_confirm(bool hit)
        {
            Astra_Count = 0;
            if (hit)
            {
                Astra_Count = ASTRA_HITS;
                for (int i = 0; i < ASTRA_HITS - 1; i++)
                    Added_Attacks.Add(1);
            }
            else
                Astra_Missed = true;
        }

        public void astra_use()
        {
            Astra_Count = Math.Max(Astra_Count - 1, 0);
        }

        // Luna
        protected bool Luna_Activated;
        public bool luna_activated { get { return Luna_Activated; } }

        public void activate_luna()
        {
            Luna_Activated = true;
            skill_activated = true;
            //skill_flash = true; //Yeti
        }

        // Sol
        protected bool Sol_Activated;
        public bool sol_activated { get { return Sol_Activated; } }

        public void activate_sol()
        {
            Sol_Activated = true;
            skill_activated = true;
            //skill_flash = true; //Yeti
        }

        // Bastion
        protected bool Bastion_Activated;
        public bool bastion_activated { get { return Bastion_Activated; } }

        public void activate_bastion()
        {
            Bastion_Activated = true;
            skill_activated = true;
            //skill_flash = true; //Yeti
        }

        // Spiral Dive
        protected bool SprlDve_Activated;
        public bool sprldve_activated { get { return SprlDve_Activated; } }

        public void activate_sprldve()
        {
            SprlDve_Activated = true;
            skill_activated = true;
            //skill_flash = true; //Yeti
        }

        // Nova
        protected bool Nova_Activated;
        public bool nova_activated { get { return Nova_Activated; } }

        public void activate_nova()
        {
            Nova_Activated = true;
            skill_activated = true;
            //skill_flash = true; //Yeti
        }

        // Determination
        protected bool Deter_Activated, Deter_Counter;
        public bool deter_activated { get { return Deter_Activated; } }
        public bool deter_counter
        {
            get { return Deter_Counter; }
            set { Deter_Counter = value; }
        }

        public void activate_deter()
        {
            Deter_Activated = true;
            Deter_Counter = false;
            skill_activated = true;
        }

        // Adept
        protected bool AdeptActivated;
        public bool adeptActivated { get { return AdeptActivated; } }
        protected int AdeptCount;
        public int adeptCount { get { return AdeptCount; } }

        public void activate_adept()
        {
            AdeptActivated = true;
            skill_activated = true;
            AdeptCount = 1;
            Added_Attacks.Add(1);
        }

        public void adept_use()
        {
            AdeptCount = Math.Max(AdeptCount - 1, 0);
        }

        // Frenzy
        protected bool Frenzy_Activated;
        public bool frenzy_activated { get { return Frenzy_Activated; } }
        protected int Frenzy_Count;
        public int frenzy_count { get { return Frenzy_Count; } }

        public void activate_frenzy()
        {
            Frenzy_Activated = true;
            skill_activated = true;
            Frenzy_Count = 1;
            Added_Attacks.Add(1);
        }

        public void frenzy_use()
        {
            Frenzy_Count = Math.Max(Frenzy_Count - 1, 0);
        }
        #endregion

        #region Weapon Level
        public void wexp_from_weapon_skill(Data_Weapon weapon, ref int wexp)
         {
            // Skills: Discipline
            if (has_skill("DISCIPLINE"))
                wexp *= 2;
            // Skills: Veteran
            if (has_skill("VETERAN"))
                wexp *= 2;
            // Skills: Academic
            else if (has_skill("ACADEMIC"))
                wexp *= 2;
        }

        protected int min_weapon_exp_skill(WeaponType type, int wexp)
        {
            // Skills: Veteran
            if (has_skill("VETERAN"))
                if (wexp <= 0 && actor_class.Max_WLvl[type.Key - 1] <= 0)
                {
                    if (type.DisplayedInStatus)
                        if (!type.IsMagic && !type.IsStaff)
                            wexp = 1;
                }
            // Skills: Academic
            if (has_skill("ACADEMIC"))
                if (wexp <= 0 && actor_class.Max_WLvl[type.Key - 1] <= 0)
                {
                    if (type.DisplayedInStatus)
                        if (type.IsMagic || type.IsStaff)
                            wexp = 1;
                }
            return wexp;
        }

        protected int max_weapon_level_skill(WeaponType type, int rank)
        {
            // Skills: Veteran
            if (has_skill("VETERAN"))
                if (rank <= 4)
                {
                    if (type.DisplayedInStatus)
                        if (!type.IsMagic && !type.IsStaff)
                            rank = 4;
                }
            // Skills: Academic
            if (has_skill("ACADEMIC"))
                if (rank <= 4)
                {
                    if (type.DisplayedInStatus)
                        if (type.IsMagic || type.IsStaff)
                            rank = 4;
                }
            return rank;
        }

        protected int max_weapon_level_override(WeaponType type, int rank)
        {
            // Skills: Peerless
            if (has_skill("PEERLESS"))
                rank = Math.Max(rank, actor_class.Max_WLvl[type.Key - 1]);
            return rank;
        }
        #endregion

        public bool can_dance_skill()
        {
            // Skills: Play
            if (has_skill("PLAY"))
                return true;
            // Skills: Dance
            if (has_skill("DANCE"))
                return true;
            return false;
        }

        public bool can_construct_skill()
        {
            // Skills: Machinist
            if (has_skill("MACHINE"))
                return true;
            return false;
        }

        protected int? weapon_use_count_skill(bool is_hit, int uses)
        {
            // Skills: Blessing
            if (has_skill("BLESS"))
                return 0;
            // Astra
            if (Astra_Count > 0)
                return 0;
            return null;
        }
    }
}
