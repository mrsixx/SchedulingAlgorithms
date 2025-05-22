using Scheduling.Core.FJSP;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2
{
    public class FeasibleMoveV2(Operation operation, Machine machine) : IFeasibleMove<Allocation>
    {
        public double Weight { get; } = operation.GetProcessingTime(machine);

        public Allocation Allocation { get; } = new(operation, machine);

        public Machine Machine => Allocation.Machine;


        public Operation Operation => Allocation.Operation;

        public double GetPheromoneAmount(IPheromoneStructure<Allocation> trail)
        {
            if (trail.TryGetValue(Allocation, out var pheromone))
                return pheromone;

            Console.WriteLine($"Allocation O{operation.Id}M{machine.Id} out of trail");
            return 0.0;
        }
    }
}
