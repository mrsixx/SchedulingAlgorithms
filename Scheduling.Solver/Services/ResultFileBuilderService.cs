
using System.Text;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.Services
{
    public class ResultFileBuilderService : IResultFileBuilderService
    {
        public void Export(string instanceFile, string solverName, IEnumerable<IFjspSolution> solutions)
        {
            var filenameSplit = instanceFile.Split(".fjs");
            var outputFilename = $"{filenameSplit[0]}.{solverName}.csv";
            using StreamWriter writer = new(outputFilename);
            
            StringBuilder sb = new();
            sb.AppendLine(string.Join(',', ["Makespan", "Ellapsed(ms)"]));

            foreach(var solution in solutions){
                sb.AppendLine(string.Join(',', [solution.Makespan, Convert.ToString(solution.Watch.ElapsedMilliseconds)]));
            }

            writer.Write(sb);

        }
    }
}
