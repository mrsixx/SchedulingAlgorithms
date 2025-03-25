namespace Scheduling.Solver.AntColonyOptimization
{
    public class AsyncAnt<TSelf> : BaseAnt<TSelf> where TSelf : BaseAnt<TSelf>
    {
        public AsyncAnt(BaseAnt<TSelf> ant)
        {
            Ant = ant;
            Task = new Task(WalkAround);
            Task.Start();
        }

        public BaseAnt<TSelf> Ant { get; }

        public Task Task { get; }

        public override double Makespan => Ant.Makespan;

        public override void WalkAround() => Ant.WalkAround();

        public override void Log()
        {
            Ant.Log();
        }
    }
}
