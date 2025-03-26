using Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV2.Ants
{
    /// <summary>
    /// Elitist ant is equals an normal ant
    /// </summary>
    /// <param name="id"></param>
    /// <param name="generation"></param>
    /// <param name="context"></param>
    public class ElitistAntSystemAntV2(int id, int generation, ElitistAntSystemAlgorithmV2 context) : AntV2<ElitistAntSystemAntV2, ElitistAntSystemAlgorithmV2>(id, generation, context)
    {
        public override void WalkAround()
        {
            ListSchedulingV2Heuristic<ElitistAntSystemAlgorithmV2, ElitistAntSystemAntV2>.Construct(this);
        }
    }
}
