using QuikGraph;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Conjunction : IEdge<Node>
    {
        public Conjunction(Node source, Node target, double weight)
        {
            Source = source;
            Target = target;
            Weight = weight;
        }

        public Node Source { get; }

        public Node Target { get; }

        public double Weight { get; }
    }
}
