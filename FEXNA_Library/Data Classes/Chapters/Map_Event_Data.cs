using System.IO;
using System.Collections.Generic;
using ListExtension;
using ArrayExtension;

namespace FEXNA_Library
{
    public class Map_Event_Data
    {
        public List<Event_Data> Events = new List<Event_Data>();

        #region Serialization
        public void write(BinaryWriter writer)
        {
            Events.write(writer);
        }

        public static Map_Event_Data read(BinaryReader reader)
        {
            Map_Event_Data event_data = new Map_Event_Data();
            event_data.Events.read(reader);
            return event_data;
        }
        #endregion

        public Map_Event_Data copy()
        {
            Map_Event_Data new_data = new Map_Event_Data();
            foreach (Event_Data data in Events)
                new_data.Events.Add(data.copy());
            return new_data;
        }
    }

    public enum Event_Action_Triggers { Any, Turn, Combat, Battle, Staff, Wait }
    public enum Event_Action_Timings { Start, End, Any }
    public class Event_Data
    {
        public string name;
        public int trigger; // 0 = Run Once, 1 = Parallel, 2 = Triggered, 3 = Called
        public int trigger_action; // 0 = Any, 1 = Turn, 2 = Combat, 3 = Battle, 4 = Staff, 5 = Wait
        public int trigger_timing; // 0 = Start, 1 = End
        public int trigger_turn; // Team turn, actual turn determined with if controls
        public List<Event_Control> data = new List<Event_Control>();

        #region Serialization
        public static Event_Data read(BinaryReader reader)
        {
            Event_Data event_data = new Event_Data { data = new List<Event_Control>() };
            event_data.name = reader.ReadString();
            event_data.trigger = reader.ReadInt32();
            event_data.trigger_action = reader.ReadInt32();
            event_data.trigger_timing = reader.ReadInt32();
            event_data.trigger_turn = reader.ReadInt32();
            event_data.data.read(reader);
            return event_data;
        }

        public void write(BinaryWriter writer)
        {
            writer.Write(name);
            writer.Write(trigger);
            writer.Write(trigger_action);
            writer.Write(trigger_timing);
            writer.Write(trigger_turn);
            data.write(writer);
        }
        #endregion

        public override string ToString()
        {
            return string.Format("Event_Data: \"{0}\"", name);
        }

        public Event_Data copy()
        {
            Event_Data new_data = new Event_Data
            {
                name = name,
                trigger = trigger,
                trigger_action = trigger_action,
                trigger_timing = trigger_timing,
                trigger_turn = trigger_turn,
                data = new List<Event_Control>(this.data)
            };
            //foreach (Event_Control control in data)
            //    new_data.data.Add(control);
            return new_data;
        }
    }

    public struct Event_Control
    {
        public int Key;
        public string[] Value;

        public override string ToString()
        {
            return string.Format("Event Control: Key {0}, {1} value lines", Key, Value.Length);
        }

        /*public Event_Control(int key, string[] value) //Debug
        {
            Key = key;
            Value = value;
        }*/
        public Event_Control(int key, params string[] values) //: this(key, values) { }
        {
            Key = key;
            Value = values;
        }

        #region Serialization
        public static Event_Control read(BinaryReader reader)
        { 
            int key = reader.ReadInt32();
            string[] value = new string[] { };
            value = value.read(reader);
            return new Event_Control(key, value);
        }

        public void write(BinaryWriter writer)
        {
            writer.Write(Key);
            Value.write(writer);
        }
        #endregion
    }
}
