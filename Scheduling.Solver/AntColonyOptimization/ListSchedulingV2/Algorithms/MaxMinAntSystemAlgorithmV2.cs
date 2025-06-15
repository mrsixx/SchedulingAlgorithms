using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;
using Scheduling.Core.Extensions;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Algorithms
{
    public class MaxMinAntSystemAlgorithmV2(Parameters parameters, double tauMin, double tauMax, ISolveApproach solveApproach)
        : AntColonyV2AlgorithmSolver<MaxMinAntSystemAlgorithmV2, MaxMinAntSystemAntV2>(parameters, solveApproach)
    {
        /// <summary>
        /// Max pheromone amount accepted over graph edges
        /// </summary>
        public double TauMax { get; private set; } = tauMax;

        /// <summary>
        /// Min pheromone amount accepted over graph edges
        /// </summary>
        public double TauMin { get; private set; } = tauMin;

        public override void DorigosTouch(Instance instance)
        {
            Parameters.Alpha = 1;
            Parameters.Rho = 0.02;
            AntCount = instance.Jobs.Count();
            TauMax = 1.DividedBy(Parameters.Rho * instance.UpperBound);
            TauMin = TauMax.DividedBy(instance.MachinesPerOperation);
        }

        private void UpdatePheromoneTrailLimits(IColony<MaxMinAntSystemAntV2> colony)
        {
            TauMax = 1.DividedBy(Parameters.Rho * colony.BestSoFar.Makespan);
            TauMin = TauMax.DividedBy(colony.BestSoFar.Instance.MachinesPerOperation);
        }

        public override IFjspSolution Solve(Instance instance)
        {
            Instance = instance;
            Log($"Starting MMASV2 algorithm with following parameters:");
            if (Parameters.EnableDorigosTouch)
                DorigosTouch(instance);
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Min pheromone = {TauMin}; Max pheromone = {TauMax}.");
            Stopwatch iSw = new();
            Colony<MaxMinAntSystemAntV2> colony = new();
            colony.Watch.Start();
            SetInitialPheromoneAmount(TauMax);
            Log($"Depositing {TauMax} pheromone units over {PheromoneStructure.Count()} machine-operation pairs...");
            for (int i = 0; i < Parameters.Iterations; i++)
            {
                var currentIteration = i + 1;
                Log($"Generating {AntCount} artificial ants from #{currentIteration}th wave...");
                iSw.Restart();
                var ants = BugsLife(currentIteration);
                iSw.Stop();
                Log($"#{currentIteration}th wave ants has stopped after {iSw.Elapsed}!");
                colony.UpdateBestPath(ants);
                Log($"Running global pheromone update...");
                PheromoneUpdate(colony, ants, currentIteration);
                UpdatePheromoneTrailLimits(colony);
                Log($"Iteration best makespan: {colony.IterationBests[currentIteration].Makespan}");
                Log($"Best so far makespan: {colony.EmployeeOfTheMonth?.Makespan}");

                var generationsSinceLastImprovement = i - colony.LastProductiveGeneration;
                if (generationsSinceLastImprovement > Parameters.StagnantGenerationsAllowed)
                {
                    Log($"\n\nDeath by stagnation...");
                    break;
                }
            }
            colony.Watch.Stop();

            Log($"Every ant has stopped after {colony.Watch.Elapsed}.");
            Log("\nFinishing execution...");

            if (colony.EmployeeOfTheMonth is not null)
                Log($"Better solution found by ant {colony.EmployeeOfTheMonth.Id} on #{colony.EmployeeOfTheMonth.Generation}th wave!");

            AntColonyOptimizationSolution<MaxMinAntSystemAntV2> solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        public override MaxMinAntSystemAntV2[] BugsLife(int currentIteration)
        {
            MaxMinAntSystemAntV2 BugSpawner(int id, int generation) => new(id, generation, this);

            return SolveApproach.LastMarchOfTheAnts(currentIteration, this, BugSpawner);
        }

        public override void PheromoneUpdate(IColony<MaxMinAntSystemAntV2> colony, MaxMinAntSystemAntV2[] ants, int currentIteration)
        {
            var bestSolutionPath = colony.BestSoFar.Path;

            foreach (var (allocation, currentPheromoneAmount) in PheromoneStructure)
            {
                var allocationBelongsToBestScheduling = bestSolutionPath.Contains(allocation);
                // pheromone deposited only by best so far ant
                var delta = allocationBelongsToBestScheduling ? colony.BestSoFar.Makespan.Inverse() : 0;

                var updatedAmount = Math.Max(Math.Min((1 - Parameters.Rho) * currentPheromoneAmount + delta, TauMax), TauMin);

                if (!PheromoneStructure.TryUpdate(allocation, updatedAmount, currentPheromoneAmount))
                    Log($"Offline Update pheromone failed on {allocation}");
            }
        }
    }
}
