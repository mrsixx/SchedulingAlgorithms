namespace Scheduling.Solver.Interfaces
{
    public interface IPheromoneTrail<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        bool TryAdd(TKey key, TValue value);

        bool TryGetValue(TKey key, out TValue value);

        bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue);
    }
}
