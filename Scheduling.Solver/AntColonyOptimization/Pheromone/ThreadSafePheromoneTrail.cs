using Scheduling.Solver.Interfaces;
using System.Collections;
using System.Collections.Concurrent;

namespace Scheduling.Solver.AntColonyOptimization.Pheromone
{
    public class ThreadSafePheromoneTrail<TPheromonePoint> : IPheromoneTrail<TPheromonePoint> where TPheromonePoint : notnull
    {
        private readonly ConcurrentDictionary<TPheromonePoint, double> _trail = new();


        public bool TryAdd(TPheromonePoint key, double value)
            => _trail.TryAdd(key, value);


        public bool TryGetValue(TPheromonePoint key, out double value)
            => _trail.TryGetValue(key, out value);

        public bool TryUpdate(TPheromonePoint key, double newValue, double comparisonValue)
            => _trail.TryUpdate(key, newValue, comparisonValue);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _trail.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TPheromonePoint, double>> GetEnumerator()
        {
            foreach (var t in _trail)
                yield return t;
        }
    }
}
