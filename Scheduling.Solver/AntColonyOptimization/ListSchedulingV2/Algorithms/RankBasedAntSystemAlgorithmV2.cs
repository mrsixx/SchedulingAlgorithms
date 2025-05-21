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
        public int RankSize { get; protected set; } = rankSize;
        public override void DorigosTouch(Instance instance)
        {
            AntCount = instance.Jobs.Count();
            Parameters.Rho = 0.1;
            RankSize = 6;
            Parameters.Tau0 = 1.DividedBy(Parameters.Rho * instance.UpperBound);
        }

        public override IFjspSolution Solve(Instance instance)
        {
            Instance = instance;
            Log($"Starting RBASV2 algorithm with following parameters:");
            DorigosTouch(instance);
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Rank size = {RankSize}; Initial pheromone = {Parameters.Tau0}.");
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
                PheromoneUpdate(colony, ants, currentIteration);
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

        public override void PheromoneUpdate(IColony<RankBasedAntSystemAntV2> colony, RankBasedAntSystemAntV2[] ants, int currentIteration)
        {
            var size = Math.Max(1, Math.Min(RankSize, ants.Length)); // ensures that size is an int between 1 and ants.Length
            var topAnts = ants.OrderBy(a => a.Makespan).Take(size-1).ToArray();
            var bestSolutionPath = colony.BestSoFar.Path;
            foreach (var (allocation, currentPheromoneAmount) in PheromoneTrail)
            {
                // if using allocation, increase is proportional rank position and quality
                var delta = topAnts.Select((ant, rank) =>
                    ant.Path.Contains(allocation) ? (size - rank - 1) * ant.Makespan.Inverse() : 0
                ).Sum();

                var allocationBelongsToBestScheduling = bestSolutionPath.Contains(allocation);
                // pheromone deposited only by best so far ant
                var deltaOpt = allocationBelongsToBestScheduling ? colony.BestSoFar.Makespan.Inverse() : 0;
                var updatedAmount = (1 - Parameters.Rho) * currentPheromoneAmount + delta + RankSize * deltaOpt;

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
