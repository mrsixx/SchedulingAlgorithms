using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants
{
    public class AntSystemAntV2(int id, int generation, AntSystemAlgorithmV2 context) : AntV2<AntSystemAntV2, AntSystemAlgorithmV2>(id, generation, context)
    {
        public override void WalkAround()
        {
            ListSchedulingV2Heuristic<AntSystemAlgorithmV2, AntSystemAntV2>.Construct(this);
        }
    }
}
