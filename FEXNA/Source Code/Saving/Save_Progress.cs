using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DictionaryExtension;
using HashSetExtension;
using FEXNAVersionExtension;

namespace FEXNA.IO
{
    class Save_Progress
    {
        private Dictionary<int, HashSet<string>> Completed_Chapters = new Dictionary<int, HashSet<string>>();
        private HashSet<string> Available_Chapters = new HashSet<string>();
        private Dictionary<string, int> Supports = new Dictionary<string, int>();
        private HashSet<int> RecruitedActors = new HashSet<int>();
        private HashSet<string> PlayedBgms = new HashSet<string>();

        #region Accessors
        public Dictionary<string, int> supports
        {
            get { return new Dictionary<string, int>(Supports); }
        }

        public HashSet<int> recruitedActors
        {
            get
            {
#if DEBUG
                //@Debug
                if (false)
                {
                    var recruitedActors = new HashSet<int>(
                        Global.data_supports.Select(x => x.Value.Id1));
                    recruitedActors.UnionWith(
                        Global.data_supports.Select(x => x.Value.Id2));
                    return recruitedActors;
                }
#endif
                return new HashSet<int>(RecruitedActors);
            }
        }
        #endregion

        #region Serialization
        public void write(BinaryWriter writer)
        {
            Completed_Chapters.write(writer);
            Available_Chapters.write(writer);
            Supports.write(writer);
            RecruitedActors.write(writer);
            PlayedBgms.write(writer);
        }

        public static Save_Progress read(BinaryReader reader, Version loaded_version)
        {
            Save_Progress result = new Save_Progress();

            var completed = new Dictionary<int, HashSet<string>>();
            completed.read(reader);
            result.CombineCompleted(completed);

            //if (Global.LOADED_VERSION.older_than(0, 6, 1, 0)) //Debug
            if (loaded_version.older_than(0, 6, 1, 0))
            {
                Dictionary<int, HashSet<string>> available = new Dictionary<int, HashSet<string>>();
                available.read(reader);
                result.Available_Chapters = new HashSet<string>(
                    available.Values.SelectMany(x => x));
            }
            else
            {
                result.Available_Chapters.read(reader);
            }
            result.Supports.read(reader);
            if (!loaded_version.older_than(0, 6, 7, 3))
            {
                result.RecruitedActors.read(reader);
                result.PlayedBgms.read(reader);
            }

            return result;
        }
        #endregion

        public Save_Progress()
        {
            foreach (Difficulty_Modes difficulty in Enum_Values.GetEnumValues(typeof(Difficulty_Modes)))
            {
                Completed_Chapters.Add((int)difficulty, new HashSet<string>());
            }
        }

        public void combine_progress(Save_Progress source_progress)
        {
            // Completed Chapters
            CombineCompleted(source_progress.Completed_Chapters);
            // Available Chapters
            Available_Chapters.UnionWith(source_progress.Available_Chapters);
            // Supports
            foreach (var pair in source_progress.Supports)
            {
                if (!Supports.ContainsKey(pair.Key))
                    Supports.Add(pair.Key, 0);
                Supports[pair.Key] = Math.Max(Supports[pair.Key], source_progress.Supports[pair.Key]);
            }
            // Recruited
            RecruitedActors.UnionWith(source_progress.RecruitedActors);
            // Bgm
            PlayedBgms.UnionWith(source_progress.PlayedBgms);
        }

        private void CombineCompleted(Dictionary<int, HashSet<string>> sourceCompleted)
        {
            foreach (var pair in sourceCompleted)
            {
                if (!Completed_Chapters.ContainsKey(pair.Key))
                    Completed_Chapters.Add(pair.Key, new HashSet<string>());
                Completed_Chapters[pair.Key].UnionWith(sourceCompleted[pair.Key]);
            }
        }

        public void update_progress(Save_File file)
        {
            foreach (Difficulty_Modes difficulty in Enum_Values.GetEnumValues(typeof(Difficulty_Modes)))
            {
                // Completed chapters
                foreach (string chapter_id in Global.data_chapters.Keys)
                    if (file.ContainsKey(chapter_id, difficulty))
                        Completed_Chapters[(int)difficulty].Add(chapter_id);
                // Available chapters
                foreach (string chapter_id in Global.data_chapters.Keys)
                    if (!Available_Chapters.Contains(chapter_id))
                        if (file.chapter_available(chapter_id))
                            Available_Chapters.Add(chapter_id);
            }
            // Supports
            Dictionary<string, int> supports = file.acquired_supports;
            foreach (var pair in supports)
                AddSupport(pair.Key, pair.Value);
            // Recruited
            RecruitedActors.UnionWith(file.RecruitedActors);
        }

        internal void AddSupport(string key, int level)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (!Supports.ContainsKey(key))
                Supports.Add(key, 0);
            Supports[key] = Math.Max(Supports[key], level);
        }

        internal void AddRecruit(int actorId)
        {
            RecruitedActors.Add(actorId);
        }

        public bool ChapterCompleted(string key)
        {
            return Completed_Chapters.Values.Any(x => x.Contains(key));
        }

        public bool ChapterAvailable(string key)
        {
            return Available_Chapters.Contains(key);
        }

        public int SupportPercent
        {
            get
            {
                int max = 0, level = 0;
                foreach (var pair in Global.data_supports)
                {
                    max += pair.Value.MaxLevel;
                    if (Supports.ContainsKey(pair.Key))
                    {
                        if (RecruitedActors.Contains(pair.Value.Id1) &&
                            RecruitedActors.Contains(pair.Value.Id2))
                        {
                            level += Math.Min(Supports[pair.Key], pair.Value.MaxLevel);
                        }
                    }
                }

                return (100 * level) / max;
            }
        }

        internal static IEnumerable<string> ProgressionDataChapterIds
        {
            get
            {
                foreach (string id in Global.Chapter_List)
                {
                    // If the chapter is standalone, there needs to be at least one chapter following from it or it's a trial map/etc and shouldn't be counted
                    if (Global.data_chapters[id].Standalone)
                    {
                        // Compare the prior chapter list for each chapter against the progression ids for the current chapter
                        if (!Global.data_chapters
                                // If this other chapter is standalone it can't follow from this chapter
                                .Any(x => !x.Value.Standalone && x.Value.Prior_Chapters
                                    .Intersect(Global.data_chapters[id].Progression_Ids).Any()))
                            continue;
                    }

                    yield return id;
                }
            }
        }

        #region Extras Menu
        public bool ExtrasAccessible
        {
            get { if (this.SoundRoomAccessible || this.SupportViewerAccessible)
                    return true;
                // If there are no chapters that could ever enable the support viewer,
                // just show extras so that credits can be selected
                if (!Save_Progress.ProgressionDataChapterIds.Any())
                    return true;

                return false;
            }
        }

        public bool SoundRoomAccessible
        {
            get
            {
                return false;
            }
        }

        public bool SupportViewerAccessible
        {
            get
            {
                // If there are any recruited actors, and they have any supports
                if (this.recruitedActors.Any())
                    foreach (int actorId in this.recruitedActors)
                    {
                        var actorData = Global.data_actors[actorId];
                        if (actorData.SupportPartners(Global.data_supports, Global.data_actors).Any())
                            return true;
                    }

                return false;
            }
        }
        #endregion
    }
}
