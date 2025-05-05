using Scheduling.Solver.DataStructures;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Model
{
    public class ConjunctiveDigraph : Digraph
    {
        public List<AbstractVertex> TopologicalSort()
        {
            var inDegree = new Dictionary<AbstractVertex, int>();
            var queue = new Queue<AbstractVertex>();
            var topologicalSort = new List<AbstractVertex>();

            foreach (var v in VertexSet)
                inDegree[v] = IncomingArcs(v).Count;

            foreach (var v in VertexSet)
                if (inDegree[v] == 0)
                    queue.Enqueue(v);

            while (queue.Any())
            {
                var currentVertex = queue.Dequeue();
                topologicalSort.Add(currentVertex);
                
                foreach (var outgoingArc in OutgoingArcs(currentVertex))
                    if (--inDegree[outgoingArc.Head] == 0)
                        queue.Enqueue(outgoingArc.Head);
            }

            // Se contador != número de vértices, há um ciclo
            if (topologicalSort.Count != VertexSet.Count)
                throw new Exception("Digraph must be a DAG.");

            return topologicalSort;
        }
    }
}
