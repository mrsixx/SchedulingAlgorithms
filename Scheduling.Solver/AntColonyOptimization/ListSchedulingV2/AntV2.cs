using Scheduling.Core.FJSP;
using Scheduling.Solver.Models;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2
{
    public abstract class AntV2<TSelf, TContext> : BaseAnt<TSelf>
        where TSelf : AntV2<TSelf, TContext>
        where TContext : AntColonyV2AlgorithmSolver<TContext, TSelf>
    {
        protected AntV2(int id, int generation, TContext context)
        {
            Id = id;
            Generation = generation;
            Context = context;
        }
        
        public TContext Context { get; }

        public Instance Instance => Context.Instance;

        public GreedySolution Solution { get; } = new();

        public HashSet<Allocation> Path { get; } = [];

        public override double Makespan => Solution.Makespan;

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
        }
    }
}
