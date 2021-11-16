using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TactileLibrary.Pathfinding
{
    class PassabilityMap3D : IMovementMap<Vector3>
    {
        private Tuple<bool, bool>[, ,] Map;
        private int Width, Height, Depth;

        internal PassabilityMap3D(Tuple<bool, bool>[, ,] map)
        {
            Map = map;

            Width = Map.GetLength(0);
            Height = Map.GetLength(1);
            Depth = Map.GetLength(2);
        }

        public Pathfinder<Vector3> Pathfind()
        {
            return new Pathfinder<Vector3>(this);
        }

        public TileData GetTileData(Vector3 loc, Vector3 goalLoc)
        {
            bool passable = Passable(loc, goalLoc);
            int tileCost = -1;
            if (passable)
                tileCost = TileCost(loc, goalLoc);

            return new TileData(passable, tileCost);
        }

        public bool Passable(Vector3 loc)
        {
            bool occupied = Map[(int)loc.X, (int)loc.Y, (int)loc.Z].Item1;
            bool on_floor = !InvalidLocation(loc + new Vector3(0, -1, 0)) &&
                Map[(int)loc.X, (int)loc.Y - 1, (int)loc.Z].Item2;
            return !occupied && on_floor;
        }

        public bool Passable(Vector3 loc, Vector3 goalLoc)
        {
            return Passable(loc);
        }

        public int TileCost(Vector3 loc, Vector3 goalLoc)
        {
            if (!Passable(loc))
                return -10;
            return 10;
        }

        public IEnumerable<Vector3> AdjacentLocations(Vector3 loc)
        {

            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (dx == 0 && dy == 0 && dz == 0)
                            continue;
                        if (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) <= 2)
                            yield return new Vector3(loc.X + dx, loc.Y + dy, loc.Z + dz);
                    }
        }

        public bool IsAdjacent(Vector3 loc, Vector3 targetLoc)
        {
            return (loc.X == targetLoc.X && loc.Y == targetLoc.Y && Math.Abs(loc.Z - targetLoc.Z) <= 1) ||
                (loc.X == targetLoc.X && loc.Z == targetLoc.Z && Math.Abs(loc.Y - targetLoc.Y) <= 1) ||
                (loc.Y == targetLoc.Y && loc.Z == targetLoc.Z && Math.Abs(loc.X - targetLoc.X) <= 1);
        }

        public int HeuristicPenalty(Vector3 loc, Vector3 goalLoc, Vector3 prevLoc)
        {
            return 0;
        }

        public int Distance(Vector3 loc, Vector3 targetLoc, bool useEuclideanDistance = false)
        {
            if (!useEuclideanDistance)
                return manhatten_dist(loc, targetLoc);
            else
                return euclidean_dist(loc, targetLoc);
        }
        private int euclidean_dist(Vector3 loc, Vector3 targetLoc)
        {
            return (int)(Math.Sqrt(
                Math.Pow(loc.X - targetLoc.X, 2) +
                Math.Pow(loc.Y - targetLoc.Y, 2) +
                Math.Pow(loc.Z - targetLoc.Z, 2)) * 10);
        }
        private int manhatten_dist(Vector3 loc, Vector3 targetLoc)
        {
            return (int)(Math.Abs(loc.X - targetLoc.X) +
                Math.Abs(loc.Y - targetLoc.Y) +
                Math.Abs(loc.Z - targetLoc.Z)) * 10;
        }

        public bool RestrictToMap(Vector3 loc, Vector3 goalLoc, bool restrictToPlayable = true)
        {
            return !InvalidLocation(loc, restrictToPlayable) &&
                !InvalidLocation(goalLoc, restrictToPlayable);
        }

        public bool InvalidLocation(Vector3 loc, bool restrictToPlayable = false)
        {
            return (int)loc.X < 0 || (int)loc.X >= Width ||
                (int)loc.Y < 0 || (int)loc.Y >= Height ||
                (int)loc.Z < 0 || (int)loc.Z >= Depth;
        }
        public bool InvalidLocation(Vector3 loc, Vector3 goalLoc, bool restrictToPlayable)
        {
            // If the checked location isn't the target but is off the map, and off the map is not allowed
            if (loc != goalLoc &&
                    InvalidLocation(loc, restrictToPlayable))
                return true;
            return false;
        }
    }
}
