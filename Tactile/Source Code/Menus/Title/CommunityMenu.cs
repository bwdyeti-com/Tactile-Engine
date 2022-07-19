using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows.UserInterface;
using Tactile.Windows.UserInterface.Title;

namespace Tactile.Menus.Title
{
    class CommunityMenu : StandardMenu
    {
        const int FADE_TIME = 8;

        public readonly static List<CommunityEntry> COMMUNITY_ENTRIES = new List<CommunityEntry>
        {
            //new CommunityEntry("@[YourGame'sTwitter]", "twitter.com/[YourGame'sTwitter]", "TwitterBranding"),
            //new CommunityEntry("r/[YourGame'sSubreddit]", "www.reddit.com/r/[YourGame'sSubreddit]/", "RedditBranding"),
        };

        readonly static Vector2 MENU_LOC = new Vector2(96, 32);

        private UINodeSet<CommunityUINode> Nodes;
        private UICursor<CommunityUINode> Cursor;
        private Sprite DarkenedBackground;

        public CommunityMenu(IHasCancelButton menu = null) : base(menu)
        {
            List<CommunityUINode> nodes = new List<CommunityUINode>();
            for (int i = 0; i < COMMUNITY_ENTRIES.Count; i++)
            {
                var entry = COMMUNITY_ENTRIES[i];
                var newNode = new CommunityUINode(entry);
                newNode.loc = MENU_LOC + new Vector2(0, i * newNode.Size.Y);
                nodes.Add(newNode);
            }

            Nodes = new UINodeSet<CommunityUINode>(nodes);
            Nodes.CursorMoveSound = System_Sounds.Menu_Move1;

            Nodes.set_active_node(nodes.FirstOrDefault());

            Cursor = new UICursor<CommunityUINode>(Nodes);
            Cursor.draw_offset = new Vector2(-12, 8);

            DarkenedBackground = new Sprite();
            DarkenedBackground.texture =
                Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            DarkenedBackground.dest_rect =
                new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            DarkenedBackground.tint = new Color(0, 0, 0, 128);
        }

        #region StandardMenu Abstract
        public override int Index { get { return Nodes.ActiveNodeIndex; } }

        protected override bool SelectedTriggered(bool active)
        {
            if (!active)
                return false;

            var selected = Nodes.consume_triggered(Inputs.A, MouseButtons.Left, TouchGestures.Tap);
            return selected.IsSomething;
        }

        protected override void UpdateStandardMenu(bool active)
        {
            int index = Nodes.ActiveNodeIndex;
            Nodes.Update(active);
            Cursor.update();
            if (index != Nodes.ActiveNodeIndex)
            {
                OnIndexChanged(new EventArgs());
            }
        }
        #endregion

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = active && Global.Input.triggered(Inputs.B);
            return cancel || base.CanceledTriggered(active);
        }

        #region IFadeMenu
        public override ScreenFadeMenu FadeInMenu(bool skipFadeIn = false)
        {
            if (skipFadeIn)
                return null;

            var fade = new ScreenFadeMenu(
                FADE_TIME,
                0,
                0,
                true,
                this);
            fade.SetHoldColor(new Color(0, 0, 0, 128));
            return fade;
        }
        public override ScreenFadeMenu FadeOutMenu()
        {
            var fade = new ScreenFadeMenu(
                0,
                0,
                FADE_TIME,
                false,
                this);
            fade.SetHoldColor(new Color(0, 0, 0, 128));
            return fade;
        }
        #endregion

        #region IMenu
        public override bool HidesParent { get { return false; } }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DataDisplayed)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                DarkenedBackground.draw(spriteBatch);
                Nodes.Draw(spriteBatch);
                Cursor.draw(spriteBatch);
                spriteBatch.End();
            }

            base.Draw(spriteBatch);
        }
        #endregion
    }

    struct CommunityEntry
    {
        public string Name { get; private set; }
        public string Url { get; private set; }
        public string Texture { get; private set; }

        public CommunityEntry(string name, string url, string texture)
        {
            Name = name;
            Url = url;
            Texture = texture;
        }
    }
}
