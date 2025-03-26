using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants
{
    public class MaxMinAntSystemAntV2(int id, int generation, MaxMinAntSystemAlgorithmV2 context)
        : AntV2<MaxMinAntSystemAntV2, MaxMinAntSystemAlgorithmV2>(id, generation, context)
    {
        public override void WalkAround()
        {
            ListSchedulingV2Heuristic<MaxMinAntSystemAlgorithmV2, MaxMinAntSystemAntV2>.Construct(this);
        }
    }
}
