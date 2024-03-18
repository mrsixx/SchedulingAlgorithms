using QuickGraph;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class DisjunctiveGraphModel : BidirectionalGraph<OperationVertex, VertexEdge>
    {
        public DisjunctiveGraphModel() : base(allowParallelEdges: true)
        {
        }
    }

    [Serializable]
    public class OperationVertex
    {
        public OperationVertex(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }

    [Serializable]
    public class VertexEdge : IEdge<OperationVertex>
    {
        public VertexEdge(OperationVertex source, OperationVertex target)
        {
            Source = source;
            Target = target;
        }

        public OperationVertex Source { get; }

        public OperationVertex Target { get; }
    }
}
