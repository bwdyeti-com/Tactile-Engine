namespace FEXNA.AI
{
    /// <summary>
    /// Represents a unit on a map, and the travel distance to get there.
    /// </summary>
    struct UnitDistance
    {
        private int _unitId;
        private int Dist;

        #region Accessors
        public Game_Unit unit { get { return Global.game_map.units[_unitId]; } }
        public int UnitId { get { return _unitId; } }
        public int dist { get { return Dist; } }
        #endregion

        public override string ToString()
        {
            return string.Format("Unit: {0}; Dist: {1}", unit, Dist);
        }

        public UnitDistance(int unitId, int dist)
        {
            _unitId = unitId;
            Dist = dist;
        }
    }
}
