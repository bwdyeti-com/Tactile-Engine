using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Windows.Target;

namespace Tactile.Menus.Map.Unit.Target
{
    abstract class TargetMenu<T> : BaseMenu, ITargetMenu, IHasCancelButton
    {
        protected Window_Target<T> Window;
        protected Button_Description CancelButton;
        
        protected TargetMenu(Window_Target<T> window, IHasCancelButton menu = null)
        {
            Window = window;
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
                    Global.player.is_on_left() ? Config.WINDOW_WIDTH - (32 + 48) : 32,
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
            bool cancel = Window.is_canceled();
            if (active)
            {
                if (CancelButton != null)
                {
                    cancel |= CancelButton.consume_trigger(MouseButtons.Left) ||
                        CancelButton.consume_trigger(TouchGestures.Tap);
                }
                // If right clicked
                cancel |= Global.Input.mouse_click(MouseButtons.Right);
            }
            return cancel;
        }

        public event EventHandler<EventArgs> Selected;
        protected void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        public event EventHandler<EventArgs> IndexChanged;
        protected void OnIndexChanged(EventArgs e)
        {
            if (IndexChanged != null)
                IndexChanged(this, e);
        }

        public event EventHandler<EventArgs> Canceled;
        protected void OnCanceled(EventArgs e)
        {
            if (Canceled != null)
                Canceled(this, e);
        }

        public void Accept()
        {
            Window.accept();
        }

        #region IMenu
        protected override void UpdateMenu(bool active)
        {
            int index = Window.index;
            Window.update(active);
            if (index != Window.index)
                OnIndexChanged(new EventArgs());

            if (CancelButton != null)
                CancelButton.Update(active);
            bool cancel = CanceledTriggered(active);

            if (cancel)
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                Window.cancel();
                OnCanceled(new EventArgs());
            }
            else if (Window.is_selected())
            {
                OnSelected(new EventArgs());
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
            Window.draw(spriteBatch);
            if (CancelButton != null)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                CancelButton.Draw(spriteBatch);
                spriteBatch.End();
            }
        }
        #endregion

        #region ITargetMenu
        public int UnitId { get { return Window.get_unit().id; } }
        
        public bool IsWindowA<K>()
        {
            return Window is K;
        }

        public bool ManualTargeting { get { return Window.manual_targeting; } }
        public bool HasTarget { get { return Window.has_target; } }

        public Vector2 TargetLoc
        {
            get
            {
                return Window.target_loc(Window.target);
            }
        }
        public Vector2 LastTargetLoc
        {
            get
            {
                return Window.target_loc(Window.targets[Window.LastTargetIndex]);
            }
        }

        public bool IsRescueDropMenu
        {
            get
            {
                return Window is Window_Target_Rescue &&
                    (Window as Window_Target_Rescue).mode == 1;
            }
        }
        #endregion

        #region IHasCancelButton
        public bool HasCancelButton { get { return CancelButton != null; } }
        public Vector2 CancelButtonLoc { get { return CancelButton.loc; } }
        #endregion
    }
}
