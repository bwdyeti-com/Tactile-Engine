using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FEXNA_Library.Pathfinding.Old
{
    public struct PathfindingNode
    {
        public int Id { get; private set; }
        public int MoveCost { get; private set; }
        public HashSet<int> Adjacent { get; private set; }

        public PathfindingNode(int id, int move_cost, HashSet<int> adjacent) : this()
        {
            Id = id;
            MoveCost = move_cost;
            Adjacent = new HashSet<int>(adjacent);
        }
        public PathfindingNode(int id, int move_cost, params int[] adjacent) : this()
        {
            Id = id;
            MoveCost = move_cost;
            Adjacent = new HashSet<int>(adjacent);
        }

        public override string ToString()
        {
            return string.Format("PathfindingNode: Id = {0}, MoveCost = {1}, Adjacent Count = {2}",
                Id, MoveCost, Adjacent.Count);
        }

        /*public static PathfindingNode[] nodes_from_point_grid(int width, int height) //Debug
        {
            int[,] move_costs = new int[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    move_costs[x, y] = 1;
            return nodes_from_point_grid(move_costs);
        }*/
        public static PathfindingNode[] nodes_from_point_grid(int[,] move_costs)
        {
            int width = move_costs.GetLength(0);
            int height = move_costs.GetLength(1);

            PathfindingNode[] result = new PathfindingNode[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int i = x + y * width;
                    HashSet<int> adjacent = new HashSet<int>();
                    // Down
                    if (y < height - 1)
                        adjacent.Add(i + width);
                    // Left
                    if (x > 0)
                        adjacent.Add(i - 1);
                    // Right
                    if (x < width - 1)
                        adjacent.Add(i + 1);
                    // Up
                    if (y > 0)
                        adjacent.Add(i - width);


                    result[i] = new PathfindingNode(i, move_costs[x, y], adjacent);
                }

            return result;
        }

        public static PathfindingNode[] nodes_from_rectangle_grid(
            List<Rectangle> rectangles, List<int> move_costs = null)
        {
            // If move costs are given
            if (move_costs != null && move_costs.Count != rectangles.Count)
                throw new ArgumentException(
                    "Rectangles list and move cost\nlist need to have the same count");

            // Get the points composing each rectangle
            Dictionary<Point, int> rect_composite_points = new Dictionary<Point, int>();
            for (int i = 0; i < rectangles.Count; i++)
            {
                var rect = rectangles[i];
                for (int height = 0; height < rect.Height; height++)
                    for (int width = 0; width < rect.Width; width++)
                        rect_composite_points.Add(new Point(rect.X + width, rect.Y + height), i);
            }
            // Determine rectangle adjacency
            PathfindingNode[] result = new PathfindingNode[rectangles.Count];
            for (int i = 0; i < rectangles.Count; i++)
            {
                var rect = rectangles[i];
                HashSet<int> adjacent = new HashSet<int>();
                for (int y = 0; y < rect.Height; y++)
                {
                    Point left_point = new Point(rect.X - 1, rect.Y + y);
                    if (rect_composite_points.ContainsKey(left_point))
                        adjacent.Add(rect_composite_points[left_point]);
                    Point right_point = new Point(rect.Right, rect.Y + y);
                    if (rect_composite_points.ContainsKey(right_point))
                        adjacent.Add(rect_composite_points[right_point]);
                }
                for (int x = 0; x < rect.Width; x++)
                {
                    Point above_point = new Point(rect.X + x, rect.Y - 1);
                    if (rect_composite_points.ContainsKey(above_point))
                        adjacent.Add(rect_composite_points[above_point]);
                    Point below_point = new Point(rect.X + x, rect.Bottom);
                    if (rect_composite_points.ContainsKey(below_point))
                        adjacent.Add(rect_composite_points[below_point]);
                }

                result[i] = new PathfindingNode(i,
                    move_costs == null ? 1 : move_costs[i], adjacent);
            }

            return result;
        }
    }
}
