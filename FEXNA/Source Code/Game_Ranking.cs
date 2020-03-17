using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FEXNAVersionExtension;
using FEXNA_Library;
using FEXNA_Library.Chapters;

namespace FEXNA
{
    public class Game_Ranking
    {
        internal string ChapterId { get; private set; }
        internal Difficulty_Modes Difficulty { get; private set; }
        internal DataRanking Data { get; private set; }

        #region Accessors
        private Data_Chapter chapter_data
        {
            get { return Global.data_chapters.ContainsKey(ChapterId) ? Global.data_chapters[ChapterId] : null; }
        }

        public int turns { get { return Data.Turns; } }
        public int combat { get { return Data.Combat; } }
        public int exp { get { return Data.Exp; } }

        public string turns_letter
        {
            get { return DataRanking.individual_score_letter(this.turns); }
        }
        public string combat_letter
        {
            get { return DataRanking.individual_score_letter(this.combat); }
        }
        public string exp_letter
        {
            get { return DataRanking.individual_score_letter(this.exp); }
        }

        public int completion { get { return Data.Completion; } }

        public string completion_letter
        {
            get
            {
                return DataRanking.individual_score_letter(
                    Data.Completion);
            }
        }

        public int ranking_index
        {
            get { return Data.RankingIndex; }
        }

        public int score { get { return Data.Score; } }
        public string rank
        {
            get
            {
                return Data.Rank;
            }
        }
        #endregion

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(ChapterId);
            writer.Write((int)Difficulty);
            writer.Write(Data.Actual_Turns);
            writer.Write(Data.Actual_Combat);
            writer.Write(Data.Actual_Exp);
            writer.Write(Data.Actual_Survival);
            writer.Write(Data.Actual_Completion);
        }

        public static Game_Ranking read(BinaryReader reader)
        {
            string chapter = reader.ReadString();
            Difficulty_Modes difficulty = Difficulty_Modes.Normal;
            if (!Global.LOADED_VERSION.older_than(0, 6, 1, 1))
            {
                difficulty = (Difficulty_Modes)reader.ReadInt32();
            }
            int turns = reader.ReadInt32();
            int combat = reader.ReadInt32();
            int exp = reader.ReadInt32();
            int survival = 0;
            int completion = 0;
            if (!Global.LOADED_VERSION.older_than(0, 4, 0, 5))
            {
                survival = reader.ReadInt32();
                completion = reader.ReadInt32();
            }
            var ranking = new DataRanking(turns, combat, exp, survival, completion);

            return new Game_Ranking(chapter, difficulty, ranking);
        }
        #endregion

        internal Game_Ranking() :
            this(Global.game_state.chapter_id, Global.game_system.Difficulty_Mode,
                Global.game_system.chapter_turn, Global.game_system.chapter_damage_taken,
                Global.game_system.chapter_exp_gain, Global.game_system.chapter_deaths,
                Global.game_system.chapter_completion) { }
        public Game_Ranking(string chapter_id, Difficulty_Modes difficulty,
                int turns, int combat, int exp, int survival, int completion)
            : this(chapter_id, difficulty, new DataRanking(
                turns, combat, exp, survival, completion)) { }
        public Game_Ranking(Game_Ranking ranking) :
            this(ranking.ChapterId, ranking.Difficulty, new DataRanking(ranking.Data)) { }
        internal Game_Ranking(Game_Ranking ranking, Difficulty_Modes difficulty) :
            this(ranking.ChapterId, difficulty, new DataRanking(ranking.Data)) { }

        public Game_Ranking(string chapter_id, Difficulty_Modes difficulty, DataRanking ranking)
        {
            ChapterId = chapter_id;
            Difficulty = difficulty;

            Data = ranking;
            Data.set_par(this.chapter_data);
        }

        public override string ToString()
        {
            return string.Format("Game_Ranking: {0}, {1} - {2} Rank",
                ChapterId, this.score, this.rank);
        }
    }
}
