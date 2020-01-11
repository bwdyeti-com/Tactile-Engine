using System;
using System.IO;
using FEXNA.IO;
using FEXNA_Library.Encryption;
using FEXNAVersionExtension;

namespace FEXNA
{
    public static partial class Global
    {
        private static readonly byte[] SAVE_KEY = new byte[]
        {
            0xd9, 0x27, 0xf2, 0x16, 0x6e, 0xf2, 0xee, 0xf2,
            0xa3, 0xf1, 0x07, 0x77, 0x27, 0x2b, 0x47, 0x9e
        };

        // Load Suspend
        internal static bool Loading_Suspend { get; private set; }
        internal static void call_load_suspend()
        {
            Loading_Suspend = true;
        }
        internal static void reset_suspend_load()
        {
            Loading_Suspend = false;
        }

        static bool Suspend_Load_Successful = false;
        internal static bool suspend_load_successful
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
        internal static void suspend_finish_load(bool resume)
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

        internal static bool save_version_too_new(Version v)
        {
            // If game version is older than the save, don't load the save
            return Global.RUNNING_VERSION.older_than(v);
        }

        #region Save/load file
        internal static void save(BinaryWriter writer)
        {
            // Write version
            writer.Write(Global.RUNNING_VERSION);

            // Write options and file
            EncryptStream(writer, WriteFile);
        }

        private static void WriteFile(BinaryWriter writer)
        {
            Global.game_options.write(writer);
            Global.save_file.write(writer);
        }

        internal static bool load(
            BinaryReader reader,
            out Save_File_Data fileData,
            Version oldestAllowedVersion)
        {
            // Read and check version
            Version v;
            if (!ValidVersion(reader, oldestAllowedVersion, out v))
            {
                fileData = new Save_File_Data();
                return false;
            }

            Global.LOADED_VERSION = v;

            // Read options and file
            if (IsEncryptedVersion(Global.LOADED_VERSION))
            {
                fileData = DecryptStream(reader, ReadFile);
            }
            else
            {
                fileData = ReadFile(reader);
            }

            return true;
        }

        private static Save_File_Data ReadFile(BinaryReader reader)
        {
            if (false) { } //@Debug: if different versions are handled differently
            else
            {
                return LoadFileV0_4_4_0(reader);
            }

            return new Save_File_Data();
        }

        private static Save_File_Data LoadFileV0_4_4_0(BinaryReader reader)
        {
            return new Save_File_Data
            {
                Options = Game_Options.read(reader),
                File = Save_File.read(reader)
            };
        }
        #endregion

        #region Save/load suspend
        internal static void save_suspend(BinaryWriter writer, int fileId, byte[] screenshot)
        {
            // Write version
            writer.Write(Global.RUNNING_VERSION);

            // Write suspend info encrypted
            EncryptStream(writer, (BinaryWriter w) => WriteSuspendInfo(fileId, screenshot, w));

            // Write game data encrypted
            EncryptStream(writer, WriteSuspend);
        }

        private static void WriteSuspendInfo(int fileId, byte[] screenshot, BinaryWriter writer)
        {
            writer.Write(DateTime.Now.ToBinary());

            var suspend_info = Suspend_Info.get_suspend_info(fileId, screenshot);
            suspend_info.write(writer);
        }

        private static void WriteSuspend(BinaryWriter writer)
        {
            Global.game_battalions.write(writer);
            Global.game_actors.write(writer);
            Global.write_game_system(writer);
            Global.game_options.write(writer);

            Global.player.write(writer);
            Global.game_state.write(writer);
            Global.game_map.write(writer);
            Global.write_events(writer);
        }

        internal static bool load_suspend(
            BinaryReader reader,
            out int fileId,
            Version oldestAllowedVersion)
        {
            // Read and check version
            Version v;
            if (!ValidVersion(reader, oldestAllowedVersion, out v))
            {
                fileId = 0;
                return false;
            }

            Global.LOADED_VERSION = v;

            return load_suspend(reader, out fileId);
        }
        private static bool load_suspend(BinaryReader reader, out int fileId)
        {
            // Read suspend info
            Suspend_Info info = load_suspend_info(reader);

            // Read game data
            if (Global.Data_Chapters.ContainsKey(info.chapter_id))
            {
                fileId = info.save_id;

                if (IsEncryptedVersion(Global.LOADED_VERSION))
                {
                    DecryptStream(reader, load_v_0_4_7_0);
                    return true;
                }
                else
                {
                    load_v_0_4_7_0(reader);
                    return true;
                }
            }

            // Something went wrong
            fileId = 0;
            return false;
        }

        private static Suspend_Info ReadSuspendInfo(BinaryReader reader)
        {
            DateTime modified_time = DateTime.FromBinary(reader.ReadInt64());

            var info = Suspend_Info.read(reader);
            info.SuspendModifiedTime = modified_time;
            return info;
        }

        internal static Suspend_Info load_suspend_info(
            BinaryReader reader,
            Version oldestAllowedVersion)
        {
            Version v;
            if (!ValidVersion(reader, oldestAllowedVersion, out v))
            {
                return null;
            }
            Global.LOADED_VERSION = v;

            return load_suspend_info(reader);
        }
        private static Suspend_Info load_suspend_info(BinaryReader reader)
        {
            // Read suspend info
            Suspend_Info info;
            if (IsEncryptedVersion(Global.LOADED_VERSION))
            {
                info = DecryptStream(reader, ReadSuspendInfo);
            }
            else
                info = ReadSuspendInfo(reader);

            return info;
        }

        internal static void copy_suspend(
            BinaryReader sourceReader,
            BinaryWriter targetWriter,
            Suspend_Info info)
        {
            // Write version
            targetWriter.Write(Global.LOADED_VERSION);

            // Write suspend info
            Action<BinaryWriter> suspendInfoAction = (BinaryWriter w) =>
            {
                w.Write(info.SuspendModifiedTime.ToBinary());

                info.write(w);
            };

            if (Global.IsEncryptedVersion(Global.LOADED_VERSION))
            {
                EncryptStream(targetWriter, suspendInfoAction);
            }
            else
            {
                suspendInfoAction(targetWriter);
            }

            // Move the actual map save data, everything after the info
            sourceReader.BaseStream.CopyTo(targetWriter.BaseStream);
        }

        /* ReadSuspend */
        private static void load_v_0_4_7_0(BinaryReader reader)
        {
            // Create a new Game_Temp
            Global.game_temp = new Game_Temp();

            Global.game_battalions = new Game_Battalions();
            Global.game_battalions.read(reader);
            Global.game_actors = new Game_Actors();
            Global.game_actors.read(reader);
            Global.read_game_system(reader);
            Global.game_options = Game_Options.read(reader);

            Global.player = new Player();
            Global.player.read(reader);
            //@Yeti: // Why does this read into the active game state object, instead of generating a new one
            Global.game_state.read(reader);
            Global.game_map = new Game_Map();
            Global.game_map.read(reader);
            Global.read_events(reader);
        }
        #endregion

        #region Save/load progress data
        internal static void save_progress(BinaryWriter writer)
        {
            // Write version
            writer.Write(Global.RUNNING_VERSION);

            // Write progression
            EncryptStream(writer, WriteProgress);
        }

        private static void WriteProgress(BinaryWriter writer)
        {
            Global.progress.write(writer);
        }

        internal static bool load_progress(
            BinaryReader reader,
            out Save_Progress progress)
        {
            // Read and check version
            Version v;
            if (!ValidVersion(reader, new Version(0, 0, 0, 0), out v))
            {
                progress = null;
                return false;
            }

            // Read progression
            Func<BinaryReader, Save_Progress> suspendInfoFunction = (BinaryReader r) =>
            {
                return ReadProgress(r, v);
            };
            
            if (IsEncryptedVersion(v))
            {
                progress = DecryptStream(reader, suspendInfoFunction);
            }
            else
            {
                progress = suspendInfoFunction(reader);
            }
            
            return true;
        }

        private static Save_Progress ReadProgress(BinaryReader reader, Version version)
        {
            if (false) { } //@Debug: if different versions are handled differently
            else
            {
                return Save_Progress.read(reader, version);
            }

            return null;
        }
        #endregion

        #region Save/load config data
        internal static void SaveConfig(BinaryWriter writer, int zoom)
        {
            // Write version
            writer.Write(Global.RUNNING_VERSION);

            // Write config
            WriteConfig(writer, zoom);
        }

        private static void WriteConfig(BinaryWriter writer, int zoom)
        {
            writer.Write(zoom);
            writer.Write(Global.fullscreen);
            writer.Write(Global.stereoscopic_level);
            writer.Write(Global.anaglyph);
            writer.Write((int)Global.metrics);
            writer.Write(Global.updates_active);
            writer.Write(Global.rumble);
            FEXNA.Input.write(writer);
        }

        internal static bool LoadConfig(
            BinaryReader reader)
        {
            // Read and check version
            Version v;
            if (!ValidVersion(reader, new Version(0, 0, 0, 0), out v))
            {
                return false;
            }

            Global.LOADED_VERSION = v;

            // Read config
            ReadConfig(reader);

            return true;
        }

        private static void ReadConfig(BinaryReader reader)
        {
            if (Global.LOADED_VERSION.older_than(0, 4, 2, 0))
            {
                Global.zoom = reader.ReadInt32();
                Global.fullscreen = false;
                Global.stereoscopic_level = 0;
                Global.anaglyph = true;
                bool unused = reader.ReadBoolean();
                FEXNA.Input.read(reader);
            }
            else if (Global.LOADED_VERSION.older_than(0, 4, 6, 3))
            {
                Global.zoom = reader.ReadInt32();
                Global.fullscreen = reader.ReadBoolean();
                Global.stereoscopic_level = reader.ReadInt32();
                Global.anaglyph = reader.ReadBoolean();
                FEXNA.Input.read(reader);
            }
            else if (Global.LOADED_VERSION.older_than(0, 5, 0, 6))
            {
                Global.zoom = reader.ReadInt32();
                Global.fullscreen = reader.ReadBoolean();
                Global.stereoscopic_level = reader.ReadInt32();
                Global.anaglyph = reader.ReadBoolean();
                Global.metrics = (Metrics_Settings)reader.ReadInt32();
                FEXNA.Input.read(reader);
            }
            else
            {
                Global.zoom = reader.ReadInt32();
                Global.fullscreen = reader.ReadBoolean();
                Global.stereoscopic_level = reader.ReadInt32();
                Global.anaglyph = reader.ReadBoolean();
                Global.metrics = (Metrics_Settings)reader.ReadInt32();
                Global.updates_active = reader.ReadBoolean();
                Global.rumble = reader.ReadBoolean();
                FEXNA.Input.read(reader);
            }
        }
        #endregion

        private static bool ValidVersion(
            BinaryReader reader,
            Version oldestAllowedVersion,
            out Version version)
        {
            // Read version
            version = reader.ReadVersion();

            // If game version is older than the save, don't load the save
            if (save_version_too_new(version))
                return false;
            // If the game version is too old
            if (version.older_than(oldestAllowedVersion))
                return false;

            return true;
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

        internal static bool IsEncryptedVersion(Version version)
        {
            return !version.older_than(0, 6, 9, 0);
        }

        private static void EncryptStream(BinaryWriter writer, Action<BinaryWriter> save)
        {
            // Create a temporary memory stream to hold the unencrypted data
            using (MemoryStream ms = new MemoryStream())
            {
                // Write to the memory stream
                BinaryWriter memoryWriter = new BinaryWriter(ms);
                save(memoryWriter);
                memoryWriter.Flush();

                // Reset to the start of the memory stream, then copy it encrypted
                ms.Seek(0, SeekOrigin.Begin);
                StreamEncryption
                    .EncryptStream(SAVE_KEY, writer, ms);
            }
        }

        private static void DecryptStream(
            BinaryReader reader,
            Action<BinaryReader> read)
        {
            using (BinaryReader decryptedReader =
                StreamEncryption.DecryptStream(SAVE_KEY, reader))
            {
                read(decryptedReader);
            }
        }
        private static T DecryptStream<T>(
            BinaryReader reader,
            Func<BinaryReader, T> read)
        {
            using (BinaryReader decryptedReader =
                StreamEncryption.DecryptStream(SAVE_KEY, reader))
            {
                return read(decryptedReader);
            }
        }

        internal static bool ignore_options_load = false;
        internal static bool savestate = false;

        internal static bool savestate_ready
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
        internal static bool savestate_load_ready
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

    struct Save_File_Data
    {
        public Game_Options Options;
        public Save_File File;
    }
}
