using Scheduling.Solver.DataStructures;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Model
{
    public class ConjunctiveArc(AbstractVertex tail, AbstractVertex head) : Arc(tail, head)
    {
        public override string ToString() => $"{Tail}->{Head}";
    }
}
