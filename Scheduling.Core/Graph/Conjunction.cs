using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Conjunction(Node source, Node target) : BaseEdge
    {
        public Conjunction(Node source, Node target, double weight) : this(source, target)
        {
            Weight = weight;
        }

        public Conjunction SetOriginalDisjunction(Disjunction associatedDisjunction, Direction choosenDirection)
        {
            ChoosenDirection = choosenDirection;
            AssociatedDisjunction = associatedDisjunction;
            return this;
        }

        public override Node Source { get; } = source;

        public override Node Target { get; } = target;

        public double Weight { get; }

        public override string Log => $"{Source.Id} -[{Weight}]-> {Target.Id}";

        public Disjunction? AssociatedDisjunction { get; private set; }

        public Direction? ChoosenDirection { get; private set; }

        public bool HasAssociatedDisjunction => AssociatedDisjunction is not null;
    }
}
