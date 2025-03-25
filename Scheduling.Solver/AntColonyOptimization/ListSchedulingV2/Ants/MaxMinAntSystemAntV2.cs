using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Algorithms;
using Scheduling.Solver.Models;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants
{
    public class MaxMinAntSystemAntV2(int id, int generation, MaxMinAntSystemAlgorithmV2 context)
        : AntV2<MaxMinAntSystemAntV2, MaxMinAntSystemAlgorithmV2>(id, generation, context)
    {
        public override void WalkAround()
        {
            Solution.Watch.Start();
            // creating data structures
            //Log($"Starting greedy algorithm");
            var unscheduledJobOperations = new Dictionary<Job, LinkedListNode<Operation>>();
            var loadingSequence = new Dictionary<Machine, LinkedList<Operation>>();
            Instance.Jobs.ToList().ForEach(job => unscheduledJobOperations.Add(job, job.Operations.First));
            Instance.Machines.ToList().ForEach(m => loadingSequence.Add(m, new LinkedList<Operation>()));

            while (unscheduledJobOperations.Any())
            {
                var (operation, machine) = GetGreedyMachineAllocation(unscheduledJobOperations, Solution);

                // evaluate start e completion times
                var machinePredecessor = loadingSequence[machine].Last;
                var jobPredecessor = operation.Previous;
                var jobReleaseDate = Convert.ToDouble(operation.Value.Job.ReleaseDate);

                var startTime = Math.Max(
                    machinePredecessor != null ? Solution.CompletionTimes[machinePredecessor.Value.Id] : 0,
                    jobPredecessor != null ? Solution.CompletionTimes[jobPredecessor.Value.Id] : jobReleaseDate
                );

                Solution.StartTimes.TryAdd(operation.Value.Id, startTime);
                Solution.CompletionTimes.TryAdd(operation.Value.Id, startTime + operation.Value.GetProcessingTime(machine));

                // updating data structures
                loadingSequence[machine].AddLast(operation.Value);
                if (operation.Next is null)
                    unscheduledJobOperations.Remove(operation.Value.Job);
                else
                    unscheduledJobOperations[operation.Value.Job] = operation.Next;
            }
            Solution.Watch.Stop();
            //Log($"Solution found after {solution.Watch.Elapsed}.");
            //Log("\nFinishing execution...");

            // creating mu function
            foreach (var (m, operations) in loadingSequence)
            foreach (var o in operations)
                Solution.MachineAssignment.Add(o.Id, m);
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
