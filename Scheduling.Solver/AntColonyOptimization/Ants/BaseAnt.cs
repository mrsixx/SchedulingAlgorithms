using QuikGraph.Algorithms;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using static Scheduling.Core.Enums.DirectionEnum;
using Scheduling.Solver.AntColonyOptimization.Solvers;

namespace Scheduling.Solver.AntColonyOptimization.Ants
{
    public abstract class BaseAnt(int id, int generation, AntColonyOptimizationAlgorithmSolver context)
    {
        public int Id { get; init; } = id;

        public int Generation { get; init; } = generation;

        public ConjunctiveGraphModel ConjunctiveGraph { get; } = new();

        public Dictionary<Machine, Stack<Node>> LoadingSequence { get; } = [];

        public Dictionary<int, Machine> MachineAssignment { get; } = [];

        public Dictionary<int, double> CompletionTimes { get; } = [];

        public Dictionary<int, double> StartTimes { get; } = [];

        public double Makespan => CompletionTimes[FinalNode.Operation.Id];

        public Node StartNode => Context.DisjunctiveGraph.Source;

        public Node FinalNode => Context.DisjunctiveGraph.Sink;

        public AntColonyOptimizationAlgorithmSolver Context { get; } = context;


        protected void InitializeDataStructures()
        {
            // Initialize starting and completion times for each operation
            Context.DisjunctiveGraph.Vertices.ToList().ForEach(node =>
            {
                CompletionTimes.Add(node.Operation.Id, 0);
                StartTimes.Add(node.Operation.Id, 0);
            });
            // Initialize loading sequences for each machine
            Context.DisjunctiveGraph.Machines.ForEach(machine =>
                LoadingSequence.Add(machine, new Stack<Node>([Context.DisjunctiveGraph.Source]))
            );
        }


        protected void EvaluateCompletionTime(Orientation selectedMove)
        {
            var node = selectedMove.Target;
            var machine = selectedMove.Machine;

            var jobPredecessorNode = node.DirectPredecessor;
            var machinePredecessorNode = LoadingSequence[machine].Peek();


            var jobCompletionTime = CompletionTimes[jobPredecessorNode.Operation.Id];
            var machineCompletionTime = CompletionTimes[machinePredecessorNode.Operation.Id];
            var processingTime = node.Operation.GetProcessingTime(machine);

            // update loading sequence, starting and completion times
            StartTimes[node.Operation.Id] = Math.Max(machineCompletionTime, jobCompletionTime);
            CompletionTimes[node.Operation.Id] = StartTimes[node.Operation.Id] + processingTime;
            LoadingSequence[machine].Push(node);
            if (!MachineAssignment.TryAdd(node.Operation.Id, machine))
                throw new Exception($"Machine already assigned to this operation");

            ConjunctiveGraph.AddConjunctionAndVertices(selectedMove);
            ConjunctiveGraph.AddConjunctionAndVertices(new Conjunction(jobPredecessorNode, node));
        }

        protected IFeasibleMove ProbabilityRule(IEnumerable<IFeasibleMove> feasibleMoves)
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

        protected IEnumerable<IFeasibleMove> GetFeasibleMoves(HashSet<Node> unscheduledNodes, HashSet<Node> scheduledNodes) {
            return unscheduledNodes.SelectMany(candidateNode =>
            {
                return candidateNode.IncidentDisjunctions
                    .Where(disjunction => scheduledNodes.Contains(disjunction.Other(candidateNode)))
                    .Select(disjunction => new FeasibleMove(disjunction,
                        disjunction.Target == candidateNode ? Direction.SourceToTarget : Direction.TargetToSource));
            });
        }
        
        protected void SinksToSink()
        {
            var sinks = ConjunctiveGraph.Sinks().ToList();
            foreach (var sink in sinks)
            {
                if(sink.Equals(FinalNode)) continue;
                var machine = MachineAssignment[sink.Operation.Id];

                Disjunction disjunction = sink.IncidentDisjunctions.First(
                    d => d.Machine.Equals(machine) && d.Other(sink).Equals(FinalNode)
                );                

                var orientation = disjunction.Orientations.First(c => c.Target == FinalNode);
                ConjunctiveGraph.AddConjunctionAndVertices(orientation);
                CompletionTimes[FinalNode.Operation.Id] = Math.Max(CompletionTimes[FinalNode.Operation.Id], CompletionTimes[sink.Operation.Id]);
            }
        }
        
        public abstract void WalkAround();

        public void Log()
        {
            // print loading sequence
            foreach (var machine in LoadingSequence.Keys)
            {
                Console.Write($"{machine.Id}: ");
                foreach (var node in LoadingSequence[machine].Reverse())
                    Console.Write($" {node}[{StartTimes[node.Operation.Id]}-{CompletionTimes[node.Operation.Id]}] ");

                Console.WriteLine("");
            }

            // printing topological sort (acyclic evidence)
            Console.WriteLine("Topological sort: ");
            foreach (var node in ConjunctiveGraph.TopologicalSort())
                Console.Write($" {node} ");
            Console.WriteLine("");
        }
    }
}
