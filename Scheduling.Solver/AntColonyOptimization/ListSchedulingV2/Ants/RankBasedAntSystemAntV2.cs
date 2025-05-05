using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants
{
    public class RankBasedAntSystemAntV2(int id, int generation, RankBasedAntSystemAlgorithmV2 context) : AntV2<RankBasedAntSystemAntV2, RankBasedAntSystemAlgorithmV2>(id, generation, context)
    {
        public override void WalkAround()
        {
            ListSchedulingV2Heuristic<RankBasedAntSystemAlgorithmV2,RankBasedAntSystemAntV2>.Construct(this);
        }
    }
}
