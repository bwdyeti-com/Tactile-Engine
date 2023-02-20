using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using ArrayExtension;
using DictionaryExtension;
using HashSetExtension;
using ListExtension;
using Vector2Extension;
using TactileListExtension;
using TactileVersionExtension;

namespace Tactile
{
    internal partial class Game_System
    {
        public bool Instant_Move;
        protected int Character_Anim_Count = 0;
        public int Selected_Unit_Id;
        //protected List<int> Rng;
        private Rng Rand;
        protected List<int> Preset_Rng;
        protected List<int> Saved_Rns;
        protected bool Saving_Rns;
        protected bool Preparations, Home_Base;
        public Canto_Records Menu_Canto;
        public int Battler_1_Id;
        public int Battler_2_Id;
        public List<int> Aoe_Targets;
        public int Staff_User_Id;
        public int Staff_Target_Id;
        public Vector2 Staff_Target_Loc;
        public int Rescuer_Id;
        public int Rescuee_Id;
        public int Item_User, Item_Used, Item_Inventory_Target, ItemPromotionId;
        public Vector2 ItemTargetLoc;
        public int Visitor_Id;
        public Vector2 Visit_Loc;
        public int Shopper_Id;
        public Vector2 Shop_Loc;
        public bool SecretShop;
        public bool In_Arena;
        public int Wager;
        public int Arena_Distance;
        public int Arena_Round;
        public bool Arena_Retreat;
        public int Stolen_Item;
        public int Dance_Item;
        public int Discarder_Id;
        public int SupportGainId;
        public List<int> SupportGainTargets;
        internal Constants.Animation_Modes Battle_Mode;
        public int Status_Page;
        public int Unit_Page;
        public int Unit_Sort;
        public bool Unit_Sort_Up;
        public int DataPage;
        public int Preparations_Actor_Id;
        public int Supply_Item_Type;
        public string Objective_Text, Victory_Text, Loss_Text;
        public int[] Objective_Mode;
        private HashSet<int> Loss_On_Death;
        private List<int> Ally_Loss_On_Death;
        public Difficulty_Modes Difficulty_Mode;
        public Mode_Styles Style;
        protected List<string> Previous_Chapters;
        protected string Chapter_Id;
        protected Dictionary<string, string> PreviousChapterIds = new Dictionary<string,string>();
        protected PastRankings Rankings;
        protected int Total_Play_Time, Chapter_Play_Time;
        protected DateTime Chapter_Start_Time, GameplayStartTime;
        protected int Deployed_Unit_Count, Deployed_Unit_Avg_Level;
        protected int Chapter_Turn, Chapter_Exp_Gain, Chapter_Damage_Taken, Chapter_Deaths, Chapter_Completion;
        protected string Home_Base_Background = "";

        // Clean up the handling on these later, but I had to do something to move them out of Event_Processor static //Yeti
        internal Event_Variable_Data<bool> SWITCHES = new Event_Variable_Data<bool>(Config.EVENT_DATA_LENGTH);
        internal Event_Variable_Data<int> VARIABLES = new Event_Variable_Data<int>(Config.EVENT_DATA_LENGTH);

        protected int Unit_Blink_Timer = 0;
        public string New_Chapter_Id = "", WorldmapChapterId;
        public string[] Chapter_Save_Progression_Keys;
        public bool Preparation_Events_Ready;
        public int Class_Changer = -1;
        public int Class_Change_To = -1;
        protected bool Victory;
        protected bool Failure;
        protected List<Event_Processor> Events = new List<Event_Processor>();
        public readonly int Unit_Highlight_Anim_Time, Unit_Moving_Anim_Time;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Instant_Move);
            writer.Write(Character_Anim_Count);
            writer.Write(Selected_Unit_Id);
            //Rng.write(writer);
            Rand.write(writer);
            Preset_Rng.write(writer);
            writer.Write(Preparations);
            writer.Write(Home_Base);
            writer.Write((int)Menu_Canto);
            writer.Write(Battler_1_Id);
            writer.Write(Battler_2_Id);
            Aoe_Targets.write(writer);
            writer.Write(Staff_User_Id);
            writer.Write(Staff_Target_Id);
            Staff_Target_Loc.write(writer);
            writer.Write(Rescuer_Id);
            writer.Write(Rescuee_Id);
            writer.Write(Item_User);
            writer.Write(Item_Used);
            writer.Write(Item_Inventory_Target);
            writer.Write(ItemPromotionId);
            ItemTargetLoc.write(writer);
            writer.Write(Shopper_Id);
            Shop_Loc.write(writer);
            writer.Write(SecretShop);
            writer.Write(In_Arena);
            writer.Write(Wager);
            writer.Write(Arena_Distance);
            writer.Write(Arena_Round);
            writer.Write(Stolen_Item);
            writer.Write(Dance_Item);
            writer.Write(Discarder_Id);
            writer.Write(SupportGainId);
            SupportGainTargets.write(writer);

            writer.Write((int)Battle_Mode);
            writer.Write(Objective_Text);
            writer.Write(Victory_Text);
            writer.Write(Loss_Text);
            Objective_Mode.write(writer);
            Loss_On_Death.write(writer);
            Ally_Loss_On_Death.write(writer);
            writer.Write((int)Difficulty_Mode);
            writer.Write((int)Style);
            Previous_Chapters.write(writer);
            writer.Write(Chapter_Id);
            PreviousChapterIds.write(writer);
            Rankings.write(writer);
            writer.Write(Total_Play_Time);
            writer.Write(Chapter_Play_Time);
            writer.Write(Chapter_Start_Time.ToBinary());
            writer.Write(GameplayStartTime.ToBinary());
            writer.Write(Deployed_Unit_Count);
            writer.Write(Deployed_Unit_Avg_Level);
            writer.Write(Chapter_Turn);
            writer.Write(Chapter_Exp_Gain);
            writer.Write(Chapter_Damage_Taken);
            writer.Write(Chapter_Deaths);
            writer.Write(Chapter_Completion);
            writer.Write(Home_Base_Background);

            SWITCHES.write(writer);
            VARIABLES.write(writer);
            //Event_Processor.write_data(writer); //Debug
        }

        public void write_events(BinaryWriter writer)
        {
            Events.write(writer);
        }

        public void read(BinaryReader reader)
        {
            read(reader, Global.LOADED_VERSION);
        }
        public void read(BinaryReader reader, Version loadedVersion)
        {
            Instant_Move = reader.ReadBoolean();
            Character_Anim_Count = reader.ReadInt32();
            Selected_Unit_Id = reader.ReadInt32();
            if (loadedVersion.older_than(0, 4, 3, 2))
            {
                List<int> rand = new List<int>();
                rand.read(reader);
                reset_rng();
            }
            else
                Rand = Rng.read(reader, loadedVersion);
            //reset_rng(); //Debug
            Preset_Rng.read(reader);
            Preparations = reader.ReadBoolean();
            if (!loadedVersion.older_than(0, 4, 3, 6))
                Home_Base = reader.ReadBoolean();
            Menu_Canto = (Canto_Records)reader.ReadInt32();
            Battler_1_Id = reader.ReadInt32();
            Battler_2_Id = reader.ReadInt32();
            Aoe_Targets.read(reader);
            Staff_User_Id = reader.ReadInt32();
            Staff_Target_Id = reader.ReadInt32();
            Staff_Target_Loc = Staff_Target_Loc.read(reader);
            Rescuer_Id = reader.ReadInt32();
            Rescuee_Id = reader.ReadInt32();
            Item_User = reader.ReadInt32();
            Item_Used = reader.ReadInt32();
            if (!loadedVersion.older_than(0, 4, 6, 1))
                Item_Inventory_Target = reader.ReadInt32();
            if (!loadedVersion.older_than(0, 6, 6, 0))
                ItemPromotionId = reader.ReadInt32();
            if (!loadedVersion.older_than(0, 6, 4, 1))
                ItemTargetLoc = ItemTargetLoc.read(reader);
            Shopper_Id = reader.ReadInt32();
            Shop_Loc = Shop_Loc.read(reader);
            if (!loadedVersion.older_than(0, 5, 0, 5))
                SecretShop = reader.ReadBoolean();
            In_Arena = reader.ReadBoolean();
            Wager = reader.ReadInt32();
            Arena_Distance = reader.ReadInt32();
            Arena_Round = reader.ReadInt32();
            Stolen_Item = reader.ReadInt32();
            Dance_Item = reader.ReadInt32();
            Discarder_Id = reader.ReadInt32();
            if (!loadedVersion.older_than(0, 5, 5, 4))
            {
                SupportGainId = reader.ReadInt32();
                SupportGainTargets.read(reader);
            }

            Battle_Mode = (Constants.Animation_Modes)reader.ReadInt32();
            Objective_Text = reader.ReadString();
            Victory_Text = reader.ReadString();
            Loss_Text = reader.ReadString();
            Objective_Mode = Objective_Mode.read(reader);
            Loss_On_Death.read(reader);
            Ally_Loss_On_Death.read(reader);
            if (loadedVersion.older_than(0, 4, 2, 0))
                Difficulty_Mode = reader.ReadBoolean() ? Difficulty_Modes.Hard: Difficulty_Modes.Normal;
            else
                Difficulty_Mode = (Difficulty_Modes)reader.ReadInt32();
            Style = (Mode_Styles)reader.ReadInt32();
            if (!loadedVersion.older_than(0, 4, 0, 4))
                Previous_Chapters.read(reader);
            if (!loadedVersion.older_than(0, 4, 4, 0))
            {
                Chapter_Id = reader.ReadString();
                if (!loadedVersion.older_than(0, 5, 6, 3))
                    PreviousChapterIds.read(reader);
                else
                {
                    PreviousChapterIds.Clear();
                    string previous_chapter = reader.ReadString();
                    if (!string.IsNullOrEmpty(previous_chapter))
                    {
                        if (!Global.data_chapters.ContainsKey(Chapter_Id))
                            throw new KeyNotFoundException(string.Format("Problem loading save, cannot find chapter {0}", Chapter_Id));
                        else if (!Global.data_chapters.ContainsKey(previous_chapter))
                            throw new KeyNotFoundException(string.Format("Problem loading save, cannot find chapter {0}", previous_chapter));
                        else
                        {
                            string progression_id;
                            if (!Global.data_chapters[Chapter_Id].Prior_Chapters
                                    .Intersect(Global.data_chapters[previous_chapter].Progression_Ids)
                                    .Any())
                            {
#if DEBUG
                                //throw new KeyNotFoundException(string.Format(
                                //    "Chapter {0}'s prior chapters do not match chapter {1}'s progression ids",
                                //    Chapter_Id, previous_chapter));
#endif

                                // Incorrect but eh @Debug
                                progression_id = Global.data_chapters[Chapter_Id].Prior_Chapters.First();
                            }
                            else
                            {
                                progression_id = Global.data_chapters[Chapter_Id].Prior_Chapters
                                    .Intersect(Global.data_chapters[previous_chapter].Progression_Ids)
                                    .First();
                            }
                            PreviousChapterIds.Add(progression_id, previous_chapter);
                        }
                    }
                }
            }
            if (!loadedVersion.older_than(0, 6, 7, 0))
                Rankings = PastRankings.read(reader, Difficulty_Mode);
            else
                Rankings = new PastRankings();
            Total_Play_Time = reader.ReadInt32();
            Chapter_Play_Time = reader.ReadInt32();
            if (!loadedVersion.older_than(0, 4, 6, 3))
            {
                Chapter_Start_Time = DateTime.FromBinary(reader.ReadInt64());
                if (!loadedVersion.older_than(0, 6, 3, 1))
                    GameplayStartTime = DateTime.FromBinary(reader.ReadInt64());
                else
                    GameplayStartTime = Chapter_Start_Time;
                Deployed_Unit_Count = reader.ReadInt32();
                Deployed_Unit_Avg_Level = reader.ReadInt32();
            }
            Chapter_Turn = reader.ReadInt32();
            Chapter_Exp_Gain = reader.ReadInt32();
            Chapter_Damage_Taken = reader.ReadInt32();
            Chapter_Deaths = reader.ReadInt32();
            Chapter_Completion = reader.ReadInt32();
            if (!loadedVersion.older_than(0, 4, 4, 3))
                Home_Base_Background = reader.ReadString();

            SWITCHES = Event_Variable_Data<bool>.read(reader, Config.EVENT_DATA_LENGTH);
            VARIABLES = Event_Variable_Data<int>.read(reader, Config.EVENT_DATA_LENGTH);
            //Event_Processor.read_data(reader); //Debug
        }

        public void read_events(BinaryReader reader)
        {
            Events.read(reader);
        }

        internal void get_event_data(Event_Variable_Data<bool> switches, Event_Variable_Data<int> variables)
        {
            Event_Variable_Data<bool>.copy(SWITCHES, switches);
            Event_Variable_Data<int>.copy(VARIABLES, variables);
        }

        internal void set_event_data(
            Event_Variable_Data<bool> switches,
            Event_Variable_Data<int> variables,
            bool overwriteAlreadySet = true)
        {
            Event_Variable_Data<bool>.copy(switches, SWITCHES, overwriteAlreadySet);
            Event_Variable_Data<int>.copy(variables, VARIABLES, overwriteAlreadySet);
        }
        #endregion

        #region Accessors
        public int unit_anim_count { get { return Character_Anim_Count; } }
        public int unit_anim_idle_frame
        {
            get
            {
                int anim_count = Character_Anim_Count;
                int index = 0;
                for (int i = 0; i < Config.CHARACTER_IDLE_ANIM_TIMES.Length; i++)
                {
                    if (anim_count < Config.CHARACTER_IDLE_ANIM_TIMES[i])
                    {
                        index = i;
                        break;
                    }
                    else
                        anim_count -= Config.CHARACTER_IDLE_ANIM_TIMES[i];
                }
                return Config.CHARACTER_IDLE_ANIM_FRAMES[index];
            }
        }

        public bool preparations
        {
            get { return Preparations; }
            set { Preparations = value; }
        }
        public bool home_base
        {
            get { return Home_Base; }
            set { Home_Base = value; }
        }

        /// <summary>
        /// Actor ids of characters that the player must keep alive this chapter, if the characters are on the player team.
        /// </summary>
        private List<int> ally_loss_on_death
        {
            get
            {
                return Ally_Loss_On_Death.Union(Constants.Gameplay.LOSS_ON_DEATH).ToList();
            }
        }

        public bool hard_mode { get { return Difficulty_Mode == Difficulty_Modes.Hard; } }

        public string difficulty_append
        {
            get
            {
                return Difficulty_Mode == Difficulty_Modes.Normal ?
                    "" : Constants.Difficulty.DIFFICULTY_SAVE_APPEND[Difficulty_Mode].ToString();
            }
        }

        public List<string> previous_chapters
        {
            get { return Previous_Chapters; }
            set { Previous_Chapters = value; }
        }
        public string chapter_id { get { return Chapter_Id; } }
        internal Dictionary<string, string> previous_chapter_id { get { return PreviousChapterIds; } }
        internal PastRankings rankings { get { return Rankings; } }

        public int total_play_time { get { return Total_Play_Time; } }
        public int chapter_play_time { get { return Chapter_Play_Time; } }

        public DateTime chapter_start_time { get { return Chapter_Start_Time; } }
        public DateTime gameplay_start_time { get { return GameplayStartTime; } }
        public int deployed_unit_count { get { return Deployed_Unit_Count; } }
        public int deployed_unit_avg_level { get { return Deployed_Unit_Avg_Level; } }

        public int chapter_turn
        {
            get { return Chapter_Turn; }
            set { Chapter_Turn = value; }
        }
        public int chapter_exp_gain
        {
            get { return Chapter_Exp_Gain; }
            set { Chapter_Exp_Gain = value; }
        }
        public int chapter_damage_taken
        {
            get { return Chapter_Damage_Taken; }
            set
            {
                Chapter_Damage_Taken = value;
#if DEBUG
                Console.WriteLine(string.Format("Chapter damage taken: {0}", Chapter_Damage_Taken)); //Yeti
#endif
            }
        }
        public int chapter_deaths
        {
            get { return Chapter_Deaths; }
            set { Chapter_Deaths = value; }
        }
        public int chapter_completion
        {
            get { return Chapter_Completion; }
            set { Chapter_Completion = value; }
        }

        public string home_base_background
        {
            get { return Home_Base_Background; }
            set { Home_Base_Background = value; }
        }

        public bool unit_blink { get { return Unit_Blink_Timer < Config.UNIT_BLINK_TIME; } }

        public string active_event_name { get { return Events.Count > 0 ? Events[0].name : "-----"; } }
        #endregion

        public Game_System()
        {
            Unit_Highlight_Anim_Time = 0;
            foreach (int time in Config.CHARACTER_HIGHLIGHT_ANIM_TIMES)
                Unit_Highlight_Anim_Time += time;
            Unit_Moving_Anim_Time = 0;
            foreach (int time in Config.CHARACTER_MOVING_ANIM_TIMES)
                Unit_Moving_Anim_Time += time;
            reset();
        }

        public Game_System copy()
        {
            // Hey maybe make this not awful and actually copy the values //Yeti
            Game_System result = new Game_System();
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                write(writer);
                writer.Flush();

                ms.Position = 0;
                using (BinaryReader reader = new BinaryReader(ms))
                    result.read(reader, Global.RUNNING_VERSION);
            }
            return result;
        }

        public void reset()
        {
            Instant_Move = false;
            Selected_Unit_Id = -1;
            //Rng = new List<int>();
            reset_rng();
            Preset_Rng = new List<int>();
            Saved_Rns = new List<int>();
            Saving_Rns = false;
            Preparations = false;
            Home_Base = false;
            Menu_Canto = Canto_Records.None;
            Battler_1_Id = -1;
            Battler_2_Id = -1;
            Aoe_Targets = new List<int>();
            Staff_User_Id = -1;
            Staff_Target_Id = -1;
            Staff_Target_Loc = new Vector2(-1, -1);
            Rescuer_Id = -1;
            Rescuee_Id = -1;
            Item_User = -1;
            Item_Used = -1;
            Item_Inventory_Target = -1;
            ItemPromotionId = -1;
            ItemTargetLoc = new Vector2(-1, -1);
            Visitor_Id = -1;
            Visit_Loc = new Vector2(-1, -1);
            Shopper_Id = -1;
            Shop_Loc = new Vector2(-1, -1);
            SecretShop = false;
            In_Arena = false;
            Wager = -1;
            Arena_Distance = -1;
            Arena_Round = -1;
            Arena_Retreat = false;
            Stolen_Item = -1;
            Discarder_Id = -1;
            SupportGainId = -1;
            SupportGainTargets = new List<int>();

            Battle_Mode = Constants.Animation_Modes.Map;
            Status_Page = 0;
            Unit_Page = 0;
            Unit_Sort = 0;
            Unit_Sort_Up = true;
            DataPage = 0;
            Supply_Item_Type = 0;
            Objective_Text = "";
            Victory_Text = "";
            Loss_Text = "";
            Objective_Mode = new int[] { 0, 0 };
            Loss_On_Death = new HashSet<int>();
            Ally_Loss_On_Death = new List<int>();
            Difficulty_Mode = Difficulty_Modes.Normal;
            Style = Global.save_file == null ? Mode_Styles.Standard : Global.save_file.Style;
            Previous_Chapters = new List<string>();
            Rankings = new PastRankings();
            Total_Play_Time = 0;
            Chapter_Save_Progression_Keys = new string[0];
            Victory = false;
            Failure = false;
            clear_events();
        }

        internal void reset_event_variables()
        {
            SWITCHES = new Event_Variable_Data<bool>(Config.EVENT_DATA_LENGTH);
            VARIABLES = new Event_Variable_Data<int>(Config.EVENT_DATA_LENGTH);
        }

        #region RNG
        public int get_rng()
        {
#if DEBUG
            rng_activity();
#endif

            int result;
            if (Preset_Rng.Count > 0)
            {
                result = Preset_Rng.shift();
            }
            else
            {
                result = Rand.get_rng(100);
                //value = Rng.shift();
                //Rng.Add(rand.Next(100));
            }

            if (Saving_Rns)
                Saved_Rns.Add(result);

            return result;
        }

        public bool roll_rng(int rate)
        {
            int rn;
            return roll_rng(rate, out rn);
        }
        public bool roll_rng(int rate, out int rn)
        {
            rn = get_rng();
            return rn < rate;
        }
        public bool roll_dual_rng(int rate)
        {
            return (get_rng() + get_rng()) / 2 < rate;
        }

        public void add_preset_rn(int rn)
        {
            Preset_Rng.Add(rn);
        }

        public void clear_preset_rns()
        {
            Preset_Rng.Clear();
        }

        internal bool has_preset_rns { get { return Preset_Rng.Count > 0; } }

        public void save_rns()
        {
            Saving_Rns = true;
        }

        public void readd_saved_rns()
        {
            Saved_Rns.AddRange(Preset_Rng);
            Preset_Rng = Saved_Rns;
            Saved_Rns = new List<int>();
            Saving_Rns = false;
        }

        protected void reset_rng()
        {
            Rand = new Rng();
            //Rng.Clear(); //Debug
            //for (int i = 0; i < Config.RNG_VALUES; i++)
            //    Rng.Add(rand.Next(100));
#if DEBUG
            rng_activity();
#endif
        }

#if DEBUG
        internal void reseed_rng()
        {
            reset_rng();
        }

        internal int[] preview_rng(int range = 100)
        {
            int[] result = new int[range];
            Rng rng = Rand.copy_generator();
            for (int i = 0; i < range; i++)
                result[i] = rng.get_rng(100);
            return result;
        }

        public bool RngActivity { get { return LastRngActivity > 0; } }
        private byte LastRngActivity = 2;

        private void rng_activity()
        {
            LastRngActivity = 2;
        }
        private void update_rng_activity()
        {
            if (LastRngActivity > 0)
                LastRngActivity--;
        }
#endif
        #endregion

        public void new_chapter(List<string> previous_chapters, string chapter_id,
            Dictionary<string, string> previous_chapter_ids)
        {
            Previous_Chapters.Clear();
            Previous_Chapters.AddRange(previous_chapters);
            Chapter_Id = chapter_id;
            PreviousChapterIds = previous_chapter_ids;

            Chapter_Turn = 0;
            Chapter_Exp_Gain = 0;
            Chapter_Damage_Taken = 0;
            Chapter_Deaths = 0;
            Chapter_Completion = 0;

            Chapter_Play_Time = 0;
            Chapter_Start_Time = DateTime.UtcNow;
            GameplayStartTime = Chapter_Start_Time;
            Deployed_Unit_Count = -1;
            Deployed_Unit_Avg_Level = -1;

            if (Global.save_file == null)
                Rankings = new PastRankings();
            else
                Rankings = Global.save_file.past_rankings(chapter_id, previous_chapter_ids);
        }

        public void change_chapter(string id)
        {
            Objective_Text = "";
            Victory_Text = "";
            Loss_Text = "";

            reset_victory();
            Global.game_actors.heal_battalion();
            Global.game_actors.temp_clear();
        }

        public void set_gameplay_start()
        {
            // Don't allow setting the start time more than once per chapter
            if (GameplayStartTime == Chapter_Start_Time)
                GameplayStartTime = DateTime.UtcNow;
        }
        public void set_deployed_unit_stats(int count, int avg_level)
        {
            if (Deployed_Unit_Count == -1)
            {
                Deployed_Unit_Count = count;
                Deployed_Unit_Avg_Level = avg_level;
            }
        }

        #region Victory/Loss
        // override_victory(condition) //Debug

        // set_victory_text(str)

        // public void set_defeat_text(string str){}

        public void reset_victory()
        {
            Victory = false;
            Failure = false;
        }

        public void add_loss_on_death(int actor_id)
        {
            Loss_On_Death.Add(actor_id);
        }
        public void remove_loss_on_death(int actor_id)
        {
            Loss_On_Death.Remove(actor_id);
        }

        public void add_loss_on_ally_death(int actor_id)
        {
            if (!Ally_Loss_On_Death.Contains(actor_id))
                Ally_Loss_On_Death.Add(actor_id);
        }

        public void remove_loss_on_ally_death(int actor_id)
        {
            Ally_Loss_On_Death.Remove(actor_id);
        }

        public bool death_causes_loss(Game_Unit unit)
        {
            if (Loss_On_Death.Contains(unit.actor.id))
                return true;
            if (unit.is_player_allied)
                if (this.ally_loss_on_death.Contains(unit.actor.id))
                    return true;
            /* //Debug
            if (!unit.is_player_allied)
                if (Ally_Loss_On_Death.Contains(unit.actor.id))
                    return true;*/
            return false;
        }

        public void clear_loss_on_death()
        {
            Loss_On_Death.Clear();
            Ally_Loss_On_Death.Clear();
        }

        public bool is_victory()
        {
            return (Victory && !Failure);
        }

        public bool is_loss()
        {
            return Failure;
        }

        internal void update_victory()
        {
            // This should probably not be called every frame //Yeti
            if (!Failure && !Preparations && Global.map_exists)
            {
                foreach (int actor_id in Loss_On_Death)
                {
                    if (Global.game_map.actor_defeated(actor_id))
                    {
                        Failure = true;
                        continue;
                    }
#if DEBUG
                    if (!Global.game_state.battle_active && Global.game_actors[actor_id].is_dead())
                    {
                        throw new Exception("hey this old code got triggered weirdly"); //Yeti
                    }
#endif
                }
                foreach (int actor_id in this.ally_loss_on_death) // This is only supposed to check player units //Yeti
                {
                    if (Global.game_map.actor_defeated(actor_id))
                    {
                        if (!Global.game_map.defeated_actor_was_ally(actor_id))
                            continue;

                        Failure = true;
                        continue;
                    }
#if DEBUG
                    if (!Global.game_state.battle_active && Global.game_actors[actor_id].is_dead())
                    {
                        // Triggers when units die in the arena, make an exception
                        if (!Global.game_system.In_Arena)
                            throw new Exception("hey this old code got triggered weirdly"); //Yeti
                    }
#endif
                }
            }
        }
        #endregion

        internal Game_Ranking calculate_ranking_progress(TactileLibrary.Data_Chapter chapter,
            int killed_enemies, int enemies_remaining)
        {
            float chapter_progress = Chapter_Turn >= chapter.Ranking_Turns ? 1f :
                Chapter_Turn / (float)chapter.Ranking_Turns;

            int total_enemies = killed_enemies + enemies_remaining;
            // How many turns it should take to finish the map
            float kills_per_turn = MathHelper.Lerp(total_enemies / (float)chapter.Ranking_Turns,
                killed_enemies / (float)Chapter_Turn, chapter_progress);
            int turn_to_clear = (int)Math.Round(enemies_remaining / kills_per_turn) + Chapter_Turn;
            // If a survive/timed map, min this with the time limit since you can't take longer than that //Yeti

            // How much more damage will be taken
            killed_enemies = Math.Max(1, killed_enemies); // Avoid divide by 0s
            float damage_per_enemy = MathHelper.Lerp(chapter.Ranking_Combat / (float)total_enemies,
                Chapter_Damage_Taken / (float)killed_enemies, chapter_progress);
            int damage_on_clear = (int)Math.Round(enemies_remaining * damage_per_enemy) + Chapter_Damage_Taken;
            // How much more exp will be gained
            float exp_per_enemy = MathHelper.Lerp(chapter.Ranking_Exp / (float)total_enemies,
                Chapter_Exp_Gain / (float)killed_enemies, chapter_progress);
            int exp_on_clear = (int)Math.Round(enemies_remaining * exp_per_enemy) + Chapter_Exp_Gain;

            return new Game_Ranking(chapter.Id, Difficulty_Mode,
                turn_to_clear, damage_on_clear, exp_on_clear, Chapter_Deaths, Chapter_Completion);
        }

        #region Events
        internal bool call_event(string name, bool insert)
        {
            if (Global.map_exists)
            {
                return Global.game_state.activate_event_by_name(name, insert);
            }
            else
                throw new ArgumentNullException("Map doesn't exist");
        }

        public bool add_event(int id, bool run_lone_event, bool insert)
        {
            TryResetPrompts();

            // If the event is already running
            foreach (Event_Processor processor in Events)
                if (processor.key == Global.game_state.event_handler.name + id)
                    return false;
            if (insert)
                Events.Insert(0, new Event_Processor(id));
            else
                Events.Add(new Event_Processor(id));

            if (Events.Count == 1)
                if (run_lone_event)
                    update_event(0);
            return true;
        }
        public void add_event(TactileLibrary.Event_Data event_data)
        {
            TryResetPrompts();

            Events.Add(new Event_Processor(event_data));
            if (Events.Count == 1)
                update_event(0);
        }

        private void TryResetPrompts()
        {
            // Called when adding new events
            // If there are no events already running, reset the prompt results to nothing
            if (!Events.Any())
            {
                Global.game_temp.LastDialoguePrompt = TactileLibrary.Maybe<int>.Nothing;
                Global.game_temp.LastConfirmationPrompt = TactileLibrary.Maybe<bool>.Nothing;
            }
        }


        /// <summary>
        /// Sets the parent id of the currently running event
        /// </summary>
        internal void set_event_parent_id(int id)
        {
            Events[0].parent_id = id;
        }

        internal void skip_parent_event(int id)
        {
            var child_event = Events.Single(x => x.id == id);
            var parent_event = Events.Single(x => x.id == child_event.parent_id);
            parent_event.start_skip();
        }

        protected void update_events()
        {
            bool running = false;
            int i = 0;
            while (i < Events.Count)
            {
                running = true;
                if (!update_event(i))
                    break;
            }
            // If on a map
            if (Global.map_exists)
                // And a move range is active and events just ended and there is a cantoing unit selected
                if (!Global.game_map.is_move_range_active && running && !is_interpreter_running &&
                    Selected_Unit_Id != -1 && Global.game_map.get_selected_unit().cantoing &&
                    Global.game_state.is_map_ready(true))
                {
                    Global.game_map.get_selected_unit().open_move_range();
                }
        }

        protected bool update_event(int i)
        {
            Events[i].update();

            // Only comes up when a Worldmap event ends by immediately transitioning to a map
            if (Events.Count == 0)
                return false;

            if (Events[i].finished)
            {
                // If any event is the parent of this event
                if (Events.Any(x => x.id == Events[i].parent_id))
                    // End this event while passing in the parent
                    Events[i].end(Events.Single(x => x.id == Events[i].parent_id));
                // Else just end normally
                else
                    Events[i].end();

                Events.RemoveAt(i);
                if (Events.Count == 0)
                {
                    if (Global.scene.is_message_window_waiting)
                        Global.scene.end_waiting_message_window();
                    if (Global.scene.is_map_scene && !Global.game_map.is_off_map(Global.player.loc, false))
                        Global.game_map.highlight_test();
                }
                else
                    Events[0].ignore_move_update();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cancels events that would be performed after the current one
        /// </summary>
        public void cancel_further_events()
        {
            if (Events.Count > 1)
                Events.RemoveRange(1, Events.Count - 1);
        }

        public void clear_events()
        {
            Events.Clear();
        }
        public bool is_interpreter_running
        {
            get
            {
                if (Preparations)
                {
                    // If preparations is ready to run events, and events exist, and the active event is not waiting for preparations to continue
                    return Preparation_Events_Ready && Events.Count > 0 && !Events[0].waiting_for_prep;
                    //return Preparation_Events_Ready && Events.Any(x => !x.waiting_for_prep); //Debug
                }
                else
                    return Events.Count > 0; //Debug
                    //return (!Preparations || !Global.scene.is_map_scene) && Events.Count > 0; //Debug
                //return (Global.scene.is_map_scene ? !Global.game_system.preparations : true) && Events.Count > 0;
            }
        }

        public bool is_battle_interpreter_running
        {
            get
            {
                if (!Preparations || !Global.scene.is_map_scene)
                {
                    if (Events.Any(event_processor => event_processor.battle_wait))
                        return false;
                    return Events.Count > 0;
                }
                return false;
            }
        }
        public bool is_rescue_interpreter_running
        {
            get
            {
                if (!Preparations || !Global.scene.is_map_scene)
                {
                    if (Events.Any(event_processor => event_processor.rescue_wait))
                        return false;
                    return Events.Count > 0;
                }
                return false;
            }
        }

#if DEBUG
        /// <summary>
        /// Returns a string representing an event command in the active event.
        /// </summary>
        /// <param name="offset">Added to the current index in the event queue.</param>
        internal string active_event_command(int offset)
        {
            if (Events.Count == 0)
                return "-----";
            return Events[0].event_command_string(offset);
        }
#endif

        public int get_variable(int index)
        {
            return VARIABLES[index];
        }
        public bool get_switch_state(int index)
        {
            return SWITCHES[index];
        }
        #endregion

        public void update()
        {
            const int MAX_TICKS = 100 * 60 * 60 * Config.FRAME_RATE;

            Total_Play_Time++;
            // If past 99:59:59
            if (Total_Play_Time >= MAX_TICKS)
                // Lock time
                Total_Play_Time -= Config.FRAME_RATE;

            Chapter_Play_Time++;
            // If past 99:59:59
            if (Chapter_Play_Time >= MAX_TICKS)
                // Lock time
                Chapter_Play_Time -= Config.FRAME_RATE;

            update_timers();
            update_victory();
            update_events();

#if DEBUG
            update_rng_activity();
#endif
        }

        public float play_time_sine_wave(
            int periodInTicks,
            int offset = 0,
            bool unitSineWave = true)
        {
            return play_time_sine_wave(
                periodInTicks / (float)Config.FRAME_RATE,
                offset, unitSineWave);
        }
        public float play_time_sine_wave(
            float periodInSeconds,
            int offset = 0,
            bool unitSineWave = true)
        {
            int time = Global.game_system.total_play_time + offset;
            float value = (float)Math.Sin(
                (time * MathHelper.TwoPi) /
                (Config.FRAME_RATE * periodInSeconds));
            if (unitSineWave)
                value = (value / 2f) + 0.5f;
            return value;
        }

        public void update_timers()
        {
            Character_Anim_Count = (Character_Anim_Count + 1) % Config.CHARACTER_TIME;
            Unit_Blink_Timer = (Unit_Blink_Timer + 1) % Config.UNIT_BLINK_PERIOD;
        }

        public int unit_highlight_anim_frame(int tick)
        {
            int index = 0;
            for (int i = 0; i < Config.CHARACTER_HIGHLIGHT_ANIM_TIMES.Length; i++)
            {
                if (tick < Config.CHARACTER_HIGHLIGHT_ANIM_TIMES[i])
                {
                    index = i;
                    break;
                }
                else
                    tick -= Config.CHARACTER_HIGHLIGHT_ANIM_TIMES[i];
            }
            return Config.CHARACTER_HIGHLIGHT_ANIM_FRAMES[index];
        }
    }
}
