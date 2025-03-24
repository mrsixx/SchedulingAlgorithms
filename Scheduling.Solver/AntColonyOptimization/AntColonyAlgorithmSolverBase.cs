using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization
{
    public abstract class AntColonyAlgorithmSolverBase(double alpha,
        double beta,
        double rho,
        double tau0,
        int ants,
        int iterations,
        int stagnantGenerationsAllowed)
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


        public abstract IPheromoneTrail<Orientation> PheromoneTrail { get; protected set; }
    

    }
}
