using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Graphics.Text;
using FEXNA.Windows.Command;
using FEXNA_Library;

namespace FEXNA.Menus.Map.Unit.Item
{
    class TradeMenu : BaseMenu, IHasCancelButton
    {
        internal bool Traded { get; private set; }
        protected Window_Trade TradeWindow;
        protected Button_Description CancelButton;

        public TradeMenu(Window_Trade window, IHasCancelButton menu = null)
        {
            TradeWindow = window;
            CreateCancelButton(menu);
        }

        private void CreateCancelButton(IHasCancelButton menu = null)
        {
            if (menu != null && menu.HasCancelButton)
            {
                CreateCancelButton(
                    (int)menu.CancelButtonLoc.X,
                    Config.MAPCOMMAND_WINDOW_DEPTH);
            }
            else
            {
                CreateCancelButton(
                    Config.WINDOW_WIDTH - (32 + 48),
                    Config.MAPCOMMAND_WINDOW_DEPTH);
            }
        }
        public void CreateCancelButton(int x, float depth = 0)
        {
            CancelButton = Button_Description.button(Inputs.B, x);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = depth;
        }

        protected bool CanceledTriggered(bool active)
        {
            bool cancel = TradeWindow.is_canceled();
            return cancel;
        }

        public event EventHandler<EventArgs> Trade;
        protected void OnTrade(EventArgs e)
        {
            if (Trade != null)
                Trade(this, e);
        }

        public event EventHandler<EventArgs> Closed;
        protected void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }
        
        public void StaffFix()
        {
            TradeWindow.staff_fix();
        }

        #region IMenu
        protected override void UpdateMenu(bool active)
        {
            TradeWindow.update(active);

            if (CancelButton != null)
                CancelButton.Update(active);
            bool cancel = CanceledTriggered(active);
            
            if (TradeWindow.is_help_active)
            {
                if (cancel)
                    TradeWindow.close_help();
            }
            else
            {
                if (TradeWindow.getting_help())
                    TradeWindow.open_help();
                else if (cancel)
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    // An item is selected, deselect it
                    if (TradeWindow.mode > 0)
                    {
                        TradeWindow.cancel();
                    }
                    // Nothing selected, close the menu
                    else
                    {
                        TradeWindow.staff_fix();
                        OnClosed(new EventArgs());
                    }
                    return;
                }
                else if (TradeWindow.is_selected())
                {
                    Traded |= TradeWindow.enter();
                    if (Traded)
                    {
                        OnTrade(new EventArgs());
                        if (CancelButton != null)
                            CancelButton.description = "Close";
                    }
                }
            }
        }

        protected override void UpdateAncillary()
        {
            if (CancelButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    CreateCancelButton(this);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TradeWindow.draw(spriteBatch);
            if (CancelButton != null)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                CancelButton.Draw(spriteBatch);
                spriteBatch.End();
            }
        }
        #endregion

        #region IHasCancelButton
        public bool HasCancelButton { get { return CancelButton != null; } }
        public Vector2 CancelButtonLoc { get { return CancelButton.loc; } }
        #endregion
    }
}
