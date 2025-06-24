using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scheduling.Solver.Algorithms
{
    public static class FlexibleJobShopLocalSearch2Algorithm<T> where T : IFjspSolution, new()
    {
        //public static T Run(Instance instance, T initialSolution)
        //{
        //    HashSet<T> neighbors = GenerateNeighbours(instance, initialSolution);
        //    if (!neighbors.Any()) return initialSolution;

        //    var bestNeighbor = neighbors.MinBy(n => n.Makespan);
        //    var improved = bestNeighbor is not null && bestNeighbor.Makespan < initialSolution.Makespan;
        //    if (!improved) return initialSolution;

        //    Console.WriteLine($"IMPROVED: from {initialSolution.Makespan} to {bestNeighbor.Makespan} ({(initialSolution.Makespan - bestNeighbor.Makespan) * 100 / initialSolution.Makespan}%)");
        //    return bestNeighbor;

        //}

        //private static HashSet<T> GenerateNeighbours(Instance instance, T initialSolution)
        //{
        //    var neighbours = new HashSet<AntSolution>();
        //    foreach (var (mIdx, operations) in initialSolution.LoadingSequence)
        //    {
        //        for (int i = 0; i < operations.Count; i++)
        //        {
        //            var op = operations[i];
        //            foreach (var altMachine in op.EligibleMachines.Where(m => m.Index != mIdx))
        //            {
        //                var originalOps = new List<Operation>(initialSolution.LoadingSequence[mIdx]);
        //                originalOps.RemoveAt(i);
        //                for (int pos = 0; pos < initialSolution.LoadingSequence[altMachine.Index].Count; pos++)
        //                {
        //                    var jobOps = op.Job.Operations.ToList();
        //                    var index = jobOps.FindIndex(o => o.Id == op.Id);
        //                    if (index > 0)
        //                    {
        //                        var prevOp = jobOps[index - 1];
        //                        var altOps = initialSolution.LoadingSequence[altMachine.Index];
        //                        if (altOps.Take(pos).Any(o => o.Id == prevOp.Id))
        //                            continue;
        //                    }

        //                    AntSolution copy = CloneSolution(initialSolution);
        //                    copy.LoadingSequence[mIdx].Remove(op);
        //                    copy.LoadingSequence[altMachine.Index].Insert(pos, op);
        //                    copy.MachineAssignment[op.Id] = altMachine.Index;
        //                    EvaluateSolution(copy, ant);
        //                    neighbours.Add(copy);
        //                }
        //            }
        //        }
        //    }

        //    return neighbours;
        //}

        //// extract operation blocks from critical path
        //private static List<List<Operation>> ExtractCriticalBlocks(T initialSolution, Operation[] criticalPath)
        //{
        //    var blocks = new List<List<Operation>>();
        //    int cpLength = criticalPath.Length;
        //    List<Operation> currentBlock = criticalPath.Length > 0 ? [criticalPath[0]] : [];
        //    for (int i = 1; i < cpLength; i++)
        //    {
        //        Operation curOperation = criticalPath[i];
        //        var curBlockMachine = initialSolution.MachineAssignment[currentBlock.Last().Id];
        //        var curMachine = initialSolution.MachineAssignment[curOperation.Id];
        //        if (curBlockMachine == curMachine)
        //        {
        //            currentBlock.Add(curOperation);
        //        }
        //        else
        //        {
        //            if (currentBlock.Count > 2)
        //                blocks.Add(currentBlock);

        //            currentBlock = [curOperation];
        //        }
        //    }

        //    return blocks;
        //}


        //private static void EvaluateSolution(Instance instance, T solution)
        //{
        //    var precedenceDigraph = new PrecedenceDigraph(instance);
        //    var machineOcupation = new Dictionary<int, double>();
        //    var scheduledOperations = new HashSet<OperationVertex>();
        //    var unscheduledOperations = new List<OperationVertex>();
        //    var inDegreeCounters = new Dictionary<OperationVertex, int>();

        //    precedenceDigraph.VertexSet.ForEach(o =>
        //        inDegreeCounters.Add(o, precedenceDigraph.NeighbourhoodIn(o).Count)
        //    );

        //    inDegreeCounters.Where(o => o.Value == 0)
        //        .ForEach(o => unscheduledOperations.Add(o.Key));


        //    while (unscheduledOperations.Any())
        //    {
        //        var node = unscheduledOperations.First();
        //        var operation = node.Operation;
        //        var mIdx = solution.MachineAssignment[operation.Id];
        //        machineOcupation.TryAdd(mIdx, 0);
        //        var machine = instance.Machines[mIdx];

        //        // evaluate start e completion times
        //        List<Operation> predecessors = [];
        //        if (!operation.FirstOperation) //non initial operations
        //            predecessors.Add(operation.Job.Operations[operation.Index - 1]);

        //        var idx = solution.LoadingSequence[mIdx].IndexOf(operation);
        //        if (idx > 0) // if has machine predecessor (-1 or 0 it's false)
        //            predecessors.Add(solution.LoadingSequence[mIdx][idx - 1]);

        //        if (predecessors.Any(o => !solution.CompletionTimes.ContainsKey(o.Id)))
        //        {
        //            unscheduledOperations.Remove(node);
        //            unscheduledOperations.Add(node);
        //            continue;
        //        }
        //        var startTime = Convert.ToDouble(operation.Job.ReleaseDate); // s(o) >= r_j for all o \in \underline{\pi_j}
        //        if (predecessors.Any())
        //        {
        //            var criticalPredecessor = predecessors.MaxBy(p => solution.CompletionTimes[p.Id]);
        //            var criticalPredecessorCompletionTime = criticalPredecessor != null
        //                ? solution.CompletionTimes[criticalPredecessor.Id]
        //                : 0;

        //            solution.CriticalPredecessors[operation.Id] = criticalPredecessor;
        //            startTime = Math.Max(startTime, criticalPredecessorCompletionTime);
        //        }

        //        var processingTime = operation.GetProcessingTime(machine);
        //        var completionTime = startTime + processingTime;
        //        solution.StartTimes.Add(operation.Id, startTime);
        //        solution.CompletionTimes.Add(operation.Id, completionTime);
        //        machineOcupation[mIdx] = completionTime;
        //        scheduledOperations.Add(node);
        //        unscheduledOperations.Remove(node);
        //        precedenceDigraph.NeighbourhoodOut(node)
        //            .ForEach(v =>
        //            {
        //                inDegreeCounters[v] -= 1;
        //                if (inDegreeCounters[v] == 0)
        //                    unscheduledOperations.Add(v);
        //            });
        //    }
        //}

        //private static T CloneSolution(IFjspSolution solution)
        //{
        //    var copy = new T();
        //    foreach (var kvPair in solution.LoadingSequence)
        //    {
        //        copy.LoadingSequence.Add(kvPair.Key, [.. kvPair.Value]);
        //    }


        //    foreach (var kvPair in solution.CriticalPredecessors)
        //    {
        //        copy.CriticalPredecessors.Add(kvPair.Key, null);
        //    }

        //    // creating mu function
        //    foreach (var (mIdx, operations) in copy.LoadingSequence)
        //        foreach (var o in operations)
        //            copy.MachineAssignment.Add(o.Id, mIdx);
        //    return copy;
        //}
    }
}
