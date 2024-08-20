using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization;

namespace Scheduling.Solver.Models
{
    public class FjspSolution(Colony colony)
    {
        public Colony Context { get; } = colony;


        public double Makespan => Context.EmployeeOfTheMonth.Makespan;

        public Dictionary<Operation, Machine> MachineAssignment { get; } = [];
        
        public Dictionary<Operation, DateTime> StartTimes { get; } = [];

    }
}
