using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Algorithms
{
    public class ElitistAntSystemAlgorithmV2(Parameters parameters, double e, ISolveApproach solveApproach) : 
        AntColonyV2AlgorithmSolver<ElitistAntSystemAlgorithmV2, ElitistAntSystemAntV2>(parameters, solveApproach)
    {
        /// <summary>
        /// Elitist weight
        /// </summary>
        public double E { get; init; } = e;

        public override IFjspSolution Solve(Instance instance)
        {
            Instance = instance;
            Log($"Starting EAS algorithm with following parameters:");
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Initial pheromone = {Parameters.Tau0}.");
            Stopwatch iSw = new();
            Colony<ElitistAntSystemAntV2> colony = new();
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
                PheromoneUpdate(ants, colony);
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

            AntColonyOptimizationSolution<ElitistAntSystemAntV2> solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }
        
        private void PheromoneUpdate(ElitistAntSystemAntV2[] ants, IColony<ElitistAntSystemAntV2> colony)
        {
            var bestSoFarSolution = colony.BestSoFar.Path;
            var bestSoFarDelta = colony.BestSoFar.Makespan.Inverse();
            foreach (var (allocation, currentPheromoneAmount) in PheromoneTrail)
            {
                var antsUsingAllocation = ants.Where(ant => ant.Path.Contains(allocation)).ToHashSet();

                // if the ant is not using this orientation, then its contribution to delta is 0
                var sum = antsUsingAllocation.Sum(ant => ant.Makespan.Inverse());
                var elitistReinforcement = bestSoFarSolution.Contains(allocation) ? bestSoFarDelta : 0;
                var updatedAmount = (1 - Parameters.Rho) * currentPheromoneAmount + sum + E * elitistReinforcement;

                if (!PheromoneTrail.TryUpdate(allocation, updatedAmount, currentPheromoneAmount))
                    Log($"Offline Update pheromone failed on {allocation}");
            }
        }

        public override ElitistAntSystemAntV2[] BugsLife(int currentIteration)
        {
            ElitistAntSystemAntV2 BugSpawner(int id, int generation) => new(id, generation, this);

            return SolveApproach.Solve(currentIteration, this, BugSpawner);
        }
    }
}
