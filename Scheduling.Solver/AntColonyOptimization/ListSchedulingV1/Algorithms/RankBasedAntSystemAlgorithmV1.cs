using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Algorithms
{
    public class RankBasedAntSystemAlgorithmV1(Parameters parameters, int rankSize,ISolveApproach solveApproach) : AntColonyV1AlgorithmSolver<RankBasedAntSystemAlgorithmV1, RankBasedAntSystemAntV1>(parameters, solveApproach)
    {
        /// <summary>
        /// Elitist weight
        /// </summary>
        public int RankSize { get; init; } = rankSize;

        public override IFjspSolution Solve(Instance instance)
        {
            Log($"Creating disjunctive graph...");
            CreateDisjunctiveGraphModel(instance);
            Log($"Starting AS algorithm with following parameters:");
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Initial pheromone = {Parameters.Tau0}.");
            Stopwatch iSw = new();
            IColony<RankBasedAntSystemAntV1> colony = new ColonyV1<RankBasedAntSystemAntV1>(DisjunctiveGraph);
            colony.Watch.Start();
            SetInitialPheromoneAmount(Parameters.Tau0);
            Log($"Depositing {Parameters.Tau0} pheromone units over {DisjunctiveGraph.DisjuntionCount} disjunctions...");
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
                Log($"Best so far makespan: {colony.EmployeeOfTheMonth.Makespan}");

                var generationsSinceLastImprovement = i - colony.LastProductiveGeneration;
                if (generationsSinceLastImprovement > Parameters.StagnantGenerationsAllowed)
                {
                    Log($"\n\nDeath from stagnation...");
                    break;
                }
            }
            colony.Watch.Stop();

            Log($"Every ant has stopped after {colony.Watch.Elapsed}.");
            Log("\nFinishing execution...");

            if (colony.EmployeeOfTheMonth is not null)
                Log($"Better solution found by ant {colony.EmployeeOfTheMonth.Id} on #{colony.EmployeeOfTheMonth.Generation}th wave!");

            AntColonyOptimizationSolution<RankBasedAntSystemAntV1> solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        private void PheromoneUpdate(RankBasedAntSystemAntV1[] ants)
        {
            var size = Math.Max(1, Math.Min(RankSize, ants.Length)); // ensures that size is an int between 1 and ants.Length
            var topAnts = ants.OrderBy(a => a.Makespan).Take(size).ToArray();
            foreach (var (orientation, currentPheromoneAmount) in PheromoneTrail)
            {
                // if using orientation, increase is proportional rank position and quality
                var sum = topAnts.Select((ant, rank) =>
                    ant.ConjunctiveGraph.Contains(orientation) ? (size - rank) * ant.Makespan.Inverse() : 0
                ).Sum();

                var updatedAmount = (1 - Parameters.Rho) * currentPheromoneAmount + sum;

                if (!PheromoneTrail.TryUpdate(orientation, updatedAmount, currentPheromoneAmount))
                    Log($"Offline Update pheromone failed on {orientation}");
            }
        }

        public override RankBasedAntSystemAntV1[] BugsLife(int currentIteration)
        {
            return SolveApproach.Solve(currentIteration, this, BugSpawner);
        }

        private RankBasedAntSystemAntV1 BugSpawner(int id, int currentIteration) => new(id, currentIteration, this);

    }
}
