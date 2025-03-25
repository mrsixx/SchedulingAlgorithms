using Scheduling.Solver.Interfaces;
using System.Diagnostics;
using Machine = Scheduling.Core.FJSP.Machine;

namespace Scheduling.Solver.Models
{
    public class GreedySolution : IFjspSolution
    {
        public Dictionary<int, double> StartTimes { get; } = [];

        public Dictionary<int, double> CompletionTimes { get; } = [];

        public Dictionary<int, Machine> MachineAssignment { get; } = [];

        public double Makespan => CompletionTimes.Count > 0 ? CompletionTimes.MaxBy(c => c.Value).Value : 0;

        public Stopwatch Watch { get; } = new();

        public void Log()
        {
            Console.WriteLine($"Makespan: {Makespan}");
        }
    }
}
