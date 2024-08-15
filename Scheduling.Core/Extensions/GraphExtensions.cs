using QuikGraph;
using Scheduling.Core.Graph;

namespace Scheduling.Core.Extensions
{
    public static class GraphExtensions
    {

        public static double CalculateLength(this IEnumerable<Conjunction> path)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));
            // sum of the weights of each edge of the path
            return path.Aggregate(0.0, (acc, edge) => acc + edge.Weight);
        }

        public static IList<Conjunction> GetConjunctions(this Node node, DisjunctiveGraphModel graph)
        {
            if(graph.TryGetConjunctions(node.Id, out var conjunctions))
                return conjunctions.ToList();
            return default;
        }


    }
}
