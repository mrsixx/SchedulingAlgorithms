using Scheduling.Core.FJSP;
using Scheduling.Solver.DataStructures;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3
{
    public class PrecedenceDigraph : Digraph<OperationVertex>
    {
        public PrecedenceDigraph(Instance instance)
        {
            Instance = instance;
            foreach (var job in instance.Jobs)
            {
                var previous = job.Operations[0];
                var operationsLength = job.Operations.Length;
                for (var i = 1; i < operationsLength; i++)
                {
                    var current = job.Operations[i];
                    AddArc(
                        new PrecedenceArc(new(previous), new(current))
                    );
                    previous = current;
                }
            }
        }

        public Instance Instance { get; }
    }
    public class OperationVertex(Operation operation) : AbstractVertex(id: operation.Id, label: $"O{operation.Id}")
    {
        public Operation Operation { get; } = operation;
    }
    public class PrecedenceArc(OperationVertex tail, OperationVertex head) : Arc<OperationVertex>(tail, head)  { }

}
