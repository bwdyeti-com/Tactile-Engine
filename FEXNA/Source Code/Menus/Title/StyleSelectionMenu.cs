using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FEXNA.Graphics.Help;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Title;

namespace FEXNA.Menus.Title
{
    class StyleSelectionMenu : BaseMenu
    {
        public Vector2 MenuLoc { get; private set; }
        private bool MenusHidden = false;
        private Mode_Style_Info_Panel[] StylePanels;
        private UINodeSet<Mode_Style_Info_Panel> StyleNodes;

        private Button_Description CancelButton;

        public override bool HidesParent { get { return false; } }

        public StyleSelectionMenu()
        {
            StylePanels = new Mode_Style_Info_Panel[Enum_Values.GetEnumCount(typeof(Mode_Styles))];
            int offset = 0;
            for (int i = 0; i < StylePanels.Length; i++)
            {
                StylePanels[i] = new Mode_Style_Info_Panel((Mode_Styles)i);
                StylePanels[i].stereoscopic = Config.TITLE_MENU_DEPTH;
                StylePanels[i].active = false;
                StylePanels[i].loc = new Vector2(0, offset);
                offset += StylePanels[i].height + 8;
            }

            MenuLoc = new Vector2(
                (Config.WINDOW_WIDTH - Mode_Style_Info_Panel.WIDTH) / 2,
                (Config.WINDOW_HEIGHT - 16) / 2);
            MenuLoc -= new Vector2(0, (offset / 2) / 8 * 8);

            StyleNodes = new UINodeSet<Mode_Style_Info_Panel>(StylePanels);
            StyleNodes.set_active_node(StyleNodes[(int)Mode_Styles.Standard]);
            StyleNodes.ActiveNode.active = true;

            CancelButton = Button_Description.button(Inputs.B,
                Config.WINDOW_WIDTH - 64);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = Config.TITLE_MENU_DEPTH;
        }

        public Mode_Styles SelectedStyle
        {
            get { return (Mode_Styles)StyleNodes.ActiveNodeIndex; }
        }

        #region Events
        public event EventHandler<EventArgs> Selected;
        protected void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        public event EventHandler<EventArgs> Canceled;
        protected void OnCanceled(EventArgs e)
        {
            if (Canceled != null)
                Canceled(this, e);
        }
        #endregion

        protected override void Activate()
        {
            MenusHidden = false;
        }

        public void HideMenus()
        {
            MenusHidden = true;
        }

        protected override void UpdateMenu(bool active)
        {
            int index = StyleNodes.ActiveNodeIndex;
            StyleNodes.Update(active, -MenuLoc);
            if (index != StyleNodes.ActiveNodeIndex)
            {
                StylePanels[index].active = false;
                StyleNodes.ActiveNode.active = true;
            }

            CancelButton.Update(active);

            if (active)
            {
                var styleIndex = StyleNodes.consume_triggered(
                    Inputs.A, MouseButtons.Left, TouchGestures.Tap);
                if (styleIndex.IsSomething)
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    StyleNodes.set_active_node(StyleNodes[styleIndex]);
                    OnSelected(new EventArgs());
                }
                else if (Global.Input.triggered(Inputs.B) ||
                    Global.Input.KeyPressed(Keys.Escape) ||
                    CancelButton.consume_trigger(MouseButtons.Left) ||
                    CancelButton.consume_trigger(TouchGestures.Tap))
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    OnCanceled(new EventArgs());
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!MenusHidden)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                StyleNodes.Draw(spriteBatch, -MenuLoc);
                CancelButton.Draw(spriteBatch);
                spriteBatch.End();
            }
        }
    }
}
