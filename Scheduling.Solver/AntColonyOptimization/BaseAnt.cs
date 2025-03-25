using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;

namespace Scheduling.Solver.AntColonyOptimization
{
    public abstract class BaseAnt<TSelf> where TSelf : BaseAnt<TSelf>
    {
        public int Id { get; init; }

        public int Generation { get; init; }

        public Dictionary<Machine, Stack<Node>> LoadingSequence { get; } = [];

        public Dictionary<int, Machine> MachineAssignment { get; } = [];

        public Dictionary<int, double> CompletionTimes { get; } = [];

        public Dictionary<int, double> StartTimes { get; } = [];

        public abstract double Makespan { get; }

        public abstract void WalkAround();

        public abstract void Log();
    }
}
