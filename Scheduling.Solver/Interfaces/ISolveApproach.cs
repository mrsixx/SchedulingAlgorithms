using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.Ants;

namespace Scheduling.Solver.Interfaces
{
    public interface ISolveApproach
    {
        TAnt[] Solve<TPheromonePoint, TAnt>(int currentIteration, 
                    IAntColonyAlgorithm<TPheromonePoint,TAnt> solverContext, 
                    Func<int, int, TAnt> bugSpawner) where TAnt : BaseAnt; 
    }
}
