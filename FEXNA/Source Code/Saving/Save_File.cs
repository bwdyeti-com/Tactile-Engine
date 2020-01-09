using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if DEBUG
using System.Diagnostics;
#endif
using FEXNAVersionExtension;
using FEXNA_Library;

namespace FEXNA.IO
{
    class Save_File
    {
        public Mode_Styles Style = Mode_Styles.Standard;
        public Difficulty_Modes Difficulty = Difficulty_Modes.Normal;
        public string Description { get; private set; }
        private Dictionary<string, Dictionary<string, Save_Data>> Data =
            new Dictionary<string, Dictionary<string, Save_Data>>();

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write((int)Style);
            writer.Write((int)Difficulty);
            writer.Write(Description);

            writer.Write(Data.Count);
            foreach(var pair in Data)
            {
                writer.Write(pair.Key);
                write_chapter(pair.Value, writer);
            }
        }
        private static void write_chapter(Dictionary<string, Save_Data> data, BinaryWriter writer)
        {
            writer.Write(data.Count);
            foreach (var pair in data)
            {
                writer.Write(pair.Key);
                pair.Value.write(writer);
            }
        }

        public static Save_File read(BinaryReader reader)
        {
            Save_File result = new Save_File();
            result.Style = (Mode_Styles)reader.ReadInt32();
            if (!Global.LOADED_VERSION.older_than(0, 4, 3, 4))
                 result.Difficulty = (Difficulty_Modes)reader.ReadInt32();
            if (!Global.LOADED_VERSION.older_than(0, 5, 7, 0))
                result.Description = reader.ReadString();

            int chapter_count = reader.ReadInt32();
            if (Global.LOADED_VERSION.older_than(0, 6, 1, 0))
            {
                var old_data = new Dictionary<string, Dictionary<Difficulty_Modes, Dictionary<string, Save_Data>>>();
                if (Global.LOADED_VERSION.older_than(0, 4, 4, 0))
                {
                    for (int i = 0; i < chapter_count; i++)
                    {
                        string key = reader.ReadString();
                        Save_Data value = Save_Data.read(reader);
                        old_data.Add(key, new Dictionary<Difficulty_Modes, Dictionary<string, Save_Data>> {
                        { value.difficulty, new Dictionary<string, Save_Data> { { value.progression_id, value } } }
                    });
                    }
                }
                else
                {
                    for (int i = 0; i < chapter_count; i++)
                    {
                        string chapter_key = reader.ReadString();
                        Dictionary<Difficulty_Modes, Dictionary<string, Save_Data>> chapter =
                            new Dictionary<Difficulty_Modes, Dictionary<string, Save_Data>>();

                        int count = reader.ReadInt32();
                        for (int j = 0; j < count; j++)
                        {
                            Difficulty_Modes key = (Difficulty_Modes)reader.ReadInt32();
                            Dictionary<string, Save_Data> value = read_chapter(reader);
                            chapter.Add(key, value);
                        }
                        old_data.Add(chapter_key, chapter);
                    }
                }
                // Select the newer save data for each chapter?
                result.Data = new Dictionary<string, Dictionary<string, Save_Data>>();
                foreach (var pair in old_data)
                {
                    result.Data.Add(pair.Key, new Dictionary<string, Save_Data>());
                    // Get all progression ids
                    var progression_ids = pair.Value.SelectMany(y => y.Value.Select(x => x.Key))
                        .Distinct()
                        .ToList();
                    foreach(string progression in progression_ids)
                    {
                        var chapter_dataset = pair.Value
                            .Where(x => x.Value.ContainsKey(progression))
                            .Select(x => x.Value[progression])
                            // Find the newest
                            .OrderByDescending(x =>
                                {
                                    if (x.difficulty != Difficulty_Modes.Normal)
                                    { }
                                    // Add 5 minutes if the save is a on hard mode,
                                    //     to account for hard saving before normal
                                    int extra_minutes =
                                        x.difficulty == Difficulty_Modes.Normal ? 0 : 5;
                                    return x.time + new TimeSpan(0, extra_minutes, 0);

                                })
                            .ToList();
                        if (chapter_dataset.Count > 1 && !(
                            chapter_dataset[0].time.Date == chapter_dataset[1].time.Date &&
                            chapter_dataset[0].time.Hour == chapter_dataset[1].time.Hour &&
                            chapter_dataset[0].time.Minute == chapter_dataset[1].time.Minute))
                        { }
                        result.Data[pair.Key].Add(progression, chapter_dataset.First());
                    }
                }
            }
            else
            {
                for (int i = 0; i < chapter_count; i++)
                {
                    string chapter_key = reader.ReadString();
                    Dictionary<string, Save_Data> value = read_chapter(reader);
                    result.Data.Add(chapter_key, value);
                }
            }
            return result;
        }
        private static Dictionary<string, Save_Data> read_chapter(BinaryReader reader)
        {
            Dictionary<string, Save_Data> result = new Dictionary<string, Save_Data>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                Save_Data value = Save_Data.read(reader);
                result.Add(key, value);
            }
            return result;
        }
        #endregion

        #region Accessors
        //public Dictionary<string, Dictionary<string, Save_Data>> data { get { return Data; } }
        private int Count { get { return Data.Count; } }
        public bool NoData { get { return this.Count == 0; } }
        internal Save_Data most_recent_save
        {
            get
            {
                return recent_save();
            }
        }

        internal Dictionary<string, int> acquired_supports
        {
            get
            {
                Dictionary<string, int> result = new Dictionary<string, int>();
                var saves = this.ProgressionDataChapters.ToList();
                foreach (Save_Data save in saves)
                {
                    Dictionary<string, int> supports = save.acquired_supports;
                    foreach (var pair in supports)
                    {
                        if (!result.ContainsKey(pair.Key))
                            result.Add(pair.Key, 0);
                        result[pair.Key] = Math.Max(result[pair.Key], supports[pair.Key]);
                    }
                }
                return result;
            }
        }

        internal HashSet<int> RecruitedActors
        {
            get
            {
                HashSet<int> result = new HashSet<int>();
                var saves = this.ProgressionDataChapters.ToList();
                foreach (Save_Data save in saves)
                {
                    result.UnionWith(save.RecruitedActors);
                }
                return result;
            }
        }

        private IEnumerable<Save_Data> ProgressionDataChapters
        {
            get
            {
                HashSet<string> progressionIds = new HashSet<string>(Save_Progress.ProgressionDataChapterIds);

                foreach (Save_Data save in Data
                    .SelectMany(p => p.Value)
                    .Select(p => p.Value))
                {
                    if (progressionIds.Contains(save.chapter_id))
                        yield return save;
                }
            }
        }
        #endregion

        public Save_File()
        {
            Description = "";
        }

        internal void save_data(string chapter_id, string progression_id, PastRankings rankings)
        {
            if (!Data.ContainsKey(chapter_id))
                Data[chapter_id] = new Dictionary<string, Save_Data>();

            Data[chapter_id][progression_id] = new Save_Data();
            Data[chapter_id][progression_id].save_data(chapter_id, progression_id, rankings);
        }

        public Dictionary<string, Game_Ranking> all_rankings(
            string chapterId)
        {
            if (!ContainsKey(chapterId))
                return null;

            var save = recent_save(chapterId, "");
            if (save == null)
                return null;

            var rankings = Save_Data.process_past_ranking(
                new List<Save_Data> { save });

            return rankings.GetData();
        }

        internal PastRankings past_rankings(
            string chapter_id, Dictionary<string, string> previousChapterIds)
        {
            List<Save_Data> previous_chapters = new List<Save_Data>();
            List<string> previous_ranking_progression_ids = new List<string>(
                Global.data_chapters[chapter_id].Prior_Chapters);
            // If using a subset of prior chapters for rankings
            if (Global.data_chapters[chapter_id].Prior_Ranking_Chapters.Count != 0)
                previous_ranking_progression_ids = new List<string>(
                    previous_ranking_progression_ids.Intersect(
                        Global.data_chapters[chapter_id].Prior_Ranking_Chapters));

            if (previous_ranking_progression_ids.Count > 0)
            {
                for (int i = 0; i < previous_ranking_progression_ids.Count; i++)
                {
                    string progression_id = previous_ranking_progression_ids[i];
                    if (previousChapterIds.ContainsKey(progression_id))
                    {
                        string previous_chapter_id = previousChapterIds[progression_id];
                        if (Data.ContainsKey(previous_chapter_id) &&
                                Data[previous_chapter_id].ContainsKey(progression_id))
                            previous_chapters.Add(Data[previous_chapter_id][progression_id]);
#if DEBUG
                        else
                            Debug.Assert(string.IsNullOrEmpty(previous_chapter_id));
#endif
                    }
                    else
                    {
                        Save_Data previous_data = recent_save(progression_id);
#if DEBUG
                        Debug.Assert(previous_data != null);
#endif
                        previous_chapters.Add(previous_data);
                    }
                }
            }

            return Save_Data.process_past_ranking(previous_chapters);
        }

        public void load_data(string chapter_id,
            Dictionary<string, string> previous_chapters, string progression_id)
        {
            FEXNA_Library.Data_Chapter chapter = Global.data_chapters[chapter_id];
            if (chapter.Prior_Chapters.Count > 0)
            {
#if DEBUG
                foreach (var previous_chapter in previous_chapters)
                {
                    // Keys for Data are progression ids
                    Debug.Assert(Data.ContainsKey(previous_chapter.Value),
                        string.Format(
                            "No save data for \"{0}\", the previous chapter of {1}",
                            previous_chapter.Value, chapter_id));

                    if (chapter.Prior_Chapters.Contains(previous_chapter.Key))
                    {
                        // This doesn't seem formed correctly, why is Prior_Chapters[0] required? //Yeti
                        Debug.Assert(Data[previous_chapter.Value].ContainsKey(
                                previous_chapter.Key),
                            string.Format(
                                "Chapter \"{0}\" doesn't have save data for progression id {1}",
                                previous_chapter.Value,
                                previous_chapter.Key));
                    }
                }
#endif
            }
            // An list of the battalions of the chapters being loaded from, in order with the last being the most important
            List<int> previous_chapter_battalions = new List<int>(), completed_chapter_battalions = new List<int>();
            Dictionary<int, Save_Data> battalion_chapters = new Dictionary<int, Save_Data>();

            for (int i = 0; i < chapter.Prior_Chapters.Count; i++)
            {
                string prior_id = chapter.Prior_Chapters[i];
                Save_Data data;
                if (previous_chapters.ContainsKey(prior_id))
                    data = Data[previous_chapters[prior_id]][prior_id];
                else
                    data = recent_save(prior_id);
                //int battalion = Global.data_chapters.Values.Single(x => x.Id == data.chapter_id).Battalion; //Debug
                int battalion = Global.data_chapters[data.chapter_id].Battalion;

                // If no data for this chapter's battalion yet
                if (!battalion_chapters.ContainsKey(battalion))
                {
                    battalion_chapters[battalion] = data;
                    // Insert instead of add so the first added data will be at the end of the list, and iterated last below
                    previous_chapter_battalions.Insert(0, battalion);
                }
            }
            for (int i = 0; i < chapter.Completed_Chapters.Count; i++)
            {
                string prior_id = chapter.Completed_Chapters[i];
                Save_Data data;
                // If there's specific previous chapter data, load that
                if (previous_chapters.ContainsKey(prior_id))
                {
                    data = Data[previous_chapters[prior_id]][prior_id];
                }
                // Otherwise default to the most recent valid save
                else
                    data = recent_save(prior_id);

                //int battalion = Global.data_chapters.Values.Single(x => x.Id == data.chapter_id).Battalion; //Debug
                int battalion = Global.data_chapters[data.chapter_id].Battalion;

                // If no data for this chapter's battalion yet
                if (!battalion_chapters.ContainsKey(battalion))
                {
                    battalion_chapters[battalion] = data;
                    completed_chapter_battalions.Insert(0, battalion);
                }
            }

            int chapter_battalion = chapter.Battalion;
            Save_Data.reset_old_data();
            // Load system and event data from each previous file
            foreach (int battalion in previous_chapter_battalions)
                battalion_chapters[battalion].load_data();
            foreach (int battalion in previous_chapter_battalions.Take(previous_chapter_battalions.Count - 1))
                battalion_chapters[battalion].load_event_data(false);
            // I need to load event data from completed chapters, to check what gaidens other lords took and such //Debug
            // Actually no if you want event data from a previous chapter you need to use Prior Chapters
            //foreach (int battalion in completed_chapter_battalions) //Debug
            //    battalion_chapters[battalion].load_event_data(false);
            // Load all actors from each save
            foreach (int battalion in completed_chapter_battalions)
                battalion_chapters[battalion].load_actors();
            foreach (int battalion in previous_chapter_battalions)
                battalion_chapters[battalion].load_actors();
            // Then load only battalion actors from every save, overwriting data actors might have on routes where they aren't PCs
            foreach (int battalion in completed_chapter_battalions)
                battalion_chapters[battalion].load_battalion(battalion);
            foreach (int battalion in previous_chapter_battalions)
                battalion_chapters[battalion].load_battalion(battalion);


            if (!Global.game_battalions.ContainsKey(chapter_battalion))
            {
                Global.game_battalions.add_battalion(chapter_battalion);
            }
            Global.game_battalions.current_battalion = chapter_battalion;
        }

        public string displayed_rank(string chapter_id)
        {
            if (Global.data_chapters[chapter_id].Unranked)
                return "+";

            var rank = ranking(chapter_id);
            if (rank == null)
                return "";

            return rank.rank;
        }
        public Game_Ranking ranking(string chapter_id)
        {
            var recent_chapter = recent_save(chapter_id, "");
            if (recent_chapter == null)
                return null;
            return recent_chapter.ranking;
        }

        public Maybe<Difficulty_Modes> displayed_difficulty(string chapter_id)
        {
            if (Global.data_chapters[chapter_id].Unranked)
                return Maybe<Difficulty_Modes>.Nothing;

            var recent_chapter = recent_save(chapter_id, "");
            if (recent_chapter == null)
                return Difficulty_Modes.Normal;
            return recent_chapter.difficulty;
        }

        public List<string> available_chapters()
        {
            return Global.data_chapters
                .Where(x => chapter_available(x.Key))
                .Select(x => x.Key)
                .ToList();
        }

        /// <summary>
        /// Checks if a chapter is available to play. Returns true if the chapter is open.
        /// </summary>
        /// <param name="chapter_id">The key of the chapter to check</param>
        public bool chapter_available(string chapter_id)
        {
            var chapter = Global.data_chapters[chapter_id];
            if (chapter.Prior_Chapters.Count > 0)
                // If the chapter has prior chapters, to be viable it has to have save data to load from those chapters
                for (int i = 0; i < chapter.Prior_Chapters.Count; i++)
                {
                    if (!has_progression_key(chapter.Prior_Chapters[i]))
                        return false;

                    // Followup chapters must have the same battalion
                    // in their first prior chapter
                    if (!chapter.Standalone && i == 0)
                    {
                        bool sameBattalion = has_progression_key(chapter.Prior_Chapters[i],
                            //Global.data_chapters.Values.Single(x => x.Id == chapter_id).Battalion)) //Debug
                            chapter.Battalion);
#if DEBUG
                        Debug.Assert(sameBattalion, string.Format(
@"Invalid prior chapter for ""{0}"".

As a follow-up chapter, the first prior
chapter must use the same battalion.

No completed chapter with the
progression id ""{1}"" uses battalion {2}.",
                            chapter.ShortName,
                            chapter.Prior_Chapters[0],
                            chapter.Battalion));
#endif
                        if (!sameBattalion)
                            return false;
                    }
                }

            // Completed chapters require only that the chapter has been beaten
            for (int i = 0; i < chapter.Completed_Chapters.Count; i++)
            {
                if (!has_progression_key(chapter.Completed_Chapters[i]))
                    return false;
            }

            return true;
        }

        private bool has_progression_key(string progression_id, int battalion = -1)
        {
            foreach (var chapter in Data)
                if (has_progression_key(chapter.Key, progression_id, battalion))
                    return true;
            return false;
        }
        private bool has_progression_key(string chapter, string progression_id, int battalion = -1)
        {
            if (Data[chapter].ContainsKey(progression_id))
            {
                if (battalion != -1)
                {
                    //if (Global.data_chapters.Values.Single(x => x.Id == chapter).Battalion == battalion) //Debug
                    if (Global.data_chapters[chapter].Battalion == battalion)
                        return true;
                }
                else
                    return true;
            }
            return false;
        }

        /* //Debug
        public static bool chapter_available(string chapter_id, Dictionary<string, List<string>> completedChapters)
        {
            if (Global.data_chapters[chapter_id].Prior_Chapters.Count > 0)
                // If the chapter has prior chapters, to be viable it has to have save data to load from those chapters
                for (int i = 0; i < Global.data_chapters[chapter_id].Prior_Chapters.Count; i++)
                {
                    // The first entry must have the same battalion
                    if (i == 0)
                    {
                        if (!has_progression_key(Global.data_chapters[chapter_id].Prior_Chapters[i],
                                completedChapters,
                                Global.data_chapters[chapter_id].Battalion))
                            return false;
                    }
                    else
                        if (!has_progression_key(Global.data_chapters[chapter_id].Prior_Chapters[i],
                                completedChapters))
                            return false;
                }

            // Completed chapters require only that the chapter has been beaten, on any difficulty
            for (int i = 0; i < Global.data_chapters[chapter_id].Completed_Chapters.Count; i++)
            {
                if (!has_progression_key(Global.data_chapters[chapter_id].Completed_Chapters[i],
                        completedChapters))
                    return false;
            }

            return true;
        }

        private static bool has_progression_key(string progression_id, Dictionary<string, List<string>> completedChapters, int battalion = -1)
        {
            foreach (var chapter in completedChapters)
            {
                if (battalion != -1)
                {
                    if (Global.data_chapters[chapter.Key].Battalion != battalion)
                        continue;
                }
                if (chapter.Value.Contains(progression_id))
                    return true;
            }
            return false;
        }
        */

        public List<string> progression_ids(string chapter)
        {
            if (!Data.ContainsKey(chapter))
                return new List<string>();
            return new List<string>(Data[chapter].Keys);
        }

        public static List<string> previous_chapters(string chapter_id)
        {
            if (!Global.data_chapters[chapter_id].Prior_Chapters.Any())
                return new List<string>();
            string progression_id = Global.data_chapters[chapter_id].Prior_Chapters[0];
            //int battalion = Global.data_chapters.Values.Single(x => x.Id == chapter_id).Battalion; //Debug
            int battalion = Global.data_chapters[chapter_id].Battalion;

            List<string> result = Global.data_chapters.Values
                .Where(x => x.Progression_Ids.Contains(progression_id) && x.Battalion == battalion)
                .Select(x => x.Id).ToList();
            return result;
        }
        public Dictionary<string, List<string>> valid_previous_chapters(string chapter_id)
        {
            var chapter = Global.data_chapters[chapter_id];
            Dictionary<string, List<string>> previous_chapters = new Dictionary<string, List<string>>();
            // Prior Chapters
            for (int i = 0; i < chapter.Prior_Chapters.Count; i++)
            {
                string progression_id = chapter.Prior_Chapters[i];
                // Duplicate; already added the set of chapters for this id
                if (previous_chapters.ContainsKey(progression_id))
                    continue;
                int battalion = -1;
                if (!chapter.Standalone && i == 0)
                    battalion = chapter.Battalion;
                List<string> result = get_previous_chapters(
                    chapter_id, progression_id, battalion);
                previous_chapters.Add(progression_id, result);
            }
            // If no prior chapters, don't have the player select completed chapters
            // If there's nothing important to load 
            if (previous_chapters.Count == 0)
                return previous_chapters;
            // Completed Chapters
            for (int i = 0; i < chapter.Completed_Chapters.Count; i++)
            {
                string progression_id = chapter.Completed_Chapters[i];
                if (previous_chapters.ContainsKey(progression_id))
                    continue;
                int battalion = -1;

                List<string> result = get_previous_chapters(
                    chapter_id, progression_id, battalion);

                // Don't bother selecting from completed chapters if all of them are unranked
                if (result.All(x => Global.data_chapters[x].Unranked))
                    continue;

                previous_chapters.Add(progression_id, result);
            }
            return previous_chapters;
        }

        private List<string> get_previous_chapters(string chapter_id, string progression_id, int battalion)
        {
            List<string> result = new List<string>(
                // Get keys from the data where the matching value has the given progression id
                Data.Keys.Where(key => has_progression_key(key, progression_id, battalion)));

            foreach (var chapter in Data)
                if (has_progression_key(chapter.Key, progression_id, battalion))
                { }//result.Add(chapter.Key);

#if DEBUG
            if (result.Count == 0 && !Global.data_chapters[chapter_id].Standalone)
                if (!Global.UnitEditorActive)
                {
                    //throw new ArgumentException(string.Format("Trying to get previous chapters for \"{0}\"\nwhen no valid saves exist", chapter_id));
                    Print.message(string.Format(
                        "Trying to get previous chapters for\n" +
                        "\"{0}\" when no valid saves exist",
                        chapter_id));
                }
#endif
            // Sort the chapters by their order in the chapter list
            result.Sort(delegate(string a, string b)
            {
                return Global.Chapter_List.IndexOf(a) - Global.Chapter_List.IndexOf(b);
            });
            return result;
        }

        public bool ContainsKey(string key)
        {
            return Data.ContainsKey(key);
        }
        public bool ContainsKey(string key, Difficulty_Modes difficulty)
        {
            return Data.ContainsKey(key) && Data[key].Any(x => x.Value.difficulty == difficulty);
        }

        private Save_Data recent_save(string progression_id = "")
        {
            if (this.NoData)
                return null;

            DateTime time = new DateTime();
            int index = -1;
            List<string> indices = Data.Keys.ToList();
            for (int i = indices.Count - 1; i >= 0; i--)
            {
                foreach (var pair in Data[indices[i]])
                {
                    if (string.IsNullOrEmpty(progression_id) || pair.Key == progression_id)
                    {
                        DateTime temp_time = pair.Value.time;
                        if (index == -1 || temp_time > time)
                        {
                            index = i;
                            time = temp_time;
                        }
                    }
                }
            }
            if (index == -1)
                return null;
            return recent_save(indices[index], progression_id);
        }
        private Save_Data recent_save(string chapter_id, string progression_id)
        {
            if (this.NoData)
                return null;

            DateTime time = new DateTime();
            int index = -1;
            List<string> indices = Data[chapter_id].Keys.ToList();
            for (int i = indices.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(progression_id) || indices[i] == progression_id)
                {
                    DateTime temp_time = Data[chapter_id][indices[i]].time;
                    if (index == -1 || temp_time > time)
                    {
                        index = i;
                        time = temp_time;
                    }
                }
            }
            if (index == -1)
                return null;

            return Data[chapter_id][indices[index]];
        }

        internal string lord_map_sprite(string progressionId, string chapterId)
        {
            if (!ContainsKey(chapterId))
                return "";

            var chapter_data = Data[chapterId];
            if (!chapter_data.ContainsKey(progressionId))
                return "";

            var data = chapter_data[progressionId];
            int lord_id = Global.data_chapters[chapterId].World_Map_Lord_Id;
            return data.actor_map_sprite(lord_id);
        }
    }
}
