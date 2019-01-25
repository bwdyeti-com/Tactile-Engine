using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using FEXNA_Library;
using HashSetExtension;
using ListExtension;
using FEXNAVector2Extension;
using FEXNAListExtension;
using FEXNAWeaponExtension;
using FEXNAVersionExtension;
using FEXNAContentExtension;

namespace FEXNA
{
    public enum Aid_Types { Infantry, Mounted }
    internal enum EventedMoveCommands { Move, Highlight, Notice, SetSpeed }
    internal partial class Game_Unit : Combat_Map_Object
    {
        static bool WAITING_FOR_SCROLL_BEFORE_MOVING = false;

        protected Vector2 Move_Loc = Vector2.Zero, Turn_Start_Loc = Vector2.Zero, Prev_Loc = Vector2.Zero, Rescue_Drop_Loc = Vector2.Zero;
        protected int Moved_So_Far = 0, Temp_Moved = 0;
        protected int Team, Group;
        protected int ActorId;
        protected bool Highlighted = false;
        protected int ForceHighlightTimer = 0;
        protected int Moving_Anim = -1, Highlighted_Anim = -1;
        protected int Move_Timer = 0;
        protected bool Ready = true;
        protected bool Sprite_Moving = false, Battling = false;
        protected bool Cantoing = false;
        protected int Rescued = 0, Rescuing = 0;
        protected bool Magic_Attack = false;
        protected HashSet<Vector2> Move_Range = new HashSet<Vector2>(), Attack_Range = new HashSet<Vector2>(),
            Staff_Range = new HashSet<Vector2>(), Talk_Range = new HashSet<Vector2>();
        protected List<Vector2> Move_Route = new List<Vector2>();
        protected int Mission = 0, Ai_Mission = 2;
        protected bool Dead = false;
        protected bool Boss = false, Drops_Item = false;
        protected int Priority = 0;
        protected bool Gladiator = false;
        protected int Vision_Bonus = 0;
        protected List<int> Stat_Bonuses = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 }; // Make this determine the proper length //Debug
        protected bool Blocked = false;
        protected bool Using_Siege_Engine = false;
        protected HashSet<int> Attack_Targets_This_Turn = new HashSet<int>();
        protected int Exp_Given = 0;
        protected bool Ai_Wants_Rescue = false, Ai_Terrain_Healing = false;

        private List<int> TempBuffs;
        protected List<Vector2> Attack_Movement = new List<Vector2>();
        protected List<int> Wiggle_Movement = new List<int>();
        protected List<int> Color_List = new List<int>();
        protected List<int> Opacity_List = new List<int>();
        protected Color Unit_Color = Color.Transparent;
        protected int Opacity = 255;
        protected bool Ai_Move = false;
        protected int Enemy_Move_Wait = 0;
        protected Vector2? Force_Move = null;
        protected bool Evented_Move = false;
        private List<Tuple<EventedMoveCommands, float>> EventedMoveRoute;
        protected int EventedMoveSpeed = -1;
        protected bool Ignore_Terrain_Cost = false;
        protected List<Vector2> Formation_Locs = new List<Vector2>();
        protected Vector2 Prev_Move_Route_Loc;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Id);
            Loc.write(writer);
            Real_Loc.write(writer);
            Move_Loc.write(writer);
            Turn_Start_Loc.write(writer);
            Prev_Loc.write(writer);
            Rescue_Drop_Loc.write(writer);
            writer.Write(Moved_So_Far);
            writer.Write(Temp_Moved);
            writer.Write(Facing);
            writer.Write(Frame);
            writer.Write(Team);
            writer.Write(Group);
            writer.Write(ActorId);
            writer.Write(Highlighted);
            writer.Write(ForceHighlightTimer);
            writer.Write(Moving_Anim);
            writer.Write(Highlighted_Anim);
            writer.Write(Move_Timer);
            writer.Write(Ready);
            writer.Write(Sprite_Moving);
            writer.Write(Battling);
            writer.Write(Cantoing);
            writer.Write(Rescued);
            writer.Write(Rescuing);
            writer.Write(Magic_Attack);
            Move_Range.write(writer);
            Attack_Range.write(writer);
            Staff_Range.write(writer);
            Talk_Range.write(writer);
            Move_Route.write(writer);
            writer.Write(Mission);
            writer.Write(Ai_Mission);
            writer.Write(Dead);
            writer.Write(Boss);
            writer.Write(Drops_Item);
            writer.Write(Priority);
            writer.Write(Gladiator);
            writer.Write(Vision_Bonus);
            Stat_Bonuses.write(writer);
            writer.Write(Blocked);
            writer.Write(Using_Siege_Engine);
            Attack_Targets_This_Turn.write(writer);
            writer.Write(Exp_Given);
            writer.Write(Ai_Wants_Rescue);
            writer.Write(Ai_Terrain_Healing);

            skills_write(writer);
        }

        public void read(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            Loc = Loc.read(reader);
            Real_Loc = Real_Loc.read(reader);
            Move_Loc = Move_Loc.read(reader);
            Turn_Start_Loc = Turn_Start_Loc.read(reader);
            Prev_Loc = Prev_Loc.read(reader);
            Rescue_Drop_Loc = Rescue_Drop_Loc.read(reader);
            Moved_So_Far = reader.ReadInt32();
            Temp_Moved = reader.ReadInt32();
            Facing = reader.ReadInt32();
            Frame = reader.ReadInt32();
            Team = reader.ReadInt32();
            Group = reader.ReadInt32();
            ActorId = reader.ReadInt32();
            Highlighted = reader.ReadBoolean();
            if (!Global.LOADED_VERSION.older_than(0, 5, 1, 5)) // This is a suspend load, so this isn't needed for public release //Debug
                ForceHighlightTimer = reader.ReadInt32(); 
            Moving_Anim = reader.ReadInt32();
            Highlighted_Anim = reader.ReadInt32();
            Move_Timer = reader.ReadInt32();
            Ready = reader.ReadBoolean();
            Sprite_Moving = reader.ReadBoolean();
            Battling = reader.ReadBoolean();
            Cantoing = reader.ReadBoolean();
            Rescued = reader.ReadInt32();
            Rescuing = reader.ReadInt32();
            Magic_Attack = reader.ReadBoolean();
            Move_Range.read(reader);
            Attack_Range.read(reader);
            Staff_Range.read(reader);
            Talk_Range.read(reader);
            Move_Route.read(reader);
            Mission = reader.ReadInt32();
            Ai_Mission = reader.ReadInt32();
            Dead = reader.ReadBoolean();
            Boss = reader.ReadBoolean();
            Drops_Item = reader.ReadBoolean();
            Priority = reader.ReadInt32();
            Gladiator = reader.ReadBoolean();
            Vision_Bonus = reader.ReadInt32();
            Stat_Bonuses.read(reader);
            Blocked = reader.ReadBoolean();
            Using_Siege_Engine = reader.ReadBoolean();
            Attack_Targets_This_Turn.read(reader);
            if (!Global.LOADED_VERSION.older_than(0, 4, 6, 5)) // This is a suspend load, so this isn't needed for public release //Debug
                Exp_Given = reader.ReadInt32();
            Ai_Wants_Rescue = reader.ReadBoolean();
            if (!Global.LOADED_VERSION.older_than(0, 4, 3, 8)) // This is a suspend load, so this isn't needed for public release //Debug
                Ai_Terrain_Healing = reader.ReadBoolean();

            skills_read(reader);
        }
        #endregion

        #region Accessors
        public Vector2 turn_start_loc { get { return Turn_Start_Loc; } }
        public Vector2 move_start_loc { get { return Prev_Loc; } }
        public Vector2 rescue_drop_loc { get { return Rescue_Drop_Loc; } }
        public Vector2 pathfinding_loc { get { return Evented_Move ? Move_Loc : Loc; } }

        public int facing
        {
            get { return Facing; }
            set { Facing = value; }
        }
        public int frame
        {
            set { Frame = value; }
        }

        public Game_Actor actor { get { return Global.game_actors[ActorId]; } }

        public bool highlighted
        {
            get { return Highlighted; }
            set
            {
                if (Highlighted != value)
                {
                    Highlighted = value;
                    Frame = 0;
                    update_map_animation(true);
                }
                Highlighted = value;
            }
        }

        public bool ready { get { return Ready; } }

        public bool sprite_moving
        {
            get { return Sprite_Moving; }
            set
            {
                Sprite_Moving = value;
                update_map_animation(true);
                refresh_sprite();
            }
        }

        public bool battling
        {
            get { return Battling; }
            set
            {
                Battling = value;
                update_map_animation(true);
                if (Global.scene.is_strict_map_scene)
                    refresh_sprite();
            }
        }

        public int rescued
        {
            get { return Rescued; }
            set { Rescued = value; }
        }
        public int rescuing
        {
            get { return Rescuing; }
            set { Rescuing = value; }
        }

        public Game_Unit rescuer_unit
        {
            get
            {
                if (!Global.game_map.units.ContainsKey(Rescued))
                    return null;
                return Global.game_map.units[Rescued];
            }
        }
        public Game_Unit rescuing_unit
        {
            get
            {
                if (!Global.game_map.units.ContainsKey(Rescuing))
                    return null;
                return Global.game_map.units[Rescuing];
            }
        }

        public bool magic_attack
        {
            get { return Magic_Attack; }
            set
            {
                // Overriden by Swoop/Skills
                // Skills : Swoop
                if (Old_Swoop_Activated) // New Swoop shouldn't care if it's a magic attack or not
                {
                    Magic_Attack = false;
                    return;
                }
                // Skills: Trample
                if (Trample_Activated)
                {
                    Magic_Attack = false;
                    return;
                }
                Magic_Attack = value;
            }
        }

        public bool cantoing
        {
            get { return Cantoing; }
            set { Cantoing = value; }
        }

        public override int team { get { return Team; } }
        public bool has_flipped_map_sprite
        {
            get { return Constants.Team.flipped_map_sprite(Team); }
        }
        public bool has_flipped_face_sprite
        {
            get { return Team % 2 == 1; }
        }

        public int group
        {
            get { return Group; }
            set { Group = value; }
        }

        public Vector2 canto_loc { get { return (Cantoing ? Move_Loc : Loc); } }

        public HashSet<Vector2> move_range
        {
            get
            {
                HashSet<Vector2> range = new HashSet<Vector2>(Move_Range); //HashSet
                return range;
            }
        }
        public HashSet<Vector2> attack_range
        {
            get
            {
                if (!Global.game_map.updated_attack_range_units.Contains(Id))
                    update_attack_range();
                HashSet<Vector2> range = new HashSet<Vector2>(Attack_Range); //HashSet
                return range;
            }
        }
        public HashSet<Vector2> staff_range
        {
            get
            {
                if (!Global.game_map.updated_staff_range_units.Contains(Id))
                    update_staff_range();
                HashSet<Vector2> range = new HashSet<Vector2>(Staff_Range); //HashSet
                return range;
            }
        }
        public HashSet<Vector2> talk_range
        {
            get
            {
                HashSet<Vector2> range = new HashSet<Vector2>(Talk_Range); //HashSet
                return range;
            }
        }

        public bool move_route_empty { get { return Move_Route.Count == 0; } }
        public bool evented_move_route_empty { get { return EventedMoveRoute == null; } }

        public int mission
        {
            get { return Mission; }
            set { Mission = value; }
        }
        public int full_ai_mission
        {
            get { return Ai_Mission; }
            set { Ai_Mission = value; }
        }
        public int ai_mission
        {
            get { return Ai_Mission % Game_AI.MISSION_COUNT; }
        }
        public int ai_priority
        {
            get { return Ai_Mission / Game_AI.MISSION_COUNT; }
        }

        public bool dead { get { return Dead; } }

        public bool boss
        {
            get { return Boss; }
            set { Boss = value; }
        }

        public bool drops_item
        {
            get { return Drops_Item && actor.has_items; }
            set { Drops_Item = value; }
        }

        public int priority
        {
            get { return Priority; }
            set { Priority = value; }
        }

        public bool gladiator
        {
            get { return Gladiator; }
            set { Gladiator = value; }
        }

        public bool using_siege_engine
        {
            get { return Using_Siege_Engine; }
            set { Using_Siege_Engine = value; }
        }

        public int exp_given { get { return Exp_Given; } }

        public bool ai_wants_rescue
        {
            get { return Ai_Wants_Rescue; }
            set { Ai_Wants_Rescue = value; }
        }
        public bool ai_terrain_healing
        {
            get { return Ai_Terrain_Healing; }
            set { Ai_Terrain_Healing = value; }
        }

        internal bool is_evented_move { get { return Evented_Move; } }
        #endregion

        public override string ToString()
        {
#if WINDOWS && DEBUG
            string mission = Game_AI.MISSION_NAMES.ContainsKey(this.ai_mission) ?
                Game_AI.MISSION_NAMES[this.ai_mission] : string.Format("<{0}>", this.ai_mission);
            string current_mission = Game_AI.MISSION_NAMES.ContainsKey(Mission) ?
                Game_AI.MISSION_NAMES[Mission] : string.Format("<{0}>", Mission);
            if (!is_player_team)
                return String.Format("{0}, {1}{2}, Team: {3}, Loc: {4} {5}, Mission: {6} ({7}), Ai Priority: {8}",
                    actor.name_full, actor.class_name, actor.gender, Team, (int)Loc.X, (int)Loc.Y,
                    mission, current_mission, ai_priority);
            else
#endif
                return String.Format("{0}, {1}{2}, Team: {3}, Loc: {4} {5}",
                    actor.name_full, actor.class_name, actor.gender, Team, (int)Loc.X, (int)Loc.Y);
        }

        public Game_Unit() { }
        public Game_Unit(int id, int actor_id)
        {
            Game_Actor temp_actor = Global.game_actors[actor_id];
            ActorId = actor_id;
            initialize(id, Config.OFF_MAP, Constants.Team.PLAYER_TEAM);
        }
        public Game_Unit(int id, Vector2 loc, int team, int priority)
        {
            Game_Actor actor = Global.game_actors.new_actor();
            ActorId = actor.id;
            initialize(id, loc, team, priority);
        }
        public Game_Unit(int id, Vector2 loc, int team, int priority, int actor_id)
        {
            Game_Actor temp_actor = Global.game_actors[actor_id];
            ActorId = actor_id;
            initialize(id, loc, team, priority);
        }

        public static Game_Unit class_reel_unit()
        {
            Game_Unit unit = new Game_Unit();
            unit.ActorId = Global.game_actors.new_actor().id;
            return unit;
        }

        protected void initialize(int id, Vector2 loc, int team)
        {
            //actor.setup_items(false); //Debug
            Id = id;
            Turn_Start_Loc = Prev_Loc = Move_Loc = Loc = loc;
            refresh_real_loc();
            Team = (int)MathHelper.Clamp(team, 1, Constants.Team.NUM_TEAMS);
        }
        protected void initialize(int id, Vector2 loc, int team, int priority)
        {
            //actor.setup_items(false); // Doing this once in the Game_Actor constructor instead //Debug
            Id = id;
            Turn_Start_Loc = Prev_Loc = Move_Loc = Loc = loc;
            refresh_real_loc();

            Priority = priority;
            change_team((int)MathHelper.Clamp(team, 1, Constants.Team.NUM_TEAMS));
            if (Global.scene.is_map_scene && !Global.scene.is_test_battle)
                init_sprites();
        }

        public override bool is_unit()
        {
            return true;
        }

        internal void change_team(int team)
        {
            Team = team;
            // Clear item drop flag if switching to an allied team
            if (!is_attackable_team(Constants.Team.PLAYER_TEAM))
                Drops_Item = false;

            if (!Global.scene.is_test_battle)
            {
                Global.game_map.team_add(Team, this);
                for (int i = 1; i <= Constants.Team.NUM_TEAMS; i++)
                {
                    if (i != Team)
                        Global.game_map.team_remove(i, Id);
                }
                // This is no longer automatic, and handled entirely through event code //Debug
                //if (team == Constants.Team.PLAYER_TEAM && !Global.game_actors.is_temp_actor(actor) && !actor.is_out_of_lives()) //Debug
                //    Global.battalion.add_actor(actor.id);
                //else
                //    Global.battalion.remove_actor(actor.id);
                Group = 0;
                Attack_Targets_This_Turn.Clear();
            }
        }

        #region Actor Properties
        public override int maxhp { get { return actor.maxhp; } }
        public override int hp
        {
            get { return actor.hp; }
            set
            {
                int hp = actor.hp;
                actor.hp = value;
                refresh_hp(hp - actor.hp);
            }
        }
        public override bool is_dead { get { return actor.is_dead(); } }

        public override string name { get { return actor.name; } }

        private void refresh_hp(int damageTaken)
        {
            refresh_hp_skill(damageTaken);
        }

        public List<Item_Data> items
        {
            get
            {
                //Debug
                //List<Item_Data> items = actor.items.GetRange(0, Constants.Actor.NUM_ITEMS);
                List<Item_Data> items = actor.items;
                if (can_use_siege() && Loc != Config.OFF_MAP && !Gladiator)
                {
                    Siege_Engine siege = Global.game_map.get_siege(Loc);
                    if (siege != null && siege.is_ready &&
                            actor.is_equippable_as_siege(Global.data_weapons[siege.item.Id]))
                        items.Add(siege.item);
                }
                return items;
            }
        }

        public int mov
        {
            get
            {
                return this.immobile ?
#if DEBUG
                    0 : (int)MathHelper.Clamp(actor.mov + mov_plus, 0, 100);//actor.mov_cap); } }
#else
                    0 : (int)MathHelper.Clamp(actor.mov + mov_plus, 0, actor.mov_cap);
#endif
            }
        }

        public int mov_plus
        {
            get
            {
                return actor.mov_plus +
                    mov_plus_skill() + temporary_stat_buff(Buffs.Mov);
            }
        }

        public int base_mov { get { return (int)MathHelper.Clamp(actor.mov, 0, actor.mov_cap); } }

        public bool is_mov_capped()
        {
            return (base_mov >= actor.mov_cap);
        }

        public int canto_mov
        {
            get
            {
#if DEBUG
                if (Global.scene.scene_type != "Scene_Map_Unit_Editor")
                    if (is_ally && actor.mov > 0)
                        if (INFINITE_MOVE_ALLOWED)//actor.has_skill("ASTRA"))
                            return 100;
#endif
                int mov = this.mov;
                // Skills: Dash
                if (DashActivated)
                    return mov;

                if (Cantoing)
                    mov -= Moved_So_Far;
                return mov;
            }
        }

        internal bool immobile
        {
            get
            {
                return !is_player_team &&
                    Game_AI.IMMOBILE_MISSIONS.Contains(this.ai_mission) &&
                    (this.ai_mission) != Game_AI.DO_NOTHING_MISSION;
            }
        }
        #endregion

        #region Stats
        public int stat(Stat_Labels stat)
        {
            switch (stat)
            {
                case Stat_Labels.Hp:
                    return actor.stat(stat); //Yeti
                case Stat_Labels.Pow:
                case Stat_Labels.Lck:
                case Stat_Labels.Def:
                case Stat_Labels.Res:
                    return actor.stat(stat) + stat_bonus(stat);
                case Stat_Labels.Skl:
                    int skill = actor.stat(stat);
                    if (is_weighed_stat(stat))
                        skill /= 2;
                    return skill + stat_bonus(stat);
                case Stat_Labels.Spd:
                    return spd();
                case Stat_Labels.Con:
                    return Math.Min(
                        Math.Max(1, actor.stat(Stat_Labels.Con) +
                            stat_bonus(Stat_Labels.Con)),
                        actor.get_cap(Stat_Labels.Con));
                case Stat_Labels.Mov:
                    return mov;
            }
            throw new NotImplementedException("Invalid stat parameter: " + stat.ToString());
        }

        public int stat_cap(Stat_Labels stat)
        {
            int cap = this.actor.get_cap(stat);
            switch(stat)
            {
                case Stat_Labels.Skl:
                case Stat_Labels.Spd:
                    if (is_weighed_stat(stat))
                        cap /= 2;
                    break;
            }
            return cap;
        }

        public int atk_pow()
        {
            return atk_pow(null, false);
        }
        public int atk_pow(Data_Weapon weapon)
        {
            return atk_pow(weapon, false);
        }
        public int atk_pow(Data_Weapon weapon, bool magic)
        {
            Maybe<int> skill = atk_pow_skill(weapon, magic);
            if (skill.IsSomething)
                return skill;

            // Staves
            if (weapon.is_staff() && weapon.Heals())
            {
                int base_stat = Constants.Combat.STAFF_HEAL_WITH_RES ?
                    stat(Stat_Labels.Res) : stat(Stat_Labels.Pow);
                return (int)(base_stat * Constants.Combat.STAFF_HEAL_POW_RATE);
            }
            // MWeapons
            else if (Constants.Combat.IMBUE_WITH_RES && magic &&
                    actor.power_type() == Power_Types.Strength) // == str or != mag ? //Debug
                return stat(Stat_Labels.Res);

            return stat(Stat_Labels.Pow);
        }

        protected int mag_range_pow(Data_Weapon weapon, bool magic)
        {
            return stat(Stat_Labels.Pow);
        }

        protected int spd()
        {
            return spd(null);
        }
        protected int spd(int? weapon_id)
        {
            // Gets spd from actor
            int speed = actor.stat(Stat_Labels.Spd);
            // If rescuing/etc and spd is halved
            if (is_weighed_stat(Stat_Labels.Spd))
                speed /= 2;
            // If an equipped weapon is being tested, get the wgt penalty for it
            if (weapon_id != null)
                speed -= wgt_penalty((int)weapon_id);
            // Add skill/etc bonuses and return
            return Math.Max(0, speed) + stat_bonus(Stat_Labels.Spd);
        }

        public int atk_spd()
        {
            return atk_spd(1, -1);
        }
        public int atk_spd(int? distance, int item_index = -1)
        {
            if (item_index == -1)
                return atk_spd((int)distance, new Item_Data(Item_Data_Type.Weapon, actor.weapon_id, 1));
            else
                return atk_spd((int)distance, items[item_index]);
        }
        internal int atk_spd(int distance, Item_Data item_data)
        {
            if (item_data != null && item_data.is_weapon)
                return atk_spd(distance, item_data.to_weapon);

            int speed = spd();
            return Math.Max(0, speed);
        }
        internal int atk_spd(int distance, Data_Weapon weapon)
        {
            int speed = spd();

            if (weapon != null)
            {
                bool magic_attack = check_magic_attack(weapon, distance);
                Maybe<int> skill = atk_spd_skill(speed, weapon, magic_attack);
                if (skill.IsSomething)
                    speed = skill;

                // If an equipped weapon is being tested, get the wgt penalty for it
                speed -= wgt_penalty(weapon.Id);
            }

            return Math.Max(0, speed);
        }

        private bool ignore_terrain_def()
        {
            return Config.IGNORE_TERRAIN_DEF.Intersect(actor.actor_class.Class_Types).Any();
        }

        private bool ignore_terrain_avo()
        {
            return Config.IGNORE_TERRAIN_AVO.Intersect(actor.actor_class.Class_Types).Any();
        }

        internal int terrain_def_bonus()
        {
            return Global.game_map.terrain_def_bonus(Loc);
        }
        internal Maybe<int> terrain_def_bonus(Game_Unit target)
        {
            Maybe<int> result = default(Maybe<int>);
            // Skills: Parity
            if (target != null)
            {
                if (!nihil(target))
                    if (actor.has_skill("PARITY"))
                        return 0;
                if (!target.nihil(this))
                    if (target.actor.has_skill("PARITY"))
                        return 0;
            }
            // Skills: Commando
            if (target != null && !nihil(target))
                if (actor.has_skill("CMNDO"))
                    result = result.ValueOrDefault + (terrain_def_bonus() / 2);
            // If terrain is ignored, return the result so far
            if (target != null && ignore_terrain_def())
                return result;
            return result.ValueOrDefault + terrain_def_bonus();
        }

        internal int terrain_res_bonus()
        {
            return Global.game_map.terrain_res_bonus(Loc);
        }
        internal Maybe<int> terrain_res_bonus(Game_Unit target)
        {
            Maybe<int> result = default(Maybe<int>);
            // Skills: Parity
            if (target != null)
            {
                if (!nihil(target))
                    if (actor.has_skill("PARITY"))
                        return 0;
                if (!target.nihil(this))
                    if (target.actor.has_skill("PARITY"))
                        return 0;
            }
            // Skills: Commando
            if (target != null && !nihil(target))
                if (actor.has_skill("CMNDO"))
                    result = result.ValueOrDefault + (terrain_res_bonus() / 2);
            // If terrain is ignored, return the result so far
            if (ignore_terrain_def())
                //if (target != null && ignore_terrain_def()) //Debug
                return result;
            return result.ValueOrDefault + terrain_res_bonus();
        }

        internal int terrain_avo_bonus()
        {
            return Global.game_map.terrain_avo_bonus(Loc);
        }
        internal Maybe<int> terrain_avo_bonus(Game_Unit target, bool magicAttack)
        {
            Maybe<int> result = default(Maybe<int>);
            // Skills: Parity
            if (target != null)
            {
                if (!nihil(target))
                    if (actor.has_skill("PARITY"))
                        return 0;
                if (!target.nihil(this))
                    if (target.actor.has_skill("PARITY"))
                        return 0;
            }
            // Skills: Commando
            if (target != null && !nihil(target))
                if (actor.has_skill("CMNDO"))
                    result = result.ValueOrDefault + (terrain_avo_bonus() / 2);
            // If terrain is ignored, return the result so far
            if (ignore_terrain_avo())
                //if (target != null && ignore_terrain_avo()) //Debug
                return result;
            return result.ValueOrDefault + terrain_avo_bonus();
        }

        internal bool terrain_heals()
        {
            return Global.game_map.terrain_heals(Loc);
        }

        internal bool halve_effectiveness()
        {
            if (Constants.Combat.HALVE_EFFECTIVENESS_ON_FORTS)
                return this.terrain_heals();

            return false;
        }

        public int wgt_penalty()
        {
            return wgt_penalty(actor.weapon_id);
        }
        public int wgt_penalty(int weapon_id)
        {
            if (!Global.data_weapons.ContainsKey(weapon_id))
                return 0;
            Data_Weapon weapon = Global.data_weapons[weapon_id];
            int wgt = actor.weapon_wgt(weapon);
            return (int)(stat(Stat_Labels.Con) < wgt ? wgt - stat(Stat_Labels.Con) : 0);
        }

        public int aid()
        {
            return (aid_type() == Aid_Types.Infantry ? stat(Stat_Labels.Con) - 1 : Math.Max(0, base_aid() - stat(Stat_Labels.Con)));
        }

        public Aid_Types aid_type()
        {
            if (Config.MOUNTED_CLASS_TYPES.Any(x => x.Equals(actor.actor_class.Class_Types)))
                return Aid_Types.Mounted;
            return Aid_Types.Infantry;
        }

        public int base_aid()
        {
            foreach (var type in Config.MOUNTED_CLASS_TYPES)
                if (type.Equals(actor.actor_class.Class_Types))
                    return Config.MOUNTED_CLASS_AID[type];
            return 15;
        }

        public int toughness()
        {
            return actor.hp + stat(Stat_Labels.Def) + stat(Stat_Labels.Res);
        }

        public int threat()
        {
            return threat(true);
        }
        public int threat(bool move)
        {
            int threat = (int)((move ? actor.hp : stat(Stat_Labels.Hp)) *
                Game_Actor.GetStatValue((int)Stat_Labels.Hp));
            for (int i = (int)Stat_Labels.Hp + 1; i <= (int)Stat_Labels.Con; i++)
                threat += (int)(stat((Stat_Labels)i) * Game_Actor.GetStatValue(i));
            if (!move)
                return threat;
            //@Yeti: Maybe this should use GetStatValue() too
            threat += stat(Stat_Labels.Mov) * 2;
            if (actor.equipped <= 0)
                threat /= 2;
            return threat;
        }

        public int rating()
        {
            int rating = 0;
            for (int i = (int)Stat_Labels.Hp + 1; i <= (int)Stat_Labels.Con; i++)
                rating += stat((Stat_Labels)i);
            rating += 5 * support_ranks_in_range();
            rating += (actor.tier * 7) / 2;
            return rating;
        }
        #endregion

        #region Stat Bonuses
        public int stat_bonus(Stat_Labels stat)
        {
            switch (stat)
            {
                case Stat_Labels.Hp:
                    return 0; //Debug
                case Stat_Labels.Pow:
                    return pow_bonus_skill + temporary_stat_buff(Buffs.Pow);
                case Stat_Labels.Skl:
                    return skl_bonus_skill + temporary_stat_buff(Buffs.Skl);
                case Stat_Labels.Spd:
                    return spd_bonus_skill + temporary_stat_buff(Buffs.Spd);
                case Stat_Labels.Lck:
                    return lck_bonus_skill + temporary_stat_buff(Buffs.Lck);
                case Stat_Labels.Def:
                    return def_bonus_skill + temporary_stat_buff(Buffs.Def);
                case Stat_Labels.Res:
                    return res_bonus_skill + temporary_stat_buff(Buffs.Res);
                case Stat_Labels.Con:
                    return con_bonus_skill + actor.con_plus + temporary_stat_buff(Buffs.Con);
            }
            throw new ArgumentException("Invalid stat bonus parameter: " + stat.ToString());
        }

        public int temporary_stat_buff(Buffs stat)
        {
            return Stat_Bonuses[(int)stat];
        }
        #endregion

        #region Support Bonuses
        public int support_bonus(Combat_Stat_Labels stat, bool status_display = false)
        {
            float n = 0;
            foreach (int actor_id in support_range_actors(status_display))
                n += actor.get_support_bonus(actor_id, stat);
            return (int)Math.Floor(n);
        }

        /// <summary>
        /// Returns a set of the unit ids within range to provide support bonuses.
        /// </summary>
        protected HashSet<int> support_range_units()
        {
            if (!Global.scene.is_map_scene || (!Config.SUPPORTS_IN_ARENA && Global.game_system.In_Arena))
                return new HashSet<int>();

            // Look for units within support range
            HashSet<int> result = units_in_range(Constants.Support.SUPPORT_RANGE);
            if (is_rescuing)
                result.Add(Rescuing);
            return result;
        }

        /// <summary>
        /// Returns a set of the actors within range to provide support bonuses
        /// ie. actors for units within Constants.Support.SUPPORT_RANGE many tiles.
        /// At the home base, this is the entire battalion.
        /// </summary>
        /// <param name="status_display">If true, this will not return a blank set when dealing with units are aren't on the map, for example in the home base.</param>
        protected HashSet<int> support_range_actors(bool status_display = false)
        {
            if (!Global.scene.is_map_scene || (!Config.SUPPORTS_IN_ARENA && Global.game_system.In_Arena) ||
                    (!(status_display && Global.game_system.home_base) && Global.game_map.is_off_map(Loc)))
                return new HashSet<int>();

            // If at the home base, return the whole battalion
            if (Global.game_system.home_base)
                return new HashSet<int>(Global.battalion.actors.Where(actor_id => actor_id != ActorId));
            // Otherwise look for units within support range
            else
            {
                //HashSet<int> result = units_in_range(Constants.Support.SUPPORT_RANGE); //Debug
                //if (is_rescuing)
                //    result.Add(Rescuing);
                return new HashSet<int>(support_range_units().Select(unit_id => Global.game_map.units[unit_id].actor.id));
            }
        }

        protected int support_ranks_in_range()
        {
            int n = 0;
            foreach (int actor_id in support_range_actors(true))
                n += actor.get_support_level(actor_id);
            return n;
        }

        protected bool support_possible(Game_Unit unit)
        {
            return actor.support_possible(unit.actor.id) &&
                unit.actor.support_possible(actor.id);
        }

        protected void new_turn_support_gain_display()
        {
            // Only if standing near on new turn gives support points
            if (Constants.Support.ADJACENT_SUPPORT_POINTS <= 0)
                return;

            var support_partners = support_range_units()
                .Where(x => x != Rescuing &&
                    Global.game_map.units[x].team == this.team &&
                    !Global.game_map.units[x].ready &&
                    support_possible(Global.game_map.units[x]));
            display_support_gain(support_partners);
        }

        internal void same_target_support_gain_display(int id)
        {
            // Only if attacking the same target gives support points
            if (Constants.Support.SAME_TARGET_SUPPORT_POINTS <= 0)
                return;

            var support_partners = Global.game_map.teams[Team]
                .Where(x => Global.game_map.units[x].Attack_Targets_This_Turn.Contains(id) &&
                    support_possible(Global.game_map.units[x]));
            display_support_gain(support_partners);
        }
        internal void same_target_support_gain_display(IEnumerable<int> ids)
        {
            // Only if attacking the same target gives support points
            if (Constants.Support.SAME_TARGET_SUPPORT_POINTS <= 0)
                return;

            var support_partners = Global.game_map.teams[Team]
                .Where(x => !Global.game_map.units[x].Attack_Targets_This_Turn.Intersect(ids).Any() &&
                    support_possible(Global.game_map.units[x]));
            display_support_gain(support_partners);
        }

        internal void heal_support_gain_display(int id)
        {
            // Only if healing gives support points
            if (Constants.Support.HEAL_SUPPORT_POINTS <= 0)
                return;

            if (Staff_Data.get_staff_mode(this.actor.weapon_id) == Staff_Modes.Heal)
                if (support_possible(Global.game_map.units[id]))
                    display_support_gain(id);
        }

        internal void talk_support_gain_display(int id)
        {
            // Only if talks give support points
            if (Constants.Support.TALK_SUPPORT_POINTS <= 0)
                return;

            if (support_possible(Global.game_map.units[id]))
                display_support_gain(id);
        }

        internal void rescue_support_gain_display(int id)
        {
            // Only if rescuing gives support points
            if (Constants.Support.RESCUE_SUPPORT_POINTS <= 0)
                return;
            
            if (support_possible(Global.game_map.units[id]))
                display_support_gain(id);
        }

        private void display_support_gain(params int[] support_partners)
        {
            display_support_gain((IEnumerable<int>)support_partners);
        }
        private void display_support_gain(IEnumerable<int> support_partners)
        {
            if (!Constants.Support.PLAYER_SUPPORT_ONLY || is_player_team)
            {
                if (support_partners.Any())
                    Global.game_state.call_support_gain(Id, support_partners);
            }
        }
        #endregion

        internal bool check_magic_attack(Data_Weapon weapon)
        {
            return check_magic_attack(weapon, 1);
        }
        internal bool check_magic_attack(Data_Weapon weapon, int distance)
        {
            return weapon != null &&
                (weapon.is_staff() || weapon.is_always_magic() || (distance > 1 && weapon.is_ranged_magic()));
        }

        #region Items
        public void equip(int index)
        {
            if (actor.in_equip_range(index))
                actor.equip(index);
            else
            {
                actor.weapon_id = this.items[index - 1].Id;
            }
        }

        public bool is_weapon_broke()
        {
            return is_weapon_broke(0);
        }
        public bool is_weapon_broke(int uses)
        {
            // Weapons can't break in the arena, at least for this game
            if (Global.game_system.In_Arena)
                return false;
            // If nothing is directly equipped, for whatever reason
            if (actor.equipped == 0 && !Using_Siege_Engine)
                return false;
            Data_Weapon weapon = actor.weapon;
            if (weapon.Uses <= 0)
                return false;

            Item_Data item;
            if (Using_Siege_Engine)
            {
                if (Siege_Engine.SIEGE_INVENTORY_INDEX >= this.items.Count)
                    return true;
                item = this.items[Siege_Engine.SIEGE_INVENTORY_INDEX];
            }
            else
                item = items[actor.equipped - 1];
            return item.Uses - uses <= 0;
        }

        public List<int> weapon_indices(List<int> weapon_ids)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < items.Count; i++)
            {
                Item_Data item_data = items[i];
                if (item_data.is_weapon && weapon_ids.Contains(item_data.Id))
                    result.Add(i);
            }
            return result;
        }

        public bool can_acquire_drops
        {
            get
            {
                if (Constants.Gameplay.CITIZENS_GET_DROPS &&
                        is_player_allied &&
                        !this.actor.too_many_items)
                    return true;
                return is_player_team;
            }
        }

        public bool no_ai_item_trading
        {
            get
            {
                // Maybe add a property for preventing trading in general //Yeti
                if (this.berserk)
                    return true;

                if (Drops_Item || this.boss || !actor.is_generic_actor)
                    return true;
                return false;
            }
        }
        #endregion

        #region Unit stats/skills
        public bool has_canto()
        {
            if (canto_skill())
                return true;
            return false;
        }
        public bool has_attack_canto()
        {
            if (attack_canto_skill())
                return true;
            return false;
        }

        public bool has_cover()
        {
            if (cover_skill())
                return true;
            return false;
        }

        public bool has_refuge()
        {
            if (refuge_skill())
                return true;
            return false;
        }

        public int vision()
        {
            int base_range = Global.game_map.vision_range;
            if (Config.CLASS_VISION_BONUS.ContainsKey(actor.class_id))
                base_range += Config.CLASS_VISION_BONUS[actor.class_id];
            return Math.Max(1, vision_skill(base_range, Vision_Bonus));
        }

        public int vision_bonus { set { Vision_Bonus = Math.Max(Vision_Bonus, value); } }

        public override bool visible_by(int team)
        {
            if (!Global.game_map.fow)
                return true;
            if (!is_attackable_team(team))
                return true;
            if (!Global.game_map.fow_visibility[team].Contains(Loc))
            {
                // If the unit is moving and their last location in their move route was visible, check what tile they're closer to
                if (is_in_motion() && Global.game_map.fow_visibility[team].Contains(Prev_Move_Route_Loc))
                {
                    return (Real_Loc / UNIT_TILE_SIZE - Prev_Move_Route_Loc).LengthSquared() <=
                        (Real_Loc / UNIT_TILE_SIZE - Loc).LengthSquared();
                }
                else
                    return false;
            }
            else if (!Global.game_map.fow_visibility[team].Contains(Prev_Move_Route_Loc) && is_in_motion())
                return (Real_Loc / UNIT_TILE_SIZE - Prev_Move_Route_Loc).LengthSquared() >=
                    (Real_Loc / UNIT_TILE_SIZE - Loc).LengthSquared();
            return true;
        }

        public bool can_canto_move()
        {
            bool cantoing = Cantoing;
            Cantoing = true;
            var map = new Pathfinding.UnitMovementMap.Builder()
                .Build(Id);
            var pathfinder = map.Pathfind();
            HashSet<Vector2> locs = pathfinder.get_range(loc, loc, this.canto_mov);
            //HashSet<Vector2> locs = Pathfind.get_range(Loc, canto_mov, Id); //Debug
            Cantoing = cantoing;
            locs.Remove(Loc);
            return locs.Count > 0;
        }

        public bool can_dance()
        {
            if (this.actor.can_dance_skill())
                return true;
            return false;
        }

        public string dance_name()
        {
            string name = dance_name_skill();
            if (name.Length > 0)
                return name;
            return "Dance";
        }

        public List<int> dance_targets()
        {
            return dance_targets(false);
        }
        public List<int> dance_targets(bool refresh_only)
        {
            HashSet<int> units = units_in_range(1);
            List<int> result = new List<int>();
            foreach (int id in units)
            {
                Game_Unit unit = Global.game_map.units[id];
                if (same_team(unit) && ((!unit.ready && !unit.can_dance()) || (!refresh_only && has_dancer_ring())))
                    result.Add(id);
            }
            return result;
        }

        public bool has_dancer_ring()
        {
            foreach (Item_Data item_data in actor.items)
            {
                if (item_data.is_item && item_data.Id > 0)
                    if (item_data.to_item.Dancer_Ring)
                        return true;
            }
            return false;
        }

        public void apply_dance(Game_Unit target, int dance_item)
        {
            // apply status conditions, use up rings
            if (dance_item > -1)
            {
                Data_Item item = items[dance_item].to_item;
                List<KeyValuePair<int, bool>> state_change = new List<KeyValuePair<int, bool>>();
                foreach (int i in item.Status_Remove)
                {
                    state_change.Add(new KeyValuePair<int, bool>(i, false));
                }
                foreach (int i in item.Status_Inflict)
                {
                    state_change.Add(new KeyValuePair<int, bool>(i, true));
                }
                target.state_change(state_change);
                use_item(dance_item);
            }
        }

        public int dance_exp()
        {
            int exp_gain = (Constants.Combat.DANCE_EXP > 0 ?
                Math.Min(Constants.Combat.DANCE_EXP, actor.exp_gain_possible()) :
                Math.Max(Constants.Combat.DANCE_EXP, -actor.exp_loss_possible()));
            // If unit is not playable, don't allow level ups
            if (!is_player_team)
                exp_gain = Math.Min(exp_gain, Constants.Actor.EXP_TO_LVL -
                    (this.actor.exp + 1));
            actor.exp += exp_gain; // Constants.Combat.DANCE_EXP; //Debug
            return exp_gain;
        }

        public bool can_use_siege()
        {
            if (siege_skill())
                return true;
            return false;
        }

        public bool can_assemble()
        {
            // If no siege engines in the convoy and none in this actor's inventory
            List<Item_Data> convoy = null;
            if (Global.game_battalions.contains_convoy(Global.battalion.convoy_id))
                convoy = Global.game_battalions.convoy(Global.battalion.convoy_id);
            if (convoy == null || !convoy.Any(x => x.is_weapon && x.to_weapon.Ballista()))
                if (!this.actor.items.Any(x => x.is_weapon && x.to_weapon.Ballista()))
                    return false;

            if (this.actor.can_construct_skill())
                return true;
            return false;
        }

        public List<Vector2> assemble_targets()
        {
            List<Vector2> result = new List<Vector2>();
            foreach (Vector2 offset in new Vector2[] {
                    new Vector2(0, 1), new Vector2(0, -1),
                    new Vector2(1, 0), new Vector2(-1, 0) })
                if (!Global.game_map.is_off_map(offset + Loc))
                    if (!Global.game_map.is_blocked(offset + Loc, Rescuing))
                        if (Global.game_map.get_siege(offset + Loc) == null)
                            if (Global.game_map.terrain_cost(
                                    MovementTypes.Armor, offset + Loc) >= 0)
                                result.Add(offset + Loc);
            return result;
        }

        public bool can_reload()
        {
            if (!Constants.Gameplay.SIEGE_RELOADING)
                return false;

            if (this.actor.can_construct_skill())
                return true;
            return false;
        }

        public List<Vector2> reload_targets()
        {
            List<Vector2> result = new List<Vector2>();
            foreach (Vector2 offset in new Vector2[] {
                    new Vector2(0, 1), new Vector2(0, -1),
                    new Vector2(1, 0), new Vector2(-1, 0) })
            {
                var siege = Global.game_map.get_siege(offset + Loc);
                if (siege != null)
                {
                    if (!siege.is_ready && siege.has_ammo)
                        result.Add(offset + Loc);
                }
            }
            return result;
        }

        public bool can_reclaim()
        {
            // If no convoy or convoy is full
            if (!Global.game_battalions.contains_convoy(Global.battalion.convoy_id))
                return false;
            if (Global.battalion.is_convoy_full)
                return false;

            if (this.actor.can_construct_skill())
                return true;
            return false;
        }

        public List<Vector2> reclaim_targets()
        {
            List<Vector2> result = new List<Vector2>();
            foreach (Vector2 offset in new Vector2[] {
                    new Vector2(0, 0),
                    new Vector2(0, 1), new Vector2(0, -1),
                    new Vector2(1, 0), new Vector2(-1, 0) })
            {
                var siege = Global.game_map.get_siege(offset + Loc);
                if (siege != null)
                {
                    if (siege.has_ammo)
                        result.Add(offset + Loc);
                }
            }
            return result;
        }

        public List<Vector2> placeable_targets()
        {
            List<Vector2> result = new List<Vector2>();
            foreach (Vector2 offset in new Vector2[] {
                    new Vector2(0, 1), new Vector2(0, -1),
                    new Vector2(1, 0), new Vector2(-1, 0) })
                if (!Global.game_map.is_off_map(offset + Loc))
                    if (!Global.game_map.is_blocked(offset + Loc, Rescuing))
                        if (Global.game_map.get_siege(offset + Loc) == null)
                            if (Global.game_map.terrain_cost(
                                    MovementTypes.Armor, offset + Loc) >= 0)
                                result.Add(offset + Loc);
            return result;
        }

        public bool is_member()
        {
            // Skills: Member
            return actor.has_skill("MEMBER");
        }

        public bool continue_attacking()
        {
            if (astra_continue_attacking()) return true;
            return false;
        }

        public override void combat_damage(int dmg, Combat_Map_Object attacker, List<KeyValuePair<int, bool>> states, bool backfire, bool test)
        {
            base.combat_damage(dmg, attacker, states, backfire, test);
            state_change(states);

            if (is_player_team && dmg > 0 && !test)
                if (Global.game_state.is_battle_map && !Global.game_system.In_Arena)
                    Global.game_system.chapter_damage_taken += dmg;
        }

        public void state_change(List<KeyValuePair<int, bool>> states)
        {
            foreach (KeyValuePair<int, bool> state in states)
                if (state.Value)
                    actor.add_state(state.Key);
                else
                    actor.remove_state(state.Key);
        }

        public bool can_double_attack(Combat_Map_Object target, int distance, int? item_index = null)
        {
            return attacks_per_round(target, distance, item_index) > 1; //Yeti
        }
        public bool can_double_attack(Combat_Map_Object target, int distance, Data_Weapon weapon)
        {
            return attacks_per_round(target, distance, weapon) > 1; //Yeti
        }

        public int attacks_per_round(Combat_Map_Object target, int distance, int? item_index = null)
        {
            Item_Data item_data;
            if (item_index == null)
                item_data = new Item_Data(Item_Data_Type.Weapon, actor.weapon_id);
            else
                item_data = this.items[(int)item_index];

            if (item_data != null && item_data.is_weapon)
            {
                return attacks_per_round(target, distance, item_data.to_weapon);
            }
            else
            {
                int spd = atk_spd(distance, item_index == null ? -1 : (int)item_index);
                return attacks_per_round(spd, target, distance);
            }
        }
        public int attacks_per_round(Combat_Map_Object target, int distance, Data_Weapon weapon)
        {
            int spd = atk_spd(distance, weapon);
            if (weapon.Ballista() && Constants.Gameplay.SIEGE_RELOADING)
                return 1;
            return attacks_per_round(spd, target, distance);
        }
        private int attacks_per_round(int spd, Combat_Map_Object target, int distance)
        {
            if (target.is_unit() && !is_double_disabled())
            {
                int target_spd = (target as Game_Unit).atk_spd(distance);
                // If enough faster than the opponent, attack twice
                if (spd - Constants.Combat.DBL_ATK_SPD >= target_spd)
                    return 2;
            }
            return 1;
        }

        /// <summary>
        /// Called at the start of combat and each attack this unit is in.
        /// Should pass in attackIndex -1 for the start of combat.
        /// </summary>
        /// <param name="attackIndex">The index of the attack among attack this unit performs that is about to start.</param>
        public void start_attack(int attackIndex, Combat_Map_Object target)
        {
            start_attack_skills(attackIndex, target);
        }

        public void end_battle()
        {
            if (Using_Siege_Engine)
                actor.setup_items();
            if (Using_Siege_Engine)
                Global.game_map.get_siege(Loc).refresh_sprite();
            Using_Siege_Engine = false;
            actor.reset_skills(true);
            end_battle_skills();
        }

        protected bool is_correct_attack_type()
        {
            return is_correct_attack_type(Magic_Attack);
        }
        protected bool is_correct_attack_type(bool magic)
        {
            switch (actor.power_type())
            {
                case Power_Types.Strength:
                    return !magic;
                case Power_Types.Magic:
                    return magic;
            }
            return true;
        }

        public void add_exp_given(int exp)
        {
            Exp_Given += exp;
        }

        public int cap_exp_given(int expGain)
        {
            return Math.Min(
                expGain, Constants.Actor.EXP_PER_ENEMY - this.exp_given);
        }

        public int weapon_use_count(bool is_hit, Game_Unit target = null)
        {
            // Skills: Multishot // this should be moved to skills script probs //Debug
            if (Multishot_Activated)
            {
                if (Multishot_Attacked)
                    return 0;
                Multishot_Attacked = true;
            }
            int uses = actor.weapon_use_count(is_hit, is_player_team); // Overwritten by Astra, Thaumaturgy, and Multishot //Yeti
            // Corrode stuff
            return uses;
        }

        public void weapon_use()
        {
            if (Using_Siege_Engine)
                Global.game_map.get_siege(Loc).item.consume_use();
            else
                actor.weapon_use();
        }

        public bool mounted()
        {
            return Config.MOUNTED_CLASS_TYPES.Any(x => x.Types.Intersect(actor.actor_class.Class_Types).Count() == x.Types.Count); //Yeti
            //return actor.actor_class.Class_Types.Intersect(Config.MOUNTED_CLASS_TYPES.SelectMany(x => x.Types)).ToList().Count > 0;
        }

        public bool has_magic()
        {
            if (actor.power_type() != Power_Types.Strength)
                return true;
            for (int i = 0; i < items.Count; i++)
                if (items[i].is_weapon && Global.data_weapons[items[i].Id].is_magic())
                    return true;
            return false;
        }

        internal bool any_effective_weapons(Game_Unit target)
        {
            for (int i = 0; i < this.items.Count; i++)
            {
                if (actor.is_equippable(this.items, i) &&
                        this.items[i].to_weapon.effective_multiplier(this, target) > 1)
                    return true;
            }
            return false;
        }

        public void boss_hard_mode_bonuses()
        {
            if (!Boss)
                return;
            switch (actor.id)
            {
                // Tesla
                case 104:
                    actor.gain_stat(Stat_Labels.Hp, 2);
                    actor.gain_stat(Stat_Labels.Def, 1);
                    break;

                // Boston
                case 175:
                    actor.gain_stat(Stat_Labels.Pow, 2);
                    actor.gain_stat(Stat_Labels.Skl, 1);
                    actor.gain_stat(Stat_Labels.Spd, 1);
                    actor.gain_stat(Stat_Labels.Def, 3);
                    actor.gain_stat(Stat_Labels.Res, 1);
                    break;
                // Crane
                case 182:
                    actor.gain_stat(Stat_Labels.Hp, 6);
                    actor.hp += 6;
                    actor.gain_stat(Stat_Labels.Def, 2);
                    break;
                // Stephen
                case 183:
                    actor.gain_stat(Stat_Labels.Hp, 4);
                    actor.hp += 4;
                    actor.gain_stat(Stat_Labels.Pow, 3);
                    actor.gain_stat(Stat_Labels.Lck, 3);
                    actor.gain_stat(Stat_Labels.Def, 1);
                    actor.gain_stat(Stat_Labels.Res, 3);
                    break;
            }
        }

        public bool allowed_to_gain_exp()
        {
            // The unit has to be on a player team and not dead to gain exp
            if (is_dead)
                return false;
            if (!is_ally)
            {
                // Non-generic units on teams allied with the player can gain limited exp
                if (!Constants.Actor.CITIZENS_GAIN_EXP ||
                        this.actor.is_generic_actor ||
                        !is_player_allied)
                    return false;
            }
            else
            {
                // Generic units on the player team can only gain exp if they're in the battalion
                if (this.actor.is_generic_actor && (Global.battalion == null ||
                        !Global.battalion.actors.Contains(this.actor.id)))
                    return false;
            }
            return true;
        }

        public bool average_stat_hue_shown
        {
            get
            {
#if DEBUG
                return true;
#else
                return !Constants.Actor.ONLY_PC_STAT_COLORS || this.is_player_team;
#endif
            }
        }
        #endregion

        #region Range
        public int min_range()
        {
            return min_range(-1);
        }
        public int min_range(int item_index)
        {
            return min_range(item_index, "");
        }
        public int min_range(string skill)
        {
            return min_range(-1, skill);
        }
        public int min_range(int item_index, string skill)
        {
            // Equips weapon temporarily in case it gives range affecting skills
            int equipped = actor.equipped;
            Data_Weapon weapon = range_weapon(item_index);
            if (weapon == null) return 0;
            // Allow unequippable weapons if checking equipped weapon
            if (item_index != -1 && !actor.is_equippable_as_siege(weapon))
            {
                equip(equipped);
                return 0;
            }
            int min_range = weapon.Min_Range;
            min_range = min_range_skill(weapon, min_range);
            // Makes sure this range works with the given skill
            if (skill != "")
            {
                // Sets to 0 if min range doesn't work with this skill
                if (!valid_mastery_target(skill, null, min_range, weapon.Id))
                    min_range = 0;
            }
            equip(equipped);
            return min_range;
        }
        public int min_range(Data_Weapon weapon, string skill)
        {
            // Equips weapon temporarily in case it gives range affecting skills
            int equipped = actor.equipped;
            actor.weapon_id = weapon.Id;
            int min_range = weapon.Min_Range;
            min_range = min_range_skill(weapon, min_range);
            // Makes sure this range works with the given skill
            if (skill != "")
            {
                // Sets to 0 if min range doesn't work with this skill
                if (!valid_mastery_target(skill, null, min_range, weapon.Id))
                    min_range = 0;
            }
            equip(equipped);
            return min_range;
        }

        public int max_range()
        {
            return max_range(-1);
        }
        public int max_range(int item_index)
        {
            return max_range(item_index, "");
        }
        public int max_range(string skill)
        {
            return max_range(-1, skill);
        }
        public int max_range(int item_index, string skill)
        {
            // Equips weapon temporarily in case it gives range affecting skills
            int equipped = actor.equipped;
            Data_Weapon weapon = range_weapon(item_index);
            if (weapon == null)
                return 0;
            // Allow unequippable weapons if checking equipped weapon
            if (item_index != -1 && !actor.is_equippable_as_siege(weapon))
            {
                equip(equipped);
                return 0;
            }
            int max_range = weapon.Max_Range;
            if (weapon.Mag_Range)
                max_range = Math.Max(
                    mag_range_pow(weapon, weapon.is_always_magic()) / 2, max_range);
            max_range = max_range_skill(weapon, max_range);
            if (actor.silenced)
                if (weapon.imbue_range_reduced_by_silence)
                    max_range = 1;
            // Makes sure this range works with the given skill
            if (skill != "")
            {
                // Reduces max range until it works with the skill
                for (; ; )
                {
                    if (max_range == 0 || valid_mastery_target(skill, null, max_range, weapon.Id))
                        break;
                    else
                        max_range--;
                }
            }
            equip(equipped);
            return max_range;
        }
        public int max_range(Data_Weapon weapon, string skill)
        {
            // Equips weapon temporarily in case it gives range affecting skills
            int equipped = actor.equipped;
            actor.weapon_id = weapon.Id;
            int max_range = weapon.Max_Range;
            if (weapon.Mag_Range)
                max_range = Math.Max(
                    mag_range_pow(weapon, weapon.is_always_magic()) / 2, max_range);
            max_range = max_range_skill(weapon, max_range);
            // Makes sure this range works with the given skill
            if (skill != "")
            {
                // Reduces max range until it works with the skill
                for (; ; )
                {
                    if (max_range == 0 || valid_mastery_target(skill, null, max_range, weapon.Id))
                        break;
                    else
                        max_range--;
                }
            }
            equip(equipped);
            return max_range;
        }

        public int min_range_absolute()
        {
            return min_range_absolute(false);
        }
        public int min_range_absolute(bool staff)
        {
            return min_range_absolute(staff, "");
        }
        public int min_range_absolute(string skill)
        {
            return min_range_absolute(false, skill);
        }
        protected int min_range_absolute(bool staff, string skill)
        {
            int? min = null;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].is_weapon)
                    if (Global.data_weapons[items[i].Id].is_staff() ^ staff)
                        continue;
                // Get range
                int range = min_range(i, skill);
                // If range broke for this weapon, continue
                if (range == 0) continue;
                if (min == null) min = range;
                // Set min to lowest
                min = Math.Min((int)min, range);
            }
            if (min == null) min = 0;
            return (int)min;
        }

        public int max_range_absolute()
        {
            return max_range_absolute(false);
        }
        public int max_range_absolute(bool staff)
        {
            return max_range_absolute(staff, "");
        }
        public int max_range_absolute(string skill)
        {
            return max_range_absolute(false, skill);
        }
        protected int max_range_absolute(bool staff, string skill)
        {
            int? max = null;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].is_weapon)
                    if (Global.data_weapons[items[i].Id].is_staff() ^ staff)
                        continue;
                // Get range
                int range = max_range(i, skill);
                // If range broke for this weapon, continue
                if (range == 0) continue;
                if (max == null) max = range;
                // Set max to highest
                max = Math.Max((int)max, range);
            }
            if (max == null)
                max = 0;
            return (int)max;
        }

        protected Data_Weapon range_weapon(int item_index)
        {
            int weapon_id;
            if (item_index == -1)
            {
                // Use equipped weapon
                if (actor.weapon == null)
                    return null;
                item_index = actor.equipped - 1;
                weapon_id = actor.weapon_id;
            }
            else
            {
                if (!items[item_index].is_weapon) return null;
                weapon_id = items[item_index].Id;
            }
            if (!Global.data_weapons.ContainsKey(weapon_id))
                return null;
            equip(item_index + 1);
            return Global.data_weapons[weapon_id];
        }

        public bool can_counter(Game_Unit attacker, Data_Weapon weapon1, int distance)
        {
            return can_counter(attacker, weapon1, distance, -1);
        }
        public bool can_counter(Game_Unit attacker, Data_Weapon weapon1, int distance, int item_index)
        {
            if (weapon1.No_Counter)
                return false;
            if (disabled)
                return false;
            if (Global.game_system.In_Arena && !Global.scene.is_test_battle)
                return true;
            // Prevents Anticipation units who have unequipped intentionally from pulling out a weapon to counter
            if (actor.weapon == null)
                return false;
            // If skills block countering
            if (attacker.no_counter_skill())
                return false;
            Data_Weapon weapon2;
            if (item_index == -1)
                weapon2 = actor.weapon;
            else
            {
                if (!actor.is_equippable(item_index))
                    return false;
                weapon2 = Global.data_weapons[items[item_index].Id];
            }
            if (weapon2.No_Counter)
                return false;
            // Can't counter with staves
            if (weapon2.is_staff())
                return false;
            // Check unit range to attack distance
            if (distance > max_range(item_index))
                return false;
            if (distance < min_range(item_index))
                return false;
            return true;
        }
        public bool can_counter(Game_Unit attacker, Data_Weapon weapon1, List<int> distance)
        {
            if (weapon1.No_Counter) return false;
            if (disabled)
                return false;
            if (Global.game_system.In_Arena && !Global.scene.is_test_battle)
                return true;
            // Prevents Anticipation units who have unequipped intentionally from pulling out a weapon to counter
            if (actor.weapon == null)
                return false;
            // If skills block countering
            if (attacker.no_counter_skill())
                return false;
            Data_Weapon weapon2 = actor.weapon;
            if (weapon2.No_Counter) return false;
            // Can't counter with staves
            if (weapon2.is_staff()) return false;
            // Check unit range to attack distance
            foreach (int dist in distance)
            {
                if (dist > max_range())
                    return false;
                if (dist < min_range())
                    return false;
            }
            return true;
        }

        public bool has_uncounterable()
        {
            for (int i = 0; i < this.items.Count; i++)
            {
                if (actor.is_equippable(this.items, i) &&
                        this.items[i].to_weapon.No_Counter)
                    return true;
            }
            return false;
        }

        public HashSet<int> units_in_range(int range, List<int> teams = null)
        {
            HashSet<int> result = new HashSet<int>();
            for (int y = -1 * range; y <= range; y++)
            {
                if (Global.game_map.is_off_map(Loc + new Vector2(0, y)))
                    continue;
                for (int x = -1 * (range - Math.Abs(y)); x <= (range - Math.Abs(y)); x++)
                {
                    if (Global.game_map.is_off_map(Loc + new Vector2(x, y)))
                        continue;
                    Game_Unit other_unit = Global.game_map.get_unit(Loc + new Vector2(x, y));
                    if (other_unit != null)
                    {
                        if (teams == null || teams.Contains(other_unit.team))
                            result.Add(other_unit.id);
                    }
                }
            }
            result.Remove(Id);
            return result;
        }

        public List<int> allies_in_range(int range)
        {
            int[] group = new int[0];
            for (int j = 0; j < Constants.Team.TEAM_GROUPS.Length; j++)
            {
                group = Constants.Team.TEAM_GROUPS[j];
                if (group.Contains(Team))
                    break;
            }

            List<int> teams = new List<int>();
            foreach (int team in group)
                teams.Add(team);
            HashSet<int> allies = units_in_range(range, teams);
            // Remove units that are berserk
            HashSet<int> berserk_allies = new HashSet<int>();
            foreach (int id in allies)
                if (Global.game_map.units[id].berserk)
                    berserk_allies.Add(id);
            allies.ExceptWith(berserk_allies);
            return new List<int>(allies);
        }
        #endregion

        #region Determine if Moving
        public bool is_in_motion()
        {
            // If making an evented pre-defined move
            if (!evented_move_route_empty)
                return true;
            // If not exactly at the location being moved to, and, if rescued, we're not in the off map zone
            return (Move_Loc * UNIT_TILE_SIZE != Real_Loc &&
                (Rescued != 0 ? Loc != Config.OFF_MAP : true));
        }
        #endregion

        public bool highlight_test()
        {
            // Unless all of the kinds of busy
            if (!Global.game_state.no_highlight() && visible_by())
            {
                // If not ready invisible
                if (!Ready)
                {
                    highlighted = false;
                    // If cursor is on and no one selected
                    if (Loc == Global.player.loc && Global.game_system.Selected_Unit_Id == -1 && Global.game_state.is_player_turn &&
                        !((Scene_Map)Global.scene).changing_formation && !Global.game_temp.menuing)
                    {
                        show_quick_move_range();
                    }
                    return false;
                }
                else
                {
                    // If cursor is on and no one selected
                    if (Loc == Global.player.loc && Global.game_system.Selected_Unit_Id == -1 && Global.game_state.is_player_turn)
                    {
                        if (!Highlighted)
                            highlighted = true;
                        if (!((Scene_Map)Global.scene).changing_formation)
                            show_quick_move_range();
                    }
                    else
                        highlighted = false;
                    if (Highlighted)
                        Global.game_temp.highlighted_unit_id = Id;
                }
            }
            else
                highlighted = false;

            return Highlighted;
        }

        public bool is_ally
        {
            get
            {
                return this.team == Constants.Team.PLAYER_TEAM;
            }
        }
        public bool is_enemy
        {
            get
            {
                return this.team == Constants.Team.ENEMY_TEAM;
            }
        }
        public bool is_active_team
        {
            get
            {
                return this.team == Global.game_state.team_turn;
            }
        }
        public bool is_active_player_team
        {
            get
            {
                return is_active_team && Global.game_state.is_player_turn;
            }
        }
        public bool is_player_team
        {
            get
            {
                //return is_active_team && Global.game_state.is_player_turn;
                return Constants.Team.PLAYABLE_TEAMS.Contains(team);
            }
        }

        /// <summary>
        /// Player opposed teams.
        /// Used to count how many enemy units are remaining in rout chapters.
        /// </summary>
        public bool is_opposition
        {
            get
            {
                return team == Constants.Team.ENEMY_TEAM ||
                    team == Constants.Team.INTRUDER_TEAM;
            }
        }

        public bool is_passable_team(Map_Object test_unit)
        {
            if (!is_player_allied)
            {
                if (!test_unit.visible_by(Team) && !Global.game_state.is_player_turn)
                    return true;
            }
            else
            {
                if (!test_unit.visible_by(Team))
                    return true;
            }
            if (test_unit is Combat_Map_Object)
            {
                return !is_attackable_team((test_unit as Combat_Map_Object).team);
            }
            return true;
        }

        public bool same_team(Game_Unit unit)
        {
            return this.team == unit.team;
        }
        public bool different_team(Game_Unit unit)
        {
            return !same_team(unit);
        }

        public bool can_trade
        {
            get
            {
                bool has_items = actor.has_items;
                bool can_trade = false;
                // If rescuing, check for trading with rescued unit
                if (is_rescuing)
                {
                    Game_Unit rescued_unit = Global.game_map.units[Rescuing];
                    if ((has_items || rescued_unit.actor.has_items) && same_team(rescued_unit))
                        can_trade = true;
                }
                // If no rescued unit to trade with, check allies nearby
                if (!can_trade)
                {
                    foreach (int id in allies_in_range(1))
                    {
                        Game_Unit other_unit = Global.game_map.units[id];
                        if (different_team(other_unit))
                            continue;
                        if (other_unit.actor.has_no_items && !has_items)
                        {
                            // If target is rescuing, check their rescued unit for items too
                            if (other_unit.is_rescuing)
                            {
                                Game_Unit rescued_unit = Global.game_map.units[other_unit.rescuing];
                                if (rescued_unit.actor.has_no_items || same_team(rescued_unit))
                                    continue;
                            }
                            else
                                continue;
                        }
                        can_trade = true;
                        break;
                    }
                }
                return can_trade;
            }
        }

        public bool unselectable
        {
            get
            {
                if (uncontrollable)
                    return true;
                foreach (int id in actor.states)
                    if (Global.data_statuses[id].Unselectable)
                        return true;
                return false;
            }
        }

        public bool uncontrollable
        {
            get
            {
                foreach (int id in actor.states)
                    if (Global.data_statuses[id].Ai_Controlled)
                        return true;
                return false;
            }
        }

        public bool berserk
        {
            get
            {
                foreach (int id in actor.states)
                    if (Global.data_statuses[id].Attacks_Allies)
                        return true;
                return false;
            }
        }

        public bool disabled
        {
            get
            {
                foreach (int id in actor.states)
                    if (Global.data_statuses[id].Unselectable)
                        return true;
                return false;
            }
        }

        public bool lives_visible
        {
            get
            {
                // If unit is a PC, and they're not the convoy or their death causes a game over
                return is_ally && (!is_convoy() || loss_on_death);
            }
        }

        public bool loss_on_death
        {
            get
            {
                return Global.game_system.death_causes_loss(this);
            }
        }

        #region Update
        public void update()
        {
            if (!is_on_square)
                update_movement();
            if (Force_Move != null)
            {
                if (Enemy_Move_Wait <= 0)
                {
                    pathfind_move_to((Vector2)Force_Move);
                    Force_Move = null;
                }
                else if (!Global.game_map.scrolling)
                    Enemy_Move_Wait--;
            }
            update_map_animation(true);
            update_move_route();
            if (!is_in_motion() && move_route_empty && Evented_Move)
            {
                finish_evented_move();
            }
            if (!WAITING_FOR_SCROLL_BEFORE_MOVING &&
                    is_in_motion() && !Blocked && !Global.game_state.skip_ai_turn_active && !Global.game_map.changing_formation)
                update_move_sound();
        }

        private void finish_evented_move()
        {
            done_moving();
            Evented_Move = false;
            Ignore_Terrain_Cost = false;
            update_map_animation(true);
            refresh_sprite();
        }

        public Vector2 update_player_following_movement()
        {
            if (Global.player.following_unit_id != Id)
                throw new ArgumentException();
            return update_movement_destination();
            //return update_movement(true);
        }
        private void update_movement()
        {
            // If the player is following this unit, this breaks out if called from the normal location so that the cursor can call this instead
            // But I don't understand why at all, presumably something deadlocks but //Yeti
            // Oh it's because player updates before units, qq
            //if ((Global.player.following_unit_id == Id) != player_following) //Yeti
            //    return Vector2.Zero;

            // Skills : Trample
            if (Trample_Activated && Global.game_state.combat_active)
            { }
            // Maybe recode this to work during combat, only if this unit isn't fighting // Yeti
            else if ((Global.game_state.combat_active &&
                    (Global.game_state.battler_1_id == Id || Global.game_state.battler_2_id == Id || Global.game_state.aoe_targets.Contains(Id))) ||
                    Global.game_state.dance_active || Global.game_state.steal_active || Global.game_map.changing_formation)
            {
#if DEBUG
                if (Global.game_state.combat_active &&
                        !(Global.game_state.battler_1_id == Id || Global.game_state.battler_2_id == Id || Global.game_state.aoe_targets.Contains(Id)))
                    throw new Exception();
#endif
                return;
            }

            Real_Loc = update_movement_destination();
        }
        private Vector2 update_movement_destination()
        {
            int dist;
            // If skipping the AI turn, move instantly
            if (Global.game_state.skip_ai_turn_active)
                dist = Math.Abs((int)Real_Loc.X - (int)Loc.X * UNIT_TILE_SIZE) +
                    Math.Abs((int)Real_Loc.Y - (int)Loc.Y * UNIT_TILE_SIZE);
            else if (Global.game_state.combat_active)
                dist = (UNIT_TILE_SIZE / 16) * actor.move_speed;
            // An 8th of a tile/tick when rescuing
            else if (Global.game_state.rescue_active)
                dist = UNIT_TILE_SIZE / 8;
            // A 16th of a tile/tick, times movement speed
            else if (Evented_Move)
            {
                // Unless using a fixed speed from evented movement
                if (EventedMoveSpeed > 0)
                    dist = EventedMoveSpeed / 16;
                else
                    dist = (UNIT_TILE_SIZE / 16) * actor.move_speed;
            }
            else
                dist = (UNIT_TILE_SIZE / 16) * (speed_up_input() ? 4 : actor.move_speed);

            // Modify speed by game speed, unless moving during an event
            // Actually this takes effect during events too, wao //Debug
            //if (!Global.game_system.is_interpreter_running) //Debug
                dist *= (int)Math.Pow(2, Global.game_options.game_speed);
            // Has to move at least 1 <whatever the units are> per tick, or the unit could potentially never move and lock up the game
            dist = Math.Max(1, dist);
            return new Vector2(
                Additional_Math.int_closer((int)Real_Loc.X, (int)Loc.X * UNIT_TILE_SIZE, dist),
                Additional_Math.int_closer((int)Real_Loc.Y, (int)Loc.Y * UNIT_TILE_SIZE, dist));
        }

        public bool update_attack_graphics()
        {
            bool result = false;

            if (Attack_Movement.Count > 0)
            {
                Real_Loc += Attack_Movement.pop();
                result = true;
            }
            if (Wiggle_Movement.Count > 0)
            {
                Frame = Wiggle_Movement.pop();
                result = true;
            }
            if (Color_List.Count > 0)
            {
                Unit_Color.A = (byte)Color_List.pop();
                if (Color_List.Count == 0)
                    Unit_Color = Color.Transparent;
                result = true;
            }
            if (Opacity_List.Count > 0)
            {
                Opacity = Opacity_List.pop();
                result = true;
            }

            return result;
        }
        public void reset_attack_graphics()
        {
            while (update_attack_graphics()) { }
        }

        protected bool speed_up_input()
        {
            return Global.Input.pressed(Inputs.A) ||
                Global.Input.mouse_pressed(MouseButtons.Left, false) ||
                Global.Input.touch_pressed(false);
        }

        protected void update_frame()
        {
            Frame = Global.game_system.unit_anim_idle_frame;
        }

        public void update_map_animation()
        {
            update_map_animation(false);
        }
        public void update_map_animation(bool idle)
        {
            // Item use/staff use/new turn
            if (is_active_unit_sprite) { }
            // Battling
            // Skills: Trample
            else if (is_skill_unit_sprite) { }
            // Moving
            else if (is_moving_unit_sprite)
            {
                update_move_animation();
            }
            else if (is_highlighted_unit_sprite)
            {
                update_highlighted_animation();
            }
            // Idle
            else
            {
                update_idle_animation();
            }
        }

        protected bool is_active_unit_sprite
        {
            get
            {
                //Yeti
                if (!Sprite_Moving)
                {
                    if (Global.game_state.item_user == this)
                        return true;
                    if (Global.game_state.sacrificer_id == Id)
                        return true;
                    if (Global.game_state.staff_active &&
                            Global.game_state.battler_1_id == Id)
                        return true;
                }
                return (Global.game_state.new_turn_unit_id == Id && Facing == 6);
            }
        }
        protected bool is_skill_unit_sprite { get { return Battling && !(Trample_Activated && is_in_motion()); } }
        protected bool is_moving_unit_sprite { get { return Sprite_Moving; } }
        protected bool is_highlighted_unit_sprite
        {
            get
            {
                if (ForceHighlightTimer > 0)
                    return true;
                //return Highlighted && !unselectable && is_ally && !is_on_siege() && //Multi
                return Highlighted && !unselectable && is_active_player_team && !is_on_siege() && //Multi
                    !Global.game_system.preparations && Ready && Global.game_state.is_map_ready();
            }
        }

        protected void update_idle_animation()
        {
            Moving_Anim = -1;
            Highlighted_Anim = -1;
            Facing = 2;
            update_frame();
        }
        protected void update_highlighted_animation()
        {
            Moving_Anim = -1;
            Highlighted_Anim = (Highlighted_Anim + 1) % Global.game_system.Unit_Highlight_Anim_Time;
            Facing = 6;

            int anim_count = Highlighted_Anim;
            int index = 0;
            for (int i = 0; i < Config.CHARACTER_HIGHLIGHT_ANIM_TIMES.Length; i++)
            {
                if (anim_count < Config.CHARACTER_HIGHLIGHT_ANIM_TIMES[i])
                {
                    index = i;
                    break;
                }
                else
                    anim_count -= Config.CHARACTER_HIGHLIGHT_ANIM_TIMES[i];
            }
            Frame = Config.CHARACTER_HIGHLIGHT_ANIM_FRAMES[index];
        }
        protected void update_move_animation()
        {
            Moving_Anim = (Moving_Anim + 1) % Global.game_system.Unit_Moving_Anim_Time;
            Highlighted_Anim = -1;

            int anim_count = Moving_Anim;
            int index = 0;
            for (int i = 0; i < Config.CHARACTER_MOVING_ANIM_TIMES.Length; i++)
            {
                if (anim_count < Config.CHARACTER_MOVING_ANIM_TIMES[i])
                {
                    index = i;
                    break;
                }
                else
                    anim_count -= Config.CHARACTER_MOVING_ANIM_TIMES[i];
            }
            Frame = Config.CHARACTER_MOVING_ANIM_FRAMES[index];
        }

        public void update_move_sound()
        {
            // Skills: Trample
            if (((Trample_Activated && is_in_motion()) || !Battling) &&
                Global.game_system.Battler_1_Id != Id && Global.game_system.Battler_2_Id != Id)
            {
                string sound_name = "";
                int loop_time = 1;
                int delay_time = 1;

                if (actor.class_types.Contains(ClassTypes.FDragon))
                {
                    loop_time = 20;
                    switch (Move_Timer)
                    {
                        case 0:
                            sound_name = "FDragon";
                            delay_time = 20;
                            break;
                    }
                }
                else if (actor.class_types.Contains(ClassTypes.Flier))
                {
                    loop_time = 20;
                    switch (Move_Timer)
                    {
                        case 0:
                            sound_name = "Flier";
                            delay_time = 20;
                            break;
                    }
                }
                else if (actor.class_types.Contains(ClassTypes.Cavalry))
                {
                    loop_time = 21;
                    switch (Move_Timer)
                    {
                        case 0:
                            sound_name = "Mounted1";
                            delay_time = 3;
                            break;
                        case 3:
                            sound_name = "Mounted2";
                            delay_time = 7;
                            break;
                        case 10:
                            sound_name = "Mounted3";
                            delay_time = 11;
                            break;
                    }
                }
                else if (actor.class_types.Contains(ClassTypes.Infantry) ||
                    actor.class_types.Contains(ClassTypes.Heavy))
                {
                    loop_time = 16;
                    switch (Move_Timer)
                    {
                        case 0:
                            sound_name = "Infantry1";
                            delay_time = 8;
                            break;
                        case 8:
                            sound_name = "Infantry2";
                            delay_time = 8;
                            break;
                    }
                }
                else if (actor.class_types.Contains(ClassTypes.Armor))
                {
                    loop_time = 32;
                    switch (Move_Timer)
                    {
                        case 0:
                            sound_name = "Armor1";
                            delay_time = 16;
                            break;
                        case 16:
                            sound_name = "Armor2";
                            delay_time = 16;
                            break;
                    }
                }
                if (sound_name.Length > 0 && Global.game_map.move_sound_timers.Contains(0))
                {
                    Global.Audio.play_se("Map Sounds", "Map_Step_" + sound_name);
                    for (int i = 0; i < Global.game_map.move_sound_timers.Count; i++)
                    {
                        if (Global.game_map.move_sound_timers[i] <= 0)
                        {
                            Global.game_map.move_sound_timers[i] = delay_time;
                            break;
                        }
                    }
                }
                Move_Timer = (Move_Timer + 1) % loop_time;
            }
        }

        protected void update_move_route()
        {
            // If currently between squares and thus moving, wait until on a square before processing the next movement
            if (!is_on_square)
                return;
            // If the camera is following this unit // Is not following this unit? //Debug
            if (Global.game_system.Selected_Unit_Id == Id &&
                    Global.player.following_unit_id != Id)
                if (Global.game_map.scrolling)
                {
                    WAITING_FOR_SCROLL_BEFORE_MOVING = true;
                    return;
                }

#if DEBUG
            // If the unit breaks trying to get somewhere
            if (move_route_empty && evented_move_route_empty &&
                    is_in_motion() && Sprite_Moving && !Blocked)
                throw new IndexOutOfRangeException(string.Format(
                    "Moving unit \"{0}\" Id: {1}, Loc: {2} cannot reach destination {3}",
                    this.actor.name, Id, Loc, Move_Loc));
#endif
            // If the route isn't empty, process the next movement
            if (!move_route_empty || !evented_move_route_empty)
            {
                WAITING_FOR_SCROLL_BEFORE_MOVING = false;

                if (!move_route_empty)
                {
                    if (Global.game_state.skip_ai_turn_active)
                    {
                        while (!move_route_empty)
                            process_move_route();
                        refresh_real_loc();
                    }
                    else
                    {
                        if (Global.game_system.Selected_Unit_Id == Id)
                        {
                            Global.player.following_unit_id = Id;
                        }
                        process_move_route();
                    }

                }
                else if (!evented_move_route_empty)
                {
                    if (Global.game_system.Selected_Unit_Id == Id)
                    {
                        Global.player.following_unit_id = Id;
                    }
                    process_evented_move_route();

                    if (evented_move_route_empty)
                    {
                        ForceHighlightTimer = 0;
                        Evented_Move = false;
                        sprite_moving = false;
                        fix_unit_location();
                    }
                }

                if (Global.game_system.Selected_Unit_Id == Id)
                    //Global.player.force_loc(Loc, true);
                    Global.player.loc = Loc;
                if (move_route_empty && !Evented_Move && Global.game_state.rescue_active)
                    Ignore_Terrain_Cost = false;
            }
        }

        private void process_move_route()
        {
            Prev_Move_Route_Loc = Loc;
            remove_old_unit_location();
            // Test tile being moved into
            Vector2 dir = Move_Route.pop();
            Game_Unit other_unit = Global.game_map.is_off_map(Loc + dir) ? null : Global.game_map.get_unit(Loc + dir);
            if (!fow_blocked_test(other_unit))
                Loc += dir;
            Facing = facing_from_direction(dir);
            /*if (dir == new Vector2(0, 1)) //Debug
                Facing = 2;
            else if (dir == new Vector2(-1, 0))
                Facing = 4;
            else if (dir == new Vector2(1, 0))
                Facing = 6;
            else if (dir == new Vector2(0, -1))
                Facing = 8;*/

            fix_unit_location();
        }

        private void process_evented_move_route()
        {
            if (EventedMoveRoute.Count == 0)
            {
                EventedMoveRoute = null;
                return;
            }

            Prev_Move_Route_Loc = Loc;
            remove_old_unit_location();

            var move_code = EventedMoveRoute[0];
            EventedMoveRoute.RemoveAt(0);
            switch (move_code.Item1)
            {
                case EventedMoveCommands.Move:
                    int facing = (int)move_code.Item2;
                    Facing = facing;
                    Vector2 dir = vector_from_facing(facing);
                    Loc += dir;
                    Move_Loc = Loc;
                    break;
                case EventedMoveCommands.Highlight:
#if DEBUG
                    throw new Exception();
#endif
                    ForceHighlightTimer = (int)move_code.Item2;
                    break;
                case EventedMoveCommands.Notice:
#if DEBUG
                    throw new Exception();
#endif
                    break;
                case EventedMoveCommands.SetSpeed:
                    EventedMoveSpeed = -1;
                    if (move_code.Item2 > 0)
                        EventedMoveSpeed = Math.Max(1, (int)(move_code.Item2 * UNIT_TILE_SIZE));
                    break;
            }
        }

        protected bool fow_blocked_test(Game_Unit other_unit)
        {
            if (!Evented_Move && other_unit != null)
            {
                if (is_passable_team(other_unit) && !other_unit.visible_by(Team))
                {
                    Global.game_state.call_block(Id);
                    Blocked = true;
                    Move_Route.Clear();
                    Global.game_temp.unit_menu_call = false;
                    Global.game_temp.menu_call = false;
                    Global.player.instant_move = true;
                    Global.player.loc = Loc;
                    return true;
                }
            }
            return false;
        }

        internal void drop_fow_blocked(Game_Unit other_unit)
        {
            Global.game_state.call_block(Id);
            Blocked = true;
            Global.game_temp.unit_menu_call = false;
            Global.game_temp.menu_call = false;
            Global.player.instant_move = true;
            Global.player.loc = Loc;
        }

        private static int facing_from_direction(Vector2 dir)
        {
            if (dir.Y == 0)
            {
                return dir.X > 0 ? 6 : 4;
            }
            else if (dir.X == 0)
            {
                return dir.Y > 0 ? 2 : 8;
            }
            return 0;
        }

        private static Vector2 vector_from_facing(int facing)
        {
            switch (facing)
            {
                case 1:
                    return new Vector2(-1,  1);
                case 2:
                    return new Vector2( 0,  1);
                case 3:
                    return new Vector2( 1,  1);
                case 4:
                    return new Vector2(-1,  0);
                case 6:
                    return new Vector2( 1,  0);
                case 7:
                    return new Vector2(-1, -1);
                case 8:
                    return new Vector2( 0, -1);
                case 9:
                    return new Vector2( 1, -1);
            }
            return Vector2.Zero;
        }
        #endregion

        #region Position Fix
        public override void force_loc(Vector2 loc)
        {
            remove_old_unit_location(true);
            base.force_loc(loc);
            Move_Loc = Loc;
            fix_unit_location();
            if (Evented_Move)
            {
                Sprite_Moving = false;
                Move_Route.Clear();
                finish_evented_move();
            }
            if (Global.player.is_following_unit &&
                Global.player.following_unit_id == id)
            {
                Global.player.force_loc(loc_on_map());
                Global.player.update_movement();
            }
        }

        public void remove_old_unit_location(bool warped = false)
        {
            Vector2 loc = warped && is_in_motion() ? Move_Loc : Loc;
            // return if no map //Yeti
            //if (loc.X >= 0 && loc.X < Global.game_map.width() && loc.Y >= 0 && loc.Y < Global.game_map.height())

            if (!Global.game_map.is_off_map(loc, false)) // Whoops, checking only playable isn't actually a good idea //Debug
                Global.game_map.clear_unit_location(this, loc);
        }

        public void fix_unit_location(bool spawning = false)
        {
            fix_unit_location(spawning, Loc);
        }
        public void fix_unit_location(bool spawning, Vector2 loc)
        {
            //if (loc.X >= 0 && loc.X < Global.game_map.width() && loc.Y >= 0 && loc.Y < Global.game_map.height() && (loc == Move_Loc || !Evented_Move)) //Debug

            if (!Global.game_map.is_off_map(loc, false)) // Whoops, checking only playable isn't actually a good idea //Debug
                if (loc == Move_Loc || !Evented_Move)
                    if (Global.game_map.no_unit_at_location(loc) || (loc == Move_Loc && !spawning))
                    {
                        if (!is_rescued)
                            Global.game_map.fix_unit_location(this, loc);
                    }
        }
        #endregion

        #region Move Range
        public void update_move_range()
        {
            update_move_range(Loc, Loc);
        }
        private void update_move_range(Vector2 loc, Vector2 baseLoc)
        {
            // Use locks for thread safety? //Yeti
            var map = new Pathfinding.UnitMovementMap.Builder()
                .Build(Id);
            var pathfinder = map.Pathfind();
            Move_Range = pathfinder.get_range(baseLoc, loc, this.canto_mov);
            //Move_Range = new HashSet<Vector2>( //Debug
            //    Pathfind.get_range(loc, canto_mov, Id, baseLoc)); //HashSet
            //Move_Range.AddRange(Pathfinding.get_range(Loc, canto_mov, Id));
            //Move_Range = Move_Range.Distinct().ToList(); //ListOrEquals
            clear_attack_range();
            // Update talk range
            HashSet<Vector2> talk_range = new HashSet<Vector2>();
            foreach (int id in Global.game_map.units.Keys)
                if (Global.game_map.units[id].visible_by() && Global.game_state.can_talk(Id, id))
                    talk_range.Add(Global.game_map.units[id].loc);
            // Add visit, seize, and escape locations for players
            if (is_player_team)
            {
                foreach (var visit in Global.game_map.visit_locations)
                    if (Move_Range.Contains(visit.Key))
                    {
                        // Skip places that have already been visited, so the player doesn't have to worry about them
                        if (!Global.game_map.already_visited(visit.Key))
                            talk_range.Add(visit.Key);
                    }
                if (this.can_seize)
                    foreach (var seize in Global.game_map.get_seize_points(Team, Group))
                        if (Move_Range.Contains(seize))
                            talk_range.Add(seize);

                if (this.can_seize)
                    foreach (Vector2 escape in Global.game_map.escape_point_locations(Team, Group))
                        if (Move_Range.Contains(escape))
                            talk_range.Add(escape);
            }
            Talk_Range = talk_range;
            Global.game_map.add_updated_move_range(Id);
        }

        public void clear_attack_range()
        {
            Attack_Range.Clear();
            Staff_Range.Clear();
            Global.game_map.remove_updated_attack_range(Id);
            Global.game_map.remove_updated_staff_range(Id);
        }

        public void update_attack_range()
        {
            if (Attack_Range.Count > 0)
                return;
            if (is_rescued)
            {
                Attack_Range.Clear();
                return;
            }
            // Use locks for thread safety? //Yeti
            Attack_Range = new HashSet<Vector2>(
                get_weapon_range(actor.useable_weapons()));
            //Attack_Range.AddRange(get_weapon_range(actor.useable_weapons()));
            // Siege Engines
            if (can_use_siege())
            {
                HashSet<Vector2> move_range = Global.game_map.remove_blocked(this.move_range, Id, true);
                foreach (Siege_Engine siege in Global.game_map.siege_engines.Values)
                    if (siege.is_ready && move_range.Contains(siege.loc) &&
                        actor.is_equippable_as_siege(Global.data_weapons[siege.item.Id]) &&
                        !Global.data_weapons[siege.item.Id].is_staff())
                    {
                        Maybe<int> move_distance = Pathfind.get_distance(siege.loc, Id, canto_mov, false, Loc);
                        if (move_distance.IsSomething)
                        {
                            set_ai_base_loc(siege.loc, move_distance);

                            // Adds siege engine's range to the attack range
                            int min = min_range(Siege_Engine.SIEGE_INVENTORY_INDEX);
                            int max = max_range(Siege_Engine.SIEGE_INVENTORY_INDEX);
                            var siege_range = Global.game_map.get_unit_range(
                                new HashSet<Vector2> { Loc }, min, max,
                                Global.data_weapons[siege.item.Id].range_blocked_by_walls());
                            Attack_Range = new HashSet<Vector2>(
                                siege_range.Union(Attack_Range));

                            reset_ai_loc();
                        }
                    }
            }
            Global.game_map.add_updated_attack_range(Id);
        }

        public void update_staff_range()
        {
            if (Staff_Range.Count > 0)
                return;
            if (is_rescued)
            {
                Staff_Range.Clear();
                return;
            }
            // Use locks for thread safety? //Yeti
            Staff_Range = new HashSet<Vector2>(
                get_weapon_range(actor.useable_staves()));
            //Staff_Range.AddRange(get_weapon_range(actor.useable_staves()));
            // Siege Engines
            if (can_use_siege())
            {
                HashSet<Vector2> move_range = Global.game_map.remove_blocked(this.move_range, Id, true);
                foreach (Siege_Engine siege in Global.game_map.siege_engines.Values)
                    if (siege.is_ready && move_range.Contains(siege.loc) &&
                        actor.is_equippable_as_siege(Global.data_weapons[siege.item.Id]) &&
                        Global.data_weapons[siege.item.Id].is_staff())
                    {
                        Maybe<int> move_distance = Pathfind.get_distance(siege.loc, Id, canto_mov, false, Loc);
                        if (move_distance.IsSomething)
                        {
                            set_ai_base_loc(siege.loc, move_distance);

                            int min = min_range(Siege_Engine.SIEGE_INVENTORY_INDEX);
                            int max = max_range(Siege_Engine.SIEGE_INVENTORY_INDEX);
                            var siege_range = Global.game_map.get_unit_range(
                                new HashSet<Vector2> { Loc }, min, max,
                                Global.data_weapons[siege.item.Id].range_blocked_by_walls());
                            Staff_Range = new HashSet<Vector2>(
                                siege_range.Union(Staff_Range));

                            reset_ai_loc();
                        }
                    }
            }
            //Staff_Range = Staff_Range.Distinct().ToList(); //ListOrEquals
            Global.game_map.add_updated_staff_range(Id);
        }

        protected HashSet<Vector2> get_weapon_range(List<int> useable_weapons)
        {
            return get_weapon_range(useable_weapons, Global.game_map.remove_blocked(this.move_range, Id, true));
        }
        public HashSet<Vector2> get_weapon_range(List<int> useable_weapons, HashSet<Vector2> move_range)
        {
            return get_weapon_range(useable_weapons, Global.game_map.remove_blocked(move_range, Id, true), "");
        }
        public HashSet<Vector2> get_weapon_range(List<int> useable_weapons, HashSet<Vector2> move_range, string skill)
        {
            HashSet<Vector2> weapon_range = new HashSet<Vector2>();
            weapon_range.UnionWith(get_weapon_range(useable_weapons, move_range, skill, false)); //HashSet
            weapon_range.UnionWith(get_weapon_range(useable_weapons, move_range, skill, true)); //HashSet
            //weapon_range = weapon_range.Distinct().ToList(); //ListOrEquals //HashSet
            return weapon_range;
        }
        protected HashSet<Vector2> get_weapon_range(List<int> useable_weapons, HashSet<Vector2> move_range, string skill, bool walls)
        {
            HashSet<Vector2> weapon_range = new HashSet<Vector2>();
            int min, max;
            // Get a list of all range distances the unit can attack from
            List<int> ranges = new List<int>();
            foreach (int useable_weapon in useable_weapons)
            {
                // Weapon ability to shoot through walls needs to match method
                if (items[useable_weapon].to_weapon.range_blocked_by_walls() == walls)
                {
                    min = min_range(useable_weapon, skill);
                    max = max_range(useable_weapon, skill);
                    for (int i = min; i <= max; i++)
                        ranges.Add(i);
                }
            }
            if (ranges.Count == 0)
                return weapon_range;

            ranges = ranges.Distinct().ToList(); //ListOrEquals
            ranges.Sort();

            Dictionary<int, int> range_types = new Dictionary<int, int>();
            min = max = ranges[0];
            for (int i = 1; i < ranges.Count; i++)
            {
                int range = ranges[i];
                // If 1 greater than the previous max, increase the max
                if (range - 1 == max)
                    max = range;
                // Else there is a gap, save the range and start a new one
                else
                {
                    range_types[min] = max;
                    min = range;
                    max = range;
                }
            }
            range_types[min] = max;

            // Checks range types to get squares in range
            foreach (KeyValuePair<int, int> pair in range_types)
            {
                weapon_range.UnionWith(Global.game_map.get_unit_range(move_range, pair.Key, pair.Value, walls));
            }

            return weapon_range;
        }

        public HashSet<Vector2> hit_from_loc(Vector2 target_loc, HashSet<Vector2> move_range, int weapon_index, string skill)
        {
            HashSet<Vector2> result = new HashSet<Vector2>();
            foreach (Vector2 loc in move_range)
            {
                if (get_weapon_range(new List<int> { weapon_index }, new HashSet<Vector2> { loc }, skill).Contains(target_loc))
                    result.Add(loc);
            }
            return result;
        }

        protected void show_quick_move_range()
        {
            if (Global.game_options.range_preview == 0)
            {
                Global.game_map.clear_move_range();
                Global.game_map.show_move_range(Id);
                Global.game_map.remove_updated_attack_range(Id);
                Global.game_map.remove_updated_staff_range(Id);
                Global.game_map.show_attack_range(Id);
                Global.game_map.range_start_timer = 15;
            }
        }

        public int move_cost(Vector2 target_loc)
        {
            int cost = Global.game_map.terrain_cost(this, target_loc);
            // Skill Alias
            cost = move_cost_skill(target_loc, cost);
            return Ignore_Terrain_Cost ? ((cost > 0 || Global.game_state.rescue_active) ? 1 : cost) : cost;
        }

        internal void queue_move_range_update()
        {
            Global.game_map.add_unit_move_range_update(this);
        }
        internal void queue_move_range_update(Game_Unit rescuer)
        {
            Global.game_map.add_unit_move_range_update(this, rescuer.rescue_drop_loc);
        }

        internal bool can_move_to(Vector2 loc, HashSet<Vector2> moveRange)
        {
            // Skills: Dash
            if (DashActivated)
                if (loc == Loc)
                    return false;

            if (Global.game_map.get_light_rune(loc) != null)
                return false;

            return moveRange.Contains(loc);
        }
        #endregion

        #region Facing
        public int angle(Vector2 target_loc)
        {
            Vector2 dir_vector = target_loc - Loc;
            if (dir_vector.Length() == 0) return 270;
            double angle = Math.Atan2(dir_vector.Y, dir_vector.X);
            int result = ((int)(360 - angle * 360 / MathHelper.TwoPi)) % 360;
            return result;
        }

        public void face(Map_Object target)
        {
            face(target.loc_on_map());
        }
        public void face(Vector2 target_loc)
        {
            int deg = this.angle(target_loc);
            if (deg > 45 && deg < 135)
                Facing = 8;
            else if (deg >= 135 && deg <= 225)
                Facing = 4;
            else if (deg > 225 && deg < 315)
                Facing = 2;
            else
                Facing = 6;
        }
        #endregion

        #region Menu Actions
        public bool can_visit()
        {
            if (!can_visit_skill())
                return false;
            return Global.game_map.visit_locations.ContainsKey(Loc);
        }

        public bool can_talk()
        {
            if (!can_talk_skill())
                return false;
            return talk_targets().Count > 0;
        }

        public List<int> talk_targets()
        {
            HashSet<int> units = units_in_range(1);
            List<int> result = new List<int>();
            foreach (int id in units)
                if (Global.game_state.can_talk(Id, id))
                    result.Add(id);
            return result;
        }

        public bool can_support()
        {
            if (!can_support_skill())
                return false;
            return support_targets().Count > 0;
        }

        public List<int> support_targets()
        {
            HashSet<int> units = units_in_range(1);
            List<int> result = new List<int>();
            foreach (int id in units)
                if (actor.is_support_ready(Global.game_map.units[id].actor.id))
                    if (Global.game_map.units[id].can_support_skill())
                        result.Add(id);
            return result;
        }

        public bool is_convoy()
        {
            if (Constants.Gameplay.TEAM_LEADER_CONVOY)
                if (Global.game_map.team_leaders[Team] == Id)
                    return true;
            return Global.battalion.convoy_id == ActorId;
        }
        public bool can_supply()
        {
            if (is_convoy())
                return true;
            HashSet<int> units = units_in_range(1);
            foreach (int id in units)
                if (!is_attackable_team(Global.game_map.units[id]) &&
                        Global.game_map.units[id].is_convoy())
                    return true;
            return false;
        }

        public void use_item(int index)
        {
            if (Using_Siege_Engine)
                Global.game_map.get_siege(Loc).item.consume_use();
            else
                if (!this.items[index].infinite_uses)
                    this.items[index].consume_use();
            if (this.items[index].out_of_uses)
                if (index < Constants.Actor.NUM_ITEMS) // Ensures this is working right for siege
                    actor.remove_broken_items();
        }

        public bool can_open_chest()
        {
            return can_open_chest(false);
        }
        public bool can_open_chest(bool canto_allowed)
        {
            if (!canto_allowed && Cantoing)
                return false;
            if (chest_open_skill())
                return true;
            return this.items
                .Any(x => !x.non_equipment && x.is_item &&
                    x.to_item.Chest_Key && actor.is_useable(x.to_item));
        }

        public void use_chest_key()
        {
            Maybe<int> skill = chest_key_skill();
            if (skill.IsSomething)
            {
                if (skill > 0)
                    use_item(skill);
                return;
            }
            // Gets indices of all chest keys
            var chest_key_indices = Enumerable.Range(0, this.items.Count)
                .Where(x => !this.items[x].non_equipment && this.items[x].is_item &&
                    this.items[x].to_item.Chest_Key && actor.is_useable(this.items[x].to_item));
#if DEBUG
            // Uh oh
            if (!chest_key_indices.Any())
                throw new IndexOutOfRangeException("Trying to consume chest key uses,\nbut unit has no chest keys");

#endif
            int chest_key_index = -1;
            // First check for keys with infinite uses
            if (chest_key_indices.Any(x => this.items[x].infinite_uses))
                chest_key_index = chest_key_indices.First(x => this.items[x].infinite_uses);
            // Otherwise just get the key that has the lowest value per use
            else
            {
                int min_cost = chest_key_indices.Min(x => this.items[x].cost);
                chest_key_index = chest_key_indices.First(x => this.items[x].cost == min_cost);
            }
            if (chest_key_index >= 0)
                use_item(chest_key_index);



            return;
            // Why does this use < Constants.Actor.NUM_ITEMS, but check the items list which includes siege? //Debug
            for (int i = 0; i < Constants.Actor.NUM_ITEMS; i++)
                if (items[i].is_item && items[i].Id > 0)
                    if (items[i].to_item.Chest_Key && actor.is_useable(items[i].to_item))
                        if (items[i].Uses > 0)
                        {
                            use_item(i);
                            return;
                        }
        }

        public bool can_open_door()
        {
            return can_open_door(false);
        }
        public bool can_open_door(bool canto_allowed)
        {
            if (!canto_allowed && Cantoing)
                return false;
            if (door_open_skill())
                return true;
            return this.items.Any(x => !x.non_equipment && x.is_item &&
                x.to_item.Door_Key && actor.is_useable(x.to_item));
        }

        public void use_door_key()
        {
            Maybe<int> skill = door_key_skill();
            if (skill.IsSomething)
            {
                if (skill > 0)
                    use_item(skill); // Discard item, not use item? //Debug
                return;
            }
            // Gets indices of all door keys
            var door_key_indices = Enumerable.Range(0, this.items.Count)
                .Where(x => !this.items[x].non_equipment && this.items[x].is_item &&
                    this.items[x].to_item.Door_Key && actor.is_useable(this.items[x].to_item));
#if DEBUG
            // Uh oh
            if (!door_key_indices.Any())
                throw new IndexOutOfRangeException("Trying to consume door key uses,\nbut unit has no door keys");

#endif
            int door_key_index = -1;
            // First check for keys with infinite uses
            if (door_key_indices.Any(x => this.items[x].infinite_uses))
                door_key_index = door_key_indices.First(x => this.items[x].infinite_uses);
            // Otherwise just get the key that has the lowest value per use
            else
            {
                int min_cost = door_key_indices.Min(x => this.items[x].cost);
                door_key_index = door_key_indices.First(x => this.items[x].cost == min_cost);
            }
            if (door_key_index >= 0)
                use_item(door_key_index);



            return;
            for (int i = 0; i < Constants.Actor.NUM_ITEMS; i++)
                if (items[i].is_item && items[i].Id > 0)
                    if (items[i].to_item.Door_Key && actor.is_useable(items[i].to_item))
                        if (items[i].Uses > 0)
                        {
                            use_item(i);
                            return; //Debug
                        }
        }

        public List<Vector2> door_targets()
        {
            List<Vector2> result = new List<Vector2>(Global.game_map.door_locations
                .Where(x => Math.Abs(x.Key.X - loc.X) + Math.Abs(x.Key.Y - loc.Y) == 1)
                .Select(x => x.Key));
            return result;
        }

        public bool can_steal()
        {
            // Skills: Steal
            return actor.has_skill("STEAL");
        }

        public List<int> steal_targets()
        {
            HashSet<int> units = units_in_range(1);
            List<int> result = new List<int>();
            foreach (int id in units)
            {
                Game_Unit other_unit = Global.game_map.units[id];
                if (is_attackable_team(other_unit))
                    if (can_steal_from(other_unit))
                        result.Add(id);
            }
            return result;
        }

        public bool can_steal_from(Game_Unit other_unit)
        {
            // Compares speed
            if (stat(Stat_Labels.Spd) < other_unit.stat(Stat_Labels.Spd))
                return false;
            // Tests if target has anything to steal
            for (int i = 0; i < other_unit.actor.items.Count; i++)
                if (can_steal_item(other_unit, i))
                    return true;
            return false;
        }

        public bool can_steal_item(Game_Unit target, int i)
        {
            if (target.actor.equipped == i + 1)
                return false;
            Item_Data item_data = target.actor.items[i];
            if (item_data.Id == 0)
                return false;
            if (item_data.is_weapon && item_data.to_weapon.Rank == Weapon_Ranks.None)
                return false;
            return atk_spd(1, item_data) >= target.stat(Stat_Labels.Spd);
        }

        public bool can_seize
        {
            get
            {
                return Constants.Gameplay.MAIN_CHARACTER.Contains(actor.id) ||
                    Global.game_map.team_leaders[Team] == Id;
            }
        }

        public bool can_heal_self()
        {
            return can_heal_self(this);
        }
        public bool can_heal_self(Game_Unit unit_to_heal)
        {
            var items = this == unit_to_heal ? this.items : actor.items;
            // If variable unit is not self, it checks if another unit can use the item to heal themselves (for AI looking to trade, for example)
            for (int i = 0; i < items.Count; i++)
            {
                Item_Data item_data = items[i];
                if (item_data.is_item && item_data.Id > 0)
                {
                    if (unit_to_heal.valid_healing_item(item_data))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool valid_healing_item(Item_Data itemData)
        {
            var item = itemData.to_item;

            if (item.can_heal(actor.is_full_hp(),
                actor.negative_states))
            {
                // If this item heals statuses,
                // can't heal any statuses on unit,
                // and wouldn't heal this unit out of critical health
                if (item.Status_Remove.Count > 0 &&
                        !item.can_heal(true, actor.negative_states) &&
                        !actor.would_heal_above_critcal(item))
                    return false;
                return Combat.can_use_item(this, itemData.Id);
            }
            return false;
        }

        public int heal_self_item()
        {
            return heal_self_item(this);
        }
        public int heal_self_item(Game_Unit unit)
        {
            var items = this == unit ? this.items : actor.items;
            // If unit is not self, it checks if another unit can use the item to heal themselves (for AI looking to trade, for example)
            List<int> healing_items = new List<int>();
            for (int i = 0; i < items.Count; i++)
            {
                Item_Data item_data = items[i];
                if (item_data.is_item && item_data.Id > 0)
                {
                    if (unit.valid_healing_item(item_data))
                            healing_items.Add(i);
                }
            }
            if (healing_items.Count == 0)
                return -1;
            healing_items.Sort(delegate(int a, int b)
            {
                return items[b].to_item.Cost - items[a].to_item.Cost;
            });
            return healing_items[0];
        }

        public bool could_use_torch()
        {
            return Vision_Bonus <= 0 && Global.game_map.fow;
        }

        public bool has_torch()
        {
            return torch_item() > -1;
        }

        public int torch_item()
        {
            for (int i = 0; i < items.Count; i++)
            {
                Item_Data item_data = items[i];
                if (item_data.is_item && item_data.Id > 0)
                    if (item_data.to_item.Torch_Radius > 0)
                        return i;
            }
            return -1;
        }
        #endregion

        #region Rescuing
        public void rescue_ally(int ally_id)
        {
            set_rescue(State.Rescue_Modes.Rescue, Id, ally_id);
            Rescue_Drop_Loc = Global.game_map.units[ally_id].loc;
        }
        public void take_ally(int ally_id)
        {
            set_rescue(State.Rescue_Modes.Take, Id, ally_id);
        }
        public void give_ally(int ally_id)
        {
            set_rescue(State.Rescue_Modes.Give, ally_id, Id);
        }
        protected void set_rescue(State.Rescue_Modes mode, int rescuer_id, int rescuee_id)
        {
            Global.game_state.call_rescue(mode, rescuer_id, rescuee_id, Move_Loc);
        }

        public void get_rescued(int ally_id)
        {
            pathfind_move_to(Global.game_map.units[ally_id].loc, false, rescue_move: true);
            update_map_animation(true);
            update_move_route();
            if (!is_on_square)
                update_movement();
        }

        public void drop_ally(Vector2 loc)
        {
            Rescue_Drop_Loc = loc;
            set_rescue(State.Rescue_Modes.Drop, Id, Rescuing);
            //Global.player.loc = Move_Loc; //Debug
            //Global.player.instant_move = true;
            //Global.game_map.rescue_calling = State.Rescue_Modes.Drop;
            //Global.game_system.Rescuer_Id = Id;
            //Global.game_system.Rescuee_Id = Rescuing;
        }

        public void get_dropped(Vector2 loc)
        {
            pathfind_move_to(loc, false, rescue_move: true);
            update_map_animation(true);
            update_move_route();
            if (!is_on_square)
                update_movement();
        }

        public bool can_drop()
        {
            return drop_locs().Count > 0;
        }
        public List<Vector2> drop_locs()
        {
            if (!is_rescuing)
                return new List<Vector2>();

            List<Vector2> result = new List<Vector2>();
            foreach (Vector2 offset in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) })
                if (!Global.game_map.is_off_map(offset + Loc))
                    if (!Global.game_map.is_blocked(offset + Loc, Rescuing))
                        if (Pathfind.passable(Global.game_map.units[Rescuing], offset + Loc))
                            result.Add(offset + Loc);
            return result;
        }

        public bool is_rescue_blocked()
        {
            return !can_rescue_skill();
        }

        public bool is_rescuing { get { return Rescuing > 0; } }

        public bool is_weighted_by_ally
        {
            get
            {
                if (weighted_by_ally_skill())
                    return false;
                return Rescuing > 0;
            }
        }
        public bool is_weighed_stat(Stat_Labels stat)
        {
            bool result = false;
            switch (stat)
            {
                case Stat_Labels.Skl:
                    result = is_weighted_by_ally || is_weighted_skl_override;
                    break;
                case Stat_Labels.Spd:
                    result = is_weighted_by_ally || is_weighted_spd_override;
                    break;
            }
            return result;
        }

        public bool is_rescued { get { return Rescued > 0; } }

        public bool can_rescue(Game_Unit target)
        {
            // If already rescuing/being rescued, lol no
            if (is_rescuing || is_rescued)
                return false;
            // Must be allied teams
            if (is_attackable_team(target))
                return false;
            // Don't rescue units that are already themselves rescuing
            if (target.is_rescuing)
                return false;
            // If the target is a player unit, only other player units can rescue them
            if (target.is_player_team && !this.is_player_team)
                return false;
            return actor.mov > 0 && aid() >= target.stat(Stat_Labels.Con);
        }

        protected override Vector2 real_loc_on_map()
        {
            if (!is_rescued)
                return Real_Loc;
            Game_Unit rescuer = Global.game_map.units[Rescued];
            return rescuer.real_loc;
        }
        /// <summary>
        /// Returns the location of this unit on the map.
        /// If this unit is not recused, it's simply the unit's physical location.
        /// Otherwise it's the location of the rescuer.
        /// </summary>
        public override Vector2 loc_on_map()
        {
            if (!is_rescued)
                return Loc;
            Game_Unit rescuer = Global.game_map.units[Rescued];
            return rescuer.loc;
        }
        #endregion

        #region Unit controls
        public void open_move_range()
        {
            Prev_Loc = canto_loc;
            if (!Cantoing)
                selection_facing();
            Global.game_map.clear_move_range();
            sprite_moving = true;
            Global.game_map.show_move_range(Id);
            if (!Cantoing)
                Global.game_map.show_attack_range(Id);
        }

        public void selection_facing()
        {
            Facing = (mounted() ? 6 : 2);
        }

        internal bool cannot_cancel_move()
        {
            // Skills: Dash
            if (DashActivated)
                return false;

            if (this.cantoing)
                return true;
            return false;
        }

        public void cancel_move()
        {
            Global.game_map.clear_move_range();
            if (!skill_cancel_move())
            {
                Moved_So_Far = 0;
                Temp_Moved = 0;
                sprite_moving = false;

                if (Input.ControlScheme == ControlSchemes.Buttons)
                {
                    Global.player.force_loc(Loc);
                    Global.player.instant_move = true;
                }
                highlight_test();
                Global.game_map.deselect_unit(false);
            }
        }

        public void non_ally_move_range()
        {
            highlighted = false;
            Global.game_map.show_move_range(Id);
            if (!Cantoing) Global.game_map.show_attack_range(Id);
        }

        public void kill()
        {
            kill(true);
        }
        public void kill(bool dead)
        {
            Dead = true;
            if (is_rescuing)
            {
                Game_Unit ally = Global.game_map.units[Rescuing];
                ally.rescued = 0;
                ally.force_loc(Loc);
                if (ally.is_active_team)
                {
                    ally.Ready = false;
                    ally.update_idle_animation();
                    ally.refresh_sprite();
                }
            }
            if (dead)
                actor.hp = 0;
            Global.game_map.kill_unit(this, dead);
            Team = 0;
        }

        public void escape()
        {
            moved();
            Global.game_system.Selected_Unit_Id = -1;
            Dead = true;
            if (is_rescuing)
            {
                Game_Unit ally = Global.game_map.units[Rescuing];
                ally.escape();
            }
            Global.game_map.escape_unit(Id);
            Team = 0;
        }

        public void recover_hp()
        {
            int hp = actor.hp;
            actor.recover_hp();
            refresh_hp(hp - actor.hp);
        }

        public void recover_all(bool negative_only = false)
        {
            int hp = actor.hp;
            actor.recover_all(negative_only);
            refresh_hp(hp - actor.hp);
        }

        public void item_effect(Data_Item item, int target_index)
        {
            // Torch
            vision_bonus = item.Torch_Radius;
            // Pure Water
            for (int i = 0; i < item.Stat_Buff.Length; i++)
                if (item.Stat_Buff[i] != 0)
                    set_stat_bonus((Buffs)i, item.Stat_Buff[i]);

            // Healing, stat gain, etc
            int hp = actor.hp;
            actor.item_effect(item, target_index);
            refresh_hp(hp - actor.hp);
        }

        public void staff_effect(Data_Weapon staff, int target_index)
        {
            // Barrier
            if (staff.Barrier())
                set_stat_bonus(Buffs.Res, Constants.Combat.BARRIER_BONUS);
        }

        private void set_stat_bonus(Buffs stat, int value)
        {
            int current_bonus = Stat_Bonuses[(int)stat];
            Stat_Bonuses[(int)stat] = value < 0 ?
                Math.Min(current_bonus, value) :
                Math.Max(current_bonus, value);
        }

        public void store_state()
        {
            TempBuffs = new List<int>();
            TempBuffs.AddRange(Stat_Bonuses);

            actor.store_states();
        }

        public void restore_state()
        {
            if (TempBuffs != null)
                Stat_Bonuses = TempBuffs;
            TempBuffs = null;

            actor.restore_states();
        }

        public void menu(Vector2 loc)
        {
            Pathfind.reset();
            Global.player.force_loc(Loc);
            // Get movement path
            List<Move_Arrow_Data> move_arrow = Global.game_map.move_arrow;
            Move_Route = new List<Vector2>();
            for (int i = move_arrow.Count - 1; i >= 1; i--)
            {
                Move_Route.Add(new Vector2(move_arrow[i].X, move_arrow[i].Y) -
                    new Vector2(move_arrow[i - 1].X, move_arrow[i - 1].Y));
                Temp_Moved += move_cost(new Vector2(move_arrow[i].X, move_arrow[i].Y));
            }
            Global.game_map.move_range_visible = false;
            Move_Loc = loc;
            Move_Timer = 0;
            Global.game_temp.menu_call = true;
            Global.game_temp.unit_menu_call = true;
        }

        public void ai_move_to(Vector2 target_loc)
        {
            Ai_Move = true;
            Enemy_Move_Wait = 0;
            Force_Move = target_loc;
            Global.player.force_loc(target_loc);
        }

        public bool evented_move_to(Vector2 target_loc)
        {
            return evented_move_to(target_loc, false);
        }
        public bool evented_move_to(Vector2 target_loc, bool ignore_terrain, bool ignore_units = false, float move_speed = -1)
        {
            // If already on an evented move, trying to add one is likely to cause big problems so for now just don't
            // Maybe add support for changing routes dynamically later //Debug
            if (Evented_Move)
                return false;

            Ignore_Terrain_Cost = ignore_terrain;
            Evented_Move = true;
            EventedMoveSpeed = -1;

            Game_Unit unit_at_target = null;
            if (!Global.game_map.is_off_map(target_loc))
            {
                unit_at_target = Global.game_map.get_unit(target_loc);
                if (unit_at_target == this)
                    unit_at_target = null;
            }
            // If the target on the map but someone is already there, find the nearest location instead
            if (!ignore_units && unit_at_target != null)
                target_loc = Pathfind.find_open_tile(target_loc, Id);

            // If we're already at the target
            if (this.loc == target_loc)
                Evented_Move = false;
            // If the target is the default off map location, don't move
            else  if (target_loc == Config.OFF_MAP)
                Evented_Move = false;
            // Pathfind to the target; if it fails, cancel the move
            else if (!pathfind_move_to(target_loc))
                Evented_Move = false;

            if (Evented_Move)
            {
                if (move_speed > 0)
                    EventedMoveSpeed = Math.Max(1, (int)(move_speed * UNIT_TILE_SIZE));
                if (unit_at_target != null && ignore_units)
                    remove_old_unit_location();
                else
                    fix_unit_location(false, target_loc);
                return true;
            }

            Ignore_Terrain_Cost = false;
            return false;
        }

        public void evented_switch_places(Game_Unit other_unit)
        {
            Ignore_Terrain_Cost = false;
            other_unit.Ignore_Terrain_Cost = false;

            // If either unit is off the map, don't move
            if (Global.game_map.is_off_map(Loc) || Global.game_map.is_off_map(other_unit.loc))
                return;
            // If either unit is at the default off map location, don't move
            else if (Loc == Config.OFF_MAP || other_unit.loc == Config.OFF_MAP)
                return;

            Evented_Move = true;
            other_unit.Evented_Move = true;
            // Pathfind to the target; if it fails, cancel the move
            if (!pathfind_move_to(other_unit.loc) || !other_unit.pathfind_move_to(Loc))
            {
                Evented_Move = false;
                Move_Route.Clear();
                Move_Loc = Loc;
                sprite_moving = false;

                other_unit.Evented_Move = false;
                other_unit.Move_Route.Clear();
                other_unit.Move_Loc = other_unit.loc;
                other_unit.sprite_moving = false;
            }
            if (Evented_Move)
            {
                fix_unit_location(false, other_unit.loc);
                other_unit.fix_unit_location(false, Loc);
            }
        }

        internal void evented_move(List<Tuple<EventedMoveCommands, float>> route)
        {
            Evented_Move = true;
            EventedMoveSpeed = -1;
            EventedMoveRoute = route;
            sprite_moving = true;
            Ai_Move = false;
            Move_Timer = 0;
            Ignore_Terrain_Cost = false;
        }

        protected bool pathfind_move_to(Vector2 target_loc)
        {
            return pathfind_move_to(target_loc, false);
        }
        protected bool pathfind_move_to(Vector2 target_loc, bool player_move, bool rescue_move = false)
        {
            if (rescue_move)
                Ignore_Terrain_Cost = true;
            List<Vector2> route = Pathfind.get_route(target_loc, (Evented_Move || rescue_move) ? -1 : mov, Id);
            if (route != null)
            {
                if (Blocked)
                {
                    Blocked = false;
                    return false;
                }
                if (player_move)
                {
                    Global.player.loc = target_loc;
                    Global.player.instant_move = true;
                }
                Move_Loc = target_loc;
                sprite_moving = true;
                Move_Route.AddRange(route);
                if (Ai_Move)
                    Temp_Moved += Pathfind.unit_distance;
                Ai_Move = false;
                Move_Timer = 0;
                // update if $game_map.is_map_scene or @evented_move //Yeti //Events seem to be handling this
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts a move route from a series of one tile cardinal movements into the locations to move to.
        /// </summary>
        /// <param name="move_route">A list of movement vectors for the unit to move along, </param>
        public List<Vector2> actual_move_route(List<Vector2> move_route)
        {
            return actual_move_route(Loc, move_route);
        }
        /// <summary>
        /// Converts a move route from a series of one tile cardinal movements into the locations to move to.
        /// </summary>
        /// <param name="loc">The location to start from. Defaults to the current location of the unit.</param>
        /// <param name="move_route">A list of movement vectors for the unit to move along, </param>
        public List<Vector2> actual_move_route(Vector2 loc, List<Vector2> move_route)
        {
            List<Vector2> result = new List<Vector2>();
            for (int i = move_route.Count - 1; i >= 0; i--)
            {
                loc += move_route[i];
                result.Add(loc);
            }
            return result;
        }

        public void done_moving()
        {
            Sprite_Moving = false;
            Cantoing = false;
            Move_Route.Clear();
        }

        public bool full_move()
        {
            return Moved_So_Far >= mov;
        }

        public void command_menu_close()
        {
            if (Global.player.loc != Move_Loc)
                Global.player.force_loc(Move_Loc); // the centering on this might cause problems with below //Debug
            force_loc(Prev_Loc);
            Move_Loc = Prev_Loc;
            //Global.player.center(Loc, true); //Debug
            Temp_Moved = Moved_So_Far;
            if (!Cantoing) selection_facing();
            Global.game_map.range_start_timer = 0;
            Global.game_map.move_range_visible = true;
            update_map_animation(true);
        }

        /// <summary>
        /// Queues this unit to return to idle and become inactive (greyed out) after the Game_Map finishes processing events.
        /// </summary>
        /// <param name="update_move_ranges">When the unit finally waits, this is passed into the wait() method. Defaults true.</param>
        /// <param name="ignore_wait_inactive_team">If true and the unit is not on the currently moving team, they will return to idle as normal but not grey out. Default true.</param>
        public void start_wait(bool update_move_ranges = true, bool ignore_wait_inactive_team = true)
        {
            Global.game_map.add_unit_wait(Id, update_move_ranges);
            if (ignore_wait_inactive_team && Team != Global.game_state.team_turn)
                Global.game_map.add_unit_wait_skip(Id);
        }
        public void wait(bool update_move_ranges)
        {
            Move_Loc = Loc;
            skill_wait();
            // Locks movement in
            moved();
            // Id selected
            if (Global.game_system.Selected_Unit_Id == Id)
            {
                Global.game_system.Selected_Unit_Id = -1;
                Global.game_map.clear_move_range();
            }
            if (!Global.game_map.skip_unit_wait(Id))
                Ready = false;
            done_moving();

            update_idle_animation();
            refresh_sprite();

            if (update_move_ranges)
            {
                queue_move_range_update();
                Global.game_map.refresh_move_ranges();
            }
            if (Global.scene.is_strict_map_scene)
                ((Scene_Map)Global.scene).update_map_sprite_status(Id);

            // Support gains
            new_turn_support_gain_display();
        }

        public void moved()
        {
            Moved_So_Far = Temp_Moved;
        }

        public void add_attack_target(int id)
        {
            Attack_Targets_This_Turn.Add(id);
        }

        public void end_turn()
        {
            end_turn(true);
        }
        public void end_turn(bool charge_skills)
        {
            if (Constants.Gameplay.MASTERIES_CHARGE_AT_TURN_END)
                if (charge_skills)
                    charge_masteries(MASTERY_RATE_NEW_TURN);
            if (!Constants.Support.PLAYER_SUPPORT_ONLY || is_player_team)
                // This inherently only allows gaining support points with members of the same team for efficiency
                foreach (int unit_id in Global.game_map.teams[Team])
                {
                    int actor_id = Global.game_map.units[unit_id].actor.id;
                    {
                        HashSet<int> shared_targets = new HashSet<int>(
                            Attack_Targets_This_Turn.Intersect(
                            Global.game_map.units[unit_id].Attack_Targets_This_Turn));
                        if (shared_targets.Count > 0)
                            actor.same_target_support_gain(actor_id);
                    }
                }
            if (!Ready)
            {
                Ready = true;
                refresh_sprite();
                return;
            }
            Ready = true;
        }

        public void refresh_unit()
        {
            if (Ready)
            {
                if (Global.game_map.units_waiting())
                    Global.game_map.add_unit_wait_skip(Id);
            }
            else
            {
                end_turn(false);
                Moved_So_Far = 0;
                Turn_Start_Loc = Prev_Loc = Loc;
                Temp_Moved = 0;
                Blocked = false;
            }
        }

        public void new_turn()
        {
            if (!Constants.Gameplay.MASTERIES_CHARGE_AT_TURN_END)
                charge_masteries(MASTERY_RATE_NEW_TURN);
            // Don't decrement status timers on the first team's first turn,
            // for statuses applied before the map starts
            if (!(Global.game_state.turn == 1 && Team == 1))
                actor.update_states();
            for (int i = 0; i < Stat_Bonuses.Count; i++)
            {
                if (Stat_Bonuses[i] < 0)
                    Stat_Bonuses[i] = Math.Min(0, Stat_Bonuses[i] + 1);
                else
                    Stat_Bonuses[i] = Math.Max(0, Stat_Bonuses[i] - 1);
            }
            refresh_hp(0);
            Moved_So_Far = 0;
            Turn_Start_Loc = Prev_Loc = Loc;
            Temp_Moved = 0;
            Blocked = false;
            Attack_Targets_This_Turn.Clear();
            Ai_Wants_Rescue = false;
            if (!Constants.Support.PLAYER_SUPPORT_ONLY || is_player_team)
                // This inherently only allows gaining support points with members of the same team for efficiency
                foreach (int unit_id in Global.game_map.teams[Team]
                    .Where(x => Global.game_map.units[x].ActorId != ActorId))
                {
                    int actor_id = Global.game_map.units[unit_id].actor.id;
                    if (Rescuing == unit_id || Rescued == unit_id)
                        actor.new_turn_support_gain(actor_id, 0, true);
                    else
                        actor.new_turn_support_gain(actor_id, Global.game_map.unit_distance(Id, unit_id));
                }
        }

        public void new_turn_fow()
        {
            Vision_Bonus = Math.Max(0, Vision_Bonus - 1);
        }

        public void status()
        {
            Global.game_temp.status_team = Team;
            Global.game_temp.status_unit_id = Id;
            Global.game_temp.status_menu_call = true;
        }

        public void formation_change(Vector2 loc)
        {
            Formation_Locs.Clear();
            if (loc != Loc)
            {
                Vector2 prev_real_loc = Real_Loc;
                force_loc(loc);
                Vector2 rotation_offset = Real_Loc - prev_real_loc;
                rotation_offset = new Vector2(rotation_offset.Y, -rotation_offset.X);
                rotation_offset.Normalize();
                for (int i = 0; i < Config.FORMATION_CHANGE_STEPS; i++)
                {
                    float length = (float)(
                        // Half a preiod of a sin wave
                        Math.Sin(i * Math.PI / Config.FORMATION_CHANGE_STEPS) *
                        // Amplitude multiplied by half the width of a tile
                        UNIT_TILE_SIZE / 2);
                    Vector2 offset = rotation_offset * length;
                    Formation_Locs.Add((Real_Loc * i) / Config.FORMATION_CHANGE_STEPS +
                        (prev_real_loc * (Config.FORMATION_CHANGE_STEPS - i)) / Config.FORMATION_CHANGE_STEPS + offset);

                    //float length = (float)((i < Config.FORMATION_CHANGE_STEPS / 2 ? i : Config.FORMATION_CHANGE_STEPS - i) *
                    //    16 * ((Config.FORMATION_CHANGE_STEPS / 2) / 8) *
                    //    Math.Sin(i * Math.PI / Config.FORMATION_CHANGE_STEPS));
                    //length = (float)(Math.Sin(i * Math.PI / Config.FORMATION_CHANGE_STEPS) * 16 *
                    //    (Math.Pow(Config.FORMATION_CHANGE_STEPS / 2, 2) / 8));
                    //Vector2 offset = rotation_offset * length;
                    //Formation_Locs.Add((Real_Loc * i) / Config.FORMATION_CHANGE_STEPS +
                    //    (prev_real_loc * (Config.FORMATION_CHANGE_STEPS - i)) / Config.FORMATION_CHANGE_STEPS + offset);
                }
                Formation_Locs.Add(Real_Loc);
            }
        }

        public bool is_changing_formation { get { return Formation_Locs.Count > 0; } }
        #endregion

        #region Map Combat
        public void attack_move(Map_Object target)
        {
            attack_move(target.loc, false);
        }
        public void attack_move(Map_Object target, bool reverse)
        {
            attack_move(target.loc, reverse);
        }
        public void attack_move(Vector2 target_loc)
        {
            attack_move(target_loc, false);
        }
        public void attack_move(Vector2 target_loc, bool reverse)
        {
            for (int i = 0; i < 4; i++)
            {
                int mult = (reverse ? -1 : 1) * (i % 2 == 0 ? 1 : 0) * (UNIT_TILE_SIZE / 16);
                if (mult == 0)
                    Attack_Movement.Add(Vector2.Zero);
                else if (target_loc.X < Loc.X)
                {
                    if (target_loc.Y < Loc.Y)
                        Attack_Movement.Add(new Vector2(-1, -1) * mult);
                    else if (target_loc.Y > Loc.Y)
                        Attack_Movement.Add(new Vector2(-1, 1) * mult);
                    else
                        Attack_Movement.Add(new Vector2(-1, 0) * mult);
                }
                else if (target_loc.X > Loc.X)
                {
                    if (target_loc.Y < Loc.Y)
                        Attack_Movement.Add(new Vector2(1, -1) * mult);
                    else if (target_loc.Y > Loc.Y)
                        Attack_Movement.Add(new Vector2(1, 1) * mult);
                    else
                        Attack_Movement.Add(new Vector2(1, 0) * mult);
                }
                else
                {
                    if (target_loc.Y < Loc.Y)
                        Attack_Movement.Add(new Vector2(0, -1) * mult);
                    else if (target_loc.Y > Loc.Y)
                        Attack_Movement.Add(new Vector2(0, 1) * mult);
                    else
                        Attack_Movement.Add(new Vector2(0, 0) * mult);
                }
            }
        }

        public bool is_attack_moving()
        {
            return Attack_Movement.Count > 0;
        }

        public void wiggle()
        {
            Wiggle_Movement = new List<int> { 0, 3, 3, 2, 2, 2, 1, 1, 0, 0, 0, 3, 2, 2, 2, 2, 1 };
        }

        public bool is_wiggle_moving()
        {
            return Wiggle_Movement.Count >= 6;
        }

        public static Color hit_flash_color(List<int> types)
        {
            // Make this a dictionary in config? //Yeti
            if (types.Contains(5))
                return new Color(255, 0, 0);
            else if (types.Contains(6))
                return new Color(255, 255, 0);
            else if (types.Contains(7))
                return new Color(0, 255, 0);
            else if (types.Contains(8))
                return new Color(255, 255, 128);
            else if (types.Contains(9))
                return new Color(0, 0, 0);
            return new Color(255, 255, 255);
        }

        public void hit_color()
        {
            hit_color(Color.White);
        }
        public void hit_color(Color color)
        {
            Unit_Color.R = color.R;
            Unit_Color.G = color.G;
            Unit_Color.B = color.B;
            Color_List = new List<int> {0,52,64,76,88,100,112,124,136,148,
                160,172,184,196,208,220,232,244,255};
        }

        public void crit_color()
        {
            crit_color(Color.White);
        }
        public void crit_color(Color color)
        {
            Unit_Color.R = color.R;
            Unit_Color.G = color.G;
            Unit_Color.B = color.B;
            Color_List = new List<int> {0,4,16,28,40,52,64,76,88,100,112,124,136,148,
                160,172,184,196,208,220,232,244,255,0,0,0,255,255,0,0,0,255,255,0,0};
            Attack_Movement = new List<Vector2> { new Vector2(-1, 0), new Vector2(2,0),new Vector2(0,0),new Vector2(-2,0),
                new Vector2(2,0),new Vector2(-2,0),new Vector2(2,0),new Vector2(-2,0),new Vector2(0,0),new Vector2(1,0),
                new Vector2(0,0),new Vector2(0,0),new Vector2(0,0),new Vector2(0,0),new Vector2(0,0),new Vector2(0,0),new Vector2(0,0),
                new Vector2(0,0),new Vector2(0,0),new Vector2(0,0),new Vector2(0,0),new Vector2(0,0),new Vector2(0,0) };
            for (int i = 0; i < Attack_Movement.Count; i++)
                Attack_Movement[i] *= Constants.Map.UNIT_PIXEL_SIZE;
        }

        public void status_recover()
        {
            Unit_Color.R = 255;
            Unit_Color.G = 255;
            Unit_Color.B = 255;
            Color_List.Clear();
            //for (int i = 1; i <= 31; i++) //Debug
            for (int i = 1; i < ((256 / 8) - 1); i++)
                Color_List.Add(i * 8);
            for (int i = 1; i < Status_Heal_Effect.MAX_TIME - 62; i++)
                Color_List.Add(255);
            //for (int i = 31; i >= 1; i--) //Debug
            for (int i = ((256 / 8) - 1); i >= 1; i--)
                Color_List.Add(i * 8);
        }

        public bool is_hit_flashing()
        {
            return Color_List.Count > 0;
        }

        public void kill_color()
        {
            Unit_Color.R = 255;
            Unit_Color.G = 255;
            Unit_Color.B = 255;
            Color_List = new List<int> { 0, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 255 };
            Opacity_List = new List<int> { 0,10,20,30,40,50,60,70,80,90,100,110,120,130,140,150,
                160,170,180,190,200,210,220,230,240,255 };
        }

        public bool changing_opacity()
        {
            return Opacity_List.Count > 0;
        }

        public bool is_on_siege()
        {
            if (Loc == Config.OFF_MAP)
                return false;
            if (Global.game_map.is_off_map(Loc))
                return false;
            if (can_use_siege())
            {
                Siege_Engine siege = Global.game_map.get_siege(Loc);
                if (siege != null && siege.is_ready &&
                        actor.is_equippable_as_siege(Global.data_weapons[siege.item.Id]))
                    return true;
            }
            return false;
        }
        #endregion

        #region Ranges
        // Returns a list of enemies in range and a list of weapons that can be used to attack
        public List<int>[] enemies_in_range()
        {
            return enemies_in_range(new HashSet<Vector2> { Loc });
        }
        public List<int>[] enemies_in_range(string skill)
        {
            return enemies_in_range(-1, skill);
        }

        public List<int>[] enemies_in_range(HashSet<Vector2> move_range)
        {
            return enemies_in_range(move_range, -1, "", false, null);
        }
        public List<int>[] enemies_in_range(HashSet<Vector2> move_range, bool ai_siege)
        {
            return enemies_in_range(move_range, -1, "", ai_siege, null);
        }
        public List<int>[] enemies_in_range(HashSet<Vector2> move_range, bool ai_siege, HashSet<Vector2> enemy_tiles)
        {
            return enemies_in_range(move_range, -1, "", ai_siege, enemy_tiles);
        }
        public List<int>[] enemies_in_range(int item_index, string skill)
        {
            return enemies_in_range(new HashSet<Vector2> { Loc }, item_index, skill, false, null);
        }
        public List<int>[] enemies_in_range(HashSet<Vector2> move_range, int item_index)
        {
            return enemies_in_range(move_range, item_index, "", false, null);
        }

        protected List<int>[] enemies_in_range(HashSet<Vector2> move_range, int item_index, string skill, bool ai_siege, HashSet<Vector2> enemy_tiles)
        {
            List<int> useable_weapons;
            // If the item index isn't defined, check all weapons
            if (!ai_siege && !is_ally) // I don't really know what's going on when ai_siege is false, and while actor.items is used
            {
                int x = 0; //Debug
                x++;
            }
            if (item_index == -1)
                useable_weapons = actor.useable_weapons(
                    ai_siege ? actor.items : this.items);
            // Otherwise only check the given weapon
            else
            {
                useable_weapons = new List<int>();
                if (actor.useable_weapons(this.items).Contains(item_index))
                    useable_weapons.Add(item_index);
            }

            List<int> targets = new List<int>(), in_range_weapons = new List<int>();
            List<int> result;
            foreach (int useable_weapon in useable_weapons)
            {
                int min = this.min_range(useable_weapon);
                int max = this.max_range(useable_weapon);
                result = check_range(min, max, move_range, true,
                    this.items[useable_weapon].to_weapon, skill);
                if (result.Any())
                {
                    if (enemy_tiles == null)
                    {
                        targets.AddRange(result);
                        in_range_weapons.Add(useable_weapon);
                    }
                    else
                    {
                        int i = 0;
                        while (i < result.Count)
                            if (!enemy_tiles.Contains(Global.game_map.units[result[i]].loc))
                                result.RemoveAt(i);
                            else
                                i++;
                        if (result.Count > 0)
                        {
                            targets.AddRange(result);
                            in_range_weapons.Add(useable_weapon);
                        }
                    }
                }
            }
            // Convert item indicies into weapon ids
            in_range_weapons = in_range_weapons.Distinct().ToList(); //ListOrEquals
            List<int> weapons = new List<int>();
            foreach (int index in in_range_weapons)
                weapons.Add(items[index].Id);
            // Siege Engines
            if (ai_siege && can_use_siege())
            {
                HashSet<Vector2> siege_move_range = Global.game_map.remove_blocked(move_range, Id);
                foreach (Siege_Engine siege in Global.game_map.siege_engines.Values)
                    if (siege.is_ready && siege_move_range.Contains(siege.loc) &&
                        actor.is_equippable_as_siege(Global.data_weapons[siege.item.Id]) &&
                        !Global.data_weapons[siege.item.Id].is_staff())
                    {
                        Maybe<int> move_distance = Pathfind.get_distance(siege.loc, Id, canto_mov, false, Loc);
                        if (move_distance.IsSomething)
                        {
                            set_ai_base_loc(siege.loc, move_distance);

                            int min = min_range(Siege_Engine.SIEGE_INVENTORY_INDEX);
                            int max = max_range(Siege_Engine.SIEGE_INVENTORY_INDEX);
                            result = check_range(
                                min, max, new HashSet<Vector2> { Loc }, true,
                                this.items[Siege_Engine.SIEGE_INVENTORY_INDEX].to_weapon, skill);
                            targets.AddRange(result);
                            weapons.Add(siege.item.Id);

                            reset_ai_loc();
                        }
                    }
            }

            targets = targets.Distinct().ToList(); //ListOrEquals
            weapons = weapons.Distinct().ToList(); //ListOrEquals
            return new List<int>[] { targets, weapons };
        }

        // Returns a list of enemies in range and a list of staves that can be used
        public List<int>[] enemies_in_staff_range()
        {
            return enemies_in_staff_range(new HashSet<Vector2> { Loc });
        }
        public List<int>[] enemies_in_staff_range(HashSet<Vector2> move_range)
        {
            return enemies_in_staff_range(move_range, -1, "", false);
        }
        public List<int>[] enemies_in_staff_range(HashSet<Vector2> move_range, bool ai_siege)
        {
            return enemies_in_staff_range(move_range, -1, "", ai_siege);
        }
        public List<int>[] enemies_in_staff_range(string skill)
        {
            return enemies_in_staff_range(-1, skill);
        }
        public List<int>[] enemies_in_staff_range(int item_index, string skill)
        {
            return enemies_in_staff_range(new HashSet<Vector2> { Loc }, item_index, skill, false);
        }
        public List<int>[] enemies_in_staff_range(HashSet<Vector2> move_range, int item_index)
        {
            return enemies_in_staff_range(move_range, item_index, "", false);
        }
        protected List<int>[] enemies_in_staff_range(HashSet<Vector2> move_range, int item_index, string skill, bool ai_siege)
        {
            List<int> useable_staves;
            if (item_index == -1)
                useable_staves = actor.useable_attack_staves(
                    ai_siege ? actor.items : this.items);
            else
            {
                useable_staves = new List<int>();
                if (actor.useable_attack_staves(this.items).Contains(item_index))
                    useable_staves.Add(item_index);
            }

            List<int> targets = new List<int>(), in_range_weapons = new List<int>();
            List<int> result;
            foreach (int useable_staff in useable_staves)
            {
                int min = this.min_range(useable_staff);
                int max = this.max_range(useable_staff);
                result = check_range(min, max, move_range, true,
                    this.items[useable_staff].to_weapon, skill);
                targets.AddRange(result);
                if (result.Any())
                    in_range_weapons.Add(useable_staff);
            }
            // Siege Engines
            if (ai_siege && can_use_siege())
            {
                HashSet<Vector2> siege_move_range = Global.game_map.remove_blocked(move_range, Id);
                foreach (Siege_Engine siege in Global.game_map.siege_engines.Values)
                    if (siege.is_ready && siege_move_range.Contains(siege.loc) &&
                        actor.is_equippable_as_siege(Global.data_weapons[siege.item.Id]) &&
                        Global.data_weapons[siege.item.Id].is_attack_staff())
                    {
                        Maybe<int> move_distance = Pathfind.get_distance(siege.loc, Id, canto_mov, false, Loc);
                        if (move_distance.IsSomething)
                        {
                            set_ai_base_loc(siege.loc, move_distance);

                            int min = min_range(Siege_Engine.SIEGE_INVENTORY_INDEX);
                            int max = max_range(Siege_Engine.SIEGE_INVENTORY_INDEX);
                            result = check_range(
                                min, max, new HashSet<Vector2> { Loc }, true,
                                this.items[Siege_Engine.SIEGE_INVENTORY_INDEX].to_weapon, skill);
                            targets.AddRange(result);
                            if (result.Any())
                                in_range_weapons.Add(Siege_Engine.SIEGE_INVENTORY_INDEX);

                            reset_ai_loc();
                        }
                    }
            }
            // Convert item indicies into weapon ids
            in_range_weapons = in_range_weapons.Distinct().ToList(); //ListOrEquals
            List<int> weapons = new List<int>();
            foreach (int index in in_range_weapons)
                weapons.Add(items[index].Id);

            targets = targets.Distinct().ToList(); //ListOrEquals
            return new List<int>[] { targets, weapons };
        }

        public List<int>[] allies_in_staff_range()
        {
            return allies_in_staff_range(new HashSet<Vector2> { Loc }, -1);
        }
        public List<int>[] allies_in_staff_range(HashSet<Vector2> move_range)
        {
            return allies_in_staff_range(move_range, -1);
        }
        public List<int>[] allies_in_staff_range(HashSet<Vector2> move_range, int item_index)
        {
            return allies_in_staff_range(move_range, item_index, false); //Debug
        }
        public List<int>[] allies_in_staff_range(HashSet<Vector2> move_range, int item_index, bool ai_siege)
        {
            List<int> useable_staves;
            if (item_index == -1)
                useable_staves = actor.useable_healing_staves(
                    ai_siege ? actor.items : this.items);
            else
            {
                useable_staves = new List<int>();
                if (actor.useable_healing_staves(this.items).Contains(item_index))
                    useable_staves.Add(item_index);
            }

            List<int> targets = new List<int>(), in_range_weapons = new List<int>();
            List<int> result;
            foreach (int useable_weapon in useable_staves)
            {
                int min_range = this.min_range(useable_weapon);
                int max_range = this.max_range(useable_weapon);
                result = check_range(min_range, max_range, move_range, false,
                    this.items[useable_weapon].to_weapon, "");
                bool added = false;
                foreach (int id in result)
                {
                    if (Global.data_weapons[items[useable_weapon].Id].Heals() ||
                        Global.data_weapons[items[useable_weapon].Id].Status_Remove.Count > 0)
                    {
                        if (Global.data_weapons[items[useable_weapon].Id].can_heal(Global.game_map.units[id]))
                        {
                            targets.Add(id);
                            added = true;
                        }
                    }
                    else // cases for Warp/non-healers //Yeti
                    {
                        targets.Add(id);
                        added = true;
                    }
                }
                if (added)
                    in_range_weapons.Add(useable_weapon);
            }

            targets = targets.Distinct().ToList(); //ListOrEquals
            in_range_weapons = in_range_weapons.Distinct().ToList(); //ListOrEquals
            List<int> weapons = new List<int>();
            foreach (int index in in_range_weapons)
                weapons.Add(items[index].Id);
            return new List<int>[] { targets, weapons };
        }

        public List<int>[] untargeted_staff_range()
        {
            return untargeted_staff_range(-1);
        }
        public List<int>[] untargeted_staff_range(HashSet<Vector2> move_range)
        {
            return untargeted_staff_range(move_range, false);
        }
        public List<int>[] untargeted_staff_range(int item_index)
        {
            return untargeted_staff_range(item_index, false);
        }
        public List<int>[] untargeted_staff_range(HashSet<Vector2> move_range, bool ai_siege)
        {
            return untargeted_staff_range(move_range, -1, ai_siege);
        }
        public List<int>[] untargeted_staff_range(int item_index, bool ai_siege)
        {
            return untargeted_staff_range(new HashSet<Vector2> { Loc }, item_index, ai_siege);
        }
        public List<int>[] untargeted_staff_range(HashSet<Vector2> move_range, int item_index, bool ai_siege)
        {
            List<int> useable_staves;
            if (item_index == -1)
                useable_staves = actor.useable_untargeted_staves(
                    ai_siege ? actor.items : this.items);
            else
            {
                useable_staves = new List<int>();
                if (actor.useable_untargeted_staves(this.items).Contains(item_index))
                    useable_staves.Add(item_index);
            }

            List<int> in_range_weapons = new List<int>();
            foreach (int useable_weapon in useable_staves)
            {
                if (Global.data_weapons[items[useable_weapon].Id].Torch())
                {
                    if (Global.game_map.fow)
                        in_range_weapons.Add(useable_weapon);
                }
                else if (Global.data_weapons[items[useable_weapon].Id].Hits_All_in_Range())
                {
                    //in_range_weapons.Add(useable_weapon);
                }
            }

            in_range_weapons = in_range_weapons.Distinct().ToList(); //ListOrEquals
            List<int> weapons = new List<int>();
            foreach (int index in in_range_weapons)
                weapons.Add(items[index].Id);
            return new List<int>[] { new List<int>(), weapons };
        }

        public List<int> units_in_staff_range(Data_Weapon staff, string skill = "")
        {
            var mode = Windows.Target.Window_Target_Staff.target_mode(staff);
            int min = this.min_range(staff, skill);
            int max = this.max_range(staff, skill);

            switch (mode)
            {
                case Windows.Target.Staff_Target_Mode.Heal:
                case Windows.Target.Staff_Target_Mode.Barrier:
                    List<int> targets = check_range(
                        min, max, new HashSet<Vector2> { Loc }, false, staff, skill);

                    targets = targets.Where(target_id =>
                        {
                            // Duplicated from allies_in_staff_range() //Yeti
                            if (staff.Heals() || staff.Status_Remove.Count > 0)
                            {
                                if (staff.can_heal(Global.game_map.units[target_id]))
                                {
                                    return true;
                                }
                            }
                            else // cases for Warp/non-healers //Yeti
                            {
                                return true;
                            }
                            return false;
                    })
                    .ToList();

                    return targets;
                case Windows.Target.Staff_Target_Mode.Status_Inflict:
                    return check_range(min, max, move_range, true, staff, skill);
                case Windows.Target.Staff_Target_Mode.Torch:
                    return new List<int>();
            }

            return new List<int>();
        }

        public List<int> check_range(int min_range, int max_range, HashSet<Vector2> move_range, bool attackable = true, bool objects = false)
        {
            return check_range(min_range, max_range, move_range, attackable, null, "", objects: objects);
        }
        protected List<int> check_range(int min_range, int max_range, HashSet<Vector2> move_range, bool attackable,
            Data_Weapon weapon, string skill, bool objects = true)
        {
            return check_range(min_range, max_range, move_range, attackable, weapon, skill, null, objects: objects);
        }
        protected List<int> check_range(int min_range, int max_range, HashSet<Vector2> move_range, bool attackable,
            Data_Weapon weapon, string skill, List<int> units, bool objects = true)
        {
            // If the range is invalid, return
            if (min_range < 1 || max_range < min_range || !move_range.Any())
                return new List<int>();
            // If no units were passed in, get valid units
            if (units == null)
                units = get_units(attackable, skill == "" && objects);
            // Removes units that aren't in the playable area
            units = units.Where(x => !Global.game_map.is_off_map(Global.game_map.attackable_map_object(x).loc)).ToList();
            // If there are no units being tested against, return
            if (units.Count == 0)
                return new List<int>();
            List<int> targets;
            if (weapon != null)
            {
                targets = Pathfind.targets_in_range(min_range, max_range, move_range, units,
                    weapon.range_blocked_by_walls());
                if (weapon.is_staff())
                    targets = targets.Where(x => Global.game_map.attackable_map_object(x).is_unit()).ToList();
            }
            else
                targets = Pathfind.targets_in_range(min_range, max_range, move_range, units);
            // Checks if the targets can be hit with the skill
            if (skill != "")
            {
                int i = 0;
                while (i < targets.Count)
                {
                    if (!valid_mastery_target(skill,
                            Global.game_map.units[targets[i]],
                            Global.game_map.combat_distance(Id, targets[i]),
                            weapon.Id))
                        targets.RemoveAt(i);
                    else
                        i++;
                }
            }
            return targets;
        }

        protected List<int> get_units(bool attackable, bool objects = false, bool healing = false)
        {
            List<int> units = new List<int>();
            foreach (int id in Global.game_map.units.Keys)
            {
                Game_Unit unit_here = Global.game_map.units[id];
                // If the unit is not this unit, and the unit can be attacked/can be healed
                if (!(id == Id || (healing ? unit_here.actor.is_full_hp() : unit_here.is_dead)))
                    if (berserk ? attackable : !(attackable ^ is_attackable_team(unit_here)))
                        if (unit_here.visible_by(Team))
                            units.Add(id);
            }
            if (objects && attackable && !healing)
            {
                foreach (Combat_Map_Object attackable_object in Global.game_map.enumerate_combat_objects())
                {
                    // Skip units
                    if (attackable_object is Game_Unit)
                        continue;
                    if (attackable_object is LightRune)
                        if (!is_attackable_team((attackable_object as LightRune).team))
                            continue;
                    units.Add(attackable_object.id);
                }
            }
            return units;
        }

        internal List<int> get_attackable_units()
        {
            return get_units(true);
        }

        public bool is_attackable_team(Game_Unit unit)
        {
            return is_attackable_team(unit.team);
        }
        public override bool is_attackable_team(int other_team)
        {
            foreach (int[] group in Constants.Team.TEAM_GROUPS)
                if (group.Contains(Team))
                    if (group.Contains(other_team))
                        return false;
            return true;

            //Debug
            foreach (int[] group in Constants.Team.TEAM_GROUPS)
                if (group.Contains(Team))
                    return !group.Contains(other_team);
            return false;
        }

        public List<int> attackable_teams()
        {
            return Enumerable.Range(1, Constants.Team.NUM_TEAMS)
                .Where(x => is_attackable_team(x))
                .ToList();

            //Debug
            HashSet<int> teams = new HashSet<int>();
            foreach (int[] group in Constants.Team.TEAM_GROUPS)
                if (!group.Contains(Team))
                    foreach (int team in group)
                        teams.Add(team);
            return teams.Distinct().ToList(); // Can probably change everything that uses this to use hashsets //Debug
        }

        public int[] friendly_teams()
        {
            foreach (int[] group in Constants.Team.TEAM_GROUPS)
                if (group.Contains(Team))
                    return group;
            return new int[0];
        }
        #endregion

        #region Battle Animations
        internal void preload_animations(int distance, bool dance = false)
        {
            var content = Global.Battler_Content as ContentManagers.ThreadSafeContentManager;
            // Get battler animation
            Anim_Sprite_Data anim_data = get_anim_data(dance);
            foreach(string name in preload_animation_names(anim_data, distance, dance))
            {
                content.Load<Texture2D>(string.Format(@"Graphics/Animations/{0}", name), null);
            }
        }

        private IEnumerable<string> preload_animation_names(Anim_Sprite_Data anim_data, int distance, bool dance)
        {
            bool battler_animation_exists = !string.IsNullOrEmpty(anim_data.name);
            if (battler_animation_exists)
            {
                yield return anim_data.name;
            }

            // Get spells and added animations
            if (dance)
            {
                var spells = animation_graphics(dance_effect_animation_ids());
                foreach (string filename in spells)
                    yield return filename;
                var anims = animation_graphics(dance_animation_ids(distance), true, true);
                foreach (string filename in anims)
                    yield return filename;
            }
            else
            {
                if (spell_animation())
                {
                    var spells = animation_graphics(spell_animation_ids());
                    foreach (string filename in spells)
                        yield return filename;
                }
                var skills = animation_graphics(skill_animation_ids());
                foreach (string filename in skills)
                    yield return filename;
                var anims = animation_graphics(battler_animation_ids(distance), true, battler_animation_exists);
                foreach (string filename in anims)
                    yield return filename;
            }
        }

        internal Anim_Sprite_Data get_anim_data(bool dance)
        {
            return (dance && Id == Global.game_state.dancer_id) ?
                   FE_Battler_Image_Wrapper.anim_name(this, Global.weapon_types[0].AnimName) :
                   FE_Battler_Image_Wrapper.anim_name(this);
        }

        internal HashSet<int> battler_animation_ids(int distance)
        {
            HashSet<int> result = new HashSet<int>();

            var anim_set = FE_Battler_Image.animation_set(
                FE_Battler_Image_Wrapper.animation_processor(this, distance));
            if (anim_set != null)
            {
                int offset = FE_Battler_Image_Wrapper.offset(this);
                result.UnionWith(anim_set.AttackAnimations
                    .SelectMany(x => x.all_anim_ids())
                    .Select(x => x + offset));
            }
            else
            {
                if (actor.weapon != null)
                    foreach (int hit in Enumerable.Range(0, 2))
                        foreach (int crt in Enumerable.Range(0, 2))
                        {
                            result.UnionWith(FE_Battler_Image_Wrapper.attack_animation_value(this, crt == 0, distance, hit == 0));
                            result.UnionWith(FE_Battler_Image_Wrapper.still_animation_value(this, crt == 0, distance, hit == 0));
                            result.UnionWith(FE_Battler_Image_Wrapper.return_animation_value(this, crt == 0, distance, hit == 0, false));
                        }
            }

            result.UnionWith(FE_Battler_Image_Wrapper.idle_animation_value(this, distance));
            result.UnionWith(FE_Battler_Image_Wrapper.avoid_animation_value(this, distance));
            result.UnionWith(FE_Battler_Image_Wrapper.avoid_return_animation_value(this, distance));
            result.UnionWith(FE_Battler_Image_Wrapper.get_hit_animation_value(this, false, distance));
            result.UnionWith(FE_Battler_Image_Wrapper.get_hit_animation_value(this, true, distance));
            result.UnionWith(FE_Battler_Image_Wrapper.pre_battle_animation_value(this, distance));
            return result;
        }

        internal HashSet<int> spell_animation_ids()
        {
            HashSet<int> result = new HashSet<int>();
            // Anima spell starting effect
            if (actor.weapon != null && actor.weapon.has_anima_start() && spell_animation())
            {
                int anim_offset = Global.animation_group("Spells");

                if (Global.weapon_types[actor.weapon.anima_type()].Name == "Fire") //Yeti
                    result.Add(anim_offset + 1);
                else if (Global.weapon_types[actor.weapon.anima_type()].Name == "Thunder")
                    result.Add(anim_offset + 61);
                else if (Global.weapon_types[actor.weapon.anima_type()].Name == "Wind")
                    result.Add(anim_offset + 121);
            }
            // All that other mess
            result.UnionWith(spell_anims());
            return result;
        }

        private IEnumerable<int> spell_anims()
        {
            foreach (int id in FE_Battler_Image_Wrapper.spell_attack_animation_value(actor.weapon_id, this.magic_attack, 1, true))
                yield return id;
            foreach (int id in FE_Battler_Image_Wrapper.spell_attack_animation_value_bg1(actor.weapon_id, this.magic_attack, 1, true))
                yield return id;
            foreach (int id in FE_Battler_Image_Wrapper.spell_attack_animation_value_bg2(actor.weapon_id, this.magic_attack, 1, true))
                yield return id;
            foreach (int id in FE_Battler_Image_Wrapper.spell_attack_animation_value_fg(actor.weapon_id, this.magic_attack, 1, true))
                yield return id;
            foreach (int id in FE_Battler_Image_Wrapper.spell_end_animation_value(actor.weapon_id, this.magic_attack, true))
                yield return id;
            foreach (int id in FE_Battler_Image_Wrapper.spell_lifedrain_animation_value(actor.weapon_id, this.magic_attack, 1))
                yield return id;
            foreach (int id in FE_Battler_Image_Wrapper.spell_lifedrain_end_animation_value(actor.weapon_id, this.magic_attack))
                yield return id;
        }

        internal HashSet<int> dance_animation_ids(int distance)
        {
            HashSet<int> result = new HashSet<int>();
            result.UnionWith(FE_Battler_Image_Wrapper.dance_attack_animation_value(this));
            result.UnionWith(FE_Battler_Image_Wrapper.dance_still_animation_value(this));
            result.UnionWith(FE_Battler_Image_Wrapper.dance_return_animation_value(this));

            result.UnionWith(FE_Battler_Image_Wrapper.idle_animation_value(this, distance));
            result.UnionWith(FE_Battler_Image_Wrapper.pre_battle_animation_value(this, distance));
            return result;
        }

        internal HashSet<int> dance_effect_animation_ids()
        {
            HashSet<int> result = new HashSet<int>();
            int ring_id = Global.game_state.dance_item > -1 ? items[Global.game_state.dance_item].Id : -1;
            result.UnionWith(FE_Battler_Image_Wrapper.refresh_animation_value(this, ring_id));
            return result;
        }

        internal HashSet<int> skill_animation_ids()
        {
            HashSet<int> result = new HashSet<int>();
            foreach (int animation_id in actor.all_skills.Select(x => Global.data_skills[x].Animation_Id))
                if (animation_id > -1)
                {
                    int id = animation_id + Global.animation_group("Skills");
                    List<int> anim = FE_Battler_Image_Wrapper.correct_animation_value(id, this);
                    if (anim.Count > 0)
                        result.UnionWith(anim);
                }
            return result;
        }

        internal HashSet<string> animation_graphics(HashSet<int> animation_ids, bool added_animations = false, bool only_added_animations = true)
        {
            HashSet<Battle_Animation_Data> animations = new HashSet<Battle_Animation_Data>();
            foreach (int animation_id in animation_ids)
            {
                if (!(added_animations && only_added_animations))
                    animations.Add(Global.data_animations[animation_id]);

                if (added_animations)
                {
                    var added_effects = On_Hit.ADDED_EFFECTS(animation_id);
                    if (added_effects != null)
                        foreach (var added_effect in added_effects.Select(x => x.Item2))
                        {
                            List<int> anims = new List<int>(added_effect);
                            // Correct animations for terrain, etc
                            for (int i = anims.Count - 1; i >= 0; i--)
                            {
                                List<int> corrected = FE_Battler_Image_Wrapper.correct_animation_value(anims[i], this);
                                anims.RemoveAt(i);
                                anims.InsertRange(i, corrected);
                            }
                            foreach (int temp_anim in anims)
                                animations.Add(Global.data_animations[temp_anim]);
                        }
                }
            }
            return new HashSet<string>(animations.Select(x => x.filename));
        }

        /// <summary>
        /// Returns true if this unit is performing an attack that should cause a spell animation to play.
        /// </summary>
        internal bool spell_animation()
        {
            //return this.magic_attack; //Debug
            return spell_anims().Any();
        }
        #endregion

        #region Sprite Handling
        public bool is_blinking
        {
            get { return !string.IsNullOrEmpty(has_any_mastery()) && ready_masteries().Any(); }
        }

        public Color blink_color
        {
            get
            {
                if (!string.IsNullOrEmpty(has_any_mastery()) && ready_masteries().Any())
                    return new Color(1f, 1f, 1f, 0.8f);
                return new Color();
            }
        }

        public virtual void init_sprites()
        {
            ((Scene_Map)Global.scene).add_map_sprite(Id);
            if (!(is_active_unit_sprite || is_skill_unit_sprite || is_moving_unit_sprite || is_highlighted_unit_sprite))
                update_idle_animation();
            refresh_sprite();
        }

        public string map_sprite_name
        {
            get
            {
                return actor.map_sprite_name;
            }
        }
        public string actual_map_sprite_name
        {
            get
            {
                string name = actor.map_sprite_name;

                bool moving = Sprite_Moving || Battling;
                if (!moving && !Highlighted && is_on_siege())
                {
                    string siege_name =
                        Global.game_map.get_siege(Loc).rider_map_sprite_name;
                    if (!string.IsNullOrEmpty(siege_name))
                    {
                        name = Game_Actors.class_map_sprite_name(siege_name, 0);
                    }
                }
                return name;
            }
        }

        public void refresh_sprite()
        {
            refresh_sprite(actual_map_sprite_name, Sprite_Moving ||
                (Battling && !(Global.game_state.staff_active && Global.game_state.battler_1_id == Id)));
        }
        public void refresh_sprite(string name, bool moving)
        {
            if (Global.scene.is_map_scene)
            {
                ((Scene_Map)Global.scene).refresh_map_sprite(Id, Ready ? Team : 0, name, moving);
            }
        }

        public override void update_sprite(Sprite sprite)
        {
            if (Formation_Locs.Count > 0)
                Real_Loc = Formation_Locs.shift();
            base.update_sprite(sprite);
            // Update unit sprite
            (sprite as Graphics.Map.Character_Sprite).update(this);
            sprite.mirrored =
                ((Facing != 4 && Facing != 6) ||
                    !(is_moving_unit_sprite || Battling)) &&
                has_flipped_map_sprite;
            sprite.draw_offset = new Vector2(TILE_SIZE / 2, TILE_SIZE);
        }

        internal void update_sprite_frame(Graphics.Map.Character_Sprite sprite)
        {
            sprite.frame = (Facing / 2 - 1) * ((Graphics.Map.Character_Sprite)sprite).frame_count + Frame;
        }

        public void set_sprite_batch_effects(Effect effect)
        {
            effect.Parameters["color_shift"].SetValue(Unit_Color.ToVector4());
            effect.Parameters["opacity"].SetValue(Opacity / 255f);
        }

        public void set_new_turn_sprite_batch_effects(Effect effect)
        {
            effect.Parameters["tone"].SetValue(
                Global.Map_Sprite_Colors.data[new Color(64, 56, 56, 255)][Ready ? Team : 0].ToVector4());
            effect.Parameters["color_shift"].SetValue(Unit_Color.ToVector4());
            //effect.Parameters["color_shift"].SetValue(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
        }
        #endregion
    }
}
