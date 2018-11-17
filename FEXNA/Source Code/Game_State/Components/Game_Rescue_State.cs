using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace FEXNA.State
{
    public enum Rescue_Modes { None, Rescue, Drop, Take, Give, Cover, Refuge }
    class Game_Rescue_State : Game_State_Component
    {
        protected Rescue_Modes Rescue_Calling = 0;
        protected Rescue_Modes In_Rescue = 0;
        protected int Rescue_Phase = 0;
        protected int Rescuer_Id = -1, Rescuee_Id = -1, Giver_Id = -1;
            
        private int Rescue_Mover_Id = -1, Rescue_Target_Id = -1;

        #region Serialization
        internal override void write(BinaryWriter writer)
        {
            writer.Write((int)Rescue_Calling);
            writer.Write((int)In_Rescue);
            writer.Write(Rescue_Phase);
            writer.Write(Rescuer_Id);
            writer.Write(Rescuee_Id);
            writer.Write(Giver_Id);
        }

        internal override void read(BinaryReader reader)
        {
            Rescue_Calling = (Rescue_Modes)reader.ReadInt32();
            In_Rescue = (Rescue_Modes)reader.ReadInt32();
            Rescue_Phase = reader.ReadInt32();
            Rescuer_Id = reader.ReadInt32();
            Rescuee_Id = reader.ReadInt32();
            Giver_Id = reader.ReadInt32();
        }
        #endregion

        #region Accessors
        public Rescue_Modes rescue_calling
        {
            get { return Rescue_Calling; }
            set { Rescue_Calling = value; }
        }

        public bool in_rescue { get { return In_Rescue != Rescue_Modes.None; } }

        protected Rescue_Modes rescue_mode
        {
            get
            {
                if (In_Rescue == Rescue_Modes.Cover)
                    return Rescue_Modes.Rescue;
                if (In_Rescue == Rescue_Modes.Refuge)
                    return Rescue_Modes.Rescue;
                return In_Rescue;
            }
        }

        private Game_Unit active_rescue_unit
        {
            get
            {
                int id = Rescuer_Id;
                if (In_Rescue == Rescue_Modes.Refuge)
                    id = Rescuee_Id;

                if (id == -1) return null;
                return Units[id];
            }
        }

        private Game_Unit rescuer
        {
            get
            {
                if (Rescuer_Id == -1) return null;
                return Units[Rescuer_Id];
            }
        }

        private Game_Unit rescuee
        {
            get
            {
                if (Rescuee_Id == -1) return null;
                return Units[Rescuee_Id];
            }
        }

        private Game_Unit rescue_giving_unit
        {
            get
            {
                if (Giver_Id == -1) return null;
                return Units[Giver_Id];
            }
        }

        internal Game_Unit rescue_moving_unit
        {
            get
            {
                if (Rescue_Mover_Id == -1)
                    return null;
                return Units[Rescue_Mover_Id];
            }
        }

        private Game_Unit rescue_target_unit
        {
            get
            {
                if (Rescue_Target_Id == -1) return null;
                return Units[Rescue_Target_Id];
            }
        }
        #endregion

        internal override void update()
        {
            if (Rescue_Calling != 0)
            {
                In_Rescue = Rescue_Calling;
                Rescue_Calling = 0;
            }
            if (in_rescue)
            {
                switch (Rescue_Phase)
                {
                    case 0:
                        rescue_setup();
                        Rescue_Phase++;
                        if (Global.game_state.is_player_turn) // not suspending on AI turn so I can inspect AI rescue choices //Debug
                        {
                            if (!Global.game_system.is_interpreter_running)
                                Global.scene.suspend();
                        }
                        break;
                    case 1:
                        if (Global.game_state.support_gain_active)
                            break;
                        if (!any_behaviors)
                        {
                            add_behavior(wait_behavior(3));
                            add_behavior(setup_rescue());
                        }
                        apply_behaviors();
                        break;
                }
            }
        }

        #region Behaviors
        private void add_main_rescue_processing()
        {
            add_behavior(wait_behavior(2));
            add_behavior(wait_for_moving_unit());
            add_behavior(wait_behavior(1));
            add_behavior(end_movement());
            add_behavior(wait_behavior(3));
            add_behavior(cleanup_rescue());
            add_behavior(wait_behavior(1));
            add_behavior(end_rescue());
            add_behavior(highlight_test());
        }

        private IEnumerable<bool> setup_rescue()
        {
            // Give/Take
            if (new List<Rescue_Modes> { Rescue_Modes.Give, Rescue_Modes.Take }.Contains(rescue_mode))
                rescuee.force_loc(rescue_giving_unit.loc);
            // Set ids for moving units
            Rescue_Mover_Id = Rescuee_Id;
            Rescue_Target_Id = Rescuer_Id;
            // Skills: Savior
            if (In_Rescue == Rescue_Modes.Cover)
            {
                Rescue_Mover_Id = Rescuer_Id;
                Rescue_Target_Id = Rescuee_Id;
            }

            if (rescue_mode == Rescue_Modes.Drop)
            {
                rescuer.face(rescuer.rescue_drop_loc);
                if (Global.game_map.get_unit(rescuer.rescue_drop_loc) != null)
                {
                    rescuer.drop_fow_blocked(Global.game_map.get_unit(rescuer.rescue_drop_loc));
                    add_behavior(end_rescue());
                    yield return false;
                    yield break;
                }
                else
                {
                    rescuee.rescued = -1;
                    // If drop, move rescued unit to rescuer
                    if (rescue_mode == Rescue_Modes.Drop)
                        rescuee.force_loc(rescuer.loc);
                    rescue_moving_unit.get_dropped(rescuer.rescue_drop_loc);
                }
            }
            else
            {
                rescuee.rescued = -1;
                // If drop, move rescued unit to rescuer
                if (rescue_mode == Rescue_Modes.Drop) // This is impossible because of the surrounding if block, right? //Yeti
                    rescuee.force_loc(rescuer.loc);
                rescue_moving_unit.get_rescued(rescue_target_unit.id);
            }

            add_main_rescue_processing();
            yield return false;
        }
        private IEnumerable<bool> wait_for_moving_unit()
        {
            while (!rescue_moving_unit.is_on_square)
                yield return false;
            yield return false;
        }
        private IEnumerable<bool> end_movement()
        {
            Rescue_Mover_Id = -1;
            rescuee.sprite_moving = false;
            // Sets rescue state variables
            if (rescue_mode == Rescue_Modes.Drop)
            {
                if (rescuee.is_active_team)
                    rescuee.start_wait();
                rescuer.rescuing = 0;
                rescuee.rescued = 0;
                rescuee.fix_unit_location();
            }
            else
            {
                rescuer.rescuing = rescuee.id;
                rescuee.rescued = rescuer.id;
                rescuee.force_loc(Config.OFF_MAP);
            }
            // Skills: Savior
            if (In_Rescue == Rescue_Modes.Refuge)
            {
                Global.game_system.Selected_Unit_Id = -1;
            }

            rescuer.fix_unit_location();
            if (rescue_giving_unit != null)
                rescue_giving_unit.fix_unit_location();

            yield return false;
            while (Global.game_system.is_rescue_interpreter_running || Global.scene.is_message_window_active)
                yield return false;
        }
        private IEnumerable<bool> cleanup_rescue()
        {
            // If the unit that initiated this is getting rescued, they can't canto
            if (!active_rescue_unit.is_rescued)
            {
                // Whoops have to turn canto on before updating move range
                if (new List<Rescue_Modes> { Rescue_Modes.Take, Rescue_Modes.Give }.Contains(rescue_mode))
                {
                    Game_Unit unit;
                    if (rescue_mode == Rescue_Modes.Give)
                        unit = rescue_giving_unit;
                    else
                        unit = active_rescue_unit;
                    unit.cantoing = true;
                }
                else if (active_rescue_unit.has_canto() && !active_rescue_unit.full_move())
                {
                    active_rescue_unit.cantoing = true;
                }
                // Added this because take/dropping from the edge of move range made the unit have 0 move //Debug
                else if (!active_rescue_unit.has_canto() || active_rescue_unit.full_move())
                {
                    active_rescue_unit.cantoing = false;
                }
            }

            rescuer.queue_move_range_update();
            if (rescue_mode == Rescue_Modes.Rescue)
                rescuee.queue_move_range_update(rescuer);
            else
                rescuee.queue_move_range_update();

            if (rescue_giving_unit != null)
                rescue_giving_unit.queue_move_range_update();
            refresh_move_ranges();
            yield return false;

            while (Global.game_system.is_rescue_interpreter_running || Global.scene.is_message_window_active)
                yield return false;

            wait_for_move_update();
            // Determine canto handling
            if (new List<Rescue_Modes> { Rescue_Modes.Take, Rescue_Modes.Give }.Contains(rescue_mode))
            {
                Game_Unit unit;
                if (rescue_mode == Rescue_Modes.Give)
                    unit = rescue_giving_unit;
                else
                    unit = rescuer;

                if (unit.is_active_player_team && !unit.berserk) //Multi
                {
                    if (unit.has_canto() && !unit.full_move())
                        // Adds horse canto
                        Global.game_system.Menu_Canto |= Canto_Records.Horse;
                    else
                        // Removes horse canto
                        Global.game_system.Menu_Canto &= ~Canto_Records.Horse;
                    unit.open_move_range();
                    move_range_visible = false;
                    Global.game_temp.unit_menu_call = true;
                }
            }
            else if (!active_rescue_unit.is_rescued &&
                active_rescue_unit.has_canto() && !active_rescue_unit.full_move())
            {
                if (active_rescue_unit.is_active_player_team && !active_rescue_unit.berserk) //Multi
                {
                    // Adds horse canto
                    Global.game_system.Menu_Canto |= Canto_Records.Horse;
                    active_rescue_unit.open_move_range();
                }
            }
            else
            {
                active_rescue_unit.start_wait(false);
                // Cancels canto state
                Global.game_system.Menu_Canto = Canto_Records.None;
            }
            rescuee.done_moving();
        }
        private IEnumerable<bool> end_rescue()
        {
            Rescuer_Id = -1;
            Rescuee_Id = -1;
            Giver_Id = -1;
            Rescue_Mover_Id = -1;
            Rescue_Target_Id = -1;
            In_Rescue = 0;
            Rescue_Phase = 0;

            yield break;
        }
        private new IEnumerable<bool> highlight_test() //Yeti
        {
            base.highlight_test();
            yield break;
        }
        #endregion

        protected void rescue_setup()
        {
            // Give/Take
            if (new List<Rescue_Modes> { Rescue_Modes.Give, Rescue_Modes.Take }.Contains(rescue_mode))
            {
                Rescuer_Id = Global.game_system.Rescuer_Id;
                Rescuee_Id = Units[Global.game_system.Rescuee_Id].rescuing;
                Giver_Id = Global.game_system.Rescuee_Id;
                rescue_giving_unit.rescuing = 0;
            }
            // Rescue/Drop
            else
            {
                Rescuer_Id = Global.game_system.Rescuer_Id;
                Rescuee_Id = Global.game_system.Rescuee_Id;
            }
            Global.game_system.Rescuer_Id = -1;
            Global.game_system.Rescuee_Id = -1;
            Rescue_Mover_Id = -1;
            Rescue_Target_Id = -1;
        }
    }
}
