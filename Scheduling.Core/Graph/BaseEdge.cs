using QuikGraph;

namespace Scheduling.Core.Graph
{
    public class BaseEdge : IEdge<Node>
    {
        public virtual Node Source { get; }

        public virtual Node Target { get; }

        public virtual string Log { get; } = string.Empty;
        
        public override string ToString() => Log;

    }
}
