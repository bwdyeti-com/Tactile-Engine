using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using ListExtension;

namespace TactileLibrary
{
    public class Data_Generic_Actor : TactileDataContent
    {
        public string Name;
        public string MiniFaceName = "";
        public string Description = "";
        public List<int> BaseStats = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<int> Growths = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        public List<int> Skills = new List<int>();

        #region TactileDataContent
        public override TactileDataContent EmptyInstance()
        {
            return GetEmptyInstance();
        }
        public static Data_Generic_Actor GetEmptyInstance()
        {
            return new Data_Generic_Actor();
        }

        public override void CopyFrom(TactileDataContent other)
        {
            CheckSameClass(other);

            var source = (Data_Generic_Actor)other;

            Name = source.Name;
            MiniFaceName = source.MiniFaceName;
            Description = source.Description;
            BaseStats = new List<int>(source.BaseStats);
            Growths = new List<int>(source.Growths);
            Skills = new List<int>(source.Skills);
        }

        public static Data_Generic_Actor ReadContent(BinaryReader reader)
        {
            var result = GetEmptyInstance();
            result.Read(reader);
            return result;
        }

        public override void Read(BinaryReader input)
        {
            Name = input.ReadString();
            MiniFaceName = input.ReadString();
            Description = input.ReadString();
            BaseStats.read(input);
            Growths.read(input);
            Skills.read(input);
        }

        public override void Write(BinaryWriter output)
        {
            output.Write(Name);
            output.Write(MiniFaceName);
            output.Write(Description);
            BaseStats.write(output);
            Growths.write(output);
            Skills.write(output);
        }
        #endregion

        public Data_Generic_Actor() { }
        public Data_Generic_Actor(Data_Generic_Actor source)
        {
            CopyFrom(source);
        }

        public override string ToString()
        {
            return string.Format("Generic_Actor: {0}", Name);
        }

        public override object Clone()
        {
            return new Data_Generic_Actor(this);
        }
    }
}
