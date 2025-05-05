using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Ants
{
    public class AntSystemAntV3(int id, int generation, AntSystemAlgorithmV3 context) : AntV3<AntSystemAntV3, AntSystemAlgorithmV3>(id, generation, context)
    {

        public override void WalkAround()
        {
            ListSchedulingV3Heuristic<AntSystemAlgorithmV3, AntSystemAntV3>.Construct(this);
        }
    }
}
