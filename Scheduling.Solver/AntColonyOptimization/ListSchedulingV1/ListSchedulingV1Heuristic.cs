using Scheduling.Solver.AntColonyOptimization.Ants;
using Scheduling.Core.Graph;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1
{
    public static class ListSchedulingV1Heuristic
    {
        public static void Construct(AntV1 ant) 
        {
            ant.InitializeDataStructures();

            var conjunctiveDegreeCounters = new Dictionary<Node, int>();
            ant.Context.DisjunctiveGraph.Vertices.ToList().ForEach(
                node => conjunctiveDegreeCounters.Add(node, node.IncidentConjunctions.Count)
            );

            HashSet<Node> scheduledNodes = [ant.Context.DisjunctiveGraph.Source];
            HashSet<Node> unscheduledNodes = [.. ant.Context.DisjunctiveGraph.Source.Successors];

            while (unscheduledNodes.Any())
            {
                var feasibleMoves = ant.GetFeasibleMoves(unscheduledNodes, scheduledNodes);
                var nextMove = ant.ProbabilityRule(feasibleMoves) as FeasibleMove;

                ant.EvaluateCompletionTime(nextMove.DirectedEdge);
                ant.LocalPheromoneUpdate(nextMove.DirectedEdge);

                // update data structures
                unscheduledNodes.Remove(nextMove.DirectedEdge.Target);
                scheduledNodes.Add(nextMove.DirectedEdge.Target);
                nextMove.DirectedEdge.Target.Successors.ForEach(node =>
                {
                    conjunctiveDegreeCounters[node] -= 1;
                    if (conjunctiveDegreeCounters[node] == 0)
                        unscheduledNodes.Add(node);
                });
            }
        
            ant.SinksToSink();
        }
    }
}
