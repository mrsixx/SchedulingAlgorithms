using Scheduling.Core.Graph;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization
{
    public interface IFeasibleMove
    {
        public double Weight { get; }

        public Orientation DirectedEdge { get; }

        public double GetPheromoneAmount(AntColonyOptimizationAlgorithmSolver context);
    }

    public class FeasibleMove(Disjunction disjunction, Direction direction) : IFeasibleMove
    {
        public Disjunction Disjunction { get; set; } = disjunction;

        public Direction Direction { get; set; } = direction;

        public double Weight => DirectedEdge.Weight;

        public Orientation DirectedEdge => Direction == Direction.SourceToTarget
                                            ? Disjunction.EquivalentConjunctions[0]
                                            : Disjunction.EquivalentConjunctions[1];

        public double GetPheromoneAmount(AntColonyOptimizationAlgorithmSolver context)
        {
            if (context.PheromoneTrail.TryGetValue(DirectedEdge, out double pheromone))
                return pheromone;
            
            Console.WriteLine($"Disjuntion {Disjunction} out of trail");
            return 0.0;
        }
    }
}
