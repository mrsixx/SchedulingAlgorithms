using Scheduling.Core.FJSP;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3
{
    public class FeasibleMoveV3(OperationVertex vertex, Machine machine) : IFeasibleMove<Allocation>
    {
        public double Weight { get; } = vertex.Operation.GetProcessingTime(machine);

        public Allocation Allocation { get; } = new(vertex.Operation, machine);

        public Machine Machine => Allocation.Machine;

        public OperationVertex Vertex { get; } = vertex;

        public Operation Operation => Vertex.Operation;

        public double GetPheromoneAmount(IPheromoneTrail<Allocation> trail)
        {
            if (trail.TryGetValue(Allocation, out var pheromone))
                return pheromone;

            Console.WriteLine($"Allocation O{vertex.Id}M{machine.Id} out of trail");
            return 0.0;
        }
    }
}
