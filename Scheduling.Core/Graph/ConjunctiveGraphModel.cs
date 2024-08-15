using QuikGraph;
using QuikGraph.Algorithms;
using Scheduling.Core.Extensions;

namespace Scheduling.Core.Graph
{
    public class ConjunctiveGraphModel : AdjacencyGraph<Node, Conjunction>
    {
        public double Makespan => this.Sinks().Max(n => n.Operation.CompletionTime);

        public void AddConjunctionAndVertices(Conjunction conjunction)
        {
            // add nodes if not contains
            AddVertex(conjunction.Source);
            AddVertex(conjunction.Target);
            AddEdge(conjunction);
        }

        public List<Conjunction> GetOutEdges(Node node)
        {
            if (TryGetOutEdges(node, out IEnumerable<Conjunction> conjunctions))
                return conjunctions.ToList();

            return [];
        }

    }
}
