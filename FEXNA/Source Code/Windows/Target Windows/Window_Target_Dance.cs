using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;

namespace FEXNA.Windows.Target
{
    class Window_Target_Dance : Window_Target_Unit
    {
        SystemWindowHeadered Window;
        Character_Sprite Target_Sprite;
        List<FE_Text> Stat_Labels;
        List<FE_Text> Stats;
        FE_Text Name;

        protected override int window_width
        {
            get { return 80; }
        }

        public Window_Target_Dance(int unit_id, bool using_ring, Vector2 loc)
        {
            initialize(loc);
            Right_X = Config.WINDOW_WIDTH - this.window_width;
            Unit_Id = unit_id;
            List<int> targets = get_targets(using_ring);
            Targets = sort_targets(targets);
            this.index = 0;
            Temp_Index = this.index;
            Game_Unit target = Global.game_map.units[this.target];
            cursor_move_to(target);
            Global.player.instant_move = true;
            Global.player.update_movement();
            initialize_images();
            refresh();
            index = this.index;
        }

        protected List<int> get_targets(bool using_ring)
        {
            Game_Unit unit = get_unit();
            List<int> result = new List<int>();
            List<int> allies = unit.allies_in_range(1);
            foreach(int id in allies)
                if (Global.game_map.units[id].same_team(unit) && (!Global.game_map.units[id].ready || using_ring))
                    result.Add(id);
            return result;
        }

        protected void initialize_images()
        {
            // Window
            Window = new SystemWindowHeadered();
            Window.width = this.window_width;
            Window.height = 48;
            // Target Sprite
            Target_Sprite = new Character_Sprite();
            Target_Sprite.draw_offset = new Vector2(20, 24);
            Target_Sprite.facing_count = 3;
            Target_Sprite.frame_count = 3;
            // Names
            Name = new FE_Text();
            Name.Font = "FE7_Text";
            Name.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            // Stat Labels
            Stat_Labels = new List<FE_Text>();
            for (int i = 0; i < 1; i++)
            {
                Stat_Labels.Add(new FE_Text());
                Stat_Labels[i].offset = new Vector2(-8, -(24 + (i * 16)));
                Stat_Labels[i].Font = "FE7_Text";
                Stat_Labels[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            }
            Stat_Labels[0].Font = "FE7_TextL";
            Stat_Labels[0].text = "HP";
            set_images();
        }

        protected override void set_images()
        {
            Game_Unit unit = get_unit();
            Game_Actor actor1 = unit.actor;
            Game_Unit target = Global.game_map.units[this.target];
            Game_Actor actor2 = target.actor;
            // Get weapon data // This is dancing, why do we need weapons? //Yeti
            FEXNA_Library.Data_Weapon weapon1 = actor1.weapon, weapon2 = actor2.weapon;
            // Map Sprite //
            Target_Sprite.texture = Scene_Map.get_team_map_sprite(target.team, target.map_sprite_name);
            Target_Sprite.offset = new Vector2(
                (Target_Sprite.texture.Width / Target_Sprite.frame_count) / 2,
                (Target_Sprite.texture.Height / Target_Sprite.facing_count) - 8);
            Target_Sprite.mirrored = target.has_flipped_map_sprite;
            // Sets Name //
            Name.offset = new Vector2(-(32), -(8));
            Name.text = actor2.name;

            Stats = new List<FE_Text>();
            for (int i = 0; i < 3; i++)
            {
                Stats.Add(i % 2 == 0 ? new FE_Text_Int() : new FE_Text());
                Stats[i].offset = new Vector2(-24 - ((i / 2) * 24), -24);
                Stats[i].Font = "FE7_Text";
                Stats[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + (i % 2 == 0 ? "Blue" : "White"));
            }
            Stats[1].text = "/";
            Stats[0].offset = new Vector2(-48, -24);
            Stats[1].offset = new Vector2(-48, -24);
            Stats[2].offset = new Vector2(-72, -24);
            // Hp
            Stats[0].text = actor2.hp.ToString();
            // Max Hp
            Stats[2].text = actor2.maxhp.ToString();

            refresh();
        }

        protected override void refresh()
        {
            Window.loc = Loc;
            Target_Sprite.loc = Loc;
            foreach (FE_Text label in Stat_Labels)
                label.loc = Loc;
            foreach (FE_Text stat in Stats)
                stat.loc = Loc;
            Name.loc = Loc;
        }

        protected override void update_end(int temp_index)
        {
            update_frame();
        }

        protected void update_frame()
        {
            Target_Sprite.frame = Global.game_system.unit_anim_idle_frame;
        }

        protected override void move_down()
        {
            base.move_down();
            move_timer_reset();
        }
        protected override void move_up()
        {
            base.move_up();
            move_timer_reset();
        }
        protected override void move_to(int index)
        {
            base.move_to(index);
            move_timer_reset();
        }

        protected void move_timer_reset()
        {
            // \o_O/ //Yeti
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Window.draw(sprite_batch);
            Target_Sprite.draw(sprite_batch);
            foreach (FE_Text label in Stat_Labels)
                label.draw(sprite_batch);
            foreach (FE_Text stat in Stats)
                stat.draw(sprite_batch);
            Name.draw(sprite_batch);
            sprite_batch.End();
        }
    }
}
