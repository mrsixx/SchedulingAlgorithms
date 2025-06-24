using Scheduling.Core.FJSP;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.Algorithms
{
    public static class CriticalPathMethodAlgorithm
    {

        public static Operation[] EvaluateCriticalPath(Instance instance, IFjspSolution solution)
        {
            List<Operation> reversedCriticalPath = [];
            var criticalOperationId = solution.CriticalOperationId;
            var currentOperation = criticalOperationId.HasValue
                ? instance.OperationsSet.FirstOrDefault(op => op.Id == criticalOperationId.Value)
                : null;
            while (currentOperation is not null)
            {
                var predecessor = solution.CriticalPredecessors[currentOperation.Id];
                if (predecessor is not null)
                    reversedCriticalPath.Add(predecessor);
                currentOperation = predecessor;
            }

            reversedCriticalPath.Reverse();
            return reversedCriticalPath.ToArray();
        }
    }
}
