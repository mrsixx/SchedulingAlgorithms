using System.Linq.Expressions;
using System.Reflection;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.Algorithms
{
    public static class FlexibleJobShopLocalSearchAlgorithm<T> where T : IFjspSolution, new()
    {
        public static T Run(Instance instance, T initialSolution)
        {
            HashSet<T> neighbors = GenerateNeighbours(instance, initialSolution);
            if (!neighbors.Any()) return initialSolution;

            var bestNeighbor = neighbors.MinBy(n => n.Makespan);
            var improved = bestNeighbor is not null && bestNeighbor.Makespan < initialSolution.Makespan;
            if (!improved) return initialSolution;

            Console.WriteLine($"IMPROVED: from {initialSolution.Makespan} to {bestNeighbor.Makespan} ({(initialSolution.Makespan - bestNeighbor.Makespan) * 100 / initialSolution.Makespan}%)");
            return bestNeighbor;

        }


        /// <summary>
        /// Generate neighbours based on (BRUCKER;KNUST, 2012) neighbourhood structure N^{2}_{ca} (interchanges first or the last two operations of a critical block of operations in the critical path).
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="initialSolution"></param>
        /// <returns></returns>
        private static HashSet<T> GenerateNeighbours(Instance instance, T initialSolution)
        {
            var neighbours = new HashSet<T>();
            var criticalPath = CriticalPathMethodAlgorithm.EvaluateCriticalPath(instance, initialSolution);
            var blocks = ExtractCriticalBlocks(initialSolution, criticalPath);

            foreach (var block in blocks)
            {
                if (block.Count < 2) continue;

                var machineIdx = initialSolution.MachineAssignment[block[0].Id];
                var machineSequence = initialSolution.LoadingSequence[machineIdx] ?? [];

                // Interchange two first operations of the block
                var neighbour1 = GenerateNeighbourSwappingIndex(instance, initialSolution, machineIdx, block, 0, 1);
                if (neighbour1 != null)
                    neighbours.Add(neighbour1);

                // Interchange two last operations of the block
                var n = block.Count;
                var neighbour2 = GenerateNeighbourSwappingIndex(instance, initialSolution, machineIdx, block, n-2, n-1);
                if (neighbour2 != null)
                    neighbours.Add(neighbour2);
            }

            return neighbours;
        }

        private static T? GenerateNeighbourSwappingIndex(Instance instance, T initialSolution, int machineIdx,
            List<Operation> block, int blockIdxA, int blockIdxB)
        {
            var opA = block[blockIdxA];
            var opB = block[blockIdxB];
            if (opA.JobId == opB.JobId)
                return default;

            var copy = CloneSolution(initialSolution);
            var seq = copy.LoadingSequence[machineIdx];

            int idxA = seq.FindIndex(o => o.Id == opA.Id);
            int idxB = seq.FindIndex(o => o.Id == opB.Id);
            if (idxA < 0 || idxB < 0 || Math.Abs(idxA - idxB) != 1)
                return default;

            (seq[idxA], seq[idxB]) = (seq[idxB], seq[idxA]);
            EvaluateSolution(instance, copy); //TODO only evaluate if improve makespan (Brucker; Knust, 2012, p. 249)
            return copy;
        }

        // extract operation blocks from critical path
        private static List<List<Operation>> ExtractCriticalBlocks(T initialSolution, Operation[] criticalPath)
        {
            var blocks = new List<List<Operation>>();
            int cpLength = criticalPath.Length;
            List<Operation> currentBlock = criticalPath.Length > 0 ? [criticalPath[0]] : [];
            for (int i = 1; i < cpLength; i++)
            {
                Operation curOperation = criticalPath[i];
                var curBlockMachine = initialSolution.MachineAssignment[currentBlock.Last().Id];
                var curMachine = initialSolution.MachineAssignment[curOperation.Id];
                if (curBlockMachine == curMachine)
                {
                    currentBlock.Add(curOperation);
                }
                else
                {
                    if (currentBlock.Count > 2)
                        blocks.Add(currentBlock);

                    currentBlock = [curOperation];
                }
            }

            return blocks;
        }


        private static void EvaluateSolution(Instance instance, T solution)
        {
            var precedenceDigraph = new PrecedenceDigraph(instance);
            var unscheduledOperations = new List<OperationVertex>();
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
                var mIdx = solution.MachineAssignment[operation.Id];
                var machine = instance.Machines[mIdx];

                // evaluate start e completion times
                List<Operation> predecessors = [];
                if (!operation.FirstOperation) //non initial operations
                    predecessors.Add(operation.Job.Operations[operation.Index - 1]);

                var idx = solution.LoadingSequence[mIdx].IndexOf(operation);
                if (idx > 0) // if has machine predecessor (-1 or 0 it's false)
                    predecessors.Add(solution.LoadingSequence[mIdx][idx - 1]);

                if (predecessors.Any(o => !solution.CompletionTimes.ContainsKey(o.Id)))
                {
                    unscheduledOperations.Remove(node);
                    unscheduledOperations.Add(node);
                    continue;
                }
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

                var processingTime = operation.GetProcessingTime(machine);
                var completionTime = startTime + processingTime;
                solution.StartTimes.Add(operation.Id, startTime);
                solution.CompletionTimes.Add(operation.Id, completionTime);
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
        private static T CloneSolution(IFjspSolution solution)
        {
            var copy = new T();
            foreach (var kvPair in solution.LoadingSequence)
            {
                copy.LoadingSequence.Add(kvPair.Key, [.. kvPair.Value]);
            }


            foreach (var kvPair in solution.CriticalPredecessors)
            {
                copy.CriticalPredecessors.Add(kvPair.Key, null);
            }

            // creating mu function
            foreach (var (mIdx, operations) in copy.LoadingSequence)
                foreach (var o in operations)
                    copy.MachineAssignment.Add(o.Id, mIdx);
            return copy;
        }
    }
}
