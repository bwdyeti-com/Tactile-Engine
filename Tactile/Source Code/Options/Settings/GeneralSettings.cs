using System;
using System.Collections.Generic;
using System.IO;
using Tactile.IO.Serialization;

namespace Tactile.Options
{
    enum GeneralSetting { Metrics, CheckForUpdates }

    class GeneralSettings : SettingsBase, ISerializableGameObject
    {
        private Metrics_Settings _Metrics = Metrics_Settings.Not_Set;
        private bool _CheckForUpdates;

        public Metrics_Settings Metrics
        {
            get
            {
                if (!Global.metrics_allowed)
                    return Metrics_Settings.Off;
                return (Metrics_Settings)_Metrics;
            }
        }
        public bool CheckForUpdates
        {
            get
            {
                if (!Global.update_check_allowed)
                    return false;
                return _CheckForUpdates;
            }
        }

        public GeneralSettings() { }
        private GeneralSettings(GeneralSettings source) : this()
        {
            CopySettingsFrom(source);
        }

        protected override List<SettingsData> GetSettingsData()
        {
            var settingsData = new List<SettingsData>();

            // Metrics
            if (Global.metrics_allowed)
                settingsData.Add(SettingsData.Create("Metrics", ConfigTypes.OnOffSwitch, false));
            else
                settingsData.Add(new NullSettingsData());
            // Check for Updates
            if (Global.update_check_allowed)
                settingsData.Add(SettingsData.Create("Check for Updates", ConfigTypes.OnOffSwitch, true));
            else
                settingsData.Add(new NullSettingsData());

            return settingsData;
        }
        
        /// <summary>
        /// Creates an instance and reads a stream into it.
        /// </summary>
        public static GeneralSettings ReadObject(BinaryReader reader)
        {
            var result = new GeneralSettings();
            result.Read(reader);
            return result;
        }

        #region ICloneable
        public override object Clone()
        {
            return new GeneralSettings(this);
        }
        #endregion

        #region ISettings
        protected override void CopyAdditionalSettingsFrom(ISettings other)
        {
            var otherGeneral = (GeneralSettings)other;
        }

        public override void ConfirmSetting(int index, object value)
        {
            var entry = GetEntryIndex(index);

            switch (entry.Item1)
            {
                // If confirming a value, force removing not set
                case (int)GeneralSetting.Metrics:
                    if (_Metrics == Metrics_Settings.Not_Set)
                        _Metrics = Metrics_Settings.Off;
                    break;
            }

            base.ConfirmSetting(index, value);
        }

        public override object ValueObject(Tuple<int, int> entry)
        {
            switch (entry.Item1)
            {
                case (int)GeneralSetting.Metrics:
                    return _Metrics == Metrics_Settings.On;
                case (int)GeneralSetting.CheckForUpdates:
                    return _CheckForUpdates;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public override void SetValue(Tuple<int, int> entry, object value)
        {
            switch (entry.Item1)
            {
                // bool
                case (int)GeneralSetting.Metrics:
                    if (_Metrics != Metrics_Settings.Not_Set)
                    {
                        bool metrics = _Metrics == Metrics_Settings.On;
                        SetValue(entry, ref metrics, value);
                        _Metrics = metrics ? Metrics_Settings.On : Metrics_Settings.Off;
                    }
                    break;
                case (int)GeneralSetting.CheckForUpdates:
                    SetValue(entry, ref _CheckForUpdates, value);
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
            data.ReadValue(out _Metrics, "Metrics");
            data.ReadValue(out _CheckForUpdates, "CheckForUpdates");
        }

        public SerializerData GetSaveData()
        {
            return new SerializerData.Builder()
                .Add("Metrics", _Metrics)
                .Add("CheckForUpdates", _CheckForUpdates)
                .Build();
        }

        public Dictionary<string, Type> ExpectedData(Version version)
        {
            return new Dictionary<string, Type>
            {
                { "Metrics", typeof(Metrics_Settings) },
                { "CheckForUpdates", typeof(bool) },
            };
        }
        #endregion
    }
}
