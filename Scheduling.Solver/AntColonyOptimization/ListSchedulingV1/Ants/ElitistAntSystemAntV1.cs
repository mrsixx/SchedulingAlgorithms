using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Ants
{
    /// <summary>
    /// Elitist ant is equals an normal ant
    /// </summary>
    /// <param name="id"></param>
    /// <param name="generation"></param>
    /// <param name="context"></param>
    public class ElitistAntSystemAntV1(int id, int generation, ElitistAntSystemAlgorithmV1 context) : AntV1<ElitistAntSystemAntV1, ElitistAntSystemAlgorithmV1>(id, generation, context)
    {
        public override void WalkAround()
        {
            ListSchedulingV1Heuristic<ElitistAntSystemAlgorithmV1, ElitistAntSystemAntV1>.Construct(this);
        }
    }
}
