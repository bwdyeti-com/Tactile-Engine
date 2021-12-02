using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows;

namespace Tactile
{
    class Hand_Cursor : Matrix_Position_Sprite
    {
        public const string FILENAME = @"Graphics/Windowskins/Menu_Hand";

        protected List<Vector2> Locs = new List<Vector2>();
        protected float Min_Distance_X = 0.5f;
        protected float Min_Distance_Y = 0.5f;
        protected float Override_Distance_X = 0.5f;
        protected float Override_Distance_Y = 0.5f;
        protected bool Still_Cursor = false;
        protected int[] Ratio = RATIO;

        #region Accessors
        public float min_distance_x { set { Min_Distance_X = value; } }
        public float min_distance_y { set { Min_Distance_Y = value; } }

        public float override_distance_x { set { Override_Distance_X = value; } }
        public float override_distance_y { set { Override_Distance_Y = value; } }

        public bool still_cursor { set { Still_Cursor = value; } }

        public int[] ratio
        {
            set
            {
                if (value.Length == 2)
                    Ratio = value;
            }
        }

        internal Vector2 target_loc { get { return Locs.Any() ? Locs.Last() : this.loc; } }

        internal bool is_moving { get { return Locs.Any(); } }
        #endregion

        public Hand_Cursor()
        {
            texture = Global.Content.Load<Texture2D>(FILENAME);
            Src_Rect = new Rectangle(0, 0, 16, 16);
            offset = new Vector2(0, -2);
        }

        readonly static int[] RATIO = { 3, 1 };
        public void set_loc(Vector2 new_loc)
        {
            Locs.Clear();
            Locs.Add((loc * Ratio[0] + new_loc * Ratio[1]) / (Ratio[0] + Ratio[1]));
            for (; ; )
            {
                // X
                if (Math.Abs(Locs[Locs.Count - 1].X - new_loc.X) <= Min_Distance_X)
                    Locs[Locs.Count - 1] = new Vector2(new_loc.X, Locs[Locs.Count - 1].Y);
                else if (Math.Abs(Locs[Locs.Count - 1].X - new_loc.X) <= Override_Distance_X)
                    Locs[Locs.Count - 1] = new Vector2(
                        (float)Additional_Math.double_closer(Locs[Locs.Count - 1].X, new_loc.X, Min_Distance_X), Locs[Locs.Count - 1].Y);
                // Y
                if (Math.Abs(Locs[Locs.Count - 1].Y - new_loc.Y) <= Min_Distance_Y)
                    Locs[Locs.Count - 1] = new Vector2(Locs[Locs.Count - 1].X, new_loc.Y);
                else if (Math.Abs(Locs[Locs.Count - 1].Y - new_loc.Y) <= Override_Distance_Y)
                    Locs[Locs.Count - 1] = new Vector2(
                        Locs[Locs.Count - 1].X, (float)Additional_Math.double_closer(Locs[Locs.Count - 1].Y, new_loc.Y, Min_Distance_Y));

                if (Locs[Locs.Count - 1].X == new_loc.X && Locs[Locs.Count - 1].Y == new_loc.Y)
                    break;

                Locs.Add((new Vector2(Locs[Locs.Count - 1].X, Locs[Locs.Count - 1].Y) * Ratio[0] +
                    new_loc * Ratio[1]) / (Ratio[0] + Ratio[1]));
            }
        }
        public void force_loc(Vector2 new_loc)
        {
            Locs.Clear();
            loc = new Vector2((int)new_loc.X, (int)new_loc.Y);
        }

        public override void update()
        {
            if (Locs.Count > 0)
            {
                loc = new Vector2((int)Locs[0].X, (int)Locs[0].Y);
                Locs.RemoveAt(0);
            }
            int x = 0;
            if (!Still_Cursor)
            {
                int count = Player.cursor_anim_count;
                if (count >= 0 && count < 7)
                    x = -2;
                else if (count >= 7 && count < 9)
                    x = -1;
                else if (count >= 9 && count < 12)
                    x = 0;
                else if (count >= 12 && count < 16)
                    x = 1;
                else if (count >= 16 && count < 23)
                    x = 2;
                else if (count >= 23 && count < 25)
                    x = 1;
                else if (count >= 25 && count < 28)
                    x = 0;
                else if (count >= 28 && count < 32)
                    x = -1;
                offset.X = -x;
            }
        }

        public void draw(SpriteBatch sprite_batch, IndexScrollComponent scroll, Vector2 draw_offset = default(Vector2))
        {
            Vector2 scrollOffset = scroll.ClampOffset(this.loc);
            base.draw(sprite_batch, draw_offset - scrollOffset);
        }
    }
}
