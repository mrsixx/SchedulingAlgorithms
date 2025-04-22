using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;

namespace Scheduling.Solver.Interfaces
{
    public interface IAntColonyAlgorithm<TPheromonePoint, out TAnt> : IFlexibleJobShopSchedulingSolver
    {
        int AntCount { get; }

        /// <summary>
        /// Run (DORIGO; STUTZLE, 2004) parameter settings for ACO algorithms without local search
        /// </summary>
        /// <param name="instance"></param>
        void DorigosTouch(Instance instance);


        /// <summary>
        /// Pheromone trail data structure
        /// </summary>
        IPheromoneTrail<TPheromonePoint> PheromoneTrail { get; }

        TAnt[] BugsLife(int currentIteration);

    }
}
