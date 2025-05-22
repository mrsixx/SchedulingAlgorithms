using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3
{
    public abstract class AntColonyV3AlgorithmSolver<TSelf, TAnt>(Parameters parameters, ISolveApproach solveApproach) :
        IAntColonyAlgorithm<Allocation, TAnt> where TSelf : AntColonyV3AlgorithmSolver<TSelf, TAnt> where TAnt : BaseAnt<TAnt>
    {
        protected ILogger? Logger;


        /// <summary>
        /// Amount of ants
        /// </summary>
        public int AntCount { get; protected set; } = parameters.AntCount;

        public Parameters Parameters { get; } = parameters;

        public ISolveApproach SolveApproach { get; } = solveApproach;

        public PrecedenceDigraph PrecedenceDigraph { get; private set; }

        public IPheromoneStructure<Allocation> PheromoneStructure { get; protected set; }

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
            PheromoneStructure = solveApproach.CreatePheromoneTrail<Allocation>();
            foreach (var job in Instance.Jobs)
                foreach (var operation in job.Operations)
                    foreach (var machine in operation.EligibleMachines)
                        if (!PheromoneStructure.TryAdd(new Allocation(operation, machine), amount))
                            Log($"Error on adding pheromone over O{operation.Id}M{machine.Id}");
        }

        public abstract void PheromoneUpdate(IColony<TAnt> colony, TAnt[] ants, int currentIteration);

        protected void CreatePrecedenceDigraph(Instance instance)
        {
            PrecedenceDigraph = new PrecedenceDigraph(instance);
        }

        public void Log(string message) => Logger?.Log(message);
    }
}
