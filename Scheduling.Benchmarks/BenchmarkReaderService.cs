using Scheduling.Benchmarks.Interfaces;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
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
                            .Where(pair => pair.processingTime < Benchmark.INFINITY)
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

            return new Instance(jobs, machines);
        }


        private void Log(string message) => logger?.Log(message);
    }
}
