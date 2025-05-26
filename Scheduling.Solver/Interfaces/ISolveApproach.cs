using Scheduling.Solver.AntColonyOptimization;
using System.Collections.Generic;

namespace Scheduling.Solver.Interfaces
{
    public interface ISolveApproach
    {
        IPheromoneStructure<TPheromonePoint> CreatePheromoneTrail<TPheromonePoint>() where TPheromonePoint : notnull;

        TAnt[] LastMarchOfTheAnts<TPheromonePoint, TAnt>(int currentIteration,
                    IAntColonyAlgorithm<TPheromonePoint, TAnt> solverContext,
                    Func<int, int, TAnt> bugSpawner) where TAnt : BaseAnt<TAnt>;
    }
}
