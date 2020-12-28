using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows;

namespace Tactile.Menus.Save
{
    class SaveComparisonMenu : BaseMenu, IFadeMenu
    {
        const int BLACK_SCEEN_FADE_IN_TIMER = 30;
        const int BLACK_SCEEN_FADE_OUT_TIMER = 20;
        const int BLACK_SCREEN_HOLD_TIMER = 4;

        private bool DataDisplayed = false;
        private WindowRankingComparison OldRankings, NewRankings;
        private Sprite Gradient;
        private Sprite Background;

        public SaveComparisonMenu()
        {
            var savedRankings = Global.save_file.all_rankings(
                Global.game_system.chapter_id);
            var oldRanking = savedRankings[Global.game_system.chapter_id];

            NewRankings = new WindowRankingComparison(new Game_Ranking(), oldRanking);
            NewRankings.loc = new Vector2(32,
                Config.WINDOW_HEIGHT - (16 * 5 + 32));

            OldRankings = new WindowRankingComparison(oldRanking);
            OldRankings.loc = new Vector2(
                Config.WINDOW_WIDTH - (OldRankings.Width + 32),
                Config.WINDOW_HEIGHT - (16 * 5 + 32));

            Gradient = new Sprite();
            Gradient.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Pictures/TransparentGradient");
            Gradient.dest_rect = new Rectangle(
                0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT / 2);
            Gradient.tint = new Color(0, 0, 0, 32); //224); //@Debug

            Background = new Menu_Background();
            /* //@Debug
            Background.tint = new Color(96, 96, 96, 255);
            Background.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Pictures/Status_Background");
            (Background as Menu_Background).vel = new Vector2(0, 0);
            (Background as Menu_Background).tile = new Vector2(3, 2);*/

            Background.tint = new Color(136, 136, 136, 255);
            Background.texture = Global.Content.Load<Texture2D>(
                @"Graphics/Pictures/Preparation_Background");
            (Background as Menu_Background).vel = new Vector2(0, -1 / 3f);
            (Background as Menu_Background).tile = new Vector2(1, 2);

            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
        }

        protected override void UpdateMenu(bool active)
        {
            OldRankings.Update();
            NewRankings.Update();
            Background.update();
        }

        #region Events
        public event EventHandler<EventArgs> Opened;
        protected void OnOpened(EventArgs e)
        {
            if (Opened != null)
                Opened(this, e);
        }

        public event EventHandler<EventArgs> Closed;
        protected void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }
        #endregion

        #region IFadeMenu
        public void FadeShow()
        {
            DataDisplayed = true;
        }
        public void FadeHide()
        {
            DataDisplayed = false;
        }

        public ScreenFadeMenu FadeInMenu(bool skipFadeIn)
        {
            return new ScreenFadeMenu(
                skipFadeIn ? 0 : BLACK_SCEEN_FADE_IN_TIMER,
                BLACK_SCREEN_HOLD_TIMER,
                BLACK_SCEEN_FADE_IN_TIMER,
                true,
                this);

        }
        public ScreenFadeMenu FadeOutMenu()
        {
            //@Debug: fade in over Config.TITLE_GAME_START_TIME instead
            return new ScreenFadeMenu(
                BLACK_SCEEN_FADE_OUT_TIMER,
                BLACK_SCREEN_HOLD_TIMER,
                false,
                this);
        }

        public void FadeOpen()
        {
            OnOpened(new EventArgs());
        }
        public void FadeClose()
        {
            OnClosed(new EventArgs());
        }
        #endregion

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DataDisplayed)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Background.draw(spriteBatch);
                Gradient.draw(spriteBatch);
                spriteBatch.End();

                OldRankings.Draw(spriteBatch);
                NewRankings.Draw(spriteBatch);
            }
        }
    }
}
