namespace Scheduling.Core.Graph
{
    public class Pheromone(double sourceToTarget, double targetToSource)
    {
        public double SourceToTarget { get; set; } = sourceToTarget;

        public double TargetToSource { get; set; } = targetToSource;
    }
}
