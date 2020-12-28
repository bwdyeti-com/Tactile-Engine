using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TactileListExtension;
using TactileVersionExtension;

namespace Tactile.Metrics
{
    class Gameplay_Metrics
    {
        private List<Actor_Metrics> StartingActors = new List<Actor_Metrics>();
        private List<Combat_Metrics> Combats = new List<Combat_Metrics>();
        private List<Item_Metrics> Items = new List<Item_Metrics>();
        // Rescuing?
        private List<Actor_Metrics> EndingActors = new List<Actor_Metrics>();

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Global.RUNNING_VERSION);

            StartingActors.write(writer);
            Combats.write(writer);
            Items.write(writer);
            EndingActors.write(writer);
        }

        public static Gameplay_Metrics read(BinaryReader reader)
        {
            Version v = reader.ReadVersion();

            Gameplay_Metrics metrics = new Gameplay_Metrics();
            metrics.StartingActors.read(reader);
            metrics.Combats.read(reader);
            metrics.Items.read(reader);
            metrics.EndingActors.read(reader);
            return metrics;
        }
        #endregion

        internal Gameplay_Metrics() { }
        internal Gameplay_Metrics(Gameplay_Metrics other)
        {
            StartingActors = other.StartingActors.Select(x => new Actor_Metrics(x)).ToList();
            Combats = other.Combats.Select(x => new Combat_Metrics(x)).ToList();
            Items = other.Items.Select(x => new Item_Metrics(x)).ToList();
            EndingActors = other.EndingActors.Select(x => new Actor_Metrics(x)).ToList();
        }

        internal void set_pc_starting_stats(IEnumerable<Game_Unit> units)
        {
            // Add actors that haven't been added yet
            StartingActors.AddRange(units
                    .Where(x => !StartingActors.Any(y => y.Id == x.actor.id))
                    .Select(x => new Actor_Metrics(x)));
        }

        internal void add_combat(int turn, CombatTypes type, Game_Unit attacker, int attacker_hp, int weapon_1_id, int attacker_attacks,
            List<Combat_Map_Object> targets, List<int> target_hps, List<int> weapon_2_ids, List<int> targets_attacks)
        {
            Combats.Add(new Combat_Metrics(turn, type, attacker, attacker_hp, weapon_1_id, attacker_attacks,
                targets, target_hps, weapon_2_ids, targets_attacks));
        }

        internal void add_item(int turn, Game_Unit unit, TactileLibrary.Item_Data item_data)
        {
            Items.Add(new Item_Metrics(turn, unit, item_data));
        }

        internal void set_pc_ending_stats()
        {
            EndingActors = StartingActors.Select(x => new Actor_Metrics(Global.game_actors[x.Id])).ToList();
        }

        internal string query_string()
        {
            string result;
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                write(writer);

                ms.Position = 0;

                using (MemoryStream compressed_stream = new MemoryStream())
                using (DeflateStream compressor = new DeflateStream(compressed_stream, CompressionMode.Compress))
                {
                    ms.CopyTo(compressor);
                    compressor.Close();
                    byte[] buffer = compressed_stream.ToArray();
                    result = BitConverter.ToString(buffer).Replace("-", "");
                }
            }
            return result;
        }

        internal static Gameplay_Metrics from_query_string(string query)
        {
            // Convert hex string into byte stream
            byte[] data = Enumerable.Range(0, query.Length / 2).Select(x => Convert.ToByte(query.Substring(x * 2, 2), 16)).ToArray();
            using (MemoryStream ms = new MemoryStream())
            {

                using (MemoryStream compressed_stream = new MemoryStream(data))
                using (DeflateStream decompressor = new DeflateStream(compressed_stream, CompressionMode.Decompress))
                    decompressor.CopyTo(ms);

                ms.Position = 0;
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    return Gameplay_Metrics.read(reader);
                }
            }
        }

        internal int killed_units(int attacker_team, IEnumerable<int> target_teams)
        {
            return Combats.Sum(x => x.killed_units(attacker_team, target_teams));
        }

        internal bool pc_sees_combat(int actorId)
        {
            return Combats.Any(x =>
                (x.Attacker.Team == Constants.Team.PLAYER_TEAM &&
                    x.Attacker.ActorId == actorId) ||
                (x.target != null && x.target.Value.Team ==
                    Constants.Team.PLAYER_TEAM && x.target.Value.ActorId == actorId));
        }
    }
}
