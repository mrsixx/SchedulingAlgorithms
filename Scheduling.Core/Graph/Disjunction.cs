using QuikGraph;
using Scheduling.Core.FJSP;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Disjunction : IUndirectedEdge<Node>
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

        public Node Source { get; }

        public Node Target { get; }

        public Machine Machine { get; }

        public Tuple<double, double> ProcessingTime { get; } = Tuple.Create(0.0, 0.0);

    }
}
