#if DEBUG
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Menus.Map;

namespace FEXNA.Menus.Map
{
    enum DebugMenuOptions { RefreshUnit, DeleteUnit, SkipChapter, Heal,
        MaxSupport, LevelUp, ToggleFog, InfiniteMove, ToggleAI, None }

    class MapDebugMenuManager : InterfaceHandledMenuManager<IMapDebugMenuHandler>
    {
        public MapDebugMenuManager(IMapDebugMenuHandler handler)
            : base(handler)
        {
            var debugMenu = new MapDebugMenu();
            debugMenu.Selected += debugMenu_Selected;
            debugMenu.Canceled += menu_ClosedCanceled;
            AddMenu(debugMenu);
        }

        void debugMenu_Selected(object sender, EventArgs e)
        {
            Game_Unit unit = Global.game_map.get_unit(Global.player.loc);
            // Buzz on options that require a unit
            if (unit == null)
            switch ((DebugMenuOptions)(sender as CommandMenu).SelectedIndex.ValueOrDefault)
            {
                case DebugMenuOptions.RefreshUnit:
                case DebugMenuOptions.DeleteUnit:
                case DebugMenuOptions.Heal:
                case DebugMenuOptions.MaxSupport:
                case DebugMenuOptions.LevelUp:
                    Global.game_system.play_se(System_Sounds.Buzzer);
                    return;
            }

            switch ((DebugMenuOptions)(sender as CommandMenu).SelectedIndex.ValueOrDefault)
            {
                case DebugMenuOptions.RefreshUnit:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    MenuHandler.DebugMenuRefreshUnit(unit);
                    break;
                case DebugMenuOptions.DeleteUnit:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    MenuHandler.DebugMenuDeleteUnit(unit);
                    break;
                case DebugMenuOptions.SkipChapter:
                    MenuHandler.DebugMenuSkipChapter();
                    break;
                case DebugMenuOptions.Heal:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    MenuHandler.DebugMenuHealUnit(unit);
                    break;
                case DebugMenuOptions.MaxSupport:
                    Global.game_system.play_se(System_Sounds.Confirm);
                    MenuHandler.DebugMenuSupportGainUnit(unit);
                    break;
                case DebugMenuOptions.LevelUp:
                    // If the unit can't promote or gain any levels
                    if (!unit.actor.can_promote())
                        if (unit.actor.exp_gain_possible() <= 0)
                        {
                            Global.game_system.play_se(System_Sounds.Buzzer);
                            return;
                        }
                    
                    MenuHandler.DebugMenuLevelUpUnit(unit);

                    // Grey out option if no more levels can be gained
                    if (unit.actor.exp_gain_possible() <= 0 && !unit.actor.can_promote())
                    {
                        var menu = (sender as CommandMenu);
                        menu.SetTextColor((int)DebugMenuOptions.LevelUp, "Grey");
                    }
                    break;
                case DebugMenuOptions.ToggleFog:
                    MenuHandler.DebugMenuToggleFog();
                    break;
                case DebugMenuOptions.InfiniteMove:
                    MenuHandler.DebugMenuToggleInfiniteMove();
                    break;
                case DebugMenuOptions.ToggleAI:
                    MenuHandler.DebugMenuToggleAI();
                    break;
            }
        }

        // Close the menu
        void menu_ClosedCanceled(object sender, EventArgs e)
        {
            Menus.Clear();
            Global.game_temp.menuing = false;
            Global.game_map.highlight_test();
        }
    }

    interface IMapDebugMenuHandler : IMenuHandler
    {
        void DebugMenuRefreshUnit(Game_Unit unit);
        void DebugMenuDeleteUnit(Game_Unit unit);
        void DebugMenuSkipChapter();
        void DebugMenuHealUnit(Game_Unit unit);
        void DebugMenuSupportGainUnit(Game_Unit unit);
        void DebugMenuLevelUpUnit(Game_Unit unit);
        void DebugMenuToggleFog();
        void DebugMenuToggleInfiniteMove();
        void DebugMenuToggleAI();
    }
}
#endif
