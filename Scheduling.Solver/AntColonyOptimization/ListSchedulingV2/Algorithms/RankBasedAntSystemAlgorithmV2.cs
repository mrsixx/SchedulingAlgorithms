using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Algorithms
{
    public class RankBasedAntSystemAlgorithmV2(Parameters parameters, int rankSize,ISolveApproach solveApproach) : AntColonyV2AlgorithmSolver<RankBasedAntSystemAlgorithmV2, RankBasedAntSystemAntV2>(parameters, solveApproach)
    {
        /// <summary>
        /// Elitist weight
        /// </summary>
        public int RankSize { get; init; } = rankSize;

        public override IFjspSolution Solve(Instance instance)
        {
            Instance = instance;
            Log($"Starting RBAS algorithm with following parameters:");
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Initial pheromone = {Parameters.Tau0}.");
            Stopwatch iSw = new();
            Colony<RankBasedAntSystemAntV2> colony = new();
            colony.Watch.Start();
            SetInitialPheromoneAmount(Parameters.Tau0);
            Log($"Depositing {Parameters.Tau0} pheromone units over {PheromoneTrail.Count()} machine-operation pairs...");
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
                PheromoneUpdate(ants);
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

            AntColonyOptimizationSolution<RankBasedAntSystemAntV2> solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        private void PheromoneUpdate(RankBasedAntSystemAntV2[] ants)
        {
            var size = Math.Max(1, Math.Min(RankSize, ants.Length)); // ensures that size is an int between 1 and ants.Length
            var topAnts = ants.OrderBy(a => a.Makespan).Take(size).ToArray();
            foreach (var (allocation, currentPheromoneAmount) in PheromoneTrail)
            {
                // if using allocation, increase is proportional rank position and quality
                var delta = topAnts.Select((ant, rank) =>
                    ant.Path.Contains(allocation) ? (size - rank) * ant.Makespan.Inverse() : 0
                ).Sum();

                var updatedAmount = (1 - Parameters.Rho) * currentPheromoneAmount + delta;

                if (!PheromoneTrail.TryUpdate(allocation, updatedAmount, currentPheromoneAmount))
                    Log($"Offline Update pheromone failed on {allocation}");
            }
        }

        public override RankBasedAntSystemAntV2[] BugsLife(int currentIteration)
        {
            RankBasedAntSystemAntV2 BugSpawner(int id, int generation) => new(id, generation, this);

            return SolveApproach.Solve(currentIteration, this, BugSpawner);
        }
    }
}
