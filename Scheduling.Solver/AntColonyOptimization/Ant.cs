using QuikGraph.Algorithms;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.Utils;
using System.Diagnostics;
using QuikGraph;
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

        public HashSet<Node> RemainingNodes { get; } = [];

        public void WalkAround()
        {
            var grauDeEntradaConjuntivo = new Dictionary<Node, int>();
            
            Context.DisjunctiveGraph.Vertices.ToList().ForEach(
                node => grauDeEntradaConjuntivo.Add(node, node.IncidentConjunctions.Count)
            );
            //TODO: os sucessores da fonte deveriam começar com node.IncidentConjunctions.Count - 1;

            //lista de escalaveis
            HashSet<Node> PendingNodes = [..Context.DisjunctiveGraph.Source.Successors];
            HashSet<Node> ScheduledNodes = [Context.DisjunctiveGraph.Source];
            while (PendingNodes.Any())
            {
                
                var proximoMovimento = SelecionaProximoMovimento(PendingNodes, ScheduledNodes);
                if (proximoMovimento is null)
                    break;
                //calcular starting time e preencher grafo conjuntivo
                //TODO: talvez devessemos incluir todas as conjunções incidentes sobre um vértice (pra jogar o sink na ultima posição da ordenação topológica)
                EvaluateCompletionTime(proximoMovimento.DirectedEdge);
                //atualizar trilha de feromonio
                LocalPheromoneUpdate(proximoMovimento.DirectedEdge);

                //tira o target dos escalaveis
                PendingNodes.Remove(proximoMovimento.DirectedEdge.Target);
                ScheduledNodes.Add(proximoMovimento.DirectedEdge.Target);
                //olhar para os sucessores do target e decrementar o contador deles em uma unidade
                //para cada um deles que foi pra 0,  adiciona na lista de escalaveis
                proximoMovimento.DirectedEdge.Target.Successors.ForEach(node =>
                {
                    if (grauDeEntradaConjuntivo[node] <= 0) return;
                    grauDeEntradaConjuntivo[node] -= 1;
                    if (grauDeEntradaConjuntivo[node] == 0)
                        PendingNodes.Add(node);
                });
            }
        }

        private IFeasibleMove SelecionaProximoMovimento(HashSet<Node> escalaveis, HashSet<Node> escalados)
        {
            //olhar apenas para os arcos disjuntivos que tem uma ponta nos escalaveis e outra nos escalados
            //regra de pseudoprob
            
            var feasibleMoves = escalaveis.SelectMany(candidateNode =>
            {
                List<IFeasibleMove> moves = [];
                foreach (var disj in candidateNode.IncidentDisjunctions)
                {
                    if (escalados.Contains(disj.Other(candidateNode)))
                    {
                        var direction = disj.Target == candidateNode
                            ? Direction.SourceToTarget
                            : Direction.TargetToSource;
                        var move = new FeasibleMove(disj, direction);
                        moves.Add(move);
                    }
                }

                return moves;
            });
            //return feasibleMoves.First();
            return ChooseNextMove(feasibleMoves); // <---- GARGALO
        }

        public void WalkAround2()
        {

            Console.WriteLine($"#{Id}th ant is cooking...");
            RemainingNodes.AddRange(Context.DisjunctiveGraph.OperationVertices);

            Stopwatch timer = new();
            while (RemainingNodes.Count > 0)
            {
                IEnumerable<Node> allowedNodes = RemainingNodes.Where(v => !v.Predecessors.Any(RemainingNodes.Contains));

                var feasibleMoves = GetFeasibleMoves(allowedNodes);
                if (!feasibleMoves.Any()) continue;

                //TODO: descobrir como baixar o tempo do ChooseNextMove (atualmente está levando na ordem dos décimos de segundo)
                var selectedMove = ChooseNextMove(feasibleMoves);
                
                EvaluateCompletionTime(selectedMove.DirectedEdge);
                LocalPheromoneUpdate(selectedMove.DirectedEdge);

                RemainingNodes.Remove(selectedMove.DirectedEdge.Target);
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
                var orientation = disjunction.Orientations.First(c => c.Target == FinalNode);
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
            var jobCompletionTime = CompletionTimes[jobPredecessorNode.Operation];
            var machineCompletionTime = CompletionTimes[machinePredecessorNode.Operation];

            var processingTime = node.Operation.GetProcessingTime(machine);
            StartTimes[node.Operation] = Math.Max(machineCompletionTime, jobCompletionTime);
            CompletionTimes[node.Operation] = StartTimes[node.Operation] + processingTime;


            LoadingSequence[machine].Push(node);
            if (!MachineAssignment.TryAdd(node.Operation, machine))
                throw new Exception($"Machine already assigned to this operation");
        }

        private IFeasibleMove ChooseNextMove(IEnumerable<IFeasibleMove> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(IFeasibleMove Move, double Probability)>();
            IFeasibleMove? greedyMove = null;
            var greedyFactor = double.MinValue;
            // create roulette wheel and evaluate greedy move for pseudorandom proportional rule at same time (in O(n))
            foreach (var move in feasibleMoves)
            {
                var tauXy = move.GetPheromoneAmount(Context); // pheromone amount
                var etaXy = move.Weight.Inverse(); // heuristic information
                var tauXyAlpha = Math.Pow(tauXy, Context.Alpha); // pheromone amount raised to power alpha
                var etaXyBeta = Math.Pow(etaXy, Context.Beta); // heuristic information raised to power beta

                double probFactor = tauXyAlpha * etaXyBeta, pseudoProbFactor = tauXy * etaXyBeta;
                rouletteWheel.Add((move, probFactor));
                sum += probFactor;

                if (greedyFactor >= pseudoProbFactor) continue;
                greedyFactor = pseudoProbFactor;
                greedyMove = move;
            }

            // pseudo random proportional rule
            if (Random.Shared.NextDouble() <= Context.Q0)
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

        private IFeasibleMove ChooseNextMoveOld(IEnumerable<IFeasibleMove> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(IFeasibleMove Move, double Probability)>();
            IFeasibleMove? greedyMove = null;
            var greedyFactor = double.MinValue;
            // create roulette wheel and evaluate greedy move for pseudorandom proportional rule at same time (in O(n))
            foreach (var move in feasibleMoves)
            {
                var tauXy = move.GetPheromoneAmount(Context); // pheromone amount
                var etaXy = move.Weight.Inverse(); // heuristic information
                var tauXyAlpha = Math.Pow(tauXy, Context.Alpha); // pheromone amount raised to power alpha
                var etaXyBeta = Math.Pow(etaXy, Context.Beta); // heuristic information raised to power beta

                double probFactor = tauXyAlpha * etaXyBeta, pseudoProbFactor = tauXy * etaXyBeta;
                rouletteWheel.Add((move, probFactor));
                sum += probFactor;

                if (greedyFactor >= pseudoProbFactor) continue;
                greedyFactor = pseudoProbFactor;
                greedyMove = move;
            }

            // pseudo random proportional rule
            if (Random.Shared.NextDouble() <= Context.Q0)
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

        /// <summary>
        /// mark as feasible move 
        /// each disjunctive arc which connects a candidate operation to the last operation of the loading sequence 
        /// denoted by the same kind of arc(it makes the possibility 
        /// that the candidate operation becomes the new last 
        /// operation of that loading sequence);
        /// </summary>
        /// <param name="candidateNodes"></param>
        /// <returns></returns>
        private IEnumerable<IFeasibleMove> GetFeasibleMoves(IEnumerable<Node> candidateNodes)
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
