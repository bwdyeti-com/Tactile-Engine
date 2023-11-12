using System;
using System.Collections.Generic;
using System.Linq;
using Tactile.ConfigData;
using TactileLibrary;

namespace Tactile
{
    class UnitScreenConfig
    {
        public readonly static UnitScreenData NAME_NODE =
            new UnitScreenData(0, "Name", 0,
                function: (object input, int page) => ((Game_Unit)input).actor.name);

        public readonly static UnitScreenData[] UNIT_DATA = new UnitScreenData[]
        {
            #region Page 1 - Basics
            GetClassNode(0, 0),
            new UnitScreenData(64+8, "Lvl", 0,
                function: (object input, int page) => ((Game_Unit)input).actor.level,
                sortFunc: (object a, object b) => ((Game_Unit)b).actor.full_level - ((Game_Unit)a).actor.full_level,
                dataOffset: 16),
            new UnitScreenData(88+4, "Exp", 0,
                function: (object input, int page) => ((Game_Unit)input).actor.can_level() ?
                    ((Game_Unit)input).actor.exp.ToString() : "--",
                dataOffset: 16),
            new UnitScreenData(112, "HP", 0,
                function: (object input, int page) => ((Game_Unit)input).actor.hp,
                output: UnitScreenOutput.TextDivisor,
                dataOffset: 16),
            new UnitScreenData(132, "Max", 0,
                function: (object input, int page) => ((Game_Unit)input).actor.maxhp,
                dataOffset: 20),
            new UnitScreenData(155, "Affin", 0,
                function: (object input, int page) => ((Game_Unit)input).actor.affin,
                sortFunc: (object a, object b) =>
                    (int)((Game_Unit)a).actor.affin -
                    (int)((Game_Unit)b).actor.affin,
                output: UnitScreenOutput.Affinity, align: ParagraphAlign.Left,
                dataOffset: 5),
            new UnitScreenData(188, "Cond", 0,
                function: (object input, int page) => GetStates(input),
                sortFunc: (object a, object b) =>
                {
                    Game_Unit unitA = (Game_Unit)a;
                    Game_Unit unitB = (Game_Unit)b;
                    if (unitB.actor.states.Count == unitA.actor.states.Count)
                    {
                        int turnsA = 0, turnsB = 0;
                        for (int i = 0; i < unitA.actor.states.Count; i++)
                            turnsA += unitA.actor.state_turns_left(unitA.actor.states[i]);
                        for (int i = 0; i < unitB.actor.states.Count; i++)
                            turnsB += unitB.actor.state_turns_left(unitB.actor.states[i]);
                        return turnsB - turnsA;
                    }
                    else
                        return unitB.actor.states.Count - unitA.actor.states.Count;
                },
                output: UnitScreenOutput.Status, align: ParagraphAlign.Center,
                dataOffset: 16 - 5),
            #endregion

            #region Page 2 - Primary Stats
            new UnitScreenData(0, "Pow", 1,
                function: (object input, int page) => ((Game_Unit)input).stat(Stat_Labels.Pow),
                dataOffset: 16,
                textColor: (object input) => ((Game_Unit)input).actor.get_capped(Stat_Labels.Pow) ? "Green" : "Blue"),
            new UnitScreenData(28, "Skl", 1,
                function: (object input, int page) => ((Game_Unit)input).stat(Stat_Labels.Skl),
                dataOffset: 16 - 4,
                textColor: (object input) => ((Game_Unit)input).actor.get_capped(Stat_Labels.Skl) ? "Green" : "Blue"),
            new UnitScreenData(48, "Spd", 1,
                function: (object input, int page) => ((Game_Unit)input).stat(Stat_Labels.Spd),
                dataOffset: 16,
                textColor: (object input) => ((Game_Unit)input).actor.get_capped(Stat_Labels.Spd) ? "Green" : "Blue"),
            new UnitScreenData(72, "Luck", 1,
                function: (object input, int page) => ((Game_Unit)input).stat(Stat_Labels.Lck),
                dataOffset: 16,
                textColor: (object input) => ((Game_Unit)input).actor.get_capped(Stat_Labels.Lck) ? "Green" : "Blue"),
            new UnitScreenData(96, "Def", 1,
                function: (object input, int page) => ((Game_Unit)input).stat(Stat_Labels.Def),
                dataOffset: 16,
                textColor: (object input) => ((Game_Unit)input).actor.get_capped(Stat_Labels.Def) ? "Green" : "Blue"),
            new UnitScreenData(120, "Res", 1,
                function: (object input, int page) => ((Game_Unit)input).stat(Stat_Labels.Res),
                dataOffset: 16,
                textColor: (object input) => ((Game_Unit)input).actor.get_capped(Stat_Labels.Res) ? "Green" : "Blue"),
            new UnitScreenData(148, "Move", 1,
                function: (object input, int page) => ((Game_Unit)input).mov,
                dataOffset: 16 + 4,
                textColor: (object input) => ((Game_Unit)input).is_mov_capped() ? "Green" : "Blue"),
            new UnitScreenData(178, "Con", 1,
                function: (object input, int page) => ((Game_Unit)input).stat(Stat_Labels.Con),
                dataOffset: 16 - 2,
                textColor: (object input) => ((Game_Unit)input).actor.get_capped(Stat_Labels.Con) ? "Green" : "Blue"),
            new UnitScreenData(204, "Aid", 1,
                function: (object input, int page) => ((Game_Unit)input).aid(),
                dataOffset: 16 - 4),
            #endregion

            #region Page 3 - Inventory
            GetEquipNode(16, 2),
            new UnitScreenData(104, "Skills", 2,
                function: (object input, int page) => GetSkills(input),
                sortFunc: (object a, object b) =>
                    ((Game_Unit)b).actor.skills.Count -
                    ((Game_Unit)a).actor.skills.Count,
                output: UnitScreenOutput.Skills, align: ParagraphAlign.Left),
            #endregion

            #region Page 4 - Secondary Stats
            new UnitScreenData(8, "Atk", 3,
                function: (object input, int page) => GetSecondaryStat((Game_Unit)input, 0),
                sortFunc: (object a, object b) => GetSecondaryStatSort((Game_Unit)a, (Game_Unit)b, 0),
                dataOffset: 16),
            new UnitScreenData(40, "Hit", 3,
                function: (object input, int page) => GetSecondaryStat((Game_Unit)input, 1),
                sortFunc: (object a, object b) => GetSecondaryStatSort((Game_Unit)a, (Game_Unit)b, 1),
                dataOffset: 16),
            new UnitScreenData(64, "Avoid", 3,
                function: (object input, int page) => GetSecondaryStat((Game_Unit)input, 4),
                sortFunc: (object a, object b) => GetSecondaryStatSort((Game_Unit)a, (Game_Unit)b, 4),
                dataOffset: 24),
            new UnitScreenData(104, "Crit", 3,
                function: (object input, int page) => GetSecondaryStat((Game_Unit)input, 2),
                sortFunc: (object a, object b) => GetSecondaryStatSort((Game_Unit)a, (Game_Unit)b, 2),
                dataOffset: 16),
            new UnitScreenData(128, "Dod", 3,
                function: (object input, int page) => GetSecondaryStat((Game_Unit)input, 5),
                sortFunc: (object a, object b) => GetSecondaryStatSort((Game_Unit)a, (Game_Unit)b, 5),
                dataOffset: 16),
            new UnitScreenData(160, "AS", 3,
                function: (object input, int page) => GetSecondaryStat((Game_Unit)input, 3),
                sortFunc: (object a, object b) => GetSecondaryStatSort((Game_Unit)a, (Game_Unit)b, 3),
                dataOffset: 16),
            new UnitScreenData(196, "Rng", 3, align: ParagraphAlign.Center,
                function: (object input, int page) => GetSecondaryStat((Game_Unit)input, 6),
                sortFunc: (object a, object b) => GetSecondaryStatSort((Game_Unit)a, (Game_Unit)b, 6),
                dataOffset: 8),
            #endregion

            #region Page 5 - Weapon Levels
            new UnitScreenData(0 * 20 + 8, "WType1", 4,
                function: (object input, int page) => GetWLvl((Game_Unit)input, "Sword"),
                sortFunc: (object a, object b) => GetWLvlSort((Game_Unit)a, (Game_Unit)b, "Sword"),
                align: ParagraphAlign.Left, dataOffset: 8,
                textColor: (object unit) => IsWLvlCapped((Game_Unit)unit, "Sword") ? "Green" : "Blue",
                largeText: true, weaponIcon: 1),
            new UnitScreenData(1 * 20 + 8, "WType2", 4,
                function: (object input, int page) => GetWLvl((Game_Unit)input, "Lance"),
                sortFunc: (object a, object b) => GetWLvlSort((Game_Unit)a, (Game_Unit)b, "Lance"),
                align: ParagraphAlign.Left, dataOffset: 8,
                textColor: (object unit) => IsWLvlCapped((Game_Unit)unit, "Lance") ? "Green" : "Blue",
                largeText: true, weaponIcon: 2),
            new UnitScreenData(2 * 20 + 8, "WType3", 4,
                function: (object input, int page) => GetWLvl((Game_Unit)input, "Axe"),
                sortFunc: (object a, object b) => GetWLvlSort((Game_Unit)a, (Game_Unit)b, "Axe"),
                align: ParagraphAlign.Left, dataOffset: 8,
                textColor: (object unit) => IsWLvlCapped((Game_Unit)unit, "Axe") ? "Green" : "Blue",
                largeText: true, weaponIcon: 3),
            new UnitScreenData(3 * 20 + 8, "WType4", 4,
                function: (object input, int page) => GetWLvl((Game_Unit)input, "Bow"),
                sortFunc: (object a, object b) => GetWLvlSort((Game_Unit)a, (Game_Unit)b, "Bow"),
                align: ParagraphAlign.Left, dataOffset: 8,
                textColor: (object unit) => IsWLvlCapped((Game_Unit)unit, "Bow") ? "Green" : "Blue",
                largeText: true, weaponIcon: 4),
            new UnitScreenData(4 * 20 + 8, "WType5", 4,
                function: (object input, int page) => GetWLvl((Game_Unit)input, "Fire"),
                sortFunc: (object a, object b) => GetWLvlSort((Game_Unit)a, (Game_Unit)b, "Fire"),
                align: ParagraphAlign.Left, dataOffset: 8,
                textColor: (object unit) => IsWLvlCapped((Game_Unit)unit, "Fire") ? "Green" : "Blue",
                largeText: true, weaponIcon: 5),
            new UnitScreenData(5 * 20 + 8, "WType6", 4,
                function: (object input, int page) => GetWLvl((Game_Unit)input, "Thunder"),
                sortFunc: (object a, object b) => GetWLvlSort((Game_Unit)a, (Game_Unit)b, "Thunder"),
                align: ParagraphAlign.Left, dataOffset: 8,
                textColor: (object unit) => IsWLvlCapped((Game_Unit)unit, "Thunder") ? "Green" : "Blue",
                largeText: true, weaponIcon: 6),
            new UnitScreenData(6 * 20 + 8, "WType7", 4,
                function: (object input, int page) => GetWLvl((Game_Unit)input, "Wind"),
                sortFunc: (object a, object b) => GetWLvlSort((Game_Unit)a, (Game_Unit)b, "Wind"),
                align: ParagraphAlign.Left, dataOffset: 8,
                textColor: (object unit) => IsWLvlCapped((Game_Unit)unit, "Wind") ? "Green" : "Blue",
                largeText: true, weaponIcon: 7),
            new UnitScreenData(7 * 20 + 8, "WType8", 4,
                function: (object input, int page) => GetWLvl((Game_Unit)input, "Light"),
                sortFunc: (object a, object b) => GetWLvlSort((Game_Unit)a, (Game_Unit)b, "Light"),
                align: ParagraphAlign.Left, dataOffset: 8,
                textColor: (object unit) => IsWLvlCapped((Game_Unit)unit, "Light") ? "Green" : "Blue",
                largeText: true, weaponIcon: 8),
            new UnitScreenData(8 * 20 + 8, "WType9", 4,
                function: (object input, int page) => GetWLvl((Game_Unit)input, "Dark"),
                sortFunc: (object a, object b) => GetWLvlSort((Game_Unit)a, (Game_Unit)b, "Dark"),
                align: ParagraphAlign.Left, dataOffset: 8,
                textColor: (object unit) => IsWLvlCapped((Game_Unit)unit, "Dark") ? "Green" : "Blue",
                largeText: true, weaponIcon: 9),
            new UnitScreenData(9 * 20 + 8, "WType10", 4,
                function: (object input, int page) => GetWLvl((Game_Unit)input, "Staff"),
                sortFunc: (object a, object b) => GetWLvlSort((Game_Unit)a, (Game_Unit)b, "Staff"),
                align: ParagraphAlign.Left, dataOffset: 8,
                textColor: (object unit) => IsWLvlCapped((Game_Unit)unit, "Staff") ? "Green" : "Blue",
                largeText: true, weaponIcon: 10),
            #endregion

            #region Page 6 - Supports
            new UnitScreenData(8, "Ally", 5,
                function: (object input, int page) =>
                {
                    Game_Unit unit = (Game_Unit)input;
                    List<int> readySupports = unit.actor.ready_supports();
                    int supportsPerPage = Tactile.Windows.Map.Window_Unit.SUPPORTS_PER_PAGE;
                    List<Tuple<int, bool>> result = new List<Tuple<int, bool>>();
                    for (int i = 0; i < supportsPerPage; i++)
                    {
                        int index = page * supportsPerPage + i;
                        if (index < readySupports.Count)
                        {
                            int actorId = readySupports[index];
                            bool available = true;
                            if (Global.map_exists)
                            {
                                available = false;

                                // In preparations, use whole battalion
                                if (Global.game_system.preparations)
                                    available |= Global.battalion.actors.Contains(actorId);

                                // Check if unit is on the map
                                available |= Global.game_map.is_actor_deployed(actorId);
                                // Disable if blocked
                                available &= !Global.game_state.is_support_blocked(
                                    unit.actor.id, readySupports[i], true);
                            }
                            result.Add(Tuple.Create(actorId, available));
                        }
                    }
                    if (!result.Any())
                        return null;
                    return result;
                },
                sortFunc: (object a, object b) =>
                    ((Game_Unit)b).actor.ready_supports().Count -
                    ((Game_Unit)a).actor.ready_supports().Count,

                output: UnitScreenOutput.Supports, align: ParagraphAlign.Left,
                textColor: (object input) => "White",
                multiplePageFunc: (object input) =>
                {
                    List<int> readySupports = ((Game_Unit)input).actor.ready_supports();
                    int supportsPerPage = Tactile.Windows.Map.Window_Unit.SUPPORTS_PER_PAGE;
                    int count = (int)Math.Ceiling(readySupports.Count / (float)supportsPerPage);
                    return Math.Max(1, count);
                }),
            #endregion
        };

        public readonly static UnitScreenData[] SOLO_ANIM_DATA = new UnitScreenData[]
        {
            #region Page 1 - Solo Anim
            GetClassNode(0, 0),
            GetEquipNode(104, 0),
            new UnitScreenData(184, "Anim", 0,
                function: (object input, int page) => ((Game_Unit)input).actor.IndividualAnimationName,
                sortFunc: (object a, object b) =>
                {
                    Game_Unit unitA = (Game_Unit)a;
                    Game_Unit unitB = (Game_Unit)b;

                    int count = (int)Constants.Animation_Modes.Map + 1;

                    int animationA = (unitA.actor.individual_animation + 1) % count;
                    int animationB = (unitB.actor.individual_animation + 1) % count;

                    return animationB - animationA;
                },
                align: ParagraphAlign.Left,
                textColor: (object input) =>
                    ((Game_Unit)input).actor.individual_animation ==
                    (int)Constants.Animation_Modes.Map ? "Grey" : "Green",
                noSort: true),
            #endregion
        };

        private static UnitScreenData GetClassNode(int offset, int pageIndex)
        {
            return new UnitScreenData(offset, "Class", pageIndex,
                function: (object input, int page) => ((Game_Unit)input).actor.class_name,
                sortFunc: (object a, object b) => ((Game_Unit)a).actor.class_id - ((Game_Unit)b).actor.class_id,
                align: ParagraphAlign.Left,
                textColor: (object input) => "White");
        }

        private static UnitScreenData GetEquipNode(int offset, int pageIndex)
        {
            return new UnitScreenData(offset, "Equip", pageIndex,
                function: (object input, int page) => GetInventory(input),
                sortFunc: (object a, object b) =>
                {
                    Game_Unit unitA = (Game_Unit)a;
                    Game_Unit unitB = (Game_Unit)b;
                    int value;

                    int weaponIdA = unitA.actor.weapon_id;
                    if (weaponIdA == 0)
                    {
                        if (unitA.actor.items.Any(x => x.is_weapon))
                            weaponIdA = unitA.actor.items.First(x => x.is_weapon).Id;
                        else
                            weaponIdA = 0;
                    }
                    int weaponIdB = unitB.actor.weapon_id;
                    if (weaponIdB == 0)
                    {
                        if (unitB.actor.items.Any(x => x.is_weapon))
                            weaponIdB = unitB.actor.items.First(x => x.is_weapon).Id;
                        else
                            weaponIdB = 0;
                    }

                    if (Global.HasWeapon(weaponIdA) && Global.HasWeapon(weaponIdB))
                    {
                        var weaponA = Global.GetWeapon(weaponIdA);
                        var weaponB = Global.GetWeapon(weaponIdB);

                        if ((weaponA.Rank == TactileLibrary.Weapon_Ranks.None && weaponA.is_prf) ||
                                (weaponB.Rank == TactileLibrary.Weapon_Ranks.None && weaponB.is_prf))
                            value = (int)weaponA.Rank - (int)weaponB.Rank;
                        else
                            value = (int)weaponB.Rank - (int)weaponA.Rank;
                        // If the rank is the same, go by type
                        if (value == 0)
                        {
                            value = (int)weaponA.Main_Type - (int)weaponB.Main_Type;
                            // If the type is the same, use the price/use
                            if (value == 0)
                            {
                                value = (int)weaponA.Cost - (int)weaponB.Cost;
                                // If the type is the same, use the id
                                if (value == 0)
                                    value = weaponIdB - weaponIdA;
                            }
                        }
                    }
                    // If either weapon is missing, use the ids
                    else
                        value = weaponIdB - weaponIdA;

                    if (value == 0)
                        value = unitB.actor.num_items - unitA.actor.num_items;

                    return value;
                },
                output: UnitScreenOutput.Inventory, align: ParagraphAlign.Left,
                dataOffset: -16);
        }

        private static object GetStates(object input)
        {
            var unit = (Game_Unit)input;
            var result = new List<Tuple<int, int>>();
            foreach (var state in unit.actor.states)
            {
                int counter = unit.actor.state_turns_left(state);
                result.Add(Tuple.Create(state, counter));
            }
            return result;
        }

        private static object GetInventory(object input)
        {
            var unit = (Game_Unit)input;
            var result = new List<Item_Data>();

            // Show siege engine first if possible and inventory has space
            if (!unit.actor.is_full_items && unit.is_on_siege())
            {
                var siege = unit.items[Siege_Engine.SiegeInventoryIndex];
                if (siege.is_weapon)
                {
                    result.Add(new Item_Data(unit.items[Siege_Engine.SiegeInventoryIndex]));
                }
            }

            int equipped = -1;
            if (unit.actor.equipped != 0)
            {
                equipped = unit.actor.equipped - 1;
                result.Add(new Item_Data(unit.actor.items[equipped]));
            }
            for (int i = 0; i < unit.actor.items.Count; i++)
            {
                if (i == equipped)
                    continue;
                var item = unit.actor.items[i];
                if (!item.non_equipment)
                    result.Add(new Item_Data(item));
            }
            return result;
        }

        private static object GetSkills(object input)
        {
            var unit = (Game_Unit)input;
            var result = new List<int>();
            foreach (int i in unit.actor.skills)
            {
                result.Add(i);
            }
            return result;
        }

        private static object GetSecondaryStat(Game_Unit unit, int index)
        {
            var stats = new Calculations.Stats.BattlerStats(unit.id);

            // If the unit has a weapon equipped
            bool has_weapon = stats.has_weapon;
            // If said weapon is not a staff
            bool non_staff = stats.has_non_staff_weapon;

            switch (index)
            {
                case 0:
                    return !non_staff ? "--" : stats.dmg().ToString();
                case 1:
                    return !non_staff ? "--" : stats.hit().ToString();
                case 2:
                    return !non_staff ? "--" : stats.crt().ToString();
                case 3:
                    return unit.atk_spd().ToString();
                case 4:
                    return stats.avo().ToString();
                case 5:
                    return stats.dodge().ToString();
                case 6:
                    if (!has_weapon)
                    {
                        return "--";
                    }
                    else
                    {
                        int min_range = unit.min_range();
                        int max_range = unit.max_range();
                        if (unit.actor.weapon.Mag_Range)
                            return "Mag/2 ";
                        else if (min_range == max_range)
                            return min_range.ToString();
                        else
                            return string.Format("{0}-{1}", min_range, max_range);
                    }
            }

            return "0";
        }
        private static int GetSecondaryStatSort(Game_Unit unitA, Game_Unit unitB, int index)
        {
            var statsA = new Calculations.Stats.BattlerStats(unitA.id);
            var statsB = new Calculations.Stats.BattlerStats(unitB.id);

            switch (index)
            {
                case 0:
                    return statsB.dmg() - statsA.dmg();
                case 1:
                    bool staffB = unitB.actor.weapon != null && unitB.actor.weapon.is_staff();
                    bool staffA = unitA.actor.weapon != null && unitA.actor.weapon.is_staff();
                    if (staffB && staffA)
                        return 0;
                    else if (staffB)
                        return -1;
                    else if (staffA)
                        return 1;
                    return statsB.hit() - statsA.hit();
                case 2:
                    return statsB.crt() - statsA.crt();
                case 3:
                    return unitB.atk_spd() - unitA.atk_spd();
                case 4:
                    return statsB.avo() - statsA.avo();
                case 5:
                    return statsB.dodge() - statsA.dodge();
                case 6:
                    if (statsA.has_weapon != statsB.has_weapon)
                        return statsA.has_weapon ? -1 : 1;

                    // Mag Range
                    if (unitA.actor.weapon.Mag_Range != unitB.actor.weapon.Mag_Range)
                        return unitA.actor.weapon.Mag_Range ? -1 : 1;

                    int maxRngB = unitB.max_range();
                    int maxRngA = unitA.max_range();

                    // Sort first by farthest max range
                    if (maxRngB != maxRngA)
                        return maxRngB - maxRngA;

                    // Sort second by closest min range
                    return unitA.min_range() - unitB.min_range();
            }

            return 0;
        }

        private static object GetWLvl(Game_Unit unit, string typeName)
        {
            if (!Global.weapon_types.Any(x => x.Name == typeName))
                return "-";

            var weaponType = Global.weapon_types.First(x => x.Name == typeName);
            return unit.actor.weapon_level_letter(weaponType);
        }
        private static int GetWLvlSort(Game_Unit unitA, Game_Unit unitB, string typeName)
        {
            if (!Global.weapon_types.Any(x => x.Name == typeName))
                return 0;

            var weaponType = Global.weapon_types.First(x => x.Name == typeName);
            return unitB.actor.get_weapon_level(weaponType) - unitA.actor.get_weapon_level(weaponType);
        }
        private static bool IsWLvlCapped(Game_Unit unit, string typeName)
        {
            if (!Global.weapon_types.Any(x => x.Name == typeName))
                return false;

            var weaponType = Global.weapon_types.First(x => x.Name == typeName);
            int rank = unit.actor.get_weapon_level(weaponType);
            return rank >= Data_Weapon.WLVL_THRESHOLDS.Length - 1;
        }
    }
}
