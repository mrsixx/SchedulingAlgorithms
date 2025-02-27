using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.Ants;
using Scheduling.Solver.AntColonyOptimization.Solvers;

namespace Scheduling.Solver.Interfaces
{
    public interface ISolveApproach
    {
        IPheromoneTrail<Orientation, double> CreatePheromoneTrail();

        T[] Solve<T>(int currentIteration, AntColonyOptimizationAlgorithmSolver solverContext, Func<int, int, T> bugSpawner) where T : BaseAnt; 
    }
}
