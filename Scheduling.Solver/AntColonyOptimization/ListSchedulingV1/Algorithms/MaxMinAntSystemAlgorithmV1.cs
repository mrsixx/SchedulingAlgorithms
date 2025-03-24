using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Algorithms
{
    public class MaxMinAntSystemAlgorithmV1(
        double alpha,
        double beta,
        double rho,
        double tauMin,
        double tauMax,
        int ants,
        int iterations,
        int stagnantGenerationsAllowed,
        ISolveApproach solveApproach)
        : AntColonyV1AlgorithmSolver<MaxMinAntSystemAntV1>(alpha, beta, rho, 0, ants, iterations,
            stagnantGenerationsAllowed, solveApproach)
    {

        /// <summary>
        /// Max pheromone amount accepted over graph edges
        /// </summary>
        public double TauMax { get; init; } = tauMax;

        /// <summary>
        /// Min pheromone amount accepted over graph edges
        /// </summary>
        public double TauMin { get; init; } = tauMin;

        public override MaxMinAntSystemAntV1[] BugsLife(int currentIteration)
        {
            return SolveApproach.Solve(currentIteration, this, BugSpawner);
        }

        public override IFjspSolution Solve(Instance instance)
        {
            Log($"Creating disjunctive graph...");
            CreateDisjunctiveGraphModel(instance);
            Log($"Starting ACS algorithm with following parameters:");
            Log($"Alpha = {Alpha}; Beta = {Beta}; Rho = {Rho}; Min pheromone = {TauMin}; Max pheromone = {TauMax}.");
            Stopwatch iSw = new();
            Colony colony = new(DisjunctiveGraph);
            colony.Watch.Start();
            SetInitialPheromoneAmount(TauMax);
            Log($"Depositing {TauMax} pheromone units over {DisjunctiveGraph.DisjuntionCount} disjunctions...");
            for (int i = 0; i < Iterations; i++)
            {
                var currentIteration = i + 1;
                Log($"Generating {AntCount} artificial ants from #{currentIteration}th wave...");
                iSw.Restart();
                var ants = BugsLife(currentIteration);
                iSw.Stop();
                Log($"#{currentIteration}th wave ants has stopped after {iSw.Elapsed}!");
                colony.UpdateBestPath(ants);
                Log($"Running offline pheromone update...");
                PheromoneUpdate(currentIteration, colony);
                Log($"Iteration best makespan: {colony.IterationBests[currentIteration].Makespan}");
                Log($"Best so far makespan: {colony.EmployeeOfTheMonth.Makespan}");

                var generationsSinceLastImprovement = i - colony.LastProductiveGeneration;
                if (generationsSinceLastImprovement > StagnantGenerationsAllowed)
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

            AntColonyOptimizationSolution solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        private MaxMinAntSystemAntV1 BugSpawner(int id, int currentIteration) => new(id, currentIteration, this);

        private void PheromoneUpdate(int currentIteration, Colony colony)
        {
            var bestGraphEdges = colony.BestSoFar.ConjunctiveGraph.Edges
                                .Where(e => e.HasAssociatedOrientation)
                                .Select(e => e.AssociatedOrientation)
                                .ToHashSet();

            foreach (var (orientation, currentPheromoneAmount) in PheromoneTrail)
            {
                var orientationBelongsToBestGraph = bestGraphEdges.Contains(orientation);
                // pheromone deposited only by best so far ant
                var delta = orientationBelongsToBestGraph ? colony.BestSoFar.Makespan.Inverse() : 0;
                var updatedAmount = (1 - Rho) * currentPheromoneAmount + delta;

                if (updatedAmount < TauMin)
                    updatedAmount = TauMin;
                else if (updatedAmount > TauMax)
                    updatedAmount = TauMax;

                if (!PheromoneTrail.TryUpdate(orientation, updatedAmount, currentPheromoneAmount))
                    Log($"Offline Update pheromone failed on {orientation}");
            }
        }
    }
}
