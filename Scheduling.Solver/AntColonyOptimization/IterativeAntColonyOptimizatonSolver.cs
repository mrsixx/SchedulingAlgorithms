using Scheduling.Core.Graph;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class IterativeAntColonyOptimizatonSolver : AntColonyOptimizationAlgorithmSolver
    {
        public IterativeAntColonyOptimizatonSolver(DisjunctiveGraphModel graph, double alpha = 0.9, double beta = 1.2, double rho = 0.01, double phi = 0.04, double tau0 = 0.001, int ants = 300, int iterations = 100, int stagnantGenerationsAllowed = 20) : base(graph, alpha, beta, rho, phi, tau0, ants, iterations, stagnantGenerationsAllowed)
        {
        }


        public override Ant[] BugsLife(int currentIteration)
        {

            Ant[] ants = new Ant[AntCount];
            Log($"#{currentIteration}th wave ants start to walk...");
            for (int j = 0; j < AntCount; j++)
            {
                ants[j] = new Ant(id: j + 1, currentIteration, context: this);
                ants[j].WalkAround();
            }
            return ants;
        }
    }
}
