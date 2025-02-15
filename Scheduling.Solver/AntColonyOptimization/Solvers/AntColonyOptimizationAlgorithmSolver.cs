using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;
using Scheduling.Core.Services;
using Scheduling.Solver.AntColonyOptimization.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;

namespace Scheduling.Solver.AntColonyOptimization.Solvers
{
    public abstract class AntColonyOptimizationAlgorithmSolver(
                                          double alpha,
                                          double beta,
                                          double rho,
                                          double tau0,
                                          int ants,
                                          int iterations,
                                          int stagnantGenerationsAllowed,
                                          ISolveApproach solveApproach) : IFlexibleJobShopSchedulingSolver
    {
        protected ILogger? Logger;
        protected IGraphBuilderService GraphBuilderService;
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

        public DisjunctiveGraphModel DisjunctiveGraph { get; private set; }

        public IPheromoneTrail<Orientation, double> PheromoneTrail { get; } = solveApproach.GetPheromoneTrail();

        public ISolveApproach SolveApproach { get; } = solveApproach;

        public IFlexibleJobShopSchedulingSolver WithLogger(ILogger logger, bool with = false)
        {
            if (with)
                Logger = logger;
            return this;
        }

        public abstract Solution Solve(Instance instance);

        public abstract BaseAnt[] BugsLife(int currentIteration);

        protected void SetInitialPheromoneAmount(double amount)
        {
            foreach (var disjunction in DisjunctiveGraph.Disjunctions)
                foreach (var orientation in disjunction.Orientations)
                    if (!PheromoneTrail.TryAdd(orientation, amount))
                        Log($"Error on adding pheromone over {orientation}");
        }

        protected void CreateDisjunctiveGraphModel(Instance instance)
        {
            GraphBuilderService = new GraphBuilderService(Logger);
            DisjunctiveGraph = GraphBuilderService.BuildDisjunctiveGraph(instance);
        }

        public void Log(string message) => Logger?.Log(message);
    }
}
