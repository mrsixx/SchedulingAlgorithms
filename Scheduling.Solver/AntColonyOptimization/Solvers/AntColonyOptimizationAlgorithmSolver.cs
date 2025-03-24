using Scheduling.Core.Extensions;
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
                                          ISolveApproach solveApproach) : 
                                        AntColonyAlgorithmSolverBase(alpha, beta, rho, tau0, ants, iterations, stagnantGenerationsAllowed), 
                                        IAntColonyAlgorithm<Orientation, AntColonySystemAntV0>
    {
        protected ILogger? Logger;
        protected IGraphBuilderService GraphBuilderService;
        

        public DisjunctiveGraphModel DisjunctiveGraph { get; private set; }

        public override IPheromoneTrail<Orientation> PheromoneTrail { get; protected set; }

        public ISolveApproach SolveApproach { get; } = solveApproach;

        public IFlexibleJobShopSchedulingSolver WithLogger(ILogger logger, bool with = false)
        {
            if (with)
                Logger = logger;
            return this;
        }

        public abstract IFjspSolution Solve(Instance instance);

        public abstract AntColonySystemAntV0[] BugsLife(int currentIteration);

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
