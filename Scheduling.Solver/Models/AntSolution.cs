using Scheduling.Core.FJSP;

namespace Scheduling.Solver.Models
{
    public class AntSolution
    {
        public Dictionary<int, double> StartTimes { get; } = [];

        public Dictionary<int, double> CompletionTimes { get; } = [];

        public Dictionary<int, int> MachineAssignment { get; } = [];

        public Dictionary<int, List<Operation>> LoadingSequence { get; } = [];

        public double Makespan => CompletionTimes.Any() ? CompletionTimes.MaxBy(c => c.Value).Value : 0;
    }
}
