using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ListExtension;

namespace FEXNA.Windows.Map.Info
{
    enum Map_Info_State { Offscreen, Onscreen, Refresh }
    abstract class Window_Map_Info : Stereoscopic_Graphic_Object
    {
        protected int LEFT_X = 8;
        protected int RIGHT_X = Math.Max(Config.WINDOW_WIDTH / 2, Config.WINDOW_WIDTH - 88);
        protected int TOP_Y = 8;
        protected int BOTTOM_Y = Math.Max(Config.WINDOW_HEIGHT / 2, Config.WINDOW_HEIGHT - 40);

        protected bool Offscreen = true;
        protected int Map_Busy_Timer = 1;
        protected List<int> X_Move_List = new List<int>(), Y_Move_List = new List<int>();

        #region Accessors
        protected Game_Unit get_unit
        {
            get
            {
                Game_Unit unit = Global.game_map.get_unit(Global.player.loc);
                if (unit != null)
                    if (!unit.is_rescued)
                        if (unit.visible_by()) //visible_by //Yeti
                            return unit;
                return null;
            }
        }
        #endregion

        protected virtual void initialize()
        {
            initialize_images();
        }

        protected abstract void initialize_images();

        protected abstract void set_images();

        protected virtual Map_Info_State map_check()
        {
            if (!Global.game_state.is_info_ready)
            {
                if (Map_Busy_Timer <= 0)
                {
                    Map_Busy_Timer = 1;
                    move_off();
                }
                return Map_Info_State.Offscreen;
            }
            else if (Global.player.next_unit && Global.game_map.scrolling)
            {
                Map_Busy_Timer = 1;
                move_off();
                return Map_Info_State.Offscreen;
            }
            else if (Map_Busy_Timer > 0)
            {
                Map_Busy_Timer--;
                if (Map_Busy_Timer == 0)
                    return Map_Info_State.Refresh;
            }
            return Map_Info_State.Onscreen;
        }

        #region Position Testing
        protected bool is_self_on_left { get { return loc.X < Config.WINDOW_WIDTH / 2; } }

        protected bool is_self_on_right { get { return loc.X >= Config.WINDOW_WIDTH / 2; } }

        protected bool is_self_on_top { get { return loc.Y < Config.WINDOW_HEIGHT / 2; } }

        protected bool is_self_on_bottom { get { return loc.Y >= Config.WINDOW_HEIGHT / 2; } }

        protected bool is_on_left { get { return Global.player.is_true_on_left() && is_self_on_left; } }

        protected bool is_on_right { get { return Global.player.is_true_on_right() && is_self_on_right; } }

        protected bool is_on_top { get { return Global.player.is_true_on_top() && is_self_on_top; } }

        protected bool is_on_bottom { get { return Global.player.is_true_on_bottom() && is_self_on_bottom; } }
        #endregion

        protected abstract void refresh();

        protected void refresh_graphic_object(Stereoscopic_Graphic_Object sprite)
        {
            // This should be handled entirely with offsets in the drawing calls //Debug
            if (sprite != null)
            {
                sprite.loc = loc;
                copy_stereo(sprite);
            }
        }

        #region Update
        public abstract void update();

        protected virtual void update_position()
        {
            if (X_Move_List.Count > 0)
                loc.X = X_Move_List.pop();
            if (Y_Move_List.Count > 0)
                loc.Y = Y_Move_List.pop();
        }
        #endregion

        public virtual void go_offscreen()
        {
            move_off();
            while (X_Move_List.Count > 0 || Y_Move_List.Count > 0)
                update_position();
            refresh();
        }

        protected abstract void move_off();

        public abstract void draw(SpriteBatch sprite_batch);
    }
}
