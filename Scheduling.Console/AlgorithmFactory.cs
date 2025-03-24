using Scheduling.Solver.Greedy;
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
                case "asv1":
                    return new AntSystemAlgorithmSolver(
                        args.Alpha, args.Beta, args.Rho, args.Tau0, args.Ants, args.Iterations, args.AllowedStagnantGenerations, solveApproach);
                case "rbasv1":
                    return new RankBasedAntSystemAlgorithmSolver(
                        args.Alpha, args.Beta, args.Rho, args.Tau0, args.RankSize, args.Ants, args.Iterations, args.AllowedStagnantGenerations, solveApproach);
                case "easv1":
                    return new ElitistAntSystemAlgorithmSolver(
                        args.Alpha, args.Beta, args.Rho, args.E, args.Tau0, args.Ants, args.Iterations, args.AllowedStagnantGenerations, solveApproach);
                case "mmasv1":
                    return new MaxMinAntSystemAlgorithmSolver(
                        args.Alpha, args.Beta, args.Rho, args.TauMin, args.TauMax, args.Ants, args.Iterations, args.AllowedStagnantGenerations, solveApproach);
                case "acsv0":
                    return new AntColonySystemV0Solver(
                        args.Alpha, args.Beta, args.Rho, args.Phi, args.Tau0, args.Ants, args.Iterations, args.AllowedStagnantGenerations, solveApproach);
                case "acsv1":
                    return new AntColonySystemV1Solver(
                        args.Alpha, args.Beta, args.Rho, args.Phi, args.Tau0, args.Ants, args.Iterations, args.AllowedStagnantGenerations, solveApproach);
                case "greedy":
                    return new GreedyHeuristicAlgorithmSolver();

               default:
                    throw new Exception("Algoritmo não implementado");
            }
        }
    }
}
