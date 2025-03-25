using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Ants
{
    /// <summary>
    /// Elitist ant is equals an normal ant
    /// </summary>
    /// <param name="id"></param>
    /// <param name="generation"></param>
    /// <param name="context"></param>
    public class RankBasedAntSystemAntV1(int id, int generation, RankBasedAntSystemAlgorithmV1 context) : AntV1<RankBasedAntSystemAntV1, RankBasedAntSystemAlgorithmV1>(id, generation, context)
    {
        public override void WalkAround()
        {
            ListSchedulingV1Heuristic<RankBasedAntSystemAlgorithmV1,RankBasedAntSystemAntV1>.Construct(this);
        }
    }
}
