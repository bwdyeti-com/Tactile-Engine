using System;
using System.Collections.Generic;
using System.IO;
using Tactile.IO.Serialization;

namespace Tactile.Options
{
    enum AudioSetting { MasterVolume, MusicVolume, SoundVolume, MuteWhenInactive }

    class AudioSettings : SettingsBase, ISerializableGameObject
    {
        private int _MasterVolume;
        private int _MusicVolume;
        private int _SoundVolume;
        private bool _MuteWhenInactive;

        public int MasterVolume { get { return _MasterVolume; } }
        public int MusicVolume
        {
            get
            {
                int volume = _MasterVolume * _MusicVolume;
                return volume;
            }
        }
        public int SoundVolume
        {
            get
            {
                int volume = _MasterVolume * _SoundVolume;
                return volume;
            }
        }
        public bool MuteWhenInactive { get { return _MuteWhenInactive; } }

        public AudioSettings()
        {
#if __MOBILE__
            _MuteWhenInactive = true;
#endif
        }
        private AudioSettings(AudioSettings source) : this()
        {
            CopySettingsFrom(source);
        }

        protected override List<SettingsData> GetSettingsData()
        {
            return new List<SettingsData>
            {
                SettingsData.Create("Master Volume", ConfigTypes.Slider, 7,
                    rangeMin: 0, rangeMax: 10, updateBeforeConfirming: true),
                SettingsData.Create("Music Volume", ConfigTypes.Slider, 10,
                    rangeMin: 0, rangeMax: 10, updateBeforeConfirming: true),
                SettingsData.Create("Sound Volume", ConfigTypes.Slider, 10,
                    rangeMin: 0, rangeMax: 10, updateBeforeConfirming: true),
#if !__MOBILE__
                SettingsData.Create("Mute When Inactive", ConfigTypes.OnOffSwitch, false)
#else
                new NullSettingsData()
#endif
            };
        }
        
        /// <summary>
        /// Creates an instance and reads a stream into it.
        /// </summary>
        public static AudioSettings ReadObject(BinaryReader reader)
        {
            var result = new AudioSettings();
            result.Read(reader);
            return result;
        }

        #region ICloneable
        public override object Clone()
        {
            return new AudioSettings(this);
        }
        #endregion

        #region ISettings
        protected override void CopyAdditionalSettingsFrom(ISettings other)
        {
            var otherAudio = (AudioSettings)other;

#if __MOBILE__
            _MuteWhenInactive = otherAudio._MuteWhenInactive;
#endif
        }

        public override object ValueObject(Tuple<int, int> entry)
        {
            switch (entry.Item1)
            {
                case (int)AudioSetting.MasterVolume:
                    return _MasterVolume;
                case (int)AudioSetting.MusicVolume:
                    return _MusicVolume;
                case (int)AudioSetting.SoundVolume:
                    return _SoundVolume;
                case (int)AudioSetting.MuteWhenInactive:
                    return _MuteWhenInactive;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
        
        public override string ValueString(int index)
        {
            var entry = GetEntryIndex(index);

            switch (entry.Item1)
            {
                case (int)AudioSetting.MasterVolume:
                case (int)AudioSetting.MusicVolume:
                case (int)AudioSetting.SoundVolume:
                    // Volume values are multiples of 10
                    int optionIndex = Value<int>(index) * 10;
                    return string.Format(FormatString(index), optionIndex);
            }

            return base.ValueString(index);
        }

        public override void SetValue(Tuple<int, int> entry, object value)
        {
            switch (entry.Item1)
            {
                // bool
                case (int)AudioSetting.MuteWhenInactive:
                    SetValue(entry, ref _MuteWhenInactive, value);
                    break;
                // int
                case (int)AudioSetting.MasterVolume:
                    SetValue(entry, ref _MasterVolume, value);
                    break;
                case (int)AudioSetting.MusicVolume:
                    SetValue(entry, ref _MusicVolume, value);
                    break;
                case (int)AudioSetting.SoundVolume:
                    SetValue(entry, ref _SoundVolume, value);
                    break;
            }
        }
        #endregion

        #region ISerializableGameObject
        public void Write(BinaryWriter writer)
        {
            SaveSerializer.Write(
                writer,
                this,
                SaveSerialization.ExplicitTypes);
        }

        public void Read(BinaryReader reader)
        {
            SaveSerializer.Read(
                reader,
                this);
        }

        public void UpdateReadValues(Version v, SerializerData data) { }

        public void SetReadValues(SerializerData data)
        {
            data.ReadValue(out _MasterVolume, "MasterVolume");
            data.ReadValue(out _MusicVolume, "MusicVolume");
            data.ReadValue(out _SoundVolume, "SoundVolume");
            data.ReadValue(out _MuteWhenInactive, "MuteWhenInactive");
        }

        public SerializerData GetSaveData()
        {
            return new SerializerData.Builder()
                .Add("MasterVolume", _MasterVolume)
                .Add("MusicVolume", _MusicVolume)
                .Add("SoundVolume", _SoundVolume)
                .Add("MuteWhenInactive", _MuteWhenInactive)
                .Build();
        }

        public Dictionary<string, Type> ExpectedData(Version version)
        {
            return new Dictionary<string, Type>
            {
                { "MasterVolume", typeof(int) },
                { "MusicVolume", typeof(int) },
                { "SoundVolume", typeof(int) },
                { "MuteWhenInactive", typeof(bool) },
            };
        }
        #endregion
    }
}
