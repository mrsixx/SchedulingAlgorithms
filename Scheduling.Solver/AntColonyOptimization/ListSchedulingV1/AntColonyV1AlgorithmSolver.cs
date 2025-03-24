using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;
using Scheduling.Core.Services;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1
{
    public abstract class AntColonyV1AlgorithmSolver<TAnt>(
                                          double alpha,
                                          double beta,
                                          double rho,
                                          double tau0,
                                          int ants,
                                          int iterations,
                                          int stagnantGenerationsAllowed,
                                          ISolveApproach solveApproach) : 
                                            AntColonyAlgorithmSolverBase(alpha, beta, rho, tau0, ants, iterations, stagnantGenerationsAllowed), 
                                            IAntColonyAlgorithm<Orientation, TAnt>
    {
        protected ILogger? Logger;
        protected IGraphBuilderService GraphBuilderService;

         public ISolveApproach SolveApproach { get; } = solveApproach;

        public DisjunctiveGraphModel DisjunctiveGraph { get; private set; }

        public override IPheromoneTrail<Orientation> PheromoneTrail { get; protected set; }

        public IFlexibleJobShopSchedulingSolver WithLogger(ILogger logger, bool with = false)
        {
            if (with)
                Logger = logger;
            return this;
        }

        public abstract IFjspSolution Solve(Instance instance);

        public abstract TAnt[] BugsLife(int currentIteration);

        protected void SetInitialPheromoneAmount(double amount)
        {
            PheromoneTrail = PheromoneTrail.CreatePheromoneTrail();

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
