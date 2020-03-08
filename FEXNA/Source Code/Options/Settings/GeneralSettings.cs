using System;
using System.Collections.Generic;

namespace FEXNA.Options
{
    enum GeneralSetting { Metrics, CheckForUpdates }

    class GeneralSettings : SettingsBase
    {
        private Metrics_Settings _Metrics = Metrics_Settings.Not_Set;
        private bool _CheckForUpdates;

        public Metrics_Settings Metrics { get { return (Metrics_Settings)_Metrics; } }
        public bool CheckForUpdates { get { return _CheckForUpdates; } }

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
            settingsData.Add(SettingsData.Create("Check for Updates", ConfigTypes.OnOffSwitch, true));

            return settingsData;
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
    }
}
