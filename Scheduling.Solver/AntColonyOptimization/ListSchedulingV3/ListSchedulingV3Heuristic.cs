using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Model;
using Scheduling.Solver.DataStructures;
using System.Xml.Linq;
using Scheduling.Core.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3
{
    public static class ListSchedulingV3Heuristic<TContext, TAnt>
        where TContext : AntColonyV3AlgorithmSolver<TContext, TAnt>
        where TAnt : AntV3<TAnt, TContext>
    {
        public static void Construct(TAnt ant)
        {
            ant.InitializeDataStructures();

            var conjunctiveDegreeCounters = new Dictionary<AbstractVertex, int>();
            ant.DisjunctiveGraph.VertexSet.ToList().ForEach(
                node => conjunctiveDegreeCounters.Add(node, ant.DisjunctiveGraph.NeighbourhoodIn<ConjunctiveArc>(node).Count)
            );

            // we start with source scheduled
            HashSet<AbstractVertex> unscheduledNodes = [];
            HashSet<AbstractVertex> scheduledNodes = [ant.DisjunctiveGraph.Source];
            ReleaseVertexSuccessors(ant, ant.DisjunctiveGraph.Source, conjunctiveDegreeCounters, scheduledNodes, unscheduledNodes);

            while (unscheduledNodes.Any())
            {
                var feasibleMoves = ant.GetFeasibleMoves(unscheduledNodes, scheduledNodes).ToList();
                if (feasibleMoves.IsEmpty()) break;

                var nextMove = ant.ProbabilityRule(feasibleMoves) as Orientation;

                ant.EvaluateCompletionTime(nextMove.Arc);
                ant.LocalPheromoneUpdate(nextMove.Arc);

                // update data structures
                unscheduledNodes.Remove(nextMove.Arc.Head);
                scheduledNodes.Add(nextMove.Arc.Head);

                ReleaseVertexSuccessors(ant, nextMove.Arc.Head, conjunctiveDegreeCounters, scheduledNodes, unscheduledNodes);
            }

            ant.ExtractConjunctiveDigraph();
        }

        private static void ReleaseVertexSuccessors(TAnt ant, AbstractVertex u, Dictionary<AbstractVertex, int> vertexCounters, HashSet<AbstractVertex> scheduledNodes, HashSet<AbstractVertex> unscheduledNodes)
        {
            foreach (var v in ant.DisjunctiveGraph.NeighbourhoodOut<ConjunctiveArc>(u))
            {
                vertexCounters[v] -= 1;
                if (scheduledNodes.DoesNotContain(x => x.Id == v.Id) && vertexCounters[v] == 0)
                    unscheduledNodes.Add(v);
            }
        }
    }
}
