using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEXNA.IO
{
    class Save_Info
    {
        protected int File_Id;
        protected DateTime Time;
        protected string Chapter_Id;
        protected Difficulty_Modes Difficulty;
        protected Mode_Styles Style;
        protected bool Map_Save_Exists;
        protected bool Suspend_Exists;
        protected List<string> AvailableChapters;

        public string LastStartedChapter { get; private set; }
        public DateTime LastStartTime { get; private set; }

        #region Accessors
        public int file_id { get { return File_Id; } }
        public DateTime time { get { return Time; } }
        public string chapter_id { get { return Chapter_Id; } }
        public Difficulty_Modes difficulty { get { return Difficulty; } }
        public Mode_Styles style { get { return Style; } }
        internal bool map_save_exists { get { return Map_Save_Exists; } set { Map_Save_Exists = value; } }
        internal bool suspend_exists { get { return Suspend_Exists; } set { Suspend_Exists = value; } }
        #endregion

        private Save_Info() { }
        public Save_Info(Save_Info info)
        {
            File_Id = info.File_Id;
            Time = info.Time;
            Chapter_Id = info.Chapter_Id;
            Difficulty = info.Difficulty;
            Style = info.Style;
            Map_Save_Exists = info.Map_Save_Exists;
            Suspend_Exists = info.Suspend_Exists;
            AvailableChapters = new List<string>(info.AvailableChapters);
        }

        public override string ToString()
        {
            return string.Format("Save Info: Id {0}, Difficulty {1}",
                File_Id, Difficulty);
        }

        public static Save_Info get_save_info(int file_id, Save_File file, bool suspend)
        {
            Save_Info result = new Save_Info();
            result.File_Id = file_id;
            
            result.Time = new DateTime(); //@Yeti
            var mostRecent = file.most_recent_save;
            if (mostRecent != null)
                result.Time = mostRecent.time;

            result.Style = file.Style;
            result.Map_Save_Exists = false;
            result.Suspend_Exists = suspend;

            if (file.NoData)
            {
                result.Chapter_Id = "";
                result.Difficulty = file.Difficulty;
            }
            else
            {
                Save_Data data = file.most_recent_save;
                result.Chapter_Id = data.chapter_id;
                result.Difficulty = data.difficulty;
                //result.Chapter_Id = file.data[chapter_id][indices[index]].chapter_id; //Debug
                //result.Difficulty = file.data[chapter_id][indices[index]].difficulty;
            }

            result.get_save_info(file);
            return result;
        }
        public static Save_Info get_save_info(int file_id, Save_File file, Suspend_Info suspend_info, bool map_save = false, bool suspend = false)
        {
            Save_Info result = new Save_Info();
            result.File_Id = file_id;

            result.Time = suspend_info.time;
            result.Style = suspend_info.style;
            result.Map_Save_Exists = map_save;
            result.Suspend_Exists = suspend;

            result.Chapter_Id = suspend_info.chapter_id;
            result.Difficulty = suspend_info.difficulty;

            result.get_save_info(file);
            return result;
        }

        private void get_save_info(Save_File file)
        {
            AvailableChapters = file.available_chapters();
        }

        public IEnumerable<string> available_chapters(Difficulty_Modes difficulty)
        {
            return new List<string>(AvailableChapters);
        }

        public bool chapter_available(string chapterId)
        {
            return AvailableChapters.Contains(chapterId);
        }

        public static Save_Info new_file()
        {
            Save_Info result = new Save_Info();
            result.File_Id = Global.start_game_file_id;
            result.Difficulty = Global.save_file.Difficulty;
            result.Style = Global.save_file.Style;
            result.Map_Save_Exists = false;
            result.Suspend_Exists = false;
            return result;
        }

        public void reset_suspend_exists()
        {
            Map_Save_Exists = false;
            Suspend_Exists = false;
        }

        #region Transient Info
        public void CopyTransientInfo(Save_Info sourceInfo)
        {
            if (!string.IsNullOrEmpty(sourceInfo.LastStartedChapter))
                LastStartedChapter = sourceInfo.LastStartedChapter;

            if (sourceInfo.LastStartTime != new DateTime())
            {
                if (sourceInfo.LastStartTime > LastStartTime)
                    LastStartTime = sourceInfo.LastStartTime;

                if (sourceInfo.LastStartTime > Time)
                    Time = sourceInfo.LastStartTime;
            }
        }

        public void SetStartedChapter(string newChapter)
        {
            LastStartedChapter = newChapter;
        }
        public void ResetStartedChapter()
        {
            LastStartedChapter = null;
        }

        public void SetStartTime()
        {
            LastStartTime = DateTime.Now;
        }
        public void ResetStartTime()
        {
            LastStartTime = new DateTime();
        }
        #endregion
    }
}
