
using System.Text;
using System.Text.Json;
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

        public void ExportSolution(string instanceFile, string solverName, bool parallelApproach, IFjspSolution solution, string outputDir = "")
        {
            var approach = parallelApproach ? "parallel" : "iterative";
            var fileName = Path.GetFileName(instanceFile);
            var filePath = string.IsNullOrWhiteSpace(outputDir) ? Path.GetDirectoryName(instanceFile) : outputDir;
            
            var benchmarkName = Directory.GetParent(instanceFile)?.Name ?? "Benchmark";

            var outputFilename = $"{filePath}/{fileName}.{benchmarkName}-{solverName}-{approach}-m{solution.Makespan}-{DateTime.Now.Ticks}.json";
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            using StreamWriter writer = new(outputFilename);
            
            
            StringBuilder sb = new();
            sb.AppendLine(JsonSerializer.Serialize(solution) ?? "");
           
            writer.Write(sb);
        }
    }
}
