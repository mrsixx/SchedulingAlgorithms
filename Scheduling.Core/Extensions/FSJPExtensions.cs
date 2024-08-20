using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using System.Linq;

namespace Scheduling.Core.Extensions
{
    public static class FSJPExtensions
    {
        public static IEnumerable<Disjunction> GeneratePossibleDisjunctions(this IEnumerable<Operation> operations, DisjunctiveGraphModel graph)
        {
            var pairs = operations.CartesianProductOfOperations()
                .Aggregate(new List<Tuple<Operation, Operation>>(), (acc, pair) =>
                {
                    // arcs -> and <- are the same disjunction edge --
                    if (acc.Any(p => p.Item1.Id == pair.Item2.Id && p.Item2.Id == pair.Item1.Id))
                        return acc;
                    // there no disjunction from node to itself
                    if (pair.Item1.Id == pair.Item2.Id)
                        return acc;

                    acc.Add(pair);
                    return acc;
                });

            var nodeFromDisjunctions = new List<Node>();
            // create disjunctions between every operations that can be processed by same machine
            var disjunctions = pairs.SelectMany(pair =>
            {
                var (o1, o2) = pair;
                if (graph.TryGetNode(o1.Id, out Node node1) && graph.TryGetNode(o2.Id, out Node node2))
                {
                    nodeFromDisjunctions.AddIfDoesNotContain(node1, node2);
                    var intersection = o1.EligibleMachines.IntersectBy(o2.EligibleMachines.Select(m => m.Id), m => m.Id);
                    return intersection.Select(machine => new Disjunction(node1, node2, machine));
                }
                return [];
            }).ToList();

            // create disjunctions between every disjunctive operation and source and between every disjunctive operation and sink
            //nodeFromDisjunctions.ForEach(node =>
            //{
            //    disjunctions.Add(new(graph.Source, node, new Machine(Int32.MaxValue)));
            //    disjunctions.Add(new(node, graph.Sink, new Machine(Int32.MaxValue)));
            //});

            /*
            var disjunctionsSetSize = nodeFromDisjunctions.Count(); // or disjunctions.Count()?
            //create disjunctions between source and sink for each pair of disjunction between to operations
            for (int i = 0; i < disjunctionsSetSize; i++)
                disjunctions.Add(new(graph.Source, graph.Sink));
            */
            return disjunctions;
        }

        /// <summary>
        /// Produces O x O, where O is operations set
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<Operation, Operation>> CartesianProductOfOperations(this IEnumerable<Operation> operations)
        {
            var cartesianProduct = from o1 in operations
                                   from o2 in operations
                                   select new Tuple<Operation, Operation>(o1, o2);
            return cartesianProduct;
        }

        public static bool BelongsToTheSameMachinePoolThan(this Operation @this, Operation that)
            => @this.EligibleMachines.Intersect(that.EligibleMachines).Any();
    }
}
