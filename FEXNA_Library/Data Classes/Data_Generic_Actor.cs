using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using ListExtension;

namespace FEXNA_Library
{
    public class Data_Generic_Actor : IFEXNADataContent
    {
        public string Name;
        public string MiniFaceName = "";
        public string Description = "";
        public List<int> BaseStats = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<int> Growths = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        public List<int> Skills = new List<int>();

        #region Serialization
        public IFEXNADataContent Read_Content(ContentReader input)
        {
            Data_Generic_Actor result = new Data_Generic_Actor();

            result.Name = input.ReadString();
            result.MiniFaceName = input.ReadString();
            result.Description = input.ReadString();
            result.BaseStats.read(input);
            result.Growths.read(input);
            result.Skills.read(input);

            return result;
        }

        public void Write(BinaryWriter output)
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
            Name = source.Name;
            MiniFaceName = source.MiniFaceName;
            Description = source.Description;
            BaseStats = new List<int>(source.BaseStats);
            Growths = new List<int>(source.Growths);
            Skills = new List<int>(source.Skills);
        }

        public override string ToString()
        {
            return string.Format("Generic_Actor: {0}", Name);
        }

        public object Clone()
        {
            return new Data_Generic_Actor(this);
        }
    }
}
