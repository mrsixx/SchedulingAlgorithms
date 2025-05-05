using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Ants
{
    public class MaxMinAntSystemAntV3(int id, int generation, MaxMinAntSystemAlgorithmV3 context)
        : AntV3<MaxMinAntSystemAntV3, MaxMinAntSystemAlgorithmV3>(id, generation, context)
    {
        public override void WalkAround()
        {
            ListSchedulingV3Heuristic<MaxMinAntSystemAlgorithmV3, MaxMinAntSystemAntV3>.Construct(this);
        }
    }
}
