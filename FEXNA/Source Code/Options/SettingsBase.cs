using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using FEXNA_Library;

namespace FEXNA.Options
{
    /// <summary>
    /// An abstract implementation of ISettings, to act as a base class for
    /// settings data.
    /// </summary>
    abstract class SettingsBase : ISettings
    {
        protected readonly List<SettingsData> _Data;
        private Dictionary<int, int> DataIndices = new Dictionary<int, int>();

        protected SettingsBase()
        {
            _Data = GetSettingsData();
            int index = 0;
            for (int i = 0; i < _Data.Count; i++)
            {
                DataIndices.Add(i, index);
                index += _Data[i].Size;
            }

            RestoreDefaults();
        }

        /// <summary>
        /// Checks if this object and the given <see cref="ISettings"/> are
        /// the same class, for use before copying data between them.
        /// </summary>
        /// <param name="other">The other <see cref="ISettings"/> object.</param>
        protected bool CheckSameClass(ISettings other)
        {
            if (other.GetType() != this.GetType())
            {
#if DEBUG
                throw new ArgumentException(
                    "Tried to copy different types of settings.");
#endif
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copies additional data of another <see cref="ISettings"/> into this object
        /// that are not part of the settings.
        /// </summary>
        /// <param name="other">The other <see cref="ISettings"/> object.</param>
        protected abstract void CopyAdditionalSettingsFrom(ISettings other);

        /// <summary>
        /// Gets the real index of a setting and the offset for the value in
        /// that setting.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        protected Tuple<int, int> GetEntryIndex(int index)
        {
            int key = DataIndices
                .Last(x => x.Value <= index)
                .Key;

            return Tuple.Create(key, index - DataIndices[key]);
        }

        /// <summary>
        /// Returns the list of <see cref="SettingsData"/> to use for these
        /// settings. Called by the constructor.
        /// </summary>
        protected abstract List<SettingsData> GetSettingsData();

        /// <summary>
        /// Returns true if any settings are valid,
        /// meaning they have a <see cref="SettingsData.Size"/>
        /// greater than or equal to 1.
        /// </summary>
        public bool AnyValidOptions
        {
            get
            {
                return _Data.Any(x => x.Size >= 1);
            }
        }

        /// <summary>
        /// Gets a format string to use with a setting's value.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        public virtual string FormatString(int index)
        {
            var entry = GetEntryIndex(index);

            return _Data[entry.Item1].FormatString;
        }

        /// <summary>
        /// Gets the value of a setting as an <see cref="object"/>.
        /// </summary>
        /// <param name="entry">
        /// <see cref="Tuple{int, int}.Item1"/> is the index of the setting data;
        /// <see cref="Tuple{int, int}.Item2"/> is the offset for the value.
        /// </param>
        public abstract object ValueObject(Tuple<int, int> entry);

        /// <summary>
        /// Changes the value of a setting.
        /// </summary>
        /// <param name="entry">
        /// <see cref="Tuple{int, int}.Item1"/> is the index of the setting data;
        /// <see cref="Tuple{int, int}.Item2"/> is the offset for the value.
        /// </param>
        /// <param name="value">The new value of the setting.</param>
        public abstract void SetValue(Tuple<int, int> entry, object value);

        /// <summary>
        /// Changes the value of a setting.
        /// </summary>
        /// <param name="entry">
        /// <see cref="Tuple{int, int}.Item1"/> is the index of the setting data;
        /// <see cref="Tuple{int, int}.Item2"/> is the offset for the value.
        /// </param>
        /// <param name="setting">The reference to the actual setting being edited.</param>
        /// <param name="value">The new value of the setting.</param>
        protected void SetValue<T>(Tuple<int, int> entry, ref T setting, object value)
        {
            object o = setting;

            var settingData = _Data[entry.Item1];
            settingData.SetValue(ref o, value, () => ValueRange(entry.Item1), entry.Item2);

            setting = (T)o;
        }

        #region ICloneable
        public abstract object Clone();
        #endregion

        #region ISettings
        public bool CopySettingsFrom(ISettings other)
        {
            if (!CheckSameClass(other))
                return false;

            for (int i = 0; i < _Data.Count; i++)
            {
                for (int j = 0; j < _Data[i].Size; j++)
                    CopyValueFrom(other, DataIndices[i] + j);
            }

            CopyAdditionalSettingsFrom(other);

            return true;
        }

        public void RestoreDefaults()
        {
            for (int i = 0; i < DataIndices.Count; i++)
            {
                for (int j = 0; j < _Data[i].Size; j++)
                    RestoreDefaultValue(DataIndices[i] + j);
            }
        }
        
        public void RestoreDefaultValue(int index)
        {
            var entry = GetEntryIndex(index);

            object value = _Data[entry.Item1].GetDefaultValue(entry.Item2);
            SetValue(index, value);
        }

        public IEnumerable<string> SettingLabels
        {
            get
            {
                for (int i = 0; i < DataIndices.Count; i++)
                {
                    for (int j = 0; j < _Data[i].Size; j++)
                        yield return SettingLabel(DataIndices[i] + j);
                }
            }
        }
        public string SettingLabel(int index)
        {
            var entry = GetEntryIndex(index);
            return _Data[entry.Item1].Label[entry.Item2];
        }

        public ConfigTypes SettingType(int index)
        {
            var entry = GetEntryIndex(index);
            return _Data[entry.Item1].Type;
        }

        public IEnumerable<int> DependentSettings(int index)
        {
            var entry = GetEntryIndex(index);
            if (_Data[entry.Item1].DependentSettings != null)
            {
                foreach (int dependentIndex in _Data[entry.Item1].DependentSettings)
                {
                    var dependentEntry = GetEntryIndex(dependentIndex);
                    for (int j = 0; j < _Data[dependentEntry.Item1].Size; j++)
                        yield return DataIndices[dependentEntry.Item1] + j;
                }
            }

            yield break;
        }

        public virtual void ConfirmSetting(int index, object value)
        {
            SetValue(index, value);
        }

        public virtual bool IsSettingEnabled(int index)
        {
            return true;
        }

        public T Value<T>(int index)
        {
            ConfigTypes type = SettingType(index);
            object result;
            switch (type)
            {
                case ConfigTypes.OnOffSwitch:
                    result = (bool)ValueObject(index);
                    return (T)result;
                case ConfigTypes.Number:
                case ConfigTypes.Slider:
                    result = (int)ValueObject(index);
                    return (T)result;
                case ConfigTypes.Input:
                    result = (Keys)ValueObject(index);
                    return (T)result;
            }

            return default(T);
        }

        public object ValueObject(int index)
        {
            return ValueObject(GetEntryIndex(index));
        }

        public virtual string ValueString(int index)
        {
            ConfigTypes type = SettingType(index);
            switch (type)
            {
                case ConfigTypes.OnOffSwitch:
                    bool value = Value<bool>(index);
                    return value ? "On" : "Off";
                case ConfigTypes.Number:
                case ConfigTypes.Slider:
                    int intValue = Value<int>(index);
                    return string.Format(FormatString(index), intValue);
                case ConfigTypes.Input:
                    Keys keyValue = Value<Keys>(index);
                    return keyValue.ToString(); ;
            }

            return "";
        }

        public void SetValue(int index, object value)
        {
            SetValue(GetEntryIndex(index), value);
        }

        public void CopyValueFrom(ISettings other, int index)
        {
            SetValue(index, other.ValueObject(index));
        }

        public virtual Range<int> ValueRange(int index)
        {
            var entry = GetEntryIndex(index);

            switch (SettingType(index))
            {
                case ConfigTypes.Number:
                case ConfigTypes.Slider:
                    return _Data[entry.Item1].Range;
            }

#if DEBUG
            throw new ArgumentException(
                "Tried to get the range of a\nsetting that isn't a number value.");
#endif
        }
        
        public virtual int ValueInterval(int index)
        {
            switch (SettingType(index))
            {
                case ConfigTypes.Number:
                case ConfigTypes.Slider:
                    return 1;
            }

#if DEBUG
            throw new ArgumentException(
                "Tried to get the interval of a\nsetting that isn't a number value.");
#endif
        }
        #endregion
    }
}
