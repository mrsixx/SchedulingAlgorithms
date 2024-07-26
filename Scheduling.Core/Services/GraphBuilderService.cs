using Scheduling.Core.Benchmark;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;

namespace Scheduling.Core.Services
{
    public class GraphBuilderService : IGraphBuilderService
    {
        public DisjunctiveGraphModel BuildDisjunctiveGraph(IEnumerable<Job> jobs)
        {
            var graph = new DisjunctiveGraphModel();
            graph.AddVertex(graph.Source);
            graph.AddVertex(graph.Sink);
            jobs.ToList()
                .ForEach(job =>
            {
                // 1st operation of each job linked with source
                Node previousNode = graph.Source;
                Operation? previousOperation = null;
                
                job.Operations
                    .ForEach(operation =>
                    {
                        var operationNode = new Node(operation);
                        graph.AddVertex(operationNode);
                        if(previousOperation is null)
                            graph.AddEdge(new Conjunction(previousNode, operationNode, weight: job.ReleaseDate.Ticks));
                        else
                            operation.EligibleMachines.ForEach(m => graph.AddEdge(new Conjunction(previousNode, operationNode, operation.ProcessingTime(m))));
                        previousNode = operationNode;
                        previousOperation = operation;
                    });

                //last operation of each job linked with sink
                if (previousOperation is not null)
                    previousOperation.EligibleMachines.ForEach(m => graph.AddEdge(new Conjunction(previousNode, graph.Sink, previousOperation.ProcessingTime(m))));
            });

            // create disjunctions between every operation running on same pool
            jobs.SelectMany(j => j.Operations)
                .GeneratePossibleDisjunctions(graph)
                .ToList()
                .ForEach(disjunction => graph.AddEdge(disjunction));
            return graph;
        }

        public DisjunctiveGraphModel BuildDisjunctiveGraphByBenchmarkFile(string benchmarkFile)
        {
            BenchmarkReader reader = new();
            reader.ReadInstance(benchmarkFile);

            var machines = Enumerable
                .Range(1, reader.MachineCount)
                .Select(machineId => new Machine(machineId))
                .ToList();

            var jobs = Enumerable
                .Range(1, reader.JobCount)
                .Select((jobId, jobIdx) =>
                {
                    Job job = new(jobId);
                    var opCount = reader.JobOperationCount[jobIdx];
                    var operations = Enumerable.Range(0, opCount).Select((opIdx) =>
                    {
                        var oId = reader.OperationId[jobIdx][opIdx] + 1;
                        long[] processingTimes = reader.ProcessingTime[oId - 1];

                        var eligibleMachines = processingTimes
                                .Select((processingTime, machineIdx) => (processingTime, machines.Find(m => machineIdx == m.Id - 1)))
                                .Where(pair => pair.processingTime < BenchmarkReader.INFINITY)
                                .Select(pair => pair.Item2)
                                .ToList();

                        Operation operation = new(oId, processingTime: m => processingTimes[m.Id - 1]);
                        operation.EligibleMachines.AddRange(eligibleMachines);
                        return operation;
                    });
                    job.Operations.AddRange(operations);
                    return job;
                })
                .ToList();
            return BuildDisjunctiveGraph(jobs);
        }
    }
}
