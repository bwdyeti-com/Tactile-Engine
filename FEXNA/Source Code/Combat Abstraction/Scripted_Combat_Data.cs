using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEXNA
{
    class Scripted_Combat_Data : Combat_Data
    {
        public Scripted_Combat_Data(int battler_1_id, int battler_2_id, int distance) :
            base(battler_1_id, battler_2_id, distance) { }

        protected override void set_exp(Game_Unit battler_1, Game_Unit battler_2)
        {
            // Exp/Wexp: battler 1
            Exp_Gain1 = 0;
            Wexp1 = 0;
            // Exp/Wexp: battler 2
            Exp_Gain2 = 0;
            Wexp2 = 0;

            // Set exp gain, reset HP and skills
            battler_1.actor.hp = Hp1;
            battler_1.actor.reset_skills(true);
            if (battler_2 != null)
            {
                battler_2.actor.hp = Hp2;
                battler_2.actor.reset_skills(true);
            }
        }

        protected override void process_attacks(Game_Unit battler_1, Combat_Map_Object battler_2)
        {
            if (Global.game_temp.scripted_battle_stats != null)
                process_scripted_attacks(battler_1, battler_2, Global.game_temp.scripted_battle_stats);
            else
                base.process_attacks(battler_1, battler_2);
        }
        private void process_scripted_attacks(Game_Unit battler_1, Combat_Map_Object battler_2, Scripted_Combat_Script script)
        {
            battler_1.start_attack(-1, battler_2);
            if (battler_2 is Game_Unit)
                (battler_2 as Game_Unit).start_attack(-1, battler_1);

            for (int i = 0; i < script.Stats.Count; i++)
            {
                if (script.Stats[i].Result != Attack_Results.End)
                {
                    bool cont = false;
                    while (!cont)
                    {
                        cont = true;
                        int attacker = script.Stats[i].Attacker;
                        if (attacker > 0)
                        {
                            if (battler_2 == null)
                                add_solo_attack(battler_1, attacker);
                            else if (battler_2.is_unit())
                                add_attack(battler_1, battler_2 as Game_Unit, script.Stats[i]);
                            else
                                add_attack(battler_1, battler_2 as Combat_Map_Object, attacker);
                        }
                    }
                }
            }
            Kill = script.kill;
        }

        protected void add_attack(Game_Unit battler_1, Game_Unit battler_2, Scripted_Combat_Stats stats)
        {
            add_data(battler_1, battler_2, stats);
            Data[Data.Count - 1].Key.Attacker = stats.Attacker;
            // Checks if this is the first attack of this unit
            for (int i = 0; i < Data.Count - 1; i++)
            {
                if (Data[i].Key.Attacker == Data[Data.Count - 1].Key.Attacker)
                {
                    Data[Data.Count - 1].Key.First_Attack = false;
                    break;
                }
            }
            if (stats.Attacker == 1)
                Attacked_1 = true;
            else
                Attacked_2 = true;
            Data[Data.Count - 1].Key.set_attack(Distance, Data[Data.Count - 1].Value, stats);
            // Add Attacks
            battler_1.actor.clear_added_attacks();
            battler_2.actor.clear_added_attacks();
        }

        protected void add_data(Game_Unit battler_1, Game_Unit battler_2, Scripted_Combat_Stats stats)
        {
            Data.Add(new KeyValuePair<Combat_Round_Data, List<Combat_Action_Data>>(
                new Combat_Round_Data(battler_1, battler_2, stats), new List<Combat_Action_Data>()));
        }

        protected override void end_battle()
        {
            if (Global.game_temp.scripted_battle_stats != null)
                Data[Data.Count - 1].Key.end_battle(
                    Global.game_temp.scripted_battle_stats.Stats.Last(), Data[Data.Count - 1].Value);
            else
                base.end_battle();
        }

        public override void apply_combat(bool immediate_level, bool skip_exp) { }
    }
}
