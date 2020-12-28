using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Tactile.State
{
    class Game_Talk_State : Game_State_Component
    {
        protected bool Talk_Calling = false;
        protected bool In_Talk = false;
        protected int Talk_Timer = 0;
        protected string Talk_Event_Name = "";
        protected int Talker_Id = -1;

        #region Accessors
        internal bool talk_calling
        {
            get { return Talk_Calling; }
            set { Talk_Calling = value; }
        }
        internal bool in_talk { get { return In_Talk; } }

        internal string talk_event_name { set { Talk_Event_Name = value; } }

        internal Game_Unit talker { get { return Talker_Id == -1 || !Units.ContainsKey(Talker_Id) ? null : Units[Talker_Id]; } }

        internal bool waiting_for_support_gain
        {
            get
            {
                return Talk_Timer >= 3 &&
                    (!Global.game_system.is_interpreter_running &&
                    !Global.scene.is_message_window_active);
            }
        }
        #endregion

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            writer.Write(Talk_Calling);
            writer.Write(In_Talk);
            writer.Write(Talk_Timer);
            writer.Write(Talk_Event_Name);
            writer.Write(Talker_Id);
        }

        internal override void read(BinaryReader reader)
        {
            Talk_Calling = reader.ReadBoolean();
            In_Talk = reader.ReadBoolean();
            Talk_Timer = reader.ReadInt32();
            Talk_Event_Name = reader.ReadString();
            Talker_Id = reader.ReadInt32();
        }
        #endregion

        internal override void update()
        {
            if (Talk_Calling)
            {
                In_Talk = true;
                Talk_Calling = false;
            }
            if (In_Talk)
            {
                bool cont = false;
                if (this.waiting_for_support_gain)
                    if (Global.game_state.support_gain_active)
                        cont = true;
                while (!cont)
                {
                    cont = true;
                    switch (Talk_Timer)
                    {
                        case 0:
                            Global.scene.suspend();
                            Talker_Id = Global.game_system.Visitor_Id;
                            Global.game_system.Visitor_Id = -1;
                            Talk_Timer++;
                            break;
                        case 2:
                            Global.game_state.activate_event_by_name(Talk_Event_Name);
                            Talk_Timer++;
                            break;
                        case 3:
                            if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                            {
                                if (talker != null)
                                    talker.queue_move_range_update();
                                refresh_move_ranges();
                                wait_for_move_update();

                                if (talker != null)
                                {
                                    if (talker.cantoing && talker.is_active_player_team) //Multi
                                    {
                                        Global.player.force_loc(talker.loc);
                                        talker.open_move_range();
                                        if (Constants.Gameplay.TALKING_IS_FREE_ACTION)
                                        {
                                            move_range_visible = false;
                                            Global.game_temp.unit_menu_call = true;
                                            end_talk();
                                            break;
                                        }
                                    }
                                    else
                                        talker.start_wait(false);
                                }

                                Talk_Timer++;
                            }
                            break;
                        case 4:
                            if (!Global.game_system.is_interpreter_running && !Global.scene.is_message_window_active)
                            {
                                end_talk();
                            }
                            break;
                        default:
                            Talk_Timer++;
                            break;
                    }
                }
            }
        }

        private void end_talk()
        {
            Global.game_state.resume_turn_theme();
            In_Talk = false;
            Talk_Timer = 0;
            Talk_Event_Name = "";
            Talker_Id = -1;
            highlight_test();
        }
    }
}
