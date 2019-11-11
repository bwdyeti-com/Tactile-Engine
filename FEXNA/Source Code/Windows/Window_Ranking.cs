using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;

namespace FEXNA
{
    class Window_Ranking
    {
        private int Timer, Phase;
        private int Turns, Combat, Exp, Total;
        private bool Playing_Sound;
        private System_Color_Window Window_Img;
        private Sprite Black_Fill, White_Screen, Banner;
        private Rectangle Banner_Scissor_Rect = new Rectangle(0, 0, 0, 0);
        private Ranking_Burst Burst;
        private List<FE_Text> Text;
        private Icon_Sprite Rank;
        private Game_Ranking Ranking;

        #region Accessors
        public bool is_ready { get { return Phase > 17; } }

        private bool skip_input_triggered
        {
            get
            {
                return Global.Input.triggered(Inputs.A) ||
                    Global.Input.triggered(Inputs.B) ||
                    Global.Input.triggered(Inputs.Start) ||
                    Global.Input.mouse_click(MouseButtons.Left) ||
                    Global.Input.gesture_triggered(TouchGestures.Tap);
            }
        }
        #endregion

        public Window_Ranking() : this(new Game_Ranking()) { }
        public Window_Ranking(Game_Ranking ranking)
        {
            Ranking = ranking;

            Banner = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Pictures/Ranking_Banner"));
            Banner.loc = new Vector2(0, 24);
            Banner.visible = false;
            Banner.stereoscopic = Config.RANKING_BANNER_DEPTH;
            Black_Fill = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            Black_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Fill.tint = new Color(0, 0, 0, 0);
            White_Screen = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            White_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            White_Screen.tint = new Color(0, 0, 0, 0);
            Burst = new Ranking_Burst();
            Burst.loc = new Vector2(Config.WINDOW_WIDTH / 2, Config.WINDOW_HEIGHT / 2 + 16);
            Burst.tint = new Color(0, 0, 0, 0);
            Burst.stereoscopic = Config.RANKING_BURST_DEPTH;
            Rank = new Icon_Sprite();
            Rank.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Ranking_Letters");
            Rank.size = new Vector2(144, 144);
            Rank.columns = 4;
            Rank.loc = new Vector2(104, 128);
            Rank.offset = new Vector2(72, 72);
            Rank.index = Ranking.ranking_index;
            Rank.tint = new Color(0, 0, 0, 0);

            Window_Img = new SystemWindowHeadered();
            Window_Img.width = 120;
            Window_Img.set_lines(4);
            Window_Img.loc = new Vector2(320, 80);
            Window_Img.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            Text = new List<FE_Text>();
            // Chapter name and difficulty
            var name = new FE_Text(
                "FE7_Text",
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow"),
                new Vector2(12, 8),
                Global.data_chapters[ranking.ChapterId].ShortName);
            name.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            Text.Add(name);
            var difficulty = new FE_Text_Int(
                "FE7_Text",
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue"),
                new Vector2(Window_Img.width - 12, 8),
                ranking.Difficulty.ToString());
            difficulty.stereoscopic = Config.RANKING_WINDOW_DEPTH;
            Text.Add(difficulty);

            for (int i = 0; i < 4; i++)
            {
                var label = new FE_Text();
                label.loc = new Vector2(8, 24 + i * 16);
                label.Font = "FE7_Text";
                label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
                label.stereoscopic = Config.RANKING_WINDOW_DEPTH;
                Text.Add(label);
            }
            Text[Text.Count - 4].text = "Turns";
            Text[Text.Count - 3].text = "Combat";
            Text[Text.Count - 2].text = "Experience";
            Text[Text.Count - 1].text = "Total";
            //Text[3].text = "MVP";
            for (int i = 0; i < 4; i++)
            {
                var value = new FE_Text_Int();
                value.loc = new Vector2(Window_Img.width - 8, 24 + i * 16);
                value.Font = "FE7_Text";
                value.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");
                value.stereoscopic = Config.RANKING_WINDOW_DEPTH;
                Text.Add(value);
            }
            Text[Text.Count - 4].text = "0";
            Text[Text.Count - 3].text = "0";
            Text[Text.Count - 2].text = "0";
            Text[Text.Count - 1].text = "0";
            //Text[7].text = Global.game_map.units[1].actor.name;
//#if DEBUG
            //Text[5].text += " (" + Global.game_system.chapter_turn + ":" + Global.data_chapters[Global.game_state.chapter_id].Ranking_Turns + ")";
            //Text[6].text += " (" + Global.game_system.chapter_damage_taken + ":" + Global.data_chapters[Global.game_state.chapter_id].Ranking_Combat + ")";
            //Text[7].text += " (" + Global.game_system.chapter_exp_gain + ":" + Global.data_chapters[Global.game_state.chapter_id].Ranking_Exp + ")";
//#endif
            update();
        }

        public void update()
        {
            Burst.update();
            int alpha, banner_width;

            if (Phase < 6)
                if (this.skip_input_triggered)
                {
                    Black_Fill.tint = new Color(0, 0, 0, 160);
                    Burst.scale = new Vector2(1f);
                    Burst.tint = new Color(255, 255, 255, 64);
                    Banner.visible = true;
                    Banner_Scissor_Rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                    Window_Img.loc = new Vector2(168, Window_Img.loc.Y);
                    White_Screen.tint = new Color(0, 0, 0, 0);

                    Phase = 6;
                    Timer = 0;
                }

            switch (Phase)
            {
                #region 0: Fade in
                case 0:
                    switch (Timer)
                    {
                        case 15:
                            Black_Fill.tint = new Color(0, 0, 0, (Timer + 1) * 10);
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Black_Fill.tint = new Color(0, 0, 0, (Timer + 1) * 10);
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 1: White flash, burst fade in
                case 1:
                    switch (Timer)
                    {
                        case 0:
                            White_Screen.tint = new Color(64, 64, 64, 64);
                            Timer++;
                            break;
                        case 1:
                            White_Screen.tint = new Color(192, 192, 192, 192);
                            Timer++;
                            break;
                        case 2:
                            White_Screen.tint = new Color(255, 255, 255, 255);
                            Timer++;
                            break;
                        case 3:
                        case 4:
                        case 5:
                            Timer++;
                            break;
                        case 6:
                            White_Screen.tint = new Color(0, 0, 0, 0);
                            Burst.scale = new Vector2((Timer - 5) * (1/16f));
                            alpha = (Timer - 5) * 16;
                            Burst.tint = new Color(alpha, alpha, alpha, (Timer - 5) * 4);
                            Timer++;
                            break;
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                        case 16:
                        case 17:
                        case 18:
                        case 19:
                        case 20:
                            Burst.scale = new Vector2((Timer - 5) * (1/16f));
                            alpha = (Timer - 5) * 16;
                            Burst.tint = new Color(alpha, alpha, alpha, (Timer - 5) * 4);
                            Timer++;
                            break;
                        case 21:
                            Burst.scale = new Vector2(1f);
                            Burst.tint = new Color(255, 255, 255, 64);
                            Phase++;
                            Timer = 0;
                            break;
                    }
                    break;
                #endregion
                #region 2: Wait
                case 2:
                    switch (Timer)
                    {
                        case 15:
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 3: Banner Wipe In
                case 3:
                    switch (Timer)
                    {
                        case 0:
                            Banner.visible = true;
                            banner_width = (int)Math.Pow((Timer + 1) / 8f, 4) * 2;
                            Banner_Scissor_Rect = new Rectangle(
                                (Config.WINDOW_WIDTH / 2) - (banner_width / 2), 0, banner_width, Config.WINDOW_HEIGHT);
                            Timer++;
                            break;
                        case 28:
                            Banner_Scissor_Rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            banner_width = (int)Math.Pow((Timer + 1) / 8f, 4) * 2;
                            Banner_Scissor_Rect = new Rectangle(
                                (Config.WINDOW_WIDTH / 2) - (banner_width / 2), 0, banner_width, Config.WINDOW_HEIGHT);
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 4: Wait
                case 4:
                    switch (Timer)
                    {
                        case 15:
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 5: Slide Window On
                case 5:
                    switch (Timer)
                    {
                        case 20:
                            Window_Img.loc = new Vector2(168, Window_Img.loc.Y);
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Window_Img.loc = new Vector2(168 + 8 * (20 - Timer), Window_Img.loc.Y);
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 6: Wait
                case 6:
                    switch (Timer)
                    {
                        case 25:
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 7: Individual scores increment, then wait
                case 7:
                    switch (Timer)
                    {
                        case 0:
                            if (this.skip_input_triggered)
                            {
                                Global.game_system.cancel_sound();
                                Playing_Sound = false;
                                Text[Text.Count - 4].text = Ranking.turns.ToString();
                                Text[Text.Count - 3].text = Ranking.combat.ToString();
                                Text[Text.Count - 2].text = Ranking.exp.ToString();
                                Timer++;
                            }
                            else
                            {
                                bool ready = true;
                                if (Turns < Ranking.turns)
                                {
                                    Turns++;
                                    Text[Text.Count - 4].text = Turns.ToString();
                                    ready = false;
                                }
                                if (Combat < Ranking.combat)
                                {
                                    Combat++;
                                    Text[Text.Count - 3].text = Combat.ToString();
                                    ready = false;
                                }
                                if (Exp < Ranking.exp)
                                {
                                    Exp++;
                                    Text[Text.Count - 2].text = Exp.ToString();
                                    ready = false;
                                }

                                if (ready)
                                {
                                    Global.game_system.cancel_sound();
                                    Playing_Sound = false;
                                    Timer++;
                                }
                                else if (!Playing_Sound)
                                {
                                    Playing_Sound = true;
                                    Global.game_system.play_se(System_Sounds.Exp_Gain);
                                }
                            }
                            break;
                        case 25:
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 8: Total score increments, then wait
                case 8:
                    switch (Timer)
                    {
                        case 0:
                            if (this.skip_input_triggered)
                            {
                                Global.game_system.cancel_sound();
                                Playing_Sound = false;
                                Text[Text.Count - 1].text = Ranking.score.ToString();
                                Timer++;
                            }
                            else
                            {
                                bool ready = true;
                                if (Total < Ranking.score)
                                {
                                    Total++;
                                    if (Total < Ranking.score)
                                        Total++;
                                    if (Total < Ranking.score)
                                        Total++;
                                    Text[Text.Count - 1].text = Total.ToString();
                                    ready = false;
                                }

                                if (ready)
                                {
                                    Global.game_system.cancel_sound();
                                    Playing_Sound = false;
                                    Timer++;
                                }
                                else if (!Playing_Sound)
                                {
                                    Playing_Sound = true;
                                    Global.game_system.play_se(System_Sounds.Exp_Gain);
                                }
                            }
                            break;
                        case 30:
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 9: Letter fades in
                case 9:
                    switch (Timer)
                    {
                        case 30:
                            Global.Audio.play_se("Battle Sounds", "Lightning2");
                            Rank.scale = new Vector2(1f);
                            Rank.tint = new Color(255, 255, 255, 255);
                            Rank.stereoscopic = Config.RANKING_ICON_DEPTH;
                            White_Screen.tint = new Color(255, 255, 255, 255);
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Rank.scale = new Vector2(MathHelper.Lerp(0.65f, 6f,
                                (30 - Timer) / 30f));
                            alpha = (int)(255 * (Timer) / 30f);
                            Rank.tint = new Color(alpha, alpha, alpha, alpha);
                            Rank.stereoscopic = ((Timer) / 30f) * Config.RANKING_ICON_DEPTH +
                                (1 - (Timer) / 30f) * Config.RANKING_ICON_MAX_DEPTH;
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 10: White flash
                case 10:
                    switch (Timer)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            Timer++;
                            break;
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            alpha = 32 * (11 - Timer);
                            White_Screen.tint = new Color(alpha, alpha, alpha, alpha);
                            Timer++;
                            break;
                        case 11:
                            White_Screen.tint = new Color(0, 0, 0, 0);
                            Phase++;
                            Timer = 0;
                            break;
                    }
                    break;
                #endregion
                #region 11: Wait for input
                case 11:
                    if (this.skip_input_triggered)
                    {
                        Global.game_system.play_se(System_Sounds.Confirm);
                        Phase++;
                        Timer = 0;
                    }
                    break;
                #endregion
                #region 12: Wait
                case 12:
                    switch (Timer)
                    {
                        case 4:
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 13: Slide Window Off, Fade Letter
                case 13:
                    switch (Timer)
                    {
                        case 20:
                            Window_Img.loc = new Vector2(360, Window_Img.loc.Y);
                            Rank.tint = new Color(0, 0, 0, 0);
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Window_Img.loc = new Vector2(168 + 8 * (Timer), Window_Img.loc.Y);
                            alpha = (int)(256 * (19 - Timer) / 20f);
                            Rank.tint = new Color(alpha, alpha, alpha, alpha);
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 14: Wait
                case 14:
                    switch (Timer)
                    {
                        case 8:
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 15: Fade
                case 15:
                    switch (Timer)
                    {
                        case 15:
                            Burst.tint = new Color(0, 0, 0, 0);
                            Banner.tint = new Color(0, 0, 0, 0);
                            //Global.Audio.bgm_fade(20); //Debug
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            alpha = 16 * (15 - Timer);
                            Burst.tint = new Color(alpha, alpha, alpha, 4 * (15 - Timer));
                            Banner.tint = new Color(alpha, alpha, alpha, alpha);
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 16: Fade Background
                case 16:
                    switch (Timer)
                    {
                        case 15:
                            Black_Fill.tint = new Color(0, 0, 0, 0);
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            alpha = 10 * (15 - Timer);
                            Black_Fill.tint = new Color(0, 0, 0, alpha);
                            Timer++;
                            break;
                    }
                    break;
                #endregion
                #region 17: Wait
                case 17:
                    switch (Timer)
                    {
                        case 4:
                            Phase++;
                            Timer = 0;
                            break;
                        default:
                            Timer++;
                            break;
                    }
                    break;
                #endregion
            }
        }

        public void draw(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Black_Fill.draw(sprite_batch);
            Burst.draw(sprite_batch);
            sprite_batch.End();

            Rectangle scissor_rect = new Rectangle(Banner_Scissor_Rect.X +
                    (int)Stereoscopic_Graphic_Object.graphic_draw_offset(Config.RANKING_BANNER_DEPTH).X,
                Banner_Scissor_Rect.Y, Banner_Scissor_Rect.Width, Banner_Scissor_Rect.Height);
            sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(scissor_rect);
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                null, new RasterizerState { ScissorTestEnable = true });
            Banner.draw(sprite_batch);
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Window_Img.draw(sprite_batch);
            if (Window_Img.visible)
                foreach (FE_Text text in Text)
                    text.draw(sprite_batch, -Window_Img.loc);
            Rank.draw(sprite_batch);
            White_Screen.draw(sprite_batch);
            sprite_batch.End();
        }
    }
}
