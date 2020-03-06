using System;
using System.Collections.Generic;
using FEXNA_Library;

namespace FEXNA.Options
{
    enum ConfigTypes { None, Number, Slider, OnOffSwitch, Button, Input }

    interface ISettings : ICloneable
    {
        bool CopySettingsFrom(ISettings other);

        void RestoreDefaultValue(int index);

        void RestoreDefaults();
        
        IEnumerable<string> SettingLabels { get; }
        string SettingLabel(int index);

        ConfigTypes SettingType(int index);

        IEnumerable<int> DependentSettings(int index);

        void ConfirmOption(int index);

        bool IsOptionEnabled(int index);

        T Value<T>(int index);

        object ValueObject(int index);

        string ValueString(int index);

        void SetValue(int index, object value);

        void CopyValueFrom(ISettings other, int index);

        Range<int> ValueRange(int index);

        int ValueInterval(int index);
    }
}
