using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TactileLibrary.Pathfinding
{
    public interface IMovementMap<T> where T : IEquatable<T>
    {
        Pathfinder<T> Pathfind();

        int MoveCostFactor { get; }

        TileData GetTileData(T loc, T goalLoc);

        bool Passable(T loc);
        bool Passable(T loc, T goalLoc);

        int TileCost(T loc, T goalLoc);

        bool Obstructed(T loc);
        bool Obstructed(T loc, T goalLoc);

        IEnumerable<T> AdjacentLocations(T loc);

        bool IsAdjacent(T current_loc, T target_loc);

        int HeuristicPenalty(T loc, T goalLoc, T prevLoc);

        int Distance(T loc, T targetLoc, bool useEuclideanDistance = false);

        bool RestrictToMap(T loc, T goalLoc, bool restrictToPlayable = true);

        bool InvalidLocation(T loc, bool restrictToPlayable);
        bool InvalidLocation(T loc, T goalLoc, bool restrictToPlayable);
    }
}
