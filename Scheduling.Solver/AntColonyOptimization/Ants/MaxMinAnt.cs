using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.Solvers;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization.Ants
{
    public class MaxMinAnt(int id, int generation, MaxMinAntSystemAlgorithmSolver context)
        : BaseAnt(id, generation, context)
    {

        public override void WalkAround()
        { 
            InitializeDataStructures();

            var conjunctiveDegreeCounters = new Dictionary<Node, int>();
            Context.DisjunctiveGraph.Vertices.ToList().ForEach(
                node => conjunctiveDegreeCounters.Add(node, node.IncidentConjunctions.Count)
            );

            HashSet<Node> scheduledNodes = [Context.DisjunctiveGraph.Source];
            HashSet<Node> unscheduledNodes = [..Context.DisjunctiveGraph.Source.Successors];
            
            while (unscheduledNodes.Any())
            {
                var nextMove = ChooseNextFeasibleMove(unscheduledNodes, scheduledNodes);
                
                EvaluateCompletionTime(nextMove.DirectedEdge);

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
            
            SinksToSink();
        }

        /// <summary>
        /// Create feasible moves set and use pseudo probability rule to choose one
        /// </summary>
        /// <param name="unscheduledNodes"></param>
        /// <param name="scheduledNodes"></param>
        /// <returns></returns>
        private IFeasibleMove ChooseNextFeasibleMove(HashSet<Node> unscheduledNodes, HashSet<Node> scheduledNodes)
        {
            // a feasible move is an arc from scheduled node to an unscheduled node
            var feasibleMoves = unscheduledNodes.SelectMany(candidateNode =>
            {
                return candidateNode.IncidentDisjunctions
                    .Where(disjunction => scheduledNodes.Contains(disjunction.Other(candidateNode)))
                    .Select(disjunction => new FeasibleMove(disjunction,
                        disjunction.Target == candidateNode ? Direction.SourceToTarget : Direction.TargetToSource));
            });
            
            return ProbabilityRule(feasibleMoves);
        }
    }
}
