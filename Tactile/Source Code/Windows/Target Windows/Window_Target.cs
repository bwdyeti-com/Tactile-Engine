using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TactileLibrary;

namespace Tactile.Windows.Target
{
    abstract class Window_Target<T> : ISelectionMenu
    {
        protected int Unit_Id;
        private int Index = 0, _lastTargetIndex;
        protected int Temp_Index = -1;
        protected Vector2 Loc;
        protected List<T> Targets;
        protected int Left_X = 4;
        protected int Right_X = Config.WINDOW_WIDTH - 76;// 244;
        protected bool Visible = true;
        protected bool Manual_Targeting;

        protected ConsumedInput SelectedIndex;
        private bool Canceled; 

        #region Accessors
        public int index
        {
            get { return Index; }
            protected set
            {
                Index = value;
                if (has_target)
                    _lastTargetIndex = value;
                // update help //Yeti
            }
        }

        public int LastTargetIndex { get { return _lastTargetIndex; } }

        public List<T> targets { get { return Targets; } }

        public abstract T target { get; }

        public bool visible
        {
            get { return Visible; }
            set { Visible = value; }
        }

        public bool manual_targeting { get { return Manual_Targeting; } }

        protected abstract int window_width { get; }

        internal bool has_target { get { return Index >= 0; } }

        protected virtual bool cursor_not_on_target
        {
            get { return Global.player.loc != target_loc(this.target); }
        }
        #endregion

        public Window_Target() { }

        protected virtual void initialize(Vector2 loc)
        {
            Loc = loc;
        }

        internal Game_Unit get_unit()
        {
            return Global.game_map.units[Unit_Id];
        }

        internal abstract Vector2 target_loc(T target);

        private Rectangle target_map_rect(T target)
        {
            Vector2 loc = target_loc(target);
            loc *= Constants.Map.TILE_SIZE;
            loc -= Global.game_map.display_loc;
            return new Rectangle((int)loc.X, (int)loc.Y,
                Constants.Map.TILE_SIZE, Constants.Map.TILE_SIZE);
        }

        protected abstract int distance_to_target(T target);

        protected virtual List<T> sort_targets(List<T> targets)
        {
            Game_Unit unit = get_unit();
            targets.Sort(delegate(T a, T b)
            {
                Vector2 loc1 = target_loc(a);
                Vector2 loc2 = target_loc(b);

                int angle1 = ((360 - unit.angle(loc1)) + 90) % 360;
                int angle2 = ((360 - unit.angle(loc2)) + 90) % 360;
                return (angle1 == angle2 ? (loc1.Y == loc2.Y ?
                    distance_to_target(a) - distance_to_target(b) :
                    (int)(loc1.Y - loc2.Y)) : angle1 - angle2);
            });
            return targets;
        }


        protected abstract void set_images();

        public void update(bool active)
        {
            reset_selected();

            update_begin();
            int temp_index = Index;

            if (active && !Manual_Targeting)
            {
                // If the mouse is on a target, move to it
                if (Input.IsControllingOnscreenMouse)
                {
                    bool target_found = false;
                    for (int i = 0; i < Targets.Count; i++)
                    {
                        Rectangle target_rect = target_map_rect(Targets[i]);
                        if (Global.Input.mouse_in_rectangle(target_rect))
                        {
                            target_found = true;
                            if (i != Index)
                                move_to(i);
                            break;
                        }
                    }
                    if (!target_found)
                        Temp_Index = -1;
                }
                else if (Input.ControlScheme == ControlSchemes.Touch)
                {
                    for (int i = 0; i < Targets.Count; i++)
                    {
                        if (i == Index)
                            continue;

                        Rectangle target_rect = target_map_rect(Targets[i]);
                        if (Global.Input.touch_rectangle(
                                Services.Input.InputStates.Triggered,
                                target_rect))
                        {
                            move_to(i);
                            break;
                        }
                    }
                }

                if (Input.ControlScheme == ControlSchemes.Buttons)
                {
                    if (Index == -1)
                    {
                        move_to(LastTargetIndex);
                        Global.player.instant_move = true;
                        Global.player.force_loc(target_loc(Targets[LastTargetIndex]));
                    }
                    else if (this.cursor_not_on_target)
                    {
                        move_to(Index);
                    }
                }
            }

            if (Global.player.is_on_left() && Loc.X != Right_X)
            {
                Loc.X = Right_X;
                refresh();
            }
            else if (Global.player.is_on_right() && Loc.X != Left_X)
            {
                Loc.X = Left_X;
                refresh();
            }
            else
            {
                if (active && !Manual_Targeting)
                {
                    if (Targets.Count > 1)
                    {
                        if (is_press_down())
                        {
                            move_down();
                        }
                        else if (is_press_up())
                        {
                            move_up();
                        }
                    }
                }
            }
            if (Index != Temp_Index)
            {
                this.index = Temp_Index;
                if (has_target)
                    set_images();
            }
            else
            {
                if (active)
                {
                    if (Global.Input.triggered(Inputs.A) && has_target)
                        SelectedIndex = new ConsumedInput(ControlSchemes.Buttons, Index);
                    else if (Global.Input.triggered(Inputs.B))
                        Canceled = true;
                    else if (Manual_Targeting)
                    {
                        if (Input.IsControllingOnscreenMouse)
                        {
                            if (Global.Input.mouse_click(MouseButtons.Left))
                            {
                                if (Global.player.at_mouse_loc)
                                    SelectedIndex = new ConsumedInput(ControlSchemes.Mouse, 0);
                                else
                                    Global.game_system.play_se(System_Sounds.Buzzer);
                            }
                        }
                        else if (Input.ControlScheme == ControlSchemes.Touch)
                        {
                            if (Global.Input.gesture_triggered(TouchGestures.Tap))
                            {
                                if (Global.game_state.move_to_touch_location(TouchGestures.Tap))
                                    SelectedIndex = new ConsumedInput(ControlSchemes.Touch, 0);
                                else
                                    Global.game_system.play_se(System_Sounds.Buzzer);
                            }
                        }
                    }
                    else
                    {
                        if (Input.IsControllingOnscreenMouse && has_target)
                        {
                            if (Global.Input.mouse_clicked_rectangle(MouseButtons.Left,
                                target_map_rect(Targets[Index])))
                            {
                                SelectedIndex = new ConsumedInput(ControlSchemes.Mouse, Index);
                            }
                            // buzz on clicking nothing? //Debug
                        }
                        else if (Input.ControlScheme == ControlSchemes.Touch && has_target)
                        {
                            if (Global.Input.gesture_rectangle(TouchGestures.Tap,
                                target_map_rect(Targets[Index])))
                            {
                                SelectedIndex = new ConsumedInput(ControlSchemes.Touch, Index);
                            }
                            // buzz on tapping nothing? //Debug
                        }
                    }
                }
            }

            update_end(temp_index);
            if (active) // && Help_Window != null) //Yeti
            {
                // update_help(); //Yeti
            }
        }

        protected bool is_press_down()
        {
            return (Global.Input.repeated(Inputs.Down) && !Global.Input.pressed(Inputs.Right)) || Global.Input.repeated(Inputs.Right);
        }

        protected bool is_press_up()
        {
            if (Global.Input.pressed(Inputs.Down) || Global.Input.pressed(Inputs.Right)) return false;
            return (Global.Input.repeated(Inputs.Up) && !Global.Input.pressed(Inputs.Left)) || Global.Input.repeated(Inputs.Left);
        }

        protected virtual void update_begin() { }

        protected virtual void update_end(int temp_index) { }

        protected abstract void refresh();

        protected virtual void move_down()
        {
            Temp_Index = (Index + 1) % Targets.Count;
            if (Temp_Index != LastTargetIndex)
                Global.game_system.play_se(System_Sounds.Menu_Move2);
            reset_cursor();
        }
        protected virtual void move_up()
        {
            Temp_Index = (Index - 1 + Targets.Count) % Targets.Count;
            if (Temp_Index != LastTargetIndex)
                Global.game_system.play_se(System_Sounds.Menu_Move2);
            reset_cursor();
        }
        protected virtual void move_to(int index)
        {
            Temp_Index = index;
            if (Temp_Index != LastTargetIndex)
                Global.game_system.play_se(System_Sounds.Menu_Move2);
            reset_cursor();
        }

        protected abstract void reset_cursor();

        protected void cursor_move_to(Vector2 loc)
        {
            Global.player.loc = loc;
            get_unit().face(loc);
        }
        
        public bool ChangeIndex(Vector2 loc)
        {
            var index = IndexOfLoc(loc);
            if (index.IsSomething)
            {
                _lastTargetIndex = index;
                move_to(index);
                    
                Global.player.instant_move = true;
                Global.player.force_loc(loc);

                refresh();
                this.index = Temp_Index;
                set_images();

                return true;
            }

            return false;
        }

        protected Maybe<int> IndexOfLoc(Vector2 loc)
        {
            for (int i = 0; i < Targets.Count; i++)
            {
                T target = Targets[i];
                if (target_loc(target) == loc)
                {
                    return i;
                }
            }

            return Maybe<int>.Nothing;
        }

        public ConsumedInput selected_index()
        {
            return SelectedIndex;
        }

        public bool is_selected()
        {
            return SelectedIndex.IsSomething;
        }

        public bool is_canceled()
        {
            return Canceled;
        }

        public void reset_selected()
        {
            SelectedIndex = new ConsumedInput();
            Canceled = false;
        }

        public virtual void accept() { }

        public virtual void cancel() { }

        public abstract void draw(SpriteBatch sprite_batch);
    }
}
