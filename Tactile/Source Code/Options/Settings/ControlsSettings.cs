using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tactile.IO.Serialization;
using Microsoft.Xna.Framework.Input;

namespace Tactile.Options
{
    enum ControlsSetting { Rumble, AnalogDeadZone, IconSet, Keyboard, Gamepad }
    enum ButtonIcons { DS, Xbox360 }

    class ControlsSettings : SettingsBase, ISerializableGameObject
    {
        private bool _Rumble;
        private int _AnalogDeadZone;
        private int _IconSet;
        private KeyboardSettings _Keyboard;
        private GamepadSettings _Gamepad;

        public bool Rumble { get { return _Rumble; } }
        public float AnalogDeadZone { get { return _AnalogDeadZone / 100f; } }
        public ButtonIcons IconSet { get { return (ButtonIcons)_IconSet; } }
        public Keys[] KeyboardConfig { get { return _Keyboard.KeyboardConfig; } }
        public Buttons[] GamepadConfig { get { return _Gamepad.GamepadConfig; } }

        public ControlsSettings() { }
        private ControlsSettings(ControlsSettings source) : this()
        {
            CopySettingsFrom(source);
        }

        protected override void RestoreAdditionalDefaults()
        {
            if (_Keyboard == null)
                _Keyboard = new KeyboardSettings();
            else
                _Keyboard.RestoreDefaults();

            if (_Gamepad == null)
                _Gamepad = new GamepadSettings();
            else
                _Gamepad.RestoreDefaults();
        }

        protected override List<SettingsData> GetSettingsData()
        {
            return new List<SettingsData>
            {
                SettingsData.Create("Rumble", ConfigTypes.OnOffSwitch, false,
                    updateBeforeConfirming: true),
                //@Debug: Need percent sign % in the font
                SettingsData.Create("Left Analog Dead Zone", ConfigTypes.Slider, 20,
                    formatString: "{0}%", rangeMin: 10, rangeMax: 80),
                SettingsData.Create("Button Icons", ConfigTypes.List, (int)ButtonIcons.Xbox360,
                    rangeMin: 0, rangeMax: 1),
                SettingsData.Create("Keyboard Controls", ConfigTypes.SubSettings, ""),
                SettingsData.Create("Gamepad Controls", ConfigTypes.SubSettings, ""),
            };
        }

        internal void SetLegacyKeyboardConfig(Keys[] keyConfig)
        {
            for (int i = keyConfig.Length - 1; i >= 0; i--)
            {
                _Keyboard.SetValue(KeyboardSetting.KeyboardConfig, i, keyConfig[i]);
            }
        }

        /// <summary>
        /// Creates an instance and reads a stream into it.
        /// </summary>
        public static ControlsSettings ReadObject(BinaryReader reader)
        {
            var result = new ControlsSettings();
            result.Read(reader);
            return result;
        }

        #region ICloneable
        public override object Clone()
        {
            return new ControlsSettings(this);
        }
        #endregion

        #region ISettings
        protected override void CopyAdditionalSettingsFrom(ISettings other)
        {
            var otherControls = (ControlsSettings)other;

            _Gamepad.CopySettingsFrom(otherControls._Gamepad);
        }

        public override void ConfirmSetting(int index, object value)
        {
            base.ConfirmSetting(index, value);

            var entry = GetEntryIndex(index);

            switch (entry.Item1)
            {
                case (int)ControlsSetting.IconSet:
                    // Update icons
                    Input.RefreshControlScheme();
                    break;
            }
        }

        public override object ValueObject(Tuple<int, int> entry)
        {
            switch (entry.Item1)
            {
                case (int)ControlsSetting.Rumble:
                    return _Rumble;
                case (int)ControlsSetting.AnalogDeadZone:
                    return _AnalogDeadZone;
                case (int)ControlsSetting.IconSet:
                    return _IconSet;
                case (int)ControlsSetting.Keyboard:
                    return _Keyboard;
                case (int)ControlsSetting.Gamepad:
                    return _Gamepad;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public override string ValueString(int index)
        {
            var entry = GetEntryIndex(index);

            switch (entry.Item1)
            {
                case (int)ControlsSetting.IconSet:
                    // System names
                    switch (Value<int>(index))
                    {
                        case 0:
                        default:
                            return "DS";
                        case 1:
                            return "Xbox";
                    }
            }

            return base.ValueString(index);
        }

        public override void SetValue(Tuple<int, int> entry, object value)
        {
            switch (entry.Item1)
            {
                // bool
                case (int)ControlsSetting.Rumble:
                    SetValue(entry, ref _Rumble, value);
                    break;
                // int
                case (int)ControlsSetting.AnalogDeadZone:
                    SetValue(entry, ref _AnalogDeadZone, value);
                    break;
                case (int)ControlsSetting.IconSet:
                    SetValue(entry, ref _IconSet, value);
                    break;
                // Buttons
                case (int)ControlsSetting.Keyboard:
                case (int)ControlsSetting.Gamepad:
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
            data.ReadValue(out _Rumble, "Rumble");
            data.ReadValue(out _AnalogDeadZone, "AnalogDeadZone");
            data.ReadValue(out _IconSet, "IconSet");
            data.ReadValue(out _Keyboard, "Keyboard");
            data.ReadValue(out _Gamepad, "Gamepad");
        }

        public SerializerData GetSaveData()
        {
            return new SerializerData.Builder()
                .Add("Rumble", _Rumble)
                .Add("AnalogDeadZone", _AnalogDeadZone)
                .Add("IconSet", _IconSet)
                .Add("Keyboard", _Keyboard)
                .Add("Gamepad", _Gamepad)
                .Build();
        }

        public Dictionary<string, Type> ExpectedData(Version version)
        {
            return new Dictionary<string, Type>
            {
                { "Rumble", typeof(bool) },
                { "AnalogDeadZone", typeof(int) },
                { "IconSet", typeof(int) },
                { "Keyboard", typeof(KeyboardSettings) },
                { "Gamepad", typeof(GamepadSettings) },
            };
        }
        #endregion
    }
}
