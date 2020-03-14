using System;
using System.Collections.Generic;
using System.IO;
using FEXNA.IO.Serialization;
using FEXNA_Library;

namespace FEXNA.Options
{
    enum GraphicsSetting { Fullscreen, Zoom, Stereoscopic, Anaglyph, MonitorIndex, MinimizeWhenInactive }
    enum WindowMode { Windowed, Fullscreen }

    class GraphicsSettings : SettingsBase, ISerializableGameObject
    {
        const int MAX_STEREOSCOPIC_LEVEL = 10;

        private WindowMode _Fullscreen;
        private int _Zoom;
        private int _StereoscopicLevel;
        private bool _Anaglyph;
        private Maybe<int> _MonitorIndex;
        private bool _MinimizeWhenInactive;

        public bool Fullscreen { get { return _Fullscreen == WindowMode.Fullscreen; } }
        public int Zoom { get { return _Zoom; } }
        public int StereoscopicLevel { get { return _StereoscopicLevel; } }
        public bool Stereoscopic { get { return _StereoscopicLevel > 0; } }
        public bool Anaglyph { get { return _Anaglyph; } }
        public bool AnaglyphMode { get { return this.Stereoscopic && (!this.Fullscreen || _Anaglyph); } }
        public Maybe<int> MonitorIndex { get { return _MonitorIndex; } }
        public bool MinimizeWhenInactive { get { return _MinimizeWhenInactive; } }

        public Maybe<int> ZoomMin { get; private set; }
        public Maybe<int> ZoomMax { get; private set; }

        public GraphicsSettings()
        {
#if __MOBILE__
            _Fullscreen = FullscreenSetting.Fullscreen;
            _Zoom = 2;
#endif
            ZoomMin = Maybe<int>.Nothing;
            ZoomMax = Maybe<int>.Nothing;
        }
        private GraphicsSettings(GraphicsSettings source) : this()
        {
            CopySettingsFrom(source);
        }

        protected override List<SettingsData> GetSettingsData()
        {
            return new List<SettingsData>
            {
#if !__MOBILE__
                SettingsData.Create("Window Mode", ConfigTypes.Number, (int)WindowMode.Windowed,
                    dependentSettings: new int[] { (int)GraphicsSetting.Anaglyph },
                    rangeMin: (int)WindowMode.Windowed,
                    rangeMax: (int)WindowMode.Fullscreen),
                SettingsData.Create("Zoom", ConfigTypes.Number,
                    Rendering.GameRenderer.ZOOM),
#else
                new NullSettingsData(),
                new NullSettingsData(),
#endif
                SettingsData.Create("Stereoscopic 3D", ConfigTypes.Number, 0,
                    dependentSettings: new int[] { (int)GraphicsSetting.Anaglyph },
                    formatString: "On ({0})", rangeMin: 0, rangeMax: MAX_STEREOSCOPIC_LEVEL),
                SettingsData.Create("  Red-Cyan (3D)", ConfigTypes.OnOffSwitch, false),
#if !__MOBILE__
                //@Yeti: hide this setting for now, until it's supporting
                new NullSettingsData(),
                //SettingsData.Create("Monitor Index", ConfigTypes.Number, Maybe<int>.Nothing),
                SettingsData.Create("Minimize When Inactive", ConfigTypes.OnOffSwitch, false)
#else
                new NullSettingsData(),
                new NullSettingsData(),
#endif
            };
        }

        /// <summary>
        /// Creates an instance and reads a stream into it.
        /// </summary>
        public static GraphicsSettings ReadObject(BinaryReader reader)
        {
            var result = new GraphicsSettings();
            result.Read(reader);
            return result;
        }

        public void SwitchFullscreen()
        {
            //@Yeti
            var preferredFullscreenSetting = WindowMode.Fullscreen;

            var setting = _Fullscreen == WindowMode.Fullscreen ?
                WindowMode.Windowed : preferredFullscreenSetting;
            ConfirmSetting(GraphicsSetting.Fullscreen, 0, setting);
        }

        public void SetZoomLimits(int zoomMin, int zoomMax)
        {
            ZoomMin = zoomMin;
            ZoomMax = zoomMax;
        }

        #region ICloneable
        public override object Clone()
        {
            return new GraphicsSettings(this);
        }
        #endregion

        #region ISettings
        protected override void CopyAdditionalSettingsFrom(ISettings other)
        {
            var otherGraphics = (GraphicsSettings)other;

#if __MOBILE__
            _Fullscreen = otherGraphics._Fullscreen;
            _Zoom = otherGraphics.Zoom;
#endif

            ZoomMin = otherGraphics.ZoomMin;
            ZoomMax = otherGraphics.ZoomMax;
        }

        public override bool IsSettingEnabled(int index)
        {
            if (index == (int)GraphicsSetting.Anaglyph)
                return this.Fullscreen && (_StereoscopicLevel > 0);

            return base.IsSettingEnabled(index);
        }

        public override object ValueObject(Tuple<int, int> entry)
        {
            switch (entry.Item1)
            {
                case (int)GraphicsSetting.Fullscreen:
                    return (int)_Fullscreen;
                case (int)GraphicsSetting.Zoom:
                    return _Zoom;
                case (int)GraphicsSetting.Stereoscopic:
                    return _StereoscopicLevel;
                case (int)GraphicsSetting.Anaglyph:
                    return _Anaglyph;
                case (int)GraphicsSetting.MonitorIndex:
                    return _MonitorIndex.ValueOrDefault;
                case (int)GraphicsSetting.MinimizeWhenInactive:
                    return _MinimizeWhenInactive;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public override string ValueString(int index)
        {
            var entry = GetEntryIndex(index);

            switch (entry.Item1)
            {
                case (int)GraphicsSetting.Fullscreen:
                    // Show enum name
                    switch(_Fullscreen)
                    {
                        case WindowMode.Windowed:
                            return "Windowed";
                        case WindowMode.Fullscreen:
                        default:
                            return "Fullscreen";
                    }
                case (int)GraphicsSetting.Stereoscopic:
                    // Don't show value if 0
                    if (_StereoscopicLevel == 0)
                        return "Off";
                    break;
                case (int)GraphicsSetting.Anaglyph:
                    return this.AnaglyphMode ? "On" : "Off";
            }

            return base.ValueString(index);
        }

        public override void SetValue(Tuple<int, int> entry, object value)
        {
            switch (entry.Item1)
            {
                // bool
                case (int)GraphicsSetting.Anaglyph:
                    SetValue(entry, ref _Anaglyph, value);
                    break;
                case (int)GraphicsSetting.MinimizeWhenInactive:
                    SetValue(entry, ref _MinimizeWhenInactive, value);
                    break;
                // int
                case (int)GraphicsSetting.Fullscreen:
                    int fullscreen = (int)_Fullscreen;
                    SetValue(entry, ref fullscreen, value);
                    _Fullscreen = (WindowMode)fullscreen;
                    break;
                case (int)GraphicsSetting.Zoom:
                    SetValue(entry, ref _Zoom, value);
                    break;
                case (int)GraphicsSetting.Stereoscopic:
                    SetValue(entry, ref _StereoscopicLevel, value);
                    break;
                // Maybe<int>
                case (int)GraphicsSetting.MonitorIndex:
                    if (value is Maybe<int>)
                        SetValue(entry, ref _MonitorIndex, (Maybe<int>)value);
                    else
                        SetValue(entry, ref _MonitorIndex, (Maybe<int>)(int)value);
                    break;
            }
        }

        public override Range<int> ValueRange(int index)
        {
            var entry = GetEntryIndex(index);

            switch (entry.Item1)
            {
                case (int)GraphicsSetting.Zoom:
                    return new Range<int>(
                        ZoomMin.OrIfNothing(1),
                        ZoomMax.OrIfNothing(2));
                case (int)GraphicsSetting.MonitorIndex:
                    return new Range<int>(0, 0);
            }

            return base.ValueRange(index);
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
            data.ReadValue(out _Fullscreen, "Fullscreen");
            data.ReadValue(out _Zoom, "Zoom");
            data.ReadValue(out _StereoscopicLevel, "StereoscopicLevel");
            data.ReadValue(out _Anaglyph, "Anaglyph");
            data.ReadValue(out _MonitorIndex, "MonitorIndex");
            data.ReadValue(out _MinimizeWhenInactive, "MinimizeWhenInactive");
        }

        public SerializerData GetSaveData()
        {
            return new SerializerData.Builder()
                .Add("Fullscreen", _Fullscreen)
                .Add("Zoom", _Zoom)
                .Add("StereoscopicLevel", _StereoscopicLevel)
                .Add("Anaglyph", _Anaglyph)
                .Add("MonitorIndex", _MonitorIndex)
                .Add("MinimizeWhenInactive", _MinimizeWhenInactive)
                .Build();
        }

        public Dictionary<string, Type> ExpectedData(Version version)
        {
            return new Dictionary<string, Type>
            {
                { "Fullscreen", typeof(WindowMode) },
                { "Zoom", typeof(int) },
                { "StereoscopicLevel", typeof(int) },
                { "Anaglyph", typeof(bool) },
                { "MonitorIndex", typeof(Maybe<int>) },
                { "MinimizeWhenInactive", typeof(bool) },
            };
        }
        #endregion
    }
}
