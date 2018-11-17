using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;

namespace FEXNA
{
    enum Chapter_Transition_Actions { Bg_In, Sigil_In, Pause1, Banner_Shape, Banner_In, Pause2, Bg_Out, Banner_Out, Finalize, Done}
    class Chapter_Transition_Effect : Sprite
    {
        int Loop = 0;
        int Timer = 0, Clear_Timer = 0;
        Chapter_Transition_Actions Action = Chapter_Transition_Actions.Bg_In;
        bool Clearing = false;
        int Bmp_Width;
        bool Bg_Move = false;
        Sprite Sigil, Banner, Banner_Bg, Black_Fill;
        Color Banner_Color = Color.Transparent;
        Color Text_Color = Color.White;
        FE_Text Text;
        Rectangle Text_Scissor_Rect;
        RasterizerState Text_State = new RasterizerState { ScissorTestEnable = true };

        public Chapter_Transition_Effect()
        {
            // Background
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Chapter Transition Background");
            Bmp_Width = texture.Width;
            while ((Loop - 1) * Bmp_Width < Math.Max((Config.WINDOW_WIDTH + Config.CH_TRANS_BG_DEPTH * 2), Config.WINDOW_HEIGHT))
                Loop++;
            opacity = 0;
            offset = new Vector2(Config.CH_TRANS_BG_DEPTH);
            stereoscopic = Config.CH_TRANS_BG_DEPTH;
            // Sigil
            Sigil = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Pictures/Chapter Transition Sigil"));
            Sigil.loc = new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2;
            Sigil.offset = new Vector2(Sigil.texture.Width, Sigil.texture.Height) / 2;
            Sigil.blend_mode = 1;
            Sigil.opacity = 0;
            Sigil.stereoscopic = Config.CH_TRANS_SIGIL_DEPTH;
            // Banner
            Banner = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Pictures/Chapter Transition Banner"));
            Banner.loc = new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2;
            Banner.offset = new Vector2(Banner.texture.Width, Banner.texture.Height) / 2;
            Banner.blend_mode = 1;
            Banner.opacity = 0;
            Banner.stereoscopic = Config.CH_TRANS_BANNER_DEPTH;
            // Banner Bg
            Banner_Bg = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Pictures/Chapter Transition Sigil"));
            Banner_Bg.loc = new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2;
            Banner_Bg.offset = new Vector2(Sigil.texture.Width, Banner.texture.Height) / 2;
            Banner_Bg.src_rect = new Rectangle(0, (Banner_Bg.texture.Height - Banner.texture.Height) / 2, Banner_Bg.texture.Width, Banner.texture.Height);
            Banner_Bg.tint = new Color(0, 0, 0, 0);
            Banner_Bg.stereoscopic = Config.CH_TRANS_SIGIL_DEPTH;
            // Text
            Text = new FE_Text();
            string str = Global.data_chapters[Global.game_state.chapter_id].ChapterTransitionName;
            Text.loc = new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT) / 2 - new Vector2(Font_Data.text_width(str, "FE7_Chapter") / 2, 8);
            Text_Scissor_Rect = new Rectangle(0, (int)Banner.loc.Y - Banner.texture.Height / 2, Config.WINDOW_WIDTH, Banner.texture.Height);
            Text.Font = "FE7_Chapter";
            Text.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Chapter");
            Text.text = str;
            Text.opacity = 0;
            Text.stereoscopic = Config.CH_TRANS_BANNER_DEPTH;
            // Black Screen
            Black_Fill = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/White_Square"));
            Black_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Fill.tint = new Color(0, 0, 0, 0);
        }

        public override void update()
        {
            if (Global.Input.triggered(Inputs.A) ||
                Global.Input.triggered(Inputs.Start) ||
                Global.Input.mouse_click(MouseButtons.Left) ||
                Global.Input.gesture_triggered(TouchGestures.Tap))
            {
                switch (Action)
                {
                    case Chapter_Transition_Actions.Bg_In:
                        if (Timer > 40)
                            clear();
                        break;
                    default:
                        clear();
                        break;
                    case Chapter_Transition_Actions.Banner_Out:
                        Timer = 96;
                        break;
                    case Chapter_Transition_Actions.Finalize:
                    case Chapter_Transition_Actions.Done:
                        break;
                }
            }
            if (Bg_Move)
            {
                offset.X++;
                offset.Y++;
                if (offset.X >= (Bmp_Width + Config.CH_TRANS_BG_DEPTH))
                {
                    offset.X -= Bmp_Width;
                    offset.Y -= Bmp_Width;
                }
            }
            if (Clearing)
            {
                Clear_Timer--;
                Black_Fill.TintA = (byte)Math.Min(255, Black_Fill.TintA + 32);
            }
            if (!Clearing || Clear_Timer > 0)
            {
                switch (Action)
                {
                    // Background fade in
                    case Chapter_Transition_Actions.Bg_In:
                        switch (Timer)
                        {
                            case 16:
                            case 17:
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                            case 26:
                            case 27:
                            case 28:
                            case 29:
                            case 30:
                            case 31:
                            case 32:
                            case 33:
                            case 34:
                            case 35:
                            case 36:
                            case 37:
                            case 38:
                            case 39:
                            case 40:
                            case 41:
                            case 42:
                            case 43:
                            case 44:
                                if (Timer % 4 == 0)
                                    opacity += 24;
                                Timer++;
                                break;
                            case 105:
                                Timer = 0;
                                Global.Audio.PlayBgm(Constants.Audio.Bgm.CHAPTER_TRANSITION_THEME);
                                Action = Chapter_Transition_Actions.Sigil_In;
                                break;
                            default:
                                Timer++;
                                break;
                        }
                        break;
                    // Sigil fade in
                    case Chapter_Transition_Actions.Sigil_In:
                        Timer++;
                        if (Timer % 4 == 0)
                            Sigil.opacity += 16;
                        if (Sigil.opacity >= 255)
                        {
                            Timer = 0;
                            Action = Chapter_Transition_Actions.Pause1;
                        }
                        break;
                    // Pause
                    case Chapter_Transition_Actions.Pause1:
                        switch (Timer)
                        {
                            case 88:
                                Timer = 0;
                                Action = Chapter_Transition_Actions.Banner_Shape;
                                break;
                            default:
                                Timer++;
                                break;
                        }
                        break;
                    // Banner reshapes
                    case Chapter_Transition_Actions.Banner_Shape:
                        switch (Timer)
                        {
                            case 0:
                                Global.game_system.play_se(System_Sounds.Chapter_Transition);
                                Banner.opacity = 255;
                                Banner_Color = Color.White;
                                Banner.scale.Y = 0.1f;
                                Timer++;
                                Banner.scale.X = (float)Math.Sqrt(Timer) / 4.0f;
                                break;
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 13:
                            case 14:
                            case 15:
                                Timer++;
                                Banner.scale.X = (float)Math.Sqrt(Timer) / 4.0f;
                                break;
                            case 16:
                                Timer++;
                                break;
                            case 17:
                            case 18:
                            case 19:
                                Banner.scale.Y = (Timer - 16) / 3.0f;
                                Timer++;
                                break;
                            case 20:
                            case 21:
                                Timer++;
                                break;
                            case 22:
                                Banner_Bg.opacity = 255;
                                Text.opacity = 255;
                                Timer = 0;
                                Action = Chapter_Transition_Actions.Banner_In;
                                break;
                        }
                        break;
                    // Banner fade in
                    case Chapter_Transition_Actions.Banner_In:
                        Timer++;
                        Banner_Color.A = (byte)((32 - Timer) * 8);
                        Text_Color.A = (byte)((32 - Timer) * 8);
                        if (Banner_Color.A <= 0)
                        {
                            Timer = 0;
                            Action = Chapter_Transition_Actions.Pause2;
                        }
                        break;
                    // Pause
                    case Chapter_Transition_Actions.Pause2:
                        switch (Timer)
                        {
                            case 120:
                                Global.Audio.BgmFadeOut(60);
                                Timer = 0;
                                Action = Chapter_Transition_Actions.Bg_Out;
                                break;
                            default:
                                Timer++;
                                break;
                        }
                        break;
                    // Background/Sigil fade out
                    case Chapter_Transition_Actions.Bg_Out:
                        Timer++;
                        if (Timer % 4 == 0)
                        {
                            opacity -= 16;
                            Sigil.opacity -= 16;
                            Banner_Bg.opacity -= 16;
                        }
                        if (Sigil.opacity <= 0)
                        {
                            Timer = 0;
                            Action = Chapter_Transition_Actions.Banner_Out;
                        }
                        break;
                    // Banner fade out
                    case Chapter_Transition_Actions.Banner_Out:
                        switch (Timer)
                        {
                            case 0:
                                Banner_Bg.opacity = 0;
                                Banner.blend_mode = 0;
                                Banner.opacity = 255;
                                Timer++;
                                break;
                            case 96:
                                if (Text_Scissor_Rect.Height == 2)
                                {
                                    Banner.visible = false;
                                    Text.visible = false;
                                    Timer = 0;
                                    Action = Chapter_Transition_Actions.Finalize;
                                }
                                else
                                {
                                    Text_Scissor_Rect = new Rectangle(Text_Scissor_Rect.X, Text_Scissor_Rect.Y + 1,
                                        Text_Scissor_Rect.Width, Text_Scissor_Rect.Height - 2);
                                }
                                break;
                            default:
                                Timer++;
                                break;
                        }
                        break;
                    // Pause
                    case Chapter_Transition_Actions.Finalize:
                        switch (Timer)
                        {
                            case 48:
                                Timer = 0;
                                Action = Chapter_Transition_Actions.Done;
                                break;
                            default:
                                Timer++;
                                break;
                        }
                        break;
                }
                Bg_Move = !Bg_Move;
            }
        }

        public void clear()
        {
            Global.Audio.BgmFadeOut(15);
            Global.game_system.cancel_sound();
            if (Clearing)
                return;
            Clearing = true;
            Clear_Timer = 8;
            Black_Fill.tint = new Color(0, 0, 0, 0);
        }

        public bool ready
        {
            get
            {
                if (Action >= Chapter_Transition_Actions.Banner_Out && !Clearing)
                    return true;
                return false;
            }
        }

        public bool done
        {
            get
            {
                if (Clearing && Clear_Timer <= 0)
                    return true;
                if (Action == Chapter_Transition_Actions.Done)
                    return true;
                return false;
            }
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    // Background/Sigil
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    Rectangle src_rect = this.src_rect;
                    Vector2 offset = this.offset;
                    if (mirrored) offset.X = src_rect.Width - offset.X;
                    for(int y = 0; y < Loop; y++)
                        for (int x = 0; x < Loop; x++)
                            sprite_batch.Draw(texture, loc + draw_vector() + new Vector2(x * Bmp_Width, y * Bmp_Width),
                                src_rect, tint, angle, offset, scale,
                                mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    Sigil.draw(sprite_batch);
                    Banner_Bg.draw(sprite_batch);
                    sprite_batch.End();
                    // Banner
                    Effect text_shader = Global.effect_shader();
                    if (text_shader != null)
                    {
                        text_shader.CurrentTechnique = text_shader.Techniques["Technique2"];
                        text_shader.Parameters["color_shift"].SetValue(Banner_Color.ToVector4());
                    }
                    sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(Text_Scissor_Rect);
                    if (sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0 && sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0)
                    {
                        sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Text_State, text_shader);
                        Banner.draw(sprite_batch);
                        sprite_batch.End();
                    }
                    // Text
                    if (text_shader != null)
                    {
                        text_shader.CurrentTechnique = text_shader.Techniques["Technique2"];
                        text_shader.Parameters["color_shift"].SetValue(Text_Color.ToVector4());
                    }
                    sprite_batch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(Text_Scissor_Rect);
                    if (sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0 && sprite_batch.GraphicsDevice.ScissorRectangle.Width > 0)
                    {
                        sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Text_State, text_shader);
                        Text.draw(sprite_batch);
                        sprite_batch.End();
                    }
                    // Black screen
                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    Black_Fill.draw(sprite_batch);
                    sprite_batch.End();
                }
        }
    }
}
