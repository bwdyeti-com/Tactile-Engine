﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Graphics.Text;

namespace FEXNA.Menus.Title
{
    class CreditsMenu : StandardMenu
    {
        readonly static Vector2 BASE_OFFSET = new Vector2(32, 16);
        const int MAX_SCROLL = 6;

        private int DataHeight;
        private Vector2 ScrollOffset = Vector2.Zero;
        private float ScrollSpeed = 0f;
        private List<FE_Text> CreditsText;
        private Scroll_Bar Scrollbar;
        private Button_Description FullCreditsButton;
        private Menu_Background Background;

        public CreditsMenu()
        {
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Pictures\Preparation_Background");
            (Background as Menu_Background).vel = new Vector2(0, -1 / 4f);
            (Background as Menu_Background).tile = new Vector2(1, 2);
            Background.tint = new Color(160, 160, 160, 255);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;

            // Credits
            CreditsText = new List<FE_Text>();
            Vector2 loc = Vector2.Zero;
            foreach (var credit in Constants.Credits.CREDITS)
            {
                if (!string.IsNullOrEmpty(credit.Role) || credit.Names.Any())
                {
                    // Role
                    var role = new FE_Text(
                        "FE7_Text",
                        Global.Content.Load<Texture2D>(
                            @"Graphics\Fonts\FE7_Text_Yellow"),
                        loc,
                        credit.Role);

                    CreditsText.Add(role);
                    loc += new Vector2(0, role.CharHeight * Lines(credit.Role));

                    // Names
                    foreach (string name in credit.Names)
                    {
                        var nameText = new FE_Text(
                            "FE7_Text",
                            Global.Content.Load<Texture2D>(
                                @"Graphics\Fonts\FE7_Text_White"),
                            loc + new Vector2(16, 0),
                            name);

                        CreditsText.Add(nameText);
                        loc += new Vector2(0, nameText.CharHeight * Lines(name));
                    }
                }

                loc += new Vector2(0, 16);
            }

            DataHeight = (int)loc.Y - 16;

            // Scrollbar
            if (this.MaxScroll > 0)
            {
                int height = Config.WINDOW_HEIGHT - (int)(BASE_OFFSET.Y * 2);
                Scrollbar = new Scroll_Bar(height - 16, DataHeight, height, 0);
                Scrollbar.loc = new Vector2(Config.WINDOW_WIDTH - BASE_OFFSET.X, BASE_OFFSET.Y + 8);
            }

            // Full Credits Button
            if (!string.IsNullOrEmpty(Constants.Credits.FULL_CREDITS_LINK))
            {
                CreateFullCreditsButton();
            }
        }

        #region StandardMenu Abstract
        public override int Index
        {
            get { return -1; }
        }

        protected override bool SelectedTriggered(bool active)
        {
            return false;
        }

        protected override void UpdateStandardMenu(bool active)
        {
            Background.update();
            if (Scrollbar != null)
            {
                Scrollbar.update();
                if (active)
                    Scrollbar.update_input();
            }

                bool holdDown = false, holdUp = false;
            if (Global.Input.pressed(Inputs.Down))
                holdDown = true;
            else if (Global.Input.pressed(Inputs.Up))
                holdUp = true;
            else if (Scrollbar != null)
            {
                if (Scrollbar.DownHeld)
                    holdDown = true;
                else if (Scrollbar.UpHeld)
                    holdUp = true;
            }
            
            if (holdDown)
            {
                ScrollSpeed = Math.Max(1, ScrollSpeed);
                ScrollSpeed = Math.Min(ScrollSpeed + 0.25f, +MAX_SCROLL);
            }
            else if (holdUp)
            {
                ScrollSpeed = Math.Min(-1, ScrollSpeed);
                ScrollSpeed = Math.Max(ScrollSpeed - 0.25f, -MAX_SCROLL);
            }
            else
                ScrollSpeed = 0f;
            
            ScrollOffset.Y = (int)MathHelper.Clamp(
                ScrollOffset.Y + ScrollSpeed,
                 0, this.MaxScroll);

            if (Scrollbar != null)
                Scrollbar.scroll = (int)ScrollOffset.Y;

            // Full credits link
            if (FullCreditsButton != null)
            {
                FullCreditsButton.Update(active);
                
                bool fullCredits = false;
                fullCredits |= FullCreditsButton.consume_trigger(MouseButtons.Left) ||
                    FullCreditsButton.consume_trigger(TouchGestures.Tap);
                if (active)
                    fullCredits |= Global.Input.triggered(Inputs.X);

                if (fullCredits)
                    OnOpenFullCredits(new EventArgs());
            }
        }

        protected override void UpdateAncillary()
        {
            base.UpdateAncillary();

            if (FullCreditsButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    CreateFullCreditsButton();
            }
        }

        #endregion

        private static int Lines(string str)
        {
            return str.Split(new char[] { '\n' }, StringSplitOptions.None).Length;
        }

        private float MaxScroll
        {
            get
            {
                int height = Config.WINDOW_HEIGHT - (int)(BASE_OFFSET.Y * 2);
                return Math.Max(0, DataHeight - height);
            }
        }

        protected virtual void CreateFullCreditsButton()
        {
            FullCreditsButton = Button_Description.button(Inputs.X, this.DefaultCancelPosition - 80);
            FullCreditsButton.description = "Full Credits";
            FullCreditsButton.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
        }

        public event EventHandler<EventArgs> OpenFullCredits;
        protected void OnOpenFullCredits(EventArgs e)
        {
            if (OpenFullCredits != null)
                OpenFullCredits(this, e);
        }

        protected override int DefaultCancelPosition { get { return Config.WINDOW_WIDTH - 104; } }

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = base.CanceledTriggered(active);
            if (active)
            {
                cancel |= Global.Input.triggered(Inputs.B);
                cancel |= Global.Input.mouse_click(MouseButtons.Right);
            }
            return cancel;
        }
        
        #region IMenu
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DataDisplayed)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Background.draw(spriteBatch);
                if (Scrollbar != null)
                    Scrollbar.draw(spriteBatch);
                spriteBatch.End();


                RasterizerState scissorState = new RasterizerState { ScissorTestEnable = true };
                Rectangle textClip = new Rectangle(
                    (int)BASE_OFFSET.X, (int)BASE_OFFSET.Y,
                    Config.WINDOW_WIDTH - (int)(BASE_OFFSET.X * 2),
                    Config.WINDOW_HEIGHT - (int)(BASE_OFFSET.Y * 2));
                spriteBatch.GraphicsDevice.ScissorRectangle =
                    Scene_Map.fix_rect_to_screen(textClip);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, scissorState);
                foreach (var text in CreditsText)
                    text.draw(spriteBatch, ScrollOffset - BASE_OFFSET);
                spriteBatch.End();

                if (FullCreditsButton != null)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    FullCreditsButton.Draw(spriteBatch);
                    spriteBatch.End();
                }
            }

            base.Draw(spriteBatch);
        }
        #endregion
    }
}
