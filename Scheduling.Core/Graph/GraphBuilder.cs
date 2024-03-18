using Scheduling.Core.Models;

namespace Scheduling.Core.Graph
{
    public class GraphBuilder
    {
        public DisjunctiveGraphModel BuildDisjunctiveGraph(List<Job> jobs, List<Machine> machines)
        {
            var graph = new DisjunctiveGraphModel();
            OperationVertex source = new OperationVertex(0), sink = new OperationVertex(Int32.MaxValue);
            graph.AddVertex(source);
            graph.AddVertex(sink);
            jobs.ForEach(job =>
            {
                var latest = source;
                job.Operations.Select(o => new OperationVertex(o.Id))
                    .ToList()
                    .ForEach(operation =>
                    {
                        graph.AddVertex(operation);
                        graph.AddEdge(new VertexEdge(latest, operation));
                        latest = operation;
                    });
                graph.AddEdge(new VertexEdge(latest, sink));
            });

            return graph;
        }
    }
}
