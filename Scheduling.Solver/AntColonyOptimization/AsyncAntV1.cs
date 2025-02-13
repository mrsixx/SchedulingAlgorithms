namespace Scheduling.Solver.AntColonyOptimization
{
    public class AsyncAntV1 : Ant
    {
        public AsyncAntV1(int id, int generation, AntColonyOptimizationAlgorithmSolver context) : base(id, generation, context)
        {
            Task = new Task(() => WalkAround());
            Task.Start();
        }
        public Task Task { get; }
    }
}
