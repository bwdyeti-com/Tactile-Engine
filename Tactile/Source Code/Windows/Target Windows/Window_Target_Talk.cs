using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile.Windows.Target
{
    class Window_Target_Talk : Window_Target_Unit
    {
        SystemWindowHeadered Window;
        Character_Sprite Unit_Sprite;
        TextSprite Name, Hp_Label, Slash, Hp, MaxHp;

        protected override int window_width
        {
            get { return 80; }
        }

        protected Window_Target_Talk() { }
        public Window_Target_Talk(int unit_id, Vector2 loc)
        {
            initialize(loc);
            Right_X = Config.WINDOW_WIDTH - this.window_width;
            Unit_Id = unit_id;
            List<int> targets = get_unit().talk_targets();
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

        protected void initialize_images()
        {
            // Windows
            Window = new SystemWindowHeadered();
            Window.width = this.window_width;
            Window.height = 48;
            Window.draw_offset = new Vector2(0, 0);
            // Map Sprites
            Unit_Sprite = new Character_Sprite();
            Unit_Sprite.draw_offset = new Vector2(20, 24);
            Unit_Sprite.facing_count = 3;
            Unit_Sprite.frame_count = 3;
            // Names
            Name = new TextSprite();
            Name.draw_offset = new Vector2(32, 8);
            Name.SetFont(Config.UI_FONT, Global.Content, "White");
            //Name1, , Aid_Value;
            //Name2, Con_Label, Con_Value;
            // Labels
            Hp_Label = new TextSprite();
            Hp_Label.draw_offset = new Vector2(8, 24);
            Hp_Label.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Hp_Label.text = "HP";
            Slash = new TextSprite();
            Slash.draw_offset = new Vector2(48, 24);
            Slash.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Slash.text = "/";
            // Stats
            Hp = new RightAdjustedText();
            Hp.draw_offset = new Vector2(48, 24);
            Hp.SetFont(Config.UI_FONT, Global.Content, "Blue");
            MaxHp = new RightAdjustedText();
            MaxHp.draw_offset = new Vector2(72, 24);
            MaxHp.SetFont(Config.UI_FONT, Global.Content, "Blue");

            set_images();
        }

        protected override void set_images()
        {
            Game_Unit target = Global.game_map.units[this.target];

            // Map Sprites //
            Unit_Sprite.texture = Scene_Map.get_team_map_sprite(target.team, target.map_sprite_name);
            if (Unit_Sprite.texture != null)
                Unit_Sprite.offset = new Vector2(
                    (Unit_Sprite.texture.Width / Unit_Sprite.frame_count) / 2,
                    (Unit_Sprite.texture.Height / Unit_Sprite.facing_count) - 8);
            Unit_Sprite.mirrored = target.has_flipped_map_sprite;
            // Text
            Name.text = target.actor.name;
            Hp.text = target.actor.hp.ToString();
            MaxHp.text = target.actor.maxhp.ToString();
            refresh();
        }

        protected override void refresh()
        {
            Window.loc = Loc;
            Unit_Sprite.loc = Loc;

            Name.loc = Loc;
            Hp_Label.loc = Loc;
            Slash.loc = Loc;
            Hp.loc = Loc;
            MaxHp.loc = Loc;
        }

        protected override void update_end(int temp_index)
        {
            update_frame();
        }

        protected void update_frame()
        {
            Unit_Sprite.frame = Global.game_system.unit_anim_idle_frame;
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Window.draw(sprite_batch);
            Unit_Sprite.draw(sprite_batch);

            Name.draw(sprite_batch);
            Hp_Label.draw(sprite_batch);
            Slash.draw(sprite_batch);
            Hp.draw(sprite_batch);
            MaxHp.draw(sprite_batch);
            sprite_batch.End();
        }
    }
}
