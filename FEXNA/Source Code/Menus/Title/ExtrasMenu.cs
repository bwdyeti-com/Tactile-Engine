using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Title;

namespace FEXNA.Menus.Title
{
    enum ExtrasSelections { SoundRoom, SupportViewer, Credits }

    class ExtrasMenu : StandardMenu
    {
        readonly static Vector2 MENU_LOC = new Vector2(56, 48);

        private ExtrasSelections Selection;
        private List<MainMenuChoicePanel> MenuChoices = new List<MainMenuChoicePanel>();
        private UINodeSet<MainMenuChoicePanel> ChoiceNodes;

        public ExtrasMenu() : base()
        {
            MenuChoices.Add(new MainMenuChoicePanel("Sound Room"));
            MenuChoices.Add(new MainMenuChoicePanel("Supports"));
            MenuChoices.Add(new MainMenuChoicePanel("Credits"));

            MenuChoices[(int)ExtrasSelections.SoundRoom].Visible = Global.progress.SoundRoomAccessible;
            MenuChoices[(int)ExtrasSelections.SupportViewer].Visible = Global.progress.SupportViewerAccessible;

            RefreshLocs();
            
            IEnumerable<MainMenuChoicePanel> nodes = new List<MainMenuChoicePanel>(MenuChoices);
            nodes = nodes.Where(x => x.Visible);

            ChoiceNodes = new UINodeSet<MainMenuChoicePanel>(nodes);
            ChoiceNodes.CursorMoveSound = System_Sounds.Menu_Move1;
            ChoiceNodes.SoundOnMouseMove = true;

            ChoiceNodes.set_active_node(nodes.FirstOrDefault());
            Selection = (ExtrasSelections)MenuChoices.IndexOf(ChoiceNodes.ActiveNode);
            RefreshLocs();
        }

        #region StandardMenu Abstract
        public override int Index { get { return (int)Selection; } }

        protected override bool SelectedTriggered(bool active)
        {
            if (!active)
                return false;

            var selected = ChoiceNodes.consume_triggered(Inputs.A, MouseButtons.Left, TouchGestures.Tap);
            return selected.IsSomething;
        }

        protected override void UpdateStandardMenu(bool active)
        {
            int index = ChoiceNodes.ActiveNodeIndex;
            ChoiceNodes.Update(active);
            if (index != ChoiceNodes.ActiveNodeIndex)
            {
                Selection = (ExtrasSelections)MenuChoices.IndexOf(ChoiceNodes.ActiveNode);
                RefreshLocs();
                OnIndexChanged(new EventArgs());
            }
        }
        #endregion

        private void RefreshLocs()
        {
            Vector2 loc = MENU_LOC + new Vector2(4, -12);
            for (int i = 0; i < MenuChoices.Count; i++)
            {
                MenuChoices[i].ResetOffset();
                MenuChoices[i].RefreshWidth(i == (int)Selection);

                if (i != (int)Selection)
                    MenuChoices[i].offset += new Vector2(-12, 0);

                if (MenuChoices[i].Visible)
                {
                    MenuChoices[i].loc = loc;
                    loc += new Vector2(0, MenuChoices[i].Size.Y);
                }
                MenuChoices[i].RefreshBg();
            }
        }

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = active && Global.Input.triggered(Inputs.B);
            return cancel || base.CanceledTriggered(active);
        }

        #region IFadeMenu
        public override ScreenFadeMenu FadeInMenu(bool skipFadeIn = false) { return null; }
        public override ScreenFadeMenu FadeOutMenu() { return null; }
        #endregion

        #region IMenu
        public override bool HidesParent { get { return false; } }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DataDisplayed)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                ChoiceNodes.Draw(spriteBatch);
                spriteBatch.End();
            }

            base.Draw(spriteBatch);
        }
        #endregion
    }
}
