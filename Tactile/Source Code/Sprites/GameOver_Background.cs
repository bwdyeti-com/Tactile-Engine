using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    enum GameOver_Phase { Fade_In, Wait, Fade_Up, Fade_Out }
    class GameOver_Background : Stereoscopic_Graphic_Object
    {
        const int FADE_IN_TIME = 4 * 15;
        const int WAIT_TIME = 20;
        const int FADE_UP_TIME = 8 * 16;
        const int FADE_OUT_TIME = 40;
        const int MAX_TIME = 30 * 60 - (FADE_IN_TIME + WAIT_TIME + FADE_UP_TIME + FADE_OUT_TIME);

        Texture2D texture, text_texture, overlay;
        public Vector2 vel1 = new Vector2(-0.0625f, -0.2f);
        public Vector2 vel2 = new Vector2(1 / 5.5f, -6 / 17f);
        protected Vector2 Tile = new Vector2(4, 3);
        protected Vector2 offset2 = Vector2.Zero;
        protected GameOver_Phase Phase = GameOver_Phase.Fade_In;
        protected int Timer = 0;

        #region Accessors
        public bool finished { get { return Phase == GameOver_Phase.Fade_Out && Timer > FADE_OUT_TIME; } }

        public bool fading_in { get { return Phase == GameOver_Phase.Fade_In; } }
        #endregion

        public GameOver_Background()
        {
            texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/GameOver_Background");
            text_texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/GameOver_Text");
            overlay = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
        }

        public void update()
        {
            switch(Phase)
            {
                case GameOver_Phase.Fade_In:
                    Timer++;
                    if (Timer > FADE_IN_TIME)
                    {
                        Phase = GameOver_Phase.Wait;
                        Timer = 0;
                    }
                    break;
                case GameOver_Phase.Wait:
                    Timer++;
                    if (Timer > WAIT_TIME)
                    {
                        Phase = GameOver_Phase.Fade_Up;
                        Timer = 0;
                    }
                    break;
                case GameOver_Phase.Fade_Up:
                    Timer++;
                    if (Timer > MAX_TIME ||  (Timer > FADE_UP_TIME && (
                        Global.Input.triggered(Inputs.A) || Global.Input.triggered(Inputs.B) ||
                        Global.Input.triggered(Inputs.Start) ||
                        Global.Input.any_mouse_triggered ||
                        Global.Input.gesture_triggered(TouchGestures.Tap))))
                    {
                        Global.Audio.BgmFadeOut();
                        Phase = GameOver_Phase.Fade_Out;
                        Timer = 0;
                    }
                    update_offset();
                    break;
                case GameOver_Phase.Fade_Out:
                    Timer++;
                    update_offset();
                    break;
            }
        }

        protected void update_offset()
        {
            offset -= vel1;
            if (texture != null)
            {
                while (offset.X >= texture.Width)
                    offset.X -= texture.Width;
                while (offset.X <= 0)
                    offset.X += texture.Width;
                while (offset.Y >= texture.Height)
                    offset.Y -= texture.Height;
                while (offset.Y <= 0)
                    offset.Y += texture.Height;
            }
            offset2 -= vel2;
            if (texture != null)
            {
                while (offset2.X >= texture.Width)
                    offset2.X -= texture.Width;
                while (offset2.X <= 0)
                    offset2.X += texture.Width;
                while (offset2.Y >= texture.Height)
                    offset2.Y -= texture.Height;
                while (offset2.Y <= 0)
                    offset2.Y += texture.Height;
            }
        }

        public void draw(SpriteBatch sprite_batch)
        {
            if (!(texture == null))
            {
                if (Phase == GameOver_Phase.Fade_Up || Phase == GameOver_Phase.Fade_Out)
                {
                    for (int y = 0; y < Tile.Y; y++)
                        for (int x = 0; x < Tile.X; x++)
                        {
                            sprite_batch.Draw(texture, (loc + draw_vector()) +
                                new Vector2(x * texture.Width, y * texture.Height), new Rectangle(0, 0, texture.Width, texture.Height),
                                Color.White, 0f, new Vector2((int)offset.X, (int)offset.Y), Vector2.One,
                                SpriteEffects.None, 0f);
                        }
                    for (int y = 0; y < Tile.Y; y++)
                        for (int x = 0; x < Tile.X; x++)
                        {
                            sprite_batch.Draw(texture, (this.loc + draw_vector()) +
                                new Vector2(x * texture.Width, y * texture.Height), new Rectangle(0, 0, texture.Width, texture.Height),
                                new Color(255, 255, 255, 0), 0f, new Vector2((int)offset2.X, (int)offset2.Y), Vector2.One,
                                SpriteEffects.FlipHorizontally, 0f);
                        }
                    sprite_batch.Draw(overlay, new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT),
                        new Color(0, 0, 0, Phase != GameOver_Phase.Fade_Up ? 0 : (
                            Timer < FADE_UP_TIME / 2 ? 255 : Math.Min(255, 256 - (Timer - FADE_UP_TIME / 2) / 4 * 16))));
                    for (int i = 0; i < 16; i++)
                    {
                        sprite_batch.Draw(overlay, new Rectangle(0, i * 4, Config.WINDOW_WIDTH, 4), new Color(0, 0, 0, Math.Min(255, 256 - (i * 16))));
                        sprite_batch.Draw(overlay, new Rectangle(0, Config.WINDOW_HEIGHT - i * 4, Config.WINDOW_WIDTH, 4), new Color(0, 0, 0, Math.Min(255, 256 - (i * 16))));
                    }
                    sprite_batch.Draw(text_texture, (loc + draw_vector()) +
                        new Vector2(Config.WINDOW_WIDTH / 2, Config.WINDOW_HEIGHT / 2), new Rectangle(0, 0, text_texture.Width, text_texture.Height),
                        Color.White, 0f, new Vector2((int)text_texture.Width / 2, (int)text_texture.Height / 2), Vector2.One,
                        SpriteEffects.None, 0f);
                }
                sprite_batch.Draw(overlay, new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT),
                    new Color(0, 0, 0, Phase == GameOver_Phase.Fade_In ? Math.Min(255, (Timer / 4 + 1) * 16) : (
                        Phase == GameOver_Phase.Fade_Out ? Math.Min(255, (Timer + 1) * 10) : (
                        Phase == GameOver_Phase.Wait ? 255 : (int)MathHelper.Clamp(256 - (Timer / 8 + 1) * 16, 0, 255)))));
            }
        }
    }
}
