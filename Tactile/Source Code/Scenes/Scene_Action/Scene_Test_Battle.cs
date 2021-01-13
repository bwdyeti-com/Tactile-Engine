#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TactileLibrary;

namespace Tactile
{
    class Scene_Test_Battle : Scene_Arena
    {
        public Scene_Test_Battle() { }

        protected override void initialize_base()
        {
            Scene_Type = "Scene_Test_Battle";
            main_window();
            camera = new Camera(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT, Vector2.Zero);
            Weather_Visible = BattleWeathers.Invisible;

            reset_map();
        }

        public override void initialize_action(int distance)
        {
            Global.game_state.reset();
            Global.game_map = new Game_Map();
            Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Vector2.Zero, Global.game_system.Battler_1_Id, "");
            Global.game_map.add_actor_unit(Constants.Team.ENEMY_TEAM, Vector2.Zero, Global.game_system.Battler_2_Id, "");
            Global.game_system.Battler_1_Id = 1;
            Global.game_system.Battler_2_Id = 2;
            Combat.battle_setup(Global.game_system.Battler_1_Id, Global.game_system.Battler_2_Id, distance);

            base.initialize_action(distance);
        }

        public override void suspend() { }

        protected override void new_combat_data(int distance, bool initial)
        {
            Combat_Data = new Test_Combat_Data(Battler_1.id, Battler_2.id, distance);
            Global.game_state.combat_data = Combat_Data;
            if (!initial)
            {
                Attack_Id = 0;
                HUD.combat_data = Combat_Data;
                refresh_stats();
            }
        }

        protected override void battle_end()
        {
            Global.scene_change("Scene_Title");
            Global.game_system.In_Arena = false;
        }
    }

    class Test_Battle_Character_Data
    {
        private const int DEFAULT_CLASS_ID = 16;

        internal static int default_class_id
        {
            get
            {
                if (Global.data_classes == null || Global.data_classes.ContainsKey(DEFAULT_CLASS_ID))
                    return DEFAULT_CLASS_ID;
                else if (DEFAULT_CLASS_ID > Global.data_classes.Keys.Max())
                    return Global.data_classes.Keys.Max();
                else
                    return Global.data_classes.Keys.OrderBy(x => x).First(x => x > DEFAULT_CLASS_ID);
            }
        }

        public bool Generic = true;
        public string Identifier = "";
        public string Name = "Gladiator";
        public int Actor_Id = -1;
        public int Class_Id = default_class_id;
        public int Gender = 0;
        public int Level = 1;
        public int Team = 1;
        public int Prepromote_Levels = 0;
        public Generic_Builds Build = Generic_Builds.Strong;
        public int Con = -1;
        public int Priority = 0;
        public int Mission = 0;
        public int Weapon_Id = 1;
        public TactileLibrary.Item_Data[] Items = new TactileLibrary.Item_Data[Global.ActorConfig.NumItems];
        public int[] WLvls;
        public int Tier = 0;
        public int Promotion = 0;

        private Data_Actor actor_data
        {
            get
            {
                return Global.data_actors.ContainsKey(Actor_Id) ?
                    Global.data_actors[Actor_Id] : null;
            }
        }
        public string name { get { return Generic || this.actor_data == null ? Name : this.actor_data.Name; } }
        public int class_id { get { return Generic || this.actor_data == null ? Class_Id : this.actor_data.ClassId; } }
        public int gender { get { return Generic || this.actor_data == null ? Gender : this.actor_data.Gender; } }
        public int level { get { return Generic || this.actor_data == null ? Level : this.actor_data.Level; } }
        public IEnumerable<Item_Data> items
        {
            get
            {
                return Generic || this.actor_data == null ? Items :
                    this.actor_data.Items.Select(x => new Item_Data(x[0], x[1], x[2]));
            }
        }

        public Test_Battle_Character_Data()
        {
            WLvls = new int[Global.weapon_types.Count - 1];
            for (int i = 0; i < Items.Length; i++)
                Items[i] = new TactileLibrary.Item_Data();
        }

        public static Test_Battle_Character_Data from_data(string type, string identifier, string data)
        {
            Test_Battle_Character_Data battler = new Test_Battle_Character_Data();
            battler.Generic = type == "generic";
            battler.Identifier = identifier;
            for (int i = 0; i < battler.Items.Length; i++)
                battler.Items[i] = new TactileLibrary.Item_Data();
            if (battler.Generic)
            {
                battler.Actor_Id = -1;
                string[] ary = data.Split('\n');
                battler.Name = ary[0].Split('|')[0];
                battler.Class_Id = Convert.ToInt32(ary[1].Split('|')[0]);
                battler.Gender = Convert.ToInt32(ary[2].Split('|')[0]);
                battler.Level = Convert.ToInt32(ary[3].Split('|')[0]);
                //battler.Exp = ary[4    0|Exp
                battler.Team = Convert.ToInt32(ary[5].Split('|')[0]);
                battler.Prepromote_Levels = Convert.ToInt32(ary[6].Split('|')[0]);
                battler.Build = (Generic_Builds)Convert.ToInt32(ary[7].Split('|')[0]);
                battler.Con = Convert.ToInt32(ary[8].Split('|')[0]);
                battler.Priority = Convert.ToInt32(ary[9].Split('|')[0]);
                battler.Mission = Convert.ToInt32(ary[10].Split('|')[0]);
                battler.Weapon_Id = Convert.ToInt32(ary[11].Split('|')[0].Split(new string[] { ", " }, StringSplitOptions.None)[1]);

                int numItems;
                var items = Game_Map.ReadUnitDataItems(11, ary, out numItems);
                for (int i = 0; i < battler.Items.Length; i++)
                {
                    if (i < items.Count)
                        battler.Items[i] = items[i];
                    else
                        battler.Items[i] = new Item_Data(0, 0, -1);
                }

                int index_after_items = 11 + numItems;
                string[] wlvls = ary[index_after_items].Split('|')[0]
                    .Split(new string[] { ", " }, StringSplitOptions.None);
                for (int i = 0; i < battler.WLvls.Length; i++)
                {
                    if (i < wlvls.Length)
                        battler.WLvls[i] = Convert.ToInt32(wlvls[i]);
                }
            }
            else
            {
                string[] ary = data.Split('\n');
                battler.Actor_Id = Convert.ToInt32(ary[0].Split('|')[0]);
                battler.Team = Convert.ToInt32(ary[1].Split('|')[0]);
                battler.Priority = Convert.ToInt32(ary[2].Split('|')[0]);
                battler.Mission = Convert.ToInt32(ary[3].Split('|')[0]);
            }
            return battler;
        }

        public string[] to_string()
        {
            return to_string(Team);
        }
        public string[] to_string(int team)
        {
            return to_string(team, Priority);
        }
        public string[] to_string(int team, int ai_priority)
        {
            return to_string(team, ai_priority, Mission);
        }
        public string[] to_string(int team, int ai_priority, int ai_mission)
        {
            if (!Generic)
            {
                string data = "";
                data += Actor_Id.ToString() + "|Actor ID\n";
                data += team.ToString() + "|Team\n";
                data += ai_priority.ToString() + "|AI Priority\n";
                data += ai_mission.ToString() + "|AI Mission";
                return new string[] { "character", Identifier, data };
            }
            else
            {
                Game_Actor actor = null;
                // I don't quite understand this, aren't all generics temporary actors //Yeti
                bool is_temp_actor = false;
                if (Global.game_actors.ContainsKey(Actor_Id))
                    actor = Global.game_actors[Actor_Id];
                else if (Actor_Id >= 0)
                {
                    is_temp_actor = true;
                    actor = Global.game_actors[Actor_Id];
                    actor.class_id = Class_Id;
                    for (int i = 1; i < Global.weapon_types.Count; i++)
                        if (actor.weapon_level_cap(Global.weapon_types[i]) > 0)
                            actor.wexp_gain(Global.weapon_types[i], WLvls[i - 1]);
                }

                string data = "";
                data += Name.ToString() +              "|Name\n";
                data += Class_Id.ToString() +          "|Class ID\n";
                data += Gender.ToString() +            "|Gender\n";
                data += Level.ToString() +             "|Level\n";
                data +=                               "0|Exp\n";
                data += team.ToString() +              "|Team\n";
                data += Prepromote_Levels.ToString() + "|Prepromote Levels\n";
                data += ((int)Build).ToString() +      "|Build Type\n";
                data += Con.ToString() +               "|Con\n";
                data += ai_priority.ToString() +       "|AI Priority\n";
                data += ai_mission.ToString() +        "|AI Mission\n";
                for (int i = 0; i < Global.ActorConfig.NumItems; i++)
                {
                    int uses = -1; //Debug
                    //if (Items[i].Id > 0)
                    //{
                    //    uses = Items[i].max_uses;
                    //}
                    data += ((int)Items[i].Type).ToString() + ", " + Items[i].Id.ToString() + ", " + uses.ToString() + "|Item " + (i + 1).ToString() + "\n";
                }
                for (int i = 0; i < WLvls.Length; i++)
                {
                    if (WLvls[i] > 0 && actor != null && !actor.has_rank(Global.weapon_types[i + 1]))
                        data += "0";
                    else
                        data += WLvls[i].ToString();
                    if (i + 1 < WLvls.Length)
                        data += ", ";
                }

                if (is_temp_actor)
                    Global.game_actors.temp_clear(Actor_Id);
                return new string[] { "generic", Identifier, data };
            }
        }
    }
}
#endif