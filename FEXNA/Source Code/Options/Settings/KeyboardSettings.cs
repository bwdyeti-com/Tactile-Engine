using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FEXNA.IO.Serialization;
using Microsoft.Xna.Framework.Input;

namespace FEXNA.Options
{
    enum KeyboardSetting { ResetKeyboard, KeyboardConfig }

    class KeyboardSettings : SettingsBase, ISerializableGameObject
    {
        private Keys[] _KeyboardConfig;

        public Keys[] KeyboardConfig { get { return _KeyboardConfig; } }
        
        public KeyboardSettings() { }
        private KeyboardSettings(KeyboardSettings source) : this()
        {
            CopySettingsFrom(source);
        }

        protected override List<SettingsData> GetSettingsData()
        {
            return new List<SettingsData>
            {
                SettingsData.Create("Reset to Defaults", ConfigTypes.Button, ""),
                SettingsData.CreateCollection(
                    new string[] { "Down", "Left", "Right", "Up",
                        "A\nSelect/Confirm", "B\nCancel", "Y\nCursor Speed", "X\nEnemy Range",
                        "L\nNext Unit", "R\nStatus", "Start\nSkip/Map", "Select\nMenu" },
                    ConfigTypes.Keyboard,
                    new Keys[] { Keys.NumPad2, Keys.NumPad4, Keys.NumPad6, Keys.NumPad8,
                        Keys.X, Keys.Z, Keys.D, Keys.C,
                        Keys.A, Keys.S, Keys.Enter, Keys.RightShift }),
            };
        }

        /// <summary>
        /// Creates an instance and reads a stream into it.
        /// </summary>
        public static KeyboardSettings ReadObject(BinaryReader reader)
        {
            var result = new KeyboardSettings();
            result.Read(reader);
            return result;
        }

        #region ICloneable
        public override object Clone()
        {
            return new KeyboardSettings(this);
        }
        #endregion

        #region ISettings
        protected override void CopyAdditionalSettingsFrom(ISettings other)
        {
            var otherKeyboard = (KeyboardSettings)other;
        }

        public override void ConfirmSetting(int index, object value)
        {
            base.ConfirmSetting(index, value);

            var entry = GetEntryIndex(index);

            switch (entry.Item1)
            {
                case (int)KeyboardSetting.KeyboardConfig:
                    // Update icons
                    Input.RefreshControlScheme();
                    break;
                case (int)KeyboardSetting.ResetKeyboard:
                    // Reset KeyboardConfig
                    int startIndex = GetIndexOfEntry((int)KeyboardSetting.KeyboardConfig);
                    for (int i = 0; i < _Data[(int)KeyboardSetting.KeyboardConfig].Size; i++)
                        RestoreDefaultValue(startIndex + i);
                    break;
            }
        }

        public override object ValueObject(Tuple<int, int> entry)
        {
            switch (entry.Item1)
            {
                case (int)KeyboardSetting.ResetKeyboard:
                    return _Data[entry.Item1].GetDefaultValue(entry.Item2);
                case (int)KeyboardSetting.KeyboardConfig:
                    return _KeyboardConfig[entry.Item2];
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public override void SetValue(Tuple<int, int> entry, object value)
        {
            switch (entry.Item1)
            {
                // Keys
                case (int)KeyboardSetting.KeyboardConfig:
                    // Swap if needed
                    if (_KeyboardConfig != null && _KeyboardConfig.Contains((Keys)value))
                    {
                        int oldIndex = Array.IndexOf(_KeyboardConfig, (Keys)value);
                        if (oldIndex != entry.Item2)
                            _KeyboardConfig[oldIndex] = _KeyboardConfig[entry.Item2];
                    }
                    SetValue(entry, ref _KeyboardConfig, value);
                    break;
                // Keys
                case (int)KeyboardSetting.ResetKeyboard:
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
            data.ReadValue(out _KeyboardConfig, "KeyboardConfig");
        }

        public SerializerData GetSaveData()
        {
            return new SerializerData.Builder()
                .Add("KeyboardConfig", _KeyboardConfig)
                .Build();
        }

        public Dictionary<string, Type> ExpectedData(Version version)
        {
            return new Dictionary<string, Type>
            {
                { "KeyboardConfig", typeof(Keys[]) },
            };
        }
        #endregion
    }
}
