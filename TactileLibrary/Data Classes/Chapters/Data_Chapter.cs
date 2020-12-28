using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ArrayExtension;
using ListExtension;
using Vector2Extension;

namespace TactileLibrary
{
    public enum ChapterLabels
    {
        Chapter, Act, Part, Paralogue, ExMap, TrialMap
    }

    public class Data_Chapter : TactileDataContent
    {
        public string Id = "";
        /// <summary>
        /// Chapters that precede this chapter.
        /// When this chapter is started the data from these chapters will be loaded.
        /// Battalions will be loaded from one chapter each, with preference to the start of the list.
        /// </summary>
        public List<string> Prior_Chapters = new List<string>();
        /// <summary>
        /// A subset of Prior_Chapters listing the prior chapters to load ranking data from.
        /// If this is empty, rankings are loaded from all prior chapters.
        /// </summary>
        public List<string> Prior_Ranking_Chapters = new List<string>();
        /// <summary>
        /// Chapters that need to be completed to access this chapter.
        /// This is combined with Prior_Chapters to determine if a chapter is available.
        /// ONLY battaltion data will be loaded from these chapters, and ONLY if no other prior chapters have the same battalions.
        /// </summary>
        public List<string> Completed_Chapters = new List<string>();
        public bool Standalone;
        public ChapterLabels Label = ChapterLabels.Chapter;
        public string LabelString = "";
        public string Chapter_Name = "";
        public string WorldMapNameFormatString = "";
        public string DisplayNameFormatString = "";
        public string FileSelectNameFormatString = "";
        public string ShortNameFormatString = "";
        public string ListNameFormatString = "";
        public string AlternateTitle = "";
        public string Arc = "";
        public Vector2 World_Map_Loc;
        public int World_Map_Lord_Id;
        public List<string> Turn_Themes = new List<string>();
        public List<string> Battle_Themes = new List<string>();
        public int Battalion;
        public string Text_Key = "";
        public string Event_Data_Id = "";
        /// <summary>
        /// This chapter only has cutscenes or is a strict tutorial, and
        /// should not be shown on ranking screens or have its rank on the
        /// world map.
        /// </summary>
        public bool Unranked;
        public int Ranking_Turns;
        public int Ranking_Combat = 0;
        public int Ranking_Exp = 0;
        public int Ranking_Completion = 0;
        public Preset_Chapter_Data Preset_Data;
        public List<string> Progression_Ids = new List<string>();

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static Data_Chapter GetEmptyInstance()
        {
            return new Data_Chapter();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var chapter = (Data_Chapter)other;

            Id = chapter.Id;
            Progression_Ids = new List<string>(chapter.Progression_Ids);
            Prior_Chapters = new List<string>(chapter.Prior_Chapters);
            Prior_Ranking_Chapters = new List<string>(chapter.Prior_Ranking_Chapters);
            Completed_Chapters = new List<string>(chapter.Completed_Chapters);
            Standalone = chapter.Standalone;

            Label = chapter.Label;
            LabelString = chapter.LabelString;
            Chapter_Name = chapter.Chapter_Name;

            WorldMapNameFormatString = chapter.WorldMapNameFormatString;
            DisplayNameFormatString = chapter.DisplayNameFormatString;
            FileSelectNameFormatString = chapter.FileSelectNameFormatString;
            ShortNameFormatString = chapter.ShortNameFormatString;
            ListNameFormatString = chapter.ListNameFormatString;
            AlternateTitle = chapter.AlternateTitle;

            Arc = chapter.Arc;
            World_Map_Loc = chapter.World_Map_Loc;
            World_Map_Lord_Id = chapter.World_Map_Lord_Id;

            Turn_Themes = new List<string>(chapter.Turn_Themes);
            Battle_Themes = new List<string>(chapter.Battle_Themes);

            Battalion = chapter.Battalion;
            Text_Key = chapter.Text_Key;
            Event_Data_Id = chapter.Event_Data_Id;

            Unranked = chapter.Unranked;
            Ranking_Turns = chapter.Ranking_Turns;
            Ranking_Combat = chapter.Ranking_Combat;
            Ranking_Exp = chapter.Ranking_Exp;
            Ranking_Completion = chapter.Ranking_Completion;

            Preset_Data = new Preset_Chapter_Data(chapter.Preset_Data);
        }

        public static Data_Chapter ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            Id = input.ReadString();
            Prior_Chapters.read(input);
            Prior_Ranking_Chapters.read(input);
            Completed_Chapters.read(input);
            Standalone = input.ReadBoolean();

            Label = (ChapterLabels)input.ReadInt32();
            LabelString = input.ReadString();
            Chapter_Name = input.ReadString();

            WorldMapNameFormatString = input.ReadString();
            DisplayNameFormatString = input.ReadString();
            FileSelectNameFormatString = input.ReadString();
            ShortNameFormatString = input.ReadString();
            ListNameFormatString = input.ReadString();
            AlternateTitle = input.ReadString();
            
            Arc = input.ReadString();
            World_Map_Loc = World_Map_Loc.read(input);
            World_Map_Lord_Id = input.ReadInt32();

            Turn_Themes.read(input);
            Battle_Themes.read(input);

            Battalion = input.ReadInt32();
            Text_Key = input.ReadString();
            Event_Data_Id = input.ReadString();

            Unranked = input.ReadBoolean();
            Ranking_Turns = input.ReadInt32();
            Ranking_Combat = input.ReadInt32();
            Ranking_Exp = input.ReadInt32();
            Ranking_Completion = input.ReadInt32();

            Preset_Data = Preset_Chapter_Data.Read(input);

            Progression_Ids.read(input);
        }

        public override void Write(BinaryWriter output)
        {
            output.Write(Id);
            Prior_Chapters.write(output);
            Prior_Ranking_Chapters.write(output);
            Completed_Chapters.write(output);
            output.Write(Standalone);

            output.Write((int)Label);
            output.Write(LabelString);
            output.Write(Chapter_Name);

            output.Write(WorldMapNameFormatString);
            output.Write(DisplayNameFormatString);
            output.Write(FileSelectNameFormatString);
            output.Write(ShortNameFormatString);
            output.Write(ListNameFormatString);
            output.Write(AlternateTitle);
            
            output.Write(Arc);
            World_Map_Loc.write(output);
            output.Write(World_Map_Lord_Id);

            Turn_Themes.write(output);
            Battle_Themes.write(output);

            output.Write(Battalion);
            output.Write(Text_Key);
            output.Write(Event_Data_Id);

            output.Write(Unranked);
            output.Write(Ranking_Turns);
            output.Write(Ranking_Combat);
            output.Write(Ranking_Exp);
            output.Write(Ranking_Completion);

            Preset_Data.Write(output);

            Progression_Ids.write(output);
        }
        #endregion

        public Data_Chapter() { }
        public Data_Chapter(Data_Chapter chapter)
        {
            CopyFrom(chapter);
        }
        
        public override string ToString()
        {
            return string.Format("{0}, {1}", Id, Chapter_Name);
        }

        public override object Clone()
        {
            return new Data_Chapter(this);
        }

        public string World_Map_Name
        {
            get
            {
                return FormattedName(WorldMapNameFormatString, "{3}", false);
            }
        }
        public string FullName
        {
            get
            {
                return FormattedName(DisplayNameFormatString, "{3}:{2}", true);
            }
        }
        public string FileSelectName
        {
            get
            {
                return FormattedName(FileSelectNameFormatString, "{3}:{2}", true);
            }
        }
        public string ShortName
        {
            get
            {
                return FormattedName(ShortNameFormatString, "{3}", false);
            }
        }
        public string ListName
        {
            get
            {
                return FormattedName(ListNameFormatString, "{1}", true);
            }
        }

        private string FormattedName(
            string formatString, 
            string defaultFormat,
            bool abbreviatedLabel)
        {
            try
            {
                if (string.IsNullOrEmpty(formatString))
                    formatString = defaultFormat;

                string label = GetLabelToString(Label, abbreviatedLabel);
                string labelString = LabelString;
                while (labelString.StartsWith(" "))
                    labelString = labelString.Substring(1);

                string fullLabel = string.Format("{0} {1}",
                    label,
                    labelString);

                return string.Format(formatString,
                    label, labelString, Chapter_Name, fullLabel, AlternateTitle, Id);
            }
            catch (FormatException ex)
            {
                return "-----";
            }
        }

        private static string GetLabelToString(ChapterLabels label, bool abbreviatedLabel)
        {
            switch (label)
            {
                case ChapterLabels.Act:
                    return abbreviatedLabel ? "Act" : "Act";
                case ChapterLabels.Part:
                    return abbreviatedLabel ? "Part" : "Part";
                case ChapterLabels.Paralogue:
                    return abbreviatedLabel ? "Pr." : "Paralogue";
                case ChapterLabels.ExMap:
                    return abbreviatedLabel ? "Ex." : "ExMap";
                case ChapterLabels.TrialMap:
                    return abbreviatedLabel ? "Tr." : "Trial Map";
                case ChapterLabels.Chapter:
                default:
                    return abbreviatedLabel ? "Ch." : "Chapter";
            }
        }

        public static int LabelWidth(ChapterLabels label)
        {
            switch (label)
            {
                case ChapterLabels.Act:
                    return 16;
                case ChapterLabels.Part:
                    return 24;
                case ChapterLabels.Paralogue:
                    return 48;
                case ChapterLabels.ExMap:
                    return 40;
                case ChapterLabels.TrialMap:
                    return 48;
                case ChapterLabels.Chapter:
                default:
                    return 40;
            }
        }

        public List<string> get_previous_chapters(Dictionary<string, Data_Chapter> ChapterData)
        {
            return ChapterData.Where(x => x.Value.Progression_Ids
                    .Intersect(Prior_Chapters.Union(Completed_Chapters).Distinct()).Any())
                .Select(x => x.Key)
                .ToList();
        }

        public List<string> get_prior_chapters(Dictionary<string, Data_Chapter> ChapterData)
        {
            return ChapterData.Where(x => x.Value.Progression_Ids.Intersect(Prior_Chapters).Any())
                .Select(x => x.Key)
                .ToList();
        }

        public List<string> get_followup_chapters(Dictionary<string, Data_Chapter> ChapterData)
        {
            return ChapterData.Where(x => x.Value.Prior_Chapters.Union(x.Value.Completed_Chapters)
                    .Distinct()
                    .Intersect(Progression_Ids).Any())
                .Select(x => x.Key)
                .ToList();
        }

        public bool no_previous_chapters()
        {
            return Prior_Chapters.Count == 0 && Completed_Chapters.Count == 0;
        }

        const float MAX_TURNS_SCORE_MULT = 0.5f;
        const float MIN_TURNS_SCORE_MULT = 2.5f;
        const float MAX_COMBAT_SCORE_MULT = 0.5f;
        const float MIN_COMBAT_SCORE_MULT = 2.0f;
        const float MAX_EXP_SCORE_MULT = 1.5f;
        const float MIN_EXP_SCORE_MULT = 0.25f;

        public int turns_ranking(int turns)
        {
            if (Ranking_Turns == 0)
                return 0;

            return ranking_value(turns, Ranking_Turns,
                MIN_TURNS_SCORE_MULT, MAX_TURNS_SCORE_MULT);
        }
        public int combat_ranking(int combat)
        {
            if (Ranking_Combat == 0)
                return 0;

            return ranking_value(combat, Ranking_Combat,
                MIN_COMBAT_SCORE_MULT, MAX_COMBAT_SCORE_MULT);
        }
        public int exp_ranking(int exp)
        {
            if (Ranking_Exp == 0)
                return 0;

            return ranking_value(exp, Ranking_Exp,
                MIN_EXP_SCORE_MULT, MAX_EXP_SCORE_MULT);
        }
        public int completion_ranking(int completion)
        {
            if (Ranking_Completion == 0)
                return 0;

            return ranking_value(completion, Ranking_Completion * 0.8f, 0,
                Chapters.DataRanking.MAX_INDIVIDUAL_SCORE / 100f);
        }

        private static int ranking_value(int value, float par, float minMult, float maxMult)
        {
            float rank;
            // Less than 100
            if (value < par ^ minMult > maxMult)
            {
                if (minMult < 1)
                    // Low score, positive correlation
                    rank = (value - (minMult * par)) / (par - (minMult * par));
                else
                    // Low score, inverse correlation
                    rank = ((minMult * par) - value) / ((minMult - 1) * par);

                return (int)MathHelper.Lerp(0, 100, rank);
            }
            // More than 100
            {
                if (maxMult < 1)
                    // High score, inverse correlation
                    rank = (value - (maxMult * par)) / ((1 - maxMult) * par);
                else
                    // High score, positive correlation
                    rank = ((maxMult * par) - value) / ((maxMult - 1) * par);

                return (int)MathHelper.Lerp(
                    Chapters.DataRanking.MAX_INDIVIDUAL_SCORE, 100, rank);
            }
        }

        internal static float get_turns_par(int turns, float rank)
        {
            return get_par(turns, rank, MIN_TURNS_SCORE_MULT, MAX_TURNS_SCORE_MULT);
        }
        internal static float get_combat_par(int combat, float rank)
        {
            return get_par(combat, rank, MIN_COMBAT_SCORE_MULT, MAX_COMBAT_SCORE_MULT);
        }
        internal static float get_exp_par(int exp, float rank)
        {
            return get_par(exp, rank, MIN_EXP_SCORE_MULT, MAX_EXP_SCORE_MULT);
        }
        internal static float get_completion_par(int completion, float rank)
        {
            return get_par(completion / 0.8f, rank, 0, 1.5f);
        }

        private static float get_par(float value, float rank, float minMult, float maxMult)
        {
            // Less than 100
            if (rank < 100)
            {
                rank = MathHelper.Lerp(minMult, 1, rank / 100);
            }
            // More than 100
            else
            {
                rank = MathHelper.Lerp(1, maxMult, (rank - 100) /
                    (Chapters.DataRanking.MAX_INDIVIDUAL_SCORE - 100f));
            }
            if (rank == 0)
                return 0;
            return value / rank;
        }
    }

    public struct Preset_Chapter_Data
    {
        public int Lord_Lvl;
        public int Units;
        public int Gold;
        public int Playtime;
        
        #region Serialization
        internal static Preset_Chapter_Data Read(BinaryReader input)
        {
            Preset_Chapter_Data result = new Preset_Chapter_Data();
            result.Lord_Lvl = input.ReadInt32();
            result.Units = input.ReadInt32();
            result.Gold = input.ReadInt32();
            result.Playtime = input.ReadInt32();
            return result;
        }

        public void Write(BinaryWriter output)
        {
            output.Write(Lord_Lvl);
            output.Write(Units);
            output.Write(Gold);
            output.Write(Playtime);
        }
        #endregion

        internal Preset_Chapter_Data(int lord_level, int units, int gold, int playtime)
        {
            Lord_Lvl = lord_level;
            Units = units;
            Gold = gold;
            Playtime = playtime;
        }
        internal Preset_Chapter_Data(Preset_Chapter_Data data)
        {
            Lord_Lvl = data.Lord_Lvl;
            Units = data.Units;
            Gold = data.Gold;
            Playtime = data.Playtime;
        }
    }
}
