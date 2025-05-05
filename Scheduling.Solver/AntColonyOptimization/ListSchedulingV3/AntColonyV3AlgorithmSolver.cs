using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Interfaces;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Model;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3
{
    public abstract class AntColonyV3AlgorithmSolver<TSelf, TAnt>(Parameters parameters, ISolveApproach solveApproach) :
        IAntColonyAlgorithm<DisjunctiveArc, TAnt> where TSelf : AntColonyV3AlgorithmSolver<TSelf, TAnt>
    {
        protected ILogger? Logger;
        public IDigraphBuilderService GraphBuilderService;


        /// <summary>
        /// Amount of ants
        /// </summary>
        public int AntCount { get; protected set; } = parameters.AntCount;

        public Parameters Parameters { get; } = parameters;

        public ISolveApproach SolveApproach { get; } = solveApproach;

        public DisjunctiveDigraph DisjunctiveGraph { get; private set; }

        public IPheromoneTrail<DisjunctiveArc> PheromoneTrail { get; protected set; }

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
            PheromoneTrail = solveApproach.CreatePheromoneTrail<DisjunctiveArc>();

            foreach (var arc in DisjunctiveGraph.ArcSet.OfType<DisjunctiveArc>())
                if (!PheromoneTrail.TryAdd(arc, amount))
                    Log($"Error on adding pheromone over {arc}");
        }

        protected void CreateDisjunctiveGraphModel(Instance instance)
        {
            GraphBuilderService = new DigraphBuilderService(Logger);
            DisjunctiveGraph = GraphBuilderService.BuildDisjunctiveDigraph(instance);
        }

        public void Log(string message) => Logger?.Log(message);
    }
}
