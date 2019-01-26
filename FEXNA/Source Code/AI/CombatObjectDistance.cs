using Microsoft.Xna.Framework;
namespace FEXNA.AI
{
    /// <summary>
    /// Represents an attackable object on a map, and the travel distance to get there.
    /// </summary>
    class CombatObjectDistance
    {
        private int _objectId;
        private int _Dist;

        #region Accessors
        public Combat_Map_Object MapObject { get { return Global.game_map.attackable_map_object(_objectId); } }

        public int Id { get { return _objectId; } }
        public int Dist { get { return _Dist; } }

        public virtual Vector2 Loc { get { return this.MapObject.loc; } }
        #endregion

        public override string ToString()
        {
            return string.Format("Map Object: {0}; Dist: {1}", this.MapObject, _Dist);
        }

        public CombatObjectDistance(int id, int dist)
        {
            _objectId = id;
            _Dist = dist;
        }
    }
}
