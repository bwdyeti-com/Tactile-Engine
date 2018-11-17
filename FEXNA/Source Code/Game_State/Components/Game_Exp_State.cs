using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace FEXNA.State
{
    class Game_Exp_State : Game_State_Component
    {
        private bool Exp_Calling = false;
        private bool In_Exp = false;
        private int Exp_Action = 0;
        private int Exp_Timer = 0;
        private int Exp_Unit_Id = -1;
        private int Exp_Unit_Gain = 0;

        internal int Exp_Gauge_Gain; //private //Yeti
        private bool Exp_Sound;

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            writer.Write(Exp_Calling);
            writer.Write(In_Exp);
            writer.Write(Exp_Action);
            writer.Write(Exp_Timer);
            writer.Write(Exp_Unit_Id);
            writer.Write(Exp_Unit_Gain);
        }

        internal override void read(BinaryReader reader)
        {
            Exp_Calling = reader.ReadBoolean();
            In_Exp = reader.ReadBoolean();
            Exp_Action = reader.ReadInt32();
            Exp_Timer = reader.ReadInt32();
            Exp_Unit_Id = reader.ReadInt32();
            Exp_Unit_Gain = reader.ReadInt32();
        }
        #endregion

        #region Accessors
        public bool exp_calling { get { return Exp_Calling; } }
        public bool in_exp_gain { get { return In_Exp; } }

        protected Game_Unit exp_gainer { get { return Exp_Unit_Id == -1 ? null : Units[Exp_Unit_Id]; } }
        #endregion

        internal void gain_exp(int unit_id, int exp_gain)
        {
            if (!Exp_Calling && !In_Exp)
            {
                Exp_Calling = true;
                Exp_Unit_Id = unit_id;
                Exp_Unit_Gain = exp_gain;
            }
        }

        internal override void update()
        {
            Scene_Map scene_map = get_scene_map();
            if (scene_map == null)
                return;

            if (Exp_Calling)
            {
                In_Exp = true;
                Exp_Calling = false;
            }
            if (In_Exp)
            {
                bool cont = false;
                while (!cont)
                {
                    cont = true;
                    switch (Exp_Action)
                    {
                        case 0:
                            switch (Exp_Timer)
                            {
                                case 0:
                                    if (exp_gainer.is_ally && exp_gainer.actor.can_level())
                                    {
                                        // shouldn't be hardcoded //Yeti
                                        Exp_Gauge_Gain = (Exp_Unit_Gain > 0 ?
                                            Math.Min(Exp_Unit_Gain, exp_gainer.actor.exp_gain_possible()) :
                                            Math.Max(Exp_Unit_Gain, -exp_gainer.actor.exp_loss_possible()));
                                        scene_map.create_exp_gauge(exp_gainer.actor.exp);
                                        exp_gainer.actor.exp += Exp_Unit_Gain;
                                        Exp_Timer++;
                                    }
                                    else
                                    {
                                        Exp_Action = 2;
                                        cont = false;
                                    }
                                    break;
                                case 27:
                                    if (process_exp_gain())
                                    {
                                        Global.game_system.cancel_sound();
                                        Exp_Timer++;
                                    }
                                    break;
                                // Clears exp window, continues
                                case 47:
                                    scene_map.clear_exp();
                                    Exp_Timer++;
                                    break;
                                case 78:
                                    if (exp_gainer.actor.needed_levels > 0)
                                        Exp_Action++;
                                    else
                                    {
                                        Exp_Action = 2;
                                        cont = false;
                                    }
                                    Exp_Timer = 0;
                                    break;
                                default:
                                    Exp_Timer++;
                                    break;
                            }
                            break;
                        // Level up
                        case 1:
                            switch (Exp_Timer)
                            {
                                case 0:
                                    scene_map.level_up(Exp_Unit_Id);
                                    Exp_Timer++;
                                    break;
                                case 1:
                                    if (!scene_map.is_leveling_up())
                                        Exp_Timer++;
                                    break;
                                case 31:
                                    Exp_Action = 2;
                                    cont = false;
                                    break;
                                default:
                                    Exp_Timer++;
                                    break;
                            }
                            break;
                        case 2:
                            In_Exp = false;
                            Exp_Action = 0;
                            Exp_Timer = 0;
                            Exp_Unit_Id = -1;
                            Exp_Unit_Gain = 0;
                            break;
                    }
                }
            }
        }

        internal bool process_exp_gain() //private //Yeti
        {
            if (Exp_Gauge_Gain > 0)
            {
                get_scene_map().gain_exp();
                Exp_Gauge_Gain--;
                if (!Exp_Sound)
                {
                    Global.game_system.play_se(System_Sounds.Exp_Gain);
                    Exp_Sound = true;
                }
            }
            else if (Exp_Gauge_Gain < 0)
            {
                get_scene_map().lose_exp();
                Exp_Gauge_Gain++;
            }
            else
                return true;
            return false;
        }

        internal void cancel_exp_sound()
        {
            Exp_Sound = false;
        }
    }
}
