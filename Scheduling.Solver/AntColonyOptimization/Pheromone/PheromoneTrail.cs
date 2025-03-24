using Newtonsoft.Json.Linq;
using Scheduling.Core.Graph;
using Scheduling.Solver.Interfaces;
using System.Collections;

namespace Scheduling.Solver.AntColonyOptimization.Pheromone
{
    public class PheromoneTrail : IPheromoneTrail<Orientation>
    {
        private readonly Dictionary<Orientation, double> _trail = [];


        public bool TryAdd(Orientation key, double value)
        {
            _trail.Add(key, value);
            return true;
        }

        public bool TryGetValue(Orientation key, out double value)
            => _trail.TryGetValue(key, out value);

        public bool TryUpdate(Orientation key, double newValue, double comparisonValue)
        {
            if (!_trail.ContainsKey(key)) return false;
            _trail[key] = newValue;
            return true;
        }


        public IEnumerator GetEnumerator() => _trail.GetEnumerator();

        IEnumerator<KeyValuePair<Orientation, double>> IEnumerable<KeyValuePair<Orientation, double>>.GetEnumerator()
        {
            foreach (var t in _trail)
                yield return t;
        }
    }
}
