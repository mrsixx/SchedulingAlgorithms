using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Algorithms
{
    public class ElitistAntSystemAlgorithmV1(Parameters parameters, double e, ISolveApproach solveApproach) : 
        AntColonyV1AlgorithmSolver<ElitistAntSystemAlgorithmV1, ElitistAntSystemAntV1>(parameters, solveApproach)
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
            CreateDisjunctiveGraphModel(instance);
            Log($"Starting EASV1 algorithm with following parameters:");
            if (Parameters.EnableDorigosTouch)
                DorigosTouch(instance);
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Elitist weight: {E}; Initial pheromone = {Parameters.Tau0}.");
            Stopwatch iSw = new();
            Colony<ElitistAntSystemAntV1> colony = new();
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

            AntColonyOptimizationSolution<ElitistAntSystemAntV1> solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        private void PheromoneUpdate(ElitistAntSystemAntV1[] ants, IColony<ElitistAntSystemAntV1> colony)
        {
            var bestSoFarSolution = colony.BestSoFar.ConjunctiveGraph;
            var bestSoFarDelta = colony.BestSoFar.Makespan.Inverse();
            foreach (var (orientation, currentPheromoneAmount) in PheromoneStructure)
            {
                var antsUsingOrientation = ants.Where(ant => ant.ConjunctiveGraph.Contains(orientation));

                // if the ant is not using this orientation, then its contribution to delta is 0
                var delta = antsUsingOrientation.Sum(ant => ant.Makespan.Inverse());
                var elitistReinforcement = bestSoFarSolution.Contains(orientation) ? bestSoFarDelta : 0;
                var updatedAmount = (1 - Parameters.Rho) * currentPheromoneAmount + delta + E * elitistReinforcement;

                if (!PheromoneStructure.TryUpdate(orientation, updatedAmount, currentPheromoneAmount))
                    Log($"Offline Update pheromone failed on {orientation}");
            }
        }

        public override ElitistAntSystemAntV1[] BugsLife(int currentIteration)
        {
            ElitistAntSystemAntV1 BugSpawner(int id, int generation) => new(id, generation, this);

            return SolveApproach.LastMarchOfTheAnts(currentIteration, this, BugSpawner);
        }
    }
}
