using Scheduling.Core.Graph;
using Scheduling.Solver.Interfaces;
using System.Collections;
using System.Collections.Concurrent;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class ThreadSafePheromoneTrail : IPheromoneTrail<Orientation, double>
    {
        private readonly ConcurrentDictionary<Orientation, double> _trail;
        public ThreadSafePheromoneTrail()
        {
            _trail = new ConcurrentDictionary<Orientation, double>();
        }

        public bool TryAdd(Orientation key, double value)
            => _trail.TryAdd(key, value);
       

        public bool TryGetValue(Orientation key, out double value) 
            => _trail.TryGetValue(key, out value);

        public bool TryUpdate(Orientation key, double newValue, double comparisonValue) 
            => _trail.TryUpdate(key, newValue, comparisonValue);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _trail.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<Orientation, double>> GetEnumerator()
        {
            foreach (var t in _trail)
                yield return t;
        }
    }
}
