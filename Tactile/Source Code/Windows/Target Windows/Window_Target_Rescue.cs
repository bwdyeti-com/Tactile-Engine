using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile.Windows.Target
{
    class Window_Target_Rescue : Window_Target_Unit
    {
        SystemWindowHeadered Window, Target_Window;
        Character_Sprite Unit_Sprite, Target_Sprite;
        TextSprite Name1, Aid_Label, Aid_Value;
        TextSprite Name2, Con_Label, Con_Value;
        Hand_Cursor Hand;
        Sprite Rescue_Icon;
        protected int Mode;

        #region Accessors
        public int mode { get { return Mode; } }

        protected override int window_width
        {
            get { return 80; }
        }

        private bool target_position_reversed
        {
            get
            {
                // Give
                if (Mode == 3)
                    return true;
                // Protect
                if (Mode == 5)
                    return true;
                return false;
            }
        }
        #endregion

        public Window_Target_Rescue(int unit_id, int mode, Vector2 loc)
        {
            Mode = mode;
            initialize(loc);
            Right_X = Config.WINDOW_WIDTH - this.window_width;
            Unit_Id = unit_id;
            List<int> targets = get_targets();
            Targets = sort_targets(targets);
            this.index = 0;
            Temp_Index = this.index;
            cursor_move_to(this.target);

            Global.player.instant_move = true;
            Global.player.update_movement();
            initialize_images();
            refresh();
            index = this.index;
        }

        protected override List<int> sort_targets(List<int> targets)
        {
            Game_Unit unit = get_unit();
            targets.Sort(delegate(int a, int b)
            {
                Vector2 loc1, loc2;
                int dist1, dist2;
                if (mode == 1)
                {
                    loc1 = new Vector2(a % Global.game_map.width,
                        a / Global.game_map.width);
                    loc2 = new Vector2(b % Global.game_map.width,
                        b / Global.game_map.width);
                    dist1 = (int)(Math.Abs(unit.loc.X - loc1.X) + Math.Abs(unit.loc.Y - loc1.Y));
                    dist2 = (int)(Math.Abs(unit.loc.X - loc2.X) + Math.Abs(unit.loc.Y - loc2.Y));
                }
                else
                {
                    Game_Unit unit1 = Global.game_map.units[a]; Game_Unit unit2 = Global.game_map.units[b];
                    loc1 = unit1.loc_on_map(); loc2 = unit2.loc_on_map();
                    dist1 = Global.game_map.unit_distance(Unit_Id, a);
                    dist2 = Global.game_map.unit_distance(Unit_Id, b);
                }
                int angle1 = ((360 - unit.angle(loc1)) + 90) % 360;
                int angle2 = ((360 - unit.angle(loc2)) + 90) % 360;
                return (angle1 == angle2 ?
                    (loc1.Y == loc2.Y ? dist1 - dist2 : (int)(loc1.Y - loc2.Y)) :
                    angle1 - angle2);
            });
            return targets;
        }

        protected List<int> get_targets()
        {
            Game_Unit unit = get_unit();
            List<int> temp_targets = new List<int>();
            switch(Mode)
            {
                // Looking for people to rescue
                case 0:
                    List<int> rescue_targets = unit.allies_in_range(1);
                    foreach (int id in rescue_targets)
                        if (unit.can_rescue(Global.game_map.units[id]))
                            temp_targets.Add(id);
                    break;
                // Looking for drop locations
                case 1:
                    foreach (Vector2 loc in unit.drop_locs())
                        temp_targets.Add((int)(loc.X + loc.Y * Global.game_map.width));
                    /*foreach (Vector2 loc in new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) }) //Debug
                        if (!Global.game_map.is_off_map(loc + unit.loc))
                            if (!Global.game_map.is_blocked(loc + unit.loc, unit.rescuing))
                                if (Pathfinding.passable(Global.game_map.units[unit.rescuing], loc + unit.loc))
                                    temp_targets.Add((int)((loc + unit.loc).X + (loc + unit.loc).Y * Global.game_map.width));*/
                    break;
                // Looking for people to take
                case 2:
                    List<int> take_targets = unit.allies_in_range(1);
                    foreach (int id in take_targets)
                    {
                        if (Global.game_map.units[id].different_team(unit))
                            continue;
                        if (!Global.game_map.units[id].is_rescuing)
                            continue;
                        if (unit.can_rescue(Global.game_map.units[Global.game_map.units[id].rescuing]))
                            temp_targets.Add(id);
                    }
                    break;
                // Looking for people to give to
                case 3:
                    List<int> give_targets = unit.allies_in_range(1);
                    foreach (int id in give_targets)
                    {
                        if (Global.game_map.units[id].different_team(unit))
                            continue;
                        if (Global.game_map.units[id].is_rescuing)
                            continue;
                        if (!Global.game_map.units[id].is_rescue_blocked() &&
                                Global.game_map.units[id].can_rescue(Global.game_map.units[unit.rescuing]))
                            temp_targets.Add(id);
                    }
                    break;
                // Skills: Savior
                // Looking for people to cover
                case 4:
                    List<int> cover_targets = unit.allies_in_range(1);
                    foreach (int id in cover_targets)
                        if (unit.can_rescue(Global.game_map.units[id]))
                            if (Pathfind.passable(unit, Global.game_map.units[id].loc))
                                temp_targets.Add(id);
                    break;
                // Looking for people to take refuge under
                case 5:
                    List<int> refuge_targets = unit.allies_in_range(1);
                    foreach (int id in refuge_targets)
                    {
                        Game_Unit target = Global.game_map.units[id];
                        if (target.has_refuge() && target.can_rescue(unit))
                            temp_targets.Add(id);
                    }
                    break;
            }
            return temp_targets;
        }

        protected void initialize_images()
        {
            // Windows
            Window = new SystemWindowHeadered();
            Window.width = this.window_width;
            Window.height = 48;
            Window.draw_offset = new Vector2(0, (!this.target_position_reversed ? 0 : 48));
            Target_Window = new SystemWindowHeadered();
            Target_Window.width = this.window_width;
            Target_Window.height = 48;
            Target_Window.draw_offset = new Vector2(0, (this.target_position_reversed ? 0 : 48));
            // Map Sprites
            Unit_Sprite = new Character_Sprite();
            Unit_Sprite.draw_offset = new Vector2(20, 24 + (!this.target_position_reversed ? 0 : 48));
            Unit_Sprite.facing_count = 3;
            Unit_Sprite.frame_count = 3;
            Target_Sprite = new Character_Sprite();
            Target_Sprite.draw_offset = new Vector2(20, 24 + (this.target_position_reversed ? 0 : 48));
            Target_Sprite.facing_count = 3;
            Target_Sprite.frame_count = 3;
            // Names
            Name1 = new TextSprite();
            Name1.draw_offset = new Vector2(32, 8 + (!this.target_position_reversed ? 0 : 48));
            Name1.SetFont(Config.UI_FONT, Global.Content, "White");
            Name2 = new TextSprite();
            Name2.draw_offset = new Vector2(32, 8 + (this.target_position_reversed ? 0 : 48));
            Name2.SetFont(Config.UI_FONT, Global.Content, "White");
            //Name1, , Aid_Value;
            //Name2, Con_Label, Con_Value;
            // Labels
            Aid_Label = new TextSprite();
            Aid_Label.draw_offset = new Vector2(8, 24 + (!this.target_position_reversed ? 0 : 48));
            Aid_Label.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Aid_Label.text = "Aid";
            Con_Label = new TextSprite();
            Con_Label.draw_offset = new Vector2(8, 24 + (this.target_position_reversed ? 0 : 48));
            Con_Label.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Con_Label.text = "Con";
            // Stats
            Aid_Value = new RightAdjustedText();
            Aid_Value.draw_offset = new Vector2(72, 24 + (!this.target_position_reversed ? 0 : 48));
            Aid_Value.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Con_Value = new RightAdjustedText();
            Con_Value.draw_offset = new Vector2(72, 24 + (this.target_position_reversed ? 0 : 48));
            Con_Value.SetFont(Config.UI_FONT, Global.Content, "Blue");
            // Hand
            Hand = new Hand_Cursor();
            Hand.offset = new Vector2(8, 0);
            Hand.draw_offset = new Vector2(48, 47);
            Hand.mirrored = !this.target_position_reversed;
            Hand.angle = MathHelper.PiOver2;
            // Rescue Icon
            Rescue_Icon = new Sprite();
            Rescue_Icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Characters/RescueIcon");

            set_images();
        }

        protected override void set_images()
        {

            // Drop
            if (mode == 1)
                return;

            Game_Unit unit = get_unit();
            Game_Unit target = Global.game_map.units[this.target];
            Rescue_Icon.visible = false;

            switch (mode)
            {
                // Take
                case 2:
                    target = Global.game_map.units[Global.game_map.units[this.target].rescuing];
                    Rescue_Icon.visible = true;
                    Rescue_Icon.src_rect = new Rectangle((target.team - 1) * (Rescue_Icon.texture.Width / Constants.Team.NUM_TEAMS),
                        0, Rescue_Icon.texture.Width / Constants.Team.NUM_TEAMS, Rescue_Icon.texture.Height);
                    break;
                // Give
                case 3:
                    unit = target;
                    target = Global.game_map.units[get_unit().rescuing];
                    Rescue_Icon.visible = true;
                    Rescue_Icon.src_rect = new Rectangle((target.team - 1) * (Rescue_Icon.texture.Width / Constants.Team.NUM_TEAMS),
                        0, Rescue_Icon.texture.Width / Constants.Team.NUM_TEAMS, Rescue_Icon.texture.Height);
                    break;
                // Protect
                case 5:
                    Game_Unit temp = unit;
                    unit = target;
                    target = temp;
                    break;
            }

            // Map Sprites //
            Unit_Sprite.texture = Scene_Map.get_team_map_sprite(unit.team, unit.map_sprite_name);
            if (Unit_Sprite.texture != null)
                Unit_Sprite.offset = new Vector2(
                    (Unit_Sprite.texture.Width / Unit_Sprite.frame_count) / 2,
                    (Unit_Sprite.texture.Height / Unit_Sprite.facing_count) - 8);
            Unit_Sprite.mirrored = unit.has_flipped_map_sprite;
            if (Rescue_Icon.visible)
                Rescue_Icon.draw_offset = Target_Sprite.draw_offset - new Vector2(16, 24);
            else
            {
                Target_Sprite.texture = Scene_Map.get_team_map_sprite(target.team, target.map_sprite_name);
                if (Target_Sprite.texture != null)
                    Target_Sprite.offset = new Vector2(
                        (Target_Sprite.texture.Width / Target_Sprite.frame_count) / 2,
                        (Target_Sprite.texture.Height / Target_Sprite.facing_count) - 8);
                Target_Sprite.mirrored = target.has_flipped_map_sprite;
            }
            // Text
            Name1.text = unit.actor.name;
            Name2.text = target.actor.name;
            Aid_Value.text = unit.aid().ToString();
            Con_Value.text = target.stat(Stat_Labels.Con).ToString();
            refresh();
        }

        protected override void refresh()
        {
            Window.loc = Loc;
            Target_Window.loc = Loc;
            Unit_Sprite.loc = Loc;
            Target_Sprite.loc = Loc;

            Name1.loc = Loc;
            Name2.loc = Loc;
            Aid_Label.loc = Loc;
            Con_Label.loc = Loc;
            Aid_Value.loc = Loc;
            Con_Value.loc = Loc;
            Hand.loc = Loc;
            Rescue_Icon.loc = Loc;
        }

        protected override void update_end(int temp_index)
        {
            update_frame();
        }

        protected void update_frame()
        {
            int frame = Global.game_system.unit_anim_idle_frame;
            Unit_Sprite.frame = frame;
            Target_Sprite.frame = frame;
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

        protected override void reset_cursor()
        {
            if (mode == 1)
                cursor_move_to(Targets[Temp_Index]);
            else
                cursor_move_to(Global.game_map.units[Targets[Temp_Index]]);
        }

        protected void cursor_move_to(int target)
        {
            Vector2 loc;
            if (mode == 1)
                loc = new Vector2(target % Global.game_map.width, target / Global.game_map.width);
            else
                loc = Global.game_map.units[target].loc_on_map();
            Global.player.loc = loc;
            if (mode != 1)
                get_unit().face(loc);
        }

        internal override Vector2 target_loc(int target)
        {
            // Drop
            if (mode == 1)
                return new Vector2(target % Global.game_map.width, target / Global.game_map.width);

            Combat_Map_Object unit1 = Global.game_map.attackable_map_object(target);
            return unit1.loc_on_map();
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            if (mode != 1)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Window.draw(sprite_batch);
                Target_Window.draw(sprite_batch);
                Unit_Sprite.draw(sprite_batch);
                Target_Sprite.draw(sprite_batch);

                Name1.draw(sprite_batch);
                Name2.draw(sprite_batch);
                Aid_Label.draw(sprite_batch);
                Con_Label.draw(sprite_batch);
                Aid_Value.draw(sprite_batch);
                Con_Value.draw(sprite_batch);
                Hand.draw(sprite_batch);
                if (Global.game_map.icons_visible)
                    Rescue_Icon.draw(sprite_batch);
                sprite_batch.End();
            }
        }
    }
}
