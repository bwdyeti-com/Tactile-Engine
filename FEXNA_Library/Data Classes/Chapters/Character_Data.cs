using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEXNA_Library
{
    public class Character_Data
    {
        public bool Generic = true;
        public string Identifier = "";
        public string Name = "Gladiator";
        public int Actor_Id = -1;
        public int Class_Id = -1;
        public int Gender = 0;
        public int Level = 1;
        public int Team = 1;
        public int Prepromote_Levels = 0;
        public Generic_Builds Build = Generic_Builds.Strong;
        public int Con = -1;
        public int Priority = 0;
        public int Mission = 0;
        public int Weapon_Id = 1;
        public FEXNA_Library.Item_Data[] Items;
        public int[] WLvls;
        public int Tier = 0;

        public Character_Data(int numItems, int numWLvls)
        {
            WLvls = new int[numWLvls - 1];
            Items = new Item_Data[numItems];
            for (int i = 0; i < Items.Length; i++)
                Items[i] = new FEXNA_Library.Item_Data();
        }

        public static Character_Data from_data(string type, string identifier, string data,
            int numItems, int numWLvls)
        {
            Character_Data battler = new Character_Data(numItems, numWLvls);

            battler.Generic = type == "generic";
            battler.Identifier = identifier;

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

                // If units were created with NUM_ITEMS = 6,
                // and then NUM_ITEMS was changed to 4 or 6, things would break
                // Come up with a longterm solution//Yeti
                int item_count = numItems;
                for (int i = 11; i < ary.Length; i++)
                {
                    var split = ary[i].Split('|');
                    if (split.Length < 2 || !split[1].StartsWith("Item"))
                    {
                        item_count = i - 11;
                        break;
                    }
                }

                for (int i = 0; i < item_count; i++)
                {
                    string[] item = ary[i + 11].Split('|')[0].Split(new string[] { ", " }, StringSplitOptions.None);
                    battler.Items[i] = new FEXNA_Library.Item_Data(Convert.ToInt32(item[0]), Convert.ToInt32(item[1]), Convert.ToInt32(item[2]));
                }
                int index_after_items = 11 + item_count;
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
                for (int i = 0; i < Items.Length; i++)
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
                    data += WLvls[i].ToString();
                    if (i + 1 < WLvls.Length)
                        data += ", ";
                }

                return new string[] { "generic", Identifier, data };
            }
        }

        public void get_actor_data(Dictionary<int, Data_Actor> actorData)
        {
            if (!Generic && actorData.ContainsKey(Actor_Id))
            {
                var actor = actorData[Actor_Id];
                Name = actor.Name;
                Class_Id = actor.ClassId;
                Gender = actor.Gender;
                Level = actor.Level;
                Items = actor.Items
                    .Select(x => new Item_Data(x[0], x[1], x[2]))
                    .ToArray();
            }
        }
    }
}
