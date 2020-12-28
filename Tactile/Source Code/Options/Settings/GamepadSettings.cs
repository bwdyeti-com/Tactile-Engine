using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tactile.IO.Serialization;
using Microsoft.Xna.Framework.Input;

namespace Tactile.Options
{
    enum GamepadSetting { ResetGamepad, GamepadConfig }

    class GamepadSettings : SettingsBase, ISerializableGameObject
    {
        private Buttons[] _GamepadConfig;

        public Buttons[] GamepadConfig { get { return _GamepadConfig; } }
        
        public GamepadSettings() { }
        private GamepadSettings(GamepadSettings source) : this()
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
        public static GamepadSettings ReadObject(BinaryReader reader)
        {
            var result = new GamepadSettings();
            result.Read(reader);
            return result;
        }

        #region ICloneable
        public override object Clone()
        {
            return new GamepadSettings(this);
        }
        #endregion

        #region ISettings
        protected override void CopyAdditionalSettingsFrom(ISettings other)
        {
            var otherGamepad = (GamepadSettings)other;
        }

        public override void ConfirmSetting(int index, object value)
        {
            base.ConfirmSetting(index, value);

            var entry = GetEntryIndex(index);

            switch (entry.Item1)
            {
                case (int)GamepadSetting.GamepadConfig:
                    // Update icons
                    Input.RefreshControlScheme();
                    break;
                case (int)GamepadSetting.ResetGamepad:
                    // Reset GamepadConfig
                    int startIndex = GetIndexOfEntry((int)GamepadSetting.GamepadConfig);
                    for (int i = 0; i < _Data[(int)GamepadSetting.GamepadConfig].Size; i++)
                        RestoreDefaultValue(startIndex + i);
                    // Update icons
                    Input.RefreshControlScheme();
                    break;
            }
        }

        public override object ValueObject(Tuple<int, int> entry)
        {
            switch (entry.Item1)
            {
                case (int)GamepadSetting.ResetGamepad:
                    return _Data[entry.Item1].GetDefaultValue(entry.Item2);
                case (int)GamepadSetting.GamepadConfig:
                    return _GamepadConfig[entry.Item2];
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public override void SetValue(Tuple<int, int> entry, object value)
        {
            switch (entry.Item1)
            {
                // Input.Buttons
                case (int)GamepadSetting.GamepadConfig:
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
                case (int)GamepadSetting.ResetGamepad:
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
            data.ReadValue(out _GamepadConfig, "GamepadConfig");
        }

        public SerializerData GetSaveData()
        {
            return new SerializerData.Builder()
                .Add("GamepadConfig", _GamepadConfig)
                .Build();
        }

        public Dictionary<string, Type> ExpectedData(Version version)
        {
            return new Dictionary<string, Type>
            {
                { "GamepadConfig", typeof(Buttons[]) },
            };
        }
        #endregion
    }
}
