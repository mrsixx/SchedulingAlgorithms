using QuikGraph;
using Scheduling.Core.Graph;

namespace Scheduling.Core.Extensions
{
    public static class GraphExtensions
    {
        public static bool TryGetDisjunctiveGraphOutEdges(this DisjunctiveGraphModel graph, Node vertex, out IEnumerable<BaseEdge> edges)
        {
            var conjunctions = graph.Edges.Where(e => e is Conjunction c && c.Source == vertex).Cast<Conjunction>();
            var disjunctions = graph.Edges.Where(e => e is Disjunction d && (d.Source == vertex || d.Target == vertex)).Cast<Disjunction>();
            edges = [];
            var lista = new List<BaseEdge>();
            lista.AddRange(conjunctions);
            lista.AddRange(disjunctions);
            edges = lista;
            return lista.Any();
        }

        public static bool TryGetDisjunctiveGraphEdge(this DisjunctiveGraphModel graph, Node v1, Node v2, out BaseEdge edge)
        {
            if (graph.TryGetEdge(v1, v2, out IEdge<Node> temp) && temp is BaseEdge tempCast)
            {
                edge = tempCast;
                return true;
            }
            edge = null;
            return false;
        }
    }
}
