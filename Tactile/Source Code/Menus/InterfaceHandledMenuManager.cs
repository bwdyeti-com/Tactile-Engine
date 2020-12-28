namespace Tactile.Menus
{
    class InterfaceHandledMenuManager<T> : MenuManager where T : IMenuHandler
    {
        protected T MenuHandler;

        protected InterfaceHandledMenuManager(T handler)
        {
            MenuHandler = handler;
        }
    }

    interface IMenuHandler { }
}
