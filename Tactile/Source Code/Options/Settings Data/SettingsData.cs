using System;
using System.Linq;
using TactileLibrary;

namespace Tactile.Options
{
    abstract class SettingsData
    {
        public string[] Label { get; private set; }
        public ConfigTypes Type { get; private set; }
        public string FormatString { get; private set; }
        private readonly int[] _DependentSettings;
        public IntRange Range { get; private set; }
        public bool UpdateBeforeConfirming { get; private set; }

        public int[] DependentSettings { get { return _DependentSettings == null ? null : _DependentSettings.ToArray(); } }

        protected SettingsData()
        {
            Label = null;
            Type = ConfigTypes.None;
            FormatString = "{0}";
        }
        protected SettingsData(
            string[] label,
            ConfigTypes type,
            string formatString,
            int[] dependentSettings,
            int rangeMin,
            int rangeMax,
            bool updateBeforeConfirming)
        {
            Label = label;
            Type = type;
            FormatString = formatString;
            _DependentSettings = dependentSettings;
            Range = new IntRange(rangeMin, rangeMax);
            UpdateBeforeConfirming = updateBeforeConfirming;
        }

        public static SettingsData<T> Create<T>(
            string label,
            ConfigTypes type,
            T defaultValue,
            string formatString = "{0}",
            int[] dependentSettings = null,
            int rangeMin = 0,
            int rangeMax = 0,
            bool updateBeforeConfirming = false)
        {
            return new SettingsData<T>(
                label,
                type,
                defaultValue,
                formatString,
                dependentSettings,
                rangeMin,
                rangeMax,
                updateBeforeConfirming);
        }
        public static CollectionSettingsData<T> CreateCollection<T>(
            string[] labels,
            ConfigTypes type,
            T[] defaultValue,
            string formatString = "{0}",
            int[] dependentSettings = null,
            int rangeMin = 0,
            int rangeMax = 0,
            bool updateBeforeConfirming = false)
        {
            return new CollectionSettingsData<T>(
                labels,
                type,
                defaultValue,
                formatString,
                dependentSettings,
                rangeMin,
                rangeMax,
                updateBeforeConfirming);
        }

        public abstract int Size { get; }

        public abstract object GetDefaultValue(int offset);

        public abstract void SetValue(ref object setting, object value, Func<IntRange> rangeCallback, int offset);
    }

    class SettingsData<T> : SettingsData
    {
        public T DefaultValue { get; private set; }

        public SettingsData(
            string label,
            ConfigTypes type,
            T defaultValue,
            string formatString,
            int[] dependentSettings,
            int rangeMin,
            int rangeMax,
            bool updateBeforeConfirming) : base(
                new string[] { label },
                type,
                formatString,
                dependentSettings,
                rangeMin,
                rangeMax,
                updateBeforeConfirming)
        {
            DefaultValue = defaultValue;
        }

        public override int Size { get { return 1; } }

        public override object GetDefaultValue(int offset)
        {
            return DefaultValue;
        }

        public override void SetValue(ref object setting, object value, Func<IntRange> rangeCallback, int offset)
        {
            if (rangeCallback != null)
            {
                // If int, clamp to range
                if (typeof(int).IsAssignableFrom(typeof(T)))
                {
                    int v = (int)(object)value;
                    var range = rangeCallback();
                    value = (T)(object)ClampRange(range, v);
                }
                else if (typeof(Maybe<int>).IsAssignableFrom(typeof(T)))
                {
                    Maybe<int> v = (Maybe<int>)(object)value;
                    if (v.IsSomething)
                    {
                        var range = rangeCallback();
                        value = (T)(object)(Maybe<int>)ClampRange(range, v);
                    }
                }
            }

            setting = (T)value;
        }

        protected int ClampRange(IntRange range, int value)
        {
            return Math.Min(range.Maximum,
                Math.Max(range.Minimum, value));
        }

    }

    class CollectionSettingsData<T> : SettingsData
    {
        public T[] DefaultValue { get; private set; }

        public CollectionSettingsData(
            string[] labels,
            ConfigTypes type,
            T[] defaultValue,
            string formatString,
            int[] dependentSettings,
            int rangeMin,
            int rangeMax,
            bool updateBeforeConfirming) : base(
                labels,
                type,
                formatString,
                dependentSettings,
                rangeMin,
                rangeMax,
                updateBeforeConfirming)
        {
            if (labels.Length != defaultValue.Length)
            {
                throw new ArgumentException("CollectionSettingsData labels length\ndoesn't match values length");
            }

            DefaultValue = defaultValue;
        }

        public override int Size { get { return DefaultValue.Length; } }

        public override object GetDefaultValue(int offset)
        {
            return DefaultValue[offset];
        }

        public override void SetValue(ref object setting, object value, Func<IntRange> rangeCallback, int offset)
        {
            if (rangeCallback != null)
            {
                // If int, clamp to range
                if (typeof(int).IsAssignableFrom(typeof(T)))
                {
                    int v = (int)(object)value;
                    var range = rangeCallback();
                    value = (T)(object)ClampRange(range, v);
                }
                else if (typeof(Maybe<int>).IsAssignableFrom(typeof(T)))
                {
                    Maybe<int> v = (Maybe<int>)(object)value;
                    if (v.IsSomething)
                    {
                        var range = rangeCallback();
                        value = (T)(object)(Maybe<int>)ClampRange(range, v);
                    }
                }
            }

            if (setting == null)
                setting = new T[this.Size];

            var settingArray = (T[])setting;
            settingArray[offset] = (T)value;
        }

        protected int ClampRange(IntRange range, int value)
        {
            return Math.Min(range.Maximum,
                Math.Max(range.Minimum, value));
        }

    }
}
