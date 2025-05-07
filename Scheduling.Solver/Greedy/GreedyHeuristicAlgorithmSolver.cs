using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;

namespace Scheduling.Solver.Greedy
{
    public class GreedyHeuristicAlgorithmSolver : IFlexibleJobShopSchedulingSolver
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
            Log($"Starting greedy algorithm");
            var unscheduledJobOperations = new Dictionary<Job, LinkedListNode<Operation>>();
            var loadingSequence = new Dictionary<Machine, LinkedList<Operation>>();
            instance.Jobs.ForEach(job => unscheduledJobOperations.Add(job, job.Operations.First));
            instance.Machines.ForEach(m => {
                loadingSequence.Add(m, new LinkedList<Operation>());
                solution.MachineOccupancy.Add(m, 0);
            });

            while (unscheduledJobOperations.Any())
            {
                var (operation, machine) = GetGreedyMachineAllocation(unscheduledJobOperations, solution);

                // evaluate start e completion times
                var machinePredecessor = loadingSequence[machine].Last;
                var jobPredecessor = operation.Previous;
                var jobReleaseDate = Convert.ToDouble(operation.Value.Job.ReleaseDate);

                var startTime = Math.Max(
                    machinePredecessor != null ? solution.CompletionTimes[machinePredecessor.Value.Id] : 0,
                    jobPredecessor != null ? solution.CompletionTimes[jobPredecessor.Value.Id] : jobReleaseDate
                );

                solution.StartTimes.TryAdd(operation.Value.Id, startTime);
                solution.CompletionTimes.TryAdd(operation.Value.Id, startTime + operation.Value.GetProcessingTime(machine));

                // updating data structures
                loadingSequence[machine].AddLast(operation.Value);
                solution.MachineOccupancy[machine] += operation.Value.GetProcessingTime(machine);

                if (operation.Next is null)
                    unscheduledJobOperations.Remove(operation.Value.Job);
                else
                    unscheduledJobOperations[operation.Value.Job] = operation.Next;
            }
            solution.Watch.Stop();
            Log($"Solution found after {solution.Watch.Elapsed}.");
            Log("\nFinishing execution...");

            // creating mu function
            foreach (var (m, operations) in loadingSequence)
                foreach (var o in operations)
                    solution.MachineAssignment.Add(o.Id, m);

            return solution;
        }

        private (LinkedListNode<Operation>, Machine) GetGreedyMachineAllocation(Dictionary<Job, LinkedListNode<Operation>> unscheduledJobOperations, GreedySolution solution)
        {
            var candidateAllocations = unscheduledJobOperations.Values
                                                        .SelectMany(operation => operation.Value.EligibleMachines
                                                                            .Select(machine => (operation, machine)));
            return candidateAllocations.MinBy(om =>
                solution.MachineOccupancy[om.machine] + om.operation.Value.GetProcessingTime(om.machine)
            );
        }
    }
}
