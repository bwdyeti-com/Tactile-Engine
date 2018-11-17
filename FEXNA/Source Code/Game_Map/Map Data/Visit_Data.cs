using System.IO;

namespace FEXNA.Map
{
    internal struct Visit_Data
    {
        public string Name { get; private set; }
        public string VisitEvent { get; private set; }
        public string PillageEvent { get; private set; }

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(VisitEvent);
            writer.Write(PillageEvent);
        }

        public static Visit_Data read(BinaryReader reader)
        {
            Visit_Data result = new Visit_Data();
            result.Name = reader.ReadString();
            result.VisitEvent = reader.ReadString();
            result.PillageEvent = reader.ReadString();
            return result;
        }
        #endregion

        public Visit_Data(string visit_event, string pillage_event = "", string name = "") : this()
        {
            VisitEvent = visit_event;
            PillageEvent = pillage_event;
            Name = name;
        }
    }
}
