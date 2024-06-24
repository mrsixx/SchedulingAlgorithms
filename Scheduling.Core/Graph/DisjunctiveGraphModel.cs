using QuickGraph;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class DisjunctiveGraphModel : UndirectedGraph<Node, IEdge<Node>>
    {
        public const int SOURCE_ID = 0;
        public const int SINK_ID = -1;

        public DisjunctiveGraphModel() : base(allowParallelEdges: true) { }

        public bool HasConjunction(int sourceId, int targetId) => TryGetConjunction(sourceId, targetId, out _);
        public bool TryGetConjunction(int sourceId, int targetId, out Conjunction arc)
        {
            var nodes = Vertices.ToList();
            var sourceNode = nodes.Find(node => node.Id == sourceId);
            var targetNode = nodes.Find(node => node.Id == targetId);

            var hasNodes = sourceNode != null && targetNode != null;

            if (hasNodes
                && TryGetEdge(sourceNode, targetNode, out IEdge<Node> edge)
                && edge is Conjunction conjunction)
            {
                arc = conjunction;
                return true;
            }

            arc = default;
            return false;
        }


        public bool HasDisjunction(int sourceId, int targetId) => TryGetConjunction(sourceId, targetId, out _);
        public bool TryGetDisjunction(int sourceId, int targetId, out Conjunction arc)
        {
            var nodes = Vertices.ToList();
            var sourceNode = nodes.Find(node => node.Id == sourceId);
            var targetNode = nodes.Find(node => node.Id == targetId);

            var hasNodes = sourceNode != null && targetNode != null;

            if (hasNodes
                && TryGetEdge(sourceNode, targetNode, out IEdge<Node> edge)
                && edge is Conjunction conjunction)
            {
                arc = conjunction;
                return true;
            }

            arc = default;
            return false;
        }
    }
}
