using QuikGraph;
using Scheduling.Core.Interfaces;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Conjunction : BaseEdge
    {
        public Conjunction(Node source, Node target, double weight)
        {
            Source = source;
            Target = target;
            Weight = weight;
        }

        public override Node Source { get; }

        public override Node Target { get; }

        public double Weight { get; }

        public override string Log => $"{Source.Id} -[{Weight}]-> {Target.Id}";

    }
}
