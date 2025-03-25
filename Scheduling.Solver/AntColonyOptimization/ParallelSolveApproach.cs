using Scheduling.Solver.AntColonyOptimization.Pheromone;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class ParallelSolveApproach : ISolveApproach
    {
        public IPheromoneTrail<TPheromonePoint> CreatePheromoneTrail<TPheromonePoint>() where TPheromonePoint : notnull 
            => new ThreadSafePheromoneTrail<TPheromonePoint>();

        public TAnt[] Solve<TPheromonePoint, TAnt>(
            int currentIteration, 
            IAntColonyAlgorithm<TPheromonePoint, TAnt> solverContext, 
            Func<int, int, TAnt> bugSpawner) where TAnt : BaseAnt<TAnt>
        {

            var ants = GenerateAntsWave(currentIteration, solverContext, bugSpawner);
            solverContext.Log($"#{currentIteration}th wave ants start to walk...");
            WaitForAntsToStop(ants);
            return ants.Select(a => (TAnt)a.Ant).ToArray();
        }

        private static AsyncAnt<TAnt>[] GenerateAntsWave<TPheromonePoint, TAnt>(int currentIteration, IAntColonyAlgorithm<TPheromonePoint, TAnt> solverContext,
            Func<int, int, TAnt> bugSpawner) where TAnt : BaseAnt<TAnt>
        {
            var ants = new AsyncAnt<TAnt>[solverContext.AntCount];
            for (int i = 0; i < solverContext.AntCount; i++)
                ants[i] = new AsyncAnt<TAnt>(bugSpawner(i + 1, currentIteration));
            return ants;
        }

        private static void WaitForAntsToStop<TAnt>(AsyncAnt<TAnt>[] ants) where TAnt : BaseAnt<TAnt>
        {
            Task.WaitAll(ants.Select(a => a.Task).ToArray());
        } 
    }
}
