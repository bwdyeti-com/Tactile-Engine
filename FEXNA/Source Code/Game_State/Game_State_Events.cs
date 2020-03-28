using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using FEXNA_Library;
using HashSetExtension;
using FEXNAVersionExtension;

namespace FEXNA
{
    partial class Game_State
    {
        private Event_Data_Handler Event_Handler = new Event_Data_Handler();

        #region Serialization
        public void write_events(BinaryWriter writer)
        {
            Event_Handler.write(writer);
        }

        public void read_events(BinaryReader reader)
        {
            Event_Handler = Event_Data_Handler.read(reader);
        }
        #endregion

        #region Accessors
        public Event_Data_Handler event_handler { get { return Event_Handler; } }
        private Map_Event_Data map_event_data { get { return Event_Handler.event_data; } }
        #endregion

        private void reset_events()
        {
            Event_Handler = new Event_Data_Handler();
        }

        #region Event Activation
#if DEBUG
        internal void activate_autorun_events()
#else
        private void activate_autorun_events()
#endif
        {
            HashSet<int> events_to_start = new HashSet<int>();
            // Get events that match the parameters
            int index = 0;
            while (index < Event_Handler.events.Count)
            {
                int i = Event_Handler.events[index];
                Event_Data event_data = map_event_data.Events[i];
                if (event_data.trigger == 0)
                {
                    events_to_start.Add(i);
                }
                if (Event_Handler.events.Contains(i))
                    index++;
            }
            // Run those events
            start_new_events(events_to_start, true);

#if DEBUG
            // Run debug event if playtesting from editor
            if (Global.UnitEditorActive)
            {
                activate_event_by_name("Debug Playtest", true);
            }
#endif
        }

        internal void any_trigger_events(int team_turn = -1)
        {
            if (team_turn == -1)
                team_turn = Team_Turn;
            activate_events_by_action(Event_Action_Triggers.Any, 0, team_turn);
            activate_events_by_action(Event_Action_Triggers.Any, 1, team_turn);
        }
        internal void any_trigger_start_events(int team_turn = -1)
        {
            if (team_turn == -1)
                team_turn = Team_Turn;
            activate_events_by_action(Event_Action_Triggers.Any, 0, team_turn);
        }
        internal void any_trigger_end_events(int team_turn = -1)
        {
            if (team_turn == -1)
                team_turn = Team_Turn;
            activate_events_by_action(Event_Action_Triggers.Any, 1, team_turn);
        }
        private void turn_start_events(int team_turn = -1)
        {
            if (team_turn == -1)
                team_turn = Team_Turn;
            activate_events_by_action(Event_Action_Triggers.Turn, 0, team_turn);
            activate_events_by_action(Event_Action_Triggers.Any, 0, team_turn);
        }
        private void turn_end_events(int team_turn = -1)
        {
            if (team_turn == -1)
                team_turn = Team_Turn;
            activate_events_by_action(Event_Action_Triggers.Turn, 1, team_turn);
            activate_events_by_action(Event_Action_Triggers.Any, 1, team_turn);
        }
        internal void combat_events(bool start, bool staff, int team_turn = -1) //private //Yeti 
        {
            if (team_turn == -1)
                team_turn = Team_Turn;
            int timing = start ? 0 : 1;
            if (staff)
                activate_events_by_action(Event_Action_Triggers.Staff, timing, team_turn);
            else
                activate_events_by_action(Event_Action_Triggers.Battle, timing, team_turn);
            activate_events_by_action(Event_Action_Triggers.Combat, timing, team_turn);
            activate_events_by_action(Event_Action_Triggers.Any, timing, team_turn);
        }
        internal void unit_wait_events(int team_turn = -1)
        {
            if (team_turn == -1)
                team_turn = Team_Turn;
            activate_events_by_action(Event_Action_Triggers.Wait, 0, team_turn);
            activate_events_by_action(Event_Action_Triggers.Wait, 1, team_turn);
            activate_events_by_action(Event_Action_Triggers.Any, 0, team_turn);
            activate_events_by_action(Event_Action_Triggers.Any, 1, team_turn);
        }

        private void activate_events_by_action(Event_Action_Triggers action, int timing, int turn)
        {
            if (action == Event_Action_Triggers.Any && timing == 1)
            { }
            HashSet<int> events_to_start = new HashSet<int>();
            // Get events that match the parameters
            int index = 0;
            while (index < Event_Handler.events.Count)
            {
                int i = Event_Handler.events[index];
                Event_Data event_data = map_event_data.Events[i];
                if (event_data.trigger == 2)
                    if (event_data.trigger_action == (int)action &&
                        event_data.trigger_timing == timing && (event_data.trigger_turn == turn || event_data.trigger_turn == 0))
                    {
                        events_to_start.Add(i);
                    }
                if (Event_Handler.events.Contains(i))
                    index++;
            }
            // Run those events
            start_new_events(events_to_start, true);
        }

        internal void activate_event_by_name(string name) //private //Yeti
        {
            HashSet<int> events_to_start = new HashSet<int>();
            // Get events that match the parameters
            foreach (int i in Event_Handler.events)
            {
                Event_Data event_data = map_event_data.Events[i];
                // Only checks events that are Called
                if (event_data.name == name && event_data.trigger == 3)
                    if (event_data.trigger_turn == Team_Turn || event_data.trigger_turn == 0)
                    {
                        events_to_start.Add(i);
                    }
            }
            // Run those events
            start_new_events(events_to_start, false, true);
        }
        internal bool activate_event_by_name(string name, bool insert)
        {
            //foreach (int i in Event_Handler.events) //Debug
            if (Event_Handler.events.Any(x => map_event_data.Events[x].name == name))
            {
                int i = Event_Handler.events.First(x => map_event_data.Events[x].name == name);
                Event_Data event_data = map_event_data.Events[i];
                //if (event_data.name == name)
                //{
                    if (Global.game_system.add_event(i, true, insert))
                    {
                        if (Global.game_system.is_interpreter_running)
                        {
                            wait_time = 2;
                            Global.game_map.clear_move_range();
                        }
                        return true;
                    }
                //    break; //Debug
                //}
            }
#if DEBUG
            else
                if (!Event_Handler.event_data.Events.Any(x => x.name == name))
                    Print.message(string.Format("Failed to find and run event named\n\"{0}\"", name));
#endif
            return false;
        }

        private void start_new_events(IEnumerable<int> events_to_start, bool run_lone_event, bool insert = false)
        {
            if (events_to_start.Any())
            {
                foreach (int id in events_to_start)
                {
                    Global.game_system.add_event(id, run_lone_event, insert);
                }
                if (Global.game_system.is_interpreter_running)
                {
                    wait_time = 2;
                    Global.game_map.clear_move_range();
                }
            }
        }
        #endregion
    }

    class Talk_Event
    {
        public int ActorId1;
        public int ActorId2;
        public string Event_Name;
        public bool Both_Ways;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(ActorId1);
            writer.Write(ActorId2);
            writer.Write(Event_Name);
            writer.Write(Both_Ways);
        }

        public void read(BinaryReader reader)
        {
            ActorId1 = reader.ReadInt32();
            ActorId2 = reader.ReadInt32();
            Event_Name = reader.ReadString();
            Both_Ways = reader.ReadBoolean();
        }
        #endregion

        public override string ToString()
        {
            return string.Format("Talk_Event: {0}-{1}, {2}", ActorId1, ActorId2, Event_Name);
        }

        internal bool for_these_actors(int actorId1, int actorId2)
        {
            if (ActorId1 == actorId1 && ActorId2 == actorId2)
                return true;
            if (ActorId1 == actorId2 && ActorId2 == actorId1 && Both_Ways)
                return true;
            return false;
        }

        internal bool for_these_units(Game_Unit unit1, Game_Unit unit2)
        {
            return for_these_actors(unit1.actor.id, unit2.actor.id);
        }

        internal bool for_these_units(Game_Unit unit)
        {
            if (ActorId1 == unit.actor.id)
                return true;
            if (ActorId2 == unit.actor.id && Both_Ways)
                return true;
            return false;
        }

        internal int other_unit_id(Game_Unit target)
        {
            if (ActorId1 == target.actor.id)
                return ActorId2;
            if (ActorId2 == target.actor.id && Both_Ways)
                return ActorId1;
            return -1;
        }
    }

    class Event_Data_Handler
    {
        const string EVENTS_PATH = @"Data/Map Data/Event Data/";
        const string GLOBAL_EVENT = "Global";

        public string Name = "";
        // Events that have been "deleted" and should not be run again
        private HashSet<string> Finished_Events = new HashSet<string>();
        // Events that have been run through at least once before, so if they are called again they are being repeated
        private HashSet<string> Repeated_Events = new HashSet<string>();
        private Map_Event_Data Event_Data;

        private List<int> Events = new List<int>();

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Name);
            Finished_Events.write(writer);
            Repeated_Events.write(writer);
        }

        public static Event_Data_Handler read(BinaryReader reader)
        {
            Event_Data_Handler result = new Event_Data_Handler();
            result.Name = reader.ReadString();
            result.load_events(reader);
            //result.Finished_Events.read(reader);
            //result.Event_Data = result.Name == "" ? new Map_Event_Data() : Global.Chapter_Text_Content.Load<Map_Event_Data>(@"Data/Map Data/Event Data/" + result.Name);
            //result.refresh_events();
            return result;
        }
        #endregion

        #region Accessors
        public string name { get { return Name; } }

        public Map_Event_Data event_data { get { return Event_Data; } }

        public List<int> events
        {
            get
            {
                return Events;
                /*List<int> result = new List<int>(); //Debug
                for (int i = 0; i < Event_Data.Events.Count; i++)
                    if (!Finished_Events.Contains(Event_Data.Events[i].name))
                        result.Add(i);
                return result;*/
            }
        }
        #endregion

        public void load_events()
        {
            load_events((BinaryReader)null);
        }
        public void load_events(BinaryReader reader)
        {
            if (reader != null)
            {
                Finished_Events.read(reader);
                if (!Global.LOADED_VERSION.older_than(0, 4, 5, 1)) // This is a suspend load, so this isn't needed for public release //Debug
                    Repeated_Events.read(reader);
                else
                    Repeated_Events.Clear();
            }
            else
            {
                Finished_Events.Clear();
                Repeated_Events.Clear();
            }

            Event_Data = new Map_Event_Data();
            add_events(GLOBAL_EVENT);
            //if (Name != "")
            //{
            foreach (string name in Name.Split(Config.EVENT_NAME_DELIMITER, StringSplitOptions.RemoveEmptyEntries))
                add_events(name);
            //}
            refresh_events();
        }
        public void load_events(Map_Event_Data event_data)
        {
            Finished_Events.Clear();
            Repeated_Events.Clear();
            Event_Data = event_data;
            refresh_events();
        }

        private void add_events(string name)
        {
            bool event_exists = Global.content_exists(EVENTS_PATH + name);
#if DEBUG
            System.Diagnostics.Debug.Assert(event_exists,
                string.Format("Event data file \"{0}\" does not exist", name));
#endif
            if (event_exists)
            {
                var event_data = Global.Chapter_Text_Content.Load<Map_Event_Data>(EVENTS_PATH + name);
                Event_Data.Events.AddRange(event_data.Events);
            }
        }

        public void remove(string key)
        {
#if DEBUG
            if (Finished_Events.Contains(key))
                throw new ArgumentException(string.Format("Trying to delete an event that is\nalready deleted: {0}", key));
#endif
            Finished_Events.Add(key);
            refresh_events();
        }

        public void event_completed(string key)
        {
            Repeated_Events.Add(key);
        }

        public bool is_event_repeat(string key)
        {
            return Repeated_Events.Contains(key);
        }

        private void refresh_events()
        {
            Events.Clear();
            for (int i = 0; i < Event_Data.Events.Count; i++)
                if (!Finished_Events.Contains(Event_Data.Events[i].name))
                    Events.Add(i);

        }
    }

    class Home_Base_Event_Data
    {
        public string Name;
        public int Priority;
        public string Event_Name;
        public bool Activated = false;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Priority);
            writer.Write(Event_Name);
            writer.Write(Activated);
        }

        public void read(BinaryReader reader)
        {
            Name = reader.ReadString();
            Priority = reader.ReadInt32();
            Event_Name = reader.ReadString();
            if (!Global.LOADED_VERSION.older_than(0, 4, 5, 5)) // This is a suspend load, so this isn't needed for public release //Debug
                Activated = reader.ReadBoolean();
        }
        #endregion
    }
}
