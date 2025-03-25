using Scheduling.Solver.AntColonyOptimization.Pheromone;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class IterativeSolveApproach : ISolveApproach
    {
        public IPheromoneTrail<TPheromonePoint> CreatePheromoneTrail<TPheromonePoint>() where TPheromonePoint : notnull, new() => new PheromoneTrail<TPheromonePoint>();

        public TAnt[] Solve<TPheromonePoint, TAnt>(int currentIteration, IAntColonyAlgorithm<TPheromonePoint, TAnt> solverContext, Func<int, int, TAnt> bugSpawner) where TAnt : BaseAnt<TAnt>
        {
            var ants = new TAnt[solverContext.AntCount];
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
