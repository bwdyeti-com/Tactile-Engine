using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FEXNA.Graphics.Help;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Title;

namespace FEXNA.Menus.Title
{
    class DifficultySelectionMenu : BaseMenu
    {
        public Vector2 MenuLoc { get; private set; }
        private bool MenusHidden = false;
        private Difficulty_Info_Panel[] DifficultyPanels;
        private UINodeSet<Difficulty_Info_Panel> DifficultyNodes;

        private Button_Description CancelButton;

        public override bool HidesParent { get { return false; } }

        public DifficultySelectionMenu()
        {
            DifficultyPanels = new Difficulty_Info_Panel[Enum_Values.GetEnumCount(typeof(Difficulty_Modes))];
            int offset = 0;
            for (int i = 0; i < DifficultyPanels.Length; i++)
            {
                DifficultyPanels[i] = new Difficulty_Info_Panel((Difficulty_Modes)i);
                DifficultyPanels[i].stereoscopic = Config.TITLE_MENU_DEPTH;
                DifficultyPanels[i].active = false;
                DifficultyPanels[i].loc = new Vector2(0, offset);
                offset += DifficultyPanels[i].height + 8;
            }

            MenuLoc = new Vector2(
                (Config.WINDOW_WIDTH - Difficulty_Info_Panel.WIDTH) / 2,
                (Config.WINDOW_HEIGHT - 16) / 2);
            MenuLoc -= new Vector2(0, (offset / 2) / 8 * 8);

            DifficultyNodes = new UINodeSet<Difficulty_Info_Panel>(DifficultyPanels);
            DifficultyNodes.set_active_node(DifficultyNodes[(int)Difficulty_Modes.Normal]);
            DifficultyNodes.ActiveNode.active = true;

            CancelButton = Button_Description.button(Inputs.B,
                Config.WINDOW_WIDTH - 64);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = Config.TITLE_MENU_DEPTH;
        }

        public Difficulty_Modes SelectedDifficulty
        {
            get { return (Difficulty_Modes)DifficultyNodes.ActiveNodeIndex; }
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
            int index = DifficultyNodes.ActiveNodeIndex;
            DifficultyNodes.Update(active, -MenuLoc);
            if (index != DifficultyNodes.ActiveNodeIndex)
            {
                DifficultyPanels[index].active = false;
                DifficultyNodes.ActiveNode.active = true;
            }

            CancelButton.Update(active);

            if (active)
            {
                var styleIndex = DifficultyNodes.consume_triggered(
                    Inputs.A, MouseButtons.Left, TouchGestures.Tap);
                if (styleIndex.IsSomething)
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    DifficultyNodes.set_active_node(DifficultyNodes[styleIndex]);
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
                DifficultyNodes.Draw(spriteBatch, -MenuLoc);
                CancelButton.Draw(spriteBatch);
                spriteBatch.End();
            }
        }
    }
}
