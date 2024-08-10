using Scheduling.Core.Graph;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class FeasibleMove(Disjunction disjunction, Direction direction)
    {
        public Disjunction Disjunction { get; set; } = disjunction;

        public Direction Direction { get; set; } = direction;


        public double Weight => DirectedEdge.Weight;

        public double PheromoneAmount => Direction == Direction.SourceToTarget 
                                        ? Disjunction.Pheromone.SourceToTarget 
                                        : Disjunction.Pheromone.TargetToSource;

        public Conjunction DirectedEdge => Direction == Direction.SourceToTarget
                                            ? Disjunction.EquivalentConjunctions[0]
                                            : Disjunction.EquivalentConjunctions[1];
    }
}
