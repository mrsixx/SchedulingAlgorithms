using Scheduling.Core.Graph;
using Scheduling.Solver.Interfaces;
using System.Collections;

namespace Scheduling.Solver.AntColonyOptimization.Pheromone
{
    public class PheromoneTrail<TPheromonePoint> : IPheromoneTrail<TPheromonePoint> where TPheromonePoint : notnull
    {
        private readonly Dictionary<TPheromonePoint, double> _trail = [];

        public bool TryAdd(TPheromonePoint key, double value)
        {
            _trail.Add(key, value);
            return true;
        }

        public bool TryGetValue(TPheromonePoint key, out double value)
            => _trail.TryGetValue(key, out value);

        public bool TryUpdate(TPheromonePoint key, double newValue, double comparisonValue)
        {
            if (!_trail.ContainsKey(key)) return false;
            _trail[key] = newValue;
            return true;
        }


        public IEnumerator GetEnumerator() => _trail.GetEnumerator();

        IEnumerator<KeyValuePair<TPheromonePoint, double>> IEnumerable<KeyValuePair<TPheromonePoint, double>>.GetEnumerator()
        {
            foreach (var t in _trail)
                yield return t;
        }

    }
}
