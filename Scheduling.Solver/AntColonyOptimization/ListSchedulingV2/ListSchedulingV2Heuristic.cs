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
            ant.Instance.Jobs.ForEach(job => unscheduledOperations.Add(job.Operations[0]));
            ant.Instance.Machines.ForEach(m => ant.Solution.LoadingSequence.Add(m.Index, []));

            while (unscheduledOperations.Any())
            {
                var feasibleMoves = ant.GetFeasibleMoves(unscheduledOperations);
                var nextMove = ant.ProbabilityRule(feasibleMoves) as FeasibleMoveV2;

                ant.EvaluateCompletionTime(ant.Solution, nextMove.Allocation);
                ant.LocalPheromoneUpdate(nextMove.Allocation);
                ant.Solution.MachineAssignment.Add(nextMove.Allocation.Operation.Id, nextMove.Machine.Index);

                unscheduledOperations.Remove(nextMove.Operation);
                if (!nextMove.Operation.LastOperation)
                    unscheduledOperations.Add(nextMove.Operation.Job.Operations[nextMove.Operation.Index + 1]);
            }
           
            if (ant.Context.Parameters.DisableLocalSearch)
                return;
            ant.NonImprovedSolution = ant.Solution;
            ant.Solution = LocalSearchAlgorithm<TContext, TAnt>.Run(ant);
        }


    }
}
