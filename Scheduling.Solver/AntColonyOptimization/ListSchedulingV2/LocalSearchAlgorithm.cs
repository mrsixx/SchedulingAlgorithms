using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3;
using Scheduling.Solver.Models;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2
{
    public static class LocalSearchAlgorithm<TContext, TAnt>
        where TContext : AntColonyV2AlgorithmSolver<TContext, TAnt>
        where TAnt : AntV2<TAnt, TContext>
    {
        public static AntSolution Run(TAnt ant)
        {
            //TODO: enable/disable local search
            HashSet<AntSolution> neighbors = GenerateNeighbours(ant);
            if (neighbors.Any())
            {
                var bestNeighbor = neighbors.MinBy(n => n.Makespan);
                if (bestNeighbor is not null && bestNeighbor.Makespan < ant.Solution.Makespan)
                {
                    Console.WriteLine($"IMPROVED: from {ant.Solution.Makespan} to {bestNeighbor.Makespan} ({(ant.Solution.Makespan - bestNeighbor.Makespan) / ant.Solution.Makespan})");
                    return bestNeighbor;
                }
            }

            return ant.Solution;
        }

        private static HashSet<AntSolution> GenerateNeighbours(TAnt ant)
        {
            var neighbours = new HashSet<AntSolution>();
            foreach (var (mIdx, operations) in ant.Solution.LoadingSequence)
            {
                for (int i = 0; i < operations.Count; i++)
                {
                    var op = operations[i];
                    foreach (var altMachine in op.EligibleMachines.Where(m => m.Index != mIdx))
                    {
                        var originalOps = new List<Operation>(ant.Solution.LoadingSequence[mIdx]);
                        originalOps.RemoveAt(i);
                        for (int pos = 0; pos < ant.Solution.LoadingSequence[altMachine.Index].Count; pos++)
                        {
                            var jobOps = op.Job.Operations.ToList();
                            var index = jobOps.FindIndex(o => o.Id == op.Id);
                            if (index > 0)
                            {
                                var prevOp = jobOps[index - 1];
                                var altOps = ant.Solution.LoadingSequence[altMachine.Index];
                                if (altOps.Take(pos).Any(o => o.Id == prevOp.Id))
                                    continue;
                            }

                            AntSolution copy = CloneSolution(ant.Solution);
                            copy.LoadingSequence[mIdx].Remove(op);
                            copy.LoadingSequence[altMachine.Index].Insert(pos, op);
                            copy.MachineAssignment[op.Id] = altMachine.Index;
                            EvaluateSolution(copy, ant);
                            neighbours.Add(copy);
                        }
                    }
                }
            }

            return neighbours;
        }


        private static void EvaluateSolution(AntSolution sol, TAnt ant)
        {
            var precedenceDigraph = new PrecedenceDigraph(ant.Instance);
            var machineOcupation = new Dictionary<int, double>();
            var scheduledOperations = new HashSet<OperationVertex>();
            var unscheduledOperations = new HashSet<OperationVertex>();
            var inDegreeCounters = new Dictionary<OperationVertex, int>();

            precedenceDigraph.VertexSet.ForEach(o =>
                inDegreeCounters.Add(o, precedenceDigraph.NeighbourhoodIn(o).Count)
            );

            inDegreeCounters.Where(o => o.Value == 0)
                .ForEach(o => unscheduledOperations.Add(o.Key));


            while (unscheduledOperations.Any())
            {
                var node = unscheduledOperations.First();
                var operation = node.Operation;
                var mIdx = sol.MachineAssignment[operation.Id];
                if (!machineOcupation.ContainsKey(mIdx))
                    machineOcupation.Add(mIdx, 0);
                var machine = ant.Instance.Machines[mIdx];

                var jobPredecessor =
                    (operation.FirstOperation) ? null : operation.Job.Operations[operation.Index - 1];

                double jobPredecessorCompletionTime = operation.Job.ReleaseDate;
                if (jobPredecessor is not null)
                {
                    if (!sol.StartTimes.ContainsKey(jobPredecessor.Id))
                        throw new Exception();
                    jobPredecessorCompletionTime = sol.CompletionTimes[jobPredecessor.Id];
                }
                var processingTime = operation.GetProcessingTime(machine);

                var start = Math.Max(jobPredecessorCompletionTime, machineOcupation[mIdx]);
                var end = start + processingTime;
                sol.StartTimes.Add(operation.Id, start);
                sol.CompletionTimes.Add(operation.Id, end);
                machineOcupation[mIdx] = end;
                scheduledOperations.Add(node);
                unscheduledOperations.Remove(node);
                precedenceDigraph.NeighbourhoodOut(node)
                    .ForEach(v =>
                    {
                        inDegreeCounters[v] -= 1;
                        if (inDegreeCounters[v] == 0)
                            unscheduledOperations.Add(v);
                    });
            }
        }

        private static AntSolution CloneSolution(AntSolution antSolution)
        {
            var copy = new AntSolution();
            foreach (var kvPair in antSolution.LoadingSequence)
            {
                copy.LoadingSequence.Add(kvPair.Key, [.. kvPair.Value]);
            }

            // creating mu function
            foreach (var (mIdx, operations) in copy.LoadingSequence)
                foreach (var o in operations)
                    copy.MachineAssignment.Add(o.Id, mIdx);
            return copy;
        }
    }
}
