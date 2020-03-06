using System;
using System.Collections.Generic;

namespace FEXNA.Options
{
    enum GeneralSetting { Metrics, CheckForUpdates }

    class GeneralSettings : SettingsBase
    {
        private bool _Metrics;
        private bool _CheckForUpdates;

        public bool Metrics { get { return _Metrics; } }
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
            if (true)
                settingsData.Add(SettingsData.Create("Metrics", ConfigTypes.OnOffSwitch, false));
            else
                settingsData.Add(new NullSettingsData());
            // Check for Updates
            if (true)
                settingsData.Add(SettingsData.Create("Check for Updates", ConfigTypes.OnOffSwitch, true));
            else
                settingsData.Add(new NullSettingsData());

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

        public override object ValueObject(Tuple<int, int> entry)
        {
            switch (entry.Item1)
            {
                case (int)GeneralSetting.Metrics:
                    return _Metrics;
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
                    SetValue(entry, ref _Metrics, value);
                    break;
                case (int)GeneralSetting.CheckForUpdates:
                    SetValue(entry, ref _CheckForUpdates, value);
                    break;
            }
        }
        #endregion
    }
}
