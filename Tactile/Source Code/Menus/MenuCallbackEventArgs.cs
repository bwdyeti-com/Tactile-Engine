using System;

namespace Tactile.Menus
{
    internal delegate void MenuCallbackEventHandler(EventArgs e);

    class MenuCallbackEventArgs : EventArgs
    {
        public Action<IMenu> AddMenuCall { get; private set; }
        public EventHandler<EventArgs> MenuClosedCall { get; private set; }

        public MenuCallbackEventArgs(Action<IMenu> addMenuCall, EventHandler<EventArgs> menuClosedCall)
        {
            AddMenuCall = addMenuCall;
            MenuClosedCall = menuClosedCall;
        }
    }
}
