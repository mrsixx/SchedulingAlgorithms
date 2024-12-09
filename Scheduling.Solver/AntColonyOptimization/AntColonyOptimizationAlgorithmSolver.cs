using Scheduling.Core.Extensions;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization
{
    public abstract class AntColonyOptimizationAlgorithmSolver(DisjunctiveGraphModel graph,
                                          double alpha = 0.9,
                                          double beta = 1.2,
                                          double rho = 0.01,
                                          double phi = 0.04,
                                          double tau0 = 0.001,
                                          int ants = 300,
                                          int iterations = 100,
                                          int stagnantGenerationsAllowed = 20) : IFlexibleJobShopSchedulingSolver
    {
        private ILogger? _logger;

        /// <summary>
        /// Weight of pheromone factor constant
        /// </summary>
        public double Alpha { get; init; } = alpha;

        /// <summary>
        /// Weight of distance factor constant
        /// </summary>
        public double Beta { get; init; } = beta;

        /// <summary>
        /// Pheromone evaporation rate constant
        /// </summary>
        public double Rho { get; init; } = rho;


        /// <summary>
        /// Pheromone decay coefficient
        /// </summary>
        public double Phi { get; init; } = phi;

        /// <summary>
        /// Pseudorandom proportional rule parameter (<= 1
        /// </summary>
        public double Q0 { get; internal set; }

        /// <summary>
        /// Initial pheromone amount over graph edges
        /// </summary>
        public double Tau0 { get; init; } = tau0;

        /// <summary>
        /// Amount of ants
        /// </summary>
        public int AntCount { get; init; } = ants;

        /// <summary>
        /// Number of iterations
        /// </summary>
        public int Iterations { get; init; } = iterations;


        /// <summary>
        /// How long should ants continue without improving the solution
        /// </summary>
        public int StagnantGenerationsAllowed { get; init; } = stagnantGenerationsAllowed;

        public DisjunctiveGraphModel DisjunctiveGraph { get; init; } = graph;

        public ConcurrentDictionary<Orientation, double> PheromoneTrail { get; } = [];

        public AntColonyOptimizationAlgorithmSolver Verbose(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public FjspSolution Solve()
        {
            Log($"Starting ACO algorithm with following parameters:");
            Log($"Alpha = {Alpha}; Beta = {Beta}; Rho = {Rho}; Initial pheromone = {Tau0}.");

            Stopwatch sw = new();
            Stopwatch iSw = new();
            Colony colony = new();
            sw.Start();
            SetInitialPheromoneAmount(Tau0);
            Log($"Depositing {Tau0} pheromone units over {DisjunctiveGraph.DisjuntionCount} disjunctions...");
            for (int i = 0; i < Iterations; i++)
            {
                var currentIteration = i + 1;
                Q0 = Math.Log(currentIteration) / Math.Log(Iterations);
                Log($"\nQ0 becomes {Q0}");
                Log($"Generating {AntCount} artificial ants from #{currentIteration}th wave...");
                iSw.Restart();
                Ant[] ants = BugsLife(currentIteration);
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
            sw.Stop();

            Log($"Every ant has stopped after {sw.Elapsed}.");
            Log($"\nFinishing execution...");

            if (colony.EmployeeOfTheMonth is not null)
                Log($"Better solution found by ant {colony.EmployeeOfTheMonth.Id} on #{colony.EmployeeOfTheMonth.Generation}th wave!");

            FjspSolution solution = new(colony);
            Log($"Makespan: {solution.Makespan}");

            return solution;
        }

        public abstract Ant[] BugsLife(int currentIteration);

        private void PheromoneOfflineUpdate(int currentIteration, Colony colony)
        {
            var iterationBestAnt = colony.IterationBests[currentIteration];
            var bestGraphEdges = iterationBestAnt.ConjunctiveGraph.Edges
                                .Where(e => e.HasAssociatedOrientation)
                                .Select(e => e.AssociatedOrientation)
                                .ToHashSet();

            foreach (var (orientation, currentPheromoneAmount) in PheromoneTrail)
            {
                var orientationBelongsToBestGraph = bestGraphEdges.Contains(orientation);
                var delta = iterationBestAnt.Makespan.Inverse();
                var updatedAmount = orientationBelongsToBestGraph
                    ? (1 - Rho) * currentPheromoneAmount + Rho * delta
                    : currentPheromoneAmount;

                if (!PheromoneTrail.TryUpdate(orientation, updatedAmount, currentPheromoneAmount))
                    Console.WriteLine($"Offline Update pheromone failed on {orientation}");
            }
        }

        private void SetInitialPheromoneAmount(double amount)
        {
            foreach (var disjunction in DisjunctiveGraph.Disjunctions)
                foreach (var orientation in disjunction.Orientations)
                    if (!PheromoneTrail.TryAdd(orientation, amount))
                        Console.WriteLine($"Error on adding pheromone over {orientation}");
        }

        protected void Log(string message) => _logger?.Log(message);
    }
}
