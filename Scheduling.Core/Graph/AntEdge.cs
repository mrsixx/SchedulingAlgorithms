namespace Scheduling.Core.Graph
{
    public abstract class AntEdge : BaseEdge
    {
        public abstract void EvaporatePheromone(double rate);
    }
}
