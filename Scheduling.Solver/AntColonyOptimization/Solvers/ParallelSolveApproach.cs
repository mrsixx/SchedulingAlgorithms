using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.Ants;
using Scheduling.Solver.AntColonyOptimization.Pheromone;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.Solvers
{
    public class ParallelSolveApproach : ISolveApproach<Orientation>
    {
        public IPheromoneTrail<Orientation> CreatePheromoneTrail() => new ThreadSafePheromoneTrail();

        public T[] Solve<T>(int currentIteration, IAntColonyAlgorithm solverContext, Func<int, int, T> bugSpawner) where T : BaseAnt
        {
            
            var ants = GenerateAntsWave(currentIteration, solverContext, bugSpawner);
            solverContext.Log($"#{currentIteration}th wave ants start to walk...");
            WaitForAntsToStop(ants);
            return ants.Select(a => (T)a.Ant).ToArray();
        }

        private static AsyncAnt[] GenerateAntsWave<T>(int currentIteration, IAntColonyAlgorithm solverContext,
            Func<int, int, T> bugSpawner) where T : BaseAnt
        {
            var ants = new AsyncAnt[solverContext.AntCount];
            for (int i = 0; i < solverContext.AntCount; i++)
                ants[i] = new AsyncAnt(bugSpawner(i + 1, currentIteration));
            return ants;
        }

        private static void WaitForAntsToStop(AsyncAnt[] ants) => Task.WaitAll(ants.Select(a => a.Task).ToArray());
    }
}
