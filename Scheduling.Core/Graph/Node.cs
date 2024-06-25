namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Node
    {

        public const int SOURCE_ID = 0;
        public const int SINK_ID = -1;

        public Node(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        public bool IsSourceNode => Id == SOURCE_ID;

        public bool IsSinkNode => Id == SINK_ID;
    }
}
