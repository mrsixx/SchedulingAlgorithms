using Scheduling.Core.FJSP;
using Scheduling.Solver.Interfaces;
using System.Reflection.PortableExecutable;
using Machine = Scheduling.Core.FJSP.Machine;

namespace Scheduling.Solver.Models
{
    public class GreedySolution : IFjspSolution
    {
        public Dictionary<Operation, double> StartTimes { get; } = [];

        public Dictionary<Operation, double> CompletionTimes { get; } = [];

        public Dictionary<Operation, Machine> MachineAssignment { get; } = [];

        public double Makespan => CompletionTimes.Count > 0 ? CompletionTimes.MaxBy(c => c.Value).Value : 0;
        public void Log()
        {
            Console.WriteLine($"Makespan: {Makespan}");
        }
    }
}
