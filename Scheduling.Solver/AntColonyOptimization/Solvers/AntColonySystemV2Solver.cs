using Scheduling.Solver.AntColonyOptimization.Ants;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.Solvers
{
    public class AntColonySystemV2Solver(
        double alpha,
        double beta,
        double rho,
        double phi,
        double tau0,
        int ants,
        int iterations,
        int stagnantGenerationsAllowed,
        ISolveApproach solveApproach)
        : AntColonySystemAlgorithmSolver(alpha, beta, rho, phi, tau0, ants, iterations,
            stagnantGenerationsAllowed, solveApproach)
    {
        public override BaseAnt[] BugsLife(int currentIteration)
        {
            return SolveApproach.Solve<BaseAnt>(currentIteration, this, BugSpawner);
        }

        private ListSchedulingAcsAntV2 BugSpawner(int id, int currentIteration) => new(id, currentIteration, this);
    }
}
