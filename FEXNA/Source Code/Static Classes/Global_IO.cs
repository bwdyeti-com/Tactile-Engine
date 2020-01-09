using System;
using System.IO;
using FEXNAVersionExtension;

namespace FEXNA
{
    public static partial class Global
    {
        // Load Suspend
        public static bool Loading_Suspend { get; private set; }
        public static void call_load_suspend()
        {
            Loading_Suspend = true;
        }
        public static void reset_suspend_load()
        {
            Loading_Suspend = false;
        }

        static bool Suspend_Load_Successful = false;
        public static bool suspend_load_successful
        {
            get { return Suspend_Load_Successful; }
            set { Suspend_Load_Successful = value; }
        }

        internal static void suspend_load_fail(Scene_Base old_scene)
        {
            Game_System.play_se(System_Sounds.Buzzer);
            Game_Actors = null;
            Game_System = new Game_System();
            Player = null;
            Game_Map = null;
            Scene = old_scene;
            if (Scene.scene_type != "Scene_Title")
                scene_change("Scene_Title");
        }
        public static void suspend_finish_load(bool resume)
        {
            Global.change_to_new_scene("Scene_Map");
            Global.suspend_fade_in();
            Global.game_map.load_suspend();

            // Resume Preparations
            if (Game_System.home_base)
            {
                if (resume)
                    (Scene as Scene_Map).resume_home_base();
                else
                    (Scene as Scene_Map).activate_home_base();
            }
            else if (Game_System.preparations)
            {
                if (resume)
                    (Scene as Scene_Map).resume_preparations();
                else
                    (Scene as Scene_Map).activate_preparations();
            }
            else
            {
                Game_State.resume_turn_theme();
                if (!Global.game_state.is_map_ready(true))
                    Game_State.any_trigger_start_events();
                else
                    Game_State.any_trigger_events();
#if DEBUG
                Game_State.activate_autorun_events();
#endif
            }
        }

        public static bool save_version_too_new(Version v)
        {
            // If game version is older than the save, don't load the save
            return Global.RUNNING_VERSION.older_than(v);
        }

        // Save/load suspend
        public static void save_suspend(BinaryWriter writer, int fileId, byte[] screenshot)
        {
            writer.Write(Global.RUNNING_VERSION);
            writer.Write(DateTime.Now.ToBinary());
            var suspend_info = FEXNA.IO.Suspend_Info.get_suspend_info(fileId, screenshot);
            suspend_info.write(writer);
            /* Call Serialize */
            Global.game_battalions.write(writer);
            Global.game_actors.write(writer);
            Global.write_game_system(writer);
            Global.game_options.write(writer);
            Global.player.write(writer);
            Global.game_state.write(writer);
            Global.game_map.write(writer);
            Global.write_events(writer);
        }

        public static bool load_suspend(BinaryReader reader, out int fileId,
            Version v, Version oldestAllowedVersion)
        {
            // If game version is older than the save, don't load the save
            if (save_version_too_new(v))
            {
                fileId = 0;
                return false;
            }

            if (!Global.LOADED_VERSION.older_than(oldestAllowedVersion))
            {
                var info = load_info(reader);
                if (Global.Data_Chapters.ContainsKey(info.chapter_id))
                {
                    fileId = info.save_id;
                    load_v_0_4_7_0(reader);
                    return true;
                }
            }

            fileId = 0;
            return false;
        }

        private static FEXNA.IO.Suspend_Info load_info(BinaryReader reader)
        {
            DateTime modified_time = DateTime.FromBinary(reader.ReadInt64());
            var info = FEXNA.IO.Suspend_Info.read(reader);
            return info;
        }

        private static void load_v_0_4_7_0(BinaryReader reader)
        {
            Global.game_temp = new Game_Temp();
            Global.game_battalions = new Game_Battalions();
            Global.game_battalions.read(reader);
            Global.game_actors = new Game_Actors();
            Global.game_actors.read(reader);
            Global.read_game_system(reader);
            Global.game_options = Game_Options.read(reader);

            Global.player = new Player();
            Global.player.read(reader);
            // Why does this read into the active game state object, instead of generating a new one
            Global.game_state.read(reader);
            Global.game_map = new Game_Map();
            Global.game_map.read(reader);
            Global.read_events(reader);
        }

        private static void load_v_0_4_3_0(BinaryReader reader)
        {
            DateTime modified_time = DateTime.FromBinary(reader.ReadInt64());
            FEXNA.IO.Suspend_Info info = FEXNA.IO.Suspend_Info.read(reader);
            int file_id = info.save_id;

            Global.game_temp = new Game_Temp();
            Global.game_battalions = new Game_Battalions();
            Global.game_battalions.read(reader);
            Global.game_actors = new Game_Actors();
            Global.game_actors.read(reader);
            Global.read_game_system(reader);
            Global.game_options = Game_Options.read(reader);
            //Global.game_options.read(reader);

            Global.player = new Player();
            Global.player.read(reader);
            Global.game_map = new Game_Map();
            Global.game_map.read(reader);
            Global.read_events(reader);
        }

        private static void load_v_0_3_2_0(BinaryReader reader)
        {
            int file_id = reader.ReadInt32();
            Global.game_temp = new Game_Temp();
            Global.game_battalions = new Game_Battalions();
            Global.game_battalions.read(reader);
            Global.game_actors = new Game_Actors();
            Global.game_actors.read(reader);
            Global.read_game_system(reader);
            Global.game_options = Game_Options.read(reader);
            //Global.game_options.read(reader);

            Global.player = new Player();
            Global.player.read(reader);
            Global.game_map = new Game_Map();
            Global.game_map.read(reader);
            Global.read_events(reader);
        }

        #region IO Calling
        static bool Load_Save_File = false;
        public static bool load_save_file
        {
            get { return Load_Save_File; }
            set { Load_Save_File = value; }
        }

        static bool Delete_File = false;
        public static bool delete_file
        {
            get { return Delete_File; }
            internal set { Delete_File = value; }
        }
        static bool Delete_Map_Save = false;
        public static bool delete_map_save
        {
            get { return Delete_Map_Save; }
            internal set { Delete_Map_Save = value; }
        }
        static bool Delete_Suspend = false;
        public static bool delete_suspend
        {
            get { return Delete_Suspend; }
            internal set { Delete_Suspend = value; }
        }

        public static void file_deleted()
        {
            Delete_File = false;
        }
        public static void map_save_deleted()
        {
            Delete_Map_Save = false;
        }
        public static void suspend_deleted()
        {
            Delete_Suspend = false;
        }

        static bool Copy_File = false;
        public static bool copying
        {
            get { return Copy_File; }
            set
            {
                Copy_File = value;
                Move_File = value;
            }
        }
        static bool Move_File = false;
        public static bool move_file
        {
            get { return Move_File; }
            set { Move_File = value; }
        }
        static int Move_To_File_Id = -1;
        public static int move_to_file_id
        {
            get { return Move_To_File_Id; }
            set { Move_To_File_Id = value; }
        }
        #endregion

        public static bool ignore_options_load = false;
        public static bool savestate = false;

        public static bool savestate_ready
        {
            get
            {
#if !MONOGAME && DEBUG
                if (Scene != null && Scene.is_unit_editor)
                    return false;
#endif
                return Scene != null && Scene.is_strict_map_scene && Game_State.is_map_ready() &&
                    Game_System.Selected_Unit_Id == -1 && !Game_System.is_interpreter_running &&
                    Player.is_on_square && !Game_Temp.menuing;
            }
        }
        public static bool savestate_load_ready
        {
            get
            {
#if !MONOGAME && DEBUG
                if (Scene != null && Scene.is_unit_editor)
                    return false;
#endif
                return Scene != null && (!Scene.is_map_scene || !Game_System.preparations);
            }
        }
    }
}
