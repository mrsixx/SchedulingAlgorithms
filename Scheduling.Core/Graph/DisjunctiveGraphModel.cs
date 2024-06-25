using QuickGraph;
using Scheduling.Core.Interfaces;
using System.Xml.Linq;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class DisjunctiveGraphModel : UndirectedGraph<Node, IEdge<Node>>, IDisjunctiveGraph
    {
        public DisjunctiveGraphModel() : base(allowParallelEdges: true)
        {
            Source = new(Node.SOURCE_ID);
            Sink = new(Node.SINK_ID);
        }

        public Node Source { get; }

        public Node Sink { get; }


        public bool HasNode(int id) => TryGetNode(id, out _);

        public bool TryGetNode(int id, out Node node)
        {
            var vertex = Vertices.ToList().Find(node => node.Id == id);
            if(vertex != null)
            {
                node = vertex;
                return true;
            }

            node = default;
            return false;
        }

        public bool HasConjunction(int sourceId, int targetId) => TryGetConjunction(sourceId, targetId, out _);
        
        public bool TryGetConjunction(int sourceId, int targetId, out Conjunction conjunction)
        {
            bool hasSrc = TryGetNode(sourceId, out Node sourceNode), 
                 hasTarget = TryGetNode(targetId, out Node targetNode);
            var hasNodes =  hasSrc && hasTarget;

            if (hasNodes
                && TryGetEdge(sourceNode, targetNode, out IEdge<Node> link)
                && link is Conjunction arc)
            {
                conjunction = arc;
                return true;
            }

            conjunction = default;
            return false;
        }

        public bool HasDisjunction(int sourceId, int targetId) => TryGetDisjunction(sourceId, targetId, out _);
        
        public bool TryGetDisjunction(int sourceId, int targetId, out Disjunction disjunction)
        {
            bool hasSrc = TryGetNode(sourceId, out Node sourceNode), 
                 hasTarget = TryGetNode(targetId, out Node targetNode);
            var hasNodes = hasSrc && hasTarget;

            // lowerIds always in the edge source
            if (hasNodes && sourceId < targetId)
                (sourceNode, targetNode) = (targetNode, sourceNode);

            if (hasNodes
                && TryGetEdge(sourceNode, targetNode, out IEdge<Node> link)
                && link is Disjunction edge)
            {
                disjunction = edge;
                return true;
            }

            disjunction = default;
            return false;
        }

    }
}
