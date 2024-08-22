using QuikGraph;

namespace Scheduling.Core.Graph
{
    public abstract class BaseEdge : IEdge<Node>
    {
        public abstract Node Source { get; }

        public abstract Node Target { get; }

        public virtual string Log { get; } = string.Empty;
        
        public override string ToString() => Log;

    }
}
