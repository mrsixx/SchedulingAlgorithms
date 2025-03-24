using Scheduling.Core.FJSP;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2
{
    public record Allocation(Operation operation, Machine machine)
    {
        public Operation Operation { get; } = operation;

        public Machine Machine { get; } = machine;

    }
}
