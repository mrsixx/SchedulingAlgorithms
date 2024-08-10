using QuikGraph;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Interfaces;
using System.Text;

namespace Scheduling.Solver.Extensions
{
    public static class AntColonyOptimizationExtensions
    {

        //public static double CalculateTotalPheromoneAmount(this DisjunctiveGraphModel graph) => graph.Edges.Sum(s => s.Pheromone);
        //public static double CalculateAvgPheromoneAmount(this DisjunctiveGraphModel graph) => graph.Edges.Aggregate(0.0, (acc, edge) => acc + (edge.Pheromone * edge.Weight.Inverse()))
        //                                                                                    .DividedBy(graph.Edges.Sum(e => e.Weight.Inverse()));
        public static BaseEdge GetRandomEdge(this IEnumerable<BaseEdge> @enumerable) => @enumerable.ElementAt(Random.Shared.Next(@enumerable.Count()));

        public static void LogPath(this ILogger logger, IEnumerable<Conjunction> path)
        {
            var first = true;
            var sb = new StringBuilder();
            foreach (var step in path)
            {
                sb.Append(first ? $"{step.Source} -{step.Weight}-> {step.Target}" : $" -{step.Weight}-> {step.Target}");
                first = false;
            }
            logger.Log(sb.ToString());
        }
    }
}