using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization;
using System.Drawing;

namespace Scheduling.Solver.Models
{
    public class Solution(Colony colony)
    {
        public Colony Context { get; } = colony;

        public Dictionary<Operation, double> StartTimes { get; } = colony.EmployeeOfTheMonth?.StartTimes ?? [];

        public Dictionary<Operation, double> CompletionTimes { get; } = colony.EmployeeOfTheMonth?.CompletionTimes ?? [];
        
        public Dictionary<Operation, Machine> MachineAssignment { get; } = colony.EmployeeOfTheMonth?.MachineAssignment ?? [];

        public new double Makespan => colony.EmployeeOfTheMonth?.Makespan ?? 0;

    }
}
