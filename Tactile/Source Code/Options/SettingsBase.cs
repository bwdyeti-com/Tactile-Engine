﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using TactileLibrary;

namespace Tactile.Options
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
        /// Resets any other settings necessary to restore defaults.
        /// </summary>
        protected virtual void RestoreAdditionalDefaults() { }

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
        /// Gets the real index of a setting.
        /// </summary>
        /// <param name="index">The setting index in Data.</param>
        protected int GetIndexOfEntry(int entry)
        {
            return DataIndices[entry];
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
        public bool AnyValidSettings
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
        /// Changes the value of a setting and confirms the change.
        /// Also called when modifying values of temporary settings before
        /// copying them back to real settings objects.
        /// </summary>
        /// <param name="entryIndex">The index of the setting data. Must be an enum.</param>
        /// <param name="offset">The offset for the value.</param>
        /// <param name="value">The new value of the setting.</param>
        public void ConfirmSetting<TEnum>(TEnum entryIndex, int offset, object value) where TEnum : struct, IConvertible
        {
            if (typeof(TEnum).IsEnum)
            {
                Enum enumValue = Enum.Parse(typeof(TEnum), entryIndex.ToString()) as Enum;
                int index = Convert.ToInt32(enumValue);

                ConfirmSetting(GetIndexOfEntry(index) + offset, value);
            }
            else
                throw new ArgumentException();
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
        /// <param name="entryIndex">The index of the setting data. Must be an enum.</param>
        /// <param name="offset">The offset for the value.</param>
        /// <param name="value">The new value of the setting.</param>
        public void SetValue<TEnum>(TEnum entryIndex, int offset, object value) where TEnum : struct, IConvertible
        {
            if (typeof(TEnum).IsEnum)
            {
                Enum enumValue = Enum.Parse(typeof(TEnum), entryIndex.ToString()) as Enum;
                int index = Convert.ToInt32(enumValue);

                SetValue(Tuple.Create(index, offset), value);
            }
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// Changes the value of a setting.
        /// </summary>
        /// <param name="entry">
        /// <see cref="Tuple{int, int}.Item1"/> is the displayed index of the setting data;
        /// <see cref="Tuple{int, int}.Item2"/> is the offset for the value.
        /// </param>
        /// <param name="setting">The reference to the actual setting being edited.</param>
        /// <param name="value">The new value of the setting.</param>
        protected void SetValue<T>(Tuple<int, int> entry, ref T setting, object value)
        {
            object o = setting;

            var settingData = _Data[entry.Item1];
            settingData.SetValue(ref o, value, () => ValueRange(entry), entry.Item2);

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

            CopyAdditionalSettingsFrom(other);

            for (int i = 0; i < _Data.Count; i++)
            {
                for (int j = 0; j < _Data[i].Size; j++)
                    CopyValueFrom(other, DataIndices[i] + j);
            }

            return true;
        }

        public void RestoreDefaults()
        {
            for (int i = 0; i < DataIndices.Count; i++)
            {
                for (int j = 0; j < _Data[i].Size; j++)
                    RestoreDefaultValue(DataIndices[i] + j);
            }

            RestoreAdditionalDefaults();
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
            return SettingLabel(GetEntryIndex(index));
        }
        public string SettingLabel(Tuple<int, int> entry)
        {
            return _Data[entry.Item1].Label[entry.Item2];
        }

        public ConfigTypes SettingType(int index)
        {
            return SettingType(GetEntryIndex(index));
        }
        public ConfigTypes SettingType(Tuple<int, int> entry)
        {
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

        public bool SettingUpdatesBeforeConfirm(int index)
        {
            var entry = GetEntryIndex(index);
            return _Data[entry.Item1].UpdateBeforeConfirming;
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
                case ConfigTypes.List:
                case ConfigTypes.Slider:
                    result = (int)ValueObject(index);
                    return (T)result;
                case ConfigTypes.Button:
                    result = (string)ValueObject(index);
                    return (T)result;
                case ConfigTypes.SubSettings:
                    var entry = GetEntryIndex(index);
                    result = _Data[entry.Item1].GetDefaultValue(entry.Item2);
                    return (T)result;
                case ConfigTypes.Keyboard:
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
                    return ValueString(index, Value<bool>(index));
                case ConfigTypes.List:
                case ConfigTypes.Slider:
                    return ValueString(index, Value<int>(index));
                case ConfigTypes.Keyboard:
                    return ValueString(index, Value<Keys>(index));
            }

            return "";
        }
        public virtual string ValueString(int index, object value)
        {
            ConfigTypes type = SettingType(index);
            switch (type)
            {
                case ConfigTypes.OnOffSwitch:
                    return (bool)value ? "On" : "Off";
                case ConfigTypes.List:
                case ConfigTypes.Slider:
                    return string.Format(FormatString(index), (int)value);
                case ConfigTypes.Keyboard:
                    return ((Keys)value).ToString();
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

        public Range<int> ValueRange(int index)
        {
            return ValueRange(GetEntryIndex(index));
        }
        public virtual Range<int> ValueRange(Tuple<int, int> entry)
        {
            switch (SettingType(entry))
            {
                case ConfigTypes.List:
                case ConfigTypes.Slider:
                    return _Data[entry.Item1].Range;
            }

#if DEBUG
            throw new ArgumentException(
                string.Format(
                    "Tried to get the range of a\nsetting that isn't a number value.\n" +
                    "Type \"{0}\", Label \"{1}\", Entry {2}",
                    this.GetType().Name, SettingLabel(entry), entry));
#endif

            return new Range<int>(0, 0);
        }

        public virtual int ValueInterval(int index)
        {
            switch (SettingType(index))
            {
                case ConfigTypes.List:
                case ConfigTypes.Slider:
                    return 1;
            }

#if DEBUG
            throw new ArgumentException(
                "Tried to get the interval of a\nsetting that isn't a number value.");
#endif

            return 1;
        }

        public ISettings GetSubSettings(int index)
        {
            return ValueObject(GetEntryIndex(index)) as ISettings;
        }
        #endregion
    }
}
