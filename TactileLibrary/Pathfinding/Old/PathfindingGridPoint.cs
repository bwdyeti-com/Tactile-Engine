using System;
using Microsoft.Xna.Framework;

namespace TactileLibrary.Pathfinding.Old
{
    public struct PathfindingGridPoint :
        IEquatable<PathfindingGridPoint>, IDistanceMeasurable<PathfindingGridPoint>
    {
        private Point P;
        public int MoveCost { get; private set; }

        public int X { get { return P.X; } }
        public int Y { get { return P.Y; } }

        public PathfindingGridPoint(int x, int y, int move_cost) : this()
        {
            P = new Point(x, y);
            MoveCost = move_cost;
        }

        public override string ToString()
        {
            return string.Format("PathfindingGridPoint: {0}, MoveCost = {1}",
                P, MoveCost);
        }

        public static GridPointPathfinder pathfinder(int width, int height)
        {
            int[,] move_costs = new int[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    move_costs[x, y] = 1;
            return pathfinder(move_costs);
        }
        public static GridPointPathfinder pathfinder(int[,] move_costs)
        {
            int width = move_costs.GetLength(0);
            int height = move_costs.GetLength(1);

            PathfindingGridPoint[,] points_array = new PathfindingGridPoint[width, height];
            PathfindingGridPoint[] points = new PathfindingGridPoint[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int i = x + y * width;
                    points_array[x, y] = points[i] =
                        new PathfindingGridPoint(x, y, move_costs[x, y]);
                }
            var nodes = PathfindingNode.nodes_from_point_grid(move_costs);
            var pathfinder = new Pathfinder<PathfindingGridPoint>(points, nodes);

            return new GridPointPathfinder(points_array, pathfinder);
        }

        public int Distance(PathfindingGridPoint other)
        {
            return (int)(Math.Abs(X - other.X) + Math.Abs(Y - other.Y));
        }

        public bool SameLocation(PathfindingGridPoint other)
        {
            return P == other.P;
        }

        public bool Equals(PathfindingGridPoint other)
        {
            return P == other.P && MoveCost == other.MoveCost;
        }
    }

    public struct GridPointPathfinder
    {
        public PathfindingGridPoint[,] PointArray;
        public Pathfinder<PathfindingGridPoint> Pathfinder;

        public GridPointPathfinder(PathfindingGridPoint[,] points_array, Pathfinder<PathfindingGridPoint> pathfinder)
        {
            PointArray = points_array;
            Pathfinder = pathfinder;
        }
    }
}
