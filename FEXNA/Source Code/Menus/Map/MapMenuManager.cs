using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Menus.Map;
using FEXNA.Windows.Map;

namespace FEXNA.Menus.Map
{
    enum Map_Menu_Options { Unit, Data, Options, Suspend, End, None }

    class MapMenuManager : InterfaceHandledMenuManager<IMapMenuHandler>
    {
        public MapMenuManager(IMapMenuHandler handler)
            : base(handler)
        {
            var mapMenu = new MapCommandMenu();
            mapMenu.Selected += mapMenu_Selected;
            mapMenu.Canceled += menu_ClosedCanceled;
            AddMenu(mapMenu);
        }

        // Selected an item in the map menu
        private void mapMenu_Selected(object sender, EventArgs e)
        {
            switch ((sender as MapCommandMenu).SelectedOption)
            {
                case Map_Menu_Options.Unit:
                    RemoveTopMenu();

                    var unitMenu = new Window_Unit();
                    unitMenu.Status += unitMenu_Status;
                    unitMenu.Closed += unitMenu_Closed;
                    AddMenu(unitMenu);
                    break;
                case Map_Menu_Options.Data:
                    RemoveTopMenu();

                    var dataMenu = new Window_Data();
                    dataMenu.Closed += menu_ClosedCanceled;
                    AddMenu(dataMenu);
                    break;
                case Map_Menu_Options.Options:
                    RemoveTopMenu();

                    var optionsMenu = new Window_Options();
                    optionsMenu.SoloAnim += optionsMenu_SoloAnim;
                    optionsMenu.Closed += menu_ClosedCanceled;
                    AddMenu(optionsMenu);
                    break;
                case Map_Menu_Options.Suspend:
                    MenuHandler.MapMenuSuspend();
                    break;
                case Map_Menu_Options.End:
                    MenuHandler.MapMenuEndTurn();
                    break;
            }
        }

        // Open status screen from unit menu
        void unitMenu_Status(object sender, EventArgs e)
        {
            Global.game_temp.menu_call = false;
            Global.game_temp.status_menu_call = false;

            var unitWindow = (Menus.Peek() as Window_Unit);
            List<int> team = unitWindow.team;
            int index = team.IndexOf(Global.game_temp.status_unit_id);
            index = Math.Max(0, index);

            var statusMenu = new Window_Status(team, index);
            statusMenu.Closed += statusMenu_Closed;
            AddMenu(statusMenu);
        }

        // Unit menu canceled
        void unitMenu_Closed(object sender, EventArgs e)
        {
            var unitMenu = Menus.Peek() as Window_Unit;
            if (unitMenu.unit_selected)
                Global.player.force_loc(Global.game_map.units[unitMenu.unit_index].loc);

            menu_ClosedCanceled(sender, e);
        }

        // Open unit screen from options to change solo anims
        void optionsMenu_SoloAnim(object sender, EventArgs e)
        {
            var soloAnimMenu = new Window_SoloAnim();
            soloAnimMenu.Status += unitMenu_Status;
            soloAnimMenu.Closed += soloAnimMenu_Closed;
            AddMenu(soloAnimMenu);
        }

        void soloAnimMenu_Closed(object sender, EventArgs e)
        {
            RemoveTopMenu();
        }

        // Status menu canceled
        void statusMenu_Closed(object sender, EventArgs e)
        {
            var statusMenu = Menus.Peek() as Window_Status;
            int currentUnit = statusMenu.current_unit;

            Global.game_temp.status_team = 0;
            statusMenu.close();
            RemoveTopMenu();

            var unitMenu = Menus.Peek() as Window_Unit;
            unitMenu.unit_index = currentUnit;

        }

        // Close the menu
        void menu_ClosedCanceled(object sender, EventArgs e)
        {
            Menus.Clear();
            Global.game_temp.menuing = false;
            Global.game_map.highlight_test();
        }
    }

    interface IMapMenuHandler : IMenuHandler
    {
        void MapMenuSuspend();
        void MapMenuEndTurn();
    }
}
