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
