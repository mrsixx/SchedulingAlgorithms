using Scheduling.Solver.AntColonyOptimization.Pheromone;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class ParallelSolveApproach : ISolveApproach
    {
        public IPheromoneStructure<TPheromonePoint> CreatePheromoneTrail<TPheromonePoint>() where TPheromonePoint : notnull 
            => new ThreadSafePheromoneStructure<TPheromonePoint>();

        public TAnt[] LastMarchOfTheAnts<TPheromonePoint, TAnt>(
            int currentIteration, 
            IAntColonyAlgorithm<TPheromonePoint, TAnt> solverContext, 
            Func<int, int, TAnt> bugSpawner) where TAnt : BaseAnt<TAnt>
        {
            var ants = Enumerable.Range(0, solverContext.AntCount)
                                        .Select(i => bugSpawner(i + 1, currentIteration)).ToArray();
            solverContext.Log($"#{currentIteration}th wave ants start to walk...");
            return ants.AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Select(a =>
                {
                    a.WalkAround();
                    return a;
                })
                .ToArray();
        }

    }
}
