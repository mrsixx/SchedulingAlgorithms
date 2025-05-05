using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Ants
{
    public class ElitistAntSystemAntV3(int id, int generation, ElitistAntSystemAlgorithmV3 context) : AntV3<ElitistAntSystemAntV3, ElitistAntSystemAlgorithmV3>(id, generation, context)
    {
        public override void WalkAround()
        {
            ListSchedulingV3Heuristic<ElitistAntSystemAlgorithmV3, ElitistAntSystemAntV3>.Construct(this);
        }
    }
}
