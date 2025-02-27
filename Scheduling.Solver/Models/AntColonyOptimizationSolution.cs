using System.Diagnostics;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.Models
{
    public class AntColonyOptimizationSolution(Colony colony) : IFjspSolution
    {
        public Colony Context { get; } = colony;

        public Dictionary<int, double> StartTimes { get; } = colony.EmployeeOfTheMonth?.StartTimes ?? [];

        public Dictionary<int, double> CompletionTimes { get; } = colony.EmployeeOfTheMonth?.CompletionTimes ?? [];

        public Dictionary<int, Machine> MachineAssignment { get; } = colony.EmployeeOfTheMonth?.MachineAssignment ?? [];

        public new double Makespan => colony.EmployeeOfTheMonth?.Makespan ?? 0;

        public Stopwatch Watch => colony.Watch;
        
        public void Log()
        {
            Context.EmployeeOfTheMonth.Log();
        }
    }
}
