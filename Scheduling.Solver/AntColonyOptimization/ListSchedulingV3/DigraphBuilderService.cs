using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Interfaces;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Model;
using Scheduling.Solver.DataStructures;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3
{
    public class DigraphBuilderService : IDigraphBuilderService
    {
        public DigraphBuilderService(ILogger? logger)
        {
            Logger = logger;
        }

        public ILogger? Logger { get; }
        
        public DisjunctiveDigraph BuildDisjunctiveDigraph(Instance fjspInstance)
        {
            DummyOperationVertex source = new(-1, "S"), sink = new(-2, "T");
            var digraph = new DisjunctiveDigraph { Source = source, Sink = sink, Instance = fjspInstance };
            digraph.AddVertex(source);
            digraph.AddVertex(sink);
            List<DisjunctiveArc> disjunctiveArcs = [];
            fjspInstance.Jobs.ToList()
                .ForEach(job =>
                {
                    // 1st operation of each job linked with source
                    AbstractVertex tailVertex = source;
                    var operation = job.Operations.First;
                    var isFirstOperation = true;
                    while (operation is not null)
                    {
                        var headVertex = new OperationVertex(operation.Value);
                        digraph.AddArc(new ConjunctiveArc(tailVertex, headVertex));
                        
                        // disjunctions between src and any operation
                        operation.Value.EligibleMachines.ToList().ForEach(
                            machine => disjunctiveArcs.AddRange(DisjunctiveArc.Disjunction(source, headVertex, machine))
                        );
                        
                        tailVertex = headVertex;
                        operation = operation.Next;
                    }

                    //last operation of each job linked with sink
                    digraph.AddArc(new ConjunctiveArc(tailVertex, sink));
                });

            // create disjunctions between every operation running on same pool

            var operationVertices = digraph.VertexSet.OfType<OperationVertex>().ToList();
            IEnumerable<Tuple<OperationVertex, OperationVertex>> cartesianProduct =
                from o1 in operationVertices
                from o2 in operationVertices
                where o1.Id < o2.Id
                select new Tuple<OperationVertex, OperationVertex>(o1, o2);


            disjunctiveArcs.AddRange(cartesianProduct
                .ToList()
                .SelectMany(pair =>
                {
                    var (o1, o2) = pair;
                    var intersection = o1.Operation.EligibleMachines.Intersect(o2.Operation.EligibleMachines);
                    return intersection.SelectMany(machine => DisjunctiveArc.Disjunction(o1, o2, machine));
                })
            );

            foreach (var arc in disjunctiveArcs)
                digraph.AddArc(arc);

            return digraph;
        }

        public ConjunctiveDigraph BuildConjunctiveDigraph(DisjunctiveDigraph disjunctiveDigraph,
            HashSet<DisjunctiveArc> selection)
        {
            var digraph = new ConjunctiveDigraph();
            foreach(var conjunctiveArc in disjunctiveDigraph.ArcSet.OfType<ConjunctiveArc>())
                digraph.AddArc(conjunctiveArc);

            foreach(var disjunctiveArc in selection)
                digraph.AddArc(disjunctiveArc);

            return digraph;
        }
    }
}
