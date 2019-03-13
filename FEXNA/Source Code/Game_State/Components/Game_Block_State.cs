using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace FEXNA.State
{
    class Game_Block_State : Game_State_Component
    {
        protected bool Block_Calling = false;
        protected bool In_Block = false;
        protected int Block_Timer = 0;
        protected int Blocked_Id = -1;

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            writer.Write(In_Block);
            writer.Write(Block_Timer);
            writer.Write(Blocked_Id);
        }

        internal override void read(BinaryReader reader)
        {
            In_Block = reader.ReadBoolean();
            Block_Timer = reader.ReadInt32();
            Blocked_Id = reader.ReadInt32();
        }
        #endregion

        #region Accessors
        public bool block_calling
        {
            get { return Block_Calling; }
            set { Block_Calling = value; }
        }

        public bool in_block { get { return In_Block; } }

        public int blocked_id { set { Blocked_Id = value; } }

        public Game_Unit blocked_unit { get { return Blocked_Id == -1 ? null : Units[Blocked_Id]; } }
        #endregion

        internal override void update()
        {
            if (Block_Calling)
            {
                In_Block = true;
                Block_Calling = false;
            }
            if (In_Block && get_scene_map() != null)
            {
                bool cont = false;
                while (!cont)
                {
                    cont = true;
                    switch (Block_Timer)
                    {
                        case 0:
                            if (Global.game_state.is_player_turn)
                                Global.scene.suspend();
                            Block_Timer++;
                            break;
                        case 1:
                            get_scene_map().set_map_effect(blocked_unit.loc, 4, 6);
                            Block_Timer++;
                            break;
                        case 2:
                            if (!get_scene_map().is_map_effect_active())
                            {
                                // Reset since this unit couldn't open the menu
                                Global.game_temp.ResetContextSensitiveUnitControl();

                                blocked_unit.start_wait();
                                blocked_unit.mission = -1;
                                Blocked_Id = -1;
                                In_Block = false;
                                Block_Timer = 0;
                                Global.game_map.move_range_visible = true;
                                //highlight_test(); //Debug
                            }
                            break;
                    }
                }
            }
        }
    }
}
