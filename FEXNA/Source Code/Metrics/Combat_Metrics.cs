using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using FEXNAListExtension;
using FEXNAVector2Extension;
using FEXNAVersionExtension;

namespace FEXNA.Metrics
{
    enum CombatTypes { Battle, Staff, DestroyableTerrain, AoE }

    class Combat_Metrics
    {
        internal int Turn { get; private set; }
        private string CombatType;
        internal Combatant_Metric Attacker { get; private set; }
        private List<Combatant_Metric> Targets = new List<Combatant_Metric>();

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Turn);
            writer.Write(CombatType);
            Attacker.write(writer);
            Targets.write(writer);
        }

        public static Combat_Metrics read(BinaryReader reader)
        {
            Combat_Metrics result = new Combat_Metrics();
            if (!Global.LOADED_VERSION.older_than(0, 4, 6, 9))
                result.Turn = reader.ReadInt32();
            result.CombatType = reader.ReadString();
            result.Attacker = Combatant_Metric.read(reader);
            result.Targets.read(reader);
            return result;
        }
        #endregion

        #region Accessors
        internal Combatant_Metric? target { get { return Targets.Any() ? Targets.First() : (Combatant_Metric?)null; } }
        #endregion

        public override string ToString()
        {
            return string.Format("Combat Metrics: Turn {0}, Actor {2}, {1}", Turn, CombatType, Attacker.ActorId);
        }

        private Combat_Metrics() { }
        internal Combat_Metrics(int turn, CombatTypes type, Game_Unit attacker, int attacker_hp, int weapon_1_id, int attacker_attacks,
            List<Combat_Map_Object> targets, List<int> target_hps, List<int> weapon_2_ids, List<int> targets_attacks)
        {
            Turn = turn;
            CombatType = type.ToString();
            Attacker = new Combatant_Metric(attacker, attacker_hp, weapon_1_id, attacker_attacks);
            Targets = targets == null ? new List<Combatant_Metric>() : Enumerable.Range(0, targets.Count)
                .Select(x => new Combatant_Metric(targets[x], target_hps[x], weapon_2_ids[x], targets_attacks[x])).ToList();
        }
        internal Combat_Metrics(Combat_Metrics other)
        {
            Turn = other.Turn;
            CombatType = other.CombatType;
            Attacker = other.Attacker;
            Targets = new List<Combatant_Metric>(other.Targets);
        }

        internal int killed_units(int attacker_team, IEnumerable<int> target_teams)
        {
            // If the attacking unit is on the team being tested, check if any of their targets are dead and on the target teams
            if (Attacker.Team == attacker_team)
                return Targets.Count(x => x.is_dead && target_teams.Contains(x.Team));
            // Else if the attacker is on the target teams and dead, check if any of the targets are on the team being tested
            else if (Attacker.is_dead && target_teams.Contains(Attacker.Team) && Targets.Any(x => x.Team == attacker_team))
                return 1;
            // Else no one is valid
            else
                return 0;
        }
    }

    struct Combatant_Metric
    {
        internal int Team { get; private set; }
        internal int ActorId { get; private set; }
        internal int ClassId { get; private set; }
        internal int StartHp { get; private set; }
        internal int EndHp { get; private set; }
        internal int WeaponId { get; private set; }
        internal string WeaponName { get; private set; }
        internal int Attacks { get; private set; }
        internal Vector2 Loc { get; private set; }

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Team);
            writer.Write(ActorId);
            writer.Write(ClassId);
            writer.Write(StartHp);
            writer.Write(EndHp);
            writer.Write(WeaponId);
            writer.Write(WeaponName);
            writer.Write(Attacks);
            Loc.write(writer);
        }

        public static Combatant_Metric read(BinaryReader reader)
        {
            Combatant_Metric result = new Combatant_Metric();
            result.Team = reader.ReadInt32();
            result.ActorId = reader.ReadInt32();
            result.ClassId = reader.ReadInt32();
            result.StartHp = reader.ReadInt32();
            result.EndHp = reader.ReadInt32();
            result.WeaponId = reader.ReadInt32();
            result.WeaponName = reader.ReadString();
            result.Attacks = reader.ReadInt32();
            result.Loc = result.Loc.read(reader);
            return result;
        }
        #endregion

        #region Accessors
        internal bool is_dead { get { return EndHp <= 0; } }
        #endregion

        public Combatant_Metric(Combat_Map_Object map_object, int start_hp, int weapon_id, int attack_count) : this()
        {
            Team = map_object is Game_Unit ? (map_object as Game_Unit).team : -1;
            ActorId = map_object is Game_Unit ? (map_object as Game_Unit).actor.id : map_object.id;
            ClassId = map_object is Game_Unit ? (map_object as Game_Unit).actor.class_id : -1;
            StartHp = start_hp;
            EndHp = map_object.hp;
            WeaponId = weapon_id;
            WeaponName = Global.data_weapons.ContainsKey(weapon_id) ? Global.data_weapons[weapon_id].full_name() : "-----";
            Attacks = attack_count;
            Loc = map_object.loc;
        }
    }
}
