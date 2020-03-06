using System;
using System.Collections.Generic;
using FEXNA_Library;

namespace FEXNA.Options
{
    enum GraphicsSetting { Fullscreen, Zoom, Stereoscopic, Anaglyph, MonitorIndex, MinimizeWhenInactive }

    class GraphicsSettings : SettingsBase
    {
        private bool _Fullscreen;
        private int _Zoom;
        private int _StereoscopicLevel;
        private bool _Anaglyph;
        private Maybe<int> _MonitorIndex;
        private bool _MinimizeWhenInactive;

        public bool Fullscreen { get { return _Fullscreen; } }
        public int Zoom { get { return _Zoom; } }
        public int StereoscopicLevel { get { return _StereoscopicLevel; } }
        public bool Anaglyph { get { return (!_Fullscreen && _StereoscopicLevel > 0) ? true : _Anaglyph; } }
        public Maybe<int> MonitorIndex { get { return _MonitorIndex; } }
        public bool MinimizeWhenInactive { get { return _MinimizeWhenInactive; } }

        private Maybe<int> ZoomMin, ZoomMax;
        
        public GraphicsSettings()
        {
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
                SettingsData.Create("Fullscreen", ConfigTypes.OnOffSwitch, false,
                    dependentSettings: new int[] { (int)GraphicsSetting.Anaglyph }),
                SettingsData.Create("Zoom", ConfigTypes.Number, 2),
                SettingsData.Create("Stereoscopic 3D", ConfigTypes.Number, 0,
                    dependentSettings: new int[] { (int)GraphicsSetting.Anaglyph },
                    formatString: "On ({0})", rangeMin: 0, rangeMax: Global.MAX_STEREOSCOPIC_LEVEL),
                SettingsData.Create("  Red-Cyan (3D)", ConfigTypes.OnOffSwitch, false),
                SettingsData.Create("Monitor Index", ConfigTypes.Number, Maybe<int>.Nothing),
                SettingsData.Create("Minimize When Inactive", ConfigTypes.OnOffSwitch, false)
            };
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

            ZoomMin = otherGraphics.ZoomMin;
            ZoomMax = otherGraphics.ZoomMax;
        }
        
        public override bool IsOptionEnabled(int index)
        {
            if (index == (int)GraphicsSetting.Anaglyph)
                return _Fullscreen && (_StereoscopicLevel > 0);

            return base.IsOptionEnabled(index);
        }

        public override object ValueObject(Tuple<int, int> entry)
        {
            switch (entry.Item1)
            {
                case (int)GraphicsSetting.Fullscreen:
                    return _Fullscreen;
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
                case (int)GraphicsSetting.Stereoscopic:
                    // Don't show value if 0
                    if (_StereoscopicLevel == 0)
                        return "Off";
                    break;
                case (int)GraphicsSetting.Anaglyph:
                    return this.Anaglyph ? "On" : "Off";
            }

            return base.ValueString(index);
        }

        public override void SetValue(Tuple<int, int> entry, object value)
        {
            switch (entry.Item1)
            {
                // bool
                case (int)GraphicsSetting.Fullscreen:
                    SetValue(entry, ref _Fullscreen, value);
                    break;
                case (int)GraphicsSetting.Anaglyph:
                    SetValue(entry, ref _Anaglyph, value);
                    break;
                case (int)GraphicsSetting.MinimizeWhenInactive:
                    SetValue(entry, ref _MinimizeWhenInactive, value);
                    break;
                // int
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
                    return new Range<int>(Global.zoom_min, Global.zoom_max);
                    return new Range<int>(ZoomMin, ZoomMax); //@Yeti
                case (int)GraphicsSetting.MonitorIndex:
                    return new Range<int>(0, 0);
            }

            return base.ValueRange(index);
        }
        #endregion
    }
}
