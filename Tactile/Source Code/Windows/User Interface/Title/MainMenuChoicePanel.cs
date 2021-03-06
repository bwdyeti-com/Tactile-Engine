﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;

namespace Tactile.Windows.UserInterface.Title
{
    class MainMenuChoicePanel : Title_Info_Panel
    {
        internal const int PANEL_WIDTH = 208;

        public bool Visible = true, BgVisible = true;
        private TextSprite ChoiceText;

        public MainMenuChoicePanel(string text)
        {
            ResetOffset();

            ChoiceText = new TextSprite();
            ChoiceText.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            ChoiceText.stereoscopic = Config.TITLE_CHOICE_DEPTH;
            ChoiceText.text = text;
            
            WindowPanel window;
            window = new System_Color_Window();
            window.height = 12;
            window.offset = new Vector2(16, 12);

            window.loc = new Vector2(-4, 16);
            window.width = PANEL_WIDTH;
            window.stereoscopic = Config.TITLE_MENU_DEPTH;
            Window = window;

            Size = new Vector2(PANEL_WIDTH - 8, 16);
        }

        public void RefreshBg()
        {
            if (Window is System_Color_Window)
            {
                (Window as System_Color_Window).color_override =
                    Global.current_save_info == null ? 0 :
                        Constants.Difficulty.DIFFICULTY_COLOR_REDIRECT[
                            Global.current_save_info.difficulty];
            }
        }

        public void RefreshWidth(bool active)
        {
            int width = active ? PANEL_WIDTH : PANEL_WIDTH - 32;// 80 + 24;

            Window.width = width;
        }

        public void ResetOffset()
        {
            this.offset = new Vector2(-16, 0);
        }

        internal override void Activate()
        {
            ChoiceText.SetColor(Global.Content, "Green");
        }
        internal override void Deactivate()
        {
            ChoiceText.SetColor(Global.Content, "Yellow");
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (Visible)
            {
                Vector2 loc = (draw_offset + this.offset) -
                    (this.loc + this.draw_offset + stereo_offset());

                if (BgVisible)
                    Window.draw(sprite_batch, loc);
                ChoiceText.draw(sprite_batch, loc);
            }
        }
    }
}
