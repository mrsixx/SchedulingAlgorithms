﻿using QuikGraph;
using Scheduling.Core.Graph;
using Scheduling.Solver.Extensions;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class AntColonyOptimizationAlgorithmSolver : IFlexibleJobShopSchedulingSolver
    {
        private ILogger? _logger;
        public AntColonyOptimizationAlgorithmSolver(DisjunctiveGraphModel graph,
                                              double alpha = 0.9,
                                              double beta = 1.2,
                                              double rho = 0.01,
                                              double q = 5000,
                                              double initialPheromoneAmount = 0.001,
                                              int ants = 300,
                                              int iterations = 100)
        {
            DisjunctiveGraph = graph;
            Alpha = alpha;
            Beta = beta;
            Rho = rho;
            Q = q;
            InitialPheromoneAmount = initialPheromoneAmount;
            AntCount = ants;
            Iterations = iterations;
        }

        /// <summary>
        /// Weight of distance factor constant
        /// </summary>
        public double Alpha { get; }

        /// <summary>
        /// Weight of pheromone factor constant
        /// </summary>
        public double Beta { get; }

        /// <summary>
        /// Pheromone evaporation rate constant
        /// </summary>
        public double Rho { get; }

        /// <summary>
        /// Pheromone Update constant
        /// </summary>
        public double Q { get; }

        /// <summary>
        /// Initial pheromone amount over graph edges
        /// </summary>
        public double InitialPheromoneAmount { get; }

        /// <summary>
        /// Amount of ants
        /// </summary>
        public int AntCount { get; }

        /// <summary>
        /// Number of iterations
        /// </summary>
        public int Iterations { get; }

        public DisjunctiveGraphModel DisjunctiveGraph { get; }

        public AntColonyOptimizationAlgorithmSolver Verbose(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public FjspSolution Solve()
        {
            Log($"Starting ACO algorithm with following parameters:");
            Log($"Alpha = {Alpha}; Beta = {Beta}; Rho = {Rho}; Q = {Q}; Initial pheromone = {InitialPheromoneAmount}.");

            Stopwatch sw = new();
            Stopwatch iSw = new();
            Colony colony = new();
            sw.Start();
            DisjunctiveGraph.SetInitialPheromoneAmount(InitialPheromoneAmount);
            for (int i = 0; i < Iterations; i++)
            {
                Log($"\nGenerating {AntCount} artificial ants from #{i + 1}th wave...");
                //Log($"Graph pheromone on #{i + 1}th wave: total = {Graph.CalculateTotalPheromoneAmount()}; average = {Graph.CalculateAvgPheromoneAmount()}");
                iSw.Restart();
                Ant[] ants = GenerateAntsWave(generation: i + 1);
                Log($"#{i + 1}th wave ants start to walk...");
                WaitForAntsToStop(ants);
                iSw.Stop();
                Log($"#{i + 1}th wave ants has stopped after {iSw.Elapsed}!");
                colony.UpdateBestPath(ants);
                Log($"Better makespan: {colony.BestPath.CalculateDistance()}");
            }
            sw.Stop();

            Log($"Every ant has stopped after {sw.Elapsed}.");
            Log($"\nFinishing execution...");

            if (colony.EmployeeOfTheMonth is not null)
                Log($"Better solution found by ant {colony.EmployeeOfTheMonth.Id} on #{colony.EmployeeOfTheMonth.Generation}th wave!");
            
            FjspSolution solution = new() { Makespan = colony.BestPath.CalculateDistance() };
            solution.Path.AddRange(colony.BestPath);
            _logger.LogPath(colony.BestPath);
            Log($"Makespan: {solution.Makespan}");
            return solution;
        }

        private void Log(string message) => _logger?.Log(message);

        private static void WaitForAntsToStop(Ant[] ants) => Task.WaitAll(ants.Select(a => a.Task).ToArray());

        private Ant[] GenerateAntsWave(int generation)
        {
            Ant[] ants = new Ant[AntCount];
            for (int i = 0; i < AntCount; i++)
                ants[i] = new Ant(id: i + 1, generation, context: this);
            return ants;
        }

    }
}
