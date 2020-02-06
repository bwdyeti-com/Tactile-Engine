//Sparring
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Map;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;

namespace FEXNA.Windows.Target
{
    class Window_Target_Sparring : Window_Target_Arena
    {
        const int EXP_GAUGE_WIDTH = 48;

        private bool AttackerAdvantage;
        private int HighlightedAnim;
        private WindowPanel Battler1Window;
        private WindowPanel Battler2Window;
        private Character_Sprite Battler1Sprite, Battler2Sprite;
        private FE_Text Battler1ExpLabel, Battler2ExpLabel;
        private FE_Text_Int Battler1CurrentExp, Battler2CurrentExp;
        private FE_Text Battler1ExpGain, Battler2ExpGain;
        private Sparring_Exp_Gauge Battler1ExpGauge, Battler2ExpGauge;

        public Window_Target_Sparring(int unit_id, int target_id, Vector2 loc)
            : base(unit_id, target_id, loc)
        {
            Battler1Window = new System_Color_Window();
            Battler1Window.width = 72;
            Battler1Window.height = 48;
            Battler1Window.loc = new Vector2(-80, 32);

            Battler2Window = new System_Color_Window();
            Battler2Window.width = 72;
            Battler2Window.height = 48;
            Battler2Window.loc = new Vector2(
                this.window_width + (80 - Battler2Window.width), 32);

            Game_Unit battler_1 = Global.game_map.units[this.target];
            Game_Unit battler_2 = this.get_unit();

            float odds = Combat.combat_odds(
                battler_1, battler_2, Global.game_system.Arena_Distance,
                null, true, true);

            AttackerAdvantage = odds >= 0.5f;

            // Battler 1
            Battler1Sprite = new Character_Sprite();
            Battler1Sprite.draw_offset = new Vector2(20, 24);
            Battler1Sprite.facing_count = 3;
            Battler1Sprite.frame_count = 3;
            Battler1Sprite.loc = Battler1Window.loc + new Vector2(0, 0);
            Battler1Sprite.texture = Scene_Map.get_team_map_sprite(
                battler_1.team, battler_1.map_sprite_name);
            Battler1Sprite.offset = new Vector2(
                (Battler1Sprite.texture.Width / Battler1Sprite.frame_count) / 2,
                (Battler1Sprite.texture.Height / Battler1Sprite.facing_count) - 8);
            Battler1Sprite.mirrored = battler_1.has_flipped_map_sprite;
            

            Battler1ExpLabel = new FE_Text();
            Battler1ExpLabel.loc = Battler1Window.loc + new Vector2(40, 8);
            Battler1ExpLabel.Font = "FE7_TextL";
            Battler1ExpLabel.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_Yellow");
            Battler1ExpLabel.text = "E";

            Battler1CurrentExp = new FE_Text_Int();
            Battler1CurrentExp.loc = Battler1Window.loc + new Vector2(40 + 24, 8);
            Battler1CurrentExp.Font = "FE7_Text";
            Battler1CurrentExp.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_Blue");
            Battler1CurrentExp.text = battler_1.actor.exp.ToString();

            Battler1ExpGain = new FE_Text_Int();
            Battler1ExpGain.loc = Battler1Window.loc + new Vector2(8 + 56, 24 + 8);
            Battler1ExpGain.Font = "FE7_Text";
            Battler1ExpGain.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_Combat3");

            Battler1ExpGauge = new Sparring_Exp_Gauge();
            Battler1ExpGauge.loc = Battler1Window.loc + new Vector2(8, 24);
            Battler1ExpGauge.draw_offset = new Vector2(0, 2);
            Battler1ExpGauge.offset = new Vector2(-2, 0);

            // Battler 2
            Battler2Sprite = new Character_Sprite();
            Battler2Sprite.draw_offset = new Vector2(20, 24);
            Battler2Sprite.facing_count = 3;
            Battler2Sprite.frame_count = 3;
            Battler2Sprite.loc = Battler2Window.loc + new Vector2(0, 0);
            Battler2Sprite.texture = Scene_Map.get_team_map_sprite(
                battler_2.team, battler_2.map_sprite_name);
            Battler2Sprite.offset = new Vector2(
                (Battler2Sprite.texture.Width / Battler2Sprite.frame_count) / 2,
                (Battler2Sprite.texture.Height / Battler2Sprite.facing_count) - 8);
            Battler2Sprite.mirrored = battler_2.has_flipped_map_sprite;

            Battler2ExpLabel = new FE_Text();
            Battler2ExpLabel.loc = Battler2Window.loc + new Vector2(40, 8);
            Battler2ExpLabel.Font = "FE7_TextL";
            Battler2ExpLabel.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_Yellow");
            Battler2ExpLabel.text = "E";

            Battler2CurrentExp = new FE_Text_Int();
            Battler2CurrentExp.loc = Battler2Window.loc + new Vector2(40 + 24, 8);
            Battler2CurrentExp.Font = "FE7_Text";
            Battler2CurrentExp.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_Blue");
            Battler2CurrentExp.text = battler_2.actor.exp.ToString();

            Battler2ExpGain = new FE_Text_Int();
            Battler2ExpGain.loc = Battler2Window.loc + new Vector2(8 + 56, 24 + 8);
            Battler2ExpGain.Font = "FE7_Text";
            Battler2ExpGain.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Fonts/FE7_Text_Combat3");

            Battler2ExpGauge = new Sparring_Exp_Gauge(true);
            Battler2ExpGauge.loc = Battler2Window.loc + new Vector2(8, 24);
            Battler2ExpGauge.draw_offset = new Vector2(0, 2);
            Battler2ExpGauge.offset = new Vector2(-2, 0);

            // Exp gain ranges
            set_exp_gauge_values(battler_1, battler_2, Battler1ExpGauge, Battler1ExpGain);
            set_exp_gauge_values(battler_2, battler_1, Battler2ExpGauge, Battler2ExpGain);
        }

        protected override void update_begin()
        {
            base.update_begin();

            Battler1Window.update();
            Battler2Window.update();

            update_map_sprites();


            Battler1ExpLabel.update();
            Battler2ExpLabel.update();
            Battler1CurrentExp.update();
            Battler2CurrentExp.update();
            Battler1ExpGain.update();
            Battler2ExpGain.update();

            Battler1ExpGauge.update();
            Battler2ExpGauge.update();
        }

        private void update_map_sprites()
        {
            Battler1Sprite.update();
            Battler2Sprite.update();

            HighlightedAnim = (HighlightedAnim + 1) %
                Global.game_system.Unit_Highlight_Anim_Time;
            int frame = Global.game_system.unit_highlight_anim_frame(HighlightedAnim);
            if (AttackerAdvantage)
            {
                Battler1Sprite.frame =
                    (6 / 2 - 1) * Battler1Sprite.frame_count + frame;
                Battler2Sprite.frame = Global.game_system.unit_anim_idle_frame;
            }
            else
            {
                Battler1Sprite.frame = Global.game_system.unit_anim_idle_frame;
                Battler2Sprite.frame =
                    (6 / 2 - 1) * Battler2Sprite.frame_count + frame;
            }
        }

        private void set_exp_gauge_values(Game_Unit attacker, Game_Unit target,
            Sparring_Exp_Gauge gauge, FE_Text gainText)
        {
            int gauge_max_exp = (int)(Constants.Actor.EXP_TO_LVL * 1.6f);

            int exp = attacker.actor.exp;
            int loss_exp = Combat.training_exp(attacker, target, false);
            int win_exp = Combat.training_exp(attacker, target, true);

            gainText.text = string.Format("{0}-{1}",
                loss_exp, win_exp);

            loss_exp = Math.Min(loss_exp, gauge_max_exp - exp);
            win_exp = Math.Min(win_exp, gauge_max_exp - exp);

            gauge.bar_width = EXP_GAUGE_WIDTH;
            gauge.fill_width = (exp * EXP_GAUGE_WIDTH) / gauge_max_exp;
            gauge.bonus_width = (win_exp * EXP_GAUGE_WIDTH) / gauge_max_exp;
            gauge.malus_width = (loss_exp * EXP_GAUGE_WIDTH) / gauge_max_exp;
            if (gauge.malus_width_has_space && loss_exp > 0)
                gauge.malus_width = Math.Max(gauge.malus_width, 1);
        }

        public override void draw(SpriteBatch sprite_batch)
        {
            base.draw(sprite_batch);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Battler1Window.draw(sprite_batch, -this.Loc);
            Battler1Sprite.draw(sprite_batch, -this.Loc);
            Battler1ExpLabel.draw(sprite_batch, -this.Loc);
            Battler1CurrentExp.draw(sprite_batch, -this.Loc);
            Battler1ExpGain.draw(sprite_batch, -this.Loc);
            Battler1ExpGauge.draw(sprite_batch, -this.Loc);

            Battler2Window.draw(sprite_batch, -this.Loc);
            Battler2Sprite.draw(sprite_batch, -this.Loc);
            Battler2ExpLabel.draw(sprite_batch, -this.Loc);
            Battler2CurrentExp.draw(sprite_batch, -this.Loc);
            Battler2ExpGain.draw(sprite_batch, -this.Loc);
            Battler2ExpGauge.draw(sprite_batch, -this.Loc);
            sprite_batch.End();
        }
    }
}
