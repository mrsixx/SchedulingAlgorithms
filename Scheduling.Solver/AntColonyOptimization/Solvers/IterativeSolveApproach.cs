using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.Ants;
using Scheduling.Solver.AntColonyOptimization.Pheromone;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.Solvers
{
    public class IterativeSolveApproach : ISolveApproach<Orientation>
    {
        public IPheromoneTrail<Orientation> CreatePheromoneTrail() => new PheromoneTrail();

        public T[] Solve<T>(int currentIteration, IAntColonyAlgorithm solverContext, Func<int, int, T> bugSpawner) where T : BaseAnt
        {
            var ants = new T[solverContext.AntCount];
            solverContext.Log($"#{currentIteration}th wave ants start to walk...");
            for (int j = 0; j < solverContext.AntCount; j++)
            {
                var antId = j + 1;
                ants[j] = bugSpawner(antId, currentIteration);
                ants[j].WalkAround();
            }
            return ants;
        }
    }
}
