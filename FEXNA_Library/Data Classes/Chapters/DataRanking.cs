using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FEXNA_Library.Chapters
{
    public class DataRanking
    {
        readonly static string[] RANKS = new string[] { "S", "A", "B", "C" };
        readonly static int[] RANK_SCORES = new int[] { 300, 250, 200 };
        public const int MAX_INDIVIDUAL_SCORE = 150;

        public static string best_rank { get { return RANKS[0]; } }

        private Data_Chapter Chapter;

        public int Actual_Turns { get; private set; }
        public int Actual_Combat { get; private set; }
        public int Actual_Exp { get; private set; }
        public int Actual_Survival { get; private set; }
        public int Actual_Completion { get; private set; }
        
        public DataRanking(int turns, int combat, int exp, int survival, int completion)
        {
            Actual_Turns = turns;
            Actual_Combat = combat;
            Actual_Exp = exp;
            Actual_Survival = survival;
            Actual_Completion = completion;
        }
        public DataRanking(DataRanking ranking)
            : this(ranking.Actual_Turns, ranking.Actual_Combat, ranking.Actual_Exp,
                ranking.Actual_Survival, ranking.Actual_Completion) { }

        public int Turns
        {
            get
            {
                return Math.Min(MAX_INDIVIDUAL_SCORE,
                    Math.Max(0, Chapter.turns_ranking(Actual_Turns)));
            }
        }
        public int Combat
        {
            get
            {
                return Math.Min(MAX_INDIVIDUAL_SCORE,
                    Math.Max(0, Chapter.combat_ranking(Actual_Combat)));
            }
        }
        public int Exp
        {
            get
            {
                return Math.Min(MAX_INDIVIDUAL_SCORE,
                    Math.Max(0, Chapter.exp_ranking(Actual_Exp)));
            }
        }
        public int Completion
        {
            get
            {
                return Math.Min(MAX_INDIVIDUAL_SCORE,
                    Math.Max(0, Chapter.completion_ranking(Actual_Completion)));
            }
        }

        public int Score
        {
            get
            {
                return Turns + Combat + Exp;
            }
        }
        public int RankingIndex
        {
            get
            {
                int i = 0;
                for (; i < RANK_SCORES.Length; i++)
                    if (Score >= RANK_SCORES[i])
                        break;
                return i;
            }
        }
        public string Rank
        {
            get
            {
                if (Chapter.Unranked)
                    return RANKS[0];
                return RANKS[RankingIndex];
            }
        }

        public static string individual_score_letter(int score)
        {
            int i = 0;
            for (; i < RANK_SCORES.Length; i++)
                if (score * 3 >= RANK_SCORES[i])
                    break;
            return RANKS[i];
        }

        public void set_par(Data_Chapter chapter)
        {
            if (Chapter == null)
                Chapter = chapter;
        }
        public void set_par(int turns, int combat, int exp, int completion)
        {
            set_par(Chapter = new Data_Chapter
                {
                    Ranking_Turns = turns,
                    Ranking_Combat = combat,
                    Ranking_Exp = exp,
                    Ranking_Completion = completion
                });
        }

        public float get_turns_par(float rank)
        {
            return Data_Chapter.get_turns_par(Actual_Turns, rank);
        }
        public float get_combat_par(float rank)
        {
            return Data_Chapter.get_combat_par(Actual_Combat, rank);
        }
        public float get_exp_par(float rank)
        {
            return Data_Chapter.get_exp_par(Actual_Exp, rank);
        }
        public float get_completion_par(float rank)
        {
            return Data_Chapter.get_completion_par(Actual_Completion, rank);
        }
    }
}
