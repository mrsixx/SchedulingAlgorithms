using QuikGraph;
using Scheduling.Core.FJSP;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Disjunction : BaseEdge, IUndirectedEdge<Node>
    {
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
            Orientations = [
                new Orientation(this, Direction.SourceToTarget),
                new Orientation(this, Direction.TargetToSource)
            ];
        }

        public override Node Source { get; }

        public override Node Target { get; }

        public override string Log => Machine != null ? $"{Source.Id} --[{Machine.Id}]-- {Target.Id}" : $"{Source.Id} --- {Target.Id}";

        public Machine Machine { get; }

        public Orientation[] Orientations { get; }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            if (obj is not Disjunction disjunction) return false;

            return (Source.Id == disjunction.Source.Id && Target.Id == disjunction.Target.Id && Machine == disjunction.Machine) ||
                   (Target.Id == disjunction.Source.Id && Source.Id == disjunction.Target.Id && Machine == disjunction.Machine);

        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Machine.Id.GetHashCode();
                hash = hash * 23 + (Source.Id.GetHashCode() + Target.Id.GetHashCode());
                return hash;
            }
        }
    }
}
