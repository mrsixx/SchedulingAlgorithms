﻿using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization.Solvers
{
    public abstract class AntColonySystemAlgorithmSolver(
        double alpha,
        double beta,
        double rho,
        double phi,
        double tau0,
        int ants,
        int iterations,
        int stagnantGenerationsAllowed,
        ISolveApproach solveApproach) : AntColonyOptimizationAlgorithmSolver(
        alpha, beta, rho, tau0, ants, iterations, stagnantGenerationsAllowed, solveApproach)
    {
        /// <summary>
        /// Pheromone decay coefficient
        /// </summary>
        public double Phi { get; init; } = phi;

        /// <summary>
        /// Pseudorandom proportional rule parameter (between 0 and 1)
        /// </summary>
        public double Q0 { get; internal set; }


        public override IFjspSolution Solve(Instance instance)
        {
            Log($"Creating disjunctive graph...");
            CreateDisjunctiveGraphModel(instance);
            Log($"Starting ACS algorithm with following parameters:");
            Log($"Alpha = {Alpha}; Beta = {Beta}; Rho = {Rho}; Phi= {Phi}; Initial pheromone = {Tau0}.");
            Stopwatch iSw = new();
            Colony colony = new(DisjunctiveGraph);
            colony.Watch.Start();
            SetInitialPheromoneAmount(Tau0);
            Log($"Depositing {Tau0} pheromone units over {DisjunctiveGraph.DisjuntionCount} disjunctions...");
            for (int i = 0; i < Iterations; i++)
            {
                var currentIteration = i + 1;
                Q0 = Math.Log(currentIteration) / Math.Log(Iterations);
                Log($"\nQ0 becomes {Q0}");
                Log($"Generating {AntCount} artificial ants from #{currentIteration}th wave...");
                iSw.Restart();
                BaseAnt[] ants = BugsLife(currentIteration);
                iSw.Stop();
                Log($"#{currentIteration}th wave ants has stopped after {iSw.Elapsed}!");
                colony.UpdateBestPath(ants);
                Log($"Running offline pheromone update...");
                PheromoneOfflineUpdate(currentIteration, colony);
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

        private void PheromoneOfflineUpdate(int currentIteration, Colony colony)
        {
            var iterationBestAnt = colony.IterationBests[currentIteration];
            var bestGraphEdges = iterationBestAnt.ConjunctiveGraph.Edges
                                .Where(e => e.HasAssociatedOrientation)
                                .Select(e => e.AssociatedOrientation)
                                .ToHashSet();

            var delta = iterationBestAnt.Makespan.Inverse();

            foreach (var orientation in bestGraphEdges)
            {
                if (orientation is not null && PheromoneTrail.TryGetValue(orientation, out double currentPheromoneAmount))
                {
                    // new pheromone amount it's a convex combination between currentPheromoneAmount and delta 
                    var updatedAmount = (1 - Rho) * currentPheromoneAmount + Rho * delta;

                    if (!PheromoneTrail.TryUpdate(orientation, updatedAmount, currentPheromoneAmount))
                        Log($"Offline Update pheromone failed on {orientation}");
                }
            }
        }
    }
}
