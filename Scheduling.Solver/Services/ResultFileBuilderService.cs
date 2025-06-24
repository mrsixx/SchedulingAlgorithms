
using System.Text;
using System.Text.Json;
using Scheduling.Core.FJSP;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.Services
{
    public class ResultFileBuilderService : IResultFileBuilderService
    {

        public void Export(string instanceFile, string solverName, bool parallelApproach, IEnumerable<IFjspSolution> solutions, string outputDir = "")
        {
            var approach = parallelApproach ? "parallel" : "iterative";
            var fileName = Path.GetFileName(instanceFile);
            var filePath = string.IsNullOrWhiteSpace(outputDir) ? Path.GetDirectoryName(instanceFile) : outputDir;
            
            var benchmarkName = Directory.GetParent(instanceFile)?.Name ?? "Benchmark";

            var outputFilename = $"{filePath}/{fileName}.{benchmarkName}-{solverName}-{approach}.csv";
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            using StreamWriter writer = new(outputFilename);
            
            StringBuilder sb = new();
            sb.AppendLine(string.Join(',', ["Makespan", "Ellapsed(ms)"]));

            foreach(var solution in solutions) {
                sb.AppendLine(string.Join(',', [solution.Makespan, Convert.ToString(solution.Watch.ElapsedMilliseconds)]));
            }

            writer.Write(sb);
        }

        public void ExportSolution(string instanceFile, string solverName, bool parallelApproach, IFjspSolution solution, bool withLocalSearch, string outputDir = "")
        {
            var approach = parallelApproach ? "parallel" : "iterative";
            var localSeach = withLocalSearch ? "ls" : "";
            var fileName = Path.GetFileName(instanceFile);
            var filePath = string.IsNullOrWhiteSpace(outputDir) ? Path.GetDirectoryName(instanceFile) : outputDir;
            
            var benchmarkName = Directory.GetParent(instanceFile)?.Name ?? "Benchmark";

            var outputFilename = $"{filePath}/{fileName}.{benchmarkName}-{solverName}-{approach}-{localSeach}-m{solution.Makespan}-{DateTime.Now.Ticks}.json";
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            using StreamWriter writer = new(outputFilename);
            
            
            StringBuilder sb = new();
            var sol = new
            {
                Operations = solution.StartTimes.Keys,
                Machines = solution.LoadingSequence.Keys,
                solution.StartTimes,
                solution.MachineAssignment,
                LoadingSequence = solution.LoadingSequence.Aggregate(new List<List<int>>(), (acc, seq) =>
                {
                    var sequence = seq.Value.Select(o => o.Id).ToList();
                    acc.Add(sequence);
                    return acc;
                })
            };
            sb.AppendLine(JsonSerializer.Serialize(sol) ?? "");
           
            writer.Write(sb);
        }
    }
}
