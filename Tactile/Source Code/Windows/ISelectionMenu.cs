using TactileLibrary;

namespace Tactile.Windows
{
    interface ISelectionMenu
    {
        Maybe<int> selected_index();

        bool is_selected();

        bool is_canceled();

        void reset_selected();
    }
}
