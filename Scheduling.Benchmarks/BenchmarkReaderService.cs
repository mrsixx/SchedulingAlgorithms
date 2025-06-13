using Scheduling.Benchmarks.Interfaces;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.Greedy;
using Scheduling.Core.Interfaces;

namespace Scheduling.Benchmarks
{
    public class BenchmarkReaderService(ILogger logger) : IBenchmarkReaderService
    {
        public Instance ReadInstance(string file)
        {
            Benchmark reader = Benchmark.FromFile(file);
            
            Log($"Benchmark: {file}");
            Log($"Jobs: {reader.JobCount}; Operations: {reader.OperationCount}; Machines: {reader.MachineCount}");
            Log($"Trivial Upper bound: {reader.TrivialUpperBound}");

            var machines = Enumerable
                .Range(1, reader.MachineCount)
                .Select(machineId => new Machine(machineId){Index = machineId-1})
                .ToArray();

            var jobs = Enumerable
                .Range(1, reader.JobCount)
                .Select((jobId, jobIdx) =>
                {
                    Job job = new(jobId);
                    var opCount = reader.JobOperationCount[jobIdx];
                    job.Operations = new Operation[opCount];
                    Enumerable.Range(0, opCount).ForEach((opIdx) =>
                    {
                        var oId = reader.OperationId[jobIdx][opIdx] + 1;
                        long[] processingTimes = reader.ProcessingTime[oId - 1];
                        var eligibleMachines = processingTimes
                            .Select((processingTime, machineIdx) => (processingTime, machines.First(m => machineIdx == m.Id - 1)))
                            .Where(pair => pair.processingTime < Benchmark.INFINITY)
                            .Select(pair => pair.Item2)
                            .ToList();

                        Dictionary<Machine, long> dict = machines.Select(m => (m, processingTimes[m.Id - 1])).ToDictionary();
                        Operation operation = new(oId, dict)
                        {
                            JobId = jobId,
                            Job = job,
                            Index = opIdx
                        };
                        operation.EligibleMachines.AddRange(eligibleMachines);
                        job.Operations[opIdx] = operation;
                    });
                    return job;
                })
                .ToList();

            var instance = new Instance(jobs, machines);
            
            instance.MachinesPerOperation = reader.MachinePerOperationAvg;
            instance.TrivialUpperBound = reader.TrivialUpperBound;
            instance.UpperBound = new LeastLoadedMachineHeuristicAlgorithmSolver().Solve(instance).Makespan;
            return instance;
        }


        private void Log(string message) => logger?.Log(message);
    }
}
