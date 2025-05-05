using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Ants
{
    public class RankBasedAntSystemAntV3(int id, int generation, RankBasedAntSystemAlgorithmV3 context) : AntV3<RankBasedAntSystemAntV3, RankBasedAntSystemAlgorithmV3>(id, generation, context)
    {
        public override void WalkAround()
        {
            ListSchedulingV3Heuristic<RankBasedAntSystemAlgorithmV3, RankBasedAntSystemAntV3>.Construct(this);
        }
    }
}
