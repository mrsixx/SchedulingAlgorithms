using QuikGraph;

namespace Scheduling.Core.Graph
{
    public class BaseEdge : IEdge<Node>
    {
        private readonly object _lock = new();
        private double _pheromoneAmount = 0;
        public BaseEdge() { }

        public double Pheromone
        {
            get
            {
                lock (_lock)
                {
                    return _pheromoneAmount;
                }
            }
        }

        public virtual Node Source { get; }

        public virtual Node Target { get; }

        public virtual string Log { get; } = string.Empty;
        
        public override string ToString() => Log;

        public void EvaporatePheromone(double rate)
        {
            lock (_lock)
            {
                var old = _pheromoneAmount;
                _pheromoneAmount = (1 - rate) * _pheromoneAmount;
            }
        }

        public void DepositPheromone(double amount)
        {
            lock (_lock)
            {
                _pheromoneAmount += amount;
            }
        }
    }
}
