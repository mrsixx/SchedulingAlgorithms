using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Ants
{
    public class AntSystemAntV1(int id, int generation, AntSystemAlgorithmV1 context) : AntV1(id, generation)
    {
        public override AntColonyAlgorithmSolverBase Context => context;

        public override void WalkAround()
        {
            ListSchedulingV1Heuristic.Construct(this);
        }
    }
}
