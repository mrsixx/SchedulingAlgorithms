using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2
{
    public class AntColonyV2AlgorithmSolver<TAnt>(
        double alpha,
        double beta,
        double rho,
        double tau0,
        int ants,
        int iterations,
        int stagnantGenerationsAllowed,
        ISolveApproach solveApproach) : IAntColonyAlgorithm<Allocation, TAnt>
    {
        /// <summary>
        /// Weight of pheromone factor constant
        /// </summary>
        public double Alpha { get; init; } = alpha;

        /// <summary>
        /// Weight of distance factor constant
        /// </summary>
        public double Beta { get; init; } = beta;

        /// <summary>
        /// Pheromone evaporation rate constant
        /// </summary>
        public double Rho { get; init; } = rho;


        /// <summary>
        /// Initial pheromone amount over graph edges
        /// </summary>
        public double Tau0 { get; init; } = tau0;

        /// <summary>
        /// Amount of ants
        /// </summary>
        public int AntCount { get; init; } = ants;

        /// <summary>
        /// Number of iterations
        /// </summary>
        public int Iterations { get; init; } = iterations;

        /// <summary>
        /// How long should ants continue without improving the solution
        /// </summary>
        public int StagnantGenerationsAllowed { get; init; } = stagnantGenerationsAllowed;

        public IPheromoneTrail<Allocation> PheromoneTrail { get; }

        public TAnt[] BugsLife(int currentIteration)
        {
            throw new NotImplementedException();
        }

        public IFlexibleJobShopSchedulingSolver WithLogger(ILogger logger, bool with = false)
        {
            throw new NotImplementedException();
        }

        public IFjspSolution Solve(Instance instance)
        {
            throw new NotImplementedException();
        }

        public void Log(string message)
        {
            throw new NotImplementedException();
        }
    }
}
