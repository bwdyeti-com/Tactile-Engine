using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
#if MONOGAME
using Microsoft.Xna.Framework.Audio;
#else
using MonoGame.Framework.Audio;
#endif
using Microsoft.Xna.Framework.Content;
using FEXNA_Library;

namespace FEXNA.Services.Audio
{
    class AudioNull
    {
        public class AudioNullService : IAudioService
        {
            private AudioNull _audio;

            public AudioNullService()
            {
                _audio = new AudioNull();
            }
            
            public void update() { }

            public void post_update() { }

            public void set_pitch_global_var(string var_name, float value) { }

            public void stop() { }

            #region BGM
            public void PlayBgm(string cueName, bool fadeIn = false, bool forceRestart = false) { }

            public void PlayMapTheme(string cueName, bool fadeIn = false) { }
            public void ResumeMapTheme(string cueName) { }

            public void PlayBattleTheme(string cueName, bool fadeIn = false) { }
            public void ResumeBattleTheme(string cueName) { }

            public bool BgmIsPlaying(string cueName) { return false; }
            public bool IsTrackPlaying(string trackName) { return false; }
            public string BgmTrackCueName(string trackName) { return null; }

            public void StopBgm() { }

            public void BgmFadeOut() { }
            public void BgmFadeOut(int time) { }

            public void DuckBgmVolume() { }
            public void RestoreBgmVolume() { }
            #endregion

            #region BGS
            public void play_bgs(string cue_name) { }

            public void stop_bgs() { }
            #endregion

            #region SFX
            public void play_se(string bank, string cue_name,
                Maybe<float> pitch,
                Maybe<int> channel,
                bool duckBgm) { }
            public void play_system_se(string bank, string cue_name, bool priority, Maybe<float> pitch = default(Maybe<float>)) { }

            public bool playing_system_sound() { return false; }
            public void cancel_system_sound() { }

            public void stop_sfx() { }

            public void sfx_fade() { }
            public void sfx_fade(int time) { }
            #endregion

            #region ME
            public void play_me(string bank, string cue_name) { }

            public bool stop_me() { return false; }
            public bool stop_me(bool bgm_stop) { return false; }
            #endregion

#if DEBUG && WINDOWS
            public AudioDiagnostics AudioDiagnostics() { return new AudioDiagnostics(); }
#endif
        }
    }
}
