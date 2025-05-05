using Scheduling.Core.FJSP;
using Scheduling.Solver.DataStructures;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Model
{
    public class OperationVertex(Operation operation) : AbstractVertex(id: operation.Id, label: $"O{operation.Id}")
    {
        public Operation Operation { get; } = operation;
    }
}
