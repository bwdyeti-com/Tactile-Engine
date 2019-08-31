using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FEXNADictionaryExtension;
using FEXNAVersionExtension;

namespace FEXNA
{
    class PastRankings : ICloneable
    {
        private Dictionary<string, Game_Ranking> Data;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            Data.write(writer);
        }

        public static PastRankings read(BinaryReader reader, Difficulty_Modes difficulty)
        {
            PastRankings result = new PastRankings();
            result.Data.read(reader);
            
            // If this save predates storing difficulty in the ranking object
            if (Global.LOADED_VERSION.older_than(0, 6, 1, 1))
            {
                foreach (var key in result.Data.Keys.ToList())
                    result.Data[key] = new Game_Ranking(result.Data[key], difficulty);
            }
            
            return result;
        }
        #endregion

        public PastRankings()
        {
            Data = new Dictionary<string, Game_Ranking>();
        }
        public PastRankings(Dictionary<string, Game_Ranking> data)
        {
            Data = data.ToDictionary(p => p.Key, p => new Game_Ranking(p.Value));
        }
        public PastRankings(PastRankings source)
        {
            Data = source.Data.ToDictionary(p => p.Key, p => new Game_Ranking(p.Value));
        }

        public override string ToString()
        {
            return string.Format("PastRankings: {0} Entries", Data.Count);
        }

        public void CopyTo(PastRankings target)
        {
            foreach (var pair in Data)
                target.SetRanking(pair.Key, pair.Value);
        }

        public void SetRanking(string key, Game_Ranking value)
        {
            Data[key] = new Game_Ranking(value);
        }

        public Game_Ranking this[string key]
        {
            get
            {
                return Data[key];
            }
        }

        public int MinimumRank
        {
            get
            {
                if (!Data.Any())
                    //@Debug: C rank
                    return 3;
                return Data.Values.Max(x => x.ranking_index);
            }
        }

        public Dictionary<string, Game_Ranking> GetData()
        {
            return Data.ToDictionary(p => p.Key, p => new Game_Ranking(p.Value));
        }

        public object Clone()
        {
            return new PastRankings(this);
        }
    }
}
