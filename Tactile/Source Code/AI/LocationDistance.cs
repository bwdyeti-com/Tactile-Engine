using Microsoft.Xna.Framework;

namespace Tactile.AI
{
    /// <summary>
    /// Represents a location on a map, and the travel distance to get there.
    /// </summary>
    struct LocationDistance
    {
        private Vector2 Loc;
        private int Dist;

        #region Accessors
        public Vector2 loc { get { return Loc; } }
        public int dist { get { return Dist; } }
        #endregion

        public override string ToString()
        {
            return string.Format("Loc: {0}; Dist: {1}", Loc, Dist);
        }

        public LocationDistance(Vector2 loc, int dist)
        {
            Loc = loc;
            Dist = dist;
        }
    }
}
