using QuickGraph;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Disjunction : IUndirectedEdge<Node>
    {
        public Disjunction(Node source, Node target)
        {
            Node src = source, tgt = target;
            if (src.Id < tgt.Id)
                (tgt, src) = (src, tgt);
            Source = src;
            Target = tgt;
        }

        public Node Source { get; }

        public Node Target { get; }
    }
}
