using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Algorithms
{
    public class AntColonySystemAlgorithmV2(Parameters parameters, double phi, ISolveApproach solveApproach)
        : AntColonyV2AlgorithmSolver<AntColonySystemAlgorithmV2, AntColonySystemAntV2>(parameters, solveApproach)
    {

        /// <summary>
        /// Pheromone decay coefficient
        /// </summary>
        public double Phi { get; protected set; } = phi;

        /// <summary>
        /// Pseudorandom proportional rule parameter (between 0 and 1)
        /// </summary>
        public double Q0 { get; internal set; }


        public override void DorigosTouch(Instance instance)
        {
            Parameters.Alpha = 1;
            AntCount = 10;
            Phi = 0.1;
            Parameters.Rho = 0.1;
            Parameters.Tau0 = 1.0.DividedBy(instance.OperationCount * instance.UpperBound);
        }

        public override AntColonySystemAntV2[] BugsLife(int currentIteration)
        {
            AntColonySystemAntV2 BugSpawner(int id, int generation) => new(id, generation, this);

            return SolveApproach.LastMarchOfTheAnts(currentIteration, this, BugSpawner);
        }

        public override IFjspSolution Solve(Instance instance)
        {
            Instance = instance;
            Log($"Starting ACSV2 algorithm with following parameters:");
            if (Parameters.EnableDorigosTouch)
                DorigosTouch(instance);
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Phi= {Phi}; Initial pheromone = {Parameters.Tau0}.");
            Stopwatch iSw = new();
            Colony<AntColonySystemAntV2> colony = new();
            colony.Watch.Start();
            SetInitialPheromoneAmount(Parameters.Tau0);
            Log($"Depositing {Parameters.Tau0} pheromone units over {PheromoneStructure.Count()} machine-operation pairs...");
            for (int i = 0; i < Parameters.Iterations; i++)
            {
                var currentIteration = i + 1;
                Q0 = Math.Log(currentIteration) / Math.Log(Parameters.Iterations);
                Log($"\nQ0 becomes {Q0}");
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

            AntColonyOptimizationSolution<AntColonySystemAntV2> solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        public override void PheromoneUpdate(IColony<AntColonySystemAntV2> colony, AntColonySystemAntV2[] ants, int currentIteration)
        {
            var iterationBestAnt = colony.IterationBests[currentIteration];
            var bestSolutionPath = iterationBestAnt.Path;

            var delta = iterationBestAnt.Makespan.Inverse();
            foreach (var allocation in bestSolutionPath)
            {
                if (allocation is not null && PheromoneStructure.TryGetValue(allocation, out double currentPheromoneAmount))
                {
                    // new pheromone amount it's a convex combination between currentPheromoneAmount and delta 
                    var updatedAmount = (1 - Parameters.Rho) * currentPheromoneAmount + Parameters.Rho * delta;

                    if (!PheromoneStructure.TryUpdate(allocation, updatedAmount, currentPheromoneAmount))
                        Log($"Offline Update pheromone failed on {allocation}");
                }
            }
        }
    }
}
