#if !MONOGAME && DEBUG
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA.Windows.Map
{
    enum Test_Battle_States { Unit_1, Unit_2, Confirm }
    class Window_Test_Battle_Setup : Window_Unit_Editor
    {
        const int BLOCK_INVALID_WEAPON_TIMES = 3;
        protected Test_Battle_States State = Test_Battle_States.Unit_1;
        private int BlockedWeaponInputs = 0;

        #region Accessors
        protected override int generic_rows { get { return 7; } }
        protected override int character_rows { get { return 6; } }

        protected bool same_unit
        {
            get
            {
                //return State == Test_Battle_States.Unit_2 && !Global.test_battler_1.Generic && !Global.test_battler_2.Generic && //Debug
                //    Global.test_battler_1.Actor_Id == Global.test_battler_2.Actor_Id;
                return !Global.test_battler_1.Generic && !Global.test_battler_2.Generic &&
                    Global.test_battler_1.Actor_Id == Global.test_battler_2.Actor_Id;
            }
        }

        internal int distance
        {
            get
            {
                if (Global.game_actors[Global.test_battler_1.Actor_Id].weapon == null)
                    return 1;
                Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Vector2.Zero, Global.test_battler_1.Actor_Id, "");
                Game_Unit unit1 = Global.game_map.last_added_unit;
                if (Global.game_actors[Global.test_battler_1.Actor_Id].weapon == null)
                    return 1;
                
                int min_range = unit1.min_range();
                int max_range = unit1.max_range();

                if (State == Test_Battle_States.Unit_1)
                    return min_range;

                Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Vector2.Zero, Global.test_battler_2.Actor_Id, "");
                Game_Unit unit2 = Global.game_map.last_added_unit;
                // nah //Debug
                // When either unit is using magic, make the battle use the longest range where the opponent can counter
                //if (unit1.actor.weapon.is_always_magic() || unit1.actor.weapon.is_ranged_magic() ||
                //        (unit2.actor.weapon != null && (unit2.actor.weapon.is_always_magic() || unit2.actor.weapon.is_ranged_magic())))
                    for (int i = max_range; i >= min_range; i--)
                    {
                        if (unit2.can_counter(unit1, unit1.actor.weapon, i))
                            return i;
                    }
                /* // Otherwise use the closest range where the opponent can counter
                else
                    for (int i = min_range; i <= max_range; i++)
                    {
                        if (unit2.can_counter(unit1, unit1.actor.weapon, i))
                            return i;
                    }*/
                return min_range;
            }
        }
        #endregion

        protected override void initialize()
        {
            if (Global.test_battler_1.Generic)
                Global.test_battler_1.Actor_Id = -1;
            if (Global.test_battler_2.Generic)
                Global.test_battler_2.Actor_Id = -1;
            //if (Global.test_battler_1.Generic)
            //    change_generic_actor();

            base.initialize();
            Weapons.Remove(0);
        }

        protected override Test_Battle_Character_Data test_battler { get { return State == Test_Battle_States.Unit_1 ? Global.test_battler_1 : Global.test_battler_2; } }

        protected override bool generic { get { return Global.test_battler_1.Generic; } }

        protected override int team()
        {
            return State == Test_Battle_States.Unit_1 ?
                Constants.Team.PLAYER_TEAM : Constants.Team.ENEMY_TEAM;
        }

        protected override void initialize_icons()
        {
            Icon_Y = 2;
            Item_Icons.Add(new Item_Icon_Sprite());
            Item_Icons[0].loc = new Vector2(Data_X - 40, 24 + Icon_Y * 16); // - 40//Yeti
            Item_Icons[0].stereoscopic = Config.TESTBATTLE_DATA_DEPTH;
        }

        protected override void refresh()
        {
            BlockedWeaponInputs = 0;

            Map_Sprite.texture = ((Scene_Title)Global.scene).get_team_map_sprite(team(), actor.map_sprite_name);
            if (Map_Sprite.texture != null)
            {
                Map_Sprite.offset = new Vector2(
                    (Map_Sprite.texture.Width / Map_Sprite.frame_count) / 2,
                    (Map_Sprite.texture.Height / Map_Sprite.facing_count) - 8);
            }
            Map_Sprite.mirrored = Constants.Team.flipped_map_sprite(team());

            if (State != Test_Battle_States.Confirm)
            {
                Test_Battle_Character_Data test_battler = this.test_battler;
                if (same_unit)
                {
                    if (test_battler == Global.test_battler_1)
                        test_battler = Global.test_battler_2;
                    else
                        test_battler = Global.test_battler_1;
                }

                Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Vector2.Zero, test_battler.Actor_Id, "");
                Game_Unit unit = Global.game_map.last_added_unit;
                // Actor data
                Data.Clear();
                for (int i = 0; i < max_index; i++)
                {
                    Data.Add(new FE_Text());
                    Data[i].loc = new Vector2(32, 24 + 16 * i);
                    Data[i].Font = "FE7_Text";
                    Data[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
                    Data[i].stereoscopic = Config.TESTBATTLE_DATA_DEPTH;
                }
                if (test_battler.Generic)
                {
                    Data[0].text = "Generic";
                    Data[1].text = actor.class_name;
                    Data[5].text = test_battler.Build.ToString();
                    Data[6].text = test_battler.Gender.ToString();
                }
                else
                {
                    Data[0].text = "Actor";
                    Data[1].text = actor.name;
                    if (same_unit)
                    {
                        for (int i = 2; i < max_index; i++)
                            Data[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");
                        if (State == Test_Battle_States.Unit_2)
                            Data[1].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");
                    }
                    Data[5].text = actor.tier.ToString() + ", " + actor.class_name;
                }
                Data[2].loc.X += 16;
                if (Global.data_weapons.ContainsKey(test_battler.Weapon_Id))
                {
                    Data[2].text = Global.data_weapons[test_battler.Weapon_Id].Name;
                    Item_Icons[0].texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/" + Global.data_weapons[test_battler.Weapon_Id].Image_Name);
                    Item_Icons[0].index = Global.data_weapons[test_battler.Weapon_Id].Image_Index;
                }
                else
                {
                    Data[2].text = "no weapon";
                    Item_Icons[0].texture = null;
                    Item_Icons[0].index = 0;
                }
                if (actor.weapon == null)
                    Data[2].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");
                Data[3].text = test_battler.Level.ToString();
                Data[4].text = test_battler.Prepromote_Levels.ToString();
                // Stats
                for (int i = 0; i < Stats.Length - 1; i++)
                {
                    Stats[i].text = actor.stat(i).ToString();
                    Stats[i].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_" + (actor.get_capped(i) ? "Green" : "Blue"));
                }
                Stats[Stats.Length - 1].text = actor.rating().ToString();
                // Combat Stats
                for (int i = 0; i < Combat_Stats.Length; i++)
                    Combat_Stats[i].texture = Global.Content.Load<Texture2D>(
                        @"Graphics/Fonts/FE7_Text_" + (actor.weapon == null ? "Grey" : "Blue"));
                // Force equips the weapon and gets the stats for it, whether it's valid or not, then requips the old weapon
                int weapon_id = unit.actor.weapon_id;
                unit.actor.weapon_id = test_battler.Weapon_Id;

                var stats = new Calculations.Stats.BattlerStats(unit.id);
                Combat_Stats[0].text = stats.hit().ToString();
                Combat_Stats[1].text = stats.dmg().ToString();
                Combat_Stats[2].text = stats.crt().ToString();
                Combat_Stats[3].text = stats.avo().ToString();
                Combat_Stats[4].text = stats.dodge().ToString();
                Combat_Stats[5].text = unit.atk_spd(this.distance).ToString();
                //Combat_Stats[6].text = "1"; //Yeti
                int min_range = unit.min_range();
                int max_range = unit.max_range();
                if (min_range == max_range && min_range == 0)
                    Combat_Stats[6].text = "--";
                else
                    Combat_Stats[6].text = min_range != max_range ? min_range.ToString() + "-" + max_range.ToString() : min_range.ToString(); //Yeti
                unit.actor.weapon_id = weapon_id;
            }
            else
            {
                Global.game_map.add_actor_unit(
                    Constants.Team.PLAYER_TEAM, Vector2.Zero,
                    Global.test_battler_1.Actor_Id, "");
                Game_Unit unit1 = Global.game_map.last_added_unit;
                Global.game_map.add_actor_unit(
                    Constants.Team.ENEMY_TEAM, Vector2.Zero,
                    Global.test_battler_2.Actor_Id, "");
                Game_Unit unit2 = Global.game_map.last_added_unit;

                int distance = this.distance;
                //Global.game_system.In_Arena = true;
                List<int?> stats = Combat.combat_stats(unit1.id, unit2.id, distance);
                //Global.game_system.In_Arena = false;
                for (int i = 0; i < Battle_Stats.Length; i++)
                {
                    int mult = 1;
                    if ((i - 1) % 4 == 0 && stats[i] != null)
                    {
                        var unit = i < 4 ? unit1 : unit2;
                        mult *= i < 4 ? unit1.attacks_per_round(unit2, distance) :
                            unit2.attacks_per_round(unit1, distance);
                        if (unit.actor.weapon != null && !unit.is_brave_blocked()) //brave //Yeti
                            mult *= unit.actor.weapon.HitsPerAttack;
                    }
                    Battle_Stats[i].text = stats[i] == null ? "--" : stats[i].ToString() + (mult > 1 ? " x" + mult.ToString() : "");
                }
            }
        }

        public event EventHandler<EventArgs> Confirm;
        protected void OnConfirm(EventArgs e)
        {
            if (Confirm != null)
                Confirm(this, e);
        }

        #region Update
        protected override void update_input(bool active)
        {
            if (active && this.ready_for_inputs)
            {
                if (State != Test_Battle_States.Confirm)
                {
                    if (Global.Input.repeated(Inputs.Left))
                    {
                        if (same_unit && Index > 1)
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                            update_option_change(false);
                        }
                    }
                    else if (Global.Input.repeated(Inputs.Right))
                    {
                        if (same_unit && Index > 1)
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                            update_option_change(true);
                        }
                    }
                    else if (Global.Input.repeated(Inputs.L))
                    {
                        if (same_unit && Index > 1)
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                            update_option_change(false, true);
                        }
                    }
                    else if (Global.Input.repeated(Inputs.R))
                    {
                        if (same_unit && Index > 1)
                            Global.game_system.play_se(System_Sounds.Buzzer);
                        else
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move2);
                            update_option_change(true, true);
                        }
                    }
                    else if (Global.Input.repeated(Inputs.Down))
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move1);
                        move_down();
                        validate_weapon(actor);
                    }
                    else if (Global.Input.repeated(Inputs.Up))
                    {
                        Global.game_system.play_se(System_Sounds.Menu_Move1);
                        move_up();
                        validate_weapon(actor);
                    }
                }
                if (Global.Input.triggered(Inputs.A))
                {
                    switch (State)
                    {
                        case Test_Battle_States.Unit_1:
                            if (same_unit)
                            {
                                Global.game_system.play_se(System_Sounds.Cancel);
                                Global.test_battler_1 = Global.test_battler_2;
                                Global.test_battler_2 = new Test_Battle_Character_Data();
                                refresh();
                            }
                            else
                            {
                                if (actor.maxhp <= 0)
                                    Global.game_system.play_se(System_Sounds.Buzzer);
                                else if (!valid_weapon_test())
                                    Global.game_system.play_se(System_Sounds.Buzzer);
                                else
                                {
                                    Global.game_system.play_se(System_Sounds.Confirm);
                                    State = Test_Battle_States.Unit_2;
                                    if (!Global.game_actors.ContainsKey(Global.test_battler_2.Actor_Id) ||
                                        (!Global.game_actors.is_temp_actor(Global.test_battler_2.Actor_Id) &&
                                        !Global.game_actors.actor_loaded(Global.test_battler_2.Actor_Id)))
                                    {
                                        int weapon_id = Global.test_battler_2.Weapon_Id;
                                        setup_actor(Global.test_battler_2);
                                        Global.test_battler_2.Weapon_Id = weapon_id;
                                    }
                                    refresh();
                                }
                            }
                            break;
                        case Test_Battle_States.Unit_2:
                            if (same_unit)
                                Global.game_system.play_se(System_Sounds.Buzzer);
                            else
                            {
                                if (actor.maxhp <= 0)
                                    Global.game_system.play_se(System_Sounds.Buzzer);
                                else if (!valid_weapon_test())
                                    Global.game_system.play_se(System_Sounds.Buzzer);
                                else
                                {
                                    Global.game_system.play_se(System_Sounds.Confirm);
                                    State = Test_Battle_States.Confirm;
                                    refresh();
                                }
                            }
                            break;
                        case Test_Battle_States.Confirm:
                            Global.game_system.play_se(System_Sounds.Confirm);
                            OnConfirm(new EventArgs());
                            break;
                    }
                }
                else if (Global.Input.triggered(Inputs.B))
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    switch (State)
                    {
                        case Test_Battle_States.Unit_1:
                            close();
                            break;
                        case Test_Battle_States.Unit_2:
                            State = Test_Battle_States.Unit_1;
                            refresh();
                            break;
                        case Test_Battle_States.Confirm:
                            State = Test_Battle_States.Unit_2;
                            refresh();
                            break;
                    }
                }
            }
        }

        private bool valid_weapon_test()
        {
            if (actor.weapon == null)
            {
                Global.game_system.play_se(System_Sounds.Buzzer);
                BlockedWeaponInputs++;
                if (Index != 2)
                {
                    validate_weapon(actor);
                }
                else if (BlockedWeaponInputs >= BLOCK_INVALID_WEAPON_TIMES)
                {
                    validate_weapon(actor, true);
                }

                return false;
            }
            return true;
        }
        #endregion

        #region Movement
        protected override void update_option_change(bool right, bool trigger)
        {
            Test_Battle_Character_Data test_battler = this.test_battler;
            Data_Class class_data;
            switch (Index)
            {
                // Generic
                case 0:
                    test_battler.Generic = !test_battler.Generic;
                    if (test_battler.Generic)
                    {
                        test_battler.Class_Id = Test_Battle_Character_Data.default_class_id;
                        test_battler.Build = Generic_Builds.Strong;
                        test_battler.Level = 1;
                        change_generic_actor();
                    }
                    else
                    {
                        test_battler.Actor_Id = 1;
                        change_actor();
                    }
                    if (!same_unit)
                        setup_actor(test_battler);
                    break;
                case 1:
                    if (test_battler.Generic)
                    {
                        // Class Id
                        test_battler.Class_Id = Classes[
                            (Classes.IndexOf(test_battler.Class_Id) + Classes.Count + (right ? 1 : -1)) % Classes.Count];
                        change_generic_actor();
                    }
                    else
                    {
                        // Actor Id
                        test_battler.Actor_Id = Actors[
                            (Actors.IndexOf(test_battler.Actor_Id) + Actors.Count + (right ? 1 : -1)) % Actors.Count];
                        change_actor();
                    }
                    if (!same_unit)
                        setup_actor(test_battler);
                    break;
                // Weapon
                case 2:
                    if (trigger)
                    {
                        test_battler.Weapon_Id = next_weapon_group(right, test_battler.Weapon_Id);
                        if (test_battler.Weapon_Id == 0)
                            test_battler.Weapon_Id = next_weapon_group(right, test_battler.Weapon_Id);
                    }
                    else
                        test_battler.Weapon_Id = Weapons[
                            (Weapons.IndexOf(test_battler.Weapon_Id) + Weapons.Count +
                            (Global.Input.pressed(Inputs.Y) ? 5 : 1) * (right ? 1 : -1)) % Weapons.Count];

                    actor.clear_items();
                    actor.gain_item(new Item_Data(Item_Data_Type.Weapon, test_battler.Weapon_Id, 1));
                    test_battler.Items[0] = actor.items[0];
                    validate_weapon(actor);
                    break;
                // Level
                case 3:
                    if (right)
                    {
                        if (test_battler.Level < actor.level_cap())
                            test_battler.Level++;
                    }
                    else
                    {
                        if (test_battler.Generic)
                        {
                            if (test_battler.Level > 1)
                                test_battler.Level--;
                        }
                        else
                        {
                            Data_Actor actor_data = Global.data_actors[test_battler.Actor_Id];
                            if (test_battler.Level > (actor.tier == Global.data_classes[actor_data.ClassId].Tier ?
                                    Global.data_actors[test_battler.Actor_Id].Level : 1))
                                test_battler.Level--;
                        }
                    }
                    setup_actor(test_battler);
                    break;
                // Prepromote levels
                case 4:
                    if (test_battler.Generic)
                    {
                        if (right)
                        {
                            int max_level = 0;
                            if (actor.tier > 0)
                            {
                                max_level += Constants.Actor.TIER0_LVL_CAP;
                                max_level += (actor.tier - 1) * Constants.Actor.LVL_CAP;
                            }
                            if (test_battler.Prepromote_Levels < max_level)
                                test_battler.Prepromote_Levels++;
                        }
                        else
                        {
                            int min_level = 0;
                            if (actor.tier > 0)
                            {
                                min_level += Constants.Actor.TIER0_LVL_CAP;
                                min_level += (actor.tier - 1) * Config.PROMOTION_LVL;
                            }
                            if (test_battler.Prepromote_Levels > min_level)
                                test_battler.Prepromote_Levels--;
                        }
                    }
                    else
                    {
                        Data_Actor actor_data = Global.data_actors[test_battler.Actor_Id];
                        class_data = Global.data_classes[actor_data.ClassId];
                        if (right)
                        {
                            int max_level = -actor_data.Level;
                            if (test_battler.Tier > 0 && class_data.Tier <= 0)
                                max_level += Constants.Actor.TIER0_LVL_CAP;
                            if (test_battler.Tier > 1 && class_data.Tier <= 1)
                            {
                                max_level += (test_battler.Tier - Math.Max(class_data.Tier, 1)) * Constants.Actor.LVL_CAP;
                            }
                            if (test_battler.Prepromote_Levels < max_level)
                                test_battler.Prepromote_Levels++;
                        }
                        else
                        {
                            int min_level = -actor_data.Level;
                            if (test_battler.Tier > 0 && class_data.Tier <= 0)
                                min_level += Constants.Actor.TIER0_LVL_CAP;
                            if (test_battler.Tier > 1 && class_data.Tier <= 1)
                            {
                                min_level += (test_battler.Tier - Math.Max(class_data.Tier, 1)) * Config.PROMOTION_LVL;
                            }
                            min_level = Math.Max(min_level, 0);
                            if (test_battler.Prepromote_Levels > min_level)
                                test_battler.Prepromote_Levels--;
                        }
                    }
                    setup_actor(test_battler);
                    break;
                case 5:
                    // Build
                    if (test_battler.Generic)
                    {
                        test_battler.Build = (Generic_Builds)(MathHelper.Clamp((int)test_battler.Build + (right ? 1 : -1),
                            (int)Generic_Builds.Weak, (int)Generic_Builds.Strong));
                    }
                    // Tier
                    else
                    {
                        Data_Actor actor_data = Global.data_actors[test_battler.Actor_Id];
                        class_data = Global.data_classes[actor_data.ClassId];
                        if (right)
                        {
                            int new_class = actor_data.ClassId;
                            int max_tier = class_data.Tier;
                            while (Global.data_classes[new_class].can_promote())
                            {
                                new_class = Global.data_classes[new_class].promotion_keys.Min();
                                max_tier = Global.data_classes[new_class].Tier;
                            }
                            if (test_battler.Tier < max_tier)
                                test_battler.Tier++;
                        }
                        else
                        {
                            if (test_battler.Tier > class_data.Tier)
                            {
                                test_battler.Tier--;
                                test_battler.Level = Math.Max(test_battler.Level, actor_data.Level);
                            }
                        }
                        int max_level = -actor_data.Level;
                        int min_level = -actor_data.Level;
                        if (test_battler.Tier > 0 && class_data.Tier <= 0)
                        {
                            max_level += Constants.Actor.TIER0_LVL_CAP;
                            min_level += Constants.Actor.TIER0_LVL_CAP;
                        }
                        if (test_battler.Tier > 1 && class_data.Tier <= 1)
                        {
                            max_level += (test_battler.Tier -
                                Math.Max(class_data.Tier, 1)) * Constants.Actor.LVL_CAP;
                            min_level += (test_battler.Tier -
                                Math.Max(class_data.Tier, 1)) * Config.PROMOTION_LVL;
                        }
                        min_level = Math.Max(min_level, 0);
                        max_level = Math.Max(max_level, min_level);
                        test_battler.Prepromote_Levels = (int)MathHelper.Clamp(test_battler.Prepromote_Levels, min_level, max_level);
                    }
                    setup_actor(test_battler);
                    break;
                case 6:
                    // Gender
                    if (test_battler.Generic)
                    {
                        test_battler.Gender = (int)(MathHelper.Clamp(test_battler.Gender + (right ? 1 : -1), 0, 5));
                        actor.gender = test_battler.Gender;
                    }
                    break;
            }
            refresh();
        }

        private void validate_weapon(Game_Actor actor, bool forceEquip = false)
        {
            if (forceEquip || actor.is_equippable(0))
            {
                actor.equip(1);
                Data[2].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
            }
            else
            {
                BlockedWeaponInputs = 0;
                actor.unequip();
                Data[2].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Grey");
            }
        }

        protected override void update_selecting_option()
        {
            switch (Index)
            {
                default:
                    Global.game_system.play_se(System_Sounds.Buzzer);
                    break;
            }
        }
        #endregion

        #region Draw
        protected override void draw_background(SpriteBatch sprite_batch)
        {
            if (Background != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Background.draw(sprite_batch);
                sprite_batch.End();
            }
        }

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            if (State != Test_Battle_States.Confirm)
            {
                foreach(Item_Icon_Sprite icon in Item_Icons)
                    icon.draw(sprite_batch);
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                foreach (FE_Text data in Data)
                    data.draw(sprite_batch);
                foreach (FE_Text label in Stat_Labels)
                    label.draw(sprite_batch);
                foreach (FE_Text_Int stat in Stats)
                    stat.draw(sprite_batch);
                foreach (FE_Text label in Combat_Stat_Labels)
                    label.draw(sprite_batch);
                foreach (FE_Text_Int stat in Combat_Stats)
                    stat.draw(sprite_batch);
                Map_Sprite.draw(sprite_batch);
                // Cursor
                Cursor.draw(sprite_batch);
            }
            else
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                foreach (FE_Text_Int stat in Battle_Stats)
                    stat.draw(sprite_batch);
            }
            sprite_batch.End();
        }
        #endregion
    }
}
#endif
