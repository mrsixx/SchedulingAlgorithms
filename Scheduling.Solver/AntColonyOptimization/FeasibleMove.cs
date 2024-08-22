﻿using Scheduling.Core.Graph;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization
{
    public interface IFeasibleMove
    {
        public double Weight { get; }

        public double PheromoneAmount { get; }

        public Orientation DirectedEdge { get; }
    }

    public class FeasibleMove(Disjunction disjunction, Direction direction) : IFeasibleMove
    {
        public Disjunction Disjunction { get; set; } = disjunction;

        public Direction Direction { get; set; } = direction;


        public double Weight => DirectedEdge.Weight;

        public double PheromoneAmount => Direction == Direction.SourceToTarget
                                        ? Disjunction.Pheromone.SourceToTarget
                                        : Disjunction.Pheromone.TargetToSource;

        public Orientation DirectedEdge => Direction == Direction.SourceToTarget
                                            ? Disjunction.EquivalentConjunctions[0]
                                            : Disjunction.EquivalentConjunctions[1];
    }
}