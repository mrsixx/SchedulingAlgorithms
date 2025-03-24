using Scheduling.Solver.AntColonyOptimization.Ants;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.Solvers
{
    public class ParallelSolveApproach : ISolveApproach
    {

        public TAnt[] Solve<TPheromonePoint, TAnt>(int currentIteration, IAntColonyAlgorithm<TPheromonePoint, TAnt> solverContext, Func<int, int, TAnt> bugSpawner) where TAnt : BaseAnt
        {

            var ants = GenerateAntsWave(currentIteration, solverContext, bugSpawner);
            solverContext.Log($"#{currentIteration}th wave ants start to walk...");
            WaitForAntsToStop(ants);
            return ants.Select(a => (TAnt)a.Ant).ToArray();
        }

        private static AsyncAnt[] GenerateAntsWave<TPheromonePoint, TAnt>(int currentIteration, IAntColonyAlgorithm<TPheromonePoint, TAnt> solverContext,
            Func<int, int, TAnt> bugSpawner) where TAnt : BaseAnt
        {
            var ants = new AsyncAnt[solverContext.AntCount];
            for (int i = 0; i < solverContext.AntCount; i++)
                ants[i] = new AsyncAnt(bugSpawner(i + 1, currentIteration));
            return ants;
        }

        private static void WaitForAntsToStop(AsyncAnt[] ants) => Task.WaitAll(ants.Select(a => a.Task).ToArray());
    }
}
