using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Algorithms
{
    public class AntColonySystemAlgorithmV3(Parameters parameters, double phi, ISolveApproach solveApproach)
        : AntColonyV3AlgorithmSolver<AntColonySystemAlgorithmV3, AntColonySystemAntV3>(parameters, solveApproach)
    {
        /// <summary>
        /// Pheromone decay coefficient
        /// </summary>
        public double Phi { get; private set; } = phi;

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

        public override AntColonySystemAntV3[] BugsLife(int currentIteration)
        {
            AntColonySystemAntV3 BugSpawner(int id, int generation) => new(id, generation, this);

            return SolveApproach.LastMarchOfTheAnts(currentIteration, this, BugSpawner);
        }

        public override IFjspSolution Solve(Instance instance)
        {
            Instance = instance;
            Log($"Creating disjunctive graph...");
            CreatePrecedenceDigraph(instance);
            Log($"Starting ACSV3 algorithm with following parameters:");
            if (Parameters.EnableDorigosTouch)
                DorigosTouch(instance);
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Phi= {Phi}; Initial pheromone = {Parameters.Tau0}.");
            Stopwatch iSw = new();
            Colony<AntColonySystemAntV3> colony = new();
            colony.Watch.Start();
            SetInitialPheromoneAmount(Parameters.Tau0);
            Log($"Depositing {Parameters.Tau0} pheromone units over {PheromoneStructure.Count()} disjunctions...");
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

            AntColonyOptimizationSolution<AntColonySystemAntV3> solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        public override void PheromoneUpdate(IColony<AntColonySystemAntV3> colony, AntColonySystemAntV3[] ants, int currentIteration)
        {
            var iterationBestAnt = colony.IterationBests[currentIteration];

            var delta = iterationBestAnt.Makespan.Inverse();
            foreach (var orientation in iterationBestAnt.Allocations)
            {
                if (orientation is not null && PheromoneStructure.TryGetValue(orientation, out double currentPheromoneAmount))
                {
                    // new pheromone amount it's a convex combination between currentPheromoneAmount and delta 
                    var updatedAmount = (1 - Parameters.Rho) * currentPheromoneAmount + Parameters.Rho * delta;

                    if (!PheromoneStructure.TryUpdate(orientation, updatedAmount, currentPheromoneAmount))
                        Log($"Offline Update pheromone failed on {orientation}");
                }
            }
        }
    }
}
