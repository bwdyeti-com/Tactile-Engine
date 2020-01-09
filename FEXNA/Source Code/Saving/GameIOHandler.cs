using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Storage;
using FEXNA.Rendering;
using FEXNAVersionExtension;

namespace FEXNA.IO
{
    class GameIOHandler
    {
#if DEBUG
        const bool SAVESTATE_ENABLED = true;
#else
        const bool SAVESTATE_ENABLED = false;
#endif

        readonly static Version OLDEST_ALLOWED_SUSPEND_VERSION = new Version(0, 6, 7, 0);
        readonly static Version OLDEST_ALLOWED_SAVE_VERSION = new Version(0, 4, 4, 0);

        const string SAVESTATE_FILENAME = "savestate1";
        const string SUSPEND_FILENAME = "suspend";
        const string SAVE_LOCATION = "Save";

        private int SavestateFileId = -1;
        private bool SavestateTesting = false;

        private StorageDevice Storage;
        private IAsyncResult AsyncResult;

        private bool QuickLoad = false; //@Debug: what is this specifically

        private ISaveCallbacker Callback;
        private GameRenderer Renderer;

        private int FileId = 1; //@Debug: why static
        private int DebugFileIdTest; //@Debug: not really necessary, just confirming things are working

        //@Debug: // True until game options are loaded
        private bool STARTING = true;

        public GameIOHandler(ISaveCallbacker callback, GameRenderer renderer)
        {
            Callback = callback;
            Renderer = renderer;
        }

        public void RefreshSaveId()
        {
            Global.current_save_id = FileId;
        }
#if DEBUG
        public void RefreshDebugFileId(bool debugStart)
        {
            if (debugStart)
            {
                DebugFileIdTest = FileId = 1;
                Global.current_save_id = FileId;
            }
        }
#endif

        public void RefreshFileId()
        {
            // Switch to the appropriate save file
            FileId = Global.current_save_id;
        }

        public void UpdateIO(bool createSaveState, bool loadSaveState)
        {
            if (SAVESTATE_ENABLED)
            {
                if (createSaveState)
                {
                    if (Global.savestate_ready)
                    {
                        Global.play_se(System_Sounds.Confirm);
                        Global.scene.suspend();
                        Global.savestate = true;
                    }
                    else
                        Global.play_se(System_Sounds.Buzzer);
                }
            }

            while (true)
            {
                // Select storage
                if ((Global.Loading_Suspend || Global.load_save_file || Global.load_save_info) && Storage == null)
                    Global.storage_selection_requested = true;

                if (Global.storage_selection_requested)
                {
#if XBOX
                if (!Guide.IsVisible)
#endif
                    {
                        AsyncResult = StorageDevice.BeginShowSelector(null, null);
#if !XBOX
                        while (!AsyncResult.IsCompleted)
                            AsyncResult = StorageDevice.BeginShowSelector(null, null);
#endif
                    }
                    if (AsyncResult.IsCompleted)
                    {
                        Storage = StorageDevice.EndShowSelector(AsyncResult);
                        Global.storage_selection_requested = false;
                        // If no device selected and trying to load suspend
                        if (Storage == null && Global.Loading_Suspend)
                        {
                            Global.play_se(System_Sounds.Buzzer);
                            Global.reset_suspend_load();
                        }
                    }
                    else
                        break;
                }
                // Saving suspends or files is handled in a method below, this handles more utility actions and loading
                // Config
                else if (Global.load_config)
                {
                    if (Storage != null && Storage.IsConnected)
                    {
                        LoadConfig();
                    }
                    Global.load_config = false;
                }
                else if (Global.save_config)
                {
                    if (Storage != null && Storage.IsConnected)
                    {
                        SaveConfig();
                    }
                    Global.save_config = false;
                }
                #region Delete (File, Map Save, Suspend)
                // Delete File
                else if (Global.delete_file)
                {
                    Global.file_deleted();
                    if (Storage != null && Storage.IsConnected)
                    {
                        Global.save_files_info.Remove(Global.start_game_file_id);
                        if (Global.suspend_file_info != null &&
                                Global.suspend_file_info.save_id == Global.start_game_file_id)
                            Global.suspend_file_info = null;

                        DeleteSaveFile(Global.start_game_file_id);
                        Global.start_game_file_id = -1;
                        // Rechecks the most recent suspend and reloads save info
                        LoadSuspendInfo();
                        LoadSaveInfo();
                        Global.load_save_info = false;
                    }
                }
                // Delete Map Save
                else if (Global.delete_map_save)
                {
                    Global.map_save_deleted();
                    if (Storage != null && Storage.IsConnected)
                    {
                        // Same question as below //@Yeti
                        DeleteFile(MapSaveFilename(FileId));
                        DeleteFile(SuspendFilename(FileId));
                        // Rechecks the most current suspend
                        LoadSuspendInfo(updateFileId: false);
                        // Updates the save info for the selected file
                        LoadSaveInfo();
                        Global.load_save_info = false;
                    }
                }
                // Delete Suspend
                else if (Global.delete_suspend)
                {
                    Global.suspend_deleted();
                    if (Storage != null && Storage.IsConnected)
                    {
                        DeleteFile(SuspendFilename(FileId));
                        // Rechecks the most current suspend
                        LoadSuspendInfo(updateFileId: false);
                        // Updates the save info for the selected file
                        LoadSaveInfo();
                        Global.load_save_info = false;
                    }
                }
                #endregion
                #region Load (Save Info, Suspend, Save File)
                // Load Save Info
                else if (Global.load_save_info)
                {
                    if (Storage != null && Storage.IsConnected)
                    {
                        // Load progress data
                        LoadProgress();
                        // Rechecks the most recent suspend and reloads save info
                        LoadSuspendInfo();
                        LoadSaveInfo();
                    }
                    Global.load_save_info = false;
                }
                // Load Suspend
                else if (Global.Loading_Suspend)
                {
                    if (Storage != null && Storage.IsConnected)
                    {
                        // If a specific file is selected, use it
                        if (Global.start_game_file_id != -1)
                        {
                            FileId = Global.start_game_file_id;
                            Global.start_game_file_id = -1;
                        }
                        // Otherwise loading the most recent suspend
                        else
                            FileId = Global.suspend_file_info.save_id;
                        // Load save file first, then test if the suspend works
                        if (LoadFile() && TestLoad(true))
                        {
                            // Then load relevant suspend
                            if (Global.scene.scene_type != "Scene_Soft_Reset")
                            {
                                Global.cancel_sound();
                                Global.play_se(System_Sounds.Confirm);
                            }
                            load();
#if DEBUG
                            if (SavestateFileId != -1)
                            {
                                FileId = SavestateFileId;
                                DebugFileIdTest = FileId;
                                SavestateFileId = -1;
                            }
#endif
                        }
                        else
                        {
                            if (Global.scene.scene_type != "Scene_Soft_Reset")
                                Global.play_se(System_Sounds.Buzzer);
                            Global.scene.reset_suspend_filename();
                        }
                    }
                    else
                        Global.play_se(System_Sounds.Buzzer);
                    Global.reset_suspend_load();
                }
                // Load Save File
                else if (Global.load_save_file)
                {
                    if (Storage != null && Storage.IsConnected)
                    {
                        if (Global.start_game_file_id != -1)
                        {
                            FileId = Global.start_game_file_id;
                            Global.start_game_file_id = -1;
                        }
                        DebugFileIdTest = FileId;
                        LoadFile();
                        Global.game_options.post_read();
                    }
                    else
                    {
                        ResetFile();
                    }
                    Global.load_save_file = false;
                }
                #endregion
                // Copy File
                else if (Global.copying)
                {
                    // Why is there a breakpoint here though //@Debug
                    Global.copying = false;
                    if (Storage != null && Storage.IsConnected)
                    {
                        Global.save_files_info[Global.move_to_file_id] =
                            new Save_Info(Global.save_files_info[Global.start_game_file_id]);
                        Global.save_files_info[Global.move_to_file_id].reset_suspend_exists();

                        MoveFile(Global.start_game_file_id, Global.move_to_file_id, true);
                        Global.start_game_file_id = -1;
                        Global.move_to_file_id = -1;

                    }
                }
                // Move File
                else if (Global.move_file)
                {
                    // Why is there a breakpoint here though //@Debug
                    Global.move_file = false;
                    if (Storage != null && Storage.IsConnected)
                    {
                        Global.save_files_info[Global.move_to_file_id] =
                            new Save_Info(Global.save_files_info[Global.start_game_file_id]);
                        Global.save_files_info.Remove(Global.start_game_file_id);

                        MoveFile(Global.start_game_file_id, Global.move_to_file_id);
                        Global.start_game_file_id = -1;
                        Global.move_to_file_id = -1;
                    }
                }
                else
                {
                    // Load suspend with F1, in Debug
                    if (SAVESTATE_ENABLED)
                    {
                        if (Global.savestate_load_ready)
                            if (loadSaveState)
                            {
                                if (Storage != null && Storage.IsConnected)
                                {
                                    QuickLoad = true;
                                    // Tells the game to ignore suspend id/current save id disparity temporarily
                                    SavestateTesting = true;
                                    // Checks if the suspend can be loaded, and sets Savestate_File_Id to its file id
                                    if (TestLoad(SAVESTATE_FILENAME + Config.SAVE_FILE_EXTENSION, true))
                                    {
                                        FileId = SavestateFileId;
                                        DebugFileIdTest = FileId;
                                        SavestateFileId = -1;
                                        LoadFile();
                                        Global.cancel_sound();
                                        load(SAVESTATE_FILENAME + Config.SAVE_FILE_EXTENSION);
                                        RefreshSaveId();
                                    }
                                    else
                                    {
                                        if (Global.scene.scene_type == "Scene_Title")
                                            Global.play_se(System_Sounds.Buzzer);
                                        Global.scene.reset_suspend_filename();
                                    }
                                    SavestateTesting = false;
                                }
                            }
                    }
                    break;
                }
            }
        }

        public void UpdateSuspend()
        {
            if (Global.scene.suspend_calling && !Global.scene.suspend_blocked())
            {
                if (Storage != null && Storage.IsConnected)
                {
                    bool mapSave = Global.scene.is_map_save_filename;
                    if (SAVESTATE_ENABLED && Global.savestate)
                    {
                        save(SAVESTATE_FILENAME + Config.SAVE_FILE_EXTENSION);
                        Global.savestate = false;
                    }
                    else
                    {
                        if (save())
                            if (mapSave)
                                Global.map_save_created();
                    }
                    // Write progress data
                    SaveProgress();
                    Global.scene.reset_suspend_calling();
                    Global.scene.reset_suspend_filename();
                    if (Global.return_to_title)
#if DEBUG
                        if (Global.UnitEditorActive)
                            Global.scene_change("Scene_Map_Unit_Editor");
                        else
#endif
                            Callback.TitleLoadScene();
                }
            }
            if (Global.scene.save_data_calling)
            {
                if (Storage != null && Storage.IsConnected)
                {
                    if (Global.start_new_game)
                    {
                        FileId = Global.start_game_file_id;
                        DebugFileIdTest = FileId;

                        Global.start_new_game = false;
                        Global.start_game_file_id = -1;
                    }
                    if (DebugFileIdTest != FileId)
                    {
                        throw new Exception();
                    }
                    SaveFile(FileId);
                    // Update and write progress data
                    Global.progress.update_progress(Global.save_file);
                    SaveProgress();
                    Global.scene.EndSaveData();
                }
            }
        }

        #region Save/Load Data
        private string SaveLocation()
        {
#if WINDOWS
#if !MONOGAME
            return SAVE_LOCATION;
#else
            return string.Format("{0}\\Save\\AllPlayers", Global.GAME_ASSEMBLY.GetName().Name);
#endif
#else
            return Global.GAME_ASSEMBLY.GetName().Name;
            //return Global.GAME_ASSEMBLY.GetName().Name + "\\" + SAVE_LOCATION;
#endif
        }

        private string ConvertSaveName()
        {
            // If the scene has a specific filename it wants
            if (Global.scene != null && !string.IsNullOrEmpty(Global.scene.suspend_filename))
            {
                if (Global.scene.is_map_save_filename)
                    return FileId + Global.scene.suspend_filename + Config.SAVE_FILE_EXTENSION;
                else
                    return Global.scene.suspend_filename;
            }
            // Otherwise load the suspend for the current file
            else
                return SuspendFilename(FileId);
        }

        private string SuspendFilename(int id)
        {
            return id + SUSPEND_FILENAME + Config.SAVE_FILE_EXTENSION;
        }
        private string MapSaveFilename(int id)
        {
            return id + Config.MAP_SAVE_FILENAME + Config.SAVE_FILE_EXTENSION;
        }

        private bool TestLoad(bool suspend)
        {
            return TestLoad(ConvertSaveName(), suspend);
        }
        private bool TestLoad(string filename, bool suspend)
        {
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                /* Call FileExists */
                // Check to see whether the save exists.
                if (container == null || !container.FileExists(filename))
                {
                    Global.suspend_load_successful = false;
                    return false;
                }
                // Is a suspend/checkpoint/savestate, make sure the file opens properly, check that the version isn't too old, and test the file id
                else if (suspend)
                {
                    using (Stream stream = container.OpenFile(
                        filename, FileMode.Open, FileAccess.Read))
                    {
                        try
                        {
                            using (BinaryReader reader = new BinaryReader(stream))
                            {
                                Global.LOADED_VERSION = LoadVersion(reader);
                                if (!Global.LOADED_VERSION.older_than(OLDEST_ALLOWED_SUSPEND_VERSION))
                                {
                                    DateTime modifiedTime = DateTime.FromBinary(reader.ReadInt64());
                                    Suspend_Info info = Suspend_Info.read(reader);
                                    // If the file id isn't the currently active one, return false
                                    SavestateFileId = info.save_id;
                                    if (!SavestateTesting)
                                    {
                                        if (SavestateFileId != FileId)
                                        {
#if DEBUG
                                            Print.message(string.Format(
                                                "Trying to load a suspend for file {0},\n" +
                                                "but the suspend was saved to slot {1}.\n" +
                                                "This would fail in release.",
                                                FileId, SavestateFileId));
                                            SavestateFileId = FileId;
                                        }
                                        else
#else
                                            Global.suspend_load_successful = false;
                                            return false;
                                        }
#endif
                                            SavestateFileId = -1;
                                    }
                                    SavestateTesting = false;
                                }
                            }
                        }
                        catch (EndOfStreamException e)
                        {
                            Global.suspend_load_successful = false;
                            return false;
                        }
                    }
                }
                // Saves Files/etc, check that the file opens and isn't too out of date
                else
                {
                    using (Stream stream = container.OpenFile(
                        filename, FileMode.Open, FileAccess.Read))
                    {
                        try
                        {
                            using (BinaryReader reader = new BinaryReader(stream))
                                Global.LOADED_VERSION = LoadVersion(reader);
                            if (!ValidSaveVersion(Global.LOADED_VERSION))
                                return false;
                        }
                        catch (EndOfStreamException e)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        #region Progress Data
        private void SaveProgress()
        {
            string filename = "progress" + Config.SAVE_FILE_EXTENSION;
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                // Close the wait handle
                result.AsyncWaitHandle.Close();

                if (container != null)
                {
                    // Delete file if it already exists
                    if (container.FileExists(filename))
                        container.DeleteFile(filename);
                    using (Stream stream = container.CreateFile(filename))
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            Version version = Global.RUNNING_VERSION;
                            writer.Write(version.Major);
                            writer.Write(version.Minor);
                            writer.Write(version.Build);
                            writer.Write(version.Revision);

                            Global.progress.write(writer);
                        }
                    }
                }
            }
        }
        private void LoadProgress()
        {
            string filename = "progress" + Config.SAVE_FILE_EXTENSION;
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                // Close the wait handle
                result.AsyncWaitHandle.Close();
                // If the file doesn't exist, return
                if (container == null || !container.FileExists(filename))
                {
                    return;
                }
                using (Stream stream = container.OpenFile(
                    filename, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Version progressVersion = LoadVersion(reader);

                            Save_Progress progress = Save_Progress.read(reader, progressVersion);
                            Global.progress.combine_progress(progress);
                        }
                    }
                    catch (EndOfStreamException e)
                    {
                    }
                }
            }
        }
        #endregion

        #region Save File
        private void SaveFile(int id)
        {
            string filename = id.ToString() + Config.SAVE_FILE_EXTENSION;
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();

                if (container != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Write the save to a memory stream to make sure the save is successful, before actually writing it to file
                        // Make all the saving more like this //@Debug
                        using (BinaryWriter writer = new BinaryWriter(ms))
                        {
                            Version version = Global.RUNNING_VERSION;
                            writer.Write(version.Major);
                            writer.Write(version.Minor);
                            writer.Write(version.Build);
                            writer.Write(version.Revision);
                            /* Call Serialize */
                            Global.game_options.write(writer);
                            Global.save_file.write(writer);
                        }

                        // Check to see whether the save exists.
                        if (container.FileExists(filename))
                            // Delete it so that we can create one fresh.
                            container.DeleteFile(filename);

                        // Create the file, copy the memory stream to it
                        using (Stream stream = container.CreateFile(filename))
                        {
                            using (BinaryWriter writer = new BinaryWriter(stream))
                            {
                                writer.Write(ms.GetBuffer());
                            }
                        }
                    }
                }
            }
        }

        private void ResetFile()
        {
            if (!Global.ignore_options_load)
                Global.game_options = new Game_Options();
            Global.save_file = new Save_File();
        }

        public bool LoadFile()
        {
            string filename = FileId.ToString() + Config.SAVE_FILE_EXTENSION;
            ResetFile();
            if (!TestLoad(filename, false))
            {
                return false;
            }
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                /* Call FileExists */
                // Check to see whether the save exists.
                if (container == null || !container.FileExists(filename))
                {
                    return false;
                }
                /* Create Stream object */
                // Open the file.
                // Add a IOException handler for if the file is being used by another process //@Yeti
                using (Stream stream = container.OpenFile(
                    filename, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        /* Create XmlSerializer */
                        // Convert the object to XML data and put it in the stream.
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Version v;
                            Save_File_Data data;
                            if (LoadFile(stream, out v, out data))
                            {
                                Global.LOADED_VERSION = v;

                                if (false) { } //@Debug: if different versions are handled differently
                                else
                                {
                                    Global.game_options = data.Options;
                                    Global.save_file = data.File;
                                }
                            }
                            else
                            {
                                ResetFile();
                                return false;
                            }
                        }
                    }
                    catch (EndOfStreamException e)
                    {
                        ResetFile();
                        return false;
                    }
                }
                /* Dispose the StorageContainer */
                // Dispose the container, to commit changes.
            }
            STARTING = false;
            return true;
        }

        private bool LoadFile(Stream stream, out Version v, out Save_File_Data data)
        {
            /* Create XmlSerializer */
            // Convert the object to XML data and put it in the stream.
            using (BinaryReader reader = new BinaryReader(stream))
            {
                v = LoadVersion(reader);
                if (!ValidSaveVersion(v))
                    throw new EndOfStreamException();

                /* Call Deserialize */
                if (false) { } //@Debug: if different versions are handled differently
                else
                {
                    data = LoadFileV0_4_4_0(reader);
                }
                return true;
            }

            v = new Version();
            data = new Save_File_Data();
            return false;
        }

        private bool MoveFile(int id, int moveToId, bool copying = false)
        {
            string filename = id.ToString() + Config.SAVE_FILE_EXTENSION;
            if (!TestLoad(filename, false))
            {
                return false;
            }
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                /* Call FileExists */
                // Check to see whether the save exists.
                if (container == null || !container.FileExists(filename))
                {
                    return false;
                }
                string targetFilename = moveToId.ToString() + Config.SAVE_FILE_EXTENSION;
                // Delete old target files
                DeleteSaveFile(moveToId, container);

                // Copy old file to new location
                using (Stream moveFromStream = container.OpenFile(
                    filename, FileMode.Open, FileAccess.Read))
                using (Stream stream = container.CreateFile(targetFilename))
                {
                    byte[] buffer = new byte[moveFromStream.Length];
                    moveFromStream.Read(buffer, 0, buffer.Length);
                    stream.Write(buffer, 0, buffer.Length);
                }

                // If not copying (ie just moving)
                if (!copying)
                {
                    // Copy the suspend and map save
                    CopySuspend(id, moveToId, container);
                    CopyMapSave(id, moveToId, container);

                    // And then delete the old files
                    DeleteSaveFile(id, container);
                }
            }
            return true;
        }

        private void CopySuspend(int id, int moveToId, StorageContainer container)
        {
            string sourceSuspendFilename = SuspendFilename(id);
            string targetSuspendFilename = SuspendFilename(moveToId);

            CopySuspend(id, moveToId, container, sourceSuspendFilename, targetSuspendFilename);
        }
        private void CopySuspend(int id, int moveToId, StorageContainer container,
            string sourceSuspendFilename, string targetSuspendFilename)
        {
            if (container.FileExists(sourceSuspendFilename))
                using (Stream moveFromStream = container.OpenFile(
                    sourceSuspendFilename, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(moveFromStream))
                    {
                        try
                        {
                            Global.LOADED_VERSION = LoadVersion(reader);
                            // If the map save is valid
                            if (!Global.LOADED_VERSION.older_than(OLDEST_ALLOWED_SUSPEND_VERSION))
                            {
                                DateTime modifiedTime = DateTime.FromBinary(reader.ReadInt64());
                                Suspend_Info info = Suspend_Info.read(reader);

                                if (info.save_id == id)
                                    using (Stream stream = container.CreateFile(targetSuspendFilename))
                                    {
                                        info.save_id = moveToId;
                                        using (BinaryWriter writer = new BinaryWriter(stream))
                                        {
                                            writer.Write(Global.LOADED_VERSION.Major);
                                            writer.Write(Global.LOADED_VERSION.Minor);
                                            writer.Write(Global.LOADED_VERSION.Build);
                                            writer.Write(Global.LOADED_VERSION.Revision);
                                            writer.Write(modifiedTime.ToBinary());
                                            info.write(writer);
                                            // Move the actual map save data, everything after the info
                                            byte[] buffer = new byte[moveFromStream.Length - moveFromStream.Position];
                                            moveFromStream.Read(buffer, 0, buffer.Length);
                                            stream.Write(buffer, 0, buffer.Length);
                                        }
                                    }
                            }
                            // If the map save is too old to read properly, just move everything in it
                            else
                            {
                                using (Stream stream = container.CreateFile(targetSuspendFilename))
                                {
                                    moveFromStream.Position = 0;
                                    byte[] buffer = new byte[moveFromStream.Length];
                                    moveFromStream.Read(buffer, 0, buffer.Length);
                                    stream.Write(buffer, 0, buffer.Length);
                                }
                            }
                        }
                        catch (EndOfStreamException e)
                        {
#if DEBUG
                            Print.message("Failed to copy suspend");
#endif
                        }
                    }
                }
        }
        private void CopyMapSave(int id, int moveToId, StorageContainer container)
        {
            string sourceSuspendFilename = MapSaveFilename(id);
            string targetSuspendFilename = MapSaveFilename(moveToId);

            CopySuspend(id, moveToId, container, sourceSuspendFilename, targetSuspendFilename);
        }

        private bool DeleteSaveFile(int id)
        {
            string filename = id.ToString() + Config.SAVE_FILE_EXTENSION;
            if (!TestLoad(filename, false))
            {
                return false;
            }
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                DeleteSaveFile(id, container);
            }
            return true;
        }
        private bool DeleteSaveFile(int id, StorageContainer container)
        {
            string filename = id.ToString() + Config.SAVE_FILE_EXTENSION;
            // Close the wait handle.
            AsyncResult.AsyncWaitHandle.Close();
            /* Call FileExists */
            // Check to see whether the save exists.
            if (container == null || !container.FileExists(filename))
            {
                return false;
            }
            container.DeleteFile(filename);
            if (container.FileExists(SuspendFilename(id)))
                container.DeleteFile(SuspendFilename(id));
            if (container.FileExists(MapSaveFilename(id)))
                container.DeleteFile(MapSaveFilename(id));

            return true;
        }

        private bool DeleteFile(string filename)
        {
            if (!TestLoad(filename, false))
            {
                return false;
            }
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                /* Call FileExists */
                // Check to see whether the save exists.
                if (container == null || !container.FileExists(filename))
                {
                    return false;
                }
                container.DeleteFile(filename);
            }
            return true;
        }

        private Save_File_Data LoadFileV0_4_0_2(BinaryReader reader)
        {
            return new Save_File_Data
            {
                Options = Game_Options.read(reader),
                File = Save_File.read(reader)
            };
        }
        private Save_File_Data LoadFileV0_4_4_0(BinaryReader reader)
        {
            return new Save_File_Data
            {
                Options = Game_Options.read(reader),
                File = Save_File.read(reader)
            };
        }
        #endregion

        #region Map Save
        private bool save()
        {
            return save(ConvertSaveName());
        }
        private bool save(string filename)
        {
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();

                if (container != null)
                {
                    // Check to see whether the save exists.
                    if (container.FileExists(filename))
                        // Delete it so that we can create one fresh.
                        container.DeleteFile(filename);

                    // Create the file
                    using (Stream stream = container.CreateFile(filename))
                    {
                        // Write to it
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            // Save FinalRender to a byte[]
                            byte[] screenshot;
                            using (MemoryStream ms = new MemoryStream())
                            {
                                Renderer.SaveScreenshot(ms);
                                screenshot = ms.ToArray();
                            }
                            Global.save_suspend(writer, FileId, screenshot);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private void load()
        {
            load(ConvertSaveName());
        }
        private void load(string filename)
        {
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {

                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                /* Call FileExists */
                // Check to see whether the save exists.
                if (container == null || !container.FileExists(filename))
                {
                    Global.suspend_load_successful = false;
                    QuickLoad = false;
                    return;
                }
                /* Create Stream object */
                // Open the file.
                using (Stream stream = container.OpenFile(
                    filename, FileMode.Open, FileAccess.Read))
                {
                    Scene_Base oldScene = Global.scene;
                    try
                    {
                        /* Create XmlSerializer */
                        // Convert the object to XML data and put it in the stream.
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Global.LOADED_VERSION = LoadVersion(reader);
                            // Wait for move range update thread to finish
                            Callback.CloseMoveRangeThread();
                            /* Call Deserialize */
                            int fileId;
                            bool loadSuccessful = Global.load_suspend(
                                reader, out fileId,
                                Global.LOADED_VERSION,
                                OLDEST_ALLOWED_SUSPEND_VERSION);

                            if (loadSuccessful)
                            {
#if !DEBUG
                                if (FILE_ID != fileId) //Yeti
                                {
                                    throw new Exception();
                                }
#endif
                                FileId = fileId;
                                DebugFileIdTest = FileId; //@Yeti
                                Global.game_options.post_read();
                                if (!Global.Audio.stop_me(true))
                                    Global.Audio.BgmFadeOut(20);
                                Global.Audio.stop_bgs();
                            }
                            else
                                throw new EndOfStreamException("Load unsuccessful");
                        }

                        if (QuickLoad)
                        {
                            // Resume Arena
                            if (Global.in_arena())
                            {
                                Callback.ArenaScene();
                                Callback.StartMoveRangeThread();
                            }
                            else
                            {
                                Global.suspend_finish_load(false);
                                Callback.StartMoveRangeThread();
                            }
                        }
                        else
                            Global.suspend_load_successful = true;
                    }
                    catch (EndOfStreamException e)
                    {
                        Global.suspend_load_successful = false;
                        Callback.CloseMoveRangeThread();
                        Global.suspend_load_fail(oldScene);

                        Print.message("Suspend file not in the right format");
                    }
                }
                /* Dispose the StorageContainer */
                // Dispose the container, to commit changes.
            }
            Callback.FinishLoad();
            QuickLoad = false;
        }
        #endregion

        #region Save Info
        private void LoadSuspendInfo(bool updateFileId = true)
        {
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();
                /* Call FileExists */
                // Check to see whether the save exists.
                if (container == null)
                {
                    Global.suspend_file_info = null;
                    return;
                }

                // Get valid suspend file names
                List<string> suspendFiles = new List<string>(
                    container.GetFileNames("*" + SUSPEND_FILENAME + Config.SAVE_FILE_EXTENSION)
                        .Select(x => Path.GetFileName(x)));
                int i = 0;
                int test;
                while (i < suspendFiles.Count)
                {
                    // Gets the file id from the file name
                    string suspendFile = suspendFiles[i];
                    suspendFile = suspendFile.Substring(0, suspendFile.Length - (SUSPEND_FILENAME.Length + Config.SAVE_FILE_EXTENSION.Length));
                    // If the id is a number, keep it in the list
                    if (suspendFile.Length > 0 && int.TryParse(suspendFile, System.Globalization.NumberStyles.None, System.Globalization.NumberFormatInfo.InvariantInfo, out test))
                        i++;
                    else
                        suspendFiles.RemoveAt(i);
                }
                // If no files are valid, return
                if (suspendFiles.Count == 0)
                {
                    Global.suspend_file_info = null;
                    return;
                }
                // Get the newest file
                DateTime modifiedTime = new DateTime();
                int index = -1;
                for (i = 0; i < suspendFiles.Count; i++)
                {
                    try
                    {
                        using (Stream stream = container.OpenFile(
                            suspendFiles[i], FileMode.Open, FileAccess.Read))
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Global.LOADED_VERSION = LoadVersion(reader);
                            if (!Global.LOADED_VERSION.older_than(OLDEST_ALLOWED_SUSPEND_VERSION))
                            {
                                DateTime time = DateTime.FromBinary(reader.ReadInt64());
                                Global.suspend_file_info = Suspend_Info.read(reader);
                                int filenameId = Convert.ToInt32(suspendFiles[i].Substring(0,
                                    suspendFiles[i].Length - (SUSPEND_FILENAME.Length + Config.SAVE_FILE_EXTENSION.Length)));
                                // If the file id in the data does not match the id in the filename, or there isn't a save for this id
                                if (Global.suspend_file_info.save_id != filenameId ||
                                        !container.FileExists(Global.suspend_file_info.save_id.ToString() + Config.SAVE_FILE_EXTENSION))
                                    continue;
                                if (index == -1 || time > modifiedTime)
                                {
                                    index = i;
                                    modifiedTime = time;
                                }
                            }
                        }
                    }
                    catch (EndOfStreamException e) { }
                }
                // No suspends found
                if (index == -1)
                {
                    Global.suspend_file_info = null;
                    return;
                }

                string filename = suspendFiles[index];
                /* Create Stream object */
                // Open the file.
                using (Stream stream = container.OpenFile(
                    filename, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        /* Create XmlSerializer */
                        // Convert the object to XML data and put it in the stream.
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Global.LOADED_VERSION = LoadVersion(reader);
                            /* Call Deserialize */
                            if (!Global.LOADED_VERSION.older_than(OLDEST_ALLOWED_SUSPEND_VERSION))
                            {
                                DateTime time = DateTime.FromBinary(reader.ReadInt64());
                                Global.suspend_file_info = Suspend_Info.read(reader);
                                if (updateFileId)
                                    FileId = Global.suspend_file_info.save_id;
                            }
                            else
                            {
                                Global.suspend_file_info = new Suspend_Info();
                            }
                        }
                    }
                    catch (EndOfStreamException e)
                    {
                        Global.suspend_file_info = null;
                    }
                }
                /* Dispose the StorageContainer */
                // Dispose the container, to commit changes.
            }
        }
        private void LoadSaveInfo()
        {
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                // Close the wait handle.
                result.AsyncWaitHandle.Close();

                // Get valid save file names
                List<string> saveFiles = new List<string>(
                    container.GetFileNames("*" + Config.SAVE_FILE_EXTENSION)
                        .Select(x => Path.GetFileName(x)));
                int i = 0;
                int test;
                while (i < saveFiles.Count)
                {
                    string saveFile = saveFiles[i];
                    saveFile = saveFile.Substring(0, saveFile.Length - (Config.SAVE_FILE_EXTENSION.Length));
                    if (saveFile.Length > 0 && int.TryParse(saveFile, System.Globalization.NumberStyles.None, System.Globalization.NumberFormatInfo.InvariantInfo, out test) &&
                            (test < Config.SAVES_PER_PAGE * Config.SAVE_PAGES))
                        i++;
                    else
                        saveFiles.RemoveAt(i);
                }
                // If no files are valid, return
                if (saveFiles.Count == 0)
                {
                    Global.save_files_info = null;
                    Global.suspend_files_info = null;
                    Global.checkpoint_files_info = null;
                    Global.latest_save_id = -1;
                    return;
                }

                // If any old data needs referenced
                var oldSaveFilesInfo = Global.save_files_info;
                // Get the newest file
                Global.save_files_info = new Dictionary<int, Save_Info>();
                Global.suspend_files_info = new Dictionary<int, Suspend_Info>();
                Global.checkpoint_files_info = new Dictionary<int, Suspend_Info>();
                DateTime modifiedTime = new DateTime();
                int index = -1;
                i = 0;
                while (i < saveFiles.Count)
                {
                    using (Stream stream = container.OpenFile(
                        saveFiles[i], FileMode.Open, FileAccess.Read))
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        int fileId = Convert.ToInt32(saveFiles[i].Substring(0, saveFiles[i].Length - (Config.SAVE_FILE_EXTENSION.Length)));
                        Global.LOADED_VERSION = LoadVersion(reader);
                        try
                        {
                            if (!ValidSaveVersion(Global.LOADED_VERSION))
                                throw new EndOfStreamException(
                                    "Save file is too outdated or from a newer version");

                            Save_File_Data data = LoadFileV0_4_4_0(reader);
                            // Updates the progress data with this save file
                            Global.progress.update_progress(data.File);
                            bool suspendExists = container.FileExists(SuspendFilename(fileId));
                            bool mapSaveExists = container.FileExists(MapSaveFilename(fileId));
                            Save_Info info;
                            Suspend_Info suspendInfo = null;
                            // Check if the map save actually exists
                            if (mapSaveExists)
                            {
                                Version v = Global.LOADED_VERSION;
                                using (Stream suspendStream = container.OpenFile(
                                    MapSaveFilename(fileId), FileMode.Open, FileAccess.Read))
                                {
                                    try
                                    {
                                        using (BinaryReader suspendReader = new BinaryReader(suspendStream))
                                        {
                                            Global.LOADED_VERSION = LoadVersion(suspendReader);
                                            /* Call Deserialize */
                                            if (!Global.LOADED_VERSION.older_than(OLDEST_ALLOWED_SUSPEND_VERSION))
                                            {
                                                DateTime time = DateTime.FromBinary(suspendReader.ReadInt64());
                                                suspendInfo = Suspend_Info.read(suspendReader);
                                                Global.checkpoint_files_info[fileId] = suspendInfo;
                                            }
                                            else
                                                mapSaveExists = false;
                                        }
                                    }
                                    catch (EndOfStreamException e)
                                    {
                                        mapSaveExists = false;
                                    }
                                }
                                Global.LOADED_VERSION = v;
                            }
                            // If a suspend exists, always use its data for the save file instead of the most recent save
                            if (suspendExists)
                            {
                                Version v = Global.LOADED_VERSION;
                                using (Stream suspendStream = container.OpenFile(
                                    SuspendFilename(fileId), FileMode.Open, FileAccess.Read))
                                {
                                    try
                                    {
                                        using (BinaryReader suspendReader = new BinaryReader(suspendStream))
                                        {
                                            Global.LOADED_VERSION = LoadVersion(suspendReader);
                                            /* Call Deserialize */
                                            if (!Global.LOADED_VERSION.older_than(OLDEST_ALLOWED_SUSPEND_VERSION))
                                            {
                                                DateTime time = DateTime.FromBinary(suspendReader.ReadInt64());
                                                suspendInfo = Suspend_Info.read(suspendReader);
                                                Global.suspend_files_info[fileId] = suspendInfo;
                                            }
                                            else
                                                suspendExists = false;
                                        }
                                    }
                                    catch (EndOfStreamException e)
                                    {
                                        suspendExists = false;
                                    }
                                }
                                Global.LOADED_VERSION = v;
                            }

                            if (suspendInfo != null)
                            {
                                info = Save_Info.get_save_info(fileId, data.File, suspendInfo, map_save: mapSaveExists, suspend: suspendExists);
                            }
                            else
                                info = Save_Info.get_save_info(fileId, data.File, suspendExists);

                            // Copy transient file info (last chapter played, last time started)
                            if (oldSaveFilesInfo != null &&
                                oldSaveFilesInfo.ContainsKey(fileId))
                            {
                                var oldInfo = oldSaveFilesInfo[fileId];
                                info.CopyTransientInfo(oldInfo);
                            }

                            // Set the file info into the dictionary
                            Global.save_files_info[fileId] = info;

                            if (index == -1 || info.time > modifiedTime)
                            {
                                index = fileId;
                                modifiedTime = info.time;
                                if (STARTING)
                                {
                                    Global.game_options = data.Options;  //@Debug: // This needs to only happen when just starting the game
                                    STARTING = false;
                                }
                            }
                            i++;
                        }
                        catch (EndOfStreamException e)
                        {
                            saveFiles.RemoveAt(i);
                            continue;
                        }
                    }
                }
                // If no files were loaded successfully, the index will still be -1
                if (index == -1)
                {
                    // Since nothing was loaded, null things back out
                    Global.save_files_info = null;
                    Global.suspend_files_info = null;
                    Global.checkpoint_files_info = null;
                    Global.latest_save_id = -1;
                    return;
                }
                Global.latest_save_id = index;
                /* Dispose the StorageContainer */
                // Dispose the container, to commit changes.
            }

            RefreshSuspendScreenshots();
        }

        private bool ValidSaveVersion(Version loadedVersion)
        {
            // If game version is older than the save, don't load the save
            if (Global.save_version_too_new(loadedVersion))
                return false;

            if (loadedVersion.older_than(OLDEST_ALLOWED_SAVE_VERSION))
                return false;

            return true;
        }

        private void RefreshSuspendScreenshots()
        {
            // Dispose old screenshots
            Global.dispose_suspend_screenshots();
            // Load new ones from each suspend info
            if (Global.suspend_files_info != null)
            {
                // Associate each suspend info with its loaded screenshot
                if (Global.suspend_file_info != null)
                    Global.suspend_file_info.load_screenshot("recent");

                foreach (var suspend in Global.suspend_files_info)
                {
                    string name = SuspendFilename(suspend.Key);
                    suspend.Value.load_screenshot(name);
                }
                foreach (var mapSave in Global.checkpoint_files_info)
                {
                    string name = MapSaveFilename(mapSave.Key);
                    mapSave.Value.load_screenshot(name);
                }
            }
        }
        #endregion

        private Version LoadVersion(BinaryReader reader)
        {
            return new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }
        #endregion

        #region Save/Load Config
        private void SaveConfig()
        {
            string filename = "config" + Config.SAVE_FILE_EXTENSION;
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                // Close the wait handle
                result.AsyncWaitHandle.Close();

                if (container != null)
                {
                    // Delete file if it already exists
                    if (container.FileExists(filename))
                        container.DeleteFile(filename);
                    using (Stream stream = container.CreateFile(filename))
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            Version version = Global.RUNNING_VERSION;
                            writer.Write(version.Major);
                            writer.Write(version.Minor);
                            writer.Write(version.Build);
                            writer.Write(version.Revision);

                            writer.Write(GameRenderer.ZOOM); //@Debug
                            writer.Write(Global.fullscreen);
                            writer.Write(Global.stereoscopic_level);
                            writer.Write(Global.anaglyph);
                            writer.Write((int)Global.metrics);
                            writer.Write(Global.updates_active);
                            writer.Write(Global.rumble);
                            FEXNA.Input.write(writer);
                        }
                    }
                }
            }
        }
        private void LoadConfig()
        {
            string filename = "config" + Config.SAVE_FILE_EXTENSION;
            /* Create Storage Containter*/
            // Open a storage container.
            IAsyncResult result =
                Storage.BeginOpenContainer(SaveLocation(), null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            using (StorageContainer container = Storage.EndOpenContainer(result))
            {
                // Close the wait handle
                result.AsyncWaitHandle.Close();
                // If the file doesn't exist, return
                if (container == null || !container.FileExists(filename))
                {
                    return;
                }
                using (Stream stream = container.OpenFile(
                    filename, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Global.LOADED_VERSION = LoadVersion(reader);
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
                    }
                    catch (EndOfStreamException e)
                    {
                        ResetConfig();
                    }
                }
            }
        }

        private void ResetConfig()
        {
            Global.zoom = 1;
            Global.fullscreen = false;
            Global.stereoscopic_level = 0;
            Global.anaglyph = true;
            FEXNA.Input.default_controls();
        }
        #endregion
    }

    public interface ISaveCallbacker
    {
        void TitleLoadScene();
        void ArenaScene();

        void StartMoveRangeThread();
        void CloseMoveRangeThread();

        void FinishLoad();
    }

    struct Save_File_Data
    {
        public Game_Options Options;
        public Save_File File;
    }
}
