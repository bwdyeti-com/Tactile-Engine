using System.Collections.Generic;
using System.Linq;
using FEXNA_Library;

namespace FEXNA.Menus.Worldmap
{
    class WorldmapMenuData
    {
        public bool Classic { get; private set; }
        public int Index;
        private Dictionary<string, List<string>> CompletedChapters =
            new Dictionary<string, List<string>>();
        private List<int> UnlockedChapters = new List<int>();
        private string CompletedChapterId;

        private int ActiveArc = 0;
        protected List<int>[] IndexRedirects;

        public List<int> IndexRedirect { get { return IndexRedirects[ActiveArc]; } }
        public bool MultipleArcs { get { return IndexRedirects.Length > 1; } }

        public Data_Chapter Chapter { get { return Global.chapter_by_index(Index); } }
        public string ChapterId { get { return Global.Chapter_List[Index]; } }

        private Dictionary<string, int> PreviousChapterIndices =
            new Dictionary<string, int>();
        public Dictionary<string, List<string>> ValidPreviousChapters =
            new Dictionary<string, List<string>>();

        public Dictionary<string, int> UsablePreviousChapterIndices
        {
            get
            {
                return PreviousChapterIndices.ToDictionary(
                    p => p.Key, p => p.Value >= 0 ?
                        p.Value : ValidPreviousChapters[p.Key].Count - 1);
            }
        }

        public WorldmapMenuData(string completedChapterId)
        {
            CompletedChapterId = completedChapterId;
            Global.game_system.Difficulty_Mode = Global.save_file.Difficulty;
            Classic = Global.save_file.Style == Mode_Styles.Classic;
            List<int> availableChapters = GetChapters();
            GetArcs(availableChapters);

            JumpToDefaultChapter(availableChapters);
        }

        private List<int> GetChapters()
        {
            // If no chapters have been played at all, then treat the file as classic mode to attempt to automatically select the first chapter
            if (!Classic && Global.save_file.NoData)
                Classic = true;

            Index = 0;
            GetCompletedChapters();
            GetViableChapters();

            List<int> chapters;
            if (Classic)
                chapters = GetClassicChapterSubset(UnlockedChapters);
            else
                chapters = new List<int>(UnlockedChapters);

            // Selects the chapter in Classic mode
            if (Classic)
            {
                // If no chapters have been played and there are multiple choices,
                // cancel classic because the player needs to select their first chapter
                if (!CompletedChapters.Any() && chapters.Count > 1)
                {
                    Classic = false;
                }
                // If save data exists for all chapters, turn off classic
                else if (!Classic || chapters.All(x => CompletedChapters.ContainsKey(Global.Chapter_List[x])))
                    Classic = false;
                else
                {
                    // If there's more than one chapter that can be selected
                    // First remove chapters that have no previous chapter at all, if possible
                    if (chapters.Count != 1)
                        if (chapters.Except(
                                Global.data_chapters
                                    .Where(x => x.Value.no_previous_chapters())
                                    .Select(x => Global.Chapter_List.IndexOf(x.Key))).Any())
                            chapters = chapters.Except(
                                Global.data_chapters
                                    .Where(x => x.Value.no_previous_chapters())
                                    .Select(x => Global.Chapter_List.IndexOf(x.Key))).ToList();
                    // Then select from among chapters that have followup chapters, because if they do they must be part of the main game right?
                    List<int> continuable_chapters = new List<int>();
                    if (chapters.Count != 1)
                        for (int i = 0; i < chapters.Count; i++)
                        {
                            foreach (Data_Chapter possible_followup_chapter in Global.data_chapters.Values)
                                // If any chapter has the tested chapter as a previous chapter
                                if (possible_followup_chapter.get_previous_chapters(Global.data_chapters)
                                    .Contains(Global.Chapter_List[chapters[i]]))
                                {
                                    continuable_chapters.Add(i);
                                    break;
                                }
                        }
                    // If multiple chapters have valid followups, cancel Classic because the player needs to select one
                    if (chapters.Count != 1 && continuable_chapters.Count > 1)
                        Classic = false;
                    else
                    {
                        if (chapters.Count == 1)
                            Index = 0;
                        else
                            Index = continuable_chapters[0];
                    }
                }
            }

            return chapters;
        }

        protected virtual void GetArcs(List<int> availableChapters)
        {
            IndexRedirects = new List<int>[] { new List<int>(availableChapters) };
        }

        private bool ChapterAvailable(string chapterId, List<int> availableChapters)
        {
            return availableChapters.Contains(
                Global.Chapter_List.IndexOf(chapterId));
        }

        private void GetCompletedChapters()
        {
            CompletedChapters.Clear();
            for (int i = 0; i < Global.Chapter_List.Count; i++)
            {
                if (Global.save_file.ContainsKey(Global.Chapter_List[i]))
                    CompletedChapters.Add(Global.Chapter_List[i],
                        Global.save_file.progression_ids(Global.Chapter_List[i]));
            }
        }
        private void GetViableChapters()
        {
            UnlockedChapters.Clear();
            for (int i = 0; i < Global.Chapter_List.Count; i++)
            {
                // If there is save data for a standalone chapter it must be playable
                if (Global.data_chapters[Global.Chapter_List[i]].Standalone &&
                        Global.save_file.ContainsKey(Global.Chapter_List[i]))
                    UnlockedChapters.Add(i);
                // If all prior chapters have been completed
                else if (Global.save_file.chapter_available(Global.Chapter_List[i]))
                    UnlockedChapters.Add(i);
            }
        }

        private List<int> GetClassicChapterSubset(List<int> unlockedChapters)
        {
            List<int> chapters = new List<int>();
            // Given a list of unlocked chapters gets the chapters available for the player to select from in Classic style
            // Optimally this list is one element long and automatically selected

            foreach (int i in unlockedChapters)
            {
                // If all prior chapters have been completed
                if (Global.save_file.chapter_available(Global.Chapter_List[i]))
                    // If either it's not classic mode, or it is classic mode but there's no save data for the chapter
                    if (!(Classic && Global.save_file.ContainsKey(Global.Chapter_List[i])))
                        chapters.Add(i);
            }

            HashSet<int> skipped_gaidens = GetSkippedGaidens(chapters);

            bool classic = true;
            // If all we have left are skipped gaidens, load every chapter up because they beat the game
            if (!chapters.Except(skipped_gaidens).Any())
            {
                chapters.Clear();
                classic = false;
            }
            else
            {
                chapters = chapters.Except(skipped_gaidens).ToList();
                skipped_gaidens.Clear();
            }

            // If no chapters are valid for classic mode, reload chapters with classic off to try to find anything playable
            if (!chapters.Any())
            {
                Classic = false;
                chapters = new List<int>(unlockedChapters);
                Classic = classic;
            }
            else
            {
                // If only game starting chapters, add all of them that are viable and not beaten
                if (chapters.All(x => Global.data_chapters[Global.Chapter_List[x]].no_previous_chapters()))
                {
                    chapters = Global.data_chapters
                        .Where(x => x.Value.no_previous_chapters() &&
                            Global.save_file.chapter_available(x.Key))
                        .Select(x => Global.Chapter_List.IndexOf(x.Key)).ToList();
                }
            }

            return chapters;
        }

        private HashSet<int> GetSkippedGaidens(List<int> chapters)
        {
            // If any other chapters were unlocked at the same time as a chapter
            // and any of those others are cleared,
            // they're probably a gaiden that was skipped intentionally
            HashSet<int> skipped_gaidens = new HashSet<int>(chapters.Where(i =>
            {
                // Gets all chapters that unlocked this chapter
                var previous = Global.data_chapters[Global.Chapter_List[i]].get_previous_chapters(Global.data_chapters);
                // Gets all chapters that follow from the previous chapters
                var followups = previous
                    .Select(x => Global.data_chapters[x].get_followup_chapters(Global.data_chapters))
                    .ToList();
                // If every previous chapter has one of their followup chapters completed
                return followups.Any() && followups.All(x => x.Any(y => Global.save_file.ContainsKey(y)));
            }));
            return skipped_gaidens;
        }

        public void SetChapter(string chapterId)
        {
            int chapterIndex = Global.Chapter_List.IndexOf(chapterId);
            SetChapter(chapterIndex);
        }
        private void SetChapter(int chapterIndex)
        {
            ActiveArc = IndexRedirects.ToList().FindIndex(x => x.Contains(chapterIndex));
            Index = chapterIndex;
        }

        private void JumpToDefaultChapter(List<int> availableChapters)
        {
            // If returning to the world map after beating a chapter
            if (!string.IsNullOrEmpty(CompletedChapterId))
            {
                // Go to the chapter it unlocks
                IEnumerable<string> followupChapters = 
                    Global.data_chapters[CompletedChapterId]
                        .get_followup_chapters(Global.data_chapters);
                // Remove followup chapters that aren't unlocked
                followupChapters = followupChapters
                    .Where(x => ChapterAvailable(x, availableChapters))
                    .ToList();
                // If possible, remove already completed chapters
                var withoutCompleted = followupChapters
                    .Where(x => !Global.save_file.ContainsKey(x))
                    .ToList();
                if (withoutCompleted.Any())
                    followupChapters = withoutCompleted;

                if (followupChapters.Any())
                {
                    string nextChapter = followupChapters.First();
                    SetChapter(nextChapter);
                    return;
                }
                // If no followup is available, jump to the completed chapter
                else if (ChapterAvailable(CompletedChapterId, availableChapters))
                {
                    SetChapter(CompletedChapterId);
                    return;
                }
            }

            // If there is a suspend for this file
            if (Global.current_save_info.suspend_exists)
            {
                // Go to the chapter of the suspend
                string suspendChapter = Global.suspend_files_info[
                    Global.current_save_id].chapter_id;
                if (ChapterAvailable(suspendChapter, availableChapters))
                {
                    SetChapter(suspendChapter);
                    return;
                }
            }

            // If a chapter has been started in this file since launching the
            // game, and no chapter has been beaten since then
            if (!string.IsNullOrEmpty(Global.current_save_info.LastStartedChapter))
            {
                // Go to that chapter
                if (ChapterAvailable(
                    Global.current_save_info.LastStartedChapter, availableChapters))
                {
                    SetChapter(Global.current_save_info.LastStartedChapter);
                    return;
                }
            }

            // Jump  to the first unplayed chapter
            JumpToFirstChapter(availableChapters);
        }

        private void JumpToFirstChapter(List<int> availableChapters)
        {
            // Jumps to the first unplayed chapter
            if (!Classic)
            {
                Maybe<int> index = FirstUnplayedChapter(availableChapters);
                if (index.IsSomething && IndexRedirects.Any(x => x.Contains(index)))
                {
                    SetChapter(index);
                }
                else
                {
                    ActiveArc = 0;
                    Index = 0;
                }
            }
        }

        private Maybe<int> FirstUnplayedChapter(List<int> availableChapters)
        {
            // If any chapters haven't been played yet, jump to the first one on the list
            var non_completed_chapters = availableChapters
                .Where(x => !Global.save_file.ContainsKey(Global.Chapter_List[x]))
                .ToList();
            if (non_completed_chapters.Any())
            {
                var skipped_gaidens = GetSkippedGaidens(availableChapters);
                // Remove chapters with completed followups, if there are any other chapters
                if (skipped_gaidens.Any() && non_completed_chapters.Except(skipped_gaidens).Any())
                    non_completed_chapters = non_completed_chapters.Except(skipped_gaidens).ToList();
                return non_completed_chapters.First();
            }
            return Maybe<int>.Nothing;
        }

        public virtual bool HardModeEnabled(int index)
        {
            var chapter = Global.chapter_by_index(index);
            if (chapter.Standalone && chapter.Prior_Chapters.Count == 0)
                return true;

            return Global.save_file.chapter_available(Global.Chapter_List[index]);
        }

        public void RefreshPreviousChapters(string redirectChapterId)
        {
            PreviousChapterIndices.Clear();
            ValidPreviousChapters.Clear();

            ValidPreviousChapters = Global.save_file.valid_previous_chapters(
                redirectChapterId);
            PreviousChapterIndices = ValidPreviousChapters
                .ToDictionary(p => p.Key, p => p.Value.Count != 1 ? -1 : 0);
        }

        public bool PreviousChapterSelectionIncomplete()
        {
            return PreviousChapterIndices.Any(x => x.Value == -1);
        }

        public void SetPreviousChapterIndices(Dictionary<string, int> previousChapterIndices)
        {
            PreviousChapterIndices = previousChapterIndices;
        }

        public Dictionary<string, string> GetSelectedPreviousChapters()
        {
            Dictionary<string, string> previous_chapters = ValidPreviousChapters
                .ToDictionary(p => p.Key, p =>
                {
                    if (PreviousChapterIndices[p.Key] == -1)
                        return p.Value.Last();
                    return p.Value[PreviousChapterIndices[p.Key]];
                });
            return previous_chapters;
        }

        public void PickDefaultUnselectedPreviouschapters()
        {
            foreach (string key in PreviousChapterIndices.Keys.ToList())
                if (PreviousChapterIndices[key] == -1)
                    PreviousChapterIndices[key] = ValidPreviousChapters[key].Count - 1;
        }

        public void ChangeArc(bool increase)
        {
            Global.game_system.play_se(System_Sounds.Status_Page_Change);
            if (increase)
                ActiveArc = (ActiveArc + 1) % IndexRedirects.Length;
            else
                ActiveArc = (ActiveArc + IndexRedirects.Length - 1) %
                    IndexRedirects.Length;
        }
    }
}
