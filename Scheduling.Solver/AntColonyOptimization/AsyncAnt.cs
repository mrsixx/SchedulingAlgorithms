namespace Scheduling.Solver.AntColonyOptimization
{
    public class AsyncAnt : Ant
    {
        public AsyncAnt(int id, int generation, AntColonyOptimizationAlgorithmSolver context) : base(id, generation, context)
        {
            Task = new Task(() => WalkAround());
            Task.Start();
        }
        public Task Task { get; }
    }
}
