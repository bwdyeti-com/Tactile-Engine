using System;
using System.IO;

namespace Tactile.State
{
    class Game_Shop_Suspend_State : Game_State_Component
    {
        protected int Shop_Suspend = 0;

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            writer.Write(Shop_Suspend);
        }

        internal override void read(BinaryReader reader)
        {
            Shop_Suspend = reader.ReadInt32();
        }
        #endregion

        internal bool in_shop_suspend { get { return Shop_Suspend > 0; } }

        public void suspend_shop()
        {
            if (Shop_Suspend == 0)
            {
                Shop_Suspend = 4;
                update();
            }
        }

        internal override void update()
        {
            if (Shop_Suspend > 0)
            {
                switch (Shop_Suspend)
                {
                    case 4:
                        if (Global.game_state.arena)
                        {
                            Game_Unit unit = Units[Global.game_system.Shopper_Id];
                            if (unit.is_dead)
                            {
                                Global.game_map.add_dying_unit_animation(unit.id);
                            }
                        }
                        else
                            Global.scene.suspend();
                        break;
                    case 3:
                        if (Global.game_state.arena)
                        {
                            Game_Unit unit = Units[Global.game_system.Shopper_Id];
                            if (unit.is_dead)
                            {
                                unit.update_attack_graphics();
                                if (unit.changing_opacity())
                                    return;
                            }
                        }
                        break;
                    case 2:
                        if (Global.game_state.arena)
                        {
                            Game_Unit unit = Units[Global.game_system.Shopper_Id];
                            if (unit.is_dead)
                            {
                                unit.kill();
                                Global.game_map.remove_unit(unit.id);
                                Global.game_system.Selected_Unit_Id = -1;
                            }
                        }
                        break;
                    case 1:
                        if (Units.ContainsKey(Global.game_system.Shopper_Id))
                        {
                            Game_Unit unit = Units[Global.game_system.Shopper_Id];
                            if (unit.has_canto() && unit.ready && !Global.game_state.arena)
                            {
                                unit.cantoing = true;
                                unit.open_move_range();
                            }
                            else if (Global.game_state.arena && !Global.game_system.In_Arena)
                                unit.start_wait();
                        }
                        Global.game_system.Shopper_Id = -1;
                        Global.game_temp.menuing = false;
                        Global.game_state.arena = Global.game_system.In_Arena;
                        break;
                }
                Shop_Suspend--;
            }
        }
    }
}
