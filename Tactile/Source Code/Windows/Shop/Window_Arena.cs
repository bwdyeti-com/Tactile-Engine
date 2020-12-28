using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using Tactile.Map;
using TactileLibrary;

namespace Tactile
{
    enum Arena_Messages { Intro, Question, Confirm_A, Confirm_B, Win, Lose, Leave, Confirm_Not_Enough, Yield }
    class Window_Arena : Window_Business
    {
        readonly static Arena_Messages[] WAIT_TEXT = { Arena_Messages.Intro, Arena_Messages.Confirm_A, Arena_Messages.Confirm_B, Arena_Messages .Win,
            Arena_Messages.Lose, Arena_Messages.Leave, Arena_Messages.Confirm_Not_Enough, Arena_Messages.Yield };
        const Arena_Messages FINAL_MESSAGE = Arena_Messages.Confirm_B;

        protected int Unit_Id;
        private int Opponent_Id = -1;
        private bool Post_Fight;
        private int Actual_Distance = -1;
        private System_Color_Window Window;
        private TextSprite[] Labels = new TextSprite[5];

        #region Accessors
        public Game_Unit unit { get { return Global.game_map.units[Unit_Id]; } }

        private Game_Unit opponent { get { return Opponent_Id == -1 ? null : Global.game_map.units[Opponent_Id]; } }
        #endregion

        public Window_Arena(int unit_id, Shop_Data shop, bool post_fight)
        {
            Unit_Id = unit_id;
            Shop = shop;
            if (!Global.game_system.is_loss())
                Global.Audio.BgmFadeOut(60);
            Post_Fight = post_fight;
            if (Post_Fight)
            {
                Opponent_Id = Global.game_system.Battler_2_Id;
                Traded = true;
                Global.game_system.Battler_1_Id = -1;
                Global.game_system.Battler_2_Id = -1;
            }
            else
                Global.game_system.Arena_Retreat = false;
            initialize_images();
        }

        protected override int choice_offset()
        {
            return Shop.offsets[0];
        }

        #region Image Setup
        protected override void initialize_images()
        {
            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 0);
            // Background
            Background = new Sprite();
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Panoramas/Arena");
            // Darkened Bar
            Darkened_Bar = new Sprite();
            Darkened_Bar.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Darkened_Bar.dest_rect = new Rectangle(0, 8, Config.WINDOW_WIDTH, 48);
            Darkened_Bar.tint = new Color(0, 0, 0, 128);
            // Portrait BG
            Portrait_Bg = new Sprite();
            Portrait_Bg.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Shop_Portrait_bg");
            Portrait_Bg.src_rect = new Rectangle(0, 0, 57, 57);
            Portrait_Bg.loc = new Vector2(12, 4);
            // Gold Window
            Gold_Window = new Sprite();
            Gold_Window.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Gold_Window");
            Gold_Window.loc = new Vector2(212, 48);
            // Gold_Data
            Gold_Data = new RightAdjustedText();
            Gold_Data.loc = new Vector2(216 + 48, 48);
            Gold_Data.SetFont(Config.UI_FONT, Global.Content, "Blue");
            redraw_gold();
            Gold_G = new TextSprite();
            Gold_G.loc = new Vector2(216 + 48, 48);
            Gold_G.SetFont(Config.UI_FONT + "L", Global.Content, "Yellow", Config.UI_FONT);
            Gold_G.text = "G";
            // Text
            Message = new Message_Box(64, 8, 160, 2, false, "White");
            if (Post_Fight)
            {
                set_text(unit.is_dead ? Arena_Messages.Lose : (
                    Global.game_system.Arena_Retreat ? Arena_Messages.Yield : Arena_Messages.Win));
                Message.finished = true;
            }
            else
                set_text(Arena_Messages.Intro);

            Message_Active = true;
            // Choices

            set_images();
        }
        private void set_text(Arena_Messages message_id)
        {
            set_text((int)message_id);
        }
        #endregion

        #region Processing
        protected override int item_cost()
        {
            return wager();
        }

        protected override bool is_wait_text(int message_id)
        {
            return WAIT_TEXT.Contains((Arena_Messages)message_id);
        }

        private void generate_opponent()
        {
            // doot doot 14 RNs; only 13 used so far
            // Get the tier level the unit should be fighting at
            int tier = unit.actor.tier;
            while (!Config.GLADIATORS.ContainsKey(tier))
                if (Config.GLADIATORS.Keys.Max() < tier)
                    tier--;
                else
                    tier++;
            // Get a list of possible opponents
            int[] distance = Config.ARENA_WEAPON_TYPES[unit.actor.determine_arena_weapon_type().Key].Key;
            List<Gladiators> opponents = new List<Gladiators>();
            int weapon_type = 0;
            foreach (KeyValuePair<Gladiators, Gladiator_Data> pair in Config.GLADIATORS[tier])
            {
                weapon_type = pair.Value.Weapon_Type;
                if (distance.Intersect(Config.ARENA_WEAPON_TYPES[weapon_type].Key).Any())
                {
                    opponents.Add(pair.Key);
                }
            }
            // Choose
            Gladiators key = opponents[(int)((Global.game_system.get_rng() / 100f) * opponents.Count)]; // 1 RN
            weapon_type = Config.GLADIATORS[tier][key].Weapon_Type;
            Actual_Distance = distance.Intersect(Config.ARENA_WEAPON_TYPES[weapon_type].Key).Max();
            Global.game_map.add_gladiator(
                Constants.Team.ENEMY_TEAM, Config.OFF_MAP,
                Config.GLADIATORS[tier][key].Class_Id,
                Config.GLADIATORS[tier][key].Gender, "");
            Opponent_Id = Global.game_map.last_added_unit.id;
            // Sets opponent's stats
            opponent.actor.name = "Gladiator";
            opponent.actor.level_down();
            opponent.actor.exp = 0;

            // Set opponent's level
            int level = 0;
            if (opponent.actor.tier < unit.actor.tier)
                level += Global.ActorConfig.RawLevelCap(opponent.actor.tier);
            else if (opponent.actor.tier == unit.actor.tier)
                level += unit.actor.level;
            // Add levels by RN
            for (int i = 0; i < 3; i++)
            {
                // Level is from player lvl-3 to lvl+3, distributed around +0
                int rn = Global.game_system.get_rng(); // 3 RNs
                if (rn < 33)
                    level--;
                else if (rn > 66)
                    level++;
            }
            // Get extra exp for prepromote levels or difficulty
            int exp_gain = 0;
            if (Global.ActorConfig.ResetLevelOnPromotion)
            {
                exp_gain = Global.ActorConfig.LevelsBeforeTier(opponent.actor.tier);
            }
            exp_gain += Config.ARENA_LVL_BONUS[Global.game_system.Difficulty_Mode];
            exp_gain *= Global.ActorConfig.ExpToLvl;
            // Ensure level is within bounds
            int minLevel = Global.ActorConfig.ResetLevelOnPromotion ?
                1 : Global.ActorConfig.LevelsBeforeTier(opponent.actor.tier);
            if (level < minLevel)
            {
                exp_gain -= (minLevel - level) * Global.ActorConfig.ExpToLvl;
                level = minLevel;
            }
            else if (level > Global.ActorConfig.LevelCap(opponent.actor.tier))
            {
                exp_gain += (level - Global.ActorConfig.LevelCap(opponent.actor.tier)) *
                    Global.ActorConfig.ExpToLvl;
                level = Global.ActorConfig.LevelCap(opponent.actor.tier);
            }
            
            // 8 RNs, 1 for affinity, one each for 7 stats
            opponent.actor.setup_generic(Config.GLADIATORS[tier][key].Class_Id, level,
                0, exp_gain / Global.ActorConfig.ExpToLvl, Generic_Builds.Strong,
                Config.GLADIATORS[tier][key].Con, true);

            float stat_comparison =
                ((float)(unit.actor.stat_total()) / opponent.actor.stat_total()) *
                unit.actor.full_level / 3;
            // Decide weapon
            opponent.actor.wexp_set(Global.weapon_types[weapon_type],
                TactileLibrary.Data_Weapon.WLVL_THRESHOLDS[(int)TactileLibrary.Weapon_Ranks.A], false);
            opponent.actor.clear_wlvl_up();
            int weapon_id = -1;
            foreach (int id in Config.ARENA_WEAPON_TYPES[weapon_type].Value)
            {
                if (!opponent.actor.class_equippable(Global.data_weapons[id]))
                    break;
                opponent.actor.clear_items();
                opponent.actor.gain_item(new Item_Data(0, id, 100));
                weapon_id = id;
                if (Global.data_weapons[id].Mgt >= stat_comparison)
                    break;
            }
            opponent.actor.equip(1);
            int unit_weapon_id = unit.actor.weapon_id;
            unit.actor.weapon_id = Config.ARENA_WEAPON_TYPES[unit.actor.determine_arena_weapon_type().Key].Value[0];
            rebalance_arena_fight();
            unit.actor.weapon_id = unit_weapon_id;
        }

        private void rebalance_arena_fight()
        {
            // Mess with stats if people can't hit/damage
            Global.game_system.In_Arena = true;
            int dmg, hit;
            var stats = new Calculations.Stats.CombatStats(
                unit.id, opponent.id, distance: Actual_Distance);
            var opponent_stats = new Calculations.Stats.CombatStats(
                opponent.id, unit.id, distance: Actual_Distance);
            // Rounds for unit to kill gladiator
            int rounds_to_kill = (int)Math.Ceiling(
                ((float)opponent.actor.maxhp) / Math.Max(1, dmg = stats.dmg_per_round()));
            // While gladiator rounds to kill unit is more than unit's (max 6), increase gladiator Pow
            while ((dmg = opponent_stats.dmg_per_round()) <=
                unit.actor.maxhp / Math.Max(2, Math.Min(6, rounds_to_kill - 1)) &&
                !opponent.actor.get_capped(Stat_Labels.Pow))
            {
                opponent.actor.gain_stat(Stat_Labels.Pow, 2);
                opponent.actor.gain_stat(Stat_Labels.Def, -1);
                opponent.actor.gain_stat(Stat_Labels.Res, -1);
            }
            // While gladiator can one round unit, decrease Pow
            while ((dmg = opponent_stats.dmg_per_round()) >=
                unit.actor.maxhp &&
                opponent.actor.stat(Stat_Labels.Pow) > 0)
            {
                opponent.actor.gain_stat(Stat_Labels.Pow, -1);
                opponent.actor.gain_stat(Stat_Labels.Def, 1);
                opponent.actor.gain_stat(Stat_Labels.Res, 1);
            }
            while ((hit = opponent_stats.hit()) <= Config.MIN_ARENA_HIT &&
                !opponent.actor.get_capped(Stat_Labels.Skl))
            {
                opponent.actor.gain_stat(Stat_Labels.Skl, 3);
                opponent.actor.gain_stat(Stat_Labels.Spd, -1);
                opponent.actor.gain_stat(Stat_Labels.Lck, -2);
            }
            while ((dmg = stats.dmg_per_round()) <=
                (opponent.actor.maxhp / Config.MAX_ARENA_DMG_ROUNDS) &&
                (opponent.actor.stat(Stat_Labels.Def) > 0 || opponent.actor.stat(Stat_Labels.Res) > 0))
            {
                opponent.actor.gain_stat(Stat_Labels.Def, -1);
                opponent.actor.gain_stat(Stat_Labels.Res, -1);
            }
            while ((hit = stats.hit()) <= Config.MIN_ARENA_HIT
                && (opponent.actor.stat(Stat_Labels.Spd) > 0 || opponent.actor.stat(Stat_Labels.Lck) > 0))
            {
                opponent.actor.gain_stat(Stat_Labels.Spd, -1);
                opponent.actor.gain_stat(Stat_Labels.Lck, -1);
            }
            Global.game_system.In_Arena = false;
        }

        private void create_opponent_window()
        {
            Window = new System_Color_Window();
            Window.width = 136;
            Window.height = 48;
            Window.loc = new Vector2(Config.WINDOW_WIDTH / 2 - Window.width / 2, 88);
            // Labels
            for (int i = 0; i < Labels.Length - 1; i++)
            {
                Labels[i] = new TextSprite();
                Labels[i].loc = Window.loc + new Vector2(8 + 56 * (i % 2), 8 + 16 * (i / 2));
                Labels[i].SetFont(Config.UI_FONT, Global.Content, "White");
            }
            Labels[4] = new RightAdjustedText();
            Labels[4].loc = Window.loc + new Vector2(8 + 40, 8);
            Labels[4].SetFont(Config.UI_FONT, Global.Content, "Blue");
            Labels[0].text = "Lv";
            Labels[1].text = opponent.actor.class_name;
            Labels[2].text = "Enemy";
            Labels[3].text = opponent.actor.weapon.Name;
            Labels[4].text = opponent.actor.level.ToString();
        }

        private float combat_odds(Game_Unit unit, Game_Unit opponent)
        {
            Global.game_system.In_Arena = true;
            int unit_weapon_id = unit.actor.weapon_id;
            unit.actor.weapon_id = Config.ARENA_WEAPON_TYPES[unit.actor.determine_arena_weapon_type().Key].Value[0];

            float odds = 1 - Combat.combat_odds(unit, opponent, Actual_Distance, null, true, true);

            Global.game_system.In_Arena = false;
            unit.actor.weapon_id = unit_weapon_id;
            return odds;
        }

        private int wager()
        {
            int val = 0;
            if (Opponent_Id == -1)
                val = Config.WAGER_VARIANCE / 2 + Config.MIN_WAGER;
            else if (Global.game_system.Wager != -1)
                val = Global.game_system.Wager;
            else
            {
                float odds = combat_odds(unit, opponent);
                val = (int)(((Math.Asin((2 * odds) - 1) / Math.PI) + 0.5f) * Config.WAGER_VARIANCE + Config.MIN_WAGER);
                // Multiplies by 0.9 to 1.1
                val = (int)(val * (1 + ((Global.game_system.get_rng() - 50) / 500f)));
                // Truncates one's place
                Global.game_system.Wager = val = Math.Max(1, val / 10 * 10);
            }
            return val * (Message_Id == (int)Arena_Messages.Win ? 2 : 1);
        }
        #endregion

        protected override void play_shop_theme()
        {
            if (Global.game_system.is_loss())
                return;
            if (Post_Fight && !unit.is_dead && !Global.game_system.Arena_Retreat)
                Global.Audio.play_me("System Sounds", "Arena_Victory");
            Global.Audio.PlayBgm(Shop.song);
        }

        #region Update
        protected override void UpdateMenu(bool active)
        {
            update_black_screen();
            if (Delay > 0)
                Delay--; // Does this do anything //Yeti
            Background.update();

            bool input = !Closing && Delay == 0 && Black_Screen_Timer == 0;

            if (Choices != null)
            {
                Choices.Update(input && Message.text_end);
                update_cursor_location();
            }
            if (input)
            {
                if (Message_Active)
                {
                    update_message();
                }
                else
                {
                    update_main_selection();
                }
            }
            Face.update();
        }

        protected override void update_black_screen()
        {
            base.update_black_screen();
            if (!_Visible && Post_Fight)
                Black_Screen.TintA = 255;
        }

        protected override void update_message()
        {
            Message.update();
            if (Message.text_end && !Message.wait)
            {
                switch ((Arena_Messages)Message_Id)
                {
                    case Arena_Messages.Intro:
                        generate_opponent();
                        set_text(Arena_Messages.Question);
                        break;
                    case Arena_Messages.Question:
                        Message_Active = false;
                        set_choices(choice_offset(), "Yes", "No");
                        break;
                    case Arena_Messages.Confirm_A:
                        Traded = true;
                        set_text(Arena_Messages.Confirm_B);
                        Message.finished = true;
                        break;
                    case Arena_Messages.Confirm_B:
                        unit.actor.weapon_id = Config.ARENA_WEAPON_TYPES[unit.actor.determine_arena_weapon_type().Key].Value[0];
                        Closing = true;
                        Black_Screen_Timer = BLACK_SCREEN_HOLD_TIMER + (BLACK_SCREEN_FADE_TIMER * 2);
                        break;
                    case Arena_Messages.Leave:
                    case Arena_Messages.Confirm_Not_Enough:
                    case Arena_Messages.Win:
                    case Arena_Messages.Lose:
                    case Arena_Messages.Yield:
                        if (Message_Id == (int)Arena_Messages.Win)
                        {
                            Global.battalion.gold += wager();
                            Global.Audio.play_se("System Sounds", "Gold_Change");
                            redraw_gold();
                            Wait = 30;
                        }
                        Global.game_system.Wager = -1;
                        if (Post_Fight)
                            Global.game_system.In_Arena = false;
                        if (opponent != null && !Global.game_system.In_Arena)
                        {
                            opponent.kill();
                            Global.game_map.remove_unit(Opponent_Id);
                        }
                        Post_Fight = false;
                        close();
                        Black_Screen_Timer = BLACK_SCREEN_HOLD_TIMER + (BLACK_SCREEN_FADE_TIMER * 2);
                        if (!Global.game_system.is_loss())
                            Global.Audio.BgmFadeOut(60);
                        break;
                }
            }
        }

        protected override void update_main_selection()
        {
            On_Buy = Choices.ActiveNodeIndex == 0;

            var selected = Choices.consume_triggered(
                Inputs.A, MouseButtons.Left, TouchGestures.Tap);

            if (selected.IsSomething)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Message_Active = true;
                if (On_Buy)
                {
                    if (Global.battalion.gold >= wager())
                    {
                        set_text(Arena_Messages.Confirm_A);
                        create_opponent_window();
                        Global.battalion.gold -= wager();
                        Global.Audio.play_se("System Sounds", "Gold_Change");
                        redraw_gold();
                        Global.game_system.Battle_Mode = Constants.Animation_Modes.Full;
                        Global.game_system.Battler_1_Id = Unit_Id;
                        Global.game_system.Battler_2_Id = Opponent_Id;
                        Global.game_system.Arena_Distance = Actual_Distance;
                        Global.game_state.to_arena();
                    }
                    else
                    {
                        set_text(Arena_Messages.Confirm_Not_Enough);
                        Message.finished = true;
                    }
                }
                else
                {
                    set_text(Arena_Messages.Leave);
                    Message.finished = true;
                }
                clear_choices();
                update_cursor_location(true);
            }
            else if (Global.Input.triggered(Inputs.B))
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                Message_Active = true;
                set_text(Arena_Messages.Leave);
                Message.finished = true;
                clear_choices();
            }
        }
        #endregion

        protected override void draw_background(SpriteBatch sprite_batch)
        {
            Effect background_shader = Global.effect_shader();
            if (background_shader != null)
            {
                background_shader.CurrentTechnique = background_shader.Techniques["Tone"];
                background_shader.Parameters["tone"].SetValue(Global.game_state.screen_tone.to_vector_4(1.0f));
            }
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, background_shader);
            Background.draw(sprite_batch);
            sprite_batch.End();
        }

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            if (Window != null)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Window.draw(sprite_batch);
                foreach (TextSprite label in Labels)
                    label.draw(sprite_batch);
                sprite_batch.End();
            }
        }
    }
}
