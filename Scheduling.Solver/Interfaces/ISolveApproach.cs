using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.Ants;
using Scheduling.Solver.AntColonyOptimization.Solvers;

namespace Scheduling.Solver.Interfaces
{
    public interface ISolveApproach<TPheromonePoint>
    {
        IPheromoneTrail<TPheromonePoint> CreatePheromoneTrail();

        T[] Solve<T>(int currentIteration, 
                    IAntColonyAlgorithm solverContext, 
                    Func<int, int, T> bugSpawner) where T : BaseAnt; 
    }
}
