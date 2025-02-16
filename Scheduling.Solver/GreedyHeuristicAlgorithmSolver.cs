using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using System.Diagnostics;

namespace Scheduling.Solver
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
            Stopwatch sw = new();
            Log($"Starting greedy algorithm");
            sw.Start();
            // creating data structures
            var solution = new GreedySolution();
            var unscheduledJobOperations = new Dictionary<Job, LinkedListNode<Operation>>();
            var loadingSequence = new Dictionary<Machine, LinkedList<Operation>>();
            instance.Jobs.ToList().ForEach(job => unscheduledJobOperations.Add(job, job.Operations.First));
            instance.Machines.ToList().ForEach(m => loadingSequence.Add(m, new LinkedList<Operation>()));

            while (unscheduledJobOperations.Any())
            {
                var (operation, machine) = GetGreedyMachineAllocation(unscheduledJobOperations, solution);

                // evaluate start e completion times
                var machinePredecessor = loadingSequence[machine].Last;
                var jobPredecessor = operation.Previous;

                var startTime = Math.Max(
                    machinePredecessor != null ? solution.CompletionTimes[machinePredecessor.Value] : 0,
                    jobPredecessor != null ? solution.CompletionTimes[jobPredecessor.Value] : 0
                );

                solution.StartTimes.TryAdd(operation.Value, startTime);
                solution.CompletionTimes.TryAdd(operation.Value, startTime + operation.Value.GetProcessingTime(machine));

                // updating data structures
                loadingSequence[machine].AddLast(operation.Value);
                if (operation.Next is null)
                    unscheduledJobOperations.Remove(operation.Value.Job);
                else
                    unscheduledJobOperations[operation.Value.Job] = operation.Next;
            }
            sw.Stop();
            Log($"Solution found after {sw.Elapsed}.");
            Log("\nFinishing execution...");
            Log($"Makespan: {solution.Makespan}");

            // creating mu function
            foreach (var (m, operations) in loadingSequence)
                foreach (var o in operations)
                    solution.MachineAssignment.Add(o, m);

            return solution;
        }

        private (LinkedListNode<Operation>, Machine) GetGreedyMachineAllocation(Dictionary<Job, LinkedListNode<Operation>> unscheduledJobOperations, GreedySolution solution)
        {
            var candidateAllocations = unscheduledJobOperations.Values
                                                        .SelectMany(operation => operation.Value.EligibleMachines
                                                                            .Select(machine => (operation, machine)));

            return candidateAllocations.MinBy(
                (allocation) => solution.Makespan + allocation.operation.Value.GetProcessingTime(allocation.machine)
            );
        }
    }
}
