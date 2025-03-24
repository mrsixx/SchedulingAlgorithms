using Scheduling.Core.Graph;
using Scheduling.Solver.Interfaces;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class FeasibleMove(Disjunction disjunction, Direction direction) : IFeasibleMove<Orientation>
    {

        public Disjunction Disjunction { get; set; } = disjunction;

        public Direction Direction { get; set; } = direction;

        public double Weight => DirectedEdge.Weight;

        public Orientation DirectedEdge => Direction == Direction.SourceToTarget
                                            ? Disjunction.Orientations[0]
                                            : Disjunction.Orientations[1];

        public double GetPheromoneAmount(IPheromoneTrail<Orientation> trail)
        {
            if (trail.TryGetValue(DirectedEdge, out var pheromone))
                return pheromone;
            
            Console.WriteLine($"Disjunction {Disjunction} out of trail");
            return 0.0;
        }
    }
}
