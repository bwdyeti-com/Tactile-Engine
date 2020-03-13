using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FEXNA.IO.Serialization;
using Microsoft.Xna.Framework.Input;

namespace FEXNA.Options
{
    enum ControlsSetting { Rumble, AnalogDeadZone, IconSet, ResetKeyboard, KeyboardConfig, ResetGamepad, GamepadConfig }
    enum ButtonIcons { DS, Xbox360 }

    class ControlsSettings : SettingsBase, ISerializableGameObject
    {
        private bool _Rumble;
        private int _AnalogDeadZone;
        private int _IconSet;
        private Keys[] _KeyboardConfig;
        private Buttons[] _GamepadConfig;

        public bool Rumble { get { return _Rumble; } }
        public float AnalogDeadZone { get { return _AnalogDeadZone / 100f; } }
        public ButtonIcons IconSet { get { return (ButtonIcons)_IconSet; } }
        public Keys[] KeyboardConfig { get { return _KeyboardConfig; } }
        public Buttons[] GamepadConfig { get { return _GamepadConfig; } }
        
        public ControlsSettings() { }
        private ControlsSettings(ControlsSettings source) : this()
        {
            CopySettingsFrom(source);
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
                SettingsData.Create("Button Icons", ConfigTypes.Number, (int)ButtonIcons.Xbox360,
                    rangeMin: 0, rangeMax: 1),
                SettingsData.Create("Keyboard Controls:", ConfigTypes.Button, "Reset to Default"),
                SettingsData.CreateCollection(
                    new string[] { "Down", "Left", "Right", "Up",
                        "A\nSelect/Confirm", "B\nCancel", "Y\nCursor Speed", "X\nEnemy Range",
                        "L\nNext Unit", "R\nStatus", "Start\nSkip/Map", "Select\nMenu" },
                    ConfigTypes.Keyboard,
                    new Keys[] { Keys.NumPad2, Keys.NumPad4, Keys.NumPad6, Keys.NumPad8,
                        Keys.X, Keys.Z, Keys.D, Keys.C,
                        Keys.A, Keys.S, Keys.Enter, Keys.RightShift }),
                SettingsData.Create("Gamepad Controls:", ConfigTypes.Button, "Reset to Default"),
                SettingsData.CreateCollection(
                    new string[] { "Down", "Left", "Right", "Up",
                        "A\nSelect/Confirm", "B\nCancel", "Y\nCursor Speed", "X\nEnemy Range",
                        "L\nNext Unit", "R\nStatus", "Start\nSkip/Map", "Select\nMenu" },
                    ConfigTypes.Gamepad,
                    new Buttons[] { Buttons.DPadDown, Buttons.DPadLeft, Buttons.DPadRight, Buttons.DPadUp,
#if __ANDROID__
            Buttons.Back,
#else
            Buttons.A,
#endif
                        Buttons.B, Buttons.X, Buttons.Y,
                        Buttons.LeftShoulder, Buttons.RightShoulder, Buttons.Start,
#if __ANDROID__
            Buttons.A
#else
            Buttons.Back
#endif
                    }),
            };
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
        }

        public override void ConfirmSetting(int index, object value)
        {
            base.ConfirmSetting(index, value);

            var entry = GetEntryIndex(index);

            switch (entry.Item1)
            {
                case (int)ControlsSetting.IconSet:
                case (int)ControlsSetting.KeyboardConfig:
                case (int)ControlsSetting.GamepadConfig:
                    // Update icons
                    Input.RefreshControlScheme();
                    break;
                case (int)ControlsSetting.ResetKeyboard:
                    // Reset KeyboardConfig
                    int startIndex = GetIndexOfEntry((int)ControlsSetting.KeyboardConfig);
                    for (int i = 0; i < _Data[(int)ControlsSetting.KeyboardConfig].Size; i++)
                        RestoreDefaultValue(startIndex + i);
                    break;
                case (int)ControlsSetting.ResetGamepad:
                    // Reset GamepadConfig
                    startIndex = GetIndexOfEntry((int)ControlsSetting.GamepadConfig);
                    for (int i = 0; i < _Data[(int)ControlsSetting.GamepadConfig].Size; i++)
                        RestoreDefaultValue(startIndex + i);
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
                case (int)ControlsSetting.ResetKeyboard:
                    return _Data[entry.Item1].GetDefaultValue(entry.Item2);
                case (int)ControlsSetting.KeyboardConfig:
                    return _KeyboardConfig[entry.Item2];
                case (int)ControlsSetting.ResetGamepad:
                    return _Data[entry.Item1].GetDefaultValue(entry.Item2);
                case (int)ControlsSetting.GamepadConfig:
                    return _GamepadConfig[entry.Item2];
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
                // Keys
                case (int)ControlsSetting.KeyboardConfig:
                    // Swap if needed
                    if (_KeyboardConfig != null && _KeyboardConfig.Contains((Keys)value))
                    {
                        int oldIndex = Array.IndexOf(_KeyboardConfig, (Keys)value);
                        if (oldIndex != entry.Item2)
                            _KeyboardConfig[oldIndex] = _KeyboardConfig[entry.Item2];
                    }
                    SetValue(entry, ref _KeyboardConfig, value);
                    break;
                // Input.Buttons
                case (int)ControlsSetting.GamepadConfig:
                    // Swap if needed
                    if (_GamepadConfig != null && _GamepadConfig.Contains((Buttons)value))
                    {
                        int oldIndex = Array.IndexOf(_GamepadConfig, (Buttons)value);
                        if (oldIndex != entry.Item2)
                            _GamepadConfig[oldIndex] = _GamepadConfig[entry.Item2];
                    }
                    SetValue(entry, ref _GamepadConfig, value);
                    break;
                // Buttons
                case (int)ControlsSetting.ResetKeyboard:
                case (int)ControlsSetting.ResetGamepad:
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
            data.ReadValue(out _KeyboardConfig, "KeyboardConfig");
            data.ReadValue(out _GamepadConfig, "GamepadConfig");
        }

        public SerializerData GetSaveData()
        {
            return new SerializerData.Builder()
                .Add("Rumble", _Rumble)
                .Add("AnalogDeadZone", _AnalogDeadZone)
                .Add("IconSet", _IconSet)
                .Add("KeyboardConfig", _KeyboardConfig)
                .Add("GamepadConfig", _GamepadConfig)
                .Build();
        }

        public Dictionary<string, Type> ExpectedData(Version version)
        {
            return new Dictionary<string, Type>
            {
                { "Rumble", typeof(bool) },
                { "AnalogDeadZone", typeof(int) },
                { "IconSet", typeof(int) },
                { "KeyboardConfig", typeof(Keys[]) },
                { "GamepadConfig", typeof(Buttons[]) },
            };
        }
        #endregion
    }
}
