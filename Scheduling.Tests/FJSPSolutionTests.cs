﻿using Scheduling.Benchmarks;
using Scheduling.Benchmarks.Interfaces;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.Solvers;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Tests
{
    public class FJSPSolutionTests
    {
        private IBenchmarkReaderService _readerService = new BenchmarkReaderService(null);
        private const string BENCHMARK_FILE = "C:\\Users\\Matheus Ribeiro\\source\\repos\\mrsixx\\SchedulingAlgorithms\\Scheduling.Benchmarks\\Data\\6_Fattahi\\Fattahi12.fjs";

        #region Solvers batch
        public static IEnumerable<object[]> GetSolvers()
        {
            yield return
            [
                new AntColonySystemV1Solver(alpha: 1.0, beta: 1.0, rho: 0.5, phi: 0.5, tau0: 100, ants: 10, iterations: 1, stagnantGenerationsAllowed: 1, new IterativeSolveApproach())
            ];
            yield return
            [
                new AntColonySystemV2Solver(alpha: 1.0, beta: 1.0, rho: 0.5, phi: 0.5, tau0: 100, ants: 10, iterations: 1, stagnantGenerationsAllowed: 1, new IterativeSolveApproach())
            ];
            yield return
            [
                new AntColonySystemV1Solver(alpha: 1.0, beta: 1.0, rho: 0.5, phi: 0.5, tau0: 100, ants: 10, iterations: 1, stagnantGenerationsAllowed: 1, new ParallelSolveApproach())
            ];
            yield return
            [
                new AntColonySystemV2Solver(alpha: 1.0, beta: 1.0, rho: 0.5, phi: 0.5, tau0: 100, ants: 10, iterations: 1, stagnantGenerationsAllowed: 1, new ParallelSolveApproach())
            ];
        }
        #endregion

        [Theory]
        [MemberData(nameof(GetSolvers))]
        public void FjspSolution_ReleaseDateRestriction_MustBeSatisfied(IFlexibleJobShopSchedulingSolver solver)
        {
            var instance = _readerService.ReadInstance(BENCHMARK_FILE);
            var solution = solver.Solve(instance);
            
            foreach (var job in instance.Jobs)
            {
                var firstOperation = job.Operations.First();
                var jobStartTime = solution.StartTimes[firstOperation];
                Assert.True(jobStartTime >= job.ReleaseDate, "Restriction 1 was violated");
            }
        }


        [Theory]
        [MemberData(nameof(GetSolvers))]
        public void FjspSolution_ConjunctiveRestriction_MustBeSatisfied(IFlexibleJobShopSchedulingSolver solver)
        {
            var instance = _readerService.ReadInstance(BENCHMARK_FILE);
            var solution = solver.Solve(instance);

            foreach (var job in instance.Jobs)
            {
                Operation previousOperation = null;
                foreach (var operation in job.Operations)
                {
                    if (previousOperation is not null)
                        Assert.True(solution.StartTimes[operation] >= solution.CompletionTimes[previousOperation], "Restriction 2 was violated");

                    previousOperation = operation;
                }
            }
        }


        [Theory]
        [MemberData(nameof(GetSolvers))]
        public void FjspSolution_DisjunctiveRestriction_MustBeSatisfied(IFlexibleJobShopSchedulingSolver solver)
        {
            var instance = _readerService.ReadInstance(BENCHMARK_FILE);
            var solution = solver.Solve(instance);


            foreach (var machine in instance.Machines)
            {
                var operations = solution.MachineAssignment.Where(mu => mu.Value == machine).Select(mu => mu.Key);

                foreach (var o1 in operations)
                {
                    foreach (var o2 in operations)
                    {
                        if(o1 != o2)
                            Assert.True(
                                solution.StartTimes[o2] >= solution.CompletionTimes[o1] ||
                                solution.StartTimes[o1] >= solution.CompletionTimes[o2],
                                "Restriction 3 was violated");
                    }
                }
            }
        }
    }
}
