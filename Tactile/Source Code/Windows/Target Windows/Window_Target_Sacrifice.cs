using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile.Windows.Target
{
    class Window_Target_Sacrifice : Window_Target_Staff
    {
        SystemWindowHeadered Unit_Window;
        Character_Sprite Unit_Sprite;
        TextSprite Unit_Name;

        public Window_Target_Sacrifice(int unit_id, Vector2 loc)
        {
            initialize(loc);
            Mode = Staff_Target_Mode.Heal;
            Right_X = Config.WINDOW_WIDTH - this.window_width;
            Unit_Id = unit_id;
            List<int> targets = get_targets();
            Targets = sort_targets(targets);
            this.index = set_base_index(Targets);
            Temp_Index = this.index;
            if (Targets.Count > 0)
            {
                Game_Unit target = Global.game_map.units[this.target];
                cursor_move_to(target);
            }
            Global.player.instant_move = true;
            Global.player.update_movement();
            initialize_images();
            refresh();
            index = this.index;
        }

        protected List<int> get_targets()
        {
            Game_Unit unit = get_unit();
            List<int> allies = unit.allies_in_range(1);
            int i = 0;
            while (i < allies.Count)
            {
                if (Global.game_map.units[allies[i]].actor.is_full_hp())
                    allies.RemoveAt(i);
                else
                    i++;
            }
            return allies;
        }

        protected override void initialize_images()
        {
            // Window
            Unit_Window = new SystemWindowHeadered();
            Unit_Window.width = this.window_width;
            // Target Sprite
            Unit_Sprite = new Character_Sprite();
            Unit_Sprite.draw_offset = new Vector2(20, 24 + 48);
            Unit_Sprite.facing_count = 3;
            Unit_Sprite.frame_count = 3;
            // Names
            Unit_Name = new TextSprite();
            Unit_Name.SetFont(Config.UI_FONT, Global.Content, "White");

            base.initialize_images();
        }

        protected override void initialize_stat_labels()
        {
            StatLabels = new List<TextSprite>();
            // Healing
            if (Mode == Staff_Target_Mode.Heal)
            {
                Window.height = 48;
                for (int i = 0; i < 2; i++)
                {
                    StatLabels.Add(new TextSprite());
                    StatLabels[i].offset = new Vector2(-8, -(24 + (i * 16)));
                    StatLabels[i].SetFont(Config.UI_FONT, Global.Content, "Yellow");
                }
                StatLabels[0].SetFont(Config.UI_FONT + "L", Config.UI_FONT);
                StatLabels[0].text = "HP";

                Unit_Window.height = 48;
                Unit_Window.draw_offset = new Vector2(0, 48);
                for (int i = 2; i < 4; i++)
                {
                    StatLabels.Add(new TextSprite());
                    StatLabels[i].offset = new Vector2(-8, -(72 + ((i - 2) * 16)));
                    StatLabels[i].SetFont(Config.UI_FONT, Global.Content, "Yellow");
                }
                StatLabels[2].SetFont(Config.UI_FONT + "L", Config.UI_FONT);
                StatLabels[2].text = "HP";
            }
        }

        protected override void set_images()
        {
            if (Mode != Staff_Target_Mode.Torch)
            {
                Game_Unit unit = get_unit();
                Game_Actor actor1 = unit.actor;
                Game_Unit target = Global.game_map.units[this.target];
                Game_Actor actor2 = target.actor;
                // Map Sprite //
                Target_Sprite.texture = Scene_Map.get_team_map_sprite(target.team, target.map_sprite_name);
                Target_Sprite.offset = new Vector2(
                    (Target_Sprite.texture.Width / Target_Sprite.frame_count) / 2,
                    (Target_Sprite.texture.Height / Target_Sprite.facing_count) - 8);
                Target_Sprite.mirrored = target.has_flipped_map_sprite;
                Unit_Sprite.texture = Scene_Map.get_team_map_sprite(unit.team, unit.map_sprite_name);
                Unit_Sprite.offset = new Vector2(
                    (Unit_Sprite.texture.Width / Unit_Sprite.frame_count) / 2,
                    (Unit_Sprite.texture.Height / Unit_Sprite.facing_count) - 8);
                Unit_Sprite.mirrored = unit.has_flipped_map_sprite;
                // Sets Name //
                Name.offset = new Vector2(-(32), -(8));
                Name.text = actor2.name;
                Unit_Name.offset = new Vector2(-(32), -(8 + 48));
                Unit_Name.text = actor1.name;

                Stats = new List<TextSprite>();
                // Healing
                if (Mode == Staff_Target_Mode.Heal)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Stats.Add(i % 2 == 0 ? new RightAdjustedText() : new TextSprite());
                        Stats[i].offset = new Vector2(-(48 + ((i / 2) * 24)), -24);
                        Stats[i].SetFont(Config.UI_FONT, Global.Content, (i % 2 == 0 ? "Blue" : "White"));
                    }
                    for (int i = 5; i < 10; i++)
                    {
                        Stats.Add(i % 2 == 1 ? new RightAdjustedText() : new TextSprite());
                        Stats[i].offset = new Vector2(-(48 + (((i - 5) / 2) * 24)), -72);
                        Stats[i].SetFont(Config.UI_FONT, Global.Content, (i % 2 == 1 ? "Blue" : "White"));
                    }

                    int heal = unit.sacrifice_heal_amount(target);

                    Stats[1].text = "-";
                    Stats[6].text = "-";
                    Stats[3].text = "/";
                    Stats[8].text = "/";
                    // Hp
                    Stats[0].text = actor2.hp.ToString();
                    Stats[5].text = actor1.hp.ToString();
                    // New Hp
                    Stats[2].text = (actor2.hp + heal).ToString();
                    Stats[7].text = (actor1.hp - heal).ToString();
                    // Max Hp
                    Stats[4].text = actor2.maxhp.ToString();
                    Stats[9].text = actor1.maxhp.ToString();
                    // Status
                    StatLabels[1].text = "";
                    StatLabels[3].text = "";
                }
                refresh();
            }
        }

        protected override void refresh()
        {
            base.refresh();
            if (Mode != Staff_Target_Mode.Torch)
            {
                Unit_Window.loc = Loc;
                Unit_Sprite.loc = Loc;
                Unit_Name.loc = Loc;
            }
        }

        protected override void update_frame()
        {
            base.update_frame();
            Unit_Sprite.frame = Global.game_system.unit_anim_idle_frame;
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Unit_Window.draw(sprite_batch);
            Unit_Sprite.draw(sprite_batch);
            Unit_Name.draw(sprite_batch);
            sprite_batch.End();

            base.draw(sprite_batch);
        }
    }
}
