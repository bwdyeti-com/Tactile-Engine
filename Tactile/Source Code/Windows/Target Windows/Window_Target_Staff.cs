using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using TactileWeaponExtension;

namespace Tactile.Windows.Target
{
    enum Staff_Target_Mode { Heal, Status_Inflict, Torch, Barrier }

    class Window_Target_Staff : Window_Target_Unit
    {
        protected SystemWindowHeadered Window;
        protected Character_Sprite Target_Sprite;
        protected List<TextSprite> StatLabels;
        protected List<TextSprite> Stats;
        List<Status_Icon_Sprite> Status_Icons = new List<Status_Icon_Sprite>();
        protected TextSprite Name;
        protected Staff_Target_Mode Mode;

        protected override int window_width
        {
            get
            {
                switch (Mode)
                {
                    case Staff_Target_Mode.Status_Inflict:
                        return 96;
                    case Staff_Target_Mode.Barrier:
                        return 80;
                }
                return 104;
            }
        }

        protected Window_Target_Staff() { }
        public Window_Target_Staff(int unit_id, int item_index, Vector2 loc)
        {
            initialize(loc);
            Unit_Id = unit_id;
            var staff = get_unit().actor.items[item_index].to_weapon;
            Mode = target_mode(staff);
            if (staff.Torch())
                Manual_Targeting = true;

            Right_X = Config.WINDOW_WIDTH - this.window_width;

            List<int> targets = get_targets(item_index);
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

        public static Staff_Target_Mode target_mode(TactileLibrary.Data_Weapon staff)
        {
            if (staff.is_attack_staff())
                return Staff_Target_Mode.Status_Inflict;
            else if (staff.Torch())
                return Staff_Target_Mode.Torch;
            else if (staff.Heals())
                return Staff_Target_Mode.Heal;
            else if (staff.Barrier())
                return Staff_Target_Mode.Barrier;
            else
                return Staff_Target_Mode.Heal;
        }

        protected List<int> get_targets(int item_index)
        {
            Game_Unit unit = get_unit();
            switch (Mode)
            {
                case Staff_Target_Mode.Heal:
                case Staff_Target_Mode.Barrier:
                    return unit.allies_in_staff_range(new HashSet<Vector2> { unit.loc }, item_index)[0];
                case Staff_Target_Mode.Status_Inflict:
                    return unit.enemies_in_staff_range(new HashSet<Vector2> { unit.loc }, item_index)[0];
                case Staff_Target_Mode.Torch:
                    return new List<int>();
            }
            return new List<int>(); // other things not yet coded, oops //Yeti
        }

        protected int set_base_index(List<int> Targets)
        {
            // Healing
            if (Mode == Staff_Target_Mode.Heal)
            {
                float min_hp = 1f;
                int index = 0;
                for (int i = 0; i < Targets.Count; i++)
                {
                    int id = Targets[i];
                    Game_Unit target = Global.game_map.units[id];
                    float percent = target.actor.hp / (float)target.actor.maxhp;
                    if (percent <= min_hp)
                    {
                        index = i;
                        min_hp = percent;
                    }
                }
                return index;
            }
            else
                return 0;
        }

        protected virtual void initialize_images()
        {
            // Window
            Window = new SystemWindowHeadered();
            Window.width = this.window_width;
            // Target Sprite
            Target_Sprite = new Character_Sprite();
            Target_Sprite.draw_offset = new Vector2(20, 24);
            Target_Sprite.facing_count = 3;
            Target_Sprite.frame_count = 3;
            // Names
            Name = new TextSprite();
            Name.SetFont(Config.UI_FONT, Global.Content, "White");
            // Stat Labels
            initialize_stat_labels();

            set_images();
        }

        protected virtual void initialize_stat_labels()
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
            }
            // Status Inflict
            else if (Mode == Staff_Target_Mode.Status_Inflict)
            {
                Window.height = 64;
                for (int i = 0; i < 2; i++)
                {
                    StatLabels.Add(new TextSprite());
                    StatLabels[i].offset = new Vector2(-8, -(24 + (i * 16)));
                    StatLabels[i].SetFont(Config.UI_FONT, Global.Content, "Yellow");
                }
                StatLabels[0].text = "Res";
                StatLabels[1].text = "Hit";
            }
            // Barrier
            if (Mode == Staff_Target_Mode.Barrier)
            {
                Window.height = 48;
                for (int i = 0; i < 2; i++)
                {
                    StatLabels.Add(new TextSprite());
                    StatLabels[i].offset = new Vector2(-8, -(24 + (i * 16)));
                    StatLabels[i].SetFont(Config.UI_FONT, Global.Content, "Yellow");
                }
                StatLabels[0].SetFont(Config.UI_FONT);
                StatLabels[0].text = "Res";
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
                int distance = Global.game_map.combat_distance(unit.id, target.id);
                // Get weapon data
                TactileLibrary.Data_Weapon weapon1 = actor1.weapon, weapon2 = actor2.weapon;
                // Map Sprite //
                Target_Sprite.texture = Scene_Map.get_team_map_sprite(target.team, target.map_sprite_name);
                Target_Sprite.offset = new Vector2(
                    (Target_Sprite.texture.Width / Target_Sprite.frame_count) / 2,
                    (Target_Sprite.texture.Height / Target_Sprite.facing_count) - 8);
                Target_Sprite.mirrored = target.has_flipped_map_sprite;
                // Sets Name //
                Name.offset = new Vector2(-(32), -(8));
                Name.text = actor2.name;

                var stats = new Calculations.Stats.CombatStats(
                    unit.id, target.id, distance: distance);
                var target_stats = new Calculations.Stats.CombatStats(
                    target.id, unit.id, distance: distance);
                Stats = new List<TextSprite>();
                // Healing
                if (Mode == Staff_Target_Mode.Heal)
                {
                    List<int> status_heal = unit.actor.weapon.healable_statuses(target);
                    Window.height = status_heal.Count > 0 ? 64 : 48;
                    for (int i = 0; i < 5; i++)
                    {
                        Stats.Add(i % 2 == 0 ? new RightAdjustedText() : new TextSprite());
                        Stats[i].offset = new Vector2(-(48 + ((i / 2) * 24)), -24);
                        Stats[i].SetFont(Config.UI_FONT, Global.Content, (i % 2 == 0 ? "Blue" : "White"));
                    }
                    bool hp_restore = unit.actor.weapon.Heals();
                    if (hp_restore)
                        Stats[1].text = "-";
                    Stats[3].text = "/";
                    if (hp_restore)
                    {
                        // Hp
                        Stats[0].text = actor2.hp.ToString();
                        // New Hp
                        Stats[2].text = Math.Min(actor2.hp + stats.dmg(), actor2.maxhp).ToString();
                    }
                    else
                        // Hp
                        Stats[2].text = actor2.hp.ToString();
                    // Max Hp
                    Stats[4].text = actor2.maxhp.ToString();
                    // Status
                    Status_Icons.Clear();
                    StatLabels[1].text = "";
                    if (status_heal.Count > 0)
                    {
                        StatLabels[1].SetFont(Config.UI_FONT);
                        StatLabels[1].text = "Cond";
                        for (int i = 0; i < status_heal.Count; i++)
                        {
                            Status_Icons.Add(new Status_Icon_Sprite());
                            Status_Icons[Status_Icons.Count - 1].texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Statuses");
                            Status_Icons[Status_Icons.Count - 1].size = new Vector2(16, 16);
                            Status_Icons[Status_Icons.Count - 1].offset = new Vector2(-(32 + (i * 16)), -40);
                            Status_Icons[Status_Icons.Count - 1].index = Global.data_statuses[status_heal[i]].Image_Index;
                            Status_Icons[Status_Icons.Count - 1].counter = actor2.state_turns_left(status_heal[i]);
                        }
                    }
                }
                // Status Inflict
                else if (Mode == Staff_Target_Mode.Status_Inflict)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Stats.Add(new RightAdjustedText());
                        Stats[i].offset = new Vector2(-88, -(24 + i * 16));
                        Stats[i].SetFont(Config.UI_FONT, Global.Content, "Blue");
                    }
                    // Hp
                    Stats[0].text = target_stats.res().ToString();
                    // New Hp
                    Stats[1].text = Math.Min(100, stats.hit()).ToString();
                }
                // Barrier
                if (Mode == Staff_Target_Mode.Barrier)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Stats.Add(i % 2 == 0 ? new RightAdjustedText() : new TextSprite());
                        Stats[i].offset = new Vector2(-(48 + ((i / 2) * 24)), -24);
                        Stats[i].SetFont(Config.UI_FONT, Global.Content, (i % 2 == 0 ? "Blue" : "Yellow"));
                    }
                    Stats[1].text = "-";
                    // Res
                    Stats[0].text = (actor2.stat(Stat_Labels.Res) +
                        target.temporary_stat_buff(TactileLibrary.Buffs.Res)).ToString();
                    // New Res
                    Stats[2].text = (actor2.stat(Stat_Labels.Res) +
                        Constants.Combat.BARRIER_BONUS).ToString();
                }
                refresh();
            }
        }

        protected override void refresh()
        {
            if (Mode != Staff_Target_Mode.Torch)
            {
                Window.loc = Loc;
                Target_Sprite.loc = Loc;
                foreach (TextSprite label in StatLabels)
                    label.loc = Loc;
                foreach (TextSprite stat in Stats)
                    stat.loc = Loc;
                foreach (Status_Icon_Sprite icon in Status_Icons)
                    icon.loc = Loc;
                Name.loc = Loc;
            }
        }

        protected override void update_end(int temp_index)
        {
            update_frame();
        }

        protected virtual void update_frame()
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
            //Yeti
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            if (Mode == Staff_Target_Mode.Torch)
                return;

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Window.draw(sprite_batch);
            Target_Sprite.draw(sprite_batch);
            foreach (TextSprite label in StatLabels)
                label.draw(sprite_batch);
            foreach (TextSprite stat in Stats)
                stat.draw(sprite_batch);
            foreach (Status_Icon_Sprite icon in Status_Icons)
                icon.draw(sprite_batch);
            Name.draw(sprite_batch);
            sprite_batch.End();
        }
    }
}
