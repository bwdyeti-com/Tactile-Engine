using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Menus.Save;

namespace Tactile
{
    enum Save_Phases { Load_File, Cleanup, Save_Data, Overwrite_Confirm, World_Map }
    class Scene_Save : Scene_Base, ISaveMenuHandler
    {
        const int WAIT_TIME = 20;

        protected Save_Phases Phase = Save_Phases.Load_File;
        protected int Timer;
        private SaveMenuManager MenuManager;

        //protected int Next_Chapter_Index = 0;
        protected bool Saving_Complete = false;
        protected bool Hard = false;
        protected bool All_Saves_Overwritten = true;

        public override void update()
        {
            Player.update_anim();
            switch (Phase)
            {
                case Save_Phases.Load_File:
                    Global.load_save_file = true;
                    Global.ignore_options_load = true;
                    Phase = Save_Phases.Cleanup;
                    break;
                case Save_Phases.Cleanup:
                    Global.game_actors.heal_battalion();
                    Global.game_actors.temp_clear();
                    Global.current_save_info.ResetStartedChapter();
                    if (Global.game_system.Style != Mode_Styles.Classic)
                        Global.save_file.Difficulty = Global.game_system.Difficulty_Mode;

                    Phase = Save_Phases.Save_Data;
                    break;
                case Save_Phases.Save_Data:
                    if (MenuManager != null)
                    {
                        MenuManager.Update();
                        if (MenuManager.Finished)
                            MenuManager = null;
                    }
                    else
                        UpdateSaveData();
                    break;
                case Save_Phases.Overwrite_Confirm:
                    MenuManager.Update();
                    break;
                case Save_Phases.World_Map:
                    // Advance after save complete
                    if (!this.save_data_calling)
                    {
                        if (Timer > 0)
                            Timer--;
                        else
                        {
                            Global.Audio.StopBgm();

                            Global.scene_change("Scene_Worldmap");
                            if (All_Saves_Overwritten)
                                Global.delete_suspend = true;
                            Global.load_save_info = true;
                            Global.skipUpdatingFileId = true;
                        }
                    }
                    break;
            }
        }

        private void UpdateSaveData()
        {
            if (Timer > 0)
                Timer--;
            else
            {
                if (Global.save_file == null)
                    Phase = Save_Phases.World_Map;
                else
                {
                    // If we've run out of chapters to write to, write the save file
                    if (Saving_Complete)
                    {
                        Timer = WAIT_TIME;
                        // Save file
                        CallSaveData();
                        Phase = Save_Phases.World_Map;
                    }
                    // Check if data for this file exists already
                    else
                    {
                        // If so test if we actually want to overwrite
                        if (ConfirmOverwrite(Global.game_state.chapter_id))
                        {
                            MenuManager = new SaveMenuManager(this);
                            Phase = Save_Phases.Overwrite_Confirm;
                        }
                        // Otherwise save the file
                        else
                        {
                            Save();
                            close_save_overwrite_window();
                        }
                    }
                }
            }
        }

        private bool ConfirmOverwrite(string chapterId)
        {
            // Never confirm overwrite on unranked chapters
            if (Global.data_chapters[chapterId].Unranked)
                return false;

            return Global.save_file.ContainsKey(chapterId);
        }

        protected void close_save_overwrite_window()
        {
            if (Hard)
            {
                Hard = false;
                Saving_Complete = true;
            }
            else
            {
                /* //Debug
                if (Global.game_system.hard_mode)
                    Hard = true;
                else*/
                Saving_Complete = true;
            }
            Phase = Save_Phases.Save_Data;
            Timer = WAIT_TIME;
        }

        private static void Save()
        {
            foreach (string progression_id in Global.game_system.Chapter_Save_Progression_Keys)
                Global.save_file.save_data(Global.game_state.chapter_id,
                    progression_id, Global.game_system.rankings);
        }

        public override void draw(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            if (MenuManager != null)
            {
                MenuManager.Draw(sprite_batch, device, render_targets);
            }
            /* //#Debug
            if (Save_Overwrite_Window != null)
            {
                Save_Overwrite_Window.draw(sprite_batch);
                if (Save_Overwrite_Window.is_ready)
                {
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    Chapter_Name.draw(sprite_batch);
                    sprite_batch.End();
                }
            }*/
        }

        #region ISaveMenuHandler
        public void SaveOverwriteConfirm()
        {
            //string next_chapter = Global.game_system.Chapter_Save_Progression_Keys[Next_Chapter_Index]; //Debug
            //Global.save_file.save_data(chapter, next_chapter);
            Save();

            close_save_overwrite_window();
        }

        public void SaveOverwriteCancel()
        {
            All_Saves_Overwritten = false;
            close_save_overwrite_window();
        }
        #endregion
    }
}
