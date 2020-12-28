using Microsoft.Xna.Framework;

namespace Tactile.AI
{
    /// <summary>
    /// Represents a destroyable object on a map, and the travel distance to get there.
    /// </summary>
    class DestroyableDistance : CombatObjectDistance
    {
        #region Accessors
        public Destroyable_Object Destroyable { get { return Global.game_map.get_destroyable(this.Id); } }

        public override Vector2 Loc { get { return this.Destroyable.loc; } }
        #endregion

        public override string ToString()
        {
            return string.Format("Destroyable: Loc: {0}, {1}; Dist: {2}",
                this.Loc.X, this.Loc.Y,
                this.Dist);
        }

        public DestroyableDistance(int id, int dist) : base(id, dist) { }
    }
}
