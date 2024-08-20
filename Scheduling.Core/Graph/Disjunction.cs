using QuikGraph;
using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Disjunction : BaseEdge, IUndirectedEdge<Node>
    {
        private readonly object _lock = new();
        private Pheromone _pheromoneAmount = new(0.0, 0.0);

        private Disjunction(Node u, Node v)
        {
            Node node1 = u, node2 = v;
            //if (node1.Id < node2.Id)
            //    (node2, node1) = (node1, node2);
            Source = node1;
            Target = node2;
        }

        public Disjunction(Node u, Node v, Machine machine) : this(u, v)
        {
            
            Machine = machine;
            EquivalentConjunctions = [
                new Conjunction(Source, Target, Machine).SetOriginalDisjunction(this, Direction.SourceToTarget),
                new Conjunction(Target, Source, Machine).SetOriginalDisjunction(this, Direction.TargetToSource)
            ];
        }

        public override Node Source { get; }

        public override Node Target { get; }

        public override string Log => Machine != null ? $"{Source.Id} --[{Machine.Id}]-- {Target.Id}" : $"{Source.Id} --- {Target.Id}";

        public Machine? Machine { get; }

        public Conjunction[] EquivalentConjunctions { get; }

        public Pheromone Pheromone
        {
            get
            {
                lock (_lock)
                {
                    return _pheromoneAmount;
                }
            }
        }

        public override void EvaporatePheromone(double rate)
        {
            lock (_lock)
            {
                _pheromoneAmount.SourceToTarget = (1 - rate) * _pheromoneAmount.SourceToTarget;
                _pheromoneAmount.TargetToSource = (1 - rate) * _pheromoneAmount.TargetToSource;
            }
        }

        public void DepositPheromone(double amount, Direction direction)
        {
            lock (_lock)
            {
                if (direction == Direction.SourceToTarget)
                    _pheromoneAmount.SourceToTarget += amount;
                else if (direction == Direction.TargetToSource)
                    _pheromoneAmount.TargetToSource += amount;
            }
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            if (obj is not Disjunction disjunction) return false;

            return (Source.Id == disjunction.Source.Id && Target.Id == disjunction.Target.Id && Machine == disjunction.Machine) ||
                   (Target.Id == disjunction.Source.Id && Source.Id == disjunction.Target.Id && Machine == disjunction.Machine);

        }

        public override int GetHashCode()
        {
            return $"{Source.Id}-[{Machine?.Id}]-{Target.Id}".GetHashCode();
        }
    }
}
