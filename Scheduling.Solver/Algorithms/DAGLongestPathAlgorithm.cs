using QuikGraph.Algorithms;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;

namespace Scheduling.Solver.Algorithms
{
    /// <summary>
    /// DAG Longest Path given from PERT/CPM algorithm
    /// </summary>
    public static class DAGLongestPathAlgorithm
    {
        public static IEnumerable<Conjunction> LongestPath(ConjunctiveGraphModel graph)
        {
            var lpt = new Dictionary<int, Conjunction>(
                graph.Vertices.Select(v => new KeyValuePair<int, Conjunction>(v.Id, null))
            );
            var wt = new Dictionary<int, double>(
                graph.Vertices.Select(v => new KeyValuePair<int, double>(v.Id, 0))
            );


            foreach (var vertex in graph.TopologicalSort())
            {
                foreach (var edge in graph.GetOutEdges(vertex))
                {
                    if (wt[edge.Target.Id] < wt[vertex.Id] + edge.Weight)
                    {
                        wt[edge.Target.Id] = wt[vertex.Id] + edge.Weight;
                        lpt[edge.Target.Id] = edge;
                    }
                }
            }

            var path = new List<Conjunction>();
            for (var e = lpt[Operation.SINK_ID]; e != null; e = lpt[e.Source.Id])
                path.Add(e);
            
            path.Reverse();
            return path;
        }
    }
}
