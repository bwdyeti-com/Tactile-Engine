using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNAWeaponExtension;

namespace FEXNA.Windows.Target
{
    internal class Window_Target_Combat : Window_Target_Unit
    {
        internal const int LINE_HEIGHT = 16;
        const int ROWS = 4;

        protected bool Weapon_Name_Visible = true;
        protected FE_Combat_Target_Window Window;
        protected List<FE_Text> Stat_Labels;
        protected List<Sprite> Stats;
        Item_Icon_Sprite Icon1, Icon2;
        Multiplier_Img Mult1, Mult2;
        Effective_WT_Arrow WTA1, WTA2;
        FE_Text Target_Weapon, Name1, Name2;
        protected string Skill = "";

        protected override int window_width
        {
            get { return 76; }
        }

        protected Window_Target_Combat() { }
        public Window_Target_Combat(int unit_id, int item_index, Vector2 loc, string skill)
        {
            initialize(loc);
            Unit_Id = unit_id;
            Skill = skill;

            List<int> targets = get_targets(item_index);
            get_unit().equip(item_index + 1);
            Targets = sort_targets(targets);
            this.index = 0;
            Temp_Index = this.index;
            Combat_Map_Object target =
                Global.game_map.attackable_map_object(this.target);
#if DEBUG
            if (target == null)
                throw new IndexOutOfRangeException("No combat targets found");
#endif
            attacker_target_unit(target);
            cursor_move_to(target);
            Global.player.instant_move = true;
            Global.player.update_movement();
            initialize_images();
            refresh();
            index = this.index;
        }

        protected virtual void attacker_target_unit(Combat_Map_Object target)
        {
            if (target.is_unit())
                ((Game_Unit)target).target_unit(
                    get_unit(), get_unit().actor.weapon, Global.game_map.combat_distance(Unit_Id, ((Game_Unit)target).id));
        }

        protected List<int> get_targets(int item_index)
        {
            Game_Unit unit = get_unit();
            // Skills: Swoop
            if (unit.swoop_activated)
                return unit.enemies_in_swoop_range()[0];
            // Skills: Old Swoop
            else if (unit.old_swoop_activated)
                return unit.enemies_in_old_swoop_range()[0];
            // Skills: Trample
            else if (unit.trample_activated)
                return unit.enemies_in_trample_range()[0];
            else
                return unit.enemies_in_range(item_index, Skill)[0];
        }

        protected virtual int window_rows { get { return ROWS; } }
        protected virtual int stat_rows { get { return ROWS; } }

        protected virtual void initialize_images()
        {
            // Window
            Window = windowskin();
            Window.rows = window_rows;
            Window.team1 = get_unit().team;
            Window.offset = new Vector2(0, -1);
            // Names
            Name1 = new FE_Text();
            Name1.Font = "FE7_Text";
            Name1.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Name2 = new FE_Text();
            Name2.Font = "FE7_Text";
            Name2.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            Target_Weapon= new FE_Text();
            Target_Weapon.Font = "FE7_Text";
            Target_Weapon.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White");
            // Attack Multipliers
            Mult1 = new Multiplier_Img();
            Mult2 = new Multiplier_Img();
            // Icons
            Icon1 = new Item_Icon_Sprite();
            Icon2 = new Item_Icon_Sprite();
            // Weapon triangle arrows
            WTA1 = new Effective_WT_Arrow();
            WTA2 = new Effective_WT_Arrow();
            // Stat Labels
            Stat_Labels = new List<FE_Text>();
            for (int i = 0; i < stat_rows; i++)
            {
                Stat_Labels.Add(new FE_Text());
                Stat_Labels[i].offset = new Vector2(-28, -(4 + ((i + 1) * LINE_HEIGHT)));
                Stat_Labels[i].Font = "FE7_Text";
                Stat_Labels[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            }
            //Stat_Labels[1].offset.X -= 3;
            Stat_Labels[2].offset.X -= 2;
            Stat_Labels[0].Font = "FE7_TextL";
            Stat_Labels[0].text = "HP";
            Stat_Labels[1].text = "Dmg";// "Mt";
            Stat_Labels[2].text = "Hit";
            Stat_Labels[3].text = "Crit";

            set_images();
        }

        protected virtual FE_Combat_Target_Window windowskin()
        {
            return new FE_Combat_Target_Window(Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Combat_Window"));
        }

        protected override void set_images()
        {
            Icon1.flash = false;
            Icon2.flash = false;
            Mult1.value = 0;
            Mult2.value = 0;
            WTA1.set_effectiveness(1);
            WTA2.set_effectiveness(1);
            Game_Unit unit = get_unit();
            Game_Actor actor1 = unit.actor;
            Combat_Map_Object target =
                Global.game_map.attackable_map_object(this.target);
            Game_Unit target_unit = null;
            Game_Actor actor2 = null;
            bool is_target_unit = false;
            if (target.is_unit())
            {
                is_target_unit = true;
                target_unit = (Game_Unit)target;
                actor2 = target_unit.actor;
                Window.team2 = target_unit.team;
            }
            else
            {
                Window.team2 = target.team;
            }
            int distance = combat_distance(unit.id, this.target);
            // Get weapon data
            FEXNA_Library.Data_Weapon weapon1 = actor1.weapon, weapon2 = null;
            if (is_target_unit)
                weapon2 = actor2.weapon;
            // Weapon triangle arrows
            WTA1.value = 0;
            WTA2.value = 0;
            WeaponTriangle tri = WeaponTriangle.Nothing;
            if (is_target_unit)
                tri = Combat.weapon_triangle(unit, target_unit, weapon1, weapon2, distance);
            if (tri != WeaponTriangle.Nothing)
            {
                WTA1.value = tri;
                if (weapon2 != null)
                    WTA2.value = Combat.reverse_wta(tri);
            }

            List<int?> combat_stats = get_combat_stats(unit.id, this.target, distance);
            Stats = new List<Sprite>();
            for (int i = 0; i < stat_rows * 2; i++)
            {
                FE_Text_Int text = new FE_Text_Int();
                Stats.Add(text);
                text.draw_offset = new Vector2(68 - (48 * (i / 4)), 4 + ((i % 4 + 1) * LINE_HEIGHT));
                text.Font = "FE7_Text";
                text.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
                text.text = "0";
            }
            // Sets first units's stats //
            //Name1.offset = new Vector2(-(44 - Font_Data.text_width(actor1.name, "FE7_Text")/2), -(4));
            Name1.draw_offset = new Vector2(44, 4);
            Name1.offset = new Vector2(Font_Data.text_width(actor1.name, "FE7_Text") / 2, 0);
            Name1.text = actor1.name;
            // Multiplier
            Mult1.value = Mult1.get_multi(unit, target, weapon1, distance);
            // Icon
            Icon1.index = weapon1.Image_Index;
            Icon1.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + weapon1.Image_Name);
            if (weapon1.effective_multiplier(unit, target_unit) > 1)
                Icon1.flash = true;
            float effectiveness = weapon1.effective_multiplier(unit, target_unit, false);
            WTA1.set_effectiveness((int)effectiveness,
                is_target_unit && target_unit.halve_effectiveness());
            // Hp
            if (actor1.hp >= Math.Pow(10, Constants.BattleScene.STATUS_HP_COUNTER_VALUES))
            {
                //Stats[0].text = ""; //Debug
                //for (int i = 0; i < Constants.BattleScene.STATUS_HP_COUNTER_VALUES; i++)
                //    Stats[0].text += "?";
                ((FE_Text_Int)Stats[0]).text = "--";
            }
            else
                ((FE_Text_Int)Stats[0]).text = actor1.hp.ToString();
            // Dmg
            ((FE_Text_Int)Stats[1]).text = combat_stats[1].ToString();
            // Hit
            if (combat_stats[0] >= 100)
            {
                ((FE_Text_Int)Stats[2]).Font = "Blue_100";
                Stats[2].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/Blue_100");
            }
            ((FE_Text_Int)Stats[2]).text = combat_stats[0].ToString();
            // Crt
            if (combat_stats[2] >= 100)
            {
                ((FE_Text_Int)Stats[3]).Font = "Blue_100";
                Stats[3].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/Blue_100");
            }
            ((FE_Text_Int)Stats[3]).text = combat_stats[2].ToString();
            // Sets second units's stats //
            string name2 = is_target_unit ? actor2.name : target.name;
            //Name2.offset = new Vector2(-(28 - Font_Data.text_width(name2, "FE7_Text") / 2), -(4 + (Window.rows + 1) * LINE_HEIGHT));
            Name2.draw_offset = new Vector2(28, 4 + (Window.rows + 1) * LINE_HEIGHT);
            Name2.offset = new Vector2(Font_Data.text_width(name2, "FE7_Text") / 2, 0);
            Name2.text = name2;
            // Icon
            if (weapon2 != null)
            {
                Target_Weapon.draw_offset = new Vector2(
                    32 - Font_Data.text_width(weapon2.Name, "FE7_Text") / 2, 4 + (Window.rows + 2) * LINE_HEIGHT);
                Target_Weapon.text = weapon2.Name;
                Icon2.index = weapon2.Image_Index;
                Icon2.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + weapon2.Image_Name);
                if (weapon2.effective_multiplier(target_unit, unit) > 1)
                    Icon2.flash = true;
                effectiveness = weapon2.effective_multiplier(target_unit, unit, false);
                WTA2.set_effectiveness((int)effectiveness, unit.halve_effectiveness());
            }
            else
            {
                Target_Weapon.draw_offset = new Vector2(
                    32 - Font_Data.text_width("---", "FE7_Text") / 2, 4 + (Window.rows + 2) * LINE_HEIGHT);
                Target_Weapon.text = "---";
                Icon2.texture = null;
            }
            // Hp
            //int target_hp = is_target_unit ? actor2.hp : ((Destroyable_Object)target).hp; //Debug

            if (target.hp >= Math.Pow(10, Constants.BattleScene.STATUS_HP_COUNTER_VALUES))
            {
                //Stats[4].text = ""; //Debug
                //for (int i = 0; i < Constants.BattleScene.STATUS_HP_COUNTER_VALUES; i++)
                //    Stats[4].text += "?";
                ((FE_Text_Int)Stats[4]).text = "--";
            }
            else
                ((FE_Text_Int)Stats[4]).text = target.hp.ToString();
            if (is_target_unit && can_counter(unit, target_unit, weapon1, distance))
            {
                // Multiplier
                Mult2.value = Mult2.get_multi(target_unit, unit, weapon2, distance);
                // Dmg
                ((FE_Text_Int)Stats[5]).text = combat_stats[5].ToString();
                // Hit
                if (combat_stats[4] >= 100)
                {
                    ((FE_Text_Int)Stats[6]).Font = "Blue_100";
                    Stats[6].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/Blue_100");
                }
                ((FE_Text_Int)Stats[6]).text = combat_stats[4].ToString();
                // Crt
                if (combat_stats[6] >= 100)
                {
                    ((FE_Text_Int)Stats[7]).Font = "Blue_100";
                    ((FE_Text_Int)Stats[7]).texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/Blue_100");
                }
                ((FE_Text_Int)Stats[7]).text = combat_stats[6].ToString();
            }
            else
            {
                ((FE_Text_Int)Stats[5]).text = "--";
                ((FE_Text_Int)Stats[6]).text = "--";
                ((FE_Text_Int)Stats[7]).text = "--";
            }
            refresh();
        }

        protected virtual int combat_distance(int unit_id, int target_id)
        {
            return Global.game_map.combat_distance(unit_id, target_id);
        }

        protected virtual List<int?> get_combat_stats(int unit_id, int target_id, int distance)
        {
            return Combat.combat_stats(unit_id, target_id, distance);
        }

        protected virtual bool can_counter(Game_Unit unit, Game_Unit target, FEXNA_Library.Data_Weapon weapon, int distance)
        {
            // Skills: Old Swoop
            if (unit.old_swoop_activated)
                return false;
            // Skills: Trample
            if (unit.trample_activated)
                return false;
            return target.can_counter(unit, weapon, distance);
        }

        protected override void refresh()
        {
            //Window.loc = Loc;
            //foreach (FE_Text label in Stat_Labels)
            //    label.loc = Loc;
            //foreach (Sprite stat in Stats)
            //    stat.loc = Loc;
            //Name1.loc = Loc;
            //Name2.loc = Loc;
            //Target_Weapon.loc = Loc;
            //Icon1.loc = Loc + new Vector2(4, 4);
            //Icon2.loc = Loc + new Vector2(52, 4 + (Window.rows + 1) * LINE_HEIGHT);
            //Mult1.loc = Loc + new Vector2(65, 12 + 2 * LINE_HEIGHT);
            //Mult2.loc = Loc + new Vector2(20, 12 + 2 * LINE_HEIGHT);
            //WTA1.loc = Loc + new Vector2(4, 4);
            //WTA2.loc = Loc + new Vector2(52, 4 + (Window.rows + 1) * LINE_HEIGHT);

            Icon1.draw_offset = new Vector2(4, 4);
            Icon2.draw_offset = new Vector2(52, 4 + (Window.rows + 1) * LINE_HEIGHT);
            Mult1.draw_offset = new Vector2(65, 12 + 2 * LINE_HEIGHT);
            Mult2.draw_offset = new Vector2(20, 12 + 2 * LINE_HEIGHT);
            WTA1.draw_offset = new Vector2(4, 4);
            WTA2.draw_offset = new Vector2(52, 4 + (Window.rows + 1) * LINE_HEIGHT);
        }

        protected override void update_end(int temp_index)
        {
            Mult1.update();
            Mult2.update();
            WTA1.update();
            WTA2.update();
            Icon1.update();
            Icon2.update();
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
            if (Temp_Index != this.index)
                move_timer_reset();
        }

        protected void move_timer_reset()
        {
            //what exactly wants to be here //Yeti
            // WTA Timer
            WTA1.timer = 23;
            WTA2.timer = 23;
            // Multiplier timer
            Mult1.timer = 63;
            Mult2.timer = 63;
            Mult1.offset = new Vector2(0, -1);
            Mult2.offset = new Vector2(0, -1);
            // Multiplier offsets
        }

        protected override void reset_cursor()
        {
            if (this.index == -1)
                this.index = LastTargetIndex;

            if (this.index >= 0 &&
                    Global.game_map.attackable_map_object(this.target).is_unit())
                Global.game_map.units[this.target].cancel_targeted();
            if (Global.game_map.attackable_map_object(Targets[Temp_Index]).is_unit())
            {
                Game_Unit target = Global.game_map.units[Targets[Temp_Index]];
                target.target_unit(get_unit(), get_unit().actor.weapon, Global.game_map.combat_distance(Unit_Id, target.id));
            }
            cursor_move_to(Global.game_map.attackable_map_object(Targets[Temp_Index]));
        }

        protected override void cursor_move_to(Map_Object target)
        {
            base.cursor_move_to(target);
            if (Manual_Targeting)
                return;
            // Skills: Swoop
            if (get_unit().swoop_activated)
            {
                // Redraw current attack range under targeted units
                Global.game_temp.temp_skill_ranges["SWOOP"] = get_unit().swoop_range();
            }
            // Skills: Old Swoop
            else if (get_unit().old_swoop_activated)
            {
                // Redraw current attack range under targeted units
                Global.game_temp.temp_skill_ranges["OLDSWOOP"] = get_unit().old_swoop_range(get_unit().facing);
            }
            else if (get_unit().trample_activated)
            {
                // Redraw current attack range under targeted units
                Global.game_temp.temp_skill_ranges["TRAMPLE"] = get_unit().trample_range(get_unit().facing);
                Global.game_temp.temp_skill_move_ranges["TRAMPLE"] = get_unit().trample_move_range(get_unit().facing);
            }
        }

        public override void accept()
        {
            base.accept();
            if (Global.game_map.attackable_map_object(this.target).is_unit())
                Global.game_map.units[this.target].accept_targeting();
        }

        public override void cancel()
        {
            base.cancel();
            if (this.index == -1)
                this.index = LastTargetIndex;

            if (Global.game_map.attackable_map_object(this.target).is_unit())
                Global.game_map.units[this.target].cancel_targeted();
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Window.draw(sprite_batch, -Loc);
            draw_data(sprite_batch);
            sprite_batch.End();

            Icon1.draw(sprite_batch, -Loc);
            Icon2.draw(sprite_batch, -Loc);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            WTA1.draw(sprite_batch, -Loc);
            WTA2.draw(sprite_batch, -Loc);
            Mult1.draw(sprite_batch, -Loc);
            Mult2.draw(sprite_batch, -Loc);
            sprite_batch.End();
        }

        protected virtual void draw_data(SpriteBatch sprite_batch)
        {
            foreach (FE_Text label in Stat_Labels)
                label.draw(sprite_batch, -Loc);
            foreach (Sprite stat in Stats)
                stat.draw(sprite_batch, -Loc);
            Name1.draw(sprite_batch, -Loc);
            Name2.draw(sprite_batch, -Loc);
            if (Weapon_Name_Visible)
                Target_Weapon.draw(sprite_batch, -Loc);
        }
    }
}
