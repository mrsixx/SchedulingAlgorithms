using Scheduling.Core.FJSP;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Conjunction(Node source, Node target) : BaseEdge
    {
        private readonly object _lock = new();
        private double _pheromoneAmount = 0.0;

        public Conjunction(Node source, Node target, Machine machine) : this(source, target)
        {
            Machine = machine;
        }

        public Conjunction SetOriginalDisjunction(Disjunction associatedDisjunction, Direction choosenDirection)
        {
            ChoosenDirection = choosenDirection;
            AssociatedDisjunction = associatedDisjunction;
            return this;
        }

        public override Node Source { get; } = source;

        public override Node Target { get; } = target;

        public double Weight {
            get
            {
                if (Source.IsSourceNode)
                    return Target.Operation.EligibleMachines.Contains(Machine) ? Target.Operation.GetProcessingTime(Machine) : 0;
                return Source.Operation.EligibleMachines.Contains(Machine) ? Source.Operation.GetProcessingTime(Machine) : 0;
            }
        }

        public Machine Machine { get; set; }

        public override string Log => $"{Source.Id} -[{Weight}]-> {Target.Id}";

        public Disjunction? AssociatedDisjunction { get; private set; }

        public Direction? ChoosenDirection { get; private set; }

        public bool HasAssociatedDisjunction => AssociatedDisjunction is not null;

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

        public override void EvaporatePheromone(double rate)
        {
            if (HasAssociatedDisjunction)
            {
                AssociatedDisjunction.EvaporatePheromone(rate);
                return;
            }

            lock (_lock)
            {
                _pheromoneAmount = (1 - rate) * _pheromoneAmount;
            }
        }

        public void DepositPheromone(double amount)
        {
            if (HasAssociatedDisjunction)
            {
                AssociatedDisjunction.DepositPheromone(amount, ChoosenDirection.Value);
                return;
            }

            lock (_lock)
            {
                _pheromoneAmount += amount;
            }
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            if (obj is not Conjunction conjunction) return false;

            return Source.Id == conjunction.Source.Id && Target.Id == conjunction.Target.Id;
        }

        public override int GetHashCode()
        {
            return $"{Source.Id}-[${Machine?.Id}]->{Target.Id}".GetHashCode();
        }
    }
}
