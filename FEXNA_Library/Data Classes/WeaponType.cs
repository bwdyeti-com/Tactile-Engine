using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using ListExtension;

namespace FEXNA_Library
{
    public class WeaponType : IFEXNADataContent
    {
        public int Key;
        public string Name;
        public string StatusHelpName;
        public string EventName;

        public int IconIndex;
        public int ClassReelIconIndex;
        public int StatusIndex;
        public string AnimName;
        public string ThrownAnimName;
        public string StatusDescription;
        public string UnitDescription;
        public bool IsMagic;
        public bool IsStaff;
        public bool DisplayedInStatus;

        public List<int> WtaTypes = new List<int>();
        public List<int> WtaReaverTypes = new List<int>();
        public List<int> WtaRanges = new List<int>();
        public List<int> WtdRanges = new List<int>();

        [ContentSerializer(Optional = true)]
        public int ParentKey;

        #region Serialization
        public IFEXNADataContent Read_Content(ContentReader input)
        {
            WeaponType result = new WeaponType();

            result.Key = input.ReadInt32();
            result.Name = input.ReadString();
            result.StatusHelpName = input.ReadString();
            result.EventName = input.ReadString();

            result.IconIndex = input.ReadInt32();
            result.ClassReelIconIndex = input.ReadInt32();
            result.StatusIndex = input.ReadInt32();
            result.AnimName = input.ReadString();
            result.ThrownAnimName = input.ReadString();
            result.StatusDescription = input.ReadString();
            result.UnitDescription = input.ReadString();
            result.IsMagic = input.ReadBoolean();
            result.IsStaff = input.ReadBoolean();
            result.DisplayedInStatus = input.ReadBoolean();

            result.WtaTypes.read(input);
            result.WtaReaverTypes.read(input);
            result.WtaRanges.read(input);
            result.WtdRanges.read(input);

            result.ParentKey = input.ReadInt32();

            return result;
        }

        public void Write(BinaryWriter output)
        {
            output.Write(Key);
            output.Write(Name);
            output.Write(StatusHelpName);
            output.Write(EventName);

            output.Write(IconIndex);
            output.Write(ClassReelIconIndex);
            output.Write(StatusIndex);
            output.Write(AnimName);
            output.Write(ThrownAnimName);
            output.Write(StatusDescription);
            output.Write(UnitDescription);
            output.Write(IsMagic);
            output.Write(IsStaff);
            output.Write(DisplayedInStatus);

            WtaTypes.write(output);
            WtaReaverTypes.write(output);
            WtaRanges.write(output);
            WtdRanges.write(output);

            output.Write(ParentKey);
        }
        #endregion

        public WeaponType() { }
        public WeaponType(WeaponType source)
        {
            Key = source.Key;
            Name = source.Name;
            StatusHelpName = source.StatusHelpName;
            EventName = source.EventName;

            IconIndex = source.IconIndex;
            ClassReelIconIndex = source.ClassReelIconIndex;
            StatusIndex = source.StatusIndex;
            AnimName = source.AnimName;
            ThrownAnimName = source.ThrownAnimName;
            StatusDescription = source.StatusDescription;
            UnitDescription = source.UnitDescription;
            IsMagic = source.IsMagic;
            IsStaff = source.IsStaff;
            DisplayedInStatus = source.DisplayedInStatus;

            WtaTypes = new List<int>(source.WtaTypes);
            WtaReaverTypes = new List<int>(source.WtaReaverTypes);
            WtaRanges = new List<int>(source.WtaRanges);
            WtdRanges = new List<int>(source.WtdRanges);

            ParentKey = source.ParentKey;
        }

        public override string ToString()
        {
            return string.Format("WeaponType {0}: {1}", Key, Name);
        }

        public HashSet<WeaponType> type_and_parents(List<WeaponType> list)
        {
            HashSet<WeaponType> result = new HashSet<WeaponType>();
            WeaponType type = this;
            while (type.Key != 0 && !result.Contains(type))
            {
                result.Add(type);
                type = list[type.ParentKey];
            }
            return result;
        }

        public object Clone()
        {
            return new WeaponType(this);
        }
    }
}
