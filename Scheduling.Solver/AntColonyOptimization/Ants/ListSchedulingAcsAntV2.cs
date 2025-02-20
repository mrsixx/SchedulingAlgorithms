using QuikGraph.Algorithms;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.Solvers;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization.Ants
{
    public class ListSchedulingAcsAntV2(int id, int generation, AntColonySystemAlgorithmSolver context)
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
                
                LocalPheromoneUpdate(nextMove.DirectedEdge);

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
            
            return PseudoProbabilityRule(feasibleMoves);
        }

        private IFeasibleMove PseudoProbabilityRule(IEnumerable<IFeasibleMove> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(IFeasibleMove Move, double Probability)>();
            IFeasibleMove? greedyMove = null;
            var greedyFactor = double.MinValue;
            // create roulette wheel and evaluate greedy move for pseudorandom proportional rule at same time (in O(n))
            foreach (var move in feasibleMoves)
            {
                var tauXy = move.GetPheromoneAmount(context); // pheromone amount
                var etaXy = move.Weight.Inverse(); // heuristic information
                var tauXyAlpha = Math.Pow(tauXy, context.Alpha); // pheromone amount raised to power alpha
                var etaXyBeta = Math.Pow(etaXy, context.Beta); // heuristic information raised to power beta

                double probFactor = tauXyAlpha * etaXyBeta, pseudoProbFactor = tauXy * etaXyBeta;
                rouletteWheel.Add((move, probFactor));
                sum += probFactor;

                if (greedyFactor >= pseudoProbFactor) continue;
                greedyFactor = pseudoProbFactor;
                greedyMove = move;
            }

            // pseudo random proportional rule
            if (Random.Shared.NextDouble() <= context.Q0)
                return greedyMove;

            // roulette wheel
            var cumulative = 0.0;
            var randomValue = Random.Shared.NextDouble() * sum;

            foreach (var (move, probability) in rouletteWheel)
            {
                cumulative += probability;
                if (randomValue <= cumulative)
                    return move;
            }

            throw new InvalidOperationException("FATAL ERROR: No move was selected.");
        }

        private void LocalPheromoneUpdate(Orientation selectedMove)
        {
            if (!Context.PheromoneTrail.TryGetValue(selectedMove, out double currentPheromoneValue) || !Context.PheromoneTrail.TryUpdate(selectedMove, (1 - context.Phi) * currentPheromoneValue + context.Phi * context.Tau0, currentPheromoneValue))
                Console.WriteLine("Unable to decay pheromone after construction step...");
        }
    }
}
