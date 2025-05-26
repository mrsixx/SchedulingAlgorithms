using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Ants;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Algorithms
{
    public class MaxMinAntSystemAlgorithmV1(Parameters parameters, double tauMin, double tauMax, ISolveApproach solveApproach)
        : AntColonyV1AlgorithmSolver<MaxMinAntSystemAlgorithmV1, MaxMinAntSystemAntV1>(parameters, solveApproach)
    {

        /// <summary>
        /// Max pheromone amount accepted over graph edges
        /// </summary>
        public double TauMax { get; private set; } = tauMax;

        /// <summary>
        /// Min pheromone amount accepted over graph edges
        /// </summary>
        public double TauMin { get; private set; } = tauMin;

        public override void DorigosTouch(Instance instance)
        {
            Parameters.Alpha = 1;
            Parameters.Rho = 0.02;
            AntCount = instance.Jobs.Count();
            TauMax = 1.DividedBy(Parameters.Rho * instance.UpperBound);
            TauMin = TauMax.DividedBy(instance.MachinesPerOperation);
        }

        private void UpdatePheromoneTrailLimits(IColony<MaxMinAntSystemAntV1> colony)
        {
            TauMax = 1.DividedBy(Parameters.Rho * colony.BestSoFar.Makespan);
            TauMin = TauMax.DividedBy(Instance.MachinesPerOperation);
        }

        public override MaxMinAntSystemAntV1[] BugsLife(int currentIteration)
        {
            MaxMinAntSystemAntV1 BugSpawner(int id, int generation) => new(id, generation, this);

            return SolveApproach.LastMarchOfTheAnts(currentIteration, this, BugSpawner);
        }

        public override IFjspSolution Solve(Instance instance)
        {
            Instance = instance;
            Log($"Creating disjunctive graph...");
            CreateDisjunctiveGraphModel(instance);
            Log($"Starting MMASV1 algorithm with following parameters:");
            DorigosTouch(instance);
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Min pheromone = {TauMin}; Max pheromone = {TauMax}.");
            Stopwatch iSw = new();
            Colony<MaxMinAntSystemAntV1> colony = new();
            colony.Watch.Start();
            SetInitialPheromoneAmount(TauMax);
            Log($"Depositing {TauMax} pheromone units over {DisjunctiveGraph.DisjuntionCount} disjunctions...");
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
                PheromoneUpdate(currentIteration, colony);
                UpdatePheromoneTrailLimits(colony);
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

            AntColonyOptimizationSolution<MaxMinAntSystemAntV1> solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        private void PheromoneUpdate(int currentIteration, IColony<MaxMinAntSystemAntV1> colony)
        {
            var bestGraphEdges = colony.BestSoFar.ConjunctiveGraph.Edges
                                .Where(e => e.HasAssociatedOrientation)
                                .Select(e => e.AssociatedOrientation)
                                .ToHashSet();

            foreach (var (orientation, currentPheromoneAmount) in PheromoneStructure)
            {
                var orientationBelongsToBestGraph = bestGraphEdges.Contains(orientation);
                // pheromone deposited only by best so far ant
                var delta = orientationBelongsToBestGraph ? colony.BestSoFar.Makespan.Inverse() : 0;

                var updatedAmount = Math.Max(Math.Min((1 - Parameters.Rho) * currentPheromoneAmount + delta, TauMax), TauMin);

                if (!PheromoneStructure.TryUpdate(orientation, updatedAmount, currentPheromoneAmount))
                    Log($"Offline Update pheromone failed on {orientation}");
            }
        }
    }
}
