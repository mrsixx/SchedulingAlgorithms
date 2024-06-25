using QuickGraph.Serialization;
using Scheduling.Core.Extensions;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;
using Scheduling.Core.Models;

namespace Scheduling.Core.Services
{
    public class GraphBuilderService : IGraphBuilderService
    {
        public DisjunctiveGraphModel BuildDisjunctiveGraph(List<Job> jobs, List<Machine> machines)
        {
            var graph = new DisjunctiveGraphModel();
            graph.AddVertex(graph.Source);
            graph.AddVertex(graph.Sink);
            jobs.ForEach(job =>
            {
                // 1st operation of each job linked with source
                var previous = graph.Source;
                job.Operations.Select(o => new Node(o.Id))
                    .ToList()
                    .ForEach(operation =>
                    {
                        graph.AddVertex(operation);
                        graph.AddEdge(new Conjunction(previous, operation));
                        previous = operation;
                    });

                //last operation of each job linked with sink
                graph.AddEdge(new Conjunction(previous, graph.Sink));
            });

            // create disjunctions between every operation running on same pool
            jobs.GeneratePossibleDisjunctions()
                .ToList()
                .ForEach(pair =>
                {
                    var (o1, o2) = pair;
                    // if some node not found
                    if (graph.TryGetNode(o1.Id, out Node node1) && graph.TryGetNode(o2.Id, out Node node2))
                        graph.AddEdge(new Disjunction(node1, node2));

                });
                return graph;
        }

        public DisjunctiveGraphModel BuildDisjunctiveGraphByBenchmarkFile(string benchmarkFile)
        {
            throw new NotImplementedException();
        }
    }
}
