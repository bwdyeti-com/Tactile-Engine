using System;
using System.Collections.Generic;
using System.IO;
using TactileVersionExtension;

namespace Tactile
{
    partial class Scene_Map
    {
        public bool check_talk(Game_Unit battler_1, Game_Unit battler_2, bool reverse)
        {
            return check_talk(battler_1, battler_2, reverse, false);
        }
        public bool check_talk(Game_Unit battler_1, Game_Unit battler_2, bool reverse, bool test)
        {
            if (battler_2 == null)
                return false;
            // Specific for two people
            for (int i = 0; i < Global.game_state.battle_convos.Count; i++)
            {
                Battle_Convo convo = Global.game_state.battle_convos[i];
                if ((convo.Id1 == battler_1.id && convo.Id2 == battler_2.id) || (convo.Id1 == battler_2.id && convo.Id2 == battler_1.id))
                {
                    if (convo.Activated)
                        return false;
                    if (!string.IsNullOrEmpty(convo.Value))
                    {
                        if (!Global.battle_text.ContainsKey(convo.Value))
                        {
#if DEBUG
                            Print.message(string.Format(
                                "Boss quote \"{0}\" does not exist", convo.Value));
#endif
                            return false;
                        }

                        if (test)
                            return true;
                        new_message_window();
                        if (convo.Id1 == battler_1.id ^ !reverse)
                            message_reverse();
                        Global.game_temp.message_text = Global.battle_text[convo.Value];
                        Global.game_state.clear_battle_convo(i);
                    }
                    return false;
                }
            }
            // Specific for one person
            for (int i = 0; i < Global.game_state.battle_convos.Count; i++)
            {
                Battle_Convo convo = Global.game_state.battle_convos[i];
                if (!convo.Activated &&
                    ((convo.Id1 == battler_1.id || convo.Id1 == battler_2.id) && convo.Id2 == -1 &&
                    battler_1.is_attackable_team(battler_2)))
                {
                    if (!string.IsNullOrEmpty(convo.Value))
                    {
                        if (!Global.battle_text.ContainsKey(convo.Value))
                        {
#if DEBUG
                            Print.message(string.Format(
                                "Boss quote \"{0}\" does not exist", convo.Value));
#endif
                            return false;
                        }

                        if (test)
                            return true;
                        Global.game_temp.message_text = Global.battle_text[convo.Value]; //Debug
                        Global.game_state.clear_battle_convo(i);
                        new_message_window();
                        if (convo.Id1 == battler_1.id ^ !reverse)
                            message_reverse();
                        //Global.game_temp.message_text = Global.battle_text[convo.Value]; //Debug
                        //Global.game_map.clear_battle_convo(i);
                    }
                    return false;
                }
            }
            return false;
        }
    }
    
    class Battle_Convo
    {
        public int Id1;
        public int Id2;
        public string Value;
        public bool Activated = false;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Id1);
            writer.Write(Id2);
            writer.Write(Value);
            writer.Write(Activated);
        }

        public void read(BinaryReader reader)
        {
            Id1 = reader.ReadInt32();
            Id2 = reader.ReadInt32();
            Value = reader.ReadString();
            if (!Global.LOADED_VERSION.older_than(0, 4, 5, 6)) // This is a suspend load, so this isn't needed for public release //Debug
                Activated = reader.ReadBoolean();
        }
        #endregion

        public override string ToString()
        {
            string name1 = Global.game_map.units.ContainsKey(Id1) ? Global.game_map.units[Id1].actor.name : "<Removed Unit>";
            string name2 = Id2 == -1 ? "<Any Unit>" :
                (Global.game_map.units.ContainsKey(Id2) ? Global.game_map.units[Id2].actor.name : "<Removed Unit>");
            return string.Format("{0}: {1}, {2}", Value, name1, name2);
        }
    }
}
