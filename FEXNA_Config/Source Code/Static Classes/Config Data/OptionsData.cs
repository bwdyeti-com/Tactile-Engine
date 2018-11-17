using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEXNA.ConfigData
{
    public struct OptionsData
    {
        public string Label { get; internal set; }
        public bool Gauge { get; internal set; }
        public byte GaugeMin { get; internal set; }
        public byte GaugeMax { get; internal set; }
        public byte GaugeInterval { get; internal set; }
        public int GaugeWidth { get; internal set; }
        public int GaugeOffset { get; internal set; }
        public OptionsSetting[] Options { get; internal set; }

        public int gauge_offset { get { return GaugeWidth + GaugeOffset; } }
    }

    public struct OptionsSetting
    {
        public int Offset { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        internal OptionsSetting(int offset, string name, string description) : this()
        {
            Offset = offset;
            Name = name;
            Description = description;
        }
    }
}
