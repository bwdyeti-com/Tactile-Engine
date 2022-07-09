using System;
using TactileLibrary;

namespace Tactile.Options
{
    class NullSettingsData : SettingsData
    {
        public NullSettingsData() : base() { }

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

        public override void SetValue(ref object setting, object value, Func<IntRange> rangeCallback, int offset) { }
    }
}
