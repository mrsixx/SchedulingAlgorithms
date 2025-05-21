using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2
{
    public static class ListSchedulingV2Heuristic<TContext, TAnt>
        where TContext : AntColonyV2AlgorithmSolver<TContext, TAnt>
        where TAnt : AntV2<TAnt, TContext>
    {
        public static void Construct(TAnt ant)
        {
            // creating data structures
            var unscheduledOperations = new HashSet<Operation>();
            ant.Instance.Jobs.ForEach(job => unscheduledOperations.Add(job.Operations.First.Value));
            ant.Instance.Machines.ForEach(m => ant.LoadingSequence.Add(m, new List<Operation>()));

            while (unscheduledOperations.Any())
            {
                var feasibleMoves = ant.GetFeasibleMoves(unscheduledOperations);
                var nextMove = ant.ProbabilityRule(feasibleMoves) as FeasibleMoveV2;

                var operationLinkedListNode = nextMove.Operation.Job.Operations.Find(nextMove.Operation);

                ant.EvaluateCompletionTime(nextMove, operationLinkedListNode);
                ant.LocalPheromoneUpdate(nextMove.Allocation);

                unscheduledOperations.Remove(nextMove.Operation);
                if (operationLinkedListNode.Next is not null)
                    unscheduledOperations.Add(operationLinkedListNode.Next.Value);
            }
            //Log($"Solution found after {solution.Watch.Elapsed}.");
            //Log("\nFinishing execution...");

            // creating mu function
            foreach (var (m, operations) in ant.LoadingSequence)
            foreach (var o in operations)
                ant.MachineAssignment.Add(o.Id, m);
        }
    }
}
