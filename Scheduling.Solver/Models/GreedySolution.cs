using Scheduling.Solver.Interfaces;
using System.Diagnostics;
using Scheduling.Core.FJSP;
using Machine = Scheduling.Core.FJSP.Machine;

namespace Scheduling.Solver.Models
{
    public class GreedySolution : IFjspSolution
    {
        public Dictionary<int, double> StartTimes { get; } = [];

        public Dictionary<int, double> CompletionTimes { get; } = [];

        public Dictionary<int, int> MachineAssignment { get; } = [];

        public Dictionary<Machine, double> MachineOccupancy {get;} = [];

        public Dictionary<int, Operation?> CriticalPredecessors { get; } = [];

        public Dictionary<int, List<Operation>> LoadingSequence { get; } = [];
        public int? CriticalOperationId => CompletionTimes.Any() ? CompletionTimes.MaxBy(c => c.Value).Key : null;

        public double Makespan => CriticalOperationId.HasValue ? CompletionTimes[CriticalOperationId.Value] : 0;

        public Stopwatch Watch { get; } = new();

        public void Log()
        {
            Console.WriteLine($"Makespan: {Makespan}");
        }
    }
}
