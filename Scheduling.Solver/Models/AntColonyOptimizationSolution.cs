using Scheduling.Solver.AntColonyOptimization;
using Scheduling.Solver.Interfaces;
using System.Diagnostics;
using Scheduling.Core.FJSP;

namespace Scheduling.Solver.Models
{
    public class AntColonyOptimizationSolution<TAnt>(IColony<TAnt> colony) : IFjspSolution where TAnt : BaseAnt<TAnt>
    {
        public Dictionary<int, double> StartTimes { get; } = colony.EmployeeOfTheMonth?.Solution?.StartTimes ?? [];

        public Dictionary<int, double> CompletionTimes { get; } = colony.EmployeeOfTheMonth?.Solution?.CompletionTimes ?? [];

        public Dictionary<int, int> MachineAssignment { get; } = colony.EmployeeOfTheMonth?.Solution?.MachineAssignment ?? [];

        public new double Makespan => colony.EmployeeOfTheMonth?.Solution?.Makespan ?? 0;
        public Dictionary<int, List<Operation>> LoadingSequence { get; } = colony.EmployeeOfTheMonth?.Solution?.LoadingSequence ?? [];
        public int? CriticalOperationId { get; } = colony.EmployeeOfTheMonth?.Solution?.CriticalOperationId;
        public Dictionary<int, Operation?> CriticalPredecessors { get; } = colony.EmployeeOfTheMonth?.Solution?.CriticalPredecessors ?? [];

        public Stopwatch Watch => colony.Watch;

        public void Log()
        {
            colony.EmployeeOfTheMonth?.Log();
        }
    }
}
