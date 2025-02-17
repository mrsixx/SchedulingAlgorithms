using Scheduling.Solver;
using Scheduling.Solver.AntColonyOptimization.Solvers;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Console
{
    internal class AlgorithmFactory(Arguments args) : IAlgorithmFactory
    {
        public Arguments Arguments { get; } = args;

        public IFlexibleJobShopSchedulingSolver GetSolverAlgorithm()
        {
            ISolveApproach solveApproach = Arguments.UseParallelApproach ? new ParallelSolveApproach() : new IterativeSolveApproach();
            
            switch (args.SolverName.ToLowerInvariant().Trim())
            {
                case "as":
                    return new AntSystemAlgorithmSolver(
                        args.Alpha, args.Beta, args.Rho, args.Tau0, args.Ants, args.Iterations, args.AllowedStagnantGenerations, solveApproach);
                case "eas":
                    return new ElitistAntSystemAlgorithmSolver(
                        args.Alpha, args.Beta, args.Rho, args.E, args.Tau0, args.Ants, args.Iterations, args.AllowedStagnantGenerations, solveApproach);
                case "acsv1":
                    return new AntColonySystemV1Solver(
                        args.Alpha, args.Beta, args.Rho, args.Phi, args.Tau0, args.Ants, args.Iterations, args.AllowedStagnantGenerations, solveApproach);
                case "acsv2":
                    return new AntColonySystemV2Solver(
                        args.Alpha, args.Beta, args.Rho, args.Phi, args.Tau0, args.Ants, args.Iterations, args.AllowedStagnantGenerations, solveApproach);
                case "greedy":
                    return new GreedyHeuristicAlgorithmSolver();

               default:
                    throw new Exception("Algoritmo não implementado");
            }
        }
    }
}
