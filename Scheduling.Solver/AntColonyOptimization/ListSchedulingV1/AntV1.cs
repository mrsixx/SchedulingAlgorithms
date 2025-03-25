using QuikGraph.Algorithms;
using Scheduling.Core.Extensions;
using Scheduling.Core.Graph;
using Scheduling.Solver.Interfaces;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1
{
    public abstract class AntV1<TSelf, TContext> : BaseAnt<TSelf> 
        where TSelf: AntV1<TSelf, TContext>
        where TContext : AntColonyV1AlgorithmSolver<TContext, TSelf>
    {
        protected AntV1(int id, int generation, TContext context)
        {
            Id = id;
            Generation = generation;
            Context = context;
        }


        public TContext Context { get; }

        public ConjunctiveGraphModel ConjunctiveGraph { get; } = new();

        public override double Makespan => CompletionTimes[FinalNode.Operation.Id];

        public DisjunctiveGraphModel DisjunctiveGraph => Context.DisjunctiveGraph;

        public Node StartNode => DisjunctiveGraph.Source;

        public Node FinalNode => DisjunctiveGraph.Sink;


        public void InitializeDataStructures()
        {
            // Initialize starting and completion times for each operation
            DisjunctiveGraph.Vertices.ToList().ForEach(node =>
            {
                CompletionTimes.Add(node.Operation.Id, 0);
                StartTimes.Add(node.Operation.Id, 0);
            });
            // Initialize loading sequences for each machine
            DisjunctiveGraph.Machines.ForEach(machine =>
                LoadingSequence.Add(machine, new Stack<Node>([DisjunctiveGraph.Source]))
            );
        }


        public void EvaluateCompletionTime(Orientation selectedMove)
        {
            var node = selectedMove.Target;
            var machine = selectedMove.Machine;

            var jobPredecessorNode = node.DirectPredecessor;
            var machinePredecessorNode = LoadingSequence[machine].Peek();

            //se a primeira operação do job, start time tem que ser maior ou igual release date, 
            var jobCompletionTime = jobPredecessorNode.Equals(StartNode)
                                                ? node.Operation.Job.ReleaseDate  // release date if it's first operation
                                                : CompletionTimes[jobPredecessorNode.Operation.Id]; // else it's predecessor completionTime
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

        public virtual IFeasibleMove<Orientation> ProbabilityRule(IEnumerable<IFeasibleMove<Orientation>> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(IFeasibleMove<Orientation> Move, double Probability)>();

            // create roulette wheel and evaluate greedy move for pseudorandom proportional rule at same time (in O(n))
            foreach (var move in feasibleMoves)
            {
                var tauXy = move.GetPheromoneAmount(Context.PheromoneTrail); // pheromone amount
                var etaXy = move.Weight.Inverse(); // heuristic information
                var tauXyAlpha = Math.Pow(tauXy, Context.Parameters.Alpha); // pheromone amount raised to power alpha
                var etaXyBeta = Math.Pow(etaXy, Context.Parameters.Beta); // heuristic information raised to power beta

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

        public IEnumerable<IFeasibleMove<Orientation>> GetFeasibleMoves(HashSet<Node> unscheduledNodes, HashSet<Node> scheduledNodes)
        {
            return unscheduledNodes.SelectMany(candidateNode =>
            {
                return candidateNode.IncidentDisjunctions
                    .Where(disjunction => scheduledNodes.Contains(disjunction.Other(candidateNode)))
                    .Select(disjunction => new FeasibleMove(disjunction,
                        disjunction.Target == candidateNode ? Direction.SourceToTarget : Direction.TargetToSource));
            });
        }

        public void SinksToSink()
        {
            var sinks = ConjunctiveGraph.Sinks().ToList();
            foreach (var sink in sinks)
            {
                if (sink.Equals(FinalNode)) continue;
                var machine = MachineAssignment[sink.Operation.Id];

                Disjunction disjunction = sink.IncidentDisjunctions.First(
                    d => d.Machine.Equals(machine) && d.Other(sink).Equals(FinalNode)
                );

                var orientation = disjunction.Orientations.First(c => c.Target == FinalNode);
                ConjunctiveGraph.AddConjunctionAndVertices(orientation);
                CompletionTimes[FinalNode.Operation.Id] = Math.Max(CompletionTimes[FinalNode.Operation.Id], CompletionTimes[sink.Operation.Id]);
            }
        }

        public virtual void LocalPheromoneUpdate(Orientation selectedMove) { }

        public override void Log()
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
