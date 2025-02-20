using QuikGraph.Algorithms;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.Solvers;

namespace Scheduling.Solver.AntColonyOptimization.Ants
{
    public abstract class BaseAnt(int id, int generation, AntColonyOptimizationAlgorithmSolver context)
    {
        public int Id { get; init; } = id;

        public int Generation { get; init; } = generation;

        public ConjunctiveGraphModel ConjunctiveGraph { get; } = new();

        public Dictionary<Machine, Stack<Node>> LoadingSequence { get; } = [];

        public Dictionary<Operation, Machine> MachineAssignment { get; } = [];

        public Dictionary<Operation, double> CompletionTimes { get; } = [];

        public Dictionary<Operation, double> StartTimes { get; } = [];

        public double Makespan => CompletionTimes[FinalNode.Operation];

        public Node StartNode => Context.DisjunctiveGraph.Source;

        public Node FinalNode => Context.DisjunctiveGraph.Sink;

        public AntColonyOptimizationAlgorithmSolver Context { get; } = context;


        protected void InitializeDataStructures()
        {
            // Initialize starting and completion times for each operation
            Context.DisjunctiveGraph.Vertices.ToList().ForEach(node =>
            {
                CompletionTimes.Add(node.Operation, 0);
                StartTimes.Add(node.Operation, 0);
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

        
        protected void SinksToSink()
        {
            var sinks = ConjunctiveGraph.Sinks().ToList();
            foreach (var sink in sinks)
            {
                if(sink.Equals(FinalNode)) continue;
                var machine = MachineAssignment[sink.Operation];

                Disjunction disjunction = sink.IncidentDisjunctions.First(
                    d => d.Machine.Equals(machine) && d.Other(sink).Equals(FinalNode)
                );                

                var orientation = disjunction.Orientations.First(c => c.Target == FinalNode);
                ConjunctiveGraph.AddConjunctionAndVertices(orientation);
                CompletionTimes[FinalNode.Operation] = Math.Max(CompletionTimes[FinalNode.Operation], CompletionTimes[sink.Operation]);
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
                    Console.Write($" {node}[{StartTimes[node.Operation]}-{CompletionTimes[node.Operation]}] ");

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
