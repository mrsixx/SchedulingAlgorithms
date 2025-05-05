using Scheduling.Core.FJSP;
using Scheduling.Solver.DataStructures;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Model
{
    public class DisjunctiveDigraph : Digraph
    {
        public required DummyOperationVertex Source { get; init; }

        public required DummyOperationVertex Sink { get; init; }

        public required Instance Instance { get; init; }
    }
}
