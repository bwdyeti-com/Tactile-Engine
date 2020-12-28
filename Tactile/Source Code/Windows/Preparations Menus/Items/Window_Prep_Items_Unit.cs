using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using Tactile.Windows.UserInterface;
using Tactile.Windows.UserInterface.Preparations;
using TactileLibrary;

namespace Tactile
{
    internal class Window_Prep_Items_Unit : Windows.Preparations.WindowPrepActorList
    {
        const int COLUMNS = 4;
        const int ROW_SIZE = 16;
        
        private bool Trading = false;
        
        #region Accessors
        public bool trading
        {
            set
            {
                Trading = value;
                if (Trading)
                {
                    int index = this.index + 1;
                    if (index >= Global.battalion.actors.Count)
                        index = Math.Max(0, index - 2);
                    this.index = index;
                }
                else
                {
                    ResetToSelectedIndex();
                }

                refresh_scroll(false);
                update_cursor();
                UnitCursor.move_to_target_loc();
            }
        }
        #endregion
        
        #region WindowPrepActorList Abstract
        protected override int Columns { get { return COLUMNS; } }
        protected override int VisibleRows { get { return (Config.WINDOW_HEIGHT - (Global.ActorConfig.NumItems + 1) * 16) / ROW_SIZE; } }
        protected override int RowSize { get { return ROW_SIZE; } }

        protected override List<int> GetActorList()
        {
            return new List<int>(Global.battalion.actors);
        }

        protected override string ActorName(int actorId)
        {
            return Global.game_actors[actorId].name;
        }
        protected override string ActorMapSpriteName(int actorId)
        {
            return Global.game_actors[actorId].map_sprite_name;
        }

        protected override void refresh_font(int i)
        {
            int actor_id = Global.battalion.actors[i];
            bool forced = Global.game_map.forced_deployment.Contains(actor_id);
            if (!forced)
            {
                if (Global.game_system.home_base)
                {
                    //Yeti
                }
                else
                {
                    int unit_id = Global.game_map.get_unit_id_from_actor(actor_id);
                    if (unit_id != -1)
                        forced = !Global.game_map.deployment_points.Contains(Global.game_map.units[unit_id].loc);
                }
            }
            refresh_font(i, forced);
        }
        #endregion

        protected override bool map_sprite_ready(int index)
        {
            bool ready = base.map_sprite_ready(index);

            if (!ready)
            {
                if (Global.battalion.is_actor_deployed(index))
                    if (Global.game_map.units[Global.game_map.get_unit_id_from_actor(Global.battalion.actors[index])].loc != Config.OFF_MAP)
                        ready = true;
            }

            return ready;
        }
    }
}
