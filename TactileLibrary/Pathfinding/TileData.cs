namespace TactileLibrary.Pathfinding
{
    public struct TileData
    {
        internal bool Passable { get; private set; }
        internal int TileCost { get; private set; }

        public TileData(bool passable, int tileCost)
        {
            Passable = passable;
            TileCost = tileCost;
        }
    }
}
