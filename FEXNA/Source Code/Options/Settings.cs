using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using FEXNAVersionExtension;

namespace FEXNA.Options
{
    class Settings
    {
        private GeneralSettings _General;
        private GraphicsSettings _Graphics;
        private AudioSettings _Audio;
        private ControlsSettings _Controls;

        public GeneralSettings General { get { return _General; } }
        public GraphicsSettings Graphics { get { return _Graphics; } }
        public AudioSettings Audio { get { return _Audio; } }
        public ControlsSettings Controls { get { return _Controls; } }

        public Settings()
        {
            _General = new GeneralSettings();
            _Graphics = new GraphicsSettings();
            _Audio = new AudioSettings();
            _Controls = new ControlsSettings();
        }

        public void RestoreDefaults()
        {
            _General.RestoreDefaults();
            _Graphics.RestoreDefaults();
            _Audio.RestoreDefaults();
            _Controls.RestoreDefaults();
        }

        #region Serialization
        public void Read(BinaryReader reader)
        {
            int zoom;
            bool fullscreen = false;
            int stereoscopicLevel = 0;
            bool anaglyph = true;
            Metrics_Settings metrics = Metrics_Settings.Not_Set;
            bool updatesActive = false;
            bool rumble = true;
            Keys[] keyConfig;

            if (Global.LOADED_VERSION.older_than(0, 4, 2, 0))
            {
                zoom = reader.ReadInt32();
                bool unused = reader.ReadBoolean();
            }
            else if (Global.LOADED_VERSION.older_than(0, 4, 6, 3))
            {
                zoom = reader.ReadInt32();
                fullscreen = reader.ReadBoolean();
                stereoscopicLevel = reader.ReadInt32();
                anaglyph = reader.ReadBoolean();
            }
            else if (Global.LOADED_VERSION.older_than(0, 5, 0, 6))
            {
                zoom = reader.ReadInt32();
                fullscreen = reader.ReadBoolean();
                stereoscopicLevel = reader.ReadInt32();
                anaglyph = reader.ReadBoolean();
                metrics = (Metrics_Settings)reader.ReadInt32();
            }
            else
            {
                zoom = reader.ReadInt32();
                fullscreen = reader.ReadBoolean();
                stereoscopicLevel = reader.ReadInt32();
                anaglyph = reader.ReadBoolean();
                metrics = (Metrics_Settings)reader.ReadInt32();
                updatesActive = reader.ReadBoolean();
                rumble = reader.ReadBoolean();
            }
            keyConfig = ReadLegacyInputs(reader);

            Global.zoom = zoom;
            Global.fullscreen = fullscreen;
            Global.stereoscopic_level = stereoscopicLevel;
            Global.anaglyph = anaglyph;
            Global.metrics = metrics;
            Global.updates_active = updatesActive;
            Global.rumble = rumble;
            for (int i = keyConfig.Length - 1; i >= 0; i--)
            {
                Input.remap_key((Inputs)i, keyConfig[i]);
            }

            // Apply the read values to the settings
            RestoreDefaults();
            Graphics.SetValue(GraphicsSetting.Zoom, 0, zoom);
            Graphics.SetValue(GraphicsSetting.Fullscreen, 0, fullscreen);
            Graphics.SetValue(GraphicsSetting.Stereoscopic, 0, stereoscopicLevel);
            Graphics.SetValue(GraphicsSetting.Anaglyph, 0, anaglyph);
            if (metrics != Metrics_Settings.Not_Set)
                General.ConfirmSetting(GeneralSetting.Metrics, 0, metrics == Metrics_Settings.On);
            General.SetValue(GeneralSetting.CheckForUpdates, 0, updatesActive);
            Controls.SetValue(ControlsSetting.Rumble, 0, rumble);
            for (int i = keyConfig.Length - 1; i >= 0; i--)
            {
                Controls.SetValue(ControlsSetting.KeyboardConfig, i, keyConfig[i]);
            }
        }

        private Keys[] ReadLegacyInputs(BinaryReader reader)
        {
            Keys[] keys;
            ControlsSettings controls = new ControlsSettings();
            keys = controls.KeyboardConfig.ToArray();

            if (Global.LOADED_VERSION.older_than(0, 6, 5, 0))
            {
                for (byte i = 0; i < (byte)Inputs.Select; i++)
                {
                    Keys key = (Keys)reader.ReadInt32();
                    // If a remappable key
                    if (FEXNA.Input.REMAPPABLE_KEYS.ContainsKey(key))
                        // If not loaded already
                        if (!keys.Take(i).Contains(key))
                            keys[i] = key;
                }
            }
            else
            {
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    Inputs key = (Inputs)reader.ReadByte();
                    Keys value = (Keys)reader.ReadInt32();

                    // If a remappable key
                    if (FEXNA.Input.REMAPPABLE_KEYS.ContainsKey(value))
                        // If not loaded already
                        if (!keys.Take(i).Contains(value))
                            keys[i] = value;
                }
            }
            return keys;
        }
        #endregion
    }
}
