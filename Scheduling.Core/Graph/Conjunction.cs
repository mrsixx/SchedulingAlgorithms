namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Conjunction(Node source, Node target) : BaseEdge
    {
        public Conjunction(Orientation orientation) : this(orientation.Source, orientation.Target)
        {
            AssociatedOrientation = orientation;
        }
        public override Node Source { get; } = source;

        public override Node Target { get; } = target;

        public double Weight { get; set; }

        public override string Log => $"{Source.Id} --> {Target.Id}";

        public Orientation? AssociatedOrientation { get; set; }

        public bool HasAssociatedOrientation => AssociatedOrientation != null;

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            if (obj is not Conjunction conjunction) return false;

            return Source.Id == conjunction.Source.Id && Target.Id == conjunction.Target.Id;
        }

        public override int GetHashCode()
        {
            return $"{Source.Id}-->{Target.Id}".GetHashCode();
        }
    }
}
