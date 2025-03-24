namespace Scheduling.Solver.AntColonyOptimization.Ants
{
    public class AsyncAnt : BaseAnt
    {

        public AsyncAnt(BaseAnt ant)
        {
            Ant = ant;
            Task = new Task(WalkAround);
            Task.Start();
        }

        public BaseAnt Ant { get; }

        public Task Task { get; }

        public override double Makespan => Ant.Makespan;

        public override void WalkAround() => Ant.WalkAround();
    }
}
