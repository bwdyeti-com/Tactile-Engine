using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using ListExtension;

namespace Tactile.Windows.Map.Info
{
    class Window_Unit_Info_Burst : Window_Unit_Info_Panel
    {
        protected bool Enemy_Info = false;

        #region Accessors
        public bool enemy_info { set { Enemy_Info = value; } }
        #endregion

        public Window_Unit_Info_Burst()
        {
            BOTTOM_Y = Math.Max(Config.WINDOW_HEIGHT / 2, Config.WINDOW_HEIGHT - 56);
            NAME_LOC = new Vector2(56, 0);
            Window_Width = 80;
            Window_Height = 32;
            initialize();
            loc = new Vector2(-(Window_Width + 8), TOP_Y);
            refresh();
        }

        protected override void initialize_images()
        {
            Window_Img = new Unit_Window(Window_Width, Window_Height, 1);
            Window_Img.offset = new Vector2(5, 5);
            Window_Img.tint = new Color(240, 240, 240, 224);
            Face = new Miniface();
            Face.draw_offset = new Vector2(16, 0);
            init_hp_gauge();
            Name = new TextSprite();
            Name.SetFont(Config.INFO_FONT, Global.Content);
        }

        public override void set_images(bool player_called)
        {
            if (player_called)
                if (map_check() != Map_Info_State.Onscreen)
                    return;
            Game_Unit unit = get_unit;
            bool valid_unit = unit != null && unit.visible_by();
            if (valid_unit && Enemy_Info)
            {
                var selected_unit = Global.game_map.get_selected_unit();
                if (selected_unit == null)
                    valid_unit &= !unit.is_player_allied;
                else
                    valid_unit &= unit.is_attackable_team(selected_unit);

            }
            if (valid_unit)
            {
                move_on(); // This needs to come on somewhere else, it doesn't care about move list count currently //Yeti
                draw_images(unit);
            }
            else
                move_off();
        }

        protected override Map_Info_State map_check()
        {
            if (!(Enemy_Info ? Global.game_state.is_enemy_info_ready :
                Global.game_state.is_info_ready))
            {
                if (Map_Busy_Timer <= 0)
                {
                    Map_Busy_Timer = 1;
                    move_off();
                }
                return Map_Info_State.Offscreen;
            }
            else if (Global.game_map.scrolling)
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

        #region Refresh
        protected override void draw_images(Game_Unit unit)
        {
            Window_Img.team = unit.team;
            // Face
            Face.set_actor(unit.actor);
            if (!unit.actor.generic_face)
                Face.mirrored = unit.has_flipped_face_sprite;
            // Name
            set_name(unit);
            // HP
            Hp_Gauge.set_val(unit.actor.hp, unit.actor.maxhp);
        }

        protected override void set_name(Game_Unit unit)
        {
            Name.text = unit.actor.name;
            Name.offset = new Vector2(Font_Data.text_width(Name.text, Config.INFO_FONT) / 2, 0);
        }

        protected override void refresh()
        {
            refresh_graphic_object(Window_Img);
            refresh_graphic_object(Face);
            refresh_graphic_object(Name);
        }
        #endregion

        #region Update
        public override void update()
        {
            base.update();
        }

        protected override bool update_position_switch() { return false; }

        protected override void update_position(Map_Info_State map_ready)
        {
            if (X_Move_List.Count > 0 && map_ready == Map_Info_State.Onscreen)
            {
                X_Move_List.pop();
                if (X_Move_List.Count == 0)
                {
                    fix_position();
                }
            }
            if (Y_Move_List.Count > 0)
                Y_Move_List.pop();
        }

        protected void fix_position()
        {
            bool on_top;
            // If near the center, position toward the side of the screen with less space because there's room?
            if (Global.player.is_in_center(8))
            {
                on_top = Global.player.is_true_on_top();
            }
            // Else, position away from the edge
            else
            {
                on_top = Global.player.is_true_on_bottom();
            }

            loc = Global.player.loc_on_map() * Constants.Map.TILE_SIZE - Global.game_map.display_loc;
            if (on_top)
            {
                Window_Img.tail = Unit_Burst_Tail.Bottom;
                loc += new Vector2(0, -(Window_Height + 16 - 4));
            }
            else
            {
                Window_Img.tail = Unit_Burst_Tail.Top;
                loc += new Vector2(0, 32 - 4);
            }

            if (Config.WINDOW_WIDTH - (int)loc.X <= Window_Width)
            {
                Window_Img.tail |= Unit_Burst_Tail.Right;
                loc += new Vector2(-(Window_Width - 8), 0);
            }
            else if ((int)loc.X == 0)
            {
                Window_Img.tail |= Unit_Burst_Tail.Left;
                loc += new Vector2(8, 0);
            }
            else
            {
                Window_Img.tail |= Unit_Burst_Tail.Middle;
                loc += new Vector2(0, 0);
            }
        }
        #endregion

        #region Movement
        public override void go_offscreen()
        {
            move_off();
            X_Move_List.Clear();
            Y_Move_List.Clear();
            refresh();
        }

        protected override void move_off(bool also_y_move)
        {
            if (Offscreen)
                return;
            X_Move_List = new List<int> { 0 };
            Offscreen = true;
        }

        protected override void move_on()
        {
            Offscreen = false;
            X_Move_List = new List<int> { 0, 0, 0, 0, 0 };
        }
        #endregion

        #region Draw
        protected override bool is_onscreen_for_drawing
        {
            get
            {
                return !Offscreen && X_Move_List.Count == 0;
            }
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            if (is_onscreen_for_drawing)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Window_Img.draw(sprite_batch, new Vector2(3, 3) - draw_vector());
                draw_hp(sprite_batch);
                Name.draw(sprite_batch, -NAME_LOC);
                sprite_batch.End();
                Face.draw(sprite_batch);

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                //Stats.draw(sprite_batch);
                sprite_batch.End();
            }
        }
        #endregion
    }
}
