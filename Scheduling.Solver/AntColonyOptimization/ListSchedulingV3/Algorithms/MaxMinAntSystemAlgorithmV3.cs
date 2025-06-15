using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Algorithms
{
    public class MaxMinAntSystemAlgorithmV3(Parameters parameters, double tauMin, double tauMax, ISolveApproach solveApproach)
        : AntColonyV3AlgorithmSolver<MaxMinAntSystemAlgorithmV3, MaxMinAntSystemAntV3>(parameters, solveApproach)
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

        private void UpdatePheromoneTrailLimits(IColony<MaxMinAntSystemAntV3> colony)
        {
            TauMax = 1.DividedBy(Parameters.Rho * colony.BestSoFar.Makespan);
            TauMin = TauMax.DividedBy(Instance.MachinesPerOperation);
        }

        public override MaxMinAntSystemAntV3[] BugsLife(int currentIteration)
        {
            MaxMinAntSystemAntV3 BugSpawner(int id, int generation) => new(id, generation, this);

            return SolveApproach.LastMarchOfTheAnts(currentIteration, this, BugSpawner);
        }

        public override IFjspSolution Solve(Instance instance)
        {
            Instance = instance;
            Log($"Creating disjunctive graph...");
            CreatePrecedenceDigraph(instance);
            Log($"Starting MMASV3 algorithm with following parameters:");
            if (Parameters.EnableDorigosTouch)
                DorigosTouch(instance);
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Min pheromone = {TauMin}; Max pheromone = {TauMax}.");
            Stopwatch iSw = new();
            Colony<MaxMinAntSystemAntV3> colony = new();
            colony.Watch.Start();
            SetInitialPheromoneAmount(TauMax);
            Log($"Depositing {TauMax} pheromone units over {PheromoneStructure.Count()} disjunctions...");
            for (int i = 0; i < Parameters.Iterations; i++)
            {
                var currentIteration = i + 1;
                Log($"Generating {AntCount} artificial ants from #{currentIteration}th wave...");
                iSw.Restart();
                var ants = BugsLife(currentIteration);
                iSw.Stop();
                Log($"#{currentIteration}th wave ants has stopped after {iSw.Elapsed}!");
                colony.UpdateBestPath(ants);
                Log($"Running offline pheromone update...");
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

            AntColonyOptimizationSolution<MaxMinAntSystemAntV3> solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        public override void PheromoneUpdate(IColony<MaxMinAntSystemAntV3> colony, MaxMinAntSystemAntV3[] ants, int currentIteration)
        {
            var bestSolution = colony.BestSoFar.Allocations;

            foreach (var (allocation, currentPheromoneAmount) in PheromoneStructure)
            {
                var allocationBelongsToBestScheduling = bestSolution.Contains(allocation);
                // pheromone deposited only by best so far ant
                var delta = allocationBelongsToBestScheduling ? colony.BestSoFar.Makespan.Inverse() : 0;

                var updatedAmount = Math.Max(Math.Min((1 - Parameters.Rho) * currentPheromoneAmount + delta, TauMax), TauMin);

                if (!PheromoneStructure.TryUpdate(allocation, updatedAmount, currentPheromoneAmount))
                    Log($"Offline Update pheromone failed on {allocation}");
            }
        }
    }
}
