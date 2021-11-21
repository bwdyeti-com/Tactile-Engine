using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using TactileLibrary;
using TactileLibrary.Pathfinding;

namespace Tactile.Pathfinding
{
    enum Unit_Passable { Clear, Blocked, PassableFullMoveEnemy, PassableEnemy, PassableAlly }

    class UnitMovementMap : IMovementMap<Vector2>
    {
        const int OFF_MAP_PENALTY_MULT = 1; // Multiplied by the cost of tiles offmap; if greater than 1, encourages units to enter the map faster

        private int UnitId;
        private Game_Map Map;
        private bool IgnoreUnits, ThroughDoors, IgnoreDoors;
        private Maybe<bool> FullMoveThroughEnemies;
        private bool ForceGoalPassable;

        Dictionary<Vector2, int> MoveCosts;
        private Dictionary<Vector2, Unit_Passable> UnitLocs = new Dictionary<Vector2, Unit_Passable>(); // should maybe be an array?
        private HashSet<Vector2> ObstructionSources = new HashSet<Vector2>();
        private HashSet<Vector2> Doors = new HashSet<Vector2>();

        private Game_Unit Unit { get { return Map.units[UnitId]; } }
        
        private UnitMovementMap(int unitId, Game_Map map)
        {
            UnitId = unitId;
            Map = map;
            
            reset_move_costs();
        }

        private void initialize_unit_locs()
        {
            var unit = this.Unit;

            if (FullMoveThroughEnemies.IsNothing)
                FullMoveThroughEnemies = unit.is_evented_move;
            initialize_unit_locs(unit);

            // Doors
            if (ThroughDoors)
                if ((unit.can_open_door() || IgnoreDoors) && !unit.is_player_team)
                    foreach (var door in Map.door_locations)
                        Doors.Add(door.Key);
        }
        private void initialize_unit_locs(Game_Unit unit)
        {
            // Goes through all units and sets a passable tag for the tile that unit is on
            // This should probably refer to the map's unit location data instead of figuring things out itself //Yeti
            foreach (Game_Unit test_unit in Map.units.Values.Where(x => !x.is_rescued && !x.is_dead))
            {
                Vector2 loc = test_unit.pathfinding_loc;
                if (test_unit != unit && !Map.is_off_map(loc))
                {
                    Unit_Passable pass = tile_unit_passability(unit, loc, test_unit);
#if DEBUG
                    if (!Global.game_system.is_interpreter_running)
                        // if the location is already occupied, problems
                        Debug.Assert(!UnitLocs.ContainsKey(loc), string.Format(
                            "Two units share a location when trying to start pathfinding\n\n{0}\n{1}",
                            unit, test_unit));
                    UnitLocs[loc] = pass;
#else
                    if (!UnitLocs.ContainsKey(loc))
                        UnitLocs.Add(loc, pass);
#endif
                    if (pass == Unit_Passable.Blocked && test_unit.HasZoneOfControl())
                    {
                        foreach (Vector2 adjacentLoc in AdjacentLocations(loc))
                            ObstructionSources.Add(adjacentLoc);
                    }
                }
            }

            foreach (LightRune rune in Map.enumerate_light_runes())
            {
                Vector2 loc = rune.loc;
                if (!Map.is_off_map(loc))
                {
                    UnitLocs[loc] = tile_unit_passability(unit, loc, rune);
                }
            }
        }

        private Unit_Passable tile_unit_passability(Game_Unit unit, Vector2 loc, Game_Unit unit_here)
        {
            if (unit.is_passable_team(unit_here))
                return Unit_Passable.PassableAlly;
            else
            {
                if (FullMoveThroughEnemies)
                    return Unit_Passable.PassableFullMoveEnemy;
                else if (unit.can_pass_enemies())
                    return Unit_Passable.PassableAlly;
                if (IgnoreUnits)
                    return Unit_Passable.PassableEnemy;
                return Unit_Passable.Blocked;
            }
        }
        private Unit_Passable tile_unit_passability(Game_Unit unit, Vector2 loc, Map_Object unit_here)
        {
            if (unit.is_passable_team(unit_here))
                return Unit_Passable.PassableAlly;
            else
            {
                if (FullMoveThroughEnemies)
                    return Unit_Passable.PassableFullMoveEnemy;
                else if (unit.can_pass_enemies())
                    return Unit_Passable.PassableAlly;
                if (IgnoreUnits)
                    return Unit_Passable.PassableEnemy;
                return Unit_Passable.Blocked;
            }
        }

        private void reset_move_costs()
        {
            MoveCosts = new Dictionary<Vector2, int>();
            /* //Yeti
            if (MoveCosts == null ||
                    MoveCosts.GetLength(0) != Map.width ||
                    MoveCosts.GetLength(1) != Map.height)
                MoveCosts = new Maybe<int>[Map.width, Map.height];
            for (int y = 0; y < MoveCosts.GetLength(1); y++)
                for (int x = 0; x < MoveCosts.GetLength(0); x++)
                    MoveCosts[x, y] = Maybe<int>.Nothing;*/
        }

        public List<Vector2> get_route(Vector2 target_loc, int mov)
        {
            return get_route(target_loc, mov, this.Unit.loc);
        }
        public List<Vector2> get_route(Vector2 target_loc, int mov, Vector2 loc)
        {
            var pathfinder = Pathfind();
            return pathfinder.get_route(loc, target_loc, mov);
        }

        public List<Vector2> get_reverse_route(Vector2 loc, int mov)
        {
            return get_reverse_route(loc, mov, this.Unit.loc);
        }
        public List<Vector2> get_reverse_route(Vector2 loc, int mov, Vector2 target_loc)
        {
            var pathfinder = Pathfind();
            return pathfinder.get_reverse_route(loc, target_loc, mov);
        }

        public Maybe<int> get_distance(Vector2 target_loc, int mov)
        {
            return get_distance(target_loc, mov, this.Unit.loc);
        }
        public Maybe<int> get_distance(Vector2 target_loc, int mov, Vector2 loc)
        {
            var pathfinder = Pathfind();
            return pathfinder.get_distance(loc, target_loc, mov);
        }

        public List<Vector2> convert_to_motions(List<Vector2> route)
        {
            if (route == null)
                return null;

            if (route.Count <= 1)
                return new List<Vector2>();

            return Enumerable.Range(0, route.Count - 1)
                .Select(x => route[x] - route[x + 1])
                .ToList();
        }

        #region IMovementMap
        public Pathfinder<Vector2> Pathfind()
        {
            Pathfinder<Vector2> result;
            if (Constants.Gameplay.MOVE_ARROW_WIGGLING)
            {
                Func<bool> wiggleChance = () => Global.game_system.roll_rng(50);
                result = new Pathfinder<Vector2>(this, wiggleChance);
            }
            else
                result = new Pathfinder<Vector2>(this);

            return result;
        }

        public int MoveCostFactor { get { return 10; } }

        public TileData GetTileData(Vector2 loc, Vector2 goalLoc)
        {
            bool passable = Passable(loc, goalLoc);
            int tileCost = -1;
            if (passable)
                tileCost = TileCost(loc, goalLoc);
            bool obstructed = Obstructed(loc, goalLoc);

            return new TileData(passable, tileCost, obstructed);
        }

        public bool Passable(Vector2 loc)
        {
            return terrain_cost(loc) >= 0;
        }
        public bool Passable(Vector2 loc, Vector2 goalLoc)
        {
            // return false if terrain type of this tile doesn't have stats? //Yeti

            if (Doors.Contains(loc))
                return true;

            if (loc == goalLoc && Map.is_off_map(goalLoc, false))
                return true;

            int move_cost = TileCost(loc, goalLoc);
            bool pass = move_cost >= 0;
            if (pass)
            {
                // If team matters for this test, make sure no enemy is blocking
                if (loc != goalLoc)
                    pass &= team_passable(this.Unit, loc);
            }
            return pass;
        }

        private bool team_passable(Game_Unit unit, Vector2 loc)
        {
            if (!UnitLocs.Keys.Contains(loc))
                return true;
            return UnitLocs[loc] != Unit_Passable.Blocked;
        }

        public int TileCost(Vector2 loc, Vector2 goalLoc)
        {
            if (MoveCosts.ContainsKey(loc) && loc != goalLoc)
                return MoveCosts[loc];
            else
            {
                // If the location is completely off the map, return the move cost
                if ((int)loc.X < 0 || (int)loc.X >= Map.width ||
                        (int)loc.Y < 0 || (int)loc.Y >= Map.height)
                    return terrain_cost(loc);

                // If the terrain cost for this tile hasn't been cached yet
                int cost = MoveCosts[loc] = terrain_cost(loc);

                if (cost < 0)
                {
                    // If the goal should be forced passable
                    if (loc == goalLoc && ForceGoalPassable)
                    {
                        cost = this.MoveCostFactor;
                    }
                    // If unit is off map and the tile is impassable,
                    // make it passable but costly so they can always
                    // get onto the map
                    else if (Map.is_off_map(this.Unit.loc) && Map.is_off_map(loc))
                    {
                        cost = MoveCosts[loc] =
                            (this.Unit.mov * 2) * this.MoveCostFactor * OFF_MAP_PENALTY_MULT;
                    }

                }
                return cost;
            }
        }
        private int terrain_cost(Vector2 loc)
        {
            int terr_cost = this.Unit.move_cost(loc);
            // If this is an event move and a unit is blocking
            // but they can be passed through,
            // prefer to move around them if possible
            if (UnitLocs.ContainsKey(loc) &&
                    UnitLocs[loc] == Unit_Passable.PassableFullMoveEnemy)
                terr_cost += Math.Max(1, this.Unit.mov);
            return terr_cost * this.MoveCostFactor *
                (Map.is_off_map(loc) ? OFF_MAP_PENALTY_MULT : 1);
        }

        public bool Obstructed(Vector2 loc)
        {
            return ObstructionSources.Contains(loc);
        }
        public bool Obstructed(Vector2 loc, Vector2 goalLoc)
        {
            if (loc == goalLoc)
                return false;
            return Obstructed(loc);
        }

        public IEnumerable<Vector2> AdjacentLocations(Vector2 loc)
        {
            yield return new Vector2(loc.X, loc.Y - 1);
            yield return new Vector2(loc.X, loc.Y + 1);
            yield return new Vector2(loc.X - 1, loc.Y);
            yield return new Vector2(loc.X + 1, loc.Y);
        }

        public bool IsAdjacent(Vector2 loc, Vector2 targetLoc)
        {
            return (loc.X == targetLoc.X && Math.Abs(loc.Y - targetLoc.Y) <= 1) ||
                (loc.Y == targetLoc.Y && Math.Abs(loc.X - targetLoc.X) <= 1);
        }

        public int HeuristicPenalty(Vector2 loc, Vector2 goalLoc, Vector2 prevLoc)
        {
            int heuristic = 0;
            if (Doors.Contains(loc) && TileCost(loc, goalLoc) < 0)
            {
                heuristic = (this.Unit.mov + 1) * this.MoveCostFactor;
            }

            // If fog and AI controlled and the unit can't see this tile
            if (Map.fow && Global.game_state.ai_active && !Map.fow_visibility[this.Unit.team].Contains(loc))
                // Make the tile less desirable for the unit to cross
                heuristic += this.Unit.mov * this.MoveCostFactor;
            return heuristic;
        }

        public int Distance(Vector2 loc, Vector2 targetLoc, bool useEuclideanDistance = false)
        {
            if (!useEuclideanDistance)
                return manhatten_dist(loc, targetLoc);
            else
                return euclidean_dist(loc, targetLoc);
        }
        private int euclidean_dist(Vector2 loc, Vector2 targetLoc)
        {
            return (int)(Math.Sqrt(Math.Pow(loc.X - targetLoc.X, 2) + Math.Pow(loc.Y - targetLoc.Y, 2)) * this.MoveCostFactor);
        }
        private int manhatten_dist(Vector2 loc, Vector2 targetLoc)
        {
            return (int)(Math.Abs(loc.X - targetLoc.X) + Math.Abs(loc.Y - targetLoc.Y)) * this.MoveCostFactor;
        }

        public bool RestrictToMap(Vector2 loc, Vector2 goalLoc, bool restrictToPlayable = true)
        {
            return !InvalidLocation(loc, restrictToPlayable) &&
                !InvalidLocation(goalLoc, restrictToPlayable);
        }

        public bool InvalidLocation(Vector2 loc, bool restrictToPlayable)
        {
            return Map.is_off_map(loc, restrictToPlayable);
        }
        public bool InvalidLocation(Vector2 loc, Vector2 goalLoc, bool restrictToPlayable)
        {
            // If the checked location isn't the target but is off the map, and off the map is not allowed
            if (loc != goalLoc &&
                    InvalidLocation(loc, restrictToPlayable))
                return true;
            return false;
        }
        #endregion

        internal class Builder
        {
            Game_Map Map = null;
            bool IgnoreUnits = false;
            bool ThroughDoors = false;
            bool IgnoreDoors = false;
            Maybe<bool> FullMoveThroughEnemies;
            bool ForceGoalPassable = false;

            public Builder WithIgnoreUnits(bool value)
            {
                IgnoreUnits = value;
                return this;
            }
            public Builder WithThroughDoors(bool value)
            {
                ThroughDoors = value;
                return this;
            }
            public Builder WithIgnoreDoors(bool value)
            {
                IgnoreDoors = value;
                return this;
            }
            public Builder WithFullMoveThroughEnemies(bool value)
            {
                FullMoveThroughEnemies = value;
                return this;
            }
            public Builder WithForceGoalPassable(bool value)
            {
                ForceGoalPassable = value;
                return this;
            }

            public UnitMovementMap Build(
                int unitId)
            {
                var result = new UnitMovementMap(
                    unitId,
                    Map == null ? Global.game_map : Map)
                {
                    IgnoreUnits = IgnoreUnits,
                    ThroughDoors = ThroughDoors,
                    IgnoreDoors = IgnoreDoors,
                    FullMoveThroughEnemies = FullMoveThroughEnemies.IsSomething,
                    ForceGoalPassable = ForceGoalPassable
                };
                result.initialize_unit_locs();
                return result;
            }
        }
    }
}
