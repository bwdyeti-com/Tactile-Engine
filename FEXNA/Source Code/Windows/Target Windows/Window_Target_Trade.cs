using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Text;

namespace FEXNA.Windows.Target
{
    class Window_Target_Trade : Window_Target_Unit
    {
        Trade_Target_Window_Img Window;
        FE_Text Name;
        Character_Sprite Target_Sprite;
        Sprite Rescue_Icon;
        protected List<Status_Item> Items = new List<Status_Item>();

        protected override int window_width
        {
            get { return 112; }
        }

        public Window_Target_Trade() { }
        public Window_Target_Trade(int unit_id, Vector2 loc)
        {
            initialize_trade(unit_id, loc);
        }

        protected void initialize_trade(int unit_id, Vector2 loc)
        {
            initialize(loc);
            Right_X = Config.WINDOW_WIDTH - this.window_width;
            Unit_Id = unit_id;
            List<int> targets = get_targets();
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

        protected virtual List<int> get_targets()
        {
            Game_Unit unit = get_unit();
            bool has_items = unit.actor.has_items;
            List<int> temp_targets = unit.allies_in_range(1);
            List<int> targets = new List<int>();
            // Rescued unit
            if (unit.is_rescuing)
            {
                Game_Unit rescued_unit = Global.game_map.units[unit.rescuing];
                if ((has_items || rescued_unit.actor.has_items) && unit.same_team(rescued_unit))
                    targets.Add(unit.rescuing);
            }
            foreach (int id in temp_targets)
            {
                Game_Unit other_unit = Global.game_map.units[id];
                if (unit.different_team(other_unit))
                    continue;

                if (has_items || other_unit.actor.has_items)
                    targets.Add(id);
                if (other_unit.is_rescuing)
                {
                Game_Unit rescued_unit = Global.game_map.units[other_unit.rescuing];
                if ((has_items || rescued_unit.actor.has_items) && unit.same_team(rescued_unit))
                    targets.Add(other_unit.rescuing);
                }
            }
            return targets;
        }

        protected void initialize_images()
        {
            // Windows
            Window = new Trade_Target_Window_Img();
            // Names
            Name = new FE_Text();
            Name.Font = "FE7_Text";
            Name.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Name.draw_offset = new Vector2(32, 8);
            // Target Sprite
            Target_Sprite = new Character_Sprite();
            Target_Sprite.draw_offset = new Vector2(20, 24);
            Target_Sprite.facing_count = 3;
            Target_Sprite.frame_count = 3;
            // Rescue Icon
            Rescue_Icon = new Sprite();
            Rescue_Icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/RescueIcon");
            Rescue_Icon.draw_offset = new Vector2(4, 0);
            set_images();
        }

        protected override void set_images()
        {
            Game_Unit target = Global.game_map.units[this.target];
            Window.rows = Math.Max(1, target.actor.num_items);
            Items.Clear();
            for(int i = 0; i < Math.Max(1, target.actor.num_items); i++)
            {
                draw_item(target, i);
            }
            // Sets Name //
            Name.text = target.actor.name;
            // Map Sprite //
            if (target.is_rescued)
            {
                Rescue_Icon.visible = true;
                Rescue_Icon.src_rect = new Rectangle(
                    (target.team - 1) *
                        (Rescue_Icon.texture.Width / Constants.Team.NUM_TEAMS),
                    0,
                    Rescue_Icon.texture.Width / Constants.Team.NUM_TEAMS,
                    Rescue_Icon.texture.Height);
                Target_Sprite.texture = null;
            }
            else
            {
                Rescue_Icon.visible = false;
                Target_Sprite.texture = Scene_Map.get_team_map_sprite(target.team, target.map_sprite_name);
                Target_Sprite.offset = new Vector2(
                    (Target_Sprite.texture.Width / Target_Sprite.frame_count) / 2,
                    (Target_Sprite.texture.Height / Target_Sprite.facing_count) - 8);
                Target_Sprite.mirrored = target.has_flipped_map_sprite;
            }
            refresh();
        }

        protected virtual void draw_item(Game_Unit target, int i)
        {
            Items.Add(new Trade_Target_Item());
            Items[i].set_image(target.actor, target.actor.items[i]);
            Items[i].draw_offset = new Vector2(8, 24 + i * 16);
        }

        protected override void refresh()
        {
            Window.loc = Loc;
            Name.loc = Loc;
            Target_Sprite.loc = Loc;
            Rescue_Icon.loc = Loc;
            foreach (Status_Item item in Items)
                item.loc = Loc;
        }

        protected override void update_end(int temp_index)
        {
            update_frame();
        }

        protected void update_frame()
        {
            Target_Sprite.frame = Global.game_system.unit_anim_idle_frame;
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            if (Visible)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Window.draw(sprite_batch);
                Name.draw(sprite_batch);
                Target_Sprite.draw(sprite_batch);
                if (Global.game_map.icons_visible)
                    Rescue_Icon.draw(sprite_batch);
                foreach (Status_Item item in Items)
                    item.draw(sprite_batch);
                sprite_batch.End();
            }
        }
    }
}
