namespace Scheduling.Solver.AntColonyOptimization.Ants
{
    public class AsyncAnt : BaseAnt
    {

        public AsyncAnt(BaseAnt ant) : base(ant.Id, generation: ant.Generation, ant.Context)
        {
            Ant = ant;
            Task = new Task(WalkAround);
            Task.Start();
        }

        public BaseAnt Ant { get; }

        public Task Task { get; }

        public override void WalkAround() => Ant.WalkAround();
    }
}
