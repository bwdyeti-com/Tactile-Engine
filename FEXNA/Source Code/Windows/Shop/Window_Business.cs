using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Map;
using FEXNA.Menus;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Command;

namespace FEXNA
{
    abstract class Window_Business : BaseMenu
    {
        protected const int BLACK_SCREEN_FADE_TIMER = 16;
        protected const int BLACK_SCREEN_HOLD_TIMER = 8;
        protected const int ROWS = 6;

        protected Shop_Data Shop;
        protected bool On_Buy = true;
        protected bool On_Yes = true;
        protected int Black_Screen_Timer = BLACK_SCREEN_HOLD_TIMER + (BLACK_SCREEN_FADE_TIMER * 2);
        protected int Wait = 0;
        protected int Delay = 0;
        protected bool Closing = false;
        protected bool Trading = false, Accepting = false, Traded = false;
        // @Debug
        protected bool _Visible = false;
        protected bool Message_Active;
        protected int Message_Id;

        protected Sprite Background;
        protected Sprite Black_Screen;
        protected Sprite Darkened_Bar;
        protected Sprite Gold_Window;
        protected FE_Text_Int Gold_Data;
        protected FE_Text Gold_G;
        protected Message_Box Message;
        protected Sprite Face;
        protected Sprite Portrait_Bg;
        protected FE_Text Portrait_Label;

        protected UICursor<TextUINode> Cursor;
        protected UINodeSet<TextUINode> Choices;

        private RasterizerState Item_State = new RasterizerState { ScissorTestEnable = true };
        protected Rectangle Item_Rect;

        #region Accessors
        public bool traded { get { return Traded; } }

        public bool closed { get { return Black_Screen_Timer <= 0 && Closing; } }

        protected virtual bool trading
        {
            get { return Trading; }
            set { Trading = value; }
        }

        public override bool HidesParent { get { return false; } }
        #endregion

        protected abstract int choice_offset();

        #region Image Setup
        protected abstract void initialize_images();

        protected void set_images()
        {
            // Face
            if (Global.game_system.preparations && string.IsNullOrEmpty(Shop.face))
            {
                Face = new Miniface();
                (Face as Miniface).set_actor(Global.battalion.convoy_face_name);
                Face.offset += new Vector2(0, 32);
                Face.loc = new Vector2(40, 40);
            }
            else
            {
                Face = new Face_Sprite(Shop.face, true);
                Message.speaker = (Face as Face_Sprite);
                (Face as Face_Sprite).loc = new Vector2(40, 56 - 48); //Debug
                (Face as Face_Sprite).offset.Y = 0; //Debug
            }
        }

        protected void redraw_gold()
        {
            Gold_Data.text = Global.battalion.gold.ToString();
        }

        protected abstract bool is_wait_text(int message_id);

        protected void set_text(int message_id)
        {
            Message_Id = message_id;
            string message = Shop.text(message_id);
            int cost = item_cost();
            message = Regex.Replace(message, @"<cost>", cost.ToString());
            string convoy_name, convoy_capital_name;
            if (Global.battalion.convoy_id == -1)
            {
                convoy_name = "your merchant";
                convoy_capital_name = "Your merchant";
            }
            else
            {
                convoy_name = convoy_capital_name = Global.game_actors[Global.battalion.convoy_id].name;
            }
            message = Regex.Replace(message, @"<Merchant>", convoy_capital_name);
            message = Regex.Replace(message, @"<merchant>", convoy_name);
            message = Regex.Replace(message, @"\\n", "\n");
            Message.set_text(string.Format("{0}{1}", message, is_wait_text(message_id) ? "|" : ""));
        }

        protected void set_choices(int ox, string str1, string str2)
        {
            List<TextUINode> choices = new List<TextUINode>();

            var text1 = new FE_Text();
            text1.Font = "FE7_Convo";
            text1.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Convo_White");
            text1.text = str1;

            var node = new TextUINode("", text1, text1.text_width);
            node.loc = new Vector2(88 + ox, 32);
            choices.Add(node);

            var text2 = new FE_Text();
            text2.Font = "FE7_Convo";
            text2.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Convo_White");
            text2.text = str2;

            node = new TextUINode("", text2, text2.text_width);
            node.loc = new Vector2(88 + ox + 40, 32);
            choices.Add(node);

            Choices = new UINodeSet<TextUINode>(choices);
            Cursor = new UICursor<TextUINode>(Choices);
            Cursor.draw_offset = new Vector2(-16, 0);
            Cursor.ratio = new int[] { 1, 1 };
            Cursor.hide_when_using_mouse(false);
            Cursor.move_to_target_loc();

            /* //Debug
            Choices[0] = new FE_Text();
            Choices[0].loc = new Vector2(88 + ox, 32);
            Choices[0].Font = "FE7_Convo";
            Choices[0].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Convo_White");
            Choices[0].text = str1;
            Choices[1] = new FE_Text();
            Choices[1].loc = new Vector2(88 + ox + 40, 32);
            Choices[1].Font = "FE7_Convo";
            Choices[1].texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Convo_White");
            Choices[1].text = str2;*/
        }

        protected void clear_choices()
        {
            Choices = null;
            Cursor = null;
        }
        #endregion

        #region Processing
        protected abstract int item_cost();
        #endregion

        protected abstract void play_shop_theme();

        public event EventHandler<EventArgs> Closed;
        protected void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }

        #region Update
        public event EventHandler Shop_Close;
        protected void close()
        {
            Closing = true;
            Shop_Close(this, new EventArgs());
        }

        protected void update_cursor_location()
        {
            update_cursor_location(false);
        }
        protected void update_cursor_location(bool instant)
        {
            if (Cursor != null)
            {
                if (instant)
                {
                    Cursor.update();
                    Cursor.move_to_target_loc();
                }
                else
                {
                    Cursor.update();
                }
            }
        }

        protected virtual void update_black_screen()
        {
            if (Wait > 0)
            {
                Wait--;
                return;
            }
            if (Black_Screen_Timer > 0)
            {
                Black_Screen_Timer--;
                if (Black_Screen_Timer > BLACK_SCREEN_FADE_TIMER + (BLACK_SCREEN_HOLD_TIMER / 2))
                    Black_Screen.TintA = (byte)Math.Min(255,
                        (BLACK_SCREEN_HOLD_TIMER + (BLACK_SCREEN_FADE_TIMER * 2) - Black_Screen_Timer) * (256 / BLACK_SCREEN_FADE_TIMER));
                else
                    Black_Screen.TintA = (byte)Math.Min(255,
                        Black_Screen_Timer * (256 / BLACK_SCREEN_FADE_TIMER));

                if (Black_Screen_Timer == BLACK_SCREEN_FADE_TIMER + (BLACK_SCREEN_HOLD_TIMER / 2))
                {
                    _Visible = !_Visible;
                    if (Closing)
                    {
                        // message.set_speaker(null) //Yeti
                        // message.dispose() //Yeti
                    }
                    else
                        play_shop_theme();
                }
                else if (Black_Screen_Timer == 0)
                {
                    if (Closing)
                        OnClosed(new EventArgs());
                }
            }
        }

        protected abstract void update_message();

        protected abstract void update_main_selection();
        #endregion

        #region Draw
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_Visible)
            {
                draw_background(spriteBatch);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Darkened_Bar.draw(spriteBatch);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Portrait_Bg.draw(spriteBatch);
                if (Portrait_Label != null)
                    Portrait_Label.draw(spriteBatch);
                spriteBatch.End();

                Face.draw(spriteBatch);

                draw_window(spriteBatch);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Gold_Window.draw(spriteBatch);
                Gold_Data.draw(spriteBatch);
                Gold_G.draw(spriteBatch);
                spriteBatch.End();
                spriteBatch.GraphicsDevice.ScissorRectangle = Scene_Map.fix_rect_to_screen(Item_Rect);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Item_State);
                //foreach (Shop_Item item in Item_Data) //Debug
                { }//item.draw(sprite_batch, new Vector2(0, Scroll_Real)); //Debug
                spriteBatch.End();

                Message.draw_background(spriteBatch);
                Message.draw_faces(spriteBatch);
                Message.draw_foreground(spriteBatch);

                if (Choices != null)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    Choices.Draw(spriteBatch);
                    Cursor.draw(spriteBatch);
                    spriteBatch.End();
                }
            }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Black_Screen.draw(spriteBatch);
            spriteBatch.End();
        }

        protected abstract void draw_background(SpriteBatch sprite_batch);

        protected abstract void draw_window(SpriteBatch sprite_batch);
        #endregion
    }
}
