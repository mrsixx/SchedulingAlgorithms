using QuickGraph;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Conjunction : IEdge<Node>
    {
        public Conjunction(Node source, Node target)
        {
            Source = source;
            Target = target;
        }

        public Node Source { get; }

        public Node Target { get; }
    }
}
