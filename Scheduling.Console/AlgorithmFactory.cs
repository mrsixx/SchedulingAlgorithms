using Scheduling.Solver.AntColonyOptimization;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV0;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Algorithms;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Algorithms;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Algorithms;
using Scheduling.Solver.Greedy;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Console
{
    internal class AlgorithmFactory(Arguments args) : IAlgorithmFactory
    {
        public Arguments Arguments { get; } = args;

        public IFlexibleJobShopSchedulingSolver GetSolverAlgorithm()
        {
            ISolveApproach solveApproach = Arguments.UseParallelApproach ? new ParallelSolveApproach() : new IterativeSolveApproach();
            Parameters parameters = new Parameters(args.Alpha, args.Beta, args.Rho, args.Tau0, args.Ants, args.Iterations, args.AllowedStagnantGenerations);
            switch (args.SolverName.ToLowerInvariant().Trim())
            {
                case "asv1":
                    return new AntSystemAlgorithmV1(parameters, solveApproach);
                case "asv2":
                    return new AntSystemAlgorithmV2(parameters, solveApproach);
                case "asv3":
                    return new AntSystemAlgorithmV3(parameters, solveApproach);
                case "rbasv1":
                    return new RankBasedAntSystemAlgorithmV1(parameters, args.RankSize, solveApproach);
                case "rbasv2":
                    return new RankBasedAntSystemAlgorithmV2(parameters, args.RankSize, solveApproach);
                case "rbasv3":
                    return new RankBasedAntSystemAlgorithmV3(parameters, args.RankSize, solveApproach);
                case "easv1":
                    return new ElitistAntSystemAlgorithmV1(parameters, args.E, solveApproach);
                case "easv2":
                    return new ElitistAntSystemAlgorithmV2(parameters, args.E, solveApproach);
                case "easv3":
                    return new ElitistAntSystemAlgorithmV3(parameters, args.E, solveApproach);
                case "mmasv1":
                    return new MaxMinAntSystemAlgorithmV1(parameters, args.TauMin, args.TauMax, solveApproach);
                case "mmasv2":
                    return new MaxMinAntSystemAlgorithmV2(parameters, args.TauMin, args.TauMax, solveApproach);
                case "mmasv3":
                    return new MaxMinAntSystemAlgorithmV3(parameters, args.TauMin, args.TauMax, solveApproach);
                case "acsv0":
                    return new AntColonySystemAlgorithmV0(parameters, args.Phi, solveApproach);
                case "acsv1":
                    return new AntColonySystemAlgorithmV1(parameters, args.Phi, solveApproach);
                case "acsv2":
                    return new AntColonySystemAlgorithmV2(parameters, args.Phi, solveApproach);
                case "acsv3":
                    return new AntColonySystemAlgorithmV3(parameters, args.Phi, solveApproach);
                case "greedy":
                    return new LeastLoadedMachineHeuristicAlgorithmSolver();

                default:
                    throw new Exception("Algoritmo não implementado");
            }
        }
    }
}
