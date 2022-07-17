﻿using System;
using Tactile.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile.Menus
{
    class ScreenFadeMenu : BaseMenu
    {
        private bool FadeIn;
        private IFadeMenu ParentMenu;
        private ScreenFade Fade;

        public ScreenFadeMenu(
            int fadeTime,
            int fadeHoldTime,
            bool fadeIn = false,
            IFadeMenu parent = null)
        {
            Fade = new ScreenFade(fadeTime, fadeHoldTime);

            FadeIn = fadeIn;
            ParentMenu = parent;
        }
        public ScreenFadeMenu(
            int fadeInTime,
            int fadeHoldTime,
            int fadeOutTime,
            bool fadeIn = false,
            IFadeMenu parent = null)
        {
            FadeIn = fadeIn;
            Fade = new ScreenFade(fadeInTime, fadeHoldTime, fadeOutTime);

            FadeIn = fadeIn;
            ParentMenu = parent;
        }

        public override bool HidesParent { get { return false; } }

        public event EventHandler<EventArgs> Finished;
        protected void OnFinished(EventArgs e)
        {
            if (Finished != null)
                Finished(this, e);
        }

        public void SetHoldColor(Color color)
        {
            Fade.SetHoldColor(color);
        }

        protected override void UpdateMenu(bool active)
        {
            if (active)
            {
                bool alreadyAtHoldEnd = Fade.AtHoldEnd;
                bool alreadyFinished = Fade.Finished;
                Fade.update();

                if ((Fade.AtHoldEnd && !alreadyAtHoldEnd) ||
                    // Hold End is supposed to happen on frame 0
                    Fade.FrameOneHoldEnd)
                {
                    if (ParentMenu != null)
                    {
                        if (FadeIn)
                            ParentMenu.FadeShow();
                        else
                            ParentMenu.FadeHide();
                    }
                }

                if (Fade.Finished && !alreadyFinished)
                {
                    OnFinished(new EventArgs());
                    if (ParentMenu != null)
                    {
                        if (FadeIn)
                            ParentMenu.FadeOpen();
                        else
                            ParentMenu.FadeClose();
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Fade.draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
