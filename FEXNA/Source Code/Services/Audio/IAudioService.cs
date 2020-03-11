using System.Collections.Generic;
using FEXNA_Library;

namespace FEXNA.Services.Audio
{
    interface IAudioService
    {
        void update();

        void post_update();

        void set_pitch_global_var(string var_name, float value);

        void stop();

        #region BGM
        void PlayBgm(string cueName, bool fadeIn = false, bool forceRestart = false);

        void PlayMapTheme(string cueName, bool fadeIn = false);
        void ResumeMapTheme(string cueName);

        void PlayBattleTheme(string cueName, bool fadeIn = false);
        void ResumeBattleTheme(string cueName);

        bool BgmIsPlaying(string cueName);
        bool IsTrackPlaying(string trackName);
        string BgmTrackCueName(string trackName);

        void StopBgm();

        void BgmFadeOut();
        void BgmFadeOut(int time);

        void DuckBgmVolume();
        void RestoreBgmVolume();
        #endregion

        #region BGS
        void play_bgs(string cue_name);

        void stop_bgs();
        #endregion

        #region SFX
        void play_se(string bank, string cue_name,
            Maybe<float> pitch = default(Maybe<float>),
            Maybe<int> channel = default(Maybe<int>),
            bool duckBgm = false);
        void play_system_se(string bank, string cue_name,
            bool priority,
            Maybe<float> pitch);

        bool playing_system_sound();
        void cancel_system_sound();

        void stop_sfx();

        void sfx_fade();
        void sfx_fade(int time);
        #endregion

        #region ME
        void play_me(string bank, string cue_name);

        bool stop_me();
        bool stop_me(bool bgm_stop);
        #endregion

#if DEBUG && WINDOWS
        AudioDiagnostics AudioDiagnostics();
#endif
    }
}
