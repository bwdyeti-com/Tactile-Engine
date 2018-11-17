using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FEXNA
{
    internal abstract class Map_Object
    {
        protected int Id = 0;
        protected Vector2 Loc = Vector2.Zero, Real_Loc = Vector2.Zero;
        protected int Facing = 2, Frame = 0;

        protected int TILE_SIZE { get { return Constants.Map.TILE_SIZE; } }
        protected int UNIT_TILE_SIZE { get { return Constants.Map.UNIT_TILE_SIZE; } }

        #region Accessors
        public int id { get { return Id; } }

        public Vector2 loc { get { return Loc; } }

        public Vector2 real_loc { get { return Real_Loc; } }

        public Vector2 pixel_loc { get { return (real_loc_on_map() / UNIT_TILE_SIZE) * TILE_SIZE; } }
        #endregion

        public bool is_on_square
        {
            get
            {
                return (Loc * UNIT_TILE_SIZE == Real_Loc);
            }
        }

        public virtual void force_loc(Vector2 loc)
        {
            Loc = loc;
            refresh_real_loc();
        }

        protected void refresh_real_loc()
        {
            Real_Loc = Loc * UNIT_TILE_SIZE;
        }

        protected virtual Vector2 real_loc_on_map()
        {
            return Real_Loc;
        }
        public virtual Vector2 loc_on_map()
        {
            return Loc;
        }

        public bool visible_by()
        {
            return visible_by(Constants.Team.PLAYER_TEAM);
        }
        public virtual bool visible_by(int team)
        {
            return true;
        }

        #region Position Testing
        public bool is_on_bottom()
        {
            return is_on_bottom(0);
        }
        public bool is_on_bottom(int offset)
        {
            return !is_on_top(offset);
        }

        public bool is_on_left()
        {
            return is_on_left(0);
        }
        public bool is_on_left(int offset)
        {
            int center_x = Config.WINDOW_WIDTH / 2;
            return (pixel_loc.X - Global.game_map.display_x + offset < center_x);
        }

        public bool is_on_right()
        {
            return is_on_right(0);
        }
        public bool is_on_right(int offset)
        {
            return !is_on_left(offset);
        }

        public bool is_on_top()
        {
            return is_on_top(0);
        }
        public bool is_on_top(int offset)
        {
            int center_y = Config.WINDOW_HEIGHT / 2;
            return (pixel_loc.Y - Global.game_map.display_y + offset < center_y);
        }

        public bool is_true_on_bottom()
        {
            return is_true_on_bottom(0);
        }
        public bool is_true_on_bottom(int offset)
        {
            return !is_true_on_top(offset);
        }

        public bool is_true_on_left()
        {
            return is_true_on_left(0);
        }
        public bool is_true_on_left(int offset)
        {
            int center_x = Config.WINDOW_WIDTH / 2;
            return (Loc.X * TILE_SIZE - Global.game_map.display_x + offset < center_x);
        }

        public bool is_true_on_right()
        {
            return is_true_on_right(0);
        }
        public bool is_true_on_right(int offset)
        {
            return !is_true_on_left(offset);
        }

        public bool is_true_on_top()
        {
            return is_true_on_top(0);
        }
        public bool is_true_on_top(int offset)
        {
            int center_y = Config.WINDOW_HEIGHT / 2;
            return (Loc.Y * TILE_SIZE - Global.game_map.display_y + offset < center_y);
        }

        public bool is_in_center(int offset = 0)
        {
            float y = Loc.Y * TILE_SIZE - Global.game_map.display_y + offset;
            int center_y = Config.WINDOW_HEIGHT / 2;
            return y > (center_y - Config.WINDOW_HEIGHT / 12) &&
                y < (center_y + Config.WINDOW_HEIGHT / 12);
        }
        #endregion

        public int terrain_id()
        {
            return Global.game_map.terrain_id(Loc);
        }

        #region Sprite Handling
        public virtual void update_sprite(Sprite sprite)
        {
            Vector2 pixel_loc = this.pixel_loc;
            sprite.loc = new Vector2((int)pixel_loc.X, (int)pixel_loc.Y);
        }
        #endregion
    }

    internal abstract class Combat_Map_Object : Map_Object
    {
        #region Accessors
        public abstract int maxhp { get; }
        public abstract int hp { get; set; }
        public abstract bool is_dead { get; }

        public virtual int def { get { return 0; } }

        public abstract string name { get; }

        public abstract int team { get; }
        #endregion

        public virtual bool is_unit()
        {
            return false;
        }

        public virtual void combat_damage(int dmg, Combat_Map_Object attacker, List<KeyValuePair<int, bool>> states, bool backfire, bool test)
        {
            hp -= dmg;
        }

        public virtual bool is_attackable_team(int other_team)
        {
            return true;
        }

        public bool is_player_allied
        {
            get
            {
                return !is_attackable_team(Constants.Team.PLAYER_TEAM);
            }
        }

        internal void hit_rumble(bool no_damage, int dmg, bool crit)
        {
            // Ramble
            if (no_damage)
            {
                Global.Rumble.add_rumble(TimeSpan.FromSeconds(0.2f), 0, 0.5f);
            }
            else
            {
                if (dmg > 0)
                {
                    float rumble_force = MathHelper.Lerp(crit ? 0.75f : 0.33f, 1f, MathHelper.Clamp(dmg / (float)maxhp, 0, 1));
                    float duration = MathHelper.Lerp(0.3f, 0.9f, MathHelper.Clamp(dmg / (float)maxhp, 0, 1));
                    // If the unit isn't on the player's team, shake less on the low frequency motor
                    if (!is_player_allied)
                        Global.Rumble.add_rumble(TimeSpan.FromSeconds(duration), rumble_force / 3, rumble_force, mult: 0.8f);
                    else
                        Global.Rumble.add_rumble(TimeSpan.FromSeconds(duration), rumble_force, rumble_force, mult: 0.8f);
                }
            }
        }
    }
}
