using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;

namespace Scheduling.Solver.Greedy
{
    public class LeastLoadedMachineHeuristicAlgorithmSolver : IFlexibleJobShopSchedulingSolver
    {
        protected ILogger? Logger;
        public void Log(string message) => Logger?.Log(message);

        public IFlexibleJobShopSchedulingSolver WithLogger(ILogger logger, bool with = false)
        {
            if (with)
                Logger = logger;
            return this;
        }

        public IFjspSolution Solve(Instance instance)
        {
            var solution = new GreedySolution();
            solution.Watch.Start();
            
            // creating data structures
            Log($"Starting LLM algorithm");
            var unscheduledJobOperations = new Dictionary<Job, Operation>();
            var loadingSequence = new Dictionary<Machine, List<Operation>>();
            instance.Jobs.ForEach(job => unscheduledJobOperations.Add(job, job.Operations[0]));
            instance.Machines.ForEach(m => {
                loadingSequence.Add(m, new List<Operation>());
                solution.MachineOccupancy.Add(m, 0);
            });

            while (unscheduledJobOperations.Any())
            {
                var (operation, machine) = GetGreedyMachineAllocation(unscheduledJobOperations, solution);

                // evaluate start e completion times
                var machinePredecessor = loadingSequence[machine].LastOrDefault();
                var jobPredecessor = !operation.FirstOperation ? operation.Job.Operations[operation.Index - 1] : null;
                var jobReleaseDate = Convert.ToDouble(operation.Job.ReleaseDate);

                var startTime = Math.Max(
                    machinePredecessor != null ? solution.CompletionTimes[machinePredecessor.Id] : 0,
                    jobPredecessor != null ? solution.CompletionTimes[jobPredecessor.Id] : jobReleaseDate
                );

                solution.StartTimes.TryAdd(operation.Id, startTime);
                solution.CompletionTimes.TryAdd(operation.Id, startTime + operation.GetProcessingTime(machine));

                // updating data structures
                loadingSequence[machine].Add(operation);
                solution.MachineOccupancy[machine] += operation.GetProcessingTime(machine);

                if (operation.LastOperation)
                    unscheduledJobOperations.Remove(operation.Job);
                else
                    unscheduledJobOperations[operation.Job] = operation.Job.Operations[operation.Index + 1];
            }
            solution.Watch.Stop();
            Log($"Solution found after {solution.Watch.Elapsed}.");
            Log("\nFinishing LLM execution...");

            // creating mu function
            foreach (var (m, operations) in loadingSequence)
                foreach (var o in operations)
                    solution.MachineAssignment.Add(o.Id, m.Index);

            return solution;
        }

        private (Operation, Machine) GetGreedyMachineAllocation(Dictionary<Job, Operation> unscheduledJobOperations, GreedySolution solution)
        {
            var candidateAllocations = unscheduledJobOperations.Values
                                                        .SelectMany(operation => operation.EligibleMachines
                                                                            .Select(machine => (operation, machine)));
            return candidateAllocations.MinBy(om =>
                solution.MachineOccupancy[om.machine] + om.operation.GetProcessingTime(om.machine)
            );
        }
    }
}
