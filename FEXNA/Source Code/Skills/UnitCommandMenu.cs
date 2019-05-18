using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FEXNA.Menus.Map.Unit
{
    enum SkillMenuIds : int
    {
        Shelter = 0,
        Dash = 1,
        Swoop = 2,
        Trample = 3,
        Sacrifice = 4,
        Refuge = 5,
        OldSwoop = 20
    }

    partial class UnitCommandMenu
    {
        const int BASE_SKILL_MENU_ID = 100;
        const int BASE_MASTERY_MENU_ID = 200;

#if DEBUG
        public static void CheckValidCommands(IEnumerable<int> commands)
        {
            if (commands.Any(x => x >= UnitCommandMenu.BASE_SKILL_MENU_ID))
            {
                throw new ArgumentException(string.Format(
                    "All unit menu commands must be lower than BASE_SKILL_MENU_ID ({0})",
                    UnitCommandMenu.BASE_SKILL_MENU_ID));
            }
        }
#endif

        private void AddSkillCommands(ref List<string> commands, Game_Unit unit)
        {
            // Actions:
            //   100 = Shelter
            //   101 = Dash
            //   102 = Swoop
            //   103 = Trample
            //   104 = Sacrifice
            //   105 = Refuge
            //   120 = Old Swoop //Debug
            if (CantoAllowsNormalActions(Canto))
            {
                // Skills: Savior
                if (commands.Contains("Rescue") && unit.has_cover() &&
                    !unit.is_rescue_blocked())
                {
                    List<int> allyRange = unit.allies_in_range(1);
                    bool canRescue = false;
                    foreach (int id in allyRange)
                        if (unit.can_rescue(Global.game_map.units[id]))
                            if (Pathfind.passable(unit, Global.game_map.units[id].loc))
                            {
                                canRescue = true;
                                break;
                            }
                    if (canRescue)
                    {
                        int index = commands.IndexOf("Rescue");
                        commands.Insert(index + 1, "Shelter");
                        AddSkillIndex(index, SkillMenuIds.Shelter);
                    }
                }
                {
                    List<int> allyRange = unit.allies_in_range(1);
                    bool canTakeRefuge = false;
                    foreach (int id in allyRange)
                    {
                        Game_Unit target = Global.game_map.units[id];
                        if (target.has_refuge() && target.can_rescue(unit) &&
                            !unit.is_rescue_blocked())
                        {
                            canTakeRefuge = true;
                            break;
                        }
                    }
                    if (canTakeRefuge)
                    {
                        // Place before status and wait, at least
                        int index = Math.Min(
                            commands.IndexOf("Status"), commands.IndexOf("Wait"));

                        // Try placing before Item
                        int itemIndex = commands.IndexOf("Item");
                        if (itemIndex >= 0)
                            index = itemIndex;
                        // Try placing after rescue
                        int rescueIndex = commands.IndexOf("Rescue");
                        if (rescueIndex >= 0)
                            index = rescueIndex + 1;
                        // Try placing after shelter
                        int shelterIndex = commands.IndexOf("Shelter");
                        if (shelterIndex >= 0)
                            index = shelterIndex + 1;

                        commands.Insert(index + 0, "Refuge");
                        AddSkillIndex(index - 1, SkillMenuIds.Refuge);
                    }
                }
                // Skills: Dash
                if (unit.actor.has_skill("DASH"))
                {
                    // Can move and not in starting location/has done something
                    if (unit.base_mov > 0 && (unit.turn_start_loc != unit.loc ||
                        Canto != Canto_Records.None))
                    {
                        int index = Math.Min(
                            commands.IndexOf("Status"), commands.IndexOf("Wait"));
                        commands.Insert(index + 0, "Dash");
                        AddSkillIndex(index - 1, SkillMenuIds.Dash);
                    }
                }
                // Skills: Swoop
                if (unit.actor.has_skill("SWOOP"))
                {
                    List<int>[] ary = unit.enemies_in_swoop_range();
                    List<int> enemyRange = ary[0];
                    if (enemyRange.Count > 0)
                    {
                        Global.game_temp.temp_skill_ranges["SWOOP"] = unit.swoop_range();
                        Global.game_map.range_start_timer = 0;
                        int index = commands.IndexOf("Attack");
                        commands.Insert(index + 1, "Swoop");
                        AddSkillIndex(index, SkillMenuIds.Swoop);
                    }
                }
                // Skills: Trample
                if (unit.actor.has_skill("TRAMPLE"))
                {
                    List<int>[] ary = unit.enemies_in_trample_range();
                    List<int> enemyRange = ary[0];
                    if (enemyRange.Count > 0)
                    {
                        Global.game_temp.temp_skill_ranges["TRAMPLE"] = unit.trample_range();
                        Global.game_temp.temp_skill_move_ranges["TRAMPLE"] = unit.trample_move_range();
                        Global.game_map.range_start_timer = 0;
                        int index = commands.IndexOf("Attack");
                        commands.Insert(index + 1, "Trample");
                        AddSkillIndex(index, SkillMenuIds.Trample);
                    }
                }
                // Skills: Sacrifice
                if (unit.actor.has_skill("SACRIFICE"))
                {
                    if (unit.actor.hp > 1)
                    {
                        List<int> allyRange = unit.allies_in_range(1);
                        bool canHeal = false;
                        foreach (int id in allyRange)
                            if (!Global.game_map.units[id].actor.is_full_hp())
                            {
                                canHeal = true;
                                break;
                            }
                        if (canHeal)
                        {
                            int index = commands.IndexOf("Attack");
                            commands.Insert(index + 1, "Sacrifice");
                            AddSkillIndex(index, SkillMenuIds.Sacrifice);
                        }
                    }
                }
                // Skills: Old Swoop //@Debug
                if (unit.actor.has_skill("OLDSWOOP"))
                {
                    List<int>[] ary = unit.enemies_in_old_swoop_range();
                    List<int> enemyRange = ary[0];
                    if (enemyRange.Count > 0)
                    {
                        Global.game_temp.temp_skill_ranges["OLDSWOOP"] = unit.old_swoop_range();
                        Global.game_map.range_start_timer = 0;
                        int index = commands.IndexOf("Attack");
                        commands.Insert(index + 1, "OldSwoop");
                        AddSkillIndex(index, SkillMenuIds.OldSwoop);
                    }
                }
                // Skills: Masteries
                for (int i = 0; i < Game_Unit.MASTERIES.Count; i++)
                {
                    if (unit.actor.has_skill(Game_Unit.MASTERIES[i]) && unit.is_mastery_ready(Game_Unit.MASTERIES[i]))
                    {
                        string skill = Game_Unit.MASTERIES[i];
                        List<int>[] rangeAry = unit.enemies_in_range(skill);
                        if (rangeAry[1].Count > 0)
                        {
                            List<int> itemIndices = unit.weapon_indices(rangeAry[1]);
                            Global.game_temp.temp_skill_ranges[skill] = unit.get_weapon_range(itemIndices, new HashSet<Vector2> { unit.loc }, skill);

                            //Global.game_temp.temp_skill_ranges[skill] = Global.game_map.get_unit_range(new List<Vector2> { unit.loc },
                            //    unit.min_range_absolute(skill), unit.max_range_absolute(skill), Game_Unit.mastery_blocked_through_walls(skill));
                            Global.game_map.range_start_timer = 0;

                            int index = commands.IndexOf("Attack");
                            commands.Insert(index + 1, Global.skill_from_abstract(skill).Name);
                            IndexRedirect.Insert(index + 1, BASE_MASTERY_MENU_ID + i);
                        }
                    }
                }
            }
        }
        
        private void AddSkillIndex(int index, SkillMenuIds skill)
        {
            IndexRedirect.Insert(index + 1, SkillCommandId(skill));
        }

        public static int SkillCommandId(SkillMenuIds skill)
        {
            return BASE_SKILL_MENU_ID + (int)skill;
        }
        public static int MasteryCommandId(string skill)
        {
            int index = Game_Unit.MASTERIES.IndexOf(skill);
            if (index < 0)
            {
#if DEBUG
                throw new ArgumentException("\"{0}\" is not in Game_Unit.MASTERIES");
#endif
                return -1;
            }
            return BASE_MASTERY_MENU_ID + index;
        }

        public static bool ValidMasteryCommand(int command)
        {
            return command >= BASE_MASTERY_MENU_ID &&
                command < BASE_MASTERY_MENU_ID + Game_Unit.MASTERIES.Count;
        }
        public static string GetMastery(int command)
        {
            return Game_Unit.MASTERIES[command - BASE_MASTERY_MENU_ID];
        }
    }
}
