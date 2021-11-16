namespace TactileLibrary.Pathfinding
{
    public struct TileData
    {
        /// <summary>
        /// True if the tile can be entered.
        /// </summary>
        internal bool Passable { get; private set; }
        /// <summary>
        /// The movement cost of entering the tile.
        /// </summary>
        internal int TileCost { get; private set; }
        /// <summary>
        /// True if entering the tile consumes all remaining movement.
        /// </summary>
        internal bool Obstructs { get; private set; }

        public TileData(bool passable, int tileCost, bool obstructs)
        {
            Passable = passable;
            TileCost = tileCost;
            Obstructs = obstructs;
        }
    }
}
