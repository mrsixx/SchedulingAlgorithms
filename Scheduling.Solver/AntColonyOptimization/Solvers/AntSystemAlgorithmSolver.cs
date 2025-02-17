using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduling.Solver.AntColonyOptimization.Solvers
{
    public class AntSystemAlgorithmSolver(
        double alpha,
        double beta,
        double rho,
        double tau0,
        int ants,
        int iterations,
        int stagnantGenerationsAllowed,
        ISolveApproach solveApproach) : AntColonyOptimizationAlgorithmSolver(
        alpha, beta, rho, tau0, ants, iterations, stagnantGenerationsAllowed, solveApproach)
    {
        public override IFjspSolution Solve(Instance instance)
        {
            Log($"Creating disjunctive graph...");
            CreateDisjunctiveGraphModel(instance);
            Log($"Starting AS algorithm with following parameters:");
            Log($"Alpha = {Alpha}; Beta = {Beta}; Rho = {Rho}; Initial pheromone = {Tau0}.");
            Stopwatch sw = new();
            Stopwatch iSw = new();
            Colony colony = new(DisjunctiveGraph);
            sw.Start();
            SetInitialPheromoneAmount(Tau0);
            Log($"Depositing {Tau0} pheromone units over {DisjunctiveGraph.DisjuntionCount} disjunctions...");
            for (int i = 0; i < Iterations; i++)
            {
                var currentIteration = i + 1;
                Log($"Generating {AntCount} artificial ants from #{currentIteration}th wave...");
                iSw.Restart();
                BaseAnt[] ants = BugsLife(currentIteration);
                iSw.Stop();
                Log($"#{currentIteration}th wave ants has stopped after {iSw.Elapsed}!");
                colony.UpdateBestPath(ants);
                Log($"Running offline pheromone update...");
                PheromoneOfflineUpdate(ants);
                Log($"Iteration best makespan: {colony.IterationBests[currentIteration].Makespan}");
                Log($"Best so far makespan: {colony.EmployeeOfTheMonth.Makespan}");

                var generationsSinceLastImprovement = i - colony.LastProductiveGeneration;
                if (generationsSinceLastImprovement > StagnantGenerationsAllowed)
                {
                    Log($"\n\nDeath from stagnation...");
                    break;
                }
            }
            sw.Stop();

            Log($"Every ant has stopped after {sw.Elapsed}.");
            Log("\nFinishing execution...");

            if (colony.EmployeeOfTheMonth is not null)
                Log($"Better solution found by ant {colony.EmployeeOfTheMonth.Id} on #{colony.EmployeeOfTheMonth.Generation}th wave!");

            AcsSolution solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }
        
        private void PheromoneOfflineUpdate(BaseAnt[] ants)
        {
            foreach (var (orientation, currentPheromoneAmount) in PheromoneTrail)
            {
                var delta = ants.Sum(ant =>
                {
                    var bestGraphEdges = ant.ConjunctiveGraph.Edges
                        .Where(e => e.HasAssociatedOrientation)
                        .Select(e => e.AssociatedOrientation)
                        .ToHashSet();
                    
                    if (!bestGraphEdges.Contains(orientation)) return 0;
                    return ant.Makespan.Inverse();
                });

                var updatedAmount =  (1 - Rho) * currentPheromoneAmount + Rho * delta;

                if (!PheromoneTrail.TryUpdate(orientation, updatedAmount, currentPheromoneAmount))
                    Log($"Offline Update pheromone failed on {orientation}");
            }
        }

        public override BaseAnt[] BugsLife(int currentIteration)
        {
            return SolveApproach.Solve<BaseAnt>(currentIteration, this, BugSpawner);
        }

        private AntSystemAnt BugSpawner(int id, int currentIteration) => new(id, currentIteration, this);

    }
}
