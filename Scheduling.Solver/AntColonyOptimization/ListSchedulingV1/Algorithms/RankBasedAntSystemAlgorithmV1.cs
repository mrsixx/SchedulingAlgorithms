using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Ants;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants;
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
        public int RankSize { get; private set; } = rankSize;
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
            Log($"Creating disjunctive graph...");
            CreateDisjunctiveGraphModel(instance);
            Log($"Starting RBAS algorithm with following parameters:");
            DorigosTouch(instance);
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Rank size = {RankSize}; Initial pheromone = {Parameters.Tau0}.");
            Stopwatch iSw = new();
            Colony<RankBasedAntSystemAntV1> colony = new();
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
                PheromoneUpdate(colony, ants);
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

            AntColonyOptimizationSolution<RankBasedAntSystemAntV1> solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        private void PheromoneUpdate(IColony<RankBasedAntSystemAntV1> colony, RankBasedAntSystemAntV1[] ants)
        {
            var size = Math.Max(1, Math.Min(RankSize, ants.Length)); // ensures that size is an int between 1 and ants.Length
            var topAnts = ants.OrderBy(a => a.Makespan).Take(size - 1).ToArray();
            var bestGraphEdges = colony.BestSoFar.ConjunctiveGraph.Edges
                .Where(e => e.HasAssociatedOrientation)
                .Select(e => e.AssociatedOrientation)
                .ToHashSet();
            foreach (var (orientation, currentPheromoneAmount) in PheromoneTrail)
            {
                // if using orientation, increase is proportional rank position and quality
                var delta = topAnts.Select((ant, rank) =>
                    ant.ConjunctiveGraph.Contains(orientation) ? (size - rank - 1) * ant.Makespan.Inverse() : 0
                ).Sum();

                var allocationBelongsToBestScheduling = bestGraphEdges.Contains(orientation);
                // pheromone deposited only by best so far ant
                var deltaOpt = allocationBelongsToBestScheduling ? colony.BestSoFar.Makespan.Inverse() : 0;
                var updatedAmount = (1 - Parameters.Rho) * currentPheromoneAmount + delta + RankSize * deltaOpt;

                if (!PheromoneTrail.TryUpdate(orientation, updatedAmount, currentPheromoneAmount))
                    Log($"Offline Update pheromone failed on {orientation}");
            }
        }
        public override RankBasedAntSystemAntV1[] BugsLife(int currentIteration)
        {
            RankBasedAntSystemAntV1 BugSpawner(int id, int generation) => new(id, generation, this);

            return SolveApproach.Solve(currentIteration, this, BugSpawner);
        }
    }
}
