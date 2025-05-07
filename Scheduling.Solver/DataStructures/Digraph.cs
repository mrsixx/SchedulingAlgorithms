using System.Collections.Immutable;
using Scheduling.Core.Extensions;

namespace Scheduling.Solver.DataStructures
{
    public abstract class Digraph<TVertex> where TVertex : AbstractVertex // (V,A)
    {
        #region Internal structures
        private HashSet<TVertex> InternalVertexSet { get; } = []; // V

        private HashSet<Arc<TVertex>> InternalArcSet { get; } = []; // A

        private Dictionary<int, HashSet<Arc<TVertex>>> InternalIncomingArcs { get; } = []; // \delta^-(v)

        private Dictionary<int, HashSet<Arc<TVertex>>> InternalOutgoingArcs { get; } = [];// \delta^+(v)

        #endregion

        public IReadOnlyCollection<TVertex> VertexSet => InternalVertexSet;

        public IReadOnlyCollection<Arc<TVertex>> ArcSet => InternalArcSet;

        public IReadOnlyCollection<Arc<TVertex>> IncomingArcs(TVertex vertex)
        {
            return DoesNotContainVertex(vertex) ? [] : InternalIncomingArcs[vertex.Id];
        }

        public IReadOnlyCollection<TArc> IncomingArcs<TArc>(TVertex vertex) where TArc : Arc<TVertex>
        {
            return DoesNotContainVertex(vertex) ? [] : InternalIncomingArcs[vertex.Id].OfType<TArc>().ToImmutableHashSet();
        }

        public IReadOnlyCollection<Arc<TVertex>> OutgoingArcs(TVertex vertex)
        {
            return DoesNotContainVertex(vertex) ? [] : InternalOutgoingArcs[vertex.Id];
        }

        public IReadOnlyCollection<TArc> OutgoingArcs<TArc>(TVertex vertex) where TArc : Arc<TVertex>
        {
            return DoesNotContainVertex(vertex) ? [] : InternalOutgoingArcs[vertex.Id].OfType<TArc>().ToImmutableHashSet();
        }

        public IReadOnlyCollection<TVertex> NeighbourhoodIn(TVertex vertex)
        {
            return IncomingArcs(vertex).Select(arc => arc.Tail).ToImmutableHashSet();
        }
        
        public IReadOnlyCollection<TVertex> NeighbourhoodIn<TArc>(TVertex vertex) where TArc : Arc<TVertex>
        {
            return IncomingArcs<TArc>(vertex).Select(arc => arc.Tail).ToImmutableHashSet();
        }

        public IReadOnlyCollection<TVertex> NeighbourhoodOut(TVertex vertex)
        {
            return OutgoingArcs(vertex).Select(arc => arc.Head).ToImmutableHashSet();
        }

        public IReadOnlyCollection<TVertex> NeighbourhoodOut<TArc>(TVertex vertex) where TArc : Arc<TVertex>
        {
            return OutgoingArcs<TArc>(vertex).Select(arc => arc.Head).ToImmutableHashSet();
        }

        public void AddVertex(TVertex vertex)
        {
            if (InternalVertexSet.Any(v => v.Id == vertex.Id))
                throw new Exception($"Digraph already has a vertex with id {vertex.Id}");
            
            if (!InternalIncomingArcs.ContainsKey(vertex.Id))
                InternalIncomingArcs.Add(vertex.Id, []);

            if (!InternalOutgoingArcs.ContainsKey(vertex.Id))
                InternalOutgoingArcs.Add(vertex.Id, []);

            InternalVertexSet.Add(vertex);
        }

        public void AddArc(Arc<TVertex> arc)
        {
            if (DoesNotContainVertex(arc.Tail))
                AddVertex(arc.Tail);

            if (DoesNotContainVertex(arc.Head))
                AddVertex(arc.Head);

            InternalOutgoingArcs[arc.Tail.Id].Add(arc);
            InternalIncomingArcs[arc.Head.Id].Add(arc);

            InternalArcSet.Add(arc);
        }


        public bool DoesNotContainVertex(TVertex vertex) =>
            InternalVertexSet.DoesNotContain(v => v.Id == vertex.Id);
    }
}
