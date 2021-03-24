using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using TactileLibrary;

namespace Tactile
{
    partial class Game_Map
    {
        protected int Last_Added_Unit_Id = 0;
        protected List<int> Preparations_Unit_Team = null;

        #region Accessors
        public List<int> preparations_unit_team { get { return Preparations_Unit_Team; } }
        #endregion

        internal Game_Unit last_added_unit
        {
            get
            {
                if (!Objects.unit_exists(Last_Added_Unit_Id))
                    return null;
                return this.units[Last_Added_Unit_Id];
            }
        }

        public void remove_unit(int id)
        {
            if (Objects.unit_exists(id))
            {
                //Last_Added_Unit something

                Objects.remove_unit(id);
                Global.scene.remove_map_sprite(id);

                Global.game_state.remove_ai_unit(id);
                foreach (List<int> team in Teams)
                    team.Remove(id);
                if (Global.game_system.Selected_Unit_Id == id)
                    Global.game_system.Selected_Unit_Id = -1;
                //Delete saved event
            }
        }
        public void completely_remove_unit(int id)
        {
            if (Last_Added_Unit_Id == id && Objects.unit_exists(Last_Added_Unit_Id))
            {
                remove_unit(id);
                Last_Added_Unit_Id--;
            }
            else
                remove_unit(id);
        }

        #region Add Units
        protected void add_unit(Vector2 loc, Data_Unit data)
        {
            add_unit(loc, data, data.identifier);
        }
        protected void add_unit(Vector2 loc, Data_Unit data, string identifier, int team = -1)
        {
            int count = this.units.Count;

            if (string.IsNullOrEmpty(identifier))
                identifier = data.identifier;
            switch (data.type)
            {
                case "character":
                    add_character_unit(loc, team, identifier, data.data.Split('\n'));
                    break;
                case "generic":
                    add_generic_unit(loc, team, identifier, data.data.Split('\n'));
                    break;
                case "temporary":
                    add_temporary_unit(loc, team, identifier, data.data.Split('\n'));
                    break;
            }
            if (this.units.Count > count)
            {
                var unit = this.last_added_unit;
                // If a dead actor, give them 1 hp so they can limp around unless they're a dead PC
                if (unit.actor.is_out_of_lives() && unit.is_player_team)
                    unit.hp = 0;
                else
                    unit.actor.hp = Math.Max(1, unit.actor.hp);

                unit.actor.setup_items();
                unit.actor.staff_fix();
            }
        }

        protected void add_character_unit(Vector2 loc, int team, string identifier, string[] data_ary)
        {
            int id = Convert.ToInt32(data_ary[0].Split('|')[0]);
            if (team <= 0)
                team = Convert.ToInt32(data_ary[1].Split('|')[0]);
            int priority = Convert.ToInt32(data_ary[2].Split('|')[0]);
            int mission = Convert.ToInt32(data_ary[3].Split('|')[0]);
            new_unit_id();
            Game_Unit new_unit = Objects.add_unit(
                new Game_Unit(Last_Added_Unit_Id, loc, team, priority, id), identifier, mission);
            // If a dead actor, and not out of lives or anything, give them 1 hp so they can limp around
            if (new_unit.is_dead && !new_unit.actor.is_out_of_lives())
                new_unit.actor.hp = 1;
        }

        protected void add_generic_unit(Vector2 loc, int team, string identifier, string[] data_ary)
        {
            Game_Actor actor = Global.game_actors.new_actor();
            actor.name = data_ary[0].Split('|')[0];
            actor.class_id = Convert.ToInt32(data_ary[1].Split('|')[0]); //Debug
            actor.gender = Convert.ToInt32(data_ary[2].Split('|')[0]);
            actor.level_down();
            actor.exp = 0;
            
            int level = Convert.ToInt32(data_ary[3].Split('|')[0]);
            int exp = Convert.ToInt32(data_ary[4].Split('|')[0]);
            int prepromote_levels = Convert.ToInt32(data_ary[6].Split('|')[0]);
            int build = Convert.ToInt32(data_ary[7].Split('|')[0]);
            int con = Convert.ToInt32(data_ary[8].Split('|')[0]);

            int numItems;
            var items = ReadUnitDataItems(11, data_ary, out numItems);
            actor.set_items(items);

            int index_after_items = 11 + numItems;
            string[] wexp_ary = data_ary[index_after_items + 0]
                .Split('|')[0].Split(new string[] { ", " }, StringSplitOptions.None);
            int[] wexp = Enumerable.Range(0, Global.weapon_types.Count - 1)
                .Select(x => x < wexp_ary.Length ? Convert.ToInt32(wexp_ary[x]) : 0)
                .ToArray();

            actor.setup_generic(
                Convert.ToInt32(data_ary[1].Split('|')[0]), level, exp,
                prepromote_levels, (Generic_Builds)build, con, wexp: wexp);
            
            if (team <= 0)
                team = Convert.ToInt32(data_ary[5].Split('|')[0]);
            int priority = Convert.ToInt32(data_ary[9].Split('|')[0]);
            int mission = Convert.ToInt32(data_ary[10].Split('|')[0]);
            new_unit_id();
            var new_unit = Objects.add_unit(
                new Game_Unit(Last_Added_Unit_Id, loc, team, priority, actor.id), identifier, mission);
        }

        protected void add_temporary_unit(Vector2 loc, int team, string identifier, string[] data_ary)
        {
            Game_Actor actor = Global.game_actors.new_actor();
            actor.name = data_ary[0].Split('|')[0];
            actor.class_id = Convert.ToInt32(data_ary[1].Split('|')[0]);
            actor.gender = Convert.ToInt32(data_ary[2].Split('|')[0]);
            actor.level = Convert.ToInt32(data_ary[3].Split('|')[0]);
            actor.exp = Convert.ToInt32(data_ary[4].Split('|')[0]);
            //actor.maxhp = Convert.ToInt32(data_ary[6].Split('|')[0]);
            actor.hp = Convert.ToInt32(data_ary[7].Split('|')[0]);
            /*actor.stats[0] = Convert.ToInt32(data_ary[8].Split('|')[0]);
            actor.stats[1] = Convert.ToInt32(data_ary[9].Split('|')[0]);
            actor.stats[2] = Convert.ToInt32(data_ary[10].Split('|')[0]);
            actor.stats[3] = Convert.ToInt32(data_ary[11].Split('|')[0]);
            actor.stats[4] = Convert.ToInt32(data_ary[12].Split('|')[0]);
            actor.stats[5] = Convert.ToInt32(data_ary[13].Split('|')[0]);
            actor.stats[6] = Convert.ToInt32(data_ary[14].Split('|')[0]);*/

            int numItems;
            var items = ReadUnitDataItems(17, data_ary, out numItems);
            actor.set_items(items);

            int index_after_items = 17 + numItems;
            string[] wexp_ary = data_ary[index_after_items]
                .Split('|')[0].Split(new string[] { ", " }, StringSplitOptions.None);
            for (int i = 0; i < Global.weapon_types.Count - 1; i++)
                if (i < wexp_ary.Length)
                    actor.wexp_set(Global.weapon_types[i + 1], Convert.ToInt32(wexp_ary[i]), false);
            actor.clear_wlvl_up();
            
            if (team <= 0)
                team = Convert.ToInt32(data_ary[5].Split('|')[0]);
            int priority = Convert.ToInt32(data_ary[15].Split('|')[0]);
            int mission = Convert.ToInt32(data_ary[16].Split('|')[0]);
            new_unit_id();
            var new_unit = Objects.add_unit(
                new Game_Unit(Last_Added_Unit_Id, loc, team, priority, actor.id), identifier, mission);
        }

        internal static List<Item_Data> ReadUnitDataItems(
            int firstItemIndex,
            string[] data_ary,
            out int numItems)
        {
            numItems = Global.ActorConfig.NumItems;

            if (firstItemIndex + numItems != data_ary.Length - 1)
            {
                numItems = 0;
                for (int i = 1; firstItemIndex + i < data_ary.Length; i++)
                {
                    var match = Regex.Match(
                        data_ary[firstItemIndex + i - 1],
                        string.Format("\\|Item {0}$", i));
                    if (!match.Success)
                        break;

                    numItems = i;
                }
                //@Debug: not actually a problem, but worth noting
                //if (firstItemIndex + numItems != data_ary.Length - 1)
                //    throw new IndexOutOfRangeException("Unit data had the wrong number of items");
            }

            // If units were created with NUM_ITEMS = 6,
            // and then NUM_ITEMS was changed to 4 or 5, things would break
            //@Yeti: Come up with a longterm solution
            //@Debug: should be solved for now
            List<Item_Data> items = new List<Item_Data>();
            for (int item_index = 0; item_index < numItems; item_index++)
            {
                int dataIndex = firstItemIndex + item_index;
                string[] item_ary = data_ary[dataIndex].Split('|')[0].Split(new string[] { ", " }, StringSplitOptions.None);
                items.Add(new Item_Data(Convert.ToInt32(item_ary[0]),
                    Convert.ToInt32(item_ary[1]), Convert.ToInt32(item_ary[2])));
            }

            items = items.Take(Global.ActorConfig.NumItems).ToList();

            return items;
        }

        public bool add_actor_unit(int team, Vector2 loc, int actor_id, string identifier)
        {
#if DEBUG
            if (Global.scene.is_map_scene && Global.scene.scene_type != "Scene_Map_Unit_Editor" && !Global.data_actors.ContainsKey(actor_id))
                Print.message("Adding an actor unit with actor id " + actor_id.ToString() + "\nThis actor id has no data defined.\nAre you sure this id is correct?");
#endif
            new_unit_id();
            var new_unit = Objects.add_unit(
                new Game_Unit(Last_Added_Unit_Id, loc, team, 0, actor_id), identifier);

            // If a dead actor, give them 1 hp so they can limp around unless they're a dead PC
            if (new_unit.actor.is_out_of_lives() && new_unit.is_player_team)
                new_unit.hp = 0;
            else
                new_unit.actor.hp = Math.Max(1, new_unit.actor.hp);
            return true;
        }
        public bool replace_actor_unit(int team, Vector2 loc, int actor_id, int old_unit_id)
        {
            if (Last_Added_Unit_Id == old_unit_id && Objects.unit_exists(Last_Added_Unit_Id))
            {
                completely_remove_unit(Last_Added_Unit_Id);
            }
            return add_actor_unit(team, loc, actor_id, "");
        }

        public bool add_undeployed_battalion_unit(int team, Vector2 loc, int index, string identifier)
        {
            int id = Global.battalion.undeployed_actor(index);
            if (id == -1)
                return false;
            return add_actor_unit(team, loc, id, identifier);
        }

        public bool add_temp_unit(int team, Vector2 loc, int class_id, int gender, string identifier)
        {
            new_unit_id();
            var new_unit = Objects.add_unit(
                new Game_Unit(Last_Added_Unit_Id, loc, team, 0), identifier);
            new_unit.actor.class_id = class_id;
            new_unit.actor.gender = gender;
            new_unit.refresh_sprite();
            return true;
        }
        public bool add_gladiator(int team, Vector2 loc, int class_id, int gender, string identifier)
        {
            bool update_victory_theme = Global.game_state.Update_Victory_Theme;
            bool result = add_temp_unit(team, loc, class_id, gender, identifier);
            if (result)
                last_added_unit.gladiator = true;
            Global.game_state.Update_Victory_Theme = update_victory_theme;
            return result;
        }

        public bool add_reinforcement_unit(int team, Vector2 loc, int index, string identifier)
        {
            if (index < 0 || index >= Unit_Data.Reinforcements.Count)
                return false;
            add_unit(loc, Unit_Data.Reinforcements[index], identifier, team);
            return true;
        }
        #endregion

        #region Add Destroyables
        public void add_destroyable_object(Vector2 loc, int hp, string event_name)
        {
            if (is_off_map(loc))
                return;
            new_unit_id();
            Objects.add_destroyable_object(new Destroyable_Object(Last_Added_Unit_Id, loc, hp, event_name));
            Destroyable_Locations[(int)loc.X, (int)loc.Y] = Last_Added_Unit_Id + 1;
        }

        internal void remove_destroyable(int id, bool refresh_move_ranges = false) //private //Yeti
        {
            if (Objects.destroyable_exists(id))
            {
                Destroyable_Object destroyable = Objects.destroyable(id);
                Destroyable_Locations[(int)destroyable.loc.X, (int)destroyable.loc.Y] = 0;
                Objects.remove_destroyable(id);

                if (refresh_move_ranges)
                    Refresh_All_Ranges = true;
            }
        }
        #endregion

        #region Add Siege Engines
        public void add_siege_engine(Vector2 loc, Item_Data item)
        {
            if (is_off_map(loc))
                return;
            new_unit_id();
            Siege_Engine siege = new Siege_Engine(Last_Added_Unit_Id, loc, item);
            Objects.add_siege_engine(siege);
            Siege_Locations[(int)loc.X, (int)loc.Y] = Last_Added_Unit_Id + 1;
            if (siege.Unit != null)
                siege.Unit.refresh_sprite();
            clear_updated_attack_ranges();
            clear_updated_staff_ranges();
        }

        public void remove_siege_engine(int id)
        {
            if (this.siege_engines.ContainsKey(id))
            {
                Siege_Engine siege = this.siege_engines[id];
                Game_Unit unit = siege.Unit;
                Siege_Locations[(int)siege.loc.X, (int)siege.loc.Y] = 0;
                Objects.remove_siege_engine(id);
                if (get_scene_map() != null)
                    get_scene_map().remove_map_sprite(id);
                if (unit != null)
                    unit.refresh_sprite();
            }
        }
        #endregion

        #region Add Light Runes
        public void add_light_rune(Vector2 loc, int team)
        {
            if (is_off_map(loc))
                return;
            new_unit_id();
            Objects.add_light_rune(new LightRune(Last_Added_Unit_Id, loc, team));
            clear_updated_move_ranges();
        }
        public void add_permanent_light_rune(Vector2 loc, int team)
        {
            if (is_off_map(loc))
                return;
            new_unit_id();
            Objects.add_light_rune(
                new LightRune(Last_Added_Unit_Id, loc, team, false));
            clear_updated_move_ranges();
        }

        internal void remove_light_rune(int id)
        {
            if (Objects.light_rune_exists(id))
            {
                Objects.remove_light_rune(id);
                if (get_scene_map() != null)
                    get_scene_map().remove_map_sprite(id);

                Refresh_All_Ranges = true;
                clear_updated_move_ranges();
            }
        }
        #endregion

        protected void new_unit_id()
        {
            Last_Added_Unit_Id++;
            while (Objects.id_in_use(Last_Added_Unit_Id))
                Last_Added_Unit_Id++;
            if (Global.Audio.IsTrackPlaying("MapBgm"))
                Global.game_state.Update_Victory_Theme = true;
        }

        public void init_preparations_unit_team()
        {
            clear_preparations_unit_team();
            Preparations_Unit_Team = new List<int>();

            for (int i = 0; i < Global.battalion.actors.Count; i++)
            {
                add_actor_unit(Global.game_state.team_turn, Config.OFF_MAP, Global.battalion.actors[i], "");
                Preparations_Unit_Team.Add(Last_Added_Unit_Id);
            }
        }

        public void clear_preparations_unit_team()
        {
            if (Preparations_Unit_Team != null)
                for (int i = 0; i < Preparations_Unit_Team.Count; i++)
                    remove_unit(Preparations_Unit_Team[i]);
            Preparations_Unit_Team = null;
        }

        public int find_reinforcement_index(string identifier)
        {
            // Don't allow identifiers that are just numbers
            int num_test;
            if (int.TryParse(identifier, System.Globalization.NumberStyles.Any,
                    System.Globalization.NumberFormatInfo.InvariantInfo, out num_test))
                return Convert.ToInt32(identifier);

            int index = Unit_Data.Reinforcements.FindIndex(x => x.identifier == identifier);
            return index;
        }
    }
}
