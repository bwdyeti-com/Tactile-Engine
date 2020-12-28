using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using ListExtension;

namespace Tactile
{
    enum Worldmap_Unit_Queue { Move, Idle, Pose, Remove, Track }
    class Worldmap_Unit : Stereoscopic_Graphic_Object
    {
        const int FADE_TIME = 32;
        readonly static Vector2 WORLDMAP_UNIT_OFFSET = new Vector2(0, 2);

        protected static Vector2 Tracking_Unit_Min, Tracking_Unit_Max;

        protected int Team;
        protected string Filename;
        protected int Frame, Facing;
        protected int Moving_Anim, Highlighted_Anim;
        protected int Fade_Timer = FADE_TIME;
        protected List<int> Color_List = new List<int>();
        protected List<int> Opacity_List = new List<int>();
        protected Color Unit_Color = Color.Transparent;
        protected int Opacity = 255;
        protected bool Remove;
        protected bool Highlighted;
        protected bool Tracking;
        protected Vector2[] Waypoints;
        protected float Movement_Speed;
        protected float Waypoint_Length, Waypoint_Total_Length = 0;
        protected List<KeyValuePair<Worldmap_Unit_Queue, object>> Unit_Queue = new List<KeyValuePair<Worldmap_Unit_Queue,object>>();

        protected Character_Sprite Unit_Sprite;

        #region Accessors
        public static Vector2 tracking_unit_min { get { return Tracking_Unit_Min; } }
        public static Vector2 tracking_unit_max { get { return Tracking_Unit_Max; } }

        public bool is_removed { get { return Remove; } }
        public bool finished { get { return Remove && (Fade_Timer == 0 && !Color_List.Any() && !Opacity_List.Any()); } }

        public bool tracking { get { return Tracking; } }

        public bool moving { get { return Waypoints != null; } }
        #endregion

        public Worldmap_Unit(int team, string filename)
        {
            Team = team;
            Filename = filename;
            Unit_Sprite = new Character_Sprite();
            Unit_Sprite.draw_offset = WORLDMAP_UNIT_OFFSET;
            Scene_Map.refresh_map_sprite(Unit_Sprite, Team, Filename, false);
        }

        public void queue_move(int speed, Vector2[] waypoints)
        {
            Unit_Queue.Add(new KeyValuePair<Worldmap_Unit_Queue, object>(Worldmap_Unit_Queue.Move,
                new KeyValuePair<float, Vector2[]>(speed / Worldmap_Arrow.MOVEMENT_SPEED_DIVISOR, waypoints)));
        }

        public void queue_idle()
        {
            Unit_Queue.Add(new KeyValuePair<Worldmap_Unit_Queue, object>(Worldmap_Unit_Queue.Idle, null));
        }

        public void queue_pose()
        {
            Unit_Queue.Add(new KeyValuePair<Worldmap_Unit_Queue, object>(Worldmap_Unit_Queue.Pose, null));
        }

        public void queue_remove(bool kill = false)
        {
            Unit_Queue.Add(new KeyValuePair<Worldmap_Unit_Queue, object>(Worldmap_Unit_Queue.Remove, kill));
        }

        public void remove_if_queued()
        {
            int i = 0;
            while (i < Unit_Queue.Count)
            {
                if (Unit_Queue[i].Key == Worldmap_Unit_Queue.Remove)
                {
                    remove((bool)Unit_Queue[i].Value);
                    Unit_Queue.RemoveAt(i);
                    break;
                }
                else
                    i++;
            }
        }

        public void queue_tracking(Vector2 min, Vector2 max)
        {
            Unit_Queue.Add(new KeyValuePair<Worldmap_Unit_Queue, object>(Worldmap_Unit_Queue.Track, new Vector2[] { min, max }));
        }
        public void remove_all_tracking()
        {
            Unit_Queue = Unit_Queue.Where(x => x.Key != Worldmap_Unit_Queue.Track).ToList();
        }

        public void remove(bool kill)
        {
            Remove = true;
            if (kill)
            {
                Unit_Color.R = 255;
                Unit_Color.G = 255;
                Unit_Color.B = 255;
                Color_List = new List<int> { 0, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 255 };
                Opacity_List = new List<int> { 0,10,20,30,40,50,60,70,80,90,100,110,120,130,140,150,
                    160,170,180,190,200,210,220,230,240,255 };
            }
            else
                Fade_Timer = FADE_TIME;
        }

        public void update()
        {
            if (Fade_Timer > 0)
            {
                Fade_Timer--;
                int alpha = ((Remove ? Fade_Timer : (FADE_TIME - Fade_Timer)) * 255) / FADE_TIME;
                Unit_Sprite.tint = new Color(alpha, alpha, alpha, 255);
            }
            if (Color_List.Count > 0)
                Unit_Color.A = (byte)Color_List.pop();
            if (Opacity_List.Count > 0)
                Opacity = Opacity_List.pop();

            Tracking = false;
            if (!moving)
                process_unit_queue();
            update_movement();
            update_frame();
        }

        protected void update_movement()
        {
            if (moving)
            {
                Waypoint_Length += Movement_Speed;
                loc = Worldmap_Arrow.point_along_route(Waypoint_Length, Waypoints);
                if (loc == Waypoints[Waypoints.Length - 1])
                {
                    if (Unit_Queue.Count == 0)
                        Highlighted = false;
                    Waypoints = null;
                    Scene_Map.refresh_map_sprite(Unit_Sprite, Team, Filename, false);
                    process_unit_queue();
                    return;
                }
                else if (Unit_Queue.Count > 0 && Unit_Queue[0].Key == Worldmap_Unit_Queue.Remove &&
                    Worldmap_Arrow.point_along_route(Waypoint_Length, Waypoints) == Waypoints[Waypoints.Length - 1])
                {
                    remove((bool)Unit_Queue[0].Value);
                    Unit_Queue.RemoveAt(0);
                }
                update_movement_facing();
            }
        }

        protected void update_movement_facing()
        {
            float angle = Worldmap_Arrow.angle_along_route(Waypoint_Length, Waypoints);
            int deg = ((int)(360 - angle * 360 / MathHelper.TwoPi)) % 360;

            if (deg > 45 && deg < 135)
                Facing = 8;
            else if (deg >= 135 && deg <= 225)
                Facing = 4;
            else if (deg > 225 && deg < 315)
                Facing = 2;
            else
                Facing = 6;
        }

        protected void update_frame()
        {
            if (moving)
            {
                update_move_animation();
            }
            else if (Highlighted)
            {
                update_highlighted_animation();
            }
            else
            {
                update_idle_animation();
            }
            Map_Object.UpdateSpriteFrame((Graphics.Map.Character_Sprite)Unit_Sprite, Facing, Frame);
            Unit_Sprite.mirrored = Constants.Team.flipped_map_sprite(Team) &&
                (!moving || (Facing != 4 && Facing != 6));
        }

        protected void update_idle_animation()
        {
            Moving_Anim = -1;
            Highlighted_Anim = -1;
            Facing = 2;

            Frame = Global.game_system.unit_anim_idle_frame;
        }
        protected void update_highlighted_animation()
        {
            Moving_Anim = -1;
            Highlighted_Anim = (Highlighted_Anim + 1) % Global.game_system.Unit_Highlight_Anim_Time;
            Facing = 6;

            int anim_count = Highlighted_Anim;
            int index = 0;
            for (int i = 0; i < Config.CHARACTER_HIGHLIGHT_ANIM_TIMES.Length; i++)
            {
                if (anim_count < Config.CHARACTER_HIGHLIGHT_ANIM_TIMES[i])
                {
                    index = i;
                    break;
                }
                else
                    anim_count -= Config.CHARACTER_HIGHLIGHT_ANIM_TIMES[i];
            }
            Frame = Config.CHARACTER_HIGHLIGHT_ANIM_FRAMES[index];
        }
        protected void update_move_animation()
        {
            Moving_Anim = (Moving_Anim + 1) % Global.game_system.Unit_Moving_Anim_Time;
            Highlighted_Anim = -1;

            int anim_count = Moving_Anim;
            int index = 0;
            for (int i = 0; i < Config.CHARACTER_MOVING_ANIM_TIMES.Length; i++)
            {
                if (anim_count < Config.CHARACTER_MOVING_ANIM_TIMES[i])
                {
                    index = i;
                    break;
                }
                else
                    anim_count -= Config.CHARACTER_MOVING_ANIM_TIMES[i];
            }
            Frame = Config.CHARACTER_MOVING_ANIM_FRAMES[index];
        }

        protected void process_unit_queue()
        {
            while (Unit_Queue.Count > 0)
            {
                var unit_action = Unit_Queue[0];
                Unit_Queue.RemoveAt(0);
                switch (unit_action.Key)
                {
                    case Worldmap_Unit_Queue.Move:
                        Waypoint_Length = 0;
                        Movement_Speed = ((KeyValuePair<float, Vector2[]>)unit_action.Value).Key;
                        Vector2[] waypoints = ((KeyValuePair<float, Vector2[]>)unit_action.Value).Value;
                        Waypoints = new Vector2[waypoints.Length + 1];
                        Waypoints[0] = loc;
                        for (int i = 0; i < waypoints.Length; i++)
                            Waypoints[i + 1] = waypoints[i];
                        for (int i = 1; i < Waypoints.Length; i++)
                            Waypoint_Total_Length += (Waypoints[i] - Waypoints[i - 1]).Length();
                        Scene_Map.refresh_map_sprite(Unit_Sprite, Team, Filename, true);
                        return;
                    case Worldmap_Unit_Queue.Idle:
                        Highlighted = false;
                        break;
                    case Worldmap_Unit_Queue.Pose:
                        Highlighted = true;
                        break;
                    case Worldmap_Unit_Queue.Remove:
                        remove((bool)unit_action.Value);
                        break;
                    case Worldmap_Unit_Queue.Track:
                        Tracking_Unit_Min = ((Vector2[])unit_action.Value)[0];
                        Tracking_Unit_Max = ((Vector2[])unit_action.Value)[1];
                        Tracking = true;
                        break;
                }
            }
        }

        public void set_sprite_batch_effects(Effect effect)
        {
            effect.Parameters["color_shift"].SetValue(Unit_Color.ToVector4());
            effect.Parameters["opacity"].SetValue(Opacity / 255f);
            //effect.Parameters["color_shift"].SetValue(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
        }

        public void draw(SpriteBatch sprite_batch, Vector2 offset)
        {
            Unit_Sprite.draw(sprite_batch, offset - (loc + draw_vector()));
        }
    }
}
