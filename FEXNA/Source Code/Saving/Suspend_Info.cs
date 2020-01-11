using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using FEXNAVersionExtension;
using ArrayExtension;

namespace FEXNA.IO
{
    class Suspend_Info : ICloneable
    {
        private string Chapter_Id;
        private string Lord_Actor_Face;
        private int Turn;
        private int Units;
        private int Playtime;
        private int Gold;
        private int Save_Id;
        private bool Preparations;
        private bool HomeBase;
        private Difficulty_Modes Difficulty;
        private Mode_Styles Style;
        private DateTime Time;
        private byte[] ScreenshotData = new byte[0];
        internal Texture2D Screenshot { get; private set; }

        internal DateTime SuspendModifiedTime;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Chapter_Id);
            writer.Write(Lord_Actor_Face);
            writer.Write(Turn);
            writer.Write(Units);
            writer.Write(Playtime);
            writer.Write(Gold);
            writer.Write(Save_Id);
            writer.Write(Preparations);
            writer.Write(HomeBase);
            writer.Write((int)Difficulty);
            writer.Write((int)Style);
            writer.Write(Time.ToBinary());
            ScreenshotData.write(writer);
        }

        public static Suspend_Info read(BinaryReader reader)
        {
            Suspend_Info result = new Suspend_Info();

            result.Chapter_Id = reader.ReadString();
            result.Lord_Actor_Face = reader.ReadString();
            result.Turn = reader.ReadInt32();
            result.Units = reader.ReadInt32();
            result.Playtime = reader.ReadInt32();
            result.Gold = reader.ReadInt32();
            result.Save_Id = reader.ReadInt32();
            if (!Global.LOADED_VERSION.older_than(0, 5, 4, 0))
            {
                result.Preparations = reader.ReadBoolean();
                result.HomeBase = reader.ReadBoolean();
            }
            else
            {
                result.Preparations = result.Turn <= 0;
                result.HomeBase = false;
            }
            result.Difficulty = (Difficulty_Modes)reader.ReadInt32();
            result.Style = (Mode_Styles)reader.ReadInt32();
            result.Time = DateTime.FromBinary(reader.ReadInt64());
            if (!Global.LOADED_VERSION.older_than(0, 5, 7, 0))
            {
                result.ScreenshotData = result.ScreenshotData.read(reader);
            }

            return result;
        }
        #endregion

        #region Accessors
        public string chapter_id { get { return Chapter_Id; } }
        public string lord_actor_face { get { return Lord_Actor_Face; } }
        public int turn { get { return Turn; } }
        public int units { get { return Units; } }
        public int playtime { get { return Playtime; } }
        public int gold { get { return Gold; } }
        public int save_id
        {
            get { return Save_Id; }
            set { Save_Id = value; }
        }
        public bool preparations { get { return Preparations; } }
        public bool home_base { get { return HomeBase; } }
        public Difficulty_Modes difficulty { get { return Difficulty; } }
        public Mode_Styles style { get { return Style; } }
        public DateTime time { get { return Time; } }
        #endregion

        public Suspend_Info() { }
        private Suspend_Info(Suspend_Info source)
        {
            Chapter_Id = source.Chapter_Id;
            Lord_Actor_Face = source.Lord_Actor_Face;
            Turn = source.Turn;
            Units = source.Units;
            Playtime = source.Playtime;
            Gold = source.Gold;
            Save_Id = source.Save_Id;
            Preparations = source.Preparations;
            HomeBase = source.HomeBase;
            Difficulty = source.Difficulty;
            Style = source.Style;
            Time = source.Time;
            ScreenshotData = source.ScreenshotData.ToArray();

            SuspendModifiedTime = source.SuspendModifiedTime;
        }

        public static Suspend_Info get_suspend_info(int file_id, byte[] screenshot)
        {
            Suspend_Info result = new Suspend_Info();
            result.Chapter_Id = Global.game_state.chapter_id;
            string lord_face;
            // Use the first unit in the battalion, during preparations
            if (Global.game_system.preparations && Global.battalion.actors.Count > 0)
                lord_face = Global.game_actors[Global.battalion.actors[0]].face_name;
            // Use team leader
            else if (Global.game_map.team_leaders[Constants.Team.PLAYER_TEAM] != -1 &&
                    Global.game_map.units.ContainsKey(Global.game_map.team_leaders[Constants.Team.PLAYER_TEAM]))
                lord_face = Global.game_map.units[Global.game_map.team_leaders[Constants.Team.PLAYER_TEAM]].actor.face_name;
            else
                lord_face = "";
            result.Lord_Actor_Face = lord_face;
            result.Turn = Global.game_system.chapter_turn;
            result.Units = Global.game_map.teams[Constants.Team.PLAYER_TEAM].Count;
            result.Playtime = Global.game_system.total_play_time;
            result.Gold = Global.battalion.gold;
            result.Save_Id = file_id;
            result.Preparations = Global.game_system.preparations;
            result.HomeBase = Global.game_system.home_base;
            result.Difficulty = Global.game_system.Difficulty_Mode;
            result.Style = Global.game_system.Style;
            result.Time = DateTime.Now;
            if (screenshot != null)
                result.ScreenshotData = screenshot;

            return result;
        }

        public void load_screenshot(string name)
        {
            // If a screenshot with this name already exists for some reason, just return
            if (Global.SuspendScreenshots.ContainsKey(name))
            {
#if DEBUG
                throw new ArgumentException(string.Format(
                    "Suspend tried to load a screenshot but\nthe name \"{0}\" was already taken",
                    name));
#endif
                return;
            }

            if (ScreenshotData != null && ScreenshotData.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream(ScreenshotData))
                {
                    Screenshot = (Global.Content as ContentManagers.ThreadSafeContentManager).FromStream(ms);
                    Global.SuspendScreenshots[name] = Screenshot;
                }
            }
        }

        public object Clone()
        {
            return new Suspend_Info(this);
        }
    }
}
