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
    class Audio_Engine
    {
        public class Audio_Service : IAudioService
        {
            private Audio_Engine _audio;

            public Audio_Service()
            {
                _audio = new Audio_Engine();
            }
            
            public void update()
            {
                _audio.update();
            }

            public void post_update()
            {
                _audio.post_update();
            }

            public void set_pitch_global_var(string var_name, float value)
            {
                _audio.set_pitch_global_var(var_name, value);
            }

            public void stop()
            {
                _audio.stop();
            }

            #region BGM
            public void PlayBgm(string cueName, bool fadeIn = false, bool forceRestart = false)
            {
                _audio.play_bgm(cueName, fadeIn, forceRestart);
            }

            public void PlayMapTheme(string cueName, bool fadeIn = false)
            {
                _audio.play_map_bgm(cueName, fadeIn);
            }
            public void ResumeMapTheme(string cueName)
            {
                _audio.resume_map_bgm(cueName);
            }

            public void PlayBattleTheme(string cueName, bool fadeIn = false)
            {
                _audio.play_battle_bgm(cueName, fadeIn);
            }
            public void ResumeBattleTheme(string cueName)
            {
                _audio.resume_battle_bgm(cueName);
            }

            public bool BgmIsPlaying(string cueName)
            {
                return _audio.is_playing(cueName);
            }
            public bool IsTrackPlaying(string trackName)
            {
                return _audio.IsTrackPlaying(trackName);
            }
            public string BgmTrackCueName(string trackName)
            {
                return _audio.BgmTrackCueName(trackName);
            }

            public void StopBgm()
            {
                _audio.stop_bgm();
            }

            public void BgmFadeOut()
            {
                _audio.bgm_fade();
            }
            public void BgmFadeOut(int time)
            {
                _audio.bgm_fade(time);
            }
            
            public void DuckBgmVolume()
            {
                _audio.DuckBgmVolume();
            }
            public void RestoreBgmVolume()
            {
                _audio.RestoreBgmVolume();
            }
            #endregion

            #region BGS
            public void play_bgs(string cue_name)
            {
                _audio.play_bgs(cue_name);
            }

            public void stop_bgs()
            {
                _audio.stop_bgs();
            }
            #endregion

            #region SFX
            public void play_se(string bank, string cue_name,
                Maybe<float> pitch = default(Maybe<float>),
                Maybe<int> channel = default(Maybe<int>),
                bool duckBgm = false)
            {
                _audio.play_se(bank, cue_name, pitch, channel, duckBgm);
            }
            public void play_system_se(string bank, string cue_name,
                bool priority,
                Maybe<float> pitch)
            {
                _audio.prepare_system_se(bank, cue_name, priority, pitch);
            }

            public bool playing_system_sound()
            {
                return _audio.playing_system_sound();
            }
            public void cancel_system_sound()
            {
                _audio.cancel_system_sound();
            }

            public void stop_sfx()
            {
                _audio.stop_sfx();
            }

            public void sfx_fade()
            {
                _audio.sfx_fade();
            }
            public void sfx_fade(int time)
            {
                _audio.sfx_fade(time);
            }
            #endregion

            #region ME
            public void play_me(string bank, string cue_name)
            {
                _audio.play_me(bank, cue_name);
            }

            public bool stop_me()
            {
                return _audio.stop_me();
            }
            public bool stop_me(bool bgm_stop)
            {
                return _audio.stop_me(bgm_stop);
            }
            #endregion

#if DEBUG && WINDOWS
            public AudioDiagnostics AudioDiagnostics()
            {
                return _audio.AudioDiagnostics();
            }
#endif
        }

        const int SIMULTANEOUS_SOUNDS = 24;
        private static Dictionary<string, int[]> LOOP_DATA = new Dictionary<string, int[]>();
        private static Dictionary<string, byte[]> SOUND_DATA = new Dictionary<string, byte[]>();

        private MusicPlayer BgmManager = new MusicPlayer();
        //private List<SoundEffectInstance> Playing_Sounds = new List<SoundEffectInstance>();
        private ChannelSound[] Playing_Sounds = new ChannelSound[SIMULTANEOUS_SOUNDS];
        private SoundEffectInstance System_Sound;
        private SoundEffectInstance Background_Sound;
        private bool System_SFX_Priority = false;
        private bool BgmDuckedBySfx;
        private int Bgs_Fade_Out_Time = 0, Bgs_Fade_Timer = 0;
        private int Sound_Fade_Out_Time = 0, Sound_Fade_Timer = 0;
        private float Sound_Volume = 1, Bgs_Volume = 1;
        private float Sound_Fade_Volume = 1, Bgs_Fade_Volume = 1;
        private Dictionary<string, float> PITCHES = new Dictionary<string, float>();
        private Sound_Name_Data New_System_Sound_Data;

        #region Accessors
        private bool music_muted { get { return Global.game_options.music_volume == 1; } }
        private bool sound_muted { get { return Global.game_options.sound_volume == 1; } }
        
        private bool bgs_fading_out { get { return Bgs_Fade_Out_Time > 0; } }
        private bool sound_fading_out { get { return Sound_Fade_Out_Time > 0; } }
        
        private bool too_many_active_sounds
        {
            get
            {
                return !Playing_Sounds.Any(x => x == null);
            }
        }
        #endregion

        private Audio_Engine() { }

        private void update()
        {
            BgmManager.Update();

            // Update volume
            UpdateSoundVolume();

            update_sounds();
            update_bgs_fade();
        }

        private void post_update()
        {
            if (New_System_Sound_Data != null)
                play_system_se(New_System_Sound_Data.Bank, New_System_Sound_Data.Name, New_System_Sound_Data.Priority, New_System_Sound_Data.Pitch);
            New_System_Sound_Data = null;
        }

        private void UpdateSoundVolume()
        {
            float volume = Global.gameSettings.Audio.SoundVolume / 100f;
            volume = MathHelper.Clamp(volume, 0, 1);

            if (Sound_Volume != volume)
            {
                set_bgs_volume(volume);
                set_sfx_volume(volume);
            }
        }

        protected void update_sounds()
        {
            for (int i = 0; i < Playing_Sounds.Length;)
            {
                if (Playing_Sounds[i] != null)
                    if (!Playing_Sounds[i].IsLooped && Playing_Sounds[i].State == SoundState.Stopped)
                    {
                        Playing_Sounds[i].Dispose();
                        pop_sound_ids(i);
                        continue;
                    }
                i++;
            }

            // Restore BGM volume if it was ducked by sound effects
            if (BgmDuckedBySfx)
            {
                if (BgmManager.IsBgmDucked)
                {
                    if (!Playing_Sounds.Any(x => x != null && x.DucksBgm))
                    {
                        BgmManager.RestoreBgmVolume();
                        BgmDuckedBySfx = false;
                    }
                }
                else
                    BgmDuckedBySfx = false;
            }

            if (System_Sound != null)
            {
                if (!System_Sound.IsLooped && System_Sound.State == SoundState.Stopped)
                {
                    cancel_system_sound();
                }
            }
            /*int i = 0;
            while (i < Playing_Sounds.Count)
            {
                if (!Playing_Sounds[i].IsPlaying)
                    Playing_Sounds.RemoveAt(i);
                else
                    i++;
            }*/
            update_sfx_fade();
        }

        protected void update_bgs_fade()
        {
            if (bgs_fading_out)
            {
                Bgs_Fade_Timer++;
                if (Bgs_Fade_Timer >= Bgs_Fade_Out_Time)
                {
                    cancel_bgs_fade_out();
                    stop_bgs();
                    //if (!sound_muted)
                        set_bgs_fade_volume(1);
                }
                else
                    //if (!sound_muted)
                        set_bgs_fade_volume((1f * (Bgs_Fade_Out_Time - Bgs_Fade_Timer)) / Bgs_Fade_Out_Time);
            }
        }

        protected void update_sfx_fade()
        {
            if (sound_fading_out)
            {
                Sound_Fade_Timer++;
                if (Sound_Fade_Timer >= Sound_Fade_Out_Time)
                {
                    cancel_sound_fade_out();
                    stop_sfx();
                    //if (!sound_muted)
                        set_sfx_fade_volume(1);
                }
                else
                    //if (!sound_muted)
                        set_sfx_fade_volume((1f * (Sound_Fade_Out_Time - Sound_Fade_Timer)) / Sound_Fade_Out_Time);
            }
        }

        private void set_pitch_global_var(string var_name, float value)
        {
            PITCHES[var_name] = value;
        }

        private void stop()
        {
            stop_bgm();
            stop_bgs();
            stop_sfx();
        }

        #region BGM
        private void play_bgm(string cueName, bool fadeIn, bool forceRestart)
        {
            if (forceRestart)
                BgmManager.Stop("Bgm");

            BgmManager.Restore(cueName, "Bgm", fadeIn, forceRestart);
        }

        private void play_map_bgm(string cueName, bool fadeIn = false)
        {
            BgmManager.TryPlay(cueName, "MapBgm");
        }
        private void resume_map_bgm(string cueName)
        {
            BgmManager.Resume(cueName, "MapBgm");
        }

        private void play_battle_bgm(string cueName, bool fadeIn = false)
        {
            BgmManager.TryPlay(cueName, "BattleBgm");
        }
        private void resume_battle_bgm(string cueName)
        {
            BgmManager.Resume(cueName, "BattleBgm");
        }
        
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
#if __ANDROID__
            SoundEffect sound_effect = SoundEffectStreamed.FromVorbis(
                vorbis, intro_start, loop_start, loop_start + loop_length);
            music = sound_effect.CreateInstance();
            music.AlsoDisposeEffect();
#else
            if (loop_start != -1)
                music = new SoundEffectInstance(vorbis, intro_start, loop_start, loop_start + loop_length);
            else
                music = new SoundEffectInstance(vorbis, 0, -1, -1);
#endif

            return music;
        }
        private static SoundEffectInstance get_effect_music(
            SoundEffect song, string cue_name,
            int intro_start, int loop_start, int loop_length)
        {
            SoundEffectInstance music;
#if __ANDROID__
            if (song == null)
                return null;
            music = song.CreateInstance();
            music.AlsoDisposeEffect();
#else
                if (loop_start != -1)
                    music = new SoundEffectInstance(song, intro_start, loop_start, loop_start + loop_length);
                else
                    music = new SoundEffectInstance(song);
#endif

            return music;
        }
        
        private bool is_playing(string cue_name)
        {
            return BgmManager.IsPlaying(cue_name);
        }
        private bool IsTrackPlaying(string trackName)
        {
            return BgmManager.IsTrackPlaying(trackName);
        }
        private string BgmTrackCueName(string trackName)
        {
            return BgmManager.BgmTrackCueName(trackName);
        }

        private void stop_bgm()
        {
            BgmManager.Stop();
        }
        
        private void bgm_fade()
        {
            bgm_fade(BgmManager.DefaultFadeOutTime);
        }
        private void bgm_fade(int time)
        {
            BgmManager.FadeOut(time);
            
            bgs_fade(time);
        }

        public void DuckBgmVolume()
        {
            BgmManager.DuckBgmVolume();
        }
        public void RestoreBgmVolume()
        {
            BgmManager.RestoreBgmVolume();
        }
        #endregion

        #region BGS
        private void set_bgs_volume(float volume)
        {
            Bgs_Volume = MathHelper.Clamp(volume, 0, 1);
            set_bgs_fade_volume(Bgs_Fade_Volume);
        }
        private void set_bgs_fade_volume(float volume)
        {
            Bgs_Fade_Volume = volume;
            if (Background_Sound != null)
                Background_Sound.Volume = volume * Bgs_Volume;
        }

        private void play_bgs(string cue_name)
        {
            stop_bgs();
            Background_Sound = get_music(cue_name); // maybe its own bank *folder //Debug
            if (Background_Sound == null)
                return;

            Background_Sound.Volume = Bgs_Fade_Volume * Bgs_Volume;
            Background_Sound.Play();
            //if (!sound_muted) //Debug
                set_bgs_fade_volume(1);
        }

        private void stop_bgs()
        {
            cancel_bgs_fade_out();
            if (Background_Sound != null)
            {
                Background_Sound.Stop();
                Background_Sound.Dispose();
            }
            Background_Sound = null;
        }

        private void bgs_fade(int time)
        {
            if (time > 0 && !bgs_fading_out)
            {
                //if (!sound_muted) //Debug
                    set_bgs_fade_volume(1);
                Bgs_Fade_Out_Time = time;
                Bgs_Fade_Timer = 0;
            }
        }

        private void cancel_bgs_fade_out()
        {
            Bgs_Fade_Out_Time = 0;
            Bgs_Fade_Timer = 0;
        }
        #endregion

        #region SFX
        private void set_sfx_volume(float volume)
        {
            Sound_Volume = MathHelper.Clamp(volume, 0, 1);
            set_sfx_fade_volume(Sound_Fade_Volume);
        }
        private void set_sfx_fade_volume(float volume)
        {
            Sound_Fade_Volume = volume;
            foreach (var sound in Playing_Sounds)
                if (sound != null)
                    sound.Instance.Volume = volume * Sound_Volume;
            if (System_Sound != null)
                System_Sound.Volume = volume * Sound_Volume;
        }

        private void play_se(
            string bank,
            string cue_name,
            Maybe<float> pitch,
            Maybe<int> channel,
            bool duckBgm)
        {
            // If too many sounds and not important enough
            if (too_many_active_sounds && false)
                return;
            SoundEffectGetter sound = get_sound(bank, cue_name);
            if (sound.Sound == null)
            {
                sound.Dispose();
                return;
            }
            SoundEffectInstance instance = sound_instance(sound, cue_name, pitch);

            add_new_playing_sound(instance, channel, duckBgm);
            
            sound.Dispose();
        }
        private void prepare_system_se(string bank, string cue_name, bool priority, Maybe<float> pitch = default(Maybe<float>))
        {
            New_System_Sound_Data = new Sound_Name_Data(bank, cue_name, priority, pitch);
        }
        private void play_system_se(string bank, string cue_name, bool priority, Maybe<float> pitch)
        {
            SoundEffectGetter sound = get_sound(bank, cue_name, true);
            if (sound.Sound == null)
            {
                sound.Dispose();
                return;
            }

            cancel_system_sound();
            System_Sound = sound_instance(sound, cue_name, pitch);
            System_Sound.Play();
            System_SFX_Priority = priority;
            if (System_Sound.State == SoundState.Initial)
            {
                System_Sound.Dispose();
                System_Sound = null;
                System_SFX_Priority = false;
            }
            sound.Dispose();
            return;
        }

        private SoundEffectInstance sound_instance(SoundEffectGetter sound, string cue_name, Maybe<float> pitch)
        {
            SoundEffectInstance instance = sound.Instance;
            instance.Volume = Sound_Fade_Volume * Sound_Volume;
            if (pitch.IsSomething)
                instance.Pitch = pitch;
            else if (PITCHES.ContainsKey(cue_name))
            {
                instance.Pitch = PITCHES[cue_name];
            }

            return instance;
        }

        private static SoundEffectGetter get_sound(string bank, string cue_name)
        {
            return get_sound(bank, cue_name, false);
        }
        private static SoundEffectGetter get_sound(string bank, string cue_name, bool looping)
        {
            string filename = @"Content\Audio\SE\" + bank + @"\" + cue_name + ".ogg";
            SoundEffect sound = null;
            SoundEffectInstance instance = null;
            bool ogg = false;

            try
            {
                sound_effect_from_ogg(out sound, filename);
                ogg = true;
            }
            catch (FileNotFoundException ex)
            {
#if WINDOWS || __ANDROID__
                return new SoundEffectGetter();
#endif
                sound = Global.Content.Load<SoundEffect>(@"Audio/" + filename);
            }
#if __ANDROID__
            catch (Java.IO.FileNotFoundException e)
            {
                return new SoundEffectGetter();
            }
#endif

            sound.Name = cue_name;
#if __ANDROID__
            instance = sound.CreateInstance();
            instance.AlsoDisposeEffect();
#else
            if (looping && LOOP_DATA.ContainsKey(filename) && LOOP_DATA[filename][1] != -1)
            {
                instance = new SoundEffectInstance(sound, LOOP_DATA[filename][0], LOOP_DATA[filename][1], LOOP_DATA[filename][1] + LOOP_DATA[filename][2]);
                instance.IsLooped = true;
            }
            else
                instance = new SoundEffectInstance(sound);
#endif
            return new SoundEffectGetter(ogg, sound, instance);
        }

        private bool playing_system_sound()
        {
            return (System_SFX_Priority && System_Sound != null && System_Sound.State != SoundState.Stopped) ||
                (New_System_Sound_Data != null && New_System_Sound_Data.Priority);
        }
        private void cancel_system_sound()
        {
            if (System_Sound != null)
            {
                System_Sound.Stop();
                System_Sound.Dispose();
                System_Sound = null;
                System_SFX_Priority = false;

                New_System_Sound_Data = null;
            }
        }

        private void stop_sfx()
        {
            for (int i = 0; i < Playing_Sounds.Length; i++)
            {
                if (Playing_Sounds[i] != null)
                {
                    Playing_Sounds[i].Stop();
                    Playing_Sounds[i].Dispose();
                    Playing_Sounds[i] = null;
                }
            }

            cancel_system_sound();
            BgmManager.StopMusicEffect();

            set_sfx_fade_volume(1);
            cancel_sound_fade_out();
        }

        private void sfx_fade()
        {
            sfx_fade(60);
        }
        private void sfx_fade(int time)
        {
            if (time > 0 && !sound_fading_out)
            {
                //if (!sound_muted)
                    set_sfx_fade_volume(1);
                Sound_Fade_Out_Time = time;
                Sound_Fade_Timer = 0;
            }
            bgs_fade(time);
        }

        private void cancel_sound_fade_out()
        {
            Sound_Fade_Out_Time = 0;
            Sound_Fade_Timer = 0;
        }

        private Maybe<int> add_new_playing_sound(
            SoundEffectInstance instance,
            Maybe<int> channel,
            bool duckBgm)
        {
            int actual_channel;

            // If the channel is defined
            if (channel.IsSomething)
            {
                if (channel >= Playing_Sounds.Length || channel < 0)
                    throw new ArgumentException();

                // If a sound is already on this channel
                if (Playing_Sounds[channel] != null)
                {
                    // If the sound on this channel is also a channel sound,
                    if (Playing_Sounds[channel].FixedChannel)
                    {
                        // Dispose the sound so it can be replaced
                        Playing_Sounds[channel].Stop();
                        Playing_Sounds[channel].Dispose();
                        Playing_Sounds[channel] = null;
                    }
                    else
                    {
                        // If there aren't empty channels to move the sound to instead
                        // Remove the oldest sound
                        if (too_many_active_sounds)
                            remove_oldest_playing_sound();
                        // Move all sounds up a channel if this one is still taken
                        if (Playing_Sounds[channel] != null)
                            push_sound_ids(channel);
                    }
                }

                actual_channel = channel;
            }
            // Else play on the first open channel
            else
            {
                actual_channel = -1;
                // Remove the oldest sound if there are too many
                if (too_many_active_sounds)
                    remove_oldest_playing_sound();
                // Find the first empty channel
                for (int i = 0; i < Playing_Sounds.Length; i++)
                    if (Playing_Sounds[i] == null)
                    {
                        actual_channel = i;
                        break;
                    }
            }

            // If we found a channel to play the sound on
            if (actual_channel >= 0)
            {
                instance.Play();
                // If calling play on the sound results in some error, dispose it
                if (instance.State == SoundState.Initial)
                    instance.Dispose();
                else
                {
                    // Don't duck by sfx if the BGM is manually ducked
                    if (!BgmDuckedBySfx && BgmManager.IsBgmDucked)
                        duckBgm = false;

                    Playing_Sounds[actual_channel] = new ChannelSound(
                        instance, channel.IsSomething, duckBgm);
                    if (duckBgm)
                    {
                        BgmManager.DuckBgmVolume();
                        BgmDuckedBySfx = true;
                    }
                    return actual_channel;
                }
            }
            return Maybe<int>.Nothing;
        }

        private bool remove_oldest_playing_sound()
        {
            for (int i = 0; i < Playing_Sounds.Length; i++)
            {
                if (!Playing_Sounds[i].FixedChannel)
                {
                    Playing_Sounds[i].Stop();
                    Playing_Sounds[i].Dispose();
                    pop_sound_ids(i);
                    return true;
                }
            }
            return false;
        }

        private void push_sound_ids(int i)
        {
            if (Playing_Sounds[i].FixedChannel)
                throw new ArgumentException();

            // First determine if there are any null channels between the last unfixed
            // channel and the end of the array
            int null_channel = Playing_Sounds.Length;
            int unfixed_channel = Playing_Sounds.Length;
            for (int j = Playing_Sounds.Length - 1; j >= i; j--)
            {
                if (Playing_Sounds[j] == null)
                {
                    null_channel = j;
                    break;

                }
            }
            for (int j = Playing_Sounds.Length - 1; j >= i; j--)
            {
                if (Playing_Sounds[j] != null && !Playing_Sounds[j].FixedChannel)
                {
                    unfixed_channel = j;
                    break;
                }
            }

            // If there are no available null channels
            if (null_channel == Playing_Sounds.Length || null_channel < unfixed_channel)
            {
                // The first fixed sound encountered will be disposed
                for (; i < Playing_Sounds.Length; i++)
                {
                    if (Playing_Sounds[i] != null && !Playing_Sounds[i].FixedChannel)
                    {
                        Playing_Sounds[i].Stop();
                        Playing_Sounds[i].Dispose();
                        Playing_Sounds[i] = null;
                        return;
                    }
                }
            }
            // Else move all sound ids that don't have their channel defined up
            else
            {
                var sound = Playing_Sounds[i];
                Playing_Sounds[i] = null;
                i++;
                for (; i < Playing_Sounds.Length; i++)
                {
                    if (Playing_Sounds[i] == null)
                    {
                        Playing_Sounds[i] = sound;
                        sound = null;
                        break;
                    }
                    else
                    {
                        if (Playing_Sounds[i].FixedChannel)
                            continue;
                        else
                        {
                            var temp = Playing_Sounds[i];
                            Playing_Sounds[i] = sound;
                            sound = temp;
                        }
                    }
                }
                // Should never hit this, but just in case
                if (sound != null)
                    sound.Dispose();
            }
        }
        private void pop_sound_ids(int i)
        {
            Playing_Sounds[i] = null;
            // Move all sound ids that don't have their channel defined down
            for (int j = i + 1; j < Playing_Sounds.Length; j++)
            {
                if (Playing_Sounds[j] != null && !Playing_Sounds[j].FixedChannel)
                {
                    Playing_Sounds[i] = Playing_Sounds[j];
                    i = j;
                    Playing_Sounds[i] = null;
                }
            }
        }
        #endregion

        #region ME
        private void play_me(string bank, string cue_name)
        {
            SoundEffectGetter sound = get_sound(bank, cue_name);
            if (sound.Sound == null)
            {
                sound.Dispose();
                return;
            }

            sound.Instance.Pitch = 0f;
            if (PITCHES.ContainsKey(cue_name))
                sound.Instance.Pitch = PITCHES[cue_name];
            BgmManager.PlayMusicEffect(sound.Instance);
            
            sound.Dispose();
            return;
        }

        private bool stop_me()
        {
            return stop_me(false);
        }
        private bool stop_me(bool bgm_stop)
        {
            if (bgm_stop)
                BgmManager.Stop();
            return BgmManager.StopMusicEffect();
        }
        #endregion

#if DEBUG && WINDOWS
        private AudioDiagnostics AudioDiagnostics()
        {
            return new AudioDiagnostics(
                BgmManager.BgmDiagnostics(),
                BgmManager.PendingBgmDiagnostics());
        }
#endif

        private static void sound_effect_from_ogg(out SoundEffect sound, string cue_name, int intro_start = 0, int loop_start = -1, int loop_length = -1)
        {
            sound_effect_from_ogg(out sound, cue_name, out intro_start, out loop_start, out loop_length);
        }
        private static void sound_effect_from_ogg(out SoundEffect sound, string cue_name, out int intro_start, out int loop_start, out int loop_length)
        {
            if (!LOOP_DATA.ContainsKey(cue_name))
            {
                try
                {
                    using (Stream cue_stream = TitleContainer.OpenStream(cue_name))
                    {
#if __ANDROID__
						MemoryStream stream = new MemoryStream();
						cue_stream.CopyTo(stream);
                        using (var vorbis = new NVorbis.VorbisReader(stream, cue_name, false))
#else
                        using (var vorbis = new NVorbis.VorbisReader(cue_stream, cue_name, false))
#endif
                        {
                            // Stores sound effect data, so it doesn't have to be reloaded repeatedly
                            if (!SOUND_DATA.ContainsKey(cue_name))
                                SOUND_DATA[cue_name] = get_ogg_pcm_data(vorbis);

                            get_loop_data(vorbis, out intro_start, out loop_start, out loop_length);

                            LOOP_DATA[cue_name] = new int[5];
                            LOOP_DATA[cue_name][0] = intro_start;
                            LOOP_DATA[cue_name][1] = loop_start;
                            LOOP_DATA[cue_name][2] = loop_length;
                            LOOP_DATA[cue_name][3] = vorbis.Channels;
                            LOOP_DATA[cue_name][4] = vorbis.SampleRate;
                        }
#if __ANDROID__
						stream.Dispose();
#endif
                    }
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

            intro_start = LOOP_DATA[cue_name][0];
            loop_start = LOOP_DATA[cue_name][1];
            loop_length = LOOP_DATA[cue_name][2];
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // Writes the decoded ogg data to a memorystream
                WriteWave(writer, LOOP_DATA[cue_name][3], LOOP_DATA[cue_name][4], SOUND_DATA[cue_name]);
                // Resets the stream's position back to 0 after writing
                stream.Position = 0;
#if __ANDROID__
                sound = SoundEffect.FromStream(
                    stream, intro_start, loop_start, loop_start + loop_length);
#else
                sound = SoundEffect.FromStream(stream);
#endif
                sound.Name = cue_name;
            }
        }

        private static byte[] get_ogg_pcm_data(NVorbis.VorbisReader vorbis)
        {
            return vorbis.SelectMany(x => x).ToArray();
        }

        private static void get_loop_data(NVorbis.VorbisReader reader, out int intro_start, out int loop_start, out int loop_length)
        {
            string[] comments = reader.Comments;
            get_loop_data(comments, out intro_start, out loop_start, out loop_length);
        }
        private static void get_loop_data(string[] comments, out int intro_start, out int loop_start, out int loop_length)
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

        private static void WriteWave(BinaryWriter writer, int channels, int rate, byte[] data)
        {
            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
            writer.Write((int)(36 + data.Length));
            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });

            writer.Write(new char[4] { 'f', 'm', 't', ' ' });
            writer.Write((int)16);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write((int)rate);
            writer.Write((int)(rate * ((16 * channels) / 8)));
            writer.Write((short)((16 * channels) / 8));
            writer.Write((short)16);

            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write((int)data.Length);
            writer.Write(data);
        }
    }

    internal struct SoundEffectGetter : IDisposable
    {
        public bool Ogg;
        public SoundEffect Sound;
        public SoundEffectInstance Instance;

        public SoundEffectGetter(bool ogg, SoundEffect sound, SoundEffectInstance instance)
        {
            Ogg = ogg;
            Sound = sound;
            Instance = instance;
        }

        private bool dispose_sound
        {
            get
            {
#if __ANDROID__
                return false;
#endif
                return Ogg;
            }
        }

        public void Dispose()
        {
            if (Sound != null)
                if (dispose_sound)
                    Sound.Dispose();
        }
    }

#if DEBUG && WINDOWS
    struct AudioDiagnostics
    {
        public IEnumerable<string> Bgm { get; private set; }
        public IEnumerable<string> PendingBgm { get; private set; }

        public AudioDiagnostics(IEnumerable<string> bgm, IEnumerable<string> pendingBgm)
            : this()
        {
            Bgm = bgm;
            PendingBgm = pendingBgm;
        }
    }
#endif
}