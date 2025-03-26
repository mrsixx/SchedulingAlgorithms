using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Models;
using static Scheduling.Core.Enums.DirectionEnum;

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

        public HashSet<Allocation> Path { get; } = [];

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

        public virtual void LocalPheromoneUpdate(Allocation selectedMove) { }

        public IEnumerable<IFeasibleMove<Allocation>> GetFeasibleMoves(HashSet<Operation> unscheduledNodes)
        {
            return unscheduledNodes.SelectMany(candidateNode =>
                candidateNode.EligibleMachines.Select(m => new FeasibleMoveV2(candidateNode, m)));
        }
    }
}
