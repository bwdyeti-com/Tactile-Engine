using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#if MONOGAME
using Microsoft.Xna.Framework.Audio;
#else
using MonoGame.Framework.Audio;
#endif

namespace Tactile.Services.Audio
{
    class MusicPlayer : IDisposable
    {
        const int SIMULTANEOUS_TRACKS = 4;
        private Dictionary<string, MusicInstance> Music = new Dictionary<string, MusicInstance>();
        private List<MusicCue> CuedTracks = new List<MusicCue>();
        // Used for ducking and restoring music level
        private float Volume = 1f, TargetVolume = 1f;
        public bool IsBgmDucked { get; private set; }
        private bool GameInactive;
        private SoundEffectInstance MusicEffect;
        private List<string> MusicPausedForME = new List<string>();

        public int DefaultFadeInTime { get { return 30; } }
        public int DefaultFadeOutTime { get { return 60; } }
        private float BgmVolume
        {
            get
            {
                if (GameInactive && Global.gameSettings.Audio.MuteWhenInactive)
                    return 0;

                return Volume * Global.gameSettings.Audio.MusicVolume / 100f;
            }
        }
        private bool TracksFadingOut { get { return Music.Any(x => x.Value.IsFadeOut); } }
        private bool QueueNewTracks { get { return MusicEffect != null || this.TracksFadingOut; } }
        public bool IsBgmPlaying { get { return Music.Any(x => x.Value.IsPlaying); } }

        #region Controls
        public void TryPlay(string bgmName, string trackName = "", bool fadeIn = false, bool resume = false)
        {
            // Wait for any fading tracks to finish
            CuedTracks.Add(new MusicCue(bgmName, trackName, fadeIn, resume));

            PlayCuedTracks();
        }
        public void Play(MusicCue track)
        {
            // Set volume immediately if starting a song from silence
            if (!IsBgmPlaying)
                Volume = TargetVolume;

            // Resume the existing track
            if (track.Resume && CueAlreadyExists(track))
            {
                // If this track is already playing normally or
                // fading back in, do nothing
                if (!(Music[track.TrackName].IsPlaying &&
                        !Music[track.TrackName].IsFadeOut))
                    Restore(track.TrackName, track.FadeIn);
                return;
            }

            // Pause any other playing tracks
            PauseOther(track.TrackName);

            StopPlayingTrack(track.TrackName);

            SoundEffectInstance instance = null;
            try
            {
                instance = get_music(track.CueName);
            }
            catch (FileNotFoundException e)
            {
#if DEBUG
                Print.message("Tried to play nonexistant BGM: " + track.CueName);
#endif
            }
            catch (ContentLoadException e)
            {
#if DEBUG
                Print.message("Tried to play nonexistant BGM: " + track.CueName);
#endif
            }
#if __ANDROID__
            catch (Java.IO.FileNotFoundException e)
            {
            }
#endif

            if (instance != null)
            {
                Music.Add(track.TrackName,
                    new MusicInstance(instance, track.CueName, this.BgmVolume));
                if (track.FadeIn)
                    Music[track.TrackName].FadeIn(this.DefaultFadeInTime);
                Music[track.TrackName].RefreshVolume(this.BgmVolume);
                Music[track.TrackName].Play();
            }

            if (Music.Count > SIMULTANEOUS_TRACKS)
            {
                throw new Exception();
            }
        }

        public void Restore(string bgmName, string trackName = "", bool fadeIn = false, bool forceRestart = false)
        {
            // If track exists
            if (CueAlreadyExists(trackName, bgmName))
            {
                // This is the active track again, so clear the other tracks
                CuedTracks.Clear();

                // Resume if track playing
                if (TrackPlayingCue(trackName, bgmName))
                {
                    Restore(trackName, fadeIn);
                }
                else
                {
                    // Fade in if other tracks are fading out first
                    fadeIn |= this.TracksFadingOut;
                    TryPlay(bgmName, trackName, fadeIn, !forceRestart);
                }
            }
            // Play new track
            else
                TryPlay(bgmName, trackName, fadeIn, !forceRestart);
        }
        private void Restore(string trackName, bool fadeIn)
        {
            // Also pause any other tracks
            PauseOther(trackName);

            if (fadeIn)
                Music[trackName].FadeIn(this.DefaultFadeInTime);
            Music[trackName].Play();
        }

        public void Resume(string bgmName, string trackName)
        {
            // Resume and fade in if track exists
            if (CueAlreadyExists(trackName, bgmName))
            {
                TryPlay(bgmName, trackName, true, true);
            }
            // Play new track
            else
            {
                TryPlay(bgmName, trackName, false);
            }
        }

        public void Pause()
        {
            foreach (var pair in Music)
                pair.Value.Pause();
        }
        public void PauseOther(string trackName)
        {
            foreach (var pair in Music)
                if (pair.Key != trackName)
                    pair.Value.Pause();
        }

        /// <summary>
        /// Stops all music playback and clears all instances and cues.
        /// </summary>
        public void Stop()
        {
            foreach (var pair in Music)
                pair.Value.Dispose();
            Music.Clear();
            CuedTracks.Clear();
            MusicPausedForME.Clear();

            RestoreBgmVolume();
            Volume = TargetVolume;
            
            //@Debug: mostly sure this should be here
            StopMusicEffect();
        }
        /// <summary>
        /// Stops music playing on a specific track and clears any cues for
        /// that track.
        /// </summary>
        public void Stop(string trackName)
        {
            StopPlayingTrack(trackName);

            CuedTracks = CuedTracks
                .Where(x => x.TrackName != trackName)
                .ToList();
        }

        private void StopPlayingTrack(string trackName)
        {
            if (Music.ContainsKey(trackName))
            {
                Music[trackName].Dispose();
                Music.Remove(trackName);
            }

            MusicPausedForME.Remove(trackName);
        }

        public void FadeOut(int time)
        {
            foreach (var pair in Music)
                pair.Value.FadeOut(time);

            // Cancel any existing cues, they're effectively
            // fading from 0 current volume to 0 volume
            CuedTracks.Clear();
        }

        public bool IsPlaying(string bgmName)
        {
            return Music.Any(x => x.Value.BgmName == bgmName);
        }
        public bool IsTrackPlaying(string trackName)
        {
            return Music.ContainsKey(trackName) && Music[trackName].IsPlaying;
        }
        public string BgmTrackCueName(string trackName)
        {
            if (Music.ContainsKey(trackName))
                return Music[trackName].BgmName;
            return null;
        }
        #endregion

        public void DuckBgmVolume()
        {
            TargetVolume = 0.5f;
            IsBgmDucked = true;
        }
        public void RestoreBgmVolume()
        {
            TargetVolume = 1f;
            IsBgmDucked = false;
        }

        #region Music Effect
        public void PlayMusicEffect(SoundEffectInstance instance)
        {
            // If a music effect already exists, replace it
            if (MusicEffect != null)
            {
                MusicEffect.Stop();
                MusicEffect.Dispose();
            }
            else
                MusicPausedForME.Clear();

            foreach (var pair in Music)
            {
                if (pair.Value.IsPlaying)
                {
                    pair.Value.Pause();
                    MusicPausedForME.Add(pair.Key);
                }
            }

            MusicEffect = instance;
            MusicEffect.Volume = this.BgmVolume;
            MusicEffect.Play();
        }

        public bool StopMusicEffect()
        {
            bool result = false;

            if (MusicEffect != null)
            {
                MusicEffect.Stop();
                MusicEffect.Dispose();
                result = true;
            }
            MusicEffect = null;

            foreach (string track in MusicPausedForME)
                if (Music.ContainsKey(track))
                    Music[track].Play();

            return result;
        }
        #endregion

        #region Update
        public void Update(bool gameInactive)
        {
            GameInactive = gameInactive;

            if (Volume != TargetVolume)
                Volume = (float)Additional_Math.double_closer(Volume, TargetVolume, 0.08f);

            UpdateBgm();
            UpdateMusicEffect();

            // Play any tracks waiting to start
            PlayCuedTracks();
        }

        private void UpdateBgm()
        {
            foreach (var pair in Music)
                pair.Value.Update(this.BgmVolume);

            // Check if any songs ended
            foreach (var trackName in Music.Keys.ToList())
            {
                if (Music[trackName].Finished)
                    StopPlayingTrack(trackName);
            }
        }

        private void UpdateMusicEffect()
        {
            if (MusicEffect != null)
            {
                MusicEffect.Volume = this.BgmVolume;
                // Check if the music effect ended
                if (MusicEffect.State == SoundState.Stopped)
                    StopMusicEffect();
            }
        }
        #endregion

        private void PlayCuedTracks()
        {
            if (CuedTracks.Any())
            {
                // Any cue wants to resume on a track that is fading out
                bool replaceFadingTrack = CuedTracks
                    .Any(x => x.Resume && TrackPlayingCue(x) &&
                        Music[x.TrackName].IsFadeOut);
                if (!this.QueueNewTracks || replaceFadingTrack)
                {
                    foreach (var cuedTrack in CuedTracks)
                        Play(cuedTrack);
                    CuedTracks.Clear();
                }
            }
        }

        private bool TrackPlayingCue(MusicCue cue)
        {
            return TrackPlayingCue(cue.TrackName, cue.CueName);
        }
        private bool TrackPlayingCue(string trackName, string cueName)
        {
            return CueAlreadyExists(trackName, cueName) && Music[trackName].IsPlaying;
        }

        private bool CueAlreadyExists(MusicCue cue)
        {
            return CueAlreadyExists(cue.TrackName, cue.CueName);
        }
        private bool CueAlreadyExists(string trackName, string cueName)
        {
            return Music.ContainsKey(trackName) && Music[trackName].BgmName == cueName;
        }

        #region Sound Effect Source
        private static SoundEffectInstance get_music(string cue_name)
        {
            SoundEffect song = null;
            SoundEffectInstance music = null;
            int intro_start = 0, loop_start = -1, loop_length = -1;

            NVorbis.VorbisReader vorbis = null;
            try
            {
                try
                {
                    Stream cue_stream = TitleContainer.OpenStream(@"Content\Audio\BGM\" + cue_name + ".ogg");


#if __ANDROID__
					MemoryStream stream = new MemoryStream();
					cue_stream.CopyTo(stream);
                    vorbis = new NVorbis.VorbisReader(stream, cue_name, true);
#else
                    vorbis = new NVorbis.VorbisReader(cue_stream, cue_name, true);
#endif
                    get_loop_data(vorbis, out intro_start, out loop_start, out loop_length);


                    // If the loop points are set past the end of the song, don't play
                    if (vorbis.TotalSamples < loop_start || vorbis.TotalSamples < loop_start + loop_length)
                    {
#if DEBUG
                        throw new IndexOutOfRangeException("Loop points are set past the end of the song");
#endif
#if __ANDROID__
                        cue_stream.Dispose();
                        vorbis.Dispose();
#else
                        vorbis.Dispose();
#endif
                        throw new FileNotFoundException();
                    }
#if __ANDROID__
					cue_stream.Dispose();
#endif

                }
                catch (FileNotFoundException ex)
                {
                    throw;
                }
#if __ANDROID__
                catch (Java.IO.FileNotFoundException e)
                {
                    throw;
                }
#endif
            }
            // If loaded as an ogg failed, try loading as a SoundEffect
            catch (FileNotFoundException ex)
            {
                intro_start = 0;
                loop_start = -1;
                loop_length = -1;
                song = Global.Content.Load<SoundEffect>(@"Audio/" + cue_name);
            }

            // If the file is an ogg file and was found and initialized successfully
            if (vorbis != null)
            {
                music = get_vorbis_music(vorbis, cue_name,
                    intro_start, loop_start, loop_length);
            }
            else
            {
                music = get_effect_music(song, cue_name, intro_start, loop_start, loop_length);
            }

            if (music != null)
                music.IsLooped = true;
#if !__ANDROID__
            if (song != null)
                song.Dispose();
#endif
            return music;
        }
        private static SoundEffectInstance get_vorbis_music(
            NVorbis.VorbisReader vorbis, string cue_name,
            int intro_start, int loop_start, int loop_length)
        {
            SoundEffectInstance music;
            //@Yeti: Audio Merge
            SoundEffect sound_effect = SoundEffectStreamed.FromVorbis(
                vorbis, intro_start, loop_start, loop_start + loop_length);
            music = sound_effect.CreateInstance();
            music.AlsoDisposeEffect();

            return music;
        }
        private static SoundEffectInstance get_effect_music(
            SoundEffect song, string cue_name,
            int intro_start, int loop_start, int loop_length)
        {
            SoundEffectInstance music;
            //@Yeti: Audio Merge
            //@Debug: also the loop points don't matter for this function,
            // even for the old code because real numbers were never passed in?
            // Could be worth reworking for the functionality though
            if (song == null)
                return null;
            music = song.CreateInstance();
            music.AlsoDisposeEffect();

            return music;
        }

        private static void get_loop_data(
            NVorbis.VorbisReader reader,
            out int intro_start,
            out int loop_start,
            out int loop_length)
        {
            string[] comments = reader.Comments;
            get_loop_data(comments, out intro_start, out loop_start, out loop_length);
        }
        private static void get_loop_data(
            string[] comments,
            out int intro_start,
            out int loop_start,
            out int loop_length)
        {
            intro_start = 0;
            loop_start = -1;
            loop_length = -1;

            string[] str_ary;
            for (int i = 0; i < comments.Length; i++)
            {
                str_ary = comments[i].Split('\0');
                str_ary = str_ary[0].Split('=');
                switch (str_ary[0])
                {
                    case "INTROSTART":
                        intro_start = Convert.ToInt32(str_ary[1]);
                        break;
                    case "LOOPSTART":
                        loop_start = Convert.ToInt32(str_ary[1]);
                        break;
                    case "LOOPLENGTH":
                        loop_length = Convert.ToInt32(str_ary[1]);
                        break;
                }
            }
            if (loop_start == -1 || loop_length == -1)
            {
                loop_start = -1;
                loop_length = -1;
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            foreach (var pair in Music)
                pair.Value.Dispose();
            Music = null;

            if (MusicEffect != null)
                MusicEffect.Dispose();
            MusicEffect = null;
        }
        #endregion

#if DEBUG && WINDOWS
        public IEnumerable<string> BgmDiagnostics()
        {
            foreach (var pair in Music)
            {
                string stateString = "Playing";
                if (!pair.Value.IsPlaying)
                    stateString = "Stopped";
                else if (pair.Value.IsFadeOut)
                    stateString = string.Format("Fade Out: {0}%",
                        (int)(pair.Value.FadeVolume * 100));
                else if (pair.Value.IsFadeIn)
                    stateString = string.Format("Fade In: {0}%",
                        (int)(pair.Value.FadeVolume * 100));

                string result = string.Format(
                    "{0}:    {1} - ({2})",
                    pair.Key,
                    pair.Value.BgmName,
                    stateString);
                yield return result;
            }
        }

        public IEnumerable<string> PendingBgmDiagnostics()
        {
            foreach (var cued in CuedTracks)
            {
                string result = string.Format(
                    "{0}:    {1}",
                    cued.TrackName,
                    cued.CueName);
                yield return result;
            }
        }
#endif
    }

    struct MusicCue
    {
        public string CueName { get; private set; }
        public string TrackName { get; private set; }
        public bool FadeIn { get; private set; }
        public bool Resume { get; private set; }

        public MusicCue(string cueName, string trackName, bool fadeIn, bool resume)
            : this()
        {
            CueName = cueName;
            TrackName = trackName;
            FadeIn = fadeIn;
            Resume = resume;
        }
    }
}
