using System.Collections.Immutable;
using Scheduling.Core.Extensions;

namespace Scheduling.Solver.DataStructures
{
    public abstract class Digraph // (V,A)
    {
        #region Internal structures
        private HashSet<AbstractVertex> InternalVertexSet { get; } = []; // V

        private HashSet<Arc> InternalArcSet { get; } = []; // A

        private Dictionary<AbstractVertex, HashSet<Arc>> InternalIncomingArcs { get; } = []; // \delta^-(v)

        private Dictionary<AbstractVertex, HashSet<Arc>> InternalOutgoingArcs { get; } = [];// \delta^+(v)

        #endregion

        public IReadOnlyCollection<AbstractVertex> VertexSet => InternalVertexSet;

        public IReadOnlyCollection<Arc> ArcSet => InternalArcSet;

        public IReadOnlyCollection<Arc> IncomingArcs(AbstractVertex vertex)
        {
            return DoesNotContainVertex(vertex) ? [] : InternalIncomingArcs[vertex];
        }
        public IReadOnlyCollection<TArc> IncomingArcs<TArc>(AbstractVertex vertex) where TArc : Arc
        {
            return DoesNotContainVertex(vertex) ? [] : InternalIncomingArcs[vertex].OfType<TArc>().ToImmutableHashSet();
        }
        public IReadOnlyCollection<Arc> OutgoingArcs(AbstractVertex vertex)
        {
            return DoesNotContainVertex(vertex) ? [] : InternalOutgoingArcs[vertex];
        }
        public IReadOnlyCollection<TArc> OutgoingArcs<TArc>(AbstractVertex vertex) where TArc : Arc
        {
            return DoesNotContainVertex(vertex) ? [] : InternalOutgoingArcs[vertex].OfType<TArc>().ToImmutableHashSet();
        }
        public IReadOnlyCollection<AbstractVertex> NeighbourhoodIn(AbstractVertex vertex)
        {
            return IncomingArcs(vertex).Select(arc => arc.Tail).ToImmutableHashSet();
        }
        public IReadOnlyCollection<AbstractVertex> NeighbourhoodIn<TArc>(AbstractVertex vertex) where TArc : Arc
        {
            return IncomingArcs<TArc>(vertex).Select(arc => arc.Tail).ToImmutableHashSet();
        }
        public IReadOnlyCollection<AbstractVertex> NeighbourhoodOut(AbstractVertex vertex)
        {
            return OutgoingArcs(vertex).Select(arc => arc.Head).ToImmutableHashSet();
        }

        public IReadOnlyCollection<AbstractVertex> NeighbourhoodOut<TArc>(AbstractVertex vertex) where TArc : Arc
        {
            return OutgoingArcs<TArc>(vertex).Select(arc => arc.Head).ToImmutableHashSet();
        }

        public void AddVertex(AbstractVertex vertex)
        {
            if (InternalVertexSet.Any(v => v.Id == vertex.Id))
                throw new Exception($"Digraph already has a vertex with id {vertex.Id}");
            
            if (!InternalIncomingArcs.ContainsKey(vertex))
                InternalIncomingArcs.Add(vertex, []);

            if (!InternalOutgoingArcs.ContainsKey(vertex))
                InternalOutgoingArcs.Add(vertex, []);

            InternalVertexSet.Add(vertex);
        }

        public void AddArc(Arc arc)
        {
            if (DoesNotContainVertex(arc.Tail))
                AddVertex(arc.Tail);

            if (DoesNotContainVertex(arc.Head))
                AddVertex(arc.Head);

            InternalOutgoingArcs[arc.Tail].Add(arc);
            InternalIncomingArcs[arc.Head].Add(arc);

            InternalArcSet.Add(arc);
        }


        public bool DoesNotContainVertex(AbstractVertex vertex) =>
            InternalVertexSet.DoesNotContain(v => v.Id == vertex.Id);
    }
}
