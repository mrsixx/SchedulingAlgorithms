using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2
{
    public abstract class AntColonyV2AlgorithmSolver<TSelf, TAnt>(Parameters parameters, ISolveApproach solveApproach) :
        IAntColonyAlgorithm<Allocation, TAnt> where TSelf : AntColonyV2AlgorithmSolver<TSelf, TAnt> where TAnt : BaseAnt<TAnt>
    {
        protected ILogger? Logger;

        /// <summary>
        /// Amount of ants
        /// </summary>
        public int AntCount { get; protected set; } = parameters.AntCount;

        public Parameters Parameters { get; } = parameters;

        public ISolveApproach SolveApproach { get; } = solveApproach;

        public abstract void DorigosTouch(Instance instance);

        public IPheromoneTrail<Allocation> PheromoneTrail { get; protected set; }

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
            PheromoneTrail = solveApproach.CreatePheromoneTrail<Allocation>();

            foreach (var job in Instance.Jobs)
                foreach (var operation in job.Operations)
                    foreach (var machine in operation.EligibleMachines)
                        if (!PheromoneTrail.TryAdd(new Allocation(operation, machine), amount))
                            Log($"Error on adding pheromone over O{operation.Id}M{machine.Id}");
        }

        public abstract void PheromoneUpdate(IColony<TAnt> colony, TAnt[] ants, int currentIteration);

        public Instance Instance { get; protected set; }

        public void Log(string message) => Logger?.Log(message);
    }
}
