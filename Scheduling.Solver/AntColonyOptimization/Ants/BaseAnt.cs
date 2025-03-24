using QuikGraph.Algorithms;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using static Scheduling.Core.Enums.DirectionEnum;
using Scheduling.Solver.AntColonyOptimization.Solvers;

namespace Scheduling.Solver.AntColonyOptimization.Ants
{
    public abstract class BaseAnt
    {
        public int Id { get; init; }
        
        public int Generation { get; init; }

        public Dictionary<Machine, Stack<Node>> LoadingSequence { get; } = [];

        public Dictionary<int, Machine> MachineAssignment { get; } = [];

        public Dictionary<int, double> CompletionTimes { get; } = [];

        public Dictionary<int, double> StartTimes { get; } = [];

        public abstract double Makespan { get; }
        public abstract void WalkAround();
    }
}
