using Scheduling.Core.FJSP;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Node
    {
        public Node(Operation operation)
        {
            Id = operation.Id;
            Operation = operation;
        }

        public int Id { get; set; }

        public Operation Operation { get; set; }

        public bool IsSourceNode => Id == Operation.SOURCE_ID;

        public bool IsSinkNode => Id == Operation.SINK_ID;

        public bool IsDummyNode => IsSourceNode || IsSinkNode;

        public List<Node> Predecessors { get; } = [];

        public override string ToString() => Id.ToString();
    }
}
