using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Ants
{
    public class AntSystemAntV1(int id, int generation, AntSystemAlgorithmV1 context) : AntV1<AntSystemAntV1, AntSystemAlgorithmV1>(id, generation, context)
    {

        public override void WalkAround()
        {
            ListSchedulingV1Heuristic<AntSystemAlgorithmV1, AntSystemAntV1>.Construct(this);
        }
    }
}
