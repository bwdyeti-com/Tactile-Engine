using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TactileDictionaryExtension;
using TactileListExtension;
using TactileVersionExtension;

namespace Tactile
{
    internal class Game_Actors
    {
        protected Dictionary<int, Game_Actor> actors;
        protected List<Game_Actor> temp_actors;
        protected int Max_Actor_Id = -1;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            actors.write(writer);
            temp_actors.write(writer);
        }

        public void read(BinaryReader reader)
        {
            actors.read(reader);
            temp_actors.read(reader);
        }
        #endregion

        public Game_Actors()
        {
            actors = new Dictionary<int, Game_Actor>();
            temp_actors = new List<Game_Actor>();
        }

        public Game_Actor this[int index]
        {
            get
            {
                index = Math.Max(index, 0);
                int data_count = max_actor_id();
                // Temp Actor
                if (index > data_count)
                {
                    if (index > data_count + temp_actors.Count + 1)
                    {
#if DEBUG
                        Print.message("what are you doing");
#endif
                    }
                    // If a new temp actor
                    if (index == data_count + temp_actors.Count + 1)
                    {
                        temp_actors.Add(new Game_Actor(index));
                    }
                    // If temp actor was nulled before
                    if (temp_actors[index - (data_count + 1)] == null)
                        temp_actors[index - (data_count + 1)] = new Game_Actor(index);
                    // Return the temp_actors actor
                    return temp_actors[index - (data_count + 1)];
                }
                // Normal Actor
                if (!actors.ContainsKey(index))
                {
                    if (!Global.data_actors.Keys.Contains(index))
                        throw new IndexOutOfRangeException("Actor data with id " + index.ToString() + " does not exist");
                    actors[index] = new Game_Actor(Global.data_actors[index]);
                }
                return actors[index];
            }
        }

        public bool ContainsKey(int key)
        {
            int max_index = max_actor_id();
            int temp_index = key - (max_index + 1);
            return Global.data_actors.ContainsKey(key) || actors.ContainsKey(key) ||
                (temp_index >= 0 && temp_index < temp_actors.Count && temp_actors[temp_index] != null);
        }

        /// <summary>
        /// Checks if the actor with this id has been initialized from its data.
        /// Defaults to false if there is no actor data for this id.
        /// </summary>
        /// <param name="key">The id of the actor being checked.</param>
        public bool actor_loaded(int key)
        {
            // If there is no pre-defined data for this id, just return false
            if (!Global.data_actors.ContainsKey(key))
                return false;
            return actors.ContainsKey(key);
        }

        public Game_Actor new_actor()
        {
            if (temp_actors.Count > 0 && temp_actors[temp_actors.Count - 1] == null)
                return this[next_actor_id() - 1];
            return this[next_actor_id()];
        }

        public void temp_clear()
        {
            temp_actors.Clear();
        }
        public void temp_clear(int index)
        {
            int max_index = max_actor_id();
            index -= (max_index + 1);
            if (index >= 0 && index < temp_actors.Count)
                temp_actors[index] = null;
        }

        public void remove_actor(int i)
        {
            actors.Remove(i);
        }

        private int max_actor_id()
        {
            // We're switching over to using this system in the new version, but for now using the old version just so I can load old suspends
            return Constants.Actor.MAX_ACTOR_COUNT; //Yeti


#if DEBUG
            System.Diagnostics.Debug.Assert(Global.RUNNING_VERSION.older_than(0, 5, 1, 0));
#endif
            if (Global.data_actors.Keys.Count == 0)
                return -1;
            else
            {
                if (Max_Actor_Id == -1)
                    Max_Actor_Id = Global.data_actors.Keys.Max();
                return Max_Actor_Id;
            }
        }
#if DEBUG
        public int next_actor_id()
#else
        private int next_actor_id()
#endif
        {
            return max_actor_id() + temp_actors.Count + 1;
        }

        public bool is_temp_actor(Game_Actor actor)
        {
            return temp_actors.Contains(actor);
        }
        public bool is_temp_actor(int actorId)
        {
            return temp_actors.Any(x => x != null && x.id == actorId);
        }

        public void heal_battalion()
        {
            if (Global.battalion != null)
                foreach (int id in Global.battalion.actors)
                    this[id].recover_all();
        }

        public void heal_actors()
        {
            foreach (Game_Actor actor in actors.Values)
                if (!actor.is_dead())
                    actor.recover_all();
        }

        /// <summary>
        /// Returns the core map sprite name of an actor, of the form "MapClassF".
        /// </summary>
        internal string get_map_sprite_name(int actor_id)
        {
            if (actor_loaded(actor_id))
                return this[actor_id].map_sprite_name;
            else
                return get_map_sprite_name(Global.data_actors[actor_id]);
        }
        internal static string get_map_sprite_name(TactileLibrary.Data_Actor actor)
        {
            return get_map_sprite_name(actor.ClassId, actor.Gender);
        }
        internal static string get_map_sprite_name(int classId, int gender)
        {
            return get_map_sprite_name(Global.data_classes[classId].Name, classId, gender);
        }
        internal static string get_map_sprite_name(string class_name, int class_id, int gender)
        {
            if (TactileBattlerImage.Single_Gender_Map_Sprite.Contains(class_id))
                gender = (gender / 2) * 2;
            return class_map_sprite_name(class_name, gender);
        }

        public static string class_map_sprite_name(
            string className,
            int gender)
        {
            string name = "Map" + className;
            string result = name + (gender % 2 == 0 ? "M" : "F");

            if (gender % 2 != 0 &&
                !Global.content_exists(@"Graphics/Characters/" + result))
            {
                gender = (gender / 2) * 2;
                return class_map_sprite_name(className, gender);
            }

            return result;
        }

        /// <summary>
        /// Returns a full map sprite name, corrected to an existing value and with a postfix for moving.
        /// </summary>
        public static string map_sprite_name(
            string name,
            bool moving)
        {
            name += moving ? "_move" : "";

            if (!Global.content_exists(@"Graphics/Characters/" + name))
            {
                name = Scene_Map.DEFAULT_MAP_SPRITE + (moving ? "_move" : "");
                //name = Scene_Map.DEFAULT_MAP_SPRITE + (name.Substring(name.Length - 5, 5) == "_move" ? "_move" : ""); //Debug
            }
            return name;
        }
        public static string map_sprite_name(
            int classId,
            int gender,
            bool moving)
        {
            string name = class_map_sprite_name(Global.data_classes[classId].Name, gender);
            return map_sprite_name(name, moving);
        }

#if DEBUG
        public int get_unit_from_actor(int actor_id, bool noUnitHandled = false)
#else
        public int get_unit_from_actor(int actor_id)
#endif
        {
            if (Global.game_map.actor_defeated(actor_id))
                return 0;
            if (Global.game_map.units.Any(x => x.Value.actor.id == actor_id))
                return Global.game_map.units.First(x => x.Value.actor.id == actor_id).Key;
            /*foreach (KeyValuePair<int, Game_Unit> pair in Global.game_map.units) //Debug
                if (pair.Value.actor.id == actor_id)
                    return pair.Key;*/
#if DEBUG
            if (!Global.game_system.preparations)
                if (!noUnitHandled)
                    throw new KeyNotFoundException("No unit has the actor with id " + actor_id.ToString());
#endif
            return -1;
        }

        internal int get_first_generic_from_name(string actor_name)
        {
#if DEBUG
            var generics = Global.game_map.units.Where(x => x.Value.actor.name_full == actor_name).ToList();
#else
            var generics = Global.game_map.units.Where(x => x.Value.actor.name_full == actor_name);
#endif
            if (!generics.Any())
                return 0;
            return generics.Min(x => x.Key);
        }

        public void fix_actor(int index, Game_Actor actor)
        {
            if (!Global.data_actors.ContainsKey(index))
                return;
            actors[index] = actor;
        }

        public void copy_actors_to(Game_Actors target, Battalion battalion = null)
        {
            foreach (var pair in battalion == null ? actors : actors.Where(x => battalion.actors.Contains(x.Key)))
            {
                target.actors[pair.Key] = pair.Value;
            }
        }
    }
}
