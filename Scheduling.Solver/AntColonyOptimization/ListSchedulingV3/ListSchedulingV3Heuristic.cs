using Scheduling.Core.Extensions;
using Scheduling.Solver.DataStructures;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3
{
    public static class ListSchedulingV3Heuristic<TContext, TAnt>
        where TContext : AntColonyV3AlgorithmSolver<TContext, TAnt>
        where TAnt : AntV3<TAnt, TContext>
    {

        public static void Construct(TAnt ant)
        {
            // creating data structures

            ant.InitializeDataStructures();
            var scheduledOperations = new HashSet<OperationVertex>();
            var unscheduledOperations = new HashSet<OperationVertex>();
            var inDegreeCounters = new Dictionary<OperationVertex, int>();

            ant.PrecedenceDigraph.VertexSet.ForEach(o =>
                inDegreeCounters.Add(o, ant.PrecedenceDigraph.NeighbourhoodIn(o).Count)
            );

            inDegreeCounters.Where(o => o.Value == 0)
                            .ForEach(o => unscheduledOperations.Add(o.Key));


            while (unscheduledOperations.Any())
            {
                var feasibleMoves = ant.GetFeasibleMoves(unscheduledOperations, scheduledOperations);
                var nextMove = ant.ProbabilityRule(feasibleMoves);

                ant.EvaluateCompletionTime(nextMove);
                ant.LocalPheromoneUpdate(nextMove);
                unscheduledOperations.Remove(nextMove.Vertex);

                ReleaseVertexSuccessors(ant, nextMove.Vertex, inDegreeCounters, scheduledOperations, unscheduledOperations);
            }

            // creating mu function
            foreach (var (m, operations) in ant.LoadingSequence)
                foreach (var o in operations)
                    ant.MachineAssignment.Add(o.Id, m);
        }

        private static void ReleaseVertexSuccessors(TAnt ant, OperationVertex u, Dictionary<OperationVertex, int> vertexCounters, HashSet<OperationVertex> scheduledNodes, HashSet<OperationVertex> unscheduledNodes)
        {
            ant.PrecedenceDigraph.NeighbourhoodOut(u)
                .ForEach(v =>
                {
                    vertexCounters[v] -= 1;
                    if (vertexCounters[v] == 0 && scheduledNodes.DoesNotContain(v))
                        unscheduledNodes.Add(v);
                });
        }
    }
}
