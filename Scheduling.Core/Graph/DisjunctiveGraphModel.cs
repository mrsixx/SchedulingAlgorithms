using QuikGraph;
using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Core.Graph
{
    [Serializable]
    public class DisjunctiveGraphModel : UndirectedGraph<Node, BaseEdge>, IDisjunctiveGraph
    {
        public DisjunctiveGraphModel() : base(allowParallelEdges: true)
        {
            Source = new(Operation.Source());
            Sink = new(Operation.Sink());
        }

        public Node Source { get; }

        public Node Sink { get; }

        public List<Machine> Machines { get; } = [];

        /// <summary>
        /// All nodes except dummy ones
        /// </summary>
        public IEnumerable<Node> OperationVertices => Vertices.Where(v => !v.IsDummyNode);


        public IEnumerable<Conjunction> Conjunctions => Edges.Where(e => e is Conjunction).Cast<Conjunction>();

        public int ConjuntionCount => Conjunctions.Count();

        public IEnumerable<Disjunction> Disjunctions => Edges.Where(e => e is Disjunction).Cast<Disjunction>();

        public int DisjuntionCount => Disjunctions.Count();

        public bool HasNode(int id) => TryGetNode(id, out _);

        public bool TryGetNode(int id, out Node node)
        {
            var vertex = Vertices.ToList().Find(node => node.Id == id);
            if(vertex != null)
            {
                node = vertex;
                return true;
            }

            node = default;
            return false;
        }

        public bool HasConjunction(int sourceId, int targetId) => TryGetConjunction(sourceId, targetId, out _);
        
        public bool TryGetConjunction(int sourceId, int targetId, out Conjunction conjunction)
        {
            bool hasSrc = TryGetNode(sourceId, out Node sourceNode), 
                 hasTarget = TryGetNode(targetId, out Node targetNode);
            var hasNodes =  hasSrc && hasTarget;

            if (hasNodes
                && TryGetEdge(sourceNode, targetNode, out BaseEdge link)
                && link is Conjunction arc)
            {
                conjunction = arc;
                return true;
            }

            conjunction = default;
            return false;
        }

        public bool TryGetConjunctions(int nodeId, out IEnumerable<Conjunction> conjunctions)
        {
            bool hasNode = TryGetNode(nodeId, out Node node);

            if (hasNode)
            {
                conjunctions = Edges.Where(e => e is Conjunction c && c.Source == node).Cast<Conjunction>();
                return true;
            }

            conjunctions = default;
            return false;
        }

        public bool HasDisjunction(int sourceId, int targetId) => TryGetDisjunction(sourceId, targetId, out _);
        
        public bool TryGetDisjunction(int sourceId, int targetId, out Disjunction disjunction)
        {
            bool hasSrc = TryGetNode(sourceId, out Node sourceNode), 
                 hasTarget = TryGetNode(targetId, out Node targetNode);
            var hasNodes = hasSrc && hasTarget;

            // lowerIds always in the edge source
            if (hasNodes && sourceId < targetId)
                (sourceNode, targetNode) = (targetNode, sourceNode);

            if (hasNodes
                && TryGetEdge(sourceNode, targetNode, out BaseEdge link)
                && link is Disjunction edge)
            {
                disjunction = edge;
                return true;
            }

            disjunction = default;
            return false;
        }

        public IEnumerable<Conjunction> GetDirectSuccessors(Node node) => Conjunctions.Where(c => c.Source == node);


        public Node? GetPrecessor(Node node)
        {
            var conjunction = Conjunctions.First(c => c.Target == node);
            return conjunction?.Source;
        }

        public IEnumerable<Node> GetPredecessors(Node node) => node.Predecessors;

        public bool AddConjunction(Node source, Node target)
        {
            target.Predecessors.AddRange([..source.Predecessors, source]);
            IEnumerable<Node> predecessors = [..target.Predecessors];
            target.Predecessors.Clear();
            target.Predecessors.AddRange(predecessors.DistinctBy(p => p.Id));
            return AddEdge(new Conjunction(source, target));
        }
        public bool AddConjunction(Node source, Node target, Machine machine)
        {
            target.Predecessors.AddRange([.. source.Predecessors, source]);
            IEnumerable<Node> predecessors = [.. target.Predecessors];
            target.Predecessors.Clear();
            target.Predecessors.AddRange(predecessors.DistinctBy(p => p.Id));
            return AddEdge(new Conjunction(source, target, machine));
        }
        public void SetInitialPheromoneAmount(double amount)
        {
            foreach (var disjunction in Disjunctions)
            {
                disjunction.DepositPheromone(amount, Direction.SourceToTarget);
                disjunction.DepositPheromone(amount, Direction.TargetToSource);
            }

            foreach (var conjunction in Conjunctions)
            {
                conjunction.DepositPheromone(amount);
            }
        }
    }
}
