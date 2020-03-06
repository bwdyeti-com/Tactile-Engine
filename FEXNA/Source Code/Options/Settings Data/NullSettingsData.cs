using System;
using FEXNA_Library;

namespace FEXNA.Options
{
    class NullSettingsData : SettingsData
    {
        public NullSettingsData() : base(null, ConfigTypes.None) { }

        public override int Size
        {
            get
            {
                return 0;
            }
        }

        public override object GetDefaultValue(int offset)
        {
            return null;
        }

        public override void SetValue(ref object setting, object value, Func<Range<int>> rangeCallback, int offset) { }
    }
}
