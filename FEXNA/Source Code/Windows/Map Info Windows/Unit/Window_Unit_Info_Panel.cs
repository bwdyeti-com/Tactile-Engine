using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using ListExtension;

namespace FEXNA.Windows.Map.Info
{
    class Window_Unit_Info_Panel : Window_Map_Info
    {
        protected Unit_Window Window_Img;
        protected Miniface Face;
        protected Unit_Info_Hp_Gauge Hp_Gauge;
        protected FE_Text Name;
        protected int Window_Width = 80, Window_Height = 32;
        protected Vector2 NAME_LOC = new Vector2(56, 0);

        public Window_Unit_Info_Panel()
        {
            BOTTOM_Y = Math.Max(Config.WINDOW_HEIGHT / 2,
                BOTTOM_Y - (Global.game_options.controller == 0 ? 16 : 0));
            initialize();
            refresh();
        }

        protected override void initialize()
        {
            base.initialize();
            loc = new Vector2(-86, TOP_Y);
        }

        protected override void initialize_images()
        {
            Window_Img = new Unit_Window(Window_Width, Window_Height, 1);
            Window_Img.offset = new Vector2(5, 5);
            Window_Img.tint = new Color(240, 240, 240, 224);
            Face = new Miniface();
            Face.draw_offset = new Vector2(16, 0);
            init_hp_gauge();
            Name = new FE_Text();
            Name.Font = "FE7_Text_Info";
            Name.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Info");
        }

        protected virtual void init_hp_gauge()
        {
            Hp_Gauge = new Unit_Info_Hp_Gauge();
        }

        protected override void set_images()
        {
            set_images(false);
        }
        public virtual void set_images(bool player_called)
        {
            if (player_called)
                if (map_check() != Map_Info_State.Onscreen)
                    return;
            Game_Unit unit = get_unit;
            if (unit != null && unit.visible_by())
            {
                if (!update_position_switch())
                {
                    if (Offscreen) move_on(); // This needs to come on somewhere else, it doesn't care about move list count currently //Yeti
                    draw_images(unit);
                }
            }
            else
            {
                // Switching vertical positions will handle moving off, so only move off if it doesn't happen
                if (!update_position_switch())
                    move_off();
            }
        }

        #region Refresh
        protected virtual void draw_images(Game_Unit unit)
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

        protected virtual void set_name(Game_Unit unit)
        {
            Name.text = unit.actor.name;
            Name.offset.X = Font_Data.text_width(Name.text, "FE7_Text_Info") / 2;
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
            Map_Info_State map_ready = map_check();
            update_position_switch();
            if (map_ready == Map_Info_State.Refresh)
                set_images();
            update_position(map_ready);
            refresh();
        }

        protected virtual bool update_position_switch()
        {
            if (Global.player.is_true_on_top() && Global.player.is_true_on_left() && is_self_on_top && Y_Move_List.Count == 0)
            {
                move_to_bottom();
                move_off(true);
                return true;
            }
            else if ((Global.player.is_true_on_bottom() || Global.player.is_true_on_right()) && is_self_on_bottom && Y_Move_List.Count == 0)
            {
                move_to_top();
                move_off(true);
                return true;
            }
            return false;
        }

        protected override void update_position()
        {
            update_position(Map_Info_State.Offscreen);
        }
        protected virtual void update_position(Map_Info_State map_ready)
        {
            if (X_Move_List.Count > 0)
                loc.X = X_Move_List.pop();
            if (Y_Move_List.Count > 0)
            {
                loc.Y = Y_Move_List.pop();
                if (Y_Move_List.Count == 0 && map_ready == Map_Info_State.Onscreen)
                    set_images();
            }
        }
        #endregion

        #region Movement
        protected override void move_off()
        {
            move_off(false);
        }
        protected virtual void move_off(bool also_y_move)
        {
            if (Offscreen)
                return;
            X_Move_List = new List<int> { -86, LEFT_X - 40, LEFT_X - 8, LEFT_X };
            if (!also_y_move)
            {
                for (int i = 0; i < Y_Move_List.Count - 1; i++)
                    X_Move_List.Add(-86); //should this be 5, it seems odd // was it 5 before and fixed, or should it be switched to 5 now? //Yeti
            }
            Offscreen = true;
        }

        protected virtual void move_on()
        {
            update_position_switch();
            Offscreen = false;
            X_Move_List = new List<int> { LEFT_X, LEFT_X - 8, LEFT_X - 24, LEFT_X - 56 };
            for (int i = 0; i < Y_Move_List.Count - 1; i++)
                X_Move_List.Add(-86);
        }

        protected void move_to_bottom()
        {
            int y1 = (int)loc.Y;
            int y2 = BOTTOM_Y;
            if (Offscreen)
                Y_Move_List = new List<int> { y2 };
            else
                Y_Move_List = new List<int> { y2, y1, y1, y1, y1 };
        }

        protected void move_to_top()
        {
            int y1 = (int)loc.Y;
            int y2 = TOP_Y;
            if (Offscreen)
                Y_Move_List = new List<int> { y2 };
            else
                Y_Move_List = new List<int> { y2, y1, y1, y1, y1 };
        }
        #endregion

        protected virtual bool is_onscreen_for_drawing
        {
            get
            {
                return !(Offscreen && Y_Move_List.Count == 0 && X_Move_List.Count == 0);
            }
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            if (is_onscreen_for_drawing) // Add something like this to the other info windows //Yeti
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Window_Img.draw(sprite_batch, new Vector2(3, 3) - draw_vector());
                sprite_batch.End();
                Face.draw(sprite_batch);

                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                draw_hp(sprite_batch);
                Name.draw(sprite_batch, -NAME_LOC);
                sprite_batch.End();
            }
        }

        protected virtual void draw_hp(SpriteBatch sprite_batch)
        {
            Hp_Gauge.draw(sprite_batch, -(loc + draw_vector()));
        }
    }
}
