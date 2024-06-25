using Scheduling.Core.Graph;

namespace Scheduling.Core.Interfaces
{
    internal interface IDisjunctiveGraph
    {
        Node Source { get; }

        Node Sink { get; }

        bool HasNode(int id);

        bool TryGetNode(int id, out Node node);

        bool HasConjunction(int sourceId, int targetId);

        bool TryGetConjunction(int sourceId, int targetId, out Conjunction conjunction);

        bool HasDisjunction(int sourceId, int targetId);

        bool TryGetDisjunction(int sourceId, int targetId, out Disjunction disjunction);
    }
}
