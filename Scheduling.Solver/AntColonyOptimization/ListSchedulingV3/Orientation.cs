using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Model;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3
{
    public class Orientation(DisjunctiveArc arc) : IFeasibleMove<DisjunctiveArc>
    {
        public DisjunctiveArc Arc { get; } = arc;
        public double Weight => Arc.Weight;

        public double GetPheromoneAmount(IPheromoneTrail<DisjunctiveArc> trail)
        {
            if (trail.TryGetValue(Arc, out var pheromone))
                return pheromone;

            Console.WriteLine($"Orientation {Arc.Tail}-{Arc.Machine}->{Arc.Head.Id} out of trail");
            return 0.0;
        }
    }
}
