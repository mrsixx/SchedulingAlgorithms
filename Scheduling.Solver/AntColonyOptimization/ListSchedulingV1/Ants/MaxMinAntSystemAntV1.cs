using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Ants
{
    public class MaxMinAntSystemAntV1(int id, int generation, MaxMinAntSystemAlgorithmV1 context)
        : AntV1<MaxMinAntSystemAntV1, MaxMinAntSystemAlgorithmV1>(id, generation, context)
    {
        public override void WalkAround()
        {
            ListSchedulingV1Heuristic<MaxMinAntSystemAlgorithmV1, MaxMinAntSystemAntV1>.Construct(this);
        }
    }
}
