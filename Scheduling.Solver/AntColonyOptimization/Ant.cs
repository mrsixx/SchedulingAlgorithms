using QuikGraph.Algorithms;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.Utils;
using System.Diagnostics;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class Ant
    {
        public Ant(int id, int generation, AntColonyOptimizationAlgorithmSolver context)
        {
            Id = id;
            Context = context;
            Generation = generation;
            Context.DisjunctiveGraph.Vertices.ToList().ForEach(n =>
            {
                CompletionTimes.Add(n.Operation, 0);
                StartTimes.Add(n.Operation, 0);
            });
            Context.DisjunctiveGraph.Machines.ForEach(m => LoadingSequence.Add(m, new([Context.DisjunctiveGraph.Source])));
            Task = new Task(() => WalkAround());
            Task.Start();
        }

        public int Id { get; init; }

        public int Generation { get; init; }

        public ConjunctiveGraphModel ConjunctiveGraph { get; } = new ConjunctiveGraphModel();

        public Dictionary<Machine, Stack<Node>> LoadingSequence { get; } = [];

        public Dictionary<Operation, Machine> MachineAssignment { get; } = [];

        public Dictionary<Operation, double> CompletionTimes { get; } = [];

        public Dictionary<Operation, double> StartTimes { get; } = [];

        public double Makespan => CompletionTimes[FinalNode.Operation];

        public Node StartNode => Context.DisjunctiveGraph.Source;

        public Node FinalNode => Context.DisjunctiveGraph.Sink;

        public AntColonyOptimizationAlgorithmSolver Context { get; }

        public Task Task { get; }

        private void WalkAround()
        {
            var remainingNodes = Context.DisjunctiveGraph.OperationVertices.ToHashSet();

            Stopwatch timer = new();
            while (remainingNodes.Count > 0)
            {
                List<Node> allowedNodes = GetAllowedNodes(remainingNodes);

                var feasibleMoves = GetFeasibleMoves(allowedNodes);
                if (feasibleMoves.Any())
                {
                    //todo: descobrir como baixar o tempo do ChooseNextMove (atualmente está levando na ordem dos décimos de segundo)
                    Orientation selectedMove = ChooseNextMove(feasibleMoves);
                    if (selectedMove is null)
                    {
                        Console.WriteLine("FATAL ERROR: Set of feasible moves is empty :/");
                        return;
                    }
                    EvaluateCompletionTime(selectedMove);
                    LocalPheromoneUpdate(selectedMove);

                    remainingNodes.Remove(selectedMove.Target);
                }
            }
            LinkinToSink();
        }

        private void LocalPheromoneUpdate(Orientation selectedMove)
        {
            if (!Context.PheromoneTrail.TryGetValue(selectedMove, out double currentPheromoneValue) || !Context.PheromoneTrail.TryUpdate(selectedMove, (1 - Context.Phi) * currentPheromoneValue + Context.Phi * Context.Tau0, currentPheromoneValue))
                Console.WriteLine("Unable to decay pheromone after construction step...");
        }

        public void Log()
        {
            // print loading sequence
            foreach (var machine in LoadingSequence.Keys)
            {
                Console.Write($"{machine.Id}: ");
                foreach (var node in LoadingSequence[machine].Reverse())
                    Console.Write($" {node}[{StartTimes[node.Operation]}-{CompletionTimes[node.Operation]}] ");

                Console.WriteLine("");
            }

            // printing topological sort (acyclic evidence)
            Console.WriteLine("Topological sort: ");
            foreach (var node in ConjunctiveGraph.TopologicalSort())
                Console.Write($" {node} ");
            Console.WriteLine("");
        }

        private void LinkinToSink()
        {
            var sinkDisjunctions = FinalNode.IncidentDisjunctions;
            var sinks = ConjunctiveGraph.Sinks().ToList();
            foreach (var sink in sinks)
            {
                var machine = MachineAssignment[sink.Operation];
                var disjunction = sink.IncidentDisjunctions.Intersect(sinkDisjunctions).First(d => d.Machine == machine);
                if (disjunction is null) continue;
                var orientation = disjunction.EquivalentConjunctions.First(c => c.Target == FinalNode);
                ConjunctiveGraph.AddConjunctionAndVertices(orientation);
                CompletionTimes[FinalNode.Operation] = Math.Max(CompletionTimes[FinalNode.Operation], CompletionTimes[sink.Operation]);
            }
        }

        private void EvaluateCompletionTime(Orientation selectedMove)
        {
            var node = selectedMove.Target;
            var machine = selectedMove.Machine;
            EvaluateCompletionTime(node, machine);
            ConjunctiveGraph.AddConjunctionAndVertices(selectedMove);
        }

        private void EvaluateCompletionTime(Node node, Machine machine)
        {

            var jobPredecessorNode = node.DirectPredecessor;
            var machinePredecessorNode = LoadingSequence[machine].Peek();
            ConjunctiveGraph.AddConjunctionAndVertices(new Conjunction(jobPredecessorNode, node));
            double jobCompletionTime = CompletionTimes[jobPredecessorNode.Operation];
            double machineCompletionTime = CompletionTimes[machinePredecessorNode.Operation];

            var processingTime = node.Operation.GetProcessingTime(machine);
            CompletionTimes[node.Operation] = Math.Max(machineCompletionTime, jobCompletionTime) + processingTime;
            StartTimes[node.Operation] = CompletionTimes[node.Operation] - processingTime;

            LoadingSequence[machine].Push(node);
            MachineAssignment.Add(node.Operation, machine);
        }

        //TODO: Maybe theres a better way of choose a random Orientation (too slow)
        private Orientation ChooseNextMove(IEnumerable<IFeasibleMove> feasibleMoves)
        {
            var sum = 0.0;
            // calculate the product tauK^ALPHA * etaK ^ BETA of the ant k for each edge
            var factors = feasibleMoves.Select(move =>
            {
                // assume that x = edge.Start and y = edge.Target
                var tauK_xy = move.GetPheromoneAmount(Context); //pheromone amount over x -> y path
                var etaK_xy = move.Weight.Inverse(); // heuristic information (processing time) of including x -> y

                var factorAlphaBeta = Math.Pow(tauK_xy, Context.Alpha) * Math.Pow(etaK_xy, Context.Beta);
                var factorBeta = tauK_xy * Math.Pow(etaK_xy, Context.Beta);
                sum += factorAlphaBeta;
                return new Tuple<IFeasibleMove, double, double>(move, factorAlphaBeta, factorBeta);
            }).ToList();

            //pseudorandom proportional rule
            if (Random.Shared.NextDouble() <= Context.Q0)
            {
                var max = factors.MaxBy(triple => triple.Item3);
                return max.Item1.DirectedEdge;
            }


            var roulette = factors.Aggregate(new RouletteWheelSelection<Orientation>(), (roulette, pair) =>
            {
                var (move, factor, _) = pair;

                roulette.AddItem(move.DirectedEdge, factor / sum);
                return roulette;
            });
            // calculate the probability pK of the ant k choosing each edge
            // then we draw one based on probability
            return roulette.SelectItem();
        }

        private static Func<Node, bool> DoesNotContainPredecessorsIn(HashSet<Node> remainingNodes)
        {
            return v => v.Predecessors.Intersect(remainingNodes).IsEmpty();
        }

        private static List<Node> GetAllowedNodes(HashSet<Node> remainingNodes)
        {
            //TODO: em vez de gerar a lista a cada iteração, remover os nós da lista já existente
            return remainingNodes
                    .Where(DoesNotContainPredecessorsIn(remainingNodes)).ToList();
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
        private IEnumerable<IFeasibleMove> GetFeasibleMoves(List<Node> candidateNodes)
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

        private void Measure(Stopwatch timer, string msg, Action action)
        {
            timer.Restart();
            action.Invoke();
            timer.Stop();
            Console.WriteLine($"{msg}: {timer.Elapsed}");
        }

    }
}
