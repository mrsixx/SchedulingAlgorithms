using QuikGraph.Algorithms;
using Scheduling.Core.Extensions;
using Scheduling.Solver.Interfaces;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1;
using System.Diagnostics;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV0
{
    /// <summary>
    /// ACS List scheduling iterative ant V1
    /// </summary>
    /// <param name="id"></param>
    /// <param name="generation"></param>
    /// <param name="context"></param>
    public class AntColonySystemAntV0(int id, int generation, AntColonySystemAlgorithmSolver context)
        : AntV1(id, generation)
    {
        public override AntColonyAlgorithmSolverBase Context => context;
        public HashSet<Node> RemainingNodes { get; } = [];


        public override void WalkAround()
        {
            InitializeDataStructures();
            Console.WriteLine($"#{Id}th ant is cooking...");
            RemainingNodes.AddRange(DisjunctiveGraph.OperationVertices);

            Stopwatch timer = new();
            while (RemainingNodes.Count > 0)
            {
                IEnumerable<Node> allowedNodes = RemainingNodes.Where(v => !v.Predecessors.Any(RemainingNodes.Contains));

                var feasibleMoves = GetFeasibleMoves(allowedNodes);
                if (!feasibleMoves.Any()) continue;

                //TODO: descobrir como baixar o tempo do ChooseNextMove (atualmente está levando na ordem dos décimos de segundo)
                var selectedMove = ChooseNextMove(feasibleMoves);

                EvaluateCompletionTime(selectedMove);
                LocalPheromoneUpdate(selectedMove);

                RemainingNodes.Remove(selectedMove.Target);
            }
            LinkinToSink();
        }

        public override void LocalPheromoneUpdate(Orientation selectedMove)
        {
            if (!Context.PheromoneTrail.TryGetValue(selectedMove, out double currentPheromoneValue) || !Context.PheromoneTrail.TryUpdate(selectedMove, (1 - context.Phi) * currentPheromoneValue + context.Phi * context.Tau0, currentPheromoneValue))
                Console.WriteLine("Unable to decay pheromone after construction step...");
        }

        private void LinkinToSink()
        {
            var sinkDisjunctions = FinalNode.IncidentDisjunctions;
            var sinks = ConjunctiveGraph.Sinks().ToList();
            foreach (var sink in sinks)
            {
                var machine = MachineAssignment[sink.Operation.Id];
                var disjunction = sink.IncidentDisjunctions.Intersect(sinkDisjunctions).First(d => d.Machine == machine);
                if (disjunction is null) continue;
                var orientation = disjunction.Orientations.First(c => c.Target == FinalNode);
                ConjunctiveGraph.AddConjunctionAndVertices(orientation);
                CompletionTimes[FinalNode.Operation.Id] = Math.Max(CompletionTimes[FinalNode.Operation.Id], CompletionTimes[sink.Operation.Id]);
            }
        }

        private Orientation ChooseNextMove(IEnumerable<IFeasibleMove<Orientation>> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(FeasibleMove Move, double Probability)>();
            FeasibleMove? greedyMove = null;
            var greedyFactor = double.MinValue;
            // create roulette wheel and evaluate greedy move for pseudorandom proportional rule at same time (in O(n))
            foreach (var move in feasibleMoves)
            {
                var tauXy = move.GetPheromoneAmount(Context.PheromoneTrail); // pheromone amount
                var etaXy = move.Weight.Inverse(); // heuristic information
                var tauXyAlpha = Math.Pow(tauXy, Context.Alpha); // pheromone amount raised to power alpha
                var etaXyBeta = Math.Pow(etaXy, Context.Beta); // heuristic information raised to power beta

                double probFactor = tauXyAlpha * etaXyBeta, pseudoProbFactor = tauXy * etaXyBeta;
                rouletteWheel.Add((move as FeasibleMove, probFactor));
                sum += probFactor;

                if (greedyFactor >= pseudoProbFactor) continue;
                greedyFactor = pseudoProbFactor;
                greedyMove = move as FeasibleMove;
            }

            // pseudo random proportional rule
            if (Random.Shared.NextDouble() <= context.Q0)
                return greedyMove?.DirectedEdge;

            // roulette wheel
            var cumulative = 0.0;
            var randomValue = Random.Shared.NextDouble() * sum;

            foreach (var (move, probability) in rouletteWheel)
            {
                cumulative += probability;
                if (randomValue <= cumulative)
                    return move.DirectedEdge;
            }

            throw new InvalidOperationException("FATAL ERROR: No move was selected.");
        }

        /// <summary>
        /// mark as feasible move 
        /// each disjunctive arc which connects a candidate operation to the last operation of the loading sequence 
        /// denoted by the same kind of arc(it makes the possibility 
        /// that the candidate operation becomes the new last 
        /// operation of that loading sequence);
        /// </summary>
        /// <param name="candidateNodes"></param>
        /// <returns></returns>
        private IEnumerable<IFeasibleMove<Orientation>> GetFeasibleMoves(IEnumerable<Node> candidateNodes)
        {
            //pegar apenas o topo da pilha está restringindo alguma opção?
            var lastScheduledNodes = LoadingSequence.Values.Select(sequence => sequence.Peek());

            var disjunctiveMoves = candidateNodes.SelectMany(candidateNode =>
            {
                return lastScheduledNodes.SelectMany(lastScheduledNode =>
                {
                    var intersection = lastScheduledNode.IncidentDisjunctions.Intersect(candidateNode.IncidentDisjunctions);
                    return intersection.Select(disjunction =>
                    {
                        var direction = disjunction.Target == candidateNode
                                        ? Direction.SourceToTarget
                                        : Direction.TargetToSource;
                        return new FeasibleMove(disjunction, direction);
                    });
                });

            });
            return disjunctiveMoves;
        }
    }
}
