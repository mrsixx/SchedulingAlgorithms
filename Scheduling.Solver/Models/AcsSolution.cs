using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.Models
{
    public class AcsSolution(Colony colony) : IFjspSolution
    {
        public Colony Context { get; } = colony;

        public Dictionary<Operation, double> StartTimes { get; } = colony.EmployeeOfTheMonth?.StartTimes ?? [];

        public Dictionary<Operation, double> CompletionTimes { get; } = colony.EmployeeOfTheMonth?.CompletionTimes ?? [];

        public Dictionary<Operation, Machine> MachineAssignment { get; } = colony.EmployeeOfTheMonth?.MachineAssignment ?? [];

        public new double Makespan => colony.EmployeeOfTheMonth?.Makespan ?? 0;
        public void Log()
        {
            Context.EmployeeOfTheMonth.Log();
        }
    }
}
