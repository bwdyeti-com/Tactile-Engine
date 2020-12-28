using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TactileLibrary
{
    using NodeTargets = Dictionary<Vector2,
        Dictionary<CardinalDirections, Tuple<Vector2, float>>>;
    public enum CardinalDirections { None = 0, Down = 2, Left = 4, Right = 6, Up = 8, All = 10 }
    public class NodeDestinationMap
    {
        Dictionary<Vector2, Dictionary<CardinalDirections, Vector2>> Map;

#if DEBUG
        public HashSet<Vector2> UnmatchedNodes = new HashSet<Vector2>();
#endif

        private NodeDestinationMap() { }

        public static NodeDestinationMap construct(
            IEnumerable<Vector2> nodes, float angleMultiplier = 1f)
        {
            return construct(nodes, new List<CardinalDirections>
                {
                    CardinalDirections.Down, CardinalDirections.Left,
                    CardinalDirections.Right, CardinalDirections.Up
                }, angleMultiplier);
        }
        public static NodeDestinationMap construct(
            IEnumerable<Vector2> nodes,
            IEnumerable<CardinalDirections> tangentDirections,
            float angleMultiplier = 1f)
        {
            var result = new NodeDestinationMap();
            result.generate_map(nodes, tangentDirections, angleMultiplier);
            return result;
        }

        public Maybe<Vector2> destination(Vector2 loc, CardinalDirections dir)
        {
            if (!Map.ContainsKey(loc))
                return Maybe<Vector2>.Nothing;

            if (Map[loc].ContainsKey(dir))
                return Map[loc][dir];
            return Maybe<Vector2>.Nothing;
        }

        private void generate_map(
            IEnumerable<Vector2> nodes,
            IEnumerable<CardinalDirections> tangentDirections,
            float angleMultiplier = 1f)
        {
            if (nodes.Count() != nodes.Distinct().Count())
            {
                var duplicate_loc = nodes
                    .GroupBy(x => x)
                    .OrderByDescending(x => x.Count())
                    .First()
                    .Key;
                int count = nodes.Count(x => x == duplicate_loc);
                throw new ArgumentException(string.Format(
                    "Node locations must all be unique\n" +
                    "{1} instances of {0}",
                    duplicate_loc, count));
            }

            // Each of these are half the actual range in radians, because the search checks in both directions
            const float HALF_CIRCLE_RANGE = MathHelper.PiOver2;
            const float SEVEN_SIXTEENTHS_CIRCLE_RANGE = MathHelper.Pi * 7 / 16f;
            const float TWO_FIFTHS_CIRCLE_RANGE = MathHelper.TwoPi / 5f;
            const float THREE_EIGHTHS_CIRCLE_RANGE = MathHelper.PiOver4 * 3 / 2f;
            const float THIRD_CIRCLE_RANGE = MathHelper.PiOver2 * 2 / 3f;
            const float QUARTER_CIRCLE_RANGE = MathHelper.PiOver4;
            const float FIFTH_CIRCLE_RANGE = MathHelper.Pi / 5f;
            const float EIGHTH_CIRCLE_RANGE = MathHelper.PiOver4 / 2;

            Map = nodes.ToDictionary(x => x,
                x => new Dictionary<CardinalDirections, Vector2>());

            var true_best_fit = find_best_fit(
                nodes, QUARTER_CIRCLE_RANGE * angleMultiplier, 2f);

            // First find the best fits for each point in each direction
            var best_fit = find_best_fit(
                nodes, FIFTH_CIRCLE_RANGE * angleMultiplier, 3);
            // For each pair where the best fits match up, add those pairs
            match_best_fit(nodes, best_fit);

            // Repeat with wider search angle
            var wider_best_fit = find_best_fit(
                nodes, QUARTER_CIRCLE_RANGE * angleMultiplier, 3, true_best_fit);
            match_best_fit(nodes, wider_best_fit);
            // Then for best fits that are unpaired, if the destination is unpaired, pair them
            match_unpaired_best_fit(true_best_fit);

#if DEBUG
            // Find nodes with no connections and insert them onto existing lines as best as possible
            UnmatchedNodes.Clear();
            UnmatchedNodes.UnionWith(Map.Where(x => !x.Value.Any()).Select(x => x.Key));
#endif
            
            HashSet<Vector2> completely_unmatched = new HashSet<Vector2>();
            while (Map
                .Where(x => !completely_unmatched.Contains(x.Key))
                .Any(x => !x.Value.Any()))
            {
                Vector2 unmatched_node = Map.First(x => !x.Value.Any()).Key;
                // Try to find a line to insert the node on
                if (!match_unmatched_node(unmatched_node))
                {
                    completely_unmatched.Add(unmatched_node);
                    continue;
                }
            }

            // This should be modified to only connect if no closer nodes have shallower angles, so lines can't intersect //Yeti
            // Repeat with narrow search angle
            // This one lets nodes make long distance cardinal direction connections
            // Often skipping past other nodes in the way
            // Somewhat necessary when dealing with random node placement
            var narrow_best_fit = find_best_fit(
                nodes, EIGHTH_CIRCLE_RANGE * angleMultiplier, 8);
            match_best_fit(nodes, narrow_best_fit);

            // Connect parallel segments
            // Isn't working how I want, since it doesn't reduce complexity
            //while (match_parallels(Directions.Right,
            //    QUARTER_CIRCLE_RANGE * angleMultiplier, 2)) { } //Debug
            //while (match_parallels(Directions.Down,
            //    QUARTER_CIRCLE_RANGE * angleMultiplier, 2)) { }

            var neighbor_tangents = find_neighbor_tangent(
                nodes, QUARTER_CIRCLE_RANGE * angleMultiplier,
                EIGHTH_CIRCLE_RANGE * angleMultiplier, 4);
            // Same as above, shouldn't intersect //Yeti
            // Repeat with widest search angle
            var widest_best_fit = find_best_fit(
                nodes, HALF_CIRCLE_RANGE * angleMultiplier, 4, neighbor_tangents);
            match_best_fit(nodes, widest_best_fit);
            match_unpaired_best_fit(widest_best_fit);

            // Add one way connections in places where an input would otherwise do nothing
            // For each node without any destination in a direction, check its neighbors in tangential directions
            neighbor_tangents = find_neighbor_tangent(
                nodes, tangentDirections.Intersect(new List<CardinalDirections> {
                    CardinalDirections.Down,
                    CardinalDirections.Up }),
                SEVEN_SIXTEENTHS_CIRCLE_RANGE * angleMultiplier,
                EIGHTH_CIRCLE_RANGE * angleMultiplier, 1, true);
            add_neighbor_tangets(neighbor_tangents);
            
            neighbor_tangents = find_neighbor_tangent(
                nodes, tangentDirections.Intersect(new List<CardinalDirections> {
                    CardinalDirections.Left,
                    CardinalDirections.Right }),
                SEVEN_SIXTEENTHS_CIRCLE_RANGE * angleMultiplier,
                EIGHTH_CIRCLE_RANGE * angleMultiplier, 1, true);
            add_neighbor_tangets(neighbor_tangents);
        }

        /// <param name="nodes">Set of nodes to test</param>
        /// <param name="max_angle">Maximum angle off a cardinal direction to look for neighbors</param>
        /// <param name="angle_factor">Factor used to sort neighbors. The higher the factor, the higher the preference for neighbors with a lower angle.</param>
        /// <param name="true_best_fit">Pre-calculated set of best fits, to remove neighbors that are too far off base</param>
        private NodeTargets find_best_fit(
            IEnumerable<Vector2> nodes,
            float max_angle,
            float angle_factor,
            NodeTargets true_best_fit = null)
        {
            var result = nodes.ToDictionary(node => node, node =>
            {
                var map = new Dictionary<CardinalDirections, Tuple<Vector2, float>>();
                foreach (CardinalDirections dir in new List<CardinalDirections> {
                    CardinalDirections.Down,
                    CardinalDirections.Left,
                    CardinalDirections.Right,
                    CardinalDirections.Up })
                {
                    var destinations = possible_nodes(node, dir, max_angle);
                    // Remove nodes that are further away than the true best fit and also further in tangential directions
                    if (true_best_fit != null)
                    {
                        destinations = destinations.Where(dest =>
                        {
                            if (!true_best_fit[node].ContainsKey(dir))
                                return true;
                            // If the distance to the target is more than four times the distance to the best target
                            if (distance(node, dest, vertical_direction(dir)) >
                                    distance(true_best_fit[node][dir].Item1, node,
                                        vertical_direction(dir)) * 4 * 4)
                                return false;
                            return valid_tangent(node, dest, true_best_fit[node][dir].Item1, dir);
                        });
                    }
                    if (destinations.Any())
                    {
                        Vector2 best_dest = destinations
                            .OrderBy(dest => destination_fitness(node, dest, dir, max_angle, angle_factor))
                            .First();
                        float fitness = destination_fitness(node, best_dest, dir, max_angle, angle_factor);
                        map[dir] = new Tuple<Vector2, float>(best_dest, fitness);
                    }
                }
                return map;
            });
            
            return result
                .Where(x => x.Value.Any())
                .ToDictionary(p => p.Key, p => p.Value);
        }
        private void match_best_fit(IEnumerable<Vector2> nodes, NodeTargets best_fit)
        {
            foreach (Vector2 node in nodes)
                if (best_fit.ContainsKey(node))
                    foreach (CardinalDirections dir in new List<CardinalDirections> {
                        CardinalDirections.Down,
                        CardinalDirections.Left,
                        CardinalDirections.Right,
                        CardinalDirections.Up })
                    {
                        // If there's anything in this direction
                        if (best_fit[node].ContainsKey(dir))
                        {
                            Vector2 dest = best_fit[node][dir].Item1;
                            CardinalDirections opp_dir =
                                (CardinalDirections)(CardinalDirections.All - dir);
                            // If the destination also points back to this node
                            if (best_fit.ContainsKey(dest) &&
                                best_fit[dest].ContainsKey(opp_dir) &&
                                best_fit[dest][opp_dir].Item1 == node)
                            {
                                add_destination(node, dest, dir);

                                best_fit[node].Remove(dir);
                                best_fit[dest].Remove(opp_dir);
                            }
                        }
                    }
        }
        private void match_unpaired_best_fit(NodeTargets best_fit)
        {
            var ordered_best_pairs = best_fit.SelectMany(x => x.Value
                .Select(y => new Tuple<Vector2, CardinalDirections, Vector2, float>(x.Key, y.Key, y.Value.Item1, y.Value.Item2)))
                .OrderBy(x => x.Item4)
                .ToList();
            foreach (var pair in ordered_best_pairs)
            {
                Vector2 node = pair.Item1;
                CardinalDirections dir = pair.Item2;
                Vector2 dest = pair.Item3;
                CardinalDirections opp_dir = (CardinalDirections)(CardinalDirections.All - dir);
                // If the route is possible
                if (!Map[node].ContainsKey(dir) && !Map[dest].ContainsKey(opp_dir))
                {
                    add_destination(node, dest, dir);

                    best_fit[node].Remove(dir);
                    if (best_fit.ContainsKey(dest))
                        best_fit[dest].Remove(opp_dir);
                }
            }
        }
        private bool match_unmatched_node(Vector2 unmatched_node)
        {
            // The biggest problem, and the biggest edge case worth solving
            // is a square of points around a central point
            // This is what match_unmatched_node() does, theoretically?
            // All the square points connect to each other, and none connect to the center
            // So after doing quarter circle connections, perhaps run a pass that will
            // search along line for unconnected points in that axis that could be
            // inserted on the line
            //  1-----4
            //  |     |
            //  |     |
            //  |     |
            //  |  5  |
            //  | / \ |
            //  2=====3
            //  Detect 5 along the === line,
            // and insert it as the horizontal connections for 2 and 3
            // The other lines would be tested, as would all others around it
            // and 2===3 would be used as the best fit


            List<Tuple<Vector2, Vector2, CardinalDirections, float, float>> lines_to_intersect = new List<Tuple<Vector2, Vector2, CardinalDirections, float, float>>();
            // Get all current paths where the endpoints are on opposite sides of the unmatched node
            foreach (var pair in Map)
            {
                // Get the fitness of the two line segments that would occur if those endpoints both pointed at the unmatched node
                if (pair.Value.ContainsKey(CardinalDirections.Down) &&
                    unmatched_node.Y > pair.Key.Y &&
                    unmatched_node.Y < pair.Value[CardinalDirections.Down].Y)
                {
                    lines_to_intersect.Add(new Tuple<Vector2, Vector2, CardinalDirections, float, float>(
                        pair.Key, pair.Value[CardinalDirections.Down],
                        CardinalDirections.Up,
                        destination_fitness(unmatched_node, pair.Key, CardinalDirections.Up, MathHelper.Pi / 4, 4),
                        destination_fitness(unmatched_node, pair.Value[CardinalDirections.Down], CardinalDirections.Down, MathHelper.Pi / 4, 4)));
                }
                if (pair.Value.ContainsKey(CardinalDirections.Right) &&
                    unmatched_node.X > pair.Key.X &&
                    unmatched_node.X < pair.Value[CardinalDirections.Right].X)
                {
                    lines_to_intersect.Add(new Tuple<Vector2, Vector2, CardinalDirections, float, float>(
                        pair.Key, pair.Value[CardinalDirections.Right],
                        CardinalDirections.Left,
                        destination_fitness(unmatched_node, pair.Key, CardinalDirections.Left, MathHelper.Pi / 4, 4),
                        destination_fitness(unmatched_node, pair.Value[CardinalDirections.Right], CardinalDirections.Right, MathHelper.Pi / 4, 4)));
                }
            }
            if (lines_to_intersect.Any())
            {
                // Select the best
                var intersect = lines_to_intersect.OrderBy(x => x.Item4 * x.Item5).First();
                insert_destination(unmatched_node, intersect.Item1, intersect.Item2, intersect.Item3);
                return true;
            }
            else
                return false;
        }

        private bool match_parallels(CardinalDirections dir, float max_angle, float angle_factor)
        {
            // Get all lines going in the same direction
            var segments = Map
                .SelectMany(x => x.Value
                    .Where(y => y.Key == dir && Map[y.Value].ContainsKey((CardinalDirections)(CardinalDirections.All - dir)) && Map[y.Value][(CardinalDirections)(CardinalDirections.All - dir)] == x.Key)
                    .Select(y => new Tuple<Vector2, Vector2>(x.Key, y.Value))).ToList();
            for (int i = 0; i < segments.Count; i++)
                for (int j = i + 1; j < segments.Count; j++)
                {
                    // Find unconnected line segment pairs
                    if (segments[i].Item1 == segments[j].Item1 || segments[i].Item1 == segments[j].Item2 ||
                            segments[i].Item2 == segments[j].Item1 || segments[i].Item2 == segments[j].Item2)
                        continue;
                    // Where one endpoint of each is within the range of the other segment
                    if ((point_within_segment(segments[i].Item1, segments[j], dir) || point_within_segment(segments[i].Item2, segments[j], dir)) &&
                        (point_within_segment(segments[j].Item1, segments[i], dir) || point_within_segment(segments[j].Item2, segments[i], dir)))
                    {
                        Vector2 a1, a2, b1, b2;
                        arrange_parallel_segments(out a1, out a2, out b1, out b2, segments[i], segments[j], dir);
                        // They also have to be not connected in the other direction
                        if (segments_connected(a1, a2, b1, b2))
                            continue;
                        // Determine if connecting the AB CD segment as AC and BD would significantly decrease fitness
                        float a1a2 = destination_fitness(a1, a2, dir, max_angle, angle_factor);
                        float a1b1 = destination_fitness(a1, b1, dir, max_angle, angle_factor);
                        if (a1b1 > a1a2)
                            continue;
                        float a2b2 = destination_fitness(a1, b2, dir, max_angle, angle_factor);
                        float b1b2 = destination_fitness(b1, b2, dir, max_angle, angle_factor);
                        if (a2b2 > b1b2)
                            continue;
                        // If not connect ACBD
                        Map[a1].Remove(dir);
                        Map[a2].Remove((CardinalDirections)(CardinalDirections.All - dir));
                        Map[a2].Remove(dir);
                        Map[b1].Remove(dir);
                        Map[b1].Remove((CardinalDirections)(CardinalDirections.All - dir));
                        Map[b2].Remove((CardinalDirections)(CardinalDirections.All - dir));

                        add_destination(a1, b1, dir);
                        add_destination(b1, a2, dir);
                        add_destination(a2, b2, dir);
                        return true;
                    }
                }
            return false;
        }
        private bool point_within_segment(Vector2 loc, Tuple<Vector2, Vector2> segment, CardinalDirections dir)
        {
            switch (dir)
            {
                case CardinalDirections.Down:
                case CardinalDirections.Up:
                    return loc.Y > Math.Min(segment.Item1.Y, segment.Item2.Y) &&
                        loc.Y < Math.Max(segment.Item1.Y, segment.Item2.Y);
                case CardinalDirections.Left:
                case CardinalDirections.Right:
                    return loc.X > Math.Min(segment.Item1.X, segment.Item2.X) &&
                        loc.X < Math.Max(segment.Item1.X, segment.Item2.X);
            }
            throw new ArgumentException();
        }
        private bool arrange_parallel_segments(out Vector2 a1, out Vector2 a2, out Vector2 b1, out Vector2 b2,
            Tuple<Vector2, Vector2> segment1, Tuple<Vector2, Vector2> segment2, CardinalDirections dir)
        {
            a1 = segment1.Item1;
            a2 = segment1.Item2;
            b1 = segment2.Item1;
            b2 = segment2.Item2;
            switch (dir)
            {
                case CardinalDirections.Down:
                case CardinalDirections.Up:
                    // Ensure a1 is above a2
                    if (a2.Y < a1.Y)
                        swap(ref a1, ref a2);
                    // Ensure b1 is above b2
                    if (b2.Y < b1.Y)
                        swap(ref b1, ref b2);
                    // Ensure a segment is above b segment
                    if (b1.Y < a1.Y)
                    {
                        swap(ref a1, ref b1);
                        swap(ref a2, ref b2);
                    }
                    break;
                case CardinalDirections.Left:
                case CardinalDirections.Right:
                    // Ensure a1 is left of a2
                    if (a2.X < a1.X)
                        swap(ref a1, ref a2);
                    // Ensure b1 is left of b2
                    if (b2.X < b1.X)
                        swap(ref b1, ref b2);
                    // Ensure a segment is left of b segment
                    if (b1.X < a1.X)
                    {
                        swap(ref a1, ref b1);
                        swap(ref a2, ref b2);
                    }
                    break;
                default:
                    throw new ArgumentException();
            }
            return true;
        }
        private bool segments_connected(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            return Map[a1].Any(x => x.Value == b1 || x.Value == b2) ||
                Map[a2].Any(x => x.Value == b1 || x.Value == b2) ||
                Map[b1].Any(x => x.Value == a1 || x.Value == a2) ||
                Map[b2].Any(x => x.Value == a1 || x.Value == a2);
        }

        private NodeTargets find_neighbor_tangent(
            IEnumerable<Vector2> nodes,
            float maxVertAngle,
            float maxHoriAngle,
            float angle_factor,
            bool repeatThroughNeighbors = false)
        {
            var dirs = new List<CardinalDirections> {
                CardinalDirections.Down,
                CardinalDirections.Up,
                CardinalDirections.Left,
                CardinalDirections.Right };
            return find_neighbor_tangent(
                nodes, dirs,
                maxVertAngle, maxHoriAngle,
                angle_factor, repeatThroughNeighbors);
        }
        private NodeTargets find_neighbor_tangent(
            IEnumerable<Vector2> nodes,
            IEnumerable<CardinalDirections> dirs,
            float maxVertAngle,
            float maxHoriAngle,
            float angle_factor,
            bool repeatThroughNeighbors = false)
        {
            return nodes.ToDictionary(node => node, node =>
            {
                var map = new Dictionary<CardinalDirections, Tuple<Vector2, float>>();
                foreach (CardinalDirections dir in dirs)
                    if (!Map[node].ContainsKey(dir))
                    {
                        var tangent_dirs = tangent_directions(dir);
                        float max_angle =
                            (dir == CardinalDirections.Left ||
                            dir == CardinalDirections.Right) ?
                                maxHoriAngle : maxVertAngle;

                        List<Vector2> neighbor_tangents = new List<Vector2>();
                        foreach(var tangent_dir in tangent_dirs)
                        {
                            check_neighbor_tangent(node, dir, neighbor_tangents,
                                tangent_dir, max_angle, repeatThroughNeighbors);
                        }

                        Maybe<Vector2> best_tangent_dest = Maybe<Vector2>.Nothing;
                        Maybe<float> fitness = Maybe<float>.Nothing;
                        if (neighbor_tangents.Any())
                        {
                            // This is the closest neighbor tangent in the given direction
                            best_tangent_dest = neighbor_tangents
                                .OrderBy(y => destination_fitness(node, y, dir, max_angle, angle_factor))
                                .First();
                            // This is now the max range in the given direction, so try to find something closer
                            fitness = destination_fitness(
                                node, best_tangent_dest, dir, max_angle, angle_factor);
                        }

                        var destinations = possible_nodes(node, dir, max_angle, true);
                        if (best_tangent_dest.IsSomething)
                             destinations = destinations
                                 .Union(new Vector2[] { best_tangent_dest });

                        if (destinations.Any())
                        {
                            var best_dests = destinations
                                .Where(dest => fitness.IsNothing ||
                                    destination_fitness(
                                        node, dest, dir, max_angle, angle_factor) <= fitness);
                            if (!best_dests.Any())
                                best_dests = destinations;
                            Vector2 best_dest = best_dests
                                .OrderBy(dest => destination_fitness(
                                    node, dest, dir, max_angle, angle_factor))
                                .First();

                            fitness = destination_fitness(
                                node, best_dest, dir, max_angle, angle_factor);
                            map[dir] = new Tuple<Vector2, float>(best_dest, fitness);
                        }
                    }
                return map;
            });
        }
        private void check_neighbor_tangent(
            Vector2 x,
            CardinalDirections dir,
            List<Vector2> neighbor_tangents,
            CardinalDirections neighbor_dir,
            float maxAngle,
            bool repeatThroughNeighbors)
        {
            Vector2 neighbor = x;
            Vector2 current;

            for (int i = 0; ; i++)
            {
                // If only checking the first neighbor, break after the first loop
                if (i >= 1 && !repeatThroughNeighbors)
                    return;

                current = neighbor;
                if (Map[current].ContainsKey(neighbor_dir))
                {
                    neighbor = Map[current][neighbor_dir];

                    if (Map[neighbor].ContainsKey(dir))
                    {
                        switch (dir)
                        {
                            case CardinalDirections.Down:
                            case CardinalDirections.Up:
                                break;
                            case CardinalDirections.Left:
                            case CardinalDirections.Right:
                                // If the distance to the possible destination is less than the distance to the neighbor, skip
                                if (Math.Abs(x.X - Map[neighbor][dir].X) < Math.Abs(x.Y - neighbor.Y))
                                    continue;
                                break;
                            default:
                                throw new ArgumentException();
                        }

                        if (valid_angle(x, Map[neighbor][dir], dir, maxAngle))
                            neighbor_tangents.Add(Map[neighbor][dir]);
                    }
                }
                else
                    return;
            }
        }

        private void add_neighbor_tangets(NodeTargets neighbor_tangents)
        {
            var neighbors = neighbor_tangents
                .Where(x => x.Value.Any())
                .ToDictionary(p => p.Key, p => p.Value);
            foreach (var pair in neighbors)
                foreach (var tangent in pair.Value)
                    Map[pair.Key][tangent.Key] = tangent.Value.Item1;
        }

        internal static IEnumerable<CardinalDirections> tangent_directions(
            CardinalDirections dir)
        {
            switch (dir)
            {
                case CardinalDirections.Down:
                case CardinalDirections.Up:
                    yield return CardinalDirections.Left;
                    yield return CardinalDirections.Right;
                    break;
                case CardinalDirections.Left:
                case CardinalDirections.Right:
                    yield return CardinalDirections.Down;
                    yield return CardinalDirections.Up;
                    break;
                default:
                    yield break;
            }
        }

        private void add_destination(Vector2 node, Vector2 destination, CardinalDirections dir)
        {
            CardinalDirections opp_dir = (CardinalDirections)(CardinalDirections.All - dir);
            if (Map[node].ContainsKey(dir) || Map[destination].ContainsKey(opp_dir))
                throw new ArgumentException();
            Map[node][dir] = destination;
            Map[destination][opp_dir] = node;
        }

        private void insert_destination(Vector2 node, Vector2 destination1, Vector2 destination2, CardinalDirections dir)
        {
            CardinalDirections opp_dir = (CardinalDirections)(CardinalDirections.All - dir);
            // Destination1 and destination2 have to be currently pointing at each other
            if (Map[destination1][opp_dir] != destination2 || Map[destination2][dir] != destination1)
                throw new ArgumentException();
            // Remove the path between the two destinations
            Map[destination1].Remove(opp_dir);
            Map[destination2].Remove(dir);
            // Reconnect them through the new midpoint
            add_destination(node, destination1, dir);
            add_destination(node, destination2, opp_dir);
        }

        private static void swap(ref Vector2 a, ref Vector2 b)
        {
            Vector2 temp = a;
            a = b;
            b = temp;
        }

        /*private bool any_possible_nodes(Vector2 node) //Debug
        {
            foreach (Directions dir in new List<Directions> { Directions.Down, Directions.Left, Directions.Right, Directions.Up })
            {
                if (possible_nodes(node, dir).Any())
                    return true;
            }
            return false;
        }*/
        private IEnumerable<Vector2> possible_nodes(
            Vector2 node,
            CardinalDirections dir,
            float max_angle,
            bool allowAlreadyMatched = false)
        {
            if (Map[node].ContainsKey(dir))
                return new List<Vector2>();
            return Map
                .Where(x =>
                {
                    if (x.Key == node)
                        return false;
                    var opp_dir = (CardinalDirections)(CardinalDirections.All - dir);
                    if (!allowAlreadyMatched && x.Value.ContainsKey(opp_dir))
                        return false;
                    // If either node already points at the other in a tangent direction
                    foreach (var tangent_dir in tangent_directions(dir))
                    {
                        if ((x.Value.ContainsKey(tangent_dir) &&
                                    x.Value[tangent_dir] == node) ||
                                (Map[node].ContainsKey(tangent_dir) &&
                                    Map[node][tangent_dir] == x.Key))
                            return false;
                    }
                    return valid_destination(node, x.Key, dir, max_angle) &&
                        // Ensure destination is valid from the other direction
                        valid_destination(x.Key, node, opp_dir, max_angle);
                })
                .Select(x => x.Key)
                .ToList();
        }

        private bool valid_destination(
            Vector2 node, Vector2 dest, CardinalDirections dir, float max_angle)
        {
            // Check angle to target
            if (!valid_angle(node, dest, dir, max_angle))
                return false;

            // Check if target destination is more extreme than destinations the node has in tangential directions
            // ie if the target is up, the target cannot also be further left than the already confirmed left destination
            if (!valid_tangent(node, dest, dir))
                return false;

            return destination_correct_direction(node, dest, dir);
        }
        private bool valid_angle(Vector2 node, Vector2 dest, CardinalDirections dir, float max_angle)
        {
            float angle = angle_off_base(node, dest, dir);
            // If the angle is too wide
            if (Math.Abs(angle) > max_angle)
                return false;
            return true;
        }
        private bool valid_tangent(Vector2 node, Vector2 dest, CardinalDirections dir)
        {
            switch(dir)
            {
                case CardinalDirections.Down:
                case CardinalDirections.Up:
                    if (dest.X < node.X)
                    {
                        return !Map[node].ContainsKey(CardinalDirections.Left) ||
                            !Map[dest].ContainsKey(CardinalDirections.Right) ||
                            Map[node][CardinalDirections.Left].X <= dest.X;
                    }
                    else
                    {
                        return !Map[node].ContainsKey(CardinalDirections.Right) ||
                            !Map[dest].ContainsKey(CardinalDirections.Left) ||
                            Map[node][CardinalDirections.Right].X >= dest.X;
                    }
                case CardinalDirections.Left:
                case CardinalDirections.Right:
                    if (dest.Y < node.Y)
                    {
                        return !Map[node].ContainsKey(CardinalDirections.Up) ||
                            !Map[dest].ContainsKey(CardinalDirections.Down) ||
                            Map[node][CardinalDirections.Up].Y <= dest.Y;
                    }
                    else
                    {
                        return !Map[node].ContainsKey(CardinalDirections.Down) ||
                            !Map[dest].ContainsKey(CardinalDirections.Up) ||
                            Map[node][CardinalDirections.Down].Y >= dest.Y;
                    }
            }
            throw new ArgumentException();
        }
        private bool valid_tangent(Vector2 node, Vector2 dest, Vector2 best_dest, CardinalDirections dir)
        {
            // If the current destination is closer than the best destination in the travelled direction, it's fine
            if (destination_correct_direction(dest, best_dest, dir))
                return true;

            // Otherwise the current destination has to at least be closet in tangential directions
            switch (dir)
            {
                case CardinalDirections.Down:
                case CardinalDirections.Up:
                    if (dest.X < node.X)
                        return best_dest.X < dest.X;
                    else
                        return best_dest.X > dest.X;
                case CardinalDirections.Left:
                case CardinalDirections.Right:
                    if (dest.Y < node.Y)
                        return best_dest.Y < dest.Y;
                    else
                        return best_dest.Y > dest.Y;
            }
            throw new ArgumentException();
        }

        private static bool destination_correct_direction(Vector2 node, Vector2 dest, CardinalDirections dir)
        {
            switch (dir)
            {
                case CardinalDirections.Down:
                    return dest.Y > node.Y;
                case CardinalDirections.Left:
                    return dest.X < node.X;
                case CardinalDirections.Right:
                    return dest.X > node.X;
                case CardinalDirections.Up:
                    return dest.Y < node.Y;
            }
            throw new ArgumentException();
        }

        private static float destination_fitness(Vector2 node, Vector2 dest, CardinalDirections dir, float max_angle, float angle_factor)
        {
            float length = distance(node, dest, vertical_direction(dir));
            float angle = angle_off_base(node, dest, dir);
            //return length * (float)Math.Pow(1 + Math.Abs(angle) / max_angle, angle_factor);
            return length * MathHelper.Lerp(1, angle_factor, Math.Abs(angle) / max_angle);
        }

        private static bool vertical_direction(CardinalDirections dir)
        {
            switch (dir)
            {
                case CardinalDirections.Down:
                case CardinalDirections.Up:
                    return true;
                case CardinalDirections.Left:
                case CardinalDirections.Right:
                    return false;
                default:
                    throw new ArgumentException();
            }
        }

        private static float distance(Vector2 node, Vector2 dest, bool vertical)
        {
            Vector2 offset = node - dest;

            if (vertical)
                // Prefer close Y values over close X values
                return (float)(Math.Pow(Math.Abs(offset.X), 1f) +
                    Math.Pow(Math.Abs(offset.Y), 3f));
            else
                // Prefer close Y values over close X values
                return (float)(Math.Pow(Math.Abs(offset.X), 1.7f) +
                    Math.Pow(Math.Abs(offset.Y), 2.2f));
            
            //return offset.LengthSquared(); //Debug
        }

        private static float base_angle(CardinalDirections dir)
        {
            switch(dir)
            {
                case CardinalDirections.Down:
                    return (float)Math.Atan2(1, 0);
                case CardinalDirections.Left:
                    return (float)Math.Atan2(0, -1);
                case CardinalDirections.Right:
                    return (float)Math.Atan2(0, 1);
                case CardinalDirections.Up:
                    return (float)Math.Atan2(-1, 0);
            }
            throw new ArgumentException();
        }

        private static float angle_off_base(Vector2 node, Vector2 dest, CardinalDirections dir)
        {
            float angle = (float)Math.Atan2(dest.Y - node.Y, dest.X - node.X);
            angle = base_angle(dir) - angle;
            if (angle > MathHelper.Pi)
                angle -= MathHelper.TwoPi;
            else if (angle < -MathHelper.Pi)
                angle += MathHelper.TwoPi;
            return angle;
        }
    }
}
