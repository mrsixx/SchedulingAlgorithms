using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Algorithms
{
    public class ElitistAntSystemAlgorithmV3(Parameters parameters, double e, ISolveApproach solveApproach) :
        AntColonyV3AlgorithmSolver<ElitistAntSystemAlgorithmV3, ElitistAntSystemAntV3>(parameters, solveApproach)
    {
        /// <summary>
        /// Elitist weight
        /// </summary>
        public double E { get; protected set; } = e;

        public override void DorigosTouch(Instance instance)
        {
            Parameters.Alpha = 1;
            Parameters.Rho = 0.5;
            AntCount = instance.Jobs.Count();
            E = AntCount;
            Parameters.Tau0 = (E + AntCount).DividedBy(Parameters.Rho * instance.UpperBound);
        }

        public override IFjspSolution Solve(Instance instance)
        {
            Instance = instance;
            Log($"Creating disjunctive graph...");
            CreatePrecedenceDigraph(instance);
            Log($"Starting EASV3 algorithm with following parameters:");
            DorigosTouch(instance);
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Elitist weight: {E}; Initial pheromone = {Parameters.Tau0}.");
            Stopwatch iSw = new();
            Colony<ElitistAntSystemAntV3> colony = new();
            colony.Watch.Start();
            SetInitialPheromoneAmount(Parameters.Tau0);
            Log($"Depositing {Parameters.Tau0} pheromone units over {PheromoneTrail.Count()} disjunctions...");
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

            AntColonyOptimizationSolution<ElitistAntSystemAntV3> solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        public override void PheromoneUpdate(IColony<ElitistAntSystemAntV3> colony, ElitistAntSystemAntV3[] ants, int currentIteration)
        {
            var bestSoFarSolution = colony.BestSoFar.Allocations;
            var bestSoFarDelta = colony.BestSoFar.Makespan.Inverse();
            foreach (var (allocation, currentPheromoneAmount) in PheromoneTrail)
            {
                var antsUsingAllocation = ants.Where(ant => ant.Allocations.Contains(allocation));

                // if the ant is not using this orientation, then its contribution to delta is 0
                var delta = antsUsingAllocation.Sum(ant => ant.Makespan.Inverse());
                var elitistReinforcement = bestSoFarSolution.Contains(allocation) ? bestSoFarDelta : 0;
                var updatedAmount = (1 - Parameters.Rho) * currentPheromoneAmount + delta + E * elitistReinforcement;

                if (!PheromoneTrail.TryUpdate(allocation, updatedAmount, currentPheromoneAmount))
                    Log($"Offline Update pheromone failed on {allocation}");
            }
        }
        public override ElitistAntSystemAntV3[] BugsLife(int currentIteration)
        {
            ElitistAntSystemAntV3 BugSpawner(int id, int generation) => new(id, generation, this);

            return SolveApproach.Solve(currentIteration, this, BugSpawner);
        }
    }
}
