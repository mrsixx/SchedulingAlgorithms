namespace Scheduling.Solver.Interfaces
{
    public interface IPheromoneTrail<TKey>: IEnumerable<KeyValuePair<TKey, double>>
    {
        bool TryAdd(TKey key, double value);

        bool TryGetValue(TKey key, out double value);

        bool TryUpdate(TKey key, double newValue, double comparisonValue);
    }
}
