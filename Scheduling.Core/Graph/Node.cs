namespace Scheduling.Core.Graph
{
    [Serializable]
    public class Node
    {
        public Node(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}
