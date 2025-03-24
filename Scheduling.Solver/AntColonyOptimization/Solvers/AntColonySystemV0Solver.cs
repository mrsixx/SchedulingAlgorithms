using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.Ants;
using Scheduling.Solver.AntColonyOptimization.Pheromone;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization.Solvers
{
    public class AntColonySystemV0Solver(
        double alpha,
        double beta,
        double rho,
        double phi,
        double tau0,
        int ants,
        int iterations,
        int stagnantGenerationsAllowed,
        ISolveApproach<Orientation> solveApproach)
        : AntColonySystemAlgorithmSolver(alpha, beta, rho, phi, tau0, ants, iterations,
            stagnantGenerationsAllowed, solveApproach)
    {
        public override BaseAnt[] BugsLife(int currentIteration)
        {
            return SolveApproach.Solve<BaseAnt>(currentIteration, this, BugSpawner);
        }

        private AntColonySystemAntV0 BugSpawner(int id, int currentIteration) => new(id, currentIteration, this);

    }
}
