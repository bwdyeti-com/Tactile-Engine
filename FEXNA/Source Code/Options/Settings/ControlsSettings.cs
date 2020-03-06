using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace FEXNA.Options
{
    enum ControlsSetting { Rumble, AnalogDeadZone, IconSet, KeyboardConfig }
    enum ButtonIcons { DS, Xbox360 }

    class ControlsSettings : SettingsBase
    {
        private bool _Rumble;
        private int _AnalogDeadZone;
        private int _IconSet;
        private Keys[] _KeyboardConfig;

        public bool Rumble { get { return _Rumble; } }
        public int AnalogDeadZone { get { return _AnalogDeadZone; } }
        public ButtonIcons IconSet { get { return (ButtonIcons)_IconSet; } }
        public Keys[] KeyboardConfig { get { return _KeyboardConfig; } }
        
        public ControlsSettings() { }
        private ControlsSettings(ControlsSettings source) : this()
        {
            CopySettingsFrom(source);
        }

        protected override List<SettingsData> GetSettingsData()
        {
            return new List<SettingsData>
            {
                SettingsData.Create("Rumble", ConfigTypes.OnOffSwitch, false),
                // Need percent sign %
                SettingsData.Create("Left Analog Dead Zone", ConfigTypes.Slider, 10,
                    formatString: "{0}%", rangeMin: 0, rangeMax: 50),
                SettingsData.Create("Button Icons", ConfigTypes.Number, 0,
                    rangeMin: 0, rangeMax: 1),
                SettingsData.CreateCollection(
                    new string[] { "Down", "Left", "Right", "Up",
                        "A\nSelect/Confirm", "B\nCancel", "Y\nCursor Speed", "X\nEnemy Range",
                        "L\nNext Unit", "R\nStatus", "Start\nSkip/Map", "Select\nMenu" },
                    ConfigTypes.Input,
                    new Keys[] { Keys.NumPad2, Keys.NumPad4, Keys.NumPad6, Keys.NumPad8,
                        Keys.X, Keys.Z, Keys.D, Keys.C,
                        Keys.A, Keys.S, Keys.Enter, Keys.RightShift },
                    rangeMin: 0, rangeMax: 10),
            };
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
                case (int)ControlsSetting.KeyboardConfig:
                    return _KeyboardConfig[entry.Item2];
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
                        _KeyboardConfig[oldIndex] = _KeyboardConfig[entry.Item2];
                    }
                    SetValue(entry, ref _KeyboardConfig, value);
                    break;
            }
        }
        #endregion
    }
}
