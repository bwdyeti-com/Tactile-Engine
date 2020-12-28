using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using TactileListExtension;

namespace Tactile.Windows.Map.Info
{
    class Window_Terrain_Info : Window_Map_Info
    {
        protected Terrain_Window Window_Img;
        protected TextSprite Name;
        protected RightAdjustedText Def, Avo;
        protected readonly static Vector2 NAME_LOC = new Vector2(16, 6);

        public Window_Terrain_Info()
        {
            //BOTTOM_Y = Config.WINDOW_HEIGHT - (54 + (Global.game_options.controller == 0 ? 16 : 0)); //Debug
            BOTTOM_Y = Math.Max(Config.WINDOW_HEIGHT / 2,
                Config.WINDOW_HEIGHT - (46 + (Global.game_options.controller == 0 ? 16 : 0)));
            RIGHT_X = Math.Max(Config.WINDOW_WIDTH / 2, Config.WINDOW_WIDTH - 40);
            initialize();
            loc = new Vector2(-48, BOTTOM_Y);
            refresh();
        }

        protected override void initialize()
        {
            base.initialize();
            loc = new Vector2(-86, TOP_Y);
        }

        protected override void initialize_images()
        {
            Window_Img = new Terrain_Window();
            Window_Img.offset = new Vector2(5, 5);
            Window_Img.tint = new Color(240, 240, 240, 224);
            Name = new TextSprite();
            Name.SetFont(Config.UI_FONT, Global.Content, "White");
            Def = new RightAdjustedText();
            Def.SetFont(Config.INFO_FONT + "2", Global.Content, null, Config.INFO_FONT);
            Avo = new RightAdjustedText();
            Avo.SetFont(Config.INFO_FONT + "2", Global.Content, null, Config.INFO_FONT);
        }

        protected override void set_images()
        {
            set_images(false);
        }
        public void set_images(bool player_called)
        {
            if (player_called)
                if (map_check() != Map_Info_State.Onscreen)
                    return;
            if (Offscreen)
            {
                while (X_Move_List.Count > 0)
                    update_position();
                move_on();
            }
            draw_images();
        }

        #region Refresh
        protected virtual void draw_images()
        {
            if (Global.game_map.fow && !Constants.Map.FOW_TERRAIN_DATA &&
                !Global.game_map.fow_visibility[Constants.Team.PLAYER_TEAM].Contains(Global.player.loc))
            {
                set_name("--");
                Def.text = "";
                Avo.text = "";
            }
            else
            {
                int id = Global.game_map.terrain_id(Global.player.loc);
                // Name
                set_name(id);
                // Stats
                if (Global.game_map.get_destroyable(Global.player.loc) != null)
                {
                    Window_Img.is_destroyable_object = true;
                    set_destroyable_stats(Global.game_map.get_destroyable(Global.player.loc).hp);
                }
                else if (Global.game_map.get_light_rune(Global.player.loc) != null)
                {
                    Window_Img.is_destroyable_object = true;
                    set_name("Light Rune");
                    set_destroyable_stats(Global.game_map.get_light_rune(Global.player.loc).hp);
                }
                else
                {
                    Window_Img.is_destroyable_object = false;
                    set_stats(id);
                }
            }
        }

        protected void set_name(int id)
        {
            set_name(Global.data_terrains[id].Name);
        }
        protected void set_name(string name)
        {
            Name.text = name;
            Name.offset.X = Name.text_width / 2;
        }

        protected void set_destroyable_stats(int hp)
        {
            Def.text = "";
            Avo.text = hp.ToString();
        }

        protected virtual void set_stats(int id)
        {
            if (Global.data_terrains[id].Stats_Visible)
            {
                Def.text = Global.data_terrains[id].Def.ToString();
                Avo.text = Global.data_terrains[id].Avoid.ToString();
                //if (Global.data_terrains[id].Heal != null) //Debug
                //    Avo.text += "\n" + Global.data_terrains[id].Heal[0].ToString() + "%";
            }
            else
            {
                Def.text = "";
                Avo.text = "";
            }
        }

        protected override void refresh()
        {
            refresh_graphic_object(Window_Img);
            refresh_graphic_object(Name);
            refresh_graphic_object(Def);
            refresh_graphic_object(Avo);
        }
        #endregion

        #region Update
        public override void update()
        {
            Map_Info_State map_ready = map_check();
            if (map_ready == Map_Info_State.Refresh)
                set_images();
            if (Global.player.is_true_on_left() && is_self_on_left && X_Move_List.Count == 0)
            {
                if (!Offscreen)
                    move_off_left();
                else if (map_ready != Map_Info_State.Offscreen)
                    move_on_right();
            }
            else if (Global.player.is_true_on_right() && is_self_on_right && X_Move_List.Count == 0)
            {
                if (!Offscreen)
                    move_off_right();
                else if (map_ready != Map_Info_State.Offscreen)
                    move_on_left();
            }
            update_position();
            refresh();
        }
        #endregion

        #region Movement
        protected override void move_off()
        {
            if (is_self_on_left)
                move_off_left();
            else
                move_off_right();
        }

        protected virtual void move_on()
        {
            if (Global.player.is_true_on_right())
                move_on_left();
            else
                move_on_right();
        }

        protected void move_off_left()
        {
            if (Offscreen) return;
            X_Move_List = new List<int> { LEFT_X - 48, LEFT_X - 48, LEFT_X - 16, LEFT_X - 8 };
            Offscreen = true;
        }

        protected void move_off_right()
        {
            if (Offscreen) return;
            X_Move_List = new List<int> { RIGHT_X + 48, RIGHT_X + 48, RIGHT_X + 16, RIGHT_X + 8 };
            Offscreen = true;
        }

        protected void move_on_left()
        {
            X_Move_List = new List<int> { LEFT_X + 0, LEFT_X + 0, LEFT_X - 8, LEFT_X - 16 };
            Offscreen = false;
        }

        protected void move_on_right()
        {
            X_Move_List = new List<int> { RIGHT_X + 0, RIGHT_X + 0, RIGHT_X + 8, RIGHT_X + 16 };
            Offscreen = false;
        }
        #endregion

        public override void draw(SpriteBatch sprite_batch)
        {
            Window_Img.draw(sprite_batch, new Vector2(3, 3));
            Name.draw(sprite_batch, -NAME_LOC);
            Def.draw(sprite_batch, -(NAME_LOC + new Vector2(16 + 1, 12)));
            Avo.draw(sprite_batch, -(NAME_LOC + new Vector2(16 + 1, 20)));
        }
    }
}
