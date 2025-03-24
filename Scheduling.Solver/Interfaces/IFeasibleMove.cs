namespace Scheduling.Solver.Interfaces
{
    public interface IFeasibleMove<T> where T : class
    {
        public double Weight { get; }

        public double GetPheromoneAmount(IPheromoneTrail<T> trail);
    }
}
