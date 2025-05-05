using Scheduling.Core.FJSP;
using Scheduling.Solver.DataStructures;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Model
{
    public class DisjunctiveArc(AbstractVertex tail, AbstractVertex head, Machine machine) : Arc(tail, head)
    {
        public Machine Machine { get; } = machine;

        public DisjunctiveArc AntiParallelDisjunctiveArc { get; private set; }

        public DisjunctiveArc WithWeight(double weight)
        {
            Weight = weight;
            return this;
        }


        public static DisjunctiveArc[] Disjunction(DummyOperationVertex v1, OperationVertex v2, Machine machine)
        {
            //TODO: maybe remove d2 vertex (src will be always a source vertex)
            var d1 = new DisjunctiveArc(v1, v2, machine).WithWeight(v2.Operation.Job.ReleaseDate);
            var d2 = new DisjunctiveArc(v2, v1, machine).WithWeight(Double.MaxValue);
            d1.AntiParallelDisjunctiveArc = d2;
            d2.AntiParallelDisjunctiveArc = d1;
            return [d1, d2];
        }

        public static DisjunctiveArc[] Disjunction(OperationVertex v1, OperationVertex v2, Machine machine)
        {
            var d1 = new DisjunctiveArc(v1, v2, machine).WithWeight(v1.Operation.GetProcessingTime(machine));
            var d2 = new DisjunctiveArc(v2, v1, machine).WithWeight(v2.Operation.GetProcessingTime(machine));
            d1.AntiParallelDisjunctiveArc = d2;
            d2.AntiParallelDisjunctiveArc = d1;
            return [d1, d2];
        }


        public void Deconstruct(out AbstractVertex tail, out Machine machine, out AbstractVertex head)
        {
            tail = Tail;
            head = Head;
            machine = Machine;
        }

        public override string ToString() => $"{Tail}-{Machine}-{Head}";
    }
}
