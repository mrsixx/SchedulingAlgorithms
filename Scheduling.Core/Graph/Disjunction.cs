using QuikGraph;
using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Disjunction : BaseEdge, IUndirectedEdge<Node>
    {
        public Disjunction(Node u, Node v)
        {
            Node node1 = u, node2 = v;
            if (node1.Id < node2.Id)
                (node2, node1) = (node1, node2);
            Source = node1;
            Target = node2;
        }
        public Disjunction(Node u, Node v, Machine machine) : this(u, v)
        {
            
            Machine = machine;
            ProcessingTime = Tuple.Create(
                Source.IsDummyNode ? 0 : Source.Operation.ProcessingTime(machine),
                Target.IsDummyNode ? 0 : Target.Operation.ProcessingTime(machine)
            );
        }

        public override Node Source { get; }

        public override Node Target { get; }

        public override string Log => $"{Source.Id} --[{ProcessingTime.Item1};{ProcessingTime.Item2}]-- {Target.Id}";

        public Machine? Machine { get; }

        public Tuple<double, double> ProcessingTime { get; } = Tuple.Create(0.0, 0.0);

        public Conjunction[] EquivalentConjunctions => [new(Source, Target, ProcessingTime.Item1), new(Target, Source, ProcessingTime.Item2)];

    }
}
