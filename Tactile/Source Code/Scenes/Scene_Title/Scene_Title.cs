using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if DEBUG
using Microsoft.Xna.Framework.Content;
#endif
using Tactile.Menus.Title;

namespace Tactile
{
    enum TitleThemeStarts { None, PlayTheme, FadeInTheme, Transition }

    class Scene_Title : Scene_Base, ITitleMenuHandler
    {
        private bool SkipIntro = false;
        private TitleThemeStarts TitleTheme = TitleThemeStarts.None;
        protected int Timer = 0;
        protected int Class_Reel_Timer = 0;
        private bool Closing = false;

        private TitleMenuManager MenuManager;

        protected bool Starting = false;
        protected bool Loading_Suspend = false, Quitting = false;

#if DEBUG
        public Texture2D get_team_map_sprite(int team, string name)
        {
            return Scene_Map.get_team_map_sprite(team, name);
        }
#endif
        public Scene_Title(bool intro)
        {
            SkipIntro = !intro;
            Global.storage_selection_requested = true;
            Global.load_config = true;
            Global.game_map = null;
            initialize();
        }

        protected void initialize()
        {
            initialize_base();
            Scene_Type = "Scene_Title";

            if (SkipIntro)
            {
                if (Global.scene.scene_type == "Scene_Soft_Reset")
                {
                    MenuManager = TitleMenuManager.MainMenu(this);
                    TitleTheme = TitleThemeStarts.FadeInTheme;
                }
                else
                {
                    MenuManager = TitleMenuManager.TitleScreen(this);
                    TitleTheme = TitleThemeStarts.PlayTheme;
                }
            }
            else
            {
                MenuManager = TitleMenuManager.Intro(this);
                TitleTheme = TitleThemeStarts.Transition;
            }

            Global.load_save_info = true;
            Global.check_for_updates();
        }

        public override bool fullscreen_switch_blocked()
        {
            if (MenuManager != null && MenuManager.FullscreenSwitchBlocked)
                return true;

            return base.fullscreen_switch_blocked();
        }

        #region ITitleMenuHandler
        public void TitleClassReel()
        {
            MenuManager = null;
            Global.scene_change("Scene_Class_Reel");
        }

        public void TitleResume()
        {
            Global.call_load_suspend();
            Loading_Suspend = true;
            Closing = true;
            Timer = 0;
        }

        public void TitleNewGame(int fileId, Mode_Styles style, Difficulty_Modes difficulty)
        {
            Global.save_file = new Tactile.IO.Save_File();
            Global.save_file.Style = style;
            Global.save_file.Difficulty = difficulty;

            Global.game_options.reset_options();
            Global.start_game_file_id = fileId;
            Global.start_new_game = true;

            // Save file on creation
            CallSaveData();
            Loading_Suspend = false;
            Closing = true;
            Timer = 0;
            if (Global.save_files_info == null)
                Global.save_files_info = new Dictionary<int, Tactile.IO.Save_Info>();
            Global.save_files_info.Add(
                Global.start_game_file_id, Tactile.IO.Save_Info.new_file());
        }

        public void TitleStartGame(int fileId)
        {
            Global.start_game_file_id = fileId;
            Global.load_save_file = true;
            Loading_Suspend = false;
            Closing = true;
            Timer = 0;
        }

        public void TitleLoadSuspend(int fileId)
        {
            Global.start_game_file_id = fileId;
            Global.call_load_suspend();
            Loading_Suspend = true;
            Closing = true;
            Timer = 0;
        }

        public void TitleLoadCheckpoint(int fileId)
        {
            Global.start_game_file_id = fileId;
            Suspend_Filename = Config.MAP_SAVE_FILENAME;
            Global.call_load_suspend();
            Loading_Suspend = true;
            Closing = true;
            Timer = 0;
        }

        public void TitleSaveConfig()
        {
            Global.save_config = true;
        }

#if !MONOGAME && DEBUG
        public void TitleTestBattle(int distance)
        {
            MenuManager = null;

            Global.game_system.Battler_1_Id = Global.test_battler_1.Actor_Id;
            Global.game_system.Battler_2_Id = Global.test_battler_2.Actor_Id;

            Global.game_system.Arena_Distance = distance;
            Global.game_system.In_Arena = true;
            Global.scene_change("Scene_Test_Battle");
        }
#endif

        public void TitleSupportConvo(string supportKey, int level, bool atBase, string background)
        {
            if (Global.data_supports.ContainsKey(supportKey))
            {
                var supportData = Global.data_supports[supportKey];
                if (level < supportData.Supports.Count)
                {
                    string convoName = supportData.Supports[level].ConvoName(atBase);
                    if (Global.supports.ContainsKey(convoName))
                    {
                        Global.game_temp.message_text += Global.supports[convoName];
                        // Add a background
                        if (!Global.game_temp.message_text.StartsWith("\\g["))
                        {
                            Global.game_temp.message_text = string.Format("\\g[{0}]{1}",
                                background, Global.game_temp.message_text);
                        }
                        Global.scene.new_message_window();
                    }
                }
            }
        }

        public void TitleOpenCommunityLink(int index)
        {
#if !__MOBILE__
            if (Global.gameSettings.Graphics.Fullscreen)
            {
                // Switch off fullscreen
                Global.gameSettings.Graphics.ConfirmSetting(
                    Tactile.Options.GraphicsSetting.Fullscreen, 0, false);
                Global.save_config = true;
            }
#endif

            System.Diagnostics.Process.Start(
                string.Format("http://{0}", CommunityMenu.COMMUNITY_ENTRIES[index].Url));
        }

        public void TitleOpenFullCredits()
        {
#if !__MOBILE__
            if (Global.gameSettings.Graphics.Fullscreen)
            {
                // Switch off fullscreen
                Global.gameSettings.Graphics.ConfirmSetting(
                    Tactile.Options.GraphicsSetting.Fullscreen, 0, false);
                Global.save_config = true;
            }
#endif
            
            System.Diagnostics.Process.Start(
                string.Format("http://{0}", Constants.Credits.FULL_CREDITS_LINK));
        }

        public void TitleQuit()
        {
            Quitting = true;
            Closing = true;
            Timer = 0;
            Global.Audio.BgmFadeOut(Config.TITLE_GAME_START_TIME * 2);
        }
        #endregion

        #region Update
        public void update(KeyboardState key_state)
        {
            update_message();

            Player.update_anim();
            if (update_soft_reset())
                return;

            if (!Global.load_save_info)
            {
                if (TitleTheme != TitleThemeStarts.None)
                {
                    switch (TitleTheme)
                    {
                        case TitleThemeStarts.PlayTheme:
                            Global.Audio.PlayBgm(Global.BgmConfig.TitleTheme);
                            break;
                        case TitleThemeStarts.FadeInTheme:
                            Global.Audio.PlayBgm(Global.BgmConfig.TitleTheme, true);
                            break;
                        case TitleThemeStarts.Transition:
                            Global.Audio.PlayBgm(Global.BgmConfig.ChapterTransitionTheme);
                            break;
                    }
                    TitleTheme = TitleThemeStarts.None;
                }
            }

            if (Closing)
            {
                // If trying to load suspend and failed
                if (Loading_Suspend && !Global.suspend_load_successful)
                {
                    Loading_Suspend = false;
                    Closing = false;
                }
                else
                {
                    Timer++;
                    if (Timer >= Config.TITLE_GAME_START_TIME)
                    {
                        // If shutting down
                        if (Quitting)
                            Global.quit();
                        // If loading suspend
                        else if (Loading_Suspend)
                            Global.scene_change("Load_Suspend");
                        // Else start game
                        else
                        {
                            Global.game_system.Difficulty_Mode = Global.save_file.Difficulty;
                            start_game();
                        }
                        MenuManager = null;
                        return;
                    }
                }
            }

            MenuManager.Update(!Closing);
            SoftResetBlocked = MenuManager != null && MenuManager.SoftResetBlocked;
        }

        protected void start_game()
        {
            Global.scene_change("Start_Game");
            Global.Audio.BgmFadeOut(30);
        }
        #endregion

        #region Draw
        public override void draw(SpriteBatch sprite_batch, GraphicsDevice device, RenderTarget2D[] render_targets)
        {
            device.SetRenderTarget(render_targets[1]);
            device.Clear(Color.Transparent);

            // Draw Menus
            if (MenuManager != null)
                MenuManager.Draw(sprite_batch, device, render_targets);

            // Draw Convos
            draw_message(sprite_batch, device, render_targets);

            device.SetRenderTarget(render_targets[0]);
            device.Clear(Color.Transparent);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            int alpha = !Closing ? 255 :
                ((Config.TITLE_GAME_START_TIME - Timer) * 255 /
                    Config.TITLE_GAME_START_TIME);
            sprite_batch.Draw(render_targets[1], Vector2.Zero, new Color(alpha, alpha, alpha, alpha));
            sprite_batch.End();
        }
        #endregion
    }
}
