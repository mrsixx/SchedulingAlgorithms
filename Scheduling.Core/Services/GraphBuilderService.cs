using Scheduling.Core.Benchmark;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;

namespace Scheduling.Core.Services
{
    public class GraphBuilderService : IGraphBuilderService
    {
        public DisjunctiveGraphModel BuildDisjunctiveGraph(List<Job> jobs, List<MachinePool> machinePools)
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
            jobs.SelectMany(j => j.Operations).GeneratePossibleDisjunctions(machinePools)
                .ToList()
                .ForEach(disjunction =>
                {
                    var (o1, o2, machine) = disjunction;
                    // if some node not found
                    if (graph.TryGetNode(o1.Id, out Node node1) && graph.TryGetNode(o2.Id, out Node node2))
                        graph.AddEdge(new Disjunction(node1, node2, machine.Id));

                });
            return graph;
        }

        public DisjunctiveGraphModel BuildDisjunctiveGraphByBenchmarkFile(string benchmarkFile)
        {
            BenchmarkReader reader = new();
            reader.ReadInstance(benchmarkFile);

            var machinePools = new List<MachinePool>();
            var machines = Enumerable
                .Range(1, reader.MachineCount)
                .Select(machineId => new Machine(machineId));

            var jobs = Enumerable
                .Range(1, reader.JobCount)
                .Select((jobId, jobIdx) =>
                {
                    Job job = new(jobId);
                    var opCount = reader.JobOperationCount[jobIdx];
                    var operations = Enumerable.Range(0, opCount).Select((opIdx) =>
                    {
                        var oId = reader.OperationId[jobIdx][opIdx] + 1;
                        var processingTimes = reader.ProcessingTime[oId - 1];

                        MachinePool pool = new(oId);
                        machinePools.Add(pool);
                        pool.Machines.AddRange(
                            processingTimes
                                .Select((time, machineIdx) => time != BenchmarkReader.INFINITY
                                    ? machines.ToList().Find(m => machineIdx == m.Id - 1)
                                    : null)
                                .Where(m => m is not null));

                        Operation operation = new(oId, machinePoolId: oId, processingTime: m => processingTimes[m.Id - 1]);
                        return operation;
                    });
                    job.Operations.AddRange(operations);
                    return job;
                })
                .ToList();
            return BuildDisjunctiveGraph(jobs, machinePools);
        }
    }
}
