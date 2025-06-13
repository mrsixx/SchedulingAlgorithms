using Scheduling.Solver.AntColonyOptimization;
using Scheduling.Solver.Interfaces;
using System.Diagnostics;

namespace Scheduling.Solver.Models
{
    public class AntColonyOptimizationSolution<TAnt>(IColony<TAnt> colony) : IFjspSolution where TAnt : BaseAnt<TAnt>
    {
        public Dictionary<int, double> StartTimes { get; } = colony.EmployeeOfTheMonth?.ImprovedSolution?.StartTimes ?? [];

        public Dictionary<int, double> CompletionTimes { get; } = colony.EmployeeOfTheMonth?.ImprovedSolution?.CompletionTimes ?? [];

        public Dictionary<int, int> MachineAssignment { get; } = colony.EmployeeOfTheMonth?.ImprovedSolution?.MachineAssignment ?? [];

        public new double Makespan => colony.EmployeeOfTheMonth?.ImprovedSolution?.Makespan ?? 0;

        public Stopwatch Watch => colony.Watch;

        public void Log()
        {
            colony.EmployeeOfTheMonth?.Log();
        }
    }
}
