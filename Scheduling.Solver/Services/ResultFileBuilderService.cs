
using System.Text;
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
            var outputFilename = $"{filePath}/{fileName}.{solverName}-{approach}.csv";
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
    }
}
