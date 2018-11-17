using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    class Worldmap_Arrow : Flashing_Worldmap_Object
    {
        public const float MOVEMENT_SPEED_DIVISOR = 16f;
        const float ARROW_TURN_SPEED = MathHelper.Pi / 32;
        const float DOTS_PER_LENGTH = 1 / 12f;
        protected readonly static Vector2 DOT_OFFSET = new Vector2(4, 4);

        protected Vector2[] Waypoints;
        protected float Movement_Speed;
        protected float Length, Total_Length = 0;
        protected Vector2[] Points;
        protected Rectangle Dot_Src_Rect;

        public Worldmap_Arrow(int team, int speed, Vector2[] waypoints)
        {
            Team = team;
            Movement_Speed = speed / MOVEMENT_SPEED_DIVISOR;
            Waypoints = waypoints;
            offset = new Vector2(8, 8);
            for (int i = 1; i < Waypoints.Length; i++)
                Total_Length += (Waypoints[i] - Waypoints[i - 1]).Length();
            Points = new Vector2[(int)(Total_Length * DOTS_PER_LENGTH)];
            angle = angle_along_route(Length, Waypoints);
        }

        public override void update()
        {
            Src_Rect = new Rectangle(8 + (Team - 1) * 24, Flash_Frame * 16, 16, 16);
            Dot_Src_Rect = new Rectangle((Team - 1) * 24, 8 + Flash_Frame * 16, 8, 8);
            if (Length < Total_Length)
                Length += Movement_Speed;
            else
                Length = Total_Length;
            for (int i = 0; i < Points.Length; i++)
                Points[i] = point_along_route((Length * i) / Points.Length, Waypoints);
            loc = point_along_route(Length, Waypoints);
            float arrow_angle = angle_along_route(Length + Movement_Speed * 4, Waypoints);
            if (angle != arrow_angle)
            {
                if (Math.Abs(arrow_angle - angle) > MathHelper.Pi)
                    if (angle < 0)
                        angle += MathHelper.TwoPi;
                    else if (angle > 0)
                        angle -= MathHelper.TwoPi;
                angle = (float)Additional_Math.double_closer(angle, arrow_angle, ARROW_TURN_SPEED);
            }
        }

        public static Vector2 point_along_route(float length, Vector2[] waypoints)
        {
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                Vector2 waypoint_vector = waypoints[i + 1] - waypoints[i];
                if (length < waypoint_vector.Length())
                {
                    waypoint_vector.Normalize();
                    waypoint_vector *= length;
                    return new Vector2((int)(waypoints[i] + waypoint_vector).X, (int)(waypoints[i] + waypoint_vector).Y);
                }
                else
                    length -= waypoint_vector.Length();
            }
            return waypoints[waypoints.Length - 1];
        }

        public static float angle_along_route(float length, Vector2[] waypoints)
        {
            Vector2 waypoint_vector = Vector2.Zero;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                waypoint_vector = waypoints[i + 1] - waypoints[i];
                if (length < waypoint_vector.Length())
                {
                    return (float)Math.Atan2(waypoint_vector.Y, waypoint_vector.X);
                }
                else
                    length -= waypoint_vector.Length();
            }
            return (float)Math.Atan2(waypoint_vector.Y, waypoint_vector.X);
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {

                    for (int i = 0; i < Points.Length; i++)
                    {
                        sprite_batch.Draw(texture, (Points[i] + draw_vector()) - draw_offset,
                            Dot_Src_Rect, tint, 0f, DOT_OFFSET, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    }

                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    sprite_batch.Draw(texture, (this.loc + draw_vector()) - draw_offset,
                        src_rect, tint, angle, offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                }
        }
    }
}
