using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Algorithms;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1.Ants
{
    public class MaxMinAntSystemAntV1(int id, int generation, MaxMinAntSystemAlgorithmV1 context)
        : AntV1(id, generation)
    {
        public override AntColonyAlgorithmSolverBase Context => context;

        public override void WalkAround()
        {
            ListSchedulingV1Heuristic.Construct(this);
        }
    }
}
