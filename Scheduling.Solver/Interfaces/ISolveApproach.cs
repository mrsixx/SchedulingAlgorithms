using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization;

namespace Scheduling.Solver.Interfaces
{
    public interface ISolveApproach
    {
        IPheromoneTrail<TPheromonePoint> CreatePheromoneTrail<TPheromonePoint>() where TPheromonePoint : notnull;

        TAnt[] Solve<TPheromonePoint, TAnt>(int currentIteration, 
                    IAntColonyAlgorithm<TPheromonePoint,TAnt> solverContext, 
                    Func<int, int, TAnt> bugSpawner) where TAnt : BaseAnt<TAnt>; 
    }
}
