using Scheduling.Core.Graph;
using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using Scheduling.Solver.AntColonyOptimization;

namespace Scheduling.Solver.Heuristics
{
    public static class ListSchedulingV2
    {
        //public static void Construct(BaseAnt ant) 
        //{

        //    // var solution = new GreedySolution();
        //    // solution.Watch.Start();

        //    // ant.InitializeDataStructures();
        //    // // creating data structures
        //    // ant.Context.Log($"Starting greedy algorithm");
        //    // var unscheduledJobOperations = new Dictionary<Job, LinkedListNode<Operation>>();
        //    // var loadingSequence = new Dictionary<Machine, LinkedList<Operation>>();
        //    // instance.Jobs.ToList().ForEach(job => unscheduledJobOperations.Add(job, job.Operations.First));
        //    // instance.Machines.ToList().ForEach(m => loadingSequence.Add(m, new LinkedList<Operation>()));

        //    // while (unscheduledJobOperations.Any())
        //    // {
        //    //     var (operation, machine) = GetGreedyMachineAllocation(unscheduledJobOperations, solution);

        //    //     // evaluate start e completion times
        //    //     var machinePredecessor = loadingSequence[machine].Last;
        //    //     var jobPredecessor = operation.Previous;
        //    //     var jobReleaseDate = Convert.ToDouble(operation.Value.Job.ReleaseDate);

        //    //     var startTime = Math.Max(
        //    //         machinePredecessor != null ? solution.CompletionTimes[machinePredecessor.Value.Id] : 0,
        //    //         jobPredecessor != null ? solution.CompletionTimes[jobPredecessor.Value.Id] : jobReleaseDate
        //    //     );

        //    //     solution.StartTimes.TryAdd(operation.Value.Id, startTime);
        //    //     solution.CompletionTimes.TryAdd(operation.Value.Id, startTime + operation.Value.GetProcessingTime(machine));

        //    //     // updating data structures
        //    //     loadingSequence[machine].AddLast(operation.Value);
        //    //     if (operation.Next is null)
        //    //         unscheduledJobOperations.Remove(operation.Value.Job);
        //    //     else
        //    //         unscheduledJobOperations[operation.Value.Job] = operation.Next;
        //    // }
        //    // solution.Watch.Stop();
        //    // ant.Context.Log($"Solution found after {solution.Watch.Elapsed}.");
        //    // ant.Context.Log("\nFinishing execution...");

        //    // // creating mu function
        //    // foreach (var (m, operations) in loadingSequence)
        //    //     foreach (var o in operations)
        //    //         solution.MachineAssignment.Add(o.Id, m);

        //    // //solution;
        //}
    }
}
