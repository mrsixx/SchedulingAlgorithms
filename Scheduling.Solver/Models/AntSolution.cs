using System.Diagnostics;
using Scheduling.Core.FJSP;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.Models
{
    public class AntSolution() : IFjspSolution
    {
        public Dictionary<int, double> StartTimes { get; } = [];

        public Dictionary<int, double> CompletionTimes { get; } = [];

        public Dictionary<int, int> MachineAssignment { get; } = [];
        
        public Dictionary<int, List<Operation>> LoadingSequence { get; } = [];

        public Dictionary<int, Operation?> CriticalPredecessors { get; } = [];

        public int? CriticalOperationId => CompletionTimes.Any() ? CompletionTimes.MaxBy(c => c.Value).Key : null;

        public double Makespan => CriticalOperationId.HasValue ? CompletionTimes[CriticalOperationId.Value] : 0;

        public Stopwatch Watch { get; }

        public void Log()
        {
            throw new NotImplementedException();
        }

    }
}
