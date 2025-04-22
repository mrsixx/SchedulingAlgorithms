using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;
using Scheduling.Core.Services;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1
{
    public abstract class AntColonyV1AlgorithmSolver<TSelf, TAnt>(Parameters parameters, ISolveApproach solveApproach) : 
        IAntColonyAlgorithm<Orientation, TAnt> where TSelf : AntColonyV1AlgorithmSolver<TSelf, TAnt>
    {
        protected ILogger? Logger;
        protected IGraphBuilderService GraphBuilderService;


        /// <summary>
        /// Amount of ants
        /// </summary>
        public int AntCount { get; protected set; } = parameters.AntCount;

        public Parameters Parameters { get; } = parameters;

        public ISolveApproach SolveApproach { get; } = solveApproach;

        public DisjunctiveGraphModel DisjunctiveGraph { get; private set; }

        public IPheromoneTrail<Orientation> PheromoneTrail { get; protected set; }

        public IFlexibleJobShopSchedulingSolver WithLogger(ILogger logger, bool with = false)
        {
            if (with)
                Logger = logger;
            return this;
        }

        public abstract IFjspSolution Solve(Instance instance);

        public abstract TAnt[] BugsLife(int currentIteration);

        public Instance Instance { get; protected set; }

        public abstract void DorigosTouch(Instance instance);

        protected void SetInitialPheromoneAmount(double amount)
        {
            PheromoneTrail = solveApproach.CreatePheromoneTrail<Orientation>();

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
