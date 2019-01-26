namespace FEXNA.AI
{
    /// <summary>
    /// Represents a unit on a map, and the travel distance to get there.
    /// </summary>
    class UnitDistance : CombatObjectDistance
    {
        #region Accessors
        public Game_Unit unit { get { return Global.game_map.units[this.Id]; } }
        #endregion

        public override string ToString()
        {
            return string.Format("Unit: {0}; Dist: {1}", this.unit, this.Dist);
        }

        public UnitDistance(int unitId, int dist) : base(unitId, dist) { }
    }
}
