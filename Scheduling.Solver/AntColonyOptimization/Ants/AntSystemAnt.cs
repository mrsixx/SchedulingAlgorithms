using Scheduling.Core.Extensions;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.Solvers;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization.Ants
{
    public class AntSystemAnt(int id, int generation, AntSystemAlgorithmSolver context) : BaseAnt(id, generation, context)
    {

        public override void WalkAround()
        {
            InitializeDataStructures();

            var conjunctiveDegreeCounters = new Dictionary<Node, int>();
            Context.DisjunctiveGraph.Vertices.ToList().ForEach(
                node => conjunctiveDegreeCounters.Add(node, node.IncidentConjunctions.Count)
            );

            HashSet<Node> scheduledNodes = [Context.DisjunctiveGraph.Source];
            HashSet<Node> unscheduledNodes = [.. Context.DisjunctiveGraph.Source.Successors];

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

        private IFeasibleMove ProbabilityRule(IEnumerable<IFeasibleMove> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(IFeasibleMove Move, double Probability)>();

            // create roulette wheel and evaluate greedy move for pseudorandom proportional rule at same time (in O(n))
            foreach (var move in feasibleMoves)
            {
                var tauXy = move.GetPheromoneAmount(context); // pheromone amount
                var etaXy = move.Weight.Inverse(); // heuristic information
                var tauXyAlpha = Math.Pow(tauXy, context.Alpha); // pheromone amount raised to power alpha
                var etaXyBeta = Math.Pow(etaXy, context.Beta); // heuristic information raised to power beta

                double probFactor = tauXyAlpha * etaXyBeta;
                rouletteWheel.Add((move, probFactor));
                sum += probFactor;
            }

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

        private void EvaluateCompletionTime(Orientation selectedMove)
        {
            var node = selectedMove.Target;
            var machine = selectedMove.Machine;

            var jobPredecessorNode = node.DirectPredecessor;
            var machinePredecessorNode = LoadingSequence[machine].Peek();


            var jobCompletionTime = CompletionTimes[jobPredecessorNode.Operation];
            var machineCompletionTime = CompletionTimes[machinePredecessorNode.Operation];
            var processingTime = node.Operation.GetProcessingTime(machine);

            // update loading sequence, starting and completion times
            StartTimes[node.Operation] = Math.Max(machineCompletionTime, jobCompletionTime);
            CompletionTimes[node.Operation] = StartTimes[node.Operation] + processingTime;
            LoadingSequence[machine].Push(node);
            if (!MachineAssignment.TryAdd(node.Operation, machine))
                throw new Exception($"Machine already assigned to this operation");

            ConjunctiveGraph.AddConjunctionAndVertices(selectedMove);
            ConjunctiveGraph.AddConjunctionAndVertices(new Conjunction(jobPredecessorNode, node));
        }

    }
}
