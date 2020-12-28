using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Tactile.Menus.Map.Unit.Item;
using Tactile.Menus.Map.Unit.Target;
using Tactile.Windows.Command.Items;
using Tactile.Windows.Target;

namespace Tactile.Menus.Map.Unit
{
    partial class UnitMenuManager
    {
        private void AddSkillCommands()
        {
#if DEBUG
            UnitCommandMenu.CheckValidCommands(SimpleCommands.Keys);
            UnitCommandMenu.CheckValidCommands(TargetCommands.Keys);
#endif

            // Skills: Savior
            TargetCommands.Add(UnitCommandMenu.SkillCommandId(SkillMenuIds.Shelter),
                (Game_Unit unit, UnitCommandMenu menu) => Shelter(unit, menu));
            TargetCommands.Add(UnitCommandMenu.SkillCommandId(SkillMenuIds.Refuge),
                (Game_Unit unit, UnitCommandMenu menu) => Refuge(unit, menu));
            // Skills: Dash
            SimpleCommands.Add(UnitCommandMenu.SkillCommandId(SkillMenuIds.Dash),
                (Game_Unit unit) => Dash(unit));
            // Skills: Swoop
            SimpleCommands.Add(UnitCommandMenu.SkillCommandId(SkillMenuIds.Swoop),
                (Game_Unit unit) => Swoop(unit));
            // Skills: Trample
            SimpleCommands.Add(UnitCommandMenu.SkillCommandId(SkillMenuIds.Trample),
                (Game_Unit unit) => Trample(unit));
            // Skills: Sacrifice
            TargetCommands.Add(UnitCommandMenu.SkillCommandId(SkillMenuIds.Sacrifice),
                (Game_Unit unit, UnitCommandMenu menu) => Sacrifice(unit, menu));
            // Skills: Old Swoop //@Debug
            SimpleCommands.Add(UnitCommandMenu.SkillCommandId(SkillMenuIds.OldSwoop),
                (Game_Unit unit) => OldSwoop(unit));
        }

        private bool UnitMenuSelectSkill(int selectedOption, UnitCommandMenu unitMenu, Game_Unit unit)
        {
            // Skills: Masteries
            if (UnitCommandMenu.ValidMasteryCommand(selectedOption))
            {
                string skill = UnitCommandMenu.GetMastery(selectedOption);
                Mastery(unit, skill);
                return true;
            }
            return false;
        }

        #region Skills: Savior (Shelter)
        private void Shelter(Game_Unit unit, UnitCommandMenu unitMenu)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetWindow = new Window_Target_Rescue(
                unit.id, 4, new Vector2(-80, 0));
            var targetMenu = new UnitTargetMenu(targetWindow, unitMenu);
            targetMenu.Selected += shelterTargetMenu_Selected;
            targetMenu.Canceled += unitTargetMenu_Canceled;
            AddMenu(targetMenu);

            Global.player.facing = 4;
            Global.player.update_cursor_frame();
        }

        private void shelterTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as UnitTargetMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
            MenuHandler.UnitMenuShelter(unit, targetMenu.SelectedUnitId);
        }
        #endregion

        #region Skills: Savior (Refuge)
        private void Refuge(Game_Unit unit, UnitCommandMenu unitMenu)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetWindow = new Window_Target_Rescue(
                unit.id, 5, new Vector2(-80, 0));
            var targetMenu = new UnitTargetMenu(targetWindow, unitMenu);
            targetMenu.Selected += refugeTargetMenu_Selected;
            targetMenu.Canceled += unitTargetMenu_Canceled;
            AddMenu(targetMenu);

            Global.player.facing = 4;
            Global.player.update_cursor_frame();
        }

        private void refugeTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as UnitTargetMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
            MenuHandler.UnitMenuRefuge(unit, targetMenu.SelectedUnitId);
        }
        #endregion

        // Skills: Dash
        private void Dash(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var unitMenu = (Menus.Peek() as UnitCommandMenu);

            Global.game_temp.menuing = false;
            Global.game_system.Menu_Canto = unitMenu.Canto | Canto_Records.Dash;
            CloseCommandMenu(true);

            unit.activate_dash();
        }

        // Skills: Swoop
        private void Swoop(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var unitMenu = (Menus.Peek() as UnitCommandMenu);
            Global.game_map.range_start_timer = 0;
            unit.swoop_activated = true;

            var attackItemWindow = new Window_Command_Item_Swoop(unit.id, new Vector2(24, 8));
            attackItemWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            attackItemWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            attackItemWindow.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
            Global.game_temp.temp_skill_ranges["SWOOP"] = unit.swoop_range();

            var attackMenu = new AttackItemMenu(attackItemWindow, unitMenu);
            attackMenu.IndexChanged += attackMenu_IndexChanged;
            attackMenu.Selected += attackMenu_Selected;
            attackMenu.Canceled += attackMenu_Canceled;
            AddMenu(attackMenu);
        }

        // Skills: Trample
        private void Trample(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var unitMenu = (Menus.Peek() as UnitCommandMenu);
            Global.game_map.range_start_timer = 0;
            unit.trample_activated = true;

            var attackItemWindow = new Window_Command_Item_Trample(unit.id, new Vector2(24, 8));
            attackItemWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            attackItemWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            attackItemWindow.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
            Global.game_temp.temp_skill_ranges["TRAMPLE"] = unit.trample_range();
            Global.game_temp.temp_skill_move_ranges["TRAMPLE"] = unit.trample_move_range();

            var attackMenu = new AttackItemMenu(attackItemWindow, unitMenu);
            attackMenu.IndexChanged += attackMenu_IndexChanged;
            attackMenu.Selected += attackMenu_Selected;
            attackMenu.Canceled += attackMenu_Canceled;
            AddMenu(attackMenu);
        }

        #region Skills: Sacrifice
        private void Sacrifice(Game_Unit unit, UnitCommandMenu unitMenu)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetWindow = new Window_Target_Sacrifice(unit.id, new Vector2(0, 0));
            var targetMenu = new UnitTargetMenu(targetWindow, unitMenu);
            targetMenu.Selected += sacrificeTargetMenu_Selected;
            targetMenu.Canceled += unitTargetMenu_Canceled;
            AddMenu(targetMenu);

            Global.player.facing = 4;
            Global.player.update_cursor_frame();
        }

        private void sacrificeTargetMenu_Selected(object sender, EventArgs e)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var targetMenu = (sender as UnitTargetMenu);
            targetMenu.Accept();

            Game_Unit unit = Global.game_map.units[targetMenu.UnitId];
            MenuHandler.UnitMenuSacrifice(unit, targetMenu.SelectedUnitId);
        }
        #endregion

        // Skills: Old Swoop //@Debug
        private void OldSwoop(Game_Unit unit)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var unitMenu = (Menus.Peek() as UnitCommandMenu);
            Global.game_map.range_start_timer = 0;
            unit.old_swoop_activated = true;

            var attackItemWindow = new Window_Command_Item_Swoop(unit.id, new Vector2(24, 8));
            attackItemWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            attackItemWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            attackItemWindow.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
            Global.game_temp.temp_skill_ranges["OLDSWOOP"] = unit.old_swoop_range();

            var attackMenu = new AttackItemMenu(attackItemWindow, unitMenu);
            attackMenu.IndexChanged += attackMenu_IndexChanged;
            attackMenu.Selected += attackMenu_Selected;
            attackMenu.Canceled += attackMenu_Canceled;
            AddMenu(attackMenu);
        }

        // Skills: Masteries
        private void Mastery(Game_Unit unit, string skill)
        {
            Global.game_system.play_se(System_Sounds.Confirm);
            var unitMenu = (Menus.Peek() as UnitCommandMenu);
            Global.game_map.range_start_timer = 0;
            unit.call_mastery(skill);

            var attackItemWindow = new Window_Command_Item_Attack(
                unit.id, new Vector2(24, 8), skill);
            attackItemWindow.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            attackItemWindow.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            attackItemWindow.data_stereoscopic = Config.MAPCOMMAND_DATA_DEPTH;
            unitMenu.RefreshTempAttackRange(attackItemWindow.redirect());

            var attackMenu = new AttackItemMenu(attackItemWindow, unitMenu);
            attackMenu.IndexChanged += attackMenu_IndexChanged;
            attackMenu.Selected += attackMenu_Selected;
            attackMenu.Canceled += attackMenu_Canceled;
            AddMenu(attackMenu);
        }

        private bool CancelCommandSkillCommand(UnitCommandMenu unitMenu)
        {
            Game_Unit unit = Global.game_map.units[unitMenu.UnitId];
            if (unit.DashActivated)
            {
                Global.game_system.Menu_Canto = unitMenu.Canto;
                menu_ClosedCanceled(unitMenu, new EventArgs());
                return true;
            }
            return false;
        }

        public bool ShowSkillRange(string skill)
        {
            for (int i = 0; i < Menus.Count; i++)
            {
                if (Menus.ElementAt(i) is UnitCommandMenu)
                {
                    var unitMenu = (Menus.ElementAt(i) as UnitCommandMenu);
                    switch (skill)
                    {
                        // Skills: Swoop
                        case "SWOOP":
                            if (unitMenu.CommandAtIndex ==
                                    UnitCommandMenu.SkillCommandId(SkillMenuIds.Swoop))
                                return true;
                            break;
                        // Skills: Trample
                        case "TRAMPLE":
                            if (unitMenu.CommandAtIndex ==
                                    UnitCommandMenu.SkillCommandId(SkillMenuIds.Trample))
                                return true;
                            break;
                        // Skills: Old Swoop //Debug
                        case "OLDSWOOP":
                            if (unitMenu.CommandAtIndex ==
                                    UnitCommandMenu.SkillCommandId(SkillMenuIds.OldSwoop))
                                return true;
                            break;
                    }
                    // Skills: Masteries
                    if (Game_Unit.MASTERIES.Contains(skill))
                    {
                        int index = Game_Unit.MASTERIES.IndexOf(skill);
                        if (unitMenu.CommandAtIndex == UnitCommandMenu.MasteryCommandId(skill))
                            return true;
                    }
                    break;
                }
            }
            return false;
        }
    }

    partial interface IUnitMenuHandler : IMenuHandler
    {
        void UnitMenuShelter(Game_Unit unit, int targetId);
        void UnitMenuRefuge(Game_Unit unit, int targetId);
        void UnitMenuSacrifice(Game_Unit unit, int targetId);
    }
}
