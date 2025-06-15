using System.Runtime.InteropServices;

namespace Scheduling.Solver.AntColonyOptimization
{
    public record Parameters(
        double alpha,
        double beta,
        double rho,
        double tau0,
        int ants,
        int iterations,
        int stagnantGenerationsAllowed,
        bool disableLocalSearch,
        bool enableDorigosTouch)
    {
        /// <summary>
        /// Weight of pheromone factor constant
        /// </summary>
        public double Alpha { get; set; } = alpha;

        /// <summary>
        /// Weight of distance factor constant
        /// </summary>
        public double Beta { get; init; } = beta;

        /// <summary>
        /// Pheromone evaporation rate constant
        /// </summary>
        public double Rho { get; set; } = rho;


        /// <summary>
        /// Initial pheromone amount over graph edges
        /// </summary>
        public double Tau0 { get; set; } = tau0;

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

        /// <summary>
        /// Disable Local Search
        /// </summary>
        public bool DisableLocalSearch { get; init; } = disableLocalSearch;

        /// <summary>
        /// Enable Dorigo's Parametrization
        /// </summary>
        public bool EnableDorigosTouch { get; init; } = enableDorigosTouch;
    }
}
