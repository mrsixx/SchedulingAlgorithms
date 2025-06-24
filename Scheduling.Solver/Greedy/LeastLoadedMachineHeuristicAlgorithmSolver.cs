using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Algorithms;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;

namespace Scheduling.Solver.Greedy
{
    public class LeastLoadedMachineHeuristicAlgorithmSolver(bool withLocalSearch = false) : IFlexibleJobShopSchedulingSolver
    {
        protected ILogger? Logger;
        public void Log(string message) => Logger?.Log(message);

        public bool DisableLocalSearch { get; } = !withLocalSearch;

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
            instance.Jobs.ForEach(job =>
            {
                job.Operations.ForEach(o => solution.CriticalPredecessors.Add(o.Id, null));
                unscheduledJobOperations.Add(job, job.Operations[0]);
            });
            instance.Machines.ForEach(m =>
            {
                solution.LoadingSequence.Add(m.Index, []);
                solution.MachineOccupancy.Add(m, 0);
            });

            while (unscheduledJobOperations.Any())
            {
                var (operation, machine) = GetGreedyMachineAllocation(unscheduledJobOperations, solution);
                // evaluate start e completion times
                List<Operation> predecessors = [];
                if (!operation.FirstOperation) //non initial operations
                    predecessors.Add(operation.Job.Operations[operation.Index - 1]);

                if (solution.LoadingSequence[machine.Index].Any()) // if exist machine predecessor
                    predecessors.Add(solution.LoadingSequence[machine.Index].Last());


                var startTime = Convert.ToDouble(operation.Job.ReleaseDate); // s(o) >= r_j for all o \in \underline{\pi_j}
                if (predecessors.Any())
                {
                    var criticalPredecessor = predecessors.MaxBy(p => solution.CompletionTimes[p.Id]);
                    var criticalPredecessorCompletionTime = criticalPredecessor != null
                        ? solution.CompletionTimes[criticalPredecessor.Id]
                        : 0;

                    solution.CriticalPredecessors[operation.Id] = criticalPredecessor;
                    startTime = Math.Max(startTime, criticalPredecessorCompletionTime);
                }

                var completionTime = startTime + operation.GetProcessingTime(machine);
                solution.StartTimes.TryAdd(operation.Id, startTime);
                solution.CompletionTimes.TryAdd(operation.Id, completionTime);

                // updating data structures
                solution.LoadingSequence[machine.Index].Add(operation);
                solution.MachineOccupancy[machine] = completionTime;

                if (operation.LastOperation)
                    unscheduledJobOperations.Remove(operation.Job);
                else
                    unscheduledJobOperations[operation.Job] = operation.Job.Operations[operation.Index + 1];
            }
            solution.Watch.Stop();
            Log($"Solution found after {solution.Watch.Elapsed}.");
            Log("\nFinishing LLM execution...");

            // creating mu function
            foreach (var (m, operations) in solution.LoadingSequence)
                foreach (var o in operations)
                    solution.MachineAssignment.Add(o.Id, m);
            if (DisableLocalSearch)
                return solution;
            return FlexibleJobShopLocalSearchAlgorithm<GreedySolution>.Run(instance, solution);
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
