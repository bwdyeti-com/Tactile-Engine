#if !MONOGAME && DEBUG
using System;
using Microsoft.Xna.Framework;
using Tactile.Windows.Map;
using Tactile.Windows.UserInterface.Command;
using TactileLibrary;
using TactileLibrary.EventArg;

namespace Tactile.Menus.Map
{
    enum UnitEditorUnitOptions
        { EditUnit, MoveUnit, ChangeTeam, CopyUnit, RemoveUnit, None }

    class UnitEditorUnitMenuManager : InterfaceHandledMenuManager<IUnitEditorUnitMenuHandler>
    {
        protected UnitEditorUnitMenuManager(IUnitEditorUnitMenuHandler handler)
            : base(handler) { }

        public static UnitEditorUnitMenuManager CommandMenu(IUnitEditorUnitMenuHandler handler)
        {
            var manager = new UnitEditorUnitMenuManager(handler);

            var unitMenu = new UnitEditorUnitCommandMenu();
            unitMenu.Selected += manager.unitMenu_Selected;
            unitMenu.TeamChanged += manager.UnitMenu_TeamChanged;
            unitMenu.Canceled += manager.menu_ClosedCanceled;
            manager.AddMenu(unitMenu);

            return manager;
        }

        public static UnitEditorUnitMenuManager UnitEditor(
            IUnitEditorUnitMenuHandler handler,
            Data_Unit unitData,
            bool reinforcement = false)
        {
            var manager = new UnitEditorUnitMenuManager(handler);
            
            Global.test_battler_1 = Test_Battle_Character_Data.from_data(
                unitData.type,
                unitData.identifier,
                unitData.data);
            if (reinforcement)
            {
                Global.test_battler_1.Actor_Id = Global.game_map.last_added_unit.actor.id;
            }

            var unitEditor = new Window_Unit_Editor(reinforcement);
            unitEditor.Confirmed += manager.UnitEditor_Confirmed;
            unitEditor.Canceled += manager.UnitEditor_Canceled;
            unitEditor.Closing += manager.UnitEditor_Closing;
            manager.AddMenu(unitEditor);

            return manager;
        }

        // Selected an item in the map menu
        private void unitMenu_Selected(object sender, EventArgs e)
        {
            var unitMenu = (sender as UnitEditorUnitCommandMenu);

            switch ((UnitEditorUnitOptions)unitMenu.SelectedOption)
            {
                case UnitEditorUnitOptions.EditUnit:
                    Menus.Clear();
                    MenuHandler.UnitEditorUnitMenuEditUnit();
                    break;
                case UnitEditorUnitOptions.MoveUnit:
                    MenuHandler.UnitEditorUnitMenuMoveUnit();
                    break;
                case UnitEditorUnitOptions.CopyUnit:
                    MenuHandler.UnitEditorUnitMenuCopyUnit();
                    break;
                case UnitEditorUnitOptions.RemoveUnit:
                    MenuHandler.UnitEditorUnitMenuRemoveUnit();
                    break;
            }
        }

        #region Unit Editor
        private void UnitEditor_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            MenuHandler.UnitEditorUnitMenuConfirmEditUnit();
        }

        private void UnitEditor_Canceled(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Cancel);
            string caption = "Cancel editing?\nChanges will be lost.";

            var cancelWindow = new Window_Confirmation();
            cancelWindow.loc = new Vector2(56, 32);
            cancelWindow.set_text(caption);
            cancelWindow.add_choice("Yes", new Vector2(16, 32));
            cancelWindow.add_choice("No", new Vector2(56, 32));
            cancelWindow.size = new Vector2(104, 64);
            cancelWindow.index = 1;

            var cancelMenu = new ConfirmationMenu(cancelWindow);
            cancelMenu.Confirmed += UnitEditorCancelMenu_Confirmed;
            cancelMenu.Canceled += menu_Closed;
            AddMenu(cancelMenu);
        }
        private void UnitEditorCancelMenu_Confirmed(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Cancel);
            Menus.Clear();
            MenuHandler.UnitEditorUnitMenuClose();
        }

        private void UnitEditor_Closing(object sender, EventArgs e)
        {
            Menus.Clear();
            MenuHandler.UnitEditorUnitMenuClose();
        }
        #endregion

        private void UnitMenu_TeamChanged(object sender, BoolEventArgs e)
        {
            MenuHandler.UnitEditorUnitMenuChangeTeam(e.Value);
        }

        // Close the menu
        void menu_ClosedCanceled(object sender, EventArgs e)
        {
            Menus.Clear();
            MenuHandler.UnitEditorUnitMenuClose();
        }

        protected void menu_Closed(object sender, EventArgs e)
        {
            RemoveTopMenu();
        }
    }

    interface IUnitEditorUnitMenuHandler : IMenuHandler
    {
        void UnitEditorUnitMenuEditUnit();
        void UnitEditorUnitMenuConfirmEditUnit();
        void UnitEditorUnitMenuMoveUnit();
        void UnitEditorUnitMenuChangeTeam(bool increment);
        void UnitEditorUnitMenuCopyUnit();
        void UnitEditorUnitMenuRemoveUnit();
        void UnitEditorUnitMenuClose();
    }
}
#endif
