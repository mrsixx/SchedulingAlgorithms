using QuikGraph;
using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Disjunction : BaseEdge, IUndirectedEdge<Node>
    {
        public Disjunction(Node u, Node v, Machine machine)
        {
            Node node1 = u, node2 = v;
            if (node1.Id < node2.Id)
                (node2, node1) = (node1, node2);
            Source = node1;
            Target = node2;
            Machine = machine;
            ProcessingTime = Tuple.Create(
                node1.IsDummyOperation ? 0 : node1.Operation.ProcessingTime(machine),
                node2.IsDummyOperation ? 0 : node2.Operation.ProcessingTime(machine)
            );
        }

        public override Node Source { get; }

        public override Node Target { get; }

        public override string Log => $"{Source.Id} --[{ProcessingTime.Item1};{ProcessingTime.Item2}]-- {Target.Id}";

        public Machine Machine { get; }

        public Tuple<double, double> ProcessingTime { get; } = Tuple.Create(0.0, 0.0);

        public Conjunction[] EquivalentConjunctions => [new(Source, Target, ProcessingTime.Item1), new(Target, Source, ProcessingTime.Item2)];

    }
}
