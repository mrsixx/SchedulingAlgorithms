using Scheduling.Core.Graph;

namespace Scheduling.Solver.Interfaces
{
    public interface IAntColonyAlgorithm<TPheromonePoint, out TAnt> : IFlexibleJobShopSchedulingSolver
    {
        int AntCount { get; }

        /// <summary>
        /// Pheromone trail data structure
        /// </summary>
        IPheromoneTrail<TPheromonePoint> PheromoneTrail { get; }

        TAnt[] BugsLife(int currentIteration);
    }
}
