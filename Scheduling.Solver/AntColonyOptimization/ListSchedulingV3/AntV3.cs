using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Model;
using Scheduling.Solver.DataStructures;
using Scheduling.Solver.Interfaces;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3
{
    public abstract class AntV3<TSelf, TContext> : BaseAnt<TSelf>
        where TSelf : AntV3<TSelf, TContext>
        where TContext : AntColonyV3AlgorithmSolver<TContext, TSelf>
    {
        protected AntV3(int id, int generation, TContext context)
        {
            Id = id;
            Generation = generation;
            Context = context;
        }

        public TContext Context { get; }

        public ConjunctiveDigraph ConjunctiveDigraph { get; protected set; }

        public HashSet<DisjunctiveArc> Selection { get; } = [];

        public virtual Dictionary<Machine, HashSet<AbstractVertex>> LoadingSequence { get; } = [];

        public override double Makespan => CompletionTimes.Values.Max();

        public DisjunctiveDigraph DisjunctiveGraph => Context.DisjunctiveGraph;

        public DummyOperationVertex StartNode => DisjunctiveGraph.Source;

        public DummyOperationVertex FinalNode => DisjunctiveGraph.Sink;

        public void InitializeDataStructures()
        {
            // Initialize starting and completion times for each operation
            DisjunctiveGraph.VertexSet.ToList().ForEach(vertex =>
            {
                CompletionTimes.Add(vertex.Id, 0);
                StartTimes.Add(vertex.Id, 0);
            });

            // Initialize loading sequences for each machine
            DisjunctiveGraph.Instance.Machines.ToList().ForEach(machine =>
                LoadingSequence.Add(machine, [DisjunctiveGraph.Source])
            );
        }

        public void EvaluateCompletionTime(DisjunctiveArc selectedMove)
        {
            var (tail, machine, head) = selectedMove;

            var machinePredecessor = LoadingSequence[machine].Last();
            var criticalPredecessor = 
                DisjunctiveGraph
                    .IncomingArcs<ConjunctiveArc>(head)
                    .MaxBy(VertexCompletionTime);

            var machineCompletionTime = CompletionTimes[machinePredecessor.Id];
            var criticalPredecessorCompletionTime = criticalPredecessor is not null ? VertexCompletionTime(criticalPredecessor) : 0;
            var processingTime = (head is OperationVertex o) ? o.Operation.GetProcessingTime(machine) : 0;

            // update loading sequence, starting and completion times
            StartTimes[head.Id] = Math.Max(machineCompletionTime, criticalPredecessorCompletionTime);
            CompletionTimes[head.Id] = StartTimes[head.Id] + processingTime;
            LoadingSequence[machine].Add(head);

            if (!MachineAssignment.TryAdd(head.Id, machine))
                throw new Exception($"Machine already assigned to this operation");

            Selection.Add(selectedMove);
        }

        public virtual IFeasibleMove<DisjunctiveArc> ProbabilityRule(IEnumerable<IFeasibleMove<DisjunctiveArc>> feasibleMoves)
        {
            var sum = 0.0;
            var rouletteWheel = new List<(IFeasibleMove<DisjunctiveArc> Move, double Probability)>();

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

        public IEnumerable<IFeasibleMove<DisjunctiveArc>> GetFeasibleMoves(HashSet<AbstractVertex> unscheduledNodes, HashSet<AbstractVertex> scheduledNodes)
        {
            return unscheduledNodes.SelectMany(candidateNode =>
            {
                return DisjunctiveGraph.IncomingArcs<DisjunctiveArc>(candidateNode)
                    .Where(arc => scheduledNodes.Contains(arc.Tail))
                    .Select(arc => new Orientation(arc));
            });
        }

        public void ExtractConjunctiveDigraph()
        {
            ConjunctiveDigraph = Context.GraphBuilderService.BuildConjunctiveDigraph(DisjunctiveGraph, Selection);
        }

        //public void SinksToSink()
        //{
        //    var sinks = ConjunctiveGraph.Sinks().ToList();
        //    foreach (var sink in sinks)
        //    {
        //        if (sink.Equals(FinalNode)) continue;
        //        var machine = MachineAssignment[sink.Operation.Id];

        //        Disjunction disjunction = sink.IncidentDisjunctions.First(
        //            d => d.Machine.Equals(machine) && d.Other(sink).Equals(FinalNode)
        //        );

        //        var orientation = disjunction.Orientations.First(c => c.Target == FinalNode);
        //        ConjunctiveGraph.AddConjunctionAndVertices(orientation);
        //        CompletionTimes[FinalNode.Operation.Id] = Math.Max(CompletionTimes[FinalNode.Operation.Id], CompletionTimes[sink.Operation.Id]);
        //    }
        //}

        public virtual void LocalPheromoneUpdate(DisjunctiveArc selectedMove) { }

        public override void Log()
        {
            // print loading sequence
            ////foreach (var machine in LoadingSequence.Keys)
            ////{
            ////    Console.Write($"{machine.Id}: ");
            ////    foreach (var node in LoadingSequence[machine].Reverse())
            ////        Console.Write($" {node}[{StartTimes[node.Operation.Id]}-{CompletionTimes[node.Operation.Id]}] ");

            ////    Console.WriteLine("");
            ////}

            //// printing topological sort (acyclic evidence)
            Console.WriteLine("Topological sort: ");
            foreach (var node in ConjunctiveDigraph.TopologicalSort())
                Console.Write($" {node} ");
            Console.WriteLine("");
            Console.WriteLine($"Makespan: {Makespan}");
        }

        private double VertexCompletionTime(ConjunctiveArc conjunction)
        {
            var (tail, head) = conjunction;

            if (tail is DummyOperationVertex src && src.Equals(StartNode))
            {
                var o = head as OperationVertex;
                return o.Operation.Job.ReleaseDate;
            }

            return CompletionTimes[tail.Id];
        }
    }
}
