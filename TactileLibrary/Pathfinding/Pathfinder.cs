using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TactileLibrary;

namespace TactileLibrary.Pathfinding
{
    public class Pathfinder<T> where T : IEquatable<T>
    {
        private IMovementMap<T> Map;
        private Func<bool> ReverseRouteWiggleChance;
        internal int RouteDistance { get; private set; }

        static Random rand = new Random();

        public Pathfinder(IMovementMap<T> map, Func<bool> wiggleChance = null)
        {
            Map = map;
            ReverseRouteWiggleChance = wiggleChance;
        }

        public List<T> get_route(T loc, T target_loc, int mov)
        {
            bool restrict_to_map = Map.RestrictToMap(loc, target_loc);
            bool use_euclidean_distance = false; //@Debug: not really beneficial even with -1 mov

            //Prepare pathfinding variables
            OpenList<T> open_list = new OpenList<T>();
            ClosedListRoute<T> closed_list = new ClosedListRoute<T>();

            // Start pathfinding
            bool route_found = false;
            open_list.add_initial_item(
                loc, Map.Distance(loc, target_loc, use_euclidean_distance));
            int last_added = -1;
            for (; ; )
            {
                if (open_list.size <= 0)
                    break;

                OpenItem<T> lowest_f_item = open_list.remove_open_item();
                last_added = closed_list.add_item(lowest_f_item);
                T current_loc = lowest_f_item.Loc;

                if (current_loc.Equals(target_loc))
                {
                    route_found = true;
                    break;
                }
                else
                {
                    IEnumerable<T> check_locs = Map.AdjacentLocations(current_loc);
                    foreach (T test_loc in check_locs)
                    {
                        // If the location is already on the closed list
                        if (closed_list.already_added(test_loc))
                            continue;
                        if (Map.InvalidLocation(test_loc, target_loc, restrict_to_map))
                            continue;
                        check_tile(test_loc, last_added, mov, target_loc,
                            open_list, closed_list, use_euclidean_distance: use_euclidean_distance);
                    }
                }
            }
            RouteDistance = 0;
            if (route_found)
            {
                RouteDistance = closed_list.get_g(last_added) / Map.MoveCostFactor;
                return closed_list.get_route(last_added);
            }
            return null;
        }

        public List<T> get_reverse_route(T loc, T target_loc, int mov)
        {
            bool restrict_to_map = Map.RestrictToMap(loc, target_loc);

            //Prepare pathfinding variables
            OpenList<T> open_list = new OpenList<T>();
            ClosedListRoute<T> closed_list = new ClosedListRoute<T>();

            // Start pathfinding
            bool route_found = false;
            open_list.add_initial_item(loc, Map.TileCost(loc, target_loc), Map.Distance(loc, target_loc));
            int last_added = -1;
            for (; ; )
            {
                if (open_list.size <= 0)
                    break;

                if (ReverseRouteWiggleChance != null)
                    open_list.rng_lowest_f_id(ReverseRouteWiggleChance);
                OpenItem<T> lowest_f_item = open_list.remove_open_item();
                last_added = closed_list.add_item(lowest_f_item);
                T current_loc = lowest_f_item.Loc;

#if DEBUG
                if (current_loc.Equals(target_loc))
                {
                    throw new IndexOutOfRangeException("Target location somehow got added to closed list");
                }
#endif
                if (Map.IsAdjacent(current_loc, target_loc))
                {
                    route_found = true;
                    break;
                }
                else
                {
                    IEnumerable<T> check_locs = Map.AdjacentLocations(current_loc);
                    foreach (T test_loc in check_locs)
                    {
                        // If the location is already on the closed list
                        if (closed_list.already_added(test_loc))
                            continue;
                        if (Map.InvalidLocation(test_loc, target_loc, restrict_to_map))
                            continue;
                        check_tile(test_loc, last_added, mov, target_loc,
                            open_list, closed_list);
                    }
                }
            }
            RouteDistance = 0;
            if (route_found)
            {
                RouteDistance = closed_list.get_g(last_added) / Map.MoveCostFactor;
                return closed_list.get_reverse_route(last_added, target_loc);
            }
            return null;
        }

        public HashSet<T> get_range(T loc, T target_loc, int mov)
        {
            /* //Debug
            if (Global.game_map.width == 0)
                return new HashSet<Vector2>();*/

            ClosedListRoute<T> closed_list = GetRange(loc, target_loc, mov, false);

            return closed_list.get_range();
        }
        private ClosedListRoute<T> GetRange(T loc, T target_loc, int mov, bool dijkstras)
        {
            //Prepare pathfinding variables
            OpenList<T> open_list = new OpenList<T>();
            ClosedListRoute<T> closed_list = new ClosedListRoute<T>();

            // Start pathfinding
            open_list.add_initial_item(
                loc, Map.Distance(loc, target_loc));
            int last_added = -1;
            for (;;)
            {
                if (open_list.size <= 0)
                    break;

                OpenItem<T> lowest_f_item = open_list.remove_open_item();
                last_added = closed_list.add_item(lowest_f_item);
                T current_loc = lowest_f_item.Loc;

                bool reverse = (rand.Next(2) == 0);
                reverse = false;
                IEnumerable<T> check_locs = Map.AdjacentLocations(current_loc);
                if (reverse)
                    check_locs = check_locs.Reverse();
                foreach (T test_loc in check_locs)
                {
                    // If the location is already on the closed list
                    if (closed_list.already_added(test_loc))
                        continue;
                    if (Map.InvalidLocation(test_loc, target_loc, true))
                        continue;
                    check_tile(test_loc, last_added, mov, target_loc,
                        open_list, closed_list,
                        dijkstras: dijkstras);
                }
            }

            return closed_list;
        }

        public Maybe<int> get_distance(T loc, T target_loc, int mov)
        {
            bool restrict_to_map = Map.RestrictToMap(loc, target_loc, false);
            //Prepare pathfinding variables
            OpenList<T> open_list = new OpenList<T>();
            ClosedListRoute<T> closed_list = new ClosedListRoute<T>();

            // Start pathfinding
            bool route_found = false;
            open_list.add_initial_item(
                loc, Map.Distance(loc, target_loc, false));
            int last_added = -1;
            for (; ; )
            {
                if (open_list.size <= 0)
                    break;
                // Most time is spent adding and removing items from the open list,
                // and checking if a tile is Passable //Profiler
                OpenItem<T> lowest_f_item = open_list.remove_open_item();
                last_added = closed_list.add_item(lowest_f_item);
                T current_loc = lowest_f_item.Loc;

                if (current_loc.Equals(target_loc))
                {
                    route_found = true;
                    break;
                }
                else
                {
                    bool reverse = (rand.Next(2) == 0);
                    reverse = false;
                    IEnumerable<T> check_locs = Map.AdjacentLocations(current_loc);
                    if (reverse)
                        check_locs = check_locs.Reverse();
                    foreach(T test_loc in check_locs)
                    {
                        // If the location is already on the closed list
                        if (closed_list.already_added(test_loc))
                            continue;
                        if (Map.InvalidLocation(test_loc, restrict_to_map))
                            continue;
                        check_tile(test_loc, last_added, mov, target_loc,
                            open_list, closed_list);
                    }
                }
            }
            RouteDistance = 0;
            if (route_found)
            {
                return closed_list.get_g(last_added) / Map.MoveCostFactor;
            }
            return new Maybe<int>();
        }

        public Dictionary<T, int> DistanceToAll(T loc, int mov = -1)
        {
            ClosedListRoute<T> closed_list = GetRange(loc, loc, mov, false);// true);
            
            return closed_list
                .GetMoveCosts()
                .ToDictionary(p => p.Key, p => p.Value / Map.MoveCostFactor);
        }

        private void check_tile(
            T loc, int parent, int mov, T target_loc,
            OpenList<T> open_list, ClosedListRoute<T> closed_list, bool dijkstras = false, bool use_euclidean_distance = false)
        {
            T prevLoc = closed_list.loc(parent);

            var tileData = Map.GetTileData(loc, target_loc);

            if (tileData.Passable)
            {
                int g = tileData.TileCost + closed_list.get_g(parent);
                // If using a limited move score
                if (mov >= 0)
                {
                    // Return if this tile is too expensive to move into
                    if (g > mov * Map.MoveCostFactor)
                        return;
                    // If obstructed, g is set to the total move score
                    else if (tileData.Obstructs)
                        g = Math.Max(g, mov * Map.MoveCostFactor);
                }

                int heuristic = Map.HeuristicPenalty(loc, target_loc, prevLoc);
                if (!dijkstras)
                    heuristic += Map.Distance(loc, target_loc, use_euclidean_distance);

                int f = g + heuristic;
                int on_list = open_list.search(loc);
                if (on_list > -1)
                {
                    open_list.repoint(on_list, parent, f, g);
                }
                else
                {
                    open_list.add_item(loc, parent, f, g, tileData.Passable);
                }
            }
        }
        
        /// <summary>
        /// Returns the total move cost of a move route.
        /// The final element of the list is assumed to be the start point of
        /// the route, and will not be counted.
        /// </summary>
        public int MoveCost(List<T> route)
        {
            int cost = 0;
            if (!route.Any())
                return cost;

            T goalLoc = route.Last();
            for (int i = 0; i < route.Count - 1; i++)
            {
                T targetLoc = route[i];
                cost += Map.TileCost(targetLoc, goalLoc);
            }

            return cost / Map.MoveCostFactor;
        }
    }
}
