using System;
using System.Collections.Generic;
using TactileLibrary;

namespace Tactile.Options
{
    enum ConfigTypes { None, List, Slider, OnOffSwitch, Button, Keyboard, Gamepad, SubSettings }

    interface ISettings : ICloneable
    {
        /// <summary>
        /// Copies the data of another <see cref="ISettings"/> into this object.
        /// Expects the other object to be the same same class.
        /// Returns true on success.
        /// </summary>
        /// <param name="other">The other <see cref="ISettings"/> object.</param>
        bool CopySettingsFrom(ISettings other);

        /// <summary>
        /// Restores all settings to their default values.
        /// </summary>
        void RestoreDefaults();

        /// <summary>
        /// Restores one setting to its default value.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        void RestoreDefaultValue(int index);

        /// <summary>
        /// Gets the label strings of all settings.
        /// </summary>
        IEnumerable<string> SettingLabels { get; }
        /// <summary>
        /// Gets the label string of a setting.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        string SettingLabel(int index);

        /// <summary>
        /// Gets the <see cref="ConfigTypes"/> of a setting.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        ConfigTypes SettingType(int index);

        /// <summary>
        /// Gets a collection of indices of other settings that should be
        /// updated when the given setting changes.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        IEnumerable<int> DependentSettings(int index);

        /// <summary>
        /// Returns true if the given setting, when changed in an options menu,
        /// should preview the new value even before confirming.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        bool SettingUpdatesBeforeConfirm(int index);

        /// <summary>
        /// Changes the value of a setting and confirms the change.
        /// Also called when modifying values of temporary settings before
        /// copying them back to real settings objects.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        /// <param name="value">The new value of the setting.</param>
        void ConfirmSetting(int index, object value);

        /// <summary>
        /// Returns true if the setting is enabled for modifying.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        bool IsSettingEnabled(int index);

        /// <summary>
        /// Gets the value of a setting as <see cref="{T}"/>.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        T Value<T>(int index);

        /// <summary>
        /// Gets the value of a setting as an <see cref="object"/>.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        object ValueObject(int index);

        /// <summary>
        /// Gets a string representing a setting's value.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        string ValueString(int index);
        /// <summary>
        /// Gets a string representing a setting's value.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        string ValueString(int index, object value);

        /// <summary>
        /// Changes the value of a setting.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        /// <param name="value">The new value of the setting.</param>
        void SetValue(int index, object value);

        /// <summary>
        /// Copies the value of a setting from another <see cref="ISettings"/>.
        /// </summary>
        /// <param name="other">The other <see cref="ISettings"/> object.</param>
        /// <param name="index">The index of the setting.</param>
        void CopyValueFrom(ISettings other, int index);

        /// <summary>
        /// Gets range of values for a setting.
        /// Throws an exception if the setting is not numeric.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        Range<int> ValueRange(int index);

        /// <summary>
        /// Gets interval step a setting changes by.
        /// Throws an exception if the setting is not numeric.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        int ValueInterval(int index);

        /// <summary>
        /// Gets the child settings of these settings at the given index.
        /// </summary>
        /// <param name="index">The index of the setting.</param>
        ISettings GetSubSettings(int index);
    }
}
