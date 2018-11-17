using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FEXNA.State
{
    abstract class Game_State_Component
    {
        // Remember that this can't be serialized, so suspending always has to happen before it has a state
        private List<IEnumerator<bool>> Behaviors = new List<IEnumerator<bool>>();

        #region Game_Map holdovers; could be removed
        protected Dictionary<int, Game_Unit> Units { get { return Global.game_map.units; } }
        protected bool move_range_visible { set { Global.game_map.move_range_visible = value; } }
        protected bool is_player_turn { get { return Global.game_state.is_player_turn; } }
        protected bool Scrolling { get { return Global.game_map.scrolling; } }
        protected int No_Input_Timer { get { return Global.game_state.No_Input_Timer; } }
        protected bool Refresh_Move_Ranges { get { return Global.game_map.Refresh_Move_Ranges; } }
        protected int Team_Turn { get { return Global.game_state.team_turn; } }
        //protected Dictionary<int, List<Rectangle>> team_defend_area { get { return Global.game_map.Team_Defend_Areas[Team_Turn]; } } //Debug
        protected List<Tuple<int, bool>> Waiting_Units { get { return Global.game_map.Waiting_Units; } }

        protected bool is_changing_turns { get { return Global.game_state.is_changing_turns; } }

        protected void highlight_test()
        {
            Global.game_map.highlight_test();
        }

        protected void wait_for_move_update()
        {
            Global.game_map.wait_for_move_update();
        }

        protected Scene_Map get_scene_map()
        {
            return Global.game_map.get_scene_map();
        }
        
        public bool is_blocked(Vector2 loc, int id)
        {
            return Global.game_map.is_blocked(loc, id);
        }
        public bool is_blocked(Vector2 loc, int id, bool fow)
        {
            return Global.game_map.is_blocked(loc, id, fow);
        }

        protected bool is_map_ready()
        {
            return Global.game_state.is_map_ready();
        }
        protected bool is_map_ready(bool lite)
        {
            return Global.game_state.is_map_ready(lite);
        }

        protected bool is_off_screen(Vector2 loc)
        {
            return Global.game_map.is_off_screen(loc);
        }

        protected void refresh_move_ranges()
        {
            Global.game_map.refresh_move_ranges();
        }
        protected void refresh_move_ranges(bool update_all)
        {
            Global.game_map.refresh_move_ranges(update_all);
        }
        #endregion

        internal abstract void write(BinaryWriter writer);

        internal abstract void read(BinaryReader reader);

        internal abstract void update();

        // Handle all the Game_State_Component update loops with behaviors for the heavy lifting //Yeti
        #region Behaviors
        protected bool any_behaviors { get { return Behaviors.Any(); } }

        protected void add_behavior(IEnumerable<bool> behavior)
        {
            Behaviors.Add(behavior.GetEnumerator());
        }

        protected void apply_behaviors()
        {
            if (Behaviors.Count == 0)
            {
                throw new NullReferenceException("Tried to run a behavior when none exist");
            }

            int index = 0;
            while (Behaviors.Count > index)
            {
                if (!Behaviors[0].MoveNext())
                    Behaviors.RemoveAt(0);
                // If a behavior yields true, move to the next behavior
                else if (Behaviors[0].Current)
                    index++;
                // If a behavior yields false, stop processing behaviors for this tick
                else
                    break;
            }
        }

        protected IEnumerable<bool> wait_behavior(int wait_time)
        {
            for (int i = 0; i < wait_time; i++)
                yield return false;
        }
        #endregion
    }
}
