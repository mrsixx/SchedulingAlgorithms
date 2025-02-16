using Scheduling.Core.Benchmark;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;

namespace Scheduling.Core.Services
{
    public class GraphBuilderService : IGraphBuilderService
    {
        public GraphBuilderService() { }

        public GraphBuilderService(ILogger? logger)
        {
            Logger = logger;
        }

        public ILogger? Logger { get; }

        public DisjunctiveGraphModel BuildDisjunctiveGraph(Instance fjspInstance)
        {
            var graph = new DisjunctiveGraphModel();
            graph.AddVertex(graph.Source);
            graph.AddVertex(graph.Sink);
            graph.Source.Operation.EligibleMachines.AddRange(fjspInstance.Machines);
            graph.Machines.AddRange(fjspInstance.Machines);
            fjspInstance.Jobs.ToList()
                .ForEach(job =>
            {
                // 1st operation of each job linked with source
                Node previousNode = graph.Source;
                Operation? previousOperation = null;
                var currentOperation = job.Operations.First;
                while (currentOperation is not null)
                {
                    var operationNode = new Node(currentOperation.Value);
                    graph.AddVertex(operationNode);
                    graph.AddConjunction(previousNode, operationNode);
                    previousNode = operationNode;
                    currentOperation = currentOperation.Next;
                }

                //last operation of each job linked with sink
                graph.AddConjunction(previousNode, graph.Sink);
            });

            // create disjunctions between every operation running on same pool
            graph.GeneratePossibleDisjunctions().ToList()
                .ForEach(disjunction => graph.AddDisjunction(disjunction));
            return graph;
        }
    }
}
