namespace TactileLibrary.Pathfinding.Old
{
    public interface IDistanceMeasurable<T>
    {
        int Distance(T other);

        bool SameLocation(T other);
    }
}
