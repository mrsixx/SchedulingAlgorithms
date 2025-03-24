namespace Scheduling.Solver.Interfaces
{
    public interface IAntColonyAlgorithm : IFlexibleJobShopSchedulingSolver
    {
        /// <summary>
        /// Weight of pheromone factor constant
        /// </summary>
        public double Alpha { get; }

        /// <summary>
        /// Weight of distance factor constant
        /// </summary>
        public double Beta { get; }

        /// <summary>
        /// Pheromone evaporation rate constant
        /// </summary>
        public double Rho { get; }


        /// <summary>
        /// Initial pheromone amount over graph edges
        /// </summary>
        public double Tau0 { get; }

        /// <summary>
        /// Amount of ants
        /// </summary>
        public int AntCount { get; }

        /// <summary>
        /// Number of iterations
        /// </summary>
        public int Iterations { get; }


        /// <summary>
        /// How long should ants continue without improving the solution
        /// </summary>
        public int StagnantGenerationsAllowed { get; }
    }
}
