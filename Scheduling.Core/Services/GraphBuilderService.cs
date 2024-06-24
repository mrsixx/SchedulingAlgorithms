using QuickGraph.Serialization;
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
            Node source = new(DisjunctiveGraphModel.SOURCE_ID), sink = new(DisjunctiveGraphModel.SINK_ID);
            graph.AddVertex(source);
            graph.AddVertex(sink);
            jobs.ForEach(job =>
            {
                var latest = source;
                job.Operations.Select(o => new Node(o.Id))
                    .ToList()
                    .ForEach(operation =>
                    {
                        graph.AddVertex(operation);
                        graph.AddEdge(new Conjunction(latest, operation));
                        latest = operation;
                    });
                graph.AddEdge(new Conjunction(latest, sink));
            });

            // create disjunctions between every operation running on same pool
            jobs.SelectMany(job => job.Operations)
            .GroupBy(o => o.MachinePoolId)
            .ToList()
            .ForEach(grp =>
            {
                var machinePoolOperations = grp.ToList();
                var cartesianProduct = from o1 in machinePoolOperations
                                       from o2 in machinePoolOperations
                                       select new Tuple<Operation, Operation>(o1, o2);

                var edges = new List<Tuple<int, int>>();
                foreach (var (o1, o2) in cartesianProduct)
                {
                    if (o1.Id != o2.Id)
                    {
                        if (edges.Any((e) => e.Item1 == o2.Id && e.Item2 == o1.Id))
                            continue;

                        var node1 = graph.Vertices.ToList().Find(o => o.Id == o1.Id);
                        var node2 = graph.Vertices.ToList().Find(o => o.Id == o2.Id);
                        if (node1 is null || node2 is null)
                            continue;

                        graph.AddEdge(new Disjunction(node1, node2));
                        edges.Add(new Tuple<int, int>(node1.Id, node2.Id));
                    }
                }

            });


            return graph;
        }

        public DisjunctiveGraphModel BuildDisjunctiveGraphByBenchmarkFile(string benchmarkFile)
        {
            throw new NotImplementedException();
        }
    }
}
