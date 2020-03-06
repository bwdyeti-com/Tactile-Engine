using System;
using System.Collections.Generic;
using System.Linq;
using FEXNA_Library;

namespace FEXNA.Options
{
    enum AudioSetting { MasterVolume, MusicVolume, SoundVolume, MuteWhenInactive }

    class AudioSettings : SettingsBase
    {
        private int _MasterVolume;
        private int _MusicVolume;
        private int _SoundVolume;
        private bool _MuteWhenInactive;

        public int MasterVolume { get { return _MasterVolume; } }
        public int MusicVolume { get { return _MusicVolume; } }
        public int SoundVolume { get { return _SoundVolume; } }
        public bool MuteWhenInactive { get { return _MuteWhenInactive; } }

        public AudioSettings() { }
        private AudioSettings(AudioSettings source) : this()
        {
            CopySettingsFrom(source);
        }

        protected override List<SettingsData> GetSettingsData()
        {
            return new List<SettingsData>
            {
                SettingsData.Create("Master Volume", ConfigTypes.Slider, 7,
                    rangeMin: 0, rangeMax: 10),
                SettingsData.Create("Music Volume", ConfigTypes.Slider, 10,
                    rangeMin: 0, rangeMax: 10),
                SettingsData.Create("Sound Volume", ConfigTypes.Slider, 10,
                    rangeMin: 0, rangeMax: 10),
                SettingsData.Create("Mute When Inactive", ConfigTypes.OnOffSwitch, false)
            };
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
    }
}
