using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ArrayExtension;
using FEXNAVersionExtension;
using FEXNADictionaryExtension;

namespace FEXNA.IO
{
    internal class Save_Data
    {
        protected DateTime Time;
        protected string Chapter_Id, Progression_Id;
        protected Game_System System;
        protected Game_Battalions Battalions;
        protected Game_Actors Actors;
        protected Event_Variable_Data<bool> Switches;
        protected Event_Variable_Data<int> Variables;
        protected Game_Ranking Ranking;
        protected Dictionary<string, Game_Ranking> Past_Rankings;

        #region Accessors
        public string chapter_id { get { return Chapter_Id; } }
        public string progression_id { get { return Progression_Id; } }

        public Difficulty_Modes difficulty { get { return System.Difficulty_Mode; } }
        public Mode_Styles style { get { return System.Style; } }

        public Game_Ranking ranking { get { return Ranking; } }

        public DateTime time { get { return Time; } }

        internal Dictionary<string, int> acquired_supports
        {
            get
            {
                Dictionary<string, int> result = new Dictionary<string, int>();
                foreach (int actor_id in Battalions.all_actor_ids)
                    // If this actor actually exists, checks its support data
                    if (Actors.actor_loaded(actor_id))
                        foreach (var pair in Actors[actor_id].supports)
                            // If this support has been activated
                            if (pair.Value > 0)
                            {
                                // Finds the key for this support data
                                foreach (var support_pair in Global.data_supports)
                                {
                                    if ((support_pair.Value.Id1 == actor_id && support_pair.Value.Id2 == pair.Key) ||
                                        (support_pair.Value.Id1 == pair.Key && support_pair.Value.Id2 == actor_id))
                                    {
                                        if (!result.ContainsKey(support_pair.Key))
                                            result.Add(support_pair.Key, 0);
                                        result[support_pair.Key] = Math.Max(result[support_pair.Key], pair.Value);
                                        break;
                                    }
                                }
                            }
                return result;
            }
        }
        #endregion

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Time.ToBinary());
            writer.Write(Chapter_Id);
            writer.Write(Progression_Id);
            System.write(writer);
            Battalions.write(writer);
            Actors.write(writer);
            Switches.write(writer);
            Variables.write(writer);
            Ranking.write(writer);
            Past_Rankings.write(writer);
        }

        public static Save_Data read(BinaryReader reader)
        {
            Save_Data result = new Save_Data();
            if (!Global.LOADED_VERSION.older_than(0, 4, 3, 0))
                result.Time = DateTime.FromBinary(reader.ReadInt64());
            result.Chapter_Id = reader.ReadString();
            result.Progression_Id = reader.ReadString();
            result.System = new Game_System();
            result.System.read(reader);
            if (Global.LOADED_VERSION.older_than(0, 4, 0, 4))
            {
                List<string> previous_chapters = Global.data_chapters[result.Chapter_Id].Prior_Chapters;
                result.System.previous_chapters.AddRange(previous_chapters);
            }
            result.Battalions = new Game_Battalions();
            result.Battalions.read(reader);
            if (Global.LOADED_VERSION.older_than(0, 5, 3, 1))
            {
                if (Global.data_chapters.ContainsKey(result.Chapter_Id))
                    // Correct battalion id
                    result.Battalions.correct_battalion_id(
                        Global.data_chapters[result.Chapter_Id]);
            }


            result.Actors = new Game_Actors();
            result.Actors.read(reader);
            result.Switches = Event_Variable_Data<bool>.read(reader);
            //result.Switches = new bool[Config.EVENT_DATA_LENGTH]; //Debug
            //result.Switches = result.Switches.read(reader);
            result.Variables = Event_Variable_Data<int>.read(reader);
            //result.Variables = new int[Config.EVENT_DATA_LENGTH];
            //result.Variables = result.Variables.read(reader);
            result.Ranking = Game_Ranking.read(reader);
            result.Past_Rankings = new Dictionary<string, Game_Ranking>();
            if (!Global.LOADED_VERSION.older_than(0, 4, 4, 0))
                result.Past_Rankings.read(reader);
            // If this save predates storing difficultt in the ranking object
            if (Global.LOADED_VERSION.older_than(0, 6, 1, 1))
            {
                Difficulty_Modes difficulty = result.System.Difficulty_Mode;
                result.Ranking = new Game_Ranking(result.Ranking, difficulty);
                foreach (var key in result.Past_Rankings.Keys.ToList())
                    result.Past_Rankings[key] = new Game_Ranking(result.Past_Rankings[key], difficulty);
            }

            return result;
        }
        #endregion

        public override string ToString()
        {
            return string.Format("Save_Data: {0}, {1}, {2} - {3}", Chapter_Id,
                System.Difficulty_Mode, System.Style, Time);
        }

        public void save_data(string chapter_id, string progression_id, Dictionary<string, Game_Ranking> past_rankings)
        {
            Time = DateTime.Now;
            Chapter_Id = chapter_id;
            Progression_Id = progression_id;
            System = Global.game_system.copy();
            Battalions = Global.game_battalions;
            Actors = Global.game_actors;
            Switches = new Event_Variable_Data<bool>(Config.EVENT_DATA_LENGTH);
            Variables = new Event_Variable_Data<int>(Config.EVENT_DATA_LENGTH);
            System.get_event_data(Switches, Variables);
            //Event_Processor.get_data(Switches, Variables); //Debug
            Ranking = new Game_Ranking();
            Past_Rankings = past_rankings;
        }

        public static Dictionary<string, Game_Ranking> process_past_ranking(List<Save_Data> previous_data)
        { 
            Dictionary<string, Game_Ranking> past_rankings = new Dictionary<string, Game_Ranking>();

            // Go through data in reverse order, the data at the start of the list is the most important to maintain
            for (int i = previous_data.Count - 1; i >= 0; i--)
            {
                foreach (var pair in previous_data[i].Past_Rankings)
                    past_rankings[pair.Key] = new Game_Ranking(pair.Value);
                past_rankings[previous_data[i].Chapter_Id] = new Game_Ranking(previous_data[i].Ranking);
            }

            return past_rankings;
        }

        public void load_data()
        {
            // Game_System can probably be edited to move the bits that need saved out to somewhere else //Yeti
            // It's mostly just the Rng and playtime?
            // It's mostly just Rng and playtime that should stay in system? or
            Global.game_system = System.copy();
            load_event_data();
            //Event_Processor.set_data(Switches, Variables); //Debug
        }
        public void load_event_data(bool overwriteAlreadySet = true)
        {
            Global.game_system.set_event_data(Switches, Variables, overwriteAlreadySet);
        }
        public void load_actors()
        {
            Global.game_battalions = new Game_Battalions(Battalions);
            Actors.copy_actors_to(Global.game_actors);
        }
        public void load_battalion(int battalion)
        {
            Battalions.copy_battalion_to(Global.game_battalions, battalion);
            Actors.copy_actors_to(Global.game_actors, Global.game_battalions[battalion]);
        }
        /*public void load_data(int battalion) //Debug
        {
            // Game_System can probably be edited to move the bits that need saved out to somewhere else //Yeti
            // It's mostly just the Rng and playtime?
            Global.game_system = System;
            Global.game_battalions = Battalions;
            Actors.copy_actors(Global.game_actors);
            Event_Processor.set_data(Switches, Variables);
        }*/

        public static void reset_old_data()
        {
            Global.game_system = new Game_System();
            Global.game_battalions = new Game_Battalions();
            Global.game_actors = new Game_Actors();
            //Event_Processor.reset_variables(); //Debug
        }

        internal string actor_map_sprite(int actorId)
        {
            if (Actors.actor_loaded(actorId))
                return Actors[actorId].map_sprite_name;
            return "";
        }
    }
}
