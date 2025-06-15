using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.Interfaces;
using System.Diagnostics;
using Scheduling.Core.Interfaces;
using Scheduling.Core.Services;
using Scheduling.Solver.Models;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV0
{
    public class AntColonySystemAlgorithmV0(Parameters parameters, double phi, ISolveApproach solveApproach) : 
        IAntColonyAlgorithm<Orientation, AntColonySystemAntV0>
    {

        protected ILogger? Logger;
        protected IGraphBuilderService GraphBuilderService;

        public Parameters Parameters { get; } = parameters;

        public Instance Instance { get; protected set; }


        /// <summary>
        /// Pheromone decay coefficient
        /// </summary>
        public double Phi { get; protected set; } = phi;

        /// <summary>
        /// Pseudorandom proportional rule parameter (between 0 and 1)
        /// </summary>
        public double Q0 { get; internal set; }

        public ISolveApproach SolveApproach { get; } = solveApproach;

        public int AntCount { get; protected set; } = parameters.AntCount;

        public void DorigosTouch(Instance instance)
        {
            Parameters.Alpha = 1;
            AntCount = 10;
            Phi = 0.1;
            Parameters.Rho = 0.1;
            Parameters.Tau0 = 1.0.DividedBy(instance.OperationCount * instance.UpperBound);
        }

        public IPheromoneStructure<Orientation> PheromoneStructure { get; private set; }

        public DisjunctiveGraphModel DisjunctiveGraph { get; private set; }


        public IFlexibleJobShopSchedulingSolver WithLogger(ILogger logger, bool with = false)
        {
            if (with)
                Logger = logger;
            return this;
        }

        public IFjspSolution Solve(Instance instance)
        {
            Instance = instance;
            Log($"Creating disjunctive graph...");
            CreateDisjunctiveGraphModel(instance);
            Log($"Starting ACS algorithm with following parameters:");
            if (Parameters.EnableDorigosTouch) 
                DorigosTouch(instance);
            Log($"Alpha = {Parameters.Alpha}; Beta = {Parameters.Beta}; Rho = {Parameters.Rho}; Phi= {Phi}; Initial pheromone = {Parameters.Tau0}.");
            Stopwatch iSw = new();
            Colony<AntColonySystemAntV0> colony = new();
            colony.Watch.Start();
            SetInitialPheromoneAmount(Parameters.Tau0);
            Log($"Depositing {Parameters.Tau0} pheromone units over {DisjunctiveGraph.DisjuntionCount} disjunctions...");
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
                PheromoneOfflineUpdate(currentIteration, colony);
                Log($"Iteration best makespan: {colony.IterationBests[currentIteration].Makespan}");
                Log($"Best so far makespan: {colony.EmployeeOfTheMonth.Makespan}");

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

            AntColonyOptimizationSolution<AntColonySystemAntV0> solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        public void Log(string message) => Logger?.Log(message);

        private void CreateDisjunctiveGraphModel(Instance instance)
        {
            GraphBuilderService = new GraphBuilderService(Logger);
            DisjunctiveGraph = GraphBuilderService.BuildDisjunctiveGraph(instance);
        }

        private void SetInitialPheromoneAmount(double amount)
        {
            PheromoneStructure = solveApproach.CreatePheromoneTrail<Orientation>();

            foreach (var disjunction in DisjunctiveGraph.Disjunctions)
            foreach (var orientation in disjunction.Orientations)
                if (!PheromoneStructure.TryAdd(orientation, amount))
                    Log($"Error on adding pheromone over {orientation}");
        }

        private void PheromoneOfflineUpdate(int currentIteration, IColony<AntColonySystemAntV0> colony)
        {
            var iterationBestAnt = colony.IterationBests[currentIteration];
            var bestGraphEdges = iterationBestAnt.ConjunctiveGraph.Edges
                                .Where(e => e.HasAssociatedOrientation)
                                .Select(e => e.AssociatedOrientation)
                                .ToHashSet();

            var delta = iterationBestAnt.Makespan.Inverse();

            foreach (var orientation in bestGraphEdges)
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

        public AntColonySystemAntV0[] BugsLife(int currentIteration)
        {
            return SolveApproach.LastMarchOfTheAnts(currentIteration, this, BugSpawner);
        }
        private AntColonySystemAntV0 BugSpawner(int id, int currentIteration) => new(id, currentIteration, this);
    }
}
