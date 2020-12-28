using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TactileLibrary.Pathfinding.Old
{
    public class Pathfinder<T> where T : IDistanceMeasurable<T>
    {
        private List<T> Objects;
        private PathfindingNode[] Nodes;

        static Random rand = new Random();

        public Pathfinder(IEnumerable<T> objects, PathfindingNode[] nodes)
        {
            if (objects.Count() != nodes.Length)
                throw new ArgumentException(
                    "Pathfinding object and node lists\nneed to have the same length");

            Objects = objects.ToList();
            Nodes = nodes;
        }

        #region Pathfinding
        public List<Tuple<int, T>> get_route(T target_loc, T loc, int mov = -1)
        {
            //int start_index = Objects.IndexOf(loc); //@Debug
            //int end_index = Objects.IndexOf(target_loc);
            int start_index = Objects.FindIndex(x => x.SameLocation(loc));
            int end_index = Objects.FindIndex(x => x.SameLocation(target_loc));

            if (start_index == -1 || end_index == -1)
                return null;

            //Prepare pathfinding variables
            OpenList open_list = new OpenList();
            ClosedListRoute closed_list = new ClosedListRoute();

            int temp_parent = -1;

            int temp_index = -1;
            int temp_f = 0;
            int temp_g = 0;
            int temp_h = 0;
            bool temp_accessible = true;

            bool route_found = false;

            // Start pathfinding
            temp_g = 0;
            temp_h = distance(start_index, end_index);
            temp_f = temp_g + temp_h;
            open_list.add_item(start_index, temp_parent, temp_f, temp_g, temp_accessible);
            for (; open_list.size > 0; )
            {
                OpenItem lowest_f_item = open_list.get_lowest_f_item();
                temp_index = lowest_f_item.Index;
                temp_parent = lowest_f_item.Parent;
                temp_f = lowest_f_item.Fcost;
                temp_g = lowest_f_item.Gcost;
                temp_accessible = lowest_f_item.Accessible;

                temp_parent = closed_list.add_item(temp_index, temp_parent, temp_f, temp_g, temp_accessible);
                open_list.remove_open_item();
                if (temp_index == end_index)
                {
                    route_found = true;
                    break;
                }
                else
                {
                    foreach(int test_index in Nodes[temp_index].Adjacent)
                    {
                        // If the checked location isn't the target but is off the map, and off the map is not allowed
                        if (test_index != end_index && (test_index >= Objects.Count || test_index < 0))
                            continue;
                        // If the location is already on the closed list
                        if (closed_list.search(test_index) > -1)
                            continue;
                        check_tile(test_index, temp_parent, mov, end_index, open_list, closed_list);
                    }
                }
            }
            if (route_found)
            {
                return closed_list.get_route(temp_parent)
                    .Select(x => new Tuple<int, T>(x, Objects[x]))
                    .Reverse()
                    .ToList();
            }
            return null;
        }

        /*

        public List<T> get_reverse_route(T loc, int mov, T target_loc)
        {
            //Prepare pathfinding variables
            OpenList open_list = new OpenList();
            ClosedListRoute closed_list = new ClosedListRoute();

            int temp_parent = -1;

            Vector2 temp_loc = Vector2.Zero;
            int temp_f = 0;
            int temp_g = 0;
            int temp_h = 0;
            bool temp_accessible = true;

            Vector2 test_loc;
            bool route_found = false;

            // Start pathfinding
            //temp_g = 0; //Debug
            temp_g = pathfinding_terrain_cost(unit, loc);
            temp_h = manhatten_dist(loc, target_loc);
            temp_f = temp_g + temp_h;
            open_list.add_item(loc, temp_parent, temp_f, temp_g, temp_accessible);
            for (; ; )
            {
                if (open_list.size <= 0)
                    break;
                OpenItem lowest_f_item = open_list.get_lowest_f_item();
                temp_loc = lowest_f_item.Loc;
                temp_parent = lowest_f_item.Parent;
                temp_f = lowest_f_item.Fcost;
                temp_g = lowest_f_item.Gcost;
                temp_accessible = lowest_f_item.Accessible;

                temp_parent = closed_list.add_item(temp_loc, temp_parent, temp_f, temp_g, temp_accessible);
                open_list.remove_open_item();
#if DEBUG
                if (temp_loc == target_loc)
                {
                    throw new IndexOutOfRangeException("Target location somehow got added to closed list");
                }
#endif
                if ((temp_loc.X == target_loc.X && Math.Abs(temp_loc.Y - target_loc.Y) <= 1) ||
                    (temp_loc.Y == target_loc.Y && Math.Abs(temp_loc.X - target_loc.X) <= 1))
                {
                    route_found = true;
                    break;
                }
                else
                {
                    for (int i = 0; i < check_loc.Length; i++)
                    {
                        test_loc = temp_loc + check_loc[i];
#if DEBUG
                        if (Global.game_map.is_off_map(test_loc, Restrict_To_Map))
                        {
                            int test = 0;
                        }
#endif
                        // If the checked location isn't the target but is off the map, and off the map is not allowed
                        if (test_loc != target_loc && Global.game_map.is_off_map(test_loc, Restrict_To_Map))
                            continue;
                        // If the location is already on the closed list
                        if (closed_list.search(test_loc) > -1)
                            continue;
                        check_tile(unit, test_loc, temp_parent, mov, target_loc, open_list, closed_list);
                    }
                }
            }
            if (route_found)
            {
                return closed_list.get_reverse_route(temp_parent, target_loc);
            }
            return null;
        }

        public HashSet<T> get_range(T target_loc, int mov, int id, T loc)
        {
            if (Global.game_map.width() == 0)
                return new HashSet<Vector2>();
            //Prepare pathfinding variables
            OpenList open_list = new OpenList();
            ClosedListRoute closed_list = new ClosedListRoute();

            int temp_parent = -1;

            Vector2 temp_loc = Vector2.Zero;
            int temp_f = 0;
            int temp_g = 0;
            int temp_h = 0;
            bool temp_accessible = true;

            Vector2 test_loc;

            // Start pathfinding
            temp_g = 0;
            temp_h = manhatten_dist(loc, target_loc);
            temp_f = temp_g + temp_h;
            open_list.add_item(loc, temp_parent, temp_f, temp_g, temp_accessible);
            for (; ; )
            {
                if (open_list.size <= 0)
                    break;
                OpenItem lowest_f_item = open_list.get_lowest_f_item();
                temp_loc = lowest_f_item.Loc;
                temp_parent = lowest_f_item.Parent;
                temp_f = lowest_f_item.Fcost;
                temp_g = lowest_f_item.Gcost;
                temp_accessible = lowest_f_item.Accessible;

                temp_parent = closed_list.add_item(temp_loc, temp_parent, temp_f, temp_g, temp_accessible);
                open_list.remove_open_item();

                bool reverse = (rand.Next(2) == 0);
                reverse = false;
                for (int i = 0; i < check_loc.Length; i++)
                {
                    test_loc = temp_loc + check_loc[reverse ? 3 - i : i];
                    if (Global.game_map.is_off_map(test_loc))
                        continue;
                    if (closed_list.search(test_loc) > -1)
                        continue;
                    check_tile(unit, test_loc, temp_parent, mov, target_loc, open_list, closed_list);
                }
            }

            return closed_list.get_range();
        }

        public Maybe<int> get_distance(T target_loc, int mov, bool through_doors, Vector2 loc, bool ignore_doors = false)
        {
            //Prepare pathfinding variables
            OpenList open_list = new OpenList();
            ClosedListRoute closed_list = new ClosedListRoute();

            int temp_parent = -1;

            Vector2 temp_loc = Vector2.Zero;
            int temp_f = 0;
            int temp_g = 0;
            int temp_h = 0;
            bool temp_accessible = true;

            Vector2 test_loc;
            bool route_found = false;

            // Start pathfinding
            temp_g = 0;
            temp_h = manhatten_dist(loc, target_loc);
            temp_f = temp_g + temp_h;
            open_list.add_item(loc, temp_parent, temp_f, temp_g, temp_accessible);
            for (; ; )
            {
                if (open_list.size <= 0)
                    break;
                OpenItem lowest_f_item = open_list.get_lowest_f_item();
                temp_loc = lowest_f_item.Loc;
                temp_parent = lowest_f_item.Parent;
                temp_f = lowest_f_item.Fcost;
                temp_g = lowest_f_item.Gcost;
                temp_accessible = lowest_f_item.Accessible;

                temp_parent = closed_list.add_item(temp_loc, temp_parent, temp_f, temp_g, temp_accessible);
                open_list.remove_open_item();
                if (temp_loc == target_loc)
                {
                    route_found = true;
                    break;
                }
                else
                {
                    bool reverse = (rand.Next(2) == 0);
                    reverse = false;
                    for (int i = 0; i < check_loc.Length; i++)
                    {
                        test_loc = temp_loc + check_loc[reverse ? 3 - i : i];
                        if (Global.game_map.is_off_map(test_loc, Restrict_To_Map))
                            continue;
                        if (closed_list.search(test_loc) > -1)
                            continue;
                        check_tile(unit, test_loc, temp_parent, mov, target_loc, open_list, closed_list);
                    }
                }
            }
            if (route_found)
            {
                return closed_list.get_g(temp_parent) / 10;
            }
            return new Maybe<int>();
        }

        */

        private void check_tile(
            int current_index, int parent, int mov, int end_index,
            OpenList open_list, ClosedListRoute closed_list, bool dijkstras = false)
        {
            // return if terrain type of this tile doesn't have stats //Yeti
            int move_cost = pathfinding_terrain_cost(current_index);
            bool pass = passable(current_index);
            int h = 0;
            if (pass)
            {
                int g = move_cost + closed_list.get_g(parent);
                if (mov < 0 || g <= mov * 10)
                {
                    h += node_h_cost(current_index, end_index, dijkstras);
                    int f = g + h;
                    int on_list = open_list.search(current_index);
                    if (on_list > -1)
                    {
                        open_list.repoint(on_list, parent, f, g);
                    }
                    else
                    {
                        open_list.add_item(current_index, parent, f, g, pass);
                    }
                }
            }
        }

        private int node_h_cost(int current_index, int end_index, bool dijkstras)
        {
            int h = (dijkstras ? 0 : distance(current_index, end_index));
            
            // If fog and AI controlled and the unit can't see this tile
            //if (Global.game_map.fow && Global.game_state.ai_active && !Global.game_map.fow_visibility[unit.team].Contains(current_index)) //Debug
                // Make the tile less desirable for the unit to cross
            //    h += unit.mov * 10;

            return h;
        }

        public bool passable(int index)
        {
            return terrain_cost(index) >= 0;
        }

        protected int pathfinding_terrain_cost(int index)
        {
            return Nodes[index].MoveCost * 10;
        }
        protected int terrain_cost(int index)
        {
            return Nodes[index].MoveCost * 10;
        }
        #endregion

        private int distance(int start_index, int end_index)
        {
            return Objects[start_index].Distance(Objects[end_index]) * 10;
        }
    }
}
