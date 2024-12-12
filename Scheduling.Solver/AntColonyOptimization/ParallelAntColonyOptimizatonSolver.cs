using Scheduling.Core.Graph;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class ParallelAntColonyOptimizatonSolver : AntColonyOptimizationAlgorithmSolver
    {
        public ParallelAntColonyOptimizatonSolver(DisjunctiveGraphModel graph, double alpha = 0.9, double beta = 1.2, double rho = 0.01, double phi = 0.04, double tau0 = 0.001, int ants = 300, int iterations = 100, int stagnantGenerationsAllowed = 20) : base(graph, alpha, beta, rho, phi, tau0, ants, iterations, stagnantGenerationsAllowed)
        {
            PheromoneTrail = new ThreadSafePheromoneTrail();
        }

        public override IPheromoneTrail<Orientation, double> PheromoneTrail { get; }


        public override Ant[] BugsLife(int currentIteration)
        {
            Log($"#{currentIteration}th wave ants start to walk...");
            AsyncAnt[] ants = GenerateAntsWave(generation: currentIteration);
            WaitForAntsToStop(ants);
            return ants;
        }


        private static void WaitForAntsToStop(AsyncAnt[] ants) => Task.WaitAll(ants.Select(a => a.Task).ToArray());

        private AsyncAnt[] GenerateAntsWave(int generation)
        {
            AsyncAnt[] ants = new AsyncAnt[AntCount];
            for (int i = 0; i < AntCount; i++)
                ants[i] = new AsyncAnt(id: i + 1, generation, context: this);
            return ants;
        }
    }
}
