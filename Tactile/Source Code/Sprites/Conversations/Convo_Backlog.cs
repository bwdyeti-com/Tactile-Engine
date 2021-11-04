using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Windows;

namespace Tactile
{
    class Convo_Backlog
    {
        public bool Active { get; private set; }
        private int Index = -1;
        private TextSprite[] Text = new TextSprite[Config.CONVO_BACKLOG_LINES];
        private Sprite Black_Fill;
        private int FadeTimer = 0, PanTimer = 0;
        private ScrollComponent Scroll;
        private bool Fading_In = false;
        private string LastColorSet;
        protected Page_Arrow UpArrow, DownArrow;

        #region Accessors
        public bool ready { get { return FadeTimer == 0 && PanTimer == 0; } }
        
        public float stereoscopic
        {
            set
            {
                for (int i = 0; i < Text.Length; i++)
                    Text[i].stereoscopic = value;
            }
        }

        public bool started { get { return Index >= 0; } }
        #endregion

        public Convo_Backlog()
        {
            Active = false;
            for (int i = 0; i < Text.Length; i++)
            {
                Text[i] = new TextSprite();
                Text[i].loc = new Vector2(24, LineY(i));
                Text[i].SetFont(Config.CONVO_FONT);
                Text[i].stereoscopic = Config.CONVO_BACKLOG_DEPTH;
            }
            Black_Fill = new Sprite();
            Black_Fill.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Fill.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Fill.tint = new Color(0, 0, 0, 0);

            UpArrow = new Page_Arrow();
            UpArrow.loc = new Vector2(Config.WINDOW_WIDTH / 2 + 8, 4);
            UpArrow.angle = MathHelper.PiOver2;
            DownArrow = new Page_Arrow();
            DownArrow.loc = new Vector2(Config.WINDOW_WIDTH / 2 + 8, Config.WINDOW_HEIGHT - 4);
            DownArrow.mirrored = true;
            DownArrow.angle = MathHelper.PiOver2;

            Scroll = new ScrollComponent(
                new Vector2(Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT),
                new Vector2(Config.WINDOW_WIDTH, Window_Message.FontData.CharHeight),
                ScrollDirections.Vertical);
            Scroll.SetScrollSpeeds(
                Config.CONVO_BACKLOG_MAX_SCROLL_SPEED,
                Config.CONVO_BACKLOG_TOUCH_SCROLL_FRICTION);
            Scroll.loc = Vector2.Zero;
            Scroll.UpArrow = UpArrow;
            Scroll.DownArrow = DownArrow;
        }

        private int LineY(int index)
        {
            return (index - Text.Length) * Window_Message.FontData.CharHeight +
                Config.WINDOW_HEIGHT + 8;
        }
        
        public void add_text(char c)
        {
            if (!started)
            {
                if (Global.scene.is_map_scene)
                    throw new IndexOutOfRangeException("First convo backlog line not started");
                else
                    new_line();
            }
            Text[Index].text += c;
        }
        public void add_text(string str)
        {
            if (!started)
            {
                if (Global.scene.is_map_scene)
                    throw new IndexOutOfRangeException("First convo backlog line not started");
                else
                    new_line();
            }
            Text[Index].text += str;
        }

        public void new_speaker_line()
        {
            new_line();
            if (Index >= 1)
                new_line();
            Text[Index].offset.X = 8;
            LastColorSet = null;
        }
        public void new_line()
        {
            Index++;
            if (Index >= Text.Length)
                remove_lines((Text.Length + 1) - Index);
            Text[Index].text = "";
            Text[Index].clear_text_colors();
            if (!string.IsNullOrEmpty(LastColorSet))
            {
                Text[Index].SetTextFontColor(0, LastColorSet);
            }
            Text[Index].offset = Vector2.Zero;
            // Scroll to the bottom
            Scroll.SetElementLengths(new Vector2(1, Index + 2));
            Scroll.ScrollToBottom();
        }

        public void set_color(string color)
        {
            if (!started)
                throw new IndexOutOfRangeException("First convo backlog line not started");
            Text[Index].SetTextFontColor(color);
            LastColorSet = color;
        }

        public void reset()
        {
            Index = -1;
            LastColorSet = null;
        }

        //@Debug: this is never called?
        private void add_line(List<string> lines, List<string> colors)
        {
            if (lines.Count + Index > Text.Length)
                remove_lines(Text.Length - (lines.Count + Index));
            for (int i = Math.Max(lines.Count - Text.Length, 0); i < lines.Count; i++)
            {
                Index++;
                Text[Index].text = lines[i];
                Text[Index].SetTextFontColor(0, colors[i]);
            }
            // Scroll to the bottom
            Scroll.SetElementLengths(new Vector2(1, Index + 2));
            Scroll.ScrollToBottom();
        }

        private void remove_lines(int count)
        {
            if (count > Index)
            {
                Index = -1;
                return;
            }

            // Move the oldest lines to the end
            TextSprite[] removed = new TextSprite[count];
            for (int i = 0; i < count; i++)
            {
                removed[i] = Text[i];
                removed[i].text = "";
                removed[i].clear_text_colors();
            }
            for (int i = 0; i < (Text.Length - count); i++)
            {
                Text[i] = Text[i + count];
                Text[i].loc.Y = LineY(i);
            }
            for (int i = 0; i < count; i++)
            {
                Text[Text.Length + i - count] = removed[i];
                Text[Text.Length + i - count].loc.Y = LineY(Text.Length + i - count);
            }
            Index -= count;
        }

        public void fade_in(bool swipeIn = false)
        {
            Active = true;
            Scroll.Reset();
            FadeTimer = Config.CONVO_BACKLOG_FADE_TIME;
            Fading_In = true;
            PanTimer = swipeIn ? Config.CONVO_BACKLOG_PAN_IN_TIME : 0;

            foreach (var text in Text)
            {
                if (swipeIn)
                    text.tint = Color.Transparent;
                text.draw_offset = Vector2.Zero;
            }
        }

        public void fade_out(bool swipeOut = false)
        {
            Active = false;
            Scroll.Reset();
            FadeTimer = Config.CONVO_BACKLOG_FADE_TIME;
            Fading_In = false;
            PanTimer = swipeOut ? Config.CONVO_BACKLOG_PAN_OUT_TIME : 0;

            if (swipeOut)
                foreach (var text in Text)
                    text.tint = Color.White;
        }

        public void update()
        {
            UpArrow.update();
            DownArrow.update();

            // Pan out
            if (PanTimer > 0 && !Fading_In)
            {
                PanTimer--;
                int pan = Config.CONVO_BACKLOG_PAN_OUT_TIME - PanTimer;
                // Set full alpha if the pan is over
                int alpha = 255;
                if (PanTimer > 0)
                    alpha = Math.Min(255, PanTimer * 256 / Config.CONVO_BACKLOG_PAN_OUT_TIME);
                Color tint = new Color(alpha, alpha, alpha, alpha);
                foreach (var text in Text)
                {
                    text.draw_offset = new Vector2(
                        -(int)Math.Pow(pan, 1.8f), 0);
                    text.tint = tint;
                }
            }
            else if (FadeTimer > 0)
            {
                FadeTimer--;
                Black_Fill.tint = new Color(0, 0, 0,
                    ((Fading_In ? (Config.CONVO_BACKLOG_FADE_TIME - FadeTimer) : FadeTimer) *
                        Config.CONVO_BACKLOG_BG_OPACITY) /
                    Config.CONVO_BACKLOG_FADE_TIME);
            }
            // Pan in
            else if (PanTimer > 0)
            {
                PanTimer--;
                int pan = Config.CONVO_BACKLOG_PAN_IN_TIME - PanTimer;
                int alpha = Math.Min(255, pan * 256 / Config.CONVO_BACKLOG_PAN_IN_TIME);
                Color tint = new Color(alpha, alpha, alpha, alpha);
                foreach (var text in Text)
                {
                    text.draw_offset = new Vector2(
                        -(int)Math.Pow(PanTimer, 1.8f), 0);
                    text.tint = tint;
                }
            }
            if (FadeTimer == 0 && PanTimer == 0)
                Fading_In = false;

            Scroll.Update(this.ready && Active);

            UpArrow.visible = !Scroll.AtTop;
            DownArrow.visible = !Scroll.AtBottom;
        }

        public void draw(SpriteBatch sprite_batch)
        {
            Vector2 offset = Scroll.offset -
                new Vector2(0, Config.CONVO_BACKLOG_LINES * Window_Message.FontData.CharHeight - Config.WINDOW_HEIGHT);
            Vector2 int_offset = new Vector2((int)offset.X, (int)offset.Y);

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Black_Fill.draw(sprite_batch);
            sprite_batch.End();

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (PanTimer > 0 || (Fading_In ? FadeTimer == 0 : Active))
            {
                for (int i = 0; i <= Index; i++)
                    Text[i].draw_multicolored(sprite_batch, int_offset);

                UpArrow.draw(sprite_batch);
                DownArrow.draw(sprite_batch);
            }
            sprite_batch.End();
        }
    }
}
