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

        public GraphBuilderService(ILogger logger)
        {
            Logger = logger;
        }

        public ILogger? Logger { get; }

        public DisjunctiveGraphModel BuildDisjunctiveGraph(IEnumerable<Job> jobs, IEnumerable<Machine> machines)
        {
            var graph = new DisjunctiveGraphModel();
            graph.AddVertex(graph.Source);
            graph.AddVertex(graph.Sink);
            graph.Source.Operation.EligibleMachines.AddRange(machines);
            graph.Machines.AddRange(machines);
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
                        graph.AddConjunction(previousNode, operationNode);
                        previousNode = operationNode;
                        previousOperation = operation;
                    });

                //last operation of each job linked with sink
                if (previousOperation is not null)
                    graph.AddConjunction(previousNode, graph.Sink);
            });

            // create disjunctions between every operation running on same pool
            graph.GeneratePossibleDisjunctions().ToList()
                .ForEach(disjunction => graph.AddDisjunction(disjunction));
            return graph;
        }

        public DisjunctiveGraphModel BuildDisjunctiveGraphByBenchmarkFile(string benchmarkFile)
        {
            BenchmarkReader reader = new();
            reader.ReadInstance(benchmarkFile);

            Log($"Benchmark: {benchmarkFile}");
            Log($"Jobs: {reader.JobCount}; Operations: {reader.OperationCount}; Machines: {reader.MachineCount}");
            Log($"Trivial Upper bound: {reader.TrivialUpperBound}");

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
                                .Select((processingTime, machineIdx) => (processingTime, machines.First(m => machineIdx == m.Id - 1)))
                                .Where(pair => pair.processingTime < BenchmarkReader.INFINITY)
                                .Select(pair => pair.Item2)
                                .ToList();

                        Dictionary<Machine, long> dict = machines.Select(m => (m, processingTimes[m.Id - 1])).ToDictionary();
                        Operation operation = new(oId, dict)
                        {
                            JobId = jobId,
                            Job = job
                        };
                        operation.EligibleMachines.AddRange(eligibleMachines);
                        return operation;
                    });
                    job.Operations.AddRange(operations);
                    return job;
                })
                .ToList();
            return BuildDisjunctiveGraph(jobs, machines);
        }

        private void Log(string message) => Logger?.Log(message);
    }
}
