using Scheduling.Benchmarks;
using Scheduling.Benchmarks.Interfaces;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.Solvers;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Tests
{
    public class FJSPSolutionTests
    {
        public const string BENCHMARK_FILE = "C:\\Users\\Matheus Ribeiro\\source\\repos\\mrsixx\\SchedulingAlgorithms\\Scheduling.Benchmarks\\Data\\6_Fattahi\\Fattahi12.fjs";
        private IBenchmarkReaderService _readerService = new BenchmarkReaderService(null);
        private IFlexibleJobShopSchedulingSolver _solver = new AntColonySystemV2Solver(
            1.0, 1.0, 0.5, 0.5, 100, 10, 1,
            1, new IterativeSolveApproach());
        
        [Fact]
        public void FjspSolution_ReleaseDateRestriction_MustBeSatisfied()
        {
            var instance = _readerService.ReadInstance(BENCHMARK_FILE);
            var solution = _solver.Solve(instance);
            
            foreach (var job in instance.Jobs)
            {
                var firstOperation = job.Operations.First();
                var jobStartTime = solution.StartTimes[firstOperation];
                Assert.True(jobStartTime >= job.ReleaseDate, "Restriction 1 was violated");
            }
        }


        [Fact]
        public void FjspSolution_ConjunctiveRestriction_MustBeSatisfied()
        {
            var instance = _readerService.ReadInstance(BENCHMARK_FILE);
            var solution = _solver.Solve(instance);

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


        [Fact]
        public void FjspSolution_DisjunctiveRestriction_MustBeSatisfied()
        {
            var instance = _readerService.ReadInstance(BENCHMARK_FILE);
            var solution = _solver.Solve(instance);


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
