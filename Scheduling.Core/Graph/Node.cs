using Scheduling.Core.FJSP;
using System.Diagnostics.CodeAnalysis;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Node(Operation operation)
    {
        public int Id { get; set; } = operation.Id;

        public Operation Operation { get; set; } = operation;

        public bool IsSourceNode => Id == Operation.SOURCE_ID;

        public bool IsSinkNode => Id == Operation.SINK_ID;

        public bool IsDummyNode => IsSourceNode || IsSinkNode;

        public List<Node> Predecessors { get; } = [];

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            if (obj is not Node node) return false;

            return Id == node.Id;
        }
        public override int GetHashCode() => Id;

        public override string ToString()
        {
            if (IsSourceNode) return "S";
            if (IsSinkNode) return "F";
            return Id.ToString();
        }
    }
}
