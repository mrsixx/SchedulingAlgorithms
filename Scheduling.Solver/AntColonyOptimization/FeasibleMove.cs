using Scheduling.Core.Graph;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization
{
    public interface IFeasibleMove
    {
        public double Weight { get; }

        public double PheromoneAmount { get; }

        public Conjunction DirectedEdge { get; }
    }

    public class DisjunctiveFeasibleMove(Disjunction disjunction, Direction direction) : IFeasibleMove
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

    public class ConjunctiveFeasibleMove(Conjunction conjunction) : IFeasibleMove
    {
        public Conjunction Conjunction { get; set; } = conjunction;

        public double Weight => Conjunction.Source.IsDummyNode ? 1 : DirectedEdge.Weight;

        public double PheromoneAmount => Conjunction.Pheromone;

        public Conjunction DirectedEdge => Conjunction;
    }
}
